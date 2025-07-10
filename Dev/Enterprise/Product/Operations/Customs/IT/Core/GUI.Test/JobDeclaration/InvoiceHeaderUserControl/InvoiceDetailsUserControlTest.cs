using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InvoiceDetailsUserControlTest : TestCaseWithFactory
{
	public void TestAgreedPlaceCodeDropEdit()
	{
		AssertType<ZDropEdit>(control.AgreedPlaceCodeDropEdit);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new InvoiceDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	InvoiceDetailsUserControl control;
}
