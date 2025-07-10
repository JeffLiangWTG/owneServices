#define CODE_ANALYSIS

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.Business.Presentation.GUI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.TaxFramework.GUI;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class InvoiceUserControl : ZUserControl, IDataGridLayoutIdentifierRoot
	{
		public ZGroupBox HeaderGroupBox;
		public ZPanel HeaderGroupBoxOuterPanel;
		public ZDropEdit AH_InvoiceTermDropEdit;
		public ZDropEdit AH_ComplianceSubTypeDropEdit;
		public ZCheckBox CashBasisVATIndicatorCheckbox;
		public ZCalcEdit AH_InvoiceTermDaysCalcEdit;
		public ZDateEdit AH_DueDateEdit;
		public ZTextBox AH_DescTextbox;
		public ZDateEdit AH_PostDateEdit;
		public ZTextBox AH_TransactionNumTextBox;
		public ZDateEdit AH_InvoiceDateEdit;
		public ZGroupBox LineDetailsGroupbox;
		public ZCheckBox IsDisbursementInvoiceCheckBox;
		public ZExchangeRateControl ExchangeRateControl;
		public ZCalcEdit ValidInvoiceTotalCalcEdit;
		public ZGuidFindBox TransactionGuidFindBox;
		public ZButton ApportionChargesButton;
		public ZCheckBox GSTInclusiveAmountsCheckBox;
		public ZButton BulkChargeImportButton;
		ZPanel LineSummaryOnlyPanel;
		ZPanel LineAndJobSummaryPanel;
		ZTabControl TabControl;
		public ZTabPage LineSummaryTabPage;
		public ZTabPage JobSummaryTabPage;
		public ZGrid TransactionLinesGrid;
		ZLabel InvoiceLineHidingMessageLabel;
		public ZGrid JobSummaryGrid;
		public ZCheckBox UseJobExRateCheckBox;
		public ZCheckBox OverrideJobExRateCheckBox;
		ZDropEdit PaymentCriticalityDropEdit;
		ZDateEdit PaymentRequestedDateEdit;
		public ZTextBox AH_ConsolidatedInvoiceRefTextBox;
		public ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		public ZCheckBox InvoiceTotalValidationCheckBox;
		public ZCalcEdit ExpectedExclTaxCalcEdit;
		public ZCalcEdit ExpectedTaxCalcEdit;
		public ZTextBox AH_ChequeOrReferenceTextBox;
		public ZTextBox InvoiceRemittanceReferenceTextBoxForAR;
		public ZTextBox InvoiceRemittanceReferenceTextBoxForAP;
		CollapsibleTableLayoutPanel InvoiceCollapsibleTableLayoutPanel;
		ZPanel zPanel2;
		ZPanel zPanel3;
		ZPanel zPanel4;
		ZPanel zPanel5;
		ZPanel zPanel6;
		ZPanel zPanel7;
		ZPanel zPanel8;
		ZPanel zPanel9;
		ZPanel zPanel10;
		ZPanel zPanel11;
		ZPanel zPanel12;
		public MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl AddressWithContactControl;
		ZPanel zPanel1;
		ZTabPage TaxSummaryTabPage;
		public ZDropEdit zDropEditPlaceOfSupply;
		public ZGuidFindBox AH_GB_TaxBranchFindBox;
		ZTabPage TaxTransactionSummaryTabPage;
		InvoiceOtherTaxesControl InvoiceTaxTransactionsControl;
		public ZDateEdit AH_DocReceivedDateEdit;
		ZPanel OtherTaxesBottomPanel;
		ZButton RemoveTaxTransactionsButton;
		public ZTextBox SourceReferenceTextBox;
		public ZTextBox GovernmentAllocatedIDTextBox;
		public ZTextBox ReasonDescriptionTextBox;
		public ZDropEdit ReasonCodeDropEdit;
		public ZTextBox AH_OriginalTransactionNumTextBox;
		public ZDateEdit AH_OriginalInvoiceDateEdit;
		public ZTextBox ComplianceSequenceTextBox;
		public ZTextBox AH_TransactionReferenceTextBox;
		ZGrid TaxSummaryGrid;
		public ZDateEdit AH_OriginalReferenceEndDateEdit;
		ZDropEdit ExtendDropEdit;
		public ZDropEdit ReversalDropEdit;
		public ZDateEdit AH_OriginalReferenceStartDateEdit;
		ZMenuItem ChangeSizeMenuItem;
		ContextMenu HeaderGroupBoxOuterContextMenu;

		public InvoiceUserControl()
		{
			fReadOnly = false;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			InitializeAdditionalCaptions();
			this.AddressWithContactControl.ContactInfoTabVisible = false;

			InvoiceRemittanceReferenceTextBoxForAP.AllowOverlap(InvoiceRemittanceReferenceTextBoxForAR);
			UseJobExRateCheckBox.AllowOverlap(OverrideJobExRateCheckBox);
			zPanel4.AllowOverlap(zPanel5);
			zPanel4.AllowOverlap(zPanel6);
			zPanel4.AllowOverlap(zPanel7);
			zPanel4.AllowOverlap(zPanel8);
			zPanel4.AllowOverlap(zPanel9);
			zPanel4.AllowOverlap(zPanel12);

			zPanel5.AllowOverlap(zPanel6);
			zPanel5.AllowOverlap(zPanel7);
			zPanel5.AllowOverlap(zPanel8);
			zPanel5.AllowOverlap(zPanel9);
			zPanel5.AllowOverlap(zPanel12);

			zPanel6.AllowOverlap(zPanel7);
			zPanel6.AllowOverlap(zPanel8);
			zPanel6.AllowOverlap(zPanel9);
			zPanel6.AllowOverlap(zPanel12);

			zPanel7.AllowOverlap(zPanel8);
			zPanel7.AllowOverlap(zPanel9);
			zPanel7.AllowOverlap(zPanel12);

			zPanel8.AllowOverlap(zPanel9);
			zPanel8.AllowOverlap(zPanel12);

			zPanel9.AllowOverlap(zPanel12);

			zPanel4.AllowOutsideOfParent();
			zPanel5.AllowOutsideOfParent();
			zPanel6.AllowOutsideOfParent();
			zPanel7.AllowOutsideOfParent();
			zPanel8.AllowOutsideOfParent();
			zPanel9.AllowOutsideOfParent();
			zPanel12.AllowOutsideOfParent();
		}

		public void InitExtendField(Action<ZDropEdit> registryExtendDropEdit)
		{
			registryExtendDropEdit(ExtendDropEdit);
		}

		internal void ActivateOtherTaxesTab()
		{
			if (TaxTransactionSummaryTabPage.TabVisible)
			{
				TabControl.SelectTab(TaxTransactionSummaryTabPage);
			}
		}

		public IDisposable SuspendTaxRecordsGridTaxTransactionControl()
		{
			return this.InvoiceTaxTransactionsControl?.SuspendTaxRecordsGridListChanged();
		}

		internal string[] ControlsAllowedToRemainEditableAfterOtherTaxesCalculated => new Control[] { AH_DescTextbox, TaxTransactionSummaryTabPage, ValidInvoiceTotalCalcEdit }.Select(c => c.Name).ToArray();

		InvoicingBase currentDataItem => this.CurrentDataItem as InvoicingBase;

		ZBool isAPInvoiceOrAPCreditNote => ((this.CurrentDataItem as APInvoice) != null) || ((this.CurrentDataItem as APCreditNote) != null);

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			if (isAPInvoiceOrAPCreditNote)
			{
				currentDataItem.AH_OHInfo.ValueChanged += OnHasPCDSettingInfoChanged;
			}
			if (TaxRecordParent != null)
			{
				TaxRecordParent.OnOtherTaxesCalculatedBeforePosting_Changed += OnOtherTaxesCalculatedBeforePosting_Changed;
			}
			base.OnCurrentDataItemChanged(e);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			if (isAPInvoiceOrAPCreditNote)
			{
				currentDataItem.AH_OHInfo.ValueChanged -= OnHasPCDSettingInfoChanged;
			}
			if (TaxRecordParent != null)
			{
				TaxRecordParent.OnOtherTaxesCalculatedBeforePosting_Changed -= OnOtherTaxesCalculatedBeforePosting_Changed;
			}
			base.OnCurrentDataItemChanging(e);
		}

		InvoicingBaseTaxRecordParent TaxRecordParent => TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(currentDataItem);

		InvoicingBaseForDisplayOtherTaxes OtherTaxesDisplayObj => TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(currentDataItem);

		IInvoicingBaseTaxFrameworkViewModel InvoicingBaseTaxFrameworkViewModel => TaxFrameworkObjectFactory.GetInvoicingBaseTaxFrameworkViewModel(currentDataItem);

		IInvoiceFormPresentationProvider InvoiceFormPresentationProvider => ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetInvoiceFormPresentationProvider();

		ZBool isPostedAndTaxTransactionsExists => currentDataItem.IsPosted && OtherTaxesDisplayObj.TaxRecordTransactionLinePivotForDisplay.TaxTransactionCollection.Any();

		readonly string[] complianceDocumentRelatedColumns = new string[] { InvoicingLineBase.Schema.CreateComplianceDocumentRecordOnPosting, InvoicingLineBase.Schema.ComplianceDocumentOrganization, InvoicingLineBase.Schema.ComplianceDocumentVATRegistrationNum, InvoicingLineBase.Schema.ComplianceSubType, InvoicingLineBase.Schema.ComplianceDocumentNumber, InvoicingLineBase.Schema.ComplianceDocumentDate, InvoicingLineBase.Schema.ComplianceDocumentReportingPeriod, InvoicingLineBase.Schema.ComplianceDocumentSupportingReason, InvoicingLineBase.Schema.ComplianceSupportingDocumentType, InvoicingLineBase.Schema.ComplianceSupportingDocumentNumber };

		void OnHasPCDSettingInfoChanged(object sender, EventArgs e)
		{
			if (isAPInvoiceOrAPCreditNote && TransactionLinesGrid != null)
			{
				if (shouldShowComplianceDocumentColumns)
				{
					TransactionLinesGrid.AddToAvailableColumns(complianceDocumentRelatedColumns);
					TransactionLinesGrid.Columns[InvoicingLineBase.Schema.ComplianceSubType].ColumnStyle.ReadOnly = false;
					TransactionLinesGrid.Columns[InvoicingLineBase.Schema.ComplianceDocumentNumber].ColumnStyle.ReadOnly = false;
				}
				else if (enableComplianceDocumentModule)
				{
					TransactionLinesGrid.RemoveFromAvailableColumns(complianceDocumentRelatedColumns);
				}
			}
		}

		void InitializeAdditionalCaptions()
		{
			LineSummaryTabPage.RunWhenTabInitialized((sender, e) =>
			{
				TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSExTaxAmount).GroupName = Res.GetData("InvoiceUserControl|c3ae0ffd-0d36-4d0a-a5ae-e75f1c133271|GroupName", "Amount");
				TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_AT).ToolTip = Res.GetString("InvoiceUserControl|e3687688-f7b0-4238-bf09-bcc5e54ee712|ToolTip", "Tax Rate");
				TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSTaxAmount).ToolTip = Res.GetString("InvoiceUserControl|7892c3c5-1f82-495e-a7cf-06f552f1f868|ToolTip", "Tax Amount");
				TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_AW).ToolTip = Res.GetString("InvoiceUserControl|24e37055-d4e2-4934-96ce-8db9c08f41ca|ToolTip", "Withholding Tax");
				TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OverseasTotal).ToolTip = Res.GetString("InvoiceUserControl|ba6df7fb-5858-4549-a9e4-e2bfa64058de|ToolTip", "Total Amount");
				TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_IsFinalCharge).ToolTip = Res.GetString("InvoiceUserControl|229ba9af-7904-48a2-b8de-886c94a690f7|ToolTip", "Final");
				TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_PreventInvoicePrintGrouping).ToolTip = Res.GetString("InvoiceUserControl|eeb69747-6c72-4422-9b3d-7346620dfc74|ToolTip", "Prevent Grouping");
				if (!DesignModeFinder.IsDesigning)
				{
					TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSGSTAmount).CaptionResourceString = AccountingCaptionHelper.OSTaxAmountCaption;
					TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_LocalGSTAmount).CaptionResourceString = AccountingCaptionHelper.LocalTaxAmountCaption;
					if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
					{
						TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_OSExtraTaxAmount).CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
						TransactionLinesGrid.GetColumnStyle(InvoicingLineBase.Schema.AL_LocalExtraTaxAmount).CaptionResourceString = AccountingCaptionHelper.LocalExtraTaxAmountCaption;
					}
					else
					{
						TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_OSExtraTaxAmount);
						TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_LocalExtraTaxAmount);
					}
				}
			});
		}

		public void SetInvoiceTypeCaptions(string invoiceType, string captionForInsertingIntoLabels)
		{
			SetCaption(this.HeaderGroupBox, invoiceType, true);
			SetCaption(this.AH_InvoiceDateEdit, captionForInsertingIntoLabels, true);
			SetCaption(this.AH_TransactionNumTextBox, captionForInsertingIntoLabels, true);
		}

		public void SetOrgInformationGroupBoxCaption(string accountType)
		{
			SetCaption(this.AddressWithContactControl, accountType, false);
		}

		void SetCaption(Control control, string value, bool format)
		{
			if (format)
			{
				IResCaptionedControl resControl = control as IResCaptionedControl;
				resControl.CaptionResourceString = resControl.CaptionResourceString.Format(value);
			}
			else
			{
				control.GetExtension<LabelCaptionRenderer>().Caption = value;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				var zForm = this.FindForm() as ZForm;

				if (zForm != null)
				{
					oDisplayMode = zForm.DisplayMode;
					zForm.Saved += zForm_Saved;
					if (InvoicingBase != null)
					{
						if (oDisplayMode == ODisplayMode.New ||
								InvoicingBase.AH_Ledger == LedgerTypes.IncompleteTransactions ||
								InvoicingBase.AH_Ledger == LedgerTypes.TransactionsPendingAllocation ||
								InvoicingBase.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
						{
							TaxSummaryTabPage.TabVisible = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
						}
						else if (InvoicingBase.IsPosted)
						{
							TaxSummaryTabPage.TabVisible = InvoicingBase.IsTaxed;
						}

						if (TaxSummaryTabPage.TabVisible)
						{
							SetCaption(TaxSummaryTabPage, InvoicingBase.Company.ConsumptionTaxDescriptionForCompanyForm, true);
						}

						TaxTransactionSummaryTabPage.TabVisible = TaxRecordParent.IsApplicableForTaxTransactions || isPostedAndTaxTransactionsExists;
						if (TaxTransactionSummaryTabPage.TabVisible)
						{
							TaxTransactionSummaryTabPage.TabInitialized += OtherTaxesSummaryTabPage_TabInitialized;
						}
					}
				}

				if (DataSource is APInvoice)
				{
					LineSummaryOnlyPanel.Visible = false;
					LineAndJobSummaryPanel.Visible = true;
					LineAndJobSummaryPanel.Controls.Add(this.InvoiceLineHidingMessageLabel);
					LineSummaryTabPage.Controls.Add(TransactionLinesGrid);
					JobSummaryTabPage.RunWhenTabInitialized((sender, args) =>
					{
						JobSummaryGrid.ContextMenu.MenuItems.Add("-");
						JobSummaryGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("InvoiceUserControl|NavigateToCosts", "Navigate to Costs"), JobSummaryGrid_NavigateToCosts));
					});
					PaymentRequestedDateEdit.Visible = true;
					PaymentCriticalityDropEdit.Visible = true;
					this.BindingSource.SetBindingMember(this.PaymentRequestedDateEdit, "AH_RequisitionDate");
					this.BindingSource.SetBindingMember(this.PaymentCriticalityDropEdit, "AH_RequisitionStatus");
				}
				else
				{
					if (TabControl.AllTabPages.Any(page => page.Name != LineSummaryTabPage.Name && ((ZTabPage)page).TabVisible))
					{
						LineSummaryOnlyPanel.Visible = false;
						LineAndJobSummaryPanel.Visible = true;
						LineAndJobSummaryPanel.Controls.Add(this.InvoiceLineHidingMessageLabel);
						LineSummaryTabPage.Controls.Add(TransactionLinesGrid);
						JobSummaryTabPage.TabVisible = false;
					}
					else
					{
						LineSummaryOnlyPanel.Visible = true;
						LineAndJobSummaryPanel.Visible = false;
						LineDetailsGroupbox.Controls.Add(TransactionLinesGrid);
					}

					PaymentRequestedDateEdit.Visible = false;
					PaymentCriticalityDropEdit.Visible = false;
				}

				if (InvoicingBase != null && (ZString)InvoicingBase.AH_LedgerInfo.OriginalValue != LedgerTypes.TransactionsPendingAllocation)
				{
					TransactionLinesGrid.RemoveFromAvailableColumns("Job+JH_GS_NKRepOps");
				}
				AH_NumberOfSupportingDocumentsCalcEdit.Visible = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China;

				if (InvoicingBase != null)
				{
					bool shouldShowSellReferenceForAR = InvoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable &&
						(InvoicingBase.AH_TransactionType == TransactionTypes.Invoice ||
						InvoicingBase.AH_TransactionType == TransactionTypes.CreditNote ||
						InvoicingBase.AH_TransactionType == TransactionTypes.AdjustmentNote);

					AH_ConsolidatedInvoiceRefTextBox.Visible = InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable || InvoicingBase.AH_Ledger == LedgerTypes.IncompleteTransactions;

					AH_ChequeOrReferenceTextBox.Visible = InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable ||
						InvoicingBase.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
						InvoicingBase.AH_Ledger == LedgerTypes.IncompleteTransactions ||
						shouldShowSellReferenceForAR;

					if (shouldShowSellReferenceForAR)
					{
						AH_ChequeOrReferenceTextBox.CaptionResourceString = Res.GetData("InvoiceUserControl|3b31e72d-674e-4bcf-a3d4-8aadd9160a38", "Sell Ref.", "Sell Reference", "");
					}

					InvoiceRemittanceReferenceTextBoxForAR.Visible = InvoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable;
					InvoiceRemittanceReferenceTextBoxForAP.Visible = InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable || InvoicingBase.AH_Ledger == LedgerTypes.IncompleteTransactions;
					AH_DocReceivedDateEdit.Visible = InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable || InvoicingBase.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || InvoicingBase.AH_Ledger == LedgerTypes.IncompleteTransactions;
					AH_OriginalReferenceEndDateEdit.Visible = InvoicingBase.ShouldShowOriginalInvoiceReferenceDatesFields;
					AH_OriginalReferenceStartDateEdit.Visible = InvoicingBase.ShouldShowOriginalInvoiceReferenceDatesFields;
					ReasonDescriptionTextBox.Visible = InvoicingBase.ShouldShowOriginalInvoiceReferenceReasonFields;
					ReasonCodeDropEdit.Visible = InvoicingBase.ShouldShowOriginalInvoiceReferenceReasonFields;

					if (InvoicingBase.Lines.Cast<InvoicingLineBase>().All(x => x.IndexOfImportedUniversalTransactionLine == -1))
					{
						TransactionLinesGrid.RemoveFromAvailableColumns("JobConsolXMLData");
						TransactionLinesGrid.RemoveFromAvailableColumns("ImportedChargeCode");
						TransactionLinesGrid.RemoveFromAvailableColumns("ImportedChargeCodeXmlCode");
					}

					if (!InvoicingBase.SupportMultiPeriodApportionment)
					{
						TransactionLinesGrid.RemoveFromAvailableColumns("PeriodApportionmentMethod");
						TransactionLinesGrid.RemoveFromAvailableColumns("PeriodStartDate");
						TransactionLinesGrid.RemoveFromAvailableColumns("PeriodEndDate");
						TransactionLinesGrid.RemoveFromAvailableColumns("PeriodClearingGLAccountPK");
					}
				}

				HookAL_JHFindBox_MaxLengthChanged();

				ShowTaxAndVATRecoverableColumns();

				ShowComplianceDocumentColumns();

				ShowOverrideTransactionLineDescription();

				ShowOverrideTransactionLineSequence();

				ShowRelatedJobNumberColumn();

				ShowSupplyTypeColumnOnLineSummaryTab();

				HideFPOSDropDown();

				if (!GlbCompany.CurrentCompany.IsExtraTaxApplicable())
				{
					TransactionLinesGrid.RemoveFromAvailableColumns(InvoiceLine.Schema.AL_OSExtraTaxAmount, InvoiceLine.Schema.AL_LocalExtraTaxAmount, InvoiceLine.Schema.AL_OSGSTAmount, InvoiceLine.Schema.AL_LocalGSTAmount);
				}

				if (!AccountingMasterFilesUtils.HasGLAccountSelectionAndEntry)
				{
					TransactionLinesGrid.RemoveFromAvailableColumns("AlternateGLAccountNumber");
					TransactionLinesGrid.RemoveFromAvailableColumns("AlternateGLAccountDescription");
				}
			}
		}

		void OtherTaxesSummaryTabPage_TabInitialized(object sender, EventArgs e)
		{
			InvoiceTaxTransactionsControl.Initialize(InvoiceFormPresentationProvider);
			InvoiceTaxTransactionsControl.Bind(OtherTaxesDisplayObj, "TaxRecordTransactionLinePivotForDisplay");
			SetRemoveOtherTaxesButtonEnabled();
		}

		ODisplayMode oDisplayMode;

		void ShowSupplyTypeColumnOnLineSummaryTab()
		{
			if (!InvoiceFormPresentationProvider.IsSupplyTypeColumnVisible())
			{
				TransactionLinesGrid.RemoveFromAvailableColumns("AL_SupplyType");
			}
		}

		void ShowRelatedJobNumberColumn()
		{
			if (InvoicingBase != null && !InvoicingBase.IsAPInvoiceOrCreditNote)
			{
				TransactionLinesGrid.RemoveFromAvailableColumns("AL_Calc_RelatedJobNumber");
			}
		}

		void ShowComplianceDocumentColumns()
		{
			LineSummaryTabPage.RunWhenTabInitialized((sender, e) =>
			{
				if (!shouldShowComplianceDocumentColumns)
				{
					TransactionLinesGrid.RemoveFromAvailableColumns(complianceDocumentRelatedColumns);
				}

				if (enableComplianceDocumentModule && oDisplayMode != ODisplayMode.New && InvoicingBase.AH_Ledger != LedgerTypes.IncompleteTransactions)
				{
					TransactionLinesGrid.AddToAvailableColumns(InvoicingLineBase.Schema.ComplianceDocumentNumber);
					TransactionLinesGrid.AddToAvailableColumns(InvoicingLineBase.Schema.ComplianceSubType);
					TransactionLinesGrid.Columns[InvoicingLineBase.Schema.ComplianceSubType].ColumnStyle.ReadOnly = true;
					TransactionLinesGrid.Columns[InvoicingLineBase.Schema.ComplianceDocumentNumber].ColumnStyle.ReadOnly = true;
				}
			});
		}

		void HideFPOSDropDown()
		{
			zDropEditPlaceOfSupply.Visible = InvoicingBase?.NeedPlaceOfSupplyAtHeaderLevel ?? false;

			if (InvoicingBase == null || !InvoicingBase.NeedPlaceOfSupplyAtLineLevel)
			{
				TransactionLinesGrid.RemoveFromAvailableColumns("AL_PlaceOfSupply");
			}
		}

		public void SetOverrideExchRateCheckBoxPositionAndVisible(bool isForeignCurrencyInvoice, bool isSettingLineExchangeRateSupported)
		{
			var isUseJobExRateCheckBoxVisible = false;
			var isOverrideJobExRateCheckBoxVisible = false;

			if (isSettingLineExchangeRateSupported && Env.Security.NewPayablesOverridePostingExchangeRateAllows.IsAllowed)
			{
				if (isForeignCurrencyInvoice)
				{
					isUseJobExRateCheckBoxVisible = true;
					if (GetInvoicePostingExchangeRateOptionAP(InvoicePostingExchangeRateCurrencyType.Code.Foreign) != ExRateOption.Default.Code)
					{
						isOverrideJobExRateCheckBoxVisible = true;
					}
				}
				else
				{
					if (GetInvoicePostingExchangeRateOptionAP(InvoicePostingExchangeRateCurrencyType.Code.Local) == ExRateOption.Default.Code)
					{
						isUseJobExRateCheckBoxVisible = true;
					}
					else
					{
						isOverrideJobExRateCheckBoxVisible = true;
					}
				}
			}
			else if (isSettingLineExchangeRateSupported)
			{
				isUseJobExRateCheckBoxVisible = true;
			}

			UseJobExRateCheckBox.Visible = isUseJobExRateCheckBoxVisible;
			OverrideJobExRateCheckBox.Visible = isOverrideJobExRateCheckBoxVisible;

			if (isOverrideJobExRateCheckBoxVisible)
			{
				OverrideJobExRateCheckBox.GetExtension<LabelCaptionRenderer>().Caption = isUseJobExRateCheckBoxVisible
					? this.OverrideJobExRateCheckBox.CaptionResourceString.ShortCaption
					: this.OverrideJobExRateCheckBox.CaptionResourceString.Caption;
			}

			UseJobExRateCheckBox.Location = isUseJobExRateCheckBoxVisible && !isOverrideJobExRateCheckBoxVisible
				? CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 3, true)
				: CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 3, true);
		}

		string GetInvoicePostingExchangeRateOptionAP(string invoiceCurrencyType)
		{
			return AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.Value
				.Cast<InvoicePostingExRateOption>()
				.FirstOrDefault(x => x.InvoiceCurrencyType == invoiceCurrencyType)
				.ExRateOption;
		}

		ZBool enableComplianceDocumentModule => AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value;

		ZBool shouldShowComplianceDocumentColumns => (oDisplayMode == ODisplayMode.New && (InvoicingBase?.HasPCDSettingForAP ?? false))
			|| (InvoicingBase?.HasPCDSettingForIN ?? false);

		void ShowTaxAndVATRecoverableColumns()
		{
			LineSummaryTabPage.RunWhenTabInitialized((sender, e) =>
			{
				bool showTaxColumns = InvoicingBase != null && (InvoicingBase.IsTaxed || InvoicingBase.AH_GC_IsGSTRegistered_ReadOnly);
				if (!showTaxColumns)
				{
					TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_OSGSTAmount, InvoicingLineBase.Schema.AL_LocalGSTAmount);
				}
				bool showTaxBasisColumn = showTaxColumns &&
					(InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable ||
					 InvoicingBase.AH_Ledger == LedgerTypes.IncompleteTransactions ||
					 InvoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable);
				if (!showTaxBasisColumn)
				{
					TransactionLinesGrid.RemoveFromAvailableColumns("TaxReportingBasisHumanReadableName");
				}

				bool showVATRecoverableColumns = showTaxColumns &&
					(InvoicingBase.AH_Ledger == LedgerTypes.AccountsPayable || InvoicingBase.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions || InvoicingBase.AH_Ledger == LedgerTypes.IncompleteTransactions);
				if (!showVATRecoverableColumns)
				{
					TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage);
					TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_OSTaxAmount_Recoverable);
					TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_OSTaxAmount_NotRecoverable);
					TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_LocalTaxAmount_Recoverable);
					TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_LocalTaxAmount_NotRecoverable);
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
				{
					TransactionLinesGrid.RemoveFromAvailableColumns(InvoicingLineBase.Schema.AL_GovtChargeCode);
				}
			});
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var invoicingBase = DataSource as InvoicingBase;
			if (invoicingBase != null)
			{
				if (invoicingBase.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					TransactionGuidFindBox.ModuleID = ModuleIDs.APTransaction;
				}
				else if (invoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					TransactionGuidFindBox.ModuleID = ModuleIDs.ARTransaction;
				}

				if (invoicingBase.FilteredLines.Count < invoicingBase.Lines.Count)
				{
					InvoiceLineHidingMessageLabel.Visible = true;
				}
			}
		}

		void zForm_Saved(object sender, EventArgs e)
		{
			var invoicingBase = DataSource as InvoicingBase;

			if (invoicingBase != null && invoicingBase.AH_OHInfo.ReadOnly)
			{
				AddressWithContactControl.SetReadOnlyIncludingChildren();
			}
		}

		void ShowOverrideTransactionLineDescription()
		{
			if (InvoicingBase != null && InvoicingBase.IsInDatabase && InvoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable &&
				(InvoicingBase.AH_TransactionType == TransactionTypes.Invoice || InvoicingBase.AH_TransactionType == TransactionTypes.CreditNote || InvoicingBase.AH_TransactionType == TransactionTypes.AdjustmentNote) &&
				InvoicingBase.AH_GC.ToGuid() == Env.CurrentCompany.PK
				)
			{
				var allowChargeDescriptionOverrideOnPostedARInvoiceValue = AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.Value;
				if (allowChargeDescriptionOverrideOnPostedARInvoiceValue != null && !string.IsNullOrEmpty(allowChargeDescriptionOverrideOnPostedARInvoiceValue))
				{
					TransactionLinesGrid.ContextMenu.MenuItems.Add(
						new ZMenuItem(ResString.GetMultilingualString("InvoiceUserControl.OverrideTransactionDescriptionMenuItem", "Override Transaction Line Description"),
							OverrideTransactionDescription_Click));
				}
			}
		}

		void OverrideTransactionDescription_Click(object sender, EventArgs e)
		{
			if (!Env.Security.OverrideTransactionLineDescription.IsAllowed)
			{
				Globals.Message.Show(Res.GetString("9F5FB6E1-D0C5-4362-823E-95EFAF8E844D", "You do not have sufficient rights to override transaction line descriptions. {0}",
					Env.Security.OverrideTransactionLineDescription.ErrorMessageForNotAllowed));
			}
			else if (TransactionLinesGrid.SelectedElements.Length == 0)
			{
				ShowNoSelectedLineMessage();
			}
			else
			{
				var adaptor = new InvoiceLineOverrideForEditingDescriptionAdaptor(new BusinessObjectFactory(), TransactionLinesGrid.SelectedElements.Select(bizo => bizo.PK).ToArray());
				ZFormModaliser.Show(new OverrideInvoiceLineDescriptionForm(adaptor), FindForm());
			}
		}

		void ShowOverrideTransactionLineSequence()
		{
			if (InvoicingBase != null)
			{
				var overrideTransactionLineSequenceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(InvoicingBase.Company.GC_RN_NKCountryCode) as IOverrideTransactionLineSequenceProvider;

				if (overrideTransactionLineSequenceProvider?.CanOverrideTransactionLineSequence(InvoicingBase) ?? false)
				{
					TransactionLinesGrid.ContextMenu.MenuItems.Add(
						new ZMenuItem(ResString.GetMultilingualString("InvoiceUserControl.OverrideTransactionSequenceMenuItem", "Override Transaction Line Sequence"),
							OverrideTransactionLineSequence_Click));
				}
			}
		}

		void OverrideTransactionLineSequence_Click(object sender, EventArgs e)
		{
			var security = Env.Security.OverrideTransactionLineSequence;

			if (security.IsAllowed)
			{
				var selectedTransactions = TransactionLinesGrid.SelectedElements;
				if (selectedTransactions != null && selectedTransactions.Length > 0)
				{
					var adaptor = new InvoiceLineOverrideForEditingSequenceAdaptor(new BusinessObjectFactory(), selectedTransactions.Select(bizo => bizo.PK).ToArray());
					ZFormModaliser.Show(new OverrideInvoiceLineSequenceForm(adaptor), ParentForm);
				}
				else
				{
					ShowNoSelectedLineMessage();
				}
			}
			else
			{
				security.ShowError();
			}
		}

		void ShowNoSelectedLineMessage()
		{
			Globals.Message.Show(Res.GetString("0e403729-26aa-4db1-9ff7-c428c382174a", "Please select one or more invoice lines"));
		}

		void HookAL_JHFindBox_MaxLengthChanged()
		{
			ZGridColumn gridColumn = TransactionLinesGrid.Columns[InvoicingLineBase.Schema.AL_JH];
			if (gridColumn != null)
			{
				ZCustomControlColumnStyle columnStyle = (ZCustomControlColumnStyle)gridColumn.ColumnStyle;
				if (columnStyle != null)
				{
					ZGridFindBox findBox = (ZGridFindBox)columnStyle.EditControl;
					if (findBox != null)
					{
						findBox.MaxLengthChanged += new EventHandler(AL_JHFindBox_MaxLengthChanged);
					}
				}
			}
		}

		void AL_JHFindBox_MaxLengthChanged(object sender, EventArgs e)
		{
			ZGridColumn gridColumn = TransactionLinesGrid.Columns[InvoicingLineBase.Schema.AL_JH];
			if (gridColumn != null)
			{
				ZCustomControlColumnStyle columnStyle = (ZCustomControlColumnStyle)gridColumn.ColumnStyle;
				if (columnStyle != null)
				{
					ZGridFindBox findBox = (ZGridFindBox)columnStyle.EditControl;
					if (findBox != null)
					{
						findBox.MaxLength = 50;
					}
				}
			}
		}

		InvoicingBase InvoicingBase
		{
			get { return DataSource as InvoicingBase; }
		}

		void JobSummaryGrid_NavigateToCosts(object sender, EventArgs e)
		{
			if (JobSummaryGrid.CurrentRowIndex != -1)
			{
				TabControl.SelectTab(LineSummaryTabPage);
				SelectCosts(InvoicingBase.InvoiceDependentJobs[JobSummaryGrid.CurrentRowIndex].Job);
			}
		}

		void SelectCosts(Job job)
		{
			int i = 0;
			foreach (InvoicingLineBase line in InvoicingBase.Lines)
			{
				if (line.AL_JH == job.PK)
				{
					TransactionLinesGrid.Select(i);
				}
				i++;
			}
		}

		bool fReadOnly;

		[Browsable(true), Category(ZGUIConstants.DesignerCategory)]
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				if (fReadOnly != value)
				{
					fReadOnly = value;

					this.AH_DescTextbox.ReadOnly = fReadOnly;
				}
			}
		}

		void TabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			var controlSender = sender as ZTabControl;
			if (controlSender?.SelectedTab != null)
			{
				if (controlSender.SelectedTab.Name == "TaxSummaryTabPage")
				{
					if (!InvoicingBase.IsTaxSummaryTabSelected)
					{
						InvoicingBase.IsTaxSummaryTabSelected = true;
						InvoicingBase.LoadInvoiceLineTaxSummaries();
					}
				}
				else if (controlSender.SelectedTab.Name == "JobSummaryTabPage")
				{
					InvoicingBase.IsTaxSummaryTabSelected = false;
					InvoicingBase.LoadInvoiceDependentJobs();
				}
				else
				{
					InvoicingBase.IsTaxSummaryTabSelected = false;
				}
			}
		}

		#region IDataGridLayoutIdentifierRoot Members

		string IDataGridLayoutIdentifierRoot.ID
		{
			get
			{
				ZForm form = FindForm() as ZForm;

				string result = string.Empty;

				if (form != null)
				{
					result = form.Name;

					if (form.BusinessEntity != null)
					{
						result += form.BusinessEntity.GetType().Name;
					}
				}

				return result;
			}
		}

		#endregion

		public void CollapseTableWhenBecomeVisible()
		{
			if (InvoiceCollapsibleTableLayoutPanel.Visible)
			{
				CollapseTable();
			}
		}

		void CollapseTable()
		{
			InvoiceCollapsibleTableLayoutPanel.Collapse();
			InvoiceCollapsibleTableLayoutPanel.StopFlicker();
		}

