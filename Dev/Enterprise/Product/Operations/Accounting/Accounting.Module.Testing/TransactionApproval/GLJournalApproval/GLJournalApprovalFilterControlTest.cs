using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	public class GLJournalApprovalFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var approvals = new GLJournalApprovalRequestCollection(Factory);
			var filterBO = new TransactionApprovalFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				var filterControl = new GLJournalApprovalFilterControl(approvals, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}

