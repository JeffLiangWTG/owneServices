using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
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

		public void TestBranchCodeFindBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("BindTo", "BH_GB", control.BranchCodeFindBox.BindTo);
				AssertEquals("BindToList", "Lookups.Branches", control.BranchCodeFindBox.BindToList);
				AssertEquals("Caption", "Branch Code", control.BranchCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("PreBoundMaxLength", 3, control.BranchCodeFindBox.PreBoundMaxLength);
				AssertEquals("Visible", true, control.BranchCodeFindBox.Visible);
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
}
