using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class DepartureDetailsUserControlTest : TestCaseWithFactory
{
	public void TestLocationOfGoodsUserControl()
	{
		var locationOfGoodsUserControl = control.LocationOfGoodsUserControl;
		CombineAssertions(() =>
		{
			AssertType<LocationOfGoodsUserControl>("Type", locationOfGoodsUserControl);
			AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader), locationOfGoodsUserControl.GetBindingMember());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new DepartureDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	DepartureDetailsUserControl control;
}
