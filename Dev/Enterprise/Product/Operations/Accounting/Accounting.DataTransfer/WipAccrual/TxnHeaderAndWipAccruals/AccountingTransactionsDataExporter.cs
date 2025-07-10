using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.WipsAndAccruals;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public abstract class AccountingTransactionsDataExporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected AccountingTransactionsDataExporter(BusinessObjectFactory factory)
			: base(factory)
		{
			this.Notify = new NotificationBuffer(new NotificationBuffer());
			FactoryProvider = new BusinessObjectFactoryProvider();
		}

		public event EventHandler ProcessingProgressed;

		public void Export(Stream streamToNewFile)
		{
			var isDisposed = false;
			try
			{
				var poke = streamToNewFile.Length;
			}
			catch (ObjectDisposedException)
			{
				isDisposed = true;
			}

			if (!isDisposed)
			{
				InitialiseDocumentWriter(streamToNewFile);
				ResetNumberOfTransactionsExported();
				ResetFiltersPKCache();
				LastBatchNumberOfTransactions = NumberOfTransactionsInBatch;

				try
				{
					if (FilterProvider.CurrentBatchNo == 0)
					{
						CreateNewBatch();
					}

					if (AllowExportWhithoutBatchNumber || FilterProvider.CurrentBatchNo != 0)
					{
						BeforeDocumentBuild();

						try
						{
							WriteObjectsInChunks(AccTransactionHeaderSchema.PK, InvoiceBatchFilterPks, InvoiceBatchFilter, ZString.Empty);
							WriteObjectsInChunks(AccTransactionLinesSchema.PK, WIPAccPostBatchFilterPks, WIPAccrualPostBatchFilter, nameof(Xsd.WipOrAccrualPostOrReverse.P));
							WriteObjectsInChunks(AccTransactionLinesSchema.PK, WIPAccReverseBatchFilterPks, WIPAccrualReverseBatchFilter, nameof(Xsd.WipOrAccrualPostOrReverse.R));
							WriteObjectsInChunks(AccTransactionHeaderSchema.PK, UnallocatedTransactionBatchFilterPks, UnallocatedTransactionFilter, ZString.Empty);
						}
						finally
						{
							ResetFiltersPKCache();
						}

						AfterDocumentBuild();
					}
					else
					{
						BeforeDocumentBuild();
						AfterDocumentBuild();
					}
				}
				finally
				{
					DeInitialiseDocumentWriter();
					fNumberOfTransactionsProcessed = 0;
				}
			}
		}

		internal int WriteObjectsChunkSize
		{
			get { return writeObjectsChunkSize; }
		}
		int writeObjectsChunkSize = 1000;

#if DEBUG
		protected void SetWriteObjectsChunkSizeForTestOnly(int value)
		{
			writeObjectsChunkSize = value;
		}
#endif

		protected void WriteObjectsInChunks(SchemaColumn schemaColumn, List<ZGuid> filterPks, TransactionExportFilter filter, ZString status)
		{
			int quotient = (int)Math.Ceiling((double)filterPks.Count / WriteObjectsChunkSize);
			for (int i = 0; i < quotient; i++)
			{
				WriteObjects(new FilteredBusinessObjectReader(FactoryProvider, new ZQuery(schemaColumn, filterPks.Skip(i * WriteObjectsChunkSize).Take(WriteObjectsChunkSize)), filter.BusinessObjectType), status);
			}
		}

		protected void WriteObjects(FilteredBusinessObjectReader reader, ZString status)
		{
			foreach (BusinessObject bizObj in reader)
			{
				ExportObjectsToEndPoint(bizObj, DecideAdapterByBusinessObjectType(bizObj), status);
				fNumberOfTransactionsProcessed++;
				if (ProcessingProgressed != null)
				{
					ProcessingProgressed(this, EventArgs.Empty);
				}
			}
			Document.Flush();
		}

		protected IValueObject CreateNewValueObject(IValueObjectDataAdapter dataAdapter)
		{
			return (IValueObject)dataAdapter.ValueObjectType.GetConstructor(Type.EmptyTypes).Invoke(null);
		}

		protected virtual void BeforeDocumentBuild()
		{
		}

		protected virtual void AfterDocumentBuild()
		{
		}

		protected abstract void ExportObjectsToEndPoint(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, ZString status);
		protected abstract void InitialiseDocumentWriter(Stream docStream);
		protected virtual void DeInitialiseDocumentWriter()
		{
			Document.Close();
		}

		protected BusinessObject LoadCorrectTypeOfBusinessObject(IValueObjectDataAdapter dataAdapter, BusinessObject bizObj, ZString status)
		{
			BusinessObject result = bizObj;

			if (bizObj is BaseWIPAccrual)
			{
				Xsd.WipOrAccrualPostOrReverse postOrReverseStatus;
				if (status == nameof(Xsd.WipOrAccrualPostOrReverse.P))
				{
					postOrReverseStatus = Xsd.WipOrAccrualPostOrReverse.P;
				}
				else
				{
					postOrReverseStatus = Xsd.WipOrAccrualPostOrReverse.R;
				}

				result = new WIPAccrualPRBusinessObject((BaseWIPAccrual)result, postOrReverseStatus);
			}
			else if (!dataAdapter.BusinessObjectType.IsAssignableFrom(bizObj.GetType()))
			{
				result = FactoryProvider.Current.Load(dataAdapter.BusinessObjectType, bizObj.PK);
			}
			return result;
		}

		#region Implementation

		#region New Batch Creation

