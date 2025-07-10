using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class LegalActInfoValidation : Customs.Business.CusSupportingInfoValidation
	{
		public LegalActInfoValidation(LegalActInfo parent) : base(parent)
		{
		}

		public new LegalActInfo Parent => (LegalActInfo)base.Parent;

		JobComInvoiceLine invoiceLine => Parent.Parent as JobComInvoiceLine;

		bool IsIPITaxBenefitsRequireLegalAct => invoiceLine != null && Parent.CSI_SubType == AdditionalTaxTypeList.Codes.IPITaxBenefit && ipiTaxRegimesRequireLegalAct.Contains(invoiceLine.IPITaxRegime);

		bool RequiresLegalAct => invoiceLine != null && (Parent.CSI_SubType == AdditionalTaxTypeList.Codes.ExIPITariff
					|| (IsIPITaxBenefitsRequireLegalAct
					|| (Parent.CSI_SubType == AdditionalTaxTypeList.Codes.ExDutyTariff && invoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.ExTariff && invoiceLine.DutyRateIsOverridden)));

		readonly ZString[] ipiTaxRegimesRequireLegalAct = new ZString[] { IPITaxRegimeList.Codes.Suspension, IPITaxRegimeList.Codes.Reduction, IPITaxRegimeList.Codes.Exemption };

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCSI_YearOfIssue();
		}

		protected override void CheckCSI_DateOfIssueIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.CSI_DateOfIssueInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			AddNotificationIfNotEntered(Parent.CSI_ReferenceNumberInfo);

			if (!Parent.CSI_ReferenceNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("DF66F5FE-A5CF-4CA5-A165-C835C6DE18CD", "Act Number must be numeric."));
			}
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			AddNotificationIfNotEntered(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_IssuerType()
		{
			base.CheckCSI_IssuerType();
			AddNotificationIfNotEntered(Parent.CSI_IssuerTypeInfo);
		}

		public void ValidateCSI_YearOfIssue()
		{
			ValidateCalculatedProperty(Parent.CSI_YearOfIssueInfo);
		}

		protected void CheckCSI_YearOfIssue()
		{
			AddNotificationIfNotEntered(Parent.CSI_YearOfIssueInfo);
		}

		void AddNotificationIfNotEntered(ZPropertyInfo zPropertyInfo)
		{
			if (IsIPITaxBenefitsRequireLegalAct)
			{
				MandatoryValidation.WarnIfNotEntered(zPropertyInfo);
			}
			else if (RequiresLegalAct)
			{
				MandatoryValidation.MessageErrorIfNotEntered(zPropertyInfo);
			}
		}
	}
}
