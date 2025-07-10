using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CustomerService.Business
{
	public class IncidentApprovalLookups : AutoIncidentApprovalLookups
	{
		public IncidentApprovalLookups(AutoIncidentApproval parent)
			: base(parent)
		{
		}

		#region Static

		public static ModuleListType GetModuleListType(string criticality)
		{
			switch (criticality)
			{
				case "":
					return ModuleListType.Unspecified;

				case Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement:
					return ModuleListType.Cr8;

				case Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest:
					return ModuleListType.Cr9;

				default:
					return ModuleListType.MenuSection;
			}
		}

		public static ZString GetOtherModuleCode(ModuleListType moduelListType)
		{
			switch (moduelListType)
			{
				case ModuleListType.MenuSection:
					return MandatoryCustomerServiceMenuSectionList.Codes.Other;

				case ModuleListType.Cr8:
					return Business.Cr8ModuleList.Codes.OtherComplianceIssue;

				case ModuleListType.Cr9:
					return Business.Cr9ModuleList.Codes.OtherConsultingPleaseDescribeClearly;
			}

			return ZString.Empty;
		}

		#endregion

		#region ModuleList

		#region MenuSection List

		public CodeDescriptionPairList MenuSectionList
		{
			get
			{
				var result = GetMenuSectionList(false);
				result.SortByDescription();
				return result;
			}
		}

		public CodeDescriptionPairList MenuSectionListIncludingHidden
		{
			get { return GetMenuSectionList(true); }
		}

		CodeDescriptionPairList GetMenuSectionList(bool includeHidden)
		{
			var modules = new CodeDescriptionPairList();
			var mandatoryCustomerServiceMenuSectionList = new MandatoryCustomerServiceMenuSectionList();
			foreach (var menuSection in mandatoryCustomerServiceMenuSectionList.Values)
			{
				modules.AddPair(menuSection.Code, menuSection.Description);
			}

			var moduleTreeCustomerServiceMenuSectionList = new ModuleTreeCustomerServiceMenuSectionList();
			foreach (var category in includeHidden ? ModuleTree.Tree.Categories.ValuesIncludingHidden : ModuleTree.Tree.Categories.Values.Cast<ModuleCategory>())
			{
				foreach (var section in includeHidden ? category.Sections.ValuesIncludingHidden : category.Sections.Values.Cast<ModuleSection>())
				{
					var customerServiceMenuSectionCode = section.CustomerServiceMenuSectionCode;
					if (!string.IsNullOrEmpty(customerServiceMenuSectionCode))
					{
						CustomerServiceMenuSection menuSection;
						if (mandatoryCustomerServiceMenuSectionList.TryGetValue(customerServiceMenuSectionCode, out menuSection)
							|| moduleTreeCustomerServiceMenuSectionList.TryGetValue(customerServiceMenuSectionCode, out menuSection))
						{
							modules.AddPairIfNotExist(menuSection.Code, menuSection.Description);
						}
					}
				}
			}

			return modules;
		}

		#endregion

		#region CR8 Module List

		public CodeDescriptionPairList Cr8ModuleList
		{
			get { return new Cr8ModuleList(); }
		}

		#endregion

		#region CR9 Module List

		public CodeDescriptionPairList Cr9ModuleList
		{
			get { return new Cr9ModuleList(); }
		}

		#endregion

		#endregion

		#region Criticality

		public CodeDescriptionPairList CriticalityList
		{
			get
			{
				CodeDescriptionPairList criticalityList = new CodeDescriptionPairList();
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR1_SystemDown, ResString.GetMultilingualString("CustomerService|CriticalityList|CR1_SystemDown", "Entire system is down – system failure"));
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR2_ModuleDown, ResString.GetMultilingualString("CustomerService|CriticalityList|CR2_ModuleDown", "Entire module not working with no manual work around"));
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround, ResString.GetMultilingualString("CustomerService|CriticalityList|CR3_SingleFunctionNoWorkAround", "Single function not working with no manual work around"));
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, ResString.GetMultilingualString("CustomerService|CriticalityList|CR4_SingleFunctionWithWorkAround", "Single function not working with manual work around"));
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR5_Training, ResString.GetMultilingualString("CustomerService|CriticalityList|CR5_Training", "Training Questions"));
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest, ResString.GetMultilingualString("CustomerService|CriticalityList|CR6_FeatureRequest", "Feature Request"));
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest, ResString.GetMultilingualString("CustomerService|CriticalityList|CR7_CustomisationRequest", "Estimate / Quote Request"));
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, ResString.GetMultilingualString("CustomerService|CriticalityList|CR8_ComplianceRequirement", "Compliance, Reference and Master Data"));
				criticalityList.AddPair(Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, ResString.GetMultilingualString("CustomerService|CriticalityList|CR9_CustomerServiceRequest", "Service Request"));

				return criticalityList;
			}
		}

		#endregion Criticality

		#region Status

		public CodeDescriptionPairList StatusList
		{
			get
			{
				CodeDescriptionPairList statusList = new CodeDescriptionPairList();
				statusList.AddPair(StatusCodes.New, Res.GetString("CustomerService|StatusList|New", "Unsent"));
				statusList.AddPair(StatusCodes.ApprovedAndSent, Res.GetString("CustomerService|StatusList|ApprovedAndSent", "Approved and Sent to CargoWise"));
				statusList.AddPair(StatusCodes.SupportTeam, Res.GetString("CustomerService|StatusList|SupportTeam", "Customer Service Actioning"));
				statusList.AddPair(StatusCodes.DevelopmentTeam, Res.GetString("CustomerService|StatusList|DevelopmentTeam", "Development Team Actioning"));
				statusList.AddPair(StatusCodes.PendingFeatureResult, Res.GetString("CustomerService|StatusList|PendingFeatureResult", "Feature Request Sent"));
				statusList.AddPair(StatusCodes.FeatureRequestAccepted, Res.GetString("CustomerService|StatusList|PendingFeatureAccepted", "Closed - Feature Suggestion Received"));
				statusList.AddPair(StatusCodes.DevelopmentEstimateRequested, Res.GetString("CustomerService|StatusList|DevelopmentEstimateRequested", "Development Estimate Requested"));
				statusList.AddPair(StatusCodes.DevelopmentEstimateProvided, Res.GetString("CustomerService|StatusList|DevelopmentEstimateProvided", "Closed - Development Estimate Provided"));
				statusList.AddPair(StatusCodes.FormalQuotationRequested, Res.GetString("CustomerService|StatusList|FormalQuotationRequested", "Formal Quotation Requested"));
				statusList.AddPair(StatusCodes.FormalQuotationProvided, Res.GetString("CustomerService|StatusList|FormalQuotationProvided", "Closed - Formal Quotation Provided"));
				statusList.AddPair(StatusCodes.FormalQuotationAccepted, Res.GetString("CustomerService|StatusList|FormalQuotationAccepted", "Formal Quotation Accepted"));
				statusList.AddPair(StatusCodes.FormalQuotationDeclinded, Res.GetString("CustomerService|StatusList|FormalQuotationDeclinded", "Closed - Formal Quotation Declined"));
				statusList.AddPair(StatusCodes.ClosedAwaitingResponse, Res.GetString("CustomerService|StatusList|ClosedAwaitingResponse", "Awaiting Client Response"));
				statusList.AddPair(StatusCodes.Closed, Res.GetString("CustomerService|StatusList|Closed", "Closed"));

				return statusList;
			}
		}

		public static class StatusCodes
		{
			public const string New = "NEW";
			public const string ApprovedAndSent = "APP";
			public const string SupportTeam = "CSV";
			public const string DevelopmentTeam = "DEV";
			public const string PendingFeatureResult = "FTR";
			public const string FeatureRequestAccepted = "FRA";
			public const string DevelopmentEstimateRequested = "DER";
			public const string DevelopmentEstimateProvided = "DEP";
			public const string FormalQuotationRequested = "FQR";
			public const string FormalQuotationProvided = "FQP";
			public const string FormalQuotationAccepted = "FQA";
			public const string FormalQuotationDeclinded = "FQD";
			public const string ClosedAwaitingResponse = "CWR";
			public const string Closed = "CLS";
		}

		#endregion Status

		#region Customer Status

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public ReadOnlyCodeDescriptionPairList CustomerStatusList
		{
			get { return SystemDataRegistry.Instance.CustomerStatuses.Value; }
		}

		#endregion

		#region Licence Company

		public CodeDescriptionPairList LicenceCompanyList
		{
			get
			{
				return CreateLicenceCompanyList(Factory);
			}
		}

		public static CodeDescriptionPairList CreateLicenceCompanyList(CargoWise.EntityFramework.BusinessObjectFactory factory)
		{
			CodeDescriptionPairList licenceCompanyList = new UntranslatableCodeDescriptionPairList((NoResString)"Company names are not translatable");

			var currentCompany = GlbCompany.CurrentCompany;
			licenceCompanyList.AddPair(currentCompany.LicenceKeyIdentifier, currentCompany.GC_Name);

			GlbCompany[] list =
				factory.Load<GlbCompany>(
					new CargoWise.EntityFramework.ZQuery(ZArchitecture.Schema.GlbCompanySchema.GC_IsActive, ZBool.True));

			foreach (GlbCompany company in list)
			{
				ZString code = company.LicenceKeyIdentifier;
				if (code.Length == 9)
				{
					licenceCompanyList.AddPairIfNotExist(code, company.GC_Name);
				}
			}

			licenceCompanyList.SortByDescription();

			return licenceCompanyList;
		}

		#endregion

		#region Actions

		public static class Actions
		{
			public const string Add = "ADD"; // Still needed for eRequest in progress, yet to be approved
			public const string Update = "UPD"; // Needed for a reply to a new incident, containing our incident number
			public const string Email = "EML";
		}

		#endregion

		#region Constants

		public static class EmailRecipientSettings
		{
			public const string ApprovingStaff = "APP";
			public const string ReportedByStaff = "RPT";
			public const string AllParties = "ALL";
			public const string ThirdPartyNotifyOnly = "THI";
		}

		public const string IncidentApprovalLinkMacro = "(*ServiceRequestLink*)"; // Macro used for email content replacement.

		public const string EnterpriseProductCode = "ENT";

		#endregion
	}
}
