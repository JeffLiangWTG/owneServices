using System;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SqlSystemConfigurationRegistryItem))]
	sealed class SqlSystemConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<PersistedSqlConfigurationsCollection>
	{
		protected override StronglyTypedRegistryItem<PersistedSqlConfigurationsCollection, PersistedSqlConfigurationsCollection> GetNewRegistryItem()
		{
			return new SqlSystemConfigurationRegistryItem();
		}

		public void TestLoadPersistedSqlConfigurationsAreSameAsSavedInTheRegistry()
		{
			var configuredValues = new PersistedSqlConfigurationsCollection();

			var config1 = configuredValues.AddNew();
			config1.ConfigurationId = 1536;
			config1.ProposedValue = 0;

			var config2 = configuredValues.AddNew();
			config2.ConfigurationId = 1562;
			config2.ProposedValue = 1;

			SystemDataRegistry.Instance.SqlSystemConfigurations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configuredValues);

			using (var cmd = TestConnection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @name"))
			{
				cmd.AddParameterBasedOnDbColumn("@name", "SqlSystemConfigurations", StmDataSchema.SD_Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var bytes = reader[StmDataSchema.Constants.SD_BinaryValue];
						if (bytes == DBNull.Value)
						{
							Fail("Invalid value for SqlSystemConfigurations registry");
						}
						else
						{
							var registryValue = new PersistedSqlConfigurationsDataType().Deserialise((byte[])bytes);
							AssertEquals(configuredValues, registryValue);
						}
					}
					else
					{
						Fail("No entry for SqlSystemConfigurations registry");
					}
				}
			}
		}
	}
}
