using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class GoodsItemDifferencesDetailsColumnUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType() => AssertEquals(typeof(NctsArrivalCargoDesc), userControl.BindingSource.DataSourceType);

	public void TestDeclaredCommodityCode() => CombineAssertions(() =>
	{
		AssertType<NctsTariffFindBox>("Type", userControl.DeclaredCommodityCodeCodeFindBox);
		AssertEquals("GetShouldShowExactDescription", false, userControl.DeclaredCommodityCodeCodeFindBox.GetShouldShowExactDescription());
		AssertEquals("BindTo", "BY_FormattedHarmonisedTariff", userControl.DeclaredCommodityCodeCodeFindBox.BindTo);
	});

	public void TestUnloadedCommodityCode() => CombineAssertions(() =>
	{
		AssertType<NctsTariffFindBox>("Type", userControl.UnloadedCommodityCodeCodeFindBox);
		AssertEquals("GetShouldShowExactDescription", false, userControl.UnloadedCommodityCodeCodeFindBox.GetShouldShowExactDescription());
		AssertEquals("BindTo", "UnloadedGoodsItem.BY_FormattedHarmonisedTariff", userControl.UnloadedCommodityCodeCodeFindBox.BindTo);
	});

	public void TestUnloadingRemarkCodeDropEdit() => CombineAssertions(() =>
	{
		AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.UnloadingRemarkCode), userControl.UnloadingRemarkCodeDropEdit.BindTo);
		AssertEquals("Visible", true, userControl.UnloadingRemarkCodeDropEdit.Visible);
	});

	public void TestUnloadingRemarkTextTextBox() => CombineAssertions(() =>
	{
		AssertEquals("BindingSource", nameof(NctsArrivalCargoDesc.UnloadingRemarkText), userControl.UnloadingRemarkTextTextBox.BindTo);
		AssertEquals("Visible", true, userControl.UnloadingRemarkTextTextBox.Visible);
		AssertEquals("Multiline", true, userControl.UnloadingRemarkTextTextBox.Multiline);
		AssertEquals("CharacterCasing", CharacterCasing.Normal, userControl.UnloadingRemarkTextTextBox.CharacterCasing);
	});

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new GoodsItemDifferencesDetailsColumnUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}
	GoodsItemDifferencesDetailsColumnUserControl userControl;
}
