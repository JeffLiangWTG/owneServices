using CargoWise.ComponentModel;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine.ReportErrorManagement
{
	public class ReportError : IReportError
	{
		internal ReportError(string message, INotificationType type)
		{
			Message = message;
			Type = type;
		}

		public string Message { get; }
		public INotificationType Type { get; }
	}
}
