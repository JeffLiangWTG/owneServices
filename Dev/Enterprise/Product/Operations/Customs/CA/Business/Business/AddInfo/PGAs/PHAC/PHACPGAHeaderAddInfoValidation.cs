//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPHACPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoPHACPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class PHACPGAHeaderAddInfoValidation : AutoPHACPGAHeaderAddInfoValidation
	{
		public PHACPGAHeaderAddInfoValidation(AutoPHACPGAHeaderAddInfo parent) : base(parent)
		{
		}

		protected override void CheckCA_Category()
		{
			base.CheckCA_Category();
			if (Parent.CA_HAPProgramInd == Customs.Business.YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_CategoryInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_CategoryInfo, Parent.Lookups.Categories);
			}
		}

		protected override void CheckCA_IntendedUseCode()
		{
			base.CheckCA_IntendedUseCode();
			if (Parent.CA_HAPProgramInd == Customs.Business.YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_IntendedUseCodeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_IntendedUseCodeInfo, Parent.Lookups.IntendedUseCodes);
			}
		}
	}
}
