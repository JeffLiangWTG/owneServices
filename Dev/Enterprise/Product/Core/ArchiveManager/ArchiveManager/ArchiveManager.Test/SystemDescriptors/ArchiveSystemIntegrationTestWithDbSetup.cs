namespace Enterprise.ArchiveManager.Test.SystemDescriptors
{
	public abstract class ArchiveSystemIntegrationTestWithDbSetup : ArchiveSystemIntegrationTest
	{
		public int DbNumber => 333;

		readonly DBHelper DbHelper = new();

		protected override void SetUp()
		{
			base.SetUp();
			_ = DbHelper.RecreateDatabase(DbNumber);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DbHelper.DropDatabase(DbNumber);
		}
	}
}
