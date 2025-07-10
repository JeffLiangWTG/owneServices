using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CDSResponse.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSInventoryLinkingResponseMessageProcessorTests : TestCaseWithFactory
	{
		public void TestFindingJob<T>(ZString message, CDSMessageProcessor<T> processor, LoggingInformation logger) where T : CDSEDIMessage
		{
			ResponseFunctionTests.SetUpZZRefData(Factory);

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B0000001";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.LRN = "1GB1223456789000-B00XXXX";
			Factory.Save();
			var outgoingInterchange = Factory.New<EDIInterchange>();
			ZGuid eHubTrackingId = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = eHubTrackingId;
			outgoingInterchange.EI_HeaderText = "";
			outgoingInterchange.EI_InterchangeNum = "1";
			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = "2";
			var outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			entry.Messages.Add(outgoingSentMessage);
			outgoingSentMessage.EM_MessageText = "";
			outgoingSentMessage.EM_ApplicationReference = "d03f84e3b5894aaaa1f8cafd744add8e";
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingSentMessage.EM_MessageNum = "999";
			outgoingSentMessage.EM_MessageSubType = GbCusDecMessageFunctionsList.Codes.Associate;
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			incomingInterchange.EI_Status = EDIInterchange.Status.Queued;
			incomingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			outgoingSentMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "AAW";
			outgoingInterchange.EI_BodyText = "";
			incomingInterchange.EI_BodyText = string.Format(message, eHubTrackingId);

			var messageWithCID = entry.Messages.OfType<EDIMessage>().FirstOrDefault(x => x.EM_ApplicationReference == "d03f84e3b5894aaaa1f8cafd744add8e");
			AssertNotNull(messageWithCID);

			var ediMessage = incomingInterchange.ContainedMessages.AddNew(typeof(T));
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_ApplicationReference = "d03f84e3b5894aaaa1f8cafd744add8e";
			ediMessage.EM_MessageText = message;
			processor.ProcessMessage(ediMessage);

			CombineAssertions("Find CDS Inventory linking job using conversation ID", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals(GbCusDecMessageFunctionsList.Codes.Associate, ediMessage.EM_MessageSubType);
				AssertCollectionContains(ediMessage, ((CDSEDIMessage)messageWithCID).LinkedEntry.Messages);
				AssertEquals(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}B", outgoingSentMessage.EM_MessageNum), ediMessage.EM_MessageNum);
			});

			ediMessage.Delete();

			ediMessage = incomingInterchange.ContainedMessages.AddNew(typeof(T));
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_MessageText = message;
			processor.ProcessMessage(ediMessage);

			CombineAssertions("Find CDS Inventory linking job using eHub Tracking ID", () =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertEquals(GbCusDecMessageFunctionsList.Codes.Associate, ediMessage.EM_MessageSubType);
				AssertCollectionContains(ediMessage, ((CDSEDIMessage)messageWithCID).LinkedEntry.Messages);
				AssertEquals(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}B", outgoingSentMessage.EM_MessageNum), ediMessage.EM_MessageNum);
			});

			ediMessage.Delete();

			ediMessage = incomingInterchange.ContainedMessages.AddNew(typeof(T));
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_MessageText = message;
			outgoingInterchange.EI_SessionGUID = ZGuid.Empty;

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				processor.ProcessMessage(ediMessage);
			}

			CombineAssertions("No job found - retry", () =>
			{
				var allMessages = string.Join(" ", logger.Logs.Select(x => x.Message));
				AssertContains("Failed to find the corresponding CDS Inventory Linking Request", allMessages);
				AssertContains("Holding message for future re-processing", allMessages);
				AssertCollectionNotContains(ediMessage, ((CDSEDIMessage)messageWithCID).LinkedEntry.Messages);
				AssertEquals(EDIMessageStatusList.Codes.Queued, ediMessage.EM_Status);
				AssertLessThanOrEqualTo("EM_HeldUntilDate eligible to be picked up by CDR task iteration,ZDateTime.UtcNow +1", ediMessage.EM_HeldUntilDate, ZDateTime.UtcNow.AddMinutes(1));
			});

			for (int i = 0; i < 5; i++)
			{
				processor.ProcessMessage(ediMessage);
			}

			CombineAssertions("No job found", () =>
			{
				var allMessages = string.Join(" ", logger.Logs.Select(x => x.Message));
				AssertContains("Failed to find the corresponding CDS Inventory Linking Request", allMessages);
				AssertCollectionNotContains(ediMessage, ((CDSEDIMessage)messageWithCID).LinkedEntry.Messages);
				AssertEquals(EDIMessageStatusList.Codes.Failed, ediMessage.EM_Status);
			});
		}
	}
}
