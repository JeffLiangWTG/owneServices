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
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class InvoicePrintingModule : ZFilterGridModule
	{
		public InvoicePrintingModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.InvoicePrinting; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.InvoicePrinting);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			return Array.Empty<MenuItem>();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			return result.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			MenuItem printMenuItem = new ZMenuItem(ResString.GetMultilingualString("037F8ACD-BF5A-4708-90B0-7189E832516B", "&Print"));
			MenuItem standardInvoicePrintMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.InvoicePrinting.Print.PrintStandardInvoice", "Print Standard Invoice"), new EventHandler(PrintStandardInvoicesOnly));
			MenuItem govtTaxInvoicePrintMenuItem = new ZMenuItem(ResString.GetMultilingualString("Accounting.InvoicePrinting.Print.PrintTaxInvoice", "Print Compliance Document"), new EventHandler(PrintGovtTaxInvoicesOnly));
			MenuItem govtTaxAndStandardInvoicePrintMenuItem = new ZMenuItem(Res.GetString("4c4f83d0-4b28-4880-9b70-4cfb763c56a1", "Print Standard and Compliance Document"), new EventHandler(PrintGovtTaxAndStandardInvoice));
			MenuItem updateGovtTaxPrintMenuItem = new ZMenuItem(Res.GetString("68601fd6-d3c8-45f3-8ac7-e932725d243a", "Update Compliance Sub Type and/or Number"), new EventHandler(UpdateGovtTaxInvoice));

			bool isInPeru = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Peru;
			if ((GlbCompany.CurrentCompany.Country.SupportComplianceSubType && GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.China))
			{
				switch (AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value)
				{
					case GovtTaxInvoicePrintTask.EnterpriseInvoice:
						if (!isInPeru)
						{
							printMenuItem.MenuItems.Add(standardInvoicePrintMenuItem);
							printMenuItem.MenuItems.Add(govtTaxInvoicePrintMenuItem);
							printMenuItem.MenuItems.Add(govtTaxAndStandardInvoicePrintMenuItem);
						}
						else
						{
							printMenuItem.MenuItems.Add(govtTaxInvoicePrintMenuItem);
							printMenuItem.MenuItems.Add(standardInvoicePrintMenuItem);
						}
						printMenuItem.Click += new EventHandler(PrintStandardInvoicesOnly);
						break;
					case GovtTaxInvoicePrintTask.GovtTaxInvoice:
						printMenuItem.MenuItems.Add(govtTaxInvoicePrintMenuItem);
						printMenuItem.MenuItems.Add(standardInvoicePrintMenuItem);
						if (!isInPeru)
						{
							printMenuItem.MenuItems.Add(govtTaxAndStandardInvoicePrintMenuItem);
						}
						printMenuItem.Click += new EventHandler(PrintGovtTaxInvoicesOnly);
						break;
					case GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice:
						if (!isInPeru)
						{
							printMenuItem.MenuItems.Add(govtTaxAndStandardInvoicePrintMenuItem);
							printMenuItem.MenuItems.Add(standardInvoicePrintMenuItem);
							printMenuItem.MenuItems.Add(govtTaxInvoicePrintMenuItem);
						}
						else
						{
							printMenuItem.MenuItems.Add(govtTaxInvoicePrintMenuItem);
							printMenuItem.MenuItems.Add(standardInvoicePrintMenuItem);
						}
						printMenuItem.Click += new EventHandler(PrintGovtTaxAndStandardInvoice);
						break;
				}

				if (GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
				{
					printMenuItem.MenuItems.Add(new ZMenuItem("-"));
					printMenuItem.MenuItems.Add(updateGovtTaxPrintMenuItem);
				}
			}
			else if (GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
			{
				printMenuItem.MenuItems.Add(standardInvoicePrintMenuItem);
				printMenuItem.MenuItems.Add(updateGovtTaxPrintMenuItem);
				printMenuItem.Click += new EventHandler(PrintStandardInvoicesOnly);
			}
			else
			{
				printMenuItem.Click += new EventHandler(PrintStandardInvoicesOnly);
			}

			result.Add(printMenuItem);
			return result.ToArray();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new InvoicePrintingFilterControl(GridCollection, (InvoicePrintingFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var transactionHeaders = new TransactionHeaderCollection(Factory);
			return new FilteredTransactionHeaderCollectionView(transactionHeaders);
		}

		protected override void PushItemsIntoCollectionCore(IBusinessObjectCollection collection, PerformSearchResult searchResult, SortInfo sort)
		{
			var filteredCollection = collection as FilteredTransactionHeaderCollectionView;

			IBusinessObjectCollection originalCollection;
			if (filteredCollection == null)
			{
				originalCollection = collection;
			}
			else
			{
				originalCollection = filteredCollection.CollectionToFilter;
			}
			base.PushItemsIntoCollectionCore(originalCollection, searchResult, sort);
		}

		protected override void OnAfterPerformSearchCore()
		{
			if (GridCollection is FilteredTransactionHeaderCollectionView && ((FilteredTransactionHeaderCollectionView)GridCollection).CollectionToFilter.Count > GridCollection.Count)
			{
				Globals.Message.ShowWarning(ViewingRestrictionOutsideLoginWarningMessage, Caption);
			}
		}

		protected MultilingualString ViewingRestrictionOutsideLoginWarningMessage
		{
			get { return ResString.GetMultilingualString("A1A8FB60-02B0-4021-B831-4E09F130F183", "Transaction created in branch / dept outside your login permission are not listed."); }
		}

		protected MultilingualString Caption
		{
			get { return ResString.GetMultilingualString("58bd8f04-1515-45cd-8e1a-eae255a62a95", "Search Results"); }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new InvoicePrintingFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ReceivablesPrintInvoice; }
		}

		#region Printing

		void PrintStandardInvoicesOnly(object sender, EventArgs e)
		{
			PrintInvoicesCore(Grid.GetSelectedElements<TransactionHeader>(), GovtTaxInvoicePrintTask.EnterpriseInvoice);
		}

		void PrintGovtTaxInvoicesOnly(object sender, EventArgs e)
		{
			PrintInvoicesCore(Grid.GetSelectedElements<TransactionHeader>(), GovtTaxInvoicePrintTask.GovtTaxInvoice);
		}

		void PrintGovtTaxAndStandardInvoice(object sender, EventArgs e)
		{
			PrintInvoicesCore(Grid.GetSelectedElements<TransactionHeader>(), GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
		}

		void UpdateGovtTaxInvoice(object sender, EventArgs e)
		{
			var securityCheckpoint = Env.Security.ReceivablesModifyComplianceSubTypeOrNumber;
			if (!securityCheckpoint.IsAllowed)
			{
				securityCheckpoint.ShowError();
				return;
			}
			var rowIsSelected = Grid.ListManager.Position >= 0;

			if (rowIsSelected)
			{
				var errorMessageForARComplianceSubTypeAndNumberUpdate = ElectronicInvoicingUpdateActionPermissions.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(CurrentBusinessObjectInGrid.Cast<AccTransactionHeader>().ToArray());

				if (!errorMessageForARComplianceSubTypeAndNumberUpdate.IsEmpty)
				{
					Globals.Message.ShowError(errorMessageForARComplianceSubTypeAndNumberUpdate);
					return;
				}

				var transactionHeader = CurrentBusinessObjectInGrid as AccTransactionHeader;
				var messageForUpdateActionPermissions = ElectronicInvoicingUpdateActionPermissions.CheckComplianceSubTypeAndNumberManualUpdateToAnyValue(transactionHeader.AH_Ledger, transactionHeader.Company);

				if (!messageForUpdateActionPermissions.IsEmpty)
				{
					Globals.Message.ShowInformation(messageForUpdateActionPermissions);
					return;
				}
			}

			bool show = CheckIndonesianConstraints(CurrentBusinessObjectInGrid as InvoicingBase, true);

			if (show)
			{
				if (rowIsSelected)
				{
					ShowEditForm(CurrentBusinessObjectInGrid);
				}
				else
				{
					ShowNoSelectedMessage();
				}
			}
		}

		protected bool CheckIndonesianConstraints(InvoicingBase invoice, bool displayMessage)
		{
			string errorMessage = GovtTaxInvoicePrintTask.CheckIndonesianConstraints(invoice);
			if (errorMessage.Length != 0)
			{
				Globals.Message.ShowError(errorMessage, Res.GetString("377c9b5e-9f2f-4557-ba91-62271cf37d05", "Error"));
				return false;
			}
			else
			{
				return true;
			}
		}

		protected void PrintInvoicesCore(TransactionHeader[] invoicesToPrint, ZString invoicePrintingOptionCode)
		{
			if (invoicesToPrint.Length == 0)
			{
				string caption = Res.GetString("526982b6-85b2-4460-97a4-fc18c625b87e", "Print Invoice");
				string message = Res.GetString("7069c5ab-d415-4c24-9f1f-b823d349b2cc", "Please select an invoice or invoices to print.\r\n\r\n - You can select multiple invoices by clicking while holding down Ctrl or Shift Key.");
				Globals.Message.ShowInformation(message, caption);
			}
			else
			{
				if (invoicePrintingOptionCode == GovtTaxInvoicePrintTask.EnterpriseInvoice)
				{
					PrintInvoices(invoicesToPrint);
				}
				else
				{
					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
					{
						if (invoicesToPrint.Any(invoice => invoice.Header.UNLOCO.RL_RN_NKCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
						{
							string caption = Res.GetString("b4dd6295-ba44-4a48-95d3-b99fce948339", "Print Compliance Document");
							string message = Res.GetString("3d69615f-7337-41a4-9163-e27f303b39d2", "All the selected invoice(s) must have a debtor in the this country/region ({0}).", GlbCompany.CurrentCompany.Country.Description);
							Globals.Message.ShowInformation(message, caption);
							return;
						}
					}

					if (AccountingUtils.IsVietnamCompanyEInvoicingEnabled)
					{
						invoicesToPrint = invoicesToPrint.Where(x => (x as InvoicingBase)?.IsValidTransactionToPrintGovtTaxInvoiceInVietnam ?? false).ToArray();

						if (invoicesToPrint.Length == 0)
						{
							Globals.Message.ShowError(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToPrint);
							return;
						}

						if (invoicesToPrint.Length < SelectedBusinessObjects.Length)
						{
							Globals.Message.ShowWarning(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToPrint);
						}
					}

					PrintGovtTaxInvoices(invoicesToPrint, invoicePrintingOptionCode);
				}
			}
		}

		protected virtual void PrintInvoices(TransactionHeader[] invoicesToPrint)
		{
			var transactionsEligibleToPrint = InvoicePrintHelper.GetEligibleForPrintingTransactions(invoicesToPrint);

			if (transactionsEligibleToPrint.Any())
			{
				Print(transactionsEligibleToPrint);
			}

			void Print(IEnumerable<TransactionHeader> headers)
			{
				try
				{
					new InvoicePrintTask(new InvoicePrintTask.Configuration(headers.ToArray())).Run();
				}
				catch (UnableToFindInvoiceDocumentCommandException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		protected virtual void PrintGovtTaxInvoices(TransactionHeader[] invoicesToPrint, ZString invoicePrintingOptionCode)
		{
			new GovtTaxInvoicePrinter(invoicePrintingOptionCode).PrintGovtTaxInvoices(invoicesToPrint);
		}

		#endregion
	}
}
