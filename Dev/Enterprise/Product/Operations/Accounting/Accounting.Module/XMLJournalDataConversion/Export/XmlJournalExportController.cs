using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class XMLJournalExportController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.XmlJournalExport; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return null; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override IZForm ShowNewForm()
		{
			AccGLHeader apJornalClearingAccount = Factory.Load<AccGLHeader>((Guid)AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AccGLHeader arJornalClearingAccount = Factory.Load<AccGLHeader>((Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			if (apJornalClearingAccount == null || arJornalClearingAccount == null)
			{
				Globals.Message.ShowError(Res.GetString("42571AA2-F8BA-41d4-BD06-9AB891A84EE8",
@"This action is prevented because a Registry configuration is missing.
Please modify your Registry setup.
Please define a General Ledger Link Accounts for the AP and AR Journal Clearing Account registry."));
			}
			else
			{
				var exporter = new XmlDataTransferExporter(new FinancialInvoiceAsJournalDataAdapter(), true);
				var loader = new InvoiceLoader(Factory);
				var transactions = loader.GetOutstandingARAPTransactions(
					ZArchitecture.Core.TransactionTypes.Invoice,
					ZArchitecture.Core.TransactionTypes.CreditNote,
					ZArchitecture.Core.TransactionTypes.AdjustmentNote,
					ZArchitecture.Core.TransactionTypes.Journal,
					ZArchitecture.Core.TransactionTypes.Contra,
					ZArchitecture.Core.TransactionTypes.Transfer,
					ZArchitecture.Core.TransactionTypes.Receipt,
					ZArchitecture.Core.TransactionTypes.Payment);
				if (transactions.Count > 0)
				{
					exporter.PromptUserAndExport(transactions.ToArray());
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("1912471d-5796-4025-a8ec-96713d28ee33", "There are no outstanding transactions to export."));
				}
			}
			return null;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ExportOutstandingJournals; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}
	}
}
