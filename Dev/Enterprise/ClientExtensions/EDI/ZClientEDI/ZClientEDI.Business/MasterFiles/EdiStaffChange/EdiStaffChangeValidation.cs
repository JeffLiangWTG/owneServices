//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiStaffChangeValidation
//
//    This class should be used for overriding validation in AutoEdiStaffChangeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.HR
{
	public class EdiStaffChangeValidation : AutoEdiStaffChangeValidation
	{
		public EdiStaffChangeValidation(AutoEdiStaffChange parent) : base(parent)
		{
		}
	}
}

