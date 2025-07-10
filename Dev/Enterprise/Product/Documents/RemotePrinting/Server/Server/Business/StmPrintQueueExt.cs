using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.RemotePrinting.Server.Business
{
	public class StmPrintQueueExt : StmPrintQueue
	{
		public StmPrintQueueExt(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZInt QueuedPrintJobs
		{
			get
			{
				return GetPrintJobCount(nameof(PrintType.PRN), nameof(PrintJobStatus.QUE));
			}
		}

		public ZInt FailedPrintJobs
		{
			get
			{
				return GetPrintJobCount(nameof(PrintType.PRN), nameof(PrintJobStatus.FAL));
			}
		}

		public ZInt WorkingPrintJobs
		{
			get
			{
				return GetPrintJobCount(nameof(PrintType.PRN), nameof(PrintJobStatus.WRK));
			}
		}

		int GetPrintJobCount(string jobtype, string status)
		{
			var query = new ZQuery(StmPrintJobSchema.SP_JobType, jobtype);
			query.AddToFilter(new ZQuery(StmPrintJobSchema.SP_Status, status));
			query.AddToFilter(new ZQuery(StmPrintJobSchema.SP_SQ, this.PK));

			return Factory.GetDatabaseCount(typeof(StmPrintJob), query);
		}
	}
}
