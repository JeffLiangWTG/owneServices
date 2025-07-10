using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(DiagnosticCriteriaInvestigationItemPivotCollection))]
	public class DiagnosticCriteriaInvestigationItemPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAdd()
		{
			var incidentDiagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var item1 = Factory.NewWithValidTestData<InvestigationItem>();
			var item2 = Factory.NewWithValidTestData<InvestigationItem>();
			Factory.Save();
			var collection = new DiagnosticCriteriaInvestigationItemPivotCollection(incidentDiagnosticCriteria, Factory);
			var pivot1 = collection.AddNew();
			var pivot2 = Factory.New<DiagnosticCriteriaInvestigationItemLink>();
			collection.Add(pivot2);
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);

			AssertEquals("Add should create the FK relationship to DiagnosticCriteria", incidentDiagnosticCriteria.PK, pivot1.DIL_IMD_DiagnosticCriteria);
			AssertEquals("Add should create the FK relationship to DiagnosticCriteria", incidentDiagnosticCriteria.PK, pivot2.DIL_IMD_DiagnosticCriteria);

			pivot1.DIL_INV_InvestigationItem = item1.PK;
			pivot2.DIL_INV_InvestigationItem = item2.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var diagnosticCriteriaReloaded = newFactory.Load<IncidentDiagnosticCriteria>(incidentDiagnosticCriteria.PK);
			var collectionReloaded = new DiagnosticCriteriaInvestigationItemPivotCollection(diagnosticCriteriaReloaded, newFactory);
			collectionReloaded.Load();
			AssertEquals("2 pivots should exist in the collection", 2, collectionReloaded.Count);
			AssertEquals(true, collectionReloaded.Contains(pivot1.PK));
			AssertEquals(true, collectionReloaded.Contains(pivot2.PK));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var triage = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			return new DiagnosticCriteriaInvestigationItemPivotCollection(triage, Factory);
		}

		#endregion
	}
}
