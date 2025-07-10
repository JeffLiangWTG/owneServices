

using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AccountingTransactionExportBatchCreationTest : ScriptTest
	{
		public void TestAccountingTransactionExportBatchCreation()
		{
			ARInvoice invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			APInvoice invoice2 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);

			WIP wip1 = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			WIP wip2 = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc2", 10M);
			WIP wip3 = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc2", 10M);
			Factory.Save();

			int batchNumber = 51;
			string headerBatchPks = invoice1.PK.ToString() + "," + invoice2.PK.ToString();
			string wipAccrualPostBatchPks = wip1.PK.ToString() + "," + wip2.PK.ToString();
			string wipAccrualReverseBatchPks = wip1.PK.ToString() + "," + wip2.PK.ToString() + "," + wip3.PK.ToString();

			RunScript(batchNumber, headerBatchPks, wipAccrualPostBatchPks, wipAccrualReverseBatchPks);
			var results = Factory.Load<GenExportBatchSequence>(new ZQuery());
			AssertEquals("Should Save 7 Records", 7, results.Length);

			results = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_BatchNumber, batchNumber));
			AssertEquals("Should Save 7 Records with Same batch Number", 7, results.Length);

			var values = GetGenExportBatchSequenceForBatch(batchNumber);
			var headers = new[] { GenExportBatchSequence.Schema.XB_Type, GenExportBatchSequence.Schema.XB_BatchNumber, GenExportBatchSequence.Schema.XB_ParentTableCode, GenExportBatchSequence.Schema.XB_ParentID };

			DataRow[] exportedRows = values.Select(string.Format("{0} = '{1}'", GenExportBatchSequence.Schema.XB_ParentID, invoice1.PK));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] {  Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport, 51, AccTransactionHeaderSchema.Constants.Prefix, invoice1.PK });

			exportedRows = values.Select(string.Format("{0} = '{1}'", GenExportBatchSequence.Schema.XB_ParentID, invoice2.PK));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] { Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport, 51, AccTransactionHeaderSchema.Constants.Prefix, invoice2.PK });

			exportedRows = values.Select(string.Format("{0} = '{1}' and {2} = '{3}'", GenExportBatchSequence.Schema.XB_ParentID, wip1.PK, GenExportBatchSequence.Schema.XB_Type, Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] { Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport, 51, AccTransactionLinesSchema.Constants.Prefix, wip1.PK });

			exportedRows = values.Select(string.Format("{0} = '{1}' and {2} = '{3}'", GenExportBatchSequence.Schema.XB_ParentID, wip1.PK, GenExportBatchSequence.Schema.XB_Type, Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] {  Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport, 51, AccTransactionLinesSchema.Constants.Prefix, wip1.PK });

			exportedRows = values.Select(string.Format("{0} = '{1}' and {2} = '{3}'", GenExportBatchSequence.Schema.XB_ParentID, wip2.PK, GenExportBatchSequence.Schema.XB_Type, Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] { Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport, 51, AccTransactionLinesSchema.Constants.Prefix, wip2.PK });

			exportedRows = values.Select(string.Format("{0} = '{1}' and {2} = '{3}'", GenExportBatchSequence.Schema.XB_ParentID, wip2.PK, GenExportBatchSequence.Schema.XB_Type, Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] { Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport, 51, AccTransactionLinesSchema.Constants.Prefix, wip2.PK });

			exportedRows = values.Select(string.Format("{0} = '{1}'",GenExportBatchSequence.Schema.XB_ParentID, wip3.PK));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] { Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport, 51, AccTransactionLinesSchema.Constants.Prefix, wip3.PK });

			results = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_Sequence, 0));
			AssertEquals("Should Save 2 Header Records With Sequence Number 0", 2, results.Length);

			results = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_Sequence, 1));
			AssertEquals("Should Save 1 Records With Sequence Number 1", 1, results.Length);

			results = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_Sequence, 2));
			AssertEquals("Should Save 1 Records With Sequence Number 2", 1, results.Length);

			results = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_Sequence, 3));
			AssertEquals("Should Save 1 Records With Sequence Number 3", 1, results.Length);

			results = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_Sequence, 4));
			AssertEquals("Should Save 1 Records With Sequence Number 4", 1, results.Length);

			results = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_Sequence, 5));
			AssertEquals("Should Save 1 Records With Sequence Number 5", 1, results.Length);
		}

		DataTable RunScript(int batchNumber, string headersBatchFilterPks, string wIPAccrualPostBatchFilterPKs, string wIPAccrualReverseBatchFilterPKs)
		{
			string sql = string.Format(@"Exec AccountingTransactionExportBatchCreation 
			'{0}', --@CompanyCode
			'{1}', --@BatchNumber
			'{2}', --@HeadersBatchFilterPks
			'{3}', --@WIPAccrualPostBatchFilterPKs
			'{4}'  --@WIPAccrualReverseBatchFilterPKs
			", GlbCompany.CurrentCompany.PK.ToString(), batchNumber, headersBatchFilterPks, wIPAccrualPostBatchFilterPKs, wIPAccrualReverseBatchFilterPKs);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		DataTable GetGenExportBatchSequenceForBatch(int batchNumber)
		{
			string sql = string.Format(@"SELECT 
			XB_Type,
			XB_BatchNumber,
			XB_Sequence,
			XB_ParentTableCode,
			XB_ParentID
			FROM dbo.GenExportBatchSequence WHERE XB_BatchNumber='{0}'", batchNumber.ToString());
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}

	#region NonTransactionedTestingForGetAppLock

	[UseSnapshotProtection]
	public class NonTransactionedTestingForGetAppLock : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			var factory = new BusinessObjectFactory();
			invoice = factory.New<ARInvoice>();

			factory.Save();
		}
		ARInvoice invoice;

		public void TestGetAppLockDuringExport()
		{
			using (var testConnection = Db.NewExtraConnectionToMainDb())
			{
				testConnection.BeginTransaction();
				RunScript(100, invoice.PK.ToString(), Guid.Empty.ToString(), Guid.Empty.ToString(), testConnection);
				Db.Connection.DefaultCommandTimeOutInSeconds = 20;

				using (var otherConnection = Db.Connection)
				{
					Assert("Different Connections", !Object.ReferenceEquals(testConnection, otherConnection));
					try
					{
						RunScript(100, invoice.PK.ToString(), Guid.Empty.ToString(), Guid.Empty.ToString(), otherConnection);
						Fail("Sql exception should be raised.");
					}
					catch (System.Data.Common.DbException ex)
					{
						Assert("There Should Be Error Could not obtain lock Because of sp_getapplock", ex.Message.StartsWith("Could not obtain lock - process is already running"));
					}
				}
				var results = GetGenExportBatchSequenceForBatch(testConnection);
				AssertEquals("Should Export 1 Record", 1, results.Rows.Count);
				AssertEquals("Batch Number Should Be 100", 100, results.Rows[0][GenExportBatchSequence.Schema.XB_BatchNumber]);

				testConnection.RollbackTransaction();
				testConnection.CloseConnection();
			}
		}

		DataTable RunScript(int batchNumber, string headersBatchFilterPks, string wIPAccrualPostBatchFilterPKs, string wIPAccrualReverseBatchFilterPKs, DbConnection connection)
		{
			string sql = string.Format(@"Exec AccountingTransactionExportBatchCreation 
			'{0}', --@CompanyCode
			'{1}', --@BatchNumber
			'{2}', --@HeadersBatchFilterPks
			'{3}', --@WIPAccrualPostBatchFilterPKs
			'{4}'  --@WIPAccrualReverseBatchFilterPKs
			", GlbCompany.CurrentCompany.PK.ToString(), batchNumber, headersBatchFilterPks, wIPAccrualPostBatchFilterPKs, wIPAccrualReverseBatchFilterPKs);
			return DataUtils.GetDataTableFromQuery(connection, sql);
		}

		DataTable GetGenExportBatchSequenceForBatch(DbConnection connection)
		{
			string sql = string.Format(@"SELECT 
			XB_Type,
			XB_BatchNumber,
			XB_Sequence,
			XB_ParentTableCode,
			XB_ParentID
			FROM dbo.GenExportBatchSequence");
			return DataUtils.GetDataTableFromQuery(connection, sql);
		}
	}

	#endregion
}


