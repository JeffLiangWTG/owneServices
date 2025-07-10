using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.GUI.Testing
{
	[TestedType(typeof(FeatureControlRuleForm))]
	public class FeatureControlRuleFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule = header.FeatureControlRules.AddNew();
			var form = new FeatureControlRuleForm(rule);
			form.Show();
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("BasicInfoGroupBox", true).Single());
			return form;
		}

		public void TestBeforeParametersOverwrite()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var ruleGlobal = header.FeatureControlRules.AddNew();
			ruleGlobal.FCR_Description = "GlobalRule1";
			ruleGlobal.IsGlobalRule = true;
			ruleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			ruleGlobal.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);
			var ruleClient = header.FeatureControlRules.AddNew();
			ruleClient.FCR_Description = "Rule1";
			ruleClient.IsGlobalRule = false;
			ruleClient.FCR_Parameters = "{\"key2\":\"value2\"}";
			ruleClient.FCR_StartDateUtc = new ZDateTime(2025, 1, 1);
			Factory.Save();

			using (var form = new FeatureControlRuleForm(ruleClient))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ruleClient.FCR_UseGlobalParameters = true;
				AssertEquals("LastMessage", "This will clear the current saved parameters and replace them with the global parameters. Continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, ruleClient.FCR_UseGlobalParameters);
				AssertEquals("{\"key2\":\"value2\"}", ruleClient.FCR_Parameters);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ruleClient.FCR_UseGlobalParameters = true;
				AssertEquals("LastMessage", "This will clear the current saved parameters and replace them with the global parameters. Continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, ruleClient.FCR_UseGlobalParameters);
				AssertEquals("{\"key1\":\"value1\"}", ruleClient.FCR_Parameters);
			}
		}

		public void TestPopupButton()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var ruleGlobal = header.FeatureControlRules.AddNew();
			ruleGlobal.FCR_Description = "GlobalRule1";
			ruleGlobal.IsGlobalRule = true;
			ruleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			ruleGlobal.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);
			Factory.Save();

			using (var form = new FeatureControlRuleForm(ruleGlobal))
			{
				form.Show();
				var button = (ZButton)form.Controls.Find("PopupButton", true).Single();
				form.Activate();
				Application.DoEvents();
				button.PerformClick();
				Application.DoEvents();
				AssertEquals("ZTextBoxPopupForm", ZFormModaliser.LastFormShownForTest.GetType().Name);
			}
		}

		public void TestAuditTabpage()
		{
			using (var form = GetFormToBashCore() as FeatureControlRuleForm)
			{
				form.Show();
				var propertyInfo = typeof(FeatureControlRuleForm)
					.GetProperty("ShowAuditTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				var propertyValue = (bool)propertyInfo.GetValue(form);
				AssertEquals(true, propertyValue);
			}
		}

		public void TestFormCanAttachWithoutSecurity()
		{
			using (var form = GetFormToBashCore() as FeatureControlRuleForm)
			{
				Assert(form is ICanAttachWithoutSecurity);
			}
		}

		public void TestRuleTypeChangeIsManagedBySecurityRight()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule = header.FeatureControlRules.AddNew();
			rule.FCR_Description = "Rule1";
			rule.IsGlobalRule = true;
			rule.FCR_IsActive = false;
			rule.FCR_Parameters = "{\"key2\":\"value2\"}";
			rule.FCR_StartDateUtc = new ZDateTime(2025, 1, 1);
			Factory.Save();

			bool originalFeatureAdminSecurity = EDISecurityCheckpoints.FeatureAdmin.IsAllowed;
			try
			{
				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = false;
				using (var form = new FeatureControlRuleForm(rule))
				{
					form.Show();

					var fieldInfo1 = typeof(FeatureControlRuleForm).GetField("GlobalCheckBox", BindingFlags.NonPublic | BindingFlags.Instance);
					var fieldValue1 = (ZCheckBox)fieldInfo1.GetValue(form);
					AssertEquals(true, fieldValue1.ReadOnly);

					var fieldInfo2 = typeof(FeatureControlRuleForm).GetField("FeatureSetCheckBox", BindingFlags.NonPublic | BindingFlags.Instance);
					var fieldValue2 = (ZCheckBox)fieldInfo2.GetValue(form);
					AssertEquals(true, fieldValue2.ReadOnly);

					var fieldInfo3 = typeof(FeatureControlRuleForm).GetField("FeatureSetDropEdit", BindingFlags.NonPublic | BindingFlags.Instance);
					var fieldValue3 = (ZGuidDropEdit)fieldInfo3.GetValue(form);
					AssertEquals(true, fieldValue3.ReadOnly);
				}

				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = true;
				using (var form = new FeatureControlRuleForm(rule))
				{
					form.Show();

					var fieldInfo1 = typeof(FeatureControlRuleForm).GetField("GlobalCheckBox", BindingFlags.NonPublic | BindingFlags.Instance);
					var fieldValue1 = (ZCheckBox)fieldInfo1.GetValue(form);
					AssertEquals(false, fieldValue1.ReadOnly);

					var fieldInfo2 = typeof(FeatureControlRuleForm).GetField("FeatureSetCheckBox", BindingFlags.NonPublic | BindingFlags.Instance);
					var fieldValue2 = (ZCheckBox)fieldInfo2.GetValue(form);
					AssertEquals(false, fieldValue2.ReadOnly);

					var fieldInfo3 = typeof(FeatureControlRuleForm).GetField("FeatureSetDropEdit", BindingFlags.NonPublic | BindingFlags.Instance);
					var fieldValue3 = (ZGuidDropEdit)fieldInfo3.GetValue(form);
					AssertEquals(false, fieldValue3.ReadOnly);
				}
			}
			finally
			{
				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = originalFeatureAdminSecurity;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestOnRuleActiveValueChanged()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule = header.FeatureControlRules.AddNew();
			rule.FCR_Description = "Rule1";
			rule.IsGlobalRule = true;
			rule.FCR_IsActive = false;
			rule.FCR_Parameters = "{\"key2\":\"value2\"}";
			rule.FCR_StartDateUtc = new ZDateTime(2025, 1, 1);
			Factory.Save();

			bool originalFeatureAdminSecurity = EDISecurityCheckpoints.FeatureAdmin.IsAllowed;
			try
			{
				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = false;
				using (var form = new FeatureControlRuleForm(rule))
				{
					form.Show();
					rule.FCR_IsActive = true;
					AssertEquals(EDISecurityCheckpoints.FeatureAdmin.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("\"Global rule cannot be set to active without security granted\"", false, rule.FCR_IsActive);
				}

				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = true;
				using (var form = new FeatureControlRuleForm(rule))
				{
					form.Show();
					rule.FCR_IsActive = true;
					AssertEquals(true, rule.FCR_IsActive);
				}
			}
			finally
			{
				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = originalFeatureAdminSecurity;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestOnRuleIsGlobalValueChanged()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule = header.FeatureControlRules.AddNew();
			rule.FCR_Description = "Rule1";
			rule.IsGlobalRule = false;
			rule.FCR_IsActive = true;
			rule.FCR_Parameters = "{\"key2\":\"value2\"}";
			rule.FCR_StartDateUtc = new ZDateTime(2025, 1, 1);
			Factory.Save();

			bool originalFeatureAdminSecurity = EDISecurityCheckpoints.FeatureAdmin.IsAllowed;
			try
			{
				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = false;
				using (var form = new FeatureControlRuleForm(rule))
				{
					form.Show();
					rule.IsGlobalRule = true;
					AssertEquals(EDISecurityCheckpoints.FeatureAdmin.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Active rule cannot be marked as global without security granted", false, rule.IsGlobalRule);
				}

				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = true;
				using (var form = new FeatureControlRuleForm(rule))
				{
					form.Show();
					rule.IsGlobalRule = true;
					AssertEquals(true, rule.IsGlobalRule);
				}
			}
			finally
			{
				EDISecurityCheckpoints.FeatureAdmin.IsAllowed = originalFeatureAdminSecurity;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}
	}
}
