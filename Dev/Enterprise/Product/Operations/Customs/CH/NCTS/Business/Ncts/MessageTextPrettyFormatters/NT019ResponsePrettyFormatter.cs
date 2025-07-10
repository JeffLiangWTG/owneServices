using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT019ResponsePrettyFormatter : NCTS019ResponsePrettyFormatter, IMessagePrettyFormatter
{
	public NT019ResponsePrettyFormatter(EDIMessage message) : base(message)
	{
	}

	public ZString GetFormattedText()
	{
		return base.MakeInboundPrettyForPhase5Interpretation();
	}
}
