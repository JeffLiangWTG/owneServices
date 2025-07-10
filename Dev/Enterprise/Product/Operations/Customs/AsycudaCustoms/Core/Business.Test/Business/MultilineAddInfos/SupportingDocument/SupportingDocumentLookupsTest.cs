using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSupportingDocumentCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("UDD0C", "RefCusCodeListType Distracter");
			helper.CreateNewOrGetExistingCusCodeType("UDDOC", "UDDOC-User Defined Supporting Documents");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Botswana, "UDD0C", "CD1", "RefCusCodeList Distracter", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Botswana, "UDDOC", "CD2", "UDDOC-User Defined Supporting Documents", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Namibia, "UDDOC", "CD3", "UDDOC-User Defined Supporting Documents", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var supDoc = ((ISupportingDocumentsProvider)invoiceHeader).SupportingDocuments.AddNew();
			var tetList = supDoc.Lookups.SupportingDocumentCodeList;
			AssertEquals("Should have loaded the only 1 correct RefCusCodeList for using declaration default datagrouping BW", 1, tetList.Count);
			Assert("Should have loaded the only 1 correct RefCusCodeList", tetList.ContainsCode("CD2"));
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertSame("Should have cached", tetList, ((ISupportingDocumentsProvider)invoiceLine).SupportingDocuments.AddNew().Lookups.SupportingDocumentCodeList);
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader2.JZ_GB = branch.PK;
			var supDoc2 = ((ISupportingDocumentsProvider)invoiceHeader2).SupportingDocuments.AddNew();
			var tetList2 = supDoc2.Lookups.SupportingDocumentCodeList;
			AssertEquals("1 RefCusCodeList if no declaration binded and use invoice default datagrouping NA", 1, tetList2.Count);
			Assert("Should have loaded the only 1 correct RefCusCodeList", tetList2.ContainsCode("CD3"));
			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			AssertSame("Should have cached", tetList2, ((ISupportingDocumentsProvider)invoiceLine2).SupportingDocuments.AddNew().Lookups.SupportingDocumentCodeList);
		}
	}
}
