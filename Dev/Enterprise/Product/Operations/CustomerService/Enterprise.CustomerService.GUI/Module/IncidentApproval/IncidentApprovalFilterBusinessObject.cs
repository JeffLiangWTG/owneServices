
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.CustomerService.GUI.ResString;

namespace Enterprise.CustomerService.Module
{
	public class IncidentApprovalFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddNkFilter(FilterDescription.ReportingUser, IncidentApprovalSchema.IA_SystemCreateUser, ModuleIDs.GlbStaff, StaffMembers).MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|ReportingUser", FilterDescription.ReportingUser);
			filters.AddNkFilter(FilterDescription.ApprovingUser, IncidentApprovalSchema.IA_GS_NKApprovingStaff, ModuleIDs.GlbStaff, StaffMembers).MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|ApprovingUser", FilterDescription.ApprovingUser);

			var menuSectionFilter = filters.AddTextFilter(FilterDescription.MenuSection, GetMenuSectionQuery, MenuSectionList);
			menuSectionFilter.MaxLength = IncidentApprovalSchema.IA_Module.MaxLength;
			menuSectionFilter.MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|MenuSection", FilterDescription.MenuSection);

			var requirementFilter = filters.AddTextFilter(FilterDescription.Requirement, GetComplianceRequirementQuery, RequirementList);
			requirementFilter.MaxLength = IncidentApprovalSchema.IA_Module.MaxLength;
			requirementFilter.MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|Requirement", FilterDescription.Requirement);

			var serviceFilter = filters.AddTextFilter(FilterDescription.Service, GetCustomerServiceQuery, ServiceList);
			serviceFilter.MaxLength = IncidentApprovalSchema.IA_Module.MaxLength;
			serviceFilter.MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|Service", FilterDescription.Service);

			filters.AddTextFilter(FilterDescription.Status, IncidentApprovalSchema.IA_Status, StatusList).MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|Status", FilterDescription.Status);
			filters.AddTextFilter(FilterDescription.CustomerStatus, IncidentApprovalSchema.IA_ClientSpecifiedStatus, CustomerStatusList).MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|CustomerStatus", FilterDescription.CustomerStatus);
			filters.AddTextFilter(FilterDescription.Criticality, IncidentApprovalSchema.IA_Criticality, CriticalityList).MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|Criticality", FilterDescription.Criticality);
			filters.AddTextFilter(FilterDescription.IncidentSummary, IncidentApprovalSchema.IA_IncidentSummary).MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|IncidentSummary", FilterDescription.IncidentSummary);
			filters.AddTextFilter(FilterDescription.Company, IncidentApprovalSchema.IA_LicenceCode, LicenceCompanyList).MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|Company", FilterDescription.Company);

			filters.AddFountainFilter(FilterDescription.IncidentNumber, IncidentApprovalSchema.IA_IncidentNumber, "CS").MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|IncidentNumber", FilterDescription.IncidentNumber);
			filters.AddFountainFilter(FilterDescription.ClientReferenceNumber, IncidentApprovalSchema.IA_ClientReference, "SR").MultilingualDescription = ResString.GetMultilingualString("CustomerService|IncidentApprovalFilter|ClientReferenceNumber", FilterDescription.ClientReferenceNumber);

			return filters;
		}

		#endregion

		#region Module Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string ReportingUser = "Reporting User";
			public const string ApprovingUser = "Approving User";
			public const string MenuSection = "Menu Section";
			public const string Requirement = "Requirement";
			public const string AgreementId = "AgreementId";
			public const string Product = "Product";
			public const string Service = "Service";
			public const string Status = "Status";
			public const string CustomerStatus = "Customer Status";
			public const string Criticality = "Criticality";
			public const string IncidentSummary = "Incident Summary";
			public const string Company = "Company";
			public const string IncidentNumber = "Incident Number";
			public const string ClientReferenceNumber = "Client Reference Number";
			#endregion
		}

		#endregion

		#region Lookups

		#region Module / Requirement / Service List

		#region ModuleList

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

			var query = new ZQuery(IncidentApprovalSchema.IA_Criticality, SQLComparisonOperator.Equal, menuSectionPriorities);
			query.AddToFilter(IncidentApprovalSchema.IA_Module, comparisonOperator, value);

			return query;
		}

		static CodeDescriptionPairList MenuSectionList
		{
			get { return new IncidentApprovalLookups(null).MenuSectionList; }
		}

		#endregion

		#region RequirementList

		ZQuery GetComplianceRequirementQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery(IncidentApprovalSchema.IA_Criticality, SQLComparisonOperator.Equal, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement);
			query.AddToFilter(IncidentApprovalSchema.IA_Module, comparisonOperator, value);
			return query;
		}

		static CodeDescriptionPairList RequirementList
		{
			get { return new IncidentApprovalLookups(null).Cr8ModuleList; }
		}

		#endregion

		#region ServiceList

		ZQuery GetCustomerServiceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery(IncidentApprovalSchema.IA_Criticality, SQLComparisonOperator.Equal, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest);
			query.AddToFilter(IncidentApprovalSchema.IA_Module, comparisonOperator, value);
			return query;
		}

		static CodeDescriptionPairList ServiceList
		{
			get { return new IncidentApprovalLookups(null).Cr9ModuleList; }
		}

		#endregion

		#endregion

		#region CriticalityList

		static CodeDescriptionPairList CriticalityList
		{
			get { return new IncidentApprovalLookups(null).CriticalityList; }
		}

		#endregion

		#region StatusList

		static CodeDescriptionPairList StatusList
		{
			get { return new IncidentApprovalLookups(null).StatusList; }
		}

		#endregion

		#region CustomerStatusList

		static ReadOnlyCodeDescriptionPairList CustomerStatusList
		{
			get { return SystemDataRegistry.Instance.CustomerStatuses.Value; }
		}

		#endregion

		#region StaffMembers

		GlbStaffCollection StaffMembers
		{
			get
			{
				if (fStaffMembers == null)
				{
					fStaffMembers = new GlbStaffCollection(Factory);
				}
				return fStaffMembers;
			}
		}

		GlbStaffCollection fStaffMembers;

		#endregion

		#region LicenceCompanyList

		CodeDescriptionPairList LicenceCompanyList
		{
			get { return IncidentApprovalLookups.CreateLicenceCompanyList(Factory); }
		}

		#endregion

		#endregion
	}
}
