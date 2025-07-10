using System.Collections.Generic;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Schema
{
	sealed class EdwDatabasePopulatorForTest : EdwDatabasePopulator
	{
		public EdwDatabasePopulatorForTest(DbConnection connection)
			: base(connection)
		{ }

		public string GetDataTypeDefinition_Exposed(string dataType, int maxLength, int precision, int scale)
		{
			return GetDataTypeDefinition(dataType, maxLength, precision, scale);
		}

		public void PopulateCustomTableConfiguration_Exposed()
		{
			PopulateCustomTableConfiguration();
		}

		public IEnumerable<string> TablesForInitialLoad_Exposed
		{
			get
			{
				return tablesForInitialLoad;
			}
		}
	}
}
