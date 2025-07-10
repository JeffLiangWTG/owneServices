using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class TransportProvider : IEMCSTransport
	{
		readonly EMCSCusContainer container;
		readonly TransportProviderHelper helper;

		public TransportProvider(EMCSCusContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
			helper = new TransportProviderHelper(container);
		}

		public string UnitCode => helper.UnitCode;

		public string IdentityOfUnit => helper.IdentityOfUnit;

		public string CommercialSealIdentification => helper.CommercialSealIdentification;

		public ITextAndLanguage ComplementaryInformation => comment ?? (comment = new TextAndLanguageProvider(container.Comment));
		ITextAndLanguage comment;

		public ITextAndLanguage SealInformation => sealInformation ?? (sealInformation = new TextAndLanguageProvider(container.SealDetails));
		ITextAndLanguage sealInformation;
	}
}
