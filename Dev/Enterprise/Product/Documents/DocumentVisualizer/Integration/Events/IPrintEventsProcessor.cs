using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IPrintEventsProcessor
	{
		void OnPrintJobsCreated(IStmPrintJob[] printJobs);
	}
}
