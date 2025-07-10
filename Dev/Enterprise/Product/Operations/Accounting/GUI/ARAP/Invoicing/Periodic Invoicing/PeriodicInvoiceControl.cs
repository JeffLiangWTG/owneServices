using System;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoiceControl : ZUserControl
	{
		public PeriodicInvoiceControl()
		{
			InitializeComponent();

			InitializeAdditionalCaptions();
			JobsGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("DA0998A7-09E6-44E9-83A4-7B90EAD7A81B", "Refresh"), new EventHandler(RefreshMenuItem_Click)));
		}

		PeriodicInvoiceBase PeriodicInvoice
		{
			get
			{
				PeriodicInvoiceBase result = (PeriodicInvoiceBase)BindingSource.Current;
				if (periodicInvoiceForDispose == null)
				{
					periodicInvoiceForDispose = result;
				}
				return result;
			}
		}
		PeriodicInvoiceBase periodicInvoiceForDispose;

		public ZArchitecture.ZGrid InternalJobsGrid
		{
			get { return JobsGrid; }
		}

		public ZCalcFindBox InternalLocalExtraTaxAmount
		{
			get { return LocalExtraTaxAmount; }
		}

		public ZCalcFindBox InternalOSExtraTaxAmount
		{
			get { return OSExtraTaxAmount; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				JobsFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				JobsGridPanel.Dock = System.Windows.Forms.DockStyle.None;
				MiscInvoicesFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				MiscInvoicesGridPanel.Dock = System.Windows.Forms.DockStyle.None;

				OSTaxAmountCalcFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("dc544e2a-3f1a-4587-8f31-9ddfa5bb2f96", "Tax Amount");
				OSExTaxAmountCalcFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("be4705a5-f5f9-4a43-9597-0cfa9444454e", "Invoice Amount Ex Tax");

				PeriodicInvoice.CurrencyNKInfo.ValueChanged += CurrencyNKInfo_ValueChanged;
				CurrencyNKInfo_ValueChanged(this, EventArgs.Empty);
				if (!PeriodicInvoice.IsValidationSuspended)
				{
					PeriodicInvoice.ValidateInvoiceDate();
					PeriodicInvoice.ValidatePostDate();
				}

				if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
				{
					JobsGrid.RemoveFromAvailableColumns(PeriodicInvoiceSelectableJob.Schema.JH_GB_TaxBranch);
					JobsGrid.RemoveFromAvailableColumns(PeriodicInvoiceSelectableJob.Schema.ChargeTaxBranches);
				}
			}
		}

		void RefreshMenuItem_Click(object sender, EventArgs e)
		{
			if (JobsGrid.SelectedElements.Length != 1)
			{
				Globals.Message.Show(Res.GetString("1B57CDC9-4DAC-45ED-9500-495F82879383", "Please select one Job to refresh."));
				return;
			}
			else
			{
				PeriodicInvoice.ReloadChargesByJob(((PeriodicInvoiceSelectableJob)JobsGrid.SelectedElements[0]).Parent.PK);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (periodicInvoiceForDispose != null && disposing && !this.IsDesignMode())
			{
				periodicInvoiceForDispose.CurrencyNKInfo.ValueChanged -= CurrencyNKInfo_ValueChanged;
			}
			base.Dispose(disposing);
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			if (JobsFilterControl != null)
			{
				JobsFilterPanel.Controls.Remove(JobsFilterControl);
				JobsFilterControl.PerformSearch -= JobsFilterControl_PerformSearch;
				JobsFilterControl.FiltersCleared -= JobsFilterControl_ClearButtonClicked;
			}

			if (MiscInvoicesFilterControl != null)
			{
				MiscInvoicesFilterPanel.Controls.Remove(MiscInvoicesFilterControl);
				MiscInvoicesFilterControl.PerformSearch -= MiscInvoicesFilterControl_PerformSearch;
				MiscInvoicesFilterControl.FiltersCleared -= MiscInvoicesFilterControl_ClearButtonClicked;
			}

			if (PeriodicInvoice != null)
			{
				JobsFilterBuisnessObject = PeriodicInvoice.JobsFilter;
				JobsFilterControl = new AccountingOnFormFilterControl(new PeriodicInvoiceSelectableJobCollection(PeriodicInvoice.Factory), JobsFilterBuisnessObject);
				JobsFilterControl.SetMaxFilterStripPanelHeight(220);
				JobsFilterControl.Size = JobsFilterPanel.ClientSize;
				JobsFilterControl.BackColor = BackColor;
				JobsFilterControl.Dock = System.Windows.Forms.DockStyle.Fill;

				JobsGridPanel.AllowOverlap(JobsFilterControl);

				JobsFilterPanel.Controls.Add(JobsFilterControl);
				JobsFilterControl.PerformSearch += JobsFilterControl_PerformSearch;
				JobsFilterControl.FiltersCleared += JobsFilterControl_ClearButtonClicked;
				JobsFilterControl.FilteredGrid.SizeChanged += JobsFilteredGrid_BoundsChanged;
				JobsFilterControl.FilteredGrid.LocationChanged += JobsFilteredGrid_BoundsChanged;
				JobsFilteredGrid_BoundsChanged(JobsFilterControl.FilteredGrid, EventArgs.Empty);

				MiscInvoicesFilterBuisnessObject = PeriodicInvoice.MiscInvoicesFilter;
				MiscInvoicesFilterControl = new AccountingOnFormFilterControl(new PeriodicInvoiceMiscInvoiceCollection(PeriodicInvoice.Factory), MiscInvoicesFilterBuisnessObject);
				MiscInvoicesFilterControl.SetMaxFilterStripPanelHeight(250);
				MiscInvoicesFilterControl.Size = MiscInvoicesFilterPanel.ClientSize;
				MiscInvoicesFilterControl.BackColor = BackColor;
				MiscInvoicesFilterControl.Dock = System.Windows.Forms.DockStyle.Fill;
				MiscInvoicesFilterPanel.Controls.Add(MiscInvoicesFilterControl);
				MiscInvoicesFilterControl.PerformSearch += MiscInvoicesFilterControl_PerformSearch;
				MiscInvoicesFilterControl.FiltersCleared += MiscInvoicesFilterControl_ClearButtonClicked;
				MiscInvoicesFilterControl.FilteredGrid.SizeChanged += MiscInvoicesFilteredGrid_BoundsChanged;
				MiscInvoicesFilterControl.FilteredGrid.LocationChanged += MiscInvoicesFilteredGrid_BoundsChanged;
				MiscInvoicesFilteredGrid_BoundsChanged(MiscInvoicesFilterControl.FilteredGrid, EventArgs.Empty);

				PeriodicInvoice.OnChangeJobs -= periodicInvoiceBase_OnChangeJobs;
				PeriodicInvoice.OnChangeMiscInvoices -= periodicInvoiceBase_OnChangeMiscInvoices;
				PeriodicInvoice.OnChangeJobs += periodicInvoiceBase_OnChangeJobs;
				PeriodicInvoice.OnChangeMiscInvoices += periodicInvoiceBase_OnChangeMiscInvoices;
			}
		}

		void JobsFilteredGrid_BoundsChanged(object sender, EventArgs e)
		{
			JobsGridPanel.Bounds = JobsFilterControl.FilteredGrid.Bounds;
		}

		void MiscInvoicesFilteredGrid_BoundsChanged(object sender, EventArgs e)
		{
			MiscInvoicesGridPanel.Bounds = MiscInvoicesFilterControl.FilteredGrid.Bounds;
		}

		void periodicInvoiceBase_OnChangeJobs(object sender, EventArgs e)
		{
			JobsInfoLabel.Text = GetJobsInfoLableText();
		}

		void periodicInvoiceBase_OnChangeMiscInvoices(object sender, EventArgs e)
		{
			MiscInvoicesInfoLabel.Text = GetMiscInvoicesInfoLableText();
		}

		protected virtual string GetJobsInfoLableText()
		{
			return Res.GetString("A9340457-7ED5-4290-83E1-2153D54E82D9", "Found {0} records that match your criteria.", PeriodicInvoice.Jobs.Count);
		}

		protected virtual string GetMiscInvoicesInfoLableText()
		{
			return Res.GetString("D7B43F12-51B8-4e7e-A87B-5AD374C19DBD", "Found {0} records that match your criteria.", PeriodicInvoice.MiscInvoices.Count);
		}

		protected void JobsFilterControl_PerformSearch(object sender, EventArgs e)
		{
			PeriodicInvoice.ValidateBeforeFindingJobs();

			if (PeriodicInvoice.HasErrors)
			{
				ShowErrorMessageToUser();
			}
			else
			{
				PeriodicInvoice.LoadJobs();
			}
		}

		protected virtual void JobsFilterControl_ClearButtonClicked(object sender, EventArgs e)
		{
			PeriodicInvoice.ClearJobs();
		}

		protected void MiscInvoicesFilterControl_PerformSearch(object sender, EventArgs e)
		{
			PeriodicInvoice.ValidateBeforeFindingMiscInvoices();

			if (PeriodicInvoice.HasErrors)
			{
				ShowErrorMessageToUser();
			}
			else
			{
				PeriodicInvoice.LoadMiscInvoices();
			}
		}

		void ShowErrorMessageToUser()
		{
			if (!Globals.IsTest)
			{
				using (var form = new ZErrorMessageBox(PeriodicInvoice, Res.GetString("a25831cc-81d5-4582-a082-b0624f7ac1d0", "search"), Res.GetString("b48329d8-e65c-4bb7-90b2-5c0710f75719", "perform"), Res.GetString("a28d06f7-7f7c-42c3-a250-0901158cdf28", "performed")))
				{
					ZFormModaliser.ShowMessageBoxWithoutDispose(form);
				}
			}
			else
			{
				Globals.Message.ShowError(String.Join(System.Environment.NewLine, PeriodicInvoice.Notifications.GetFatalNotifications().GetUniqueMessageList()));
			}
		}

		protected virtual void MiscInvoicesFilterControl_ClearButtonClicked(object sender, EventArgs e)
		{
			PeriodicInvoice.ClearMiscInvoices();
		}

		void PeriodicInvoiceControl_BackColorChanged(object sender, EventArgs e)
		{
			JobsTabPage.BackColor = BackColor;
			JobsNotificationPanel.BackColor = BackColor;
			MiscInvoicesTabPage.BackColor = BackColor;
			MiscInvoicesNotificationPanel.BackColor = BackColor;

			if (JobsFilterControl != null)
			{
				JobsFilterControl.BackColor = BackColor;
			}

			if (MiscInvoicesFilterControl != null)
			{
				MiscInvoicesFilterControl.BackColor = BackColor;
			}
		}

		void CurrencyNKInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!PeriodicInvoice.CurrencyNK.IsEmpty)
			{
				LocalExTaxAmountCalcFindBox.Visible = PeriodicInvoice.CurrencyNK != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				LocalTaxAmountCalcFindBox.Visible = LocalExTaxAmountCalcFindBox.Visible;
				LocalTotalAmountCalcFindBox.Visible = LocalExTaxAmountCalcFindBox.Visible;
				LocalExtraTaxAmount.Visible = LocalExTaxAmountCalcFindBox.Visible;
			}
		}

		public virtual void PreparePeriodicInvoiceControl()
		{
			if (!this.IsDesignMode())
			{
				if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
				{
					SetGridAndFieldsForCharges(AccountingCaptionHelper.OSExtraTaxAmountCaption, AccountingCaptionHelper.LocalExtraTaxAmountCaption);
				}
				else
				{
					this.JobsGrid.RemoveFromAvailableColumns(Job.Schema.JH_OSExtraTaxAmount, Job.Schema.JH_LocalExtraTaxAmount);
					this.ExtraTaxGroupBox.Visible = false;
				}
			}
		}

		void SetGridAndFieldsForCharges(ResourceStringData oSExtraTaxAmountCalcEditCaptionResource, ResourceStringData localExtraTaxAmountCalcEditCaptionResource)
		{
			this.LocalExtraTaxAmount.CaptionResourceString = localExtraTaxAmountCalcEditCaptionResource;
			this.ExtraTaxGroupBox.Visible = true;
			this.OSExtraTaxAmount.CaptionResourceString = oSExtraTaxAmountCalcEditCaptionResource;
		}

		void InitializeAdditionalCaptions()
		{
			if (!DesignModeFinder.IsDesigning && GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				JobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount).CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
				JobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount).CaptionResourceString = AccountingCaptionHelper.LocalExtraTaxAmountCaption;
			}
		}
	}
}

