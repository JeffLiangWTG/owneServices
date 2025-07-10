using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;
namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSectionStatementOfAccountDocStripsTestHelper
	{
		public static string DescriptionContentForStatement => @"<IF(""<Contains(""FJ"",""<CompanyCountryCode>"")>""==""Y"",""<GenericTransactionHeader.GenericTransactions.EInvoicingAuthorisationNumber>"","""")><IF(""<Contains(""BR"",""<CompanyCountryCode>"")>""==""Y"",""<GenericTransactionHeader.GenericTransactions.EInvoicingGovernmentAllocatedNumber>"","""")>";

		public static string DescriptionContentForAccountMovement => @"<IF(""<Contains(""FJ"",""<CompanyCountryCode>"")>""==""Y"",""<GenericTransactionHeader.Transactions.EInvoicingAuthorisationNumber>"","""")><IF(""<Contains(""BR"",""<CompanyCountryCode>"")>""==""Y"",""<GenericTransactionHeader.Transactions.EInvoicingGovernmentAllocatedNumber>"","""")>";

		public static void TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(BusinessObjectFactory factory, string sectionName,string expectedContent)
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(factory, DocBuilderTemplateType.System);
			var excelWorkSheet = ConfigurableTemplateTestHelper.GetExcelWorkSheetFromSection(systemTemplate, sectionName);
			var excelWorkSheetContent = excelWorkSheet.ToString();

			Assertion.AssertContains("The section should contain description macros", expectedContent, excelWorkSheetContent);
		}
	}
}
