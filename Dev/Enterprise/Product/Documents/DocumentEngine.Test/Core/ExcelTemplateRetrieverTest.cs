using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ExcelTemplateRetrieverTest : TestCaseWithFactory
	{
		public void TestGetTemplate()
		{
			AssertNull("Returns null for Non-existing template name", ExcelTemplateRetriever.GetTemplate("Non-Existing", Core.Constants.DataContext.Shipment, null));

			var template = Factory.New<StmTemplate>();
			template.SO_Name = TemplateNameForTesting;
			template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);

			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
				template.SO_Template = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName)).GetAsByteArray();
			}

			var result = ExcelTemplateRetriever.GetTemplate(TemplateNameForTesting, Core.Constants.DataContext.Shipment, Factory);
			AssertNotNull("Template byte array should not be null", result.GetAsByteArray());
			AssertEquals("Template name should match", TemplateNameForTesting, result.TemplateName);

			result = ExcelTemplateRetriever.GetTemplate(TemplateNameForTesting, nameof(Core.Constants.DataContext.Shipment), Factory);
			AssertNotNull("Template byte array should not be null", result.GetAsByteArray());
			AssertEquals("Template name should match", TemplateNameForTesting, result.TemplateName);
		}

		const string TemplateNameForTesting = "New Template";
	}
}
