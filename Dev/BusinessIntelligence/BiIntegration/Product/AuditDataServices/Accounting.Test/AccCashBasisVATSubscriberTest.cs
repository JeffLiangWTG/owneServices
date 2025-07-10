using System;
using System.Data;
using CargoWise.Types;
using Enterprise.AuditDataServices.Accounting.Subscribers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	[TestedType(typeof(AccCashBasisVATSubscriber))]
	public class AccCashBasisVATSubscriberTest : AccountingSubscriberBaseTest
	{
		public void TestAccCashBasisVATSubscriber()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);

			Assert("NotifyInsert is true", subscriber.NotifyInsert);
			Assert("NotifyDelete is false", !subscriber.NotifyDelete);
			Assert("NotifyUpdate is false", !subscriber.NotifyUpdate);
		}

		[TestDate(2023, 5, 17)]
		public override void TestProcessChanges()
		{
			AssertProcessChanges(new DateTime(2023, 5, 15), 1);
		}

		[TestDate(2023, 5, 17)]
		public override void TestProcessChanges_BeforeGenerateJournalEntriesCDCStartDate()
		{
			AssertProcessChanges(new DateTime(2023, 5, 1), 0);
		}

		[TestDate(2023, 5, 17)]
		public override void TestProcessChanges_HasException()
		{
			generalLedgerDataProcessorMock.Setup(m => m.ProcessData(It.IsAny<DataRow[]>())).Throws(new Exception("Error message for process data"));
			AssertExceptionThrown<Exception>(() => AssertProcessChanges(new DateTime(2023, 5, 1), 1));
		}

		void AssertProcessChanges(DateTime expectedTime, int count)
		{
			var changeTable = GetTestDataTable();
			var header = factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = "AP";
			header.AH_TransactionType = "PAY";
			var line = factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = header.PK;
			factory.Save();

			AddRowtoTable(changeTable, line.PK.ToGuid(), expectedTime, 20m);
			AddRowtoTable(changeTable, line.PK.ToGuid(), new DateTime(2023, 4, 12), 20m);
			AddRowtoTable(changeTable, line.PK.ToGuid(), ZDateTime.Now.ToDateTime(), 20m);
			NewDataChangeSubscriber().ProcessChanges(loggerMock.Object, changeTable);
			generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccCashBasisVATSchema.PK.Name, 0))), Times.Exactly(0));

			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
			{
				NewDataChangeSubscriber().ProcessChanges(loggerMock.Object, changeTable);

				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccCashBasisVATSchema.PK.Name, count + 1))), Times.Exactly(1));
				loggerMock.Verify(m => m.Log(LogType.Debug, It.Is<string>(s => s.StartsWith("AccCashBasisVAT Received DataRows:"))), Times.Exactly(2));
			}
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(GlbPersonSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AccCashBasisVATSchema.Constants.YC_AL_TransactionLine, typeof(Guid));
			changeTable.Columns.Add(AccCashBasisVATSchema.Constants.YC_GC, typeof(Guid));
			changeTable.Columns.Add(AccCashBasisVATSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(AccCashBasisVATSchema.Constants.YC_PostDate, typeof(DateTime));
			changeTable.Columns.Add(AccCashBasisVATSchema.Constants.YC_SystemCreateTimeUtc, typeof(DateTime));
			changeTable.Columns.Add(AccCashBasisVATSchema.Constants.YC_TaxAmount, typeof(Decimal));
			return changeTable;
		}

		void AddRowtoTable(DataTable changeTable, Guid linePK, DateTime postDate, Decimal taxAmount)
		{
			var vatPK = Guid.NewGuid();
			var changeRow = changeTable.NewRow();
			changeRow[AccCashBasisVATSchema.Constants.PK] = vatPK;
			changeRow[AccCashBasisVATSchema.Constants.YC_AL_TransactionLine] = linePK;
			changeRow[AccCashBasisVATSchema.Constants.YC_GC] = GlbCompany.CurrentCompany.PK.ToGuid();
			changeRow[AccCashBasisVATSchema.Constants.YC_PostDate] = postDate;
			changeRow[AccCashBasisVATSchema.Constants.YC_SystemCreateTimeUtc] = postDate;
			changeRow[AccCashBasisVATSchema.Constants.YC_TaxAmount] = taxAmount;
			changeTable.Rows.Add(changeRow);

			generalLedgerDataRows.Add(vatPK, changeRow);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new AccCashBasisVATSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		#region SetUp

		protected override string ExpectedCode => "ACB";

		protected override string ExpectedDescription => "AccCashBasisVAT Subscriber";

		protected override string ExpectedTableName => "AccCashBasisVAT";

		#endregion
	}
}
