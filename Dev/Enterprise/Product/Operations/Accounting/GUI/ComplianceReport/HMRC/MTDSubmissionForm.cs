using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC
{
	public partial class MTDSubmissionForm : ZChildForm
	{
		public MTDSubmissionForm()
		{
		}

		public MTDSubmissionForm(MTDSubmissionDataColumns submissionData) : base(submissionData)
		{
			submissionData.PopulateDataFromReport();
			TooOldWarningOnce = true;
			OverThresholdWarningOnce = true;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);
		}

		#region Implementations

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SubmitButton.Enabled = SubmissionData.CanBeSubmittedToHMRC;
			SubmissionData.HasChangesChanged += HandleSubmitButtonAvailability;
			returnDueDateText.Text = SubmissionData.IsSubmitted
				? Res.GetString("f68ad2c5-15c0-4981-a1b4-82e292f386f5", "Return Received Date:")
				: Res.GetString("06b3e2d1-00b7-4331-834a-90dc8b10dc91", "Return Due Date:");

			if (SubmissionData.IsGroupMemberSubmission)
			{
				SubmitButton.Text = Res.GetString("7c9820fc-76bc-43cb-bbb3-f5f972284d1e", "Submit VAT Return to Group");
			}
		}

		protected MTDSubmissionDataColumns SubmissionData => (MTDSubmissionDataColumns)BusinessEntity;

		public override string FormCaption =>
			Res.GetString("267564de-8dd2-4168-9aab-539d574e0a42", "UK VAT Return Submission - {0}", !AccountingConfigurationRegistry.Instance.IsMTDProductionMode.Value ? Res.GetString("a24f0aee-f48e-4d3e-9143-d74667f1f4fc", "Test Environment") : string.Empty);

		protected override bool AllowNew => false;

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (!this.IsDesignMode())
			{
				if (SubmissionData.IsSubmitted)
				{
					MakeFormReadonly();
					SubmissionData.RefreshBinding();
				}
				else
				{
					if (SubmissionData.TooOldWarning && TooOldWarningOnce)
					{
						Globals.Message.ShowWarning(MTDSubmissionDataHelper.OldTxnMsg);
						TooOldWarningOnce = false;
					}
					if (SubmissionData.OverThresholdWarning && OverThresholdWarningOnce)
					{
						Globals.Message.ShowWarning(MTDSubmissionDataColumns.DidNotIncludeChangesInCurrentVATReturnMessage);
						OverThresholdWarningOnce = false;
					}
				}
			}
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			base.Save(factories);
			SubmissionData.HasChanges = false;
		}

		#endregion

		#region Event Handlers

		void SubmitButton_Click(object sender, EventArgs e)
		{
			if (SubmissionData.IsGroupMemberSubmission || IsDeclarationAccepted())
			{
				var cursor = Cursor.Current;
				var errorMessage = string.Empty;
				var successMessage = string.Empty;

				try
				{
					Cursor.Current = Cursors.WaitCursor;

					if (SubmissionData.IsGroupMemberSubmission)
					{
						(successMessage, errorMessage) = SubmissionData.SubmitVATDataToGroup();
					}
					else
					{
						var response = SubmitReturnToHMRC();
						successMessage = response.ResponseFromHMRC;
						errorMessage = response.ErrorMessage;
					}
				}
				finally
				{
					Cursor.Current = cursor;
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.ShowInformation(successMessage);
					MakeFormReadonly();
					SubmissionData.RefreshBinding();
				}
				else
				{
					Globals.Message.ShowError(errorMessage);
				}
			}
		}

		void MakeFormReadonly()
		{
			SubmissionData.ReadOnly = true;
			SubmissionData.HasChanges = false;
			SubmissionData.HasChangesChanged -= HandleSubmitButtonAvailability;
			SubmitButton.Enabled = false;
		}

		bool IsDeclarationAccepted()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				return IsDeclarationAccepted_TestOnly;
			}
#endif
			var declarationForm = new MTDDeclarationForm(SubmissionData);
			SubmissionData.Declaration = SubmissionData.IsSubmitted;
			return ZFormModaliser.ShowDialogAndDispose(declarationForm) == DialogResult.OK;
		}

		(string ResponseFromHMRC, string ErrorMessage) SubmitReturnToHMRC()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				return MockVATReturnToHMRC();
			}
