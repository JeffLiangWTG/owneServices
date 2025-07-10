using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

public class GroupInvoiceUserControlTest : TestCaseWithFactory
{
	public void TestIsDutiableColumnRemovedAndGSTApplicableRenamedToDutiable()
	{
		var testDec = Factory.New<JobDeclaration>();
		using (ZForm form = new ZForm(testDec))
		using (GroupInvoiceUserControl testControl = new GroupInvoiceUserControl())
		{
			form.Controls.Add(testControl);
			form.Show();
			testControl.SetDataBinding(testDec, "");
			AssertEquals("Is Dutiable is removed from the grid", null, testControl.GroupChargeGrid.Columns[BaseGroupInvoiceCharge.Schema.J7_IsDutiable]);
			AssertEquals("IsGSTApplicable renamed to IsDutiable", "Dutiable", testControl.GroupChargeGrid.Columns[BaseGroupInvoiceCharge.Schema.J7_IsGSTApplicable].ColumnStyle.HeaderText);
		}
	}
}
