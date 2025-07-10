using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class EDIProjectFilterBusinessObject : ProjectFilterBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();
			AddDateFilters(filters);
			AddTextFilters(filters);
			AddRelatedWorkItemFilters(filters);
			AddLicenceFilters(filters);

			filters.AddFilter(new RelatedIncidentsOfProjectFilter("Related Incidents", () => new SupportIncidentCollection(Factory)));

			return filters;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var processHeaderHelper = new EDIProcessHeaderFilterStripsHelper(typeof(EDIProject), Factory);

			helpers.Add(processHeaderHelper);
			return helpers;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddFountainFilter("Old Project ID", WorkProjectSchema.WKP_ProjectNumber, "PRO");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Follow Up Date", GetClientWorkProjectDateQuery(ClientWorkProjectSchema.CWP_CallbackBy));
			filters.AddDateFilter("Planned Install", GetClientWorkProjectDateQuery(ClientWorkProjectSchema.CWP_PlannedInstall));
			filters.AddDateFilter("Install Date", GetClientWorkProjectDateQuery(ClientWorkProjectSchema.CWP_InstallDate));
			filters.AddDateFilter("Planned Go-Live", GetLicenceRelatedDateQuery(LicenceHeaderSchema.LA_EstimatedLiveDate));
			filters.AddDateFilter("Go-Live Complete", GetLicenceRelatedDateQuery(LicenceHeaderSchema.LA_SiteLiveDate));
			filters.AddDateFilter("Agreed Go-Live", GetLicenceRelatedDateQuery(LicenceHeaderSchema.LA_AgreedLiveDate));
		}

		GetDateQuery GetClientWorkProjectDateQuery(SchemaDateTimeColumn column)
		{
			return
				delegate(DateComparisonOperator comparisonOperator, ZDateTime dateTime1, ZDateTime dateTime2)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EDIProject));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ClientWorkProject), ClientWorkProjectSchema.CWP_WKP);
					AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, column, dateTime1.Date, dateTime2);
					query.AddSubQuery(subQuery, JoinCondition.And);
					return query;
				};
		}

		GetDateQuery GetLicenceRelatedDateQuery(SchemaDateTimeColumn column)
		{
			return
				delegate(DateComparisonOperator comparisonOperator, ZDateTime dateTime1, ZDateTime dateTime2)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EDIProject));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ClientWorkProject), ClientWorkProjectSchema.CWP_WKP);
					ZDBOnlySubQuery licenceSubQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), ClientWorkProjectSchema.CWP_LA);
					AddDateTimeRange(licenceSubQuery, comparisonOperator, JoinCondition.And, column, dateTime1.Date, dateTime2);
					subQuery.AddSubQuery(licenceSubQuery, JoinCondition.And);
					query.AddSubQuery(subQuery, JoinCondition.And);

					return query;
				};
		}

		#region Related Work Item Filter

		void AddRelatedWorkItemFilters(ModuleFilterCollection moduleFilters)
		{
			FilterCategory category = RelatedWorkItemsFilterCategory;
			RelatedWorkItemFilterSubGroup subGroup = new RelatedWorkItemFilterSubGroup();

			ModuleFilter workItemNumberFilter = moduleFilters.AddFountainFilter("Work Item Number", WorkItemSchema.WKI_WorkItemNumber, "WI");
			workItemNumberFilter.SubGroup = subGroup;
			workItemNumberFilter.Category = category;
		}

		class RelatedWorkItemFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EDIProject));

				// Project <---> WorkItem
				GenPivot.Query.AddPivotFilter(query, WorkProjectSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
					typeof(NewWorkItem), WorkItemSchema.Constants.Prefix, filter);

				// Project <---> Incident <---> WorkItem
				GenPivot.Query.AddPivotFilter(query, WorkProjectSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
					typeof(SupportIncident), IncidentMainSchema.Constants.Prefix, new ZQuery(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident),
					typeof(NewWorkItem), WorkItemSchema.Constants.Prefix, filter);
				return query;
			}
		}

		#endregion

		#region Licence Filters

		void AddLicenceFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
			filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollection(Factory));
		}

		ZQuery GetLicenceEnterpriseQuery(ZGuid enterprisePK)
		{
			var result = new ZDBOnlyQuery(typeof(EDIProject));
			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), WorkProjectSchema.WKP_OA_ClientAddress);
			var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			var companySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH);
			var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
			enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.PK, enterprisePK);
			companySubQuery.AddSubQuery(enterpriseSubQuery, JoinCondition.And);
			orgSubQuery.AddSubQuery(companySubQuery, JoinCondition.And);
			addressSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);
			result.AddSubQuery(addressSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#endregion
	}
}
