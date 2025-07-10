using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class ShipmentDetailsUserControlTest : TestCase
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
	}

	public void TestCaptionRenderingEnabled()
	{
		AssertEquals(true, control.CaptionRenderingEnabled);
	}

	public void TestLocationOfGoodsCodeFindBox()
	{
		CombineAssertions(() =>
		{
			AssertType<ZCodeFindBox>("Type", control.LocationOfGoodsCodeFindBox);
			AssertEquals("BindTo", nameof(JobDeclaration.JE_LocationOfGoods), control.LocationOfGoodsCodeFindBox.BindTo);
			AssertEquals("ModuleID", ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, control.LocationOfGoodsCodeFindBox.ModuleID);
		});
	}

	public void TestPresentationStartDateEdit()
	{
		CombineAssertions(() =>
		{
			AssertType<ZDateEdit>("Type", control.PresentationStartDateEdit);
			AssertEquals("BindTo", nameof(JobDeclaration.ZG_PresentationStartDate), control.PresentationStartDateEdit.BindTo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ShipmentDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ShipmentDetailsUserControl control;
}
