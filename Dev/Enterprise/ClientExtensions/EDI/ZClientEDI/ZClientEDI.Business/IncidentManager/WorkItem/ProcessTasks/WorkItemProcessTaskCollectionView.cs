namespace Enterprise.Client.EDI.IncidentManager.Business
{
	using Enterprise.MasterFiles.Business;

	public class WorkItemProcessTaskCollectionView : ProcessTaskCollectionView
	{
		public WorkItemProcessTaskCollectionView(WorkItemProcessTaskCollection collection)
			: base(collection)
		{
		}

		/// <summary>
		/// DataBinding uses the indexer to determine the element type.
		/// So we implement it to tell DataBinding that our elements are of type WorkItemProcessTask
		/// rather than ProcessTask.
		/// See BusinessObjectCollection.GetElementTypeFromCollectionType.
		/// </summary>
		public new WorkItemProcessTask this[int i]
		{
			get { return (WorkItemProcessTask)base[i]; }
		}

		public new WorkItemProcessTask AddNew()
		{
			return (WorkItemProcessTask)base.AddNew();
		}
	}
}

