using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class TP5InboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestNCTSResponseMessage()
		{
			var nctsHeader = Factory.New<DummyNctsHeader_TestNewFromDeltaT>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(nctsHeader.MovementHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, nctsHeader.CountryCode);
			entryNumber.CE_EntryNum = "0000007735";

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_From = "EASYLOG2TEST_EAD";
			interchange.EI_To = "HYEDFRCMT";
			interchange.EI_ApplicationCode =  EDIMessage.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
			interchange.EI_InterchangeNum = "237";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			interchange.EI_BodyText = GetMessageText;
			Factory.Save();

			var processor = new TP5IncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			interchange.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange.EI_Status);

			var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertEquals(1, messages.Length);

			var message = messages[0];
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("message linked to interchange", interchange.PK, message.EM_EI);
			AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.FRCustomsMessage, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.TP5, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", "004", message.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_MessageNum", "237", message.EM_MessageNum);
			AssertEquals("EM_ApplicationReference", "", message.EM_ApplicationReference);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertEquals("EM_HeldUntilDate", ZDateTime.Empty, message.EM_HeldUntilDate);
			AssertEquals("EM_MessageText body text", GetMessageText, message.EM_MessageText);
			AssertEquals("EM_LinkedObject", nctsHeader.MovementHeader, message.EM_LinkedObject);
		}

		ZString GetMessageText => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC004CResponseMessage.xml");

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
