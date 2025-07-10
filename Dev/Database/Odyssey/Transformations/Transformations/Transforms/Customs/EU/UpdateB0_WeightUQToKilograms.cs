using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

sealed class UpdateB0_WeightUQToKilograms : DataTransformation
{
	const int ChunkSize = 1_000;

	const string LastProcessedChunkPkName = $"{nameof(UpdateB0_WeightUQToKilograms)}.{nameof(LastProcessedChunkPkName)}";

	const string EUCompanyCountryCodes = "('BE', 'CH', 'DE', 'ES', 'FR', 'GB', 'IE', 'IT', 'NL', 'NO', 'PL', 'TR')";

	const string SelectEuCompaniesQueryText = $"FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode IN {EUCompanyCountryCodes}";

	const string UpdateGrossWeightUnitOfMeasureQueryText =
		$"""
		UPDATE b0
		SET 
			B0_WeightUQ = 'KG',
			B0_SystemLastEditTimeUtc = GetUtcDate(),
			B0_SystemLastEditUser = '~BP'
		FROM dbo.CusInBondBill AS b0
		INNER JOIN dbo.CusInBondHeader AS bh ON bh.BH_PK = b0.B0_BH	
		INNER JOIN dbo.GlbBranch AS gb ON gb.GB_PK = bh.BH_GB 
		INNER JOIN dbo.GlbCompany AS gc ON gc.GC_PK = gb.GB_GC 
		WHERE b0.B0_WeightUQ = ''
			AND gc.GC_RN_NKCountryCode IN {EUCompanyCountryCodes}
			AND b0.B0_BH BETWEEN @StartGuid AND @EndGuid;
		""";

	public override string UserDescription => "Set NCTS Transit Movement Bill Gross Weight Unit of Measure to KG";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		if (!Db.Connection.Exists(SelectEuCompaniesQueryText))
		{
			return;
		}

		var approximateRowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, CusInBondBillSchema.Constants.TableName);
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
