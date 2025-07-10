using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.AuditDataServices.Accounting.Subscribers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	[TestedType(typeof(AccTaxGLMovementSubscriber))]
	public class AccTaxGLMovementSubscriberTest : AccountingSubscriberBaseTest
	{
		protected override string ExpectedCode => "ATM";

		protected override string ExpectedDescription => "AccTaxGLMovement Subscriber";

		protected override string ExpectedTableName => "AccTaxGLMovement";

		[TestDate(2023, 5, 14)]
		public override void TestProcessChanges()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10));
			var changeTable = GetTestDataTable();
			CreateRowForGLTaxMovement(changeTable);
			NewDataChangeSubscriber().ProcessChanges(loggerMock.Object, changeTable);

			generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTaxGLMovementSchema.Constants.PK, 1))), Times.Exactly(1));
		}

		[TestDate(2023, 5, 14)]
		public override void TestProcessChanges_BeforeGenerateJournalEntriesCDCStartDate()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 15));

			var changeTable = GetTestDataTable();
			CreateRowForGLTaxMovement(changeTable);
			NewDataChangeSubscriber().ProcessChanges(loggerMock.Object, changeTable);

			generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTaxGLMovementSchema.Constants.PK, 1))), Times.Never);
			Assert(true);
		}

		[TestDate(2023, 5, 14)]
		public override void TestProcessChanges_HasException()
		{
			generalLedgerDataProcessorMock.Setup(m => m.ProcessData(It.IsAny<DataRow[]>())).Throws(new Exception("Error message for process data"));
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10));
			var changeTable = GetTestDataTable();
			CreateRowForGLTaxMovement(changeTable);
			AssertExceptionThrown<Exception>(() => NewDataChangeSubscriber().ProcessChanges(loggerMock.Object, changeTable));

			generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTaxGLMovementSchema.PK.Name, 1))), Times.Exactly(1));

			Assert(true);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new AccTaxGLMovementSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(AccTaxGLMovementSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(AccTaxGLMovementSchema.Constants.ATM_ATT_TaxTransaction, typeof(Guid));
			changeTable.Columns.Add(AccTaxGLMovementSchema.Constants.ATM_Date, typeof(DateTime));
			changeTable.Columns.Add(AccTaxGLMovementSchema.Constants.ATM_Amount, typeof(decimal));
			changeTable.Columns.Add(AccTaxGLMovementSchema.Constants.ATM_SystemCreateTimeUtc, typeof(DateTime));
			return changeTable;
		}

		void CreateRowForGLTaxMovement(DataTable dataTable)
		{
			var taxTransaction = CreateTaxGLMovement();
			factory.Save();
			var taxGLMovement = factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransaction.PK)).FirstOrDefault();
			factory.Save();
			SetDataRow(dataTable, taxGLMovement);
		}

		AccTaxTransaction CreateTaxGLMovement()
		{
			var helper = new AccountingPeriodTestHelper(factory);
			helper.PostPeriodsForEntireYear(ZDate.Today.Year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var transactionHeader = factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;

			var taxTransaction = factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = transactionHeader.PK;
			taxTransaction.ATT_LocalTaxAmount = 100m;
			taxTransaction.ATT_OSTaxAmount = 200m;
			taxTransaction.ATT_AT_TaxID = factory.NewWithValidTestData<AccTaxRate>().PK;
			taxTransaction.ATT_Basis = "MAT";
			taxTransaction.ATT_RateNumerator = 2;
			taxTransaction.ATT_RateDenominator = 1;
			taxTransaction.ATT_A9_TaxMessage = factory.NewWithValidTestData<AccInvMsg>().PK;
			taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			taxTransaction.ATT_PostDate = ZDate.Today;
			taxTransaction.ATT_AG_LedgerControlAccount = factory.NewWithValidTestData<AccGLHeader>().PK;
			taxTransaction.ATT_AG_TaxPendingControlAccount = factory.NewWithValidTestData<AccGLHeader>().PK;
			return taxTransaction;
		}

		void SetDataRow(DataTable dataTable, AccTaxGLMovement taxGLMovement)
		{
			var row = dataTable.NewRow();
			row[AccTaxGLMovementSchema.PK.Name] = taxGLMovement.PK.ToGuid();
			row[AccTaxGLMovementSchema.ATM_ATT_TaxTransaction.Name] = taxGLMovement.ATM_ATT_TaxTransaction.ToGuid();
			row[AccTaxGLMovementSchema.ATM_Date.Name] = taxGLMovement.ATM_Date.ToDateTime();
			row[AccTaxGLMovementSchema.ATM_Amount.Name] = (decimal)taxGLMovement.ATM_Amount;
			row[AccTaxGLMovementSchema.ATM_SystemCreateTimeUtc.Name] = taxGLMovement.ATM_SystemCreateTimeUtc.ToDateTime();

			dataTable.Rows.Add(row);
			row.AcceptChanges();
			generalLedgerDataRows.Add(taxGLMovement.PK.ToGuid(), row);
		}
	}
}
