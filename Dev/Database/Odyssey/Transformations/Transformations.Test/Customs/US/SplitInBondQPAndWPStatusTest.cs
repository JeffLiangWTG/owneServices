using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(SplitInBondQPAndWPStatus))]
	sealed class SplitInBondQPAndWPStatusTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[] { "NONCLUSTERED INDEX [_WTG__Split InBond QP And WP Status_1] ON [dbo].[CusInBondMoveHeader] ([BM_SystemCreateTimeUtc]) INCLUDE ([BM_AutoVersion], [BM_BH], [BM_CustomsStatus], [BM_MessageStatus], [BM_SystemLastEditTimeUtc], [BM_SystemLastEditUser]) WHERE ([BM_CustomsStatus] IN ('AAV', 'AEX', 'ATL', 'CAV', 'CEX', 'CTL', 'EAV', 'EEX', 'ETL')) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)" };

		public void TestOnlinePostUpgradeLogging()
		{
			var oldDate = DateTime.UtcNow.AddYears(-1).Date;
			var header03 = TestDataCreator.CreateCusInbondHeader("02", branch, "INB");
			//Be processed in OfflinePostUpgrade phase.
			var moveHeader0301 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "AAV", createTimeUtc: oldDate.AddDays(1));

			//Be processed in OnlinePostUpgrade phase.
			var moveHeader0302 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "AAV", createTimeUtc: oldDate.AddHours(-1));
			var moveHeader0303 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "AAV", createTimeUtc: oldDate.AddDays(-1));
			var moveHeader0304 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "AAV", createTimeUtc: oldDate.AddDays(-2));
			var moveHeader0305 = CreateCusInBondMoveHeader(header03, customsStatus: "AAV");
			var expected = new List<string>
			{
				$"Processing batch time range {SplitInBondQPAndWPStatus.GenerateTimeRangeLog(oldDate.AddDays(-1), oldDate)}...",
				"[2] records updated.",
				$"Processing batch time range {SplitInBondQPAndWPStatus.GenerateTimeRangeLog(oldDate.AddDays(-2), oldDate.AddDays(-1))}...",
				"[1] records updated.",
				$"Processing batch time range []-[]...",
				"[1] records updated.",
				"\tCompleted: Split InBond QP And WP Status"
			};
			var actual = new List<string>();
			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			((IOnlineTransformation)transformation).Run(actual.Add, CancellationToken.None);
			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		protected override void AssertTransformationResults()
		{
			AssertQPWPStatus(nameof(moveHeader0101), moveHeader0101, "AAV", "");
			AssertQPWPStatus(nameof(moveHeader0201), moveHeader0201, "XX1", "");
			AssertQPWPStatus(nameof(moveHeader0202), moveHeader0202, "", "");

			AssertQPWPStatus(nameof(moveHeader0301), moveHeader0301, "ADA", "301");
			AssertQPWPStatus(nameof(moveHeader0302), moveHeader0302, "ADO", "AAV");
			AssertQPWPStatus(nameof(moveHeader0303), moveHeader0303, "ADW", "CPA");
			AssertQPWPStatus(nameof(moveHeader0304), moveHeader0304, "CDA", "");
			AssertQPWPStatus(nameof(moveHeader0305), moveHeader0305, "CDO", "");
			AssertQPWPStatus(nameof(moveHeader0306), moveHeader0306, "CDW", "");
			AssertQPWPStatus(nameof(moveHeader0307), moveHeader0307, "CPA", "");
			AssertQPWPStatus(nameof(moveHeader0308), moveHeader0308, "CPO", "");
			AssertQPWPStatus(nameof(moveHeader0309), moveHeader0309, "CPW", "");
			AssertQPWPStatus(nameof(moveHeader0310), moveHeader0310, "EDA", "");
			AssertQPWPStatus(nameof(moveHeader0311), moveHeader0311, "EDO", "");
			AssertQPWPStatus(nameof(moveHeader0312), moveHeader0312, "EDW", "");

			AssertQPWPStatus(nameof(moveHeader0401), moveHeader0401, "", "AAV");
			AssertQPWPStatus(nameof(moveHeader0402), moveHeader0402, "", "AEX");
			AssertQPWPStatus(nameof(moveHeader0403), moveHeader0403, "", "ATL");
			AssertQPWPStatus(nameof(moveHeader0404), moveHeader0404, "", "CAV");
			AssertQPWPStatus(nameof(moveHeader0405), moveHeader0405, "", "CEX");
			AssertQPWPStatus(nameof(moveHeader0406), moveHeader0406, "", "CTL");
			AssertQPWPStatus(nameof(moveHeader0407), moveHeader0407, "", "EAV");
			AssertQPWPStatus(nameof(moveHeader0408), moveHeader0408, "", "EEX");
			AssertQPWPStatus(nameof(moveHeader0409), moveHeader0409, "", "ETL");

			AssertQPWPStatus(nameof(moveHeader0501), moveHeader0501, "", "AAV");
			AssertQPWPStatus(nameof(moveHeader0502), moveHeader0502, "", "AEX");
			AssertQPWPStatus(nameof(moveHeader0503), moveHeader0503, "", "ATL");
			AssertQPWPStatus(nameof(moveHeader0504), moveHeader0504, "", "CAV");
			AssertQPWPStatus(nameof(moveHeader0505), moveHeader0505, "", "CEX");
			AssertQPWPStatus(nameof(moveHeader0506), moveHeader0506, "", "CTL");
			AssertQPWPStatus(nameof(moveHeader0507), moveHeader0507, "", "EAV");
			AssertQPWPStatus(nameof(moveHeader0508), moveHeader0508, "", "EEX");
			AssertQPWPStatus(nameof(moveHeader0509), moveHeader0509, "", "ETL");
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new SplitInBondQPAndWPStatus(1);

		protected override void PrepareTestData()
		{
			var nowDate = DateTime.UtcNow;

			var header01 = TestDataCreator.CreateCusInbondHeader("01", branch, "AMS");
			var header02 = TestDataCreator.CreateCusInbondHeader("02", branch, "INB");
			var header03 = TestDataCreator.CreateCusInbondHeader("03", branch, "INB");
			var header04 = TestDataCreator.CreateCusInbondHeader("04", branch, "INB");
			var header05 = TestDataCreator.CreateCusInbondHeader("05", branch, "INB");

			// -- Should skip
			// BH_ApplicationCode != 'INB'
			moveHeader0101 = TestDataCreator.CreateCusInBondMoveHeader(header01, customsStatus: "AAV");

			// BM_CustomsStatus is Unknow status
			moveHeader0201 = TestDataCreator.CreateCusInBondMoveHeader(header02, customsStatus: "XX1");

			// BM_CustomsStatus is Empty
			moveHeader0202 = TestDataCreator.CreateCusInBondMoveHeader(header02);

			// BM_CustomsStatus is QP status
			moveHeader0301 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "ADA", messageStatus: "301");
			moveHeader0302 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "ADO", messageStatus: "AAV");
			moveHeader0303 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "ADW", messageStatus: "CPA");
			moveHeader0304 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "CDA");
			moveHeader0305 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "CDO");
			moveHeader0306 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "CDW");
			moveHeader0307 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "CPA");
			moveHeader0308 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "CPO");
			moveHeader0309 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "CPW");
			moveHeader0310 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "EDA");
			moveHeader0311 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "EDO");
			moveHeader0312 = TestDataCreator.CreateCusInBondMoveHeader(header03, customsStatus: "EDW");

			// -- Should process
			// BM_CustomsStatus is WP status, BM_SystemCreateTimeUtc is not NULL
			moveHeader0401 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "AAV", messageStatus: "401");
			moveHeader0402 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "AEX", messageStatus: "CAV");
			moveHeader0403 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "ATL", messageStatus: "ADA");
			moveHeader0404 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "CAV");
			moveHeader0405 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "CEX");
			moveHeader0406 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "CTL");
			moveHeader0407 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "EAV");
			moveHeader0408 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "EEX");
			moveHeader0409 = TestDataCreator.CreateCusInBondMoveHeader(header04, customsStatus: "ETL");

			// BM_CustomsStatus is WP status, BM_SystemCreateTimeUtc is NULL
			moveHeader0501 = CreateCusInBondMoveHeader(header05, "AAV", messageStatus: "501");
			moveHeader0502 = CreateCusInBondMoveHeader(header05, "AEX", messageStatus: "CAV");
			moveHeader0503 = CreateCusInBondMoveHeader(header05, "ATL", messageStatus: "ADA");
			moveHeader0504 = CreateCusInBondMoveHeader(header05, "CAV");
			moveHeader0505 = CreateCusInBondMoveHeader(header05, "CEX");
			moveHeader0506 = CreateCusInBondMoveHeader(header05, "CTL");
			moveHeader0507 = CreateCusInBondMoveHeader(header05, "EAV");
			moveHeader0508 = CreateCusInBondMoveHeader(header05, "EEX");
			moveHeader0509 = CreateCusInBondMoveHeader(header05, "ETL");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var company = TestDataCreator.CreateCompany("AAA", "US", "USD");
			branch = TestDataCreator.CreateBranch(company, "PHL", "USPHL");
		}

		void AssertQPWPStatus(string caseID, Guid moveHeader, string expectedQPStatus, string expectedWPStatus)
		{
			var actualQPStatus = string.Empty;
			var actualWPStatus = string.Empty;
			using (var command = Db.Connection.Command($"SELECT BM_CustomsStatus, BM_MessageStatus FROM dbo.CusInBondMoveHeader WHERE BM_PK = '{moveHeader}'"))
			using (var reader = command.ExecuteReader())
			{
				{
					if (reader.Read())
					{
						actualQPStatus = reader.GetString(0);
						actualWPStatus = reader.GetString(1);
					}
				}
			}
			CombineAssertions(caseID, () =>
			{
				AssertEquals("QP", expectedQPStatus, actualQPStatus);
				AssertEquals("WP", expectedWPStatus, actualWPStatus);
			});
		}

		Guid CreateCusInBondMoveHeader(Guid bondHeaderPK, string customsStatus, string messageStatus = "")
		{
			var guid = Guid.NewGuid();
			var sqlText = "\r\nINSERT INTO dbo.CusInbondMoveHeader\r\n(BM_PK, BM_BH, BM_SubApplicationCode, BM_CustomsStatus, BM_MessageStatus, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)\r\nVALUES\r\n(@BM_PK, @BM_BH, @BM_SubApplicationCode, @BM_CustomsStatus, @BM_MessageStatus, @BM_SystemCreateTimeUtc, '~BP', GetUtcDate(), '~BP')\r\n";
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_CusInBondMoveHeader_AuditDetailsAreNotMissing_Insert", "CusInBondMoveHeader"))
			using (DbCommand dbCommand = Db.Connection.Command(sqlText))
			{
				dbCommand.AddParameter("@BM_PK", SqlDbType.UniqueIdentifier, guid);
				dbCommand.AddParameter("@BM_BH", SqlDbType.UniqueIdentifier, bondHeaderPK);
				dbCommand.AddParameter("@BM_SubApplicationCode", SqlDbType.VarChar, "");
				dbCommand.AddParameter("@BM_CustomsStatus", SqlDbType.VarChar, customsStatus);
				dbCommand.AddParameter("@BM_SystemCreateTimeUtc", SqlDbType.SmallDateTime, DBNull.Value);
				dbCommand.AddParameter("@BM_MessageStatus", SqlDbType.VarChar, "");
				dbCommand.ExecuteNonQuery();
			}

			return guid;
		}

		Guid branch;
		Guid moveHeader0101;
		Guid moveHeader0201;
		Guid moveHeader0202;
		Guid moveHeader0301;
		Guid moveHeader0302;
		Guid moveHeader0303;
		Guid moveHeader0304;
		Guid moveHeader0305;
		Guid moveHeader0306;
		Guid moveHeader0307;
		Guid moveHeader0308;
		Guid moveHeader0309;
		Guid moveHeader0310;
		Guid moveHeader0311;
		Guid moveHeader0312;
		Guid moveHeader0401;
		Guid moveHeader0402;
		Guid moveHeader0403;
		Guid moveHeader0404;
		Guid moveHeader0405;
		Guid moveHeader0406;
		Guid moveHeader0407;
		Guid moveHeader0408;
		Guid moveHeader0409;
		Guid moveHeader0501;
		Guid moveHeader0502;
		Guid moveHeader0503;
		Guid moveHeader0504;
		Guid moveHeader0505;
		Guid moveHeader0506;
		Guid moveHeader0507;
		Guid moveHeader0508;
		Guid moveHeader0509;
	}
}
