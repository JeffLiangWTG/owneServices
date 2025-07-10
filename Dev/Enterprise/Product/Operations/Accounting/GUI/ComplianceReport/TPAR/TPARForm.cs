using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ComplianceReport.TPAR;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.TPAR
{
	public partial class TPARForm : ZChildForm
	{
		public TPARForm()
		{
		}

		public TPARForm(TparReport reportData) : base(reportData)
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

		protected TparReport ReportData => (TparReport)BusinessEntity;

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
				Globals.Message.Show(Res.GetString("5abbe887-0fed-42c0-a6e0-47fbcec5142a", "Please save TPAR Summary before generating file."),
					Res.GetString("91de33fa-a9e0-4a6f-a33f-380c500ff2a8", "Generate TPAR file"),
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (ValidateReportData()
				&& DialogResult.OK == Globals.Message.ShowConfirmation(
					Res.GetString("a0fbb376-0318-4b5a-8090-1d6b2ab11aa6", "You are about to generate an output file for TPAR, which should be submitted to ATO.\r\nThe file will be added to the Compliance Report eDocs."),
					Res.GetString("91de33fa-a9e0-4a6f-a33f-380c500ff2a8", "Generate TPAR file"),
					Res.GetString("12892867-2aaf-4ef8-bfa3-66f919b9a9a9", "Yes"),
					MessageBoxIcon.Question))
			{
				var generatedFileName = ReportData.GenerateReport();
				ReportData.Factory.Save();

				if (string.IsNullOrEmpty(generatedFileName))
				{
					Globals.Message.ShowError(Res.GetString("a800e9a6-ccdf-4a8c-95d0-a7a11b285c7c", "The output file for TPAR was not generated."));
				}
				else
				{
					Globals.Message.Show(Res.GetString("4c1d07ba-0183-499f-acf6-8d727d12bfa8", "The output file for TPAR was generated with success.\r\nThe file has been added to the Compliance Report eDocs with the file name: {0}", generatedFileName));
				}
			}
		}

		void SubmitButton_Click(object sender, EventArgs e)
		{
			if (ValidateReportData()
				&& DialogResult.OK == Globals.Message.ShowConfirmation(
					Res.GetString("e0f79396-1808-4d8b-88ba-88a95d2a54d6", "Please confirm that the latest generated TPAR file: {0} has been submitted to ATO.", ReportData.LastGeneratedFileName),
					Res.GetString("fb09efc8-3313-4bef-b59f-39bfaa50c757", "Mark as submitted to ATO"),
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
				var confirmationResult = Globals.Message.ShowConfirmation(Res.GetString("47d1b7ae-7f2b-40ea-abe2-36888134cae0", @"Saving this form will make the latest generated TPAR file not valid for submitting to ATO.
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
