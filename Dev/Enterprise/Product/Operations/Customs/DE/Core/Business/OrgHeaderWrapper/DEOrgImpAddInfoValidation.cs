//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDEOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoDEOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class DEOrgImpAddInfoValidation : AutoDEOrgImpAddInfoValidation
	{
		public DEOrgImpAddInfoValidation(AutoDEOrgImpAddInfo parent) : base(parent)
		{
		}

		protected override void CheckZO_VATClaimBack()
		{
			base.CheckZO_VATClaimBack();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZO_VATClaimBackInfo);
		}
	}
}
