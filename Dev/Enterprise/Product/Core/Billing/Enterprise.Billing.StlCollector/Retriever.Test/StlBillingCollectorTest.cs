using System;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using Enterprise.Billing.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class StlBillingCollectorTest : TransactionedTestCase
	{
		public void TestCollectionEndToEnd()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				DateTime fiftyHoursAgo = EnvProxy.Instance.Time.CurrentUtcDateTime.AddHours(-50);
				SystemDataRegistry.Instance.StlCollectorHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fiftyHoursAgo);
				DateTime transactionTimePrecise = fiftyHoursAgo.AddMinutes(10);
				DateTime transactionTime = new DateTime(
					transactionTimePrecise.Year,
					transactionTimePrecise.Month,
					transactionTimePrecise.Day,
					transactionTimePrecise.Hour,
					transactionTimePrecise.Minute,
					0);
				PrepareTestData(transactionTime);

				var logger = new LoggerForTest();
				var retriever = new StlRetriever(logger);
				retriever.CollectAndSend(CancellationToken.None);
				AssertOpmBillingTransaction(transactionTime);
			}
		}

		void PrepareTestData(DateTime transactionTime)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT TOP(1) OH_PK FROM dbo.OrgHeader);

				-- OPM
				INSERT dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser) VALUES
					('{0}', 'OP01', @OhPk, @GcPk, '{2}', 'US1', GetUtcDate(), 'E');

				-- SHP
				DECLARE @JsPk UNIQUEIDENTIFIER = newid();
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered) VALUES
					(@JsPk, 'SHP01', 1);
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_GB, JH_GE, JH_Status) VALUES
					('{1}', @GcPk, @JsPk, 'SHP01', '{2}', 'US1', @GbPk, @GePk, '{3}');
				",
				testOpmTransactionPk.ToString(),
				testShpTransactionPk.ToString(),
				SqlFormatInfo.ToSqlDateTimeString(transactionTime),
				JobHeaderStatus.Working.Code);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void AssertOpmBillingTransaction(DateTime expectedTransactionTime)
		{
			var encryptedData = TestConnection.ExecuteScalar("SELECT SUD_Data FROM dbo.StmUsageData WHERE SUD_Code = 'OPM'");
			AssertEquals("OPM Data found?", true, encryptedData != null && encryptedData != DBNull.Value);

			var opmTransaction = BillingManager.DecryptTransaction(encryptedData.ToString(), BillingManager.CurrentSchemaVersion);
			AssertEquals("BillableCount", 1, opmTransaction.BillableCount);
			AssertEquals("ClientStaffCode", "US1", opmTransaction.ClientStaffCode);
			AssertEquals("PriceItemCode", "OPM", opmTransaction.PriceItemCode);
			AssertEquals("Reference1", "OP01", opmTransaction.Reference1);
			AssertEquals("Reference5", testOpmTransactionPk.ToString().ToUpper(), opmTransaction.Reference5);
			AssertEquals("ServiceOccuredUTC", expectedTransactionTime, opmTransaction.ServiceOccuredUTC);
		}

		readonly Guid testOpmTransactionPk = Guid.NewGuid();
		readonly Guid testShpTransactionPk = Guid.NewGuid();
	}
}
