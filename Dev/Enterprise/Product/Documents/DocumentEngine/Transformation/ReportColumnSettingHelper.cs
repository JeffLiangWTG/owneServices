using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Transformation
{
	public class ReportColumnSettingHelper : IReportColumnSettingHelper
	{
		StringRegistryDataType dataType;
		StringRegistryDataType DataType => dataType ?? (dataType = new StringRegistryDataType());

		public void UpdateReportColumnSettingSheetNames(List<ReportColumnSettingSheetNameUpdateParameter> sheetNameUpdateConfig)
		{
			foreach (var config in sheetNameUpdateConfig)
			{
				var reportColumnSettings = LoadReportColumnSettings(config.ReportId);
				foreach (var columnSetting in reportColumnSettings)
				{
					foreach (Worksheet workSheet in columnSetting.Value.Worksheets)
					{
						if (config.ModifiedSheetNameMapping.ContainsKey(workSheet.Name))
						{
							workSheet.Name = config.ModifiedSheetNameMapping[workSheet.Name];
						}
					}
					SaveReportColumnSettings(columnSetting);
				}
			}
			UpdateScheduledReportSheetNames(sheetNameUpdateConfig);
		}

		void UpdateScheduledReportSheetNames(List<ReportColumnSettingSheetNameUpdateParameter> sheetNameUpdateConfig)
		{
			var renamer = new ReportScheduleTaskFilterAndSortNamesRenamer(Db.Connection);
			foreach (var parameter in sheetNameUpdateConfig)
			{
				renamer.Rename(GetReportsFilter(parameter.ReportId), null, null, null, null, null, null, null,
					parameter.ModifiedSheetNameMapping, null, null, null);
			}
		}

		IEnumerable<Tuple<SchemaColumn, object>> GetReportsFilter(Guid reportMenuItemPk)
		{
			return new[]
			{
				Tuple.Create<SchemaColumn, object>(StmScheduleTaskSchema.S5_ParentTableCode, "SU"),
				Tuple.Create<SchemaColumn, object>(StmScheduleTaskSchema.S5_ParentID, reportMenuItemPk),
			};
		}

		public Dictionary<Guid, ReportColumnSettings> LoadReportColumnSettings(Guid reportId)
		{
			var result = new Dictionary<Guid, ReportColumnSettings>();
			using (var command = Db.Connection.Command(GetConfigDataSQL))
			{
				command.AddParameterBasedOnDbColumn("@Owner", reportId, StmDataSchema.SD_Owner);
				command.AddParameterBasedOnDbColumn("@Name", ReportColumnSettingRegistryPrefixHelper.RegistryKeyPrefix, StmDataSchema.SD_Name);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var pk = (Guid)reader[StmDataSchema.Constants.PK];
						var binaryData = reader[StmDataSchema.Constants.SD_BinaryValue] as byte[];
						if (binaryData != null && binaryData.Length > 0)
						{
							var xml = DataType.Deserialise(binaryData);
							var manager = new ColumnConfigurationsManager(reportId, false);
							var parameters = new ColumnSettingXMLVersionUpgraderCurrentVersionParameters(manager);
							var reportColumnSetting = ColumnHeadingListSerialiser.Deserialise(xml, parameters);
							result.Add(pk, reportColumnSetting);
						}
					}
				}
			}
			return result;
		}

		void SaveReportColumnSettings(KeyValuePair<Guid, ReportColumnSettings> reportColumnSetting)
		{
			var xml = ColumnHeadingListSerialiser.Serialise(reportColumnSetting.Value);
			var binaryData = DataType.Serialise(xml);
			using (var command = Db.Connection.Command(SetConfigDataSQL))
			{
				command.AddParameterBasedOnDbColumn("@PK", reportColumnSetting.Key, StmDataSchema.PK);
				command.AddParameterBasedOnDbColumn("@BinaryValue", binaryData, StmDataSchema.SD_BinaryValue);
				command.ExecuteNonQuery();
			}
		}

		const string GetConfigDataSQL = @"
SELECT SD_PK, SD_BinaryValue FROM dbo.StmData 
WHERE 
SD_Owner = @Owner AND SD_Name like CONCAT(@Name, '_%')";

		const string SetConfigDataSQL = @"
UPDATE dbo.StmData 
SET SD_BinaryValue = @BinaryValue
WHERE 
SD_PK = @PK";
	}
}
