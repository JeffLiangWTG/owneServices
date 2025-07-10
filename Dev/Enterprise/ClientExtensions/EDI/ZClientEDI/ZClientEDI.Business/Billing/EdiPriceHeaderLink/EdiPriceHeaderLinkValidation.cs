//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceHeaderLinkValidation
//
//    This class should be used for overriding validation in AutoEdiPriceHeaderLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderLinkValidation : AutoEdiPriceHeaderLinkValidation
	{
		public EdiPriceHeaderLinkValidation(AutoEdiPriceHeaderLink parent) : base(parent)
		{
		}

		protected override void CheckPHL_RX_NKCurrency()
		{
			base.CheckPHL_RX_NKCurrency();
			MandatoryValidation.CheckEntered(Parent.PHL_RX_NKCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PHL_RX_NKCurrencyInfo);
		}

		protected override void CheckPHL_ValidFromIsValidZDateTimeRange()
		{
		}

		protected override void CheckPHL_ValidToIsValidZDateTimeRange()
		{
		}

		protected override void CheckPHL_ValidTo()
		{
			base.CheckPHL_ValidTo();
			if (!Parent.PHL_ValidTo.IsEmpty)
			{
				CompareValidation.CheckDateIsAfterAnotherDate(Parent.PHL_ValidToInfo, Parent.PHL_ValidFrom);
			}
		}

		protected override void CheckPHL_VolumeCode()
		{
			base.CheckPHL_VolumeCode();
			MandatoryValidation.CheckEntered(Parent.PHL_VolumeCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PHL_VolumeCodeInfo);
		}

		protected override void CheckPHL_CorePackCode()
		{
			base.CheckPHL_CorePackCode();
			MandatoryValidation.CheckEntered(Parent.PHL_CorePackCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PHL_CorePackCodeInfo);
		}

		protected override void CheckPHL_CoreUpliftPercent()
		{
			base.CheckPHL_CoreUpliftPercent();
			CompareValidation.CheckWithinRange(Parent.PHL_CoreUpliftPercentInfo, -99m, 500m);
		}

		protected override void CheckPHL_L6()
		{
			base.CheckPHL_L6();
			if (!Parent.PHL_L6Info.HasErrors() && PriceHeaderType.IsGlobal(Parent.PriceHeader?.L6_SystemCode ?? ""))
			{
				Parent.PHL_L6Info.AddError("Universal Pricelist cannot be setup as a client pricelist.");
			}
		}

		new EdiPriceHeaderLink Parent
		{
			get { return (EdiPriceHeaderLink)base.Parent; }
		}
	}
}

