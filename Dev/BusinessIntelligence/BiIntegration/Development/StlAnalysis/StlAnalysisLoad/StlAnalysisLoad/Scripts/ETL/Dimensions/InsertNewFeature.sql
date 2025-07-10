-- rollback -- commit
-- SELECT * FROM DimLicensedFeature ORDER BY FeatureId DESC
-- SELECT * FROM DimLicensedFeature ORDER BY FeatureCode

INSERT DimLicensedFeature
	(
		FeatureCode, RoleName, ModuleName, FunctionName, FeatureName, IsSystemLevel, IsCompanyLevel,
		StlBasis, StlEntityCounted, StlUnitWeight,
		StlQuery, StlRules, DataSource, StlDetailSqlExpression, StlDetailCountExpression
	)
	VALUES
	(
		'Code',
		'RoleName',
		'ModuleName',
		'FunctionName',
		'FeatureName',
		0, 1,
		'Transactional', 'JobHeader', 1,
		'--',
		'<rules/>',
		'CLIENT',
		'',
		''
	)

-- FunctionName MUST be unique across Modules
IF exists (SELECT FunctionName FROM dbo.DimLicensedFeature GROUP BY FunctionName HAVING min(ModuleName) <> max(ModuleName))
	RAISERROR('Duplicated Function', 16, 1);

-- ModuleName MUST be unique across Roles
IF exists (SELECT ModuleName FROM dbo.DimLicensedFeature GROUP BY ModuleName HAVING min(RoleName) <> max(RoleName))
	RAISERROR('Duplicated Function', 16, 1);
