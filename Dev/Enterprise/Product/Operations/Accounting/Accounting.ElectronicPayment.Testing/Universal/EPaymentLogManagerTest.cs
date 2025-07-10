using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Universal
{
	public class EPaymentLogManagerTest : TestCaseWithFactory
	{
		public void TestAddLogToPaymentApproval_Quote() => AssertAddLogToPaymentApproval(Factory.NewWithValidTestData<AccEPaymentQuote>());

		public void TestAddLogToPaymentApproval_Deal() => AssertAddLogToPaymentApproval(Factory.NewWithValidTestData<AccEPaymentDeal>());

		void AssertAddLogToPaymentApproval(IEPaymentLogParent logParent)
		{
			var universalEvent = new UniversalEventDataObject();
			universalEvent.CreatedTime = ZDateTimeOffset.Today;
			universalEvent.EventReference = "Test Reference";
			universalEvent.EventTime = ZDateTimeOffset.Today.AddHours(-2);
			universalEvent.EventType = AutoEvents.InterchangeAcknowledgedCode;
			universalEvent.IsEstimate = ZBool.True;
			universalEvent.IsCancelled = ZBool.False;

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			AssertNull("Log for parent", FindMatchingLog(logParent.Logs));
			AssertNull("Log for payment approval", FindMatchingLog(logParent.PaymentApprovalForLogging.Logs));

			EPaymentLogManager.AddLogToPaymentApproval(logParent, universalEvent, message);

			AssertNull("Log for parent", FindMatchingLog(logParent.Logs));
			AssertNull("Log for payment approval", FindMatchingLog(logParent.PaymentApprovalForLogging.Logs));

			logParent.Logs.AddNew(AutoEvents.InterchangeAcknowledged);
			Factory.Save();

			EPaymentLogManager.AddLogToPaymentApproval(logParent, universalEvent, message);

			AssertNotNull("Log for parent", FindMatchingLog(logParent.Logs));
			AssertNull("Log for payment approval", FindMatchingLog(logParent.PaymentApprovalForLogging.Logs));

			logParent.Logs.AddNew(AutoEvents.InterchangeAcknowledged);

			EPaymentLogManager.AddLogToPaymentApproval(logParent, universalEvent, message);

			var parentLog = FindMatchingLog(logParent.Logs);
			var paymentApprovalLog = FindMatchingLog(logParent.PaymentApprovalForLogging.Logs);
			AssertNotNull("Log for parent", parentLog);
			AssertNotNull("Log for payment approval", paymentApprovalLog);
			AssertNotNull("Payment approval log should be linked to EDI Message", paymentApprovalLog.RelatedEDIMessage.Message);
		}

		StmALog FindMatchingLog(Logs logs) => logs.Find(l => l.SL_SE_NKEvent == AutoEvents.InterchangeAcknowledgedCode).FirstOrDefault();

		public void TestSetEPaymentLogReference_Quote() => AssertSetEPaymentLogReference(Factory.NewWithValidTestData<AccEPaymentQuote>());

		public void TestSetEPaymentLogReference_Deal() => AssertSetEPaymentLogReference(Factory.NewWithValidTestData<AccEPaymentDeal>());

		void AssertSetEPaymentLogReference(IEPaymentLogParent logParent)
		{
			logParent.Logs.AddNew(AutoEvents.InterchangeAcknowledged);
			logParent.PaymentApprovalForLogging.Logs.AddNew(AutoEvents.InterchangeAcknowledged);
			Factory.Save();
			logParent.Logs.AddNew(AutoEvents.InterchangeAcknowledged);
			logParent.PaymentApprovalForLogging.Logs.AddNew(AutoEvents.InterchangeAcknowledged);

			var expectedReference1 = "E-Payment is accepted";
			var expectedReference2 = "";
			EPaymentLogManager.SetEPaymentLogReference(logParent, AutoEvents.InterchangeAcknowledged, expectedReference1);
			AssertContainsExactElementsInAnyOrder(new string[] { expectedReference1, expectedReference2 }, logParent.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == "IAK").Select(l => l.SL_Reference));
			AssertContainsExactElementsInAnyOrder(new string[] { expectedReference1, expectedReference2 }, logParent.PaymentApprovalForLogging.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == "IAK").Select(l => l.SL_Reference));
		}
	}
}
