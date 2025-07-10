using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class VisualizerTemplateValidationTest : TestCaseWithFactory
	{
		public void TestSO_Name()
		{
			var template = Factory.New<VisualizerTemplate>();

			template.Validation.ValidateSO_Name();

			AssertHasError(template.SO_NameInfo, "Please enter a Template Name.");

			template.SO_Name = "Eeyore";

			AssertNoError(template.SO_NameInfo, "Please enter a Template Name.");
		}

		public void TestSO_DataContext()
		{
			var template = Factory.New<VisualizerTemplate>();

			template.SO_DataContext = "";

			AssertHasError(template.SO_DataContextInfo, "Please enter a value.");

			template.SO_DataContext = "Eeyore";

			AssertNoError(template.SO_DataContextInfo, "Please enter a value.");
		}

		public void TestSO_Template()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
				var template = Factory.New<VisualizerTemplate>();

				template.Validation.ValidateSO_Template();

				AssertHasError(template.SO_TemplateInfo, "Please enter a value.");

				template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(tempFileName);

				AssertNoError(template.SO_TemplateInfo, "Please enter a value.");
			}
		}
	}
}
