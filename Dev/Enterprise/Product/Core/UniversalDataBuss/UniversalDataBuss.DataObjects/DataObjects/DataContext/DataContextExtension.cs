using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public static class DataContextExtension
	{
		public static string GetDataSourceKey(this IDataContextDataObject dataContext)
		{
			var result = new List<string>();

			if (dataContext != null)
			{
				var enterpriseServerAndCompanyIDs = dataContext.GetEnterpriseServerAndCompanyIDs();
				result.Add(enterpriseServerAndCompanyIDs.EnterpriseID);
				result.Add(enterpriseServerAndCompanyIDs.ServerID);
				result.Add(enterpriseServerAndCompanyIDs.CompanyCode);

				var dataSources = dataContext.DataSourceCollection;
				if (dataSources != null)
				{
					var dataSource = dataSources.FirstOrDefault();
					if (dataSource != null)
					{
						result.Add(dataSource.Type.GetValueOrDefault());
						result.Add(dataSource.Key.GetValueOrDefault());
					}
				}
			}

			return string.Join("|", result.ToArray());
		}

		public static string GetDataSources(this IDataContextDataObject dataContext)
		{
			var result = new List<string>();

			if (dataContext != null && dataContext.DataSourceCollection != null)
			{
				foreach (var dataSource in dataContext.DataSourceCollection)
				{
					result.Add(dataSource.Type.GetValueOrDefault() + " [" + dataSource.Key.GetValueOrDefault() + "]");
				}
			}

			return string.Join(", ", result.ToArray());
		}

		public static Country GetSourceCountry(this IDataContextDataObject dataContext)
		{
			var sourceCompany = dataContext.GetTargetCompany(); // TODO: Change this to the actual source
			return sourceCompany == null ? null : sourceCompany.Country;
		}

		public static Company GetTargetCompany(this IDataContextDataObject dataContext)
		{
			Company result = null;

			if (dataContext != null && dataContext.CodesMappedToTarget)
			{
				result = new Company() { Code = dataContext.CompanyCodeToImportInto };
			}

			return result;
		}

		public static IOrgHeader GetSourceOrganisation(this IDataContextDataObject dataContext, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			IOrgHeader result = null;

			if (dataContext != null)
			{
				var sendingPartyCode = dataContext.DataProviderForCodeMapping;
				if (!sendingPartyCode.IsEmpty)
				{
					var currentCompanyCodeMapper = ObjectFactory.New<IUniversalCodeMapper>("", logger, factory);
					ZString sendingPartyOrgCode = currentCompanyCodeMapper.GetMappedOrInput(sendingPartyCode, Constants.OrgPatternMatchOverrideRelationships.Organisation);
					if (!sendingPartyOrgCode.IsEmpty)
					{
						result = factory.LoadFromUniqueKey<IOrgHeader>(OrgHeaderSchema.OH_Code, sendingPartyOrgCode);
					}

					if (result == null)
					{
						logger.Log(LogType.Warning, string.Format("Could not find Sending Party Organisation with eHub Client ID '{0}'.", sendingPartyCode));
					}
				}
			}

			return result;
		}

		public static string GetEnterpriseCode(this IDataContextDataObject dataContext)
		{
			return dataContext.DataProviderForCodeMapping.SubstringSafe(0, 3);
		}

		public static string GetServerCode(this IDataContextDataObject dataContext)
		{
			return dataContext.DataProviderForCodeMapping.SubstringSafe(3, 3);
		}

		public static string GetCompanyCode(this IDataContextDataObject dataContext)
		{
			return dataContext.DataProviderForCodeMapping.SubstringSafe(6, 3);
		}
	}
}
