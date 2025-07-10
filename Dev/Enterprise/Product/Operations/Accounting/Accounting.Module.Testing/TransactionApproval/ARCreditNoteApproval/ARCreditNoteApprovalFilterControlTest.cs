using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	public class ARCreditNoteApprovalFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var approvals = new ARCreditNoteApprovalRequestCollection(Factory);
			var filterBO = new TransactionApprovalFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				ARCreditNoteApprovalFilterControl filterControl = new ARCreditNoteApprovalFilterControl(approvals, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
