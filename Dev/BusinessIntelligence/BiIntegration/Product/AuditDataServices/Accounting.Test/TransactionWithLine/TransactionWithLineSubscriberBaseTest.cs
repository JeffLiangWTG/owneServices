using System;
using System.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	public abstract class TransactionWithLineSubscriberBaseTest : AccountingSubscriberBaseTest
	{
		protected override string ExpectedTableName => AccTransactionLinesSchema.Constants.TableName;

		protected virtual int ExpectedRowNums_BeforeGenerateJournalEntriesCDCStartDate => 1;

		[TestDate(2023, 5, 14)]
		public override void TestProcessChanges()
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
			{
				var subscriber = NewDataChangeSubscriber();
				var changeTable = CreateProcessHeaderTable();

				CreateRowForTransactionLine(changeTable, DateTime.Now);
				CreateRowForTransactionLine(changeTable, DateTime.Now, DateTime.Now);
				subscriber.ProcessChanges(loggerMock.Object, changeTable);

				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionLinesSchema.PK.Name, 2))), Times.Exactly(1));
			}
		}

		[TestDate(2023, 5, 14)]
		public override void TestProcessChanges_BeforeGenerateJournalEntriesCDCStartDate()
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
			{
				var subscriber = NewDataChangeSubscriber();
				var changeTable = CreateProcessHeaderTable();

				CreateRowForTransactionLine(changeTable, new DateTime(2023, 5, 9));
				CreateRowForTransactionLine(changeTable, ZDateTime.Today.ToDateTime());

				subscriber.ProcessChanges(loggerMock.Object, changeTable);
				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionLinesSchema.PK.Name, ExpectedRowNums_BeforeGenerateJournalEntriesCDCStartDate))), Times.Exactly(1));
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

				CreateRowForTransactionLine(changeTable, DateTime.Now);
				CreateRowForTransactionLine(changeTable, DateTime.Now, DateTime.Now);
				AssertExceptionThrown<Exception>(() => subscriber.ProcessChanges(loggerMock.Object, changeTable));

				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionLinesSchema.PK.Name, 2))), Times.Exactly(1));
			}
		}

		internal DataTable CreateProcessHeaderTable()
		{
			var changeTable = new DataTable();

			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_LineType, typeof(string));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_GC, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_ReverseDate, typeof(DateTime));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_PostDate, typeof(DateTime));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_AG, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_LineAmount, typeof(decimal));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_OSAmount, typeof(decimal));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_GSTVAT, typeof(decimal));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_GB, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_GE, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_AC, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_RX_NKTransactionCurrency, typeof(string));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_ExchangeRate, typeof(decimal));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_JH, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_OH, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_GSTVATBasis, typeof(string));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_A9_VATClass, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_GovtChargeCode, typeof(string));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_RevRecognitionType, typeof(string));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_AT, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_AW, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_InputGSTVATRecoverable, typeof(decimal));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_TaxRateNumerator, typeof(int));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_TaxRateDenominator, typeof(int));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_AH, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_GB_TaxBranch, typeof(Guid));
			changeTable.Columns.Add(AccTransactionLinesSchema.Constants.AL_SystemCreateTimeUtc, typeof(DateTime));
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			return changeTable;
		}

		void CreateRowForTransactionLine(DataTable dataTable, DateTime createTime, DateTime? reverseDate = null)
		{
			var transactionLine = CreateTransactionLineWithoutHeader(createTime);
			var transactionHeader = factory.NewWithValidTestData<AccTransactionHeader>();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			transactionHeader.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_OH = orgHeader.PK;
			transactionLine.AL_AH = transactionHeader.PK;
			factory.Save();

			if (reverseDate != null)
			{
				transactionLine.AL_ReverseDate = DateTime.Now;
				factory.Save();
			}

			SetDataRow(dataTable, transactionLine);
		}

		internal void CreateRowForTransactionLineWithoutHeader(DataTable dataTable, DateTime postDate, DateTime? reverseDate = null)
		{
			var transactionLine = CreateTransactionLineWithoutHeader(postDate);
			factory.Save();

			if (reverseDate != null)
			{
				transactionLine.AL_ReverseDate = DateTime.Now;
				factory.Save();
			}

			SetDataRow(dataTable, transactionLine);
		}

		AccTransactionLines CreateTransactionLineWithoutHeader(DateTime createTime)
		{
			var transactionLine = factory.NewWithValidTestData<AccTransactionLines>();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			transactionLine.AL_PostDate = DateTime.Now;
			transactionLine.AL_LineAmount = 100m;
			transactionLine.AL_OSAmount = 200m;
			transactionLine.AL_AT = factory.NewWithValidTestData<AccTaxRate>().PK;
			transactionLine.AL_AG = factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_GSTVAT = 50m;
			transactionLine.AL_AC = factory.NewWithValidTestData<AccChargeCode>().PK;
			transactionLine.AL_RX_NKTransactionCurrency = "USD";
			transactionLine.AL_ExchangeRate = 2m;
			transactionLine.AL_InputGSTVATRecoverable = 1m;
			transactionLine.AL_TaxRateNumerator = 2;
			transactionLine.AL_TaxRateDenominator = 1;
			transactionLine.AL_OH = orgHeader.PK;
			transactionLine.AL_A9_VATClass = factory.NewWithValidTestData<AccInvMsg>().PK;
			transactionLine.AL_AW = factory.NewWithValidTestData<AccWithholding>().PK;
			transactionLine.AL_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			transactionLine.AL_SystemCreateTimeUtc = createTime;

			return transactionLine;
		}

		void SetDataRow(DataTable dataTable, AccTransactionLines transactionLine)
		{
			var row = dataTable.NewRow();
			dataTable.TableName = AccTransactionLinesSchema.Constants.TableName;
			row[AccTransactionLinesSchema.PK.Name] = transactionLine.PK.ToGuid();
			row[AccTransactionLinesSchema.AL_LineType.Name] = transactionLine.AL_LineType;
			if (transactionLine.AL_AH.IsValid)
			{
				row[AccTransactionLinesSchema.AL_AH.Name] = transactionLine.AL_AH.ToGuid();
			}
			else
			{
				row[AccTransactionLinesSchema.AL_AH.Name] = DBNull.Value;
			}
			row[AccTransactionLinesSchema.AL_OH.Name] = transactionLine.AL_OH.ToGuid();
			row[AccTransactionLinesSchema.AL_GC.Name] = transactionLine.AL_GC.ToGuid();
			if (transactionLine.AL_ReverseDate.IsValid)
			{
				row[AccTransactionLinesSchema.AL_ReverseDate.Name] = transactionLine.AL_ReverseDate;
			}
			row[AccTransactionLinesSchema.AL_PostDate.Name] = transactionLine.AL_PostDate;
			row[AccTransactionLinesSchema.AL_AG.Name] = transactionLine.AL_AG.ToGuid();
			row[AccTransactionLinesSchema.AL_LineAmount.Name] = (decimal)transactionLine.AL_LineAmount;
			row[AccTransactionLinesSchema.AL_OSAmount.Name] = (decimal)transactionLine.AL_OSAmount;
			row[AccTransactionLinesSchema.AL_GSTVAT.Name] = (decimal)transactionLine.AL_GSTVAT;
			row[AccTransactionLinesSchema.AL_GB.Name] = transactionLine.AL_GB.ToGuid();
			row[AccTransactionLinesSchema.AL_GE.Name] = transactionLine.AL_GE.ToGuid();
			row[AccTransactionLinesSchema.AL_AC.Name] = transactionLine.AL_AC.ToGuid();
			row[AccTransactionLinesSchema.AL_RX_NKTransactionCurrency.Name] = transactionLine.AL_RX_NKTransactionCurrency;
			row[AccTransactionLinesSchema.AL_ExchangeRate.Name] = (decimal)transactionLine.AL_ExchangeRate;
			row[AccTransactionLinesSchema.AL_JH.Name] = Guid.Empty;
			row[AccTransactionLinesSchema.AL_GSTVATBasis.Name] = transactionLine.AL_GSTVATBasis;
			row[AccTransactionLinesSchema.AL_A9_VATClass.Name] = transactionLine.AL_A9_VATClass.ToGuid();
			row[AccTransactionLinesSchema.AL_GovtChargeCode.Name] = transactionLine.AL_GovtChargeCode;
			row[AccTransactionLinesSchema.AL_RevRecognitionType.Name] = transactionLine.AL_RevRecognitionType;
			row[AccTransactionLinesSchema.AL_AT.Name] = transactionLine.AL_AT.ToGuid();
			row[AccTransactionLinesSchema.AL_AW.Name] = transactionLine.AL_AW.ToGuid();
			row[AccTransactionLinesSchema.AL_InputGSTVATRecoverable.Name] = (decimal)transactionLine.AL_InputGSTVATRecoverable;
			row[AccTransactionLinesSchema.AL_TaxRateNumerator.Name] = (int)transactionLine.AL_TaxRateNumerator;
			row[AccTransactionLinesSchema.AL_TaxRateDenominator.Name] = (int)transactionLine.AL_TaxRateDenominator;
			row[AccTransactionLinesSchema.AL_GB_TaxBranch.Name] = transactionLine.AL_GB_TaxBranch.ToGuid();
			row[AccTransactionLinesSchema.AL_SystemCreateTimeUtc.Name] = transactionLine.AL_SystemCreateTimeUtc;

			dataTable.Rows.Add(row);
			row.AcceptChanges();
			row.SetAdded();
			generalLedgerDataRows.Add(transactionLine.PK.ToGuid(), row);
		}
	}
}
