using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(ModifyParentToCusInBondMoveHeaderForStmPrintJobAndStmALog))]
	class ModifyParentToCusInBondMoveHeaderForStmPrintJobAndStmALogTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new ModifyParentToCusInBondMoveHeaderForStmPrintJobAndStmALog();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Modify Parent to CusInBondMoveHeader for StmPrintJob and StmALog_1] ON [dbo].[StmPrintJob] ([SP_ParentTableName]) INCLUDE ([SP_SystemLastEditTimeUtc], [SP_SystemLastEditUser]) WHERE ([SP_ParentTableName]='USInBondMoveHeader') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		public void TestOfflinePostUpgrade()
		{
			var utcNow = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
			new ModifyParentToCusInBondMoveHeaderForStmPrintJobAndStmALog().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

			var toTimeNameForStmALog = ExtProperty.Database.Select(Db.Connection, ToTimeNameForStmALog);

#pragma warning disable CW1122 // Do Not Use DateTime Parse Method
			AssertEquals(utcNow, DateTime.Parse(toTimeNameForStmALog).ToString("yyyy-MM-dd HH:mm:ss"));
#pragma warning restore CW1122 // Do Not Use DateTime Parse Method

			PrepareTestData();

			RunOfflinePostUpgrade();
			AssertOfflineResults();
			RunOfflinePostUpgrade();
			AssertOfflineResults();
		}

		void RunOfflinePostUpgrade()
		{
			GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
		}

		void AssertOfflineResults()
		{
			var printJobTableNameResult1 = Db.Connection.ExecuteScalar($"SELECT SP_ParentTableName FROM dbo.StmPrintJob WHERE SP_PK = '{PrintJob1}'");
			AssertEquals("Changed to CusInBondMoveHeader", "CusInBondMoveHeader", printJobTableNameResult1);

			var printJobTableNameResult2 = Db.Connection.ExecuteScalar($"SELECT SP_ParentTableName FROM dbo.StmPrintJob WHERE SP_PK = '{PrintJob2}'");
			AssertEquals("Changed to CusInBondMoveHeader", "CusInBondMoveHeader", printJobTableNameResult2);

			var printJobTableNameResult3 = Db.Connection.ExecuteScalar($"SELECT SP_ParentTableName FROM dbo.StmPrintJob WHERE SP_PK = '{PrintJob3}'");
			AssertEquals("Changed to CusInBondMoveHeader", "CusInBondMoveHeader", printJobTableNameResult3);
		}

		public void TestExtPropertyShouldBeCleared()
		{
			PrepareTestData();
			new ModifyParentToCusInBondMoveHeaderForStmPrintJobAndStmALog().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);

			AssertNull($"ExtProperty '{FromTimeNameForStmALog}' should be cleared", ExtProperty.Database.Select(Db.Connection, FromTimeNameForStmALog));
			AssertNull($"ExtProperty '{ToTimeNameForStmALog}' should be cleared", ExtProperty.Database.Select(Db.Connection, ToTimeNameForStmALog));
		}

		protected override void AssertTransformationResults()
		{
			var stmALogResult1 = Db.Connection.ExecuteScalar($"SELECT SL_Table FROM dbo.StmALog WHERE SL_PK = '{StmALog1}'");
			AssertEquals("Not changed to CusInBondMoveHeader as the date is ≤ 2024/11/01.", "USInBondMoveHeader", stmALogResult1);

			var stmALogResult2 = Db.Connection.ExecuteScalar($"SELECT SL_Table FROM dbo.StmALog WHERE SL_PK = '{StmALog2}'");
			AssertEquals("Not changed to CusInBondMoveHeader as the event code is not DSN.", "USInBondMoveHeader", stmALogResult2);

			var stmALogTableResult3 = Db.Connection.ExecuteScalar($"SELECT SL_Table FROM dbo.StmALog WHERE SL_PK = '{StmALog3}'");
			AssertEquals("Changed to CusInBondMoveHeader as the date is > 2024/11/01 and event code is DSN.", "CusInBondMoveHeader", stmALogTableResult3);
		}

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var companyPK = testDataCreator.CreateGlbCompany("AAA", "US");
			var branchPK = testDataCreator.CreateGlbBranch("DDD", companyPK);

			var createTimeUtc = new DateTime(2024, 10, 29);
			InBondHeader1 = testDataCreator.CreateCusInBondHeader("INB", branchPK, Guid.Empty, string.Empty, "0");
			USInBondMoveHeader1 = testDataCreator.CreateUSInBondMoveHeader(InBondHeader1);
			PrintJob1 = testDataCreator.CreatePrintJob("EML", USInBondMoveHeader1, USInBondMoveHeaderSchema.Constants.TableName, "7512 Departure - .XLSX", "7512 Departure~EN-US", createTimeUtc);
			StmALog1 = testDataCreator.CreateStmALog(USInBondMoveHeaderSchema.Constants.TableName, USInBondMoveHeader1, "~BP", "DSN", "", createTimeUtc, createTimeUtc);

			createTimeUtc = new DateTime(2024, 11, 29);
			InBondHeader2 = testDataCreator.CreateCusInBondHeader("INB", branchPK, Guid.Empty, string.Empty, "0");
			USInBondMoveHeader2 = testDataCreator.CreateUSInBondMoveHeader(InBondHeader2);
			PrintJob2 = testDataCreator.CreatePrintJob("FAA", USInBondMoveHeader2, USInBondMoveHeaderSchema.Constants.TableName, "Test 7512 - .XLSX", "Test 7512~EN-US", createTimeUtc);
			StmALog2 = testDataCreator.CreateStmALog(USInBondMoveHeaderSchema.Constants.TableName, USInBondMoveHeader2, "~BP", "EDT", "", createTimeUtc, createTimeUtc);

			createTimeUtc = new DateTime(2024, 12, 29);
			InBondHeader3 = testDataCreator.CreateCusInBondHeader("INB", branchPK, Guid.Empty, string.Empty, "0");
			USInBondMoveHeader3 = testDataCreator.CreateUSInBondMoveHeader(InBondHeader3);
			PrintJob3 = testDataCreator.CreatePrintJob("EML", USInBondMoveHeader3, USInBondMoveHeaderSchema.Constants.TableName, "New Document - .XLSX", "New Document~EN-US", createTimeUtc);
			StmALog3 = testDataCreator.CreateStmALog(USInBondMoveHeaderSchema.Constants.TableName, USInBondMoveHeader3, "~BP", "DSN", "", createTimeUtc, createTimeUtc);

			var toTime = new DateTime(2025, 03, 28);
			ExtProperty.Database.Update(Db.Connection, ToTimeNameForStmALog, SqlFormatInfo.ToSqlDateTimeString(toTime));
		}

		Guid InBondHeader1;
		Guid InBondHeader2;
		Guid InBondHeader3;

		Guid USInBondMoveHeader1;
		Guid USInBondMoveHeader2;
		Guid USInBondMoveHeader3;

		Guid PrintJob1;
		Guid PrintJob2;
		Guid PrintJob3;

		Guid StmALog1;
		Guid StmALog2;
		Guid StmALog3;

		public void TestRunCancellationAndExtProperty()
		{
			PrepareTestData();
			var logger = new List<string>();
			var cancellationToken = new CancellationToken(true);
			var transformation = GetNewTestTransformationInstance();

			AssertNull($"ExtProperty '{FromTimeNameForStmALog}' should be empty", ExtProperty.Database.Select(Db.Connection, FromTimeNameForStmALog));
			ExtProperty.Database.Update(Db.Connection, FromTimeNameForStmALog, "2024-11-03 00:00:00.000");
			AssertExceptionThrown<OperationCanceledException>(() => ((IOnlineTransformation)transformation).Run(s => logger.Add(s), cancellationToken));
			AssertContainsExactElementsInExactOrder(new[] { "Finished processing StmALog up to 2024-11-04 00:00:00.000." }, logger);
			AssertEquals($"ExtProperty '{FromTimeNameForStmALog}' FromTimeNameForStmALog should be 2024-11-04", "2024-11-04 00:00:00.000", ExtProperty.Database.Select(Db.Connection, FromTimeNameForStmALog));
		}

		public void TestOnlinePostUpgradeRunTwice()
		{
			PrepareTestData();
			RunOfflinePostUpgrade();
			AssertNoExceptionThrown(() =>
			{
				var transformation = new ModifyParentToCusInBondMoveHeaderForStmPrintJobAndStmALog();
				transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
				transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			});
		}

		const string FromTimeNameForStmALog = "ModifyParentToCusInBondMoveHeaderInStmALog.From";
		const string ToTimeNameForStmALog = "ModifyParentToCusInBondMoveHeaderInStmALog.To";
	}
}
