//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientStaffValidation
//
//    This class should be used for overriding validation in AutoClientStaffValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientStaffValidation : AutoClientStaffValidation
	{
		public ClientStaffValidation(AutoClientStaff parent) : base(parent)
		{
		}
	}
}

