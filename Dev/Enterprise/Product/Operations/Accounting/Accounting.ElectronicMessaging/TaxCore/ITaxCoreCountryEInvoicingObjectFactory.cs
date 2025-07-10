using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public interface ITaxCoreCountryEInvoicingObjectFactory
	{
		ZString CountryCode { get; }
		ZString InvoicingSystemName { get; }
		ZString TaxFileCode { get; }
		ZString TFNCode { get; }
		ITaxCoreEInvoiceCreator GetInvoiceCreator();
		IAdditionalTransactionInfoForTaxCoreEInvoice GetAdditionalInfoFromTransactionHeader(TransactionHeader transaction);
	}

	public abstract class TaxCoreCountryEInvoicingObjectFactory : ITaxCoreCountryEInvoicingObjectFactory
	{
		ZString ITaxCoreCountryEInvoicingObjectFactory.CountryCode => CountryCode;
		ZString ITaxCoreCountryEInvoicingObjectFactory.InvoicingSystemName => InvoicingSystemName;
		ZString ITaxCoreCountryEInvoicingObjectFactory.TaxFileCode => TaxFileCode;
		ZString ITaxCoreCountryEInvoicingObjectFactory.TFNCode => TFNCode;
		ITaxCoreEInvoiceCreator ITaxCoreCountryEInvoicingObjectFactory.GetInvoiceCreator() => new TaxCoreEInvoiceCreator();
		IAdditionalTransactionInfoForTaxCoreEInvoice ITaxCoreCountryEInvoicingObjectFactory.GetAdditionalInfoFromTransactionHeader(TransactionHeader transaction)
		{
			Argument.NotNull(transaction, nameof(transaction));
			IAdditionalTransactionInfoForTaxCoreEInvoice additionalTransactionInfo = null;

			if (transaction.AH_TransactionType == TransactionTypes.CreditNote)
			{
				additionalTransactionInfo = new AdditionalTransactionInfoTaxCoreEInvoice();

				if (transaction.AH_TransactionBelongsToGroup.IsValid)
				{
					var originalTransaction = transaction.Factory.Load<TransactionHeader>(transaction.AH_TransactionBelongsToGroup);
					if (originalTransaction != null)
					{
						ZDateTime originalTransactionCreationDate;
						if (!originalTransaction.Branch.TryConvertUTCDateTimeToBranchLocalDateTime(originalTransaction.AH_SystemCreateTimeUtc, out originalTransactionCreationDate))
						{
							originalTransactionCreationDate = originalTransaction.AH_SystemCreateTimeUtc;
						}

						if (originalTransaction.IsPreEInvoicingTransaction())
						{
							additionalTransactionInfo.OriginalTransactionGovtReferenceNumber = ElectronicInvoicingHelper.TaxCorePreComplianceOriginalTransactionReferenceNumber;
							additionalTransactionInfo.OriginalTransactionCreationDate = originalTransactionCreationDate;
						}
						else
						{
							additionalTransactionInfo.OriginalTransactionGovtReferenceNumber = originalTransaction.EInvoicingGovernmentAllocatedNumber;
							additionalTransactionInfo.OriginalTransactionCreationDate = originalTransactionCreationDate;
						}
					}
				}
			}

			return additionalTransactionInfo;
		}

		protected abstract ZString CountryCode { get; }
		protected abstract ZString InvoicingSystemName { get; }
		protected virtual ZString TaxFileCode => OrgCusCode.CodeTypes.TaxFileCode;
		protected virtual ZString TFNCode => CertificateTypePairList.Codes.TFN;
	}
}
