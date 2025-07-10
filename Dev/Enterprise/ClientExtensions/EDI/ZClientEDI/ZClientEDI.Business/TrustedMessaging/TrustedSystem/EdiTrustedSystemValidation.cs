//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiTrustedSystemValidation
//
//    This class should be used for overriding validation in AutoEdiTrustedSystemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	using CargoWise.EntityFramework;

	public class EdiTrustedSystemValidation : AutoEdiTrustedSystemValidation
	{
		public EdiTrustedSystemValidation(AutoEdiTrustedSystem parent) : base(parent)
		{
		}

		protected override void CheckETS_Product()
		{
			base.CheckETS_Product();

			if (!Parent.IsInDatabase || Parent.ETS_ProductInfo.HasChanges)
			{
				MandatoryValidation.CheckEntered(Parent.ETS_ProductInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ETS_ProductInfo);
			}
		}

		protected override void CheckETS_AccessTokenExpiryOverride()
		{
			base.CheckETS_AccessTokenExpiryOverride();
			MandatoryValidation.CheckNotNegative(Parent.ETS_AccessTokenExpiryOverrideInfo);
		}
	}
}
