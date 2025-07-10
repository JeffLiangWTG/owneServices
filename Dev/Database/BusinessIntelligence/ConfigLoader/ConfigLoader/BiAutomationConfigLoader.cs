using System.Linq;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(CargoWise.Bi.Common.BiConstants))]

namespace CargoWise.Bi.ConfigLoader
{
	public class BiAutomationConfigLoader
	{
		#region Configuration Functionalities

		public static bool AuditTableExists(string sourceSchema, string sourceTable)
		{
			return Instance.ConfigData.CdcTableConfig
				.Any(t =>
					t.SourceSchema == sourceSchema &&
					t.SourceTable == sourceTable &&
					t.TableInAudit);
		}

		public static bool ColumnInAuditTableExists(string sourceSchema, string sourceTable, string sourceColumn)
		{
			return Instance.ConfigData.CdcTableConfig
				.Any(t =>
					t.SourceSchema == sourceSchema &&
					t.SourceTable == sourceTable &&
					t.GetCdcColumnConfigRows().Any(c =>
						c.SourceColumn == sourceColumn &&
						c.ColumnInAudit));
		}

		#endregion

		public static BiAutomationConfigLoader Instance
		{
			get
			{
				return instance ?? (instance = new BiAutomationConfigLoader());
			}
		}

		[ThreadSafe]
		static BiAutomationConfigLoader instance;

		public BiConfigurationData ConfigData
		{
			get
			{
				return configurationData ?? (configurationData = new BiConfigurationData());
			}
		}
		BiConfigurationData configurationData;

		public void ResetConfiguration()
		{
			configurationData = null;
		}
	}
}
