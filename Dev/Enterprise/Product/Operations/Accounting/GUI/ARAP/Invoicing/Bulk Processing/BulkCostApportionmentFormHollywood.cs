using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class BulkCostApportionmentFormHollywood : ZForm, IButtonPostTextOverride, IButtonApplyTextOverride
	{
		public BulkCostApportionmentFormHollywood(APBulkInvoicePoster bulkInvoicePosterBizObj) : base(bulkInvoicePosterBizObj)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CancelPostingButton, PostButton);
		}

		new APBulkInvoicePoster BusinessEntity
		{
			get { return (APBulkInvoicePoster)base.BusinessEntity; }
		}

		#region Overrides

		public override string FormVerb
		{
			get { return String.Empty; }
		}

		string IButtonPostTextOverride.PostButtonText
		{
			get { return Res.GetString("BulkCostApportionmentFormHollywood|ABF208D4-A2C3-4839-9167-D313EFDFB803", "Proceed To Posting"); }
		}

		string IButtonApplyTextOverride.ApplyButtonText
		{
			get { return (this as IButtonPostTextOverride).PostButtonText; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				InvoicesGrid.GetColumnStyle(TransactionHeaderWithLines.Schema.AH_OSExtraTaxAmount).CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
			}
			else
			{
				InvoicesGrid.SetColumnVisible(false, TransactionHeaderWithLines.Schema.AH_OSExtraTaxAmount);
				InvoicesGrid.SetAvailability(false, TransactionHeaderWithLines.Schema.AH_OSExtraTaxAmount);
			}

			if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				InvoicesGrid.SetColumnVisible(false, new string[] { "TaxRate", "TaxDate", TransactionHeader.Schema.AH_OSTaxAmount });
			}

			if (!this.IsDesignMode())
			{
				RetrievedAccrualsGroupBox.Dock = System.Windows.Forms.DockStyle.None;

				FilterControl = new AccountingOnFormFilterControl(BusinessEntity.Accruals, BusinessEntity.Filters);
				FilterControl.SetMaxFilterStripPanelHeight(220);
				FilterControl.BackColor = BackColor;
				FilterControl.Dock = System.Windows.Forms.DockStyle.Fill;

				this.AccrualsTabPage.Controls.Add(FilterControl);
				this.AccrualsTabPage.Controls.SetChildIndex(FilterControl, 0);
				this.AccrualsTabPage.Controls.SetChildIndex(this.RetrievedAccrualsGroupBox, 0);
				FilterControl.PerformSearch += new EventHandler<PerformSearchEventArgs>(FilterControl_PerformSearch);
				FilterControl.FilteredGrid.SizeChanged += new EventHandler(FilterGrid_BoundsChanged);
				FilterControl.FilteredGrid.LocationChanged += new EventHandler(FilterGrid_BoundsChanged);
				FilterGrid_BoundsChanged(FilterControl.FilteredGrid, EventArgs.Empty);

				RetrievedAccrualsGroupBox.AllowOverlap(FilterControl);
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave saveResult;
			try
			{
				saveResult = base.ValidateAndSave();
				if (saveResult == ContinueWithSave.Yes)
				{
					BusinessEntity.ReadOnly = true;
					PostButton.Enabled = false;
					InvoicesGrid.Enabled = false;
					CancelPostingButton.Text = Res.GetString("2fd4ee87-67fc-41c4-8143-a35aabd3e751", "Close");
				}
				return saveResult;
			}
			catch (OnSavingCriticalCheckException)
			{
				return ContinueWithSave.Yes;
			}
			catch (APBulkInvoiceComplianceSequenceRelatedException ex)
			{
				Globals.Message.ShowError(ex.UserFriendlyMessage);
				return ContinueWithSave.No;
			}
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			HookInvoiceOnNegativeComplianceFailedToCreate();
			this.TabControl.SetReadOnlyIncludingChildren(true);
			base.Save(factories);
		}

		void HookInvoiceOnNegativeComplianceFailedToCreate()
		{
			foreach (InvoicingBase invoice in BusinessEntity.Invoices)
			{
				if (invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.CreditNote)
				{
					invoice.OnNegativeCompliancesFailedToCreate += new EventHandler(NegativeCompliancesFailedToCreate);
				}
			}
		}

		void NegativeCompliancesFailedToCreate(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning(AccountingConstants.GetComplianceDocumentNegativeMessage());
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region Implementation

		void FilterControl_PerformSearch(object sender, EventArgs e)
		{
			((System.ComponentModel.IBindingList)BusinessEntity.Accruals).RemoveSort();
			BusinessEntity.LoadAccrualCollection();
		}

		void FilterGrid_BoundsChanged(object sender, EventArgs e)
		{
			RetrievedAccrualsGroupBox.Bounds = FilterControl.FilteredGrid.Bounds;
		}

		AccountingOnFormFilterControl FilterControl;

		#endregion
	}
}