#if DEBUG
		public string TransactionNumTextBoxCaption
		{
			get { return this.AH_TransactionNumTextBox.GetExtension<LabelCaptionRenderer>().Caption; }
		}

		public ZTabControl TabControl_ForTest
		{
			get { return TabControl; }
		}

		public ZGrid TaxSummaryGrid_ForTest
		{
			get { return TaxSummaryGrid; }
		}

		internal bool IsTaxTransactionSummaryTabActivated_ForTest => TabControl_ForTest.SelectedTab.Name == TaxTransactionSummaryTabPage.Name;

#endif

		void RemoveTaxTransactionsButton_Click(object sender, EventArgs e)
		{
			using (InvoicingBaseTaxFrameworkViewModel.SuspendTrackingHasChanges)
			{
				ObjectFactory.Get<ITaxProcessor>().DeleteTaxesNotInDB(TaxRecordParent);
			}
		}

		void OnOtherTaxesCalculatedBeforePosting_Changed(object sender, EventArgs e)
		{
			SetRemoveOtherTaxesButtonEnabled();
			InvoiceTaxTransactionsControl?.ProcessWhenOtherTaxesAddedOrRemoved();
		}

		void SetRemoveOtherTaxesButtonEnabled()
		{
			if (RemoveTaxTransactionsButton != null)
			{
				RemoveTaxTransactionsButton.Enabled = TaxRecordParent.IsTaxTransactionsCalculatedBeforePosting;
			}
		}

		int originalHeight;

		void ChangeSizeMenuItem_Click(object sender, EventArgs e)
		{
			if (originalHeight == 0)
			{
				originalHeight = HeaderGroupBoxOuterPanel.Size.Height;
			}

			if (ChangeSizeMenuItem.Caption.ToString() == MaximizeInvoiceHeaderMenuItemCaption.ToString())
			{
				HeaderGroupBoxOuterPanel.MaximumSize = ControlDpiScalingHelper.NewScaledSize(MaximumSize.Width, originalHeight, true);
				ChangeSizeMenuItem.Caption = MinimizeInvoiceHeaderMenuItemCaption;
			}
			else
			{
				HeaderGroupBoxOuterPanel.MaximumSize = ControlDpiScalingHelper.NewScaledSize(MaximumSize.Width, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(HeaderGroupBoxOuterPanel.MinimumSize.Height), true);
				ChangeSizeMenuItem.Caption = MaximizeInvoiceHeaderMenuItemCaption;
			}
		}

		MultilingualString MaximizeInvoiceHeaderMenuItemCaption => ResString.GetMultilingualString("10FED592-EC2F-470E-96E6-1AD05EF7C868", "Maximize Invoice Header");
		MultilingualString MinimizeInvoiceHeaderMenuItemCaption => ResString.GetMultilingualString("8D4954F2-FE82-4527-A7ED-79164CCE043B", "Minimize Invoice Header");

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			ChangeSizeMenuItem.Click -= ChangeSizeMenuItem_Click;
			base.Dispose(disposing);
		}

		#endregion
	}
}

