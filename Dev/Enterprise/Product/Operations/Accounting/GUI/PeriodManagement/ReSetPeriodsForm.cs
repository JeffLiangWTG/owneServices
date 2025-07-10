using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DataPurge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ReSetPeriodsForm : ZChildForm
	{
		public ReSetPeriodsForm(PeriodManagementForm periodManagementUserControl)
			: base()
		{
			this.PeriodManagementUserControl = periodManagementUserControl;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Implementation

		protected NewYearPeriodSettings NewYearSettings;
		ZGroupBox ReSetPeriodsGroupBox;
		ZButton BackUpButton;
		ZLabel LogOutLabel;
		ZLabel reAggregateLabel;
		ZButton CreatePeriodsButton;
		ZLabel CreatePeriodsLabel;
		ZButton DeletePeriodsButton;
		ZLabel DeletePeriodsLabel;
		ZLabel BackUpLabel;
		ZButton reAggregateButton;
		ZButton updateTaxGLMovementRecordsButton;
		ZLabel updateTaxGLMovementRecordsLabel;
		ZButton updateGeneralLedgerRecordsButton;
		ZLabel updateGeneralLedgerRecordsLabel;
		ZPanel bottomPanel;
		ZButton CloseButton;
		ZLabel deletePeriodsFromSpecifiedDateLabel;
		ZButton deletePeriodsRangeButton;
		ZButton extendLastFinancialYearButton;
		ZLabel extendLastFinancialYearLabel;
		readonly System.ComponentModel.Container components;

		#region System stuff
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			NewYearSettings.RunPreSaveValidation();
			if (NewYearSettings.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("6303db41-93ff-47ee-b849-59c8c5bbc0b6", "Please fix errors before proceeding."));
				return;
			}
			else
			{
				bool result = true;

				if (NewYearSettings.EndDate.Year < ZDateTime.Now.Year)
				{
					result = (Globals.Message.Show(Res.GetString("36ad17c5-374c-4cd6-bd25-32c299820cc0", "You are currently in year 2006. However, the new periods will be created for {0} financial year with the given dates.\r\n\r\nPlease click Yes to continue.", NewYearSettings.EndDate.Year), Res.GetString("2b289fec-12cc-4d03-b554-4fd024ce9f1d", "New Periods"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes);
				}

				if (result)
				{
					this.DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		#endregion

		#endregion

		void BackUpButton_Click(object sender, EventArgs e)
		{
			using (var dbBkpForm = new DatabaseBackupForm())
			{
				ZFormModaliser.ShowDialogWithoutDispose(dbBkpForm);
			}
		}

		void DeletePeriodsButton_Click(object sender, EventArgs e)
		{
			try
			{
				DialogResult result = Globals.Message.Show(Res.GetString("9977dff5-5e37-439a-9ea5-0f8f777cde2a", "Are you sure you want to delete all existing periods in the company [{0}]?", GlbCompany.CurrentCompany.GC_Name), Res.GetString("b67b8e48-fa97-4b64-9caf-a76a5f93c773", "Delete Periods"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (result == DialogResult.Yes)
				{
					PeriodManagementUserControl.PeriodManagerBizO.DeleteAllPeriodsInCurrentCompany();
					Globals.Message.Show(Res.GetString("fb895f34-dd7e-487e-94df-07fb02992199", "All periods have been deleted"), Res.GetString("b67b8e48-fa97-4b64-9caf-a76a5f93c773", "Delete Periods"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		void CreatePeriodsButton_Click(object sender, EventArgs e)
		{
			PeriodManagementUserControl.PromptForNewPeriodsSetup(true);
		}

		void updateTaxGLMovementRecordsButton_Click(object sender, EventArgs e)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("41663420-6444-4607-A66C-574B48ED4D90", "Are you sure you want to update periods on Transactions in the company [{0}]?", GlbCompany.CurrentCompany.GC_Name), Res.GetString("377F2B00-5F01-41D2-B6D6-7CE1D09B254A", "Update Records"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				ObjectFactory.Get<ITaxProcessor>().RecalculatePeriodForAllGLMovementRecords(GlbCompany.CurrentCompany.PK);
			}
		}

		void updateGeneralLedgerRecordsButton_Click(object sender, EventArgs e)
		{
			using (var form = new UpdateGldPeriodsDateRangeForm(new UpdateGldPeriodsDateRangeSetting()))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void reAggreateButton_Click(object sender, EventArgs e)
		{
			PeriodManagementUserControl.PromptForGLReAggregate();
		}

		readonly PeriodManagementForm PeriodManagementUserControl;

		void DeletePeriodsRangeButton_Click(object sender, EventArgs e)
		{
			using (var form = new DeletePeriodsFromForm(new DeletePeriodsFromSetting(), PeriodManagementUserControl.PeriodManagerBizO))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void ExtendLastFinancialYearButton_Click(object sender, EventArgs e)
		{
			var extendLastFinancialYearSettings = new ExtendLastFinancialYearSettings();
			var lastPeriod = PeriodManagementUserControl.LastPeriod;
			if (lastPeriod != null)
			{
				extendLastFinancialYearSettings.FinancialYear = lastPeriod.AM_Year;
				extendLastFinancialYearSettings.StartDate = Calculator.GetFirstPeriodFromYear(lastPeriod.AM_Year).AM_StartDate;
				extendLastFinancialYearSettings.EndDate = lastPeriod.AM_EndDate;
				extendLastFinancialYearSettings.LastPeriodEndDate = lastPeriod.AM_EndDate;
				extendLastFinancialYearSettings.LastPeriod = lastPeriod.AM_Period;
			}
			using (var form = new ExtendLastFinancialYearForm(extendLastFinancialYearSettings, PeriodManagementUserControl.PeriodManagerBizO))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		AccountingPeriodCalculator calculator;
		AccountingPeriodCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					calculator = new AccountingPeriodCalculator(Factory);
				}
				return calculator;
			}
		}

		BusinessObjectFactory factory;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
	}
}

