using System.Linq;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class ShipmentDetailsUserControlTest : TestCase
{
	public void TestTypeOfGoodsDropEditControl() => AssertType<ZDropEdit>(control.Find(c => c.Name == "TypeOfGoodsDropEdit").First());

	public void TestOperationalStatusDropEditControl() => AssertType<ZDropEdit>(control.Find(c => c.Name == "OperationalStatusDropEdit").First());

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
