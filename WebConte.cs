using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RazorEngineCore;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Eeva.Entity
{
    public sealed class WebconteEntity
    {
        public async Task<Microsoft.AspNetCore.Html.IHtmlContent> RenderAsync<TModel>(TModel model, ITempDataDictionary tempData)
        {
            string content = @"
Partial result:
@(await Html.PartialAsync(""PartialTest""))
                ";
            string parsed = await RenderAsync(content, model, tempData);
            return new Microsoft.AspNetCore.Html.HtmlString(parsed);
        }


        static readonly ConcurrentDictionary<int, object> templateCache = new();

        async Task<string> RenderAsync<TModel>(string template, TModel model, ITempDataDictionary tempData)
        {

            IRazorEngineCompiledTemplate<EevaRazorEngineCorePageModel<TModel>> compiledTemplate;

            int templateHashCode = template.GetHashCode(); // + model.GetHashCode();
            if (!templateCache.TryGetValue(templateHashCode, out object vaart))
            {
                vaart = await CompileTempate<TModel>(template);
                templateCache.TryAdd(templateHashCode, vaart);
            }

            compiledTemplate = (IRazorEngineCompiledTemplate<EevaRazorEngineCorePageModel<TModel>>)vaart;
            return await compiledTemplate.RunAsync(instance =>
            {
                instance.Model = model;
            });
        }

        async Task<IRazorEngineCompiledTemplate<EevaRazorEngineCorePageModel<TModel2>>> CompileTempate<TModel2>(string template)
        {
            var razorEngine = new RazorEngine();
            var compiledTemplateLocal = await razorEngine.CompileAsync<EevaRazorEngineCorePageModel<TModel2>>(template, builder =>
            {
                builder.AddAssemblyReference(typeof(RazorEngineCorePageModel));
                builder.AddAssemblyReference(typeof(ITempDataDictionary));
                builder.AddUsing("System");
            });
            return compiledTemplateLocal;
        }
    }

    public abstract class EevaRazorEngineCorePageModel<T> : RazorEngineCorePageModel, IRazorEngineTemplate
    {
        public ITempDataDictionary TempData;
    }
}