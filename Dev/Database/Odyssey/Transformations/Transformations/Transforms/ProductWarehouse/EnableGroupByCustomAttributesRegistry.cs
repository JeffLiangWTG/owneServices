using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public sealed class EnableGroupByCustomAttributesRegistry : RegistryDataTransformation
	{
		public override string UserDescription => "Populates Group Ordered Inventory By Custom Attributes registry item for branches using Custom Attributes.";
		const string LastProcessedChunkPKName = "EnableGroupByCustomAttributesRegistry.LastProcessedChunkPK";
		const string CurrentCountName = "EnableGroupByCustomAttributesRegistry.CurrentCount";
		const int BatchSize = 1000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;
			var currentCount = long.Parse(ExtProperty.Database.Select(Db.Connection, CurrentCountName) ?? "0");

			var totalDockets = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsDocketSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(BatchSize, totalDockets, lastProcessedPK);

			var stopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				using (var command = Db.Connection.Command(PopulateRegistryItemSQL))
				{
					command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, chunk.LowerBound);
					command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, chunk.UpperBound);
					command.ExecuteNonQuery();
				}

				currentCount = Math.Min(currentCount + BatchSize, totalDockets);

				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					manager?.ShowInfoMessage($"Processed {currentCount} of {totalDockets} Jobs.");

					ExtProperty.Database.Update(Db.Connection, CurrentCountName, currentCount.ToString());
					ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, chunk.UpperBound.ToString());

					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, CurrentCountName);
			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}

		const string PopulateRegistryItemSQL =
@"
;WITH CustomBranches AS (
	SELECT DISTINCT
		WW_GB_RelatedCompanyBranch
	FROM
		dbo.WhsDocketLine
		JOIN dbo.WhsDocket ON WE_WD = WD_PK
		JOIN dbo.WhsWarehouse ON WD_WW_Whs = WW_PK
	WHERE
		WE_WD >= @FromPK
		AND WE_WD <= @ToPK
		AND NOT
		(
			WE_CustomAttrib1 = ''
			AND WE_CustomAttrib2 = ''
			AND WE_CustomAttrib3 = ''
			AND WE_CustomAttrib4 = ''
			AND WE_CustomAttrib5 = ''
			AND WE_CustomAttrib6 = ''
			AND WE_CustomTextBlob1 = ''
			AND WE_CustomFlag1 = 0
			AND WE_CustomFlag2 = 0
			AND WE_CustomFlag3 = 0
			AND WE_CustomFlag4 = 0
			AND WE_CustomFlag5 = 0
			AND WE_CustomDecimal1 = 0
			AND WE_CustomDecimal2 = 0
			AND WE_CustomDecimal3 = 0
			AND WE_CustomDecimal4 = 0
			AND WE_CustomDecimal5 = 0
			AND WE_CustomDate1 IS NULL
			AND WE_CustomDate2 IS NULL
			AND WE_CustomDate3 IS NULL
			AND WE_CustomDate4 IS NULL
			AND WE_CustomDate5 IS NULL
		)
		AND NOT EXISTS (
			SELECT 1
			FROM dbo.StmData
			WHERE
			SD_Owner = WW_GB_RelatedCompanyBranch
			AND SD_Name = 'GroupOrderedInventoryByCustomAttributes'
		)
)
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_BinaryValue, SD_Type, SD_IsLogged, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
SELECT 
	NEWID(),
	'GroupOrderedInventoryByCustomAttributes',
	WW_GB_RelatedCompanyBranch,
	CONVERT(VARBINARY(MAX), '0x5400720075006500', 1),
	'BOL',
	1,
	GETUTCDATE(),
	'~BP',
	GETUTCDATE(),
	'~BP'
FROM
	CustomBranches";
	}
}
