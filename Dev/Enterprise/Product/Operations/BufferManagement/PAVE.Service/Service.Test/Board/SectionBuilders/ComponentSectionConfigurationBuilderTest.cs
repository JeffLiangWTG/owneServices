using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Service.Test
{
	public class ComponentSectionConfigurationBuilderTest : ComponentSectionConfigurationBuilderTestBase
	{
		public void TestHits_WhenGetConfiguration()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var board = system.Boards.AddNew();
			var boardPK = board.PK.ToGuid();

			for (int i = 0; i < 3; i++)
			{
				var staff = BMSTestHelper.GetOrCreateStaff(Factory, "St" + i);
				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Desc = "GROUP" + i;
				var capability = BMSTestHelper.CreateCapability(Factory, description: "Capability" + 1);
				var tagMag = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(Factory, "FL" + i), "TG" + i, description: "Tag Magnitude" + i);

				CreateBufferSection(board, buffer, staff, group, capability, tagMag, "additionalBuffer" + i, addAcceptabilityBands: false);
				CreateBucketSection(board, bucket, staff, group, capability, tagMag, "additionalBucket" + i);
			}

			Factory.Save();

			var hits = new Dictionary<string, int>
			{
				{ BMBoard.Schema.TableName, 1 },
				{ BMBoardSection.Schema.TableName, 1 },
				{ BMComponent.Schema.TableName, 2 },
				{ GlbCapability.Schema.TableName, 3 },
				{ GlbGroup.Schema.TableName, 3 },
				{ GlbStaff.Schema.TableName, 1 },
				{ TagDefinition.Schema.TableName, 1 },
				{ TagMagnitude.Schema.TableName, 3 },
				{ BMBoardSectionAdditionalComponent.Schema.TableName, 1 },
				{ BMBoardSectionChannel.Schema.TableName, 1 }
			};

			using (AssertDbHitsForAllFactories(hits, ignoreHitsFromTablesCachedInUberFactory: false, includeFactoryPredicate: f => f.NameForDebugging.StartsWith(nameof(BoardService))))
			using (TestConnection.TrackExecutedCommands())
			{
				Service.GetConfiguration(boardPK);

				var staffQueries = TestConnection.ExecutedCommands.Where(q => q.Contains("FROM dbo.GlbStaff") || q.Contains("FROM [GlbStaff]")).ToArray();
				AssertEquals(1, staffQueries.Length);
				Assert("Should not load ProfilePhoto", staffQueries.First().Contains("case when GS_ProfilePhoto is null then null when datalength(GS_ProfilePhoto) < 1024 then GS_ProfilePhoto else 0x0000 end"));
			}
		}
	}
}
