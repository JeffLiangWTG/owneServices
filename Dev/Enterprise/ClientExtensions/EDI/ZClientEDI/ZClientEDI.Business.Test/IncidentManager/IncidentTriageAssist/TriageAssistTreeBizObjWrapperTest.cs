using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.TriageAssistTreeTriageWrapper;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(TriageAssistTreeTriageWrapper))]
	public class TriageAssistTreeTriageWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var wrapper = GetNewBusinessObject() as TriageAssistTreeTriageWrapper;
			AssertEquals(Triage.IMT_SupportDescription, wrapper.Description);
			AssertEquals(Triage.TypeDescription, wrapper.Type);
			AssertEquals(TriageNodeStatus.AllConfirmed, wrapper.TriageStatus);
			AssertEquals("All Confirmed", wrapper.Status);
			AssertEquals(Triage.IMT_Product, wrapper.Product);
			AssertEquals(Triage.IMT_ProductArea, wrapper.Area);
			AssertEquals(Triage.IMT_Module, wrapper.Section);
			AssertEquals(Triage.PK, wrapper.BizObj.PK);

			Triage.IMT_SetProductAreaByMenuItem = true;
			AssertEquals("***", wrapper.Area);
		}

		public void TestAllConfirmedShouldNotBeSetIfCriteriaNull()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();

			var incidentCriteriaPivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			incidentCriteriaPivot1.LinkParent(Incident);
			incidentCriteriaPivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			incidentCriteriaPivot1.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Confirmed;

			var triageCriteriaPivot1 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			triageCriteriaPivot1.IMO_IMT_Triage = Triage.PK;
			triageCriteriaPivot1.IMO_IMD_DiagnosticCriteria = criteria1.PK;

			var triageCriteriaPivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			triageCriteriaPivot2.IMO_IMT_Triage = Triage.PK;
			triageCriteriaPivot2.IMO_IMD_DiagnosticCriteria = criteria2.PK;
			Factory.Save();

			var incident = new BusinessObjectFactory().Load<SupportIncident>(Incident.PK);
			var obj = new TriageAssistBusinessObject(incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			var triageWrapper = obj.TriageAssistTreeWrapperCollection.Single() as TriageAssistTreeTriageWrapper;
			AssertEquals(Triage.PK, triageWrapper.BizObj.PK);
			AssertEquals(TriageNodeStatus.Unknown, triageWrapper.TriageStatus);
		}

		public void TestProperties_NotFocused_Excluded()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var incidentCriteriaPivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			incidentCriteriaPivot1.LinkParent(Incident);
			incidentCriteriaPivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			incidentCriteriaPivot1.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Negate;

			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var incidentCriteriaPivot2 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			incidentCriteriaPivot2.LinkParent(Incident);
			incidentCriteriaPivot2.IMV_IMD_DiagnosticCriteria = criteria2.PK;
			incidentCriteriaPivot2.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Confirmed;

			var triageCriteriaPivot1 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			triageCriteriaPivot1.IMO_IMT_Triage = Triage.PK;
			triageCriteriaPivot1.IMO_IMD_DiagnosticCriteria = criteria1.PK;
			var triageCriteriaPivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			triageCriteriaPivot2.IMO_IMT_Triage = Triage.PK;
			triageCriteriaPivot2.IMO_IMD_DiagnosticCriteria = criteria2.PK;
			Factory.Save();

			AssertTriageNodeStatus(TriageNodeStatus.Excluded, false);
		}

		public void TestProperties_Focused_OnlyLinkedAndConfirmed_NoEssential()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var incidentCriteriaPivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			incidentCriteriaPivot1.LinkParent(Incident);
			incidentCriteriaPivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			incidentCriteriaPivot1.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Investigate;
			var incidentCriteriaPivot2 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			incidentCriteriaPivot2.LinkParent(Incident);
			incidentCriteriaPivot2.IMV_IMD_DiagnosticCriteria = criteria2.PK;
			incidentCriteriaPivot2.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Confirmed;

			var triageCriteriaPivot1 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			triageCriteriaPivot1.IMO_IMT_Triage = Triage.PK;
			triageCriteriaPivot1.IMO_IMD_DiagnosticCriteria = criteria1.PK;
			var triageCriteriaPivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			triageCriteriaPivot2.IMO_IMT_Triage = Triage.PK;
			triageCriteriaPivot2.IMO_IMD_DiagnosticCriteria = criteria2.PK;
			Factory.Save();
			AssertTriageNodeStatus(TriageNodeStatus.Investigating, true);

			incidentCriteriaPivot1.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Confirmed;
			Factory.Save();
			AssertTriageNodeStatus(TriageNodeStatus.AllConfirmed, true);
		}

		public void TestProperties_Focused_LinkedAndConfirmed_Essential()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_FocusRelatedTriageNodesOnly = true;
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var incidentCriteriaPivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			incidentCriteriaPivot1.LinkParent(Incident);
			incidentCriteriaPivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			incidentCriteriaPivot1.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Confirmed;
			var incidentCriteriaPivot2 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			incidentCriteriaPivot2.LinkParent(Incident);
			incidentCriteriaPivot2.IMV_IMD_DiagnosticCriteria = criteria2.PK;
			incidentCriteriaPivot2.IMV_Status = IncidentDiagnosticCriteriaPivotStatusList.Codes.Confirmed;

			var triageCriteriaPivot1 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			triageCriteriaPivot1.IMO_IMT_Triage = Triage.PK;
			triageCriteriaPivot1.IMO_IMD_DiagnosticCriteria = criteria2.PK;
			Factory.Save();

			AssertTriageNodeStatus(TriageNodeStatus.AllConfirmed, false);

			criteria2.IMD_FocusRelatedTriageNodesOnly = true;
			Factory.Save();
			AssertTriageNodeStatus(TriageNodeStatus.AllConfirmed, true);
		}

		void AssertTriageNodeStatus(TriageNodeStatus statusExpected, bool isFocusedExpected)
		{
			var incident = new BusinessObjectFactory().Load<SupportIncident>(Incident.PK);
			var obj = new TriageAssistBusinessObject(incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.TriageAssistTreeWrapperCollection.Load();
			var triageWrapper = obj.TriageAssistTreeWrapperCollection.Single() as TriageAssistTreeTriageWrapper;
			AssertEquals(Triage.PK, triageWrapper.BizObj.PK);
			AssertEquals(statusExpected, triageWrapper.TriageStatus);
			AssertEquals(isFocusedExpected, triageWrapper.IsFocused);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TriageAssistTreeTriageWrapper(Assist, Triage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Triage = Factory.NewWithValidTestData<IncidentTriage>();
			Triage.IMT_SetProductAreaByMenuItem = false;
			Assist = new TriageAssistBusinessObject(Incident);
		}

		SupportIncident Incident;
		TriageAssistBusinessObject Assist;
		IncidentTriage Triage;
	}

	[TestedType(typeof(TriageAssistTreeRelevantCriteriaWrapper))]
	public class TriageAssistTreeRelevantCriteriaWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			return new TriageAssistTreeRelevantCriteriaWrapper(new RelevantDiagnosticCriteria(criteria1, Assist));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Assist = new TriageAssistBusinessObject(Incident);
		}

		SupportIncident Incident;
		TriageAssistBusinessObject Assist;
	}
}
