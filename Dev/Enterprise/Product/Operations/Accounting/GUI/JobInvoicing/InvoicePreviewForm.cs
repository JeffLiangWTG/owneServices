using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class InvoicePreviewForm : ZChildForm
	{
		public InvoicePreviewForm(InvoicesPreviewer invoicesPreviewer, bool showPreviewAndDeliverButton = true, bool isPreviewConsolJob = false)
			: base(invoicesPreviewer)
		{
			InitializeComponent();
			this.InvoicesPreviewer = invoicesPreviewer;
			AddOrHideColumnsInInvoicesGrid(invoicesPreviewer.PostingOption, isPreviewConsolJob);
			HideAndRepositionPreviewAndDeliveryButton(showPreviewAndDeliverButton);

			InvoicesGrid.ContextMenu.MenuItems.Add("-");

			KMenuItem menuItemPreviewAndDeliverInvoice = new ZMenuItem(ResString.GetMultilingualString("InvoicePreviewForm|MenuItemPreviewAndDeliverInvoice", "Preview and Deliver Invoice"), delegate { Run(false); });
			InvoicesGrid.ContextMenu.MenuItems.Add(menuItemPreviewAndDeliverInvoice);
			KMenuItem menuItemPreviewOnlyInvoice = new ZMenuItem(ResString.GetMultilingualString("InvoicePreviewForm|MenuItemPreviewOnlyInvoice", "Preview Only"), delegate { Run(true); });
			InvoicesGrid.ContextMenu.MenuItems.Add(menuItemPreviewOnlyInvoice);

			InvoicesGrid.ContextMenu.MenuItems.Add("-");

			foreach (InvoicingBase postedInvoice in invoicesPreviewer.PreviewInvoices)
			{
				PreviewInvoiceIsNotSavedByFactoryServiceProvider.Register(postedInvoice.Factory);
				postedInvoice.IsInPreviewingInvoicesContext = true;
				postedInvoice.RunPreSaveValidation();
			}
		}

		readonly InvoicesPreviewer InvoicesPreviewer;

		void AddOrHideColumnsInInvoicesGrid(JobInvoicingPostingOption option, bool isPreviewConsolJob)
		{
			if (option == JobInvoicingPostingOption.Costs || option == JobInvoicingPostingOption.ConsolCosts || (isPreviewConsolJob && option == JobInvoicingPostingOption.All))
			{
				this.InvoicesGrid.ColumnStyles.Remove(InvoicesGrid.GetColumnStyle(AccTransactionHeaderSchema.AH_TransactionCategory.Name));
			}
			else
			{
				this.InvoicesGrid.ColumnStyles.Remove(InvoicesGrid.GetColumnStyle(AccTransactionHeaderSchema.AH_TransactionNum.Name));
				this.InvoicesGrid.ColumnStyles.Remove(InvoicesGrid.GetColumnStyle(AccTransactionHeaderSchema.AH_InvoiceDate.Name));
				this.InvoicesGrid.ColumnStyles.Remove(InvoicesGrid.GetColumnStyle(AccTransactionHeaderSchema.AH_DueDate.Name));
			}
		}

		void HideAndRepositionPreviewAndDeliveryButton(bool showPreviewAndDeliverButton)
		{
			PreviewAndDeliverButton.Visible = showPreviewAndDeliverButton;
			if (!showPreviewAndDeliverButton)
			{
				this.PreviewOnlyButton.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(PreviewAndDeliverButton.Location.X),
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(PreviewAndDeliverButton.Location.Y));
			}
		}

		internal AllowedDeliveryOptions DeliveryOptions
		{
			get;
			set;
		}

		internal bool FormCannotBeClosedUntilProcessingCompleted
		{
			get;
			set;
		}

		#region Implementation

		void Run(bool isPreviewOnly)
		{
			try
			{
				FormCannotBeClosedUntilProcessingCompleted = true;
				var deliveryOptions = isPreviewOnly ? AllowedDeliveryOptions.PreviewOnly : DeliveryOptions;
				var disallowDeliverWhilePreviewing = isPreviewOnly && AllowedToPreviewInvoice && !AllowedToDeliverInvoice;

				if (InvoicesGrid.SelectedElements.Length > 0)
				{
					foreach (InvoicingBase invoice in InvoicesGrid.SelectedElements)
					{
						try
						{
							if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
							{
								using (InvoicePrintTask task = GetNewInvoicePrintTask(invoice))
								{
									// Workaround to focus stealing issue - disable events while preview form is being prepared.
									ZFormModaliser.EnableForm(this, false);
									try
									{
										if (disallowDeliverWhilePreviewing)
										{
											task.RunDraftInvoiceWithDeliveryOptions(AllowedDeliveryOptions.PreviewOnly);
										}
										else
										{
											task.RunDraftInvoiceWithDeliveryOptions(deliveryOptions);
										}
									}
									finally
									{
										ZFormModaliser.EnableForm(this, true);
										BringToFront();
										Focus();
									}
#if DEBUG
									if (!task.isProFormaInvoice)
									{
										throw new ApplicationException("InvoicePreviewForm should only be used for ProForma InvoicePrintTask");
									}
#endif
								}
							}
							else
							{
								using (InvoicePrintTask task = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice) { JobParent = invoice.Job?.Parent, IsProFormaInvoice = true, MenuNames = [JobInvoicingEDocsProviderSupporter.CostConfirmationDocument] }))
								{
									// Workaround to focus stealing issue - disable events while preview form is being prepared.
									ZFormModaliser.EnableForm(this, false);
									try
									{
										task.RunDraftInvoiceWithDeliveryOptions(deliveryOptions);
									}
									finally
									{
										ZFormModaliser.EnableForm(this, true);
									}
#if DEBUG
									if (!task.isProFormaInvoice)
									{
										throw new ApplicationException("InvoicePreviewForm should only be used for ProForma InvoicePrintTask");
									}
#endif
								}
							}
						}
						catch (InvoiceIsDeletedException)
						{
							Globals.Message.Show(Res.GetString("c8f87f15-c709-4556-9fb2-b5c135421ac6", "An error has occurred. Please close the Document Form before the Invoice Preview Form."));
						}
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("f4b59bbd-3b86-432b-a6b3-2e9e9579a582", "Please select at least one invoice to preview/deliver."));
				}
			}
			finally
			{
				FormCannotBeClosedUntilProcessingCompleted = false;
			}
		}

		protected virtual InvoicePrintTask GetNewInvoicePrintTask(InvoicingBase invoice)
		{
			return new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice) { JobParent = invoice.Job?.Parent, IsProFormaInvoice = true });
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (FormCannotBeClosedUntilProcessingCompleted)
			{
				e.Cancel = true;
				Globals.Message.Show(Res.GetString("b5ef637c-81dc-4eec-a8ce-c4c4b6b1c69d", "This form cannot be closed until invoice processing is complete."));
				return;
			}

			foreach (InvoicingBase postedInvoice in InvoicesPreviewer.PreviewInvoices)
			{
				PreviewInvoiceIsNotSavedByFactoryServiceProvider.Deregister(postedInvoice.Factory);
			}

			base.OnFormClosing(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			InvoicesPreviewer.Dispose();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void PreviewAndDeliverButton_Click(object sender, EventArgs e)
		{
			if (!AllowedToDeliverInvoice && SecurityHelper != null)
			{
				SecurityHelper.ShowError(SecurityCore.PreviewAndDeliver);
			}
			else
			{
				Run(false);
			}
		}

		void PreviewOnlyButton_Click(object sender, EventArgs e)
		{
			if (!AllowedToPreviewInvoice && SecurityHelper != null)
			{
				SecurityHelper.ShowError(SecurityCore.PreviewOnly);
			}
			else
			{
				Run(true);
			}
		}

		bool AllowedToDeliverInvoice => (SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PreviewAndDeliver) ?? true);
		bool AllowedToPreviewInvoice => (SecurityHelper?.CheckIsAllowedForSecurityCheckPoint(SecurityCore.PreviewOnly) ?? true);

		JobInvoicingSecurityHelper SecurityHelper => ((InvoicesPreviewer)BusinessEntity).SecurityHelper;

		#endregion

		#region Dispose

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

		#endregion
	}
}

