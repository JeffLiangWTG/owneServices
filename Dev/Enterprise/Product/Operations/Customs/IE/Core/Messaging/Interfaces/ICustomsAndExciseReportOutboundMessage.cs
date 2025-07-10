using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public interface ICustomsAndExciseReportOutboundMessage
	{
		ZDateTime MessageDate { get; }
	}
}
