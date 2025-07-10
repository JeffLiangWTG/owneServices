using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintJobCollection : BusinessObjectCollection<StmPrintJob>
	{
		public StmPrintJobCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public StmPrintJobCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
