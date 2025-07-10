using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintQueueCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public StmPrintQueueCollectionFetchStrategy(IBusinessObjectCollection collection) : base(collection) { }

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			if (columns.Any(c => c.ColumnName.Equals(StmPrintQueueSchema.Constants.SQ_SPS_Server) || c.ColumnName.Equals(nameof(StmPrintQueue.SQ_ServerName))))
			{
				var printServerPKs = businessObjects.OfType<StmPrintQueue>().Select(q => q.SQ_SPS_Server).Distinct();
				var query = new ZQuery(StmPrintServerSchema.PK, printServerPKs);
				Collection.Factory.AddFetchHint(StmPrintServerSchema.Instance, query);
			}
		}
	}
}
