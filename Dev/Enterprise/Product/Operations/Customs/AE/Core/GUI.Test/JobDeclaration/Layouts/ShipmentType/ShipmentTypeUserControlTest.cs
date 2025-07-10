using System.Linq;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class ShipmentTypeUserControlTest : TestCase
{
	public void TestExitPointDropEditControl() => AssertType<ZDropEdit>(control.Find(c => c.Name == "ExitPointDropEdit").First());

	public void TestClearanceLocationDropEditControl() => AssertType<ZDropEdit>(control.Find(c => c.Name == "ClearanceLocationDropEdit").First());

	protected override void SetUp()
	{
		base.SetUp();
		control = new ShipmentTypeUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ShipmentTypeUserControl control;
}
