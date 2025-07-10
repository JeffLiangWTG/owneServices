using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine
{
	public interface IPrintJobManager
	{
		void ProcessPrintJobs(StmPrintJob[] jobs);
		void Delete(StmPrintJob[] jobs);
	}
}