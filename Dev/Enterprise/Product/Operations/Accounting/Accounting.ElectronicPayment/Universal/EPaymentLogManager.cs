using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	internal static class EPaymentLogManager
	{
		internal static void AddLogToPaymentApproval(IEPaymentLogParent logParent, IXmlEventValueObject eventDataObject, IEDIMessage message)
		{
			var parentLog = logParent.Logs.Find(l => !l.IsInDatabase && l.SL_SE_NKEvent == eventDataObject.EventType).FirstOrDefault();
			if (parentLog != null)
			{
				var eventType = Events.All[eventDataObject.EventType];
				var paymentApprovalLog = logParent.PaymentApprovalForLogging?.Logs.AddNew(eventType, parentLog.SL_Reference, parentLog.SL_EventTimeOffset, parentLog.SL_IsEstimate, parentLog.Parameters.ToArray());
				if (paymentApprovalLog != null)
				{
					message.AddUniversalDataLink(paymentApprovalLog);
				}
			}
		}

		internal static void SetEPaymentLogReference(IEPaymentLogParent logParent, Event eventType, ZString reference)
		{
			var formattedReference = reference.Length > StmALogSchema.SL_Reference.MaxLength ? reference.Substring(0, StmALogSchema.SL_Reference.MaxLength) : reference;
			SetEPaymentLogReferenceCore(logParent.Logs, eventType, formattedReference);
			SetEPaymentLogReferenceCore(logParent.PaymentApprovalForLogging.Logs, eventType, formattedReference);
		}

		static void SetEPaymentLogReferenceCore(Logs logs, Event eventType, ZString formattedReference)
		{
			var logToUpdate = logs.Find(l => !l.IsInDatabase && l.SL_SE_NKEvent == eventType.Code).FirstOrDefault();
			logToUpdate?.UpdateReference(formattedReference);
		}
	}
}
