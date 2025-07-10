using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public abstract class NctsHeaderMessageProvider : MessageProvider
	{
		protected NctsHeaderMessageProvider(NctsHeader nctsHeader)
		{
			NctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}
		public readonly NctsHeader NctsHeader;

		protected NctsHeaderMessageProvider(CusGuaranteeHeader cusGuaranteeHeader)
		{
			CusGuaranteeHeader = Argument.NotNull(cusGuaranteeHeader, nameof(cusGuaranteeHeader));
		}
		public readonly CusGuaranteeHeader CusGuaranteeHeader;

		public IMessageInterpreter GetMessageInterpreter(BaseEDIMessage message, IXmlMessageBuilder builder) => GetMessageInterpreterCore(message, builder);

		protected virtual IMessageInterpreter GetMessageInterpreterCore(BaseEDIMessage message, IXmlMessageBuilder builder) => new OutboundMessageInterpreter(message, this, builder);
	}
}
