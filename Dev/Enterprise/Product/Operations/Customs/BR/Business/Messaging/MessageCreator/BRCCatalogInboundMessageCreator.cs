using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCCatalogInboundMessageCreator : BRCInboundMessageCreator
	{
		public BRCCatalogInboundMessageCreator(LoggingInformation logger) : base(logger)
		{
		}

		protected override (ZString Type, ZString SubType) GetMessageTypeAndSubType(EDIInterchange interchange, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			if (universalEventData == null || universalEventData.MessageType == MessageConstants.MessageType.RES)
			{
				var outgoingMessage = BRMessageHelper.GetOutgoingMessage(interchange);
				var outgoingMessageSubType = outgoingMessage?.EM_MessageSubType ?? ZString.Empty;
				switch (outgoingMessageSubType)
				{
					case EDIMessageSubTypeList.Codes.CatalogZipFile:
					case EDIMessageSubTypeList.Codes.ManufacturerZipFile:
					case EDIMessageSubTypeList.Codes.OperatorZipFile:
					case EDIMessageSubTypeList.Codes.Link:
						return (MessageTypeList.Codes.CAT, outgoingMessageSubType);
				}
			}
			return base.GetMessageTypeAndSubType(interchange, responseMessage, universalEventData);
		}

		readonly ZString[] zipMessageSubTypes = new ZString[]
		{
			EDIMessageSubTypeList.Codes.CatalogZipFile,
			EDIMessageSubTypeList.Codes.ManufacturerZipFile,
			EDIMessageSubTypeList.Codes.OperatorZipFile,
			EDIMessageSubTypeList.Codes.Link
		};

		protected override IEnumerable<ZString> GetMessageTexts(ZString messageType, ZString messageSubType, ZString responseMessage, UniversalEventWrapper universalEventData)
		{
			return messageSubType == EDIMessageSubTypeList.Codes.Error ? [responseMessage]
				: BRMessageHelper.DeserializeJsonObjectArray(responseMessage, zipMessageSubTypes.Contains(messageSubType));
		}
	}
}
