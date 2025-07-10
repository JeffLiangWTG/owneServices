using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Germany
{
	public class TransactionBatchToGEIConverterForGermany : TransactionBatchToGEIConverter
	{
		protected override void PerformBeforeConvert(AccEInvoicingBatch batch)
		{
			base.PerformBeforeConvert(batch);
			PopulateAdditionalTransactionInfo(batch);
		}

		protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber)
		{
			return new GlobalElectronicInvoiceBuilderForGermany(batchNumber, UniversalBatch, AdditionalTransactionInfoForGermany);
		}

		protected override TransactionBatchExporter GetBatchExporter(BatchExportDataAccess dataAccess)
			=> new TransactionBatchExporter(dataAccess);

		protected AdditionalTransactionInfoForGermanyEInvoice AdditionalTransactionInfoForGermany { get; set; }

		protected virtual void PopulateAdditionalTransactionInfo(AccEInvoicingBatch batch)
		{
			AdditionalTransactionInfoForGermany = null;
			if (batch != null)
			{
				var factory = batch.Factory;
				var transaction = factory.Load<TransactionHeader>(batch.TransactionPivots[0].AIP_ParentID);
				if (transaction != null)
				{
					var bankAccount = transaction.ReceiptBankAccount;
					var orgCusCode = transaction.Header.CustomsCodes?.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Constants.CountryCodes.Germany).FirstOrDefault();
					var orgHeader = factory.Load<OrgHeader>(transaction.AH_OH);

					AdditionalTransactionInfoForGermany = new AdditionalTransactionInfoForGermanyEInvoice
					{
						OriginalTransactionPK = transaction.PK,
						OriginalTransactionNumber = transaction.AH_TransactionNum,
						VATRegistrationNum = orgCusCode?.OK_CustomsRegNo ?? ZString.Empty,
						BankName = bankAccount != null ? bankAccount.AB_BankName : ZString.Empty,
						AccountNumber = bankAccount != null ? bankAccount.AB_AccountNumber : ZString.Empty,
						SwiftNumber = bankAccount != null ? bankAccount.AB_SWIFT : ZString.Empty,
						IBANNumber = bankAccount != null ? bankAccount.IBAN : ZString.Empty,
						TransactionCategory = orgHeader.OH_Category.ToUpperInvariant(),
					};
				}
			}
		}
	}

	public class AdditionalTransactionInfoForGermanyEInvoice
	{
		public ZGuid OriginalTransactionPK { get; set; }

		public ZString OriginalTransactionNumber { get; set; }

		public ZString VATRegistrationNum { get; set; }

		public ZString BankName { get; set; }

		public ZString AccountNumber { get; set; }

		public ZString IBANNumber { get; set; }

		public ZString SwiftNumber { get; set; }

		public ZString TransactionCategory { get; set; }
	}
}
