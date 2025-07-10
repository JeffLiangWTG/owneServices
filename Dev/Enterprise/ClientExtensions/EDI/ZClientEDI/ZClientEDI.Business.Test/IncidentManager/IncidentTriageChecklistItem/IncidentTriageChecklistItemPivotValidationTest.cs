using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentTriageChecklistItemPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateIMP_IMC_ChecklistItem()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var pivot1 = Factory.New<IncidentTriageChecklistItemPivot>();
			var pivot2 = Factory.New<IncidentTriageChecklistItemPivot>();

			triage.ChecklistPivots.Add(pivot1);
			triage.ChecklistPivots.Add(pivot2);

			pivot1.Validation.ValidateIMP_IMC_ChecklistItem();
			pivot2.Validation.ValidateIMP_IMC_ChecklistItem();
			AssertHasError(pivot1.IMP_IMC_ChecklistItemInfo, "Please enter a value.");
			AssertHasError(pivot2.IMP_IMC_ChecklistItemInfo, "Please enter a value.");

			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			AssertNoErrors(pivot1.IMP_IMC_ChecklistItemInfo);

			pivot2.IMP_IMC_ChecklistItem = checklistItem1.PK;
			AssertHasError(pivot2.IMP_IMC_ChecklistItemInfo, "Cannot add the same checklist item to a triage twice.");

			pivot2.IMP_IMC_ChecklistItem = checklistItem2.PK;
			AssertNoErrors(pivot2.IMP_IMC_ChecklistItemInfo);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var triageReloaded = newFactory.Load<IncidentTriage>(triage.PK);
			AssertEquals("Precondition", 2, triageReloaded.ChecklistPivots.Count);
			var pivotsReloaded = triageReloaded.ChecklistPivots.Cast<IncidentTriageChecklistItemPivot>().ToArray();
			pivotsReloaded[1].IMP_IMC_ChecklistItem = pivotsReloaded[0].IMP_IMC_ChecklistItem;
			AssertHasError(pivotsReloaded[1].IMP_IMC_ChecklistItemInfo, "Cannot add the same checklist item to a triage twice.");
		}

		public void TestValidateIMP_Sequence()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			var checklistItem1 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			var checklistItem2 = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();

			var pivot1 = Factory.New<IncidentTriageChecklistItemPivot>();
			var pivot2 = Factory.New<IncidentTriageChecklistItemPivot>();
			pivot1.IMP_Sequence = 1;
			pivot2.IMP_Sequence = 2;
			triage.ChecklistPivots.Add(pivot1);
			triage.ChecklistPivots.Add(pivot2);

			pivot1.IMP_IMC_ChecklistItem = checklistItem1.PK;
			AssertNoErrors(pivot1.IMP_IMC_ChecklistItemInfo);

			pivot2.IMP_IMC_ChecklistItem = checklistItem2.PK;
			AssertNoErrors(pivot2.IMP_IMC_ChecklistItemInfo);

			Factory.Save();
			pivot1.IMP_Sequence = 0;
			AssertHasError(pivot1.IMP_SequenceInfo, "Please enter a 'Sequence' greater than or equal to 1.");
			pivot1.IMP_Sequence = 1;
			Factory.Save();
			pivot2.IMP_Sequence = 3;
			Assert(pivot2.Triage.RowErrors.Contains("Numbers must be sequential from 1 and increasing by 1."));
		}
	}
}
