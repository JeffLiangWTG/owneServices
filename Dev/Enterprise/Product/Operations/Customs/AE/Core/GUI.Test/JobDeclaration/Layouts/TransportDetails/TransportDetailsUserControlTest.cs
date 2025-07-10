using System.Linq;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class TransportDetailsUserControlTest : TestCase
{
	public void TestZE_PlaceOfDischargeDropEditControl() => AssertType<ZDropEdit>(control.Find(c => c.Name == "PlaceOfDischargeDropEdit").First());

	protected override void SetUp()
	{
		base.SetUp();
		control = new TransportDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	TransportDetailsUserControl control;
}
