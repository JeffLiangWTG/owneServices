using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow;

public class UpdateGlowSavedColumnsToRenameETAETDArrivalDepartureDate : DataTransformation
{
	public override string UserDescription => "Update Glow saved columns to rename ETA/ETD/ARRIVALDATE/DEPARTUREDATE to ETALOCAL/ETDLOCAL/ARRIVALDATELOCAL/DEPARTUREDATELOCAL and ETAOFFSET/ETDOFFSET/ARRIVALDATEOFFSET to ETA/ETD/ARRIVALDATE";

	const int BatchSize = 200;
	const string FilteredBySDNameCTE = @"
WITH FilteredBySDName AS (
	SELECT
		SD_PK,
		SD_BinaryValue
	FROM
		dbo.StmData
	WHERE
		SD_Name IN (
			'GridSettings_SEP_Index_IJobContainer_ea2270f57619407fbe158ff252dd94f5_c81db99f-a712-4fd9-a765-6e789dab2ebb',
			'GridSettings_SDT_Index_IJobShipment_3cfd7b375ace42ddb34220951a47a161_67d54bfc-4eb7-4b7b-9ba4-edbd29c38cbe',
			'GridSettings_SDT_Index_IJobOrderHeader_0441029ff14c4112a19a2df58a95f64f_a76bd90d-1e0b-49a0-bc58-5471e4412347',
			'GridSettings_SDT_Index_IJobOrderHeader_6baeb77b18284dcd888be64f6e0fea2c_98581ec3-bdfc-4740-8207-4f99ce982031',
			'GridSettings_SEP_Index_IJobShipment_189a43c94e784fa28743ce5e22a8951e_490465d1-5497-4aa9-bba8-fbac663311f4',
			'GridSettings_SEP_Index_IJobOrderHeader_417d399aff514143880c6e9e3fef4226_a6e4ff80-5cbe-426d-89ad-ae0b7fa55998',
			'GridSettings_SEP_Index_NeoDashboard_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4',
			'GridSettings_SEP_Index_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f',
			'GridSettings_SDT_Index_WTJ_3b2982abef5141ba94b8093d1ff752ae_766ae519-8900-4dd3-8b49-06d65eba921f',
			'GridSettings_SDT_Index_IJobShipment_fad31d07b17b44958d9f68f90cef1101_47687897-4072-40d4-999e-008845b7beda',
			'GridSettings_SDT_Index_IJobDeclaration_560bd2c23b634bb683235688d0f9f3de_000ab061-d6ca-4dbc-bb02-0d45bf101896',
			'GridSettings_Search_IJobContainer',
			'GridSettings_Search_IJobShipment',
			'GridSettings_Search_IJobOrderHeader',
			'GridSettings_Search_IHVLVConsignment'
		) OR
		SD_Name LIKE 'GridSettings[_]Search[_]Index[_]IJobShipment[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]Index[_]IJobOrderHeader[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]Index[_]WTJ[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]Index[_]IHVLVConsignment[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]Index[_]NeoDashboard[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]Index[_]IJobContainer[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]IJobShipment[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]IJobOrderHeader[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]WTJ[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]IHVLVConsignment[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]NeoDashboard[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]IJobContainer[_]%' OR
		SD_Name LIKE 'GridSettings[_]Search[_]IJobDeclaration[_]%'
)
";
	const string PreviousTransformRunCheckSql = $@"
{FilteredBySDNameCTE}
SELECT TOP 1
	SD_PK
FROM
	FilteredBySDName
WHERE
	dbo.CLRUncompressAsString(SD_BinaryValue) LIKE '%<FieldName>%LOCAL</FieldName>%'
	AND dbo.CLRUncompressAsString(SD_BinaryValue) NOT LIKE '%<FieldName>%OFFSET</FieldName>%'
";

