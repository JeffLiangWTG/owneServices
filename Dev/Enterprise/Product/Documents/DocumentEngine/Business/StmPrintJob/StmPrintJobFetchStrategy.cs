using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintJobFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public StmPrintJobFetchStrategy(StmPrintJob businessObject) : base(businessObject) { }

		protected StmPrintJob PrintJob => (StmPrintJob)BusinessObject;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				if (string.IsNullOrEmpty(column.TableName))
				{
					if (column.ColumnName == nameof(StmPrintJob.SP_UserFullName) ||
							column.ColumnName == nameof(StmPrintJob.SP_UserLoginName))
					{
						Factory.AddFetchHint(typeof(GlbStaff), GlbStaffSchema.GS_Code, PrintJob.SP_GS_NKJobSubmittedBy);
					}
				}
			}
		}
	}
}
