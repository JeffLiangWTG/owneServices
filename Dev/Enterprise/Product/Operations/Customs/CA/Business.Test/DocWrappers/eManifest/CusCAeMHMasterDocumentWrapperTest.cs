using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHMasterDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var wrapper = new CusCAeMHMasterDocumentWrapper(master);
			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("CusCAeMHMasterDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", master.PK, supporter?.SourceIdentifier);
		}

		#endregion

		public void TestProperties()
		{
			var wrapper = new CusCAeMHMasterDocumentWrapper(master);
			CombineAssertions(() =>
			{
				AssertEquals("ReferenceNumber", "081-14081201", wrapper.ReferenceNumber);
				AssertEquals("PreviousCCN", "0000", wrapper.PreviousCCN);
				AssertEquals("CarrierName", "Test Desc", wrapper.CarrierName);
				AssertEquals("HouseBills.Count", 3, wrapper.HouseBills.Count);
				AssertEquals("HouseBills[0].HouseCCN", "0001", wrapper.HouseBills[0].HouseCCN);
				AssertEquals("HouseBills[1].HouseCCN", "0002", wrapper.HouseBills[1].HouseCCN);
				AssertEquals("HouseBills[2].HouseCCN", "0003", wrapper.HouseBills[2].HouseCCN);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "1234";
			carrier.ZZ4_Description = "Test Desc";
			carrier.ZZ4_IsSea = true;
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			Factory.Save();

			master = Factory.New<CusCAeMHMaster>();
			master.BP_MasterHouseCCN = "0000";
			master.BP_CBSACarrierCode = "1234";
			var house1 = master.HouseBills.AddNew();
			house1.BW_HouseCCN = "0001";
			var house2 = master.HouseBills.AddNew();
			house2.BW_HouseCCN = "0002";
			var house3 = master.HouseBills.AddNew();
			house3.BW_HouseCCN = "0003";

			var message1 = Factory.New<ACIForwarderCloseMessage>();
			message1.EM_LinkedObject = master;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

			var message2 = Factory.New<ACIForwarderCloseMessage>();
			message2.EM_LinkedObject = master;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			message2.EM_Status = EDIMessageStatusList.Codes.Received;
			message2.EM_SystemCreateTimeUtc = ZDateTime.Today;
			message2.EM_MessageText = @"UNH+27+GOVCBR:D:11B:UN:ACIHCM
BGM+87+081-14081201+9
RFF+ABO:CLS-C10001000
NAD+FW+8036
DOC+85+8036CAH1000001
UNS+D
HYN+3
UNS+S
UNT+9+27".Replace("'\r\n", "'").Replace("\r\n", "'");
			Factory.Save();
		}
		CusCAeMHMaster master;
	}
}
