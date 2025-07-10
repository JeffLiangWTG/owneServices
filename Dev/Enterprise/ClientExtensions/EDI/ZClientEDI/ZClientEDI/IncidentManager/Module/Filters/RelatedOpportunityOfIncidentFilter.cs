using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class RelatedOpportunitiesOfIncidentFilter : ModuleGuidPivotFilter
	{
		public RelatedOpportunitiesOfIncidentFilter(ZString description, GetList listDelegate)
			: base(description, ModuleIDs.Opportunity, GenPivotSchema.XX_Relation2ID, GenPivotSchema.XX_Relation1ID, listDelegate, typeof(SupportIncident), typeof(GenPivot), CreateFilter(IncidentMainSchema.Constants.Prefix, OrgOpportunitySchema.Constants.Prefix))
		{
			MultilingualDescription = GetMultilingualDescription();
		}

		protected RelatedOpportunitiesOfIncidentFilter(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
			: base(description, otherModuleID, fromColumn, toColumn, listDelegate, parentBusinessObjectType, typeof(GenPivot), CreateFilter(relatedTablePrefix, parentTablePrefix))
		{
		}

		protected RelatedOpportunitiesOfIncidentFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected  SchemaGuidColumn ParentPKColumn => IncidentMainSchema.PK;
		protected override SchemaColumn SubQueryColumn => OrgOpportunitySchema.PK;

		protected  MultilingualString GetMultilingualDescription() => ResString.GetMultilingualString("D8506FF2-03F1-4EC8-BC0F-28DD40359E16", "Related Opportunities");

		protected ModuleGuidPivotFilter CreateFilterForOtherPivotDirection(ZString description, GetList listDelegate, SchemaGuidColumn fromColumn, SchemaGuidColumn toColumn, Type parentBusinessObjectType, string parentTablePrefix, string relatedTablePrefix, ModuleIdentifier otherModuleID)
		{
			return new RelatedOpportunitiesOfIncidentFilter(description, listDelegate, fromColumn, toColumn, parentBusinessObjectType, parentTablePrefix, relatedTablePrefix, otherModuleID);
		}

		protected static ZQuery CreateFilter(string relation1Prefix, string relation2Prefix)
		{
			return new ZQuery(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.Opportunity)
				.AddToFilter(GenPivotSchema.XX_Relation1TableCode, relation1Prefix)
				.AddToFilter(GenPivotSchema.XX_Relation2TableCode, relation2Prefix);
		}
	}
}
