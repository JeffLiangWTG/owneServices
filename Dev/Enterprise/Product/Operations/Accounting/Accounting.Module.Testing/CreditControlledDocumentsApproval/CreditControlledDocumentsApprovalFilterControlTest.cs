using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public class CreditControlledDocumentsApprovalFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			CreditControlledDocumentsApprovalCollection creditControlledDocumentsApprovals = new CreditControlledDocumentsApprovalCollection(Factory);
			CreditControlledDocumentsApprovalFilterBusinessObject filterBO = new CreditControlledDocumentsApprovalFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				CreditControlledDocumentsApprovalFilterControl filterControl = new CreditControlledDocumentsApprovalFilterControl(creditControlledDocumentsApprovals, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
