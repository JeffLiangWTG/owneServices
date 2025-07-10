using CargoWise.EntityFramework;

namespace Enterprise.RemotePrinting.Server.Business
{
	public class StmPrintQueueExtCollection : BusinessObjectCollection<StmPrintQueueExt>
	{
		public StmPrintQueueExtCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public StmPrintQueueExtCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
