using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.Accounting.DataTransfer.AccBatchRequets;
using Enterprise.Accounting.Export.Business;
using Enterprise.NumberFountain;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.AccBatchRequest
{
	public class AccountingTransactionEAdaptorExporter : AccountingTransactionExporter
	{
		public AccountingTransactionEAdaptorExporter(BatchExportDataAccess dataAccess)
			: base(dataAccess)
		{
		}

		#region AccBatchRequest CodeDecsriptionPair

		CodeDescriptionPairList AccBatchRequestTypes
		{
			get { return new AccBatchRequestTypePairList(); }
		}

		CodeDescriptionPair CreateBatchCodeDescriptionPair
		{
			get { return (CodeDescriptionPair)AccBatchRequestTypes[AccBatchRequestTypePairList.Codes.Create, StringComparison.OrdinalIgnoreCase]; }
		}

		CodeDescriptionPair ExportBatchCodeDescriptionPair
		{
			get { return (CodeDescriptionPair)AccBatchRequestTypes[AccBatchRequestTypePairList.Codes.Export, StringComparison.OrdinalIgnoreCase]; }
		}

		CodeDescriptionPair CreateAndExportCodeDescriptionPair
		{
			get { return (CodeDescriptionPair)AccBatchRequestTypes[AccBatchRequestTypePairList.Codes.CreateAndExport, StringComparison.OrdinalIgnoreCase]; }
		}

		#endregion

		public class BatchResponse
		{
			public string ErrorMessage;
			public string InfoMessage;
			public long CreatedBatchNumber;
			public bool CanContinue = true;
			public IDataObject DataObject;
		}

		public BatchResponse CreateBatch(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, string nameSpace)
		{
			var result = CreateBatchCore(companyCode);
			if (result.CanContinue)
			{
				var transactionBatch = CreateAndInitialiseAccountingBatchDataObject(writerStrategy, companyCode, result.CreatedBatchNumber, nameSpace, CreateBatchCodeDescriptionPair);
				result.DataObject = transactionBatch;
				result.InfoMessage = Res.GetString("6b0b8c99-0782-4100-b6ce-ee41b3274bc1", "New batch {0} is created", result.CreatedBatchNumber);
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is database generated message")]
		BatchResponse CreateBatchCore(string companyCode)
		{
			// Preload resource strings before starting new SqlTransaction. See CS01824750.
			var unableToLoadCompany = Res.GetString("6076AF9F-BEF0-4E62-9295-21529A1C4E94", "Unable to load company: {0}", companyCode);
			var nothingToBeBatched = Res.GetString("7753a6b9-52fa-4065-a216-17dfbf7492e5", "Nothing to be batched.");
			var anotherProcessRunning = Res.GetString("a1a4cc44-7977-4850-a767-7646a2054983", "Another process is already exporting a batch for company {0}.  You can only create one export batch for each company at a time.  Please try again later.", companyCode);
			var unexpectedError = Res.GetString("683C43D2-9E6F-4dbb-93BE-C791D0A7F081", "Unexpected error during the batch export.");

			var result = new BatchResponse();
			var company = DataAccess.LoadCompany(companyCode);
			if (company == null)
			{
				result.ErrorMessage = unableToLoadCompany;
				result.CanContinue = false;
				return result;
			}

			var transaction = GetTransaction();
			try
			{
				var batchCreateDate = GetUtcNow(transaction);
				var batchRows = DataAccess.GetPotentialBatch(companyCode, transaction);

				if (batchRows.Any())
				{
					var numberFountains = new NumberFountains();
					result.CreatedBatchNumber = numberFountains.AccountingExportWebServiceBatchNo(company.PK.ToGuid()).GetNext(transaction.Connection, transaction);

					DataAccess.SaveBatch(batchRows.ToList(), result.CreatedBatchNumber, transaction);
				}
				else
				{
					result.InfoMessage = nothingToBeBatched;
					result.CanContinue = false;
				}

				DataAccess.SetAccountingTransactionExportServiceHighWaterMark(batchCreateDate.Subtract(HighWaterMarkBuffer), transaction, company.PK.ToGuid());
				CommitTransaction(transaction);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.CanContinue = false;
				if (ex is DbException sqlEx && new SqlExceptionWrapper(sqlEx).Number == 50000 && ex.Message.StartsWith("Could not obtain lock - process is already running", StringComparison.OrdinalIgnoreCase))
				{
					result.ErrorMessage = anotherProcessRunning;
				}
				else
				{
					RollBackTransaction(transaction);
					result.ErrorMessage = unexpectedError + System.Environment.NewLine + GetErrorMessage(ex);
				}
			}
			finally
			{
				DisposeTransaction(transaction);
			}

			return result;
		}

		public BatchResponse ExportBatch(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, int batchNumber, string nameSpace)
		{
			var result = ExportBatchCore(writerStrategy, companyCode, batchNumber, nameSpace, ExportBatchCodeDescriptionPair);
			if (result.CanContinue)
			{
				result.InfoMessage = Res.GetString("1d644a9c-ce10-4230-9028-1e8418f521ab", "Batch {0} is exported", batchNumber);
			}
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		BatchResponse ExportBatchCore(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, long batchNumber, string nameSpace, ICodeDescription batchType)
		{
			var result = new BatchResponse();
			result.CreatedBatchNumber = batchType.Code == AccBatchRequestTypePairList.Codes.CreateAndExport ? batchNumber : 0;

			var batchRows = DataAccess.GetBatch(companyCode, batchNumber);
			if (batchRows.Any())
			{
				try
				{
					var transactionBatch = CreateAndInitialiseAccountingBatchDataObject(writerStrategy, companyCode, batchNumber, nameSpace, batchType);
					PopulateAccountingBatch(writerStrategy, companyCode, batchNumber, transactionBatch, batchRows);
					result.DataObject = transactionBatch;
				}
				catch (DataObjectValidationException ex)
				{
					result.ErrorMessage = ex.Message;
					result.CanContinue = false;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var unexpectedError = Res.GetString("8AF91A9B-BE67-48E3-ACFC-DBA859F44FEE", "Unexpected error during the batch export.");
					result.ErrorMessage = unexpectedError + System.Environment.NewLine + GetErrorMessage(ex);
					result.CanContinue = false;
				}
			}
			else
			{
				result.InfoMessage = Res.GetString("5a191c58-8a34-4afc-b3ef-a7fd0e5af9bc", "Nothing to export.");
				result.CanContinue = false;
			}

			return result;
		}

		public BatchResponse CreateAndExportBatch(AccountingTransactionDataObjectWriterStrategy writerStrategy, string companyCode, string nameSpace)
		{
			var result = CreateBatchCore(companyCode);
			if (result.CanContinue)
			{
				result = ExportBatchCore(writerStrategy, companyCode, result.CreatedBatchNumber, nameSpace, CreateAndExportCodeDescriptionPair);
				if (result.CanContinue)
				{
					result.InfoMessage = Res.GetString("f42052c9-6f30-49b6-bf68-1ce909d2e9e7", "New batch {0} is created and exported", result.CreatedBatchNumber);
				}
			}

			return result;
		}

		protected virtual DateTime GetUtcNow(DbTransaction transaction)
		{
			return DataAccess.GetUtcNow(transaction);
		}

		protected virtual DbTransaction GetTransaction()
		{
			return DataAccess.Connection.BeginTransaction();
		}

		protected virtual void CommitTransaction(DbTransaction transaction)
		{
			transaction.Commit();
		}

		[SuppressMessage("CargoWiseOne", "CW1019:NoSqlTransactionRollbackRule")]
		protected virtual void RollBackTransaction(DbTransaction transaction)
		{
			if (transaction.Connection == null)
			{
				return;
			}
			transaction.Rollback();
		}

		protected virtual void DisposeTransaction(DbTransaction transaction)
		{
			transaction.Dispose();
		}

		readonly TimeSpan HighWaterMarkBuffer = new TimeSpan(48, 0, 0);

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string in web accounting")]
		string GetErrorMessage(Exception ex)
		{
			var builder = new StringBuilder();
			builder.AppendLine("Exception Type:");
			builder.AppendLine(ex.GetType().ToString());
			builder.AppendLine("Exception Message:");
			builder.AppendLine(ex.Message);

			builder.AppendLine("Exception Stack Trace:");
			builder.AppendLine(ex.StackTrace);

			if (ex.InnerException != null)
			{
				GetInnerException(ex.InnerException, builder);
			}
			return builder.ToString();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string in web accounting")]
		void GetInnerException(Exception ex , StringBuilder builder)
		{
			builder.AppendLine("Inner Exception Type:");
			builder.AppendLine(ex.GetType().ToString());
			builder.AppendLine("Inner Exception Message:");
			builder.AppendLine(ex.Message);
			builder.AppendLine("Inner Exception Stack Trace:");
			builder.AppendLine(ex.StackTrace);

			if (ex.InnerException != null)
			{
				GetInnerException(ex.InnerException, builder);
			}
		}
	}
}
