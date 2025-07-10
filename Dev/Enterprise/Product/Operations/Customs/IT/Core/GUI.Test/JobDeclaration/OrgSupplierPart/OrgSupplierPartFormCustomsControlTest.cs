using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class OrgSupplierPartFormCustomsControlTest : TestCaseWithFactory
{
	public void TestUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new OrgSupplierPartFormCustomsControl())
		{
			form.Controls.Add(control);
			form.Show();
			control.Controls.Find("supportingDocsTabPage", true).First().Show();
			var supportingDocument = control.Controls.Find("SupportingDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(PartPivotLayoutSupportingDocumentsUserControl), supportingDocument.UserControlType);

			control.Controls.Find("previousDocsTabPage", true).First().Show();
			var previousDocument = control.Controls.Find("PreviousDocumentsUserControl", true).First() as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(PreviousDocumentsUserControl), previousDocument.UserControlType);
		}
	}
}
