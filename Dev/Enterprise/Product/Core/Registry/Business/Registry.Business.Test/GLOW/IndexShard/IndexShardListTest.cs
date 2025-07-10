using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IndexShardList))]
	sealed class IndexShardListTest : RegistryBusinessObjectCollectionTemplateTestCase<IndexShardList>
	{
		#region AllowNew

		public void TestAllowNew()
		{
			Assert(Collection.AllowNew);
		}

		#endregion

		#region Overrides

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override IndexShardList GetCollectionToTest()
		{
			return new IndexShardList();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IndexShard();
		}

		#endregion
	}
}
