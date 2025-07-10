using Enterprise.Accounting.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(GLJournalLinkedToApprovalController))]
	internal class GLJournalLinkedToApprovalControllerBasherTest : GLJournalControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GLJournalLinkedToApproval;
		}
	}
}
