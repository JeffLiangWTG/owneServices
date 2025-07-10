using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IndexShard))]
	sealed class IndexShardEntryTest : RegistryBusinessObjectTemplateTestCase<IndexShard>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BusinessObject GetNewBusinessObject() => new IndexShard();

		protected override IndexShard GetBusinessObjectToClone()
		{
			return new IndexShard() { IndexTableName = "TableA", IsOverridden = true, ShardIndex = 1 };
		}

		protected override IndexShard GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
