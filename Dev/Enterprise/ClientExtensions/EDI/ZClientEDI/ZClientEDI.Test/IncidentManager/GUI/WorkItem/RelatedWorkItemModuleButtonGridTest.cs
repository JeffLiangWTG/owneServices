using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	class RelatedWorkItemModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestAttach()
		{
			SupportIncident incident = new BusinessObjectFactory().NewWithValidTestData<SupportIncident>();
			incident.IM_Description = "Something";
			incident.Factory.Save();
			using (ZForm form = new ZForm(incident))
			using (RelatedWorkItemModuleButtonGrid grid = new RelatedWorkItemModuleButtonGrid())
			{
				grid.BindToGridList = "RelatedWorkItems";
				grid.BindToFindBoxList = "Lookups+WorkItems";
				ZTextBoxColumnStyleInfo column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Number";
				column.ColumnName = WorkItemSchema.Constants.WKI_WorkItemNumber;
				grid.ColumnStyles.Add(column);
				form.Controls.Add(grid);
				form.Show();
				grid.InternalAttachButton_Click(this, EventArgs.Empty);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(typeof(EmbeddedModulePopup), ZFormModaliser.ActiveForm.GetType());
			}
		}

		public void TestShowNewFormFromController()
		{
			SupportIncident incident = new BusinessObjectFactory().NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Description = "Something";
			incident.Factory.Save();
			using (ZForm form = new ZForm(incident))
			using (RelatedWorkItemModuleButtonGrid grid = new RelatedWorkItemModuleButtonGrid())
			{
				grid.BindToGridList = "RelatedWorkItems";
				grid.BindToFindBoxList = "Lookups+WorkItems";
				ZTextBoxColumnStyleInfo column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Number";
				column.ColumnName = WorkItemSchema.Constants.WKI_WorkItemNumber;
				grid.ColumnStyles.Add(column);
				form.Controls.Add(grid);
				form.Show();
				grid.FireNewButtonClick();
				AssertNotNull(grid.LastShownZForm);
				grid.LastShownZForm.Dispose();
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
				using (ZForm form = new ZForm(incident))
				using (RelatedWorkItemModuleButtonGrid grid = new RelatedWorkItemModuleButtonGrid())
				{
					grid.BindToGridList = "RelatedWorkItems";
					grid.BindToFindBoxList = "Lookups+WorkItems";
					ZTextBoxColumnStyleInfo column = new ZTextBoxColumnStyleInfo();
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

			using (ZForm form = new ZForm(incident))
			using (RelatedWorkItemModuleButtonGrid grid = new RelatedWorkItemModuleButtonGrid())
			{
				grid.BindToGridList = "RelatedWorkItems";
				grid.BindToFindBoxList = "Lookups+WorkItems";
				ZTextBoxColumnStyleInfo column = new ZTextBoxColumnStyleInfo();
				column.Caption = "Number";
				column.ColumnName = WorkItemSchema.Constants.WKI_WorkItemNumber;
				grid.ColumnStyles.Add(column);
				form.Controls.Add(grid);
				form.Show();
				grid.FireNewButtonClick();
				AssertEquals(typeof(NewWorkItemForm), grid.LastShownZForm.GetType());
				NewWorkItemForm workItemForm = (NewWorkItemForm)grid.LastShownZForm;
				AssertEquals("Something", workItemForm.DataSource.WKI_Summary);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				grid.LastShownZForm.Dispose();
			}
		}

		public void TestModuleID()
		{
			using (RelatedWorkItemModuleButtonGrid grid = new RelatedWorkItemModuleButtonGrid())
			{
				AssertEquals(ModuleIDs.WorkItem, grid.ModuleID);
			}
		}
	}
}
