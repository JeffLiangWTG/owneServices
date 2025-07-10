using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IEntityMatcher
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		BusinessObject GetMatchingBusinessObject(IEntityMatcherContext matcherContext, string dataDefinitionName, IEnumerable<(string propertyName, string stringValue)> matchingValues, bool matchCreatedBizoOnly = false);
	}

	class EntityMatcher : IEntityMatcher
	{
		public BusinessObject GetMatchingBusinessObject(IEntityMatcherContext matcherContext, string dataDefinitionName, IEnumerable<(string propertyName, string stringValue)> matchingValues, bool matchCreatedBizoOnly = false)
		{
			var tableCode = matcherContext?.MappingDataModel.GetTableCode(dataDefinitionName);
			var enterpriseItemType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tableCode);
			var query = GetQuery(matcherContext?.MappingDataModel, dataDefinitionName, enterpriseItemType, matchingValues);
			query.FetchOnlyFromLocalCache = matchCreatedBizoOnly;
			var additionalQuery = matcherContext?.RelationshipFilterProvider?.GetFilter(enterpriseItemType);
			if (additionalQuery != null)
			{
				query.AddToFilter(additionalQuery);
			}

			var entities = LoadEntities(matcherContext?.Factory, enterpriseItemType, query, matcherContext?.CompanyFilterProviderContext);

			if (entities.Length == 0 && !matchCreatedBizoOnly)
			{
				throw new EntityMatchingException(FormattableString.Invariant($"Could not find related {dataDefinitionName}."));
			}
			else if (entities.Length > 1)
			{
				if (tableCode == OrgAddressSchema.Constants.Prefix)
				{
					var data = matchingValues.ToArray();
					var matchingKey = string.Join(".", OrgHeaderSchema.Constants.TableName,
						OrgHeaderSchema.Constants.OH_Code);
					if (data.Length == 1 && data[0].propertyName == matchingKey)
					{
						var orgPK = ((IOrgAddress)entities[0]).OrganisationPK;
						var organisation = matcherContext?.Factory.Load<IOrgHeader>(orgPK);
						if (organisation != null)
						{
							return (BusinessObject)organisation.MainAddress;
						}
					}
				}

				throw new EntityMatchingException(FormattableString.Invariant($"Found multiple related {dataDefinitionName}."));
			}

			return entities.Any() ? entities[0] : null;
		}

		static BusinessObject[] LoadEntities(BusinessObjectFactory factory, Type enterpriseItemType, ZQuery query, ICompanyFilterProviderContext companyFilterProviderContext)
		{
			var companyFilterProvider = GetCompanyFilterProvider(enterpriseItemType);
			var filters = companyFilterProvider?.GetCompanyFilters(companyFilterProviderContext, Env.CurrentCompanyPK) ?? new List<ZQuery>() { new ZQuery() };

			foreach (var filter in filters)
			{
				var entities = factory.Load(enterpriseItemType, new ZQuery(query, filter));

				if (entities.Length != 0)
				{
					return entities;
				}
			}

			return Array.Empty<BusinessObject>();
		}

		static ZQuery GetQuery(MappingDataModel mappingDataModel, string dataDefinitionName, Type enterpriseType, IEnumerable<(string propertyName, string stringValue)> matchingValues)
		{
			var result = new ZQuery();
			foreach (var matchingValue in matchingValues)
			{
				var segments = matchingValue.propertyName.Split('.');
				result.AddToFilter(GetQueryPart(mappingDataModel, dataDefinitionName, enterpriseType, segments, matchingValue.stringValue));
			}

			return result;
		}

		static ZQuery GetQueryPart(MappingDataModel mappingDataModel, string dataDefinitionName, Type enterpriseType, string[] segments, string stringValue)
		{
			if (segments.Length == 0)
			{
				throw new EntityMatchingException(FormattableString.Invariant($"Value '{stringValue}' cannot be converted into {dataDefinitionName}."));
			}

			var propertyName = segments[0];
			if (segments.Length == 1)
			{
				var schemaColumn = GetSchemaColumn(enterpriseType, propertyName);

				var typeConverter = TypeDescriptor.GetConverter(schemaColumn.GetEquivalentZType());
				try
				{
					if (typeConverter.CanConvertFrom(typeof(string)))
					{
						var convertedValue = typeConverter.ConvertFrom(stringValue);
						return new ZQuery(schemaColumn, convertedValue);
					}
					else
					{
						throw new EntityMatchingException(FormattableString.Invariant($"Specified cast is not valid. Column Name: {schemaColumn.Name}. Type: {schemaColumn.ColumnType}. Value: {stringValue}."));
					}
				}
				catch (Exception ex) when (ex is FormatException || ex is ZTypeValueException || ex is OverflowException)
				{
					throw new EntityMatchingException(FormattableString.Invariant($"Specified cast is not valid. Column Name: {schemaColumn.Name}. Type: {schemaColumn.ColumnType}. Value: {stringValue}."));
				}
			}
			else
			{
				var query = new ZDBOnlyQuery(enterpriseType);
				query.AddSubQuery(GetNavigationPropertySubQuery(mappingDataModel, dataDefinitionName, enterpriseType, propertyName, segments.Skip(1), stringValue), JoinCondition.And);
				return query;
			}
		}

		static ZDBOnlySubQuery GetNavigationPropertySubQuery(MappingDataModel mappingDataModel, string dataDefinitionName, Type enterpriseType, string propertyName, IEnumerable<string> segments, string stringValue)
		{
			MappingDataDefinitionRelation relation;
			try
			{
				relation = mappingDataModel.GetRelation(dataDefinitionName, propertyName);
			}
			catch (KeyNotFoundException)
			{
				throw new EntityMatchingException(FormattableString.Invariant($"Could not find relation {dataDefinitionName}.{propertyName}."));
			}

			var tableCode = mappingDataModel.GetTableCode(relation.TargetDataDefinitionName);
			var enterpriseItemType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tableCode);

			var result = new ZDBOnlySubQuery(enterpriseItemType, GetSchemaColumn(enterpriseType, relation.ForeignKeyProperty));
			result.AddToFilter(GetQueryPart(mappingDataModel, relation.TargetDataDefinitionName, enterpriseItemType, segments.ToArray(), stringValue));
			return result;
		}

		static SchemaColumn GetSchemaColumn(Type businessObjectType, string columnName)
			=> BusinessObjectFactory.GetTableSchemaFromType(businessObjectType, false)?.GetSchemaColumn(columnName)
				?? throw new EntityMatchingException(FormattableString.Invariant($"Business object {businessObjectType.Name} does not have a property with a name {columnName}."));

		static ICompanyFilterProvider GetCompanyFilterProvider(Type businessObjectType)
		{
			var hashTable = (Hashtable)ObjectFactory.Get("CompanyFilterProviders");  // Name of an object in the ObjectFactory
			var objectHandle = hashTable[businessObjectType.Name] as ObjectHandle;
			var companyFilterProvider = objectHandle?.GetObject() as ICompanyFilterProvider;

			return companyFilterProvider;
		}
	}
}
