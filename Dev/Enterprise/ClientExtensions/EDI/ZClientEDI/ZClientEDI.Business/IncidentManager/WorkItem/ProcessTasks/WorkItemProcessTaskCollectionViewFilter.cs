namespace Enterprise.Client.EDI.IncidentManager.Business
{
	using Enterprise.MasterFiles.Business;

	public class WorkItemProcessTaskCollectionViewFilter : ProcessTaskCollectionViewFilter
	{
		public WorkItemProcessTaskCollectionViewFilter(WorkItemProcessTaskCollection workflowItems)
			: base(workflowItems)
		{
		}

		public new WorkItemProcessTaskCollectionView TasksView
		{
			get { return (WorkItemProcessTaskCollectionView)base.TasksView; }
		}
	}
}

