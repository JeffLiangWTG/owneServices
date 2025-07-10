using CargoWise.ComponentModel;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IReportError
	{
		string Message { get; }
		INotificationType Type { get; }
	}
}