#if DEBUG
		internal
#endif
 protected virtual bool CreateNewBatch()
		{
			using (var mutex = new ZGlobalMutex(MutexIDs.BatchXMLExportNumber, Env.CurrentCompany.PK.ToString()))
			{
				if (!mutex.Lock())
				{
					HandleUnableToLockMutex(mutex);
				}
				else
				{
					var batchCreateDate = ZDateTime.UtcNow;
					var saveHighWaterMark = FilterProvider.CanSaveHighWaterMark;
					try
					{
						FilterProvider.CurrentBatchNo =
							new ExportBatchNumberAllocator(this).GetBatchNumberAndAllocateToTransactionsToBeExported();
					}
					catch (System.Data.Common.DbException)
					{
						throw;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						FilterProvider.CurrentBatchNo = EmptyBatchNumber;
					}

					if (saveHighWaterMark)
					{
						FilterProvider.SaveHighWaterMarkRegistry(batchCreateDate);
					}
				}
			}

			return !FilterProvider.CurrentBatchNo.IsEmpty;
		}

		void HandleUnableToLockMutex(ZGlobalMutex mutex)
		{
#if DEBUG
			if (ReleaseLock_ForTest != null)
			{
				ReleaseLock_ForTest.Invoke();
			}
#endif

			var lockInfo = mutex.GetLockInfo();
			var who = (lockInfo != null && lockInfo.UserWithLock != null) ? lockInfo.UserWithLock.GS_FullName.ToString() : Res.GetString("ee80f118-93e7-4e06-a6f5-e551935aa407", "*unknown user*");
			var when = lockInfo != null ? lockInfo.LockStartTime.ToDateTime().ToLocalTime().ToLongTimeString() : Res.GetString("6f97a0c6-754b-41a7-955f-b4061243dc9e", "*unknown time*");

			BatchExportResult += Res.GetString("e9e2e099-449c-4f84-8e23-2097dc9e540b", "Batch Exporter update has been canceled because a Batch Exporter update is currently being run by user '{0}' since {1}", who, when);
			fFailedToAquireMutexReason = Res.GetString("7bab41f7-8dc3-42a2-891a-e0645b95886e", "Batch Exporter update is currently being run by user '{0}' since {1}", who, when);
			fFailedToAquireMutex = true;
		}

#if DEBUG
		internal Action ReleaseLock_ForTest;
