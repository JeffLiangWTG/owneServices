using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.Statements
{
	public partial class StatementPrintForm : ZChildForm
	{
		protected ZButton PrintButton;
		protected ZButton FormCancelButton;
		ZGuidFindBox CurrencyGuidFindBox;
		ZGuidFindBox OrganisationGuidFindBox;
		ZGuidFindBox SalesRepFindBox;
		ZGuidFindBox zGuidFindBox3;
		ZGuidFindBox zGuidFindBox4;
		ZDropEdit zDropEdit5;
		ZDropEdit TypeOfDocumentToPrintDropEdit;
		ZDropEdit CreditRatingDropEdit;
		ZDropEdit AccountsRelationshipDropEdit;
		ZDropEdit ConsolidationCategoryDropEdit;
		ZGuidFindBox DebtorGroupFindBox;
		ZGuidFindBox OrganisationBranchFindBox;
		ZCheckBox DisbursementInvoicesCheckBox;
		ZArchitecture.ZCalcEdit OutstandingAmountCalcEdit;
		ZDateEdit CutoffDateEdit;
		ZGroupBox OrganisationGroupBox;
		protected ZGroupBox TransactionGroupBox;
		ZGroupBox DocumentGroupBox;
		ZPeriodEdit CutoffPeriodEdit;
		ZDropEdit IssueStatementPackDropEdit;
		ZGuidFindBox TransactionBranchFindBox;
		ZCheckBox IssueByTransactionBranchCheckBox;
		ZCheckBox IssueStatementsBySettlementGroupCheckBox;
		ZCheckBox DepartmentCheckBox;
		ZCheckBox IncludeDebtorSummaryPageCheckBox;
		ZGuidFindBox DepartmentGuidFindBox;
		ZCheckBox IncludeTransactionsInActiveBatchCheckBox;
		ZGroupBox grpBxAccountMovement;
		ZDropEdit dedPrintAccMovementSOAGroupBy;
		ZDateEdit dtpPrintAccMovementSOATo;
		ZDateEdit dtpPrintAccMovementSOAFrom;
		ZCheckBox chkPrintAccMovementSOA;
		ZGroupBox zGroupBox1;
		ZArchitecture.ZTextBox txtInvoiceDescription;
		ZCheckBox chkFeePosting;
		ZDateEdit dtpAccFeePostDate;
		ZDateEdit dtpAccFeeInvoiceDate;
		ZDateEdit dtpAccFeeToDate;
		ZDateEdit dtpAccFeeFromDate;
		ZArchitecture.ZLabel accFeeTaxIDMessageLabel;
		ZGuidFindBox AL_AT_GSTTaxIDGuidFindBox;
		readonly System.ComponentModel.Container components;

		public StatementPrintForm()
		{
		}

		public StatementPrintForm(Statement businessObject)
			: base(businessObject)
		{
			businessObject.DocumentToPrintInfo.ValueChanged += TypeOfDocumentToPrintChanged;
			businessObject.DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;

			businessObject.IssueBySettlementGroupInfo.ValueChanged += SetUpOrganisationGroupBoxText;
			businessObject.IssueBySettlementGroup = false;

			businessObject.IssueByTransactionBranchInfo.ValueChanged += SetUpTransactionBranchStatus;
			businessObject.IssueByTransactionBranch = true; //it's for running event handler
			businessObject.IssueByTransactionBranch = false;
			businessObject.TransactionBranch_PKInfo.ValueChanged += TransactionBranchValueChanged;

			businessObject.IssueByTransactionDepartmentInfo.ValueChanged += SetUpTransactionDepartmentStatus;
			businessObject.IssueByTransactionDepartment = true; //it's for running event handler
			businessObject.IssueByTransactionDepartment = false;
			businessObject.TransactionDepartment_PKInfo.ValueChanged += TransactionDepartmentValueChanged;

			businessObject.NoDocumentsToPrint += BusinessObject_StatementEvent;
			SetUpControlsForStatement();
			CutoffDateEdit.GetExtension<LabelCaptionRenderer>().Options = StringRenderingOptions.Wrap;

			InitializePopupCaptions();

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			ZFormMenuStrategy.AddAdornments(this);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.ActionsMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.HelpMenuItemName, false);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ControlBasedVisibiltyProvider enProvider = new ControlBasedVisibiltyProvider(chkPrintAccMovementSOA, new Func<bool>(() => chkPrintAccMovementSOA.Checked), "CheckedChanged");
			dtpPrintAccMovementSOAFrom.SetEnabler(enProvider, null);
			dtpPrintAccMovementSOATo.SetEnabler(enProvider, null);
			dedPrintAccMovementSOAGroupBy.SetEnabler(enProvider, null);

			ControlBasedVisibiltyProvider denProvider = new ControlBasedVisibiltyProvider(chkPrintAccMovementSOA, new Func<bool>(() => !chkPrintAccMovementSOA.Checked), "CheckedChanged");
			CurrencyGuidFindBox.SetEnabler(denProvider, null);
			CutoffDateEdit.SetEnabler(denProvider, null);
			CutoffPeriodEdit.SetEnabler(denProvider, null);
			IncludeTransactionsInActiveBatchCheckBox.SetEnabler(denProvider, null);
			DisbursementInvoicesCheckBox.SetEnabler(denProvider, null);
			TransactionBranchFindBox.SetEnabler(denProvider, null);
			DepartmentGuidFindBox.SetEnabler(denProvider, null);
			OutstandingAmountCalcEdit.SetEnabler(denProvider, null);

			IncludeDebtorSummaryPageCheckBox.SetEnabler(denProvider, null);
			IssueByTransactionBranchCheckBox.SetEnabler(denProvider, null);
			IssueStatementsBySettlementGroupCheckBox.SetEnabler(denProvider, null);
			DepartmentCheckBox.SetEnabler(denProvider, null);

			ControlBasedVisibiltyProvider feeEnProvider = new ControlBasedVisibiltyProvider(chkFeePosting, new Func<bool>(() => chkFeePosting.Checked), "CheckedChanged");
			dtpAccFeeFromDate.SetEnabler(feeEnProvider, null);
			dtpAccFeeToDate.SetEnabler(feeEnProvider, null);
			dtpAccFeeInvoiceDate.SetEnabler(feeEnProvider, null);
			dtpAccFeePostDate.SetEnabler(feeEnProvider, null);
			txtInvoiceDescription.SetEnabler(feeEnProvider, null);
			AL_AT_GSTTaxIDGuidFindBox.SetEnabler(feeEnProvider, new Action(() => AL_AT_GSTTaxIDGuidFindBox.Enabled = (AL_AT_GSTTaxIDGuidFindBox.Enabled && !Statement.AccountFeeInvoiceCreator.AccFeeTaxID_ReadOnly)));

			TransactionBranchFindBox.Enabled = false;
			DepartmentGuidFindBox.Enabled = false;
			accFeeTaxIDMessageLabel.Visible = AL_AT_GSTTaxIDGuidFindBox.Visible = Statement.Company.GC_IsGSTRegistered;
		}

		void InitializePopupCaptions()
		{
			CurrencyGuidFindBox.PopupCaption = Res.GetString("Accounting|StatementPopupForm|PleaseSelectACurrency", "Please select a Currency");
			zGuidFindBox4.PopupCaption = Res.GetString("Accounting|StatementPopupForm|PleaseSelectASalesRep", "Please select a Sales Rep.");
			zGuidFindBox3.PopupCaption = Res.GetString("Accounting|StatementPopupForm|PleaseSelectASalesRep", "Please select a Sales Rep.");
			SalesRepFindBox.PopupCaption = Res.GetString("Accounting|StatementPopupForm|PleaseSelectASalesRep", "Please select a Sales Rep.");
			DebtorGroupFindBox.PopupCaption = Res.GetString("Accounting|StatementPopupForm|PleaseSelectADebtorGroup", "Please select a Debtor Group");
			OrganisationBranchFindBox.PopupCaption = Res.GetString("Accounting|StatementPopupForm|PleaseSelectABranch", "Please select a Branch");
			OrganisationGuidFindBox.PopupCaption = Res.GetString("Accounting|StatementPopupForm|PleaseSelectAnOrganisation", "Please select an Organization");
			TransactionBranchFindBox.PopupCaption = Res.GetString("Accounting|StatementPopupForm|PleaseSelectABranch", "Please select a Branch");
		}

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

				Statement.DocumentToPrintInfo.ValueChanged -= TypeOfDocumentToPrintChanged;
				Statement.IssueBySettlementGroupInfo.ValueChanged -= SetUpOrganisationGroupBoxText;
				Statement.IssueByTransactionBranchInfo.ValueChanged -= SetUpTransactionBranchStatus;
				Statement.TransactionBranch_PKInfo.ValueChanged -= TransactionBranchValueChanged;
				Statement.IssueByTransactionDepartmentInfo.ValueChanged -= SetUpTransactionDepartmentStatus;
				Statement.TransactionDepartment_PKInfo.ValueChanged -= TransactionDepartmentValueChanged;
				Statement.NoDocumentsToPrint -= BusinessObject_StatementEvent;
			}
			base.Dispose(disposing);
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			Statement.RunPreSaveValidation();

			if (Statement.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				try
				{
					Cursor.Current = Cursors.WaitCursor;

					if (Statement.DoAccountFeeTransaction)
					{
						using (var wrapper = new ProgressFormWrapper(this, new ResourceStringData("41c33ae7-cda6-43b0-b08b-32b47fc5c60f", (NoResString)"Account Fee Invoice"), false, true, Statement.AccountFeeInvoiceCreator)) // Resource string key is given
						{
							wrapper.ShowProgressForm();
							Statement.AccountFeeInvoiceCreator.CreateAndSaveAccountFeeInvoices();
						}
					}

					Statement.PrintStatements();
				}
				catch (PublishedARInvoiceDocumentNotFoundException)
				{
					Globals.Message.ShowError(Res.GetString("Accounting|StatementPrintForm|NoPublishedARInvoiceDocumentError", "There is no published AR Invoice document."));
				}
				catch (UnableToFindInvoiceDocumentCommandException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}
		}

		void FormCancelButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.HasChanges = false;
			Close();
		}

		void TypeOfDocumentToPrintChanged(object sender, EventArgs e)
		{
			switch (Statement.DocumentToPrint)
			{
				case Core.Constants.StatementCollectionLetterType.StatementOfAccount:
					SetUpControlsForStatement();
					break;

				case Core.Constants.StatementCollectionLetterType.FirstReminder:
				case Core.Constants.StatementCollectionLetterType.SecondReminder:
				case Core.Constants.StatementCollectionLetterType.CollectionLetter:
				case Core.Constants.StatementCollectionLetterType.DemandLetter:
					SetUpControlsForCollectionLetter();
					break;
			}
		}

		protected virtual void SetUpControlsForStatement()
		{
			CutoffDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("StatementPrintForm|BC67DA47-9125-4851-9F09-5097749AF85C", "Invoice Dates on or Before");
			OutstandingAmountCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("StatementPrintForm|C7DB59B0-76F9-4ee5-A02B-02055C7336A9", "Statements as at end of selected period");
			CutoffPeriodEdit.Visible = true;
			OutstandingAmountCalcEdit.Visible = false;
		}

		protected virtual void SetUpControlsForCollectionLetter()
		{
			CutoffDateEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("StatementPrintForm|22B1ADA3-52CB-41dd-BD17-D0F3619AEE2D", "Due Dates on or Before");
			OutstandingAmountCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("StatementPrintForm|541258BE-83D0-40e0-8352-2A325CD126AA", "Local Outstanding Amount Greater than/Equal to");
			OutstandingAmountCalcEdit.GetExtension<ILabelCaptionRenderer>().Options = StringRenderingOptions.Wrap;
			CutoffPeriodEdit.Visible = false;
			OutstandingAmountCalcEdit.Visible = true;
		}

		void SetUpOrganisationGroupBoxText(object sender, EventArgs e)
		{
			if (Statement.IssueBySettlementGroup)
			{
				OrganisationGroupBox.Text = Res.GetString("StatementPrintForm|916BB29C-39E2-4cfe-8CFD-1E4A98327E44", "Select which Settlement Group(s) to Print For");
				OrganisationGroupBox.Refresh();
				OrganisationGuidFindBox.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("StatementPrintForm|A4BB2C19-E341-472b-BC73-B7B841D87A9E", "Settlement Group");
				OrganisationGuidFindBox.Refresh();
			}
			else
			{
				OrganisationGroupBox.Text = Res.GetString("StatementPrintForm|34E3AA0C-F111-44f8-B09B-AABB869B2890", "Select which Debtors(s) to Print For");
				OrganisationGroupBox.Refresh();
				OrganisationGuidFindBox.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("StatementPrintForm|CF217009-0B1B-4024-8AD0-1A307E852556", "Debtor");
				OrganisationGuidFindBox.Refresh();
			}
		}

		void SetUpTransactionBranchStatus(object sender, EventArgs e)
		{
			TransactionBranchFindBox.Enabled = Statement.IssueByTransactionBranch;
			if (TransactionBranchFindBox.Enabled)
			{
				Statement.TransactionBranch_PK = OldTransactionBranchPK;
			}
			else
			{
				Statement.TransactionBranch_PK = ZGuid.Empty;
			}
		}

		ZGuid OldTransactionBranchPK = ZGuid.Empty;
		void TransactionBranchValueChanged(object sender, EventArgs e)
		{
			if (TransactionBranchFindBox.Enabled)
			{
				OldTransactionBranchPK = Statement.TransactionBranch_PK;
			}
			else
			{
				Statement.TransactionBranch_PK = ZGuid.Empty;
			}
		}

		void SetUpTransactionDepartmentStatus(object sender, EventArgs e)
		{
			DepartmentGuidFindBox.Enabled = Statement.IssueByTransactionDepartment;
			if (DepartmentGuidFindBox.Enabled)
			{
				Statement.TransactionDepartment_PK = OldTransactionDepartmentPK;
			}
			else
			{
				Statement.TransactionDepartment_PK = ZGuid.Empty;
			}
		}

		ZGuid OldTransactionDepartmentPK = ZGuid.Empty;

		void TransactionDepartmentValueChanged(object sender, EventArgs e)
		{
			if (DepartmentGuidFindBox.Enabled)
			{
				OldTransactionDepartmentPK = Statement.TransactionDepartment_PK;
			}
			else
			{
				Statement.TransactionDepartment_PK = ZGuid.Empty;
			}
		}

		public Statement Statement
		{
			get { return (Statement)BusinessEntity; }
		}
		void BusinessObject_StatementEvent(object sender, NoDocumentsToPrintEventArgs e)
		{
			if (e != null)
			{
				Globals.Message.ShowInformation(e.Message, e.Caption);
			}
		}
	}
}

