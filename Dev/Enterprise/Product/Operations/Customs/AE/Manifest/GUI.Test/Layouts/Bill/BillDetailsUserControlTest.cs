using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

sealed class BillDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using var control = new BillDetailsUserControl();
		AssertEquals(typeof(AsycudaBill), control.BindingSource.DataSourceType);
	}

	public void TestControls()
	{
		var expectedControlsAmount = 3;
		using var control = new BillDetailsUserControl();
		AssertEquals($"BillDetailsUserControl should countain only {expectedControlsAmount} controls", expectedControlsAmount, control.Controls.Count);
	}

	public void TestSplitBillNumberCodeFindBox() => CombineAssertions(() =>
	{
		var bill = Factory.New<AsycudaBill>();

		using var form = new ZForm();
		using var userControl = new BillDetailsUserControl();
		form.Controls.Add(userControl);
		form.Show();

		var codeFindBox = userControl.FindSingle<ZCodeFindBox>("SplitBillNumberCodeFindBox");
		AssertEquals("BindTo", nameof(AsycudaBill.ABL_SplitBillNumber), codeFindBox.BindTo);
		AssertEquals(true, codeFindBox.Visible);
		AssertEquals(false, codeFindBox.ShowDescriptionBox);
	});

	public void TestSplitBillCheckBox() => CombineAssertions(() =>
	{
		var bill = Factory.New<AsycudaBill>();

		using var form = new ZForm();
		using var userControl = new BillDetailsUserControl();
		form.Controls.Add(userControl);
		form.Show();

		var checkBox = userControl.FindSingle<ZCheckBox>("SplitBillCheckBox");
		AssertEquals("BindTo", nameof(AsycudaBill.ABL_SplitBill), checkBox.BindTo);
		AssertEquals("Check box should be displayed as default", true, checkBox.Visible);
		AssertEquals("Check box should be checked as default", true, checkBox.Checked);
	});

	public void TestForwarderMPCITextBox() => CombineAssertions(() =>
	{
		var bill = Factory.New<AsycudaBill>();

		using var form = new ZForm();
		using var userControl = new BillDetailsUserControl();
		form.Controls.Add(userControl);
		form.Show();

		var textBox = userControl.FindSingle<ZTextBox>("ForwarderMPCITextBox");
		AssertEquals("BindTo", nameof(AsycudaBill.ForwarderMPCI), textBox.BindTo);
		AssertEquals(true, textBox.Visible);
	});
}
