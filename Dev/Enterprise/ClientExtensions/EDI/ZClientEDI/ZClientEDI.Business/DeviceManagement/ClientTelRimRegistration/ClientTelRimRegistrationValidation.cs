//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientTelRimRegistrationValidation
//
//    This class should be used for overriding validation in AutoClientTelRimRegistrationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientTelRimRegistrationValidation : AutoClientTelRimRegistrationValidation
	{
		public ClientTelRimRegistrationValidation(AutoClientTelRimRegistration parent) : base(parent)
		{
		}
	}
}
