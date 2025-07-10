using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace nterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsGoodsItemRemarksUserControlTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestRemarksTextBoxIsVisible()
	{
		using (var form = new ZForm())
		using (var control = new NctsGoodsItemRemarksUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var remarksTextBox = control.FindSingle<ZTextBox>("RemarksTextBox");

			AssertNotNull("RemarksTextBox", remarksTextBox);
			AssertEquals("RemarksTextBox", true, remarksTextBox.Visible);
		}
	}
}
