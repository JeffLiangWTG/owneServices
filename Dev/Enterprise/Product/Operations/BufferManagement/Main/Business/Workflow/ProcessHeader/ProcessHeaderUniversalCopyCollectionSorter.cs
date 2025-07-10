using System.Collections;
using System.Linq;
using Enterprise.Integration.UniversalCopy;

namespace Enterprise.BufferManagement.Business.Workflow.ProcessHeader
{
	public class ProcessHeaderUniversalCopyCollectionSorter : IUniversalCopyTypeCollectionSorter
	{
		public string ItemsTableName { get => nameof(Business.ProcessHeader); }

		public IEnumerable GetSortedCollection(IEnumerable collection)
		{
			return collection
				.Cast<Business.ProcessHeader>()
				.OrderBy(processHeader => processHeader.FH_ParentId)
				.ThenByDescending(processHeader => processHeader.FH_Category == BMConstants.JobLevelWorkflowCategoryCode);
		}
	}
}
