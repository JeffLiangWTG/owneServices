using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	/// <summary>
	/// A parent for a collection of StmPrintJobs that should be delivered 
	/// to the one destination at the same time.
	/// </summary>
	public class StmDeliveryGroup : AutoStmDeliveryGroup, Enterprise.Integration.DocumentEngine.IStmDeliveryGroup
	{
		public StmDeliveryGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SB_SystemCreateTimeUtc = ZDateTime.UtcNow;
		}

		StmPrintJobGroupCollection printJobs;
		public StmPrintJobGroupCollection PrintJobs
		{
			get { return printJobs ?? (printJobs = new StmPrintJobGroupCollection()); }
		}

		ActiveBusinessObjectCollection<StmPrintJob> containedPrintJobs;
		public ActiveBusinessObjectCollection<StmPrintJob> ContainedPrintJobs =>
			containedPrintJobs ?? (containedPrintJobs = new ActiveBusinessObjectCollection<StmPrintJob>(Factory,
				new ZQuery(StmPrintJobSchema.SP_SB_DeliveryGroup, SQLComparisonOperator.Equal, PK)));
	}
}
