using System;
using System.Linq;
using CargoWise.EntityFramework;
using static Enterprise.Customs.BR.Business.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class AdditionalTariffValidation : ZValidation
	{
		public AdditionalTariffValidation(AdditionalTariff additionalTariff) : base(additionalTariff)
		{
			parent = additionalTariff;
		}
		readonly AdditionalTariff parent;

		public override Type AutoValidationType => typeof(AdditionalTariffValidation);

		public override void ValidateAll()
		{
			parent.LegalAct.Validation.ValidateAll();
			ValidateTariffType();
			ValidateLegalActSubject();
			ValidateExNumber();
		}

		public void ValidateTariffType()
		{
			ValidateCalculatedProperty(parent.TariffTypeInfo);
		}

		protected void CheckTariffType()
		{
			var targetInfo = parent.TariffTypeInfo;

			ListValidation.MessageErrorIfInvalidCode(targetInfo);

			if (parent.Parent is JobComInvoiceLine invoiceLine)
			{
				if (!(parent.LegalActSubject == AdditionalTaxTypeList.Codes.ExDutyTariff && invoiceLine.JI_PrimaryPreference == RatePreferenceType.ExTariff && invoiceLine.DutyRateIsOverridden))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		public void ValidateLegalActSubject()
		{
			ValidateCalculatedProperty(parent.LegalActSubjectInfo);
		}

		protected void CheckLegalActSubject()
		{
			var targetInfo = parent.LegalActSubjectInfo;

			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);

			if (!parent.LegalActSubject.IsEmpty && parent.Parent?.AdditionalTariffs.Cast<AdditionalTariff>().Count(x => x.LegalActSubject == parent.LegalActSubject) > 1)
			{
				targetInfo.AddError(Res.GetString("bb6eb991-a8b4-4b7d-bf93-37e7ee578143", "You have entered a duplicate Legal Act Subject"));
			}
		}

		public void ValidateExNumber()
		{
			ValidateCalculatedProperty(parent.ExNumberInfo);
		}

		protected void CheckExNumber()
		{
			if (parent.Parent is JobComInvoiceLine invoiceLine)
			{
				if ((parent.LegalActSubject == AdditionalTaxTypeList.Codes.ExDutyTariff && invoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.ExTariff)
					|| parent.LegalActSubject == AdditionalTaxTypeList.Codes.ExIPITariff)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.ExNumberInfo);
				}
			}
		}
	}
}
