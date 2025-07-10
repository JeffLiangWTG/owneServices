using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Business
{
	public class GBAddInfoTaxValidation : EU.Business.Declaration.MultiLineAddInfos.EUAddInfoTaxValidation
	{
		public GBAddInfoTaxValidation(Tax_OnlyForPivot parent) : base(parent)
		{
		}

		TaxValidationHelper CommonValidation => TaxValidationHelper.GetTaxValidationHelper(Parent, Parent.Pivot.IEuTaxesForValidation);// Don't cache - need to see latest version of .Taxes each time

		protected new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

		protected override void CheckG4_MethodOfPayment()
		{
			base.CheckG4_MethodOfPayment();
			CommonValidation.CheckTaxTypeAndMop(Parent.G4_MethodOfPaymentInfo);
		}

		protected override void CheckG4_RateSuspension()
		{
			base.CheckG4_RateSuspension();
			ListValidation.MessageErrorIfInvalidCode(Parent.G4_RateSuspensionInfo, Parent.Lookups.RateSuspensionList);
		}

		protected override void CheckG4_RateOverride()
		{
			base.CheckG4_RateOverride();
			ListValidation.MessageErrorIfInvalidCode(Parent.G4_RateOverrideInfo, Parent.Lookups.RateOverrideList);
		}
	}
}
