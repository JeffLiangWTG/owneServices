using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class EDIProcessHeaderFilterStripsHelper : FilterStripsHelper
	{
		public EDIProcessHeaderFilterStripsHelper() : base() { }

		public EDIProcessHeaderFilterStripsHelper(Type businessObjectType, BusinessObjectFactory factory) : base(businessObjectType, factory)
		{
		}

		#region Categories

		readonly FilterCategory workflowFilterCategory = FilterCategories.GetOrCreateFilterCategory((NoResString)"Job Workflow");

		#endregion

		#region Release Group

		void AddReleaseGroupFilter(ModuleFilterCollection filters)
		{
			var releaseGroupFilter = filters.AddGuidFilter("Release Group", ModuleIDs.GlbGroup, GetReleaseGroupFilterQuery, new GlbGroupCollection(Factory, true));
			releaseGroupFilter.Category = workflowFilterCategory;
			releaseGroupFilter.IsPublishedOnWeb = false;
			releaseGroupFilter.MultilingualDescription = ResString.GetMultilingualString("78dd1274-b29e-440c-8010-60ef12494233", "Release Group");
		}

		ZQuery GetReleaseGroupFilterQuery(ZGuid value)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_ParentId);
			headerQuery.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, value);

			var query = new ZDBOnlyQuery(BusinessObjectType);
			query.AddSubQuery(headerQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Implementation

#if DEBUG

		public override string GetAutomaticFilterTestCaseName_ForObjectFactory() => "EDIProcessHeaderFilterStripsHelperAutomaticFilterTest";

#endif

		public override bool IsApplicableToBizOTypeIsAssignableFrom()
		{
			return typeof(IWorkflowProvider).IsAssignableFrom(BusinessObjectType);
		}

		protected override void AddFilterStrips(ModuleFilterCollection filters)
		{
			AddReleaseGroupFilter(filters);
		}

		#endregion
	}
}
