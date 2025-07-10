//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmScheduleTaskCopyRecipientValidation
//
//    This class should be used for overriding validation in AutoStmScheduleTaskCopyRecipientValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskCopyRecipientValidation : AutoStmScheduleTaskCopyRecipientValidation
	{
		public StmScheduleTaskCopyRecipientValidation(AutoStmScheduleTaskCopyRecipient parent) : base(parent)
		{
		}

		protected override void CheckSCR_EmailAddress()
		{
			base.CheckSCR_EmailAddress();
			MandatoryValidation.CheckEntered(Parent.SCR_EmailAddressInfo);
			if (!EmailAddressValidation.IsEmailAddressValid(Parent.SCR_EmailAddress))
			{
				Parent.SCR_EmailAddressInfo.AddError(Res.GetString("4DFD7632-67AC-4043-89E5-79B5EF2FAA29", "The email address is invalid."));
			}
		}
	}
}
