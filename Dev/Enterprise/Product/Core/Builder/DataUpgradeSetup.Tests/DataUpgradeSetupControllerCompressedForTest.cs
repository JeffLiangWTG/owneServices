namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	class DataUpgradeSetupControllerCompressedForTest : DataUpgradeSetupControllerForTest
	{
		public DataUpgradeSetupControllerCompressedForTest()
			: base()
		{
			TaskSetupList.Clear();
			TaskSetupList.Add(new DataTaskSetupForTest(new UpgradeTaskForCompressedTest(), this));
		}
	}
}
