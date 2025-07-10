//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceHeaderDiscountValidation
//
//    This class should be used for overriding validation in AutoEdiPriceHeaderDiscountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using CargoWise.EntityFramework;

	public class EdiPriceHeaderDiscountValidation : AutoEdiPriceHeaderDiscountValidation
	{
		public EdiPriceHeaderDiscountValidation(AutoEdiPriceHeaderDiscount parent) : base(parent)
		{
		}

		protected override void CheckPHD_Name()
		{
			MandatoryValidation.CheckEntered(Parent.PHD_NameInfo);

			bool allowActiveOnly = !Parent.IsInDatabase || Parent.PHD_NameInfo.HasChanges;
			ListValidation.ErrorIfInvalidCode(Parent.PHD_NameInfo, allowActiveOnly ? Parent.Lookups.ActiveDiscountNames : Parent.Lookups.AllDiscountNames);
		}

		protected override void CheckPHD_IsDefaultEnabled()
		{
			if (Parent.PHD_IsDefaultEnabled && Parent.PHD_Type == BillingConstants.DiscountCalculator.SingleCountry)
			{
				Parent.PHD_IsDefaultEnabledInfo.AddWarning("If a country discount is active by default then all matching usage in that country is discounted for all customers forever");
			}
		}

		protected override void CheckPHD_Type()
		{
			base.CheckPHD_Type();
			if (Parent.PHD_Type == BillingConstants.DiscountCalculator.ProductBundle)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PHD_TypeInfo,
					new EdiPriceHeaderDiscountCollection(Parent.Factory, Parent.PHD_Version), "Only 1 Bundle discount can be configured against a pricelist.");
			}
		}
	}
}

