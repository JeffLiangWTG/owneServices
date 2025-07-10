using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(HouseConsignmentDifferencesLayout))]
sealed class HouseConsignmentDifferencesUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType() => AssertEquals(typeof(NctsBill), userControl.BindingSource.DataSourceType);

	public void TestUnloadingRemarkCodeDropEdit() => CombineAssertions(() =>
	{
		AssertEquals("BindingSource", nameof(NctsBill.UnloadingRemarkCode), userControl.UnloadingRemarkCodeDropEdit.BindTo);
		AssertEquals("Visible", true, userControl.UnloadingRemarkCodeDropEdit.Visible);
	});

	public void TestUnloadingRemarkTextTextBox() => CombineAssertions(() =>
	{
		AssertEquals("BindingSource", nameof(NctsBill.UnloadingRemarkText), userControl.UnloadingRemarkTextTextBox.BindTo);
		AssertEquals("Visible", true, userControl.UnloadingRemarkTextTextBox.Visible);
		AssertEquals("Multiline", true, userControl.UnloadingRemarkTextTextBox.Multiline);
		AssertEquals("CharacterCasing", CharacterCasing.Normal, userControl.UnloadingRemarkTextTextBox.CharacterCasing);
	});

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new HouseConsignmentDifferencesUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}
	HouseConsignmentDifferencesUserControl userControl;
}
