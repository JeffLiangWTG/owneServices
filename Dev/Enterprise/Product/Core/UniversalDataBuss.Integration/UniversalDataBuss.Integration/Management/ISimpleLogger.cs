using Enterprise.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ISimpleLogger : ISimpleLogResult
	{
		void Log(LogType type, string message);
	}
}
