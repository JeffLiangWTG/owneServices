using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
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

		protected IReadOnlyCollection<IEMCSPartyGuarantor> GetGuarantorTraders()
		{
			var guarantorTraders = new List<IEMCSPartyGuarantor>();
			var trader = GetGuarantorTrader();
			if (trader != null)
			{
				guarantorTraders.Add(trader);
			}
			return guarantorTraders;
		}

		protected IEMCSPartyGuarantor GetGuarantorTrader()
		{
			switch (emcsJobDeclaration.ZG_GuarantorType)
			{
				case EMCSGuarantorTypeList.Codes.Transporter:
					return PartyGuarantorProvider.NewOrNull(emcsJobDeclaration.TransporterDocumentaryAddress);

				case EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts:
					return PartyGuarantorProvider.NewOrNull(emcsJobDeclaration.OwnerDocumentaryAddress);
			}
			return null;
		}

		protected ITextAndLanguage GetComplementaryInformationWithLanguage(ZString informationText, string language) => complementaryInformationWithLanguage ?? (complementaryInformationWithLanguage = new TextAndLanguageProvider(informationText, language));
		ITextAndLanguage complementaryInformationWithLanguage;

		protected ITextAndLanguage GetComplementaryInformation(ZString informationText) => complementaryInformation ?? (complementaryInformation = new TextAndLanguageProvider(informationText));
		ITextAndLanguage complementaryInformation;
	}
}
