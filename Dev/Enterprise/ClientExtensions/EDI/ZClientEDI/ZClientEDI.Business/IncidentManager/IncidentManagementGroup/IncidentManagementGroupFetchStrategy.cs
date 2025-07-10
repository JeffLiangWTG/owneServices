using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public IncidentManagementGroupFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, BusinessObject.PK);
		}
	}
}
