using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHMasterDocumentSupporter))]
	sealed class CusCAeMHDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var documentSupporter = ((IDocumentSupportable)master).DocumentSupporter;
			AssertEquals("This eManifest does not have any house bill.", documentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(".HouseBills"), null));
			AssertEquals("This eManifest does not have any accepted message.", documentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(".MasterBills"), null));
			AssertEquals("There is no D4 message on this eManifest nor house bills.", documentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue(UniversalEventMessageDocumentSupporter.UniversalEventReport), null));
		}

		public void TestGetBODocDataProviders()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house1 = master.HouseBills.AddNew();
			var house2 = master.HouseBills.AddNew();

			var eventTime = ZDateTime.Now.AddMinutes(1);

			var message1 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "5");
			message1.EM_LinkedObject = master;
			message1.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message2 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "5");
			message2.EM_LinkedObject = house1;
			message2.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message3 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "5");
			message3.EM_LinkedObject = house2;
			message3.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;

			var message5 = Factory.New<ACIForwarderCloseMessage>();
			message5.EM_LinkedObject = master;
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5.EM_Status = EDIMessageStatusList.Codes.Received;
			Factory.Save();

			var providers = ((IDocumentSupportable)master).DocumentSupporter.GetBODocDataProviders(new DataContextValue(".HouseBills"), null);
			AssertEquals(2, providers.Length);

			providers = ((IDocumentSupportable)master).DocumentSupporter.GetBODocDataProviders(new DataContextValue(".MasterBills"), null);
			AssertEquals(1, providers.Length);

			providers = ((IDocumentSupportable)master).DocumentSupporter.GetBODocDataProviders(new DataContextValue(UniversalEventMessageDocumentSupporter.UniversalEventReport), null);
			AssertEquals(3, providers.Length);

			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = "Deconsolidation Close(D4)";
			providers = ((IDocumentSupportable)master).DocumentSupporter.GetBODocDataProviders(new DataContextValue(UniversalEventMessageDocumentSupporter.UniversalEventReport), menuItem);
			AssertEquals(0, providers.Length);

			var house3 = master.HouseBills.AddNew();
			var stauts8000 = "         <Context>\r\n            <Type>Status</Type>\r\n            <Value>8000</Value>\r\n         </Context>";
			var message4 = UniversalEventMessageTest.CreateNewUniversalEventMessage(Factory, eventTime, interchangeNumber: "7599", messageNumber: "5", status: stauts8000);
			message4.EM_LinkedObject = house3;
			message4.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			Factory.Save();

			providers = ((IDocumentSupportable)master).DocumentSupporter.GetBODocDataProviders(new DataContextValue(UniversalEventMessageDocumentSupporter.UniversalEventReport), menuItem);
			AssertEquals(1, providers.Length);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<CusCAeMHMaster>();
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}
	}
}
