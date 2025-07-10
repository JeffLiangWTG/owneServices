using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public abstract class HeaderProvider : IEMCSHeader
	{
		protected HeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
		{
			this.emcsJobDeclaration = Argument.NotNull(emcsJobDeclaration, nameof(emcsJobDeclaration));
		}
		protected readonly EMCSJobDeclaration emcsJobDeclaration;

		public string AdministrativeReferenceCode => emcsJobDeclaration.EADNumber;

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

		protected ITextAndLanguage GetComplementaryInformation(ZString informationText, string language) => complementaryInformation ?? (complementaryInformation = new TextAndLanguageProvider(informationText, language));

		protected ITextAndLanguage GetComplementaryInformation(ZString informationText) => complementaryInformation ?? (complementaryInformation = new TextAndLanguageProvider(informationText));

		ITextAndLanguage complementaryInformation;

		protected ZInt DefaultSequenceNumber => 1;

		public bool IsValidationAttributeAllowed => isValidationAttributeAllowed ?? ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.EMCSGB_ValidationAttributeAllowed, CountryCodes.UnitedKingdom, ZDateTime.Now);

		readonly bool? isValidationAttributeAllowed;
	}
}
