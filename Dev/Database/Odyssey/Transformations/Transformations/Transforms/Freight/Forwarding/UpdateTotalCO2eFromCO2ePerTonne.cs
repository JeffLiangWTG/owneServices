using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding;

public class UpdateTotalCO2eFromCO2ePerTonne : DataTransformation
{
	public override string UserDescription => "Update TotalCO2e from CO2ePerTonne and Jobs Weight";

	protected override void OfflinePostUpgradeTransform()
	{
		Db.Connection.ExecuteNonQuery(RawSQL);
	}

	const string RawSQL = @"
BEGIN TRY
	-- Shipment & Quick Booking
	UPDATE dbo.JobCO2e
	SET
		JCO_TotalCO2e = (SELECT Value FROM dbo.ConvertWeight(JS.JS_ActualWeight, JS.JS_UnitOfWeight, 'T')) * JCO_CO2ePerTonneInKg,
		JCO_SystemLastEditTimeUtc = GetUtcDate(),
		JCO_SystemLastEditUser = '~BP'
	FROM dbo.JobCO2e
	JOIN dbo.JobShipment JS ON JCO_ParentID = JS.JS_PK
	WHERE
		JCO_ParentTableCode IN ('JS', 'VB')
		AND JCO_TotalCO2e = 0
		AND JCO_CO2ePerTonneInKg > 0;

	-- Consol
	WITH ConsolWeights AS
	(
		SELECT
			JK_PK AS JK_PK,
			(SELECT Value FROM dbo.ConvertWeight(JK_TotalShipmentActWeightCheck, IIF(JK_TotalShipmentChargeableUnit = '', 'KG', JK_TotalShipmentChargeableUnit), 'T')) AS Weight,
			SUM(ISNULL(ShipmentWeight.Value, 0)) AS TotalShipmentWeight
		FROM dbo.JobConsol
		LEFT JOIN dbo.JobConShipLink ON JN_JK = JK_PK
		LEFT JOIN dbo.JobShipment JS ON JN_JS = JS.JS_PK
		LEFT JOIN dbo.JobShipment MasterJS ON MasterJS.JS_PK = JS.JS_JS_ColoadMasterShipment
		OUTER APPLY dbo.ConvertWeight(JS.JS_ActualWeight, JS.JS_UnitOfWeight, 'T') AS ShipmentWeight
		WHERE
			JS.JS_PK IS NULL
			OR
			(
				JS.JS_IsCancelled = 0
				AND
				(
					MasterJS.JS_PK IS NULL
					OR MasterJS.JS_ShipmentType = 'BCN'
					OR NOT EXISTS (SELECT JN_PK FROM JobConShipLink WHERE JN_JS = MasterJS.JS_PK AND JN_JK = JK_PK)
				)
			)
		GROUP BY JK_PK, JK_TotalShipmentActWeightCheck, JK_TotalShipmentChargeableUnit
	)
	UPDATE dbo.JobCO2e
	SET
		JCO_TotalCO2e = IIF(CW.TotalShipmentWeight = 0, CW.Weight, CW.TotalShipmentWeight) * JCO_CO2ePerTonneInKg,
		JCO_SystemLastEditTimeUtc = GetUtcDate(),
		JCO_SystemLastEditUser = '~BP'
	FROM dbo.JobCO2e
	JOIN dbo.JobConsol JK ON JCO_ParentID = JK.JK_PK
	JOIN ConsolWeights CW ON JK.JK_PK = CW.JK_PK
	WHERE
		JCO_ParentTableCode = 'JK'
		AND JCO_TotalCO2e = 0
		AND JCO_CO2ePerTonneInKg > 0;

	-- Booking with quote & One off quote
	UPDATE dbo.JobCO2e
	SET
		JCO_TotalCO2e = IIF
		(
			JS.JS_PK IS NULL,
			(SELECT Value FROM dbo.ConvertWeight(TT.TT_ActualWeight, TT.TT_UnitOfWeight, 'T')),
			(SELECT Value FROM dbo.ConvertWeight(JS.JS_ActualWeight, JS.JS_UnitOfWeight, 'T'))
		) * JCO_CO2ePerTonneInKg,
		JCO_SystemLastEditTimeUtc = GetUtcDate(),
		JCO_SystemLastEditUser = '~BP'
	FROM dbo.JobCO2e
	JOIN dbo.RatingHeader TH ON JCO_ParentID = TH.TH_PK AND TH.TH_OneTimeQuote = 1
	JOIN dbo.RateOneOffShipment TT ON TT.TT_TH = TH_PK
	LEFT JOIN dbo.JobShipment JS ON JS.JS_TH_OneTimeQuote = TH.TH_PK AND JS_IsBooking = 1
	WHERE
		JCO_ParentTableCode = 'VB'
		AND JCO_TotalCO2e = 0
		AND JCO_CO2ePerTonneInKg > 0;
END TRY
BEGIN CATCH
	THROW
END CATCH
";
}
