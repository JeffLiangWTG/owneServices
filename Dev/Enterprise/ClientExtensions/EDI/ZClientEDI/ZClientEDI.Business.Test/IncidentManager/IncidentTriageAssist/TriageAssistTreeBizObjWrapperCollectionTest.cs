using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(TriageAssistTreeBizObjWrapperCollection))]
	public class TriageAssistTreeBizObjWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TriageAssistTreeBizObjWrapperCollection>
	{
		public void TestLoad()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();

			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "VER";

			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot2.IMO_IMT_Triage = Triage.PK;
			pivot2.IMO_IMD_DiagnosticCriteria = criteria1.PK;

			var pivot3 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot3.IMO_IMT_Triage = Triage.PK;
			pivot3.IMO_IMD_DiagnosticCriteria = criteria3.PK;

			Factory.Save();

			var obj = new TriageAssistBusinessObject(Incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();

			var triageWrapper = obj.TriageAssistTreeWrapperCollection.Single() as TriageAssistTreeTriageWrapper;
			AssertEquals(Triage.PK, triageWrapper.BizObj.PK);
			AssertEquals(2, triageWrapper.Children.Count());
			AssertEquals(true, triageWrapper.Children.Select(x => x.BizObj.PK).Contains(criteria1.PK));
			AssertEquals(true, triageWrapper.Children.Select(x => x.BizObj.PK).Contains(criteria3.PK));
		}

		public void TestLoad_OnlyIsActiveIsTrue()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_IsActive = false;

			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "VER";

			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot2.IMO_IMT_Triage = Triage.PK;
			pivot2.IMO_IMD_DiagnosticCriteria = criteria1.PK;

			Factory.Save();

			var obj = new TriageAssistBusinessObject(Incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			AssertEquals("criteria1 should not be retrieved because it is inactive", 0, obj.TriageAssistTreeWrapperCollection.Count);

			criteria1.IMD_IsActive = true;
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			var triageWrapper = obj.TriageAssistTreeWrapperCollection.Single() as TriageAssistTreeTriageWrapper;
			AssertEquals(Triage.PK, triageWrapper.BizObj.PK);
			AssertEquals(1, triageWrapper.Children.Count());
			AssertEquals("criteria1 should be retrieved because it is active", true, triageWrapper.Children.Select(x => x.BizObj.PK).Contains(criteria1.PK));
		}

		protected override TriageAssistTreeBizObjWrapperCollection GetCollectionToTest()
		{
			var wrapper = new TriageAssistTreeTriageWrapper(Assist, Triage);
			var collection = new TriageAssistTreeBizObjWrapperCollection(Assist);
			collection.Add(wrapper);
			return collection;
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
			Assist = new TriageAssistBusinessObject(Incident);
		}

		SupportIncident Incident;
		TriageAssistBusinessObject Assist;
		IncidentTriage Triage;
	}
}
