//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingOrganisationValidation
//
//    This class should be used for overriding validation in AutoNettingOrganisationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingOrganisationValidation : AutoNettingOrganisationValidation
	{
		public NettingOrganisationValidation(AutoNettingOrganisation parent) : base(parent)
		{
		}
	}
}
