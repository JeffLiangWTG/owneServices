using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TradeChainPartnerCollection))]
	sealed class TradeChainPartnerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetTCPByOrgCode()
		{
			var orgImp = OrgImpAddInfo.Get(OrgHeader);
			var tcp1 = orgImp.TradeChainPartners.AddNew();
			var tcp2 = orgImp.TradeChainPartners.AddNew();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			org2.OH_Code = "TESTORG2";
			tcp1.CA_Org = org1.PK;
			tcp2.CA_Org = org2.PK;
			tcp1.CA_CSAStatus = CSAStatusList.Codes.Added;
			tcp2.CA_CSAStatus = CSAStatusList.Codes.Deleted;

			var result1 = orgImp.TradeChainPartners.GetTCPByOrgCode(org1.OH_Code);
			var result2 = orgImp.TradeChainPartners.GetTCPByOrgCode(org2.OH_Code);

			AssertEquals("Check PK for result1.", org1.PK, result1.CA_Org);
			AssertEquals("Check PK for result2.", org2.PK, result2.CA_Org);
			AssertEquals("Check status for result1.", CSAStatusList.Codes.Added, result1.CA_CSAStatus);
			AssertEquals("Check status for result2.", CSAStatusList.Codes.Deleted, result2.CA_CSAStatus);
		}

		public void TestGetTCPByCSAID()
		{
			var orgImp = OrgImpAddInfo.Get(OrgHeader);
			var tcp1 = orgImp.TradeChainPartners.AddNew();
			var tcp2 = orgImp.TradeChainPartners.AddNew();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			org2.OH_Code = "TESTORG2";
			tcp1.CA_Org = org1.PK;
			tcp2.CA_Org = org2.PK;
			tcp1.CA_CSAID = "ABC";
			tcp2.CA_CSAID = "def";
			tcp1.CA_CSAStatus = CSAStatusList.Codes.Added;
			tcp2.CA_CSAStatus = CSAStatusList.Codes.Deleted;

			var result1 = orgImp.TradeChainPartners.GetTCPByCSAID("ABC");
			var result2 = orgImp.TradeChainPartners.GetTCPByCSAID("DEF");

			AssertEquals("Check PK for result1.", org1.PK, result1.CA_Org);
			AssertEquals("Check PK for result2.", org2.PK, result2.CA_Org);
			AssertEquals("Check status for result1.", CSAStatusList.Codes.Added, result1.CA_CSAStatus);
			AssertEquals("Check status for result2.", CSAStatusList.Codes.Deleted, result2.CA_CSAStatus);
		}

		public void TestSetDefaultsForNewChild()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var tcp = OrgImpAddInfo.Get(org).TradeChainPartners.AddNew();
			AssertEquals(CSAActionTypeList.Codes.ReqAdd, tcp.CA_Action);
			AssertEquals(CSAStatusList.Codes.New, tcp.CA_CSAStatus);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TradeChainPartnerCollection(OrgHeader);
		}

		OrgHeader OrgHeader
		{
			get { return orgHeader ?? (orgHeader = Factory.New<OrgHeader>()); }
		}
		OrgHeader orgHeader;

		#endregion
	}
}
