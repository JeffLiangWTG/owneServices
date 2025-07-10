using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

public abstract class UpdateFilterAddArgumentTransformation : DataTransformation
{
	readonly short maxRecordsPerBatch = 1049; // SQL Server supports max 2100 parameters, 2 params per record

	public UpdateFilterAddArgumentTransformation(short maxRecordsPerBatch = 0)
	{
		if (maxRecordsPerBatch > 1049)
		{
			throw new ArgumentException($"{nameof(maxRecordsPerBatch)} should not be greater than 1049",
				nameof(maxRecordsPerBatch));
		}

		if (maxRecordsPerBatch > 0)
		{
			this.maxRecordsPerBatch = maxRecordsPerBatch;
		}
	}

	protected abstract string FilterDescription { get; }

	protected abstract string ModuleId { get; }

	protected abstract XElement ParameterToAdd { get; }

	protected override void OfflinePostUpgradeTransform()
	{
		var toUpdate = new Dictionary<Guid, XDocument>();

		var sql = ModuleFilterTransformationHelper.GetFilterDataByModuleAndDescriptionQuery(FilterDescription, ModuleId);

		using (var cmd = Db.Connection.Command(sql))
		using (var reader = cmd.ExecuteReader())
		{
			while (reader.Read())
			{
				XDocument filterDataXml;
				XDocument filterParameterData;
				try
				{
					filterDataXml = XDocument.Parse(Compressor.UncompressAsString((byte[])reader["FilterData"]));
					filterParameterData = XDocument.Parse(Compressor.UncompressAsString((byte[])reader["ModuleData"]));
				}
				catch (XmlException)
				{
					continue; // skip malformed filters
				}

				var isUpdated = ModuleFilterTransformationHelper.UpdateFilterParameter(filterDataXml, filterParameterData, FilterDescription, ParameterToAdd);

				if (isUpdated)
				{
					var pk = (Guid)reader["S0_PK"];
					toUpdate.Add(pk, filterParameterData);
				}
			}
		}

		if (toUpdate.Count > 0)
		{
			SaveChanges(toUpdate);
		}
	}

	void SaveChanges(Dictionary<Guid, XDocument> toUpdate)
	{
		var batches = toUpdate
			.Select((entry, index) => new { entry, index })
			.GroupBy(x => x.index / maxRecordsPerBatch)
			.Select(g => g.Select(x => x.entry).ToList())
			.ToList();

		const string declareTableSql = "DECLARE @ins TABLE (PK UNIQUEIDENTIFIER, FilterData VARBINARY(MAX));";
		const string updateSql = @"
UPDATE U SET
	S0_FilterDataValues = ins.FilterData,
	S0_SystemLastEditTimeUtc = GetUtcDate(),
	S0_SystemLastEditUser = 'E'
FROM dbo.StmModuleFilterUserData U
JOIN @ins ins ON U.S0_PK = ins.PK;
";
		var capacity = 300 + 68 * (batches.Count > 1 ? maxRecordsPerBatch : batches[0].Count);
		var sb = new StringBuilder(capacity);

		foreach (var batch in batches)
		{
			sb.Append(declareTableSql);

			var i = 0;
			for (; i < batch.Count; i++)
			{
				sb.Append("INSERT INTO @ins (PK, FilterData) VALUES (@PK")
					.Append(i)
					.Append(", @FilterData")
					.Append(i)
					.Append(");");
			}

			sb.Append(updateSql);

			using var command = Db.Connection.Command(sb.ToString());
			i = 0;
			foreach (var entry in batch)
			{
				command.AddParameter($"@PK{i}", SqlDbType.UniqueIdentifier, entry.Key);
				command.AddParameter($"@FilterData{i}", SqlDbType.VarBinary, Compressor.Compress(Encoding.UTF8.GetBytes(entry.Value.ToString())));
				i++;
			}

			command.ExecuteNonQuery();
			sb.Clear();
		}
	}
}
