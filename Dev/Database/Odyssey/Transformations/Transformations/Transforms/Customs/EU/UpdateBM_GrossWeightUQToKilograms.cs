using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

sealed class UpdateBM_GrossWeightUQToKilograms : DataTransformation
{
	const int ChunkSize = 1_000;

	const string LastProcessedChunkPkName = $"{nameof(UpdateBM_GrossWeightUQToKilograms)}.{nameof(LastProcessedChunkPkName)}";

	const string EUCompanyCountryCodes = "('BE', 'CH', 'DE', 'ES', 'FR', 'GB', 'IE', 'IT', 'NL', 'NO', 'PL', 'TR')";

	const string SelectEuCompaniesQueryText = $"FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode IN {EUCompanyCountryCodes}";

	const string UpdateGrossWeightUnitOfMeasureQueryText =
		$"""
		UPDATE bm
		SET 
			BM_GrossWeightUQ = 'KG',
			BM_SystemLastEditTimeUtc = GetUtcDate(),
			BM_SystemLastEditUser = '~BP'
		FROM dbo.CusInBondMoveHeader AS bm
		INNER JOIN dbo.CusInBondHeader AS bh ON bh.BH_PK = bm.BM_BH	
		INNER JOIN dbo.GlbBranch AS gb ON gb.GB_PK = bh.BH_GB 
		INNER JOIN dbo.GlbCompany AS gc ON gc.GC_PK = gb.GB_GC 
		WHERE bm.BM_GrossWeightUQ = ''
			AND gc.GC_RN_NKCountryCode IN {EUCompanyCountryCodes}
			AND bh.BH_PK BETWEEN @StartGuid AND @EndGuid;
		""";

	public override string UserDescription => "Set NCTS Transit Movement Gross Weight Unit of Measure to KG";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		if (!Db.Connection.Exists(SelectEuCompaniesQueryText))
		{
			return;
		}

		var approximateRowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, CusInBondMoveHeaderSchema.Constants.TableName);
		var chunkingOperation = new GuidChunkingOperation(manager, ChunkSize, approximateRowCount, ProcessChunk, LastProcessedChunkPkName, token);
		chunkingOperation.DoChunking();
	}

	static void ProcessChunk(Guid lowerBound, Guid upperBound) =>
		Db.Connection.ExecuteNonQuery(UpdateGrossWeightUnitOfMeasureQueryText, command =>
		{
			command.AddParameter("StartGuid", SqlDbType.UniqueIdentifier, lowerBound);
			command.AddParameter("EndGuid", SqlDbType.UniqueIdentifier, upperBound);
		});
}
