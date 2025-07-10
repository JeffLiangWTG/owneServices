using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(WorkItemProcessTaskCollectionView))]
	public class WorkItemProcessTaskCollectionViewTest : BusinessObjectCollectionViewTestCase<WorkItemProcessTaskCollectionView>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(WorkItemProcessTaskCollectionView);
		}

		protected override WorkItemProcessTaskCollectionView GetCollectionToTest()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			WorkItemProcessTaskCollection collection = workItem.WorkflowItems;
			WorkItemProcessTaskCollectionView view = new WorkItemProcessTaskCollectionView(collection);
			return view;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WorkItemProcessTask item = Factory.New<WorkItemProcessTask>();
			return item;
		}
	}
}
