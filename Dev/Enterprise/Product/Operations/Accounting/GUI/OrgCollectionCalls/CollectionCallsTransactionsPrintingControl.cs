using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.OrgCollectionCalls;
using Enterprise.Accounting.GUI.ARAP.Statements;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.OrgCollectionCalls
{
	public partial class CollectionCallsTransactionsPrintingControl : ZUserControl
	{
		public CollectionCallsTransactionsPrintingControl()
		{
			InitializeComponent();

			MenuItem viewMenuItem = new ZMenuItem(ViewText, new EventHandler(ViewInvoices));
			InvoicesGrid.ContextMenu.MenuItems.Add(0, viewMenuItem);

			MenuItem printMenuItem = new ZMenuItem(PrintText, new EventHandler(PrintInvoices));
			InvoicesGrid.ContextMenu.MenuItems.Add(1, printMenuItem);
			int spacerMenuItemPosition = 2;

			if (!DesignModeFinder.IsDesigning)
			{
				if ((GlbCompany.CurrentCompany.Country.SupportComplianceSubType && GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.China))
				{
					MenuItem printGovtTaxInvoiceMenuItem = new ZMenuItem(PrintGovtTaxInvoiceText, new EventHandler(PrintClassAInvoices));
					InvoicesGrid.ContextMenu.MenuItems.Add(2, printGovtTaxInvoiceMenuItem);
					spacerMenuItemPosition++;
				}

				if (GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
				{
					MenuItem updateGovtTaxMenuItem = new ZMenuItem(UpdateGovtTaxInvoiceNumberText, new EventHandler(UpdateClassAInvoices));
					InvoicesGrid.ContextMenu.MenuItems.Add(3, updateGovtTaxMenuItem);
					spacerMenuItemPosition++;
				}
			}

			MenuItem spacerMenuItem = new ZMenuItem("-");
			InvoicesGrid.ContextMenu.MenuItems.Add(spacerMenuItemPosition, spacerMenuItem);

			AH_NumberTextBox.AllowOverlap(IsDisbursementCheckBox);
			TotalOutstandingLabel.AllowOverlap(DisbursementOutstandingCalcEdit);
			TotalOverTwoTermsPastDueCalcEdit.AllowOverlap(DisbursementOverTwoTermsPastDueCalcEdit);
			TotalTwoTermsPastDueCalcEdit.AllowOverlap(DisbursementTwoTermsPastDueCalcEdit);
			TotalOneTermPastDueCalcEdit.AllowOverlap(DisbursementOneTermPastDueCalcEdit);
			TotalDueTodayCalcEdit.AllowOverlap(DisbursementDueTodayCalcEdit);
			TotalNotYetDueCalcEdit.AllowOverlap(DisbursementNotYetDueCalcEdit);
			DisbursementOutstandingCalcEdit.AllowOverlap(StandardOutstandingCalcEdit);
			DisbursementOverTwoTermsPastDueCalcEdit.AllowOverlap(StandardOverTwoTermsPastDueCalcEdit);
			DisbursementTwoTermsPastDueCalcEdit.AllowOverlap(StandardTwoTermsPastDueCalcEdit);
			DisbursementOneTermPastDueCalcEdit.AllowOverlap(StandardOneTermPastDueCalcEdit);
			DisbursementDueTodayCalcEdit.AllowOverlap(StandardDueTodayCalcEdit);
			DisbursementNotYetDueCalcEdit.AllowOverlap(StandardNotYetDueCalcEdit);
		}

		CollectionNotesTransactionsFilter CollectionNotesTransactionsFilter
		{
			get { return (CollectionNotesTransactionsFilter)CurrentDataItem; }
		}

		#region Event Handlers

		void ViewInvoices(object sender, EventArgs e)
		{
			if (InvoicesGrid.SelectedElements != null && InvoicesGrid.SelectedElements.Length == 1)
			{
				TransactionHeader selectedHeader = (TransactionHeader)InvoicesGrid.SelectedElements[0];

				if (TypesThatCanBeViewed.Contains(selectedHeader.AH_TransactionType))
				{
					GetControllerIDAndShowViewForm(selectedHeader);
				}
			}
		}

		List<ZString> TypesThatCanBeViewed
		{
			get
			{
				List<ZString> types = new List<ZString>();
				types.Add(ZArchitecture.Core.TransactionTypes.Invoice);
				types.Add(ZArchitecture.Core.TransactionTypes.CreditNote);
				types.Add(ZArchitecture.Core.TransactionTypes.AdjustmentNote);
				types.Add(ZArchitecture.Core.TransactionTypes.Journal);
				types.Add(ZArchitecture.Core.TransactionTypes.Receipt);
				types.Add(ZArchitecture.Core.TransactionTypes.Payment);
				types.Add(ZArchitecture.Core.TransactionTypes.Transfer);
				types.Add(ZArchitecture.Core.TransactionTypes.Overpayment);
				types.Add(ZArchitecture.Core.TransactionTypes.ExchangeDifference);
				types.Add(ZArchitecture.Core.TransactionTypes.Discount);
				types.Add(ZArchitecture.Core.TransactionTypes.Contra);
				return types;
			}
		}

		void PrintInvoices(object sender, EventArgs e)
		{
			ZString message = Res.GetString("17decdb5-bb50-465c-88ba-dd93e8828a4c", "Please select an invoice or invoices before printing.\r\n\r\n - You can select multiple invoices by clicking while holding down Ctrl or Shift Key.");
			ZString caption = Res.GetString("875e9441-6f31-4e5f-9c74-87ad459276d0", "Select an Invoice");

			if (CheckSelectedTransactionsOnInvoices(SecurityCore.PrintInvoice, message, caption))
			{
				var transactionsEligibleToPrint = InvoicePrintHelper.GetEligibleForPrintingTransactions(InvoicesGrid.GetSelectedElements<TransactionHeader>().ToArray());

				if (transactionsEligibleToPrint.Any())
				{
					Print(transactionsEligibleToPrint);
				}
			}

			void Print(IEnumerable<TransactionHeader> transactions)
			{
				InvoicePrintTask printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(transactions.ToArray()));
				printTask.SetOneBusinessObjectToLogAgainstForAllPrintTasks(((AccTransactionHeader)InvoicesGrid.SelectedElements[0]).Header.CompanyData);
				printTask.Run();
			}
		}

		void PrintClassAInvoices(object sender, EventArgs e)
		{
			string caption = Res.GetString("Accounting|CollectionCallsTransactionPrintingControl|PrintGovtTaxInvoice", "Print Govt Tax Invoice");
			string message = Res.GetString("e092bffe-3151-476f-a5a7-059e659b63f4", "Please select an invoice or invoices to print.\r\n\r\n - You can select multiple invoices by clicking while holding down Ctrl or Shift Key.");
			if (CheckSelectedTransactionsOnInvoices(SecurityCore.PrintGovtTax, message, caption))
			{
				TransactionHeader[] invoicesToPrint = InvoicesGrid.SelectedElements.Where(x => x is TransactionHeader).Cast<TransactionHeader>().ToArray();
				if (AccountingUtils.IsVietnamCompanyEInvoicingEnabled)
				{
					invoicesToPrint = invoicesToPrint.Where(x => (x as InvoicingBase).IsValidTransactionToPrintGovtTaxInvoiceInVietnam).ToArray();

					if (invoicesToPrint.Length == 0)
					{
						Globals.Message.ShowError(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint);
						return;
					}

					if (invoicesToPrint.Length < InvoicesGrid.SelectedElements.Length)
					{
						Globals.Message.ShowWarning(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint);
					}
				}

				if (invoicesToPrint.Where(invoice => invoice.Header != null && invoice.Header.UNLOCO != null).Any(invoice => invoice.Header.UNLOCO.RL_RN_NKCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					caption = Res.GetString("Accounting|CollectionCallsTransactionPrintingControl|PrintGovtTaxInvoice", "Print Govt Tax Invoice");
					message = Res.GetString("504ae3f9-c76e-409e-af2e-e9d14e45a931", "All the selected invoice(s) must have an A/R Client in the this country/region ({0}).", GlbCompany.CurrentCompany.Country.Description);
					Globals.Message.ShowInformation(message, caption);
					return;
				}

				new GovtTaxInvoicePrinter().PrintGovtTaxInvoices(invoicesToPrint);
			}
		}

		void UpdateClassAInvoices(object sender, EventArgs e)
		{
			string caption = Res.GetString("01ad9ad2-4661-4bf8-8dcf-55c97eb9d314", "Update Govt Tax Invoice");
			string message = Res.GetString("b2a509c2-864c-4ae3-88be-619fbaa7b4d8", "Please select an invoice or invoices to update.\r\n\r\n - You can select multiple invoices by clicking while holding down Ctrl or Shift Key.");
			if (CheckSelectedTransactionsOnInvoices(SecurityCore.UpdateGovtTax, message, caption))
			{
				BusinessObject[] invoicesToUpdate = InvoicesGrid.SelectedElements;
				UpdateGovtTaxInvoices(invoicesToUpdate);
			}
		}

		void UpdateGovtTaxInvoices(BusinessObject[] invoicesToUpdate)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
			{
				foreach (BusinessObject bizO in invoicesToUpdate)
				{
					if (bizO is AccTransactionHeader && ((AccTransactionHeader)bizO).Header != null && ((AccTransactionHeader)bizO).Header.UNLOCO != null)
					{
						if (((AccTransactionHeader)bizO).Header.UNLOCO.RL_RN_NKCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
						{
							string caption = Res.GetString("01ad9ad2-4661-4bf8-8dcf-55c97eb9d314", "Update Govt Tax Invoice");
							string message = Res.GetString("d0b9bdb4-890d-4e2c-af54-ee2b36c43eaa", "All the selected invoice(s) must have a A/R Client in the this country/region ({0}).", GlbCompany.CurrentCompany.Country.Description);
							Globals.Message.ShowInformation(message, caption);
							return;
						}
					}
				}

				if (invoicesToUpdate != null && invoicesToUpdate.Length >= 1)
				{
					AccTransactionHeader initialInvoice = invoicesToUpdate[0] as AccTransactionHeader;
					ZController classAInvController = ZControllerFactory.Create(ControllerIDs.InvoicePrinting);
					classAInvController.ShowEditForm(initialInvoice);
				}
			}
		}

		void FindButton_Click(object sender, EventArgs e)
		{
			CollectionNotesTransactionsFilter.RefreshInvoiceList();
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			CollectionNotesTransactionsFilter.ResetInvoiceList();
		}

		void PrintStatement_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new StatementPrintForm(CollectionNotesTransactionsFilter.StatementObject));
		}

		void GetControllerIDAndShowViewForm(TransactionHeader selectedHeader)
		{
			ControllerID controllerForHeader = new TransactionHeaderControllerIDLookup().GetControllerID(selectedHeader);

			if (controllerForHeader != null)
			{
				ZController controller = ZControllerFactory.Create(controllerForHeader);

				if (controller != null)
				{
					controller.ShowViewForm(selectedHeader);
				}
			}
		}

		#endregion

		#region Security

		public virtual SecurityCheckpoint PluginSecurity
		{
			get { return fPluginSecurity; }
			set { fPluginSecurity = value; }
		}
		SecurityCheckpoint fPluginSecurity;

		//SecurityCheckpoint GetInvSecurity(string Name)
		//{
		//    return Env.Security.GetInvoicingSecurityCheckPoint(PluginSecurity, Name);
		//}

		JobInvoicingSecurityHelper SecurityHelper
		{
			get
			{
				if (fSecurityHelper == null)
				{
					fSecurityHelper = new JobInvoicingSecurityHelper(PluginSecurity);
				}
				return fSecurityHelper;
			}
		}
		JobInvoicingSecurityHelper fSecurityHelper;

		#endregion

		#region Implementation

		internal static MultilingualString ViewText { get { return ResString.GetMultilingualString("Accounting|CollectionCallsTransactionPrintingControl|View", "View"); } }
		internal static MultilingualString PrintText { get { return ResString.GetMultilingualString("Accounting|CollectionCallsTransactionPrintingControl|Print", "Print"); } }
		internal static MultilingualString PrintGovtTaxInvoiceText { get { return ResString.GetMultilingualString("Accounting|CollectionCallsTransactionPrintingControl|PrintGovtTaxInvoice", "Print Govt Tax Invoice"); } }
		internal static MultilingualString UpdateGovtTaxInvoiceNumberText { get { return ResString.GetMultilingualString("Accounting|CollectionCallsTransactionPrintingControl|UpdateGovtTaxInvoiceNumber", "Update Govt Tax Invoice Number"); } }

		ZBool CheckSelectedTransactionsOnInvoices(ZString checkpointName, ZString message, ZString caption)
		{
			if (!SecurityHelper.CheckIsAllowedForSecurityCheckPoint(checkpointName))
			{
				SecurityHelper.ShowError(checkpointName);
			}
			else
			{
				BusinessObject[] selectedTransactions = InvoicesGrid.SelectedElements;

				if (selectedTransactions.Length > 0)
				{
					foreach (BusinessObject transaction in selectedTransactions)
					{
						if ((transaction as InvoicingBase) != null)
						{
							continue;
						}

						ZString errMsg = Res.GetString("67DC32B7-3601-42CA-99FA-CC4DDA701E66",
										"Please select valid transaction or transactions before printing. \r\n\r\n The valid transaction types are INV, CRD and ADJ.");
						Globals.Message.ShowInformation(errMsg, caption);
						return false;
					}
					return ZBool.True;
				}
				Globals.Message.ShowInformation(message, caption);
			}
			return false;
		}

		#endregion

		#region IDisposable Members

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
		void termsTabPage_InitializeTab(object sender, EventArgs e)
		{
			var termsUserControl = new MasterFiles.GUI.Organisation.UserControls.Receivables.TermsUserControl(true);
			termsUserControl.Dock = DockStyle.Top;
			ZControlExtensions.SetReadOnlyIncludingChildren(termsUserControl);
			termsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			termsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 306, true);
			termsUserControl.Name = "termsUserControl";
			termsUserControl.TabStop = false;
			termsUserControl.AllowOutsideOfParent();

			this.BindingSource.SetBindingMember(termsUserControl, "CompanyData");
			this.termsPanel.Controls.Add(termsUserControl);
			this.termsPanel.ResumeLayout(true);
		}
	}
}

