using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationAdditionLegalActProvider : IDeclarationAdditionLegalAct
	{
		public DeclarationAdditionLegalActProvider(LegalActInfo legalAct)
		{
			this.legalAct = Argument.NotNull(legalAct, nameof(legalAct));
		}

		public DeclarationAdditionLegalActProvider(AdditionalTariff additionalTariff)
		{
			this.additionalTariff = Argument.NotNull(additionalTariff, nameof(additionalTariff));
			legalAct = additionalTariff.LegalAct;
		}
		readonly AdditionalTariff additionalTariff;
		readonly LegalActInfo legalAct;

		public static DeclarationAdditionLegalActProvider New(LegalActInfo legalAct) => (legalAct?.IsEmpty ?? true) ? null : new DeclarationAdditionLegalActProvider(legalAct);

		public static DeclarationAdditionLegalActProvider New(AdditionalTariff additionalTariff) => additionalTariff == null ? null : new DeclarationAdditionLegalActProvider(additionalTariff);

		public string LegalActSubject
		{
			get
			{
				var subject = legalAct.CSI_SubType;

				if (subject == AdditionalTaxTypeList.Codes.TariffAgreement)
				{
					var tariffAgreementType = additionalTariff?.TariffAgreementCode?.GetAttribute(Constants.RefCusCodeList.Attributes.Type) ?? string.Empty;

					if (tariffAgreementType == Constants.TariffAgreementTypes.SGPC || tariffAgreementType == Constants.TariffAgreementTypes.OMC)
					{
						subject = "5";
					}
					else if (tariffAgreementType == Constants.TariffAgreementTypes.Aladi)
					{
						subject = AdditionalTaxTypeList.Codes.TariffAgreement;
					}
					else
					{
						subject = string.Empty;
					}
				}
				return subject;
			}
		}

		public string LegalActType => legalAct.CSI_Code;

		public string LegalActNumber => legalAct.CSI_ReferenceNumber;

		public string LegalActIssuingBody => legalAct.CSI_IssuerType;

		public string LegalActYear => legalAct.CSI_YearOfIssue;

		public string ExNumber => additionalTariff?.ExNumber ?? string.Empty;
	}
}

