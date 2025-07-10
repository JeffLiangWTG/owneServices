using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;

namespace Enterprise.Customs.GB.CDS
{
	public abstract partial class CDSInventoryLinkingRequestMessageBuilder : IGbCDSMessageBuilder
	{
		protected CDSInventoryLinkingRequestMessageBuilder(IUkCinvWrapper messageDataProvider)
		{
			this.messageDataProvider = Argument.NotNull(messageDataProvider, nameof(messageDataProvider));
		}

		ZString IGbCDSMessageBuilder.Build() => Build();

		protected abstract ZString Build();

		protected readonly IUkCinvWrapper messageDataProvider;
	}
}
