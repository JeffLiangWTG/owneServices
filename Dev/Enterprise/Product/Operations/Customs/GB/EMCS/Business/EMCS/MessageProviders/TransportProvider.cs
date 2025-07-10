using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class TransportProvider : IEMCSTransport
	{
		readonly EMCSCusContainer container;

		public TransportProvider(EMCSCusContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}

		public string UnitCode => container.ZG_UnitCode;

		public string IdentityOfUnit => container.CO_ContainerNumber;

		public string CommercialSealIdentification => container.CO_Seal;

		public ITextAndLanguage ComplementaryInformation => comment ?? (comment = new TextAndLanguageProvider(container.Comment));
		ITextAndLanguage comment;

		public ITextAndLanguage SealInformation => sealInformation ?? (sealInformation = new TextAndLanguageProvider(container.SealDetails));
		ITextAndLanguage sealInformation;
	}
}
