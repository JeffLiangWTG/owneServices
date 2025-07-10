using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobChargeUserControl : ZPlugInContainerControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		#region Initialization

		public JobChargeUserControl()
		{
			InitializeComponent();

			AutoratingCostTabPage.RunWhenBindingOrFirstShown(delegate
			{
				SetupCostAutorateNotePopupErrorText();
			});
			AutoratingSellTabPage.RunWhenBindingOrFirstShown(delegate
			{
				SetupSellAutorateNotePopupErrorText();
			});
			JobExRateBoundGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(JobExRateBoundGrid_ColourDeciding);
			JobChargeBoundGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(JobChargeBoundGrid_ColourDeciding);

			ClientContractNumberTextBox.TextChanged += ClientContractNumberTextBox_TextChanged;

			InitializeAdditionalCaptions();

			InitializeMenuItems();

#if DEBUG
			TypeDescriptor.AddAttributes(ChargeableUnitLabel, new SuppressFormsLocalizedTestAttribute());
#endif
			if (!DesignModeFinder.IsDesigning)
			{
				TaxTransactionTabPage.TabVisible = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetCompanyTaxConfigurations(new BusinessObjectFactory(), GlbCompany.CurrentCompany).Any();
			}

			UpdateJobExRateGridColumns();

			ChargesDetailsTabControl.AllowOverlap(TotalsPanel);
			ChargesDetailsTabControl.AllowOverlap(TaxExpenseTotalsPanel);
			ChargesDetailsTabControl.AllowOverlap(QuotesCodeFindBox);

			QuotesCodeFindBox.AllowOverlap(OverseasAgentPanel);
			CostTotalsPanel.AllowOverlap(CostTotalPanel);

			CashAdvancePanel.AllowOutsideOfParent();
			SellTotalsPanel.AllowOutsideOfParent();

			cashAdvanceRequestStatusTextBox.AllowOutsideOfParent();
			cashAdvanceRequestIDTextBox.AllowOutsideOfParent();
			cashAdvanceRequiedCheckBox.AllowOutsideOfParent();
		}
		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			// Allow tabbing backward between specific cash advance controls
			if ((control == cashAdvanceRequestStatusTextBox && previousControl == cashAdvanceRequestIDTextBox) ||
				(control == cashAdvanceRequestIDTextBox && previousControl == cashAdvanceRequiedCheckBox) ||
				(control == cashAdvanceRequestIDTextBox && previousControl == cashAdvanceRequestStatusTextBox))
			{
				return true;
			}

			return false;
		}

		void ClientContractNumberTextBox_TextChanged(object sender, EventArgs e)
		{
			ClientContractNumberTextBox.ManuallySetCaptionToolTip(ClientContractNumberTextBox.Text);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (Job != null)
			{
				if (Job.ChargesFilteredByChargeViewingPermission.Count < Job.Charges.Count)
				{
					this.ChargeHidingMessageLabel.Visible = true;
				}

				GlbCompany company = null;
				using (Job.Factory.AddDiagnosisForFactoryQueryCacheWhenLoadingEnvCurrentBranchOrCompany())
				{
					company = Job.Company;
				}
				TaxExpenseTotalsPanel.Visible = company.IsEnabledForTaxFrameworkConfiguration(Job.Factory);
			}
		}

		void UpdateJobExRateGridColumns()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var company = Job?.Company ?? GlbCompany.CurrentCompany;
				if (!AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					JobExRateBoundGrid.RemoveFromAvailableColumns(ExchangeRate.Schema.JF_InvoiceCurrencyType);
				}
			}
		}

		void InitializeMenuItems()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var allowChargeDescriptionOverrideOnPostedARInvoiceValue = AccountingConfigurationRegistry.Instance.AllowChargeDescriptionOverrideOnPostedARInvoice.Value;
				if (!string.IsNullOrEmpty(allowChargeDescriptionOverrideOnPostedARInvoiceValue))
				{
					JobChargeBoundGrid.ContextMenu.MenuItems.Add(
								new ZMenuItem(ResString.GetMultilingualString("JobChargeUserControl.OverrideTransactionDescriptionMenuItem", "Override Transaction Line Description"),
									OverrideTransactionDescription_Click));
				}
				JobChargeBoundGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("030626fd-39e2-4aa4-ab65-b800d62fa243", "Append Charge Line Description"), AppendToUnpostedChargeDescription_Click));
			}
		}

		void AppendToUnpostedChargeDescription_Click(object sender, EventArgs e)
		{
			if (!Job.AllowAppendToUnpostedTransactionDescriptionForJob)
			{
				Globals.Message.Show(Res.GetString("bd09e4e0-c692-4004-a4db-3a69230b383d", "You do not have sufficient rights to append transaction line descriptions. {0}", Job.AllowAppendToUnpostedTransactionDescriptionForJobErrorText));
				return;
			}

			if (JobChargeBoundGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("6e629b8e-d76e-403f-8617-1a58f483c275", "Please select one or more charge lines"));
				return;
			}

			var invalidChargeCodes = (from Charge charge in JobChargeBoundGrid.SelectedElements
									  where (charge.ChargeCode != null && charge.ChargeCode.AC_AllowDescriptionOvertype)
									  select charge.ChargeCode.AC_Code).ToArray();
			if (invalidChargeCodes.Length > 0)
			{
				var invalidChargeCodesAsString = string.Join(", ", invalidChargeCodes);
				Globals.Message.Show(Res.GetString("860825f1-ebc7-4fba-a423-7c2b64fcf13f", "Charges that allow description override cannot be selected. \r\n\r\nThese Charges are: \r\n{0}\r\n\r\nFor these charges, you can simply override description as required.\r\nPlease revise selection.", invalidChargeCodesAsString));
				return;
			}

			var charges = (from Charge charge in JobChargeBoundGrid.SelectedElements
						   where (!charge.IsRevenuePosted && !charge.JR_ACInfo.HasErrors())
						   select charge).ToArray();

			if (!charges.Any())
			{
				Globals.Message.Show(Res.GetString("48316e61-706a-4d5a-97c1-2e2cdf059983", "No suitable charge lines were selected. Please select unposted lines"));
				return;
			}

			ChargeDescriptionOverrider.AddToWrappedObjects(charges);
			ZFormModaliser.Show(new AppendToUnpostedChargeDescriptionForm(ChargeDescriptionOverrider), FindForm());
		}

		void OverrideTransactionDescription_Click(object sender, EventArgs e)
		{
			if (!Job.AllowOverridePostedTransactionDescriptionForJob)
			{
				Globals.Message.Show(Res.GetString("9F5FB6E1-D0C5-4362-823E-95EFAF8E844D", "You do not have sufficient rights to override transaction line descriptions. {0}",
					Job.AllowOverridePostedTransactionDescriptionForJobErrorText));
				return;
			}

			if (JobChargeBoundGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("0DA7BD59-D054-40BB-84B5-E3DCF5045AC3", "Please select one or more charge lines"));
				return;
			}

			var linePks = (from Charge charge in JobChargeBoundGrid.SelectedElements
						   where charge.IsRevenuePosted
						   select charge.JR_AL_ARLine).ToArray();

			if (linePks.Length == 0)
			{
				Globals.Message.Show(Res.GetString("A0757E92-6F51-4E2D-8FC5-4AF823435E70", "No suitable charge lines were selected. Please select lines with posted revenue."));
				return;
			}

			var adaptor = new InvoiceLineOverrideForEditingDescriptionAdaptor(new BusinessObjectFactory(), linePks);

			if (adaptor.WrappedObjects != null)
			{
				if (adaptor.InnerObjectType == typeof(JobRevenueJournalLine))
				{
					Globals.Message.Show(Res.GetString("2BFA1736-75D8-42B6-B870-C00F42F0974E", "You cannot Override Transaction Line Description of a Charge which pertains to a Job Revenue Journal"));
				}
				else
				{
					ZFormModaliser.Show(new OverrideInvoiceLineDescriptionForm(adaptor), FindForm());
				}
			}
		}

		void InitializeAdditionalCaptions()
		{
			this.JR_AW_CostBoundFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|JR_AW_CostBoundFindBox.PopupCaption", "WHT Rates");
			this.JR_AT_CostBoundFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|JR_AT_CostBoundFindBox.PopupCaption", "Tax Rates");
			this.SellGSTRateGuidFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|SellGSTRateGuidFindBox.PopupCaption", "Tax Rates");
			this.SellWHTRateGuidFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|SellWHTRateGuidFindBox.PopupCaption", "WHT Rates");
			this.QuotesCodeFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|QuotesCodeFindBox.PopupCaption", "Quotes");
			this.SalesRepFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|SalesRepFindBox.PopupCaption", "Staff members");
			this.OperatorFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|OperatorFindBox.PopupCaption", "Staff members");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_AC).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zGuidFindBoxColumnStyleInfo1.ToolTip", "Please enter or click to select a charge code.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.ChargeType).ToolTip = Res.GetString("Accounting|JobsChargeUserControl|zTextBoxColumnStyleInfo1.ToolTip", "Charge Type in the context of this job.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.MarginPercentage).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCalcEditColumnStyleInfo1.ToolTip", "The Margin Percentage (if this charge is a Margin charge code), in the context of this Job.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_Desc).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zMultiLineTextBoxColumnInfo1.ToolTip", "Please enter the description of the charge of up to 1000 characters.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_GB).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zGuidFindBoxColumnStyleInfo2.ToolTip", "Please enter or click to select a branch related to this charge.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_GE).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zGuidFindBoxColumnStyleInfo3.ToolTip", "Please enter or click to select a department related to this charge.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_RX_NKCostCurrency).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zGuidFindBoxColumnStyleInfo4.ToolTip", "Please enter or click to select the currency of the cost amount.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_OSCostAmt).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCalcEditColumnStyleInfo2.ToolTip", "Please enter the cost amount here if it is in foreign currency");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_LocalCostAmt).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCalcEditColumnStyleInfo4.ToolTip", "Please enter the local cost amount here if it is in local currency.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_OH_CostAccount).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zOrganisationFindBoxColumnStyleInfo1.ToolTip", "Please enter or click to select a creditor where applicable. This field is applicable when you enter an Actual Cost or want to indicate the expected Creditor for the estimated Cost or Accrual.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsCostPosted).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCheckBoxColumnStyleInfo1.ToolTip", "Tick is display when Actual Cost or Unapproved Cost is posted.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsApproved).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCheckBoxColumnStyleInfo2.ToolTip", "Tick is display when Actual Cost is posted.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsApportioned).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCheckBoxColumnStyleInfo3.ToolTip", "Tick is displayed when Actual Cost or Accrual is apportioned.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_AT_CostGSTRate).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zGuidFindBoxColumnStyleInfo5.ToolTip", "Please enter or click to select the currency of the sell amount");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_OSSellAmt).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCalcEditColumnStyleInfo5.ToolTip", "Please enter the sell amount here if it is in foreign currency.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_LocalSellAmt).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCalcEditColumnStyleInfo7.ToolTip", "Please enter the local sell amount here if it is in local currency.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_OH_SellAccount).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zOrganisationFindBoxColumnStyleInfo2.ToolTip", "Please enter or click to select a debtor where this charge is to be billed to.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_DisplaySequence).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCalcEditColumnStyleInfo9.ToolTip", "The CFX % applicable to this charge, to this debtor.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsRevenuePosted).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCheckBoxColumnStyleInfo4.ToolTip", "Tick is displayed when Actual Revenue is posted.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_ARInvoiceNumber).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zTextBoxColumnStyleInfo2.ToolTip", "AR Invoice or Credit Note number is display when the Actual Revenue is posted.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_RX_NKSellCurrency).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zGuidFindBoxColumnStyleInfo6.ToolTip", "Enter or click to select Currency other than local currency for costing or billing.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_PreventInvoicePrintGrouping).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCheckBoxColumnStyleInfo5.ToolTip", "Tick is displayed when invoice print grouping is prevented.");
			JobExRateBoundGrid.GetColumnStyle(ExchangeRate.Schema.JF_BaseRate).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCalcEditColumnStyleInfo11.ToolTip", "Exchange rate default to exchange rate enter for the currency code as at enter date.");
			JobExRateBoundGrid.GetColumnStyle(ExchangeRate.Schema.JF_TodayRate).ToolTip = Res.GetString("Accounting|JobChargeUserControl|zCalcEditColumnStyleInfo14.ToolTip", "Today\'s Exchange Rate.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_IsARCashAdvance).ToolTip = Res.GetString("A571B255-7824-46D2-AF43-5C3E483B3AA3", "Tick this checkbox if this charge needs to be paid in advance of commencement of work on this job. When ticked, an Advance Payment Request for this charge can be generated.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.ARCashAdvanceRequestStatus).ToolTip = Res.GetString("0ED443BC-76D8-45CB-A916-9CA7EADBA1EA", "This field displays the current status of a Advance Payment Request. When the Advance Payment Required checkbox is ticked, it will be set to PEN - Pending, and updated to reflect accordingly when the Advance Payment Request is generated, and when payment or part payment is made.");
			JobChargeBoundGrid.GetColumnStyle(Charge.Schema.ARCashAdvanceRequestID).ToolTip = Res.GetString("EAE09A1C-3871-4458-ABCD-A454BB681B2C", "This field displays the Advance Payment Request ID, and is not populated until an Advance Payment Request for this charge line has been generated. If multiple charge lines are grouped into a single Advance Payment Request, each charge line included in the request will have the same Request ID.");
			this.JH_GEBoundFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|JH_GEBoundFindBox.PopupCaption", "Departments");
			this.JH_GBBoundFindBox.PopupCaption = Res.GetString("Accounting|JobChargeUserControl|JH_GBBoundFindBox.PopupCaption", "Branches");

			this.TotalsLabel.Text = Res.GetString("Accounting|JobChargeUserControl|TotalsLabel.Text", "Totals:");
			this.TaxExpenseTotalsLabel.Text = Res.GetString("Accounting|JobChargeUserControl|TaxExpenseTotalsLabel.Text", "Tax Expense Totals:");

