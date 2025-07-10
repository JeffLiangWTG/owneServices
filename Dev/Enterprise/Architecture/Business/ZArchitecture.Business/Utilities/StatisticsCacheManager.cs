using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using WTG.Statistics;

namespace Enterprise.ZArchitecture.Business
{
	public class StatisticsCacheManager : IStatisticsPersister
	{
		readonly StatisticsCacheSerializer statisticsCacheSerializer = new StatisticsCacheSerializer();

		public bool TryRetrieve(string schemaName, string tableName, out SqlHistogram[] histograms)
		{
			histograms = Array.Empty<SqlHistogram>();
			var result = true;

			var key = GetKey(schemaName, tableName);
			using (var command = Db.Connection.Command("select SD_BinaryValue from dbo.StmData where SD_Name = @Name AND SD_Owner IS NULL AND SD_DepartmentGuid IS NULL"))
			{
				command.AddParameterBasedOnDbColumn("@Name", key, StmDataSchema.SD_Name);
				if (command.ExecuteScalar() is byte[] serializedHistograms)
				{
					try
					{
						histograms = statisticsCacheSerializer.DeserializeHistograms(serializedHistograms);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						result = false;

#if DEBUG
						throw;
#endif
					}
				}
			}

			return result;
		}

		public void Save(string schemaName, string tableName, SqlHistogram[] histograms)
		{
			var key = GetKey(schemaName, tableName);
			if (histograms.Length > 0)
			{
				var serializedHistograms = statisticsCacheSerializer.SerializeHistograms(histograms);
				using (var command =
					Db.Connection.Command(@"
MERGE dbo.StmData as target
USING (VALUES (@Name)) AS source (Name)
ON (target.SD_Name = source.Name and target.SD_Owner IS NULL and target.SD_DepartmentGuid IS NULL)
WHEN MATCHED THEN 
	UPDATE SET target.SD_BinaryValue = @Value
WHEN NOT MATCHED BY TARGET THEN
	INSERT (SD_PK, SD_Name, SD_BinaryValue) values (newid(), @Name, @Value);"))
				{
					command.AddParameterBasedOnDbColumn("@Name", key, StmDataSchema.SD_Name);
					command.AddParameterBasedOnDbColumn("@Value", serializedHistograms, StmDataSchema.SD_BinaryValue);
					command.ExecuteNonQuery();
				}
			}
			else
			{
				using (var command =
					Db.Connection.Command(
						"DELETE FROM dbo.StmData WHERE SD_Name = @Name AND SD_Owner IS NULL AND SD_DepartmentGuid IS NULL"))
				{
					command.AddParameterBasedOnDbColumn("@Name", key, StmDataSchema.SD_Name);
					command.ExecuteNonQuery();
				}
			}
		}

		static string GetKey(string schemaName, string tableName)
		{
			return "SqlStatistics:" + schemaName + "." + tableName; // Key value for lookup in DB
		}
	}
}
