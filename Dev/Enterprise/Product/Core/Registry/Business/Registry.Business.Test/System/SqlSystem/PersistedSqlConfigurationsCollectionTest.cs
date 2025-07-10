using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PersistedSqlConfigurationsCollection))]
	sealed class PersistedSqlConfigurationsCollectionTest : RegistryBusinessObjectCollectionTestCase<PersistedSqlConfigurationsCollection>
	{
		protected override PersistedSqlConfigurationsCollection GetCollectionToTest()
		{
			return new PersistedSqlConfigurationsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PersistedSqlConfiguration();
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;

		public void TestAddSqlSystemConfiguration()
		{
			var collection = new PersistedSqlConfigurationsCollection();
			AssertEquals(0, collection.Count);

			var sqlConfig = collection.AddNew();
			sqlConfig.ConfigurationId = 1562;
			sqlConfig.ProposedValue = 1;

			AssertEquals(1, collection.Count);
			var config = collection[0];
			AssertEquals(sqlConfig, config);
		}
	}
}
