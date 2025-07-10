using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.PrintProcessing.FaxRouting
{
	interface IFaxDeliverer
	{
		bool Deliver(StmPrintJob fax);
	}
}

