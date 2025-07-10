using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRInterchange))]
	public class CMRInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShouldSendViaEHub()
		{
			var interchange = Factory.New<CMRInterchange>();
			Assert("ShouldSendViaEHub", interchange.ShouldSendViaEHub);
		}

		public void TestNewInterchangeStatus()
		{
			var interchange = Factory.New<CMRInterchange>();
			interchange.EI_Status = EDIInterchange.Status.eHubPending;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			AssertEquals("No adjustment of intended status by default", EDIInterchange.Status.Error, interchange.NewInterchangeStatus(EDIInterchange.Status.Error));
			AssertEquals("No adjustment of intended status by default", EDIInterchange.Status.Sent, interchange.NewInterchangeStatus(EDIInterchange.Status.Sent));
			eHubMessagingRegistry.Instance.AUSuppressResends.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Not SND so No adjustment of intended status", EDIInterchange.Status.Error, interchange.NewInterchangeStatus(EDIInterchange.Status.Error));
			AssertEquals("SNT changed to PND", EDIInterchange.Status.SendPending, interchange.NewInterchangeStatus(EDIInterchange.Status.Sent));
		}

		public void TestNeedsAcknowledgement()
		{
			const string bodyTextForCONTRLInterchange = "UNH+000001+CONTRL:D:3:UN'UCI+94+AAL377A::AAL377A+A";
			AssertNeedsAcknowledgement(true);
			AssertNeedsAcknowledgement(false, body: bodyTextForCONTRLInterchange);

			AssertNeedsAcknowledgement(true, setSessionGuid: true);
			AssertNeedsAcknowledgement(false, setSessionGuid: true, body: bodyTextForCONTRLInterchange);

			AssertNeedsAcknowledgement(true, registryItemValue: true);
			AssertNeedsAcknowledgement(false, registryItemValue: true, body: bodyTextForCONTRLInterchange);

			AssertNeedsAcknowledgement(false, registryItemValue: true, setSessionGuid: true);
			AssertNeedsAcknowledgement(false, registryItemValue: true, setSessionGuid: true, body: bodyTextForCONTRLInterchange);
		}

		public void AssertNeedsAcknowledgement(bool result, bool setSessionGuid = false, string body = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+", bool registryItemValue = false)
		{
			eHubMessagingRegistry.Instance.AUSuppressCONTRLAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItemValue);
			var interchange = Factory.New<CMRInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Received;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			if (setSessionGuid)
			{
				interchange.EI_SessionGUID = interchange.PK;
			}

			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			interchange.EI_BodyText = body;
			string comment = string.Format("{0} acknowledgement. Application Code: {1}, Is an eHubID: {2}, Body: {3}, Registry item value: {4}",
				result ? "Needs" : "Does NOT need", interchange.EI_ApplicationCode, (!interchange.eHubID.IsEmpty).ToString(), interchange.EI_BodyText, eHubMessagingRegistry.Instance.AUSuppressCONTRLAcknowledgements.Value.ToString());
			AssertEquals(comment, result, interchange.EI_NeedsAcknowledgement);
		}

		public void TestCorrectMessageTypesAreCreated1()
		{
			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, incomingCONTRLInterchange);
			AssertEquals("Interchange Type", typeof(CMRInterchange), interchange.GetType());
			AssertEquals("Message Count", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(CMRCONTRLMessage), interchange.ContainedMessages[0].GetType());
		}

		public void TestCorrectMessageTypesAreCreated2()
		{
			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, incomingICSInterchange);
			AssertEquals("Interchange Type", typeof(CMRInterchange), interchange.GetType());
			AssertEquals("Message Count", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(CMRAIRCRRMessage), interchange.ContainedMessages[0].GetType());
		}

		public void TestCorrectMessageTypesAreCreated3()
		{
			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, incomingICSInterchange);
			AssertEquals("Interchange Type", typeof(CMRInterchange), interchange.GetType());
			AssertEquals("Message Count", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(CMRAIRCRRMessage), interchange.ContainedMessages[0].GetType());
		}

		public void TestCorrectMessageNumAssigned()
		{
			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, incomingCONTRLInterchange);
			AssertEquals("Message Num", "00000000000001000001", interchange.ContainedMessages[0].EM_MessageNum);
		}

		public void TestMarkAsFailedToBeSent()
		{
			var interchange = Factory.NewWithValidTestData<CMRInterchange>();
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.SendPending;
			var message = Factory.NewWithValidTestData<CMRIMDMessage>();
			interchange.ContainedMessages.Add(message);
			message.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			interchange.MarkAsFailedToBeSent("Max Resends exceeded");
			Factory.Save();

			AssertEquals("EI_Status should be Failed", EDIInterchange.Status.Failed, interchange.EI_Status);
			AssertEquals("Message should be marked Failed", EDIMessage.Status.Failed, message.EM_Status);

			var interchangeLogs = interchange.Logs;
			AssertContains("Interchange StatusUpdated log entry", "|NEW=FAL|OLD=PND", interchangeLogs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated).First().SL_Reference);
			AssertEquals("Interchange has InterchangeFailedToBeSent log entry", "Max Resends exceeded", interchangeLogs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.InterchangeFailedToBeSent).First().SL_Reference);

			var messageLogs = message.Logs;
			AssertContains("Message StatusUpdated log entry", "|NEW=FAL|OLD=SNT", messageLogs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated).First().SL_Reference);
			AssertNotNull("Message has InterchangeFailedToBeSent log entry", messageLogs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.InterchangeFailedToBeSent).FirstOrDefault());
		}

		public void TestResetToQueuedFromFailed()
		{
			var interchange = Factory.NewWithValidTestData<CMRInterchange>();
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.SendPending;
			var message = Factory.NewWithValidTestData<CMRIMDMessage>();
			interchange.ContainedMessages.Add(message);
			message.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			interchange.EI_RetryCount = 3;
			interchange.MarkAsFailedToBeSent(ZString.Empty);
			Factory.Save();

			AssertEquals("EI_Status should be Failed", EDIInterchange.Status.Failed, interchange.EI_Status);
			AssertEquals("Message should be marked Failed", EDIMessage.Status.Failed, message.EM_Status);

			interchange.ResetToQueuedStatus();
			Factory.Save();

			AssertEquals("EI_Status should be SendPending", EDIInterchange.Status.SendPending, interchange.EI_Status);
			AssertEquals("EI_RetryCount should be 0", 0, interchange.EI_RetryCount);
			AssertEquals("Message should be marked Sent", EDIMessage.Status.Sent, message.EM_Status);

			var interchangeStatusUpdates = interchange.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated);
			AssertContains("Interchange StatusUpdated log entry", "|NEW=PND|OLD=FAL", interchangeStatusUpdates.First().SL_Reference);
			AssertEquals(2, interchangeStatusUpdates.Count());

			var messageStatusUpdates = message.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated);
			AssertContains("Message StatusUpdated log entry", "|NEW=SNT|OLD=FAL", messageStatusUpdates.First().SL_Reference);
			AssertEquals(2, messageStatusUpdates.Count());
		}

		public void TestResetToQueuedFromSent()
		{
			var interchange = Factory.NewWithValidTestData<CMRInterchange>();
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			var message = Factory.NewWithValidTestData<CMRIMDMessage>();
			interchange.ContainedMessages.Add(message);
			message.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();

			interchange.EI_RetryCount = 1;
			interchange.EI_Status = EDIInterchange.Status.Sent;
			Factory.Save();

			interchange.ResetToQueuedStatus();
			Factory.Save();

			AssertEquals("EI_Status should be eHubQueued", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals("EI_RetryCount should be reset", 0, interchange.EI_RetryCount);
			AssertEquals("Message should still be marked Sent", EDIMessage.Status.Sent, message.EM_Status);
		}

		#region Implementation

		readonly string incomingCONTRLInterchange = "UNA:+.? 'UNB+UNOC:3+AAA336C::AAA336C+AAA347M+041210:1319+00000000000001'UNH+000001+CONTRL:D:3:UN'UCI+1+AAA347M::AAA347M+AAA336C+4+27+UNB'UNT+3+000001'UNZ+1+00000000000001'";
		readonly string incomingICSInterchange = "UNA:+.? 'UNB+UNOC:3+AAA336C::AAA336C+AAA374M+041210:1354+00000000261664++++1++1'UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+CCF_AAA374M_160476_ACR_1:1+11'NAD+MR+AAA374M:110:95'RFF+ACW:AIRCR'RFF+AFM:9'RFF+ABO:S00039433/1::1'DTM+310:20041210025409:204'ERP+1'ERC+CCFERROR:80:95'ERC+573:6:95'FTX+AAO+++The mandatory field TYPEOFPAYMENTIND is missing'CNT+55:1'UNT+13+000001'UNZ+1+00000000261664'";

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(CMRInterchange));
		}

		#endregion
	}
}
