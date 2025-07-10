using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class MiscellaneousOptionsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals("BindingSource DataSourceType", typeof(NctsHeader), control.BindingSource.DataSourceType);
	}

	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
	}

	public void TestCustomsProfileDropEdit()
	{
		CombineAssertions(() =>
		{
			AssertEquals("BindTo", "BH_CustomsProfile", control.CustomsProfileDropEdit.BindTo);
			AssertEquals("PreBoundMaxLength", 3, control.CustomsProfileDropEdit.PreBoundMaxLength);
			AssertEquals("ShowDescriptionBox", false, control.CustomsProfileDropEdit.ShowDescriptionBox);
			AssertEquals("Visible", true, control.CustomsProfileDropEdit.Visible);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new MiscellaneousOptionsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	MiscellaneousOptionsUserControl control;
}
