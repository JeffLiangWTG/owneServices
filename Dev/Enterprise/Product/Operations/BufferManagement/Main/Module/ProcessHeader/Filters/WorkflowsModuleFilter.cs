using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class WorkflowsModuleFilter : ModuleGuidForeignCollectionFilter
	{
		protected WorkflowsModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public WorkflowsModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, ModuleIDs.ProcessHeader, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public WorkflowsModuleFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, ModuleIDs.ProcessHeader, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("531C28A3-2496-477A-8EC4-D9D2CF81D454", "Workflows for Job");

		protected override void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query, ZDBOnlySubQuery subQuery)
		{
			if (typeof(IWorkflowProvider).IsAssignableFrom(parentBusinessObjectType))
			{
				subQuery.AddToFilter(WorkflowFilterStripsHelper.GetParentTableCodeQuery(BusinessObjectFactory.GetTableCodeFromType(parentBusinessObjectType), ProcessHeaderSchema.FH_ParentTableCode));
			}
			query.AddSubQuery(FilterColumn, subQuery, JoinCondition.And);
		}
	}
}
