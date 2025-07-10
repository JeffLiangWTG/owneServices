using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(BillDetailsLayoutBuilder))]
sealed class BillDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<BillDetailsLayoutBuilder, AsycudaBill, CommonBillControlBag>
{
	public void TestSplitBillNumberCodeFindBoxVisible()
	{
		var bill = Factory.New<AsycudaBill>();
		var builder = GetColumnLayoutBuilderForTesting();
		var aeBag = BillDetailsControlBag.Instance;

		CombineAssertions(() =>
		{
			bill.ABL_SplitBill = true;
			AssertEquals("SplitBillNumberCodeFindBox is visible", true, Layout.IsVisible(BillDetailsControlBag.Instance.SplitBillNumberCodeFindBox, bill));

			bill.ABL_SplitBill = false;
			AssertEquals("SplitBillNumberCodeFindBox is not visible", false, Layout.IsVisible(BillDetailsControlBag.Instance.SplitBillNumberCodeFindBox, bill));
		});
	}
	protected override BillDetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		var builder = new BillDetailsLayoutBuilder();
		builder.AddControlBag(BillDetailsControlBag.Instance);
		return builder;
	}

	protected override int ExpectedMaxColumns => 4;

	PanelLayout Layout => layout ??= ((IPanelLayoutProvider)new BillDetailsLayout()).Layout;
	PanelLayout layout;
}
