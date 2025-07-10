using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IOrganisationTaxRateImportDataProvider
	{
		Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> RegNumbersToImport(BusinessObjectFactory factory, ZGuid taxConfigurationPK);
	}

	class OrganisationTaxRateImportDataProvider : IOrganisationTaxRateImportDataProvider
	{
		Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> IOrganisationTaxRateImportDataProvider.RegNumbersToImport(BusinessObjectFactory factory, ZGuid taxConfigurationPK)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory), "Invalid argument, Factory argument is null.");
			}

			if (taxConfigurationPK.IsEmpty || !taxConfigurationPK.IsValid)
			{
				return new Dictionary<ZString, IReadOnlyList<(ZString, ZString, ZGuid)>>();
			}

			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@NKCodeCountry", "AR", OrgCusCodeSchema.OK_RN_NKCodeCountry),
				ZSqlParameter.New("@CodeType", "CUI", OrgCusCodeSchema.OK_CodeType),
				ZSqlParameter.New("@TaxConfigurationPK", taxConfigurationPK, AccOrgTaxConfigurationSchema.OTC_ETC),
			};

			var rawSQL = @"
SELECT
	OK_CustomsRegNo,
	OH_FullName,
	OH_Code,
	OTC_PK
FROM 
	dbo.AccOrgTaxConfiguration
	JOIN dbo.OrgCompanyData ON OTC_OB = OB_PK
	JOIN dbo.OrgHeader ON OB_OH = OH_PK
	JOIN dbo.OrgCusCode ON OH_PK = OK_OH
WHERE 
	OTC_ETC = @TaxConfigurationPK 
	AND OTC_IsActive = 1
	AND OH_IsActive = 1
	AND OK_RN_NKCodeCountry = @NKCodeCountry
	AND OK_CodeType = @CodeType
option(recompile)
";

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(rawSQL, parameters);

			return dynamicCollection
				.GroupBy(
					dynamicObject => (ZString)dynamicObject[OrgCusCodeSchema.OK_CustomsRegNo],
					dynamicObject => (
						organizationCode: (ZString)dynamicObject[OrgHeaderSchema.OH_Code],
						organizationName: (ZString)dynamicObject[OrgHeaderSchema.OH_FullName],
						orgTaxConfigPk: (ZGuid)dynamicObject[AccOrgTaxConfigurationSchema.PK]
					)
				)
				.ToDictionary(
					group => group.Key,
					group => (IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>)group.ToList()
				);
		}
	}
}
