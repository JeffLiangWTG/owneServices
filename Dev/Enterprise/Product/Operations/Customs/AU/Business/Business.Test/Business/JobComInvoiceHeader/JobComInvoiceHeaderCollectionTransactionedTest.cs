using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceHeaderCollectionTransactionedTest : TestCaseWithFactory
	{
		public void TestSetDefaultValuesForNewChildFromOrgSupplierBuyerLink()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();

			OrgHeader supplier = Factory.New<OrgHeader>();
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = "AUSYD";

			OrgSupplierBuyerLink link = supplier.BuyerLinks.AddNew(importer);
			link.OL_ValuationBasis = "FB";
			link.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			link.OL_RX_NKDefaultCurrency = "AUD";
			link.OL_RelatedParty = "Y";

			testJobDeclaration.JE_OH_Supplier = supplier.PK;
			testJobDeclaration.JE_OH_Importer = importer.PK;
			testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			AssertEquals("Default Supplier", supplier.PK, testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier);
			AssertEquals("Default Relationship", "FB", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].AddInfo.ZA_VALB_Hidden);
			AssertEquals("Default IncoTerm", Core.Constants.IncoTerms.FreeOnBoard, testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_IncoTerm);
			AssertEquals("Default Realted party", "Y", testJobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].AddInfo.ZA_HeaderREL_Hidden);
		}
	}
}
