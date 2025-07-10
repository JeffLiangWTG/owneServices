using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public interface ITransferRuleRunnerLogger : ILogger
	{
		void Log(string message);
	}
}
