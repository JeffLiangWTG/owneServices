using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentTriageDiagnosticCriteriaPivotCollection))]
	public class IncidentTriageDiagnosticCriteriaPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAdd()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			var item1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var item2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			Factory.Save();
			var collection = new IncidentTriageDiagnosticCriteriaPivotCollection(triage, Factory);
			var pivot1 = collection.AddNew();
			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			collection.Add(pivot2);
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);

			AssertEquals("Add should create the FK relationship to triage", triage.PK, pivot1.IMO_IMT_Triage);
			AssertEquals("Add should create the FK relationship to triage", triage.PK, pivot2.IMO_IMT_Triage);

			pivot1.IMO_IMD_DiagnosticCriteria = item1.PK;
			pivot2.IMO_IMD_DiagnosticCriteria = item2.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var triageReloaded = newFactory.Load<IncidentTriage>(triage.PK);
			var collectionReloaded = new IncidentTriageDiagnosticCriteriaPivotCollection(triageReloaded, newFactory);
			collectionReloaded.Load();
			AssertEquals("2 pivots should exist in the collection", 2, collectionReloaded.Count);
			AssertEquals(true, collectionReloaded.Contains(pivot1.PK));
			AssertEquals(true, collectionReloaded.Contains(pivot2.PK));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			return new IncidentTriageDiagnosticCriteriaPivotCollection(triage, Factory);
		}

		#endregion
	}
}
