using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public interface IProgressReporterProvider
	{
		IProgressReporter CreateProgressReporter(string message, int numberOfItemsToProcess, bool allowCancel = true);
	}
}
