using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(IncidentTriageModuleButtonGrid))]
	public class IncidentTriageModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestAttachCore()
		{
			var factory = new BusinessObjectFactory();
			var checklistItem = factory.New<IncidentTriageChecklistItem>();
			var triage = factory.New<IncidentTriage>();
			triage.IMT_Level = "2";
			factory.Save();
			var checklistItemCollection = new IncidentTriageChecklistItemCollection(factory);
			var findBoxList = new IncidentTriageChecklistItemPivotCollection(triage, factory);
			var attacher = new IncidentTriageGridAttacherForTest(checklistItemCollection, findBoxList, ClientModuleRegistration.IncidentTriageChecklistItem, triage);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(checklistItem, boList);

			var atcEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AttachedCode).AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"JOB={triage.IMT_TriageNumber}");
			var controlledATC = atcEventQuery.DeepClone().AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"RFN={checklistItem.IMC_ChecklistNumber}");
			AssertNotNull("An ATC event should be found in the checklistItem.", checklistItem.Logs.Find(controlledATC).FirstOrDefault());
			AssertNotNull("An ATC event should be found in the IncidentTriage.", triage.Logs.Find(controlledATC).FirstOrDefault());
			Assert("BusinessObject list should contain checklistItem pivot", boList.Cast<IncidentTriageChecklistItemPivot>().Any(x => x.ChecklistItem.PK.Equals(checklistItem.PK)));
		}

		public void TestDetachButtonClick()
		{
			var factory = new BusinessObjectFactory();
			var checklistItem = factory.New<IncidentTriageChecklistItem>();
			var triage = factory.New<IncidentTriage>();
			triage.IMT_Level = "2";
			factory.Save();
			var checklistItemCollection = new IncidentTriageChecklistItemCollection(factory);
			var findBoxList = new IncidentTriageChecklistItemPivotCollection(triage, factory);
			var attacher = new IncidentTriageGridAttacherForTest(checklistItemCollection, findBoxList, ClientModuleRegistration.IncidentTriageChecklistItem, triage);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(checklistItem, boList);
			factory.Save();
			triage.ChecklistPivots.Load();
			using (var form = new TestIncidentTriageForm(triage))
			{
				form.Show();
				var zPanel = form.Controls.Find("rightSplitContainer", true);
				var groupBox = zPanel.First().Controls.Find("checklistGroupBox", true);
				var checklistGrid = groupBox.First().Controls.Find("checklistModuleButtonGrid", true).First() as IncidentTriageModuleButtonGrid;
				checklistGrid.InnerGrid.SetAllColumnsVisible(true);
				checklistGrid.InnerGrid.SelectAllElements();
				AssertEquals(1, checklistGrid.InnerGrid.ListManager.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var detachButton = checklistGrid.DetachButtonForTest;
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				detachButton.PerformClick();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				detachButton.PerformClick();
				var expectedPrompt = $"Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled.";

				AssertContains(expectedPrompt, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No IncidentTriageChecklistItem should be linked to IncidentTriage", 0, checklistGrid.InnerGrid.ListManager.Count);
				AssertEquals("IncidentTriageChecklistItemPivot object should be delete after clicking detach.", 0, triage.ChecklistPivots.Count);
				factory.Save();
			}
			var dtcEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DetachedCode).AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"JOB={triage.IMT_TriageNumber}");
			var controlledDTC = dtcEventQuery.DeepClone().AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"RFN={checklistItem.IMC_ChecklistNumber}");
			AssertNotNull("A DTC event should be found in the checklistItem.", checklistItem.Logs.Find(controlledDTC).FirstOrDefault());
			AssertNotNull("A DTC event should be found in the IncidentTriage.", triage.Logs.Find(controlledDTC).FirstOrDefault());
		}
	}

	class IncidentTriageGridAttacherForTest : IncidentTriageModuleButtonGrid.IncidentTriageGridAttacher
	{
		public IncidentTriageGridAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, IncidentTriage incidentTriage)
			: base(destinationCollection, findBoxList, moduleID, incidentTriage)
		{
		}

		public bool TestAttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return base.AttachCore(bizO, listToBulkAdd);
		}
	}

	class TestIncidentTriageForm : IncidentTriageForm
	{
		public TestIncidentTriageForm(IncidentTriage incidentTriage) : base(incidentTriage)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}
