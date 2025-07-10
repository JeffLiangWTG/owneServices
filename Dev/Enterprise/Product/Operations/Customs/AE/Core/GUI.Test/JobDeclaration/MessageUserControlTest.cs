using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

public class MessageUserControlTest : TestCaseWithFactory
{
	public void TestAddEntryHeaderColumns()
	{
		var testDec = Factory.New<JobDeclaration>();
		using (var form = new ZForm(testDec))
		using (var userControl = new MessageUserControl())
		{
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(testDec, ".");
			form.Show();
			AssertNotNull("User control should have TotalPaid column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_TotalPaid]);
		}
	}
}