#endif
			return SubmissionData.TryToSubmitVATDataToHMRC();
		}

		#region Adjustments Buttons
		void AdjustmentsButton1_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdjustmentReasonForm(new MTDSubmissionDataRow(SubmissionData.Factory, 1, column1Text.Text, SubmissionData)));
		}

		void AdjustmentsButton2_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdjustmentReasonForm(new MTDSubmissionDataRow(SubmissionData.Factory, 2, column2Text.Text, SubmissionData)));
		}

		void AdjustmentsButton4_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdjustmentReasonForm(new MTDSubmissionDataRow(SubmissionData.Factory, 4, column4Text.Text, SubmissionData)));
		}

		void AdjustmentsButton6_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdjustmentReasonForm(new MTDSubmissionDataRow(SubmissionData.Factory, 6, column6Text.Text, SubmissionData)));
		}

		void AdjustmentsButton7_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdjustmentReasonForm(new MTDSubmissionDataRow(SubmissionData.Factory, 7, column7Text.Text, SubmissionData)));
		}

		void AdjustmentsButton8_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdjustmentReasonForm(new MTDSubmissionDataRow(SubmissionData.Factory, 8, column8Text.Text, SubmissionData)));
		}

		void AdjustmentsButton9_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new AdjustmentReasonForm(new MTDSubmissionDataRow(SubmissionData.Factory, 9, column9Text.Text, SubmissionData)));
		}

		#endregion

		#endregion

		void HandleSubmitButtonAvailability(object sender, HasChangesChangedEventArgs e)
		{
			string collectErrorReportData()
			{
				var stringBuilder = new ZStringBuilder();
				var baseOrgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var orgProxyCompany = baseOrgProxy?.NameAndCode ?? string.Empty;
				var localVATRegistrationCompany = baseOrgProxy?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("VAT", Core.Constants.CountryCodes.UnitedKingdom)?.OK_CustomsRegNo ?? string.Empty; // Developer Report Message

				stringBuilder.AppendLine($"ORGProxy Name & Code: {orgProxyCompany}"); // Developer Report Message
				stringBuilder.AppendLine($"VAT Registration Company: {localVATRegistrationCompany}"); // Developer Report Message

				var complianceReport = SubmissionData?.ComplianceReport;
				if (complianceReport != null)
				{
					stringBuilder.AppendLine(FormattableString.Invariant($@"Compliance Report Type: {complianceReport.ACR_ReportType}
Compliance Report Date From: {complianceReport.ACR_DateFrom}
Compliance Report Date To: {complianceReport.ACR_DateTo}
Compliance Report Status: {complianceReport.ACR_Status}")); // Developer Report Message
				}

				return stringBuilder.ToString();
			}

			if (SubmitButton == null)
			{
				ErrorReporter.ReportOnce("MTD HandleSubmitButtonAvailability - SubmitButton is Null.", collectErrorReportData());
			}
			else if (SubmissionData == null)
			{
				SubmitButton.Enabled = false;
				ErrorReporter.ReportOnce("MTD HandleSubmitButtonAvailability - SubmissionData Object is Null.", collectErrorReportData());
			}
			else
			{
				SubmitButton.Enabled = SubmissionData.CanBeSubmittedToHMRC;
			}
		}

		bool TooOldWarningOnce;
		bool OverThresholdWarningOnce;

#if DEBUG
		public ZButton SubmitButton_ForTestOnly
		{
			get { return SubmitButton; }
			set { SubmitButton = value; }
		}

		public void Call_HandleSubmitButtonAvailability_ForTestOnly() => HandleSubmitButtonAvailability(null, HasChangesChangedEventArgs.Create(false, this));

		public bool IsDeclarationAccepted_TestOnly { get; set; }

		public string SuccessfulReturnMessage { get; set; }

		public string ErrorMessage { get; set; }

		public void ClickSubmitButton()
		{
			SubmitButton.Enabled = true;
			SubmitButton.PerformClick();
			SubmitButton.Enabled = false;
		}

		public (string ResponseFromHMRC, string ErrorMessage) MockVATReturnToHMRC() => (SuccessfulReturnMessage, ErrorMessage);
#endif
	}
}
