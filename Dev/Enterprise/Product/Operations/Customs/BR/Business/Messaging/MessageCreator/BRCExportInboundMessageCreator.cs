using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCExportInboundMessageCreator : BRCInboundMessageCreator
	{
		public BRCExportInboundMessageCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override (ZString Type, ZString SubType) GetMessageTypeAndSubType(EDIInterchange interchange, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			var messageType = ZString.Empty;
			var messageSubType = ZString.Empty;

			var xmlDocument = BRMessageHelper.TryParseXML(responseMessage);
			if (xmlDocument == null)
			{
				if (BRMessageHelper.DeserializeObject<Due>(responseMessage, throwExceptionIfOccurs: false) is Due due && !string.IsNullOrEmpty(due.situacao))
				{
					messageType = MessageTypeList.Codes.CDE;
					messageSubType = EDIMessageSubTypeList.Codes.CompleteConsult;
				}
				else
				{
					messageType = MessageTypeList.Codes.CDC;
				}
			}
			else
			{
				if (xmlDocument.Root?.Name == nameof(pucomexReturn))
				{
					messageType = MessageTypeList.Codes.CDE;
					messageSubType = EDIMessageSubTypeList.Codes.Success;
				}
				else if (xmlDocument.Root?.Name == nameof(error))
				{
					messageType = MessageTypeList.Codes.CDE;
					messageSubType = EDIMessageSubTypeList.Codes.Error;
				}
			}

			return (messageType, messageSubType);
		}
	}
}
