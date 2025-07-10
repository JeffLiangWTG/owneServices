namespace Enterprise.Accounting.Business
{
	public interface IRegistryWithLogs
	{
		bool IsSameItem(IRegistryWithLogs item);
		bool IsEqual(IRegistryWithLogs item);
		string GetLogText(RegistryChangeLogger.EventType eventType);
	}
}
