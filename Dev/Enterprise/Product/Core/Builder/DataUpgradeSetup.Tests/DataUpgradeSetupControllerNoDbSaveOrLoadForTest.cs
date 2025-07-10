namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	class DataUpgradeSetupControllerNoDbSaveOrLoadForTest : DataUpgradeSetupControllerForTest
	{
		public DataUpgradeSetupControllerNoDbSaveOrLoadForTest()
			: base()
		{
			TaskSetupList.Clear();
			TaskSetupList.Add(new DataTaskSetupNoDbSaveOrLoadForTest(this));
		}

		public new DataTaskSetupNoDbSaveOrLoadForTest TestTaskSetup
		{
			get { return (DataTaskSetupNoDbSaveOrLoadForTest)TaskSetupList[0]; }
		}
	}
}
