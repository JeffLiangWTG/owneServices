using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(FeatureRequestRelatedFeatureRequestsCollection))]
	public class FeatureRequestRelatedFeatureRequestsCollectionTest : ProcessManagement.Business.Test.WorkTaskRelatedItemCollectionTestCase<SupportIncident>
	{
		public override void TestShouldAddToCollection()
		{
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident3 = Factory.NewWithValidTestData<SupportIncident>();
			NewWorkItem workItem1 = Factory.NewWithValidTestData<NewWorkItem>();

			incident1.SetupForProjectFeatureRequest();
			incident3.SetupForProjectFeatureRequest();

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident1.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident3.IM_Category);

			AssertEquals(0, Collection.Count);

			Collection.Add(incident1);
			Collection.Add(incident2);
			Collection.Add(incident3);
			Collection.Add(workItem1);

			AssertEquals("Should contain only Feature Request", 2, Collection.Count);
			Assert("Should have incident 1", Collection.Contains(incident1));
			Assert("Should have incident 3", Collection.Contains(incident3));
		}

		public void TestDefaultsOnNewChild()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			SupportIncident relatedIncident = incident.RelatedFeatureRequests.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("relatedIncident.IM_Category", SupportIncidentCategoriesList.Codes.FeatureRequest, relatedIncident.IM_Category);
				AssertEquals("relatedIncident.IM_Status", SupportIncidentLookups.Status.Open, relatedIncident.IM_Status);
				AssertEquals("relatedIncident.IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, relatedIncident.IM_ResolutionCode);
			});
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(FeatureRequestRelatedFeatureRequestsCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new FeatureRequestRelatedFeatureRequestsCollection(Factory.NewWithValidTestData<SupportIncident>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();
			return incident;
		}
	}
}
