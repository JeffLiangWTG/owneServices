using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using NUnit.Framework;

namespace ZClientEDI.Business.Test.IncidentManager.IncidentTriageAssist
{
	[TestedType(typeof(FilteredTriageAssistTreeBizObjWrapperCollection))]
	public class FilteredTriageAssistTreeBizObjWrapperCollectionTest : BusinessObjectCollectionViewTestCase<FilteredTriageAssistTreeBizObjWrapperCollection>
	{
		protected override FilteredTriageAssistTreeBizObjWrapperCollection GetCollectionToTest()
		{
			return Assist.FilteredTriageAssistTreeWrapperCollection;
		}

		public void TestShouldNotAddNonPublishedToAssisTriage()
		{
			Factory.Save();

			Assist.SearchTerm = "Test";
			Assist.LinkedCriteriaCollection.LoadBySearchTerm();
			AssertEquals(1, Assist.LinkedCriteriaCollection.Count);
			Assist.LinkedCriteriaCollection[0].Confirm = true;

			Assist.FilteredTriageAssistTreeWrapperCollection.CollectionToFilter.Load();
			AssertEquals(2, Assist.FilteredTriageAssistTreeWrapperCollection.CollectionToFilter.Count);

			Assist.FilteredTriageAssistTreeWrapperCollection.Rebuild();
			AssertEquals(1, Assist.FilteredTriageAssistTreeWrapperCollection.Count);
			Assert(((IncidentTriage)Assist.FilteredTriageAssistTreeWrapperCollection[0].BizObj).IMT_IsPublishedToAssist);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TriageAssistTreeTriageWrapper(Assist, Triage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Triage = Factory.NewWithValidTestData<IncidentTriage>();
			Triage.IMT_IsActive = true;
			Triage.IMT_IsPublishedToAssist = true;

			Triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			Triage2.IMT_IsActive = true;
			Triage2.IMT_IsPublishedToAssist = false;

			var criteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria.IMD_Description = "Test";
			criteria.IMD_IsActive = true;
			criteria.IncidentTriagePivots.AddNew().IMO_IMT_Triage = Triage.PK;
			criteria.IncidentTriagePivots.AddNew().IMO_IMT_Triage = Triage2.PK;

			Assist = new TriageAssistBusinessObject(Incident);
		}

		SupportIncident Incident;
		TriageAssistBusinessObject Assist;
		IncidentTriage Triage;
		IncidentTriage Triage2;
	}
}