#endif

		protected string fBatchExportResult;

		bool fFailedToAquireMutex;
		string fFailedToAquireMutexReason;

		public bool FailedToAquireMutex
		{
			get { return fFailedToAquireMutex; }
		}

		public string FailedToAquireMutexReason
		{
			get { return fFailedToAquireMutexReason; }
		}

		public string BatchExportResult
		{
			get { return fBatchExportResult; }

			set { fBatchExportResult = value; }
		}

		protected virtual bool AllowExportWhithoutBatchNumber
		{
			get { return false; }
		}

		#endregion

		#region DecideAdapterByBusinessObjectType

		protected virtual IValueObjectDataAdapter DecideAdapterByBusinessObjectType(BusinessObject bizObj)
		{
			IValueObjectDataAdapter adapter = null;

			if (bizObj is TransactionPendingAllocation)
			{
				adapter = new UnallocatedTransactionDataAdapter();
			}
			else if (bizObj is BaseWIPAccrual)
			{
				adapter = new WIPAndAccrualDataAdapter();
			}
			else if (bizObj is InvoicingBase)
			{
				adapter = new ExportFinancialInvoiceDataAdapter();
			}
			else
			{
				throw new NotSupportedException("Could not load DataAdapter for BizObj Type ( " + bizObj.GetType().FullName + ")");
			}

			return adapter;
		}

		#endregion

		#region Count Transactions Being Exported

		#region Count Exported Transactions

		protected void IncreaseStandardTransactionsProcessedCount(BusinessObject bizObj)
		{
			if (bizObj != null)
			{
				if (typeof(TransactionPendingAllocation).IsAssignableFrom(bizObj.GetType()))
				{
					fNumberOfUnallocatedTransactionsProcessed_Standard++;
				}
				else if (typeof(InvoicingBase).IsAssignableFrom(bizObj.GetType()))
				{
					InvoicingBase header = (InvoicingBase)bizObj;
					IncreaseStandardHeaderProcessedCount(header);
				}
				else if (bizObj.GetType() == typeof(WIPAccrualPRBusinessObject))
				{
					WIPAccrualPRBusinessObject wipAccrualBizObj = (WIPAccrualPRBusinessObject)bizObj;
					IncreaseStandardLineProcessedCount(wipAccrualBizObj);
				}
			}
		}

		void IncreaseStandardHeaderProcessedCount(InvoicingBase header)
		{
			switch (header.AH_TransactionType)
			{
				case ZArchitecture.Core.TransactionTypes.Invoice:
					if (header.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
					{
						fNumberOfAPInvoicesProcessed_Standard++;
					}
					else if (header.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
					{
						fNumberOfARInvoicesProcessed_Standard++;
					}
					break;
				case ZArchitecture.Core.TransactionTypes.CreditNote:
					if (header.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
					{
						fNumberOfAPCreditNotesProcessed_Standard++;
					}
					else if (header.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
					{
						fNumberOfARCreditNotesProcessed_Standard++;
					}
					break;
				case ZArchitecture.Core.TransactionTypes.AdjustmentNote:
					if (header.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
					{
						fNumberOfAPAdjustmentNotesProcessed_Standard++;
					}
					else if (header.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
					{
						fNumberOfARAdjustmentNotesProcessed_Standard++;
					}
					break;
			}
		}

		void IncreaseStandardLineProcessedCount(WIPAccrualPRBusinessObject wipAccrualBizObj)
		{
			if (wipAccrualBizObj.WIP.AL_LineType == ZArchitecture.Core.TransactionLineTypes.WIP)
			{
				if (wipAccrualBizObj.PostedOrReverseStatus == Xsd.WipOrAccrualPostOrReverse.P)
				{
					fNumberOfWipPostingProcessed_Standard++;
				}
				else if (wipAccrualBizObj.PostedOrReverseStatus == Xsd.WipOrAccrualPostOrReverse.R)
				{
					fNumberOfWipReversingProcessed_Standard++;
				}
			}
			else
			{
				if (wipAccrualBizObj.PostedOrReverseStatus == Xsd.WipOrAccrualPostOrReverse.P)
				{
					fNumberOfAccrualPostingProcessed_Standard++;
				}
				else if (wipAccrualBizObj.PostedOrReverseStatus == Xsd.WipOrAccrualPostOrReverse.R)
				{
					fNumberOfAccrualReversingProcessed_Standard++;
				}
			}
		}

		public ZString GetMessageToDisplayWhenExportIsFinished()
		{
			return GetMessageToDisplayWhenExportIsFinished("");
		}

		public ZString GetMessageToDisplayWhenExportIsFinished(ZString message)
		{
			string doubleNewLine = System.Environment.NewLine + System.Environment.NewLine;
			if (!message.IsEmpty)
			{
				message += doubleNewLine;
			}

			if (FailedToAquireMutex)
			{
				message += FailedToAquireMutexReason;
			}
			else if (LastBatchNumberOfTransactions == 0 || FilterProvider.CurrentBatchNo == 0 && !AllowExportWhithoutBatchNumber)
			{
				message += "\r\n" + Res.GetString("cdab2a52-faeb-471b-9567-e33307e79df1", "There are no transactions to export.") + "\r\n";
			}
			else if (ErrorHasOccured)
			{
				message += Res.GetString("a18244fa-2932-4f53-97c0-1546f2f912f2", "BATCH ERROR: There were problems exporting Batch {0}.", FilterProvider.CurrentBatchNo) + "\r\n";
				message += Res.GetString("5aa262f0-e320-4568-9c85-ca2306bdd90a", "BATCH ERROR: Please do not use the files exported and contact support.{0}", doubleNewLine);
			}
			else if (FilterProvider.CurrentBatchNo != 0 || AllowExportWhithoutBatchNumber)
			{
				message += Res.GetString("1892f30c-8d49-4b08-8a3a-1ee76c5b3c83", "Batch {0} was exported successfully.{1}", FilterProvider.CurrentBatchNo, doubleNewLine);
			}

			ZStringBuilder errorStringBuilder = new ZStringBuilder();

			if (FilterProvider.CurrentBatchNo != 0 && LastBatchNumberOfTransactions != 0)
			{
				GetMessageToDisplayWhenExportIsFinished_Core(errorStringBuilder);
			}

			return message + errorStringBuilder.ToStringWithNewLineBetweenAppends();
		}

		protected virtual void GetMessageToDisplayWhenExportIsFinished_Core(ZStringBuilder errorStringBuilder)
		{
			ReportDifferences(errorStringBuilder, Res.GetString("5077c3da-f1d7-4ac2-8492-45cb0999a4db", "Invoices"), NumberOfInvoicesInBatch, NumberOfInvoicesProcessed_Standard);
			ReportDifferences(errorStringBuilder, Res.GetString("1ceef468-0bfb-48d2-9f69-1cfd9bf6f6e8", "Credit Notes"), NumberOfCreditNotesInBatch, NumberOfCreditNotesProcessed_Standard);
			ReportDifferences(errorStringBuilder, Res.GetString("7646c12e-d5ce-4c04-8f2e-35db664630c3", "Adjustment Notes"), NumberOfAdjustmentNotesInBatch, NumberOfAdjustmentNotesProcessed_Standard);
			ReportDifferences(errorStringBuilder, Res.GetString("e2152c17-c3ef-4ec2-9244-c711482ef7f1", "WIP Posting"), NumberOfWipPostingsInBatch, NumberOfWipPostingProcessed_Standard);
			ReportDifferences(errorStringBuilder, Res.GetString("ac8c279b-d040-4f24-8aea-8b816652a404", "WIP Reversing"), NumberOfWipReversalsInBatch, NumberOfWipReversingProcessed_Standard);
			ReportDifferences(errorStringBuilder, Res.GetString("ed5f24c7-56eb-4a23-af6f-5849aed952db", "Accrual Posting"), NumberOfAccrualPostingsInBatch, NumberOfAccrualPostingProcessed_Standard);
			ReportDifferences(errorStringBuilder, Res.GetString("78b5c9d2-567d-4ccc-abf5-f50097cbe7e2", "Accrual Reversing"), NumberOfAccrualReversalsInBatch, NumberOfAccrualReversingProcessed_Standard);
			ReportDifferences(errorStringBuilder, Res.GetString("2c021753-85d9-4026-81c2-0edacb780b03", "Unallocated Transactions"), NumberOfUnallocatedTransactionsInBatch, NumberOfUnallocatedTransactionsProcessed_Standard);
		}

		protected void ReportDifferences(ZStringBuilder errorStringBuilder, ZString description,
			ZInt transactionsInBatch, ZInt transactionsInStandardExport)
		{
			ZString result;
			ZBool exportIsSuccessful = IsExportSuccessful(transactionsInBatch, transactionsInStandardExport);
			result = GetDifferences(exportIsSuccessful, description, transactionsInBatch, transactionsInStandardExport);
			errorStringBuilder.Append(result);
		}

		protected virtual ZString GetDifferences(ZBool exportIsSuccessful, ZString description,
			ZInt transactionsInBatch, ZInt transactionsInStandardExport)
		{
			ZString result = ZString.Empty;
			result = (exportIsSuccessful) ? MessageWhenSuccessfulExport : MessageWhenProblemWithExport;
			result += description + System.Environment.NewLine;
			result += " " + Res.GetString("791c501e-2f44-49c0-822d-b41e58028434", "- Transactions in Batch: {0}", transactionsInBatch) + "\r\n";
			result += " " + Res.GetString("e09bc056-8b95-48ce-a804-bd07fd30be11", "- Exported to Standard Format: {0}", transactionsInStandardExport) + "\r\n";
			return result;
		}

		protected virtual bool IsExportSuccessful(ZInt transactionsInBatch, ZInt transactionsInStandardExport)
		{
			return transactionsInBatch == transactionsInStandardExport;
		}

		public ZBool ErrorHasOccured
		{
			get { return ErrorHasOccuredCore; }
		}

		protected virtual ZBool ErrorHasOccuredCore
		{
			get
			{
				return !(IsExportSuccessful(NumberOfInvoicesInBatch, NumberOfInvoicesProcessed_Standard) &&
					IsExportSuccessful(NumberOfCreditNotesInBatch, NumberOfCreditNotesProcessed_Standard) &&
					IsExportSuccessful(NumberOfAdjustmentNotesInBatch, NumberOfAdjustmentNotesProcessed_Standard) &&
					IsExportSuccessful(NumberOfWipPostingsInBatch, NumberOfWipPostingProcessed_Standard) &&
					IsExportSuccessful(NumberOfWipReversalsInBatch, NumberOfWipReversingProcessed_Standard) &&
					IsExportSuccessful(NumberOfAccrualPostingsInBatch, NumberOfAccrualPostingProcessed_Standard) &&
					IsExportSuccessful(NumberOfAccrualReversalsInBatch, NumberOfAccrualReversingProcessed_Standard));
			}
		}

		protected static string MessageWhenSuccessfulExport
		{
			get { return Res.GetString("8c62ae80-228b-434c-8a7c-1fc42b24f903", "The following transactions were exported successfully:") + " "; }
		}
		protected static string MessageWhenProblemWithExport
		{
			get { return Res.GetString("ebd517c5-e996-4c53-84d1-8c524546f85c", "ERROR: There were problems exporting the following transactions:") + " "; }
		}

		#endregion

		#region Number of Transactions in Standard Export

		protected virtual void ResetNumberOfTransactionsExported()
		{
			fNumberOfARInvoicesProcessed_Standard = 0;
			fNumberOfARCreditNotesProcessed_Standard = 0;
			fNumberOfARAdjustmentNotesProcessed_Standard = 0;
			fNumberOfAPInvoicesProcessed_Standard = 0;
			fNumberOfAPCreditNotesProcessed_Standard = 0;
			fNumberOfAPAdjustmentNotesProcessed_Standard = 0;
			fNumberOfWipPostingProcessed_Standard = 0;
			fNumberOfWipReversingProcessed_Standard = 0;
			fNumberOfAccrualPostingProcessed_Standard = 0;
			fNumberOfAccrualReversingProcessed_Standard = 0;
			fNumberOfUnallocatedTransactionsProcessed_Standard = 0;
		}

		public ZInt NumberOfInvoicesProcessed_Standard
		{
			get { return fNumberOfAPInvoicesProcessed_Standard + fNumberOfARInvoicesProcessed_Standard; }
		}

		public ZInt NumberOfAPInvoicesProcessed_Standard
		{
			get { return fNumberOfAPInvoicesProcessed_Standard; }
		}
		ZInt fNumberOfAPInvoicesProcessed_Standard;

		public ZInt NumberOfARInvoicesProcessed_Standard
		{
			get { return fNumberOfARInvoicesProcessed_Standard; }
		}
		ZInt fNumberOfARInvoicesProcessed_Standard;

		public ZInt NumberOfCreditNotesProcessed_Standard
		{
			get { return fNumberOfAPCreditNotesProcessed_Standard + fNumberOfARCreditNotesProcessed_Standard; }
		}

		public ZInt NumberOfAPCreditNotesProcessed_Standard
		{
			get { return fNumberOfAPCreditNotesProcessed_Standard; }
		}
		ZInt fNumberOfAPCreditNotesProcessed_Standard;

		public ZInt NumberOfARCreditNotesProcessed_Standard
		{
			get { return fNumberOfARCreditNotesProcessed_Standard; }
		}
		ZInt fNumberOfARCreditNotesProcessed_Standard;

		public ZInt NumberOfAdjustmentNotesProcessed_Standard
		{
			get { return fNumberOfAPAdjustmentNotesProcessed_Standard + fNumberOfARAdjustmentNotesProcessed_Standard; }
		}

		public ZInt NumberOfAPAdjustmentNotesProcessed_Standard
		{
			get { return fNumberOfAPAdjustmentNotesProcessed_Standard; }
		}
		ZInt fNumberOfAPAdjustmentNotesProcessed_Standard;

		public ZInt NumberOfARAdjustmentNotesProcessed_Standard
		{
			get { return fNumberOfARAdjustmentNotesProcessed_Standard; }
		}
		ZInt fNumberOfARAdjustmentNotesProcessed_Standard;

		public ZInt NumberOfWipPostingProcessed_Standard
		{
			get { return fNumberOfWipPostingProcessed_Standard; }
		}
		ZInt fNumberOfWipPostingProcessed_Standard;

		public ZInt NumberOfUnallocatedTransactionsProcessed_Standard
		{
			get { return fNumberOfUnallocatedTransactionsProcessed_Standard; }
		}
		ZInt fNumberOfUnallocatedTransactionsProcessed_Standard;

		public ZInt NumberOfWipReversingProcessed_Standard
		{
			get { return fNumberOfWipReversingProcessed_Standard; }
		}
		ZInt fNumberOfWipReversingProcessed_Standard;

		public ZInt NumberOfAccrualPostingProcessed_Standard
		{
			get { return fNumberOfAccrualPostingProcessed_Standard; }
		}
		ZInt fNumberOfAccrualPostingProcessed_Standard;

		public ZInt NumberOfAccrualReversingProcessed_Standard
		{
			get { return fNumberOfAccrualReversingProcessed_Standard; }
		}
		ZInt fNumberOfAccrualReversingProcessed_Standard;

		#endregion

		#region Number of Transactions in Batch

		public ZInt NumberOfInvoicesInBatch
		{
			get
			{
				int count = 0;

				if (FilterProvider.IncludeAPInvoices)
				{
					ZQuery filter = new ZQuery();
					StringCollectionX invoiceTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.Invoice });
					filter.DefaultJoinCondition = JoinCondition.Or;
					filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ZArchitecture.Core.LedgerTypes.AccountsPayable, invoiceTranType));
					count += FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
				}

				if (FilterProvider.IncludeARInvoices)
				{
					ZQuery filter = new ZQuery();
					StringCollectionX invoiceTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.Invoice });
					filter.DefaultJoinCondition = JoinCondition.Or;
					filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ZArchitecture.Core.LedgerTypes.AccountsReceivable, invoiceTranType));
					count += FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
				}
				return count;
			}
		}

		public ZInt NumberOfLedgerSpecificInvoicesInBatch(string ledgerType)
		{
			ZQuery filter = new ZQuery();
			StringCollectionX invoiceTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.Invoice });
			filter.DefaultJoinCondition = JoinCondition.Or;
			filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ledgerType, invoiceTranType));
			return FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
		}

		public ZInt NumberOfCreditNotesInBatch
		{
			get
			{
				int count = 0;

				if (FilterProvider.IncludeAPCreditNotes)
				{
					ZQuery filter = new ZQuery();
					StringCollectionX creditNoteTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.CreditNote });
					filter.DefaultJoinCondition = JoinCondition.Or;
					filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ZArchitecture.Core.LedgerTypes.AccountsPayable, creditNoteTranType));
					count += FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
				}

				if (FilterProvider.IncludeARCreditNotes)
				{
					ZQuery filter = new ZQuery();
					StringCollectionX creditNoteTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.CreditNote });
					filter.DefaultJoinCondition = JoinCondition.Or;
					filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ZArchitecture.Core.LedgerTypes.AccountsReceivable, creditNoteTranType));
					count += FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
				}

				return count;
			}
		}

		public ZInt NumberOfLedgerSpecificCreditNotesInBatch(string ledgerType)
		{
			ZQuery filter = new ZQuery();
			StringCollectionX creditNoteTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.CreditNote });
			filter.DefaultJoinCondition = JoinCondition.Or;
			filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ledgerType, creditNoteTranType));
			return FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
		}

		public ZInt NumberOfAdjustmentNotesInBatch
		{
			get
			{
				int count = 0;

				if (FilterProvider.IncludeAPAdjustmentNotes)
				{
					ZQuery filter = new ZQuery();
					StringCollectionX adjustmentNoteTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.AdjustmentNote });
					filter.DefaultJoinCondition = JoinCondition.Or;
					filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ZArchitecture.Core.LedgerTypes.AccountsPayable, adjustmentNoteTranType));
					count += FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
				}

				if (FilterProvider.IncludeARAdjustmentNotes)
				{
					ZQuery filter = new ZQuery();
					StringCollectionX adjustmentNoteTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.AdjustmentNote });
					filter.DefaultJoinCondition = JoinCondition.Or;
					filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ZArchitecture.Core.LedgerTypes.AccountsReceivable, adjustmentNoteTranType));
					count += FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
				}

				return count;
			}
		}

		public ZInt NumberOfLedgerSpecificAdjustmentNotesInBatch(string ledgerType)
		{
			ZQuery filter = new ZQuery();
			StringCollectionX adjustmentNoteTranType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionTypes.AdjustmentNote });
			filter.DefaultJoinCondition = JoinCondition.Or;
			filter.AddToFilter(InvoiceBatchFilter.CreateNewQuery(ledgerType, adjustmentNoteTranType));
			return FactoryProvider.Current.GetDatabaseCount(typeof(InvoicingBase), filter);
		}

		public virtual ZInt NumberOfWipPostingsInBatch
		{
			get
			{
				int count = 0;
				if (FilterProvider.IncludeWIPsPosting)
				{
					StringCollectionX wIPLineType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionLineTypes.WIP });
					ZQuery wipPostingFilter = WIPAccrualPostBatchFilter.CreateQueryForWIPAccruals(wIPLineType);
					count += FactoryProvider.Current.GetDatabaseCount(typeof(BaseWIPAccrual), wipPostingFilter);
				}
				return count;
			}
		}

		public virtual ZInt NumberOfWipReversalsInBatch
		{
			get
			{
				int count = 0;
				if (FilterProvider.IncludeWIPsReversing)
				{
					StringCollectionX wIPLineType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionLineTypes.WIP });
					ZQuery wipReversingFilter = WIPAccrualReverseBatchFilter.CreateQueryForWIPAccruals(wIPLineType);
					count += FactoryProvider.Current.GetDatabaseCount(typeof(BaseWIPAccrual), wipReversingFilter);
				}
				return count;
			}
		}

		public virtual ZInt NumberOfAccrualPostingsInBatch
		{
			get
			{
				int count = 0;
				if (FilterProvider.IncludeAccrualsPosting)
				{
					StringCollectionX accrualLineType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionLineTypes.Accrual });
					ZQuery accrualPostingFilter = WIPAccrualPostBatchFilter.CreateQueryForWIPAccruals(accrualLineType);
					count += FactoryProvider.Current.GetDatabaseCount(typeof(BaseWIPAccrual), accrualPostingFilter);
				}
				return count;
			}
		}

		public virtual ZInt NumberOfAccrualReversalsInBatch
		{
			get
			{
				int count = 0;
				if (FilterProvider.IncludeAccrualsReversing)
				{
					StringCollectionX accrualLineType = new StringCollectionX(new string[] { ZArchitecture.Core.TransactionLineTypes.Accrual });
					ZQuery accrualReversingFilter = WIPAccrualReverseBatchFilter.CreateQueryForWIPAccruals(accrualLineType);
					count += FactoryProvider.Current.GetDatabaseCount(typeof(BaseWIPAccrual), accrualReversingFilter);
				}
				return count;
			}
		}

		public virtual ZInt NumberOfUnallocatedTransactionsInBatch
		{
			get
			{
				ZQuery unallocatedBatchCountFilter = UnallocatedTransactionFilter.Filter;
				return FactoryProvider.Current.GetDatabaseCount(typeof(TransactionPendingAllocation), unallocatedBatchCountFilter);
			}
		}

		#endregion

		#region Total Number Of Transactions In Batch

