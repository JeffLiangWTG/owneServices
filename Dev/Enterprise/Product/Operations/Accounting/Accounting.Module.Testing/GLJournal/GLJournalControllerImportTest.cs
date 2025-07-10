using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GLJournalControllerImport))]
	public class GLJournalControllerImportTest : GLJournalControllerTest
	{
		public void TestShowGLJournalImportedNoSecurityRights()
		{
			Env.Security.NewGeneralLedgerJournal.IsAllowed = false;
			var testJournal = Factory.NewWithValidTestData<GLJournal>();
			var controller = new GLJournalControllerImport();

			AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", delegate
			{
				var form = controller.ShowImportedDataForm(testJournal);
				AssertNull("Form should be null", form);
				AssertContains("Security right error message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}
}
