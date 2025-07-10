using System.IO;
using System.Text;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.ExcelTemplates.Testing
{
	sealed class ExcelTemplateReadFromByteArrayTest : BaseExcelTemplateTest
	{
		public override void TestTemplateNameAndTemplateSourceLocation()
		{
			ExcelTemplate excelTemplate = new ExcelTemplateReadFromByteArray("Hello", "Again", Encoding.UTF8.GetBytes("testagain"));
			AssertEquals("excelTemplate.TemplateName", "Hello", excelTemplate.TemplateName);
			AssertEquals("excelTemplate.TemplateSourceLocation", "Again", excelTemplate.TemplateSourceLocation);
		}

		public override void TestGetAsStreamVsGetAsByteArray()
		{
			ExcelTemplate excelTemplate = new ExcelTemplateReadFromByteArray("Hello", "Again", Encoding.UTF8.GetBytes("testagain"));
			AssertStreamAndByteArrayExpectedLength(excelTemplate, 9);
		}

		public void TestGetDataContextValueFromBlobData()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.BizObjDataSourceForGetGroups.xls", "BizObjDataSourceForGetGroups.xls");
				var testFile = new ExcelTemplateForUnitTesting("BizObjDataSourceForGetGroups.xls", Path.GetFullPath(tempFileName));
				var excelTemplate = new ExcelTemplateReadFromByteArray("Hello", testFile.FullTemplateSourceLocation, testFile.GetAsByteArray());
				AssertEquals(".DummyBusinessObject", excelTemplate.GetDataContextValueFromBlobData().FullDataContext);
			}
		}
	}
}
