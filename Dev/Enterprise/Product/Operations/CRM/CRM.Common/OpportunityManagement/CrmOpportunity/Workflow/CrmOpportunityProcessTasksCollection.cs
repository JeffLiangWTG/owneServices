using Enterprise.MasterFiles.Business;

namespace Enterprise.CRM.Common
{
	public class CrmOpportunityProcessTasksCollection : ProcessTaskCollection
	{
		public CrmOpportunityProcessTasksCollection(CrmOpportunity opportunity) : base(opportunity)
		{
		}

		public new CrmOpportunityProcessTasks this[int index]
		{
			get { return (CrmOpportunityProcessTasks)Elements[index]; }
		}

		public new CrmOpportunityProcessTasks AddNew()
		{
			return (CrmOpportunityProcessTasks)base.AddNew();
		}
	}
}
