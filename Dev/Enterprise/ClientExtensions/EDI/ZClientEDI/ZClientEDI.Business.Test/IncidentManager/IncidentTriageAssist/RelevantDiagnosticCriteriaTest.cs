using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(RelevantDiagnosticCriteria))]
	public class RelevantDiagnosticCriteriaTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOnFactorySaving()
		{
			var searchTerm = ZGuid.NewZGuid().ToSqlGuid();
			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_Description = $"{searchTerm} - 001";
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria2.IMD_Description = $"{searchTerm} - 002";
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria3.IMD_Description = "something else";
			var pivot1 = Factory.New<IncidentDiagnosticCriteriaPivot>();
			pivot1.LinkParent(Incident);
			pivot1.IMV_IMD_DiagnosticCriteria = criteria1.PK;
			pivot1.IMV_Status = "INV";
			Factory.Save();

			var obj = new TriageAssistBusinessObject(Incident);
			obj.LinkedCriteriaCollection.LoadLinkedCriteria();
			obj.RefreshSearchTerm(searchTerm, "");

			var relevantCriteria1 = Factory.Load<RelevantDiagnosticCriteria>(criteria1.PK);
			var relevantCriteria2 = Factory.Load<RelevantDiagnosticCriteria>(criteria2.PK);
			AssertNull(Factory.Load<RelevantDiagnosticCriteria>(criteria3.PK));

			AssertEquals(true, relevantCriteria1.Investigate);
			AssertEquals(false, relevantCriteria2.Investigate);

			relevantCriteria1.Investigate = false;
			relevantCriteria2.Confirm = true;
			Factory.Save();

			AssertEquals("pivot1 deleted, because it's not linked", true, pivot1.IsDeleted);
			var pivots = Factory.Load<IncidentDiagnosticCriteriaPivot>(new ZQuery(IncidentDiagnosticCriteriaPivotSchema.IMV_ParentID, Incident.PK));
			AssertNotNull("new pivot created, because it's linked", pivots.Single(x => x.IMV_IMD_DiagnosticCriteria == criteria2.PK && x.IMV_Status == "VER"));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RelevantDiagnosticCriteria(Criteria, Assist);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Criteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			Assist = new TriageAssistBusinessObject(Incident);
		}

		SupportIncident Incident;
		TriageAssistBusinessObject Assist;
		IncidentDiagnosticCriteria Criteria;
	}
}
