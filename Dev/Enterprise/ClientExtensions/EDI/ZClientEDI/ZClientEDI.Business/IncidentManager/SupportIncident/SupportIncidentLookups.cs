using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentLookups : IncidentMainLookups
	{
		public SupportIncidentLookups(SupportIncident parent) : base(parent)
		{
			factory = parent.Factory;
		}

		public SupportIncidentLookups(BusinessObjectFactory factory) : base(null)
		{
			this.factory = factory;
		}

		public SupportIncidentLookups(FilterStripBusinessObject businessObject) : base(null)
		{
			factory = businessObject.Factory;
		}

		public new SupportIncident Parent
		{
			get { return (SupportIncident)base.Parent; }
		}

		protected override BusinessObjectFactory Factory => factory ?? base.Factory;
		readonly BusinessObjectFactory factory;

		#region Clients

		public new OrganisationsFindBoxCollection Clients
		{
			get
			{
				if (fClients == null)
				{
					ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsActive, ZBool.True);
					fClients = new OrganisationsFindBoxCollection(Factory, filter);
				}

				return fClients;
			}
		}

		OrganisationsFindBoxCollection fClients;

		#endregion

		#region Contacts

		public OrgContactDependentCollection ContactList
		{
			get
			{
				if (fContactList == null || fContactList.Master != Parent.Client)
				{
					if (Parent.Client != null)
					{
						ZQuery activeContactQuery = new ZQuery(OrgContactSchema.OC_IsActive, true);
						fContactList = new OrgContactDependentCollection(Parent.Client, activeContactQuery);
					}
					else
					{
						fContactList = new OrgContactDependentCollection(Factory);
					}
				}

				return fContactList;
			}
		}

		OrgContactDependentCollection fContactList;

		#endregion

		#region Product

		public CodeDescriptionPairList ProductList
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.ProductList",
					() =>
					{
						return IncidentDetailsLookupsHelper.ProductList;
					});
			}
		}

		#endregion

		#region Module

		public CodeDescriptionPairList GetModuleList()
		{
			return GetModuleList(ZString.Empty);
		}

		public CodeDescriptionPairList GetModuleList(ZString product)
		{
			return GetModuleList(ModuleListType.Unspecified, product, ZString.Empty);
		}

		public CodeDescriptionPairList GetEnabledModuleList(ModuleListType moduleListType, ZString product, ZString productArea)
		{
			return GetNewModuleListBuilder().Build(moduleListType, product, productArea, excludeInternal: false, excludeDisabled: true);
		}

		public CodeDescriptionPairList GetModuleList(ModuleListType moduleListType, ZString product, ZString productArea)
		{
			return Factory.GetCachedValue("ModuleList:" + moduleListType.ToString() + product.PadRight(3) + productArea.PadRight(3),
				delegate
				{
					return GetModuleListCore(moduleListType, product, productArea);
				});
		}

		protected virtual CodeDescriptionPairList GetModuleListCore(ModuleListType moduleListType, ZString product, ZString productArea)
		{
			return GetNewModuleListBuilder().Build(moduleListType, product, productArea);
		}

		#endregion

		#region For RelatedItems

		#region Professional Services Quote List

		public ProfessionalServicesQuoteCollection ProfessionalServicesQuoteList
		{
			get
			{
				if (fProfessionalServicesQuoteList == null)
				{
					fProfessionalServicesQuoteList = new ProfessionalServicesQuoteCollection(Factory);
				}

				return fProfessionalServicesQuoteList;
			}
		}

		ProfessionalServicesQuoteCollection fProfessionalServicesQuoteList;

		#endregion

		#endregion

		public static class LegacyStatusCodes
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
			public const string FormalQuotationDeclined = "FQD";
			public const string ClosedAwaitingResponse = "CWR";
			public const string Closed = "CLS";
			public const string Resolved = "SLV";
		}

		public static class LegacyActions
		{
			public const string Add = "ADD";
			public const string Update = "UPD";
			public const string EConversation = "CON";
			public const string RequestReopen = "ROP";
			public const string Email = "EML";
		}

		#region Work Items

		public NewWorkItemCollection WorkItems
		{
			get
			{
				if (fWorkItems == null)
				{
					fWorkItems = new NewWorkItemCollection(Factory);
				}
				return fWorkItems;
			}
		}

		NewWorkItemCollection fWorkItems;

		#endregion

		#region Bug Severities

		public CodeDescriptionPairList BugSeverities
		{
			get
			{
				CodeDescriptionPairList bugSeverities = new CodeDescriptionPairList();

				bugSeverities = new CodeDescriptionPairList();
				bugSeverities.AddPair("EXT", "Extreme");
				bugSeverities.AddPair("HGH", "High");
				bugSeverities.AddPair("MED", "Medium");
				bugSeverities.AddPair("LOW", "Low");

				return bugSeverities;
			}
		}

		#endregion

		#region Criticality

		public CodeDescriptionPairList CriticalityList => IncidentDetailsLookupsHelper.CriticalityList;

		#endregion

		#region Product Area

		public virtual CodeDescriptionPairList ProductAreaList
		{
			get { return GetProductAreaList(); }
		}

		internal static CodeDescriptionPairList GetProductAreaList()
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(EDIDataRegistry.Instance.ProductAreas.Value);
			return result;
		}

		public CodeDescriptionPairList FilteredProductAreaList
		{
			get { return GetFilteredProductAreaList(Factory, Parent.ModuleType, Parent.IM_Product, Parent.IncidentTriage); }
		}

		public static CodeDescriptionPairList GetFilteredProductAreaList(BusinessObjectFactory factory, ModuleListType moduleType, ZString product, IncidentTriage triage = null)
		{
			return factory.GetCachedValue("SupportIncidentLookups.GetFilteredProductAreaList" + moduleType + product + triage?.PK, () =>
			{
				var result = new CodeDescriptionPairList();

				var productAreaModuleMappingsRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(moduleType);
				if (productAreaModuleMappingsRegistryItem != null)
				{
					var systemProduct = productAreaModuleMappingsRegistryItem.Value.GetProductByCode(product);
					if (systemProduct != null)
					{
						var moduleMappings = systemProduct.ModuleMappings.Cast<ProductAreaModuleMapping>();
						var moduleProductAreas = moduleMappings.Select(m => (string)m.ProductArea);
						var sourceModuleProductAreas = moduleMappings.SelectMany(m => m.SourceModuleMappings.Cast<ProductAreaSourceModuleMapping>().Select(s => (string)s.ProductArea));
						var usedProductAreas = new HashSet<string>(moduleProductAreas.Concat(sourceModuleProductAreas), StringComparer.OrdinalIgnoreCase);

						foreach (var item in EDIDataRegistry.Instance.ProductAreas.Value.Cast<ICodeDescription>())
						{
							if (usedProductAreas.Contains(item.Code))
							{
								result.Add(item);
							}
						}
					}
				}

				if (triage != null && !triage.IMT_SetProductAreaByMenuItem && !result.ContainsCode(triage.IMT_ProductArea))
				{
					result.AddPair(triage.IMT_ProductArea, triage.ProductAreaDescription);
				}

				result.Sort();
				return result;
			});
		}

		#endregion

		#region Module

		public CodeDescriptionPairList ModuleListAllModules
		{
			get
			{
				if (Parent != null)
				{
					return GetModuleList(Parent.ModuleType, Parent.IM_Product, Parent.ProductArea);
				}
				else
				{
					return GetModuleList();
				}
			}
		}

		public CodeDescriptionPairList ModuleListEnabledModulesOnly
		{
			get
			{
				var enabledModuleList = IncidentDetailsLookupsHelper.GetModuleListEnabledModulesOnly(Parent);
				AddTriageModuleToListIfRequired(enabledModuleList);

				return enabledModuleList;
			}
		}

		protected virtual IModuleListBuilder GetNewModuleListBuilder()
		{
			return new SupportIncidentModuleListBuilder();
		}

		void AddTriageModuleToListIfRequired(CodeDescriptionPairList moduleList)
		{
			if (Parent.IncidentTriage != null
					&& !Parent.IncidentTriage.IMT_SetProductAreaByMenuItem
					&& !moduleList.ContainsCode(Parent.IncidentTriage.IMT_Module)
					&& Parent.IM_Product == Parent.IncidentTriage.IMT_Product
					&& Parent.ProductArea == Parent.IncidentTriage.IMT_ProductArea
					&& Parent.ModuleType == Parent.IncidentTriage.ModuleType
					&& (EDIDataRegistry.Instance.SystemProductMappings.Value.GetMapping(Parent.IncidentTriage.IMT_Module)?.IsEnabled ?? false))
			{
				moduleList.AddPair(Parent.IncidentTriage.IMT_Module, Parent.IncidentTriage.ModuleDescription);
			}
		}

		public CodeDescriptionPairList ProductAreaIndependentModuleList
		{
			get
			{
				if (Parent != null)
				{
					var moduleList = new CodeDescriptionPairList();
					moduleList.AddRange(GetModuleList(Parent.ModuleType, Parent.IM_Product, ZString.Empty));
					AddTriageModuleToListIfRequired(moduleList);
					return moduleList;
				}
				else
				{
					return GetModuleList();
				}
			}
		}

		#endregion

		#region Soure Module List

		public static ReadOnlyCodeDescriptionPairList GetNewSearchableSourceModuleList(ModuleListType moduleType = ModuleListType.Unspecified)
		{
			var list = new CodeDescriptionPairList();

			foreach (SourceModule sourceModule in EDIDataRegistry.Instance.SourceModules.Value)
			{
				if (sourceModule.IsSearchable && (moduleType == ModuleListType.Unspecified || moduleType == sourceModule.ModuleListType))
				{
					list.AddPair(sourceModule.Code, sourceModule.Path + " " + sourceModule.Description);
				}
			}

			list.SortByDescription();

			return list;
		}

		#endregion

		#region Country

		public IBusinessObjectCollection CountryList
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		#region Feature Request Type

		public CodeDescriptionPairList FeatureRequestTypes
		{
			get
			{
				CodeDescriptionPairList featureRequestTypes = new CodeDescriptionPairList();
				featureRequestTypes.AddPair("CLI", "Client Specific");
				featureRequestTypes.AddPair("COP", "Co-Payment");
				featureRequestTypes.AddPair("PRD", "Free Product Enhancement");
				featureRequestTypes.AddPair("MOD", "New Module");

				return featureRequestTypes;
			}
		}

		#endregion

		#region Feature Request Industry Value

		public CodeDescriptionPairList FeatureRequestIndustryValueList
		{
			get
			{
				CodeDescriptionPairList featureRequestIndustryValueList = new CodeDescriptionPairList();
				featureRequestIndustryValueList.AddPair("VLW", "Very Low");
				featureRequestIndustryValueList.AddPair("LOW", "Low");
				featureRequestIndustryValueList.AddPair("MED", "Medium");
				featureRequestIndustryValueList.AddPair("HGH", "High");
				featureRequestIndustryValueList.AddPair("VHG", "Very High");

				return featureRequestIndustryValueList;
			}
		}

		#endregion

		#region Feature Request Contact

		public OrgContactDependentCollection FeatureRequestContactList
		{
			get
			{
				if (featureRequestContactList == null || featureRequestContactList.Master != Parent.FeatureRequestClient)
				{
					if (Parent.FeatureRequestClient != null)
					{
						ZQuery activeContactQuery = new ZQuery(OrgContactSchema.OC_IsActive, true);
						featureRequestContactList = new OrgContactDependentCollection(Parent.FeatureRequestClient, activeContactQuery);
					}
					else
					{
						featureRequestContactList = new OrgContactDependentCollection(Factory);
					}
				}

				return featureRequestContactList;
			}
		}

		OrgContactDependentCollection featureRequestContactList;

		#endregion

		#region Payment Terms

		#endregion

		#region Payment Types

		#endregion

		#region Source List

		public CodeDescriptionPairList SourceList
		{
			get
			{
				var sourceList = new CodeDescriptionPairList();
				if (Parent == null)
				{
					sourceList.AddPair(SourceListConstants.ERequestPortal, "eRequest Portal");
					sourceList.AddPair(SourceListConstants.IssueManagerReported, "Created From Issue Manager");
					sourceList.AddPair(SourceListConstants.CreatedFromProject, "Created From Project");
					sourceList.AddPair(SourceListConstants.APIInboundInternal, "API Inbound (Internal)");
					sourceList.AddPair(SourceListConstants.APIInboundExternal, "API Inbound (External)");
				}
				else if (Parent.IM_Source == SourceListConstants.ERequestPortal)
				{
					sourceList.AddPair(SourceListConstants.ERequestPortal, "eRequest Portal");
				}
				else if (Parent.IM_Source == SourceListConstants.IssueManagerReported)
				{
					sourceList.AddPair(SourceListConstants.IssueManagerReported, "Created From Issue Manager");
				}
				else if (Parent.IM_Source == SourceListConstants.CreatedFromProject)
				{
					sourceList.AddPair(SourceListConstants.CreatedFromProject, "Created From Project");
				}
				else if (Parent.IM_Source == SourceListConstants.APIInboundInternal)
				{
					sourceList.AddPair(SourceListConstants.APIInboundInternal, "API Inbound (Internal)");
				}
				else if (Parent.IM_Source == SourceListConstants.APIInboundExternal)
				{
					sourceList.AddPair(SourceListConstants.APIInboundExternal, "API Inbound (External)");
				}

				sourceList.AddPair(SourceListConstants.ProactiveOutbound, "Proactive Outbound");
				sourceList.AddPair(SourceListConstants.EmailInbound, "Email Inbound");
				sourceList.AddPair(SourceListConstants.ChatInbound, "Chat Inbound");
				sourceList.AddPair(SourceListConstants.PhoneInbound, "Phone Inbound");
				sourceList.AddPair(SourceListConstants.WTGInternalViaEdiProd, "WTG Internal via ediProd");

				return sourceList;
			}
		}

		public static class SourceListConstants
		{
			public const string ERequestPortal = "INC";
			public const string CreatedFromProject = "PRJ";
			public const string IssueManagerReported = "ISS";
			public const string ProactiveOutbound = "PRO";
			public const string EmailInbound = "EML";
			public const string PhoneInbound = "TEL";
			public const string APIInboundInternal = "API";
			public const string APIInboundExternal = "APE";
			public const string ChatInbound = "CHT";
			public const string WTGInternalViaEdiProd = "INT";
		}

		#endregion

		#region Service Status List

		public CodeDescriptionPairList ServiceStatusList
		{
			get
			{
				CodeDescriptionPairList serviceStatusList = new CodeDescriptionPairList();
				serviceStatusList.AddPair(ServiceStatusListConstants.ActiveServiceOutage, "Active Service Outage");
				serviceStatusList.AddPair(ServiceStatusListConstants.ServiceRestored, "Service Restored");

				return serviceStatusList;
			}
		}

		public static class ServiceStatusListConstants
		{
			public const string ActiveServiceOutage = "SVS";
			public const string ServiceRestored = "SVM";
		}

		#endregion

		#region Status Disposition

		#region List Class

		public static class DispositionList
		{
			public static class Constants
			{
				public static class Open
				{
					public const string AddedAwaitingAssignment = "ADD"; 
					public const string AssignedAwaitingAction = "AUC"; 
				}

				public static class Working
				{
					public const string Assigned = "AUC"; 
					public const string WorkInProgress = "WRK"; 
					public const string WorkItemCreated = "CFD"; 
				}

				public static class Suspended
				{
					public const string Deferred = "DEF"; 
				}

				public static class Closed
				{
					public const string NoResponseFromClient = "NRC";
					public const string ClosedAwaitingClientResponse = "CWR"; 

					//Below are special cases. Cannot be set by the user, but should be set programmatically when an incident is closed with an IsResolved IM_ClosureDisposition
					public static string Resolved => IncidentClosureDisposition.ResolvedCode;
					public static string ResolvedAndClosed => IncidentClosureDisposition.ResolvedAndClosedCode;

					// Codes below have been moved to registry. Constants here are used only for building default value of registry item IncidentClosureDispositions.
					public const string TrainingReferredToLearningMaterials = "TRN";
					public const string TrainingNoLearningMaterials = "TRM";
					public const string SystemHardwareNetwork = "SYS";
					public const string ThirdPartySystemProblem = "TSP";
					public const string NoDefectFound = "NDF";
					public const string NotFeatureRequest = "NFR";
					public const string NotComplianceRequirement = "NCR";
					public const string NotCustomerServiceRequest = "NCS";
					public const string NotContentDevelopmernt = "NCN";
					public const string WaitingUpgrade = "UPO"; 
					public const string UpgradeDelivered = "UPD";
					public const string SelfResolved = "SRS";
					public const string UnReproducible = "URP";
					public const string NoSupportContract = "NSC";
					public const string DeliveranceReindex = "REI";
					public const string Other = "OTH";
					public const string Cancelled = "CAN";
					public const string DuplicateIncident = "DUP";
					public const string CsStageDataFix = "DTF";
					public const string Completed = "COM";
					public const string ClosedInternal = "CLI";
					public const string DatabaseRequested = "DBR";
					public const string UpgradeDelayed = "UDO"; 
					public const string ClosedByIncidentGroup = "CIG";
					public const string TrainingFlaggedForContentDevelopment = "TRF";
				}

				// Feature Request CR6/CR7 workflow specific dispositions
				public const string FeatureAccepted = "AUT"; 
				public const string AwaitingDevelopmentEstimate = "ADE"; 
				public const string AwaitingFormalQuotation = "AFQ"; 
				public const string DevelopmentEstimateProvided = "DEP"; 
				public const string FormalQuotationProvided = "FQP"; 
				public const string FormalQuotationAccepted = "QAC"; 
				public const string FormalQuotationDeclined = "QDC"; 
				public const string FormalQuotationExpired = "QEX"; 
				public const string CallBackClient = "CBK";
			}
		}

		#endregion

		#region Can Re-Open IM_RequestStatus

		public static CodeDescriptionPairList CanReOpenRequestStatusList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(DispositionList.Constants.Closed.Resolved, "Resolved");
				list.AddPair(DispositionList.Constants.Closed.ResolvedAndClosed, "Closed");
				list.AddPair(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "Awaiting Client Response");
				return list;
			}
		}

		#endregion

		#region Can Close on behalf of Client (Confirmed Resolved). IM_ResolutionCode

		public static CodeDescriptionPairList CanCloseOnBehalfCodeList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(DispositionList.Constants.Open.AddedAwaitingAssignment, "Added Awaiting Assignment");
				list.AddPair(DispositionList.Constants.Open.AssignedAwaitingAction, "Assigned - Awaiting Action");
				list.AddPair(DispositionList.Constants.Working.WorkInProgress, "Work in Progress / Investigating");
				return list;
			}
		}

		#endregion

		public CodeDescriptionPairList StatusDispositionList
		{
			get { return GetStatusDispositionList(Parent.IM_Category, Parent.IM_Status, Parent.IM_Priority, Parent.IM_Product, shouldShowClosedDispositions: Parent.ShouldShowClosedDispositions, activeOnly: false); }
		}

		public CodeDescriptionPairList GetStatusDispositionList(string stageCode, string status, string criticality, string product, bool shouldShowClosedDispositions = false, bool activeOnly = true)
		{
			switch (stageCode)
			{
				case SupportIncidentCategoriesList.Codes.Support:
					return GetSupportStatusDispositionList(status, criticality, product, shouldShowClosedDispositions: shouldShowClosedDispositions, activeOnly: activeOnly);

				case SupportIncidentCategoriesList.Codes.ContentDevelopment:
					return GetContentDevelopmentStatusDispositionList(status, criticality, product, shouldShowClosedDispositions: shouldShowClosedDispositions, activeOnly: activeOnly);

				case SupportIncidentCategoriesList.Codes.Defect:
					return GetDefectStatusDispositionList(status, criticality, product, shouldShowClosedDispositions: shouldShowClosedDispositions, activeOnly: activeOnly);

				case SupportIncidentCategoriesList.Codes.FeatureRequest:
					return GetFeatureRequestStatusDispositionList(status, criticality, product, shouldShowClosedDispositions: shouldShowClosedDispositions, activeOnly: activeOnly);

				case SupportIncidentCategoriesList.Codes.ComplianceRequirement:
					return GetComplianceRequirementStatusDispositionList(status, criticality, product, shouldShowClosedDispositions: shouldShowClosedDispositions, activeOnly: activeOnly);

				case SupportIncidentCategoriesList.Codes.CustomerServiceRequest:
					return GetCustomerServiceStatusDispositionList(status, criticality, product, shouldShowClosedDispositions: shouldShowClosedDispositions, activeOnly: activeOnly);
			}

			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList ActiveCloseStatusDispositionList => new SupportIncidentCloseAction(Parent).ActiveCloseStatusDispositionList;

		public CodeDescriptionPairList AllCloseStatusDispositionList => new SupportIncidentCloseAction(Parent).GetCloseStatusDispositionList(false);

		#region Support

		public CodeDescriptionPairList GetSupportStatusDispositionList(string mainStatus, string criticality, string product, bool shouldShowClosedDispositions = false, bool activeOnly = true)
		{
			var list = new CodeDescriptionPairList();

			if (mainStatus == Status.Open || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Open.AddedAwaitingAssignment, "Added Awaiting Assignment");
				list.AddPair(DispositionList.Constants.Open.AssignedAwaitingAction, "Assigned - Awaiting Action");
			}

			if (mainStatus == Status.Working || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Working.Assigned, "Assigned - Awaiting Action");
				list.AddPair(DispositionList.Constants.Working.WorkInProgress, "Work in Progress / Investigating");
			}

			if (mainStatus == Status.Suspended || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Suspended.Deferred, GetSuspendedDescription());
			}

			if (mainStatus == Status.Closed || mainStatus == Status.ClosedDirectlyInSupport || string.IsNullOrEmpty(mainStatus) || shouldShowClosedDispositions)
			{
				list.AddRange(GetClosureDispositionList(activeOnly, SupportIncidentCategoriesList.Codes.Support, criticality, product));
				AddAwaitingClientDispositions(list);
			}

			return list;
		}

		public CodeDescriptionPairList ClosedSupportStatusDispositionList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddRange(GetClosureDispositionList(true, SupportIncidentCategoriesList.Codes.Support, Parent.IM_Priority, Parent.IM_Product));
				AddAwaitingClientDispositions(list);
				return list;
			}
		}

		#endregion

		#region Defect

		public CodeDescriptionPairList GetDefectStatusDispositionList(string mainStatus, string criticality, string product, bool shouldShowClosedDispositions = false, bool activeOnly = true)
		{
			var list = new CodeDescriptionPairList();
			if (mainStatus == Status.Open || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Open.AddedAwaitingAssignment, "Added Awaiting Assignment");
				list.AddPair(DispositionList.Constants.Open.AssignedAwaitingAction, "Assigned - Awaiting Action");
			}

			if (mainStatus == Status.Working || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Working.Assigned, "Assigned - Awaiting Action");
				list.AddPair(DispositionList.Constants.Working.WorkInProgress, "Work in Progress / Investigating");
				list.AddPair(DispositionList.Constants.Working.WorkItemCreated, "Work Item Created / Linked");
				list.AddPair(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
				list.AddPair(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
			}

			if (mainStatus == Status.Suspended || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Suspended.Deferred, GetSuspendedDescription());
			}

			if (mainStatus == Status.Closed || string.IsNullOrEmpty(mainStatus) || shouldShowClosedDispositions)
			{
				list.AddRange(GetClosureDispositionList(activeOnly, SupportIncidentCategoriesList.Codes.Defect, criticality, product));
				list.AddPairIfNotExist(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
				list.AddPairIfNotExist(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
				AddAwaitingClientDispositions(list);
			}

			return list;
		}

		#endregion

		#region Feature Request

		public CodeDescriptionPairList GetFeatureRequestStatusDispositionList(string mainStatus, string criticality, string product, bool shouldShowClosedDispositions = false, bool activeOnly = true)
		{
			var list = new CodeDescriptionPairList();
			if (mainStatus == Status.Open || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Open.AddedAwaitingAssignment, "Added Awaiting Assignment");
				list.AddPair(DispositionList.Constants.Open.AssignedAwaitingAction, "Assigned - Awaiting Action");
			}

			if (mainStatus == Status.Working || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Working.WorkInProgress, GetWorkInProgressDescription());
				list.AddPair(DispositionList.Constants.Working.Assigned, "Assigned - Awaiting Action");
				list.AddPair(DispositionList.Constants.Working.WorkItemCreated, "Work Item Created / Linked");
				list.AddPair(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
				list.AddPair(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
			}

			if (mainStatus == Status.Suspended || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Suspended.Deferred, GetSuspendedDescription());
			}

			if (mainStatus == Status.Closed || string.IsNullOrEmpty(mainStatus) || shouldShowClosedDispositions)
			{
				list.AddRange(GetClosureDispositionList(activeOnly, SupportIncidentCategoriesList.Codes.FeatureRequest, criticality, product));
				list.AddPairIfNotExist(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
				AddAwaitingClientDispositions(list);
			}

			AddCR7Dispositions(list);

			return list;
		}

		void AddCR7Dispositions(CodeDescriptionPairList list)
		{
			list.AddPairIfNotExist(DispositionList.Constants.FeatureAccepted, "Feature Request Accepted");
			list.AddPairIfNotExist(DispositionList.Constants.AwaitingDevelopmentEstimate, "Awaiting Development Estimate");
			list.AddPairIfNotExist(DispositionList.Constants.AwaitingFormalQuotation, "Awaiting Formal Quotation");
			list.AddPairIfNotExist(DispositionList.Constants.DevelopmentEstimateProvided, "Development Estimate Provided - Awaiting Customer");
			list.AddPairIfNotExist(DispositionList.Constants.FormalQuotationProvided, "Formal Quotation Provided - Awaiting Customer");
			list.AddPairIfNotExist(DispositionList.Constants.FormalQuotationAccepted, "Formal Quotation Accepted");
			list.AddPairIfNotExist(DispositionList.Constants.FormalQuotationDeclined, "Formal Quotation Declined");
			list.AddPairIfNotExist(DispositionList.Constants.FormalQuotationExpired, "Formal Quotation Expired");
		}

		string GetWorkInProgressDescription()
		{
			string workInProgressDesc;
			if (IsProjectOrInternalFeatureRequestLookups)
			{
				workInProgressDesc = "In Progress";
				ProcessTask currentTask = Parent.CurrentTask;
				if (currentTask != null && !currentTask.TypeDescription.IsEmpty)
				{
					workInProgressDesc = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", workInProgressDesc, currentTask.TypeDescription);
				}
			}
			else
			{
				workInProgressDesc = "Work in Progress / Investigating";
			}
			return workInProgressDesc;
		}

		string GetSuspendedDescription()
		{
			string suspendedDesc = string.Empty;
			if (IsProjectOrInternalFeatureRequestLookups)
			{
				ProcessTask currentTask = Parent.CurrentTask;
				if (currentTask != null && !currentTask.TypeDescription.IsEmpty)
				{
					suspendedDesc = currentTask.TypeDescription;
				}
			}
			else
			{
				suspendedDesc = "Suspended";
			}
			return suspendedDesc;
		}

		bool IsProjectOrInternalFeatureRequestLookups
		{
			get { return Parent != null && (Parent.IsProjectRelatedIncident || Parent.IsInternalFeatureRequest); }
		}

		#endregion

		#region Compliance Requirement

		public CodeDescriptionPairList GetComplianceRequirementStatusDispositionList(string mainStatus, string criticality, string product, bool shouldShowClosedDispositions = false, bool activeOnly = true)
		{
			var list = new CodeDescriptionPairList();
			AddCommonDispositions(list, mainStatus, shouldShowClosedDispositions);
			if (mainStatus == Status.Closed || string.IsNullOrEmpty(mainStatus) || shouldShowClosedDispositions)
			{
				list.AddRangeOverwriteIfExists(GetClosureDispositionList(activeOnly, SupportIncidentCategoriesList.Codes.ComplianceRequirement, criticality, product));
				list.AddPairIfNotExist(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
			}

			return list;
		}

		#endregion

		#region Customer Service

		public CodeDescriptionPairList GetCustomerServiceStatusDispositionList(string mainStatus, string criticality, string product, bool shouldShowClosedDispositions = false, bool activeOnly = true)
		{
			var list = new CodeDescriptionPairList();
			AddCommonDispositions(list, mainStatus, shouldShowClosedDispositions);
			if (mainStatus == Status.Closed || string.IsNullOrEmpty(mainStatus) || shouldShowClosedDispositions)
			{
				list.AddRangeOverwriteIfExists(GetClosureDispositionList(activeOnly, SupportIncidentCategoriesList.Codes.CustomerServiceRequest, criticality, product));
				list.AddPairIfNotExist(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
			}

			return list;
		}

		#endregion

		#region Content Development

		public CodeDescriptionPairList GetContentDevelopmentStatusDispositionList(string mainStatus, string criticality, string product, bool shouldShowClosedDispositions = false, bool activeOnly = true)
		{
			var list = new CodeDescriptionPairList();
			AddCommonDispositions(list, mainStatus, shouldShowClosedDispositions);
			if (mainStatus == Status.Closed || string.IsNullOrEmpty(mainStatus) || shouldShowClosedDispositions)
			{
				list.AddRangeOverwriteIfExists(GetClosureDispositionList(activeOnly, SupportIncidentCategoriesList.Codes.ContentDevelopment, criticality, product));
				list.AddPairIfNotExist(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
			}

			return list;
		}

		#endregion

		void AddCommonDispositions(CodeDescriptionPairList list, string mainStatus, bool isClosedOrResolved = false)
		{
			if (mainStatus == Status.Open || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Open.AddedAwaitingAssignment, "Added Awaiting Assignment");
			}

			if (mainStatus == Status.Open || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus) ||
				mainStatus == Status.Working)
			{
				list.AddPair(DispositionList.Constants.Working.Assigned, "Assigned - Awaiting Action");
			}

			if (mainStatus == Status.Working || mainStatus == Status.NotClosed || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Working.WorkInProgress, "Work in Progress / Investigating");
				list.AddPair(DispositionList.Constants.Working.WorkItemCreated, "Work Item Created / Linked");
				list.AddPair(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
				list.AddPair(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
			}

			if (mainStatus == Status.Suspended || string.IsNullOrEmpty(mainStatus))
			{
				list.AddPair(DispositionList.Constants.Suspended.Deferred, GetSuspendedDescription());
			}

			if (mainStatus == Status.Closed || string.IsNullOrEmpty(mainStatus) || isClosedOrResolved)
			{
				AddAwaitingClientDispositions(list);
			}
		}

		void AddAwaitingClientDispositions(CodeDescriptionPairList list)
		{
			list.AddPairIfNotExist(DispositionList.Constants.Closed.NoResponseFromClient, "No Response from Client");
			list.AddPairIfNotExist(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "Awaiting Client Response");
		}

		public CodeDescriptionPairList GetClosureDispositionList(bool activeOnly, ZString stage, ZString criticality, ZString product)
		{
			return Factory.GetCachedValue(
				"SupportIncidentLookups.GetClosureDispositionList:" + stage + ":" + criticality + ":" + product + ':' + (activeOnly ? 'A' : 'X'),
				() =>
				{
					var tree = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
					CodeDescriptionPairList result;

					result = tree.GetChildrenExactMatchOnly(activeOnly, stage, criticality, product);

					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, stage, criticality, CodeDescriptionBoolTreeNode.AllCode);
					}
					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, stage, CodeDescriptionBoolTreeNode.AllCode, product);
					}
					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, stage, CodeDescriptionBoolTreeNode.AllCode, CodeDescriptionBoolTreeNode.AllCode);
					}

					result.AddPair(DispositionList.Constants.Closed.Resolved, "Resolved");
					result.AddPair(DispositionList.Constants.Closed.ResolvedAndClosed, "Closed");

					return result;
				});
		}

		public CodeDescriptionPairList GetAllERequestStatuses()
		{
			var result = new CodeDescriptionPairList();
			result.AddPairIfNotExist(DispositionList.Constants.Closed.Resolved, "Resolved");
			result.AddPairIfNotExist(DispositionList.Constants.Closed.ResolvedAndClosed, "Closed");
			result.AddPairIfNotExist(DispositionList.Constants.Closed.UpgradeDelayed, "Upgrade Delayed");
			result.AddPairIfNotExist(DispositionList.Constants.Working.WorkInProgress, "Work in Progress / Investigating");
			result.AddPairIfNotExist(DispositionList.Constants.Suspended.Deferred, "Suspended");
			result.AddPairIfNotExist(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "Awaiting Client Response");
			result.AddPairIfNotExist(DispositionList.Constants.Working.WorkItemCreated, "Work Item Created / Linked");
			result.AddPairIfNotExist(DispositionList.Constants.Open.AssignedAwaitingAction, "Assigned - Awaiting Action");
			result.AddPairIfNotExist(DispositionList.Constants.AwaitingDevelopmentEstimate, "Awaiting Development Estimate");
			result.AddPairIfNotExist(DispositionList.Constants.Open.AddedAwaitingAssignment, "Added Awaiting Assignment");
			result.AddPairIfNotExist(DispositionList.Constants.Closed.WaitingUpgrade, "Awaiting Auto Upgrade Deployment");
			result.AddPairIfNotExist(DispositionList.Constants.FeatureAccepted, "Feature Request Accepted");
			result.AddPairIfNotExist(DispositionList.Constants.AwaitingFormalQuotation, "Awaiting Formal Quotation");
			result.AddPairIfNotExist(DispositionList.Constants.DevelopmentEstimateProvided, "Development Estimate Provided - Awaiting Customer");
			result.AddPairIfNotExist(DispositionList.Constants.FormalQuotationProvided, "Formal Quotation Provided - Awaiting Customer");
			result.AddPairIfNotExist(DispositionList.Constants.FormalQuotationAccepted, "Formal Quotation Accepted");
			result.AddPairIfNotExist(DispositionList.Constants.FormalQuotationDeclined, "Formal Quotation Declined");
			result.AddPairIfNotExist(DispositionList.Constants.FormalQuotationExpired, "Formal Quotation Expired");
			return result;
		}

		public static ResolutionAndClosureBehaviour GetResolutionAndClosureBehaviour(BusinessObjectFactory factory, string code, ZGuid parentID)
		{
			return factory.GetCachedValue("SupportIncidentLookups.GetResolutionAndClosureBehaviour:" + code + ":" + parentID, () =>
			{
				var collection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value.Cast<ResolutionAndClosureBehaviour>();
				return collection.FirstOrDefault(x => x.ParentID == parentID && x.Code == code) ?? collection.FirstOrDefault(x => x.ParentID == parentID && x.Code.IsEmpty);
			});
		}

		#endregion

		#region Feature Requests

		public SupportIncidentCollection FeatureRequests
		{
			get
			{
				if (fFeatureRequests == null)
				{
					ZQuery query = new ZQuery(IncidentMainSchema.IM_Category, SupportIncidentCategoriesList.Codes.FeatureRequest);
					query.AddToFilter(IncidentMainSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
					fFeatureRequests = new SupportIncidentCollection(Factory, query);
					fFeatureRequests.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Stage", "Property", (ZString)SupportIncidentCategoriesList.Codes.FeatureRequest));
				}
				return fFeatureRequests;
			}
		}

		SupportIncidentCollection fFeatureRequests;

		#endregion

		#region Staff List

		public override GlbStaffCollection SpecifiedBys
		{
			get { return AllStaffList; }
		}

		public GlbStaffCollection AllStaffList
		{
			get { return allStaffList ?? (allStaffList = new GlbStaffCollection(Factory)); }
		}
		GlbStaffCollection allStaffList;

		#endregion

		#region Stage List

		public CodeDescriptionPairList StageList
		{
			get { return new SupportIncidentCategoriesList(); }
		}

		#endregion

		#region Project List

		public ProjectCollection Projects
		{
			get { return projects ?? (projects = new ProjectCollection(Factory)); }
		}

		ProjectCollection projects;

		#endregion

		#region Escalation Criticality Mapping

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<ZString, List<ZString>> ActiveStageCriticalityMapping
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.ActiveStageCriticalityMapping",
				() =>
				{
					return GetStageCriticalityMapping(true);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<ZString, List<ZString>> AllStageCriticalityMapping
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.AllStageCriticalityMapping",
				() =>
				{
					return GetStageCriticalityMapping(false);
				});
			}
		}

		Dictionary<ZString, List<ZString>> GetStageCriticalityMapping(bool activeOnly)
		{
			var result = new Dictionary<ZString, List<ZString>>();
			var map = EDIDataRegistry.Instance.IncidentCriticalityStages.Value;
			foreach (CodeDescriptionPair criticality in CriticalityList)
			{
				var childStageList = map.GetChildren(false, criticality.Code);
				foreach (ICodeDescriptionBool stage in childStageList)
				{
					if (!activeOnly || stage.Bool)
					{
						if (!result.ContainsKey(stage.Code))
						{
							result[stage.Code] = new List<ZString>();
						}
						result[stage.Code].Add(criticality.Code);
					}
				}
			}

			return result;
		}

		public Dictionary<ZString, ZString> CriticalityDefaultStageMapping
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.CriticalityDefaultStageMapping",
				() =>
				{
					var result = new Dictionary<ZString, ZString>();
					var map = EDIDataRegistry.Instance.IncidentCriticalityStages.Value;
					foreach (CodeDescriptionPair criticality in CriticalityList)
					{
						var childStageList = map.GetChildren(false, criticality.Code);
						foreach (CriticalityStageMapping stage in childStageList)
						{
							if (stage.Bool && stage.IsDefault)
							{
								result[criticality.Code] = stage.Code;
								break;
							}
						}

						if (result[criticality.Code].IsEmpty && childStageList.Count > 0)
						{
							result[criticality.Code] = childStageList[0].Code;
						}
					}
					return result;
				});
			}
		}

		#endregion

		#region IncidentClosureDispositions

		public IncidentClosureDispositionCollection IncidentClosureDispositions
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.IncidentClosureDispositions",
					() =>
					{
						return EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
					});
			}
		}

		#endregion

		#region EnterpriseList

		public LicenceEnterpriseCollection EnterpriseList
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.EnterpriseList",
					() =>
					{
						return new LicenceEnterpriseCollection(Factory);
					});
			}
		}

		public LicenceEnterpriseCollectionForEntCodeFilter EnterpriseCodeList =>
			Factory.GetCachedValue("SupportIncidentLookups.EnterpriseCodeList", () => new LicenceEnterpriseCollectionForEntCodeFilter(Factory));

		#endregion

		#region DatabaseList

		public LicenceDatabaseNonDependentCollection DatabaseList
		{
			get { return GetCachedDatabaseListForEnterprise(Factory, Parent.EnterprisePK); }
		}

		public static LicenceDatabaseNonDependentCollection GetCachedDatabaseListForEnterprise(BusinessObjectFactory factory, ZGuid enterprisePk)
		{
			return factory.GetCachedValue("SupportIncidentLookups.DatabaseList." + enterprisePk,
				() =>
				{
					var enterprise = factory.Load<LicenceEnterprise>(enterprisePk);
					if (enterprise != null)
					{
						var query = new ZQuery(LicenceDatabaseSchema.LD_LE, enterprisePk);
						query.AddToFilter(LicenceDatabaseSchema.LD_IsActive, true);
						var collection = new LicenceDatabaseNonDependentCollection(factory, query);
						collection.Load();
						return collection;
					}
					else
					{
						var noResultQuery = new ZQuery();
						noResultQuery.IsNoResultQuery = true;
						return new LicenceDatabaseNonDependentCollection(factory, noResultQuery);
					}
				});
		}

		public CodeDescriptionPairList DatabaseCodeDescriptionPairList
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.DatabaseCodeDescriptionPairList." + Parent.EnterprisePK,
					() =>
					{
						return LicenceDatabaseLookups.BuildDatabaseCategoryCodeDescriptionList(DatabaseList.Cast<LicenceDatabase>());
					});
			}
		}

		#endregion

		#region ClientCompanyList

		public ClientCompanyCollection ActiveClientCompanyList
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.ClientCompanyList." + Parent.IM_LD,
					() =>
					{
						var query = new ZDBOnlyQuery(typeof(ClientCompany));
						if (Parent.Database != null)
						{
							query.AddToFilter(ClientCompanySchema.LCC_LD, Parent.IM_LD);
							query.AddToFilter(ClientCompanySchema.LCC_DeactivateTimeUtc, ZDateTime.Empty);
						}
						else
						{
							query.IsNoResultQuery = true;
						}

						return new ClientCompanyCollection(Factory, query);
					});
			}
		}

		public CodeDescriptionPairList ActiveClientCompanyCodeDescriptionPairList
		{
			get
			{
				return Factory.GetCachedValue("SupportIncidentLookups.ActiveClientCompanyCodeDescriptionPairList." + Parent.IM_OH_Client + "|" + Parent.IM_LD,
					() =>
					{
						return ClientCompanyLookups.BuildCompanyCategoryCodeDescriptionList(ActiveClientCompanyList, Parent.Client);
					});
			}
		}

		public CodeDescriptionPairList ClientCompanyCodeDescriptionPairList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(ActiveClientCompanyCodeDescriptionPairList);

				if (Parent.IsInDatabase && Parent.ClientCompany != null && !result.ContainsCode(Parent.ClientCompanyCode))
				{
					result.AddPair(Parent.ClientCompanyCode, Parent.ClientCompany.RelatedOrgName);
				}

				return result;
			}
		}

		#endregion

		#region ReleaseRingsList

		public ReleaseRingsList ReleaseRingsList
		{
			get { return new ReleaseRingsList(); }
		}

		#endregion

		#region Language

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		#endregion

		#region Service Type

		public CodeDescriptionPairList ServiceTypeList => IncidentDetailsLookupsHelper.GetServiceTypeList(Parent);

		#endregion

		#region Triage

		public IncidentTriageCollection TriageList => Factory.GetCachedValue("SupportIncidentLookups.TriageList", () => new IncidentTriageCollection(Factory));

		#endregion

		#region Email

		public static string SupportEmailAddress => EDIDataRegistry.Instance.IncidentFromEmailAddress.Value;
		public static string EnterpriseProductionAddress => "enterpriseproduction@cargowise.com";

		#endregion

	}
}

