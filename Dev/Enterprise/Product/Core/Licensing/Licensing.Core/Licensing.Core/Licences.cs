using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

// WARNING WARNING WARNING
// WARNING WARNING WARNING
// WARNING WARNING WARNING Please advise Chrystalla Lisgaris if you are making a change to the licencing module list.
// WARNING WARNING WARNING For example, adding, reordering or renaming licence modules.
// WARNING WARNING WARNING
// WARNING WARNING WARNING

namespace Enterprise.Licensing
{
	public class Licences : LicenceSegment, ILicenceProxy, ILicenceCheckpointUserContext
	{
		#region Construction

#if DEBUG
		/// <summary>
		/// For unit tests only, need to explicitly pass a user context instance for production use
		/// </summary>
		public Licences()
			: this(EnvProxy.Instance.CurrentUserContext)
		{
			LogVerboseIfApplicable("public Licences()");
		}

		internal Licences(IUser user)
			: this(user, EnvProxy.Instance.CurrentCompany, EnvProxy.Instance.CurrentBranch)
		{
			LogVerboseIfApplicable("internal Licences(IUser user)");
		}
#endif

		/// <summary>
		/// Create a Licence object that is editable and can be used to generate a new licence key
		/// </summary>
		public Licences(IUserContext userContext)
			: this(userContext.User, userContext.Company, userContext.Branch)
		{
			LogVerboseIfApplicable("public Licences(IUserContext userContext)");
		}

