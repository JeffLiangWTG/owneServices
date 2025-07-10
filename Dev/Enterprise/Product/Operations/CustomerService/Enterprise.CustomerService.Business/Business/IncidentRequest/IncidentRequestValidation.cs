//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoIncidentRequestValidation
//
//    This class should be used for overriding validation in AutoIncidentRequestValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CustomerService.Business
{
	public class IncidentRequestValidation : AutoIncidentRequestValidation
	{
		public IncidentRequestValidation(AutoIncidentRequest parent) : base(parent)
		{
		}
	}
}
