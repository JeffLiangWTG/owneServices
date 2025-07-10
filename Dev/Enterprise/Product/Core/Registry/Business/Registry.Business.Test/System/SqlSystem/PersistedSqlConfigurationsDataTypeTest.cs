using System.Linq;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PersistedSqlConfigurationsDataType))]
	sealed class PersistedSqlConfigurationsDataTypeTest : RegistryDataTypeTestCase<PersistedSqlConfigurationsDataType>
	{
		protected override PersistedSqlConfigurationsDataType GetNewDataType()
		{
			return new PersistedSqlConfigurationsDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = NewTestCollection();
			var collection2 = new PersistedSqlConfigurationsCollection();
			var sqlConfig = collection2.AddNew();

			sqlConfig.ConfigurationId = 1536;
			sqlConfig.ProposedValue = 0;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, new PersistedSqlConfigurationsDataType().Serialise(collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, new PersistedSqlConfigurationsDataType().Serialise(collection2)),
			};
		}

		public void TestSqlSystemConfigurationDataTypeSerialization()
		{
			var collection = NewTestCollection();
			var dataType = new PersistedSqlConfigurationsDataType();
			var bytes = dataType.Serialise(collection);
			var deserialized = dataType.Deserialise(bytes);

			foreach (var config in collection)
			{
				Assert(deserialized.Any(x => x.GetHashCode() == config.GetHashCode()));
			}
		}

		static PersistedSqlConfigurationsCollection NewTestCollection()
		{
			var collection = new PersistedSqlConfigurationsCollection();
			var sqlConfig = collection.AddNew();

			sqlConfig.ConfigurationId = 1562;
			sqlConfig.ProposedValue = 1;

			return collection;
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
