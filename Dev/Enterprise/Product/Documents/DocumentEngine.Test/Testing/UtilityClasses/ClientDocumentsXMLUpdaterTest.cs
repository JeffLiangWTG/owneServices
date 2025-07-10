using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	sealed class ClientDocumentsXMLUpdaterTest : TestCaseWithFactory
	{
		public void TestGetClientCode()
		{
			AssertEquals(@"GetTemplateClientCode(Enterprise\Product\Documents\ExcelTemplates\Documents\EDI\file.xls", "EDI", ClientDocumentsXMLUpdater.GetTemplateClientCode(@"Enterprise\Product\Documents\ExcelTemplates\Documents\EDI\file.xls"));
			AssertEquals(@"GetTemplateClientCode(Enterprise\Product\Documents\ExcelTemplates\Reports\EDI\file.xls", "EDI", ClientDocumentsXMLUpdater.GetTemplateClientCode(@"Enterprise\Product\Documents\ExcelTemplates\Reports\EDI\file.xls"));
			AssertNull(@"GetTemplateClientCode(Enterprise\Product\Documents\ExcelTemplates\Documents\XXXX\file.xls", ClientDocumentsXMLUpdater.GetTemplateClientCode(@"Enterprise\Product\Documents\ExcelTemplates\Documents\XXXX\file.xls"));
		}
	}
}
