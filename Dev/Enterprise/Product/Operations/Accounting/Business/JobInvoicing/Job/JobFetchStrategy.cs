using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobFetchStrategy : JobHeaderFetchStrategy
	{
		public JobFetchStrategy(Job job)
			: base(job)
		{
		}

		Job Job
		{
			get { return BusinessObject as Job; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(OrgAddress), Job.JH_OA_AgentCollectAddr);
			Factory.AddFetchHint(typeof(OrgAddress), Job.JH_OA_LocalChargesAddr);

			if (Job.JH_ParentID.IsValid && Factory.HasContext(BusinessContext.MatchingReadOnly))
			{
				var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(Job.JH_ParentTableCode);
				if (tableSchema != null)
				{
					Factory.AddFetchHint(tableSchema.TableName, Job.JH_ParentID);
				}
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var columnList = new List<string>();
			columnList.Add("JH_ConsolNo");
			columnList.Add("JH_HouseBillNo");
			columnList.Add("JH_MasterBillNo");

			foreach (var column in columns)
			{
				if (columnList.Contains(column.ColumnName))
				{
					Factory.AddFetchHint(ViewShipmentConsolAndMasterBillNumbersSchema.Constants.TableName, Job.JH_ParentID);
				}
			}
		}
	}
}
