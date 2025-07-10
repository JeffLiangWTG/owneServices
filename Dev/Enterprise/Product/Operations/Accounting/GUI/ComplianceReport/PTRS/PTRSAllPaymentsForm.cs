using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.PTRS
{
	public partial class PTRSAllPaymentsForm : ZChildForm
	{
		public PTRSAllPaymentsForm()
		{
		}

		public PTRSAllPaymentsForm(PtrsAllPaymentsReport reportData) : base(reportData)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);
			ReportData.HasChangesChanged += HandleButtonsAvailability;
		}

		#region Implementations

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected PtrsAllPaymentsReport ReportData => (PtrsAllPaymentsReport)BusinessEntity;

		protected override bool AllowNew => false;

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (!this.IsDesignMode())
			{
				if (ReportData.IsSubmitted)
				{
					MakeFormReadonly();
					ReportData.RefreshBinding();
				}
				else
				{
					HandleButtonsAvailability(ReportData, HasChangesChangedEventArgs.Create(true, this));
				}
			}
		}

		#endregion

		#region Event Handlers

		void SubmitButton_Click(object sender, EventArgs e)
		{
			if (ValidateReportData()
				&& DialogResult.OK == Globals.Message.ShowConfirmation(
					Res.GetString("0adc7fda-7d4b-4fe8-a0f9-c8d0ba17c47f", @"Please confirm that this is the latest version of data ready to be submitted as a part of the Payment Times Reportable Small Business report.
Once marked as Submitted this data cannot be changed"),
					Res.GetString("deead398-3da6-426d-8d5d-fe4ed56ff9e5", "Mark as submitted for PTRS"),
					Res.GetString("33845a38-4849-4a6b-82c1-1f13b44c56c8", "To continue, type: "), Res.GetString("7da28260-c52c-4c3c-bf02-80023be22ffc", "Submit"),
					MessageBoxIcon.Question))
			{
				ReportData.SubmitReport();
				ReportData.Factory.Save();
				Globals.Message.ShowInformation(Res.GetString("592a819c-146e-415f-86b3-7dfb3f8ea307", "Close and re-open form to see the submitted state."));
			}
		}

		bool ValidateReportData()
		{
			ReportData.RunPreSaveValidation();
			var result = !ReportData.HasErrors;
			if (!result)
			{
				Globals.Message.Show(Res.GetString("c59d145c-2178-493f-83e7-eaa832e5402a", "Please fix the validation errors first."));
			}

			return result;
		}

		void MakeFormReadonly()
		{
			ReportData.ReadOnly = true;
			ReportData.HasChanges = false;
			ReportData.HasChangesChanged -= HandleButtonsAvailability;
			markAsSubmittedButton.Enabled = false;
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = ContinueWithSave.Yes;

			var confirmationResult = Globals.Message.ShowConfirmation(Res.GetString("58649b45-8589-4b64-85fd-d31228bd948a", @"Saving this form will affect the AK column value in the Payment Times Reportable Small Business report.
Please, open and save that report to apply the changes you are saving now."),
				Res.GetString("624151aa-7255-4c29-86bb-4e7ca8163eaa", "Saving Confirmation"),
				Res.GetString("33845a38-4849-4a6b-82c1-1f13b44c56c8", "To continue, type: "), Res.GetString("990a6ef0-9894-49b8-97a5-e417e2b1346e", "Yes"),
				MessageBoxIcon.Question);
			if (confirmationResult != DialogResult.OK)
			{
				result = ContinueWithSave.No;
			}

			if (result == ContinueWithSave.Yes)
			{
				result = base.ShowPreSaveDialogs();
			}

			return result;
		}

		#endregion

		void HandleButtonsAvailability(object sender, HasChangesChangedEventArgs e)
		{
			if (!IsDisposing)
			{
				markAsSubmittedButton.Enabled = ReportData != null && !ReportData.IsSubmitted && ReportData.IsSaved;
			}
		}

#if DEBUG
		public ZButton MarkAsSubmittedButton_ForTestOnly => markAsSubmittedButton;
#endif
	}
}