#if DEBUG
		internal
#endif
 protected ZInt NumberOfTransactionsInBatch
		{
			get
			{
				return InvoiceBatchFilterPks.Count + UnallocatedTransactionBatchFilterPks.Count + WIPAccPostBatchFilterPks.Count + WIPAccReverseBatchFilterPks.Count;
			}
		}

		public bool IsTransactionsExistInBatch
		{
			get
			{
				return InvoiceBatchFilter.GetFilterPks(true).Count + UnallocatedTransactionFilter.GetFilterPks(true).Count + WIPAccrualPostBatchFilter.GetFilterPks(false).Count + WIPAccrualReverseBatchFilter.GetFilterPks(true).Count > 0;
			}
		}

		public ZInt LastBatchNumberOfTransactions
		{
			get;
			private set;
		}

		void ResetFiltersPKCache()
		{
			batchInvoicePKs = null;
			unAllocatedTranBatchPKs = null;
			wipAccPostBatchPKs = null;
			wipAccReverseBatchPKs = null;
		}

		public List<ZGuid> InvoiceBatchFilterPks
		{
			get { return batchInvoicePKs ?? (batchInvoicePKs = InvoiceBatchFilter.GetFilterPks(false)); }
		}
		List<ZGuid> batchInvoicePKs;

		public List<ZGuid> UnallocatedTransactionBatchFilterPks
		{
			get { return unAllocatedTranBatchPKs ?? (unAllocatedTranBatchPKs = UnallocatedTransactionFilter.GetFilterPks(false)); }
		}
		List<ZGuid> unAllocatedTranBatchPKs;

		public List<ZGuid> WIPAccPostBatchFilterPks
		{
			get { return wipAccPostBatchPKs ?? (wipAccPostBatchPKs = WIPAccrualPostBatchFilter.GetFilterPks(false)); }
		}
		List<ZGuid> wipAccPostBatchPKs;

		public List<ZGuid> WIPAccReverseBatchFilterPks
		{
			get { return wipAccReverseBatchPKs ?? (wipAccReverseBatchPKs = WIPAccrualReverseBatchFilter.GetFilterPks(false)); }
		}
		List<ZGuid> wipAccReverseBatchPKs;

		#endregion

		#region NumberOfTransactionsProcessed

		public int NumberOfTransactionsProcessed
		{
			get { return fNumberOfTransactionsProcessed; }
		}
		int fNumberOfTransactionsProcessed;

		#endregion

		#region PercentageComplete

		public int PercentageComplete
		{
			get { return Convert.ToInt32(NumberOfTransactionsProcessed / (float)LastBatchNumberOfTransactions * 100); }
		}

		#endregion

		#region FilterProvider

		public TransactionExportFilterProvider FilterProvider
		{
			get
			{
				if (fFilterProvider == null)
				{
					fFilterProvider = new TransactionExportFilterProvider(Factory);
					fFilterProvider.Exporter = this as ISupportHighWaterMark;
				}
				return fFilterProvider;
			}
		}

		#endregion

		#region Filters

		public WIPAccrualReversingTransactionExportFilter WIPAccrualReverseBatchFilter
		{
			get
			{
				if (fWIPReverseBatchFilter == null)
				{
					fWIPReverseBatchFilter = new WIPAccrualReversingTransactionExportFilter(Factory, FilterProvider);
				}
				return fWIPReverseBatchFilter;
			}
		}

		public WIPAccrualPostingTransactionExportFilter WIPAccrualPostBatchFilter
		{
			get
			{
				if (fWIPPostBatchFilter == null)
				{
					fWIPPostBatchFilter = new WIPAccrualPostingTransactionExportFilter(Factory, FilterProvider);
				}
				return fWIPPostBatchFilter;
			}
		}

		public virtual FinancialInvoiceTransactionExportFilter InvoiceBatchFilter
		{
			get
			{
				if (fInvoiceBatchFilter == null)
				{
					fInvoiceBatchFilter = new FinancialInvoiceTransactionExportFilter(Factory, FilterProvider);
				}
				return fInvoiceBatchFilter;
			}
		}

		public virtual UnallocatedTransactionExportFilter UnallocatedTransactionFilter
		{
			get
			{
				if (fUnallocatedTransactionFilter == null)
				{
					fUnallocatedTransactionFilter = new UnallocatedTransactionExportFilter(Factory, FilterProvider);
				}
				return fUnallocatedTransactionFilter;
			}
		}

		#endregion

		TransactionExportFilterProvider fFilterProvider;
		FinancialInvoiceTransactionExportFilter fInvoiceBatchFilter;
		UnallocatedTransactionExportFilter fUnallocatedTransactionFilter;
		WIPAccrualPostingTransactionExportFilter fWIPPostBatchFilter;
		WIPAccrualReversingTransactionExportFilter fWIPReverseBatchFilter;
		readonly BusinessObjectFactoryProvider FactoryProvider;
		protected readonly NotificationBuffer Notify;
		protected Stream Document;
		protected internal const int EmptyBatchNumber = 0;
		protected bool IsRecalculationDisabled;

		#endregion

		#endregion
	}
}
