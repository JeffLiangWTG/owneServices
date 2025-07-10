using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class OutboundMessageInterpreter : BaseMessageInterpreter<MessageProvider>, IMessageInterpreter
	{
		public OutboundMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, MessageProvider provider, IXmlMessageBuilder builder) : base(message, provider)
		{
			this.builder = Argument.NotNull(builder, "builder");
		}
		protected readonly IXmlMessageBuilder builder;

		public ZString GetInterpretation() => (ZString)$"<html><body><xmp>{builder.GenerateXmlMessage().GetSerializedString()}</xmp></body></html>";
	}
}
