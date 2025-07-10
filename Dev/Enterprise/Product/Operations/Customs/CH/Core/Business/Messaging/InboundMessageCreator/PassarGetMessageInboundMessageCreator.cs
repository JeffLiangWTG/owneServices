using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class PassarGetMessageInboundMessageCreator : UniversalEventInboundMessageCreator
{
	public PassarGetMessageInboundMessageCreator(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetMessageSubTypeFromUniversalEvent(UniversalEventWrapper eventData)
	{
		if (eventData.EventType == AutoEvents.InterchangeAcknowledgedCode)
		{
			var messageSubType = MessageSubTypeCodeList.Codes.Undefined;

			var responseMessage = (ZString)eventData.GetResponseMessage();
			if (!responseMessage.IsEmpty)
			{
				try
				{
					messageSubType = MessageSchemaDecider.GetSpecificMessageAnalyzer<IPassarResponseAnalyzer>(responseMessage).GetMessageSubType();
				}
				catch (NotSupportedException ex)
				{
					LoggingInformation.LogWarning(ex.Message);
				}
			}
			return messageSubType;
		}
		else
		{
			return base.GetMessageSubTypeFromUniversalEvent(eventData);
		}
	}
}
