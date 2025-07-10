using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public class CustomsAndExciseReportErrorProvider
{
	public CustomsAndExciseReportErrorProvider(MessageAcknowledgement xmlObject)
	{
		this.xmlObject = Argument.NotNull(xmlObject, nameof(this.xmlObject));
	}
	readonly MessageAcknowledgement xmlObject;

	public ZString ErrorCode => xmlObject.ErrorReference?.ErrorCode;
}
