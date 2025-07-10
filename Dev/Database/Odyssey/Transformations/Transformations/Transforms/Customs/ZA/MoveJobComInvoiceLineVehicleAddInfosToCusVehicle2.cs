using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.ZA;

public class MoveJobComInvoiceLineVehicleAddInfosToCusVehicle2 : DataTransformation
{
	public override string UserDescription => "Move ZA JI vehicle AddInfos to CusVehicle Part 2";

	const int BatchSize = 1000;
	const string LastProcessedChunkPKNameForJobComInvoiceLine = "MoveJobComInvoiceLineToCusVehicle.LastProcessedChunkPKForJobComInvoiceLine";
	const string LastProcessedChunkPKNameForCusClassPartPivot = "MoveJobComInvoiceLineToCusVehicle.LastProcessedChunkPKForCusClassPartPivot";
	const string TransformationHasRunToCompletion = "MoveJobComInvoiceLineToCusVehicle.JobComInvoiceLineTransformationRunToCompletion";

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		if (hasZACompany)
		{
			var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletion);
			if (statusValue != bool.TrueString)
			{
				TransformByChunks(JobComInvoiceLineSchema.Constants.TableName, TransformJobComInvoiceLineSQL, LastProcessedChunkPKNameForJobComInvoiceLine, token);
				ExtProperty.Database.Update(Db.Connection, TransformationHasRunToCompletion, bool.TrueString);
			}
			TransformByChunks(CusClassPartPivotSchema.Constants.TableName, TransformCusClassPartPivotSQL, LastProcessedChunkPKNameForCusClassPartPivot, token);
			ExtProperty.Database.Delete(Db.Connection, TransformationHasRunToCompletion);
		}
	}

	void TransformByChunks(string tableName, string transformSQL, string lastProcessedPkPropertyName, CancellationToken token)
	{
		var approxCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, tableName);
		var chunkingOperation = new GuidChunkingOperation(manager, BatchSize, approxCount, (lowerBound, upperBound) =>
		{
			Db.Connection.ExecuteNonQuery(transformSQL, cmd =>
			{
				cmd.AddParameter("StartGuid", SqlDbType.UniqueIdentifier, lowerBound);
				cmd.AddParameter("EndGuid", SqlDbType.UniqueIdentifier, upperBound);
			});
		}, lastProcessedPkPropertyName, token);
		chunkingOperation.DoChunking();
	}

	bool hasZACompany => DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName) &&
		Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'ZA'");

	const string TransformJobComInvoiceLineSQL = @"
INSERT INTO [dbo].[CusVehicle]
	([CVH_PK]
	,[CVH_AutoVersion]
	,[CVH_ClusterKey]
	,[CVH_SerialNumber]
	,[CVH_ModelName]
	,[CVH_VehicleIdentificationNumber]
	,[CVH_ManufacturedDate]
	,[CVH_ParentID]
	,[CVH_ParentTableCode]
	,[CVH_SystemCreateTimeUtc]
	,[CVH_SystemCreateUser]
	,[CVH_SystemLastEditTimeUtc]
	,[CVH_SystemLastEditUser]
	,[CVH_CarType]
	,[CVH_DataModel]
	,[CVH_EngineCapacity]
	,[CVH_EngineCapacityUQ]
	,[CVH_Color]
	,[CVH_SupplyMethod])
SELECT
	NEWID()
	,1
	,JI_ClusterKey
	,ISNULL(EngineNumber.Value, '')
	,ISNULL(Make.Value, '')
	,ISNULL(LEFT(VIN.Value, 17), '')
	,TRY_CAST(CONCAT(YearOfManufacture.Value, '-01-01') AS date)
	,JI_PK
	,'JI'
	,GETUTCDATE()
	,'~BP'
	,GETUTCDATE()
	,'~BP'
	,CASE WHEN VehicleType.Value = '' THEN NULL ELSE VehicleType.Value END
	,'ZA'
	,CASE WHEN TRY_CAST(EngineCapacity.Value AS int) BETWEEN 1 AND 32767 THEN TRY_CAST(EngineCapacity.Value AS smallint) ELSE NULL END
	,CASE WHEN TRY_CAST(EngineCapacity.Value AS int) BETWEEN 1 AND 32767 THEN 'CC' ELSE NULL END
	,CASE WHEN Colour.Value = '' THEN NULL ELSE Colour.Value END
	,CASE
		WHEN VehicleFormat.Value IN ('OTH', 'SKD', 'FBU', 'CKD') THEN VehicleFormat.Value
		WHEN VehicleFormat.Value = 'Other' THEN 'OTH'
		ELSE NULL
	END
FROM [dbo].[JobComInvoiceLine]
	WITH (FORCESEEK, INDEX (PK_UX__JI_PK))
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'EngineNumber') EngineNumber
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'Make') Make
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'VIN') VIN
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'YearOfManufacture') YearOfManufacture
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'VehicleType') VehicleType
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'EngineCapacity') EngineCapacity
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'Colour') Colour
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'VehicleFormat') VehicleFormat
WHERE JI_DataModel = 'ZA' AND
	NOT EXISTS (SELECT 1 FROM [dbo].[CusVehicle] WHERE CVH_ClusterKey = JI_ClusterKey AND CVH_ParentID = JI_PK) AND
	(EngineNumber.Value <> '' OR
	Make.Value <> '' OR
	VIN.Value <> '' OR
	YearOfManufacture.Value <> '' OR
	VehicleType.Value <> '' OR
	EngineCapacity.Value <> '' OR
	Colour.Value <> '' OR
	VehicleFormat.Value <> '')
	AND JI_PK BETWEEN @StartGuid AND @EndGuid;";

	const string TransformCusClassPartPivotSQL = @"
UPDATE Part
SET
	[CI_AddInfo] = AddInfoValue + '*VehicleFormat=OTH',
	[CI_SystemLastEditTimeUtc] = GETUTCDATE(),
	[CI_SystemLastEditUser] = '~BP'
FROM [dbo].[CusClassPartPivot] Part
	WITH (FORCESEEK, INDEX(PK_UX__CI_PK))
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(CI_AddInfo, 'VehicleFormat') VehicleFormat
WHERE CI_RN_NKCountry = 'ZA' AND VehicleFormat.Value = 'Other' AND CI_PK BETWEEN @StartGuid AND @EndGuid;";
}
