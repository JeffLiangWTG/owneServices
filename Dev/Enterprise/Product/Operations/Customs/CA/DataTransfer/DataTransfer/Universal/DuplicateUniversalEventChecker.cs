using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using EDIMessage = Enterprise.Customs.CA.Business.EDIMessage;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	static class DuplicateUniversalEventChecker
	{
		public static ZString GetApplicationReference(UniversalEvent eventDataObject)
		{
			var eventTime = eventDataObject.GetEventTime().ToZDateTime();
			var interchangeNumber = eventDataObject.GetContextValueByType(UniversalEventMessageProcessorConstants.ContextType.InterchangeNumber);
			var messageNumber = eventDataObject.GetContextValueByType(UniversalEventMessageProcessorConstants.ContextType.MessageNumber);
			var applicationReference = eventTime.ToLongTimeString() + interchangeNumber.PadLeft(4, '0') + messageNumber.PadLeft(4, '0');
			return applicationReference;
		}

		public static ZBool CheckDuplicateMessages(ZString applicationReference, BusinessObjectFactory factory, string[] subTypes)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.UniversalDataMessaging);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, applicationReference);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			query.AddToFilter(EDIMessageSchema.EM_Status, new[] { EDIMessage.Status.ProcessedOK, EDIMessage.Status.Rejected });
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, subTypes);
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			return factory.Exists(typeof(EDIMessage), query);
		}
	}
}
