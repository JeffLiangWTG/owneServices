using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(WorkItemProcessTaskCollection))]
	class WorkItemProcessTasksCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			return workItem.WorkflowItems;
		}
	}
}