		Licences(IUser user, ICompany company, IBranch branch)
		{
			LogVerboseIfApplicable("public Licences(IUser user, ICompany company)");
			CurrentUser = user;
			CurrentBranch = branch;
			CurrentCompany = company;
			AlwaysAllow = new AlwaysAllowLicenceCheckpoint();

			#region Checkpoint Construction

			Core = new LicenceCheckpoint("COR", "Core", this, null, LicenceModuleCategories.Codes.Core);

			Accountant = new LicenceCheckpoint("ACC", "Accountant", this, Core, LicenceModuleCategories.Codes.Accounting);
			Workflow = new LicenceCheckpoint("PRO", "WorkflowManager", this, Core, LicenceModuleCategories.Codes.WorkflowManager);
			BufferManagement = new LicenceCheckpoint("BMS", "BufferManagement", this, Core, LicenceModuleCategories.Codes.WorkflowManager);
			ProductivityTools = new LicenceCheckpoint("PRD", "ProductivityTools", this, Core, LicenceModuleCategories.Codes.WorkflowManager);
			DocumentCustomisation = new LicenceCheckpoint("DOC", "Document Customisation", this, Core, LicenceModuleCategories.Codes.Core);
			ReportWriter = new LicenceCheckpoint("REP", "ReportWriter", this, Core, LicenceModuleCategories.Codes.Core);
			BusinessIntelligence = new LicenceCheckpoint("BIA", "Business Intelligence & Analytics", this, Core, LicenceModuleCategories.Codes.Core);
			ArchiveManager = new LicenceCheckpoint("ARM", "ArchiveManager", this, Core, LicenceModuleCategories.Codes.Miscellaneous);
			MasterDataManagement = new LicenceCheckpoint("MDM", "MasterDataManagement", this, Core, LicenceModuleCategories.Codes.MasterDataManagement);

			Forwarder = new LicenceCheckpoint("FOR", "Forwarder", this, Core, LicenceModuleCategories.Codes.Forwarding);
			PRAMessaging = new LicenceCheckpoint("PRA", "PRA Messaging", this, Forwarder, LicenceModuleCategories.Codes.Forwarding)
			{
				IsObsolete = true
			};
			PRAMessagingPerTransaction = new LicenceCheckpoint("PRT", "PRA Messaging (Per Transaction)", this, Forwarder, LicenceModuleCategories.Codes.Forwarding) { IsDefaultTransactional = true };
			ExportDocuments = new LicenceCheckpoint("EXD", "ExportDocuments", this, CoreCustomsModule, LicenceModuleCategories.Codes.Eservices);
			DeniedPartyScreening = new LicenceCheckpoint("DPS", "eServices - Denied Party Screening", this, Forwarder, LicenceModuleCategories.Codes.Forwarding);
			ForwarderContainersPacking = new LicenceCheckpoint("FCP", "Forwarder - Containers Packing", this, Forwarder, LicenceModuleCategories.Codes.Forwarding);
			Manifest = new LicenceCheckpoint("MFT", "Manifest", this, Forwarder, LicenceModuleCategories.Codes.Forwarding);
			ExportManifest = new LicenceCheckpoint("EMF", "ExportManifest (CRN)", this, Manifest, LicenceModuleCategories.Codes.Customs);
			ImportManifest = new LicenceCheckpoint("IMF", "ImportManifest (ACA F, SCA F)", this, Manifest, LicenceModuleCategories.Codes.Customs);
			AirCargoReport = new LicenceCheckpoint("ACR", "AirCargoReport", this, ImportManifest, LicenceModuleCategories.Codes.Customs);
			SeaCargoReport = new LicenceCheckpoint("SCR", "SeaCargoReport", this, ImportManifest, LicenceModuleCategories.Codes.Customs);

			ImportAirCargo = new LicenceCheckpoint("IAC", "ImportAirCargo", this, Manifest, LicenceModuleCategories.Codes.Customs);
			ImportSeaCargo = new LicenceCheckpoint("ISC", "ImportSeaCargo", this, Manifest, LicenceModuleCategories.Codes.Customs);

			ACIReporting = new LicenceCheckpoint("ACI", "ACIReporting - OBSOLETE", this, Manifest, LicenceModuleCategories.Codes.Forwarding)
			{
				IsObsolete = true
			};
			ACIReportingPerTransaction = new LicenceCheckpoint("ACP", "ACIReporting (Per Transaction)", this, Manifest, LicenceModuleCategories.Codes.Forwarding) { IsDefaultTransactional = true };
			ACIeManifestReportingPerTransaction = new LicenceCheckpoint("ACE", "ACIeManifestReporting (Per Transaction)", this, Manifest, LicenceModuleCategories.Codes.Forwarding) { IsDefaultTransactional = true };
			AirCcsukBase = new LicenceCheckpoint("UKC", "AirCcsukBase", this, ImportManifest, LicenceModuleCategories.Codes.Customs);
			AirCcsuk = new LicenceCheckpoint("UKA", "AirCcsuk", this, AirCcsukBase, LicenceModuleCategories.Codes.Customs);
			AirCcsukShed = new LicenceCheckpoint("UKS", "AirCcsukShed", this, AirCcsukBase, LicenceModuleCategories.Codes.Customs);
			AirCcsukDEP = new LicenceCheckpoint("UKD", "AirCcsukDEP", this, AirCcsukBase, LicenceModuleCategories.Codes.Customs, false);
			AirCcsukDEP.IsObsolete = true;
			NCTS = new LicenceCheckpoint("NCT", "NCTS", this, Broker, LicenceModuleCategories.Codes.Customs);
			AMSReporting = new LicenceCheckpoint("AMS", "AMSReporting", this, Manifest, LicenceModuleCategories.Codes.Forwarding) { IsDefaultTransactional = true };
			StowPlanReporting = new LicenceCheckpoint("STW", "StowPlanReporting", this, Manifest, LicenceModuleCategories.Codes.Forwarding) { IsDefaultTransactional = true };
			CMDReporting = new LicenceCheckpoint("CMD", "CMDReporting", this, Manifest, LicenceModuleCategories.Codes.Forwarding) { IsDefaultTransactional = true };
			USeManifest = new LicenceCheckpoint("MAN", "Manifest (US e-Manifest)", this, Manifest, LicenceModuleCategories.Codes.Forwarding, shouldCreateConsumptionLogOnLogin: false) { IsDefaultTransactional = true };
			HVLVClearance = new LicenceCheckpoint("HLC", "HVLVClearance", this, Manifest, LicenceModuleCategories.Codes.Customs);
			eCommerce = new LicenceCheckpoint("HVL", "eCommerce", this, Forwarder, LicenceModuleCategories.Codes.Forwarding);

			Broker = new LicenceCheckpoint("BRK", "Broker", this, Core, LicenceModuleCategories.Codes.Customs);
			CoreCustomsModule = new LicenceCheckpoint("CCM", "CoreCustomsModule", this, Broker, LicenceModuleCategories.Codes.Customs);
			ExportBroker = new LicenceCheckpoint("XBR", "ExportBroker", this, Broker, LicenceModuleCategories.Codes.Customs);
			ImportBroker = new LicenceCheckpoint("IBR", "ImportBroker", this, CoreCustomsModule, LicenceModuleCategories.Codes.Customs);
			LandedCosting = new LicenceCheckpoint("LDC", "LandedCosting", this, CoreCustomsModule, LicenceModuleCategories.Codes.Customs);
			BondedWarehouse = new LicenceCheckpoint("BDS", "BondedWarehouse", this, CoreCustomsModule, LicenceModuleCategories.Codes.Warehouse);
			EMCS = new LicenceCheckpoint("EMC", "EMCS", this, CoreCustomsModule, LicenceModuleCategories.Codes.Customs);
			Intrastat = new LicenceCheckpoint("IST", "Intrastat", this, CoreCustomsModule, LicenceModuleCategories.Codes.Customs);
			Drawback = new LicenceCheckpoint("DRB", "Drawback", this, CoreCustomsModule, LicenceModuleCategories.Codes.Customs);
			ImportQuarantineMessaging = new LicenceCheckpoint("IQM", "ImportQuarantine (eBACCa)", this, ImportBroker, LicenceModuleCategories.Codes.Customs) { IsDefaultTransactional = true };
			ReleaseNotificationSystem = new LicenceCheckpoint("RNS", "ReleaseNotificationSystem", this, Core, LicenceModuleCategories.Codes.Customs);

			ImporterSecurityFiling = new LicenceCheckpoint("ISF", "ImporterSecurityFiling", this, Core, LicenceModuleCategories.Codes.Customs);

			RelationshipManager = new LicenceCheckpoint("SAL", "SalesMarketing", this, Core, LicenceModuleCategories.Codes.SalesMarketing);
			RelationshipQuotations = new LicenceCheckpoint("QTE", "SalesMarketing Quotations", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			RelationshipClientRatesTariffs = new LicenceCheckpoint("CLR", "Tariffs & Rates", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			RelationshipCompanyTariffsOLD = new LicenceCheckpoint("CTF", "SalesMarketing Company Tariffs - OBSOLETE", this, RelationshipManager, LicenceModuleCategories.Codes.None)
			{
				IsObsolete = true
			};
			RelationshipCostings = new LicenceCheckpoint("COS", "SalesMarketing Costings", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			RelationshipColdCallRegister = new LicenceCheckpoint("COL", "SalesMarketing Cold Call Register", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			InquiryManager = new LicenceCheckpoint("INQ", "Inquiry Manager", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			CommissionManager = new LicenceCheckpoint("CMG", "Commission Management", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			CommunicationManager = new LicenceCheckpoint("CMM", "Communication Manager", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			RelationshipOpportunityManager = new LicenceCheckpoint("OPP", "SalesMarketing Opportunity Manager", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			RelationshipCampaignManager = new LicenceCheckpoint("CAM", "SalesMarketing Campaign Manager", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			RelationshipClientIntelligence = new LicenceCheckpoint("CCI", "SalesMarketing Client & Competitor Intelligence", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			SalesDashboard = new LicenceCheckpoint("SDB", "Sales Dashboard", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			SalesValueAnalysis = new LicenceCheckpoint("VAL", "Sales Value Analysis", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);
			PersonIntelligence = new LicenceCheckpoint("PEI", "Person Intelligence", this, RelationshipManager, LicenceModuleCategories.Codes.SalesMarketing);

			Booking = new LicenceCheckpoint("BOO", "Booking / Spot Quotes", this, Core, LicenceModuleCategories.Codes.Forwarding);
			RoutingRealTimeLookup = new LicenceCheckpoint("RSH", "eServices - Online Airline Schedules", this, Core, LicenceModuleCategories.Codes.Schedules);

			OneStopSailingScheduleFeedIntegration = new LicenceCheckpoint("VIM", "ComTracVesselIntegration", this, Core, LicenceModuleCategories.Codes.None) { IsDefaultTransactional = true };
			DakosySailingScheduleFeedIntegration = new LicenceCheckpoint("DIM", "DakosyVesselIntegration", this, Core, LicenceModuleCategories.Codes.None) { IsDefaultTransactional = true };
			DBHSailingScheduleFeedIntegration = new LicenceCheckpoint("HIM", "DBHVesselIntegration", this, Core, LicenceModuleCategories.Codes.None) { IsDefaultTransactional = true };

			OneStopAUContainerIntegration = new LicenceCheckpoint("VAU", "ComTracAUContainerFeeds", this, OneStopSailingScheduleFeedIntegration, LicenceModuleCategories.Codes.None) { IsDefaultTransactional = true };
			OneStopNZContainerIntegration = new LicenceCheckpoint("VNZ", "ComTracNZContainerFeeds", this, OneStopSailingScheduleFeedIntegration, LicenceModuleCategories.Codes.None) { IsDefaultTransactional = true };

			OrderManager = new LicenceCheckpoint("ORD", "OrderManager", this, Core, LicenceModuleCategories.Codes.OrderManager);

			DocManager = new LicenceCheckpoint("DCM", "DocManager", this, Core, LicenceModuleCategories.Codes.Docmanager);
			DocManagerScanningStation = new LicenceCheckpoint("DSS", "DocManager ScanningStation", this, DocManager, LicenceModuleCategories.Codes.Docmanager);

			LocalTransport = new LicenceCheckpoint("LOC", "Port Transport", this, Core, LicenceModuleCategories.Codes.Transport);
			LocalTransportVehicleMonitoringAndManagement = new LicenceCheckpoint("LON", "Vehicle Monitoring and Management", this, LocalTransport, LicenceModuleCategories.Codes.Transport);
			MobileTransport = new LicenceCheckpoint("EMT", "MobileTransport", this, Core, LicenceModuleCategories.Codes.Transport);
			LandTransport = new LicenceCheckpoint("LTS", "Land Transport", this, Core, LicenceModuleCategories.Codes.Transport);
			TransitWarehouse = new LicenceCheckpoint("TWH", "Transit Warehouse", this, Core, LicenceModuleCategories.Codes.TransitWarehouse);

			TransportBookings = new LicenceCheckpoint("TBK", "TransportBookings", this, Core, LicenceModuleCategories.Codes.TransportBooking);

			CFSManager = new LicenceCheckpoint("CFS", "CFSManager", this, Core, LicenceModuleCategories.Codes.Cfs);
			SeaCargoDepot = new LicenceCheckpoint("SCD", "SeaCargoCFSCustoms", this, Core, LicenceModuleCategories.Codes.Customs);
			AirCargoDepot = new LicenceCheckpoint("ACD", "AirCargoCFSCustoms", this, Core, LicenceModuleCategories.Codes.Customs);
			HVSO = new LicenceCheckpoint("HVS", "HVSO", this, Core, LicenceModuleCategories.Codes.Forwarding);
			ImportAirCTOReport = new LicenceCheckpoint("ACT", "ImportAirCTOReport", this, Core, LicenceModuleCategories.Codes.Forwarding);
			ExportAirCTOReport = new LicenceCheckpoint("EXT", "ExportAirCTOReport", this, Core, LicenceModuleCategories.Codes.Forwarding);

			WarehouseManagerCoreAnd4PL = new LicenceCheckpoint("WAR", "WarehouseManager Core & 4PL", this, Core, LicenceModuleCategories.Codes.Warehouse);
			WarehouseManagerOperationsAnd3PL = new LicenceCheckpoint("WDF", "WarehouseManager Operations & 3PL", this, WarehouseManagerCoreAnd4PL, LicenceModuleCategories.Codes.Warehouse);
			Packing = new LicenceCheckpoint("SPM", "ScanPackManager", this, WarehouseManagerCoreAnd4PL, LicenceModuleCategories.Codes.Warehouse);
			ShippingManager = new LicenceCheckpoint("SHM", "ShippingManager", this, Core, LicenceModuleCategories.Codes.Shipping);
			ShippingManagerBillOfLading = new LicenceCheckpoint("SMD", "ShippingManager Bill of Lading", this, ShippingManager, LicenceModuleCategories.Codes.Shipping);
			ShippingManagerPortAuthorityMessaging = new LicenceCheckpoint("SPA", "ShippingManager Port Authority Messaging", this, ShippingManagerBillOfLading, LicenceModuleCategories.Codes.Shipping)
			{
				IsObsolete = true
			};
			ShippingManagerPortAuthorityMessagingPerTransaction = new LicenceCheckpoint("SPT", "ShippingManager Port Authority Messaging (Per Transaction)", this, ShippingManagerBillOfLading, LicenceModuleCategories.Codes.Shipping) { IsDefaultTransactional = true };
			ShippingManagerEIDOMessaging = new LicenceCheckpoint("SDO", "ShippingManager E-IDO Messaging", this, ShippingManagerBillOfLading, LicenceModuleCategories.Codes.Shipping)
			{
				IsObsolete = true
			};
			ShippingManagerEIDOMessagingPerTransaction = new LicenceCheckpoint("SDT", "ShippingManager E-IDO Messaging (Per Transaction)", this, ShippingManagerBillOfLading, LicenceModuleCategories.Codes.Shipping) { IsDefaultTransactional = true };
			ShippingManagerContainerControl = new LicenceCheckpoint("SMC", "ShippingManager ContainerControl", this, ShippingManager, LicenceModuleCategories.Codes.Shipping);
			ShippingManagerContainerDetention = new LicenceCheckpoint("SME", "ShippingManager Container Detention", this, ShippingManager, LicenceModuleCategories.Codes.Shipping);
			ShippingManagerVoyageAccounting = new LicenceCheckpoint("SVA", "ShippingManager Voyage Accounting", this, ShippingManager, LicenceModuleCategories.Codes.Shipping);
			ShippingManagerSundryCharges = new LicenceCheckpoint("SSC", "ShippingManager Sundry Charges", this, ShippingManager, LicenceModuleCategories.Codes.Shipping);
			ShippingManagerBookings = new LicenceCheckpoint("SMB", "ShippingManager Bookings", this, ShippingManager, LicenceModuleCategories.Codes.Shipping);
			ShippingManagerCMRExportDeclaration = new LicenceCheckpoint("SED", "ShippingManager CustomsExportManifest", this, ShippingManager, LicenceModuleCategories.Codes.Shipping);
			ShippingManagerCMRImportDeclaration = new LicenceCheckpoint("SID", "ShippingManager CustomsImportManifest", this, ShippingManager, LicenceModuleCategories.Codes.Shipping);

			WebTracker = new LicenceCheckpoint("WEB", "WebTracker", this, Core, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerForwarding = new LicenceCheckpoint("WFO", "WebTracker Forwarding", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerImportBrokerage = new LicenceCheckpoint("WBI", "WebTracker Import Brokerage", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerExportBrokerage = new LicenceCheckpoint("WBE", "WebTracker Export Brokerage", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerBooking = new LicenceCheckpoint("WBO", "WebTracker Booking / Spot Quotes", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerOrderManager = new LicenceCheckpoint("WOR", "WebTracker Order Manager", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerWarehouse = new LicenceCheckpoint("WWA", "WebTracker Warehouse", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerShippingManagerBookings = new LicenceCheckpoint("WSB", "WebTracker Shipping Bookings", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerShippingManagerBillsOfLading = new LicenceCheckpoint("WSL", "WebTracker Shipping Bills of Lading", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerCFS = new LicenceCheckpoint("WCF", "WebTracker CFS Manager", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);
			WebTrackerLocalTransport = new LicenceCheckpoint("WLO", "WebTracker Local Transport", this, WebTracker, LicenceModuleCategories.Codes.WebTracker);

			ContainerManager = new LicenceCheckpoint("CON", "ContainerManager", this, Core, LicenceModuleCategories.Codes.None)
			{
				IsObsolete = true
			};
			BarCodeLabels = new LicenceCheckpoint("BCL", "BarCodeLabels", this, Core, LicenceModuleCategories.Codes.None)
			{
				IsObsolete = true
			};
			EzycargoInterface = new LicenceCheckpoint("ECI", "Cargo 2000 Phase 1", this, Core, LicenceModuleCategories.Codes.Forwarding);
			EzycargoInterfacePhase2 = new LicenceCheckpoint("EC2", "Cargo 2000 Phase 2", this, EzycargoInterface, LicenceModuleCategories.Codes.Forwarding);
			ChequeWriter = new LicenceCheckpoint("CQW", "ChequeWriter", this, Core, LicenceModuleCategories.Codes.Accounting);
			FaxEngine = new LicenceCheckpoint("FAX", "FaxEngine", this, Core, LicenceModuleCategories.Codes.Core);
			InterfaceConnector = new LicenceCheckpoint("IFC", "InterfaceConnector", this, Core, LicenceModuleCategories.Codes.None, true);
			DataWizard = new LicenceCheckpoint("DWZ", "DataWizard", this, Core, LicenceModuleCategories.Codes.None) { IsDefaultTransactional = true };
			NativeXMLConnector = new LicenceCheckpoint("NXC", "NativeXMLConnector", this, Core, LicenceModuleCategories.Codes.None, true) { IsDefaultTransactional = true };
			eAdaptorSDK_OBSOLETE = new LicenceCheckpoint("EAS", "eAdaptor SDK - OBSOLETE", this, Core, LicenceModuleCategories.Codes.None, true) { IsDefaultTransactional = true };
			eAdaptor = new LicenceCheckpoint("EAC", "eAdaptor", this, Core, LicenceModuleCategories.Codes.None, true) { IsDefaultTransactional = true };
			FormBuilder = new LicenceCheckpoint("FMB", "FormBuilder", this, Core, LicenceModuleCategories.Codes.None);

			Recruiter = new LicenceCheckpoint("REC", "Recruiter", this, Core, LicenceModuleCategories.Codes.Miscellaneous);
			HumanResourcesCampaignManager = new LicenceCheckpoint("HRC", "Human Resources Campaign Manager", this, Recruiter, LicenceModuleCategories.Codes.Miscellaneous);
			LearningAndDevelopment = new LicenceCheckpoint("LAD", "Learning & Development", this, Core, LicenceModuleCategories.Codes.Miscellaneous);

			ediTariff = new LicenceCheckpoint("TAR", "ediTariff", this, Core, LicenceModuleCategories.Codes.Miscellaneous);
			TariffBulkUpdater = new LicenceCheckpoint("TBU", "Tariff Bulk Updater", this, Core, LicenceModuleCategories.Codes.Miscellaneous);
			RFScannerManager = new LicenceCheckpoint("RFM", "RFScannerManager", this, Core, LicenceModuleCategories.Codes.Warehouse);

			LanguagePacks = new LanguageLicenceCheckpoint("LNG", "Language Packs", this, Core);
			LanguagePackLookup = new Dictionary<string, LanguageLicenceCheckpoint>();
			LanguagePackLookup[Constants.Languages.ChineseSimplified] = new LanguageLicenceCheckpoint("GZH", "Simplified Chinese Language Pack", this, LanguagePacks);
			LanguagePackLookup[Constants.Languages.ChineseTraditional] = new LanguageLicenceCheckpoint("GZT", "Traditional Chinese Language Pack", this, LanguagePacks);
			LanguagePackLookup[Constants.Languages.Portuguese] = new LanguageLicenceCheckpoint("GPT", "Portuguese Language Pack", this, LanguagePacks);
			LanguagePackLookup[Constants.Languages.PortugueseBrazil] = new LanguageLicenceCheckpoint("GPB", "Portuguese - Brazil Language Pack", this, LanguagePacks);
			LanguagePackLookup[Constants.Languages.Spanish] = new LanguageLicenceCheckpoint("GES", "Spanish Language Pack", this, LanguagePacks);
			LanguagePackLookup[Constants.Languages.SpanishLatin] = new LanguageLicenceCheckpoint("GEA", "Spanish Latin Language Pack", this, LanguagePacks);
			var allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType();
			foreach (var language in DataFile.GetAvailableLanguages())
			{
				if (!LanguagePackLookup.ContainsKey(language) && !Res.IsEnglish(language) && allLanguages.ContainsKey(language))
				{
					LanguagePackLookup[language] = new LanguageLicenceCheckpoint("G" + language.Split('-')[0].ToUpperInvariant(), allLanguages[language].GetUnresolvedString() + " Language Pack", this, LanguagePacks);
				}
			}

			LocalLanguages = new LanguageLicenceCheckpoint("LAN", "Local Languages", this, Core);

			TranslationFeedback = new LanguageLicenceCheckpoint("TRF", "Translation Feedback", this, Core);

			RemoteDesktopServices = new LicenceCheckpoint("RDC", "Remote Desktop Connector", this, Core, LicenceModuleCategories.Codes.Core);

			UniversalCopy = new LicenceCheckpoint("UCP", "Universal Copy", this, Core, LicenceModuleCategories.Codes.Core);

			GatewayBilling = new LicenceCheckpoint("CGB", "Gateway Billing", this, Accountant, LicenceModuleCategories.Codes.Accounting);
			GLConsolidations = new LicenceCheckpoint("AGC", "GL Consolidations", this, Accountant, LicenceModuleCategories.Codes.Accounting);

			GateManager = new LicenceCheckpoint("CGM", "Gate Manager", this, Core, LicenceModuleCategories.Codes.ContainerYard);
			ContainerYard = new LicenceCheckpoint("CYD", "Container Yard", this, Core, LicenceModuleCategories.Codes.ContainerYard);

			ContainerYardStorageJob = new LicenceCheckpoint("CYS", "Container Yard Storage Job", this, ContainerYard, LicenceModuleCategories.Codes.ContainerYard);
			ContainerYardGateInJob = new LicenceCheckpoint("CYI", "Container Yard Gate In Job", this, ContainerYard, LicenceModuleCategories.Codes.ContainerYard);
			ContainerYardGateOutJob = new LicenceCheckpoint("CYO", "Container Yard Gate Out Job", this, ContainerYard, LicenceModuleCategories.Codes.ContainerYard);

			#endregion
		}

		#endregion

		#region Check Points

		public readonly LicenceCheckpoint AlwaysAllow;

		public readonly LicenceCheckpoint Core;
		public readonly LicenceCheckpoint Workflow;
		public readonly LicenceCheckpoint BufferManagement;
		public readonly LicenceCheckpoint ProductivityTools;
		public readonly LicenceCheckpoint BusinessIntelligence;
		public readonly LicenceCheckpoint Accountant;
		public readonly LicenceCheckpoint FaxEngine;
		public readonly LicenceCheckpoint FormBuilder;
		public readonly LicenceCheckpoint MasterDataManagement;

		public readonly LicenceCheckpoint RelationshipManager;
		public readonly LicenceCheckpoint RelationshipQuotations;
		public readonly LicenceCheckpoint RelationshipClientRatesTariffs;
		public readonly LicenceCheckpoint RelationshipCostings;
		public readonly LicenceCheckpoint RelationshipCompanyTariffsOLD;
		public readonly LicenceCheckpoint RelationshipColdCallRegister;
		public readonly LicenceCheckpoint InquiryManager;
		public readonly LicenceCheckpoint CommissionManager;
		public readonly LicenceCheckpoint CommunicationManager;
		public readonly LicenceCheckpoint RelationshipOpportunityManager;
		public readonly LicenceCheckpoint RelationshipCampaignManager;
		public readonly LicenceCheckpoint HumanResourcesCampaignManager;
		public readonly LicenceCheckpoint RelationshipClientIntelligence;
		public readonly LicenceCheckpoint SalesDashboard;
		public readonly LicenceCheckpoint SalesValueAnalysis;
		public readonly LicenceCheckpoint PersonIntelligence;

		public readonly LicenceCheckpoint Booking;
		public readonly LicenceCheckpoint RoutingRealTimeLookup;

		public readonly LicenceCheckpoint OneStopAUContainerIntegration;
		public readonly LicenceCheckpoint OneStopNZContainerIntegration;

		public readonly LicenceCheckpoint OneStopSailingScheduleFeedIntegration;
		public readonly LicenceCheckpoint DakosySailingScheduleFeedIntegration;
		public readonly LicenceCheckpoint DBHSailingScheduleFeedIntegration;

		public readonly LicenceCheckpoint OrderManager;

		public readonly LicenceCheckpoint Forwarder;
		public readonly LicenceCheckpoint ForwarderContainersPacking;
		public readonly LicenceCheckpoint Manifest;
		public readonly LicenceCheckpoint ImportManifest;
		public readonly LicenceCheckpoint AirCargoReport;
		public readonly LicenceCheckpoint SeaCargoReport;
		public readonly LicenceCheckpoint ImportAirCargo;
		public readonly LicenceCheckpoint ImportSeaCargo;
		public readonly LicenceCheckpoint ACIReporting;
		public readonly LicenceCheckpoint ACIReportingPerTransaction;
		public readonly LicenceCheckpoint ACIeManifestReportingPerTransaction;
		public readonly LicenceCheckpoint ImportAirCTOReport;
		public readonly LicenceCheckpoint ExportAirCTOReport;
		public readonly LicenceCheckpoint ExportManifest;
		public readonly LicenceCheckpoint ExportDocuments;
		public readonly LicenceCheckpoint AirCcsukBase;
		/// <summary>
		/// Agent licence
		/// </summary>
		public readonly LicenceCheckpoint AirCcsuk;
		public readonly LicenceCheckpoint AirCcsukShed;
		public readonly LicenceCheckpoint AirCcsukDEP;
		public readonly LicenceCheckpoint NCTS;
		public readonly LicenceCheckpoint AMSReporting;
		public readonly LicenceCheckpoint StowPlanReporting;
		public readonly LicenceCheckpoint CMDReporting;
		public readonly LicenceCheckpoint EMCS;
		public readonly LicenceCheckpoint Intrastat;
		public readonly LicenceCheckpoint USeManifest;
		public readonly LicenceCheckpoint HVLVClearance;
		public readonly LicenceCheckpoint eCommerce;

		public readonly LicenceCheckpoint PRAMessaging;
		public readonly LicenceCheckpoint PRAMessagingPerTransaction;

		public readonly LicenceCheckpoint Broker;
		public readonly LicenceCheckpoint CoreCustomsModule;
		public readonly LicenceCheckpoint ImportBroker;
		public readonly LicenceCheckpoint BondedWarehouse;
		public readonly LicenceCheckpoint ImportQuarantineMessaging;
		public readonly LicenceCheckpoint Drawback;
		public readonly LicenceCheckpoint LandedCosting;
		public readonly LicenceCheckpoint ExportBroker;
		public readonly LicenceCheckpoint ImporterSecurityFiling;
		public readonly LicenceCheckpoint ReleaseNotificationSystem;

		public readonly LicenceCheckpoint TransportBookings;
		public readonly LicenceCheckpoint LocalTransport;
		public readonly LicenceCheckpoint LocalTransportVehicleMonitoringAndManagement;
		public readonly LicenceCheckpoint MobileTransport;
		public readonly LicenceCheckpoint LandTransport;
		public readonly LicenceCheckpoint TransitWarehouse;

		public readonly LicenceCheckpoint ShippingManager;
		public readonly LicenceCheckpoint ShippingManagerPortAuthorityMessaging;
		public readonly LicenceCheckpoint ShippingManagerPortAuthorityMessagingPerTransaction;
		public readonly LicenceCheckpoint ShippingManagerEIDOMessaging;
		public readonly LicenceCheckpoint ShippingManagerEIDOMessagingPerTransaction;
		public readonly LicenceCheckpoint ShippingManagerBillOfLading;
		public readonly LicenceCheckpoint ShippingManagerContainerDetention;
		public readonly LicenceCheckpoint ShippingManagerContainerControl;
		public readonly LicenceCheckpoint ShippingManagerVoyageAccounting;
		public readonly LicenceCheckpoint ShippingManagerSundryCharges;
		public readonly LicenceCheckpoint ShippingManagerCMRExportDeclaration;
		public readonly LicenceCheckpoint ShippingManagerCMRImportDeclaration;
		public readonly LicenceCheckpoint ShippingManagerBookings;

		public readonly LicenceCheckpoint ContainerManager;

		public readonly LicenceCheckpoint HVSO;

		public readonly LicenceCheckpoint CFSManager;
		public readonly LicenceCheckpoint SeaCargoDepot;

		public readonly LicenceCheckpoint DocManager;
		public readonly LicenceCheckpoint DocManagerScanningStation;

		public readonly LicenceCheckpoint WarehouseManagerCoreAnd4PL;
		public readonly LicenceCheckpoint WarehouseManagerOperationsAnd3PL;
		public readonly LicenceCheckpoint Packing;

		public readonly LicenceCheckpoint BarCodeLabels;
		public readonly LicenceCheckpoint EzycargoInterface;
		public readonly LicenceCheckpoint EzycargoInterfacePhase2;
		public readonly LicenceCheckpoint AirCargoDepot;
		public readonly LicenceCheckpoint ChequeWriter;
		public readonly LicenceCheckpoint InterfaceConnector;
		public readonly LicenceCheckpoint DataWizard;
		public readonly LicenceCheckpoint NativeXMLConnector;

		public readonly LicenceCheckpoint eAdaptorSDK_OBSOLETE;
		public readonly LicenceCheckpoint eAdaptor;

		public readonly LicenceCheckpoint DocumentCustomisation;
		public readonly LicenceCheckpoint ReportWriter;
		public readonly LicenceCheckpoint ArchiveManager;

		public readonly LicenceCheckpoint WebTracker;
		public readonly LicenceCheckpoint WebTrackerForwarding;
		public readonly LicenceCheckpoint WebTrackerImportBrokerage;
		public readonly LicenceCheckpoint WebTrackerExportBrokerage;
		public readonly LicenceCheckpoint WebTrackerBooking;
		public readonly LicenceCheckpoint WebTrackerOrderManager;
		public readonly LicenceCheckpoint WebTrackerWarehouse;
		public readonly LicenceCheckpoint WebTrackerShippingManagerBookings;
		public readonly LicenceCheckpoint WebTrackerShippingManagerBillsOfLading;

		public readonly LicenceCheckpoint WebTrackerCFS;
		public readonly LicenceCheckpoint WebTrackerLocalTransport;

		public readonly LicenceCheckpoint Recruiter;
		public readonly LicenceCheckpoint LearningAndDevelopment;

		public readonly LicenceCheckpoint ediTariff;

		public readonly LicenceCheckpoint TariffBulkUpdater;

		public readonly LicenceCheckpoint RFScannerManager;

		public readonly LicenceCheckpoint DeniedPartyScreening;

		public readonly LicenceCheckpoint LanguagePacks;
		public readonly Dictionary<string, LanguageLicenceCheckpoint> LanguagePackLookup;

		public readonly LicenceCheckpoint LocalLanguages;

		public readonly LicenceCheckpoint TranslationFeedback;

		public readonly LicenceCheckpoint RemoteDesktopServices;

		public readonly LicenceCheckpoint UniversalCopy;

		public readonly LicenceCheckpoint GatewayBilling;

		public readonly LicenceCheckpoint GLConsolidations;

		public readonly LicenceCheckpoint GateManager;
		public readonly LicenceCheckpoint ContainerYard;

		public readonly LicenceCheckpoint ContainerYardStorageJob;
		public readonly LicenceCheckpoint ContainerYardGateInJob;
		public readonly LicenceCheckpoint ContainerYardGateOutJob;

		#endregion

		#region Properties

		internal readonly IUser CurrentUser;
		internal readonly IBranch CurrentBranch;

		internal string DatabaseType
		{
			get { return databaseType ?? (databaseType = ObjectFactory.Get<IProductRegistration>().Key.DatabaseType); }
#if DEBUG
			set { databaseType = value; }
#endif
		}
		string databaseType;

		#endregion

		#region Addition / Retrieval

		internal void Add(LicenceCheckpoint checkpoint)
		{
			Checkpoints.Add(checkpoint.Name, checkpoint);
			CheckpointOrder.Add(checkpoint);
		}

		public LicenceCheckpoint[] GetAllCheckpoints()
		{
			return CheckpointOrder.ToArray();
		}

		public IEnumerable<LicenceCheckpoint> GetAllModuleCheckpoints()
		{
			return CheckpointOrder.Where(checkpoint => !(checkpoint is LanguageLicenceChildCheckpoint));
		}

		public LicenceCheckpoint GetCheckpointFromCode(string code)
		{
			LicenceCheckpoint result = null;
			Checkpoints.TryGetValue(code, out result);
			return result;
		}

		readonly Dictionary<string, LicenceCheckpoint> Checkpoints = new Dictionary<string, LicenceCheckpoint>();
		readonly List<LicenceCheckpoint> CheckpointOrder = new List<LicenceCheckpoint>();

		#endregion

		#region Licence Key Generation

#if DEBUG
		internal override void AddToLicenceNode(XmlNode licenceKeyNode)
		{
		}
#endif

		internal override void LoadFromLicenceNode(XmlNode licenceKeyNode)
		{
		}

		#endregion

		#region ILicenceProxy Members

		ILicenceCheckpoint ILicenceProxy.AlwaysAllow
		{
			get { return AlwaysAllow; }
		}

		ILicenceCheckpoint ILicenceProxy.Booking
		{
			get { return Booking; }
		}

		ILicenceCheckpoint ILicenceProxy.ContainerManager
		{
			get { return ContainerManager; }
		}

		ILicenceCheckpoint ILicenceProxy.Core
		{
			get { return Core; }
		}

		int ILicenceProxy.DefaultLicenceGracePeriodInDays
		{
			get { return DefaultLicenceGracePeriodInDays; }
		}

		ILicenceCheckpoint ILicenceProxy.DocManager
		{
			get { return DocManager; }
		}

		ILicenceCheckpoint ILicenceProxy.FaxEngine
		{
			get { return FaxEngine; }
		}

		ILicenceCheckpoint ILicenceProxy.InterfaceConnector
		{
			get { return InterfaceConnector; }
		}

		ILicenceCheckpoint ILicenceProxy.DataWizard
		{
			get { return DataWizard; }
		}

		ILicenceCheckpoint ILicenceProxy.ShippingManager
		{
			get { return ShippingManager; }
		}

		ILicenceCheckpoint ILicenceProxy.TranslationFeedback
		{
			get { return TranslationFeedback; }
		}

		ILicenceCheckpoint ILicenceProxy.Packing
		{
			get { return Packing; }
		}

		ILicenceCheckpoint ILicenceProxy.UniversalCopy
		{
			get { return UniversalCopy; }
		}

		ILicenceCheckpoint ILicenceProxy.GetCheckpointFromCode(string code)
		{
			return GetCheckpointFromCode(code);
		}

		ILicenceCheckpoint[] ILicenceProxy.GetAllCheckpoints()
		{
			return GetAllCheckpoints();
		}

		#endregion

		public const int DefaultLicenceGracePeriodInDays = 60;
		public const int DefaultNumberOfDaysBeforeUpdateSystemKey = 30;

		public void LogVerboseIfApplicable(string text)
		{
			LogVerboseIfApplicable(LogType.Information, text);
		}

		public void LogVerboseIfApplicable(LogType type, string text)
		{
			if (EnvProxy.Instance.LoginController != null && !string.IsNullOrEmpty(EnvProxy.Instance.LoginController.VerboseLoginFilename) && VerboseLogger.CanLog)
			{
				VerboseLogger.Log(type, text);
			}
		}

		VerboseLogger VerboseLogger
		{
			get { return verboseLogger ?? (verboseLogger = new VerboseLogger(EnvProxy.Instance.LoginController.VerboseLoginFilename)); }
		}

		VerboseLogger verboseLogger;

		public IDisposable UseLanguageLicense(string language, LanguageUsageType usageType)
		{
			return UseLanguageLicense(ref language, usageType);
		}

		public IDisposable UseLanguageLicense(ref string language, LanguageUsageType usageType)
		{
			if (Res.IsEnglish(language))
			{
				return new DisposableAction(delegate
				{ });
			}
			else
			{
				LanguageLicenceCheckpoint languagePackLicence;
				LanguageLicenceChildCheckpoint childCheckpoint = null;
				LanguagePackLookup.TryGetValue(language, out languagePackLicence);

				if (languagePackLicence == null)
				{
					var baseSystemLanguage = LanguageHelper.GetBaseSystemLanguage(language);
					return UseLanguageLicense(baseSystemLanguage, usageType);
				}

				if (languagePackLicence != null)
				{
					switch (usageType)
					{
						case LanguageUsageType.DocBuilder:
							childCheckpoint = languagePackLicence.DocBuilderLanguageCheckpoint;
							break;
						case LanguageUsageType.GUI:
							childCheckpoint = languagePackLicence.GUILanguageCheckpoint;
							break;
						case LanguageUsageType.WebTracker:
							childCheckpoint = languagePackLicence.WebTrackerLanguageCheckpoint;
							break;
					}
				}
				if (childCheckpoint != null && childCheckpoint.Login(LanguageLicencedComponent.Instance) != LicenceLoginResponse.Denied)
				{
					return new DisposableAction(delegate
					{ childCheckpoint.Logout(LanguageLicencedComponent.Instance); });
				}
				else
				{
					language = Res.DefaultLanguage;
					return new DisposableAction(delegate
					{ });
				}
			}
		}

		Guid ILicenceCheckpointUserContext.BranchPk
		{
			get { return CurrentBranch != null ? CurrentBranch.PK : Guid.Empty; }
		}

		string ILicenceCheckpointUserContext.UserInitials
		{
			get { return CurrentUser != null ? CurrentUser.Initials : string.Empty; }
		}

		public ICompany CurrentCompany { get; }
	}
}
