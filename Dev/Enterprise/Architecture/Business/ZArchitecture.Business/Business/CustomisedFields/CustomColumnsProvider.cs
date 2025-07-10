using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public sealed class CustomColumnsProvider : ICustomColumnsProvider
	{
		public CustomColumnsProvider() { }

		public IEnumerable<IGenCustomColumnDefinition> GetCustomColumnDefinitions(
			BusinessObjectFactory businessObjectFactory,
			string workflowType,
			ZGuid company = default,
			ZGuid branch = default,
			ZGuid department = default)
		{
			var optionalQueryParts = new List<string>();
			var parameters = new ZSqlParameterCollection
			{
				{ "@ProcessType", workflowType, ProcessTaskTemplateSchema.P0_ProcessType }
			};

			if (!company.IsEmpty)
			{
				parameters.Add("@Company", company, ProcessTaskTemplateSchema.P0_GC);
				optionalQueryParts.Add($"AND ({ProcessTaskTemplateSchema.P0_GC.Name} IS NULL OR {ProcessTaskTemplateSchema.P0_GC.Name} = @Company)");
			}

			if (!branch.IsEmpty)
			{
				parameters.Add("@Branch", branch, ProcessTaskTemplateSchema.P0_GB);
				optionalQueryParts.Add($"AND ({ProcessTaskTemplateSchema.P0_GB.Name} IS NULL OR {ProcessTaskTemplateSchema.P0_GB.Name} = @Branch)");
			}

			if (!department.IsEmpty)
			{
				parameters.Add("@Department", department, ProcessTaskTemplateSchema.P0_GE);
				optionalQueryParts.Add($"AND ({ProcessTaskTemplateSchema.P0_GE.Name} IS NULL OR {ProcessTaskTemplateSchema.P0_GE.Name} = @Department)");
			}

			var genCustomColumnDefinitionType = ObjectFactory.GetType<IGenCustomColumnDefinition>();
			var queryDefinitions = new ZDBOnlyQuery(genCustomColumnDefinitionType);
			var distinctivenessQuery =
				$@"{GenCustomColumnDefinitionSchema.PK.Name} IN (
					SELECT {GenCustomColumnDefinitionSchema.PK.Name}
						FROM (SELECT {GenCustomColumnDefinitionSchema.PK.Name},
							ROW_NUMBER() OVER(
								PARTITION BY {GenCustomColumnDefinitionSchema.XC_Name.Name}, {GenCustomColumnDefinitionSchema.XC_Type.Name}
								ORDER BY {GenCustomColumnDefinitionSchema.XC_XR.Name} DESC) AS rowNum
							FROM {GenCustomColumnDefinitionSchema.Constants.TableName}
							WHERE {GenCustomColumnDefinitionSchema.XC_ParentID.Name} IN (
								SELECT {ProcessTaskTemplateSchema.PK.Name}
								FROM {ProcessTaskTemplateSchema.Constants.TableName}
								WHERE {ProcessTaskTemplateSchema.P0_ProcessType.Name} = @ProcessType
								AND {ProcessTaskTemplateSchema.P0_IsActive.Name} = 1 {string.Join(" ", optionalQueryParts)})
						) definitionByNameAndType
					WHERE rowNum = 1)";
			queryDefinitions.AddFilterAndZSQLParameterCollection(distinctivenessQuery, parameters);

			var genCustomColumnDefinitions =
				(IEnumerable<IGenCustomColumnDefinition>)businessObjectFactory.Load(
					genCustomColumnDefinitionType,
					queryDefinitions).AsEnumerable();

			foreach (var columnDefinition in genCustomColumnDefinitions)
			{
				if (columnDefinition.XC_XR.IsValid)
				{
					var fetchHint = new ZQuery(GenCustomAddOnRuleSchema.PK, columnDefinition.XC_XR);
					fetchHint.IncludeBlob(GenCustomAddOnRuleSchema.XR_SourceCode);
					businessObjectFactory.AddFetchHint(GenCustomAddOnRuleSchema.Instance, fetchHint);
				}
			}

			return genCustomColumnDefinitions
				.OrderBy(x => x.XC_ParentID)
				.ThenBy(x => x.XC_DisplaySequence);
		}
	}
}
