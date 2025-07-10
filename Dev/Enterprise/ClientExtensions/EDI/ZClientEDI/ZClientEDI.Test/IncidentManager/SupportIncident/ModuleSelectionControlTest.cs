using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	public class ModuleSelectionControlTest : TestCaseWithFactory
	{
		public void TestAttach()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Description = "Something";
			incident.Factory.Save();

			using (var form = new ZForm(incident))
			using (var grid = new ModuleSelectionControlForTest())
			{
				grid.BindToGridList = "RelatedWorkItems";
				grid.BindToFindBoxList = "Lookups+WorkItems";

				var column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Number";
				column.ColumnName = WorkItemSchema.Constants.WKI_WorkItemNumber;
				grid.ColumnStyles.Add(column);

				form.Controls.Add(grid);
				form.Show();

				grid.AttachButtonClick_Exposed(this, EventArgs.Empty);
				AssertEquals(grid.YouCannotCreateWorkItem_Exposed, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.Factory.Save();
			using (var form = new ZForm(incident))
			using (var grid = new ModuleSelectionControlForTest())
			{
				grid.BindToGridList = "RelatedWorkItems";
				grid.BindToFindBoxList = "Lookups+WorkItems";

				var column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Number";
				column.ColumnName = WorkItemSchema.Constants.WKI_WorkItemNumber;
				grid.ColumnStyles.Add(column);

				form.Controls.Add(grid);
				form.Show();

				grid.AttachButtonClick_Exposed(this, EventArgs.Empty);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				var formShown = ZFormModaliser.LastFormShownForTest;
				AssertEquals(typeof(EmbeddedModulePopup), formShown.GetType());
				formShown.Dispose();
			}
		}

		public void TestAttach_RetrospectivelyBroadcastEConversations()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Description = "Something";
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.Factory.Save();

			using (var form = new ZForm(incident))
			using (var grid = new ModuleSelectionControlForTest())
			{
				grid.BindToGridList = "RelatedWorkItems";
				grid.BindToFindBoxList = "Lookups+WorkItems";

				var column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Number";
				column.ColumnName = WorkItemSchema.Constants.WKI_WorkItemNumber;
				grid.ColumnStyles.Add(column);

				form.Controls.Add(grid);
				form.Show();

				grid.AttachButtonClick_Exposed(this, EventArgs.Empty);

				// HERE should call the new broadcast form
				var formShown = ZFormModaliser.LastFormShownForTest;
				AssertEquals(typeof(EmbeddedModulePopup), formShown.GetType());
				formShown.Dispose();
			}
		}

		public void TestShowNewFormFromController()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Something";
			incident.Factory.Save();

			using (var form = new ZForm(incident))
			using (var grid = new ModuleSelectionControlForTest())
			{
				grid.BindToGridList = "RelatedWorkItems";
				grid.BindToFindBoxList = "Lookups+WorkItems";

				var column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Number";
				column.ColumnName = WorkItemSchema.Constants.WKI_WorkItemNumber;
				grid.ColumnStyles.Add(column);

				form.Controls.Add(grid);
				form.Show();

				grid.FireNewButtonClick();
				AssertNull(grid.LastShownZForm);
				AssertEquals(grid.YouCannotCreateWorkItem_Exposed, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.Factory.Save();

			bool originalWorkItemSecurity = Env.Security.WorkItem.IsAllowed;
			bool originalWorkItemNewSecurity = Env.Security.WorkItemNew.IsAllowed;

			try
			{
				Env.Security.WorkItem.IsAllowed = false;
				Env.Security.WorkItemNew.IsAllowed = false;

				using (var form = new ZForm(incident))
				using (var grid = new ModuleSelectionControl())
				{
					grid.BindToGridList = "RelatedWorkItems";
					grid.BindToFindBoxList = "Lookups+WorkItems";

					var column = new ZTextBoxColumnStyleInfo();
					column.Caption = "Number";
					column.ColumnName = WorkItemSchema.Constants.WKI_WorkItemNumber;
					grid.ColumnStyles.Add(column);

					form.Controls.Add(grid);
					form.Show();

					AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
					{
						grid.FireNewButtonClick();
						AssertEquals(Env.Security.WorkItemNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					});
					AssertNull(grid.LastShownZForm);
				}
			}
			finally
			{
				Env.Security.WorkItem.IsAllowed = originalWorkItemSecurity;
				Env.Security.WorkItemNew.IsAllowed = originalWorkItemNewSecurity;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestModuleID()
		{
			using (ModuleSelectionControl grid = new ModuleSelectionControl())
			{
				AssertEquals(ModuleIDs.WorkItem, grid.ModuleID);
			}
		}

		public class ModuleSelectionControlForTest : ModuleSelectionControl
		{
			public string YouCannotCreateWorkItem_Exposed => YouCannotCreateWorkItem;

			public void AttachButtonClick_Exposed(object sender, EventArgs e)
			{
				base.AttachButton_Click(sender, e);
			}
		}

		[TestedType(typeof(ModuleSelectionControl))]
		class ModuleSelectionControlGridModuleButtonGridTest : ZModuleButtonGridTestBase
		{
		}
	}
}
