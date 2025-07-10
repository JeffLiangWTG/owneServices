using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.CW1.Resources;

namespace Enterprise.Registry.Business.Testing
{
	sealed class IndexShardValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTable()
		{
			var tableList = new IndexShardList();
			var indexShard = tableList.AddNew();
			indexShard.RunPreSaveValidation();
			AssertMandatoryValidationError(indexShard.IndexTableNameInfo, true);

			indexShard.IndexTableName = " ";
			AssertMandatoryValidationError(indexShard.IndexTableNameInfo, true);

			indexShard.IndexTableName = "ThisIsDefinitelyNotTheNameOfATable";
			Assert(indexShard.IndexTableNameInfo.GetErrors().Any(e => e.Message == IndexShard.InvalidTable));

			indexShard.IndexTableName = indexShard.IndexTableNames[0].Code;
			Assert(!indexShard.IndexTableNameInfo.HasErrors());

			var indexShard2 = tableList.AddNew();
			indexShard2.IndexTableName = indexShard.IndexTableNames[0].Code;
			AssertHasError("Table must be unique.", indexShard2.IndexTableNameInfo, "The Index Table Name has been duplicated and must be unique.");

			indexShard.RunPreSaveValidation();
			AssertHasError("Table must be unique.", indexShard.IndexTableNameInfo, "The Index Table Name has been duplicated and must be unique.");

			tableList.Remove(indexShard);
			indexShard2.RunPreSaveValidation();
			AssertNoErrors(indexShard2.IndexTableNameInfo);
		}

		public void TestValidateOverrideShard()
		{
			var mapEntry = new IndexShard() { ShardIndex = 1, IsOverridden = true };
			AssertMandatoryValidationError(mapEntry.ShardIndexInfo, false);

			mapEntry.ShardIndex = 0;
			Assert(!mapEntry.ShardIndexInfo.HasErrors());

			mapEntry.ShardIndex = -1;
			Assert(mapEntry.ShardIndexInfo.GetErrors().Any(e => e.Message == IndexShard.NegativeShard));
		}

		public void TestIndexTableNameReadOnly()
		{
			if (DefaultIndexShard.DefaultIndexShardMap.Count > 0)
			{
				var mapEntry = new IndexShard();
				mapEntry.IndexTableName = DefaultIndexShard.DefaultIndexShardMap.Keys.ToList()[0];
				Assert(mapEntry.IndexTableName_ReadOnly);
				mapEntry.IndexTableName = "ThisIsDefinitelyNotTheNameOfATable";
				Assert(!mapEntry.IndexTableName_ReadOnly);
			}
			else
			{
				Assert("No Default Index Shard.", true);
			}
		}

		public void TestOverriddenShardReadOnly()
		{
			var mapEntry = new IndexShard();
			mapEntry.IsOverridden = true;
			mapEntry.ShardIndex = 1;
			Assert(!mapEntry.ShardIndex_ReadOnly);
			mapEntry.IsOverridden = false;
			Assert(mapEntry.ShardIndex_ReadOnly);
			Assert(mapEntry.ShardIndex == 0);
		}
	}
}