	protected override void OfflinePostUpgradeTransform()
	{
		if (Db.Connection.ExecuteScalar(PreviousTransformRunCheckSql) == null)
		{
			var currIndex = 0;
			var hasMoreBatches = true;
			var resultTable = GetStmDataTable();
			var totalRows = resultTable.Rows.Count;
			var columnNameRegexMapping = new List<string> { "ETA", "ETD", "ARRIVALDATE", "DEPARTUREDATE", "ETAOFFSET", "ETDOFFSET", "ARRIVALDATEOFFSET" }
		.ToDictionary(static c => c, c => new Regex($"<GridColumnDefinition>\\s*<FieldName>{c}<\\/FieldName>\\s*(<Width>\\s*.*<\\/Width>|<Width\\/>)\\s*<\\/GridColumnDefinition>"));

			while (hasMoreBatches)
			{
				var batchTable = resultTable.AsEnumerable().Skip(currIndex).Take(BatchSize);

				var updatedValues = new List<(Guid pk, string newSDValue)>();
				foreach (var row in batchTable)
				{
					var sdValueXml = (string)row["SD_Value"];
					var pk = (Guid)row[StmDataSchema.Constants.PK];
					var updateRequired = false;

					var columnNamesRenameAddLocal = new List<string> { "ETA", "ETD", "ARRIVALDATE", "DEPARTUREDATE" };
					var matchedColumnName = columnNamesRenameAddLocal.FirstOrDefault(columnName => columnNameRegexMapping[columnName].IsMatch(sdValueXml));
					if (!string.IsNullOrEmpty(matchedColumnName))
					{
						sdValueXml = sdValueXml.Replace($"<FieldName>{matchedColumnName}</FieldName>", $"<FieldName>{matchedColumnName}LOCAL</FieldName>");
						updateRequired = true;
					}

					var columnNamesRenameRemoveOffset = new List<string> { "ETAOFFSET", "ETDOFFSET", "ARRIVALDATEOFFSET" };
					matchedColumnName = columnNamesRenameRemoveOffset.FirstOrDefault(columnName => columnNameRegexMapping[columnName].IsMatch(sdValueXml));
					if (!string.IsNullOrEmpty(matchedColumnName))
					{
						sdValueXml = sdValueXml.Replace($"<FieldName>{matchedColumnName}</FieldName>", $"<FieldName>{matchedColumnName.Replace("OFFSET", "")}</FieldName>");
						updateRequired = true;
					}

					if (updateRequired)
					{
						updatedValues.Add((pk, sdValueXml));
					}
				}

				if (updatedValues.Count > 0)
				{
					UpdateStmData(updatedValues);
				}

				hasMoreBatches = currIndex + BatchSize < totalRows;
				if (hasMoreBatches)
				{
					currIndex += BatchSize;
				}
			}
		}
	}

	DataTable GetStmDataTable()
	{
		var querySQL = $@"
{FilteredBySDNameCTE}
SELECT
	SD_PK,
	dbo.CLRUncompressAsString(SD_BinaryValue) AS SD_Value
FROM
	FilteredBySDName
WHERE
	dbo.CLRUncompressAsString(SD_BinaryValue) LIKE '<GridSettings%'";

		using (var cmd = Db.Connection.Command(querySQL))
		{
			return DataUtils.GetDataTableFromCommand(cmd);
		}
	}

	void UpdateStmData(List<(Guid PK, string SdValueXml)> newValues)
	{
		var newValueCases = string.Empty;
		for (var i = 0; i < newValues.Count; i++)
		{
			newValueCases += $"WHEN SD_PK = @pk{i} THEN dbo.CLRCompressStringAsBytes(@sdValue{i})\n";
		}

		var pkList = string.Join(", ", newValues.Select(x => $"\'{x.PK}\'"));
		var updateStmDataSql = $@"
UPDATE dbo.StmData
SET SD_BinaryValue =
	CASE
		{newValueCases}
		ELSE SD_BinaryValue
	END,
	SD_SystemLastEditTimeUtc = GetUtcDate(),
	SD_SystemLastEditUser = '~BP'
WHERE
	SD_PK IN ({pkList})
";

		using (var cmd = Db.Connection.Command(updateStmDataSql))
		{
			for (var i = 0; i < newValues.Count; i++)
			{
				cmd.AddParameterBasedOnDbColumn($"@pk{i}", newValues[i].PK, StmDataSchema.PK);
				cmd.AddParameter($"@sdValue{i}", SqlDbType.VarChar, newValues[i].SdValueXml);
			}
			cmd.ExecuteNonQuery();
		}
	}
}
