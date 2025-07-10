namespace Enterprise.DataTransfer.Native.Common.AuditLogs
{
	public interface IAuditLogger
	{
		void LogAction(IEntity entity, string action);
	}
}
