namespace Enterprise.ClientSharedComponents.Testing
{
	public abstract class AccountsExportDirectorARAPTest : AccountsExportDirectorTest
	{
		protected override void AssertNotificationsWhenSuccess()
		{
			Assert("1 files created", DirInfo.GetFiles().Length == 1);
		}
	}
}
