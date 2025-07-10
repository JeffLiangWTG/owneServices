using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ComplianceReport.PTRS;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.PTRS
{
	public partial class PTRSForm : ZChildForm
	{
		public PTRSForm()
		{
		}

		public PTRSForm(PtrsReport reportData) : base(reportData)
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

		protected PtrsReport ReportData => (PtrsReport)BusinessEntity;

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

		void GenerateButton_Click(object sender, EventArgs e)
		{
			if (ReportData.HasChanges || !(ReportData.IsSaved || ReportData.IsGenerated))
			{
				Globals.Message.Show(Res.GetString("7e01e434-a6c3-4a3a-86e0-fd586f646c84", "Please save this form before generating file."),
					Res.GetString("d1f74967-656b-4f64-ac70-0612d6e00a4e", "Generate PTRS file"),
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (ValidateReportData()
				&& DialogResult.OK == Globals.Message.ShowConfirmation(
					Res.GetString("6ac0ede3-91be-4229-8fef-106bdf37318a", "You are about to generate the output file for PTRS. which should be submitted to Payment Times Reporting Portal.\r\nThe file will be added to the Compliance Report eDocs."),
					Res.GetString("d1f74967-656b-4f64-ac70-0612d6e00a4e", "Generate PTRS file"),
					Res.GetString("d145b12c-70fa-4d7d-8c2e-588e08e6b190", "Yes"),
					MessageBoxIcon.Question))
			{
				var generatedFileName = ReportData.GenerateReport();
				ReportData.Factory.Save();

				if (string.IsNullOrEmpty(generatedFileName))
				{
					Globals.Message.ShowError(Res.GetString("b0f6a67b-ed5b-41df-b249-0af3eccbce3e", "The output file for PTRS was not generated."));
				}
				else
				{
					Globals.Message.Show(Res.GetString("66961b29-4e95-48da-8bc4-a3eb994ebe69", "The output file for PTRS was successfully generated.\r\nThe file has been added to the Compliance Report eDocs with the file name: {0}", generatedFileName));
				}
			}
		}

		void SubmitButton_Click(object sender, EventArgs e)
		{
			if (ValidateReportData()
				&& DialogResult.OK == Globals.Message.ShowConfirmation(
					Res.GetString("092d21aa-7653-492f-8968-98d6821b5ae1", "Please confirm that the latest generated PTRS file: {0} has been submitted to Payment Times Reporting Portal.", ReportData.LastGeneratedFileName),
					Res.GetString("93765e86-889a-4313-875d-6a67bed51233", "Mark as submitted"),
					Res.GetString("7da28260-c52c-4c3c-bf02-80023be22ffc", "Submit"),
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
			generateFileButton.Enabled = false;
			markAsSubmittedButton.Enabled = false;
		}

		bool ShouldDisplayWarningOnSave => ReportData.IsGenerated;

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = ContinueWithSave.Yes;

			if (ShouldDisplayWarningOnSave)
			{
				var confirmationResult = Globals.Message.ShowConfirmation(Res.GetString("5aae98bd-1fa8-4c8c-9828-6fa61e9a9c24", @"Saving this form will make the latest generated PTRS file not valid for submitting to Payment Times Reporting Portal.
A new file must be generated."),
					Res.GetString("624151aa-7255-4c29-86bb-4e7ca8163eaa", "Saving Confirmation"),
					Res.GetString("33845a38-4849-4a6b-82c1-1f13b44c56c8", "To continue, type: "), Res.GetString("990a6ef0-9894-49b8-97a5-e417e2b1346e", "Yes"),
					MessageBoxIcon.Question);
				if (confirmationResult != DialogResult.OK)
				{
					result = ContinueWithSave.No;
				}
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
				generateFileButton.Enabled = ReportData != null && !ReportData.IsSubmitted;
				markAsSubmittedButton.Enabled = ReportData != null && !ReportData.IsSubmitted && ReportData.IsGenerated;
			}
		}

#if DEBUG
		public ZButton GenerateFileButton_ForTestOnly => generateFileButton;
		public ZButton MarkAsSubmittedButton_ForTestOnly => markAsSubmittedButton;
#endif
	}
}
