namespace Enterprise.Services.OperationalActions.Support
{
	public class NullOperationalActionLog : IOperationalActionSectionLog
	{
		public NullOperationalActionLog()
		{
		}

		public void Notify(OperationalActionLogErrorLevel errorLevel, string text)
		{
		}

		public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
		{
		}

		public void SetSectionProgressMax(int max)
		{
		}

		public void BumpSectionProgress()
		{
		}
	}
}