using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(InvestigationItemDiagnosticCriteriaPivotCollection))]
	public class InvestigationItemDiagnosticCriteriaPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAdd()
		{
			var criteria = Factory.NewWithValidTestData<InvestigationItem>();
			var item1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			var item2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			Factory.Save();
			var collection = new InvestigationItemDiagnosticCriteriaPivotCollection(criteria, Factory);
			var pivot1 = collection.AddNew();
			var pivot2 = Factory.New<DiagnosticCriteriaInvestigationItemLink>();
			collection.Add(pivot2);
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);

			AssertEquals("Add should create the FK relationship to triage", criteria.PK, pivot1.DIL_INV_InvestigationItem);
			AssertEquals("Add should create the FK relationship to triage", criteria.PK, pivot2.DIL_INV_InvestigationItem);

			pivot1.DIL_IMD_DiagnosticCriteria = item1.PK;
			pivot2.DIL_IMD_DiagnosticCriteria = item2.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var criteriaReloaded = newFactory.Load<InvestigationItem>(criteria.PK);
			var collectionReloaded = new InvestigationItemDiagnosticCriteriaPivotCollection(criteriaReloaded, newFactory);
			collectionReloaded.Load();
			AssertEquals("2 pivots should exist in the collection", 2, collectionReloaded.Count);
			AssertEquals(true, collectionReloaded.Contains(pivot1.PK));
			AssertEquals(true, collectionReloaded.Contains(pivot2.PK));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var item = Factory.NewWithValidTestData<InvestigationItem>();
			return new InvestigationItemDiagnosticCriteriaPivotCollection(item, Factory);
		}

		#endregion
	}
}
