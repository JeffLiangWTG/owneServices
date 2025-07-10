//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentManagementLinkValidation
//
//    This class should be used for overriding validation in AutoIncidentManagementLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementLinkValidation : AutoIncidentManagementLinkValidation
	{
		public IncidentManagementLinkValidation(AutoIncidentManagementLink parent) : base(parent)
		{
		}
	}
}

