#define CODE_ANALYSIS

using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Accounting.Export.Business;
using Enterprise.NumberFountain;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Business
{
	public class AccountingTransactionWebExporter : AccountingTransactionExporter
	{
		public AccountingTransactionWebExporter(BatchExportDataAccess dataAccess)
			: base(dataAccess)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1019:NoSqlTransactionRollbackRule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant strings")]
		public AccountingTransactionExportResponse CreateBatch(string companyCode)
		{
			var result = new AccountingTransactionExportResponse();
			result.Succeeded = false;
			var company = DataAccess.LoadCompany(companyCode);
			if (company == null)
			{
				result.ErrorMessage = string.Format("Unable to load company: {0}", companyCode);
				return result;
			}

			System.Data.Common.DbTransaction transaction = null;
			IDisposable disposeAction = null;
			try
			{
				var transactionInfo = CreateTransactionInfo(DataAccess);
				transaction = transactionInfo.Transcation;
				disposeAction = transactionInfo.DisposeAction;
				var batchCreateDate = GetUtcNow(transaction);
				var batchRows = DataAccess.GetPotentialBatch(companyCode, transaction);

				if (batchRows.Count > 0)
				{
					var numberFountains = new NumberFountains();
					result.BatchNumber = numberFountains.AccountingExportWebServiceBatchNo(company.PK.ToGuid()).GetNext(transaction.Connection, transaction);

					DataAccess.SaveBatch(batchRows.ToList(), result.BatchNumber, transaction);
					result.Succeeded = true;
				}
				else
				{
					result.ErrorMessage = "Nothing to be batched.";
				}

				DataAccess.SetAccountingTransactionExportServiceHighWaterMark(batchCreateDate.Subtract(HighWaterMarkBuffer), transaction, company.PK.ToGuid());
				CommitTransaction(transaction);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.Succeeded = false;
				if (ex is SqlException sqlEx && sqlEx.Number == 50000 && ex.Message.StartsWith("Could not obtain lock - process is already running"))
				{
					result.ErrorMessage = string.Format("Another process is already exporting a batch for company {0}.  You can only create one export batch for each company at a time.  Please try again later.", companyCode);
				}
				else
				{
					throw;
				}
			}
			finally
			{
				disposeAction?.Dispose();
			}

			return result;
		}

		protected virtual (System.Data.Common.DbTransaction Transcation, IDisposable DisposeAction) CreateTransactionInfo(BatchExportDataAccess dataAccess)
		{
			var transaction = dataAccess.Connection.BeginTransaction();
			var action = new DisposableAction(() => transaction.Dispose());
			return (transaction, action);
		}

		protected virtual void CommitTransaction(System.Data.Common.DbTransaction transaction)
		{
			transaction.Commit();
		}

		public AccountingTransactionExportResponse ExportBatch(string companyCode, long batchNumber, string nameSpace)
		{
			AccountingTransactionExportResponse result = new AccountingTransactionExportResponse();
			result.BatchNumber = batchNumber;
			var batchRows = DataAccess.GetBatch(companyCode, batchNumber);

			if (batchRows.Count > 0)
			{
				var strategy = new AccountingTransactionDataObjectWriterStrategy(context: "AccountingWebService");
				var accountingBatch = CreateAndInitialiseAccountingBatchDataObject(strategy, companyCode, batchNumber, nameSpace);
				PopulateAccountingBatch(strategy, companyCode, batchNumber, accountingBatch, batchRows);

				XmlWriter xmlWriter = new XmlWriter();
				using (var outputStream = (SubStreamableStream)new MemoryStream())
				{
					xmlWriter.WriteXML(accountingBatch, outputStream, nameSpace);
					using (StreamReader streamReader = new StreamReader(outputStream))
					{
						result.PayLoad = streamReader.ReadToEnd();
					}
				}
				result.Succeeded = true;
			}
			else
			{
				result.ErrorMessage = (NoResString)"Nothing to export.";
				result.Succeeded = false;
			}
			return result;
		}

		protected virtual DateTime GetUtcNow(System.Data.Common.DbTransaction transaction)
		{
			return DataAccess.GetUtcNow(transaction);
		}

		readonly TimeSpan HighWaterMarkBuffer = new TimeSpan(48, 0, 0);
	}
}
