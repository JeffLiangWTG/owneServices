using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Module.Testing
{
	[TestedType(typeof(BarcodeValidationController))]
	class BarcodeValidationControllerTest : BarcodeRuleSetControllerTest
	{
		#region TestGetIDForBusinessEntity

		public void TestGetIDForBusinessEntity()
		{
			var ruleSet = Helper.CreateRuleSet();
			var rule1 = Helper.CreateValidationRule(ruleSet, DummyTargetFields.Codes.TargetField1);
			AssertEquals(false, OpenedFormCache.GetInstance().Contains(ruleSet.PK.ToGuid(), GetControllerID().ToString()));

			var controller = new BarcodeValidationController();
			using (var form = (ZForm)controller.ShowEditForm(rule1))
			{
				AssertEquals(false, OpenedFormCache.GetInstance().Contains(ruleSet.PK.ToGuid(), GetControllerID().ToString()));
			}
		}

		#endregion

		#region TestGetNewBusinessEntityInLocalFactory

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			var controller = new BarcodeValidationController();
			BarcodeValidationRule rule;
			using (var form = (ZForm)controller.ShowNewForm())
			{
				var ruleSet = (BarcodeRuleSet)form.BusinessEntity;
				ruleSet.BRS_Module = BarcodeModuleTypes.Codes.Warehouse;
				rule = Helper.CreateValidationRule(ruleSet, DummyTargetFields.Codes.TargetField1);
				Factory.Save();
			}

			using (var form = (ZForm)controller.ShowEditForm(rule))
			{
				var ruleSet = (BarcodeRuleSet)form.BusinessEntity;
				AssertNotNull(ruleSet);
				AssertContainsExactElementsInAnyOrder(new[] { rule.PK }, ruleSet.ValidationRules.Select(r => r.PK));

				var grid = (ZGrid)form.Controls["MainPanel"].Controls["MainTabControl"].Controls["MainTabPage"]
					.Controls["RulesPanel"].Controls["RulesTabControl"].Controls["ValidationRulesTabPage"].Controls["ValidationRulesGroupBox"].Controls["ValidationRulesGrid"];
				AssertContainsExactElementsInAnyOrder("If Rule was double clicked from Module, it should be selected when the form is shown.", new[] { rule.PK }, grid.SelectedElements.Select(r => r.PK));
			}
		}

		#endregion

		#region TestDeleteSingleRule

		public void TestDeleteSingleRule_AnswerYes()
		{
			var ruleSet1 = Helper.CreateRuleSet();
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField2);
			Factory.Save();

			var controller = new BarcodeValidationControllerForTest();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.ShowDeleteFormForTest(rule1);
			AssertEquals("Rule1 deleting should show message", "Are you sure you wish to delete the selected Rule?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Rule1 should be deleted if the user selects *Yes*", true, rule1.IsDeleted);
			AssertEquals("RuleSet1 should *not* be deleted", false, ruleSet1.IsDeleted);
		}

		public void TestDeleteSingleRule_AnswerNo()
		{
			var ruleSet1 = Helper.CreateRuleSet();
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField2);
			Factory.Save();

			var controller = new BarcodeValidationControllerForTest();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			controller.ShowDeleteFormForTest(rule1);
			AssertEquals("Rule1 deleting should show message", "Are you sure you wish to delete the selected Rule?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Rule1 should *not* be deleted if the user selects *No*", false, rule1.IsDeleted);
			AssertEquals("RuleSet1 should *not* be deleted", false, ruleSet1.IsDeleted);
		}

		public void TestDeleteSingleRule_NoPermission()
		{
			Env.Security.BarcodeParsingDelete.IsAllowed = false;
			var ruleSet1 = Helper.CreateRuleSet();
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			var controller = new BarcodeValidationControllerForTest();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.ShowDeleteFormForTest(rule1);
			AssertEquals("Rule1 deleting should show message that contains Security Rights",
				true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("You do not have the appropriate security rights to run this function."));
			AssertEquals("Rule1 should *not* be deleted if the user has no permission", false, rule1.IsDeleted);
		}

		public void TestDeleteSingleRule_ThrowsZCannotSaveException()
		{
			var ruleSet1 = Helper.CreateRuleSet();
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			void action(BusinessObjectFactory factory)
			{
				Factory.Saving -= action;
				throw new ZCannotSaveException("Test - Cannot Save", "Test Exception");
			}
			Factory.Saving += action;

			var controller = new BarcodeValidationControllerForTest();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.ShowDeleteFormForTest(rule1);
			AssertEquals("Rule1 deleting should show exception message", "Test - Cannot Save", UnitTestUserNotification.Instance.LastMessage.Text);

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var ruleInOtherFactory = otherFactory.Load<BarcodeValidationRule>(rule1.PK);
			AssertEquals("Rule1 should *not* be deleted if saving exception occurs", false, ruleInOtherFactory.IsDeleted);
		}

		#endregion

		#region TestDeleteMultipleRules

		public void TestDeleteMultipleRules_AnswerNo()
		{
			Env.Security.BarcodeParsingDelete.IsAllowed = true;
			var buyer = Helper.CreateOrg("Buyer");
			var ruleSet1 = Helper.CreateRuleSet(buyer);
			var ruleSet2 = Helper.CreateRuleSet();
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField2);
			var rule3 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			Controller.DeleteMultiple(new[] { rule1, rule3 });

			AssertEquals("Deleting rules should show message", "Are you sure you wish to delete the selected Rules?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Rule1 should *not* be deleted if the user selects *No*", false, rule1.IsDeleted);
			AssertEquals("Rule3 should *not* be deleted if the user selects *No*", false, rule3.IsDeleted);
			AssertEquals("RuleSet1 should *not* be deleted", false, ruleSet1.IsDeleted);
		}

		public void TestDeleteMultipleRules_AnswerYes()
		{
			Env.Security.BarcodeParsingDelete.IsAllowed = true;
			var buyer = Helper.CreateOrg("Buyer");
			var ruleSet1 = Helper.CreateRuleSet(buyer);
			var ruleSet2 = Helper.CreateRuleSet();
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField2);
			var rule3 = Helper.CreateValidationRule(ruleSet2, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Controller.DeleteMultiple(new[] { rule1, rule3 });

			AssertEquals("Deleting rules should show message", "Are you sure you wish to delete the selected Rules?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Rule1 should be deleted if the user selects *Yes*", true, rule1.IsDeleted);
			AssertEquals("Rule3 should be deleted if the user selects *Yes*", true, rule3.IsDeleted);
			AssertEquals("RuleSet1 should *not* be deleted", false, ruleSet1.IsDeleted);
			AssertEquals("RuleSet2 should be deleted if Rule3 is the only one rule left", true, ruleSet2.IsDeleted);
		}

		public void TestDeleteMultipleRules_NoPermission()
		{
			Env.Security.BarcodeParsingDelete.IsAllowed = false;
			var buyer = Helper.CreateOrg("Buyer");
			var ruleSet1 = Helper.CreateRuleSet(buyer);
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField2);
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Controller.DeleteMultiple(new[] { rule1, rule2 });
			AssertEquals("Deleting rules should show message that contains Security Rights",
				true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("You do not have the appropriate security rights to run this function."));
			AssertEquals("Rule1 should *not* be deleted if the user has no permission", false, rule1.IsDeleted);
			AssertEquals("Rule2 should *not* be deleted if the user has no permission", false, rule2.IsDeleted);
		}

		public void TestDeleteMultipleRules_ThrowsZCannotSaveException()
		{
			var buyer = Helper.CreateOrg("Buyer");
			var ruleSet1 = Helper.CreateRuleSet(buyer);
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField2);
			Factory.Save();

			void action(BusinessObjectFactory factory)
			{
				Factory.Saving -= action;
				throw new ZCannotSaveException("Test - Cannot Save", "Test Exception");
			}
			Factory.Saving += action;

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Controller.DeleteMultiple(new[] { rule1, rule2 });
			AssertEquals("Rule1 deleting should show exception message", "Test - Cannot Save", UnitTestUserNotification.Instance.LastMessage.Text);

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var rule1InOtherFactory = otherFactory.Load<BarcodeValidationRule>(rule1.PK);
			AssertEquals("Rule1 should *not* be deleted if saving exception occurs", false, rule1InOtherFactory.IsDeleted);
			var rule2InOtherFactory = otherFactory.Load<BarcodeValidationRule>(rule2.PK);
			AssertEquals("Rule2 should *not* be deleted if saving exception occurs", false, rule2InOtherFactory.IsDeleted);
		}

		#endregion

		#region TestDeleteRuleSet

		public void TestDeleteRuleSet_LastValidationRule()
		{
			Env.Security.BarcodeParsingDelete.IsAllowed = true;
			var buyer = Helper.CreateOrg("Buyer");
			var ruleSet1 = Helper.CreateRuleSet(buyer);
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Controller.DeleteMultiple(new[] { rule1 });
			AssertEquals("Deleting rules should show message", "Are you sure you wish to delete the selected Rules?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Rule1 should be deleted", true, rule1.IsDeleted);
			AssertEquals("RuleSet should be deleted when the last rule is deleted", true, ruleSet1.IsDeleted);
		}

		public void TestDeleteRuleSet_MultipleValidationRule()
		{
			var buyer = Helper.CreateOrg("Buyer");
			var ruleSet1 = Helper.CreateRuleSet(buyer);
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField2);
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Controller.DeleteMultiple(new[] { rule1, rule2 });
			AssertEquals("Deleting rules should show message", "Are you sure you wish to delete the selected Rules?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Rule1 should be deleted", true, rule1.IsDeleted);
			AssertEquals("Rule2 should be deleted", true, rule2.IsDeleted);
			AssertEquals("RuleSet should be deleted when the last rule is deleted", true, ruleSet1.IsDeleted);
		}

		public void TestDeleteRuleSet_RemainingValidationRules()
		{
			Env.Security.BarcodeParsingDelete.IsAllowed = true;
			var buyer = Helper.CreateOrg("Buyer");
			var ruleSet1 = Helper.CreateRuleSet(buyer);
			var rule1 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var rule2 = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField2);
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Controller.DeleteMultiple(new[] { rule1 });
			AssertEquals("Deleting rules should show message", "Are you sure you wish to delete the selected Rules?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Rule1 should be deleted", true, rule1.IsDeleted);
			AssertEquals("RuleSet should not be deleted when there are remaining validation rules on the ruleset", false, ruleSet1.IsDeleted);
		}

		public void TestDeleteRuleSet_LastValidationRule_RemainingParsingRules()
		{
			Env.Security.BarcodeParsingDelete.IsAllowed = true;
			var buyer = Helper.CreateOrg("Buyer");
			var ruleSet1 = Helper.CreateRuleSet(buyer);
			var validationRule = Helper.CreateValidationRule(ruleSet1, DummyTargetFields.Codes.TargetField1);
			var parsingRule = Helper.CreateRule(ruleSet1, "r1");
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Controller.DeleteMultiple(new[] { validationRule });
			AssertEquals("Deleting rules should show message", "Are you sure you wish to delete the selected Rules?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Rule1 should be deleted", true, validationRule.IsDeleted);
			AssertEquals("RuleSet should not be deleted when there are remaining parsing rules on the ruleset", false, ruleSet1.IsDeleted);
		}

		#endregion

		#region TestTypeOfTopLevelBusinessObject

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(BarcodeValidationRule), Controller.TypeOfTopLevelBusinessObject);
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID() => ControllerIDs.BarcodeValidation;
		
		#endregion
	}

	class BarcodeValidationControllerForTest : BarcodeValidationController
	{
		internal IZForm ShowDeleteFormForTest(BusinessObject sourceEntity)
		{
			return ShowDeleteForm(sourceEntity);
		}
	}
}
