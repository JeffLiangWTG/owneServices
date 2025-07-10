namespace Enterprise.ZArchitecture
{
	public interface INotificationSubscriberQueryUser
	{
		void QueryUser(IQueryUserEventArgs e);
	}

	public interface INotificationSubscriberQueryUserDataImport : INotificationSubscriberQueryUser
	{
		void ResetUpdateDuringImportFlags();
	}
}