#if DEBUG
			Enterprise.ZArchitecture.GUI.Testing.MissingResourceStringChecker.ExcludeFromTest(AutoRateDescCostTextBox);
			Enterprise.ZArchitecture.GUI.Testing.MissingResourceStringChecker.ExcludeFromTest(AutoRateDescRevenueTextBox);
#endif
		}

		#endregion

		#region Grid Colour

		void JobChargeBoundGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			BaseCharge charge = e.ObjectAtRow as BaseCharge;
			if (Job != null && charge != null && !charge.IsDeleted && charge.Job != null && charge.Job.PK != Job.PK)
			{
				e.Colour = Color.LightBlue;
			}
		}

		void JobExRateBoundGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			ExchangeRate exchangeRate = e.ObjectAtRow as ExchangeRate;
			if (exchangeRate != null && !exchangeRate.IsDeleted && !exchangeRate.IsBuyRateEqualsTodayRate)
			{
				e.Colour = Color.Gold;
			}
		}

		#endregion

		#region RelatedJob Filter Updated

		public event EventHandler<RelatedJobFilterUpdatedEventArgs> RelatedJobFilterUpdated;

		public class RelatedJobFilterUpdatedEventArgs : EventArgs
		{
			public RelatedJobFilterUpdatedEventArgs(ZBool anyFiltersApplied)
			{
				AnyFiltersApplied = anyFiltersApplied;
			}

			public ZBool AnyFiltersApplied { get; }
		}

		void AddRelatedJobsToShow(ZString jobNum)
		{
			if (RelatedJobsToShow.Add(jobNum))
			{
				UpdateRelatedJobsToShow();
			}
		}

		void RemoveRelatedJobsToShow(ZString jobNum)
		{
			if (RelatedJobsToShow.Remove(jobNum))
			{
				UpdateRelatedJobsToShow();
			}
		}

		void ResetRelatedJobsToShow()
		{
			RelatedJobsToShow.Clear();
			UpdateRelatedJobsToShow();
		}

		void UpdateRelatedJobsToShow()
		{
			Job.FilteredCharges.RelatedJobFilter = new HashSet<ZString>(RelatedJobsToShow);
			JobChargeBoundGrid.ReadOnly = RelatedJobsToShow.Any();
			RelatedJobFilterUpdated?.Invoke(this, new RelatedJobFilterUpdatedEventArgs(RelatedJobsToShow.Any()));
		}

		protected internal ZPanel TaxExpenseTotalsPanel;
		ZLabel TaxExpenseTotalsLabel;
		ZCalcEdit TotalTaxExpenseRevenueCalcEdit;
		ZCalcEdit TotalTaxExpenseCostCalcEdit;
		ZGuidFindBox TaxBranchGuidFindBox;
		ZGuidFindBox CostTaxBranchGuidFindBox;
		ZGuidFindBox SellTaxBranchGuidFindBox;
		protected internal ZPanel CashAdvancePanel;
		ZCheckBox cashAdvanceRequiedCheckBox;
		ZTextBox cashAdvanceRequestIDTextBox;
		ZTextBox cashAdvanceRequestStatusTextBox;
		ZButton CashAdvanceButton;

		HashSet<ZString> RelatedJobsToShow => relatedJobsToShow ?? (relatedJobsToShow = new HashSet<ZString>());
		HashSet<ZString> relatedJobsToShow;

		#endregion

		#region Hide / Show Fields

		public void ToggleShipmentAndBillingDetailsIsActive(bool show)
		{
			ShipmentAndBillingDetailsLinkLabel.Visible = show;
			if (!show && GetShipmentAndBillingDetailsForm() is ShipmentAndBillingDetailsForm existingForm)
			{
				existingForm.Close();
			}
		}

		public void ToggleOverseasAgentControlsVisibility(bool show)
		{
			OverseasAgentPanel.Visible = show;
			if (!show)
			{
				ControlDpiScalingHelper.SetLeft(InvoicingFieldsPanel, OverseasAgentPanel.Left + ControlDpiScalingHelper.ScaleToCurrentDpiX(30), false);
			}
			else
			{
				ControlDpiScalingHelper.SetLeft(InvoicingFieldsPanel, OverseasAgentPanel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
			}
			ControlDpiScalingHelper.SetLeft(ExchangeRatesZPanel, InvoicingFieldsPanel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
			ControlDpiScalingHelper.SetWidth(ExchangeRatesZPanel, ClientRectangle.Width - InvoicingFieldsPanel.Right - ControlDpiScalingHelper.ScaleToCurrentDpiX(10), false);
		}

		void HideNonApplicableSellControls()
		{
			SellGSTPanel.Visible = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			SellWHTPanel.Visible = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				ExtraTaxSellTaxAmountCalcEdit.CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
			}
			else
			{
				SellExtraTaxPanel.Visible = false;
			}
			if (!ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled)
			{
				cashAdvanceRequiedCheckBox.Visible = false;
				cashAdvanceRequestIDTextBox.Visible = false;
				cashAdvanceRequestStatusTextBox.Visible = false;
			}
		}

		void HideNonApplicableCostControls()
		{
			CostGSTPanel.Visible = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			CostWHTPanel.Visible = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				ExtraTaxCostTaxAmountCalcEdit.CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
			}
			else
			{
				CostExtraTaxPanel.Visible = false;
			}
			CashAdvanceButton.Visible = AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.Value;
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
		}

		public void Bind(Job jobToDisplay)
		{
			if (!IsAlreadyBound)
			{
				Job = jobToDisplay;
				SetupBinding();
				base.SetDataBinding(Job, "");
				Job.LocalChargeCFXChangedEvent += Job_LocalChargeCFXChanged;
				Job.SpotQuoteChargesExist += Job_SpotQuoteChargesExist;
				originalLocalChargesPK = Job.LocalChargesPK;
				Job.LocalZAddressWithContact.OnOrgChanged += Job_LocalChargeOrgChanged;

				SetupFlags();

				new QuickCalculateMenuItemManager(JobChargeBoundGrid, Job.PlugInData as IRatingSupporter).AddMenuItem();
				if (Job.PlugInData is IQuotedBooking quotedBooking)
				{
					ObjectFactory.Get<IQuotedBookingContextMenuManager>().AddMenuToCharges(JobChargeBoundGrid, quotedBooking);
				}

				IsAlreadyBound = true;
			}
		}

		Point? designerPosition;
		void DisplayClientContractNumber(bool display)
		{
			if (designerPosition == null)
			{
				designerPosition = QuotesCodeFindBox.Location;
			}
			ClientContractNumberTextBox.Visible = display;
			ClientContractNumberButton.Visible = display && ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled();

			QuotesCodeFindBox.Location = display
				? new Point(
					ClientContractNumberTextBox.Left + ClientContractNumberTextBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					ControlDpiScalingHelper.ScaleToCurrentDpiY(166))
				: designerPosition.Value;
		}

		void SetupFlags()
		{
			var displayPeriodicRatingCheckBox = Job.JobType != null && Job.JobType.ExcludeFromClientVisibleOption;
			var displayFilterByCostRefCheckBox = Job.PlugInData != null && Job.PlugInData.InvoicingSupporter.ShowOperationalJobRefFilter;
			var displayClientContractNumber = Job.PlugInData != null && Job.JobType != null && Job.JobType.ShouldDisplayClientContractNumber(Job.PlugInData);
			DisplayClientContractNumber(displayClientContractNumber);

			ExcludeFromPeriodicRatingCheckbox.Visible = displayPeriodicRatingCheckBox;
			IsChargeCostReferenceFilterEnabledCheckBox.Visible = displayFilterByCostRefCheckBox;

			if (Job.PlugInData is Enterprise.Integration.TransportBooking.IDtbBooking)
			{
				var dtbBooking = Job.PlugInData as Enterprise.Integration.TransportBooking.IDtbBooking;
				if (dtbBooking != null)
				{
					IsChargeCostReferenceFilterEnabledCheckBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("Accounting|IsChargeCostReferenceFilterEnabledCheckBoxIDtBooking", "Only Show Charges for {0}", dtbBooking.KM_JobID);
				}
			}
			else
			{
				var pluginData = Job.PlugInData as BusinessObject;
				if (pluginData != null)
				{
					IsChargeCostReferenceFilterEnabledCheckBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("Accounting|IsChargeCostReferenceFilterEnabledCheckBox", "Only Show Charges for this {0}", pluginData.HumanReadableName);
				}
			}

			if (!displayPeriodicRatingCheckBox && !displayFilterByCostRefCheckBox)
			{
				ControlDpiScalingHelper.SetHeight(JobExRateBoundGrid, JobExRateBoundGrid.Height + ExcludeFromPeriodicRatingCheckbox.Height, false);
			}
		}

		public ZCalcEdit TotalCFXAmountCalcEdit;
		ZPanel TotalsPanel;
		public ZLabel TotalsLabel;
		ZCodeFindBox OSSellAmountCurrencyCodeFindBox;
		ZCodeFindBox OSCostCurrencyCodeFindBox;
		ZTextBox costRatingOverrideCommentTextBox;
		ZTextBox JR_CostReferenceTextBox;
		protected internal ZCheckBox IsJobDescriptionOverriden;
		ZTextBox JobDescriptionTextBox;
		ConsolCosting.CalculationXMLUserControl CostRateCalculationXMLUserControl;
		ConsolCosting.CalculationXMLUserControl RevenueRateCalculationXMLUserControl;
		PaymentBasisUserControl CostPaymentBasisUserControl;
		PaymentBasisUserControl RevenuePaymentBasisUserControl;
		protected internal ZGrid JobChargeBoundGrid;
		ZGuidFindBox JR_GB_InternalBranchGuidFindBox;
		internal ZGuidFindBox JR_JH_InternalJobGuidFindBox;
		ZGuidFindBox JR_GE_InternalDeptGuidFindBox;
		ZCheckBox costRatedCheckBox;
		ZCheckBox sellRatedCheckBox;
		protected internal ZCheckBox ExcludeFromPeriodicRatingCheckbox;
		protected internal ZCheckBox IsChargeCostReferenceFilterEnabledCheckBox;
		protected internal ZLinkLabel ShipmentAndBillingDetailsLinkLabel;
		protected internal ZTextBox ClientContractNumberTextBox;
		protected ZButton.Bare ClientContractNumberButton;
		ZCodeFindBox QuotesCodeFindBox;
		ZPanel ExchangeRatesZPanel;
		protected internal ZGrid JobExRateBoundGrid;
		ZCheckBox hasDebtorAcceptedThisSellCharge;
		bool IsAlreadyBound;

		void SetupBinding()
		{
			SetDefaultColumns();
			SetupJobDependentControls();
			CostTabPage.RunWhenBindingOrFirstShown(delegate
			{
				HideNonApplicableCostControls();
			});
			RevenueTabPage.RunWhenBindingOrFirstShown(delegate
			{
				HideNonApplicableSellControls();
			});
			AutoratingCostTabPage.RunWhenBindingOrFirstShown(delegate
			{
				WiseRatesRawDataUserControl.Visible = DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value;
			});
		}

		#endregion

		#region Grid Columns

		void SetDefaultColumns()
		{
			SetJobChargeColumns();
			SetExchangeRateColumns();
			SetCashAdvanceColumns();
			AddCustomPropertyColumnsToGrid();
		}

		void SetDecimalsOnCalcEditInfo(ZString schemaColumnName, int decimals, ZGrid grid)
		{
			ZCalcEditColumnStyleInfo info = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(schemaColumnName);
			info.Decimals = decimals;
		}

		void SetCashAdvanceColumns()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (!ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled)
				{
					JobChargeBoundGrid.RemoveFromAvailableColumns(
						JobChargeSchema.Constants.JR_IsARCashAdvance,
						BaseCharge.Schema.ARCashAdvanceRequestStatus,
						BaseCharge.Schema.ARCashAdvanceRequestID);
				}
			}
		}

		void SetJobChargeColumns()
		{
			int decimals = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			SetDecimalsOnCalcEditInfo(Charge.Schema.JR_LocalCostAmt, decimals, JobChargeBoundGrid);
			SetDecimalsOnCalcEditInfo(Charge.Schema.JR_LocalSellAmt, decimals, JobChargeBoundGrid);
			SetDecimalsOnCalcEditInfo(Charge.Schema.JR_CFXAmtReverseSign, decimals, JobChargeBoundGrid);

			if (!DesignModeFinder.IsDesigning)
			{
				if (!Job.IsGatewayBillingJob())
				{
					JobChargeBoundGrid.RemoveFromAvailableColumns(
						nameof(BaseCharge.JR_Calc_RelatedJobNumber),
						nameof(BaseCharge.JR_Calc_InvoiceTarget));
				}

				if (!AutoJRJRegistryStatusHelper.IsAutoJRJEnabled())
				{
					JobChargeBoundGrid.RemoveFromAvailableColumns(
						JobChargeSchema.Constants.JR_GB_InternalBranch,
						JobChargeSchema.Constants.JR_GE_InternalDept,
						JobChargeSchema.Constants.JR_JH_InternalJob);
				}

				if (AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value)
				{
					JobChargeBoundGrid.RemoveFromAvailableColumns(Charge.Schema.JR_IsApproved);
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
				{
					JobChargeBoundGrid.RemoveFromAvailableColumns(Charge.Schema.JR_CostGovtChargeCode);
					JobChargeBoundGrid.RemoveFromAvailableColumns(Charge.Schema.JR_SellGovtChargeCode);
				}

				if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(Job.Company))
				{
					JobChargeBoundGrid.RemoveFromAvailableColumns(Charge.Schema.JR_CostPlaceOfSupply);
					JobChargeBoundGrid.RemoveFromAvailableColumns(Charge.Schema.JR_SellPlaceOfSupply);
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
				{
					CostSupplyTypeDropEdit.Visible = false;
					SellSupplyTypeDropEdit.Visible = false;

					JobChargeBoundGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_CostSupplyType.Name);
					JobChargeBoundGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_SellSupplyType.Name);
				}

				if (!(AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value && AccountingMasterFilesRegistry.Instance.EnableSellComplianceDescription.Value))
				{
					JobChargeBoundGrid.RemoveFromAvailableColumns(Charge.Schema.SellComplianceDescription);
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value || !GlbCompany.CurrentCompany.GC_IsGSTRegistered)
				{
					TaxBranchGuidFindBox.Visible = false;
					SellTaxBranchGuidFindBox.Visible = false;
					CostTaxBranchGuidFindBox.Visible = false;

					JobChargeBoundGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_GB_CostTaxBranch.Name);
					JobChargeBoundGrid.RemoveFromAvailableColumns(JobChargeSchema.JR_GB_SellTaxBranch.Name);

					ControlDpiScalingHelper.SetLeft(SalesRepFindBox, JobLocalReferenceTextBox.Left, false);
					ControlDpiScalingHelper.SetLeft(OperatorFindBox, JobLocalReferenceTextBox.Left, false);
				}
			}

			RevenueTabPage.RunWhenBindingOrFirstShown(delegate
			{
				if (!DesignModeFinder.IsDesigning)
				{
					if (!AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty))
					{
						ZGridColumnInfo info = JobChargeBoundGrid.GetColumnStyle(Charge.Schema.JR_CFXAmtReverseSign);
						JobChargeBoundGrid.ColumnStyles.Remove(info);
						SellWHTPanel.Controls.Remove(CFXAmtPanel);
					}
				}
			});
		}

		void SetExchangeRateColumns()
		{
			int exchangeRateDecimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
			SetDecimalsOnCalcEditInfo(ExchangeRate.Schema.JF_BaseRate, exchangeRateDecimals, JobExRateBoundGrid);
			SetDecimalsOnCalcEditInfo(ExchangeRate.Schema.JF_TodayRate, exchangeRateDecimals, JobExRateBoundGrid);
		}

		void AddCustomPropertyColumnsToGrid()
		{
			var additionalData = Job.Parent as IJobInvoicingAdditionalData;
			if (additionalData != null)
			{
				new ZGridCustomColumnsInitializer(JobChargeBoundGrid, Job.Charges, groupName: null, isVisible: true)
					.AddCustomColumns(additionalData.GetAdditionalProperties().CustomProperties);
			}
		}

		#endregion

		void SetupJobDependentControls()
		{
			if (Job != null)
			{
				JH_OH_LocalChargesBoundOrgCard.CaptionResourceString = Job.JH_OA_LocalChargesAddrCaption;
				JH_OH_AgentCollectBoundOrgCard.CaptionResourceString = Job.JH_OA_AgentCollectAddrCaption;
				JR_JH_InternalJobGuidFindBox.CaptionResourceString = Charge.GetJR_JH_InternalJobCaption(Job);
			}
		}

		#region Autorate Popup

		void SetupSellAutorateNotePopupErrorText()
		{
			AutoRatingUIHelper.SetNoNoteExistsErrorText(AutoRateNotePopupButton);
		}

		void SetupCostAutorateNotePopupErrorText()
		{
			AutoRatingUIHelper.SetNoNoteExistsErrorText(AutoRateNotePopupButton2);
		}

		#endregion

		#region Cost and Revenue Navigation

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == (Keys.F6))
			{
				if (ChargesDetailsTabControl.ContainsFocus)
				{
					if (ActiveControl != null)
					{
						ZString mappingName = FindMappingNameForControl(ActiveControl).SubstringSafe(KeepAfterIndex);
						BeginEditOnControlOnGrid(JobChargeBoundGrid, mappingName);
					}
				}
				else if (JobChargeBoundGrid.ContainsFocus)
				{
					Control foundControl = null;

					if (IsCostField)
					{
						foundControl = FindControl(CostTabPage, CurrentCellMappingName);
					}
					else if (IsRevenueField)
					{
						foundControl = FindControl(RevenueTabPage, CurrentCellMappingName);
					}
					else if (IsCommonField)
					{
						foundControl = FindControl(DetailsTabPage, CurrentCellMappingName);
					}
					if (foundControl != null)
					{
						using (JobChargeBoundGrid.SuspendCancelOfNonEditedRowOnLeaving())
						{
							foundControl.Focus();
						}
					}
				}
			}

			return base.ProcessDialogKey(keyData);
		}

		readonly int KeepAfterIndex = new ZString("FilteredCharges.").Length;

		ZString FindMappingNameForControl(Control activeControl)
		{
			return activeControl.GetBindingMember();
		}

		void BeginEditOnControlOnGrid(ZGrid parentGrid, ZString mappingName)
		{
			foreach (DataGridColumnStyle columnStyle in parentGrid.TableStyles[0].GridColumnStyles)
			{
				if (columnStyle.MappingName == mappingName)
				{
					parentGrid.BeginEdit(columnStyle, parentGrid.CurrentRowIndex);
					break;
				}
			}
		}

		void JobChargeBoundGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			BeginInvoke(new MethodInvoker(delegate
			{
				try
				{
					if (!this.IsDisposed)
					{
						using (Job.Factory.SetTempContext(BusinessContext.ChangingCurrentCellOnJobChargeBoundGrid))
						{
							bool tabChange = false;
							DataGridCell previousCurrentCell = JobChargeBoundGrid.CurrentCell;
							using (JobChargeBoundGrid.SuspendCancelOfNonEditedRowOnLeaving())
							{
								var currentCharge = JobChargeBoundGrid.GetCurrent();
								if (GetShipmentAndBillingDetailsForm() is ShipmentAndBillingDetailsForm form && currentCharge is Charge selectedCharge)
								{
									var detailsRowPK = ShipmentAndBillingDetails.GetRowPKByRelatedJob(selectedCharge.JR_Calc_RelatedJobNumber);
									if (detailsRowPK.IsValid)
									{
										form.SelectDetailRow(detailsRowPK);
									}
								}

								if (IsCostField)
								{
									if (currentCharge is Charge charge && charge.CostExchangeRate != null)
									{
										JobExRateBoundGrid.SelectSingleElementByPK(charge.CostExchangeRate.ExchangeRatePk);
									}
									if (currentCharge != null && ChargesDetailsTabControl.SelectedTab != CostTabPage)
									{
										ChargesDetailsTabControl.SelectedTab = CostTabPage;
										tabChange = true;
									}
								}
								else if (IsRevenueField)
								{
									if (currentCharge is Charge charge && charge.RevenueExchangeRate != null)
									{
										JobExRateBoundGrid.SelectSingleElementByPK(charge.RevenueExchangeRate.ExchangeRatePk);
									}
									if (currentCharge != null && ChargesDetailsTabControl.SelectedTab != RevenueTabPage)
									{
										ChargesDetailsTabControl.SelectedTab = RevenueTabPage;
										tabChange = true;
									}
								}
								else if (IsCommonField && currentCharge != null && ChargesDetailsTabControl.SelectedTab != DetailsTabPage)
								{
									ChargesDetailsTabControl.SelectedTab = DetailsTabPage;
									tabChange = true;
								}
							}

#if DEBUG
							JobChargeBoundGrid_CurrentCellChanged_InvokeEventForTestOnly?.Invoke(this, Job.FilteredCharges);
#endif

							if (tabChange)
							{
								JobChargeBoundGrid.Focus();
								JobChargeBoundGrid.CurrentCell = previousCurrentCell;
							}
						}
					}
				}
				catch (IndexOutOfRangeException exp)
				{
					var key = $"JobChargeBoundGrid_CurrentCellChanged_IndexOutOfRange";// ErrorReport Key should be in English Only

					var deletedItemTracingLog = CriticalValidationInfoCollectorService
						.GetService(Job.Factory)
						.GetInfoSafe(Job.PK, CriticalValidationInfoCollectorServiceKeyType.ChangingCurrentCellOnJobChargeBoundGrid);
					var message = $@"{exp.Message} ,some list items may be changed during processing,tracing log below:
{deletedItemTracingLog}";// ErrorReport Message should be in English Only

					ErrorReporter.ReportOnce(key, message, exp);
				}
			}));
		}

