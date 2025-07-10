using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Testing
{
	[TestedType(typeof(vw_OrgManagementGroupingNode))]
	class vw_OrgManagementGroupingNodeTest : DbCreateScriptTest
	{
		public void TestGeneralUsage()
		{
			CreateTestData();

			var trees = ReadAllTrees();
			var orgATree = trees[Guid.Parse("380fd33e-d7ca-4359-9d1c-d41aabe269dd")];
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"380fd33e-d7ca-4359-9d1c-d41aabe269dd",
					"3f22068a-98dd-43ad-a1e8-c52f0a4ac806",
					"d8d47546-8b9b-4965-85f4-0bb0edf84157",
				},
				orgATree.OrgPks.Select(x => x.ToString()));

			var orgBTree = trees[Guid.Parse("b86c5da8-01d6-443d-90e2-32fbbe9fc97b")];
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"b86c5da8-01d6-443d-90e2-32fbbe9fc97b",
					"c0dfe776-9f83-434c-add0-ede6a7b6668a",
					"e4402152-319a-4078-855b-f1e19c274d2d",
				},
				orgBTree.OrgPks.Select(x => x.ToString()));

			trees.TryGetValue(Guid.Parse("e58afb38-663c-4114-a0e6-9a686ba79fba"), out Tree orgCTree);
			AssertNull("The tree should only contain orgs with at least one related org", orgCTree);

			AssertCollectionNotContains(Guid.Parse("3f22068a-98dd-43ad-a1e8-c52f0a4ac806"), trees.Keys);
			AssertCollectionNotContains(Guid.Parse("d8d47546-8b9b-4965-85f4-0bb0edf84157"), trees.Keys);
			AssertCollectionNotContains(Guid.Parse("c0dfe776-9f83-434c-add0-ede6a7b6668a"), trees.Keys);
			AssertCollectionNotContains(Guid.Parse("e4402152-319a-4078-855b-f1e19c274d2d"), trees.Keys);
		}

		public void TestCheckForNoIndexOrTableScans()
		{
			CreateMultipleOrgsAndRelatedParties(30);
			CreateTestData();

			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS {OrgHeaderSchema.Constants.TableName} WITH FULLSCAN");
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS {OrgRelatedPartySchema.Constants.TableName} WITH FULLSCAN");

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var selectSql = $@"SELECT * FROM dbo.vw_OrgManagementGroupingNode WHERE OrgPk = '{OrgA}'";
				using (var command = TestConnection.Command(selectSql))
				using (var reader = command.ExecuteReader())
				{
					Assert(reader.Read());
				}

				var allQueryPlans = TestConnection.ExecutedCommandsAndQueryPlans;
				var viewQueryPlan = allQueryPlans?.FirstOrDefault(p => p.Item1.Contains("vw_OrgManagementGroupingNode"));
				AssertNotNull("Should be Query Plan for vw_OrgManagementGroupingNode", viewQueryPlan);
				var planalyzer = new QueryPlanalyzer(viewQueryPlan.Item2.Last());

				AssertEquals("No Table Scans", 0, planalyzer.TableScans.Count());
				AssertEquals("No Index Scans", 0, planalyzer.IndexScans.Count());
			}
		}

		#region Implementation

		readonly Guid OrgA = new Guid("380fd33e-d7ca-4359-9d1c-d41aabe269dd");

		public void CreateTestData()
		{
			var insertTestDataSql = $@"
DECLARE @CompanyPk UNIQUEIDENTIFIER = '72F5E871-A015-4B22-AD67-D4E40CF238E1';
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'DAN', 'AU company', 'AU', 'AUD')


DECLARE @OrgA UNIQUEIDENTIFIER = '{OrgA}';
DECLARE @OrgAA UNIQUEIDENTIFIER = '3f22068a-98dd-43ad-a1e8-c52f0a4ac806';
DECLARE @OrgAB UNIQUEIDENTIFIER = 'd8d47546-8b9b-4965-85f4-0bb0edf84157';

DECLARE @OrgB UNIQUEIDENTIFIER = 'b86c5da8-01d6-443d-90e2-32fbbe9fc97b';
DECLARE @OrgBA UNIQUEIDENTIFIER = 'c0dfe776-9f83-434c-add0-ede6a7b6668a';
DECLARE @OrgBAA UNIQUEIDENTIFIER = 'e4402152-319a-4078-855b-f1e19c274d2d';

DECLARE @OrgC UNIQUEIDENTIFIER = 'e58afb38-663c-4114-a0e6-9a686ba79fba';


INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	(@OrgA, 'OrgA'),
	(@OrgAA, 'OrgAA'),
	(@OrgAB, 'OrgAB'),
	(@OrgB, 'OrgB'),
	(@OrgBA, 'OrgBA'),
	(@OrgBAA, 'OrgBAA'),
	(@OrgC, 'OrgC')

INSERT INTO dbo.OrgRelatedParty
	(PR_PK  , PR_PartyType, PR_OH_RelatedParty, PR_OH_Parent, PR_GC)
VALUES
	(newid(), 'MNG', @OrgA, @OrgAA, NULL),
	(newid(), 'MNG', @OrgA, @OrgAB, NULL),

	(newid(), 'MNG', @OrgB, @OrgBA, NULL),
	(newid(), 'MNG', @OrgBA, @OrgBAA, NULL),

	(newid(), 'XXX', @OrgA, @OrgC, NULL),
	(newid(), 'MNG', @OrgB, @OrgC, @CompanyPk)
";

			using (var command = TestConnection.Command(insertTestDataSql))
			{
				command.ExecuteNonQuery();
			}
		}

		void CreateMultipleOrgsAndRelatedParties(int count)
		{
			for (int i = 0; i < count; i++)
			{
				var insertSql = $@"
DECLARE @OrgParentId UNIQUEIDENTIFIER = newid()
DECLARE @OrgChildId UNIQUEIDENTIFIER = newid()

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc) VALUES (@OrgChildId, 'OrgC{i}', 'Full Name Child xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx {i}', getutcdate())
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc) VALUES (@OrgParentId, 'OrgP{i}', 'Full Name Parent xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx {i}', getutcdate())

INSERT INTO dbo.OrgRelatedParty
	(PR_PK  , PR_PartyType, PR_OH_RelatedParty, PR_OH_Parent, PR_GC)
VALUES
	(newid(), 'MNG', @OrgChildId, @OrgParentId, NULL)
				";
				using (var command = TestConnection.Command(insertSql))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		Dictionary<Guid, Tree> ReadAllTrees()
		{
			var selectSql = @"SELECT * FROM dbo.vw_OrgManagementGroupingNode";
			using (var command = TestConnection.Command(selectSql))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, Tree>();

					while (reader.Read())
					{
						var rootOrgPk = (Guid)reader["RootOrgPk"];

						Tree tree;
						if (!result.TryGetValue(rootOrgPk, out tree))
						{
							tree = new Tree(rootOrgPk);
							result[rootOrgPk] = tree;
						}

						var orgPk = (Guid)reader["OrgPk"];

						tree.OrgPks.Add(orgPk);
					}

					return result;
				}
			}
		}

		class Tree
		{
			public Tree(Guid rootOrgPk)
			{
				RootOrgPk = rootOrgPk;
			}

			public readonly Guid RootOrgPk;
			public List<Guid> OrgPks = new List<Guid>();
		}
		#endregion
	}
}

