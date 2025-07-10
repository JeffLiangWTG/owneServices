using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	abstract class AISH7MessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider> :
		MessageAttacheeMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider, AsycudaBill, AsycudaManifestHeader>
			where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
			where TInboundEDIMessage : InboundEDIMessage
			where TOutboundEDIMessage : OutboundEDIMessage
	{
		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, TInboundEDIMessage incomingMessage)
		{
			var outgoingEmails = Env.OutgoingCustomsMailManager.EmailsCreated;
			Assert("AIS H7 processor do not send email notification", !outgoingEmails.Any());
		}

		protected override (AsycudaManifestHeader declaration, AsycudaBill messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "H7D00000001";
			manifestHeader.AMA_GB = Branch.PK;

			var bill = manifestHeader.Bills.AddNew();

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var outgoingMessage = Factory.New<TOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			bill.Messages.Add(outgoingMessage);
			return (manifestHeader, bill, outgoingMessage, incomingMessage);
		}

		protected (AsycudaBill messageAttachee, TInboundEDIMessage incomingMessage) SetupAndProcessMessage(string incomingMessageText)
		{
			var (_, _, _, incomingMessage) = CreateSetupData(InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, incomingMessageText, includeResponseWrap: false));
			var processor = Processor;
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			return (incomingMessage.EM_LinkedObject as AsycudaBill, incomingMessage);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			(var declaration, var entry, var outgoingMessage, var incomingMessage) = CreateSetupData();
			incomingMessage.EM_MessageText = "<GREETING>HELLO</GREETING>";
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertEquals("MovementReferenceNumber", ZString.Empty, entry.MovementReferenceNumber);
					AssertEquals("Entry status", string.Empty, entry.ABL_BillStatus);
					AssertEquals("Message status", EDIMessage.Status.Failed, incomingMessage.EM_Status);
				});
			}
		}
	}
}
