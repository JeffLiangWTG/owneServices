using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAOrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProperties()
		{
			AssertEquals(typeof(CFIAPaymentMethods), addInfo.Lookups.CFIAPaymentMethods.GetType());
			AssertEquals(typeof(LVSInvoiceDetailCodes), addInfo.Lookups.LVSInvoiceDetailCodes.GetType());
			AssertEquals(typeof(DelayIntervalTypeCodes), addInfo.Lookups.DelayIntervalTypeCodesListForHVS.GetType());
			AssertEquals(typeof(DelayIntervalTypeCodes), addInfo.Lookups.DelayIntervalTypeCodesListForCON.GetType());
			AssertEquals(typeof(DeferredB3SendActionListOverride), addInfo.Lookups.DeferedB3SendActionList.GetType());
			AssertEquals(typeof(ProductAuditActions), addInfo.Lookups.ProductAuditActions.GetType());
			AssertEquals(typeof(CSARSFAccountingOptionList), addInfo.Lookups.AccountingOptions.GetType());
		}

		public void TestCounts()
		{
			AssertEquals("4 methods", 4, addInfo.Lookups.CFIAPaymentMethods.Count);
			AssertEquals("4 methods", 3, addInfo.Lookups.LVSInvoiceDetailCodes.Count);
			AssertEquals("3 codes", 3, addInfo.Lookups.DelayIntervalTypeCodesListForHVS.Count);
			AssertEquals("3 codes", 3, addInfo.Lookups.DelayIntervalTypeCodesListForCON.Count);
			AssertEquals("3 codes", 3, addInfo.Lookups.DeferedB3SendActionList.Count);
			AssertEquals("4 codes", 4, addInfo.Lookups.ProductAuditActions.Count);
			AssertEquals("2 codes", 2, addInfo.Lookups.AccountingOptions.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var organisation = Factory.New<OrgHeader>();
			addInfo = new OrgImpAddInfo((ZPropertyInfoString)organisation.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
		}
		OrgImpAddInfo addInfo;
	}
}
