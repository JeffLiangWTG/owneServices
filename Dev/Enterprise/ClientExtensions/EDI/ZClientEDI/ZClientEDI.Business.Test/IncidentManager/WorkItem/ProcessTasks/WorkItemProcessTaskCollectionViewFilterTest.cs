using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(WorkItemProcessTaskCollectionViewFilter))]
	public class WorkItemProcessTaskCollectionViewFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			WorkItemProcessTaskCollection collection = workItem.WorkflowItems;
			return new WorkItemProcessTaskCollectionViewFilter(collection);
		}
	}
}
