using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(IncidentTriageDiagnosticCriteriaModuleButtonGrid))]
	public class IncidentTriageDiagnosticCriteriaModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestAttachCore()
		{
			var factory = new BusinessObjectFactory();
			var item = factory.New<IncidentDiagnosticCriteria>();
			var triage = factory.New<IncidentTriage>();
			triage.IMT_Level = "2";
			factory.Save();
			var collection = new IncidentDiagnosticCriteriaCollection(factory);
			var findBoxList = new IncidentTriageDiagnosticCriteriaPivotCollection(triage, factory);
			var attacher = new IncidentTriageDiagnosticCriteriaModuleButtonGridAttacherForTest(collection, findBoxList, ClientModuleRegistration.IncidentDiagnosticCriteria, triage);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(item, boList);
			Assert("BusinessObject list should contain pivot", boList.Cast<IncidentTriageDiagnosticCriteriaPivot>().Any(x => x.DiagnosticCriteria.PK.Equals(item.PK)));
		}

		public void TestDetachButtonClick()
		{
			var factory = new BusinessObjectFactory();
			var item = factory.New<IncidentDiagnosticCriteria>();
			var triage = factory.New<IncidentTriage>();
			triage.IMT_Level = "2";
			factory.Save();
			var collection = new IncidentDiagnosticCriteriaCollection(factory);
			var findBoxList = new IncidentTriageDiagnosticCriteriaPivotCollection(triage, factory);
			var attacher = new IncidentTriageDiagnosticCriteriaModuleButtonGridAttacherForTest(collection, findBoxList, ClientModuleRegistration.IncidentDiagnosticCriteria, triage);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(item, boList);
			factory.Save();
			triage.DiagnosticCriteriaPivots.Load();
			using (var form = new TestIncidentTriageForm(triage))
			{
				form.Show();
				var zPanel = form.Controls.Find("rightSplitContainer", true);
				var groupBox = zPanel.First().Controls.Find("tagsGroupBox", true);
				var grid = groupBox.First().Controls.Find("tagsGrid", true).First() as IncidentTriageDiagnosticCriteriaModuleButtonGrid;
				grid.InnerGrid.SetAllColumnsVisible(true);
				grid.InnerGrid.SelectAllElements();
				AssertEquals(1, grid.InnerGrid.ListManager.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var detachButton = grid.DetachButtonForTest;
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				detachButton.PerformClick();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				detachButton.PerformClick();
				var expectedPrompt = $"Are you sure you want to detach the selected records? New records will be deleted.\r\nAll unsaved changes in detached items will be canceled.";

				AssertContains(expectedPrompt, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No IncidentDiagnosticCriteria should be linked to IncidentTriage", 0, grid.InnerGrid.ListManager.Count);
				AssertEquals("IncidentDiagnosticCriteriaPivot object should be delete after clicking detach.", 0, triage.DiagnosticCriteriaPivots.Count);
				factory.Save();
			}
		}
	}

	class IncidentTriageDiagnosticCriteriaModuleButtonGridAttacherForTest : IncidentTriageDiagnosticCriteriaModuleButtonGrid.IncidentTriageGridAttacher
	{
		public IncidentTriageDiagnosticCriteriaModuleButtonGridAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, IncidentTriage incidentTriage)
			: base(destinationCollection, findBoxList, moduleID, incidentTriage)
		{
		}

		public bool TestAttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return base.AttachCore(bizO, listToBulkAdd);
		}
	}
}
