namespace Enterprise.Faxing.Integration
{
	public enum FaxLogType
	{
		Error,
		Warning,
		Information,
		Debug,
	}

	public interface IFaxLogger
	{
		void Log(FaxLogType type, string message);
	}
}
