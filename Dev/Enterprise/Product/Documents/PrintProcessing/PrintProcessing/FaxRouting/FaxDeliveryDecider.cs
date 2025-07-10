using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.PrintProcessing.FaxRouting
{
	class FaxDeliveryDecider : IFaxDeliverer
	{
		public bool Deliver(StmPrintJob fax)
		{
			return new InternetFaxDeliverer().Deliver(fax);
		}
	}
}
