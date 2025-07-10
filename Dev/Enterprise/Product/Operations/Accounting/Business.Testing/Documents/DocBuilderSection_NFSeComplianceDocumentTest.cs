using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.Accounting.Business.Testing.Documents
{
	class DocBuilderSection_NFSeComplianceDocumentTest : TestCaseWithFactory
	{
		public void TestSectionContains_ModifiedTaxBranchProperties()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var excelWorkSheet = ConfigurableTemplateTestHelper.GetExcelWorkSheetFromSection(systemTemplate, "NFSe Compliance Document");

			var expectedStateRegContent = @"{AI}-[State Registration]   {AQ}-[<ARInvoice.TaxBranch.OrganisationWithCompanyFallBack.CustomCodes.Find(""{Country.Code}""==""BR"" && ""{CodeType}""==""IEF"").CustomsRegNo>]";
			var expectedCorporateNameContent = "{C}-[Name / Corporate Name]   {O}-[<ARInvoice.TaxBranch.OrganisationWithCompanyFallBack.Name>]";
			var expectedAddressContent = "{C}-[Address]   {O}-[<ARInvoice.TaxBranch.OrganisationWithCompanyFallBack.ARAddress.PostalAddressExcludeName>]";
			var expectedZipCodeContent = "{C}-[Zip Code]   {O}-[<ARInvoice.TaxBranch.OrganisationWithCompanyFallBack.ARAddress.PostCode>]";
			var expectedMunicipalityContent = "{U}-[Municipality]   {AA}-[<ARInvoice.TaxBranch.OrganisationWithCompanyFallBack.ARAddress.City>]";
			var expectedStateContent = "{AM}-[State]   {AQ}-[<ARInvoice.TaxBranch.OrganisationWithCompanyFallBack.ARAddress.State>]";

			var excelWorkSheetContent = excelWorkSheet.ToString();

			AssertContains("State Registration", expectedStateRegContent, excelWorkSheetContent);
			AssertContains("Name /Corporate Name", expectedCorporateNameContent, excelWorkSheetContent);
			AssertContains("Address", expectedAddressContent, excelWorkSheetContent);
			AssertContains("Zip Code", expectedZipCodeContent, excelWorkSheetContent);
			AssertContains("Municipality", expectedMunicipalityContent, excelWorkSheetContent);
			AssertContains("State", expectedStateContent, excelWorkSheetContent);
		}
	}
}
