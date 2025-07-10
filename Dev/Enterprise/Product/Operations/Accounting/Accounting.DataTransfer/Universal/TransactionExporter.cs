using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Export.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class TransactionExporter : AccountingTransactionExporter
	{
		public TransactionExporter(BatchExportDataAccess dataAccess)
			: base(dataAccess)
		{
		}

		public void PopulateUniversalTransaction(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, Guid transactionHeaderPK, UniversalTransaction invoice)
		{
			var batchRows = DataAccess.GetMockBatchForTransaction(transactionHeaderPK);
			var accountingBatch = GetAccountingBatchDataObject(writerStrategy, companyCode, transactionHeaderPK, invoice, batchRows, DataAccess);

			var exported = accountingBatch.TransactionCollection.FirstOrDefault();
		}

		TransactionBatch GetAccountingBatchDataObject(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, Guid transactionHeaderPK, UniversalTransaction transactionInfoToPopulate, HashSet<BatchRow> batchRows, BatchExportDataAccess dataAccess)
		{
			var accountingBatch = new TransactionBatch(writerStrategy);
			accountingBatch.DataContext = GetContextDataObject(companyCode, -1, SchemaVersionManager.Current.Namespace);

			var headers = dataAccess.GetByPKTransactionHeaderPopulatingTransactionInfo(writerStrategy, companyCode, transactionHeaderPK, transactionInfoToPopulate);
			if (headers.Count != 1 && headers.Count != 2)
			{
				throw new Exception(string.Format("Unable to load TransactionHeader with PK: {0}", transactionHeaderPK.ToString()));
			}

			var lines = dataAccess.GetTransactionLines(companyCode, -1, transactionHeaderPK);
			var relatedJournals = dataAccess.GetJournalLines(companyCode, -1, transactionHeaderPK);
			var taxTranasctionRows = DataAccess.GetTaxTransactions(writerStrategy, companyCode, -1, transactionHeaderPK);
			var (taxGLMovementRows, taxTransactionGLMovementLink) = DataAccess.GetTaxGLMovements(companyCode, -1, transactionHeaderPK);
			var (taxLinksAgainstLinePK, linePksAgainstTaxPK) = DataAccess.GetTaxTransactionLinks(companyCode, -1, taxTranasctionRows, transactionHeaderPK);

			PopulateAccountingBatch(writerStrategy, new AccountingBatchParameters {
				CompanyCode = companyCode,
				AccountingBatch = accountingBatch,
				BatchRows = batchRows,
				HeaderRowDictionary = headers,
				LineRowDictionary = lines,
				RelatedJournalsDictionary = relatedJournals,
				TaxTransactionRowDictionary = taxTranasctionRows,
				TaxGLMovementRowDictionary = taxGLMovementRows,
				TaxLinksAgainstLineDictionary = taxLinksAgainstLinePK,
				TaxTransactionGLMovementLinkDictionary = taxTransactionGLMovementLink,
				LinePKsAgainstTaxPKDictionary = linePksAgainstTaxPK
			});

			return accountingBatch;
		}
	}
}
