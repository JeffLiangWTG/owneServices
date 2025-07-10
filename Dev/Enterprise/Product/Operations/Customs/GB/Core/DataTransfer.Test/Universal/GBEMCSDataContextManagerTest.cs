using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	[TestedType(typeof(GBEMCSDataContextManager))]
	class GBEMCSDataContextManagerTest : DataContextManagerTestCase<GBEMCSDataContextManager, EMCSJobDeclaration>
	{
		protected override void TestAttributeIsOnBusinessObjectCore()
		{
			var attribute = typeof(EMCSJobDeclaration).GetAttribute<UniversalDataContextAttribute>();
			AssertEquals(DataContextType.EMCSJobDeclaration, attribute.DataContextType);
			AssertEquals(DataContextType.EMCSJobDeclaration, new GBEMCSDataContextManager().DataContextType);
		}

		public void TestCanImportEventViaUniversalDataBuss()
		{
			SetupOutgoingMessage();
			var xmlMessage = GBEMCSEventParentFinderTest.CreateSuccessfulResponseXml(sessionGuid);
			var message = GetQueuedUniversalEventMessage(xmlMessage);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.Acknowledged, outgoingSentMessage.EM_Status);
				AssertMultilineASCIIEquals("Service Task Log", "Linked Event to Declaration B0001000.", serviceTaskLog.ToString());
				AssertMultilineASCIIEquals("Message Log Note", "Linked Event to Declaration B0001000.", message.GetLogNoteText());
			});
		}

		void SetupOutgoingMessage(string outgoingMessageStatus = EDIMessage.Status.Sent)
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B0001000";
			declaration.JE_HouseBill = "TESTHOUSE";

			var outgoingInterchange = Factory.New<EDIInterchange>();
			sessionGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = sessionGuid;
			outgoingInterchange.EI_HeaderText = "";
			outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(EMCSOutboundEDIMessage));
			declaration.Messages.Add(outgoingSentMessage);
			outgoingSentMessage.EM_EI = outgoingInterchange.PK;
			outgoingSentMessage.EM_MessageText = "";
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory.BOFactory, EDIMessage.ApplicationCodes.GbCustomsEMCS);
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingSentMessage.EM_Status = outgoingMessageStatus;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "AAW";
			outgoingInterchange.EI_BodyText = "";
			Factory.SaveForTesting();
			outgoingSentMessage.EM_MessageNum = "999";
			Factory.SaveForTesting();
		}

		EDIMessage outgoingSentMessage;
		ZGuid sessionGuid;
	}
}
