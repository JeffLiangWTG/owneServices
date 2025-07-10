using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public abstract class FilteredRelevantDiagnosticCriteriaCollection : BusinessObjectCollectionView<RelevantDiagnosticCriteria>
	{
		public FilteredRelevantDiagnosticCriteriaCollection(RelevantDiagnosticCriteriaCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Parent = collectionToFilter.Parent;
			Rebuild();
		}

		protected TriageAssistBusinessObject Parent { get; }

		protected override void RebuildCore()
		{
			try
			{
				using (SuspendCountChanged(null))
				{
					base.RebuildCore();
				}
			}
			finally
			{
				OnCountChanged(null);
			}
		}
	}
}
