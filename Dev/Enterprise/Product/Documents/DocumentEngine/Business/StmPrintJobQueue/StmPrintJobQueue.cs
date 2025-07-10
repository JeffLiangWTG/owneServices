using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Scheduler.Business;

public class StmPrintJobQueue : AutoStmPrintJobQueue
{
	public StmPrintJobQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[RelatedBusinessObject("PrintJob")]
	public override ZGuid SPQ_SP_PrintJob
	{
		get { return base.SPQ_SP_PrintJob; }
		set { base.SPQ_SP_PrintJob = value; }
	}

	public virtual StmPrintJob PrintJob
	{
		get { return Factory.Load<StmPrintJob>(SPQ_SP_PrintJob); }
	}
}