#if DEBUG
		public Action<JobChargeUserControl, FilteredChargeCollection> JobChargeBoundGrid_CurrentCellChanged_InvokeEventForTestOnly;
#endif

		Control FindControl(Control parentControl, ZString mappingName)
		{
			Control result = null;

			foreach (Control ctrl in parentControl.Controls)
			{
				var bindingMember = ctrl.GetBindingMember();
				if (!string.IsNullOrEmpty(bindingMember) && bindingMember.EndsWith(mappingName))
				{
					result = ctrl;
					break;
				}
				else
				{
					result = FindControl(ctrl, mappingName);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		#region Current Cell Mapping Name

		string CurrentCellMappingName
		{
			get
			{
				if (JobChargeBoundGrid.TableStyles[0].GridColumnStyles[JobChargeBoundGrid.CurrentCell.ColumnNumber] != null)
				{
					return JobChargeBoundGrid.TableStyles[0].GridColumnStyles[JobChargeBoundGrid.CurrentCell.ColumnNumber].MappingName;
				}
				return "";
			}
		}

		#endregion

		#region Is Cost Field

		bool IsCostField
		{
			get
			{
				return CurrentCellMappingName == JobChargeSchema.Constants.JR_RX_NKCostCurrency ||
					CurrentCellMappingName == JobChargeSchema.Constants.JR_OSCostAmt ||
					CurrentCellMappingName == JobChargeSchema.Constants.JR_LocalCostAmt ||
					CurrentCellMappingName == JobChargeSchema.Constants.JR_OH_CostAccount ||
					CurrentCellMappingName == BaseCharge.Schema.JR_IsCostPosted ||
					CurrentCellMappingName == BaseCharge.Schema.JR_IsApportioned ||
					CurrentCellMappingName == BaseCharge.Schema.JR_APInvoiceNum ||
					CurrentCellMappingName == BaseCharge.Schema.JR_APInvoiceDate ||
					CurrentCellMappingName == BaseCharge.Schema.JR_APDocumentReceivedDate ||
					CurrentCellMappingName == BaseCharge.Schema.JR_PaymentDate ||
					CurrentCellMappingName == BaseCharge.Schema.JR_CostReference ||
					CurrentCellMappingName == BaseCharge.Schema.JR_CostSupplyType ||
					CurrentCellMappingName == BaseCharge.Schema.JR_GB_CostTaxBranch;
			}
		}

		#endregion

		#region Is Revenue Field

		bool IsRevenueField
		{
			get
			{
				return CurrentCellMappingName == Charge.Schema.JR_RX_NKSellCurrency ||
					CurrentCellMappingName == Charge.Schema.JR_OSSellAmt ||
					CurrentCellMappingName == Charge.Schema.JR_LocalSellAmt ||
					CurrentCellMappingName == Charge.Schema.JR_OH_SellAccount ||
					CurrentCellMappingName == Charge.Schema.JR_CFXAmt ||
					CurrentCellMappingName == Charge.Schema.JR_PreventInvoicePrintGrouping ||
					CurrentCellMappingName == Charge.Schema.JR_InvoiceType ||
					CurrentCellMappingName == Charge.Schema.JR_IsRevenuePosted ||
					CurrentCellMappingName == Charge.Schema.JR_LineCFX ||
					CurrentCellMappingName == BaseCharge.Schema.JR_SellSupplyType ||
					CurrentCellMappingName == BaseCharge.Schema.JR_GB_SellTaxBranch;
			}
		}

		#endregion

		#region Is Common Field

		bool IsCommonField
		{
			get
			{
				return CurrentCellMappingName == Charge.Schema.JR_AC ||
					CurrentCellMappingName == Charge.Schema.ChargeType ||
					CurrentCellMappingName == Charge.Schema.MarginPercentage ||
					CurrentCellMappingName == Charge.Schema.JR_Desc ||
					CurrentCellMappingName == Charge.Schema.JR_GB ||
					CurrentCellMappingName == Charge.Schema.JR_GE ||
					CurrentCellMappingName == Charge.Schema.JR_GB_InternalBranch ||
					CurrentCellMappingName == Charge.Schema.JR_GE_InternalDept ||
					CurrentCellMappingName == Charge.Schema.JR_JH_InternalJob;
			}
		}

		#endregion

		#endregion

		#region Charge Description Override

		public ChargeDescriptionOverrideAdaptor ChargeDescriptionOverrider
		{
			get
			{
				return chargeDescriptionOverrider ?? (chargeDescriptionOverrider = new ChargeDescriptionOverrideAdaptor(Job.Factory));
			}
		}
		ChargeDescriptionOverrideAdaptor chargeDescriptionOverrider;

		#endregion

		#region Shipment and Billing Details

		void ShipmentAndBillingDetailsLinkLabel_Clicked(object sender, EventArgs e)
		{
			if (GetShipmentAndBillingDetailsForm() is ShipmentAndBillingDetailsForm existingForm)
			{
				existingForm.Close();
			}
			else if (Job.Parent is ForwardingConsol consol)
			{
				ShipmentAndBillingDetails = new ShipmentAndBillingDetails(consol);
				SubscribeToShipmentAndBillingDetails();
				var form = new ShipmentAndBillingDetailsForm(ShipmentAndBillingDetails);
				form.FormClosing += ShipmentAndBillingDetailsFormClosing;
				OpenedFormCache.GetInstance().Add(ShipmentAndBillingDetails.PK.ToGuid(), form, ShipmentAndBillingDetailsFormKey);
				form.Show();
			}
		}

		void ShipmentAndBillingDetailsFormClosing(object sender, FormClosingEventArgs e)
		{
			if (sender is ShipmentAndBillingDetailsForm form)
			{
				UnsubscribeFromShipmentAndBillingDetails();
				ResetRelatedJobsToShow();
				ShipmentAndBillingDetails = null;
			}
		}

		void SubscribeToShipmentAndBillingDetails()
		{
			ShipmentAndBillingDetails.DetailsRows.CountChanged += ShipmentAndBillingDetails_CountChanged;
			ShipmentAndBillingDetails.DetailsRows.Cast<ShipmentAndBillingDetailsRow>().ForEach(x => x.ShowRelatedChargesInfo.ValueChanged += ShowRelatedCharges_ValueChanged);
		}

		void UnsubscribeFromShipmentAndBillingDetails()
		{
			ShipmentAndBillingDetails.DetailsRows.CountChanged -= ShipmentAndBillingDetails_CountChanged;
			ShipmentAndBillingDetails.DetailsRows.Cast<ShipmentAndBillingDetailsRow>().ForEach(x => x.ShowRelatedChargesInfo.ValueChanged -= ShowRelatedCharges_ValueChanged);
		}

		void ShipmentAndBillingDetails_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is ShipmentAndBillingDetailsRow row)
			{
				if (e.ItemAdded)
				{
					row.ShowRelatedChargesInfo.ValueChanged += ShowRelatedCharges_ValueChanged;
				}
				else if (e.ItemRemoved)
				{
					row.ShowRelatedChargesInfo.ValueChanged -= ShowRelatedCharges_ValueChanged;
				}
			}
		}

		void ShowRelatedCharges_ValueChanged(object sender, EventArgs args)
		{
			if (sender is ShipmentAndBillingDetailsRow row)
			{
				if (row.ShowRelatedCharges)
				{
					AddRelatedJobsToShow(row.RelatedJobNum);
				}
				else
				{
					RemoveRelatedJobsToShow(row.RelatedJobNum);
				}
			}
		}

		internal ShipmentAndBillingDetails ShipmentAndBillingDetails;
		readonly string ShipmentAndBillingDetailsFormKey = "ShipmentAndBillingDetails";

		Form GetShipmentAndBillingDetailsForm()
		{
			return ShipmentAndBillingDetails != null
				? OpenedFormCache.GetInstance().GetForm(ShipmentAndBillingDetails.PK.ToGuid(), ShipmentAndBillingDetailsFormKey)
				: null;
		}

		#endregion

		#region ClientContractNumberButton_Click

		void ClientContractNumberButton_Click(object sender, EventArgs args)
		{
			if (Job.Parent is not IForwardingShipment shipment)
			{
				Globals.Message.Show(Res.GetString(
					"c64440f5-be97-44b2-4e5c-ec485690a55c",
					"This action is only valid for Job Headers linked to a Forwarding Shipment, but a {0} was found.",
					(Job.Parent as BusinessObject)?.TableName));
				return;
			}

			if (URLHelpers.TryGenerateURL("goto/ClientContractsImportG1", out var url))
			{
				var browserWindowFactory = ObjectFactory.Get<IBrowserInteropWindowFactory>();
				var windowTitle = Res.GetString("b952f9c8-76cb-6f8e-4d6c-2c6de9212fae", "Client Contracts Lookup");
				var browserWindow = browserWindowFactory.CreateBrowserInteropWindow(windowTitle, url);

				BrowserInteropHelperExtensions.AddBrowserCloseCommandHandler(browserWindow, data =>
				{
					if (!string.IsNullOrEmpty(data))
					{
						Job.JH_ClientContractNumber = data;
					}
				});
				BrowserInteropHelperExtensions.AddBrowserListenerCommandHandler(browserWindow, () => GetUpdateFilterData(shipment));
			
				browserWindow.ShowDialog();
			}
		}

		ClientUpdateFilterData GetUpdateFilterData(IForwardingShipment shipment)
		{
			var data = new ClientUpdateFilterData();
			data.startDate = shipment.JS_E_DEP;
			data.expiryDate = data.startDate;
			data.clientPK = Job.LocalChargesAddr?.OA_OH ?? ZGuid.Empty;
			data.contractID = Job.JH_ClientContractNumber;
			return data;
		}

		#endregion

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed(e);
			if (GetShipmentAndBillingDetailsForm() is ShipmentAndBillingDetailsForm existingForm)
			{
				existingForm.Close();
			}
		}

		#region Events

		bool Job_LocalChargeCFXChanged(ZDecimal newRate)
		{
			return Globals.Message.Show(Res.GetString("e72c4b62-41fa-4180-a695-bc38101ec55b", "You have changed the Department on this Job which affects CFX.\r\nDo you want to reset the Job CFX % to the default rate of {0}%?", newRate),
						Res.GetString("6a3b1806-7021-41f2-a447-18a1caf33e84", "Local Client CFX %"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		void Job_SpotQuoteChargesExist(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("10AB35BF-E558-42F4-816F-022D0FA6AC2B", "The Quote # has been applied to the Billing Job. Do you want to bring through all quoted charges from this Spot Quotation?"), Res.GetString("8358d879-54c8-4ca9-afec-1a240a7f1cee", "Spot Quote Charges"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			e.Cancel = result == DialogResult.No;
		}

		ZGuid originalLocalChargesPK;

		void Job_LocalChargeOrgChanged(object sender, EventArgs e)
		{
			if (Job.IsInDatabase && Job.LocalZAddressWithContact.OrgPK != originalLocalChargesPK)
			{
				if (Job.HasNonReversedCommissionHeaders(originalLocalChargesPK))
				{
					var info = Job.LocalZAddressWithContact.OrgPKInfo;
					info.HumanReadableName = Res.GetString("8A419408-0105-42EE-B0B1-DC7557A617B7", "Local Client");
					Globals.Message.Show(
						JobHeader.GetCommissionReversalWarningMessage(new[]
						{
							info
						}),
						JobHeader.CommissionReversalWarningMessageHeader,
						MessageBoxButtons.OK, MessageBoxIcon.Question);

					Job.SetOrgForCommissionsReversal(originalLocalChargesPK);
				}
			}
		}

		#endregion

		#region AP Auto Populate
		void AutoPopulateButton_Click(object sender, EventArgs e)
		{
			if (JobChargeBoundGrid.SelectedElements.Length > 0)
			{
				if (Job.HasChanges)
				{
					Globals.Message.Show(Res.GetString("1a275abb-a04f-4f2f-a3ad-0ea0a05550f0", "Job should be saved before pressing 'Populate AP Details'."));
				}
				else
				{
					Charge ch = (Charge)JobChargeBoundGrid.SelectedElements[0];

					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					Job.Loader jobLoader = new Job.Loader(newFactory, Job.Parent);
					Job job = jobLoader.Load(true);
					if (job == null)
					{
						Globals.Message.Show(Res.GetString("6E976496-33D8-454B-82C9-A227C8033351", "This job information is not found"));
					}
					else if (job.IsReadyForCostPosting)
					{
						Globals.Message.Show(Res.GetString("ab179ef7-e1c4-48c9-8073-ddf1cf0dd961", "You can't use Populate AP Details because Job has Ready to Post Cost status."));
					}
					else if (job.IsReadyForFinancialClosureWithoutModifySecurity)
					{
						Globals.Message.Show(Res.GetString("8129B275-D4C0-436F-8243-60D9FFAA0F8D", "You can't use Populate AP Details because Job has Ready For Financial Closure status."));
					}
					else if (ch.CostAccount != null && ch.CostAccount.CompanyData != null && ch.CostAccount.CompanyData.OB_APCostsSelfBilled)
					{
						Globals.Message.Show(Res.GetString("a43372f9-a81f-44d1-8dad-068cd27520cb", "You can't use Populate AP Details because the Creditor is flagged to receive Self Billing Invoices."));
					}
					else
					{
						job.Creditor = ch.JR_OH_CostAccount;
						job.InvoiceNum = ch.JR_APInvoiceNum;
						job.InvoiceDate = ch.JR_APInvoiceDate;
						job.DocumentReceivedDate = ch.JR_APDocumentReceivedDate;
						job.InvoiceDueDate = ch.JR_PaymentDate;
						job.LoadForAutoPopulate();
						ZFormModaliser.ShowDialogAndDispose(new JobAutoPopulationForm(job));
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("7f7187fd-d0c4-41ed-bf69-45f26a6e647f", "Please select a charge line in the above listing of charges before pressing 'Populate AP Details'."));
			}
		}

		#endregion

		#region Cash Advance

		void CashAdvanceButton_Click(object sender, EventArgs e)
		{
			if (JobChargeBoundGrid.SelectedElements.Length > 0)
			{
				if (Job.HasChanges)
				{
					Globals.Message.Show(Res.GetString("86DF4AF1-5D21-4875-AD81-5FA06872EEFD", "Job should be saved before pressing 'Advance Payment'."));
				}
				else
				{
					var mode = ODisplayMode.Undefined;
					var charge = (Charge)JobChargeBoundGrid.SelectedElements[0];
					ZForm popupForm = null;

					if (charge.IsEligibleForNewCashAdvanceRequest)
					{
						if (charge.JR_OSCostAmt < 0)
						{
							Globals.Message.Show(Res.GetString("BDA958DF-FE3E-4865-B501-DE3E7797D954", "Advance Payment cannot be requested for negative charge amounts. Please enter a positive charge amount."));
						}
						else if (charge.JR_OSCostAmt == 0 || !charge.JR_OH_CostAccount.IsValid)
						{
							Globals.Message.Show(Res.GetString("8D2BACE4-120B-49EE-8E54-4A7D596EEFD4", "Advance Payment cannot be requested until Creditor and OS Amount are specified. Please enter charge amount and Creditor code."));
						}
						else
						{
							mode = ODisplayMode.New;
							charge.LoadRelevantChargesForAPCashAdvance();
							popupForm = new APCashAdvanceNewForm(charge);
						}
					}
					else if (charge.IsCostPosted && charge.JR_CAL_APLine.IsEmpty)
					{
						Globals.Message.Show(Res.GetString("107682CD-55A2-4E44-A75F-03119CA669E4", "This charge has been posted without an AP Advance Payment request."));
					}
					else
					{
						mode = ODisplayMode.ReadOnly;
						var cashAdvanceHeader = Job.Factory.Load<CashAdvanceRequestHeader>(charge.APCashAdvanceRequestHeader.PK);
						popupForm = new APCashAdvanceViewForm(cashAdvanceHeader);
					}

					if (mode != ODisplayMode.Undefined)
					{
						var originalReadonlyValue = charge.ReadOnly;
						popupForm.DisplayMode = mode;
						if (ZFormModaliser.ShowDialogAndDispose(popupForm) == DialogResult.Yes)
						{
							RefreshCashAdvanceRequestGrid();
							UpdateCashAdvanceButtonText(charge);
						}
						charge.ReadOnly = originalReadonlyValue;
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("06813D7F-BAE4-4BF0-91D9-47CDE2279C90", "Please select a charge line in the above listing of charges before pressing 'Advance Payment'."));
			}
		}

		void RefreshCashAdvanceRequestGrid()
		{
			var plugIn = (InvoicingPluginToFreight)((ZForm)this.ParentForm)?.PlugIns?.GetPlugIn(ControllerIDs.JobInvoicing);
			plugIn?.RefreshCashAdvances();
		}

		void JobChargeBoundGrid_Click(object sender, EventArgs e)
		{
			if (JobChargeBoundGrid.SelectedElements.Length > 0)
			{
				var charge = (Charge)JobChargeBoundGrid.SelectedElements[0];
				UpdateCashAdvanceButtonText(charge);
			}
		}

		void UpdateCashAdvanceButtonText(Charge charge)
		{
			if (!charge.JR_CAL_APLine.IsEmpty || charge.IsCostPosted)
			{
				CashAdvanceButton.Text = Res.GetString("70ee7687-af59-4a9c-ad9d-95946f67ba81", "View Advance Payment");
			}
			else
			{
				CashAdvanceButton.Text = Res.GetString("e244279c-90c1-480d-9f1e-1d98a2338b7b", "New Advance Payment");
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Job != null)
				{
					Job.LocalChargeCFXChangedEvent -= new Job.LocalChargeCFXChanged(Job_LocalChargeCFXChanged);
					Job.SpotQuoteChargesExist -= new CancelEventHandler(Job_SpotQuoteChargesExist);
					Job.LocalZAddressWithContact.OnOrgChanged -= Job_LocalChargeOrgChanged;
				}

				ClientContractNumberTextBox.TextChanged -= ClientContractNumberTextBox_TextChanged;
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}

