using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class SupportIncidentFilterBusinessObject : IncidentMainFilterBusinessObject
	{
		protected override string IncidentType
		{
			get { return IncidentConstants.IncidentType.SupportIncident; }
		}

		protected CodeDescriptionPairList BuildIncidentModuleFilterItems(ModuleListType moduleListType, IEnumerable<ModuleTextFilter> productFilters, IEnumerable<ModuleTextFilter> productAreaFilters)
		{
			var result = new CodeDescriptionPairList();

			var products = productFilters.Select(filter => filter.Property);
			var productAreas = productAreaFilters.Select(filter => filter.Property);

			foreach (var productArea in productAreas)
			{
				if (!string.IsNullOrEmpty(productArea))
				{
					result.AddRangeOverwriteIfExists(Lookups.GetModuleList(moduleListType, ProductTypes.Codes.Enterprise, productArea));
				}
			}

			if (result.Count == 0)
			{
				foreach (var product in products)
				{
					result.AddRangeOverwriteIfExists(Lookups.GetModuleList(moduleListType, product, ZString.Empty));
				}
			}

			if (result.Count == 0)
			{
				result.AddRangeOverwriteIfExists(Lookups.GetModuleList(moduleListType, ZString.Empty, ZString.Empty));
			}

			result.Sort();

			return result;
		}

		#region Filters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Incident Number", IncidentMainSchema.IM_IncidentNumber, "CS");
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = base.GetModuleFiltersCore();

			AddTextFilters(result);
			AddGuidFilters(result);
			AddDateFilters(result);
			AddStatusAndFlagsFilters(result);
			AddMainUNLOCOFilter(result);
			AddLicenceFilters(result);
			AddReleaseBuildFilters(result);
			AddTaskFilters(result);
			AddRelatedItemsFilters(result);
			AddParticipantsFilters(result);

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowTasksHelper = new WorkflowFilterStripsHelper(typeof(SupportIncident), EDIJobInvoicingConsumerTypes.Incident.Code, Factory);
			workflowTasksHelper.SetShouldAddWorkflowCustomFieldsFilters(true, IncidentType);
			var processHeaderHelper = new EDIProcessHeaderFilterStripsHelper(typeof(SupportIncident), Factory);

			helpers.AddRange(new IFilterStripsHelper[]
			{
				workflowTasksHelper,
				processHeaderHelper
			});

			return helpers;
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var requestSubGroup = new RequestFilterSubGroup();

			filters.AddTextFilter("Product", IncidentMainSchema.IM_Product, ProductList);
			filters.AddTextFilter("Product Area", IncidentMainSchema.IM_ProgramArea, ProductAreaList);
			filters.AddTextFilter("Menu Section", GetMenuSectionQuery, GetIncidentMenuSectionFilterItems)
				.WithMaxLengthOf<ModuleTextFilter>(IncidentMainSchema.IM_Module);
			filters.AddTextFilter("Requirement", GetComplianceRequirementQuery, GetIncidentRequirementFilterItems)
				.WithMaxLengthOf<ModuleTextFilter>(IncidentMainSchema.IM_Module);
			filters.AddTextFilter("Service", GetCustomerServiceQuery, GetIncidentServiceFilterItems)
				.WithMaxLengthOf<ModuleTextFilter>(IncidentMainSchema.IM_Module);
			filters.AddTextFilter("Country", IncidentMainSchema.IM_RN_NKCountry, Lookups.CountryList);
			filters.AddTextFilter("Menu Item", IncidentMainSchema.IM_SourceModuleId, GetIncidentMenuItemFilterItems);
			filters.AddTextFilter("Source", IncidentMainSchema.IM_Source, SourceList);
			filters.AddTextFilter("Service Status", GetServiceStatusQuery, ServiceStatusList);

			var clientReferenceFilter = filters.AddTextFilter("Client Reference", IncidentRequestSchema.INC_ClientReference);
			clientReferenceFilter.SubGroup = requestSubGroup;
			filters.AddTextFilter("Problem Description", IncidentMainSchema.IM_Description);
			filters.AddTextFilter("Resolution Comment", GetResolutionCommentQuery).SqlComparisonOperator = SQLComparisonOperator.Contains;

			filters.AddTextFilter("Database Hosted Location", GetHostedLocationQuery, DatabaseHostedLocationList)
				.WithMaxLengthOf<ModuleTextFilter>(LicenceDatabaseSchema.LD_HostedLocation);

			var contactSubGroup = new ContactFilterSubGroup();
			filters.AddTextFilter("Contact Name", OrgContactSchema.OC_ContactName).SubGroup = contactSubGroup;
			filters.AddTextFilter("Contact Email", OrgContactSchema.OC_Email).SubGroup = contactSubGroup;
		}

		class RequestFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(SupportIncident));
				var taskQuery = new ZDBOnlySubQuery(typeof(IncidentRequest), IncidentMainSchema.IM_INC_Request);
				taskQuery.AddToFilter(filter);
				result.AddSubQuery(taskQuery, JoinCondition.And);
				return result;
			}
		}

		ZQuery GetResolutionCommentQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProfessionalServicesQuote));

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(StmNote), StmNoteSchema.ST_ParentID);
			subQuery.AddToFilter(StmNoteSchema.ST_NoteText, SQLComparisonOperator.Contains, value);
			subQuery.AddToFilter(StmNoteSchema.ST_Table, IncidentMainSchema.Constants.TableName);
			subQuery.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, EDIPredefinedNoteTypes.Instance.IncidentResolutionDetail.Code);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetHostedLocationQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SupportIncident));
			ZDBOnlySubQuery databaseSubQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), IncidentMainSchema.IM_LD);

			if (value == "ALL")
			{
				databaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_HostedLocation, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.LicenceConstants.NotHostedWithCargoWise);
			}
			else
			{
				databaseSubQuery.AddToFilter(LicenceDatabaseSchema.LD_HostedLocation, value);
			}

			query.AddSubQuery(databaseSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetMenuSectionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var menuSectionPriorities = new[]
				{
					Constants.CustomerService.CriticalityCodes.CR1_SystemDown,
					Constants.CustomerService.CriticalityCodes.CR2_ModuleDown,
					Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround,
					Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround,
					Constants.CustomerService.CriticalityCodes.CR5_Training,
					Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest,
					Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest,
				};

			var query = new ZQuery(IncidentMainSchema.IM_Priority, SQLComparisonOperator.Equal, menuSectionPriorities);
			query.AddToFilter(IncidentMainSchema.IM_Module, comparisonOperator, value);

			return query;
		}

		ZQuery GetComplianceRequirementQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery(IncidentMainSchema.IM_Priority, SQLComparisonOperator.Equal, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement);
			query.AddToFilter(IncidentMainSchema.IM_Module, comparisonOperator, value);
			return query;
		}

		ZQuery GetCustomerServiceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery(IncidentMainSchema.IM_Priority, SQLComparisonOperator.Equal, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest);
			query.AddToFilter(IncidentMainSchema.IM_Module, comparisonOperator, value);
			return query;
		}

		ZQuery GetServiceStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			if (value == "null")
			{
				query.AddToFilter(IncidentMainSchema.IM_ServiceStatus, SQLComparisonOperator.Equal, "");
			}
			else
			{
				query.AddToFilter(IncidentMainSchema.IM_ServiceStatus, comparisonOperator, value);
			}
			return query;
		}

		#endregion

		#region Guid Filters

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			var organisationFilter = new OrganisationSupportIncidentModuleFilter(this, "Organisation", ModuleIDs.Organisation, IncidentMainSchema.IM_OH_Client, Factory, typeof(SupportIncident));
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("49318CC9-D905-4BD5-9D7D-827E25E4F75B", "Organization");
			filters.AddFilter(organisationFilter);

			filters.AddNkFilter("Assigned To", GetAssignedToQuery, ModuleIDs.GlbStaff, StaffList).Category = FilterCategories.Organisations;

			filters.AddGuidFilter("Key Account Manager - Primary",
				ModuleIDs.GlbStaff, GetRelManagerPrimaryIncidentFilter, StaffList).Category = FilterCategories.Organisations;
			filters.AddGuidFilter("Key Account Manager - Secondary",
				ModuleIDs.GlbStaff, GetRelManagerSecondaryIncidentFilter, StaffList).Category = FilterCategories.Organisations;

			filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory)).Category = FilterCategories.Organisations;
			filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollection(Factory)).Category = FilterCategories.Organisations;
		}

		ZQuery GetLicenceEnterpriseQuery(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			var enterprisePK = ZGuid.Empty;
			ZGuid.TryParse(pkValue, out enterprisePK);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(SupportIncident));

			bool notIn = false;
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				notIn = true;
			}

			ZDBOnlySubQuery lcQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.LC_OH, notIn);
			ZDBOnlySubQuery leQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE, notIn);
			leQuery.AddToFilter(LicenceEnterpriseSchema.PK, comparisonOperator, enterprisePK);
			lcQuery.AddSubQuery(leQuery, JoinCondition.And);
			result.AddSubQuery(IncidentMainSchema.IM_OH_Client, lcQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetRelManagerPrimaryIncidentFilter(ZGuid value)
		{
			return GetRelationshipManagerFilter(value, "RM1");
		}

		ZQuery GetRelManagerSecondaryIncidentFilter(ZGuid value)
		{
			return GetRelationshipManagerFilter(value, "RM2");
		}

		ZQuery GetRelationshipManagerFilter(ZGuid value, string roleCode)
		{
			GlbStaff staff = Factory.Load<GlbStaff>(value);

			ZDBOnlyQuery incidentQuery = new ZDBOnlyQuery(typeof(SupportIncident));
			ZDBOnlySubQuery orgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), IncidentMainSchema.IM_OH_Client);
			ZDBOnlySubQuery assignmentQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);
			assignmentQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, roleCode);
			var staffCode = staff != null ? staff.GS_Code : ZString.Empty;
			assignmentQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, SQLComparisonOperator.Equal, staffCode);

			orgHeaderQuery.AddSubQuery(assignmentQuery, JoinCondition.And);
			incidentQuery.AddSubQuery(orgHeaderQuery, JoinCondition.And);

			return incidentQuery;
		}

		public new IEnumerable<ModuleFilter> GetActiveFiltersByDescription(string description)
		{
			return base.GetActiveFiltersByDescription(description);
		}

		ZQuery GetAssignedToQuery(SQLComparisonOperator comparisonOperator, ZString assignedTo)
		{
			var supportAssignedToQuery = new ZQuery(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support);
			var nonSupportAssignedToQuery = new ZQuery(IncidentMainSchema.IM_Category, SQLComparisonOperator.NotEqual, SupportIncidentCategoriesList.Codes.Support);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				supportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKCustServiceContact, SQLComparisonOperator.Equal, ZString.Empty);
				nonSupportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKAssignedToCurrent, SQLComparisonOperator.Equal, ZString.Empty);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				supportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKCustServiceContact, SQLComparisonOperator.NotEqual, ZString.Empty);
				nonSupportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKAssignedToCurrent, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else if (comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				supportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKCustServiceContact, SQLComparisonOperator.NotEqual, assignedTo);
				nonSupportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKAssignedToCurrent, SQLComparisonOperator.NotEqual, assignedTo);
			}
			else
			{
				supportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKCustServiceContact, assignedTo);
				nonSupportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKAssignedToCurrent, assignedTo);
			}

			return new ZQuery(supportAssignedToQuery, JoinCondition.Or, nonSupportAssignedToQuery);
		}

		#endregion

		#region UNLOCO Filter

		void AddMainUNLOCOFilter(ModuleFilterCollection filters)
		{
			ModuleNkFilter filter = filters.AddNkFilter(OrgConstants.FilterControl.UNLOCOType.OrgPort, GetMainUNLOCOQuery, ModuleIDs.Location, Locations);
			filter.Category = FilterCategories.Locations;

			if (!EDISecurityCheckpoints.OrganisationAllowSearchOutsideLoginCountry.IsAllowed)
			{
				filter.Visibility = FilterVisibility.AlwaysVisible;
				filter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				filter.PropertyValidation = LocationCountryFilterValidation;
			}
		}

		void LocationCountryFilterValidation(ZPropertyInfo info)
		{
			if (!((ZString)info.Value).StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				string errorMessage = string.Format(@"Your current security rights only allow you to view incidents relating to organizations based in your current login country ({0}).
If you think this is incorrect, please contact your administrator.", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				info.AddError(errorMessage);
			}
		}

		ZQuery GetMainUNLOCOQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SupportIncident));
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(EDIOrgHeader), OrgHeaderSchema.PK);
			orgSubQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, value, OrgHeaderSchema.OH_RL_NKClosestPort, typeof(OrgHeader)));
			query.AddSubQuery(IncidentMainSchema.IM_OH_Client, orgSubQuery, JoinCondition.And);
			return query;
		}
		#endregion

		#region Licence Filters

		void AddLicenceFilters(ModuleFilterCollection filters)
		{
			var category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Licence");

			filters.AddDateFilter("Go-Live Complete", GoLiveDateQuery).Category = category;

			var licenceDatabaseFilter = new LicenceDatabaseSupportIncidentModuleFilter("Reported Database", ClientModuleRegistration.LicenceDatabase, IncidentMainSchema.IM_LD, Factory, typeof(SupportIncident));
			licenceDatabaseFilter.MultilingualDescription = ResString.GetMultilingualString("d6978dd5-dbab-4f76-a098-f95cfb860581", "Reported Database");
			licenceDatabaseFilter.Category = category;
			filters.AddFilter(licenceDatabaseFilter);
		}

		/// <summary>
		/// The following methods must be synchronized logically.
		/// Enterprise.Client.EDI.IncidentManager.Business.SupportIncident.IM_ClientContractStatus
		/// Enterprise.Client.EDI.IncidentManager.Module.SupportIncidentFilterBusinessObject.GoLiveDateQuery
		/// </summary>
		ZQuery GoLiveDateQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			var query = new ZDBOnlyQuery(typeof(SupportIncident));
			var dateQuery = new ZQuery();
			var isValidFromDate = dateFrom.IsValid;
			var isValidToDate = dateTo.IsValid;
			const string fromDateParameter = "@dateFrom";
			const string toDateParameter = "@dateTo";
			const string licenceHeaderAlias = "LA";
			const string earliestLicenceHeaderAlias = "EarliestLA";

			AddDateTimeRange(dateQuery, comparisonOperator, JoinCondition.And, LicenceHeaderSchema.LA_SiteLiveDate, dateFrom, dateTo);
			var dateQueryParameterisedSql = ((IFilterPart)dateQuery).ParameterisedSql(new ParameterNameFactory("@GOLIVE"));
			var parametrisedLicenceHeaderAliasSql = GetDateQuery(comparisonOperator, FormattableString.Invariant($"{licenceHeaderAlias}.{LicenceHeaderSchema.Constants.LA_SiteLiveDate}"), isValidFromDate, isValidToDate, fromDateParameter, toDateParameter);
			var parametrisedEarliestLicenceHeaderAliasSql = GetDateQuery(comparisonOperator, FormattableString.Invariant($"MIN({earliestLicenceHeaderAlias}.{LicenceHeaderSchema.Constants.LA_SiteLiveDate})"), isValidFromDate, isValidToDate, fromDateParameter, toDateParameter);

			var sqlFilter = FormattableString.Invariant($@"IM_PK IN		/* Make the SQL clean & readable. */
(
	SELECT IM_PK
	FROM (
		SELECT IM_PK
		FROM (
			SELECT IM_PK
			FROM dbo.IncidentMain IM
			JOIN dbo.LicenceCompany LC ON LC_OH = IM_OH_Client
			JOIN dbo.LicenceHeader LA ON LA_LC = LC_PK
			WHERE IM_OH_Client IS NOT NULL 
			AND IM_LD IS NOT NULL
			AND IM_LD = LA_LD
			AND ({dateQueryParameterisedSql.ParameterisedQueryText})
			UNION
			SELECT IM_PK
			FROM dbo.IncidentMain IM
			JOIN dbo.LicenceCompany LC ON LC_OH = IM_OH_Client
			JOIN dbo.LicenceHeader {licenceHeaderAlias} ON LA_LC = LC_PK
			JOIN dbo.LicenceHeader {earliestLicenceHeaderAlias} ON {earliestLicenceHeaderAlias}.LA_LC = LC_PK
			LEFT JOIN dbo.LicenceHeader LAN ON LAN.LA_LC = LC_PK AND LAN.LA_LD = IM_LD
			WHERE IM_OH_Client IS NOT NULL 
			AND IM_LD IS NULL
			AND ({parametrisedLicenceHeaderAliasSql})
			AND {earliestLicenceHeaderAlias}.LA_SiteLiveDate IS NOT NULL
			AND LAN.LA_PK IS NULL
			GROUP BY IM_PK
			HAVING {parametrisedEarliestLicenceHeaderAliasSql}
			UNION
			SELECT IM_PK
			FROM dbo.IncidentMain IM
			JOIN dbo.LicenceHeader {licenceHeaderAlias} ON LA_LD = IM_LD
			JOIN dbo.LicenceHeader {earliestLicenceHeaderAlias} ON {earliestLicenceHeaderAlias}.LA_LD = IM_LD
			LEFT JOIN dbo.LicenceCompany LC ON LC_OH = IM_OH_Client
			WHERE IM_OH_Client IS NOT NULL AND LC_PK IS NULL AND {licenceHeaderAlias}.LA_IsActive = 1
			AND ({parametrisedLicenceHeaderAliasSql})
			AND {earliestLicenceHeaderAlias}.LA_SiteLiveDate IS NOT NULL
			GROUP BY IM_PK
			HAVING {parametrisedEarliestLicenceHeaderAliasSql}
		) SubTable1
	) SubTable2
)");

			var parameters = new ZSqlParameterCollection(dateQueryParameterisedSql.Parameters);

			if (comparisonOperator != DateComparisonOperator.HasNoDateEntered && comparisonOperator != DateComparisonOperator.HasDateEntered)
			{
				if (isValidFromDate)
				{
					parameters.Add(ZSqlParameter.New(fromDateParameter, dateFrom, LicenceHeaderSchema.LA_SiteLiveDate));
				}

				if (isValidToDate)
				{
					parameters.Add(ZSqlParameter.New(toDateParameter, dateTo, LicenceHeaderSchema.LA_SiteLiveDate));
				}
			}

			query.AddFilterAndZSQLParameterCollection(sqlFilter, parameters);
			return query;
		}

		string GetDateQuery(DateComparisonOperator comparisonOperator, string columnAliasAndName, bool isValidLowerDate, bool isValidUpperDate, string fromDateParameter, string toDateParameter)
		{
			var dateQueryStringBuilder = new StringBuilder();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				dateQueryStringBuilder.Append(FormattableString.Invariant($"{columnAliasAndName} IS NULL"));
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				dateQueryStringBuilder.Append(FormattableString.Invariant($"{columnAliasAndName} IS NOT NULL"));
			}
			else
			{
				if (isValidLowerDate)
				{
					dateQueryStringBuilder.Append(FormattableString.Invariant($"{columnAliasAndName} >= {fromDateParameter}"));
				}

				if (isValidUpperDate)
				{
					if (isValidLowerDate)
					{
						dateQueryStringBuilder.Append(" AND ");
					}

					dateQueryStringBuilder.Append(FormattableString.Invariant($"{columnAliasAndName} <= {toDateParameter}"));
				}
			}

			return dateQueryStringBuilder.ToString();
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Incident Raised", IncidentMainSchema.IM_InstallDate, true);
			filters.AddDateFilter("Incident Closed", IncidentMainSchema.IM_CloseTimeUtc, true);
			filters.AddDateFilter("Support Closed", IncidentMainSchema.IM_PlannedInstall);
			filters.AddDateFilter("Resolved Time", IncidentMainSchema.IM_ResolveTimeUtc, true);

			var estimateSubGroup = new EstimateSubGroup();
			filters.AddDateFilter("Estimate Sent Date", ClientIncidentEstimateSchema.CIE_EstimateSentDateUTC, true).SubGroup = estimateSubGroup;
			filters.AddDateFilter("Estimate Expiry Date", ClientIncidentEstimateSchema.CIE_EstimateExpiryDateUTC, true).SubGroup = estimateSubGroup;
			filters.AddDateFilter("Quote Requested Date", ClientIncidentEstimateSchema.CIE_QuoteRequestedUTC, true).SubGroup = estimateSubGroup;

			var quoteSubGroup = new QuoteSubGroup();
			filters.AddDateFilter("Quote Sent Date", ClientIncidentQuoteSchema.CIQ_QuoteSentDateUTC, true).SubGroup = quoteSubGroup;
			filters.AddDateFilter("Quote Expiry Date", ClientIncidentQuoteSchema.CIQ_QuoteExpiryDateUTC, true).SubGroup = quoteSubGroup;
			filters.AddDateFilter("Quote Accepted Date", ClientIncidentQuoteSchema.CIQ_QuoteAcceptedDateUTC, true).SubGroup = quoteSubGroup;
		}

		class EstimateSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(SupportIncident));
				var subQuery = new ZDBOnlySubQuery(typeof(ClientIncidentEstimate), ClientIncidentEstimateSchema.CIE_IM);
				subQuery.AddToFilter(filter);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			}
		}

		class QuoteSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(SupportIncident));
				var subQuery = new ZDBOnlySubQuery(typeof(ClientIncidentQuote), ClientIncidentQuoteSchema.CIQ_IM);
				subQuery.AddToFilter(filter);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region Status & Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter("Chargeable Incident", new string[] { "Chargeable" }, new SchemaBoolColumn[] { IncidentMainSchema.IM_ChargableWork });
			filters.AddTextFilter(FilterDescriptionConstants.Stage, GetStageQuery, StageList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(FilterDescriptionConstants.Status, GetStatusQuery, GetStatusList).Category = FilterCategories.StatusAndFlags;

			var filter = filters.AddTextFilter("Disposition", GetDispositionQuery, GetStatusDispositionList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MaxLength = IncidentMainSchema.IM_ResolutionCode.MaxLength;

			var eRequestStatusFilter = filters.AddTextFilter("eRequest Status", GetDispositionQuery, GetERequestStatusList);
			eRequestStatusFilter.Category = FilterCategories.StatusAndFlags;
			eRequestStatusFilter.MaxLength = IncidentMainSchema.IM_ResolutionCode.MaxLength;

			var resolutionMethodFilter = filters.AddTextFilter("Resolution Method", GetResolutionCodeQuery, GetResolutionMethodList);
			resolutionMethodFilter.Category = FilterCategories.StatusAndFlags;
			resolutionMethodFilter.MaxLength = IncidentMainSchema.IM_ClosureResolution.MaxLength;

			filters.AddTextFilter("Defect Severity", IncidentMainSchema.IM_ClientBugSeverity, DefectSeverityList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Criticality", IncidentMainSchema.IM_Priority, CriticalityList).Category = FilterCategories.StatusAndFlags;
			filters.AddFlagsFilter(FilterDescriptionConstants.ClientManagementGroup,
				new string[] { "Show all incidents for Organisations managed by Client" },
				new GetFlagsQuery[] { GetClientManagemantGroupFilter });
			filters.AddTextFilter("Sales Client Size", GetSalesClientSizeQuery, OrganisationsDataRegistry.Instance.ClientSizeList.Value).Category = FilterCategories.StatusAndFlags;
			filters.AddFlagsFilter("Disposition (No Staff Assigned)",
				new string[] { "Disposition: Added Awaiting Assignment", "Disposition: Assigned - Awaiting Action" },
				new GetFlagsQuery[] { GetAddedAwaitingAssignmentQuery, GetAssignedAwaitingActionQuery },
				JoinCondition.Or).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Language", GetLanguageFilter, LanguageList).Category = FilterCategories.StatusAndFlags;
		}

		#region Stage Query

		ZQuery GetStageQuery(ZString stage)
		{
			ZQuery result = new ZQuery();
			if (stage != AllStages)
			{
				result.AddToFilter(IncidentMainSchema.IM_Category, stage);
			}
			return result;
		}

		IEnumerable<string> GetActiveStages()
		{
			bool hasActiveStage = false;

			foreach (ZString stageValue in GetActiveStageFilterValues())
			{
				if (!stageValue.IsEmpty)
				{
					hasActiveStage = true;
					if (stageValue == AllStages)
					{
						yield return SupportIncidentCategoriesList.Codes.Support;
						yield return SupportIncidentCategoriesList.Codes.Defect;
						yield return SupportIncidentCategoriesList.Codes.FeatureRequest;
						yield return SupportIncidentCategoriesList.Codes.ContentDevelopment;
						yield return SupportIncidentCategoriesList.Codes.ComplianceRequirement;
						yield return SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
						break;
					}
					else
					{
						yield return stageValue;
					}
				}
			}

			if (!hasActiveStage)
			{
				yield return SupportIncidentCategoriesList.Codes.Support;
				yield return SupportIncidentCategoriesList.Codes.Defect;
				yield return SupportIncidentCategoriesList.Codes.FeatureRequest;
				yield return SupportIncidentCategoriesList.Codes.ContentDevelopment;
				yield return SupportIncidentCategoriesList.Codes.ComplianceRequirement;
				yield return SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			}
		}

		IEnumerable<ZString> GetActiveStageFilterValues()
		{
			foreach (ModuleTextFilter stageFilter in GetActiveFiltersByDescription(FilterDescriptionConstants.Stage))
			{
				yield return stageFilter.Property;
			}
		}

		#endregion

		#region Status Query

		ZQuery GetStatusQuery(ZString status)
		{
			ZQuery result = new ZQuery();

			if (status == SupportIncidentLookups.Status.ClosedDirectlyInSupport)
			{
				result.AddToFilter(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support);
				result.AddToFilter(IncidentMainSchema.IM_Status, SupportIncidentLookups.Status.Closed);
			}
			else
			{
				string[] statusArray = ConvertStatusFilterValueToStatusArray(status);
				result.AddToFilter(JoinCondition.Or, IncidentMainSchema.IM_Status, SQLComparisonOperator.Equal, statusArray);
			}

			return result;
		}

		string[] ConvertStatusFilterValueToStatusArray(string status)
		{
			return (status == SupportIncidentLookups.Status.NotClosed) ? NotClosedStatus : new string[] { status };
		}

		#endregion

		#region Disposition Query

		ZQuery GetDispositionQuery(SQLComparisonOperator comparisonOperator, ZString disp)
		{
			ZQuery result = new ZQuery();

			bool isExcludingOperator = comparisonOperator == SQLComparisonOperator.NotEqual
									|| comparisonOperator == SQLComparisonOperator.NotContains
									|| comparisonOperator == SQLComparisonOperator.DoesNotStartWith
									|| comparisonOperator == SpecialComparisonOperator.IsNotBlank;

			JoinCondition joinCondition = isExcludingOperator ? JoinCondition.And : JoinCondition.Or;
			AddDispositionFilter(result, joinCondition, comparisonOperator, disp);

			return result;
		}

		void AddDispositionFilter(ZQuery filter, JoinCondition joinCondition, SQLComparisonOperator comparisonOperator, string disp)
		{
			SchemaStringColumn column = IncidentMainSchema.IM_ResolutionCode;

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				filter.AddToFilter(joinCondition, column, SQLComparisonOperator.Equal, ZString.Empty);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				filter.AddToFilter(joinCondition, column, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				filter.AddToFilter(joinCondition, column, comparisonOperator, disp);
			}
		}

		#endregion

		#region ResolutionCode Query

		ZQuery GetResolutionCodeQuery(SQLComparisonOperator comparisonOperator, ZString disp)
		{
			ZQuery result = new ZQuery();

			bool isExcludingOperator = comparisonOperator == SQLComparisonOperator.NotEqual
									|| comparisonOperator == SQLComparisonOperator.NotContains
									|| comparisonOperator == SQLComparisonOperator.DoesNotStartWith
									|| comparisonOperator == SpecialComparisonOperator.IsNotBlank;

			JoinCondition joinCondition = isExcludingOperator ? JoinCondition.And : JoinCondition.Or;
			AddResolutionCodeFilter(result, joinCondition, comparisonOperator, disp);

			return result;
		}

		void AddResolutionCodeFilter(ZQuery filter, JoinCondition joinCondition, SQLComparisonOperator comparisonOperator, string disp)
		{
			SchemaStringColumn column = IncidentMainSchema.IM_ClosureResolution;

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				filter.AddToFilter(joinCondition, column, SQLComparisonOperator.Equal, ZString.Empty);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				filter.AddToFilter(joinCondition, column, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				filter.AddToFilter(joinCondition, column, comparisonOperator, disp);
			}
		}

		#endregion

		#region Sales Client Size

		ZQuery GetSalesClientSizeQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SupportIncident));
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), IncidentMainSchema.IM_OH_Client);
			ZDBOnlySubQuery orgMiscSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			orgMiscSubQuery.AddToFilter(OrgMiscServSchema.OM_CMClientSize, value);
			orgSubQuery.AddSubQuery(orgMiscSubQuery, JoinCondition.And);
			query.AddSubQuery(orgSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Staff Unassigned

		ZQuery GetAddedAwaitingAssignmentQuery(ZBool value)
		{
			return value
					? GetStaffUnassignedQuery(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment)
					: new ZQuery();
		}

		ZQuery GetAssignedAwaitingActionQuery(ZBool value)
		{
			return value
					? GetStaffUnassignedQuery(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction)
					: new ZQuery();
		}

		ZQuery GetStaffUnassignedQuery(ZString disposition)
		{
			ZQuery result = new ZQuery();

			var supportAssignedToQuery = new ZQuery(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.Support);
			supportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKCustServiceContact, SQLComparisonOperator.Equal, ZString.Empty);

			var nonSupportAssignedToQuery = new ZQuery(IncidentMainSchema.IM_Category, SQLComparisonOperator.NotEqual, SupportIncidentCategoriesList.Codes.Support);
			nonSupportAssignedToQuery.AddToFilter(IncidentMainSchema.IM_GS_NKAssignedToCurrent, SQLComparisonOperator.Equal, ZString.Empty);

			var assignedToQuery = new ZQuery(supportAssignedToQuery, JoinCondition.Or, nonSupportAssignedToQuery);

			result.AddToFilter(IncidentMainSchema.IM_ResolutionCode, disposition);
			result.AddToFilter(assignedToQuery);

			return result;
		}

		#endregion

		ZQuery GetClientManagemantGroupFilter(ZBool value)
		{
			return new ZQuery();
		}

		ZQuery GetLanguageFilter(ZString value)
		{
			ZQuery result = new ZQuery();

			if (!value.IsEmpty && value != AnyLanguage)
			{
				result.AddToFilter(IncidentMainSchema.IM_Language, value);
			}

			return result;
		}

		public static ZDBOnlyQuery GetMatchingClientOrMatchingManagementGroupQuery(ZGuid clientPK)
		{
			ZDBOnlyQuery matchingOrgOrInManagementGroupQuery = new ZDBOnlyQuery(typeof(ProfessionalServicesQuote));

			ZDBOnlySubQuery managementGroupQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
			managementGroupQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, clientPK);
			managementGroupQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ManagementGrouping);
			managementGroupQuery.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyDirectionList.Codes.Forwarder);

			matchingOrgOrInManagementGroupQuery.AddToFilter(IncidentMainSchema.IM_OH_Client, clientPK);
			matchingOrgOrInManagementGroupQuery.AddSubQuery(IncidentMainSchema.IM_OH_Client, managementGroupQuery, JoinCondition.Or);

			return matchingOrgOrInManagementGroupQuery;
		}

		string[] NotClosedStatus
		{
			get { return new string[] { SupportIncidentLookups.Status.Open, SupportIncidentLookups.Status.Working, SupportIncidentLookups.Status.Suspended }; }
		}

		#endregion

		#region Related Projects Filter

		ZQuery GetRelatedProjectsGuidFilter(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			ZGuid relatedProjectPk = ZGuid.Empty;
			ZGuid.TryParse(pkValue, out relatedProjectPk);

			bool notIn = comparisonOperator == SpecialComparisonOperator.IsBlank;

			var projectFilter = new ZQuery();
			if (!relatedProjectPk.IsEmpty)
			{
				projectFilter.AddToFilter(WorkProjectSchema.PK, comparisonOperator, relatedProjectPk);
			}

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(SupportIncident));
			var linkParentQuery = GenPivot.Query.GetPivotQuery(true, IncidentMainSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
				typeof(EDIProject), WorkProjectSchema.Constants.Prefix, projectFilter, null, notIn);

			var linkIncidentQuery = GenPivot.Query.GetPivotQuery(false, IncidentMainSchema.Constants.Prefix, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement,
				typeof(EDIProject), WorkProjectSchema.Constants.Prefix, projectFilter, null, notIn);

			JoinCondition linkJoinCondition = (notIn) ? JoinCondition.And : JoinCondition.Or;
			query.AddSubQuery(linkParentQuery, linkJoinCondition);
			query.AddSubQuery(linkIncidentQuery, linkJoinCondition);

			return query;
		}

		#endregion

		#region Task Filters

		class TaskFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(SupportIncident));
				ZDBOnlySubQuery taskQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
				taskQuery.AddToFilter(filter);
				result.AddSubQuery(taskQuery, JoinCondition.And);
				return result;
			}
		}

		void AddTaskFilters(ModuleFilterCollection moduleFilters)
		{
			ModuleFilter[] taskFilters =
			{
				moduleFilters.AddNkFilter("Task Assigned To", ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ModuleIDs.GlbStaff, StaffList),
				moduleFilters.AddGuidFilter("Task Assigned Team", ModuleIDs.GlbGroup, ProcessTasksSchema.P9_GG_AssignedGroup, Groups),
				moduleFilters.AddTextFilter("Task Status", GetTaskStatusQuery, TaskStatusList),
				moduleFilters.AddFlagsFilter("Current Task Only", new string[] { "Current Task Only" }, new GetFlagsQuery[] { GetCurrentTaskOnlyQuery }),
				moduleFilters.AddGuidFilter("Capability Only (Current Task)", ModuleIDs.GlbCapability, GetTaskCapabilityQuery, CapabilityList),
				moduleFilters.AddNkFilter("Staff Capability Only (Current Task)", GetTaskStaffCapabilityQuery, ModuleIDs.GlbStaff, StaffList),
			};

			var standardWorkflowFilter = moduleFilters.FirstOrDefault(filter => filter.Category.Description.GetUnresolvedString() == "Workflow Tasks");
			FilterCategory category = standardWorkflowFilter != null ? standardWorkflowFilter.Category : FilterCategories.GetOrCreateFilterCategory((NoResString)"Workflow Tasks");

			TaskFilterSubGroup subGroup = new TaskFilterSubGroup();
			Array.ForEach(taskFilters, delegate(ModuleFilter filter)
			{
				filter.SubGroup = subGroup;
				filter.Category = category;
			});

			moduleFilters.AddNkFilter("Last Task Closed By Staff", GetLastTaskClosedByStaffQuery, ModuleIDs.GlbStaff, StaffList).Category = category;
		}

		ZQuery GetCurrentTaskOnlyQuery(ZBool currentTaskOnly)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var p9_PK = ProcessTasksSchema.Constants.PK;

			if (currentTaskOnly)
			{
				var registry = ObjectFactory.Get<IBMSRegistry>();
				var functionHelper = ObjectFactory.Get<IBMSQLFunctionHelper>();
				var dbFunctionToRun = registry.BufferManagementEnabled ? functionHelper.GetCurrentTasks + "()" : "dbo.GetCurrentTasksNotInWorkflows()";
				var sql = FormattableString.Invariant($"{p9_PK} IN (SELECT {p9_PK} FROM {dbFunctionToRun})");
				query.AddFilterAndZSQLParameterCollection(sql, null);
			}
			return query;
		}

		ZQuery GetTaskStatusQuery(ZString status)
		{
			ZQuery query = new ZQuery();
			if (status == "NCM") // Not Complete
			{
				query.AddToFilter(ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Closed);
				query.AddToFilter(ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Cancelled);
			}
			else
			{
				query.AddToFilter(ProcessTasksSchema.P9_Status, status);
			}
			return query;
		}

		ZQuery GetTaskCapabilityQuery(ZGuid capabilityPk)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
			var currentTaskOnlyQuery = GetCurrentTaskOnlyQuery(true);
			query.AddToFilter(currentTaskOnlyQuery);
			query.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ZString.Empty);
			query.AddToFilter(ProcessTasksSchema.P9_G4_RequiredCapability, capabilityPk);

			return query;
		}

		ZQuery GetTaskStaffCapabilityQuery(ZString staffCode)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
			var currentTaskOnlyQuery = GetCurrentTaskOnlyQuery(true);
			query.AddToFilter(currentTaskOnlyQuery);
			query.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ZString.Empty);

			ZDBOnlySubQuery capabilitySubQuery = new ZDBOnlySubQuery(typeof(GlbCapability), ProcessTasksSchema.P9_G4_RequiredCapability);
			ZDBOnlySubQuery capabilityPivotSubQuery = new ZDBOnlySubQuery(typeof(GlbResourceCapabilityPivot), GlbResourceCapabilityPivotSchema.G5_G4_Capability);
			ZDBOnlySubQuery staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbResourceCapabilityPivotSchema.G5_GS_Resource);
			staffSubQuery.AddToFilter(GlbStaffSchema.GS_Code, staffCode);
			capabilityPivotSubQuery.AddSubQuery(staffSubQuery, JoinCondition.And);
			capabilitySubQuery.AddSubQuery(capabilityPivotSubQuery, JoinCondition.And);
			query.AddSubQuery(capabilitySubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetLastTaskClosedByStaffQuery(ZString staffCode)
		{
			var query = new ZDBOnlyQuery(typeof(SupportIncident));
			string additionalSql = string.Format(
@"{0} IN 
(
	SELECT P9_ParentID
	FROM 
		dbo.ProcessTasks 
		JOIN 
		(
			SELECT 
				InnerQuery.{1} AS ParentId, 
				MAX(InnerQuery.{2}) AS MaxSeq 
			FROM 
				dbo.ProcessTasks InnerQuery
			WHERE 
				{3} = '{4}'
			GROUP BY {1}
		) MaxSequence ON {1} = ParentId AND {2} = MaxSeq AND {3} = '{4}' AND {5} = '{6}'
)",

					SupportIncident.Schema.PK, //0
					ProcessTasks.Schema.P9_ParentID, //1
					ProcessTasks.Schema.P9_Sequence, //2
					ProcessTasks.Schema.P9_Status, //3
					ProcessTaskStatusCodeList.Codes.Closed, //4
					ProcessTasks.Schema.P9_GS_NKAssignedStaffMember, //5
					staffCode);//6

			query.AddFilterAndZSQLParameterCollection(additionalSql, null);

			return query;
		}

		#endregion

		#region Contact Filters

		class ContactFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(SupportIncident));
				var taskQuery = new ZDBOnlySubQuery(typeof(OrgContact), IncidentMainSchema.IM_OC_Contact);
				taskQuery.AddToFilter(filter);
				result.AddSubQuery(taskQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		void AddReleaseBuildFilters(ModuleFilterCollection moduleFilters)
		{
			FilterCategory category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Release Builds");
			ModuleGuidsFilter patchedWorkItemFilter = moduleFilters.AddGuidFilter("Patched to Upgrade", ClientModuleRegistration.ReleaseBuild, GetReleaseBuildFiltersQuery, new ReleaseBuildCollection(Factory), new ReleaseBuildCollection(Factory));
			patchedWorkItemFilter.SetItemDescriptions(Res.GetData("f578902c-3a3b-4bd6-a90f-e04371519b49", "from"), Res.GetData("cd0a0cf2-032b-4d15-8020-2bc015a88880", "to"));
			patchedWorkItemFilter.Category = category;
			patchedWorkItemFilter.Property1Validation = MandatoryValidation.CheckEntered;
			patchedWorkItemFilter.Property2Validation = MandatoryValidation.CheckEntered;
		}

		ZQuery GetReleaseBuildFiltersQuery(ZGuid value1, ZGuid value2)
		{
			return new ReleaseBuildContent(Factory).GetPatchedIncidentsQuery(value1, value2);
		}

		public static class FilterDescriptionConstants
		{
			public const string Stage = "Stage";
			public const string Status = "Status";
			public const string ClientManagementGroup = "Client Management Group";
			public const string Criticaility = "Criticality";
			public const string Product = "Product";
		}

		void AddRelatedItemsFilters(ModuleFilterCollection filters)
		{
			FilterCategory category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Related Items");
			filters.AddGuidFilter("Related Project", ModuleIDs.Project, GetRelatedProjectsGuidFilter, new ProjectCollection(Factory)).Category = category;

			var relatedWIFilter = new RelatedWorkItemsOfIncidentFilter("Related Work Items", () => new NewWorkItemCollection(Factory));
			relatedWIFilter.Category = category;
			filters.AddFilter(relatedWIFilter);

			var relatedProjectsFilter = new RelatedProjectsOfIncidentFilter("Related Projects", () => new ProjectCollection(Factory));
			relatedProjectsFilter.Category = category;
			filters.AddFilter(relatedProjectsFilter);

			var relatedTriageNodesFilter = new RelatedTriageNodesOfIncidentFilter("Related Triage Nodes", ClientModuleRegistration.IncidentTriage, IncidentMainSchema.PK, IncidentMainSchema.IM_IMT_Triage, new IncidentTriageCollection(Factory), typeof(SupportIncident));
			relatedTriageNodesFilter.MultilingualDescription = ResString.GetMultilingualString("4d30a7cc-1e4d-45dd-bf72-aff4d8c5cdbc", "Related Triage Nodes");
			relatedTriageNodesFilter.Category = category;
			filters.AddFilter(relatedTriageNodesFilter);

			var relatedIncidents = new RelatedIncidentsOfIncidentFilter("Related Incidents", () => new SupportIncidentCollection(Factory));
			relatedIncidents.Category = category;
			filters.AddFilter(relatedIncidents);

			var relatedManagementGroups = new RelatedIncidentManagementGroupOfIncidentFilter("Related Incident Management Groups", () => new IncidentManagementGroupCollection(Factory));
			relatedManagementGroups.Category = category;
			filters.AddFilter(relatedManagementGroups);

			var relatedOpportunity = new RelatedOpportunitiesOfIncidentFilter("Related Opportunities", () => new OrgOpportunityCollection(Factory));
			relatedOpportunity.Category = category;
			filters.AddFilter(relatedOpportunity);
		}

		void AddParticipantsFilters(ModuleFilterCollection filters)
		{
			var category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Participants");

			var relatedContactsFilter = new IncidentEConversationParticipantsFilter<OrgContact>("Contact Participants", ModuleIDs.OrgContacts, IncidentMainSchema.PK, new OrgContactCollection(Factory), typeof(SupportIncident));
			relatedContactsFilter.MultilingualDescription = ResString.GetMultilingualString("57fc66e2-573e-464d-93fa-4dae03b4c43b", "Contact Participants");
			relatedContactsFilter.Category = category;
			filters.AddFilter(relatedContactsFilter);

			var relatedStaffFilter = new IncidentEConversationParticipantsFilter<GlbStaff>("Staff Participants", ModuleIDs.GlbStaff, IncidentMainSchema.PK, new GlbStaffCollection(Factory), typeof(SupportIncident));
			relatedStaffFilter.MultilingualDescription = ResString.GetMultilingualString("2d137337-9af1-4274-9350-618e3eb12595", "Staff Participants");
			relatedStaffFilter.Category = category;
			filters.AddFilter(relatedStaffFilter);

			var relatedGroupsFilter = new IncidentEConversationParticipantsFilter<GlbGroup>("Group Participants", ModuleIDs.GlbGroup, IncidentMainSchema.PK, new GlbGroupCollection(Factory), typeof(SupportIncident));
			relatedGroupsFilter.MultilingualDescription = ResString.GetMultilingualString("1a1c6ed3-60a1-409d-baf9-3c49c9fa5504", "Group Participants");
			relatedGroupsFilter.Category = category;
			filters.AddFilter(relatedGroupsFilter);

			var relatedOrganisationsFilter = new IncidentEConversationParticipantsFilter<OrgHeader>("Organization Participants", ModuleIDs.Organisation, IncidentMainSchema.PK, new OrgHeaderCollection(Factory), typeof(SupportIncident));
			relatedOrganisationsFilter.MultilingualDescription = ResString.GetMultilingualString("41d3a84c-0cfa-4982-892e-b8dde559b7ce", "Organization Participants");
			relatedOrganisationsFilter.Category = category;
			filters.AddFilter(relatedOrganisationsFilter);

			var emailFilter = filters.AddTextFilter("Has Email Participant", GetEmailParticipantQuery);
			emailFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			emailFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			emailFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			emailFilter.Category = category;
		}

		ZQuery GetEmailParticipantQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(SupportIncident));

			var subQueryParticipant = new ZDBOnlySubQuery(typeof(JobConversationParticipant), JobConversationParticipantSchema.JCP_JCC_Conversation,
				notIn: false);
			subQueryParticipant.AddToFilter(JobConversationParticipantSchema.JCP_EmailAddress, comparisonOperator, value);
			subQueryParticipant.AddToFilter(JobConversationParticipantSchema.JCP_ParticipantTableCode, string.Empty);

			var subQueryConversation = new ZDBOnlySubQuery(typeof(JobConversation), JobConversationSchema.JCC_ParentID);
			subQueryConversation.AddSubQuery(subQueryParticipant, JoinCondition.And);

			query.AddSubQuery(IncidentMainSchema.IM_INC_Request, subQueryConversation, JoinCondition.And);
			return query;
		}

		#endregion

		#region Lookups

		CodeDescriptionPairList DefectSeverityList
		{
			get { return Lookups.BugSeverities; }
		}

		public CodeDescriptionPairList StageList
		{
			get
			{
				if (fStageList == null)
				{
					fStageList = new CodeDescriptionPairList();
					fStageList.AddPair(AllStages, AllStages);
					fStageList.AddRange(Lookups.StageList);
				}

				return fStageList;
			}
		}

		CodeDescriptionPairList fStageList;

		public const string AllStages = "All Stages";

		public CodeDescriptionPairList ProductList
		{
			get { return Lookups.ProductList; }
		}

		public CodeDescriptionPairList ProductAreaList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddRange(EDIDataRegistry.Instance.ProductAreas.Value);
				list.Sort();
				return list;
			}
		}

		ICodeDescriptionPairList GetIncidentMenuSectionFilterItems()
		{
			return GetIncidentModuleFilterItems(ModuleListType.MenuSection);
		}

		ICodeDescriptionPairList GetIncidentRequirementFilterItems()
		{
			return GetIncidentModuleFilterItems(ModuleListType.Cr8);
		}

		ICodeDescriptionPairList GetIncidentServiceFilterItems()
		{
			return Lookups.GetModuleList(ModuleListType.Cr9, ZString.Empty, ZString.Empty);
		}

		ICodeDescriptionPairList GetIncidentModuleFilterItems(ModuleListType moduleListType)
		{
			var productFilters = GetActiveFiltersByDescription("Product").Cast<ModuleTextFilter>();
			var productAreaFilters = GetActiveFiltersByDescription("Product Area").Cast<ModuleTextFilter>();

			return BuildIncidentModuleFilterItems(moduleListType, productFilters, productAreaFilters);
		}

		ICodeDescriptionPairList GetIncidentMenuItemFilterItems()
		{
			return SupportIncidentLookups.GetNewSearchableSourceModuleList();
		}

		public CodeDescriptionPairList SourceList
		{
			get { return Lookups.SourceList; }
		}

		public CodeDescriptionPairList ServiceStatusList
		{
			get
			{
				if (serviceStatusList == null)
				{
					serviceStatusList = Lookups.ServiceStatusList;
					serviceStatusList.AddPair("null", "null");
				}
				return serviceStatusList;
			}
		}
		CodeDescriptionPairList serviceStatusList;

		public virtual CodeDescriptionPairList CriticalityList
		{
			get { return Lookups.CriticalityList; }
		}

		public CodeDescriptionPairList LanguageList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				list.AddPair(AnyLanguage, (NoResString)"Any");
				list.AddRange(new CodeDescriptionPairList(OLookUpEditType.Language));

				return list;
			}
		}

		const string AnyLanguage = "ANY";

		public CodeDescriptionPairList GetStatusList()
		{
			CodeDescriptionPairList result = base.StatusList;

			if (GetActiveStages().Contains(SupportIncidentCategoriesList.Codes.Support))
			{
				result.AddPair(SupportIncidentLookups.Status.ClosedDirectlyInSupport, "Closed Directly in Support");
			}

			return result;
		}

		protected override CodeDescriptionPairList StatusList
		{
			get { return GetStatusList(); }
		}

		#region Disposition List

		public CodeDescriptionPairList GetStatusDispositionList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			var activeStageList = new List<string>(GetActiveStages());
			var activeCriticalityList = new List<string>(GetActiveCriticality());
			var activeProductList = new List<string>(GetActiveProduct());

			foreach (string activeStatus in GetActiveStatus())
			{
				if (activeStatus == SupportIncidentLookups.Status.ClosedDirectlyInSupport)
				{
					result.AddRangeOverwriteIfExists(Lookups.GetSupportStatusDispositionList(SupportIncidentLookups.Status.Closed, "", ""));
				}
				else
				{
					foreach (var activeStage in activeStageList)
					{
						foreach (var activeCriticality in activeCriticalityList)
						{
							foreach (var activeProduct in activeProductList)
							{
								result.AddRangeOverwriteIfExists(Lookups.GetStatusDispositionList(activeStage, activeStatus, activeCriticality, activeProduct, activeOnly: true));
							}
						}
					}
				}
			}

			return result;
		}

		public CodeDescriptionPairList GetERequestStatusList()
		{
			var result = new CodeDescriptionPairList();
			var statusDispositionList = GetStatusDispositionList();
			var allERequestStatuses = Lookups.GetAllERequestStatuses();
			foreach (ICodeDescription statusDisposition in statusDispositionList)
			{
				if (allERequestStatuses.Contains(statusDisposition))
				{
					result.Add(statusDisposition);
				}
			}
			return result;
		}

		public CodeDescriptionPairList GetResolutionMethodList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			var activeStageList = new List<string>(GetActiveStages());
			var activeCriticalityList = new List<string>(GetActiveCriticality());
			var activeProductList = new List<string>(GetActiveProduct());

			foreach (var activeStage in activeStageList)
			{
				foreach (var activeCriticality in activeCriticalityList)
				{
					foreach (var activeProduct in activeProductList)
					{
						foreach (ICodeDescription disposition in Lookups.GetClosureDispositionList(activeOnly: false, activeStage, activeCriticality, activeProduct))
						{
							result.AddOverwriteIfExists(disposition);
						}
					}
				}
			}

			return result;
		}

		IEnumerable<string> GetActiveStatus()
		{
			bool hasActiveStatus = false;

			foreach (ModuleTextFilter statusFilter in GetActiveFiltersByDescription(FilterDescriptionConstants.Status))
			{
				ZString statusValue = statusFilter.Property;
				if (!statusValue.IsEmpty)
				{
					hasActiveStatus = true;
					if (statusValue == SupportIncidentLookups.Status.NotClosed)
					{
						yield return SupportIncidentLookups.Status.Open;
						yield return SupportIncidentLookups.Status.Working;
					}
					else
					{
						yield return statusValue;
					}
				}
			}

			if (!hasActiveStatus)
			{
				yield return SupportIncidentLookups.Status.Open;
				yield return SupportIncidentLookups.Status.Working;
				yield return SupportIncidentLookups.Status.Suspended;
				yield return SupportIncidentLookups.Status.Closed;
			}
		}

		IEnumerable<string> GetActiveCriticality()
		{
			bool hasActiveCriticality = false;

			foreach (ModuleTextFilter criticalityFilter in GetActiveFiltersByDescription(FilterDescriptionConstants.Criticaility))
			{
				ZString criticalityValue = criticalityFilter.Property;
				if (!criticalityValue.IsEmpty)
				{
					hasActiveCriticality = true;
					yield return criticalityValue;
				}
			}

			if (!hasActiveCriticality)
			{
				foreach (ICodeDescription pair in CriticalityList)
				{
					yield return pair.Code;
				}
			}
		}

		IEnumerable<string> GetActiveProduct()
		{
			bool hasActiveProduct = false;

			foreach (ModuleTextFilter productFilter in GetActiveFiltersByDescription(FilterDescriptionConstants.Product))
			{
				ZString productValue = productFilter.Property;
				if (!productValue.IsEmpty)
				{
					hasActiveProduct = true;
					yield return productValue;
				}
			}

			if (!hasActiveProduct)
			{
				foreach (ICodeDescription pair in ProductList)
				{
					yield return pair.Code;
				}
			}
		}

		#endregion

		public GlbStaffCollection StaffList
		{
			get { return new GlbStaffCollection(Factory); }
		}

		public GlbCapabilityCollection CapabilityList
		{
			get { return new GlbCapabilityCollection(Factory); }
		}

		public GlbGroupCollection Groups
		{
			get { return new GlbGroupCollection(Factory); }
		}

		public OrgHeaderCollection Clients
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		CodeDescriptionPairList TaskStatusList
		{
			get
			{
				if (taskStatusList == null)
				{
					taskStatusList = new CodeDescriptionPairList();
					taskStatusList.AddRange(new ProcessTaskStatusCodeList());
					taskStatusList.AddPair("NCM", "Not Complete");
				}
				return taskStatusList;
			}
		}
		CodeDescriptionPairList taskStatusList;

		CodeDescriptionPairList DatabaseHostedLocationList
		{
			get
			{
				if (databaseHostedLocationList == null)
				{
					databaseHostedLocationList = new CodeDescriptionPairList();
					databaseHostedLocationList.AddPair("ALL", "Hosted With Any Data Center");
					databaseHostedLocationList.AddRange(EDIDataRegistry.Instance.DatabaseHostedLocations.Value);
				}
				return databaseHostedLocationList;
			}
		}
		CodeDescriptionPairList databaseHostedLocationList;

		#region Locations Collections

		public LocationCollection Locations
		{
			get { return new LocationCollection(Factory); }
		}

		#endregion

		protected new SupportIncidentLookups Lookups
		{
			get { return (SupportIncidentLookups)base.Lookups; }
		}

		protected override IncidentMainLookups GetNewLookups()
		{
			return new SupportIncidentLookups(this);
		}

		#endregion
	}
}
