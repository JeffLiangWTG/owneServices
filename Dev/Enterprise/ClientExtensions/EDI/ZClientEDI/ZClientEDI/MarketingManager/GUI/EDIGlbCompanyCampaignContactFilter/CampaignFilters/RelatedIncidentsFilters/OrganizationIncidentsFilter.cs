using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MarketingManager.GUI
{
	public class OrganizationIncidentsFilter : ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider
	{
		protected OrganizationIncidentsFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public OrganizationIncidentsFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, ClientModuleRegistration.SupportIncident, primaryKeyColumn, foreignKeyColumn, list, parentBusinessObjectType)
		{
		}

		public OrganizationIncidentsFilter(ZString description, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, ClientModuleRegistration.SupportIncident, primaryKeyColumn, foreignKeyColumn, listDelegate, parentBusinessObjectType)
		{
		}

		protected override void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query, ZDBOnlySubQuery subQuery)
		{
			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, subQuery, JoinCondition.And);
		}
	}
}
