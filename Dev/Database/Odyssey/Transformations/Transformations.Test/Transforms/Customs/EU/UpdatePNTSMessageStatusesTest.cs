using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU
{
	[TestedType(typeof(UpdatePNTSMessageStatuses))]
	class UpdatePNTSMessageStatusesTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePNTSMessageStatuses();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update existing PNTS message statuses to 3 chars. Empty PNTS message status where message status is _1] ON [dbo].[AsycudaManifestHeader] ([AMA_ApplicationCode], [AMA_MessageStatus]) WHERE ([AMA_ApplicationCode]='STO' AND ([AMA_MessageStatus] IN ('FA', 'ST', 'FR', 'NST'))) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		public void TestUserDescription()
		{
			AssertEquals("Update existing PNTS message statuses to 3 chars. Empty PNTS message status where message status is NST.", GetNewTestTransformationInstance().UserDescription);
		}

		protected override void PrepareTestData()
		{
			_ = TestConnection.ExecuteNonQuery($@"
				DECLARE	@pk1	UNIQUEIDENTIFIER = '{pk1}',
						@pk2	UNIQUEIDENTIFIER = '{pk2}',
						@pk3	UNIQUEIDENTIFIER = '{pk3}',
						@pk4	UNIQUEIDENTIFIER = '{pk4}',
						@pk5	UNIQUEIDENTIFIER = '{pk5}',
						@pk6	UNIQUEIDENTIFIER = '{pk6}',
						@pk7	UNIQUEIDENTIFIER = '{pk7}',
						@pk8	UNIQUEIDENTIFIER = '{pk8}',
						@pk9	UNIQUEIDENTIFIER = '{pk9}',
						@pk10	UNIQUEIDENTIFIER = '{pk10}',
						@pk11	UNIQUEIDENTIFIER = '{pk11}',
						@pk12	UNIQUEIDENTIFIER = '{pk12}',
						@pk13	UNIQUEIDENTIFIER = '{pk13}',
						@pk14	UNIQUEIDENTIFIER = '{pk14}',
						@pk15	UNIQUEIDENTIFIER = '{pk15}',
						@pk16	UNIQUEIDENTIFIER = '{pk16}',
						@pk17	UNIQUEIDENTIFIER = '{pk17}',
						@GC_PK UNIQUEIDENTIFIER = NEWID(),
						@GB_PK UNIQUEIDENTIFIER = NEWID()

			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
			VALUES(@GC_PK, 'FR', 'EUR', 'TST', 'FR company', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
			VALUES(@GB_PK, @GC_PK, 'TST', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

			INSERT INTO dbo.AsycudaManifestHeader(AMA_PK, AMA_ApplicationCode, AMA_MessageStatus, AMA_RN_NKCountry, AMA_ClusterKey, AMA_JobReference, AMA_GB, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser)
			VALUES
			(@pk1, 'LVC', 'FA', 'IT', '1', 'JOB_1',  @GB_PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk2, 'FHM', 'FA', 'IT', '2', 'JOB_2',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk3, 'BCD', 'FA', 'IT', '3', 'JOB_3',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk4, 'ETR', 'FA', 'IT', '4', 'JOB_4',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk5, 'BAS', 'FA', 'IT', '5', 'JOB_5',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk6, 'OUT', 'FA', 'IT', '6', 'JOB_6',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk7, 'EMP', 'FA', 'IT', '7', 'JOB_7',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk8, 'BBK', 'FA', 'IT', '8', 'JOB_8',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk9, 'NVC', 'FA', 'IT', '9', 'JOB_9',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk10, 'VOC', 'FA', 'IT', '10', 'JOB_10',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk11, 'VOC', 'ST', 'IT', '11', 'JOB_11',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk12, 'VOC', 'FR', 'IT', '12', 'JOB_12',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk13, 'VOC', 'NST', 'IT', '13', 'JOB_13',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk14, 'STO', 'FA', 'IT', '14', 'JOB_14',  @GB_PK,  GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk15, 'STO', 'ST', 'IT', '15', 'JOB_15',  @GB_PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk16, 'STO', 'FR', 'IT', '16', 'JOB_16',  @GB_PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
			(@pk17, 'STO', 'NST', 'IT', '17', 'JOB_17',  @GB_PK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		protected override void AssertTransformationResults()
		{
			var resultList = new List<Tuple<Guid, string>>();
			TestConnection.ExecuteReader($"SELECT * FROM dbo.AsycudaManifestHeader where AMA_PK in ('{pk1}', '{pk2}', '{pk3}', '{pk4}', '{pk5}', '{pk6}', '{pk7}', '{pk8}', '{pk9}', '{pk10}', '{pk11}', '{pk12}', '{pk13}')", reader => resultList.Add(Tuple.Create((Guid)reader["AMA_PK"], (string)reader["AMA_MessageStatus"])));
			AssertContainsExactElementsInAnyOrder("Transformation should not apply when AMA_MessageStatus is not STO (PNTS).", new[] { $"{pk1}, FA", $"{pk2}, FA", $"{pk3}, FA", $"{pk4}, FA", $"{pk5}, FA", $"{pk6}, FA", $"{pk7}, FA", $"{pk8}, FA", $"{pk9}, FA", $"{pk10}, FA", $"{pk11}, ST", $"{pk12}, FR", $"{pk13}, NST" }, resultList.Select(x => $"{x.Item1}, {x.Item2}").ToArray());

			resultList = new List<Tuple<Guid, string>>();
			TestConnection.ExecuteReader($"SELECT * FROM dbo.AsycudaManifestHeader where AMA_PK = '{pk14}'", reader => resultList.Add(Tuple.Create((Guid)reader["AMA_PK"], (string)reader["AMA_MessageStatus"])));
			AssertEquals("Transformation should transform AMA_MessageStatus from FA to FAL when AMA_MessageStatus is STO (PNTS).", $"{pk14}, FAL", resultList.Select(x => $"{x.Item1}, {x.Item2}").ToArray().FirstOrDefault());

			resultList = new List<Tuple<Guid, string>>();
			TestConnection.ExecuteReader($"SELECT * FROM dbo.AsycudaManifestHeader where AMA_PK = '{pk15}'", reader => resultList.Add(Tuple.Create((Guid)reader["AMA_PK"], (string)reader["AMA_MessageStatus"])));
			AssertEquals("Transformation should transform AMA_MessageStatus from ST to SNT when AMA_MessageStatus is STO (PNTS).", $"{pk15}, SNT", resultList.Select(x => $"{x.Item1}, {x.Item2}").ToArray().FirstOrDefault());

			resultList = new List<Tuple<Guid, string>>();
			TestConnection.ExecuteReader($"SELECT * FROM dbo.AsycudaManifestHeader where AMA_PK = '{pk16}'", reader => resultList.Add(Tuple.Create((Guid)reader["AMA_PK"], (string)reader["AMA_MessageStatus"])));
			AssertEquals("Transformation should transform AMA_MessageStatus from FR to REJ when AMA_MessageStatus is STO (PNTS).", $"{pk16}, REJ", resultList.Select(x => $"{x.Item1}, {x.Item2}").ToArray().FirstOrDefault());

			resultList = new List<Tuple<Guid, string>>();
			TestConnection.ExecuteReader($"SELECT * FROM dbo.AsycudaManifestHeader where AMA_PK = '{pk17}'", reader => resultList.Add(Tuple.Create((Guid)reader["AMA_PK"], (string)reader["AMA_MessageStatus"])));
			AssertEquals("Transformation should transform AMA_MessageStatus from NST to empty when AMA_MessageStatus is STO (PNTS).", $"{pk17}, ", resultList.Select(x => $"{x.Item1}, {x.Item2}").ToArray().FirstOrDefault());
		}

		Guid pk1 = Guid.NewGuid();
		Guid pk2 = Guid.NewGuid();
		Guid pk3 = Guid.NewGuid();
		Guid pk4 = Guid.NewGuid();
		Guid pk5 = Guid.NewGuid();
		Guid pk6 = Guid.NewGuid();
		Guid pk7 = Guid.NewGuid();
		Guid pk8 = Guid.NewGuid();
		Guid pk9 = Guid.NewGuid();
		Guid pk10 = Guid.NewGuid();
		Guid pk11 = Guid.NewGuid();
		Guid pk12 = Guid.NewGuid();
		Guid pk13 = Guid.NewGuid();
		Guid pk14 = Guid.NewGuid();
		Guid pk15 = Guid.NewGuid();
		Guid pk16 = Guid.NewGuid();
		Guid pk17 = Guid.NewGuid();
	}
}
