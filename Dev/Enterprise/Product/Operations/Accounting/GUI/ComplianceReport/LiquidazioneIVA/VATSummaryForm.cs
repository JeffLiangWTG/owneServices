using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.LiquidazioneIVA
{
	public partial class VATSummaryForm : ZChildForm
	{
		public VATSummaryForm()
		{
		}

		public VATSummaryForm(LIQSubmissionDataColumns submissionData) : base(submissionData)
		{
			submissionData.UpdateValuesToSubmit();
			var totalBalance = submissionData.ValuesToSubmit.Box8_TotalBalance;

			InitializeSubmissionDataWithPreviousReport(submissionData);

			submissionData.PopulateDataFromReport();

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);

			SubmissionData.HasChanges = false;
			if (submissionData.ValuesToSubmit.Box8_TotalBalance != totalBalance)
			{
				Globals.Message.Show(Res.GetString("E0C25739-6419-4F84-9EC1-FC4EA47B4536", "The data in the VAT summary report has changed. You must save before exiting."));
				SubmissionData.HasChanges = true;
			}
		}

		#region Implementations

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			SubmissionData.HasChangesChanged += HandleSubmitButtonAvailability;
			SubmissionData.HasChanges = false;
		}

		protected LIQSubmissionDataColumns SubmissionData => BusinessEntity as LIQSubmissionDataColumns;

		public override string FormCaption =>
			Res.GetString("77D1CD6E-019A-48C5-8527-27B3AE3B19EC", "VAT Summary Report");

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
			}
			SubmissionData.HasChanges = false;
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
			SubmissionData.RunPreSaveValidation();
		}

		void MakeFormReadonly()
		{
			SubmissionData.ReadOnly = true;
			SubmissionData.HasChanges = false;
			SubmissionData.HasChangesChanged -= HandleSubmitButtonAvailability;
		}

		#endregion

		#region Adjustments Buttons

		void AdjustmentsButton1_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new VATAdjustmentReasonForm(new LIQSubmissionDataRow(SubmissionData.Factory, RowType.TotalVatBaseReceivables, column1Text.Text, SubmissionData)));
		}

		void AdjustmentsButton2_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new VATAdjustmentReasonForm(new LIQSubmissionDataRow(SubmissionData.Factory, RowType.TotalVatReceivables, column2Text.Text, SubmissionData)));
		}

		void AdjustmentsButton3_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new VATAdjustmentReasonForm(new LIQSubmissionDataRow(SubmissionData.Factory, RowType.TotalVatBasePayables, column3Text.Text, SubmissionData)));
		}

		void AdjustmentsButton4_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new VATAdjustmentReasonForm(new LIQSubmissionDataRow(SubmissionData.Factory, RowType.TotalVatPayablesRecoverable, column4Text.Text, SubmissionData)));
		}

		void AdjustmentsButton5_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new VATAdjustmentReasonForm(new LIQSubmissionDataRow(SubmissionData.Factory, RowType.TotalVatPayablesNotRecoverable, column5Text.Text, SubmissionData)));
		}

		void AdjustmentsButton7_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new VATAdjustmentReasonForm(new LIQSubmissionDataRow(SubmissionData.Factory, RowType.BalancePreviousPeriod, column7Text.Text, SubmissionData)));
		}

		#endregion

		void HandleSubmitButtonAvailability(object sender, HasChangesChangedEventArgs e)
		{
		}

		void InitializeSubmissionDataWithPreviousReport(LIQSubmissionDataColumns submissionData)
		{
			var helper = new LIQSubmissionDataHelper(submissionData.ComplianceReport);

			bool hasPreviousPeriod;
			(hasPreviousPeriod, submissionData.MinPageFromAR, submissionData.MinPageFromAP, submissionData.MinPageFromLiquidazione) = helper.SetPreviousPeriodData(submissionData.ComputedByCW1);

			if (!hasPreviousPeriod)
			{
				Globals.Message.Show(Res.GetString("D1439447-1CA6-4DBF-81D3-E2EBDF7FE4DD", "No Liquidazione VAT Summary Report found for previous period. If this is your first Liquidazione IVA, you may enter a manual Adjustment. Otherwise, please ensure a 'LIQ' Compliance Report with VAT Summary exists in Generated or Finalized status."));
			}
		}

#if DEBUG

		public void Call_HandleSubmitButtonAvailability_ForTestOnly() => HandleSubmitButtonAvailability(null, HasChangesChangedEventArgs.Create(false, this));

		public bool IsDeclarationAccepted_ForTestOnly { get; set; }

		public string SuccessfulReturnMessage { get; set; }

		public string ErrorMessage { get; set; }

#endif
	}
}
