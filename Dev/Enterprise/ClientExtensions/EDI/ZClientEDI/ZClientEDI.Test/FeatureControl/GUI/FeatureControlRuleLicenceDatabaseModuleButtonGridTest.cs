using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.GUI.Testing
{
	[TestedType(typeof(FeatureControlRuleLicenceDatabaseModuleButtonGrid))]
	public class FeatureControlRuleLicenceDatabaseModuleButtonGridTest : ZModuleButtonGridTestBase
	{
		public void TestAttachCore()
		{
			var factory = new BusinessObjectFactory();
			var db1 = factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			var db2 = factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";
			var header = factory.NewWithValidTestData<FeatureControlHeader>();
			var rule1 = header.FeatureControlRules.AddNew();
			rule1.FCR_Description = "AAA";
			rule1.FCR_StartDateUtc = new CargoWise.Types.ZDateTime(2000, 1, 1);
			factory.Save();

			var collection = new LicenceDatabaseNonDependentCollection(factory);
			var findBoxList = new FeatureControlRuleLicenceDatabasePivotCollection(rule1);
			var attacher = new FeatureControlRuleGridAttacherForTest(collection, findBoxList, ClientModuleRegistration.LicenceDatabase, rule1);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(db1, boList);
			Assert("BusinessObject list should contain pivot", boList.Cast<FeatureControlRuleLicenceDatabasePivot>().Any(x => x.FCD_LD_LicenceDatabase.Equals(db1.PK) && x.FCD_LD_DatabaseNumber.Equals(db1.LD_DatabaseNumber)));
		}

		public void TestDetachButtonClick()
		{
			var factory = new BusinessObjectFactory();
			var db1 = factory.NewWithValidTestData<LicenceDatabase>();
			db1.LD_ServerCode = "DB1";
			var db2 = factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_ServerCode = "DB2";
			var header = factory.NewWithValidTestData<FeatureControlHeader>();
			var rule1 = header.FeatureControlRules.AddNew();
			rule1.FCR_Description = "AAA";
			rule1.FCR_StartDateUtc = new CargoWise.Types.ZDateTime(2000, 1, 1);
			factory.Save();

			var collection = new LicenceDatabaseNonDependentCollection(factory);
			var findBoxList = new FeatureControlRuleLicenceDatabasePivotCollection(rule1);
			var attacher = new FeatureControlRuleGridAttacherForTest(collection, findBoxList, ClientModuleRegistration.LicenceDatabase, rule1);
			var boList = new List<BusinessObject>();
			attacher.TestAttachCore(db1, boList);

			factory.Save();
			rule1.LicenceDatabasePivots.Load();
			using (var form = new FeatureControlRuleForm(rule1))
			{
				form.Show();
				var grid = form.Controls.Find("DatabaseModuleButtonGrid", true).Single() as FeatureControlRuleLicenceDatabaseModuleButtonGrid;
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
				AssertEquals("object should be delete after clicking detach.", 0, rule1.LicenceDatabasePivots.Count);
				factory.Save();
			}
		}
	}

	class FeatureControlRuleGridAttacherForTest : FeatureControlRuleLicenceDatabaseModuleButtonGrid.FeatureControlRuleGridAttacher
	{
		public FeatureControlRuleGridAttacherForTest(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, FeatureControlRule controlRule)
			: base(destinationCollection, findBoxList, moduleID, controlRule)
		{
		}

		public bool TestAttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			return base.AttachCore(bizO, listToBulkAdd);
		}
	}
}
