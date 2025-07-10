using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.Customs.BE.NCTS.GUI.Testing;

sealed class PreviousDocumentUserControlTest : TestCaseWithFactory
{
	public void TestReferenceNumberN785UserControl()
	{
		var referenceNumberN785UserControl = control.ReferenceNumberN785UserControl;
		CombineAssertions(() =>
		{
			AssertType<PreviousDocumentN785ReferenceUserControl>("Type", referenceNumberN785UserControl);
			AssertEquals("BindingMember", ".", referenceNumberN785UserControl.GetBindingMember());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new PreviousDocumentUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	PreviousDocumentUserControl control;
}
