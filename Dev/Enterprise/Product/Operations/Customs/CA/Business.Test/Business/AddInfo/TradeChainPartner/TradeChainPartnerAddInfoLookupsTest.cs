using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TradeChainPartnerAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCSAIDTypeList()
		{
			tcpAddInfo.CA_Type = TradeChainPartnersTypeList.Codes.V;
			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				CSAVendorIDTypeList.Codes.CCC, CSAVendorIDTypeList.Codes.CSA, CSAVendorIDTypeList.Codes.DUN, CSAVendorIDTypeList.Codes.EIN, CSAVendorIDTypeList.Codes.ORG, CSAVendorIDTypeList.Codes.SSN
			},
			tcpAddInfo.Lookups.CSAIDTypeList.GetAllCodesZString());

			tcpAddInfo.CA_Type = TradeChainPartnersTypeList.Codes.C;
			AssertContainsExactElementsInAnyOrder(new ZString[] { CSAConsigneeIDTypeList.Codes.BRM, CSAConsigneeIDTypeList.Codes.CSA, CSAConsigneeIDTypeList.Codes.ORG }, tcpAddInfo.Lookups.CSAIDTypeList.GetAllCodesZString());
		}

		public void TestCSAVendorIDTypeListIsCached()
		{
			tcpAddInfo.CA_Type = TradeChainPartnersTypeList.Codes.V;
			var firstCall = tcpAddInfo.Lookups.CSAIDTypeList;
			var secondCall = tcpAddInfo.Lookups.CSAIDTypeList;
			AssertSame(firstCall, secondCall);
		}

		public void TestCSAConsigneeIDTypeListIsCached()
		{
			tcpAddInfo.CA_Type = TradeChainPartnersTypeList.Codes.C;
			var firstCall = tcpAddInfo.Lookups.CSAIDTypeList;
			var secondCall = tcpAddInfo.Lookups.CSAIDTypeList;
			AssertSame(firstCall, secondCall);
		}

		public void TestTradeChainPartnersTypeListIsCached()
		{
			var firstCall = tcpAddInfo.Lookups.TradeChainPartnersTypeList;
			var secondCall = tcpAddInfo.Lookups.TradeChainPartnersTypeList;
			AssertSame(firstCall, secondCall);
		}

		public void TestCSAStatusListIsCached()
		{
			var firstCall = tcpAddInfo.Lookups.CSAStatusList;
			var secondCall = tcpAddInfo.Lookups.CSAStatusList;
			AssertSame(firstCall, secondCall);
		}

		public void TestActionTypeListIsCached()
		{
			var firstCall = tcpAddInfo.Lookups.CSAActionTypeList;
			var secondCall = tcpAddInfo.Lookups.CSAActionTypeList;
			AssertSame(firstCall, secondCall);
		}

		public void TestCSAStatusEditableByUserListIsCached()
		{
			var firstCall = tcpAddInfo.Lookups.CSAStatusEditableByUserList;
			var secondCall = tcpAddInfo.Lookups.CSAStatusEditableByUserList;
			AssertSame(firstCall, secondCall);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var org = Factory.New<OrgHeader>();
			var orgImpAddInfo = OrgImpAddInfo.Get(org);
			var tradeChainPartner = orgImpAddInfo.TradeChainPartners.AddNew();
			tcpAddInfo = new TradeChainPartnerAddInfo(tradeChainPartner.B7_AddInfoDataInfo);
		}
		TradeChainPartnerAddInfo tcpAddInfo;
	}
}
