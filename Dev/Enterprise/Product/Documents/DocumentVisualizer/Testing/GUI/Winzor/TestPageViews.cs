using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	public static class TestPageViews
	{
		public static ZPanel TestTemplate()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			var child = dummy.Collection.AddNew();
			child.Z0_Description = "I am a child";

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				var worksheet = FlexCelWorksheet.FromFile(tempFileName);
				IStandardTemplate template = new StandardTemplate(worksheet);
				var documentBuilder = new StandardDocumentBuilder(
					template.Name,
					"test",
					template,
					new MacroScope(dummy.MakeDynamic()),
					new IMacroLibrary[]
					{
					new StandardLibrary(),
					new DocumentLibrary()
					}.CreateContext(),
					null,
					DefaultLanguageProvider.Instance);

				var document = documentBuilder.Build();
				var presenter = new DummyPageViewPresenter();
				var service = new PageViewBuildService();
				var pageView = (PageView)service.CreatePageView(document.Pages.First(), presenter, isReadOnly: false);
				return pageView;
			}
		}
	}
}
