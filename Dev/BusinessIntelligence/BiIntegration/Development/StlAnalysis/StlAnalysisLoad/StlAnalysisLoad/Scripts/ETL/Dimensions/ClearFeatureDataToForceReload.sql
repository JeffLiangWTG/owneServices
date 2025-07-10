-- rollback -- commit
-- SELECT * FROM dbo.DimLicensedFeature ORDER BY FeatureId
-- SELECT * FROM dbo.ControlLastLoad ORDER BY FeatureId
-- SELECT * FROM dbo.FactTransaction ORDER BY FeatureId

DECLARE @FeatureIdToDelete int = (
	SELECT FeatureId FROM dbo.DimLicensedFeature
	WHERE RoleName = '***'
	AND ModuleName = '***'
	AND FunctionName = '***'
	AND FeatureName = '***'
);

DELETE dbo.ControlLastLoad WHERE FeatureId = @FeatureIdToDelete;
DELETE dbo.FactTransaction WHERE FeatureId = @FeatureIdToDelete;
