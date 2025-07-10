using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.GUI
{
	public class SeriesFilterBusinessObject : FilterStripBusinessObject, IRelatedModuleFilterBusinessObject
	{
		public SeriesFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.MENTSeries.Name;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var factory = new BusinessObjectFactory();
			var filters = new ModuleFilterCollection();

			AddStaffFilters(filters, factory);
			AddReleaseGroupFilters(filters);
			AddComponentFilters(filters);
			AddDateFilters(filters);
			AddAttributeValueFilters(filters);
			return filters;
		}

		#region Filters

		void AddStaffFilters(ModuleFilterCollection filters, BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(filters, nameof(filters));

			filters.AddNkFilter("Staff Code", MENTAgedScoreMetricSchema.MAS_GS_NKStaffCode, ModuleIDs.GlbStaff, GetStaffList(factory))
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|SeriesFilterBusinessObject|StaffCode", "Staff Code");
		}

		static IBusinessObjectCollection GetStaffList(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));

			return (IBusinessObjectCollection)ObjectFactory.Get<IGlbStaffCollection>(nameof(IGlbStaffCollection), factory);
		}

		void AddReleaseGroupFilters(ModuleFilterCollection filters)
		{
			Argument.NotNull(filters, nameof(filters));

			filters.AddGuidFilter("Release Group", ModuleIDs.GlbGroup, MENTAgedScoreMetricSchema.MAS_GG_ReleaseGroup, () => new GlbGroupCollection(Factory))
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|SeriesFilterBusinessObject|ReleaseGroup", "Release Group Column");

			filters.AddGuidFilter("Staff Release Group", ModuleIDs.GlbGroup, GetStaffReleaseGroupQuery, () => new GlbGroupCollection(Factory))
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|SeriesFilterBusinessObject|StaffReleaseGroup", "All Staff from Release Group");
		}

		static ZQuery GetStaffReleaseGroupQuery(ZGuid value)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@GlbGroupPK", value, GlbGroupLinkSchema.GK_GG);

			var sql = string.Format(CultureInfo.InvariantCulture,
@" {0} IN ( 
	SELECT {3} FROM {1}
	LEFT JOIN {4} ON {2} = {5}
	WHERE {6} = @GlbGroupPK
)",
				/*0*/ MENTAgedScoreMetricSchema.Constants.MAS_GS_NKStaffCode,
				/*1*/ GlbStaffSchema.Constants.TableName,
				/*2*/ GlbStaffSchema.Constants.PK,
				/*3*/ GlbStaffSchema.Constants.GS_Code,
				/*4*/ GlbGroupLinkSchema.Constants.TableName,
				/*5*/ GlbGroupLinkSchema.Constants.GK_GS,
				/*6*/ GlbGroupLinkSchema.Constants.GK_GG);

			var query = new ZQuery();
			query.AddFilterAndZSQLParameterCollection(sql, parameters, JoinCondition.And);
			return query;
		}

		void AddComponentFilters(ModuleFilterCollection filters)
		{
			Argument.NotNull(filters, nameof(filters));

			filters.AddGuidFilter("Current Component", ModuleIDs.BMComponent, MENTAgedScoreMetricSchema.MAS_FC_Component, () => new BMComponentCollection(Factory))
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|SeriesFilterBusinessObject|CurrentComponent", "Current Component");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			Argument.NotNull(filters, nameof(filters));

			filters.AddDateFilter("Time recorded", MENTAgedScoreMetricSchema.MAS_TimeRecordedUtc, true)
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|SeriesFilterBusinessObject|Timerecorded", "Time recorded");
		}

		void AddAttributeValueFilters(ModuleFilterCollection filters)
		{
			Argument.NotNull(filters, nameof(filters));

			filters.AddTextFilter("Attribute Value", MENTAgedScoreMetricSchema.MAS_AttributeValue)
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|SeriesFilterBusinessObject|AttributeValue", "Attribute Value");
		}

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		#endregion
	}
}
