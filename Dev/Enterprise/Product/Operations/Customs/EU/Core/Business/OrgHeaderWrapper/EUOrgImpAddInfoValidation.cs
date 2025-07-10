//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEUOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoEUOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class EUOrgImpAddInfoValidation : AutoEUOrgImpAddInfoValidation
	{
		public EUOrgImpAddInfoValidation(AutoEUOrgImpAddInfo parent) : base(parent)
		{
		}

		protected override void CheckZO_OtherDeferType()
		{
			base.CheckZO_OtherDeferType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZO_OtherDeferTypeInfo);
		}
	}
}
