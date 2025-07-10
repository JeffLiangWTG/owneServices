using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ComplianceSequence.Testing
{
	public class PortugalComplianceSequenceDbRestoreSubscriberTest : TransactionedTestCase
	{
		public void TestRunSubscriber_ForAccComplianceSequence()
		{
			var companyPK = ZGuid.NewZGuid();
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
VALUES ('{companyPK}', 'DPT', 'PT company','{CountryCodes.Portugal}', '{CurrencyCodes.Portugal}')");

			var complianceSequence1PK = ZGuid.NewZGuid();
			var complianceSequence2PK = ZGuid.NewZGuid();
			var complianceSequence3PK = ZGuid.NewZGuid();
			var complianceSequence4PK = ZGuid.NewZGuid();
			var complianceSequence5PK = ZGuid.NewZGuid();

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company)
VALUES ('{complianceSequence1PK}', 'SE1', 1, '1', '1', '{companyPK}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company)
VALUES ('{complianceSequence2PK}', 'SE2', 0, '1', '1', '{companyPK}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company)
VALUES ('{complianceSequence3PK}', 'SE3', 1, '1', '1', '{GlbCompany.CurrentCompany.PK}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company)
VALUES ('{complianceSequence4PK}', 'SE3', 0, '1', '1', '{GlbCompany.CurrentCompany.PK}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company, XD_PermanentDisableTimeUtc)
VALUES ('{complianceSequence5PK}', 'SE5', 0, '1', '1', '{companyPK}', dateadd(day, -1, GetUtcDate()))");

			new PortugalComplianceSequenceDbRestoreSubscriber().Run(TestConnection);

			AssertEquals("SE1 XD_IsActive", false, TestConnection.ExecuteScalar<bool>($@"SELECT XD_IsActive FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence1PK}'"));
			AssertEquals("SE2 XD_IsActive", false, TestConnection.ExecuteScalar<bool>($@"SELECT XD_IsActive FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence2PK}'"));
			AssertEquals("SE3 XD_IsActive", true, TestConnection.ExecuteScalar<bool>($@"SELECT XD_IsActive FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence3PK}'"));
			AssertEquals("SE4 XD_IsActive", false, TestConnection.ExecuteScalar<bool>($@"SELECT XD_IsActive FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence4PK}'"));
			AssertEquals("SE5 XD_IsActive", false, TestConnection.ExecuteScalar<bool>($@"SELECT XD_IsActive FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence5PK}'"));

			var permanentDisableTimeUtcForSE1 = TestConnection.ExecuteScalar<DateTime>($@"SELECT XD_PermanentDisableTimeUtc FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence1PK}'");
			var permanentDisableTimeUtcForSE2 = TestConnection.ExecuteScalar<DateTime>($@"SELECT XD_PermanentDisableTimeUtc FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence2PK}'");
			var permanentDisableTimeUtcForSE3 = TestConnection.ExecuteScalar($@"SELECT XD_PermanentDisableTimeUtc FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence3PK}'");
			var permanentDisableTimeUtcForSE4 = TestConnection.ExecuteScalar($@"SELECT XD_PermanentDisableTimeUtc FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence4PK}'");
			var permanentDisableTimeUtcForSE5 = TestConnection.ExecuteScalar<DateTime>($@"SELECT XD_PermanentDisableTimeUtc FROM dbo.AccComplianceSequence WHERE XD_PK = '{complianceSequence5PK}'");

			AssertEquals(true, permanentDisableTimeUtcForSE1 == permanentDisableTimeUtcForSE2);
			AssertEquals(true, permanentDisableTimeUtcForSE1 != permanentDisableTimeUtcForSE5);
			AssertEquals(DBNull.Value, permanentDisableTimeUtcForSE3);
			AssertEquals(DBNull.Value, permanentDisableTimeUtcForSE4);
		}

		public void TestRunSubscriber_WillInsertStmData()
		{
			var companyPK = ZGuid.NewZGuid();
			var registry = AccountingMasterFilesRegistry.Instance.LastUTCDateToDisableComplianceBookAfterDbRestored;

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
VALUES ('{companyPK}', 'DPT', 'PT company','{CountryCodes.Portugal}', '{CurrencyCodes.Portugal}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company)
VALUES (NEWID(), 'SE1', 1, '1', '1', '{companyPK}')");

			AssertEquals("NO StmData", false, TestConnection.Exists($@"FROM dbo.StmData WHERE SD_Name = '{registry.Name}'"));
			new PortugalComplianceSequenceDbRestoreSubscriber().Run(TestConnection);
			AssertEquals("StmData is created", true, TestConnection.Exists($@"FROM dbo.StmData WHERE SD_Name = '{registry.Name}'"));
		}

		public void TestRunSubscriber_WillNotInsertStmData()
		{
			var companyPK = ZGuid.NewZGuid();
			var registry = AccountingMasterFilesRegistry.Instance.LastUTCDateToDisableComplianceBookAfterDbRestored;

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
VALUES ('{companyPK}', 'DPT', 'PT company','{CountryCodes.Portugal}', '{CurrencyCodes.Portugal}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company)
VALUES ('{ZGuid.NewZGuid()}', 'SE1', 1, '1', '1', '{GlbCompany.CurrentCompany.PK}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company, XD_PermanentDisableTimeUtc)
VALUES ('{ZGuid.NewZGuid()}', 'SE2', 0, '1', '1', '{companyPK}', dateadd(day, -2, GetUtcDate()))");

			AssertEquals("NO StmData", false, TestConnection.Exists($@"FROM dbo.StmData WHERE SD_Name = '{registry.Name}'"));
			new PortugalComplianceSequenceDbRestoreSubscriber().Run(TestConnection);
			AssertEquals("NO StmData", false, TestConnection.Exists($@"FROM dbo.StmData WHERE SD_Name = '{registry.Name}'"));
		}

		public void TestRunSubscriber_WillUpdateStmData()
		{
			var companyPK = ZGuid.NewZGuid();
			var registry = AccountingMasterFilesRegistry.Instance.LastUTCDateToDisableComplianceBookAfterDbRestored;

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
VALUES ('{companyPK}', 'DPT', 'PT company','{CountryCodes.Portugal}', '{CurrencyCodes.Portugal}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue)
VALUES (NEWID(), '{registry.Name}', 'DT', 1, CAST(N'{DateTime.UtcNow.AddDays(-1)}' as varbinary(max)))");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company)
VALUES (NEWID(), 'SE2', 1, '1', '2', '{companyPK}')");

			var date1 = TestConnection.ExecuteScalar<string>($@"SELECT convert(nvarchar(max), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = '{registry.Name}'");
			new PortugalComplianceSequenceDbRestoreSubscriber().Run(TestConnection);
			var date2 = TestConnection.ExecuteScalar<string>($@"SELECT convert(nvarchar(max), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = '{registry.Name}'");

			AssertNotEquals("StmData is updated", date1, date2);
			AssertEquals("Check Registry", Convert.ToDateTime(date2), registry.Value);
		}

		public void TestRunSubscriber_WillNotUpdateStmData()
		{
			var companyPK = ZGuid.NewZGuid();
			var registry = AccountingMasterFilesRegistry.Instance.LastUTCDateToDisableComplianceBookAfterDbRestored;

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency)
VALUES ('{companyPK}', 'DPT', 'PT company','{CountryCodes.Portugal}', '{CurrencyCodes.Portugal}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue)
VALUES (NEWID(), '{registry.Name}', 'DT', 1, CAST(N'{DateTime.UtcNow.AddDays(-1)}' as varbinary(max)))");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company)
VALUES ('{ZGuid.NewZGuid()}', 'SE1', 1, '1', '1', '{GlbCompany.CurrentCompany.PK}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccComplianceSequence (XD_PK, XD_Code, XD_IsActive, XD_StartNumber, XD_NextNumber, XD_GC_Company, XD_PermanentDisableTimeUtc)
VALUES ('{ZGuid.NewZGuid()}', 'SE2', 0, '1', '1', '{companyPK}', dateadd(day, -2, GetUtcDate()))");

			var date1 = TestConnection.ExecuteScalar<string>($@"SELECT convert(nvarchar(max), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = '{registry.Name}'");
			new PortugalComplianceSequenceDbRestoreSubscriber().Run(TestConnection);
			var date2 = TestConnection.ExecuteScalar<string>($@"SELECT convert(nvarchar(max), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = '{registry.Name}'");

			AssertEquals("StmData is not updated", date1, date2);
		}

		public void TestNoSqlExceptionThrown_InvalidObjectName()
		{
			var testDatabaseName = "TestPortugalComplianceSequenceDbRestoreSubscriber";
			using (AdoTestUtils.CreateDbDropExistingDisposable(testDatabaseName))
			using (var testConnection = Db.NewAdminConnection(testDatabaseName))
			{
				var result = string.Empty;
				AssertNoExceptionThrown(() => result = new PortugalComplianceSequenceDbRestoreSubscriber().Run(testConnection));
				AssertEquals("Invalid object name 'dbo.AccComplianceSequence'.", result);
			}
		}

		public void TestNoSqlExceptionThrown_InvalidColumnName()
		{
			var testDatabaseName = "TestPortugalComplianceSequenceDbRestoreSubscriber";
			using (AdoTestUtils.CreateDbDropExistingDisposable(testDatabaseName))
			using (var testConnection = Db.NewAdminConnection(testDatabaseName))
			{
				testConnection.ExecuteNonQuery(@"
CREATE TABLE [StmData] (
	[SD_PK] UNIQUEIDENTIFIER NOT NULL,
	[SD_Name] VARCHAR(300) NOT NULL DEFAULT ''
)");

				var result = string.Empty;
				AssertNoExceptionThrown(() => result = new PortugalComplianceSequenceDbRestoreSubscriber().Run(testConnection));
				AssertContains("Invalid column name 'SD_BinaryValue'.", result);
			}
		}

		public void TestThrowException()
		{
			TestConnection.CloseConnection();
			AssertExceptionThrown<TransactionException>(() => new PortugalComplianceSequenceDbRestoreSubscriber().Run(TestConnection));
		}
	}
}
