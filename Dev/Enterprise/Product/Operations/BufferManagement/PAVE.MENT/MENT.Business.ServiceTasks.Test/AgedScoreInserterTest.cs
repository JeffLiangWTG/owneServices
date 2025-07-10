using System;
using System.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.PAVE.MENT.Business.Test;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Business.ServiceTasks.Test
{
	[UseSnapshotProtection]
	public class AgedScoreInserterTest : TestCase
	{
		#region Exceptions When Executing Ment Query

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunInserter_UserCausedExceptionsAreReported()
		{
			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var sql = @"select 69 as score, null releaseGroup, null component, MAQ_Code as attributeValue DOPASD, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sql;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Error, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
User Defined Statement is Invalid.
Incorrect syntax near 'DOPASD'.
Error Inserting QUEERY into MENTAgedScoreMetric.
The Sql being run was: select 69 as score, null releaseGroup, null component, MAQ_Code as attributeValue DOPASD, 'XAL' as staff from dbo.MENTAgedScoreQuery", result.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_UserCausedExceptionsAreReported_InvalidValue()
		{
			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var sql = @"select 'Something' as score, null as releaseGroup, null as component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sql;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Error, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
User Defined Statement is Invalid.
Error converting data type varchar to numeric.
Error Inserting QUEERY into MENTAgedScoreMetric.
The Sql being run was: select 'Something' as score, null as releaseGroup, null as component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery", result.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_UserCausedExceptionsAreReported_InvalidReleaseGroup()
		{
			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var sql = @"select 69 as score, 'AAAAA' as releaseGroup, null as component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sql;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Error, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
User Defined Statement is Invalid.
Conversion failed when converting from a character string to uniqueidentifier.
Error Inserting QUEERY into MENTAgedScoreMetric.
The Sql being run was: select 69 as score, 'AAAAA' as releaseGroup, null as component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery", result.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_UserCausedExceptionsAreReported_InvalidComponent()
		{
			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var sql = @"select 69 as score, null as releaseGroup, 'BBBBB' as component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sql;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Error, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
User Defined Statement is Invalid.
Conversion failed when converting from a character string to uniqueidentifier.
Error Inserting QUEERY into MENTAgedScoreMetric.
The Sql being run was: select 69 as score, null as releaseGroup, 'BBBBB' as component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery", result.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_ExceptionsAreReported_WithExtraInnerException_ShouldStillBeHandled()
		{
			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			const string sql = @"select 69 as score, null as releaseGroup, 'BBBBB' as component, MAQ_Code as attributeValue, 'XAL' as staff from TableThatDoesntExist";
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sql;

			Factory.Save();

			var inserter = new AgedScoreInserterWithCustomExceptionHandling();
			QueryResult? result = null;

			AssertNoExceptionThrown("The sql exception should be caught and dealth with, even if it has an inner exception. SAD!", () =>
			{
				result = inserter.RunInsertSqlAndLogErrors(queryToRun);
			});

			AssertEquals(InserterResult.Error, result?.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
User Defined Statement is Invalid.
Invalid object name 'TableThatDoesntExist'.
You did a terrible thing. One of your worst.
Random other exception tagging along.
Error Inserting QUEERY into MENTAgedScoreMetric.
The Sql being run was: select 69 as score, null as releaseGroup, 'BBBBB' as component, MAQ_Code as attributeValue, 'XAL' as staff from TableThatDoesntExist", result?.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
		}

		class AgedScoreInserterWithCustomExceptionHandling : AgedScoreInserter
		{
			protected override bool DoesSqlExceptionExist(ref Exception ex, out System.Data.Common.DbException sqlException)
			{
				sqlException = SqlExceptionBuilder.CreateSqlException(1205, 51, 13, Db.ServerName, "You did a terrible thing. One of your worst.",
					string.Empty, 1, new Win32Exception("Random other exception tagging along."));
				ex = new Exception(ex.Message, sqlException);

				return base.DoesSqlExceptionExist(ref ex, out sqlException);
			}
		}

		#endregion

		#region Related Acceptability Bands

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_InvalidBAB_Type()
		{
			var band = Factory!.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "Jazz Band";
			band.BAB_Type = "QAG";
			band.BAB_SqlText = "select 'Something' as Value, null as ReleaseGroup, null as Component";

			var queryToRun = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);
			AssertEquals(InserterResult.Error, result.Result);
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_UserCausedExceptionsAreReported_InvalidValueInAcceptabilityBandQuery()
		{
			var band = Factory!.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "Jazz Band";
			band.BAB_Type = "SQL";
			band.BAB_SqlText = "select 'Something' as Value, null as ReleaseGroup, null as Component";

			var queryToRun = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Error, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Insert of BAB data into temp table failed
Error Inserting QUEERY into MENTAgedScoreMetric.
Related Acceptability Band: Jazz Band.
The Acceptability Band Sql being run was: select 'Something' as Value, null as ReleaseGroup, null as Component", result.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_UserCausedExceptionsAreReported_InvalidReleaseGroupInAcceptabilityBandQuery()
		{
			var band = Factory!.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "Jazz Band";
			band.BAB_Type = "SQL";
			band.BAB_SqlText = "select 1 as Value, 'AAAAA' as ReleaseGroup, null as Component";

			var queryToRun = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Error, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Insert of BAB data into temp table failed
Error Inserting QUEERY into MENTAgedScoreMetric.
Related Acceptability Band: Jazz Band.
The Acceptability Band Sql being run was: select 1 as Value, 'AAAAA' as ReleaseGroup, null as Component", result.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_UserCausedExceptionsAreReported_InvalidComponentInAcceptabilityBandQuery()
		{
			var band = Factory!.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "Jazz Band";
			band.BAB_Type = "SQL";
			band.BAB_SqlText = "select 1 as Value, null as ReleaseGroup, 'BBBBB' as Component";

			var queryToRun = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Error, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Insert of BAB data into temp table failed
Error Inserting QUEERY into MENTAgedScoreMetric.
Related Acceptability Band: Jazz Band.
The Acceptability Band Sql being run was: select 1 as Value, null as ReleaseGroup, 'BBBBB' as Component", result.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_CorrectAcceptabilityBandQuery()
		{
			var band = Factory!.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "Jazz Band";
			band.BAB_Type = "SQL";
			band.BAB_SqlText = "select 1 as Value, null as ReleaseGroup, null as Component";

			var queryToRun = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Success, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Finished Insert of QUEERY into MENTAgedScoreMetric.", result.Messages);

			AssertEquals(1, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
			var rowCountSql = string.Format(MENTTestHelper.MENTRowCountForNameSql, queryToRun.MAQ_Code);
			AssertEquals(1, (int)Db.Connection.ExecuteScalar(rowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_InsertAcceptabilityBandPK()
		{
			var band = Factory!.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "Jazz Band";
			band.BAB_Type = "SQL";
			band.BAB_SqlText = "select 1 as Value, null as ReleaseGroup, null as Component";

			var queryToRun = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Success, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Finished Insert of QUEERY into MENTAgedScoreMetric.", result.Messages);

			AssertEquals(1, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
			var rowCountSql = string.Format(MENTTestHelper.MENTRowCountForNameSql, queryToRun.MAQ_Code) + $" AND MAS_BAB_AcceptabilityBand = '{band.PK}'";
			AssertEquals(1, (int)Db.Connection.ExecuteScalar(rowCountSql));
		}

		[TestDate(2019, 8, 26, 10, 0, 0)]
		public void TestRunInserter_NoAcceptabilityBandPK_WhenNoRelatedAcceptabilityBand()
		{
			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var sql = @"select 123 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sql;
			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = ZGuid.Empty;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Success, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Finished Insert of QUEERY into MENTAgedScoreMetric.", result.Messages);

			AssertEquals(1, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
			var rowCountSql = string.Format(MENTTestHelper.MENTRowCountForNameSql, queryToRun.MAQ_Code) + $" AND MAS_BAB_AcceptabilityBand is NULL";
			AssertEquals(1, (int)Db.Connection.ExecuteScalar(rowCountSql));
		}

		#endregion

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunInserter()
		{
			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var sql = @"select 69 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sql;

			//These won't be run but will be reported on.
			var query1 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query1.MAQ_Code = "ANOTHER";
			query1.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(20);
			query1.MAQ_SqlText = sql;
			var query2 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query2.MAQ_Code = "CAAAAAR";
			query2.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(20);
			query2.MAQ_SqlText = sql;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Success, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Finished Insert of QUEERY into MENTAgedScoreMetric.", result.Messages);

			AssertEquals(3, (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql));
			var rowCountSql = string.Format(MENTTestHelper.MENTRowCountForNameSql, queryToRun.MAQ_Code);
			AssertEquals(3, (int)Db.Connection.ExecuteScalar(rowCountSql));

			var sumOfScoresSql = string.Format(MENTTestHelper.MENTSumForNameSql, queryToRun.MAQ_Code);
			AssertEquals(207.0m, (decimal)Db.Connection.ExecuteScalar(sumOfScoresSql));
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunInserterWithStaff()
		{
			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var sql = @"select 69 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sql;

			//These won't be run but will be reported on.
			var query1 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query1.MAQ_Code = "ANOTHER";
			query1.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(20);
			query1.MAQ_SqlText = sql;
			var query2 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
			query2.MAQ_Code = "CAAAAAR";
			query2.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(20);
			query2.MAQ_SqlText = sql;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Success, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Finished Insert of QUEERY into MENTAgedScoreMetric.", result.Messages);

			AssertEquals(0, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.MENTAgedScoreMetric where MAS_GS_NKStaffCode <> 'XAL'"));
			AssertEquals(3, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.MENTAgedScoreMetric where MAS_GS_NKStaffCode = 'XAL'"));

			AssertEquals(207.0m, (decimal)Db.Connection.ExecuteScalar("select sum(MAS_AgedScoreValue) from dbo.MENTAgedScoreMetric where MAS_GS_NKStaffCode = 'XAL'"));
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunInserterWithAcceptabilityBand_SQL()
		{
			var sqlToNotRun = @"select 10 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";

			var buffer = BMSTestHelper.CreateBuffer(BMSTestHelper.CreateSystem(Factory, "ORG"));

			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();

			var sqlToRun = string.Format("select 100 as value, null releaseGroup, '{0}' component from dbo.MENTAgedScoreQuery", buffer.PK.ToString());
			queryToRun.MAQ_Code = "QUEERY";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sqlToNotRun;

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;
			band.BAB_SqlText = sqlToRun;
			band.BAB_FC_Component = buffer.PK;

			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Success, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUEERY into MENTAgedScoreMetric.
Finished Insert of QUEERY into MENTAgedScoreMetric.", result.Messages);

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.MENTAgedScoreMetric where MAS_MAQ_NKCode = 'QUEERY'"));

			AssertEquals(100.0m, (decimal)Db.Connection.ExecuteScalar("select sum(MAS_AgedScoreValue) from dbo.MENTAgedScoreMetric where MAS_MAQ_NKCode = 'QUEERY'"));
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunInserterWithAcceptabilityBand_Number()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var secondBuffer = BMSTestHelper.CreateBuffer(system, name: "bufferina");

			var job = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(job, "Turtle", currentComponent: buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(job, "Flamingo", currentComponent: buffer);
			var workflow3 = BMSTestHelper.CreateWorkflow(job, "Nasal Congestion", currentComponent: buffer);

			var workflow4 = BMSTestHelper.CreateWorkflow(job, "Giraffe", currentComponent: secondBuffer);
			var workflow5 = BMSTestHelper.CreateWorkflow(job, "Unicorn", currentComponent: secondBuffer);
			var workflow6 = BMSTestHelper.CreateWorkflow(job, "Brocolli and Peanut Icecream", currentComponent: secondBuffer);

			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var sqlToNotRun = @"select 69 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun.MAQ_Code = "QUESTICON";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = sqlToNotRun;

			var band = Factory!.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_SqlText = sqlToNotRun;
			band.BAB_FC_Component = buffer.PK;

			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory?.Save();

			var inserter = new AgedScoreInserter();
			var result = inserter.RunInsertSqlAndLogErrors(queryToRun);

			AssertEquals(InserterResult.Success, result.Result);
			AssertMultilineASCIIEquals(@"Starting Insert of QUESTICON into MENTAgedScoreMetric.
Finished Insert of QUESTICON into MENTAgedScoreMetric.", result.Messages);

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from dbo.MENTAgedScoreMetric where MAS_MAQ_NKCode = 'QUESTICON'"));

			AssertEquals("Job level and workflows 1, 2 and 3 should be reported on", 4.0m, (decimal)Db.Connection.ExecuteScalar(string.Format("select sum(MAS_AgedScoreValue) from dbo.MENTAgedScoreMetric where MAS_MAQ_NKCode = '{0}' AND MAS_FC_Component = '{1}'", queryToRun.MAQ_Code, buffer.PK)));
		}

		#region Implementation

		protected override void SetUp()
		{
			MENTTestHelper.ClearMENTTables();
			base.SetUp();
			Factory = new BusinessObjectFactory();
			BMSTestHelper.EnableBMSInRegistry();
		}

		BusinessObjectFactory? Factory;

		#endregion
	}
}
