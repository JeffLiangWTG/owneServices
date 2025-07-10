//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoInvestigationItemResponseOptionsValidation
//
//    This class should be used for overriding validation in AutoInvestigationItemResponseOptionsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class InvestigationItemResponseOptionValidation : AutoInvestigationItemResponseOptionValidation
	{
		public InvestigationItemResponseOptionValidation(AutoInvestigationItemResponseOption parent) : base(parent)
		{
		}
	}
}
