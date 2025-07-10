using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHMasterCloseWrapperTest : TestCaseWithFactory
	{
		public void TestFieldsForWithdraw()
		{
			AssertEquals("PreviousCCNForWithdraw", "", closeWrapper.PreviousCCNForWithdraw);
			AssertEquals("CarrierCodeForWithdraw", "", closeWrapper.CarrierCodeForWithdraw);
			master.BP_MasterHouseCCN = "CCN";
			AssertEquals("PreviousCCNForWithdraw", "CCN", closeWrapper.PreviousCCNForWithdraw);
			master.BP_CBSACarrierCode = "PCN";
			AssertEquals("CarrierCodeForWithdraw", "PCN", closeWrapper.CarrierCodeForWithdraw);
			var incomingMessage = Factory.New<ACIForwarderCloseMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			incomingMessage.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = EManifestForwarderJobStatusList.Codes.Clear;
			incomingMessage.EM_MessageText = @"UNH+1+GOVCBR:D:11B:UN
BGM+313+8036081-53451134+11
DTM+9:201407292127:203
RFF+AGO:CLS-C10001000
RCS+11
FTX+AAO+++29
GEI+5+66
ERC+463
ERP+2:973:29
UNS+D
HYN+3
UNS+S
UNT+13+1
".Replace("\r\n", "'");
			incomingMessage.EM_LinkedObject = master;
			master.Messages.Reload(true);
			AssertEquals("PreviousCCNForWithdraw", "081-53451134", closeWrapper.PreviousCCNForWithdraw);
			AssertEquals("CarrierCodeForWithdraw", "8036", closeWrapper.CarrierCodeForWithdraw);
		}

		public void TestFieldsForAmendment()
		{
			AssertEquals("AmendReasonCode", ZString.Empty, closeWrapper.AmendReasonCode);
			AssertEquals("ATA", ZDateTime.Empty, closeWrapper.ATA);
			master.BP_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.CBSAOutage;
			AssertEquals("AmendReasonCode", EManifestAmendmentReasonCodes.Codes.CBSAOutage, closeWrapper.AmendReasonCode);
			var aTA = new ZDateTime(2021, 2, 1);
			master.BP_ATA = aTA;
			AssertEquals("BP_ATA", aTA, closeWrapper.ATA);
		}

		public void TestAllCCNs()
		{
			AssertEquals("Two", 2, closeWrapper.AllCCNs.Count);
		}

		public void TestRelatedCCNs()
		{
			foreach (var ccn in closeWrapper.AllCCNs)
			{
				ccn.IsShouldSend = false;
			}
			AssertEquals("Zero", 0, closeWrapper.RelatedCCNs.Count());
			foreach (var ccn in closeWrapper.AllCCNs)
			{
				ccn.IsShouldSend = true;
			}
			AssertEquals("Two", 2, closeWrapper.RelatedCCNs.Count());
		}

		public void TestUpdateIsShouldSend()
		{
			foreach (var ccn in closeWrapper.AllCCNs)
			{
				ccn.IsShouldSend = false;
			}
			closeWrapper.UpdateIsShouldSend(house => house.BW_HouseCCN == "ccn1");
			foreach (var ccn in closeWrapper.AllCCNs)
			{
				AssertEquals("IsShouldSend", ccn.CCN == ccn1, ccn.IsShouldSend);
			}
		}

		#region Implementation

		const string ccn1 = "ccn1";
		const string ccn2 = "ccn2";
		CusCAeMHMasterCloseWrapper closeWrapper;
		CusCAeMHMaster master;

		protected override void SetUp()
		{
			base.SetUp();
			master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_HouseCCN = ccn1;
			house = master.HouseBills.AddNew();
			house.BW_HouseCCN = ccn2;
			closeWrapper = new CusCAeMHMasterCloseWrapper(master);
		}

		#endregion

	}
}
