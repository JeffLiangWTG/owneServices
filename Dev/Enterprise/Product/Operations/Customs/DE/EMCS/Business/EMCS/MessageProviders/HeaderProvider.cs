using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public abstract class HeaderProvider : IEMCSHeader
	{
		protected HeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
		{
			this.emcsJobDeclaration = Argument.NotNull(emcsJobDeclaration, nameof(emcsJobDeclaration));
			headerProviderHelper = new HeaderProviderHelper(emcsJobDeclaration);
		}
		protected readonly EMCSJobDeclaration emcsJobDeclaration;
		readonly HeaderProviderHelper headerProviderHelper;

		public string AdministrativeReferenceCode => headerProviderHelper.AdministrativeReferenceCode;

		protected IEMCSPartyGuarantor GetGuarantorTrader()
		{
			IEMCSPartyGuarantor guarantorTrader = null;
			switch (emcsJobDeclaration.ZG_GuarantorType)
			{
				case EmcsGuarantorTypeList.Codes.Transporter:
					guarantorTrader = PartyGuarantorProvider.NewOrNull(emcsJobDeclaration.TransporterDocumentaryAddress);
					break;
				case EmcsGuarantorTypeList.Codes.OwnerOfTheExciseProducts:
					guarantorTrader = PartyGuarantorProvider.NewOrNull(emcsJobDeclaration.OwnerDocumentaryAddress);
					break;
			}
			return guarantorTrader;
		}

		protected ITextAndLanguage GetComplementaryInformationWithLanguage(ZString informationText, string language) => complementaryInformationWithLanguage ?? (complementaryInformationWithLanguage = new TextAndLanguageProvider(informationText, language));
		ITextAndLanguage complementaryInformationWithLanguage;

		protected ITextAndLanguage GetComplementaryInformation(ZString informationText) => complementaryInformation ?? (complementaryInformation = new TextAndLanguageProvider(informationText));
		ITextAndLanguage complementaryInformation;
	}
}
