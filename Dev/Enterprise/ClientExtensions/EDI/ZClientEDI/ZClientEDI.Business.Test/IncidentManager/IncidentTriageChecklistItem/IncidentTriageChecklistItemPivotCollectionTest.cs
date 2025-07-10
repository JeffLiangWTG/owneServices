using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentTriageChecklistItemPivotCollection))]
	public class IncidentTriageChecklistItemPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAdd()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			Factory.Save();
			var collection = new IncidentTriageChecklistItemPivotCollection(triage, Factory);
			var checklistItemPivot1 = collection.AddNew();
			var checklistItemPivot2 = Factory.New<IncidentTriageChecklistItemPivot>();
			collection.Add(checklistItemPivot2);
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);

			AssertEquals("Add should create the FK relationship to triage", triage.PK, checklistItemPivot1.IMP_IMT_Triage);
			AssertEquals("Add should create the FK relationship to triage", triage.PK, checklistItemPivot2.IMP_IMT_Triage);

			checklistItemPivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			checklistItemPivot2.IMP_IMC_ChecklistItem = checklistItem2.PK;
			checklistItemPivot1.IMP_Sequence = 1;
			checklistItemPivot2.IMP_Sequence = 2;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var triageReloaded = newFactory.Load<IncidentTriage>(triage.PK);
			var collectionReloaded = new IncidentTriageChecklistItemPivotCollection(triageReloaded, newFactory);
			collectionReloaded.Load();
			AssertEquals("2 Checklist item pivots should exist in the collection", 2, collectionReloaded.Count);
			AssertEquals("Reloaded checklist item pivots should match the originally added ones", checklistItemPivot1.PK, collectionReloaded[0].PK);
			AssertEquals("Reloaded checklist item pivots should match the originally added ones", checklistItemPivot2.PK, collectionReloaded[1].PK);
		}

		public void TestOrder()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			var collection = new IncidentTriageChecklistItemPivotCollection(triage, Factory);
			var checklistItemPivot1 = collection.AddNew();
			var checklistItemPivot2 = Factory.New<IncidentTriageChecklistItemPivot>();
			collection.Add(checklistItemPivot2);

			AssertEquals("Add should create the FK relationship to triage", triage.PK, checklistItemPivot1.IMP_IMT_Triage);
			AssertEquals("Add should create the FK relationship to triage", triage.PK, checklistItemPivot2.IMP_IMT_Triage);

			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItemPivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			checklistItemPivot2.IMP_IMC_ChecklistItem = checklistItem2.PK;
			checklistItemPivot2.IMP_Sequence = 1;
			checklistItemPivot1.IMP_Sequence = 2;
			collection.Sort(IncidentTriageChecklistItemPivotSchema.Constants.IMP_Sequence);

			AssertEquals("Pivots should be ordered by sequence", checklistItemPivot2.PK, collection[0].PK);
			AssertEquals("Pivots should be ordered by sequence", checklistItemPivot1.PK, collection[1].PK);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var triageReloaded = newFactory.Load<IncidentTriage>(triage.PK);
			var collectionReloaded = new IncidentTriageChecklistItemPivotCollection(triageReloaded, newFactory);
			collectionReloaded.Load();
			AssertEquals("2 Checklist item pivots should exist in the collection", 2, collectionReloaded.Count);
			AssertEquals("Pivots should be ordered by sequence on load", checklistItemPivot2.PK, collectionReloaded[0].PK);
			AssertEquals("Pivots should be ordered by sequence on load", checklistItemPivot1.PK, collectionReloaded[1].PK);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			return new IncidentTriageChecklistItemPivotCollection(triage, Factory);
		}

		#endregion
	}
}
