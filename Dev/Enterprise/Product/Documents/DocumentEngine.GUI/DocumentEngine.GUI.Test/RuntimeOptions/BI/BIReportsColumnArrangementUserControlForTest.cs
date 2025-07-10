using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	sealed class BIReportsColumnArrangementUserControlForTest : BIReportsColumnArrangementUserControl
	{
		public BIReportsColumnArrangementUserControlForTest(Report report) : base(report) { }

		internal void CreateNewSettingForTest()
		{
			var newConfig = new NewConfiguration(base.headingManager);
			newConfig.NewName = "Save Me";
			AddNewConfig(newConfig);
		}
	}
}
