namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	public class DataTaskSetupNoDbSaveOrLoadForTest : DataTaskSetup
	{
		public DataTaskSetupNoDbSaveOrLoadForTest(DataUpgradeSetupController controller)
				: base(new UpgradeTaskForTest(), controller)
		{
		}

		/// <summary>
		/// Overrides to not apply XML to DB
		/// </summary>
		protected override void ApplyXmlFileDataToDatabase()
		{
		}

		/// <summary>
		/// Overrides to not save DB data to XML
		/// </summary>
		protected override void SaveNewDataToFileAndIncrementVersionIfDataHasChanged()
		{
		}
	}
}
