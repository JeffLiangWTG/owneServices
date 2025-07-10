//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentManagementGroupMessageValidation
//
//    This class should be used for overriding validation in AutoIncidentManagementGroupMessageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupMessageValidation : AutoIncidentManagementGroupMessageValidation
	{
		public IncidentManagementGroupMessageValidation(AutoIncidentManagementGroupMessage parent) : base(parent)
		{
		}
	}
}

