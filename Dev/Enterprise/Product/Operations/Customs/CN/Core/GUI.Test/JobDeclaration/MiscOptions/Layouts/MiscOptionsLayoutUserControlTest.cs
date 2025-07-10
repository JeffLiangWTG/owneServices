using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class MiscOptionsLayoutUserControlTest : TestCaseWithFactory
	{
		public void TestMoreMergeOptionsSeparatorUserControl()
		{
			AssertType<SeparatorUserControl>(control.MoreMergeOptionsSeparatorUserControl);
		}

		public void TestMergeOptionsGrid()
		{
			AssertType<ZGrid>(control.MergeOptionsGrid);
		}

		MiscOptionsLayoutUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new MiscOptionsLayoutUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
