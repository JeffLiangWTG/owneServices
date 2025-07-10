using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class ScheduleTaskForm : ZTemplateForm
	{
		public ScheduleTaskForm(ReportScheduleTask scheduleTask)
			: base(scheduleTask)
		{
			InitializeComponent();
			this.UserFindBox.ReadOnly = !Env.Instance.Security.ScheduleOtherStaffAsPrintUser.IsAllowed;

			if (scheduleTask.S5_IsPrivate)
			{
				RecurrenceControl.Visible = false;
				ControlDpiScalingHelper.SetHeight(ref TopPanel, TopPanel.Height - RecurrenceControl.Height, false);
				ChangeReportFilters.CaptionResourceString = Res.GetData("ScheduleTaskForm|03909127-485e-48c7-b95a-5853fee1c350", "View Report Filters");
			}

			var addressOverrideColumnStyleInfo = new AddressOverrideColumnStyleInfo<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>
			{
				CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("C330E08D-A504-4a19-9B06-7F828F1B6346", "Address Override", "Fax numbers or Email To addresses."),
				GetCopyRecipients = GetEmailToRecipients,
				ColumnName = "ToFaxOrEmail",
				EmailAddressPropertyName = StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress.Name,
				FieldTypeColumnName = "DeliveryMethodFieldType",
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			RecipientsGrid.ColumnStyles.Add(addressOverrideColumnStyleInfo);

			var carbonCopyRecipientsColumnStyleInfo = new CopyRecipientsColumnStyleInfo<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>
			{
				CaptionResourceString = Res.GetData("ScheduleTaskForm|0595C7DD-1656-4983-9024-36D437890097", "Email CC"),
				ColumnName = "S6_CarbonCopyRecipientsAsString",
				EmailAddressPropertyName = StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress.Name,
				GetCopyRecipients = GetCarbonCopyRecipients,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			RecipientsGrid.ColumnStyles.Add(carbonCopyRecipientsColumnStyleInfo);

			var blindCarbonCopyRecipientsColumnStyleInfo = new CopyRecipientsColumnStyleInfo<StmScheduleTaskCopyRecipient, StmScheduleTaskCopyRecipientCollection, StmScheduleTaskRecipient>
			{
				CaptionResourceString = Res.GetData("ScheduleTaskForm|2997BFCF-6F02-4E1B-8BE1-729FC6206E47", "Email BCC"),
				ColumnName = "S6_BlindCarbonCopyRecipientsAsString",
				EmailAddressPropertyName = StmScheduleTaskCopyRecipientSchema.SCR_EmailAddress.Name,
				GetCopyRecipients = GetBlindCarbonCopyRecipients,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			RecipientsGrid.ColumnStyles.Add(blindCarbonCopyRecipientsColumnStyleInfo);

			BusinessEntity.Validation.ValidateUserFK();
			foreach (var recipient in BusinessEntity.Recipients.Cast<ReportScheduleTaskRecipient>())
			{
				recipient.Validation.ValidateS6_GS_NKRecipient();
			}
		}

		public new ReportScheduleTask BusinessEntity
		{
			get { return (ReportScheduleTask)base.BusinessEntity; }
		}

		#region CopyRecipients

		StmScheduleTaskCopyRecipientCollection GetEmailToRecipients(StmScheduleTaskRecipient scheduleTaskRecipient)
			=> scheduleTaskRecipient?.EmailToRecipients;

		StmScheduleTaskCopyRecipientCollection GetCarbonCopyRecipients(StmScheduleTaskRecipient scheduleTaskRecipient)
		{
			StmScheduleTaskCopyRecipientCollection result = null;
			if (scheduleTaskRecipient != null)
			{
				result = scheduleTaskRecipient.CarbonCopyRecipients;
			}
			return result;
		}

		StmScheduleTaskCopyRecipientCollection GetBlindCarbonCopyRecipients(StmScheduleTaskRecipient scheduleTaskRecipient)
		{
			StmScheduleTaskCopyRecipientCollection result = null;
			if (scheduleTaskRecipient != null)
			{
				result = scheduleTaskRecipient.BlindCarbonCopyRecipients;
			}
			return result;
		}

		#endregion

		#region Save

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (!BusinessEntity.S5_IsPrivate && BusinessEntity.S5_IsActive && result == ContinueWithSave.Yes)
			{
				if (ShowReportFilter() != DialogResult.OK)
				{
					result = ContinueWithSave.No;
				}
			}
			return result;
		}

		#endregion

		#region Report Filters

		SecurityCheckpoint CheckpointForEdit()
		{
			if (BusinessEntity.S5_SystemCreateUser == Env.CurrentUser.Initials || String.IsNullOrWhiteSpace(BusinessEntity.S5_SystemCreateUser))
			{ return Env.Security.ScheduledTaskEdit; }
			return Env.Security.ScheduledTaskEditOtherReport;
		}

#if DEBUG
		internal
#endif
		void ChangeReportFilters_Click(object sender, EventArgs e)
		{
			var checkpoint = CheckpointForEdit();
			if (checkpoint.IsAllowed)
			{
				if (!BusinessEntity.S5_ParentID.IsValid)
				{
					Globals.Message.Show(Res.GetString("dfd406ab-17f4-407f-acf6-5dc9451215e8", "You must first specify a report before you can change filters."));
				}
				else
				{
					ShowReportFilter();
				}
			}
			else
			{
				checkpoint.ShowError();
			}
		}

		DialogResult ShowReportFilter()
		{
			var command = BusinessEntity.Factory.Load<ReportCommand>(BusinessEntity.S5_ParentID);
			if (command != null)
			{
				var pack = new DocumentPack(command);
				using (var report = (Report)pack[0])
				{
					if (report == null)
					{
						Globals.Message.Show(Res.GetString("A13570D3-721D-4BF7-9362-000E341E359D", "The report does not contain any template. Please set the report again."));
						return DialogResult.None;
					}
					else
					{
						report.SetScheduleTask(BusinessEntity);
						var dialog = new RuntimeOptionsForm(report, BusinessEntity);
						if (BusinessEntity.S5_IsPrivate)
						{
							Globals.Message.Show(Res.GetString("4d2277ac-53af-4e50-9257-16a6f92ba355", "Report Filters for One Off Reports cannot be modified and will be displayed in a read only view."));
							dialog.DisplayMode = ZArchitecture.Core.ODisplayMode.ReadOnly;
						}
						return ZFormModaliser.ShowDialogAndDispose(dialog);
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("7a318ab4-5c92-4b89-a78c-5718f3965ba0", "There is a problem with the Report on this Scheduled Task. Please set the report again."));
				return DialogResult.None;
			}
		}

		#endregion

		protected override bool ShowAuditTab => true;
	}
}
