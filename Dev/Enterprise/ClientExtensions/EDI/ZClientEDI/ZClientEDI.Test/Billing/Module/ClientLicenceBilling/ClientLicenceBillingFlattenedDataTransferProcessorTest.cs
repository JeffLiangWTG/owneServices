using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class ClientLicenceBillingFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			var flattenedCollection = new ClientLicenceBillingFlattenedCollection(Factory);
			var collectionInfo = new ClientLicenceBillingImportInfo(flattenedCollection);
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "SOMEORG1";
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "OG2");
			org2.LicCompany.SelfBilling.Delete();
			var org3 = BillingTestHelper.CreateOrganisation(Factory, "OG3");
			org3.LicCompany.SelfBilling.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "AUD";
			org3.LicCompany.SelfBilling.L4_PredeterminedPrepaidBalance = 100m;
			org3.LicCompany.SelfBilling.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = "GBP";
			org3.LicCompany.SelfBilling.L4_FuturePredeterminedPrepaidBalance = 200m;
			var org4 = BillingTestHelper.CreateOrganisation(Factory, "OG4");
			org4.LicCompany.SelfBilling.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "CNY";
			org4.LicCompany.SelfBilling.L4_PredeterminedPrepaidBalance = 300m;
			var org5 = BillingTestHelper.CreateOrganisation(Factory, "OG5");
			org5.LicCompany.SelfBilling.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "USD";
			org5.LicCompany.SelfBilling.L4_PredeterminedPrepaidBalance = 350m;
			Factory.Save();
			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = org1.OH_Code;
			rec1.CurrentPrepaymentCurrency = "AUD";
			rec1.CurrentPrepaymentBalance = 400m;
			var rec2 = flattenedCollection.AddNew();
			rec2.OrgCode = org2.OH_Code;
			rec2.CurrentPrepaymentCurrency = "AUD";
			rec2.CurrentPrepaymentBalance = 500m;
			var rec3 = flattenedCollection.AddNew();
			rec3.OrgCode = org3.OH_Code;
			rec3.CurrentPrepaymentCurrency = "USD";
			rec3.CurrentPrepaymentBalance = 6000m;
			rec3.FuturePrepaymentCurrency = "CAD";
			rec3.FuturePrepaymentBalance = 700m;
			var rec4 = flattenedCollection.AddNew();
			rec4.OrgCode = org4.OH_Code;
			rec4.FuturePrepaymentCurrency = "USD";
			rec4.FuturePrepaymentBalance = 800m;
			var recWithBadOrgCode = flattenedCollection.AddNew();
			recWithBadOrgCode.OrgCode = "ZZZZZZZ";
			recWithBadOrgCode.CurrentPrepaymentCurrency = "AUD";
			recWithBadOrgCode.CurrentPrepaymentBalance = 900m;
			var recWithInvalidCurrency = flattenedCollection.AddNew();
			recWithInvalidCurrency.OrgCode = org5.OH_Code;
			recWithInvalidCurrency.CurrentPrepaymentCurrency = "@#$";
			recWithInvalidCurrency.CurrentPrepaymentBalance = 1000m;
			var collection = new ClientLicenceBillingCollectionNonDependent(Factory);
			var processor = new ClientLicenceBillingFlattenedDataTransferProcessor(collection, collectionInfo);
			processor.Import();
			var importedList = collection.Cast<ClientLicenceBilling>().ToList();
			Factory.Save();
			AssertEquals("ClientLicenceBilling count", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder(importedList, new[] { org3.LicCompany.SelfBilling, org4.LicCompany.SelfBilling });
			var rowsAsText = string.Join("\r\n", new BusinessObjectFactory().Load<ClientLicenceBilling>(new ZQuery()).Select(x => $"{x.Company.Header.OH_Code} - {x.L4_RX_NKPredeterminedPrepaidBalanceCurrency} - {x.L4_PredeterminedPrepaidBalance} - {x.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency} - {x.L4_FuturePredeterminedPrepaidBalance}").OrderBy(x => x));
			AssertEquals(@"OG3SYD - USD - 6000.0000 - CAD - 700.0000
OG4SYD - CNY - 300.0000 - USD - 800.0000
OG5SYD - USD - 350.0000 -  - 0.0000", rowsAsText);
			AssertEquals(@"Record [Org. Code: SOMEORG1] excluded: Org. SOMEORG1 has no license
Record [Org. Code: OG2SYD] excluded: Org. OG2SYD has no license
Record [Org. Code: ZZZZZZZ] excluded: Org. Code not found
Record [Org. Code: OG5SYD] excluded: Invalid Currency: @#$
", processor.Log);
		}
	}
}
