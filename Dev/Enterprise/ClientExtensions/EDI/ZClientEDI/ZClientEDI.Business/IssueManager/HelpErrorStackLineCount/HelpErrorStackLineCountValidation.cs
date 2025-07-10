//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHelpErrorStackLineCountValidation
//
//    This class should be used for overriding validation in AutoHelpErrorStackLineCountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class HelpErrorStackLineCountValidation : AutoHelpErrorStackLineCountValidation
	{
		public HelpErrorStackLineCountValidation(AutoHelpErrorStackLineCount parent) : base(parent)
		{
		}
	}
}

