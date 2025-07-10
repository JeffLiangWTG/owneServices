using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentDiagnosticCriteriaTriagePivotCollection))]
	public class IncidentDiagnosticCriteriaTriagePivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAdd()
		{
			var criteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var item1 = Factory.NewWithValidTestData<IncidentTriage>();
			var item2 = Factory.NewWithValidTestData<IncidentTriage>();
			Factory.Save();
			var collection = new IncidentDiagnosticCriteriaTriagePivotCollection(criteria, Factory);
			var pivot1 = collection.AddNew();
			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			collection.Add(pivot2);
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);

			AssertEquals("Add should create the FK relationship to triage", criteria.PK, pivot1.IMO_IMD_DiagnosticCriteria);
			AssertEquals("Add should create the FK relationship to triage", criteria.PK, pivot2.IMO_IMD_DiagnosticCriteria);

			pivot1.IMO_IMT_Triage = item1.PK;
			pivot2.IMO_IMT_Triage = item2.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var criteriaReloaded = newFactory.Load<IncidentDiagnosticCriteria>(criteria.PK);
			var collectionReloaded = new IncidentDiagnosticCriteriaTriagePivotCollection(criteriaReloaded, newFactory);
			collectionReloaded.Load();
			AssertEquals("2 pivots should exist in the collection", 2, collectionReloaded.Count);
			AssertEquals(true, collectionReloaded.Contains(pivot1.PK));
			AssertEquals(true, collectionReloaded.Contains(pivot2.PK));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var item = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			return new IncidentDiagnosticCriteriaTriagePivotCollection(item, Factory);
		}

		#endregion
	}
}
