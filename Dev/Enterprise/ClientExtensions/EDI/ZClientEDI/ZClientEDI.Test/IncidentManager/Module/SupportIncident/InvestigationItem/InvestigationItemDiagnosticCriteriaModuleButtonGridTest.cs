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
	[TestedType(typeof(InvestigationItemDiagnosticCriteriaModuleButtonGrid))]
	public class InvestigationItemDiagnosticCriteriaModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestAttachCore()
		{
			var factory = new BusinessObjectFactory();
			var item = factory.New<InvestigationItem>();
			var criteria = factory.New<IncidentDiagnosticCriteria>();

			factory.Save();
			var collection = new IncidentDiagnosticCriteriaCollection(factory);
			var findBoxList = new InvestigationItemDiagnosticCriteriaPivotCollection(item, factory);
			var attacher = new InvestigationItemDiagnosticCriteriaModuleButtonGridAttacherForTest(collection, findBoxList, ClientModuleRegistration.IncidentDiagnosticCriteria, item);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(criteria, boList);
			Assert("BusinessObject list should contain pivot", boList.Cast<DiagnosticCriteriaInvestigationItemLink>().Any(x => x.DIL_IMD_DiagnosticCriteria.Equals(criteria.PK)));
		}

		public void TestDetachButtonClick()
		{
			var factory = new BusinessObjectFactory();
			var criteria = factory.New<IncidentDiagnosticCriteria>();
			var item = factory.New<InvestigationItem>();
			factory.Save();
			var collection = new IncidentDiagnosticCriteriaCollection(factory);
			var findBoxList = new InvestigationItemDiagnosticCriteriaPivotCollection(item, factory);
			var attacher = new InvestigationItemDiagnosticCriteriaModuleButtonGridAttacherForTest(collection, findBoxList, ClientModuleRegistration.IncidentDiagnosticCriteria, item);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(criteria, boList);
			factory.Save();
			item.DiagnosticCriteriaPivots.Load();
			using (var form = new InvestigationItemForm(item))
			{
				form.Show();
				var grid = form.Controls.Find("investigationItemDiagnosticCriteriaModuleButtonGrid", true).Single() as InvestigationItemDiagnosticCriteriaModuleButtonGrid;
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
				AssertEquals("object should be delete after clicking detach.", 0, item.DiagnosticCriteriaPivots.Count);
				factory.Save();
			}
		}
	}

	class InvestigationItemDiagnosticCriteriaModuleButtonGridAttacherForTest : InvestigationItemDiagnosticCriteriaModuleButtonGrid.InvestigationItemGridAttacher
	{
		public InvestigationItemDiagnosticCriteriaModuleButtonGridAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, InvestigationItem investigationItem)
			: base(destinationCollection, findBoxList, moduleID, investigationItem)
		{
		}

		public bool TestAttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return base.AttachCore(bizO, listToBulkAdd);
		}
	}
}
