using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class LegacyLicence : LicenceSegment
	{
		public static class Codes
		{
			public const string Core = "COR";
			public const string WebTracker = "WEB";
			public const string WebTrackerForwarding = "WFO";
			public const string WebTrackerImportBrokerage = "WBI";
			public const string WebTrackerExportBrokerage = "WBE";
			public const string WebTrackerBooking = "WBO";
			public const string WebTrackerOrderManager = "WOR";
			public const string WebTrackerWarehouse = "WWA";
			public const string WebTrackerShippingManagerBookings = "WSB";
			public const string WebTrackerShippingManagerBillsOfLading = "WSL";
			public const string WebTrackerCFS = "WCF";
			public const string WebTrackerLocalTransport = "WLO";
		}

		public static LegacyLicence Instance
		{
			get { return SingletonConstructor.Instance; }
		}

		static class SingletonConstructor
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Static fields in this class do not need to be thread-static")]
			internal static LegacyLicence Instance = new LegacyLicence();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public LegacyLicence()
		{
			company = new LicenceCompanyDetails(null);
			branches = new LicenceBranchList();
			installationDetails = new LicenceInstallationDetails();

			IsLicenceKeyValid = true;

			new LegacyLicenceCheckpoint("COR", "Core", this, null);

			new LegacyLicenceCheckpoint("ACC", "Accountant", this, "COR");
			new LegacyLicenceCheckpoint("PRO", "WorkflowManager", this, "COR");
			new LegacyLicenceCheckpoint("BMS", "BufferManagement", this, "COR");
			new LegacyLicenceCheckpoint("PRD", "ProductivityTools", this, "COR");
			new LegacyLicenceCheckpoint("REP", "ReportWriter", this, "COR");
			new LegacyLicenceCheckpoint("ARM", "ArchiveManager", this, "COR");

			new LegacyLicenceCheckpoint("FOR", "Forwarder", this, "COR");
			new LegacyLicenceCheckpoint("PRA", "PRA Messaging", this, "FOR");
			new LegacyLicenceCheckpoint("PRT", "PRA Messaging (Per Transaction)", this, "FOR") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("EXD", "ExportDocuments", this, "CCM");
			new LegacyLicenceCheckpoint("DPS", "eServices - Denied Party Screening", this, "FOR");
			new LegacyLicenceCheckpoint("FCP", "Forwarder - Containers Packing", this, "FOR");
			new LegacyLicenceCheckpoint("FPM", "Forwarder - Port Messaging", this, "FOR");
			new LegacyLicenceCheckpoint("MFT", "Manifest", this, "FOR");
			new LegacyLicenceCheckpoint("EMF", "ExportManifest (CRN)", this, "MFT");
			new LegacyLicenceCheckpoint("IMF", "ImportManifest (ACA F, SCA F)", this, "MFT");
			new LegacyLicenceCheckpoint("ACR", "AirCargoReport", this, "IMF");
			new LegacyLicenceCheckpoint("SCR", "SeaCargoReport", this, "IMF");

			new LegacyLicenceCheckpoint("IAC", "ImportAirCargo", this, "MFT");
			new LegacyLicenceCheckpoint("ISC", "ImportSeaCargo", this, "MFT");

			new LegacyLicenceCheckpoint("ACI", "ACIReporting - OBSOLETE", this, "MFT");
			new LegacyLicenceCheckpoint("ACP", "ACIReporting (Per Transaction)", this, "MFT") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("ACE", "ACIeManifestReporting (Per Transaction)", this, "MFT") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("UKC", "AirCcsukBase", this, "IMF");
			new LegacyLicenceCheckpoint("UKA", "AirCcsuk", this, "UKC");
			new LegacyLicenceCheckpoint("UKS", "AirCcsukShed", this, "UKC", isManuallyEnabled: true);
			new LegacyLicenceCheckpoint("UKD", "AirCcsukDEP", this, "UKC", isManuallyEnabled: true);
			new LegacyLicenceCheckpoint("NCT", "NCTS", this, "BRK");
			new LegacyLicenceCheckpoint("AMS", "AMSReporting", this, "MFT") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("STW", "StowPlanReporting", this, "MFT") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("CMD", "CMDReporting", this, "MFT") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("MAN", "Manifest (US e-Manifest)", this, "MFT") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("HLC", "HVLVClearance", this, "MFT", isManuallyEnabled: true);

			new LegacyLicenceCheckpoint("BRK", "Broker", this, "COR");
			new LegacyLicenceCheckpoint("CCM", "CoreCustomsModule", this, "BRK");
			new LegacyLicenceCheckpoint("XBR", "ExportBroker", this, "BRK");
			new LegacyLicenceCheckpoint("IBR", "ImportBroker", this, "CCM");
			new LegacyLicenceCheckpoint("LDC", "LandedCosting", this, "CCM");
			new LegacyLicenceCheckpoint("BDS", "BondedWarehouse", this, "CCM");
			new LegacyLicenceCheckpoint("DRB", "Drawback", this, "CCM");
			new LegacyLicenceCheckpoint("IQM", "ImportQuarantine (eBACCa)", this, "IBR") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("RNS", "ReleaseNotificationSystem", this, "COR");

			new LegacyLicenceCheckpoint("ISF", "ImporterSecurityFiling", this, "COR");

			new LegacyLicenceCheckpoint("SAL", "SalesMarketing", this, "COR");
			new LegacyLicenceCheckpoint("QTE", "SalesMarketing Quotations", this, "SAL");
			new LegacyLicenceCheckpoint("CLR", "SalesMarketing Client Rates & Comp Tariffs", this, "SAL");
			new LegacyLicenceCheckpoint("CTF", "SalesMarketing Company Tariffs - OBSOLETE", this, "SAL");
			new LegacyLicenceCheckpoint("COS", "SalesMarketing Costings", this, "SAL");
			new LegacyLicenceCheckpoint("COL", "SalesMarketing Cold Call Register", this, "SAL");
			new LegacyLicenceCheckpoint("INQ", "Inquiry Manager", this, "SAL");
			new LegacyLicenceCheckpoint("CMG", "Commission Management", this, "SAL");
			new LegacyLicenceCheckpoint("CMM", "Communication Manager", this, "SAL");
			new LegacyLicenceCheckpoint("OPP", "SalesMarketing Opportunity Manager", this, "SAL");
			new LegacyLicenceCheckpoint("CAM", "SalesMarketing Campaign Manager", this, "SAL");
			new LegacyLicenceCheckpoint("CCI", "SalesMarketing Client & Competitor Intelligence", this, "SAL");
			new LegacyLicenceCheckpoint("SDB", "Sales Dashboard", this, "SAL");

			new LegacyLicenceCheckpoint("SOP", "SalesMarketing Operations Management", this, "SAL");
			new LegacyLicenceCheckpoint("SOF", "SalesMarketing Operations Management - Forwarder", this, "SOP");
			new LegacyLicenceCheckpoint("SOQ", "SalesMarketing Operations Management - Booking / Spot Quotes", this, "SOP");
			new LegacyLicenceCheckpoint("SOO", "SalesMarketing Operations Management - Order Manager", this, "SOP");
			new LegacyLicenceCheckpoint("SOT", "SalesMarketing Operations Management - Local Transport", this, "SOP");
			new LegacyLicenceCheckpoint("SOB", "SalesMarketing Operations Management - Broker", this, "SOP");

			new LegacyLicenceCheckpoint("BOO", "Booking / Spot Quotes", this, "COR");
			new LegacyLicenceCheckpoint("RSH", "eServices - Online Airline Schedules", this, "COR");

			new LegacyLicenceCheckpoint("VIM", "ComTracVesselIntegration", this, "COR", isManuallyEnabled: true) { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("DIM", "DakosyVesselIntegration", this, "COR", isManuallyEnabled: true) { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("HIM", "DBHVesselIntegration", this, "COR", isManuallyEnabled: true) { IsDefaultTransactional = true };

			new LegacyLicenceCheckpoint("VAU", "ComTracAUContainerFeeds", this, "VIM", isManuallyEnabled: true) { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("VNZ", "ComTracNZContainerFeeds", this, "VIM", isManuallyEnabled: true) { IsDefaultTransactional = true };

			new LegacyLicenceCheckpoint("ORD", "OrderManager", this, "COR");

			new LegacyLicenceCheckpoint("DCM", "DocManager", this, "COR");
			new LegacyLicenceCheckpoint("DSS", "DocManager ScanningStation", this, "DCM");

			new LegacyLicenceCheckpoint("LOC", "Port Transport", this, "COR");
			new LegacyLicenceCheckpoint("LON", "Vehicle Monitoring and Management", this, "LOC");
			new LegacyLicenceCheckpoint("EMT", "MobileTransport", this, "COR");

			new LegacyLicenceCheckpoint("TBK", "TransportBookings", this, "COR");

			new LegacyLicenceCheckpoint("CFS", "CFSManager", this, "COR");
			new LegacyLicenceCheckpoint("SCD", "SeaCargoCFSCustoms", this, "COR");
			new LegacyLicenceCheckpoint("ACD", "AirCargoCFSCustoms", this, "COR");
			new LegacyLicenceCheckpoint("HVS", "HVSO", this, "COR");
			new LegacyLicenceCheckpoint("ACT", "ImportAirCTOReport", this, "COR");
			new LegacyLicenceCheckpoint("EXT", "ExportAirCTOReport", this, "COR");

			new LegacyLicenceCheckpoint("WAR", "WarehouseManager Core & 4PL", this, "COR");
			new LegacyLicenceCheckpoint("WDF", "WarehouseManager Operations & 3PL", this, "WAR");
			new LegacyLicenceCheckpoint("SPM", "ScanPackManager", this, "WAR");
			new LegacyLicenceCheckpoint("SHM", "ShippingManager", this, "COR");
			new LegacyLicenceCheckpoint("SMD", "ShippingManager Bill of Lading", this, "SHM");
			new LegacyLicenceCheckpoint("SPA", "ShippingManager Port Authority Messaging", this, "SMD");
			new LegacyLicenceCheckpoint("SPT", "ShippingManager Port Authority Messaging (Per Transaction)", this, "SMD") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("SDO", "ShippingManager E-IDO Messaging", this, "SMD");
			new LegacyLicenceCheckpoint("SDT", "ShippingManager E-IDO Messaging (Per Transaction)", this, "SMD") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("SMC", "ShippingManager ContainerControl", this, "SHM");
			new LegacyLicenceCheckpoint("SME", "ShippingManager Container Detention", this, "SHM");
			new LegacyLicenceCheckpoint("SVA", "ShippingManager Voyage Accounting", this, "SHM");
			new LegacyLicenceCheckpoint("SSC", "ShippingManager Sundry Charges", this, "SHM");
			new LegacyLicenceCheckpoint("SMB", "ShippingManager Bookings", this, "SHM");
			new LegacyLicenceCheckpoint("SED", "ShippingManager CustomsExportManifest", this, "SHM");
			new LegacyLicenceCheckpoint("SID", "ShippingManager CustomsImportManifest", this, "SHM");

			new LegacyLicenceCheckpoint(Codes.WebTracker, "WebTracker", this, "COR");
			new LegacyLicenceCheckpoint(Codes.WebTrackerForwarding, "WebTracker Forwarding", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerImportBrokerage, "WebTracker Import Brokerage", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerExportBrokerage, "WebTracker Export Brokerage", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerBooking, "WebTracker Booking / Spot Quotes", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerOrderManager, "WebTracker Order Manager", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerWarehouse, "WebTracker Warehouse", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerShippingManagerBookings, "WebTracker Shipping Bookings", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerShippingManagerBillsOfLading, "WebTracker Shipping Bills of Lading", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerCFS, "WebTracker CFS Manager", this, "WEB");
			new LegacyLicenceCheckpoint(Codes.WebTrackerLocalTransport, "WebTracker Local Transport", this, "WEB");

			new LegacyLicenceCheckpoint("CON", "ContainerManager", this, "COR");
			new LegacyLicenceCheckpoint("BCL", "BarCodeLabels", this, "COR");
			new LegacyLicenceCheckpoint("ECI", "Cargo 2000 Phase 1", this, "COR");
			new LegacyLicenceCheckpoint("EC2", "Cargo 2000 Phase 2", this, "ECI");
			new LegacyLicenceCheckpoint("CQW", "ChequeWriter", this, "COR");
			new LegacyLicenceCheckpoint("FAX", "FaxEngine", this, "COR");
			new LegacyLicenceCheckpoint("IFC", "InterfaceConnector", this, "COR", isManuallyEnabled: true) { IsWithoutUserLimit = true };
			new LegacyLicenceCheckpoint("DWZ", "DataWizard", this, "COR") { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("NXC", "NativeXMLConnector", this, "COR", isManuallyEnabled: true) { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("EAS", "eAdaptor SDK - OBSOLETE", this, "COR", isManuallyEnabled: true) { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("EAC", "eAdaptor", this, "COR", isManuallyEnabled: true) { IsDefaultTransactional = true };
			new LegacyLicenceCheckpoint("FMB", "FormBuilder", this, "COR");

			new LegacyLicenceCheckpoint("REC", "Recruiter", this, "COR");
			new LegacyLicenceCheckpoint("TAR", "ediTariff", this, "COR");
			new LegacyLicenceCheckpoint("TBU", "Tariff Bulk Updater", this, "COR");
			new LegacyLicenceCheckpoint("RFM", "RFScannerManager", this, "COR");

			new LegacyLanguageLicenceCheckpoint("LNG", "Language Packs", this, "COR");
			new LegacyLanguageLicenceCheckpoint("GAZ", "Azerbaijani Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GBN", "Bangla Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GBS", "Bosnian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GZH", "Simplified Chinese Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GZT", "Traditional Chinese Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GFR", "French Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GDE", "German Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GEA", "Spanish Latin Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GES", "Spanish Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GET", "Estonian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GHE", "Hebrew Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GHR", "Croation Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GHY", "Armenian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GIT", "Italian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GJA", "Japanese Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GLO", "Lao Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GMK", "Macedonian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GMN", "Mongolian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GMS", "Malay Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GMY", "Burmese Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GPT", "Portuguese Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GPB", "Portuguese - Brazil Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GNL", "Dutch Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GRU", "Russian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GUK", "Ukrainian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GAR", "Arabic Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GBG", "Bulgarian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GCS", "Czech Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GDA", "Danish Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GFI", "Finnish Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GEL", "Greek Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GHU", "Hungarian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GKM", "Khmer Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GKO", "Korean Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GLT", "Lithuanian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GLV", "Latvian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GNB", "Norwegian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GPL", "Polish Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GDZ", "Dzongkha Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GPS", "Pashto Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GRO", "Romanian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GSK", "Slovak Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GSL", "Slovenian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GSQ", "Albanian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GSR", "Serbian Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GSV", "Swedish Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GTH", "Thai Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GTR", "Turkish Language Pack", this, "LNG");
			new LegacyLanguageLicenceCheckpoint("GVI", "Vietnamese Language Pack", this, "LNG");

			new LegacyLanguageLicenceCheckpoint("TRF", "Translation Feedback", this, "COR");

			new LegacyLicenceCheckpoint("RDC", "Remote Desktop Connector", this, "COR");

			new LegacyLicenceCheckpoint("UCP", "Universal Copy", this, "COR");

			new LegacyLicenceCheckpoint("CGB", "Gateway Billing", this, "ACC");
			new LegacyLicenceCheckpoint("AGC", "GL Consolidations", this, "ACC");
		}

		public LegacyLicence(string encryptedLicenceKey)
			: this()
		{
			IsLicenceKeyValid = LoadFromLicenceKey(DecryptLicenceKey(encryptedLicenceKey));
		}

		public LegacyLicenceCheckpoint Forwarder { get { return GetCheckpointFromCode("FOR"); } }
		public LegacyLicenceCheckpoint Core { get { return GetCheckpointFromCode("COR"); } }
		public LegacyLicenceCheckpoint Accountant { get { return GetCheckpointFromCode("ACC"); } }
		public LegacyLicenceCheckpoint ExportManifest { get { return GetCheckpointFromCode("EMF"); } }
		public LegacyLicenceCheckpoint ReportWriter { get { return GetCheckpointFromCode("REP"); } }
		public LegacyLicenceCheckpoint ImportBroker { get { return GetCheckpointFromCode("IBR"); } }
		public LegacyLicenceCheckpoint InterfaceConnector { get { return GetCheckpointFromCode("IFC"); } }
		public LegacyLicenceCheckpoint WarehouseManagerCoreAnd4PL { get { return GetCheckpointFromCode("WAR"); } }
		public LegacyLicenceCheckpoint RelationshipManager { get { return GetCheckpointFromCode("SAL"); } }
		public LegacyLicenceCheckpoint RelationshipCompanyTariffsOLD { get { return GetCheckpointFromCode("CTF"); } }
		public LegacyLicenceCheckpoint WebTracker { get { return GetCheckpointFromCode("WEB"); } }
		public LegacyLicenceCheckpoint SeaCargoDepot { get { return GetCheckpointFromCode("SCD"); } }
		public LegacyLicenceCheckpoint SeaCargoReport { get { return GetCheckpointFromCode("SCR"); } }
		public LegacyLicenceCheckpoint HVSO { get { return GetCheckpointFromCode("HVS"); } }

		public string Type { get; set; }

		public LicenceCompanyDetails Company
		{
			get { return company; }
		}

		public LicenceInstallationDetails InstallationDetails
		{
			get { return installationDetails; }
		}

		public LicenceBranchList Branches
		{
			get { return branches; }
		}

		public string ToEncryptedKeyString()
		{
			return EncryptLicenceKey(GenerateLicenceKey());
		}

		#region Encryption / Decryption

		protected static string DecryptLicenceKey(string encryptedLicenceKey)
		{
			var licenceEncoder = new TwoWayEncoder(Enterprise.Core.Constants.LicenceConstants.EncryptionKey);
			string decryptedKey;

			try
			{
				decryptedKey = licenceEncoder.Decrypt(encryptedLicenceKey);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				decryptedKey = "";
			}

			return decryptedKey;
		}

		string EncryptLicenceKey(string licenceKey)
		{
			return new TwoWayEncoder(Enterprise.Core.Constants.LicenceConstants.EncryptionKey).Encrypt(licenceKey);
		}

		#endregion

		internal void Add(LegacyLicenceCheckpoint checkpoint)
		{
			checkpoints.Add(checkpoint.Name, checkpoint);
			checkpointOrder.Add(checkpoint);
		}

		public IReadOnlyList<LegacyLicenceCheckpoint> GetAllCheckpoints()
		{
			return checkpointOrder.AsReadOnly();
		}

		public IReadOnlyList<LegacyLicenceCheckpoint> CheckpointOrder
		{
			get { return checkpointOrder.AsReadOnly(); }
		}

		public IEnumerable<LegacyLicenceCheckpoint> GetAllModuleCheckpoints()
		{
			return checkpointOrder.Where(checkpoint => !(checkpoint is LegacyLanguageLicenceChildCheckpoint));
		}

		public LegacyLicenceCheckpoint GetCheckpointFromCode(string code)
		{
			LegacyLicenceCheckpoint result = null;
			checkpoints.TryGetValue(code, out result);
			return result;
		}

		readonly LicenceInstallationDetails installationDetails;
		readonly LicenceBranchList branches;
		readonly LicenceCompanyDetails company;
		public readonly bool IsLicenceKeyValid;
		readonly List<LegacyLicenceCheckpoint> checkpointOrder = new List<LegacyLicenceCheckpoint>();
		readonly Dictionary<string, LegacyLicenceCheckpoint> checkpoints = new Dictionary<string, LegacyLicenceCheckpoint>();

		#region Licence Key Generation

		public string GenerateLicenceKey()
		{
			XmlDocument licenceKeyXml = new XmlDocument();
			XmlNode licenceKeyNode = licenceKeyXml.CreateNode(XmlNodeType.Element, "LicenceKey", "");
			licenceKeyXml.AppendChild(licenceKeyNode);
			AddToLicenceNode(licenceKeyNode);

			XmlNode checkpointsNode = licenceKeyXml.CreateNode(XmlNodeType.Element, "CheckPoints", "");
			licenceKeyNode.AppendChild(checkpointsNode);

			foreach (var checkpoint in checkpointOrder)
			{
				checkpoint.AddToLicenceNode(checkpointsNode);
			}

			XmlNode companyNode = licenceKeyXml.CreateNode(XmlNodeType.Element, "Company", "");
			licenceKeyNode.AppendChild(companyNode);
			Company.AddToLicenceNode(companyNode);

			XmlNode branchesNode = licenceKeyXml.CreateNode(XmlNodeType.Element, "Branches", "");
			licenceKeyNode.AppendChild(branchesNode);
			Branches.AddToLicenceNode(branchesNode);

			XmlNode installationDetailsNode = licenceKeyXml.CreateNode(XmlNodeType.Element, "InstallationDetails", "");
			licenceKeyNode.AppendChild(installationDetailsNode);
			InstallationDetails.AddToLicenceNode(installationDetailsNode);

			return licenceKeyXml.OuterXml;
		}

		protected bool LoadFromLicenceKey(string licenceKey)
		{
			bool result = false;
			XmlDocument licenceKeyXml = new XmlDocument();
			if (!string.IsNullOrEmpty(licenceKey))
			{
				result = true;
				try
				{
					licenceKeyXml.LoadXml(licenceKey);

					XmlNode licenceKeyNode = licenceKeyXml.FirstChild;
					LoadFromLicenceNode(licenceKeyNode);

					XmlNode checkpointsNode = licenceKeyNode.FirstChild;
					foreach (XmlNode node in checkpointsNode.ChildNodes)
					{
						if (checkpoints.ContainsKey(node.Name))
						{
							var checkpoint = checkpoints[node.Name];
							checkpoint.LoadFromLicenceNode(node);
						}
					}

					XmlNode companyNode = checkpointsNode.NextSibling;
					Company.LoadFromLicenceNode(companyNode);

					XmlNode branchesNode = companyNode.NextSibling;
					Branches.LoadFromLicenceNode(branchesNode);

					XmlNode installationDetailsNode = branchesNode.NextSibling;
					InstallationDetails.LoadFromLicenceNode(installationDetailsNode);
				}
				catch (FormatException)
				{
					result = false;
				}
				catch (XmlException)
				{
					result = false;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void AddToLicenceNode(XmlNode licenceKeyNode)
		{
			XmlDocument document = licenceKeyNode.OwnerDocument;

			XmlAttribute typeAttribute = document.CreateAttribute("Type");
			typeAttribute.Value = Type;
			licenceKeyNode.Attributes.Append(typeAttribute);

			XmlAttribute supportModeAttribute = document.CreateAttribute("SupportMode");
			supportModeAttribute.Value = SupportMode;
			licenceKeyNode.Attributes.Append(supportModeAttribute);

			XmlAttribute hostedLocationAttribute = document.CreateAttribute("HostedLocation");
			hostedLocationAttribute.Value = ObsoleteHostedLocation;
			licenceKeyNode.Attributes.Append(hostedLocationAttribute);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void LoadFromLicenceNode(XmlNode licenceKeyNode)
		{
			this.Type = LicenceSegment.GetStringAttributeValueSafely(licenceKeyNode, "Type");
			this.SupportMode = LicenceSegment.GetStringAttributeValueSafely(licenceKeyNode, "SupportMode");
			this.ObsoleteHostedLocation = LicenceSegment.GetStringAttributeValueSafely(licenceKeyNode, "HostedLocation");
		}

		#endregion

		public string SupportMode { get; set; }
		public string ObsoleteHostedLocation { get; set; }
	}
}
