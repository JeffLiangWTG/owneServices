using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class EDIWorkItemFilterBusinessObject : WorkItemFilterBusinessObject
	{
		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var processHeaderHelper = new EDIProcessHeaderFilterStripsHelper(typeof(EDIWorkItem), Factory);

			helpers.Add(processHeaderHelper);
			return helpers;
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection moduleFilters = base.GetModuleFiltersCore();

			moduleFilters.AddFountainFilter("Legacy Task Number", WorkItemSchema.WKI_WorkItemNumber, "T");

			AddRelatedIncidentFilters(moduleFilters);
			AddRelatedProjectFilters(moduleFilters);
			AddReleaseBuildFilters(moduleFilters);
			AddNumberOfRelatedIncidentsFilters(moduleFilters);
			return moduleFilters;
		}

		#region Related Incident Filters

		void AddRelatedIncidentFilters(ModuleFilterCollection moduleFilters)
		{
			RelatedIncidentFilterSubGroup subGroup = new RelatedIncidentFilterSubGroup();

			var clientFilter = moduleFilters.AddGuidFilter("Related Client", ModuleIDs.Organisation, IncidentMainSchema.IM_OH_Client, OrganisationList);
			clientFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			clientFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			clientFilter.SubGroup = subGroup;

			AddRelatedSupportIncidentFilters(moduleFilters);
		}

		void AddRelatedSupportIncidentFilters(ModuleFilterCollection moduleFilters)
		{
			var subGroup = new RelatedIncidentFilterSubGroup();
			var category = RelatedItemsFilterCategory;

			var incidentNumberFilter = moduleFilters.AddFountainFilter("Incident Number", IncidentMainSchema.IM_IncidentNumber, "CS");
			incidentNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			incidentNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			incidentNumberFilter.SubGroup = subGroup;
			incidentNumberFilter.Category = category;

			var criticalityFilter = moduleFilters.AddTextFilter("Incident Criticality", IncidentMainSchema.IM_Priority, IncidentCriticalityList);
			criticalityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			criticalityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			criticalityFilter.SubGroup = subGroup;
			criticalityFilter.Category = category;

			moduleFilters.AddFilter(new RelatedIncidentsOfWorkItemFilter("Related Incidents", () => new SupportIncidentCollection(Factory)));
		}

		class RelatedIncidentFilterSubGroup : ModuleFilterSubGroup
		{
			public RelatedIncidentFilterSubGroup()
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(NewWorkItem));

				#region Link Direction 2

				ZDBOnlySubQuery linkSubQuery2 = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID);
				linkSubQuery2.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement);
				linkSubQuery2.AddToFilter(GenPivotSchema.XX_Relation1TableCode, IncidentMainSchema.Constants.Prefix);
				linkSubQuery2.AddToFilter(GenPivotSchema.XX_Relation2TableCode, WorkItemSchema.Constants.Prefix);
				ZDBOnlySubQuery incidentSubQuery2 = new ZDBOnlySubQuery(typeof(SupportIncident), GenPivotSchema.XX_Relation1ID);
				incidentSubQuery2.AddToFilter(filter);
				linkSubQuery2.AddSubQuery(incidentSubQuery2, JoinCondition.And);

				#endregion

				result.AddSubQuery(linkSubQuery2, JoinCondition.Or);

				return result;
			}
		}

		#endregion

		#region Related Project Filters

		void AddRelatedProjectFilters(ModuleFilterCollection moduleFilters)
		{
			FilterCategory category = RelatedItemsFilterCategory;
			RelatedProjectFilterSubGroup subGroup = new RelatedProjectFilterSubGroup();

			ModuleFilter projectNumberFilter = moduleFilters.AddFountainFilter("Project Number", WorkProjectSchema.WKP_ProjectNumber, "PRJ");
			projectNumberFilter.SubGroup = subGroup;
			projectNumberFilter.Category = category;

			ModuleFilter projectManagerFilter = moduleFilters.AddNkFilter("Project Manager", WorkProjectSchema.WKP_GS_NKProjectManager, ModuleIDs.GlbStaff, StaffList);
			projectManagerFilter.SubGroup = subGroup;
			projectManagerFilter.Category = category;
		}

		class RelatedProjectFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(NewWorkItem));

				var projectFilter = new ZQuery(filter);

				// WorkItem <---> Project
				GenPivot.Query.AddPivotFilter(query, WorkItemSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
					typeof(EDIProject), WorkProjectSchema.Constants.Prefix, projectFilter);

				// WorkItem <---> Incident <---> Project
				GenPivot.Query.AddPivotFilter(query, WorkItemSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
					typeof(SupportIncident), IncidentMainSchema.Constants.Prefix, new ZQuery(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident),
					typeof(EDIProject), WorkProjectSchema.Constants.Prefix, projectFilter);
				return query;
			}
		}

		#endregion

		#region Release Build Filters

		void AddReleaseBuildFilters(ModuleFilterCollection moduleFilters)
		{
			FilterCategory category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Release Builds");
			ModuleGuidsFilter patchedWorkItemFilter = moduleFilters.AddGuidFilter("Patched to Upgrade", ClientModuleRegistration.ReleaseBuild, GetReleaseBuildFiltersQuery, new ReleaseBuildCollection(Factory), new ReleaseBuildCollection(Factory));
			patchedWorkItemFilter.SetItemDescriptions(Res.GetData("acc64608-f444-4b69-a1be-bb1da0ee1bde", "from"), Res.GetData("16a7f662-c0fa-4da0-92f2-cfbf0dbeeb5d", "to"));
			patchedWorkItemFilter.Category = category;
			patchedWorkItemFilter.Property1Validation = MandatoryValidation.CheckEntered;
			patchedWorkItemFilter.Property2Validation = MandatoryValidation.CheckEntered;
		}

		ZQuery GetReleaseBuildFiltersQuery(ZGuid value1, ZGuid value2)
		{
			return new ReleaseBuildContent(Factory).GetPatchedWorkItemsQuery(value1, value2);
		}

		#endregion

		#region Number of related Incidents Filters

		void AddNumberOfRelatedIncidentsFilters(ModuleFilterCollection moduleFilters)
		{
			ModuleNumberRangeFilter numberRangeFilter = moduleFilters.AddNumberRangeFilter("Number of related Incidents", GetNumberOfRelatedIncidentsQuery);
			numberRangeFilter.Category = RelatedItemsFilterCategory;
			numberRangeFilter.PropertyType = ZCalcEditPropertyType.Int;
			numberRangeFilter.MinValue = 0;
		}

		ZQuery GetNumberOfRelatedIncidentsQuery(INumericZType value1, INumericZType value2)
		{
			ZQuery result = new ZDBOnlyQuery(typeof(NewWorkItem));

			string sql = @"((select count(*) from " +
					GenPivotSchema.Constants.SqlSchemaName + "." + GenPivotSchema.Constants.TableName + " where " +
					GenPivotSchema.Constants.XX_RelationType + " in ('" + Core.Constants.GenPivotTypes.ProcessManagement + "', '" + Core.Constants.GenPivotTypes.WorkItemCascade + "') and " +
					GenPivotSchema.Constants.XX_Relation1TableCode + " = '" + IncidentMainSchema.Constants.Prefix + "' and " +
					GenPivotSchema.Constants.XX_Relation2TableCode + " = '" + WorkItemSchema.Constants.Prefix + "' and " +
					GenPivotSchema.Constants.XX_Relation2ID + " = " + WorkItemSchema.Constants.PK +
					" ) between " + value1 + " and " + value2 + ")";

			result.AddFilterAndZSQLParameterCollection(sql, null);
			return result;
		}

		#endregion

		#endregion

		#region Lookups

		GlbStaffCollection StaffList
		{
			get { return staffList ?? (staffList = new GlbStaffCollection(Factory)); }
		}
		GlbStaffCollection staffList;

		OrganisationsFindBoxCollection OrganisationList
		{
			get { return organisationList ?? (organisationList = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection organisationList;

		CodeDescriptionPairList IncidentCriticalityList
		{
			get { return fIncidentCriticalityList ?? (fIncidentCriticalityList = new SupportIncidentLookups(this).CriticalityList); }
		}
		CodeDescriptionPairList fIncidentCriticalityList;

		protected new NewWorkItemLookups Lookups
		{
			get { return (NewWorkItemLookups)base.Lookups; }
		}

		#endregion

		#region Initial Code For Search

		protected override void ApplyInitialCode(ZString code, string propertyName)
		{
			if (propertyName == WorkItemSchema.WKI_WorkItemNumber.Name && !code.Contains(':') && SearchType == SearchType.Sql)
			{
				ModuleFilters["Work Item Number"].SetValueFromInitialCode(propertyName, code);
			}
			else
			{
				base.ApplyInitialCode(code, propertyName);
			}
		}

		#endregion
	}
}
