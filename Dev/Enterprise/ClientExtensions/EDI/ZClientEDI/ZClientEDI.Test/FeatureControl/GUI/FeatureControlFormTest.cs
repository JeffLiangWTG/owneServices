using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.GUI.Testing
{
	[TestedType(typeof(FeatureControlForm))]
	public class FeatureControlFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var featureControl = Factory.New<FeatureControlHeader>();
			var form = new FeatureControlForm(featureControl);
			form.Show();
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("InfoGroupBox1", true).Single());
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("InfoGroupBox2", true).Single());
			return form;
		}

		public void TestButtons()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
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
			var ruleClient2 = header.FeatureControlRules.AddNew();
			ruleClient2.FCR_Description = "Rule2";
			ruleClient2.IsGlobalRule = false;
			ruleClient2.FCR_UseGlobalParameters = true;
			ruleClient2.FCR_StartDateUtc = new ZDateTime(2026, 1, 1);
			Factory.Save();

			using (var form = new FeatureControlForm(header))
			{
				form.Show();

				var rulesGrid = (ZGrid)form.Controls.Find("RulesGrid", true).Single();
				var rules = rulesGrid.ListManager.List.OfType<FeatureControlRule>().ToArray();

				header.FCM_Description = ZGuid.NewZGuid().ToStringKey();
				AssertEquals(true, header.HasChanges);
				UnitTestUserNotification.Instance.AddYesAnswer();
				var newButton = (ZButton)form.Controls.Find("NewRuleButton", true).Single();
				newButton.PerformClick();

				AssertEquals("LastMessage", "You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.HasChanges);
				AssertType<FeatureControlRuleForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertType<FeatureControlRule>(ZFormModaliser.LastIBusinessShownOnDialogForTest);
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;

				rulesGrid.ListManager.Position = rules.IndexOf(x => x.IsGlobalRule);
				var editButton = (ZButton)form.Controls.Find("EditRuleButton", true).Single();
				editButton.PerformClick();
				AssertType<FeatureControlRuleForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertType<FeatureControlRule>(ZFormModaliser.LastIBusinessShownOnDialogForTest);
				AssertEquals(ruleGlobal.PK, ZFormModaliser.LastIBusinessShownOnDialogForTest.Identifier);
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;

				UnitTestUserNotification.Instance.AddOKAnswer();
				var deleteButton = (ZButton)form.Controls.Find("DeleteRuleButton", true).Single();
				deleteButton.PerformClick();
				AssertEquals("LastMessage", "This global rule cannot be deleted because its parameters are currently in use by other rules.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, ruleGlobal.IsDeleted);

				rulesGrid.ListManager.Position = rules.IndexOf(x => x.FCR_Description == "Rule1");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				deleteButton.PerformClick();
				AssertEquals("LastMessage", "Are you sure you wish to delete this rule? This operation cannot be undone.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, ruleClient.IsDeleted);
			}
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestAuditTabpage()
		{
			using (var form = GetFormToBashCore() as FeatureControlForm)
			{
				form.Show();
				var propertyInfo = typeof(FeatureControlForm)
					.GetProperty("ShowAuditTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				var propertyValue = (bool)propertyInfo.GetValue(form);
				AssertEquals(true, propertyValue);
			}
		}

		public void TestSetControlAccessBehavior()
		{
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);

			var headerWithGroup = Factory.NewWithValidTestData<FeatureControlHeader>();
			var headerWithoutGroup = Factory.NewWithValidTestData<FeatureControlHeader>();

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.Staff.Add(currentStaff);
			headerWithGroup.FCM_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			EDISecurityCheckpoints.AllowFeatureEditForAnyReleaseGroupReferenceName.IsAllowed = true;
			using (var form = new FeatureControlForm(headerWithGroup))
			{
				form.Show();
				AssertControlAccess(form, isOtherControlsReadOnly: false, isReleaseGroupReadOnly: false, "Has security right: all editable");
			}

			EDISecurityCheckpoints.AllowFeatureEditForAnyReleaseGroupReferenceName.IsAllowed = false;
			using (var form = new FeatureControlForm(headerWithGroup))
			{
				form.Show();
				AssertControlAccess(form, isOtherControlsReadOnly: false, isReleaseGroupReadOnly: true, "In group, no security right: all editable except ReleaseGroup");
			}

			using (var form = new FeatureControlForm(headerWithoutGroup))
			{
				form.Show();
				AssertControlAccess(form, isOtherControlsReadOnly: true, isReleaseGroupReadOnly: true, "Not in group, no security right: all read-only");
			}

			var headerWithoutGroupNotInDB = Factory.NewWithValidTestData<FeatureControlHeader>();
			headerWithoutGroupNotInDB.FCM_GG_ReleaseGroup = ZGuid.Empty;
			using (var form = new FeatureControlForm(headerWithoutGroupNotInDB))
			{
				form.Show();
				AssertControlAccess(form, isOtherControlsReadOnly: true, isReleaseGroupReadOnly: false, "Group is null: only ReleaseGroup editable");
			}
		}

		void AssertControlAccess(Form form, bool isOtherControlsReadOnly, bool isReleaseGroupReadOnly, string comment)
		{
			var descriptionTextBox = (ZTextBox)form.Controls.Find("DescriptionTextBox", true).Single();
			var activeWorkItemGuidFindBox = (ZGuidFindBox)form.Controls.Find("ActiveWorkItemGuidFindBox", true).Single();
			var deactivateWorkItemGuidFindBox = (ZGuidFindBox)form.Controls.Find("DeactivateWorkItemGuidFindBox", true).Single();
			var rulesGrid = (ZGrid)form.Controls.Find("RulesGrid", true).Single();
			var newRuleButton = (ZButton)form.Controls.Find("NewRuleButton", true).Single();
			var editRuleButton = (ZButton)form.Controls.Find("EditRuleButton", true).Single();
			var deleteRuleButton = (ZButton)form.Controls.Find("DeleteRuleButton", true).Single();
			var releaseGroupFindBox = (ZGuidFindBox)form.Controls.Find("ReleaseGroupFindBox", true).Single();

			AssertEquals(comment + " - DescriptionTextBox", isOtherControlsReadOnly, descriptionTextBox.ReadOnly);
			AssertEquals(comment + " - ActiveWorkItemGuidFindBox", isOtherControlsReadOnly, activeWorkItemGuidFindBox.ReadOnly);
			AssertEquals(comment + " - DeactivateWorkItemGuidFindBox", isOtherControlsReadOnly, deactivateWorkItemGuidFindBox.ReadOnly);
			AssertEquals(comment + " - RulesGrid", isOtherControlsReadOnly, rulesGrid.ReadOnly);
			AssertEquals(comment + " - NewRuleButton", isOtherControlsReadOnly, newRuleButton.ReadOnly);
			AssertEquals(comment + " - EditRuleButton", isOtherControlsReadOnly, editRuleButton.ReadOnly);
			AssertEquals(comment + " - DeleteRuleButton", isOtherControlsReadOnly, deleteRuleButton.ReadOnly);
			AssertEquals(comment + " - ReleaseGroupFindBox", isReleaseGroupReadOnly, releaseGroupFindBox.ReadOnly);
		}
	}
}
