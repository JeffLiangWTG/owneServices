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
	[TestedType(typeof(IncidentDiagnosticCriteriaTriageModuleButtonGrid))]
	public class IncidentDiagnosticCriteriaTriageModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestAttachCore()
		{
			var factory = new BusinessObjectFactory();
			var criteria = factory.New<IncidentDiagnosticCriteria>();
			var item = factory.New<IncidentTriage>();
			item.IMT_Level = "2";
			factory.Save();
			var collection = new IncidentTriageCollection(factory);
			var findBoxList = new IncidentDiagnosticCriteriaTriagePivotCollection(criteria, factory);
			var attacher = new IncidentDiagnosticCriteriaTriageModuleButtonGridAttacherForTest(collection, findBoxList, ClientModuleRegistration.IncidentTriage, criteria);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(item, boList);
			Assert("BusinessObject list should contain pivot", boList.Cast<IncidentTriageDiagnosticCriteriaPivot>().Any(x => x.IMO_IMT_Triage.Equals(item.PK)));
		}

		public void TestDetachButtonClick()
		{
			var factory = new BusinessObjectFactory();
			var item = factory.New<IncidentTriage>();
			item.IMT_Level = "2";
			var criteria = factory.New<IncidentDiagnosticCriteria>();
			factory.Save();
			var collection = new IncidentTriageCollection(factory);
			var findBoxList = new IncidentDiagnosticCriteriaTriagePivotCollection(criteria, factory);
			var attacher = new IncidentDiagnosticCriteriaTriageModuleButtonGridAttacherForTest(collection, findBoxList, ClientModuleRegistration.IncidentTriage, criteria);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(item, boList);
			factory.Save();
			criteria.IncidentTriagePivots.Load();
			using (var form = new IncidentDiagnosticCriteriaForm(criteria))
			{
				form.Show();
				var grid = form.Controls.Find("incidentDiagnosticCriteriaTriageModuleButtonGrid1", true).Single() as IncidentDiagnosticCriteriaTriageModuleButtonGrid;
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
				AssertEquals("No item should be linked", 0, grid.InnerGrid.ListManager.Count);
				AssertEquals("object should be delete after clicking detach.", 0, criteria.IncidentTriagePivots.Count);
				factory.Save();
			}
		}
	}

	class IncidentDiagnosticCriteriaTriageModuleButtonGridAttacherForTest : IncidentDiagnosticCriteriaTriageModuleButtonGrid.IncidentDiagnosticCriteriaGridAttacher
	{
		public IncidentDiagnosticCriteriaTriageModuleButtonGridAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, IncidentDiagnosticCriteria criteria)
			: base(destinationCollection, findBoxList, moduleID, criteria)
		{
		}

		public bool TestAttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return base.AttachCore(bizO, listToBulkAdd);
		}
	}
}
