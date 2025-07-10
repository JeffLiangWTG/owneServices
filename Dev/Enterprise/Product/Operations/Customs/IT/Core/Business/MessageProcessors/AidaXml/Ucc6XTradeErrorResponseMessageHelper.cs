using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using InterchangeTypes = Enterprise.Customs.IT.Business.MessageProcessorConstants.InterchangeTypes;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6XTradeErrorResponseMessageHelper
{
	internal bool IsSentInterchangeTypeApplicableForMessageProcessing(EDIMessage sentMessage)
	{
		Argument.NotNull(sentMessage, nameof(sentMessage));
		var sentInterchangeType = sentMessage.Interchange?.EI_InterchangeType ?? ZString.Empty;

		return sentInterchangeType == InterchangeTypes.ImportType
			|| sentInterchangeType == InterchangeTypes.ExportType
			|| sentInterchangeType == InterchangeTypes.TransitType;
	}
}
