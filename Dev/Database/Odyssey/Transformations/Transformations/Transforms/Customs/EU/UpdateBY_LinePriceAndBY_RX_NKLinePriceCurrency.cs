using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

sealed class UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency : DataTransformation
{
	public override string UserDescription => "Update BY_LinePrice and BY_RX_NKLinePriceCurrency for EU NCTS P5";

	protected override void OfflinePreUpgradeTransform()
	{
		DbObjectCreator.DropTriggerIfExists(Db.Connection, CusInBondCargoDescLinePriceAndCurrencyTriggerName);
		DbObjectCreator.DropTriggerIfExists(Db.Connection, GlbCompanyLinePriceCurrencyTriggerName);
	}

	protected override void OnlinePreUpgradeTransform()
	{
		if (!DbObjectCreator.TableExists(Db.Connection, CusInBondCargoDescSchema.Constants.TableName))
		{
			return;
		}

		DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_LinePrice.Name, "money", "0");
		DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_RX_NKLinePriceCurrency.Name, "varchar(3)", "''");

		if (!DbObjectCreator.TriggerExists(Db.Connection, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescLinePriceAndCurrencyTriggerName))
		{
			const string sql = $@"
				CREATE TRIGGER [dbo].[{CusInBondCargoDescLinePriceAndCurrencyTriggerName}]
				ON dbo.CusInBondCargoDesc
				FOR INSERT, UPDATE AS
				BEGIN
					IF (@@rowcount = 0) RETURN;

					SET NOCOUNT ON;

					IF UPDATE (BY_MonetaryValue)
					BEGIN
						UPDATE dbo.CusInBondCargoDesc
							SET BY_LinePrice = CargoDesc.BY_MonetaryValue,
							BY_RX_NKLinePriceCurrency = CASE GC_RN_NKCountryCode
								WHEN 'CH' THEN 'CHF'
								WHEN 'GB' THEN 'GBP'
								WHEN 'NO' THEN 'NOK'
								WHEN 'PL' THEN 'PLN'
								WHEN 'TR' THEN 'TRY'
								ELSE 'EUR'
							END,
							BY_SystemLastEditTimeUtc = GetUtcDate(),
							BY_SystemLastEditUser = 'E'
						FROM dbo.CusInBondCargoDesc as CargoDesc
						INNER JOIN inserted i ON CargoDesc.BY_PK = i.BY_PK
						INNER JOIN dbo.CusInBondBill ON CargoDesc.BY_ParentID = B0_PK
						INNER JOIN dbo.CusInBondHeader ON B0_BH = BH_PK
						INNER JOIN dbo.GlbBranch ON BH_GB = GB_PK
						INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK
						WHERE CargoDesc.BY_ParentTableCode = 'B0'
							AND GC_RN_NKCountryCode IN ('BE', 'CH', 'DE', 'ES', 'FR', 'GB', 'IE', 'IT', 'NL', 'NO', 'PL', 'TR')
							AND BH_ApplicationCode = 'NC5'
							AND BH_HeaderType = 'D';
					END;
				END;";
			Db.Connection.ExecuteNonQuery(sql);
		}

		if (DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName)
			&& !DbObjectCreator.TriggerExists(Db.Connection, GlbCompanySchema.Constants.TableName, GlbCompanyLinePriceCurrencyTriggerName))
		{
			const string sql = $@"
				CREATE TRIGGER [dbo].[{GlbCompanyLinePriceCurrencyTriggerName}]
				ON dbo.GlbCompany
				FOR UPDATE AS
				BEGIN
					IF (@@rowcount = 0) RETURN;

					SET NOCOUNT ON;

					IF UPDATE (GC_RN_NKCountryCode)
					BEGIN
							UPDATE dbo.CusInBondCargoDesc
							SET BY_RX_NKLinePriceCurrency = CASE GC_RN_NKCountryCode
									WHEN 'CH' THEN 'CHF'
									WHEN 'GB' THEN 'GBP'
									WHEN 'NO' THEN 'NOK'
									WHEN 'PL' THEN 'PLN'
									WHEN 'TR' THEN 'TRY'
									ELSE 'EUR'
								END,
							BY_SystemLastEditTimeUtc = GetUtcDate(),
							BY_SystemLastEditUser = 'E'
						FROM dbo.CusInBondCargoDesc CargoDesc
						INNER JOIN dbo.CusInBondBill ON CargoDesc.BY_ParentID = B0_PK
						INNER JOIN dbo.CusInBondHeader ON B0_BH = BH_PK
						INNER JOIN dbo.GlbBranch ON BH_GB = GB_PK
						INNER JOIN inserted ON GB_GC = GC_PK
						WHERE CargoDesc.BY_ParentTableCode = 'B0'
							AND GC_RN_NKCountryCode IN ('BE', 'CH', 'DE', 'ES', 'FR', 'GB', 'IE', 'IT', 'NL', 'NO', 'PL', 'TR')
							AND BH_ApplicationCode = 'NC5'
							AND BH_HeaderType = 'D';
					END;
				END;";

			Db.Connection.ExecuteNonQuery(sql);
		}

		if (Db.Connection.ExecuteScalar("SELECT TOP 1 1 FROM GlbCompany WHERE GC_RN_NKCountryCode IN ('BE', 'CH', 'DE', 'ES', 'FR', 'GB', 'IE', 'IT', 'NL', 'NO', 'PL', 'TR')") is not null)
		{
			const string sqlText = @"
				UPDATE dbo.CusInBondCargoDesc
				SET
					BY_LinePrice = BY_MonetaryValue,
					BY_RX_NKLinePriceCurrency = CASE GC_RN_NKCountryCode
						WHEN 'CH' THEN 'CHF'
						WHEN 'GB' THEN 'GBP'
						WHEN 'NO' THEN 'NOK'
						WHEN 'PL' THEN 'PLN'
						WHEN 'TR' THEN 'TRY'
						ELSE 'EUR'
					END,
					BY_SystemLastEditTimeUtc = GetUtcDate(),
					BY_SystemLastEditUser = 'E'
				FROM dbo.CusInBondCargoDesc
				INNER JOIN dbo.CusInBondBill ON BY_ParentID = B0_PK
				INNER JOIN dbo.CusInBondHeader ON B0_BH = BH_PK
				INNER JOIN dbo.GlbBranch ON BH_GB = GB_PK
				INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK
				WHERE BY_ParentTableCode = 'B0'
						AND GC_RN_NKCountryCode IN ('BE', 'CH', 'DE', 'ES', 'FR', 'GB', 'IE', 'IT', 'NL', 'NO', 'PL', 'TR')
						AND BH_ApplicationCode = 'NC5'
						AND BH_HeaderType = 'D'
						AND BY_ParentID BETWEEN @StartGuid AND @EndGuid;";

			const int batchSize = 1000;
			var totalCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, CusInBondCargoDescSchema.Constants.TableName);
			var lastProcessedPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var guidChunks = GuidChunker.GenerateChunks(batchSize, totalCount, lastProcessedPK);
			foreach (var chunk in guidChunks)
			{
				Db.Connection.ExecuteNonQuery(sqlText, cmd =>
				{
					cmd.AddParameter("StartGuid", SqlDbType.UniqueIdentifier, chunk.LowerBound);
					cmd.AddParameter("EndGuid", SqlDbType.UniqueIdentifier, chunk.UpperBound);
				});

				ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, chunk.UpperBound.ToString());
			}
		}
	}

	internal const string CusInBondCargoDescLinePriceAndCurrencyTriggerName = "TG_CusInBondCargoDesc_UpdateBY_LinePrice_BY_RX_NKLinePriceCurrency";
	internal const string GlbCompanyLinePriceCurrencyTriggerName = "TG_GlbCompany_UpdateBY_RX_NKLinePriceCurrency";
	const string LastProcessedChunkPKName = "UpdateBY_LinePriceAndBY_RX_NKLinePriceCurrency.LastProcessedChunkPKName";
}
