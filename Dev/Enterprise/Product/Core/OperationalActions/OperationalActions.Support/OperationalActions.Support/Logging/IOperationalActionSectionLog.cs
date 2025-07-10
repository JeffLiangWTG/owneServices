namespace Enterprise.Services.OperationalActions.Support
{
	public interface IOperationalActionSectionLog
	{
		void Notify(OperationalActionLogErrorLevel errorLevel, string text);
		void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args);
		void SetSectionProgressMax(int max);
		void BumpSectionProgress();
	}
}
