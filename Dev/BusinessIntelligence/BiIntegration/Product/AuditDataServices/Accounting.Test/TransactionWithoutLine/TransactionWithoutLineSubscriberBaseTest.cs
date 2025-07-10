using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	public abstract class TransactionWithoutLineSubscriberBaseTest : AccountingSubscriberBaseTest
	{
		protected override string ExpectedTableName => "AccTransactionHeader";

		[TestDate(2023, 5, 14)]
		public override void TestProcessChanges()
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
			{
				var changeTable = CreateProcessHeaderTable();

				CreateRow(changeTable, DateTime.Now);
				NewDataChangeSubscriber().ProcessChanges(loggerMock.Object, changeTable);
				loggerMock.Verify(m => m.Log(LogType.Debug, It.Is<string>(s => s.StartsWith("AccTransactionHeader Received DataRows: 1"))), Times.Once);
				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionHeaderSchema.PK.Name, 1))), Times.Exactly(1));
			}
		}

		[TestDate(2023, 5, 15)]
		public override void TestProcessChanges_BeforeGenerateJournalEntriesCDCStartDate()
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 11)))
			{
				var changeTable = CreateProcessHeaderTable();

				CreateRow(changeTable, new DateTime(2023, 5, 9), LedgerTypes.AccountsPayable, TransactionTypes.Receipt);
				CreateRow(changeTable, DateTime.Now, LedgerTypes.AccountsPayable, TransactionTypes.Payment);
				NewDataChangeSubscriber().ProcessChanges(loggerMock.Object, changeTable);
				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionHeaderSchema.PK.Name, 1))), Times.Exactly(1));
			}
		}

		[TestDate(2023, 5, 17)]
		public override void TestProcessChanges_HasException()
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
			{
				generalLedgerDataProcessorMock.Setup(m => m.ProcessData(It.IsAny<DataRow[]>())).Throws(new Exception("Error message for process data"));

				var subscriber = NewDataChangeSubscriber();
				var changeTable = CreateProcessHeaderTable();

				CreateRow(changeTable, DateTime.Now);
				AssertExceptionThrown<Exception>(() => subscriber.ProcessChanges(loggerMock.Object, changeTable));

				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionHeaderSchema.PK.Name, 1))), Times.Exactly(1));
			}
		}

		protected DataTable CreateProcessHeaderTable()
		{
			var changeTable = new DataTable();

			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_GC, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_PostDate, typeof(DateTime));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_Ledger, typeof(string));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_TransactionType, typeof(string));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_AG, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_InvoiceAmount, typeof(decimal));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_OSTotal, typeof(decimal));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_GSTAmount, typeof(decimal));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_TransactionNum, typeof(string));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_GB, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_GB_TaxBranch, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_GE, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_InvoiceDate, typeof(DateTime));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_ConsolidatedInvoiceRef, typeof(string));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_OH, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency, typeof(string));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_ExchangeRate, typeof(decimal));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_AB, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_ChequeOrReference, typeof(string));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_ReceiptType, typeof(string));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_TransactionBelongsToGroup, typeof(Guid));
			changeTable.Columns.Add(AccTransactionHeaderSchema.Constants.AH_SystemCreateTimeUtc, typeof(DateTime));
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			return changeTable;
		}

		protected void CreateRow(DataTable dataTable, DateTime postDate, string ledger = "AP", string transactionType = "PAY", bool isNeedMatchLink = false)
		{
			var transactionHeader = CreateTransactionHeader(postDate, ledger, transactionType, isNeedMatchLink);
			factory.Save();

			SetDataRow(dataTable, transactionHeader);
		}

		AccTransactionHeader CreateTransactionHeader(DateTime postDate, string ledger, string transactionType, bool isNeedMatchLink)
		{
			var transactionHeader = factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_PostDate = postDate;
			transactionHeader.AH_InvoiceAmount = 100m;
			transactionHeader.AH_OSTotal = 200m;
			transactionHeader.AH_OutstandingAmount = 150m;
			transactionHeader.AH_AG = factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionHeader.AH_GSTAmount = 50m;
			transactionHeader.AH_RX_NKTransactionCurrency = "USD";
			transactionHeader.AH_ExchangeRate = 2m;
			transactionHeader.AH_ConsolidatedInvoiceRef = "Test";
			transactionHeader.AH_ChequeOrReference = "CHQ";
			transactionHeader.AH_ReceiptType = "DDR";
			transactionHeader.AH_OH = factory.NewWithValidTestData<OrgHeader>().PK;
			transactionHeader.AH_AB = factory.NewWithValidTestData<AccBankAccount>().PK;
			transactionHeader.AH_TransactionBelongsToGroup = factory.NewWithValidTestData<AccTransactionHeader>().PK;
			transactionHeader.AH_Ledger = ledger;
			transactionHeader.AH_TransactionType = transactionType;
			transactionHeader.AH_SystemCreateTimeUtc = postDate;

			if (isNeedMatchLink)
			{
				transactionHeader.AH_OutstandingAmount = 0m;
				var matchLink = factory.New<AccTransactionMatchLink>();
				matchLink.AP_AH = transactionHeader.PK;
				matchLink.AP_Amount = 150m;
				matchLink.AP_MatchDate = postDate;
				matchLink.AP_MatchGroupNum = "M001";
				new MatchLinkGroupForTest(factory).Add(matchLink);
			}

			return transactionHeader;
		}

		void SetDataRow(DataTable dataTable, AccTransactionHeader transactionHeader)
		{
			var row = dataTable.NewRow();
			dataTable.TableName = transactionHeader.TableName;

			row[AccTransactionHeaderSchema.AH_GC.Name] = transactionHeader.AH_GC.ToGuid();
			row[AccTransactionHeaderSchema.AH_PostDate.Name] = transactionHeader.AH_PostDate;
			row[AccTransactionHeaderSchema.AH_Ledger.Name] = transactionHeader.AH_Ledger;
			row[AccTransactionHeaderSchema.AH_TransactionType.Name] = transactionHeader.AH_TransactionType;
			row[AccTransactionHeaderSchema.AH_AG.Name] = transactionHeader.AH_AG.ToGuid();
			row[AccTransactionHeaderSchema.AH_InvoiceAmount.Name] = (decimal)transactionHeader.AH_InvoiceAmount;
			row[AccTransactionHeaderSchema.AH_OSTotal.Name] = (decimal)transactionHeader.AH_OSTotal;
			row[AccTransactionHeaderSchema.AH_GSTAmount.Name] = (decimal)transactionHeader.AH_GSTAmount;
			row[AccTransactionHeaderSchema.AH_TransactionNum.Name] = transactionHeader.AH_TransactionNum;
			row[AccTransactionHeaderSchema.PK.Name] = transactionHeader.PK.ToGuid();
			row[AccTransactionHeaderSchema.AH_GB.Name] = transactionHeader.AH_GB.ToGuid();
			row[AccTransactionHeaderSchema.AH_GB_TaxBranch.Name] = transactionHeader.AH_GB_TaxBranch.ToGuid();
			row[AccTransactionHeaderSchema.AH_GE.Name] = transactionHeader.AH_GE.ToGuid();
			row[AccTransactionHeaderSchema.AH_InvoiceDate.Name] = transactionHeader.AH_InvoiceDate;
			row[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.Name] = transactionHeader.AH_ConsolidatedInvoiceRef;
			row[AccTransactionHeaderSchema.AH_OH.Name] = transactionHeader.AH_OH.ToGuid();
			row[AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency.Name] = transactionHeader.AH_RX_NKTransactionCurrency;
			row[AccTransactionHeaderSchema.AH_ExchangeRate.Name] = (decimal)transactionHeader.AH_ExchangeRate;
			row[AccTransactionHeaderSchema.AH_AB.Name] = transactionHeader.AH_AB.ToGuid();
			row[AccTransactionHeaderSchema.AH_ChequeOrReference.Name] = transactionHeader.AH_ChequeOrReference;
			row[AccTransactionHeaderSchema.AH_ReceiptType.Name] = transactionHeader.AH_ReceiptType;
			row[AccTransactionHeaderSchema.AH_TransactionBelongsToGroup.Name] = transactionHeader.AH_TransactionBelongsToGroup.ToGuid();
			row[AccTransactionHeaderSchema.AH_SystemCreateTimeUtc.Name] = transactionHeader.AH_SystemCreateTimeUtc;

			dataTable.Rows.Add(row);
			row.AcceptChanges();
			generalLedgerDataRows.Add(transactionHeader.PK, row);
		}

		#region SetUp

		class MatchLinkGroupForTest : BusinessObjectCollection<AccTransactionMatchLink>, ISupportCriticalValidation, IFactoryProvider
		{
			public MatchLinkGroupForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ICriticalValidation CriticalValidation => new CriticalValidationForTest(this);

			public void SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}
		}

		class CriticalValidationForTest : CriticalValidation<MatchLinkGroupForTest>
		{
			public CriticalValidationForTest(MatchLinkGroupForTest parent)
				: base(parent)
			{
			}
		}

		#endregion
	}
}
