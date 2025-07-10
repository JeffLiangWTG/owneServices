using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(NewWorkItemRelatedCollectionView))]
	public class NewWorkItemRelatedItemCollectionViewViewTest : BusinessObjectCollectionViewTestCase<NewWorkItemRelatedCollectionView>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(NewWorkItemRelatedCollectionView);
		}

		protected override NewWorkItemRelatedCollectionView GetCollectionToTest()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			return incident.RelatedWorkItems;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<NewWorkItem>();
		}
	}
}
