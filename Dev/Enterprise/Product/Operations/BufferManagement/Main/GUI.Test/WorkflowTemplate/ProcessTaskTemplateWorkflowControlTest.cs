using System;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class ProcessTaskTemplateWorkflowControlTest : BMSGUITestCase
	{
		public void TestDeleteJobHeader_ShouldNotAllow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var workflow = template.ProcessHeaders.AddNew();

			using (var control = new ProcessTaskTemplateWorkflowControl())
			using (var form = new ZForm(template))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.ListManager.Position = 0;
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(2, control.WorkflowsGrid.List.Count);
				AssertEquals("The Job-level workflow cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				control.WorkflowsGrid.ListManager.Position = 1;
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(1, control.WorkflowsGrid.List.Count);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(true, workflow.IsDeleted);
			}
		}

		#region Column Visibility

		public void TestDefaultAndInvisibleColumns()
		{
			using (var control = new ProcessTaskTemplateWorkflowControl())
			{
				var allColumns = control.WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();
				AssertEquals(19, allColumns.Count);

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"ReleaseGroup+GG_Desc",
					"FH_AllowTaskAutoAssignment",
					"FH_IsCriticalHandover",
					"FH_IsStandby",
					"FH_DateAcceptability",
					"FH_TimeDelayFactor",
					"FH_TimeDelayMinutes",
					"EffectiveNudge",
					"ProcessHeaderType",
					"Sequence",
					"FH_CompletionStatement",
					"FH_GG_ReleaseGroup",
					"FH_Category",
					"CategoryDescription",
				}, visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"FH_AgreedDeliveryDateDefaultsFrom",
					"FH_AgreedDeliveryDateDefaultHoursOffset",
					"FH_EarliestStartDateDefaultsFrom",
					"FH_EarliestStartDefaultHoursOffset",
					"FH_MilestoneCompletionPivotKey",
				}, invisibleColumns);
			}
		}

		public void TestDefaultAndInvisibleColumns_WhenReleaseSequenceModuleEnabled()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var control = new ProcessTaskTemplateWorkflowControl())
			{
				var allColumns = control.WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"ReleaseGroup+GG_Desc",
					"FH_AllowTaskAutoAssignment",
					"FH_IsCriticalHandover",
					"FH_IsStandby",
					"FH_DateAcceptability",
					"FH_TimeDelayFactor",
					"FH_TimeDelayMinutes",
					"EffectiveNudge",
					"ProcessHeaderType",
					"Sequence",
					"FH_CompletionStatement",
					"FH_GG_ReleaseGroup",
					"FH_Category",
					"CategoryDescription",
				}, visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"FH_AgreedDeliveryDateDefaultsFrom",
					"FH_AgreedDeliveryDateDefaultHoursOffset",
					"FH_EarliestStartDateDefaultsFrom",
					"FH_EarliestStartDefaultHoursOffset",
					"FH_MilestoneCompletionPivotKey",
				}, invisibleColumns);
			}
		}

		public void TestDefaultAndInvisibleColumns_WhenNewReleaseGateRegistryEnabled()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var control = new ProcessTaskTemplateWorkflowControl())
			{
				var allColumns = control.WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"ReleaseGroup+GG_Desc",
					"FH_AllowTaskAutoAssignment",
					"FH_IsCriticalHandover",
					"FH_IsStandby",
					"FH_DateAcceptability",
					"FH_TimeDelayFactor",
					"FH_TimeDelayMinutes",
					"EffectiveNudge",
					"ProcessHeaderType",
					"Sequence",
					"FH_CompletionStatement",
					"FH_GG_ReleaseGroup",
					"FH_Category",
					"CategoryDescription",
					"FH_IsApproved",
					"FH_GB_Branch",
					"FH_GE_Department",
					"FH_DeadlineType",
					"FH_BMT_BufferTimespan",
				}, visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"FH_AgreedDeliveryDateDefaultsFrom",
					"FH_AgreedDeliveryDateDefaultHoursOffset",
					"FH_EarliestStartDateDefaultsFrom",
					"FH_EarliestStartDefaultHoursOffset",
					"FH_MilestoneCompletionPivotKey",
					"FH_EffectiveNudge",
				}, invisibleColumns);
			}
		}

		#endregion
	}
}
