using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	public static class CustomFieldHelper
	{
		public static IEnumerable<ICustomProperty> GetCustomFields(string workflowType)
		{
			var propertyCollection = new UserDefinedPropertyCollectionView();

			if (string.IsNullOrEmpty(workflowType))
			{
				return propertyCollection;
			}

			var queryTemplates = new ZDBOnlySubQuery(typeof(ProcessTaskTemplate), ProcessTaskTemplateSchema.PK);
			queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, workflowType);
			queryTemplates.AddToFilter(ProcessTaskTemplateSchema.P0_IsActive, true);

			var companyFilter = new ZQuery(ProcessTaskTemplateSchema.P0_GC, GlbCompany.CurrentCompany.PK);
			companyFilter.AddToFilter(JoinCondition.Or, ProcessTaskTemplateSchema.P0_GC, null);
			queryTemplates.AddToFilter(companyFilter);

			var branchFilter = new ZQuery(ProcessTaskTemplateSchema.P0_GB, GlbBranch.CurrentBranch.PK);
			branchFilter.AddToFilter(JoinCondition.Or, ProcessTaskTemplateSchema.P0_GB, null);
			queryTemplates.AddToFilter(branchFilter);

			var departmentFilter = new ZQuery(ProcessTaskTemplateSchema.P0_GE, GlbDepartment.CurrentDepartment.PK);
			departmentFilter.AddToFilter(JoinCondition.Or, ProcessTaskTemplateSchema.P0_GE, null);
			queryTemplates.AddToFilter(departmentFilter);

			var queryDefinitions = new ZDBOnlyQuery(typeof(GenCustomColumnDefinition));
			queryDefinitions.AddSubQuery(GenCustomColumnDefinitionSchema.XC_ParentID, queryTemplates, JoinCondition.And);
			queryDefinitions.OrderBy = GenCustomColumnDefinitionSchema.XC_Name.Name;

			var uniqueNames = new List<string>();
			var uniqueNameTypes = new Dictionary<string, List<ZString>>();

			var newFactory = new BusinessObjectFactory();
			var columnDefinitions = newFactory.Load<GenCustomColumnDefinition>(queryDefinitions);

			foreach (var columnDefinition in columnDefinitions)
			{
				var name = columnDefinition.XC_NameMultilingual;
				var addNewProperty = false;

				if (!uniqueNames.Contains(name))
				{
					addNewProperty = true;
					uniqueNames.Add(name);
					uniqueNameTypes.Add(name, new List<ZString> { columnDefinition.XC_Type });
				}
				else if (!uniqueNameTypes[name].Contains(columnDefinition.XC_Type))
				{
					addNewProperty = true;
					uniqueNameTypes[name].Add(columnDefinition.XC_Type);
				}

				if (addNewProperty)
				{
					propertyCollection.AddProperty(columnDefinition);
				}
			}

			return propertyCollection;
		}
	}
}
