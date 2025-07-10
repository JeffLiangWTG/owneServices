using Enterprise.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ISimpleLog
	{
		LogType Type { get; }
		string Message { get; }
	}
}
