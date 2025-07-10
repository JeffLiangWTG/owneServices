using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed partial class ColumnArrangementUserControlTest
	{
		sealed class ColumnArrangementUserControlForTest : ColumnArrangementUserControl
		{
			public ColumnArrangementUserControlForTest(Report report) : base(report) { }

			internal void CreateNewSettingForTest()
			{
				var newConfig = new NewConfiguration(base.headingManager);
				newConfig.NewName = "Save Me";
				AddNewConfig(newConfig);
			}
		}
	}
}
