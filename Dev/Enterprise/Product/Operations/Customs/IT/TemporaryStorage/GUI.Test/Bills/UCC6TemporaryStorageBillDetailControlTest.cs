using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStorageBillDetailControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new UCC6TemporaryStorageBillDetailControl())
		{
			AssertNotNull(control.FindSingle<ZTextBox>("GoodsDescTextBox"));
			AssertNotNull(control.FindSingle<ZTextBox>("LrnTextBox"));
			AssertNotNull(control.FindSingle<ZTextBox>("MrnTextBox"));
			AssertNotNull(control.FindSingle<ZCalcEdit>("NetWeightCalcEdit"));
			AssertNotNull(control.FindSingle<ZCalcEdit>("GrossWeightCalcEdit"));
			AssertNotNull(control.FindSingle<ZCalcEdit>("SuppQtyCalcEdit"));
		}
	}

	public void TestGrossWeightCalcEdit()
	{
		using var control = new UCC6TemporaryStorageBillDetailControl();
		var element = control.FindSingle<ZCalcEdit>("GrossWeightCalcEdit");
		AssertEquals("BindTo", "Bills.GrossWeightInKG", element.BindTo);
	}

	public void TestNetWeightCalcEdit()
	{
		using var control = new UCC6TemporaryStorageBillDetailControl();
		var element = control.FindSingle<ZCalcEdit>("NetWeightCalcEdit");
		AssertEquals("BindTo", "Bills.NetWeightInKG", element.BindTo);
	}

	public void TestSuppQtyCalcEdit()
	{
		using var control = new UCC6TemporaryStorageBillDetailControl();
		var element = control.FindSingle<ZCalcEdit>("SuppQtyCalcEdit");
		AssertEquals("BindTo", "Bills.SuppQuantity", element.BindTo);
	}

	public void TestGoodsDescTextBox()
	{
		using var control = new UCC6TemporaryStorageBillDetailControl();
		var element = control.FindSingle<ZTextBox>("GoodsDescTextBox");
		AssertEquals("BindTo", "Bills.ABL_GoodsDescription", element.BindTo);
	}
}
