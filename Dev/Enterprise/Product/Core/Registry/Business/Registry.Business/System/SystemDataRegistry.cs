using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.S3;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public enum OrganisationImportMatchingType
	{
		Unknown,
		LegacyCodeMatching,
		OrganisationCodeMatching
	}

	public sealed partial class SystemDataRegistry : RegistryItemSet, ISystemDataRegistry
	{
		#region Instance

		SystemDataRegistry()
		{
		}

		public static SystemDataRegistry Instance
		{
			get { return fInstance ?? (fInstance = new SystemDataRegistry()); }
		}
		[ThreadStatic]
		static SystemDataRegistry fInstance;

		#endregion

		protected override IEnumerable<IRegistryItem> GetItemsNotAccessedUsingProperties()
		{
			var scriptFactory = ObjectFactory.Get<IScriptFactory>();
			var scripts = scriptFactory.CreateScripts(new BusinessObjectFactory());

			var groupedScripts = scripts.GroupBy(script => new
			{
				script.Module,
				Category = new DynamicCategory(script.Module).CategoryString
			});
			foreach (var group in groupedScripts)
			{
				foreach (var script in group)
				{
					yield return GetStlCollectorHighWaterMark(script.Code, script.Feature, group.Key.Category);
				}
			}
		}

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString System_ArchiveManager_InactiveOperationalJobsArchiveSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("7D6C791F-0B76-4A7F-8936-4271BBC27802", "Inactive Operational Jobs Archive System")); } }
			public static MultilingualString System_ArchiveManager_OperationalJobsArchiveSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("A08C3CD1-A95D-4016-8B4B-098F685C1C0E", "Operational Jobs Archive System")); } }
			public static MultilingualString System_ArchiveManager_StandaloneRecordsArchiveSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("15F08207-F565-4F90-82E1-5D10C095DB23", "Standalone Records Archive System")); } }
			public static MultilingualString System_ArchiveManager_PurgeDocumentsAndRecordsSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("10791E6F-F7B6-442E-9A3E-94AC2488285D", "Purge Documents and Records System")); } }
			public static MultilingualString System_ArchiveManager_PurgeArchivedRecordsSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("AE7B05E7-E7E8-4E77-8FA1-8586CB4D8CAE", "Purge Archived Records System")); } }
			public static MultilingualString System_ArchiveManager_HVLVArchiveSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("2131F974-761D-4AE7-9BF2-649DFEC1FE03", "HVLV Archive System")); } }
			public static MultilingualString System_ArchiveManager_PurgeDocumentsOfOperationalRecordsSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("C1B5772E-E297-4C45-A8C2-EF7CD8695A12", "Purge Documents of Operational Records System")); } }
			public static MultilingualString System_ArchiveManager_PurgeExpiredRatesSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("A33E4386-54FF-4064-A5DA-70D16842D317", "Purge Expired Rates System")); } }
			public static MultilingualString System_ArchiveManager_PurgeActivityLogsSystem { get { return CombineCategories(System_ArchiveManager, ResString.GetMultilingualString("2C5A9A95-0A0A-4414-8522-E202D1C6615E", "Purge Activity Logs System")); } }
			public static MultilingualString System_Logos { get { return CombineCategories(System, ResString.GetMultilingualString("1281792A-0631-40cc-94E2-91E3A9776FCF", "Logos")); } }
			public static MultilingualString System_DataImportSettings_Orders { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("D5B15361-4523-4337-A269-CB7B018AD319", "Orders")); } }
			public static MultilingualString System_DataImportSettings_UpdateAccountBalances { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("0290EA4D-3491-4bac-A9A1-E013BE51C360", "Update Account Balances")); } }
			public static MultilingualString System_DataImportSettings_Organizations { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("D90B611A-2F2F-41c2-B140-3D1868A31ECE", "Organizations")); } }
			public static MultilingualString System_DataImportSettings_Shipment { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("4BA90B02-EAFF-4552-80EE-F49CC3AA2927", "Shipment")); } }
			public static MultilingualString System_DataImportSettings_UnprocessedMessageFolder { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("1EE79C8F-776A-496d-8D31-715346EBAFEC", "Unprocessed Message Folder")); } }
			public static MultilingualString System_DataImportSettings_UnprocessedMessageNotifications { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("90D4EF1E-2BC5-4acf-83C7-87BB254A4460", "Unprocessed Message Notifications")); } }
			public static MultilingualString System_DataImportSettings_DefaultMessages { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("EBEEA095-3782-4856-9739-02B46DD5E287", "Default Messages")); } }
			public static MultilingualString System_DataImportSettings_Warehouse { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("33D9CF87-C61A-48e0-8F06-23322F45516A", "Warehouse")); } }
			public static MultilingualString System_DataImportSettings_Bookings { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("A327339F-7059-4b7e-AE30-7EED4ED4783A", "Bookings")); } }
			public static MultilingualString System_DataImportSettings_Consols { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("B31AAF64-AAB4-443d-BA96-939C55A61E0D", "Consols")); } }
			public static MultilingualString System_DataImportSettings_Consols_Departure { get { return CombineCategories(System_DataImportSettings_Consols, ResString.GetMultilingualString("9353084F-ACD4-456d-942D-CCB6A2624E7B", "Departure")); } }
			public static MultilingualString System_DataImportSettings_Consols_AutomaticUpdateonImport { get { return CombineCategories(System_DataImportSettings_Consols, ResString.GetMultilingualString("D098CCE4-E5A4-4a60-B9C6-732B6DF8D47E", "Automatic Update on Import")); } }
			public static MultilingualString System_DataImportSettings_CustomsDeclarations { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("3D1B7795-A92E-445d-A94A-CE265811DEB6", "Customs Declarations")); } }
			public static MultilingualString System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica { get { return CombineCategories(System_DataImportSettings_CustomsDeclarations, ResString.GetMultilingualString("65ab3318-a5a4-479f-8afa-1a9bd7378588", "United States of America")); } }
			public static MultilingualString System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings { get { return CombineCategories(System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica, ResString.GetMultilingualString("de5918fe-21b2-45fc-a162-91b56e50a38b", "BIRD FTP Settings")); } }
			public static MultilingualString System_DataImportSettings_DataImportWizard { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("120B2DA2-1924-4160-AD17-F2AE77A92279", "Data Import Wizard")); } }
			public static MultilingualString System_DataImportSettings_CommercialInvoices { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("16B42444-5D4C-4e73-A3FB-C251EC51220B", "Commercial Invoices")); } }
			public static MultilingualString System_DataImportSettings_PODs { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("EED80540-7BDA-472a-9320-F90C606A5F65", "PODs")); } }
			public static MultilingualString System_DataImportSettings_Products { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("C14C2508-6D41-4d24-B4DD-6037F909F2FD", "Products")); } }
			public static MultilingualString System_DataImportSettings_DestinationPortClearanceProcess { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("e7370868-6498-4c42-9177-b247d01550a9", "Destination Port Clearance Process")); } }
			public static MultilingualString System_DataImportSettings_DestinationPortClearanceProcess_Australia { get { return CombineCategories(System_DataImportSettings_DestinationPortClearanceProcess, ResString.GetMultilingualString("165b54c0-4f7e-442b-b0a1-58ac82de578e", "Australia")); } }
			public static MultilingualString System_DataImportSettings_DestinationPortClearanceProcess_Australia_AirCargo { get { return CombineCategories(System_DataImportSettings_DestinationPortClearanceProcess_Australia, ResString.GetMultilingualString("53b2cc35-9ef5-4d60-99b6-a77ced9ff648", "Air Cargo")); } }
			public static MultilingualString System_DataImportSettings_DestinationPortClearanceProcess_Australia_SeaCargo { get { return CombineCategories(System_DataImportSettings_DestinationPortClearanceProcess_Australia, ResString.GetMultilingualString("e66946c0-9c32-4417-94f2-07d14f32b46e", "Sea Cargo")); } }
			public static MultilingualString System_DataImportSettings_Schedules { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("580A9E84-8AD3-442b-8170-14AC6AAF3B33", "Schedules")); } }
			public static MultilingualString System_DataImportSettings_PortTransport { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("7EE5D8F9-2431-4d09-80EC-908EAF45CFF4", "Port Transport")); } }
			public static MultilingualString System_DataImportSettings_ContainerEvents { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("D476ED72-3A15-4d7a-8A8A-0BA806E2C9CB", "Container Events")); } }
			public static MultilingualString System_DataImportSettings_SystemMerge { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("B69B8EE7-7876-4d87-ABAB-F7669F4AF3CA", "System Merge")); } }
			public static MultilingualString System_DataImportSettings_Event { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("FF9E55F3-BF30-4d5d-AAF3-147F73A2C026", "Event")); } }
			public static MultilingualString System_DataImportSettings_ShippingBillofLading { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("c5aafc79-0be8-4cfe-a332-ab6142e90c58", "Shipping Bill of Lading")); } }
			public static MultilingualString System_DataImportSettings_Accounting { get { return CombineCategories(System_DataImportSettings, ResString.GetMultilingualString("ACA64A4D-B5D7-43FA-92A0-5B051ACFC4DF", "Accounting")); } }
			public static MultilingualString System_DataImportSettings_Accounting_Receivable { get { return CombineCategories(System_DataImportSettings_Accounting, ResString.GetMultilingualString("85D507F0-DBD5-4CEB-8127-49CE69D44239", "Receivable")); } }
			public static MultilingualString System_Statistics { get { return CombineCategories(System, ResString.GetMultilingualString("E2E47AA3-4F06-40A6-B6D3-B5C8C4957929", "Statistics")); } }
			public static MultilingualString System_DocManager { get { return CombineCategories(System, ResString.GetMultilingualString("BB8EC70D-C323-4780-937A-DFBDF670609E", "DocManager")); } }
			public static MultilingualString System_DocManager_S3Storage { get { return CombineCategories(System_DocManager, ResString.GetMultilingualString("920B8C8E-74E6-45A3-ABF0-B294694C108F", "S3 Storage")); } }
			public static MultilingualString System_DocManager_DocumentTracking { get { return CombineCategories(System_DocManager, ResString.GetMultilingualString("A48A90F9-B956-43F9-8379-9398B509D454", "Document Tracking")); } }
			public static MultilingualString System_ProcessController_QueueMonitoring { get { return CombineCategories(System_ProcessController, ResString.GetMultilingualString("A003066C-4F89-43D5-9EDF-C29C8DA3C0A1", "Queue Monitoring")); } }
			public static MultilingualString System_ProcessController_Logging { get { return CombineCategories(System_ProcessController, ResString.GetMultilingualString("93d3346c-a39a-4141-bf4d-4ebe7883a7c4", "Logging")); } }
			public static MultilingualString System_ProcessController_Logging_Kafka { get { return CombineCategories(System_ProcessController_Logging, ResString.GetMultilingualString("23243f5e-047f-4e39-8da6-048db94647d4", "Kafka")); } }
			public static MultilingualString System_ProcessController_Logging_ElasticSearch { get { return CombineCategories(System_ProcessController_Logging, ResString.GetMultilingualString("421cfbf9-03ba-41bd-b775-fe648e92cccd", "Elasticsearch")); } }
			public static MultilingualString System_ProcessController_Logging_Syslog { get { return CombineCategories(System_ProcessController_Logging, ResString.GetMultilingualString("C4E0719B-65ED-4B2E-80B1-9F1ABF751A52", "Syslog Logging")); } }
			public static MultilingualString System_ProcessController_Logging_CombinedFileSystem { get { return CombineCategories(System_ProcessController_Logging, ResString.GetMultilingualString("69ACFA56-575A-4B87-A403-8205B88B5978", "Combined File System Logging")); } }
			public static MultilingualString System_ProcessController_ResourceThrottling { get { return CombineCategories(System_ProcessController, ResString.GetMultilingualString("D6F4B630-070B-41c0-BD1B-7C58C1419EED", "Resource Throttling")); } }
			public static MultilingualString System_ProcessController_DispatcherThrottling { get { return CombineCategories(System_ProcessController, ResString.GetMultilingualString("E6F4B630-070B-41c0-BD1B-7C58C1419EED", "Dispatcher Throttling")); } }
			public static MultilingualString System_ProcessController_Notifications { get { return CombineCategories(System_ProcessController, ResString.GetMultilingualString("723541B3-C7EF-41A6-B1B2-6F1AEB86692E", "Notifications")); } }
			public static MultilingualString System_LogWalker { get { return CombineCategories(System, ResString.GetMultilingualString("019768BA-8E56-4ab2-978C-4F1F773B2C4C", "Log Walker")); } }
			public static MultilingualString System_LogWalker_WorkflowTriggerEventProcessor { get { return CombineCategories(System_LogWalker, ResString.GetMultilingualString("9B360703-2613-4EEC-8182-B126CF937E91", "Workflow Trigger Event Processor")); } }
			public static MultilingualString System_DataExportSettings_Organization { get { return CombineCategories(System_DataExportSettings, ResString.GetMultilingualString("F09516A0-073B-406e-9BD8-C715F2481553", "Organization")); } }
			public static MultilingualString System_DataExportSettings_InvoicesinPDFformat { get { return CombineCategories(System_DataExportSettings, ResString.GetMultilingualString("7aa4f85a-06c7-425a-afad-ba71f12ebfe8", "Invoices in PDF format")); } }
			public static MultilingualString System_DataExportSettings_ExportGLTransactionstoCSV { get { return CombineCategories(System_DataExportSettings, ResString.GetMultilingualString("1F68DEF4-B39D-45ea-8DAB-E698D793C39C", "Export GL Transactions to CSV")); } }
			public static MultilingualString System_DataExportSettings_ShippingBillofLading { get { return CombineCategories(System_DataExportSettings, ResString.GetMultilingualString("F5897D48-0E1A-4fa7-A405-6EAB9146857C", "Shipping Bill of Lading")); } }
			public static MultilingualString System_DataExportSettings_ShipmentExport { get { return CombineCategories(System_DataExportSettings, ResString.GetMultilingualString("C2FE776B-C215-4662-B37A-9D42912AD6DE", "Shipment Export")); } }
			public static MultilingualString System_DataExportSettings_WarehouseExport { get { return CombineCategories(System_DataExportSettings, ResString.GetMultilingualString("43c4fe4a-6bb3-4232-8902-36c58c677895", "Warehouse Export")); } }
			public static MultilingualString System_DataExportSettings_ExportDirectories { get { return CombineCategories(System_DataExportSettings, ResString.GetMultilingualString("f5c472b1-5336-4118-94cb-94ab1846d3ad", "Export Directories")); } }
			public static MultilingualString System_DataVersionLog { get { return CombineCategories(System, ResString.GetMultilingualString("c36c2203-91d3-4e6b-9241-4fa20d6014b4", "Data Version Log")); } }
			public static MultilingualString System_Diagnostics { get { return CombineCategories(System, ResString.GetMultilingualString("D07DB3DC-E3AE-4AD9-8BA5-E3FE3A5DD7A5", "Diagnostics")); } }

			public static MultilingualString System_Database_LogShrinkAfterUpgrade { get { return CombineCategories(System_Database, ResString.GetMultilingualString("31c892cc-45e1-404e-b0b1-e7a177781cc4", "Log Shrink After Upgrade")); } }
			public static MultilingualString System_ProductionRules { get { return CombineCategories(System, ResString.GetMultilingualString("709be05b-c2a2-4a5a-921a-0ef8287e67cd", "Production Rules")); } }
			public static MultilingualString System_Resources { get { return CombineCategories(System, ResString.GetMultilingualString("3769c691-9e31-4235-83fd-d6a6396b4c69", "Resources")); } }
			public static MultilingualString System_CustomerService { get { return CombineCategories(System, ResString.GetMultilingualString("4bb6ac66-7f47-4c4e-93ab-da4afb261d4b", "Customer Service (Legacy)")); } }
			public static MultilingualString System_Reports { get { return CombineCategories(System, ResString.GetMultilingualString("0b3f3261-db42-4809-aacd-e113986ab364", "Reports")); } }
			public static MultilingualString System_NewsAnnouncements { get { return CombineCategories(System, ResString.GetMultilingualString("4943EE44-4E53-4ec6-8531-770CB22E1AA1", "News & Announcements")); } }
			public static MultilingualString System_STL { get { return CombineCategories(System, ResString.GetMultilingualString("6C915994-50F6-4B63-A68A-918BD34D9B9D", "STL")); } }
			public static MultilingualString System_STLHighWaterMarks { get { return CombineCategories(System_STL, ResString.GetMultilingualString("753DBD8E-F243-4A92-927D-98A4E801278D", "High Water Marks")); } }
			public static MultilingualString System_BusinessIntelligence { get { return CombineCategories(System, ResString.GetMultilingualString("60AE2708-A3D3-495B-9DE0-348EFBF3541C", "BI")); } }
			public static MultilingualString System_BusinessIntelligence_CompanyBranding { get { return CombineCategories(System_BusinessIntelligence, ResString.GetMultilingualString("548BF08C-D033-4459-91ED-EB7FBDF4F5E4", "Company Branding")); } }
			public static MultilingualString System_BusinessIntelligence_Cdc { get { return CombineCategories(System_BusinessIntelligence, ResString.GetMultilingualString("E7B18B4F-F8DE-4AB6-9710-8D17EBFBE19F", "CDC")); } }
			public static MultilingualString System_EConversations { get { return CombineCategories(System, ResString.GetMultilingualString("912C4C07-75A0-4D25-9B86-844949C37B15", "eConversations")); } }
			public static MultilingualString System_WebServices { get { return CombineCategories(System, ResString.GetMultilingualString("E5FDEDA3-3883-44A0-B413-C9A2E357B7C9", "Web Services")); } }
			public static MultilingualString Customs_ASYCUDA { get { return CombineCategories(Customs, ResString.GetMultilingualString("12345678-AAE7-40AB-B4C6-65E2C58D88EF", "ASYCUDA")); } }
			public static MultilingualString MDM_Administration { get { return CombineCategories(MasterData, ResString.GetMultilingualString("c4cb03d6-d282-4fca-9f5f-30f04a8449e6", "MDM Administration")); } }
			public static MultilingualString Persons { get { return CombineCategories(MasterData, ResString.GetMultilingualString("62383F66-F1C7-421C-9E86-2853E154807C", "Persons")); } }
			public static MultilingualString Persons_Duplicate_Detection { get { return CombineCategories(Persons, ResString.GetMultilingualString("{4EFC21EB-3D56-456F-8D5D-D88EFEC90557}", "Duplicate Detection")); } }
			public static MultilingualString Persons_PersonMerge { get { return CombineCategories(Persons, ResString.GetMultilingualString("{5EFC21EB-3D56-456F-1D5D-D88EFEC90552}", "Person Merge")); } }
			public static MultilingualString System_TransportZones { get { return CombineCategories(System, ResString.GetMultilingualString("adf812d1-2c67-41a9-bac6-621070f37359", "Transport Zones")); } }
			public static MultilingualString Geography { get { return CombineCategories(MasterData, ResString.GetMultilingualString("f92a0757-6e8a-4c9d-8d55-3e0f5408f871", "Geography")); } }
			public static MultilingualString Enrichment { get { return CombineCategories(MasterData, ResString.GetMultilingualString("04A5DF67-C27F-4E00-95FD-CE371B108CAA", "Enrichment")); } }
			public static MultilingualString System_Time { get { return CombineCategories(System, ResString.GetMultilingualString("D9997E77-7D9E-4841-B1AA-5940179AE330", "Time Servers")); } }
			public static MultilingualString System_SecurityOverride { get { return CombineCategories(System, ResString.GetMultilingualString("C5A02513-9406-4E1B-9D38-3E6DFA0F70D6", "Security Override")); } }
		}

		#endregion

		#region DynamicCategory
		public class DynamicCategory : Categories, ICustomizableDataCaptionSource
		{
			readonly MultilingualString _categoryString;

			public DynamicCategory(string module)
			{
				var moduleCategory = CustomizableDataResourceStrings.GetMultilingualString(this, null, module);
				_categoryString = CombineCategories(Categories.System_STLHighWaterMarks, moduleCategory);
			}
			public MultilingualString CategoryString => _categoryString;

			string ICustomizableDataCaptionSource.Description
			{
				get { return Res.GetString("A84A9020-2C31-4767-8B61-C318851CE15C", "STL High Water Mark"); }
			}

			public int MaxLength => 0;
			public ushort Asmid { get; set; }

			IEnumerable<IResString> ICustomizableDataCaptionSource.GetCompileTimeSystemCaptions()
			{
				return new List<IResString>();
			}

			string ICustomizableDataCaptionSource.GetKey(object context, string caption)
			{
				return Convert.ToBase64String(Encoding.UTF8.GetBytes(caption));
			}

			IEnumerable<IResString> ICustomizableDataCaptionSource.GetRuntimeCaptions(IResString userCaption, object context)
			{
				return new List<IResString>();
			}
		}
		#endregion

		#region Phone Dialing Uri Protocols

		public CodeDescriptionWithEnabledAndDefaultsRegistryItem PhoneDialingUriProtocols
		{
			get
			{
				return GetItem<CodeDescriptionWithEnabledAndDefaultsRegistryItem>(
					"PhoneDialingUriProtocol",
					delegate
					{
						var defaultValue = new CodeDescriptionWithEnabledAndDefaultCollection(256);
						defaultValue.AddNewSystemDefined(
								SystemPhoneDiallingUriProtocols.Codes.Lync,
								SystemPhoneDiallingUriProtocols.Descriptions.Lync,
								true,
								true);
						defaultValue.AddNewSystemDefined(
								SystemPhoneDiallingUriProtocols.Codes.Skype,
								SystemPhoneDiallingUriProtocols.Descriptions.Skype,
								false,
								true);

						var registryItem = new CodeDescriptionWithEnabledAndDefaultsRegistryItem(
							"PhoneDialingUriProtocol",
							Categories.System_Miscellaneous,
							ResString.GetMultilingualString("a7779d6d-bcd6-4fb4-8896-6209fc5dc821", "Phone Dialing URI Protocols"),
							ResString.GetMultilingualString("6382ce95-534e-4442-a10f-650ebdc4dcd5", "The list of protocols to use when constructing a URI from a phone number to make a call. The full URI will be <protocol>:<phone number>. For example, sip:80012299."),
							new CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo()
							{
								CodeColumnCaption = ResString.GetMultilingualString("e8ee1965-ede8-4563-ade2-e7a223848cdf", "Protocol"),
								DescriptionColumnCaption = ResString.GetMultilingualString("dd80164c-e93e-4c15-901d-f90b11acf16a", "Application Name")
							},
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryOptions.Default,
							defaultValue,
							true);
						return registryItem;
					});
			}
		}

		#endregion

		#region Color Scheme

		public IColorTheme ColorTheme
		{
			get
			{
				if (Env.CurrentCompany == null) // not yet logged in
				{
					return StandardColorTheme;
				}

				if (fColorTheme == null)
				{
					InitializeColorTheme();
				}

				return fColorTheme;
			}
		}

#if DEBUG

		public IDisposable SetColorThemeTemporarily(ColorTheme newTheme)
		{
			var oldTheme = fColorTheme;
			fColorTheme = newTheme;

			return new DisposableAction(() => fColorTheme = oldTheme);
		}
#endif

		public void InitializeColorTheme()
		{
			fColorTheme = ColorThemeRegItem.Value.ChosenTheme ?? StandardColorTheme;
		}

		IColorTheme fColorTheme;

#if DEBUG
		internal
#endif
		IColorTheme StandardColorTheme
		{
			get { return fStandardColorTheme ?? (fStandardColorTheme = DefinedColorThemes.CargoWiseColorTheme); }
		}
		IColorTheme fStandardColorTheme;

		ColorThemeRegistryItem ColorThemeRegItem
		{
			get
			{
				return GetItem("ColorTheme", delegate
				{
					ColorThemeSelector selector = new ColorThemeSelector();
					selector.ChosenThemeName = DefinedColorThemes.CargoWiseColorTheme.Name.GetUnresolvedString();
					ColorThemeRegistryItem result = new ColorThemeRegistryItem(
						"ColorTheme",
						Categories.System_UI,
						ResString.GetMultilingualString("c0fea8a4-c921-4383-9018-b0c82be88f1a", "Color Theme"),
						ResString.GetMultilingualString("75ca9c1e-9678-4723-b4fe-3aa8931f551c", "Allows you to change the colors used for the application. You can either chose from the pre-defined color themes, or create your own. Note - you must restart {0} after changing this registry item.", Core.Constants.ProductName),
						selector);

					return result;
				});
			}
		}

		#endregion

		#region Registry Refresh
		public IntRegistryItem RegistryRefreshFrequencyInSeconds
		{
			get
			{
				return GetItem("RegistryRefreshFrequencyInSeconds",
				() => new IntRegistryItem(
						"RegistryRefreshFrequencyInSeconds",
						Categories.Optimization,
						ResString.GetMultilingualString("9348BB5D-8A74-4DDC-8FF9-FF3D1174CEBD", "Registry Refresh Frequency In Seconds"),
						ResString.GetMultilingualString("9BD70F5F-49AA-4E39-9095-C22B1E187896", "Sets the frequency of the registry refreshing its settings"),
						RegistryStorageFlags.System,
						600)
						);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		int ISystemDataRegistry.RegistryRefreshFrequencyInSeconds
		{
			get { return RegistryRefreshFrequencyInSeconds.Value; }
		}
		#endregion

		public StringRegistryItem DisableLastEditUpdateOfParentInNativeXml
			=> GetItem
			(
				"DisableLastEditUpdateOfParentInNativeXml",
				() => new StringRegistryItem
				(
					"DisableLastEditUpdateOfParentInNativeXml",
					Categories.Optimization,
					(NoResString)"Disable Last Edit Update Of Parent In Native XML",
					(NoResString)"Comma separated list of parent tables excluded from last edit update when their child tables are modified in Native XML.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport
				)
			);

		public BooleanRegistryItem AllowScalarFunctionsInCustomSql
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowScalarFunctionsInCustomSql", delegate
				{
					return new BooleanRegistryItem(
						"AllowScalarFunctionsInCustomSql",
						Categories.Optimization,
						ResString.GetMultilingualString("fb5f0b99-f6f5-3d56-afc1-1954038f6745", "Allow Custom SQL statements to use scalar functions"),
						ResString.GetMultilingualString("0fbc6bd7-37b6-353d-8a96-d85497743f3d", "If this registry is enabled, no error will be returned when using scalar functions in custom SQL statements"),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public IntRegistryItem ReportSlowUpgradeTransformsSeconds
		{
			get
			{
				return GetItem<IntRegistryItem>("ReportSlowUpgradeTransformsSeconds", delegate
				{
					return new IntRegistryItem(
						"ReportSlowUpgradeTransformsSeconds",
						Categories.Optimization,
						ResString.GetMultilingualString("f4dea659-2acc-429d-bd63-cd97dd89de0e", "Report offline upgrade transforms above this many seconds"),
						ResString.GetMultilingualString("413e0eae-1d42-401f-935a-625d02ff594e", "After each offline pre-upgrade or offline post-upgrade transform completes, if it took more than this many seconds it is reported."),
						RegistryStorageFlags.System,
						30);
				});
			}
		}

		public BooleanRegistryItem UnionOrOrFilter
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UnionOrOrFilter", delegate
				{
					return new BooleanRegistryItem(
						"UnionOrOrFilter",
						Categories.Optimization,
						ResString.GetMultilingualString("252df14f-dbaa-44d6-b4b2-8bf4aefa0f20", "Enable UNION or OR? Filter"),
						ResString.GetMultilingualString("ee722b05-b006-4994-9594-77ab153b7bc2", @"Adds a new filter to every module, UNION or OR, which will use UNION or OR to glue together 'Or' filter category filters.
Note: This is an experimental setting which is not guaranteed to work in all cases, and may affect performance. With this in mind, it is also recommended this setting is enabled with the appropriate performance profiling."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem RecompileFilter
		{
			get
			{
				return GetItem("RecompileFilter", delegate
				{
					return new BooleanRegistryItem(
						"RecompileFilter",
						Categories.Optimization,
						ResString.GetMultilingualString("0D2023A9-D18F-4F2C-8855-E7F0D2F91D80", "Enable Recompile Filter"),
						ResString.GetMultilingualString("BFF66D05-BE01-4441-A160-B32640B14310", "Enabling this registry adds the Recompile option to every module filter. This will configure the search query to use OPTION(RECOMPILE)."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem CardinalityFilter
		{
			get
			{
				return GetItem("CardinalityFilter", delegate
				{
					return new BooleanRegistryItem(
						"CardinalityFilter",
						Categories.Optimization,
						ResString.GetMultilingualString("8262D7BF-6B97-4A86-BC41-09D881892999", "Enable Legacy Cardinality Estimation Filter"),
						ResString.GetMultilingualString("C178E376-B466-4A51-82A7-84D104B69406", "Enabling this registry item adds the Legacy Cardinality Estimation option to every module filter. This will configure the search query to use the Pre-2014 Legacy Cardinality Estimation model."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableToolStripTracking
		{
			get
			{
				return GetItem("EnableToolStripTracking", delegate
				{
					return new BooleanRegistryItem(
						"EnableToolStripTracking",
						Categories.Optimization,
						ResString.GetMultilingualString("fb5f0b99-f6f5-3d56-afc1-6954038f6745", "Track tool-strip controls and dispose if leaking"),
						ResString.GetMultilingualString("0fbc6bd7-37b6-353d-8a96-695497743f3d", "Track tool-strip controls and dispose if leaking"),
						RegistryStorageFlags.All,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		public BooleanRegistryItem EMMessageDataActive
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EMMessageDataActive", delegate
				{
					return new BooleanRegistryItem(
						"EMMessageDataActive",
						Categories.Optimization,
						ResString.GetMultilingualString("fb5f0b99-f6f5-3d56-afc1-0954038f6745", "Prefer {0} to store XML in {1}", EDIMessageSchema.Constants.EM_MessageData, EDIMessageSchema.Constants.TableName),
						ResString.GetMultilingualString("0fbc6bd7-37b6-353d-8a97-d85497743f3d", "If this registry is enabled, the system will automatically compress and store XML in {0}", EDIMessageSchema.Constants.EM_MessageData),
						RegistryStorageFlags.All,
						true);
				});
			}
		}

		string ISystemDataRegistry.CachedTables
		{
			get { return CachedTables.Value; }
		}

		public StringRegistryItem CachedTables
		{
			get
			{
				return GetItem("CachedTables",
					() => new StringRegistryItem(
						"CachedTables",
						Categories.Optimization,
						ResString.GetMultilingualString("983966cc-5d90-487c-85b4-89da80f2b48e", "Cached Tables"),
						ResString.GetMultilingualString("f8968d11-17c9-40b1-b081-d90bdbb3fe6e", "Allows caching at a low level of commonly used, infrequently changed tables. Separate with commas for multiple tables."),
						RegistryStorageFlags.System,
						string.Join(",", new[]
						{
							BMComponentSchema.Constants.TableName,
							BMComponentResourceLinkSchema.Constants.TableName,
							BMSystemSchema.Constants.TableName,
							BMSystemWorkflowDeterminerSchema.Constants.TableName,

							GlbBranchSchema.Constants.TableName,
							GlbCapabilitySchema.Constants.TableName,
							GlbCompanySchema.Constants.TableName,
							GlbDepartmentSchema.Constants.TableName,
							GlbGroupSchema.Constants.TableName,
							GlbGroupLinkSchema.Constants.TableName,
							GlbHolidaySchema.Constants.TableName,
							GlbResourceCapabilityPivotSchema.Constants.TableName,
							GlbStaffSchema.Constants.TableName,
							GlbStaffHolidaySchema.Constants.TableName,
							GlbWorkTimeSchema.Constants.TableName,

							RefAirlineSchema.Constants.TableName,
							RefAirlineEFreightRuleSchema.Constants.TableName,
							RefCarrierConsortiumSchema.Constants.TableName,
							RefCityPCodePivotSchema.Constants.TableName,
							RefCityTownSchema.Constants.TableName,
							RefCommodityCodeSchema.Constants.TableName,
							RefComplianceListSchema.Constants.TableName,
							RefContainerSchema.Constants.TableName,
							RefContainerCodeMapSchema.Constants.TableName,
							RefContainerStockSchema.Constants.TableName,
							RefCountrySchema.Constants.TableName,
							RefCountryRequiredDocumentSchema.Constants.TableName,
							RefCountryRulesSchema.Constants.TableName,
							RefCountryStatesSchema.Constants.TableName,
							RefCurrencySchema.Constants.TableName,
							RefDocSourceSchema.Constants.TableName,
							RefDocTypeSchema.Constants.TableName,
							RefDomesticCartageZoneSchema.Constants.TableName,
							"RefEnergySource",
							"RefEnergySourceFactor",
							RefEquipmentSchema.Constants.TableName,
							RefEquipmentConfigSchema.Constants.TableName,
							RefEquipmentConfigItemSchema.Constants.TableName,
							RefEquipmentTemplateSchema.Constants.TableName,
							RefExchangeRateSchema.Constants.TableName,
							RefLanguageTextSchema.Constants.TableName,
							RefLatLongPostcodeSchema.Constants.TableName,
							RefLocoMapSchema.Constants.TableName,
							RefNMFCSchema.Constants.TableName,
							RefOrgConsortiumPivotSchema.Constants.TableName,
							RefPackTypeSchema.Constants.TableName,
							RefPacksSchema.Constants.TableName,
							RefPostCodeSchema.Constants.TableName,
							RefPremisesGateCodeSchema.Constants.TableName,
							RefServiceLevelSchema.Constants.TableName,
							RefShippingLineSchema.Constants.TableName,
							RefTimeZoneSetSchema.Constants.TableName,
							RefTimeZoneSchema.Constants.TableName,
							RefTimeZoneRuleSchema.Constants.TableName,
							RefTransitTimeSchema.Constants.TableName,
							RefUNLOCOSchema.Constants.TableName,
							RefZoneHeaderSchema.Constants.TableName,
							RefZonePivotSchema.Constants.TableName,

							StmModuleFilterSchema.Constants.TableName,
							StmModuleFilterUserDataSchema.Constants.TableName,
							StmServiceHostSchema.Constants.TableName,

							TagDefinitionSchema.Constants.TableName,

							RefLocalLanguageSchema.Constants.TableName,

							ProcessFieldChangeRuleSchema.Constants.TableName,
							ProcessFieldChangeRuleFieldSchema.Constants.TableName,

							WhsAreaSchema.Constants.TableName,
							WhsCartonGroupSchema.Constants.TableName,
							WhsCartonSizeSchema.Constants.TableName,
							WhsCartonGroupSizeLinkSchema.Constants.TableName,
							WhsClientParameterByWarehouseSchema.Constants.TableName,
							WhsClientPickPackParamsByWhsSchema.Constants.TableName,
							WhsInventoryHeldCodeSchema.Constants.TableName,
							WhsLocationTypeSchema.Constants.TableName,
							WhsPickFaceSchema.Constants.TableName,
							WhsProductParamsByWhsAndClientSchema.Constants.TableName,
							WhsPutawayGroupSchema.Constants.TableName,
							WhsRowSchema.Constants.TableName,
							WhsSalesChannelSchema.Constants.TableName,
							WhsWarehouseSchema.Constants.TableName,
						}))
					);
			}
		}

		public BooleanRegistryItem EnableQueryHintsForColourSchemeManager
		{
			get => GetItem("EnableQueryHintsForColourSchemeManager", () => new BooleanRegistryItem(
				"EnableQueryHintsForColourSchemeManager",
				Categories.Optimization,
				(NoResString)"Enables Query Hints for Color Scheme Manager",
				(NoResString)"Enables Query Hints for the Color Scheme Manager when the top level table is indexed.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true
				));
		}

		public IntRegistryItem QueryTimeoutForColourSchemeManager
		{
			get => GetItem("QueryTimeoutForColourSchemeManager", () => new IntRegistryItem(
				"QueryTimeoutForColourSchemeManager",
				Categories.Optimization,
				ResString.GetMultilingualString("5E3D2FAA-EB40-4F28-9B65-8F243EDB7F79", "Color Scheme Manager Query Timeout"),
				ResString.GetMultilingualString("79AC7733-9A05-406B-96D1-60ED4B01AE26", "Set query timeout for color scheme manager in order to prevent server overhead caused by over-complex color rules."),
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				5
				));
		}

		bool ISystemDataRegistry.EnableModuleQueryFromSecondaryDbReplica => EnableModuleQueryFromSecondaryDbReplica.Value;

		public BooleanRegistryItem EnableModuleQueryFromSecondaryDbReplica
		{
			get
			{
				return GetItem("EnableModuleQueryFromSecondaryDbReplica", delegate
				{
					var result = new BooleanRegistryItem(
					"EnableModuleQueryFromSecondaryDbReplica",
					Categories.Optimization,
					ResString.GetMultilingualString("BB75C772-0BFF-4A04-95CB-279995C52921", "Enable Read from Secondary Database Replica"),
					ResString.GetMultilingualString("7EDF0DA3-2676-4755-B24E-45A8134A7CAA", "When enabled, allows search operations to use a secondary database replica, reducing the load on the primary SQL database node."),
					RegistryStorageFlags.System,
					EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController,
					false
					);

					return result;
				});
			}
		}

		string[] ISystemDataRegistry.ModuleQueryDbServerNames => ModuleQueryDbServerNames.Value;

		public StringArrayRegistryItem ModuleQueryDbServerNames
		{
			get
			{
				return GetItem<StringArrayRegistryItem>("ModuleQueryDbServerNames", delegate
				{
					var result = new StringArrayRegistryItem(
						"ModuleQueryDbServerNames",
						Categories.Optimization,
						ResString.GetMultilingualString("93DC8E6E-DD0B-4D09-A3B0-CF2A8018AD6B", "Secondary Databases Full Server Names"),
						ResString.GetMultilingualString("3BB0ABB4-9081-47FD-8285-7EBDA0CA02F8", "The full server names of the secondary databases, including the instance names if applicable."),
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController);
					result.DataType.MaximumLength = 128;
					return result;
				});
			}
		}

		bool ISystemDataRegistry.AllowCTWhenCDCEnabled => AllowCTWhenCDCEnabled.Value;

		public BooleanRegistryItem AllowCTWhenCDCEnabled
		{
			get
			{
				return GetItem("AllowCTWhenCDCEnabled",
					() => new BooleanRegistryItem(
						"AllowCTWhenCDCEnabled",
						Categories.System_Upgrade,
						(NoResString)"Allows Change Tracking When CDC is enabled",
						(NoResString)"Allows Change Tracking when CDC is enabled.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false));
			}
		}

		string ISystemDataRegistry.WindowPersisterData
		{
			get
			{
				try
				{
					return WindowPersisterData.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				}
				catch (NullReferenceException ex)
				{
					ErrorReporter.ReportOnce("WindowPersisterData_Get_NRE", $"IsNull:{WindowPersisterData is null}, WindowPersisterDataName:{WindowPersisterDataName}", ex);
					return string.Empty;
				}
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					WindowPersisterData.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					((IRegistryItemInternals)WindowPersisterData).DeleteRecord(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				}
			}
		}

		StringRegistryItem WindowPersisterData
		{
			get
			{
				return GetItem(WindowPersisterDataName,
					() => new StringRegistryItem(
						WindowPersisterDataName,
						Categories.System_Miscellaneous,
						null,
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden)
					);
			}
		}

		string WindowPersisterDataName
		{
			get
			{
				return "WindowPersisterData:" + Env.CurrentUser?.Initials ?? string.Empty;
			}
		}

		#region LastDatabaseRestore

		public LastDbRestoreRegistryItem LastDatabaseRestore
		{
			get
			{
				return GetItem<LastDbRestoreRegistryItem>("LastDatabaseRestore", delegate
				{
					return new LastDbRestoreRegistryItem(
						"LastDatabaseRestore",
						Categories.System_Database,
						ResString.GetMultilingualString("30efe039-4150-400f-be97-bf3afefc366a", "Last Database Restore"),
						ResString.GetMultilingualString("76c00899-47cc-4270-9b95-fe2842bddb0d", "Details of last performed database restore using the Database Backup and Restore tool"),
						RegistryStorageFlags.System,
						RegistryOptions.IsReadOnly,
						new LastDbRestoreInfo());
				});
			}
		}

		DateTime ISystemDataRegistry.LastDatabaseRestoreDate
		{
			get { return LastDatabaseRestore.Value.CompletionDate.IsValid ? LastDatabaseRestore.Value.CompletionDate.ToDateTime() : DateTime.MinValue; }
		}

		#endregion

		#region SuspendAuditTriggers

		public BooleanRegistryItem SuspendAuditTriggers
		{
			get
			{
				return GetItem(DbRegistry.SuspendAuditTriggersName, delegate
				{
					return new BooleanRegistryItem(
					name: DbRegistry.SuspendAuditTriggersName,
					category: Categories.System_Database,
					caption: (NoResString)"Suspend Audit Triggers",
					hint: (NoResString)"Allows to suspend AuditTriggers (related to audit columns) for INSERT/UPDATE sql statements.",
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForSupport,
					defaultValue: DbRegistry.SuspendAuditTriggersDefaultValue);
				});
			}
		}

		#endregion

		#region MissingFetchHint
		public BooleanRegistryItem MissingFetchHintDetection
		{
			get
			{
				return GetItem<BooleanRegistryItem>((NoResString)"Missing Fetch Hint Detection", delegate
				{
					return new BooleanRegistryItem(
					name: (NoResString)"Missing Fetch Hint Detection",
					category: Categories.System_Database,
					caption: (NoResString)"Missing Fetch Hint Detection",
					hint: (NoResString)"Throws an error when there are more identical SQL queries than threshold called from the same call stack within 5 seconds. Refreshes on application restart.",
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForDevelopers,
					defaultValue: false);
				});
			}
		}

		public IntRegistryItem MissingFetchHintThreshold
		{
			get
			{
				return GetItem<IntRegistryItem>((NoResString)"Missing Fetch Hint Threshold", delegate
				{
					return new IntRegistryItem(
						name: (NoResString)"Missing Fetch Hint Threshold",
						category: Categories.System_Database,
						caption: (NoResString)"Missing Fetch Hint Threshold",
						hint: (NoResString)"Minimum number of identical SQL queries executed within 5 seconds to trigger an error. Refreshes on application restart.",
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForDevelopers,
						defaultValue: 25,
						minValue: 5,
						maxValue: 1000);
				});
			}
		}

		#endregion

		#region Logos

		public BooleanRegistryItem UseLoginBranchLogoForFreight
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseLoginBranchLogoForFreight", delegate
				{
					return new BooleanRegistryItem(
						"UseLoginBranchLogoForFreight",
						Categories.System_Logos,
						ResString.GetMultilingualString("fb5f0b99-f6f5-4d56-afc1-0954038f6745", "Use Logo Based On Login Branch For Freight"),
						ResString.GetMultilingualString("0fbc6bd7-37b6-453d-8a97-d85497743f3d", "If this registry is enabled, on freight job documents the company logo is based on user login branch rather than controlling branch specified for the job"),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public ImageRegistryItem CompanyLogo
		{
			get
			{
				return GetItem<ImageRegistryItem>((NoResString)"Logo", delegate
				{
					return new ImageRegistryItem(
						(NoResString)"Logo",
						Categories.System_Logos,
						ResString.GetMultilingualString("2808806d-11e1-4bd2-a012-37646c12654f", "Letterhead"),
						ResString.GetMultilingualString("2db0ea44-3df5-4983-a8fc-ebd9aafe8545", "Letterhead image that appears as the top most banner of {0} documents.", Core.Constants.ProductName),
						RegistryStorageFlags.All);
				});
			}
		}

		public ImageRegistryItem CompanyCheckLogo
		{
			get
			{
				return GetItem<ImageRegistryItem>("CompanyCheckLogo", delegate
				{
					return new ImageRegistryItem(
						"CompanyCheckLogo",
						Categories.System_Logos,
						ResString.GetMultilingualString("be2f755d-b782-4697-ac09-3c99fb8f2646", "Company Check Logo"),
						ResString.GetMultilingualString("f4cc5240-fb06-4369-8414-417e8cbe4495", "Company check logo image that appears as the top most banner of a check."),
						RegistryStorageFlags.All);
				});
			}
		}

		public ImageRegistryItem InvoceAndStatementLogo
		{
			get
			{
				return GetItem<ImageRegistryItem>("InvoiceAndStatementLogo", delegate
				{
					return new ImageRegistryItem(
						"InvoiceAndStatementLogo",
						Categories.System_Logos,
						ResString.GetMultilingualString("c0c49547-afca-4155-b66e-4eef617c80f3", "Invoice And Statement Letterhead Override"),
						ResString.GetMultilingualString("e9179b6b-bac2-479f-b0b5-5bceef96c67a", "Letterhead image that appears as the top most banner of the AR Invoice and AR Statement documents."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		Image ISystemDataRegistry.GetHtmlEmailBannerImage(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return HtmlEmailBannerImage.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		public ImageRegistryItem HtmlEmailBannerImage
		{
			get
			{
				return GetItem<ImageRegistryItem>("HtmlEmailBannerImage", delegate
				{
					Stream defaultImageStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Registry.Business.System.DefaultHtmlEmailFooterImage.gif");
					Image defaultImage = Image.FromStream(defaultImageStream);

					return new ImageRegistryItem(
						"HtmlEmailBannerImage",
						Categories.System_Logos,
						ResString.GetMultilingualString("99c6c837-d769-47b3-bd27-a938b81568a7", "HTML Email Banner Image"),
						ResString.GetMultilingualString("b6bb0e23-2fad-43a6-b2b5-afaf393b7d48", "Logo image that appears as the top most banner of notification emails that are in HTML format."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						defaultImage);
				});
			}
		}

		Image ISystemDataRegistry.GetHtmlEmailFooterImage(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return HtmlEmailFooterImage.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		public ImageRegistryItem HtmlEmailFooterImage
		{
			get
			{
				return GetItem<ImageRegistryItem>("HtmlEmailFooterImage", delegate
				{
					Stream defaultImageStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Registry.Business.System.DefaultHtmlEmailFooterImage.gif");
					Image defaultImage = Image.FromStream(defaultImageStream);

					return new ImageRegistryItem(
						"HtmlEmailFooterImage",
						Categories.System_Logos,
						ResString.GetMultilingualString("65505b96-fa59-48a8-913c-6d3899cc59ef", "HTML Email Footer Image"),
						ResString.GetMultilingualString("fac03f94-43f8-4d75-a0ef-8a71cef7f2c0", "Logo image that is placed as the footer of notification emails that are in HTML format."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						defaultImage);
				});
			}
		}

		string ISystemDataRegistry.GetHtmlEmailStyleSheet(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return HtmlEmailStyleSheet.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
		}

		string ISystemDataRegistry.HtmlEmailStyleSheet
		{
			get { return HtmlEmailStyleSheet.Value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public StringRegistryItem HtmlEmailStyleSheet
		{
			get
			{
				return GetItem<StringRegistryItem>("HtmlEmailStyleSheet", delegate
				{
					#region DefaultValue

					string defaultValue = @"
P {
		FONT-SIZE: 12px;
		COLOR: #666666;
		FONT-FAMILY: Arial, sans-serif;
		line-height: 17px;
}

body {
		background-color: #FFFFFF;
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #666666;
		margin: auto;
		width: 600px;
}

th {
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #FFFFFF;
	background-color: #005596;
}

td {
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #666666;
}

a, a:visited {
	color: #666666;
	text-decoration: underline;
}

a:hover {
	color: #00a4e4;
	text-decoration: underline;
}

.heading1 {
		font-family: Arial, sans-serif;
		font-size: 20px;
		font-weight: normal;
		color: #000000;
		line-height: 36px;
}

.subheading2 {
		font-family: Arial, sans-serif;
		font-size: 16px;
		line-height: 18px;
		font-weight: bold;
	color: #00a4e4;
}

.table {
		border-right: #666666 solid thin;
		border-top: #666666 solid thin;
		border-left: #666666 solid thin;
		border-bottom: #666666 solid thin;
}

.tableheadings td, th {
		border-bottom: #666666 solid thin;
		background-color: #005596;
}

.content {
		padding: 0em 1em;
}

.banner {
		padding-top: 1em;
}

".Trim();

					#endregion

					return new StringRegistryItem(
						"HtmlEmailStyleSheet",
						Categories.System_Logos,
						ResString.GetMultilingualString("c54532cf-a12a-41e5-bfdc-324d5813547c", "HTML Email Style Sheet (CSS)"),
						ResString.GetMultilingualString("392620c2-16aa-4d03-ad98-b8786f759af4", "HTML Style Sheet used in the format of notification emails that are in HTML format."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						defaultValue);
				});
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem EnableReportWriter
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableReportWriter", delegate
				{
					return new BooleanRegistryItem(
						"EnableReportWriter",
						Categories.System_Reports,
						(NoResString)"Enable Report Writer",
						(NoResString)"If this registry is enabled, Report Writer feature will be active in Report Customization",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem CompleteOrderLineUpdate
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CompleteOrderLineUpdate", delegate
				{
					return new BooleanRegistryItem(
						"CompleteOrderLineUpdate",
						Categories.Orders,
						(NoResString)"Complete Order Line Update",
						(NoResString)"If set to 'Yes', order lines which are not included in the XML will be deleted. NOTE: This is applicable on standalone order import only.",
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		#region DocManager

		#region DocManager DB Data File Path

		public StringRegistryItem DocManagerDBDataFilePath
		{
			get
			{
				return GetItem<StringRegistryItem>("DocManagerDBDataFilePath", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("4982032e-7dec-4ea5-b5d2-caa567a2dfc9",
@"The full path to a location on the server where DocManager data files (.MDF) should be stored. The path MUST refer to a local hard drive on the SQL server - it cannot be a network location (e.g. it cannot be {0}). Please ensure that the specified folder exists on your SQL server.

Note: changing this registry item will not move existing data files to the new location. You must move the data files manually. Please contact support if you are unsure of how to move them.", @"\\AnotherServer\data");

					return new StringRegistryItem(
						"DocManagerDBDataFilePath",
						Categories.System_DocManager,
						ResString.GetMultilingualString("8ab88f6b-b0f8-4536-b920-83d1c4c8ff51", "DocManager Databases Data File Path"),
						hint,
						new LocalDirectoryRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted,
						string.Empty);
				});
			}
		}

		#endregion

		#region DocManager Paranoid Mode

		public BooleanRegistryItem DocManagerParanoidModeEnabled
		{
			get
			{
				return GetItem("DocManagerParanoidModeEnabled", delegate
				{
					return new BooleanRegistryItem(
						"DocManagerParanoidModeEnabled",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("516792BD-A9A8-4577-972D-AD7625B3E33E", "Write Verification Mode"),
						ResString.GetMultilingualString("2A3816DE-509D-473A-88A4-EAA72AA9050F", "If enabled, the upload to S3 compatible storage will include an MD5 data integrity verification."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region DocManager DB Log File Path

		public StringRegistryItem DocManagerDBLogFilePath
		{
			get
			{
				return GetItem<StringRegistryItem>("DocManagerDBLogFilePath", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("e1ddd0ef-efc6-4012-a204-bb3e9df5617a",
@"The full path to a location on the server where DocManager log files (.LDF) should be stored. The path MUST refer to a local hard drive on the SQL server - it cannot be a network location (e.g. it cannot be {0}). Please ensure that the specified folder exists on your SQL server.

Note: changing this registry item will not move existing log files to the new location. You must move the log files manually. Please contact support if you are unsure of how to move them.", @"\\AnotherServer\data");

					return new StringRegistryItem(
						"DocManagerDBLogFilePath",
						Categories.System_DocManager,
						ResString.GetMultilingualString("f3672c19-62b6-46b3-a125-9b03bc93ec57", "DocManager Databases Log File Path"),
						hint,
						new LocalDirectoryRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted,
						string.Empty);
				});
			}
		}

		#endregion

		#region eDocs Maximum Filesize

		public IntRegistryItem eDocsMaximumFilesize
		{
			get
			{
				return GetItem<IntRegistryItem>("eDocsMaximumFilesize", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"eDocsMaximumFilesize",
						Categories.System_DocManager,
						ResString.GetMultilingualString("708e45d7-38a6-4a05-b82d-604359926712", "eDocs Maximum File Size"),
						ResString.GetMultilingualString("8a1622a6-405b-4e45-a772-3701698fd9d1", "The maximum size allowed per file on the eDocs Files tab (in MB). This registry item can be a value from 1MB to 100MB."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted,
						10);

					result.DataType = new IntRegistryDataType(0, 100);
					return result;
				});
			}
		}

		#endregion

		#region eDocs Storage

		public CodePairRegistryItem EDocsStorageProvider
		{
			get
			{
				return GetItem("EDocsStorageProvider", delegate
				{
					var registryItem = new CodePairRegistryItem(
						"EDocsStorageProvider",
						Categories.System_DocManager,
						ResString.GetMultilingualString("ef2D9291-3fc0-4dd3-97e3-0dbdc3e95854", "eDocs Storage"),
						ResString.GetMultilingualString("beb026e8-26d3-414f-9d1e-d949a186a8ba", @"Define the storage of the eDocs.
Note:
This Registry item can only be set to S3 compatible storage when the following S3 Storage Registry items have been configured.

{1}
{2}
{3}",
Constants.EDocsStorageProviders.Description.S3,
((IMultilingualRegistryItem)SystemDataRegistry.Instance.EDocsStorageServiceUrl).LocationMultilingual,
((IMultilingualRegistryItem)SystemDataRegistry.Instance.DocManagerStorageBucketName).LocationMultilingual,
((IMultilingualRegistryItem)SystemDataRegistry.Instance.EDocsStorageAccess).LocationMultilingual
),
						OLookUpEditType.EDocsStorageProvider,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController),
						Constants.EDocsStorageProviders.Code.DB)
					{
						EditorInfo = new ComboBoxRegistryEditorInfo(OLookUpEditType.EDocsStorageProvider, true)
					};
					registryItem.DataType = new EDocsStorageProviderRegistryDataType();
					return registryItem;
				});
			}
		}

		public string UpdateS3ConfigContinueString => Res.GetString("BB3E852C-0D10-4BA9-BEF2-C06BFD39D9E5", "Continue");

		public StringRegistryItem EDocsStorageAccess
		{
			get
			{
				return GetItem("EDocsStorageAccess", delegate
				{
					var result = new StringRegistryItem(
						"EDocsStorageAccess",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("0BAE8133-37F3-4F04-8B00-E8FE03945CF6", "S3 Storage Credentials"),
						ResString.GetMultilingualString("C85424D1-BC1E-47CE-B38C-59DEAB428EA8", "Please provide access to the S3 storage. For example: {0}", "\"KeyId=A1B2C3;Secret=aAbBcCdDeE\""),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						string.Empty);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					result.DataType = new S3StorageRegistryDataType();
					return result;
				});
			}
		}

		public BooleanRegistryItem UsePathStyleAddressing
		{
			get
			{
				return GetItem("UsePathStyleAddressing", delegate
				{
					return new BooleanRegistryItem(
						"UsePathStyleAddressing",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("44C55472-FFB7-41F3-9BC7-90DF9D8C45BC", "Use Path Style Addressing"),
						ResString.GetMultilingualString("CC00A237-D81E-4A6F-A3FC-08A300966ABA", "By default, the requests will always use path style addressing. If disabled, virtual hosted-style addressing will be used."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem UseChunkEncoding
		{
			get
			{
				return GetItem("UseChunkEncoding", delegate
				{
					return new BooleanRegistryItem(
						"UseChunkEncoding",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("3A0A6E97-8A34-49C5-952A-C41FE0AD15C3", "Use Chunk Encoding"),
						ResString.GetMultilingualString("E22FC6B4-0D5B-4605-BF7D-1FA3B09EB6D0", "By default, a chunked encoding upload will be used for the request. Set to No to disable it."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public StringRegistryItem EDocsStorageServiceUrl
		{
			get
			{
				return GetItem("EDocsStorageServiceUrl", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"EDocsStorageServiceUrl",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("584F0FAA-2B13-4C46-AC64-6F68F07370F0", "S3 Storage URL"),
						ResString.GetMultilingualString("6AD18636-DE03-4D51-870E-1AB72EEAEB09", "Please provide the service URL to the S3 storage."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						string.Empty);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					result.DataType = new S3StorageRegistryDataType();
					return result;
				});
			}
		}

		public StringRegistryItem DocManagerStorageBucketName
		{
			get
			{
				var key = ObjectFactory.Get<IProductRegistration>().Key;
				return GetItem("DocManagerStorageBucketName", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"DocManagerStorageBucketName",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("6387FBC8-BF3A-440C-8BD5-6858E7725FD1", "S3 Bucket Name"),
						ResString.GetMultilingualString("4FBE2DBC-DC80-4152-82D7-BFABB0243D19", "Override Default to change bucket name when using S3 for eDocs Storage."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						string.Empty);
					result.DataType = new S3StorageRegistryDataType();
					return result;
				});
			}
		}

		public IntRegistryItem EDocsStorageConnectionTimeout
		{
			get
			{
				return GetItem<IntRegistryItem>("EDocsStorageConnectionTimeout", delegate
				{
					var result = new IntRegistryItem(
						"EDocsStorageConnectionTimeout",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("314896f8-9a2c-4172-add8-fdb3a5dd5059", "Connection Timeout"),
						ResString.GetMultilingualString("d9080c34-f932-4cb6-91fa-a1905711f4c7", "Time in seconds that the application will wait for a successful connection to the S3 environment before timing out. Accepted values are from 0 to 210 seconds. 0 means no time limit."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						30);

					result.DataType = new IntRegistryDataType(0, 210);
					return result;
				});
			}
		}

		public CodePairRegistryItem AWSS3StorageClass
		{
			get
			{
				return GetItem("AWSS3StorageClass", delegate
				{
					return new CodePairRegistryItem(
						"AWSS3StorageClass",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("D2DCE2F0-868C-4651-83C1-2F6AC45F771B", "Amazon S3 Storage Class"),
						ResString.GetMultilingualString("8A858EE6-AF4D-4D3E-907A-FCDC555A1EFA", @"Define the Amazon S3 Storage Class (Tier) to be used.
For more details, refer to: {0}
This setting is applicable only for Amazon S3 Storage. If it is not set, Standard class will be used by default.",
"https://aws.amazon.com/s3/storage-classes/"),
						AWSS3StorageClassListProvider,
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue);
				});
			}
		}

		internal ICodeDescriptionPairListProvider AWSS3StorageClassListProvider =>
			new CodeDescriptionPairListProvider(() =>
			{
				var result = new CodeDescriptionPairList();
				var attributeFields = typeof(S3StorageClass).GetFields()
										.Where(f => f.IsStatic)
										.OrderBy(f => f.Name);

				foreach (var attributeField in attributeFields)
				{
					var storageClassValue = ((S3StorageClass)attributeField.GetValue(null)).Value;
					var storageClassName = Regex.Replace(attributeField.Name, "((?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z]))", " $1");

					result.Add(new CodeDescriptionPair(storageClassValue, storageClassName));
				}
				return result;
			});

		public IntRegistryItem EDocsDBMinimumStorageDays
		{
			get
			{
				return GetItem<IntRegistryItem>("EDocsDBMinimumStorageDays", delegate
				{
					var result = new IntRegistryItem(
						"EDocsDBMinimumStorageDays",
						Categories.System_DocManager,
						ResString.GetMultilingualString("B9688BD8-D364-44AA-B294-3B2E62082B5F", "eDocs Database Minimum Storage Period (Days)"),
						ResString.GetMultilingualString("010B24C9-89E3-43CD-86EE-4968FDD4C46F", @"The minimum number of days that eDocs are stored in the DocManager database on the SQL server before being pushed to the external storage environment (e.g. S3 storage).

Note: This setting only affects the regular process of eDocs by DER service but not the initial process by DES service."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						90);

					result.DataType = new IntRegistryDataType(0, 9999);
					return result;
				});
			}
		}

		public BooleanRegistryItem UseVersionID
		{
			get
			{
				return GetItem("UseVersionID", delegate
				{
					return new BooleanRegistryItem(
						"UseVersionID",
						Categories.System_DocManager_S3Storage,
						ResString.GetMultilingualString("5B0DDBDE-A684-4BB4-90AE-789E34695245", "Use Version ID"),
						ResString.GetMultilingualString("3058E007-8277-4D34-994E-7CFDCD514C2E", @"When enabled, eDocs in external storage will be retrieved using the stored version ID, if versioning is enabled on the bucket in external storage.

Note:
Versioning must be enabled on the bucket in external storage before enabling this registry item."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region DocManager Data File Size Threshold GB

		public IntRegistryItem DocManagerDataFileSizeThresholdGb
		{
			get
			{
				return GetItem<IntRegistryItem>("DocManagerDataFileSizeThresholdGb", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"DocManagerDataFileSizeThresholdGb",
						Categories.System_DocManager,
						ResString.GetMultilingualString("9b1012d9-f428-485a-aa23-ef4c7bb4757f", "DocManager Database Size Threshold"),
						ResString.GetMultilingualString("92ac78d4-43be-4ecb-acfa-8545a2d75cbd", "When the current DocManager database exceeds this size limit, the DocManager Database Creation Service Task will create a new DocManager database. The minimum database size limit is 2GB and there is no maximum size limit."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted,
						8);

					result.DataType = new DocManagerDataFileSizeThresholdGbRegistryDataType(2, int.MaxValue);
					return result;
				});
			}
		}

		public IntRegistryItem DocManagerInitialDataFileSizeGb
		{
			get
			{
				return GetItem<IntRegistryItem>("DocManagerInitialDataFileSizeGb",
					() => new IntRegistryItem(
						"DocManagerInitialDataFileSizeGb",
						Categories.System_DocManager,
						ResString.GetMultilingualString("071AB71A-98F2-4051-953D-0E6F24A18717", "DocManager Database Initial Size (in gigabytes)"),
						ResString.GetMultilingualString("46FAC90F-EA3D-4514-84FC-DABD4EC5A9B3", "The initial size (in gigabytes) each DocManager database will be when created."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						1,
						1, 1024));
			}
		}

		#endregion

		#region DocManager Service Task Import Options

		public DirectorySearchRegistryItem DocManagerBatchProcessorImportOptions
		{
			get
			{
				return GetItem<DirectorySearchRegistryItem>("DocManagerBatchProcessorImportOptions", delegate
				{
					return new DirectorySearchRegistryItem(
						"DocManagerBatchProcessorImportOptions",
						Categories.System_DocManager,
						ResString.GetMultilingualString("fd356def-9e85-410f-9f95-726b3b61bb50", "DocManager Import Service Task Options"),
						ResString.GetMultilingualString("34f68de7-5369-452b-932d-30a62a9ef319", "The directory that the DocManager Import Service Task will import DocManager files from. Files are deleted from this directory after they are imported."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted);
				});
			}
		}

		#endregion

		#region DocManager TIF Colour Depth

		public CodePairRegistryItem DocManagerTIFColourDepth
		{
			get
			{
				return GetItem("DocManagerTIFColourDepth", delegate
				{
					return new CodePairRegistryItem(
						"DocManagerTIFColourDepth",
						Categories.System_DocManager,
						ResString.GetMultilingualString("c1df8c74-a726-4076-b360-f94b31d85ca5", "Color Depth for System Generated documents stored in eDocs"),
						ResString.GetMultilingualString("d4030ab0-8b08-4f36-8779-613cdec8aa3f", "Copies of documents emailed, printed or faxed from the current application will be stored on the eDocs tab at the selected color depth. If you process a large volume of documents, or have limited storage space, you should store your documents in Black and White to minimize the amount of storage space required.\r\n\r\nNOTE: Color documents can be up to 20 times larger than Black and White.\r\nNOTE: This setting is used only if TIF format is selected in eDocs Import File Format registry item."),
						OLookUpEditType.ColourDepth,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						Constants.ColourDepth.BlackAndWhite)
					{
						EditorInfo = new ComboBoxRegistryEditorInfo(OLookUpEditType.ColourDepth, false)
					};
				});
			}
		}

		#endregion

		#region eDoc Import File Format

		public CodePairRegistryItem EDocImportFileFormat
		{
			get
			{
				return GetItem("EDocImportFileFormat", () =>
				{
					return new CodePairRegistryItem("EDocImportFileFormat",
						Categories.System_DocManager,
						ResString.GetMultilingualString("d23a61f3-c274-4093-975e-07ca24a484ea", "eDoc Import File Format"),
						ResString.GetMultilingualString("6ce18967-1418-4f09-bbcb-32f26740f13f", "Documents which are automatically added to the eDocs tab will be stored in this format."),
						DocumentFileFormatListProvider,
						RegistryStorageFlags.System,
						Enterprise.Core.Constants.FileFormats.PDF);
				});
			}
		}

		public BooleanRegistryItem AllocatePasswordProtectedExcelSpreadsheets
		{
			get
			{
				return GetItem("AllocatePasswordProtectedExcelSpreadsheets", delegate
				{
					return new BooleanRegistryItem(
						"AllocatePasswordProtectedExcelSpreadsheets",
						Categories.System_DocManager,
						ResString.GetMultilingualString("D63C4EC6-3F54-4462-8821-F7A3303B7E08", "Allocate Password Protected Excel Spreadsheets"),
						ResString.GetMultilingualString("CF0B8ADE-244F-435E-B2A7-04C5A1706555", "When enabled documents and reports will be allocated with password protection.  When disabled, the Excel spreadsheet will be allocated to the eDocs in the file format specified in the System > DocManager > eDoc Import File Format"),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		ICodeDescriptionPairListProvider DocumentFileFormatListProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair(Enterprise.Core.Constants.FileFormats.PDF, (NoResString)"Portable Document Format"),
						new CodeDescriptionPair(Enterprise.Core.Constants.FileFormats.PDFA, (NoResString)"Portable Document Format Archive (PDF/A-2)"),
						new CodeDescriptionPair(Enterprise.Core.Constants.FileFormats.TIF, (NoResString)"Tagged Image Format"),
					};
				});
			}
		}

		#endregion

		#region DocManager Import Service Task Options

		public BooleanRegistryItem AllowDocManagerBatchProcessorImports
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowDocManagerBatchProcessorImports", delegate
				{
					return new BooleanRegistryItem(
						"AllowDocManagerBatchProcessorImports",
						Categories.System_DocManager,
						ResString.GetMultilingualString("080996cb-3de2-4226-ab60-5b1bcfe67586", "Allow DocManager Import Service Task"),
						ResString.GetMultilingualString("ab7b0db7-eeec-46b8-a64f-cccf6093d6e3", "This registry item specifies whether or not the DocManager Import Service Task should process the files in the DocManager Batch Import Folder and incoming DocManager import emails."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public CodeSelectionCollectionRegistryItem DocumentTypesRestrictedForImport
		{
			get
			{
				return GetItem<CodeSelectionCollectionRegistryItem>("DocumentTypesRestrictedForImport", delegate
				{
					return new CodeSelectionCollectionRegistryItem(
						"DocumentTypesRestrictedForImport",
						Categories.System_DocManager,
						ResString.GetMultilingualString("6c0aa491-5062-4ee8-a8f1-7ca977ee5ec7", "Document Types Restricted For Import"),
						ResString.GetMultilingualString("1805fa88-86a1-456a-be79-d1215bb10b5c", "The list of Document Type codes that, when encountered, will not be imported by DocManager Import Service Task."),
						RegistryStorageFlags.System,
						DocumentTypesRestrictedListProvider,
						RegistryOptions.PreserveTestValue,
						new CodeSelectionCollection(DocumentTypesRestrictedListProvider)
						);
				});
			}
		}

		public CodeDescriptionPairListProvider DocumentTypesRestrictedListProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() => DocumentTypesRestrictedList);
			}
		}

		CodeDescriptionPairList DocumentTypesRestrictedList
		{
			get
			{
				if (documentTypesRestrictedList == null)
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList();

					string sql = "select distinct RT_DocType, RT_Desc from dbo.RefDocType order by RT_DocType asc";
					using (var reader = Db.Connection.Command(sql).ExecuteReader())
					{
						while (reader.Read())
						{
							list.AddPair((string)reader[0], (string)reader[1]); //would rather have it be multilingual -> RT_DescMultilingual, but can't resolve IRefDocType or IRefDocTypeCollection here
						}
					}
					documentTypesRestrictedList = list;
				}
				return documentTypesRestrictedList;
			}
		}
		CodeDescriptionPairList documentTypesRestrictedList;

		public CodeDescriptionPairListRegistryItem EmailAddressesAllowedForImport
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("EmailAddressesAllowedForImport", delegate
				{
					MultilingualString hint =
						ResString.GetMultilingualString("E9FD1E63-B3CD-4640-AC7B-7FD054FBEF83", "Email addresses specified will be accepted by the DocManager Import Service task for email allocation to the eDocs tabs.\r\n\r\nNote: Wildcards can be used in the email addresses.  Such as *@domain.com or user?@domain.com");

					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"EmailAddressesAllowedForImport",
						Categories.System_DocManager,
						ResString.GetMultilingualString("77E41762-4452-4AC9-BF0F-4DCB6F269797", "Email Addresses Allowed For Import"),
						hint,
						100,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue)
					{ DataType = new EmailAddressesAllowedForImportDataType(256) };

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(
						true, true,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
						CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
						ResString.GetMultilingualString("4EA6C794-00DA-46C6-81BF-1480E64A1A56", "Email Address"),
						ResString.GetMultilingualString("B20CA247-73FA-4101-BD42-0C444A758360", "Description"));

					return result;
				});
			}
		}

#if DEBUG
		public
#endif
		class EmailAddressesAllowedForImportDataType : CodeDescriptionPairListRegistryDataType
		{
			public EmailAddressesAllowedForImportDataType(int codeMaxLength)
				: base(codeMaxLength)
			{
			}

			protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

				for (int i = 0; i < proposedValue.Count; i++)
				{
					if (string.IsNullOrEmpty(proposedValue[i].Code))
					{
						throw new RegistryValidationException(Res.GetString("C36349AF-80F3-4DE0-B8F8-D6A07CAA0F69", "Please enter an Email Address at line {0}", i + 1));
					}
					else if (!IsValidEmail(proposedValue[i].Code))
					{
						throw new RegistryValidationException(Res.GetString("ADF88FAE-0515-4BFB-9E45-0F568B066710", "The following is not a valid Email Address : {0}", proposedValue[i].Code));
					}
					else if (string.IsNullOrEmpty(proposedValue[i].Description.Trim()))
					{
						throw new RegistryValidationException(Res.GetString("4C51DAF7-0AAE-442C-A4C0-5E64A0355A55", "Please enter a Description for Email Address {0}", proposedValue[i].Code));
					}
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
			bool IsValidEmail(string email)
			{
				try
				{
					var addr = new System.Net.Mail.MailAddress(email);
					return addr.Address == email;
				}
				catch
				{
					return false;
				}
			}
		}

		public IntRegistryItem MaximumNumberOfDocManagerItemsToImportInABatch
		{
			get
			{
				return GetItem("MaximumNumberOfDocManagerItemsToImportInABatch", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfDocManagerItemsToImportInABatch",
						Categories.System_DocManager,
						ResString.GetMultilingualString("64b4f6c0-12f9-4f75-bb99-873fd31970d2", "Maximum Number Of Items To Import In One Batch"),
						ResString.GetMultilingualString("a8a3b0a3-c34e-44b2-a4b2-be440e4f777f", "Maximum number of files or emails that are imported in one batch."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						10, 1, 1000);
				});
			}
		}

		public BooleanRegistryItem DocumentImportUserContextTracingEnabled
		{
			get
			{
				return GetItem("DocumentImportUserContextTracingEnabled", delegate
				{
					return new BooleanRegistryItem(
						"DocumentImportUserContextTracingEnabled",
						Categories.System_DocManager,
						ResString.GetMultilingualString("D71ABEDF-3F52-4F34-819D-5E3272BAA29E", "Unexpected Context Switch Detection Enabled"),
						ResString.GetMultilingualString("F02B11C3-287C-4539-8681-6FE1FA56096C", "Enables User Context switch detection during import of documents. Unexpected context switches can cause incorrect Workflow templates to be applied. This functionality will also attempt to switch the context back to the DMI server task user context during the application of Workflow."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Unpublish Older Versions of Document Option

		public BooleanRegistryItem UnpublishOlderVersionDocument
		{
			get
			{
				return GetItem("UnpublishOlderVersionDocument", delegate
				{
					return new BooleanRegistryItem(
						"UnpublishOlderVersionDocument",
						Categories.System_DocManager,
						ResString.GetMultilingualString("93F0F1AA-72E3-40F7-A339-0CB94DB79BF0", "Un-publish Older Versions of Documents"),
						ResString.GetMultilingualString("B693A79C-AB8D-4463-B317-06DE8E60FE1F", "When enabled and a document with the same File Name and Document Type is added to the eDocs tab, older versions of the documents will be unpublished."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Use Default Windows Image Viewer

		public BooleanRegistryItem UseDefaultWindowsImageViewer
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseDefaultWindowsImageViewer", delegate
				{
					return new BooleanRegistryItem("UseDefaultWindowsImageViewer",
						Categories.System_DocManager,
						ResString.GetMultilingualString("954f519c-4069-45a8-bbf5-baa09808e3a5", "Use default Windows image viewing application"),
						ResString.GetMultilingualString("e4e78f9a-c7af-4093-a5f9-7c8b2c9b5b61", "Enable this if you want to use windows default image viewing application to view / edit eDocs rather than the {0} Viewer. This applies to all image files.", Constants.ProductName),
						RegistryStorageFlags.All | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Document Tracking

		public BooleanRegistryItem RestrictAEDAndAIDEvents
		{
			get
			{
				return GetItem("RestrictAEDAndAIDEvents", delegate
				{
					return new BooleanRegistryItem(
						name: "RestrictAEDAndAIDEvents",
						category: Categories.System_DocManager_DocumentTracking,
						caption: ResString.GetMultilingualString("820DBE46-BE78-4EB1-823E-795AFA44AAB6", "AED and AID events"),
						hint: ResString.GetMultilingualString("7F46A281-663A-439A-9A8F-E00311E43430", "When enabled, AED/AID events will be generated only when all the documents required for export/import have been received. \r\n\r\nWhen disabled, AED/AID events may be generated even if no export/import documents are required. This is the current behavior."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						defaultValue: false);
				});
			}
		}

		#endregion

		#endregion

		#region Email

		#region Allocate reports over the email attachment limit to eDocs

		public BooleanRegistryItem AllocateReportOverEmailAttachmentLimitToEDocs
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllocateReportOverEmailAttachmentLimitToEDocs", delegate
				{
					return new BooleanRegistryItem(
						"AllocateReportOverEmailAttachmentLimitToEDocs",
						Categories.System_Email,
						ResString.GetMultilingualString("694516E3-ACCF-41C7-8514-E03BB9CF37AB", "Allocate reports over the email attachment limit to eDocs"),
						ResString.GetMultilingualString("11E08FD4-CF53-429F-97A7-BF48E7A38284", @"If a report file size exceeds the maximum email attachment size allowed by your mail server, the report will be allocated to the eDocs and an email will be sent to the recipient with a link to download the report.

Please note, this functionally only works when the GLOW web services have been deployed and configured in the GLOW > Services Registry settings."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Email Attachment Size Limit

		public IntRegistryItem EmailAttachmentSizeLimitInMB
		{
			get
			{
				return GetItem<IntRegistryItem>("EmailAttachmentSizeLimitInMB", delegate
				{
					return new IntRegistryItem(
						"EmailAttachmentSizeLimitInMB",
						Categories.System_Email,
						ResString.GetMultilingualString("c1bc7a13-f579-4664-9dd2-8c8277e56147", "Email attachment size limit in megabytes"),
						ResString.GetMultilingualString("5445cff9-eb23-4292-a6e8-d1820985738a", "The maximum email attachment size allowed by your mail server. If the size of attachments is greater than the limit, the email will not be sent. The limit must be specified in megabytes (MB)"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						20);
				});
			}
		}

		#region DocumentSource

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem DDIDocumentSource
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DDIDocumentSource", () =>
				{
					return new BooleanRegistryItem(
						"DDIDocumentSource",
						Categories.System_DocManager,
						ResString.GetMultilingualString("d6306a38-34b5-4ab6-bf85-0ba7491a2ac6", "DDI/DDA Event Reference Contains Document Source"),
						ResString.GetMultilingualString("750521e7-6349-42b3-bcb3-af605562d8a2", "When true, DDI/DDA Event Reference will contain a parameter for Document Source."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Purge Outgoing Emails Older Than

		public IntRegistryItem PurgeOutgoingEmailsOlderThan
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeOutgoingEmailsOlderThan", delegate
				{
					return new IntRegistryItem(
						"PurgeOutgoingEmailsOlderThan",
						Categories.System_Email,
						ResString.GetMultilingualString("04e4fb01-1779-475a-bce6-a4093c8658fe", "Purge outgoing emails older than n days"),
						ResString.GetMultilingualString("1f58ae48-a241-4503-a317-242c43f6a721", "Outgoing emails will be deleted n number of days after they have been sent"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						5,
						0,
						36500);
				});
			}
		}

		#endregion

		#region Purge Outgoing Unsent Emails Older Than

		public IntRegistryItem PurgeUnsentOutgoingEmailsOlderThan
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeUnsentOutgoingEmailsOlderThan", delegate
				{
					return new IntRegistryItem(
						"PurgeUnsentOutgoingEmailsOlderThan",
						Categories.System_Email,
						ResString.GetMultilingualString("805B6D75-55BF-4488-8A87-D4A5DC388972", "Purge outgoing emails older than n days in QUE status"),
						ResString.GetMultilingualString("1449C5A4-BE83-4035-855F-E4BD1648A128", "Outgoing emails in QUE status will be purged after n days"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						20,
						0,
						36500);
				});
			}
		}

		#endregion

		#region Purge Incoming Processed Emails Older Than

		public IntRegistryItem PurgeIncomingProcessedEmailsOlderThan
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeIncomingProcessedEmailsOlderThan", delegate
				{
					return new IntRegistryItem(
						"PurgeIncomingProcessedEmailsOlderThan",
						Categories.System_Email,
						ResString.GetMultilingualString("e5970b99-e960-43fa-a745-e86a10f12bd7", "Purge incoming processed emails older than n days"),
						ResString.GetMultilingualString("0776d3ea-6403-411a-bbfb-66a79cad66b3", "Incoming emails will be deleted n number of days after they have been received and processed"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						5,
						0,
						36500);
				});
			}
		}

		#endregion

		#region Purge Incoming UnProcessed Emails Older Than

		public IntRegistryItem PurgeIncomingUnProcessedEmailsOlderThan
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeIncomingUnProcessedEmailsOlderThan", delegate
				{
					return new IntRegistryItem(
						"PurgeIncomingUnProcessedEmailsOlderThan",
						Categories.System_Email,
						ResString.GetMultilingualString("8c5744b5-0684-4112-8a6a-6c534a1adb78", "Purge incoming unprocessed emails older than n days"),
						ResString.GetMultilingualString("ddd8d59c-10d2-4d91-9e2b-f09b757c91ad", "Incoming emails will be deleted n number of days after they have been received, whether or not they have been processed"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						20,
						0,
						36500);
				});
			}
		}

		public BooleanRegistryItem RunOMSInSimulationMode
		{
			get
			{
				return GetItem<BooleanRegistryItem>("RunOMSInSimulationMode", delegate
				{
					return new BooleanRegistryItem(
						"RunOMSInSimulationMode",
						Categories.System_Email,
						ResString.GetMultilingualString("37cc10f2-127a-419d-97c5-3ca0e52b478e", "Run Outbound Mail Service Task In Simulation Mode"),
						ResString.GetMultilingualString("66a5fb2a-0137-4352-b88a-713532533c62",
@"This option will set the Outbound Mail Service task to run in simulation mode.  When enabled, the Outbound Mail Service task will not log into the outbound mail server and send email messages to it for delivery.  Instead, the emails will automatically be marked as SNT status from the QUE status.

Simulation mode for the Outbound Mail Service task should only be enabled in those systems where delivery of emails are not required."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		bool ISystemDataRegistry.RunOMSInSimulationMode => RunOMSInSimulationMode.Value;

		#endregion

		#region Bounce Back Email Process

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Default Email Template Text")]
		public NotificationEmailTemplateRegistryItem NonDeliveryReceiptNotificationEmailTemplate
		{
			get
			{
				return GetItem<NotificationEmailTemplateRegistryItem>("NonDeliveryReceiptNotificationEmailTemplate", delegate
				{
					const string defaultSubject = "Email Non-delivery Receipt: (*Recipient*)";
					const string defaultBody =
@"A Non-delivery Receipt (NDR) was received for the following sent email:

  Recipient: (*Recipient*)
  Sent: (*OriginEmailSentTime*)
  Document: (*DocumentName*)
  Reference: (*JobNumber*)
  Email Sender: (*SenderStaffName*) ((*SenderStaffCode*)) <(*SenderEmailAddress*)>

  NDR Subject: (*NonDeliveryReceiptEmailSubject*)
  NDR Reason Code: (*BouncedReasonCode*)
";

					var registryItem = new NotificationEmailTemplateRegistryItem(
						"NonDeliveryReceiptNotificationEmailTemplate",
						Categories.System_Email,
						ResString.GetMultilingualString("860FEE8F-E388-493C-90EB-F4F9A41AB413", "Non-Delivery Receipt Notification Email Template"),
						ResString.GetMultilingualString("E5E08ED2-838E-48BA-8BCB-369059C635DD", "Configure the template for Non-Delivery Receipt notification emails."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(),
						defaultSubject,
						defaultBody);
					return registryItem;
				});
			}
		}

		public GuidRegistryItem NonDeliveryReceiptNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("NonDeliveryReceiptNotificationGroup", delegate
				{
					var result = new GuidRegistryItem(
						"NonDeliveryReceiptNotificationGroup",
						Categories.System_Email,
						ResString.GetMultilingualString("9DD00E3A-F7B8-481A-8160-1FC42104F95B", "Non-Delivery Receipt Notification Group"),
						ResString.GetMultilingualString("C355DDFC-8641-420A-890E-0A188C0DAE2C", "The staff group will be notified for NON-Delivery Receipt/s when a sender cannot be identified."),
						RegistryStorageFlags.System,
						Core.Constants.Groups.PostMastersGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region SystemLogBatchProcessHighWaterMark

		public DateTimeRegistryItem SystemLogBatchProcessHighWaterMark
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("SystemLogBatchProcessHighWaterMark", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
						"SystemLogBatchProcessHighWaterMark",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("dd2a5a9a-2753-4819-b0f8-7d54d648709b", "System Log Batch Process High Water Mark"),
						ResString.GetMultilingualString("9ee90ebe-bf95-4d3a-89f3-a9d2f58a88c9", "This is the Date & Time which will be used as the start Date & Time from which events will be read for generating Interface files. This should not need to be reset as resetting will affect the batch processor and potentially result in duplication of files generated if set to an earlier date."),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);
					return result;
				});
			}
		}

		#endregion

		#region RET Service Task HTTP Request Timeout

		public IntRegistryItem RETServiceTaskHttpTimeout
		{
			get
			{
				return GetItem("RETServiceTaskHttpTimeout", () =>
					new IntRegistryItem(
						"RETServiceTaskHttpTimeout",
						Categories.System_Database_UserOptions,
						ResString.GetMultilingualString("0DECAA57-F068-48E8-A1A3-5FD6CE54699E", "RET Service Task HTTP Request Timeout"),
						ResString.GetMultilingualString("49B3386D-1CC1-43E5-A8BF-35520F01D747", "Time in seconds representing timeout for HTTP requests to error reports web-service task."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						100,
						1,
						600)
					);
			}
		}

		#endregion

		#region Service Task Memory Constraint

		public IntRegistryItem ServiceTaskMemoryConstraint
		{
			get
			{
				return GetItem("ServiceTaskMemoryConstraint", () =>
					new IntRegistryItem(
						"ServiceTaskMemoryConstraint",
						Categories.System_ProcessController,
						ResString.GetMultilingualString("560A0219-5044-4FA6-A9B0-59A3A89720BF", "Service Task Memory Constraint (in megabytes)"),
						ResString.GetMultilingualString("E10473E4-3BFC-4657-AA0E-12C10650E278", "Memory constraint for each service task"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						0,
						1000,
						10000)
					);
			}
		}

		#endregion

		#region Dispatcher Throttling

		public TimeSpan ThrottlingPeriodForDispatchingLoopWithExceedingBacklog
		{
			get
			{
				return TimeSpan.FromMilliseconds(GetItem<IntRegistryItem>("ThrottlingPeriodForDispatchingLoopWithExceedingBacklogInMs", delegate
				{
					return new IntRegistryItem(
						"ThrottlingPeriodForDispatchingLoopWithExceedingBacklogInMs",
						Categories.System_ProcessController_DispatcherThrottling,
						ResString.GetMultilingualString("NF4A5EE4-4914-4396-9287-A8D3E2C5B923", "Throttling Period For Dispatching Loop With Exceeding Backlog In Ms"),
						ResString.GetMultilingualString("M89AE572-3C44-46A9-A5E4-F9014DCA7C0A", "A period (in milliseconds) the dispatching loop in Process Controller will wait for the next processing loop if the dispatching queue has number of items exceeding target value"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						30,
						0,
						1000
						);
				}).Value);
			}
		}

		public TimeSpan ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog
		{
			get
			{
				return TimeSpan.FromMilliseconds(GetItem<IntRegistryItem>("ThrottlingPeriodForDispatchingLoopWithAcceptableBacklogInMs", delegate
				{
					return new IntRegistryItem(
						"ThrottlingPeriodForDispatchingLoopWithAcceptableBacklogInMs",
						Categories.System_ProcessController_DispatcherThrottling,
						ResString.GetMultilingualString("CF4A5EE4-4914-4396-9287-A8D3E2C5B923", "Throttling Period For Dispatching Loop With Acceptable Backlog In Ms"),
						ResString.GetMultilingualString("F89AE572-3C44-46A9-A5E4-F9014DCA7C0A", "A period (in milliseconds) the dispatching loop in Process Controller will wait for the next processing loop if the dispatching queue has items within acceptable limits"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						100,
						0,
						10000
						);
				}).Value);
			}
		}

		public TimeSpan ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary
		{
			get
			{
				return TimeSpan.FromMilliseconds(GetItem<IntRegistryItem>("ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondaryInMs", delegate
				{
					return new IntRegistryItem(
						"ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondaryInMs",
						Categories.System_ProcessController_DispatcherThrottling,
						ResString.GetMultilingualString("AF4A5EE4-4914-4396-9287-A8D3E2C5B923", "Throttling Period For Dispatching Loop With Backlog Waiting For Max Secondary In Ms"),
						ResString.GetMultilingualString("B89AE572-3C44-46A9-A5E4-F9014DCA7C0A", "A period (in milliseconds) the dispatching loop in Process Controller will wait for the next processing loop if the dispatching queue has only items that reached max secondary and are waiting to run"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						1000,
						0,
						10000
						);
				}).Value);
			}
		}

		public TimeSpan ThrottlingTargetTimeToClearQueueBacklog
		{
			get
			{
				return TimeSpan.FromMilliseconds(GetItem<IntRegistryItem>("ThrottlingTargetTimeToClearQueueBacklogInMs", delegate
				{
					return new IntRegistryItem(
						"ThrottlingTargetTimeToClearQueueBacklogInMs",
						Categories.System_ProcessController_DispatcherThrottling,
						ResString.GetMultilingualString("YF4A5EE4-4914-4396-9287-A8D3E2C5B923", "Target Time To Clear Queue Backlog In Ms"),
						ResString.GetMultilingualString("X89AE572-3C44-46A9-A5E4-F9014DCA7C0A", "A period (in milliseconds) during which we expect to clear all backlog in dispatching queue. (Target Time To Clear Queue Backlog)/(Period For Dispatching Loop With Acceptable Backlog) used to calculate acceptable backlog"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						5000,
						1000,
						300000
						);
				}).Value);
			}
		}

		#endregion

		#region Certificate Store items

		public BooleanRegistryItem UseWindowsCertificateStore
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseWindowsCertificateStore_0910", () =>
				{
					return new BooleanRegistryItem(
						"UseWindowsCertificateStore_0910",
						Categories.System_Certificates,
						ResString.GetMultilingualString("a5d675c3-c720-41b4-a1ed-532c15cc53c8", "Use Windows Certificate Store"),
						ResString.GetMultilingualString("e29577e2-f94f-4334-bb00-ab660b97ee08", "Specifies whether or not {0} should use the Windows Certificate Store or the {0} Certificate Store to decrypt messages. Using the Windows Certificate Store allows multiple certificates to be used.", Constants.ProductName),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem RemoveCertificateFromUserStore
		{
			get
			{
				return GetItem<BooleanRegistryItem>("RemoveCertificateFromUserStore", () =>
				{
					return new BooleanRegistryItem(
						"RemoveCertificateFromUserStore",
						Categories.System_Certificates,
						(NoResString)"Remove Certificate From User Store",
						(NoResString)"The next time the AU Interchange Retriever or Sender executes any type 3 certificate stored in the Service Processor Logged In User store will be removed. An exception will then be thrown, and continue to be thrown until this registry item is reset.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem StoreCertificatesUnderCurrentUser
		{
			get
			{
				return GetItem<BooleanRegistryItem>("StoreCertificatesUnderCurrentUser_1304", () =>
				{
					return new BooleanRegistryItem(
						"StoreCertificatesUnderCurrentUser_1304",
						Categories.System_Certificates,
						(NoResString)"Store Digital Certificates Under the Current User",
						(NoResString)"Specifies whether Digital Certificates are stored under the local computer or under the current user. By default Digital Certificates will be stored under the current user. If this setting is overridden and changed to NO then Digital Certificates will be stored under the local computer where the task is run.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		bool ISystemDataRegistry.StoreCertificatesUnderCurrentUser
		{
			get { return StoreCertificatesUnderCurrentUser.Value; }
		}

		#endregion

		#endregion

		#region Message decrypt timeout

		public IntRegistryItem MessageDecryptTimeout
		{
			get
			{
				return GetItem("MessageDecryptTimeout", delegate
				{
					return new IntRegistryItem(
						"MessageDecryptTimeout",
						Categories.System_Messaging,
						ResString.GetMultilingualString("9532cb7d-5350-45b0-a8b3-b137edf8b888", "Message Decrypt Timeout Minutes"),
						ResString.GetMultilingualString("d3fcffe9-081c-480d-a916-62c1718bf996", "The maximum time (in minutes) to allow, when decrypting a message, before aborting the process."),
						RegistryStorageFlags.System,
						10);
				});
			}
		}

		#endregion

		#region Log Walker

		public BooleanRegistryItem LogWalkerEnabled
		{
			get
			{
				return GetItem<BooleanRegistryItem>("LogWalkerEnabled", delegate
				{
					return new BooleanRegistryItem(
						"LogWalkerEnabled",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("f5596677-a31b-491d-b9ab-9edf0b9d3c8c", "Enable Log Walker"),
						ResString.GetMultilingualString("edb01c44-4a3c-47e5-9f67-2c8a64b78e5b", "Activate Log Walker to listen to logged events and take actions upon them?"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public IntRegistryItem RetryAttemptsOnLogWalkerRecoverableErrors
		{
			get
			{
				return GetItem("RetryAttemptsOnLogWalkerRecoverableErrors", delegate
				{
					return new IntRegistryItem(
						name: "RetryAttemptsOnLogWalkerRecoverableErrors",
						category: Categories.System_LogWalker,
						caption: ResString.GetMultilingualString("56a699b5-ce28-4fb6-a642-cb6a7fbf941d", "Retry Attempts On Log Walker Recoverable Errors"),
						hint: ResString.GetMultilingualString("0cd24189-f8ef-4c26-9f12-bb383db2f5d5", "The number of retry attempts Log Walker will make in case of an error."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: 10,
						minValue: 10,
						maxValue: 10000);
				});
			}
		}

		public BooleanRegistryItem EnableLogWalkerForceSeekOnReadQueueQuery
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableLogWalkerForceSeekOnReadQueue", delegate
				{
					return new BooleanRegistryItem(
						"EnableLogWalkerForceSeekOnReadQueue",
						Categories.System_LogWalker,
						(NoResString)"Enable Log Walker Force-seek on Read Queue Query",
						(NoResString)"Enable Log Walker Force-seek and index hint on Read Queue Query",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public IntRegistryItem LogWalkerClearProcessedLogsAfterHours
		{
			get
			{
				return GetItem("LogWalkerClearProcessedLogsAfterHours", delegate
				{
					return new IntRegistryItem(
						"LogWalkerClearProcessedLogsAfterHours",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("ea3a25d4-f480-47cf-83c2-01e6cb7a2d84", "Clear Processed Log Walker Logs"),
						ResString.GetMultilingualString("56467c9b-5df0-48c2-86d3-f5e29659fb8e", "The duration for which processed logs will be kept by the system in hours."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						0, 0, 1000);
				});
			}
		}

		public IntRegistryItem LogWalkerClearFailedLogsAfterHours
		{
			get
			{
				return GetItem("LogWalkerClearFailedLogsAfterHours", delegate
				{
					return new IntRegistryItem(
						"LogWalkerClearFailedLogsAfterHours",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("b24af1b4-83f0-4eca-8ec5-5531f20ddbf8", "Clear Failed Log Walker Logs"),
						ResString.GetMultilingualString("81113d87-7c39-469f-8527-7bc9728ca730", "The duration for which failed logs will be kept by the system in hours."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						24 * 7, 1, 1000);
				});
			}
		}

		public IntRegistryItem LogWalkerBatchSize
		{
			get
			{
				return GetItem("LogWalkerBatchSize", delegate
				{
					return new IntRegistryItem(
						"LogWalkerBatchSize",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("3818baf7-8248-4e92-8120-64b4a9a0f587", "Log Walker Batch Size"),
						ResString.GetMultilingualString("502b22ae-a30d-4e6c-85f3-d4504bde3716", "The maximum number of logs to be processed by the Log Walker in each batch."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						10, 1, 1000);
				});
			}
		}

		public IntRegistryItem LogWalkerPurgeBatchSize
		{
			get
			{
				return GetItem("LogWalkerPurgeBatchSize", delegate
				{
					return new IntRegistryItem("LogWalkerPurgeBatchSize",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("c766a2f1-b1fd-4f17-ba6a-2c2476240594", "Log Walker Purge Batch Size"),
						ResString.GetMultilingualString("b2450a4f-470d-4fc1-960f-86d203875c0e", "The maximum number of logs to be removed from the system by Log Walker Purge in each batch."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						1000, 1, 50000);
				});
			}
		}

#if DEBUG
		public
#endif
		class IntWithZeroRegistryDataType : IntRegistryDataType
		{
			public IntWithZeroRegistryDataType(int lowerBound, int upperBound) : this(lowerBound, upperBound, false) { }

			public IntWithZeroRegistryDataType(int lowerBound, int upperBound, bool allowZero)
				: base(lowerBound, upperBound)
			{
				this.allowZero = allowZero;
			}

			readonly bool allowZero;

			protected override void ValidateCore(IRegistryItem registryItem, int proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				if ((proposedValue < LowerBound || proposedValue > UpperBound) && (proposedValue != 0 || !allowZero))
				{
					string message;
					if (UpperBound < int.MaxValue)
					{
						message = Res.GetString("95f792e8-1d24-4770-b1bf-ef2d0f5e2195", "The value should be between {0} and {1}", LowerBound, UpperBound);
					}
					else
					{
						message = Res.GetString("a745082c-55a1-4b81-a2e4-3ef2927c521e", "The value should be no less then {0}", LowerBound);
					}
					if (allowZero && LowerBound > 0)
					{
						message += ", " + Res.GetString("89d8392d-53a2-4a39-baf5-2c4375340b7d", "or have a 0 as a special value");
					}
					message += ".";
					throw new RegistryValidationException(message);
				}
			}
		}

		public BooleanRegistryItem LogSubscriberReportResourcesUsage
		{
			get
			{
				return GetItem(
					"LogSubscriberReportResourcesUsage",
					delegate
					{
						return new BooleanRegistryItem(
							"LogSubscriberReportResourcesUsage",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("9f7d8bab-d553-477c-8916-b6e24ed5721a", "Report Excessive Resources Usage"),
							ResString.GetMultilingualString("ad383fd8-fad0-4d9a-9a1e-6e6fcfb66ef4", "If true, there will be reports added about excessive memory and time consumption into Event Log Walker logs."),
							RegistryStorageFlags.System,
							false);
					});
			}
		}

		public BooleanRegistryItem LogSubscriberUseSharedTransaction
		{
			get
			{
				return GetItem(
					"LogSubscriberUseSharedTransaction",
					delegate
					{
						return new BooleanRegistryItem(
							"LogSubscriberUseSharedTransaction",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("d4ff9776-605b-4712-9846-ea7324d50b72", "Guarantee transactional isolation"),
							ResString.GetMultilingualString("686d02d1-1cc8-4169-af74-a647e05b1bb4", "Ensure that there is only one transaction per Log Walker batch."),
							RegistryStorageFlags.All,
							RegistryOptions.IsOnlyEditableBySupportIfHosted,
							true);
					});
			}
		}

		public IntRegistryItem LogSubscriberSlowLogBatchProcessingThresholdInSeconds
		{
			get
			{
				return GetItem(
					"LogSubscriberSlowLogBatchProcessingThresholdInSeconds",
					delegate
					{
						return new IntRegistryItem(
							"LogSubscriberSlowLogBatchProcessingThresholdInSeconds",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("bd43c949-657e-475a-ac5f-6d481c5e9547", "Processing time limit in seconds per Subscriber (batch)"),
							ResString.GetMultilingualString("8084381f-35ba-416d-8a52-95e263133edc", "Limit in seconds for a Subscriber to process a batch of logs before it is reported as slow. 0 = no limit."),
							RegistryStorageFlags.System,
							60)
						{ DataType = new IntWithZeroRegistryDataType(20, int.MaxValue, true) };
					});
			}
		}

		public IntRegistryItem LogSubscriberSlowLogProcessingThresholdInSeconds
		{
			get
			{
				return GetItem(
					"LogSubscriberSlowLogProcessingThresholdInSeconds",
					delegate
					{
						return new IntRegistryItem(
							"LogSubscriberSlowLogProcessingThresholdInSeconds",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("24ecdcd7-ab83-4fb2-b157-7c2c61a04c96", "Processing time limit in seconds per Subscriber (individual log)"),
							ResString.GetMultilingualString("7aec2523-ef41-43fa-a19c-241dcda8d605", "Limit in seconds for a Subscriber to process a log before it is reported as slow. 0 = no limit."),
							RegistryStorageFlags.System,
							10)
						{ DataType = new IntWithZeroRegistryDataType(1, int.MaxValue, true) };
					});
			}
		}

		public IntRegistryItem LogSubscriberMemoryLeakThresholdInBytes
		{
			get
			{
				return GetItem(
					"LogSubscriberMemoryLeakThresholdInBytes",
					delegate
					{
						return new IntRegistryItem(
							"LogSubscriberMemoryLeakThresholdInBytes",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("83a2a270-6d18-4d8b-ab1d-f51e43464435", "Memory leak reporting threshold in bytes per Subscriber"),
							ResString.GetMultilingualString("c9833b1c-3b04-4d50-829b-f77b1d0c1687", "Maximum allowed memory amount to be consumed and not released per Log Subscriber (in bytes). 0 = no limit."),
							RegistryStorageFlags.System,
							15000)
						{ DataType = new IntWithZeroRegistryDataType(0, int.MaxValue, true) };
					});
			}
		}

		public IntRegistryItem SecondsUntilLogWalkerStopsAllocatingLogBatchesToASubscriber
		{
			get
			{
				return GetItem(
					"SecondsUntilLogWalkerStopsAllocatingLogBatchesToASubscriber",
					delegate
					{
						return new IntRegistryItem(
							"SecondsUntilLogWalkerStopsAllocatingLogBatchesToASubscriber",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("aa4f4a23-8d0c-4eed-a5db-ca8c96e09f60", "Seconds until LogWalker stops allocating Log Batches to a Subscriber"),
							ResString.GetMultilingualString("43ae79c2-2be4-4f4f-a592-6769148b33cd", "Number of seconds until Log Walker stops allocating more log batches to a subscriber, and moves on to the next one."),
							RegistryStorageFlags.System,
							20) // 20 seconds is an acceptable value, as this will occur multiple times before the service yields.
						{ DataType = new IntWithZeroRegistryDataType(1, 10 * 60, false) };
					});
			}
		}

		public IntRegistryItem SecondsUntilLogWalkerEnds
		{
			get
			{
				return GetItem(
					"SecondsUntilLogWalkerEnds",
					delegate
					{
						return new IntRegistryItem(
							"SecondsUntilLogWalkerEnds",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("ac08b255-b61d-4f7b-a2ac-60d2b9e51c05", "Seconds until LogWalker runner yields"),
							ResString.GetMultilingualString("21a2447c-991f-4392-89f7-f07c809b96f9", "Number of seconds until a LogWalker runner will stop completely in order to reclaim memory."),
							RegistryStorageFlags.System,
							60 * 30) // Normal is 30 minutes before yielding.
						{ DataType = new IntWithZeroRegistryDataType(20, 60 * 60 * 8, false) }; // Minimum is 10 second run time, just in case we are panicked for some reason and need to shut it down.
					});
			}
		}

		public BooleanRegistryItem LogWalkerRecursOnNewEvents
		{
			get
			{
				return GetItem<BooleanRegistryItem>("LogWalkerRecursOnNewEvents", delegate
				{
					return new BooleanRegistryItem(
						"LogWalkerRecursOnNewEvents",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("4887be45-79f8-4c3d-b290-b8a28b686b62", "Enable Log Recursion"),
						ResString.GetMultilingualString("284ffda6-2ae7-49b8-85bb-2a3c2d9a1d92", "Log Walker will immediately process events created within Log Walker. (Rather than queuing events and processing them at a later time.)"),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem LogWalkerEventMappingTable
		{
			get
			{
				return GetItem<BooleanRegistryItem>("LogWalkerEventMappingTable", delegate
				{
					return new BooleanRegistryItem(
						"LogWalkerEventMappingTable",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("1E64F982-BCC4-49ED-B37F-236D838F66E9", "Event Mapping Table"),
						ResString.GetMultilingualString("7ACAFBEE-D534-43A5-A10F-A81B816E07C7", "When disabled Log Walker will create an event mapping table each time it runs instead of using the mapping table persisted in the database. This feature is used to reduce usage of temporary tables."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		public IntRegistryItem LogWalkerMaxEventRecursion
		{
			get
			{
				return GetItem(
					"LogWalkerMaxEventRecursion",
					delegate
					{
						return new IntRegistryItem(
							"LogWalkerMaxEventRecursion",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("cc81e12f-180b-4f13-8f69-1f0c134501ea", "Log Walker Recursion Limit"),
							ResString.GetMultilingualString("d0edcb01-b8db-4fa7-be7a-8c4f0ad2dbcb", "Maximum number of times Log Walker will recur on events raised during service task execution."),
							RegistryStorageFlags.All,
							5)
						{ DataType = new IntWithZeroRegistryDataType(0, int.MaxValue, false) };
					});
			}
		}

		public CodeDescriptionBoolDisallowNewRegistryItem LogWalkerLogging
		{
			get
			{
				return GetItem("LogWalkerLogging", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem("LogWalkerLogging",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("AA970758-6C1D-4784-96AC-526C02FBB0BF", "Logging Options"),
						ResString.GetMultilingualString("B7C71B6D-C3A6-4ECC-B99B-A000A5A9572F", "Configure logging categories for Log Walker diagnostics."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("821071E1-A65D-4155-9CA3-1E0B1F33F4D8", "Enable Logs"), true, true),
						new CodeDescriptionBoolDisallowNewCollection()
						{
							{ LogWalkerLoggingKeys.EnvironmentLogging, ResString.GetMultilingualString("7C65CF32-0AEC-4488-97FE-2B19F5F8963C", "Verbose environment logging"), false },
							{ LogWalkerLoggingKeys.StmJobQueueReport, ResString.GetMultilingualString("1780A81D-5085-4147-BC46-75A3C6A43402", "Queue backlog reporting"), false },
							{ LogWalkerLoggingKeys.ConcurrencyErrorReport, ResString.GetMultilingualString("43623366-19FA-4AB6-BE5F-96E7299947F7", "Concurrency error reporting"), false },
						});
				});
			}
		}

		public static class LogWalkerLoggingKeys
		{
			public const string EnvironmentLogging = "ENV";
			public const string StmJobQueueReport = "SJR";
			public const string ConcurrencyErrorReport = "CER";
		}

		public IntRegistryItem LogWalkerStmJobQueueReportLogInterval
		{
			get
			{
				return GetItem(
					"LogWalkerStmJobQueueReportLogInterval",
					delegate
					{
						return new IntRegistryItem(
							"LogWalkerStmJobQueueReportLogInterval",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("7A289EFA-22B9-4C5F-A556-5C1226C50E8A", "Log Walker queue reporting interval in minutes"),
							ResString.GetMultilingualString("F1CD74D1-4902-4F78-986C-F281DADBE949", "Interval between reports of the Log Walker queue when logging for diagnostics."),
							RegistryStorageFlags.System,
							20)
						{ DataType = new IntWithZeroRegistryDataType(0, int.MaxValue, false) };
					});
			}
		}

		public IntRegistryItem LogWalkerDelayLogTime
		{
			get
			{
				return GetItem(
					"LogWalkerDelayLogTime",
					delegate
					{
						return new IntRegistryItem(
							"LogWalkerDelayLogTime",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("B2DCEB04-AFDF-4B0F-850A-532F1D989E30", "Log delay before queue reporting in minutes"),
							ResString.GetMultilingualString("BAFD7BEE-B53F-4EA5-8604-9AD517D55105", "The time in minutes before creating periodic reports of the Log Walker queue."),
							RegistryStorageFlags.System,
							5)
						{ DataType = new IntWithZeroRegistryDataType(0, int.MaxValue, false) };
					});
			}
		}

		public IntRegistryItem LogWalkerStmJobQueueReportMaxRows
		{
			get
			{
				return GetItem(
					"LogWalkerStmJobQueueReportMaxRows",
					delegate
					{
						return new IntRegistryItem(
							"LogWalkerStmJobQueueReportMaxRows",
							Categories.System_LogWalker,
							ResString.GetMultilingualString("E2EE5930-E4A6-4F56-B7F6-8306AED920C1", "Maximum Log Walker queue rows to log"),
							ResString.GetMultilingualString("80918CCE-241A-44AA-8675-CD3454046821", "The maximum number of Log Walker queue rows to log when generating reports for diagnostics."),
							RegistryStorageFlags.System,
							5000)
						{ DataType = new IntWithZeroRegistryDataType(0, int.MaxValue, false) };
					});
			}
		}

		public IntRegistryItem LogWalkerDelayDurationInCreatingLogQueueItems
		{
			get
			{
				return GetItem("LogWalkerDelayDurationInCreatingLogQueueItems", delegate
				{
					return new IntRegistryItem(
						"LogWalkerDelayDurationInCreatingLogQueueItems",
						Categories.System_LogWalker,
						ResString.GetMultilingualString("78BEDF38-D7A7-436E-8685-51B117AFC55A", "Delay Duration in Creating New Log Items in Milliseconds"),
						ResString.GetMultilingualString("52C6A5A0-1E55-4FB6-9964-9AF2921C76FF", "The delay duration in milliseconds while creating new log items for the next processing loop in LWM."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						500, 0, 10000);
				});
			}
		}

		public CodeDescriptionBoolDisallowNewRegistryItem WorkflowEventTriggerProccessorLogging
		{
			get
			{
				return GetItem("WorkflowEventTriggerLogging", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem("WorkflowEventTriggerLogging",
						Categories.System_LogWalker_WorkflowTriggerEventProcessor,
						ResString.GetMultilingualString("1CC5C465-6C2E-46C9-A1DD-40A14FDC8FA6", "Logging Options"),
						ResString.GetMultilingualString("93607713-1E82-455E-B91A-D2685A7439E3", "Configure logging categories for the Workflow Event Trigger Processor."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("821071E1-A65D-4155-9CA3-1E0B1F33F4D8", "Enable Logs"), true, true),
						new CodeDescriptionBoolDisallowNewCollection()
						{
							{ WorkflowEventTriggerLoggingKeys.DelayedLog, ResString.GetMultilingualString("D63EAF29-10F6-4634-92C6-D024FF146530", "Delayed items"), false },
						});
				});
			}
		}

		public static class WorkflowEventTriggerLoggingKeys
		{
			public const string DelayedLog = "DLY";
		}

		public IntRegistryItem WorkflowEventTriggerProccessorDelayLogTime
		{
			get
			{
				return GetItem(
					"WorkflowEventTriggerProccessorDelayLogTime",
					delegate
					{
						return new IntRegistryItem(
							"WorkflowEventTriggerProccessorDelayLogTime",
							Categories.System_LogWalker_WorkflowTriggerEventProcessor,
							ResString.GetMultilingualString("F7D70E9A-6433-415D-A75C-F376C92D25F7", "Minutes before logging delayed WTE logs"),
							ResString.GetMultilingualString("FDF1C603-1C5A-4D23-AE52-6C4A2F3B1F3A", "The time in minutes before logging delayed WTE logs when delay logging enabled."),
							RegistryStorageFlags.System,
							5)
						{ DataType = new IntWithZeroRegistryDataType(0, int.MaxValue, false) };
					});
			}
		}

		#endregion

		#region Data Export Settings

		#region ExportEDICodeMapping

		public BooleanRegistryItem ExportEDICodeMapping
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ExportEDICodeMappings", delegate
				{
					return new BooleanRegistryItem(
						"ExportEDICodeMappings",
						Categories.System_DataExportSettings_Organization,
						ResString.GetMultilingualString("0b15f5df-e8bf-4c66-a9c6-6d12d0dddab1", "Export EDI Code Mappings"),
						ResString.GetMultilingualString("83d975b3-2bb0-49ed-bf9e-79e7644f661f", "Set this to 'Yes' if you want the EDI Code Mappings to be exported when exporting an Organization to legacy XML"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Include ARInvoices When Exporting Consol or Shipment In XML

		public BooleanRegistryItem IncludeConsolOrShipmentARInvoices
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeConsolShipmentARInvoices", delegate
				{
					return new BooleanRegistryItem(
					"IncludeConsolShipmentARInvoices",
					Categories.System_DataExportSettings,
					ResString.GetMultilingualString("95c1ca07-1414-443d-bf71-996548b2b07f", "Include AR Invoices when Exporting Consol/Shipment XML"),
					ResString.GetMultilingualString("e98deeff-c6be-4940-9d5f-07b3345ee5b4", "Set this to 'Yes' to export AR Invoices data."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
					false);
				});
			}
		}

		#endregion

		#region Accounting Transactions

		public IRegistryItem AccountingTransactionsExport
		{
			get
			{
				return GetItem("AccountingTransactionsExport", delegate
				{
					return (IRegistryItem)new DataTransferSwitchRegistryItem(
						"AccountingTransactionsExport",
						Categories.System_DataExportSettings,
						ResString.GetMultilingualString("29f41dea-bfb3-4e12-9451-4e0536051b1c", "Accounting Transactions"),
						ResString.GetMultilingualString("3f10c3a3-3702-4e4d-8130-d29d5774123a", "Automatic Export of Accounting Transactions in Standard XML-Format (The next time to run is shown as local time for a Branch with the earliest Time Zone)"),
						RegistryStorageFlags.Company,
						RegistryOptions.NotCached
						);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public DateTimeRegistryItem AccountingTransactionsExportHighWaterMark
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("AccountingTransactionsExportHighWaterMark", delegate
				{
					var result = new DateTimeRegistryItem(
						"AccountingTransactionsExportHighWaterMark",
						Categories.System_DataExportSettings,
						(NoResString)"Accounting Transactions Export High Water Mark",
						(NoResString)"To aid performance of the Accounting Transactions Export, the system will only search for un-batched transactions that were created or edited since this date.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long), RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, DateTime.MinValue, false);
					result.DataType = new AccountingTransactionExportHighWaterMarkDataType();
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Accounting Transaction Types

		public CodeDescriptionBoolRegistryItem AccountingTransactionTypes
		{
			get
			{
				return GetItem("AccountingTransactionTypes", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"AccountingTransactionTypes",
						Categories.System_DataExportSettings,
						ResString.GetMultilingualString("1c803e7e-7364-4b66-8ada-caaac553800b", "Accounting Transaction Types"),
						ResString.GetMultilingualString("9FB82A96-0869-4E33-BB96-0F761BBBC8F8", "Use this Registry setting to determine which transaction types are exported automatically by the data export. To aid performance of the export, enabling a transaction type will only take effect for transactions created or edited since the last export."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("8c2a8ce7-b6ad-428d-9653-67148aac1a16", "Export"), true, true),
						new CodeDescriptionBoolCollection {
							{ ExportTransactionTypes.ARInvoice, ResString.GetMultilingualString("993a709d-92a6-4e37-9381-d668f1ff6db4", "AR Invoice"), true },
							{ ExportTransactionTypes.APInvoice, ResString.GetMultilingualString("0c6281d6-5712-4938-b0bf-f696d6a43adb", "AP Invoice"), true },
							{ ExportTransactionTypes.ARCreditNote, ResString.GetMultilingualString("3d41a100-6d77-4df9-871d-83770979541e", "AR Credit Note"), true },
							{ ExportTransactionTypes.APCreditNote, ResString.GetMultilingualString("9c291652-2972-422b-9ef5-151ea965f491", "AP Credit Note"), true },
							{ ExportTransactionTypes.ARAdjustmentNote, ResString.GetMultilingualString("4cf5de9d-06f4-4a6e-8769-1350158cae73", "AR Adjustment Note"), true },
							{ ExportTransactionTypes.APAdjustmentNote, ResString.GetMultilingualString("0fdcc7c5-3d8d-4f8c-8dd1-bdc95219bc66", "AP Adjustment Note"), true },
							{ ExportTransactionTypes.WIPPosting, ResString.GetMultilingualString("62d23f6c-fb9d-4a08-844c-91db46110dd1", "WIP Posting"), true },
							{ ExportTransactionTypes.AccrualPosting, ResString.GetMultilingualString("d677b297-2014-4338-900d-ed2b546112fe", "Accrual Posting"), true },
							{ ExportTransactionTypes.WIPReversal, ResString.GetMultilingualString("ce4d9e84-b6e0-4186-9760-890a64e44cd3", "WIP Reversal"), true },
							{ ExportTransactionTypes.AccrualReversal, ResString.GetMultilingualString("d484bfca-3f42-43ae-a6d5-7361a9f0600f", "Accrual Reversal"), true },
							{ ExportTransactionTypes.UnallocatedAPInvoices, ResString.GetMultilingualString("a80ffe6f-57e7-4d82-8f7b-e6e80cd4ee82", "Unallocated AP Invoices"), false },
							{ ExportTransactionTypes.UnallocatedAPCreditNotes, ResString.GetMultilingualString("e42796e0-c8c2-44f4-81a0-b45015a56e9c", "Unallocated AP Credit Notes"), false }
						});
				});
			}
		}

		#endregion

		#region Invoices in PDF format

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem ShowInvoicePDFExportRegistrySettingsRaw
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShowInvoicePDFExportRegistrySettings", delegate
				{
					return new BooleanRegistryItem(
						"ShowInvoicePDFExportRegistrySettings",
						Categories.System_DataExportSettings_InvoicesinPDFformat,
						(NoResString)string.Format(CultureInfo.CurrentCulture, "Show Invoice PDF Export Registry Settings ({0} Only)", Constants.ProductSupportName),
						(NoResString)"Show Invoice PDF Export Registry Settings: Invoice Types, Export Directory",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		public bool ShowInvoicePDFExportRegistrySettings
		{
			get
			{
				return ShowInvoicePDFExportRegistrySettingsRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		public CodeDescriptionBoolRegistryItem InvoiceTypesToExportInPDFFormat
		{
			get
			{
				return GetItem("InvoiceTypesToExportInPDFFormat", delegate
				{
					return new CodeDescriptionBoolRegistryItem(
						"InvoiceTypesToExportInPDFFormat",
						Categories.System_DataExportSettings_InvoicesinPDFformat,
						ResString.GetMultilingualString("f3c7eee8-9ede-4090-ae25-b5ed5e878d4f", "Invoice Types"),
						ResString.GetMultilingualString("aa46f79c-6343-48da-ba95-9ed612426aa9", "Use this Registry setting to determine which invoice types are exported automatically by the data export."),
						RegistryStorageFlags.Company,
						ShowInvoicePDFExportRegistrySettings ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("8d59fdb6-578e-442f-baa5-dbe951fcd5ff", "Export"), true, true),
						new CodeDescriptionBoolCollection {
							{ ExportInvoiceTypesInPDFFormat.ConsolInvoice, ResString.GetMultilingualString("5b612103-e32f-4897-b783-246d8d84e070", "Consol Invoice"), true },
							{ ExportInvoiceTypesInPDFFormat.ShipmentInvoice, ResString.GetMultilingualString("8c2bd3db-d666-4e99-b30d-4dff36b50637", "Shipment Invoice"), true }
						});
				});
			}
		}

		public StringRegistryItem InvoicesInPDFFormatExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("InvoicesInPDFFormatExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"InvoicesInPDFFormatExportDirectory",
						Categories.System_DataExportSettings_InvoicesinPDFformat,
						ResString.GetMultilingualString("b3596688-8bad-4ea7-b761-5163e373674d", "Export Directory"),
						ResString.GetMultilingualString("7aaba363-9e21-4b48-9584-c44761dcc7cb", "The folder specified here should contain any invoice PDF file to be exported."),
						RegistryStorageFlags.Company,
						ShowInvoicePDFExportRegistrySettings ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region Export GL Transactions to csv

		public BooleanRegistryItem EnableAutomaticGLTransactionsCSVExport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableGLTransactionsCSVExport", delegate
				{
					return new BooleanRegistryItem(
						"EnableGLTransactionsCSVExport",
						Categories.System_DataExportSettings_ExportGLTransactionstoCSV,
						ResString.GetMultilingualString("59A73CD0-51FA-4934-A86D-5EBE002D75BD", "Enable Automatic GL Transactions Export to CSV"),
						ResString.GetMultilingualString("0CDF397B-2845-4de9-866D-A8DC786183AD", "Enable this option to activate the Automatic GL Transactions Export to CSV file."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public StringRegistryItem GLTransactionsCSVExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("GLTransactionCSVExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"GLTransactionCSVExportDirectory",
						Categories.System_DataExportSettings_ExportGLTransactionstoCSV,
						ResString.GetMultilingualString("3DD5D27C-107B-4fc8-AF47-9047D1901CD9", "Export Directory"),
						ResString.GetMultilingualString("A0098DBB-963A-41a0-B95C-1CD42E55D2F2", "The folder specified here should contain any GL Transactions CSV file to be exported."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public DateTimeRegistryItem GLTransactionsCSVExportHighWaterMark
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("GLTransactionsCSVExportHighWaterMark", delegate
				{
					var result = new DateTimeRegistryItem(
						"GLTransactionsCSVExportHighWaterMark",
						Categories.System_DataExportSettings_ExportGLTransactionstoCSV,
						(NoResString)"GL Transactions CSV Export High Water Mark",
						(NoResString)"To aid performance of the GL Transactions CSV Export, the system will only search for un-batched transactions that were created or edited since this date.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport, DateTime.MinValue, false);
					result.DataType = new AccountingTransactionExportHighWaterMarkDataType();
					return result;
				});
			}
		}

		#endregion

		public GuidRegistryItem GLTransCSVExportNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("GLTransCSVExportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"GLTransCSVExportNotificationGroup",
						Categories.System_DataExportSettings_ExportGLTransactionstoCSV,
						ResString.GetMultilingualString("969965EE-82F2-423d-AD97-B75F96306E16", "Notification Group"),
						ResString.GetMultilingualString("A366FFA6-0942-4324-8EDF-5932FD98AB5E", "The staff group will be notified about the result of GL Transactions CSV file export."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Shipping Bill of Lading Export

		public CodePairRegistryItem IncludeBillingInfoInAgencyXMLFile
		{
			get
			{
				return GetItem<CodePairRegistryItem>("IncludeBillingInfoInAgencyXMLFile", delegate
				{
					return new CodePairRegistryItem(
						"IncludeBillingInfoInAgencyXMLFile",
						Categories.System_DataExportSettings_ShippingBillofLading,
						ResString.GetMultilingualString("637ac07d-8096-4151-98c5-f0f3d7043ca8", "Include Billing Information in XML File"),
						ResString.GetMultilingualString("dd64ece8-e08c-4775-ad38-c8653be4dd93", "Include charges into the interface file"),
						new CodeDescriptionPairListProvider(() => GetIncludeBillingInfoInAgencyXMLFileList()),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetIncludeBillingInfoInAgencyXMLFileList().DefaultCode);
				});
			}
		}

		#region Implementation

		internal CodeDescriptionPairList GetIncludeBillingInfoInAgencyXMLFileList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(Constants.IncludeBillingInfoInXMLMethod.Codes.NotInclude, Constants.IncludeBillingInfoInXMLMethod.Descriptions.NotInclude);
			result.AddPair(Constants.IncludeBillingInfoInXMLMethod.Codes.All, Constants.IncludeBillingInfoInXMLMethod.Descriptions.All);
			result.AddPair(Constants.IncludeBillingInfoInXMLMethod.Codes.PrincipalOnly, Constants.IncludeBillingInfoInXMLMethod.Descriptions.PrincipalOnly);
			result.DefaultCode = Constants.IncludeBillingInfoInXMLMethod.Codes.All;
			return result;
		}

		#endregion

		#endregion

		#region Include Billing info In Shipment XML

		public BooleanRegistryItem IncludeBillingInfoInShipmentXML
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeBillingInfoInShipmentXML", delegate
				{
					return new BooleanRegistryItem(
						"IncludeBillingInfoInShipmentXML",
						Categories.System_DataExportSettings_ShipmentExport,
						ResString.GetMultilingualString("ce9a8415-82b8-4fa1-bf03-9f6b2b256b1c", "Include Billing Information in XML File"),
						ResString.GetMultilingualString("8794a263-fbb3-4b1e-abbd-fbb180989951", "Include all costs and charges from the operations job's billing tab in the XML. Warning: By turning this on, anyone who receives your XML files will be able to see all charges and costs on this job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Include Transport Booking In Shipment XML
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem IncludeTransportBookingInShipmentXML
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeTransportBookingInShipmentXML", delegate
				{
					return new BooleanRegistryItem(
						"IncludeTransportBookingInShipmentXML",
						Categories.System_DataExportSettings_ShipmentExport,
						(NoResString)"Include Transport Booking in Shipment XML File",
						(NoResString)"Universal shipment exported from a forwarding shipment can optionally include transport bookings.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion
		#endregion

		#region Include Billing info In Warehouse XML

		public BooleanRegistryItem IncludeBillingInfoInWarehouseXML
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeBillingInfoInWarehouseXML", delegate
				{
					return new BooleanRegistryItem(
						"IncludeBillingInfoInWarehouseXML",
						Categories.System_DataExportSettings_WarehouseExport,
						ResString.GetMultilingualString("caf5bff5-0417-4659-84c9-8e7ea9f22f5b", "Include Billing Information in XML File"),
						ResString.GetMultilingualString("683665af-7c13-4e8c-b154-fad7d51ee3e3", "Include all costs and charges from the warehouse job's billing tab in the XML. Warning: By turning this on, anyone who receives your XML files will be able to see all charges and costs on this job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Data Import Settings

		#region Update AP AR Account Balances Process

		public StringRegistryItem ARAPBalancesUpdateImportDirectoryItem
		{
			get
			{
				return GetItem<StringRegistryItem>("ARAPBalancesUpdateImportDirectoryItem", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ARAPBalancesUpdateImportDirectoryItem",
						Categories.System_DataImportSettings_UpdateAccountBalances,
						ResString.GetMultilingualString("1cc10e33-745e-43f1-82d7-6acc25636224", "Update AP AR Account Balances Import Directory"),
						ResString.GetMultilingualString("96b6252d-78d1-4cbe-af65-fdb6b2670df1", "Directory to be used by automatic organization balance updates import"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public CodePairRegistryItem OrganisationMatchnigTypeItem
		{
			get
			{
				return GetItem<CodePairRegistryItem>("OrganisationMatchnigTypeItem", delegate
				{
					return new CodePairRegistryItem(
						"OrganisationMatchnigTypeItem",
						Categories.System_DataImportSettings_UpdateAccountBalances,
						ResString.GetMultilingualString("41430e06-980d-4e4c-a308-47b6516e2528", "Organization Matching Type"),
						ResString.GetMultilingualString("59A06ECA-CB62-4697-B583-149DE2873BDB", "This setting only applies to Legacy XML which is imported automatically. Select LEG if the organization codes in the XML are foreign, select ENT if the organization codes in the XML are the same as the organization codes in this {0} database.", "CargoWise"),
						OrganisationMatchingListProvider,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						OrganisationMatchingList.DefaultCode);
				});
			}
		}

		#endregion

		#region Organisations

		public StringRegistryItem OrganisationDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("OrganisationDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"OrganisationDataImportDirectory",
						Categories.System_DataImportSettings_Organizations,
						ResString.GetMultilingualString("8dddffbe-f88d-40d4-a260-4242e7f5147f", "Folder to scan for Organization XML files"),
						ResString.GetMultilingualString("0cff9728-6a35-4475-9961-a1c71d2317fe", "The folder specified here should contain any Organization XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		internal static class OrganisationImportMatchingCodes
		{
			public const string MatchByLegacyCode = "LEG";
			public const string MatchByOrganisationCode = "ENT";
		}

		ICodeDescriptionPairListProvider OrganisationMatchingListProvider
		{
			get
			{
				if (organisationMatchingListProvider == null)
				{
					organisationMatchingListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(OrganisationImportMatchingCodes.MatchByLegacyCode, ResString.GetMultilingualString("3bea7f0c-1ce1-493e-8f0b-bb45cbf85000", "Matching by Legacy Code"));
						list.AddPair(OrganisationImportMatchingCodes.MatchByOrganisationCode, ResString.GetMultilingualString("ad2f8f6d-6534-4330-92b4-4d174d0ce7f3", "Matching by {0} Code", Core.Constants.ProductName));
						list.DefaultCode = OrganisationImportMatchingCodes.MatchByLegacyCode;
						return list;
					});
				}

				return organisationMatchingListProvider;
			}
		}
		ICodeDescriptionPairListProvider organisationMatchingListProvider;

		internal CodeDescriptionPairList OrganisationMatchingList
		{
			get
			{
				if (organisationMatchingList == null)
				{
					organisationMatchingList = OrganisationMatchingListProvider.CodeDescriptionPairList;
				}

				return organisationMatchingList;
			}
		}
		CodeDescriptionPairList organisationMatchingList;

		internal CodePairRegistryItem OrganisationMatchingItem
		{
			get
			{
				return GetItem<CodePairRegistryItem>("OrganisationMatchingItem", delegate
				{
					return new CodePairRegistryItem("OrganisationMatchingItem",
						Categories.System_DataImportSettings_Organizations,
						ResString.GetMultilingualString("9a5b824a-e6c8-4708-bb0a-58075c22ba12", "Organization Matching"),
						ResString.GetMultilingualString("59A06ECA-CB62-4697-B583-149DE2873BDB", "This setting only applies to Legacy XML which is imported automatically. Select LEG if the organization codes in the XML are foreign, select ENT if the organization codes in the XML are the same as the organization codes in this {0} database.", "CargoWise"),
						OrganisationMatchingListProvider,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						OrganisationMatchingList.DefaultCode);
				});
			}
		}

		public OrganisationImportMatchingType OrganisationMatching
		{
			get
			{
				switch (OrganisationMatchingItem.Value)
				{
					case OrganisationImportMatchingCodes.MatchByLegacyCode:
						return OrganisationImportMatchingType.LegacyCodeMatching;
					case OrganisationImportMatchingCodes.MatchByOrganisationCode:
						return OrganisationImportMatchingType.OrganisationCodeMatching;
					default:
						return OrganisationImportMatchingType.Unknown;
				}
			}

			set
			{
				switch (value)
				{
					case OrganisationImportMatchingType.LegacyCodeMatching:
						OrganisationMatchingItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationImportMatchingCodes.MatchByLegacyCode);
						break;
					case OrganisationImportMatchingType.OrganisationCodeMatching:
						OrganisationMatchingItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationImportMatchingCodes.MatchByOrganisationCode);
						break;
				}
			}
		}

		#endregion

		#region XmlSchemaValidationStrict

		public BooleanRegistryItem XmlSchemaValidationStrict
		{
			get
			{
				return GetItem<BooleanRegistryItem>("XmlSchemaValidationStrict", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("49C96DBC-24E1-41c3-9E38-23F6DFCEC053",
								"Turn this option on to force XML elements and attributes that aren't defined in the current version of the schema to raise an error during XML import.\r\n" +
								"If you are developing your own XML file for import into {0}, it is recommended you turn this on during testing.\r\n" +
								"This may prevent XML files generated from other {0} installations with a version greater from being imported into your {0} system.", Constants.ProductName);

					return new BooleanRegistryItem(
						"XmlSchemaValidationStrict",
						Categories.System_DataImportSettings,
						ResString.GetMultilingualString("45ed8e4c-8280-43b8-838a-a2c6d4a23ca1", "Strict XML Schema Validation"),
						hint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region XmlImportSpecifiedElementsOnly

		public BooleanRegistryItem XmlImportSpecifiedElementsOnly
		{
			get
			{
				return GetItem<BooleanRegistryItem>("XmlImportSpecifiedElementsOnly", delegate
				{
					return new BooleanRegistryItem(
						"XmlImportSpecifiedElementsOnly",
						Categories.System_DataImportSettings,
						ResString.GetMultilingualString("b77f286f-5fd2-4f9f-b1c4-f9a0a9663bf6", "Only Import Specified XML Elements"),
						ResString.GetMultilingualString("8347390e-a039-4efb-8404-9a2ea6188292", "Turn this option OFF to allow blank values to be imported for non-specified XML elements.\r\nLeave it ON to prevent overwriting existing data with blank values from non-specified XML elements."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Shipment

		public ShipmentImportBranchRuleRegistryItem ShipmentImportBranchRules
		{
			get
			{
				return GetItem("ShipmentImportBranchRules", () =>
					new ShipmentImportBranchRuleRegistryItem(
						"ShipmentImportBranchRules",
						Categories.System_DataImportSettings_Shipment,
						ResString.GetMultilingualString("36a2834c-eba5-4868-88a5-726931f7263e", "Shipment Import Branch Rules"),
						ResString.GetMultilingualString("6baa4267-c215-4884-b6e2-762db9669570", @"Override these values to set the order of the default branch on creation of a new Shipment from the import XML if the Branch specified in the XML import file is not found or is to be discarded.
The values can be between 0 and 2. A value of 0 means the rule will not be used.
There can be multiple rules with a value of 0.
Any value greater than 1 must not be duplicated, i.e. 1, 1 is not valid.
There must not be a gap between the sequence of numbers for the values, i.e. 1, 2 is valid, but 0, 2 is not valid.
One of the fallback rules ""Default to a any"" or ""Do not create"" must be selected."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new ImportBranchRule()));
			}
		}

		public StringRegistryItem ShipmentDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ShipmentDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ShipmentDataImportDirectory",
						Categories.System_DataImportSettings_Shipment,
						ResString.GetMultilingualString("8d48b5e7-5614-4163-a514-4d75a864bda1", "Folder to scan for Shipment XML files"),
						ResString.GetMultilingualString("6d140405-78df-4f75-abc2-fda809ab968f",
							"The folder specified here should contain any Booking XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public CodePairRegistryItem ImportShipmentNoFromXml
		{
			get
			{
				return GetItem<CodePairRegistryItem>("ImportShipmentNoFromXml", delegate
				{
					return new CodePairRegistryItem(
						"ImportShipmentNoFromXml",
						Categories.System_DataImportSettings_Shipment,
						ResString.GetMultilingualString("385f0013-6684-4bde-ae46-8b5d9c2c8019", "Shipment Matching Criteria"),
						ResString.GetMultilingualString("8c65c2f9-c1b6-4240-86ea-30c597a48d2e", "By default the system will try to match import shipment HBL supplied in XML with an existing HBL and prompt the user whether to update the existing shipment. (NB. No prompting occurs during automatic imports). Override Default to choose another matching behavior."),
						new CodeDescriptionPairListProvider(() => ShipmentImportTypes()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						Constants.ShipmentNumberImportTypes.Code.HouseBill);
				});
			}
		}

		internal CodeDescriptionPairList ShipmentImportTypes()
		{
			CodeDescriptionPairList shipmentImportTypes = new CodeDescriptionPairList();
			shipmentImportTypes.AddPair(Constants.ShipmentNumberImportTypes.Code.HouseBill, Constants.ShipmentNumberImportTypes.Description.HouseBill);
			shipmentImportTypes.AddPair(Constants.ShipmentNumberImportTypes.Code.ShipmentNumber, Constants.ShipmentNumberImportTypes.Description.ShipmentNumber);
			shipmentImportTypes.AddPair(Constants.ShipmentNumberImportTypes.Code.ShipmentThenHouseBill, Constants.ShipmentNumberImportTypes.Description.ShipmentThenHouseBill);
			return shipmentImportTypes;
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem AllowMatchingByOtherAgentReferencesOnImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowMatchingByOtherAgentReferencesOnImport", delegate
				{
					return new BooleanRegistryItem(
						"AllowMatchingByOtherAgentReferencesOnImport",
						Categories.System_DataImportSettings_Shipment,
						(NoResString)"HBL matching fall back to Other Agent Reference matching",
						(NoResString)"When import XML is matched by HBL (check by HBL/HAWB/Consignment number) and no match is found, the system will try to match by Other Agent References.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		#endregion

		public BooleanRegistryItem AllowExportBrokerImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("FreightAllowExportBrokerImport", delegate
				{
					return new BooleanRegistryItem(
						"FreightAllowExportBrokerImport",
						Categories.System_DataImportSettings_Shipment,
						ResString.GetMultilingualString("ccca1894-95b2-42f8-97d3-a43d6ccc3129", "Import Export Broker From XML File"),
						ResString.GetMultilingualString("072f18cc-821f-4cbe-80da-f1d2b0526af3", "Enable this option to allow Export Brokers to be imported into the relevant Freight modules."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowBillingImportIntoShipment
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowBillingImportIntoShipment", delegate
				{
					return new BooleanRegistryItem(
						"AllowBillingImportIntoShipment",
						Categories.System_DataImportSettings_Shipment,
						ResString.GetMultilingualString("febf893c-eef1-4a0b-857a-2c073bc9a85d", "Import Billing Info From XML File"),
						ResString.GetMultilingualString("86c5a05d-6d67-48d9-96c5-78afcd667d0d", "Enable this option to allow billing information to be imported into the shipment."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		#region UpdateShipmentsDuringAutomaticImport

		public CodePairRegistryItem UpdateShipmentsDuringAutomaticImport
		{
			get
			{
				return GetItem(
					"UpdateShipmentsDuringAutomaticImport",
					() => new CodePairRegistryItem(
									"UpdateShipmentsDuringAutomaticImport",
									Categories.System_DataImportSettings_Shipment,
									ResString.GetMultilingualString("6671d5bd-2b30-4e24-8dd6-06945f56127e", "Update Shipments During Automatic Import"),
									ResString.GetMultilingualString("d9fd887a-adf0-4fad-947d-74a8fb5cd061", "By default the system will try to match import shipment to an existing shipment in the system and prompt the user whether to update an existing shipment. The user will not be prompted during the automatic import, therefore, select one of the options below to chose the behavior for automatic import of shipment XML if a matching shipment is found:"),
									new CodeDescriptionPairListProvider(() => ShipmentUpdateOptions()),
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							Constants.ShipmentAutomaticImportOptions.Code.NoImport));
			}
		}

		internal CodeDescriptionPairList ShipmentUpdateOptions()
		{
			var shipmentImportTypes = new CodeDescriptionPairList();
			shipmentImportTypes.AddPair(Constants.ShipmentAutomaticImportOptions.Code.Update, Constants.ShipmentAutomaticImportOptions.Description.Update);
			shipmentImportTypes.AddPair(Constants.ShipmentAutomaticImportOptions.Code.NoImport, Constants.ShipmentAutomaticImportOptions.Description.NoImport);
			shipmentImportTypes.AddPair(Constants.ShipmentAutomaticImportOptions.Code.Create, Constants.ShipmentAutomaticImportOptions.Description.Create);
			return shipmentImportTypes;
		}

		#region Allow Updating Shipment's Order during  Automatic Shipment Import
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem UpdateShipmentOrdersDuringShipmentAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateShipmentOrdersDuringShipmentAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateShipmentOrdersDuringShipmentAutomaticImport",
						Categories.System_DataImportSettings_Shipment,
						(NoResString)"Update Shipment's Order During Automatic Import",
						(NoResString)"Set this to 'Yes' to update Shipment's order details during Shipment Automatic Import. \r\n\r\nNOTE: It will not update Orders attached to Shipment with Declaration",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion
		#endregion

		#endregion

		#endregion

		#region Unprocessed Message Folder

		public StringRegistryItem UnprocessedMessagesDataDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("UnprocessedMessagesDataDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"UnprocessedMessagesDataDirectory",
						Categories.System_DataImportSettings_UnprocessedMessageFolder,
						ResString.GetMultilingualString("f35c1836-3451-4ff3-b3fa-f3f73320f5ce", "Folder to store unprocessed files"),
						ResString.GetMultilingualString("ca27cb28-92ed-4732-bd28-643eb57ea361", "The folder specified here is used to store all unprocessed files."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region Unprocessed Message Notifications

		public GuidRegistryItem UnprocessedMessageNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("UnprocessedMessageNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"UnprocessedMessageNotificationGroup",
						Categories.System_DataImportSettings_UnprocessedMessageNotifications,
						ResString.GetMultilingualString("cb4fc03e-d6e5-4aed-88f7-6e8a1b57c820", "Unprocessed Notification Group"),
						ResString.GetMultilingualString("f44f005e-e38a-4fd7-85ea-ef4fde180c5e", "The group that will be sent email notifications for Unprocessed Messages."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						RegistryFactory.Instance.GetGroupPK("ALL"));

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Default Messages

		public StringRegistryItem DefaultMessagesDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("DefaultMessagesDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"DefaultMessagesDataImportDirectory",
						Categories.System_DataImportSettings_DefaultMessages,
						ResString.GetMultilingualString("7121b430-861e-44dd-bbca-976947199e4c", "Folder to scan for XML files"),
						ResString.GetMultilingualString("b8099572-3c23-494c-bb7a-f043a444fbd4", "The folder specified here should contain any XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#region Warehouse

		public StringRegistryItem WarehouseDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("WarehouseDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"WarehouseDataImportDirectory",
						Categories.System_DataImportSettings_Warehouse,
						ResString.GetMultilingualString("3fea43c6-28b7-4581-9720-fa07dc935b66", "Folder to scan for XML files"),
						ResString.GetMultilingualString("ec9f4564-5a50-4183-b095-2f25dcb510e4", "The folder specified here should contain Warehouse XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem WarehouseCartageDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("WarehouseCartageDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"WarehouseCartageDataImportDirectory",
						Categories.System_DataImportSettings_Warehouse,
						ResString.GetMultilingualString("c020d466-27ef-4fc8-8571-6aaf5ac92851", "Folder to scan for XML Port Transport files"),
						ResString.GetMultilingualString("bce3f4b5-28b1-4f4d-9505-d28f08c09e3d", "The folder specified here should contain Warehouse Port Transport XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem WarehouseIFSDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("WarehouseIFSDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"WarehouseIFSDataImportDirectory",
						Categories.System_DataImportSettings_Warehouse,
						ResString.GetMultilingualString("6594530c-9be4-4307-a4ef-aa47c232e85b", "Folder to scan for IFS XML files"),
						ResString.GetMultilingualString("c8506710-fc4a-4e0c-9bb9-ca450cad570b", "The folder specified here should contain IFS XML export files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem WarehouseOrderFlatFileImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("WarehouseOrderFlatFileImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"WarehouseOrderFlatFileImportDirectory",
						Categories.System_DataImportSettings_Warehouse,
						ResString.GetMultilingualString("18de9d45-ab46-452f-aa5d-55073dc302de", "Folder to scan for Order CSV files"),
						ResString.GetMultilingualString("d17f084f-59a6-43b7-9cfa-d6c2a62659ad", "The folder specified here should contain Warehouse Order CSV files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public BooleanRegistryItem FinaliseOrderOnCartageImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("FinaliseOrderOnCartageImport", delegate
				{
					return new BooleanRegistryItem(
						"FinaliseOrderOnCartageImport",
						Categories.System_DataImportSettings_Warehouse,
						ResString.GetMultilingualString("73431625-44d8-4f46-9ad8-3c9a9cdcbb3c", "Finalize Order on Port Transport Import"),
						ResString.GetMultilingualString("047797a4-1ded-4d90-b0d2-2fe667f2e8d4", "When importing an Order's Port Transport details, the Order will be finalized."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem CreateMissingWarehouseProduct
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CreateMissingWarehouseProduct", delegate
				{
					return new BooleanRegistryItem(
						"CreateMissingWarehouseProduct",
						Categories.System_DataImportSettings_Warehouse,
						ResString.GetMultilingualString("C88F1B25-E751-4EBD-947E-F0A149893977", "Create Missing Products"),
						ResString.GetMultilingualString("58218E81-9820-4887-BF9C-BB2E42EC66AD", "Create Missing Products when importing Warehouse Receives or Orders."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Bookings

		public StringRegistryItem BookingsDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("BookingsDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"BookingsDataImportDirectory",
						Categories.System_DataImportSettings_Bookings,
						ResString.GetMultilingualString("9bf5656a-763f-4a53-855c-995f4a23f61e", "Folder to scan for Booking XML files"),
						ResString.GetMultilingualString("1db41bc9-2db3-4d5d-a6da-dba0cfaeb586",
							"The folder specified here should contain any Booking XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#region Update Booking's Containers During Automatic Import

		public BooleanRegistryItem UpdateBookingContainersDuringAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateBookingContainersDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateBookingContainersDuringAutomaticImport",
						Categories.System_DataImportSettings_Bookings,
						ResString.GetMultilingualString("06978434-445e-4ea1-a3b2-84a1d95a7479", "Update Booking's Containers During Automatic Import"),
						ResString.GetMultilingualString("c5b79dbb-9ddc-4a03-8c75-3cca91c47e29", "Set this to 'Yes' to update a Booking's Containers' details during an automatic XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion
		#endregion

		#region Consols

		#region Departure

		public BooleanRegistryItem AllowDepartureContainerYardAddressImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowContainerYardAddressImport", delegate
				{
					return new BooleanRegistryItem(
						"AllowContainerYardAddressImport",
						Categories.System_DataImportSettings_Consols_Departure,
						ResString.GetMultilingualString("dcaff089-80a7-42f9-a5de-2e560d29f898", "Import Container Yard Address From XML File"),
						ResString.GetMultilingualString("79feabb3-8baa-43ff-8d0f-37a39a10744e", "Enable this option to allow Container Yard Addresses to be imported into the relevant Freight modules."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowDepartureCTOAddressImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowDepartureCTOAddressImport", delegate
				{
					return new BooleanRegistryItem(
						"AllowDepartureCTOAddressImport",
						Categories.System_DataImportSettings_Consols_Departure,
						ResString.GetMultilingualString("55a0a5a3-6608-48a1-9d03-c9032f8051f8", "Import CTO Address From XML File"),
						ResString.GetMultilingualString("9082f9c8-aad9-4a8f-96f7-a064262fd4bd", "Enable this option to allow Departure CTO Addresses to be imported into the relevant Freight modules."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowDepartureDepotAddressImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("FreightAllowDepartureDepotAddressImport", delegate
				{
					return new BooleanRegistryItem(
						"FreightAllowDepartureDepotAddressImport",
						Categories.System_DataImportSettings_Consols_Departure,
						ResString.GetMultilingualString("3e30b6c7-4ca0-44f3-9c14-53891dff78aa", "Import Depot Address From XML File"),
						ResString.GetMultilingualString("88312a20-9709-411a-99fd-00a43ccf26f4", "Enable this option to allow Departure Depot Addresses to be imported into the relevant Freight modules."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public ConsolImportBranchRuleRegistryItem ConsolImportBranchRules
		{
			get
			{
				return GetItem("ConsolImportBranchRules", () =>
					new ConsolImportBranchRuleRegistryItem(
						"ConsolImportBranchRules",
						Categories.System_DataImportSettings_Consols,
						ResString.GetMultilingualString("a9075388-c582-4aa1-87fc-99946577955c", "Consol Import Branch Rules"),
						ResString.GetMultilingualString("e1f392f0-29ca-4767-a142-09ae1a0de967", @"Override these values to set the order of the default branch on creation of a new Consol from the import XML if the Branch specified in the XML import file is not found or is to be discarded.
The values can be between 0 and 2. A value of 0 means the rule will not be used.
There can be multiple rules with a value of 0.
Any value greater than 1 must not be duplicated, i.e. 1, 1 is not valid.
There must not be a gap between the sequence of numbers for the values, i.e. 1, 2 is valid, but 0, 2 is not valid.
One of the fallback rules ""Default to a any"" or ""Do not create"" must be selected."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new ImportBranchRule()));
			}
		}
		#endregion

		#region Consols Data Import Directory

		public StringRegistryItem ConsolsDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ConsolsDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ConsolsDataImportDirectory",
						Categories.System_DataImportSettings_Consols,
						ResString.GetMultilingualString("04eacfe2-86a9-4a34-bc0b-80b3474ee475", "Folder to scan for Consol XML files"),
						ResString.GetMultilingualString("aa314cff-4062-49c9-92f2-19f797325653", "The folder specified here should contain any Consol XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public BooleanRegistryItem ImportConsolNoFromXml
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ImportConsolNoFromXml", delegate
				{
					return new BooleanRegistryItem(
						"ImportConsolNoFromXml",
						Categories.System_DataImportSettings_Consols,
						ResString.GetMultilingualString("554559c6-670c-4e07-9ce8-e9aaf83538e4", "Import Consol Number From XML File"),
						ResString.GetMultilingualString("2f9145a3-ec3f-44e7-b2e4-1b6939835900", "Set this to 'Yes' to match the Consols Number by Agent Reference during an XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Always Check for Consol's Sailing, Shipments and Containers

		public BooleanRegistryItem AlwaysCheckForConsolSailingShipmentsAndContainers
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AlwaysCheckForConsolSailingShipmentsAndContainers", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysCheckForConsolSailingShipmentsAndContainers",
						Categories.System_DataImportSettings_Consols,
						ResString.GetMultilingualString("9b5ca5fa-b424-43ec-a0de-acdb105e7dd7", "Always Check for Consol's Sailing, Shipments and Containers"),
						ResString.GetMultilingualString("92c68928-92a7-4d96-a497-29a483b5d89f", "Set this to 'Yes' to allow a Consol's Sailing, Containers or Shipments to be updated when not updating the Consol during an XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Create Declaration for Shipment during XML Import

		public BooleanRegistryItem AutoCreateDeclarationWithinShipment
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutoCreateDeclarationWithinShipment", delegate
				{
					return new BooleanRegistryItem(
						"AutoCreateDeclarationWithinShipment",
						Categories.System_DataImportSettings_Consols,
						ResString.GetMultilingualString("a508524c-20bf-4462-921d-d30c4125083e", "Auto Create Customs Declaration within Shipments"),
						ResString.GetMultilingualString("81d1aa73-5a46-48b1-b333-fc83d3990abc", "Set this to 'Yes' to create Customs Declaration within Shipment during XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Automatic Update on Import

		#region Update Consol During Automatic Import - Other

		public BooleanRegistryItem UpdateConsolDuringAutomaticImportOther
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolDuringAutomaticImportOther", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolDuringAutomaticImportOther",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("1cd591ac-56d3-4061-94ef-ae6d8dcab1be", "Update Consol During Automatic Import - Other"),
						ResString.GetMultilingualString("8818991b-6180-4f4d-a0a2-e0311a498df4", "Set this to 'Yes' to update a Consol's details during an automatic XML import for other transport modes."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Allow Updating Shipment's Order during  Automatic Consol & Shipment Import
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem UpdateShipmentOrdersDuringConsolAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateShipmentOrdersDuringConsolAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateShipmentOrdersDuringConsolAutomaticImport",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						(NoResString)"Update Shipment's Order During Automatic Import",
						(NoResString)"Set this to 'Yes' to update Shipment's order details during Consol Automatic Import. \r\n\r\nNOTE: It will not update Orders attached to Shipment with Declaration",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion
		#endregion

		#region Update Consol During Automatic Import - Air

		public BooleanRegistryItem UpdateConsolDuringAutomaticImportAir
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolDuringAutomaticImportAir", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolDuringAutomaticImportAir",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("b51f0e67-8ed2-4fca-983e-acfee7c38f18", "Update Consol During Automatic Import - Air"),
						ResString.GetMultilingualString("ef6e9e0a-f0e2-4d42-a6b5-3d498c3bb39e", "Set this to 'Yes' to update a Consol's details during an automatic XML import for Air transport mode."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Update Consol During Automatic Import - Sea

		public BooleanRegistryItem UpdateConsolDuringAutomaticImportSea
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolDuringAutomaticImportSea", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolDuringAutomaticImportSea",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("b46ea4b2-a108-4d8c-9734-390144820d2c", "Update Consol During Automatic Import - Sea"),
						ResString.GetMultilingualString("3d590e83-66a3-4189-b9d1-f1810fc5e226", "Set this to 'Yes' to update a Consol's details during an automatic XML import for Sea transport mode."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Update Consol's Shipments During Automatic Import - Other

		public BooleanRegistryItem UpdateConsolShipmentsDuringAutomaticImportOther
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolShipmentsDuringAutomaticImportOther", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolShipmentsDuringAutomaticImportOther",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("ff80b167-353f-42ea-baa0-3d470918a453", "Update Consol's Shipments During Automatic Import - Other"),
						ResString.GetMultilingualString("7a582ac8-02c2-447b-8ab0-ba2a85e2285c", "Set this to 'Yes' to update a Consol's Shipments' details during an automatic XML import for other transport modes."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Update Consol's Shipments During Automatic Import - Air

		public BooleanRegistryItem UpdateConsolShipmentsDuringAutomaticImportAir
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolShipmentsDuringAutomaticImportAir", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolShipmentsDuringAutomaticImportAir",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("b8519e75-092e-46fc-a53b-b865b659f6cd", "Update Consol's Shipments During Automatic Import - Air"),
						ResString.GetMultilingualString("29f95d11-02fa-49df-86bc-472b4f0c8a80", "Set this to 'Yes' to update a Consol's Shipments' details during an automatic XML import for Air transport mode."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Update Consol's Shipments During Automatic Import - Sea

		public BooleanRegistryItem UpdateConsolShipmentsDuringAutomaticImportSea
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolShipmentsDuringAutomaticImportSea", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolShipmentsDuringAutomaticImportSea",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("0ef424ff-ae44-4273-b105-55036761c81f", "Update Consol's Shipments During Automatic Import - Sea"),
						ResString.GetMultilingualString("33a03284-cd09-46ee-a8ef-cd577d0e4574", "Set this to 'Yes' to update a Consol's Shipments' details during an automatic XML import for Sea transport mode."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Update Consol's Sailing During Automatic Import

		public BooleanRegistryItem UpdateSailingSchedulesDuringAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolSailingDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolSailingDuringAutomaticImport",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("bde7b83e-9e13-4fa7-95ec-ea5a8c77d5ee", "Update Sailing Schedules During Automatic Import"),
						ResString.GetMultilingualString("3020934f-903b-4142-9cc9-6ad3a2ab7bee", "Set this to 'Yes' to update sailing schedules details during an automatic XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Update Consol's Routing Information During Automatic Import

		public BooleanRegistryItem UpdateConsolsRoutingInformationDuringAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolsRoutingInformationDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolsRoutingInformationDuringAutomaticImport",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("a5af2d94-b5ec-4174-9afb-00db1c4ae1bb", "Update Consol's Routing Information During Automatic Import"),
						ResString.GetMultilingualString("270653dc-df22-4e57-be0f-4a9bc3d4fb1a", "Set this to 'Yes' to update a consol's routing information during automatic XML imports."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Update Consol's Containers During Automatic Import

		public BooleanRegistryItem UpdateConsolContainersDuringAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateConsolContainersDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateConsolContainersDuringAutomaticImport",
						Categories.System_DataImportSettings_Consols_AutomaticUpdateonImport,
						ResString.GetMultilingualString("bdd5e2b7-7721-42e3-877f-33873d618f43", "Update Consol's Containers During Automatic Import"),
						ResString.GetMultilingualString("8458b2ef-d620-41b1-b0b6-f2d70a095a3a", "Set this to 'Yes' to update a Consol's Containers' details during an automatic XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#endregion

		#region Allow Linking Standalone Shipment to Consol
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem AllowLinkingStandaloneShipmentToConsol
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowLinkingStandaloneShipmentToConsol", delegate
				{
					return new BooleanRegistryItem(
						"AllowLinkingStandaloneShipmentToConsol",
						Categories.System_DataImportSettings_Consols,
						(NoResString)"Linking Existing Standalone Shipment to Consol",
						(NoResString)"Enable this option to allow linking existing standalone shipment to consol during automatic import. \r\n\r\nNOTE: This registry works only when the Shipment matching Criteria is set to 'HBL' (i.e. Update existing shipment by matching HBL).",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion
		#endregion

		#endregion

		#region Customs Declarations

		public BooleanRegistryItem AllowCustomsDeclarationUpdateItem
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowCustomsDeclarationUpdate", delegate
				{
					return new BooleanRegistryItem(
						"AllowCustomsDeclarationUpdate",
						Categories.System_DataImportSettings_CustomsDeclarations,
						ResString.GetMultilingualString("5cf35b20-dbe5-4b95-848e-0318cd6ec1d5", "Allow Customs Declaration Update"),
						ResString.GetMultilingualString("25723A31-CCEC-4A57-972C-F5369BC0A74A", "This setting only applies to Legacy XML. Set this to 'YES' to update a Customs Declaration's details during an automatic XML import."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public StringRegistryItem CustomsDeclarationsDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("CustomsDeclarationsDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"CustomsDeclarationsDataImportDirectory",
						Categories.System_DataImportSettings_CustomsDeclarations,
						ResString.GetMultilingualString("f45e1fcb-3b3e-46c4-8cb1-a017e8bf6dfb", "Folder to scan for Customs Declarations XML files"),
						ResString.GetMultilingualString("3a907361-296d-4d86-80ad-a0f77f43e855", "The folder specified here should contain any Consol XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem ACDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ACDataImportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ACDataImportDirectory",
						Categories.System_DataImportSettings_CustomsDeclarations,
						ResString.GetMultilingualString("d2e54c1a-69fc-4dc3-8c85-1a443012520d", "Folder to scan for Advantage Customs Declarations XML files"),
						ResString.GetMultilingualString("2f7e6742-651b-42c1-855e-f9a5209a2591", "The folder specified here should contain any Consol XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ?
							RegistryOptions.IsHidden :
							(Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Germany ? RegistryOptions.PreserveTestValue : RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue)
						);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public StringRegistryItem ACStatusDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ACStatusDataImportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ACStatusDataImportDirectory",
						Categories.System_DataImportSettings_CustomsDeclarations,
						ResString.GetMultilingualString("d2e54c1a-69fc-4dc3-8c85-1a443012540d", "Folder to scan for Advantage Customs Status XML files"),
						ResString.GetMultilingualString("2f7e6742-651b-42c1-855e-f9a5209a2691", "The folder specified here should contain any Advantage Customs Status XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ?
							RegistryOptions.IsHidden :
							(Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Germany ? RegistryOptions.Default : RegistryOptions.IsHidden)
						);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public BooleanRegistryItem ImportDeclarationNoFromXml
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ImportDeclarationNoFromXml", delegate
				{
					return new BooleanRegistryItem(
						"ImportDeclarationNoFromXml",
						Categories.System_DataImportSettings_CustomsDeclarations,
						ResString.GetMultilingualString("c476f939-6916-4b2c-bcd9-ad6486d29b6f", "Import Customs Declaration Number From XML File"),
						ResString.GetMultilingualString("cf0f2312-9c67-4449-b1fb-c5f548646ebc", "Set this to 'Yes' to match the Customs Declaration Number by Agent Reference during an XML Import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public StringRegistryItem BIRDImportDirectory
		{
			get
			{
				return GetItem("BIRDImportDirectory", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
					"BIRDImportDirectory",
					Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica,
					ResString.GetMultilingualString("320dec68-3a20-4c93-950e-f6d39ec63c16", "BIRD Import File Directory"),
					ResString.GetMultilingualString("38d9fab4-c3ae-4e3d-8f92-32a5b12c34f9", "A directory to get the files For BIRD Data Import"),
					RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue
					);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;

					return result;
				});
			}
		}

		public StringRegistryItem BIRDImportBackupDirectory
		{
			get
			{
				return GetItem("BIRDImportBackupDirectory", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
					"BIRDImportBackupDirectory",
					Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica,
					ResString.GetMultilingualString("d258b7d8-53fd-4975-a08b-f64e7ea0ac0f", "BIRD Import File Backup Directory"),
					ResString.GetMultilingualString("a7fbd669-cd32-4fc7-8642-6734684dc6e2", "A directory to save the files that have been failed to be processed"),
					RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue
					);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;

					return result;
				});
			}
		}

		public StringRegistryItem BIRDFTPRemoteDirectory
		{
			get
			{
				return GetItem("BIRDFTPRemoteDirectory", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
					"BIRDFTPRemoteDirectory",
					Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings,
					ResString.GetMultilingualString("BAC09EBB-BD0F-4498-A3EB-D0E85E41CC04", "FTP Remote Directory Name"),
					ResString.GetMultilingualString("8D10DB23-F4E1-441D-B89E-4062CEFE56CD", "The FTP directory for BIRD files import"),
					RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default
					);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;
					return result;
				});
			}
		}

		public StringRegistryItem BIRDFTPPassword
		{
			get
			{
				return GetItem("BIRDFTPPassword", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
					"BIRDFTPPassword",
					Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings,
					ResString.GetMultilingualString("B324C8CA-C269-4905-9F46-F1846D960E31", "FTP Password"),
					ResString.GetMultilingualString("A9F680EF-4EC6-490F-A525-3125A775A4F9", "The FTP password for BIRD files import"),
					RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default
					);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;
					return result;
				});
			}
		}

		public StringRegistryItem BIRDFTPServerAddress
		{
			get
			{
				return GetItem("BIRDFTPServerAddress", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
					"BIRDFTPServerAddress",
					Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings,
					ResString.GetMultilingualString("2FB82A79-7C27-48FE-948C-7F6F5FF116E2", "FTP Server Address"),
					ResString.GetMultilingualString("EC563900-C4F7-4F6A-8C8A-9F4058671AC2", "The FTP server address for BIRD files import. It should be entered in the following format: {0}", "ftp://{SERVER _NAME}/"),
					RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default
					);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;
					result.DataType = new FtpUriRegistryDataType();
					return result;
				});
			}
		}

		public StringRegistryItem BIRDFTPUserName
		{
			get
			{
				return GetItem("BIRDFTPUserName", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
					"BIRDFTPUserName",
					Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings,
					ResString.GetMultilingualString("3F57171C-A1BD-4862-A351-893D8B7E83DF", "FTP User Name"),
					ResString.GetMultilingualString("B7C43094-4B94-4F4E-AE0B-0456249C61DC", "The FTP user name for BIRD files import"),
					RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default
					);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;
					return result;
				});
			}
		}

		public StringRegistryItem BIRDFTPFileExtension
		{
			get
			{
				return GetItem("BIRDFTPFileExtension", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
					"BIRDFTPFileExtension",
					Categories.System_DataImportSettings_CustomsDeclarations_UnitedStatesofAmerica_BIRDFTPSettings,
					ResString.GetMultilingualString("904702A4-086B-4EE5-91A6-593DB106CB2B", "FTP File Extension"),
					ResString.GetMultilingualString("1BA97832-EE94-4725-B1B1-A98EF5F31F44", "The BIRD importer will look into the designated FTP folder for the files with specified extension. By default, the BIRD importer will try to download and import all the '.BRD' files in the specified FTP folder. You can change the value to specify the target file extension for the BIRD import to import. The target file extension can not be empty."),
					RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
					"BRD"
					);
					StringRegistryDataType datatype = (StringRegistryDataType)result.DataType;
					datatype.MinLength = 1;
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;
					return result;
				});
			}
		}

		#endregion

		#region Data Import Wizard

		public BooleanRegistryItem UseCurrentCountryNumberFormatting
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseCurrentCountryNumberFormatting", delegate
				{
					return new BooleanRegistryItem(
						"UseCurrentCountryNumberFormatting",
						Categories.System_DataImportSettings_DataImportWizard,
						ResString.GetMultilingualString("8C5C739B-1743-4d35-B41A-34FFB06E2D9A", "Use current country/region number formatting"),
						ResString.GetMultilingualString("BE28BD5D-E8C6-4539-AD77-67DB14FFD886", "This option controls the default number formatting used by the Data Import Wizard tool. By default, the Data Import Wizard interprets numbers as being formatted in the English style, with the dot as decimal separator and the comma as optional thousands separator. When the option is enabled, the Data Import Wizard will interpret numbers based on the culture of the current branch country/region."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Orders

		public StringRegistryItem OrdersXMLDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("OrdersXMLDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"OrdersXMLDataImportDirectory",
						Categories.System_DataImportSettings_Orders,
						ResString.GetMultilingualString("5759f1e9-2aba-4de8-9df6-8dde0ad36573", "Folder to scan for Orders XML files"),
						ResString.GetMultilingualString("dc6c7788-ceb1-4848-90e1-e99c2457285e",
							"The folder specified here should contain any Orders XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem OrdersCSVDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("OrdersCSVDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"OrdersCSVDataImportDirectory",
						Categories.System_DataImportSettings_Orders,
						ResString.GetMultilingualString("e78fde0c-2ec0-4044-8a84-30e9d64be8ca", "Folder to scan for Orders CSV files"),
						ResString.GetMultilingualString("bca435f4-a675-4090-83bf-28d0c4797b08",
							"The folder specified here should contain any Orders CSV files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public CreateMissingProductsRegistryItem CreateMissingProductWithRelationship
		{
			get
			{
				return GetItem<CreateMissingProductsRegistryItem>("CreateMissingProductWithRelationship", delegate
				{
					return new CreateMissingProductsRegistryItem("CreateMissingProductWithRelationship",
						Categories.System_DataImportSettings_Orders,
						ResString.GetMultilingualString("e7dfd34a-3904-417e-bc6a-edb4d2e93758", "Create Missing Products"),
						ResString.GetMultilingualString("d366ba9f-d351-44aa-af08-7289b5ca2dcf", "Create Missing Product when doing Import Process"),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new CreateMissingProductsInfo());
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem UpdateShipmentOrdersDuringAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateShipmentOrdersDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateShipmentOrdersDuringAutomaticImport",
						Categories.System_DataImportSettings_Orders,
						(NoResString)"Update Shipment's Order During Automatic Import",
						(NoResString)"Set this to 'Yes' to update Shipment's order details during Automatic Import. \r\n\r\nNOTE: It will not update Orders attached to Shipment with Declaration",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Commercial Invoices

		public StringRegistryItem CommercialInvoicesDataImportDirectory
		{
			get
			{
				return GetItem("CommercialInvoicesDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
					"CommercialInvoicesDataImportDirectory",
							Categories.System_DataImportSettings_CommercialInvoices,
							ResString.GetMultilingualString("d0ef97c1-6484-4a6d-bd85-d103930c50b6", "Folder to scan for Commercial Invoices CSV files"),
							ResString.GetMultilingualString("2e1ffba8-6326-4111-bb37-63cfddc81413", "The folder specified here should contain Commercial Invoices CSV files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem CommercialInvoicesDataImportDirectoryXml
		{
			get
			{
				return GetItem("CommercialInvoicesDataImportDirectoryXml",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
					"CommercialInvoicesDataImportDirectoryXml",
							Categories.System_DataImportSettings_CommercialInvoices,
							ResString.GetMultilingualString("60841f55-666a-4936-b224-05d8e146545c", "Folder to scan for Commercial Invoices XML files"),
							ResString.GetMultilingualString("f5b64e5e-9756-4338-80d5-fa4b048a75f0", "The folder specified here should contain Commercial Invoices XML files that are to be automatically imported."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#region PODs

		#region PODDataImportDirectory

		public StringRegistryItem PODDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("PODDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"PODDataImportDirectory",
						Categories.System_DataImportSettings_PODs,
						ResString.GetMultilingualString("ad28270f-8abb-4ef8-a95d-2cb273d0b44d", "Folder to scan for POD CSV files"),
						ResString.GetMultilingualString("6cfbb1f5-1743-4663-8553-b4d50b97f9ac", "The folder specified here should contain POD CSV files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#endregion

		#region Products

		#region ProductsDataImportDirectory

		public StringRegistryItem ProductsDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ProductsDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ProductsDataImportDirectory",
						Categories.System_DataImportSettings_Products,
						ResString.GetMultilingualString("6a26c26d-f887-4312-bff0-de3001736d43", "Folder to scan for Product CSV files"),
						ResString.GetMultilingualString("835e63fb-f114-4051-99ad-9c8a79da549f", "The folder specified here should contain Products CSV files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem ProductsXMLDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ProductsXMLDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ProductsXMLDataImportDirectory",
						Categories.System_DataImportSettings_Products,
						ResString.GetMultilingualString("8135e939-df54-4599-897f-dfcfc5385b61", "Folder to scan for Product XML files"),
						ResString.GetMultilingualString("3f209172-c900-4735-931e-42c80b6567c1", "The folder specified here should contain any Product XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#region Update Products During Automatic Import

		public BooleanRegistryItem UpdateProductsDuringAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateProductsDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateProductsDuringAutomaticImport",
						Categories.System_DataImportSettings_Products,
						ResString.GetMultilingualString("78223191-2ef9-4e96-b2c6-30e3619250cf", "Update Products During Automatic Import"),
						ResString.GetMultilingualString("20218da6-293e-474a-a8c5-a45fc348b1de", "Set this to 'Yes' to update Products details during an automatic CSV or XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Use Legacy Codes During Automatic Import

		public BooleanRegistryItem UseLegacyCodesDuringAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseLegacyCodesDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UseLegacyCodesDuringAutomaticImport",
						Categories.System_DataImportSettings_Products,
						ResString.GetMultilingualString("149cac5b-b985-4908-9f8c-8787b5ef70e1", "Use Legacy Codes for Organization Matching During Automatic Import"),
						ResString.GetMultilingualString("22f4ca08-b8e5-4de5-9cd8-2986bf0d562b", "Set this to 'Yes' to match Buyer/Supplier organizations using Legacy Codes rather than {0} Organization Codes, during an automatic CSV import.", Constants.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region ProductLastCostImportDirectory

		public StringRegistryItem ProductLastCostImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ProductLastCostImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ProductLastCostImportDirectory",
						Categories.System_DataImportSettings_Products,
						ResString.GetMultilingualString("53136926-5e00-4f84-b747-016c60b8f358", "Folder to scan for Product Last Cost CSV files"),
						ResString.GetMultilingualString("c0eb5bc4-5f7a-41e6-8e10-7a407c84995d", "The folder specified here should contain Product Last Cost data update CSV files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#endregion

		#region Destination Port Clearance Process

		#region Australia

		#region Air Cargo

		public BooleanRegistryItem AutomaticallyCreateAirCargoJob
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutomaticallyCreateAirCargoMesssage", delegate
				{
					return new BooleanRegistryItem(
						new AURegistryItem("AutomaticallyCreateAirCargoMesssage",
											Categories.System_DataImportSettings_DestinationPortClearanceProcess_Australia_AirCargo,
											ResString.GetMultilingualString("c9d05205-bcce-42d8-8629-da863254a427", "Automatically Create Air Cargo Job"),
											ResString.GetMultilingualString("55ce200d-4fae-4cf1-b07d-c6b37b3538b2", "Specify if you want the Consol XML importer to automatically generate an Air Cargo Job on a successful import. The relevant registry item is found at: \"{0} -> {1}\"", new ModifiedMultilingualString(delegate
											{ return ConsolsDataImportDirectory.Category; }), new ModifiedMultilingualString(delegate
											{ return ConsolsDataImportDirectory.Caption; })),
											RegistryStorageFlags.Company | RegistryStorageFlags.System,
											DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
											false));
				});
			}
		}

		public BooleanRegistryItem AutomaticallySendAirCargoMessage
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutomaticallySendAirCargoMesssage", delegate
				{
					return new BooleanRegistryItem(
						new AURegistryItem("AutomaticallySendAirCargoMesssage",
											Categories.System_DataImportSettings_DestinationPortClearanceProcess_Australia_AirCargo,
											ResString.GetMultilingualString("226d7141-7a55-4020-9493-f442dff49a31", "Automatically Create And Send Air Cargo Message"),
											ResString.GetMultilingualString("3c2e4c7a-30b6-4c43-a740-4ee23117c312", "Specify if you want the Consol XML importer to automatically send an Air Cargo Message on a successful import. Note: This has no effect on Consols that are imported manually. The relevant registry item is found at: \"{0} -> {1}\"", new ModifiedMultilingualString(delegate
											{ return ConsolsDataImportDirectory.Category; }), new ModifiedMultilingualString(delegate
											{ return ConsolsDataImportDirectory.Caption; })),
											RegistryStorageFlags.Company | RegistryStorageFlags.System,
											DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
											false));
				});
			}
		}

		#endregion

		#region Sea Cargo

		public BooleanRegistryItem AutomaticallyCreateSeaCargoJob
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutomaticallyCreateSeaCargoMesssage", delegate
				{
					return new BooleanRegistryItem(
						new AURegistryItem("AutomaticallyCreateSeaCargoMesssage",
											Categories.System_DataImportSettings_DestinationPortClearanceProcess_Australia_SeaCargo,
											ResString.GetMultilingualString("aeaaf2d9-a763-4691-bc0e-e24e94c464d9", "Automatically Create Sea Cargo Job"),
											ResString.GetMultilingualString("9a74ed59-c372-4ef8-ae29-ca9b94e304aa", "Specify if you want the Consol XML importer to automatically generate a Sea Cargo Job on a successful import. The relevant registry item is found at: \"{0} -> {1}\"", new ModifiedMultilingualString(delegate
											{ return ConsolsDataImportDirectory.Category; }), new ModifiedMultilingualString(delegate
											{ return ConsolsDataImportDirectory.Caption; })),
											RegistryStorageFlags.Company | RegistryStorageFlags.System,
											DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
											false));
				});
			}
		}

		public BooleanRegistryItem AutomaticallySendSeaCargoMessage
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutomaticallySendSeaCargoMessage", delegate
				{
					return new BooleanRegistryItem(
						new AURegistryItem("AutomaticallySendSeaCargoMessage",
											Categories.System_DataImportSettings_DestinationPortClearanceProcess_Australia_SeaCargo,
											ResString.GetMultilingualString("0e597fda-9f84-4ed3-a129-5f21eacccca4", "Automatically Create And Send Sea Cargo Message"),
											ResString.GetMultilingualString("0529b939-abb3-4307-a770-a2d6c5b2f9af", "Specify if you want the Consol XML importer to automatically send a Sea Cargo Message on a successful import. Note: This has no effect on Consols that are imported manually. The relevant registry item is found at: \"{0} -> {1}\"", new ModifiedMultilingualString(delegate
											{ return ConsolsDataImportDirectory.Category; }), new ModifiedMultilingualString(delegate
											{ return ConsolsDataImportDirectory.Caption; })),
											RegistryStorageFlags.Company | RegistryStorageFlags.System,
											DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
											false));
				});
			}
		}

		#endregion

		class AURegistryItem : RegistryItemImpl
		{
			public AURegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlag, RegistryOptions options, bool defaultValue)
				: base(name, category, caption, hint, RegistryDataTypes.BoolType, storageFlag, options, defaultValue)
			{
			}

			public override IEnumerable<Guid> CountryFilterPKs
			{
				get { return RegistryItemSet.CountryFilterPKs.Australia; }
			}
		}

		#endregion

		#endregion

		#region SailingSchedule

		public StringRegistryItem ScheduleDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ScheduleDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ScheduleDataImportDirectory",
						Categories.System_DataImportSettings_Schedules,
						ResString.GetMultilingualString("f0c6f7e0-ca0b-4f35-89b3-dcfed1e5adee", "Folder to scan for Schedules XML files"),
						ResString.GetMultilingualString("42b61ec8-1ff8-4e4c-bdbc-00aced3b9edc", "The folder specified here should contain any Schedule XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#region Update Schedules During Automatic Import

		public BooleanRegistryItem UpdateSchedulesDuringAutomaticImport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UpdateScheduleDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UpdateScheduleDuringAutomaticImport",
						Categories.System_DataImportSettings_Schedules,
						ResString.GetMultilingualString("7d9e4d96-b133-43fa-998f-713da796292a", "Update Schedules During Automatic Import"),
						ResString.GetMultilingualString("56850b7f-6114-4772-b5d7-e40689d4252c", "Set this to 'Yes' to update a Schedule details during an automatic XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public IntRegistryItem FlightScheduleUpdateThresholdForDataImport
		{
			get
			{
				return GetItem("FlightScheduleUpdateThresholdForDataImport", () => new IntRegistryItem(
						"FlightScheduleUpdateThresholdForDataImport",
						Categories.System_DataImportSettings_Schedules,
						ResString.GetMultilingualString("34030ae2-65b1-47b2-97a0-200c3af9a536", "Flight Schedule Update Threshold"),
						ResString.GetMultilingualString("b573a6da-e9d6-46f4-a27c-b87f48416a09", @"If 'Update Schedules During Automatic Import' registry is set to Yes, this value determines the threshold to either create a new flight schedule or update an existing flight schedule and all linked jobs during automatic data import.

By default the new flight will only be created if there are no flights with matching flight number found within the last 24 hours."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						24, 1, 24));
			}
		}

		#endregion

		#endregion

		#region Local Cartage

		public StringRegistryItem LocalCartageDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("LocalCartageDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
							"LocalCartageDataImportDirectory",
							Categories.System_DataImportSettings_PortTransport,
							ResString.GetMultilingualString("4814e841-2208-453a-a99c-f0cb59f38480", "Folder to scan for Port Transport XML files"),
							ResString.GetMultilingualString("a42422c2-01e5-44aa-a454-4aa618a5aeb5", "The folder specified here should contain any Port Transport XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#region ContainerEvents

		public StringRegistryItem ContainerEventsDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ContainerEventsDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
							"ContainerEventsDataImportDirectory",
							Categories.System_DataImportSettings_ContainerEvents,
							ResString.GetMultilingualString("aa4b32b3-dfae-48b8-a2b2-95024f3b3d54", "Folder to scan for Container Events XML files"),
							ResString.GetMultilingualString("38c1a454-5d07-4080-8d9f-4d8b38f3eccc", "The folder specified here should contain any Container Events XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#region EmailNotificationForErrorsOnly

		public BooleanRegistryItem EmailNotificationForErrorsOnly
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EmailNotificationForErrorsOnly", delegate
				{
					return new BooleanRegistryItem(
						"EmailNotificationForErrorsOnly",
						Categories.System_DataImportSettings,
						ResString.GetMultilingualString("4acf0a75-e72b-4323-a4ba-53823d2d8f68", "Email Notification For Errors Only"),
						ResString.GetMultilingualString("1a067bb6-ee11-4f79-b37f-f09d54ce28c8", "Set this option to \"Yes\" if Notification Emails are to be sent on Data Import Errors only."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region System Merge

		#region Company Code Mapping

		public CodeDescriptionPairListRegistryItem SystemMergeCompanyCodeMapping
		{
			get
			{
				return GetItem("SystemMergeCompanyCodeMapping", delegate
				{
					var hint = ResString.GetMultilingualString("a6d19d6e-67a6-4ba0-951a-b079c7294252", "This is to be used exclusively for the System Merge Data Import. It allows the mapping of company codes when importing organizations so that company codes from the source system can be associated with codes in this system. In order to set up the mapping, type the company code to be imported in 'Source Company Code' and type this system's associated company code in the adjacent 'Replacement Company Code' cell.\r\n\r\nTo skip company related data from being imported, map its code to a blank 'Replacement Company Code'.");

					var dataType = new SystemMergeCompanyCodeMappingDataType(3);
					dataType.AllowEmptyCodes = false;
					dataType.AllowEmptyDescriptions = true;
					var result = new CodeDescriptionPairListRegistryItem(
										"SystemMergeCompanyCodeMapping",
										Categories.System_DataImportSettings_SystemMerge,
										ResString.GetMultilingualString("6a86b231-ca5a-40d9-a014-3e1d0f313c8f", "Import Company Code Mapping"),
										hint,
										GlbCompanySchema.GC_Code.MaxLength,
										RegistryStorageFlags.System,
										DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default)
					{
						DataType = dataType
					};

					result.EditorInfo = new CodeDescriptionPairListEditorInfo(
												true,
												true,
												CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
												CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
												ResString.GetMultilingualString("9f6a4cc7-1f7f-46a6-9b80-acd21acadee3", "Source Company Code"),
												ResString.GetMultilingualString("053cbe29-7e13-49d0-ad19-06ccf483a2fa", "Replacement Company Code"));
					return result;
				});
			}
		}

#if DEBUG
		public
#endif
		class SystemMergeCompanyCodeMappingDataType : CodeDescriptionPairListRegistryDataType
		{
			public SystemMergeCompanyCodeMappingDataType(int codeMaxLength)
				: base(codeMaxLength)
			{
			}

			protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				for (int i = 0; i < proposedValue.Count; i++)
				{
					var pair = proposedValue[i];
					if (!AllowEmptyCodes && string.IsNullOrEmpty(pair.Code))
					{
						var codeCaption = registryItem.EditorInfo is CodeDescriptionPairListEditorInfo editorInfo && !string.IsNullOrEmpty(editorInfo.CodeColumnCaption) ?
												editorInfo.CodeColumnCaption :
												Res.GetString("4226ACAB-7C2D-4DDC-BEDD-D8AFE772FACA", "Code");

						throw new RegistryValidationException(Res.GetString("1B26E5D8-B512-44E0-882D-84D07F765362", "You cannot enter an item with no {0}.", codeCaption));
					}

					if (!AllowEmptyDescriptions && string.IsNullOrEmpty(pair.Description))
					{
						var descriptionCaption = registryItem.EditorInfo is CodeDescriptionPairListEditorInfo editorInfo && !string.IsNullOrEmpty(editorInfo.DescriptionColumnCaption) ?
														editorInfo.DescriptionColumnCaption :
														Res.GetString("A344D37F-2585-47F8-ACB0-D8CB60A0B030", "Description");

						throw new RegistryValidationException(Res.GetString("FD5A6BFE-56E1-412F-B64A-7545F790F8C9", "You cannot enter an item with no {0}.", descriptionCaption));
					}

					for (int j = 0; j < proposedValue.Count; j++)
					{
						if (i != j && !string.IsNullOrEmpty(pair.Description) && pair.Description.Equals(proposedValue[j].Description, StringComparison.InvariantCultureIgnoreCase))
						{
							throw new RegistryValidationException(Res.GetString("851aadc6-b93a-44c3-b90f-fce0a0e01fcb", "You cannot map more than one Source Company Code to a single Replacement Company Code. Duplicated Replacement Company Code: {0}", proposedValue[j].Description));
						}
					}
				}

				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			}
		}

		#endregion

		#region Activate Data Interface
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem ActivateSystemMergeDataInterface
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ActivateSystemMergeDataInterface", delegate
				{
					return new BooleanRegistryItem(
						"ActivateSystemMergeDataInterface",
						Categories.System_DataImportSettings_SystemMerge,
						ResString.GetMultilingualString("6568d89a-363c-41a0-96b1-883bd5502003", "Activate System Merge Data Interface"),
						ResString.GetMultilingualString("ab76da84-c92f-4b87-ace7-97df2dc87739", "Activate System Merge data import and export interfaces."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController),
						false);
				});
			}
		}

		#endregion
		#endregion

		#endregion

		#region Event

		public StringRegistryItem EventDataImportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("EventDataImportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"EventDataImportDirectory",
						Categories.System_DataImportSettings_Event,
						ResString.GetMultilingualString("a471fb71-f47c-4085-9d2b-d7ed247e06ba", "Folder to scan for Event XML files"),
						ResString.GetMultilingualString("f7a9ae6c-77a9-462e-8bbb-79508d3c3009", "The folder specified here should contain any Event XML files that are to be automatically imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#region Shipping Bill of Lading Import

		public CodePairRegistryItem ImportBillingInfoFromAgencyXmlFile
		{
			get
			{
				return GetItem<CodePairRegistryItem>("ImportBillingInfoFromAgencyXmlFile", delegate
				{
					var list = GetImportBillingInfoFromAgencyXMLFileList();

					return new CodePairRegistryItem(
						"ImportBillingInfoFromAgencyXmlFile",
						Categories.System_DataImportSettings_ShippingBillofLading,
						ResString.GetMultilingualString("6d2c9e9d-c3ac-4927-8c8f-77f1c99e91e1", "Import Billing Information When Importing Bills and Bookings"),
						ResString.GetMultilingualString("17fa9b88-955b-4b24-a870-eccbebc03794", "Should the billing information in the XML be imported when importing a bill of lading or a booking."),
						new CodeDescriptionPairListProvider(() => list),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						list.DefaultCode
						);
				});
			}
		}

		#region Implementation

		internal CodeDescriptionPairList GetImportBillingInfoFromAgencyXMLFileList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(Constants.ImportBillingInfoFromXMLMethod.Codes.Never, Constants.ImportBillingInfoFromXMLMethod.Descriptions.Never);
			result.AddPair(Constants.ImportBillingInfoFromXMLMethod.Codes.NewOnly, Constants.ImportBillingInfoFromXMLMethod.Descriptions.NewOnly);
			result.AddPair(Constants.ImportBillingInfoFromXMLMethod.Codes.Always, Constants.ImportBillingInfoFromXMLMethod.Descriptions.Always);
			result.DefaultCode = Constants.ImportBillingInfoFromXMLMethod.Codes.NewOnly;
			return result;
		}

		#endregion

		#endregion

		#region Accounting

		#region "Receivable"

		public CodePairRegistryItem DuplicatePaymentReferenceValidationOnImportingInvoices
		{
			get
			{
				return GetItem<CodePairRegistryItem>("DuplicatePaymentReferenceValidationOnImportingInvoices", delegate
				{
					return new CodePairRegistryItem(
						"DuplicatePaymentReferenceValidationOnImportingInvoices",
						Categories.System_DataImportSettings_Accounting_Receivable,
						ResString.GetMultilingualString("5f5d9244-caf0-48b2-802d-5a660d91b3df", "Duplicate Payment Reference Validation When Importing Invoices"),
						ResString.GetMultilingualString("11198de6-a56b-4412-b920-b22cbb343bea", "This registry item allows you to configure how {0} should treat instances of duplicated 'payment references' when importing invoices through XML or CSV", Constants.ProductName),
						new CodeDescriptionPairListProvider(() => GetDuplicatePaymentReferenceOptions()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						GetDuplicatePaymentReferenceOptions().DefaultCode);
				});
			}
		}

		internal CodeDescriptionPairList GetDuplicatePaymentReferenceOptions()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.AllowDuplicates, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Descriptions.AllowDuplicates);
			result.AddPair(Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForEntireLedger, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Descriptions.DisallowDuplicatesForEntireLedger);
			result.AddPair(Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.DisallowDuplicatesForLedgerAndTransactionType, Constants.AllowDuplicatePaymentReferenceHandlerMethods.Descriptions.DisallowDuplicatesForLedgerAndTransactionType);
			result.DefaultCode = Constants.AllowDuplicatePaymentReferenceHandlerMethods.Codes.AllowDuplicates;
			return result;
		}

		#endregion

		#endregion

		#endregion

		#region eDocs attributes

		public StringRegistryItem AssemblyDataAttribute
		{
			get
			{
				return GetItem<StringRegistryItem>("AssemblyDataAttribute", delegate
				{
					return new StringRegistryItem("AssemblyDataAttribute", Categories.System_Framework, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		#endregion

		#region Include Consol eDocs

		public BooleanRegistryItem IncludeConsoleDocs
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeConsoleDocs", delegate
				{
					return new BooleanRegistryItem(
					"IncludeConsoleDocs",
					Categories.System_DataExportSettings,
					ResString.GetMultilingualString("3b0e303c-2258-4634-94fe-b00b63e40808", "Include eDocs when Exporting Consol XML"),
					ResString.GetMultilingualString("6d9da7d6-5953-4eeb-8937-1ff117b317ef", "Set this to 'Yes' to export Consols eDocs data."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
					false);
				});
			}
		}

		#endregion

		#region Include Shipment eDocs

		public BooleanRegistryItem IncludeShipmenteDocs
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeShipmenteDocs", delegate
				{
					return new BooleanRegistryItem(
					"IncludeShipmenteDocs",
					Categories.System_DataExportSettings,
					ResString.GetMultilingualString("2fc06014-d8aa-43d2-b92c-cdd7455a7432", "Include eDocs when Exporting Shipment XML"),
					ResString.GetMultilingualString("0215a85d-36f1-4c4f-9726-1333aa3bcbb9", "Set this to 'Yes' to export Shipment eDocs data."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
					false);
				});
			}
		}

		#endregion

		#region Include Warehouse Order Confirmation eDocs

		public BooleanRegistryItem IncludeWhsOrdereDocs
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeWhsOrdereDocs", delegate
				{
					return new BooleanRegistryItem(
					"IncludeWhsOrdereDocs",
					Categories.System_DataExportSettings,
					ResString.GetMultilingualString("139b2329-8b67-481d-94a1-cd126bc6db29", "Include eDocs when Exporting Warehouse Order XML"),
					ResString.GetMultilingualString("1f506774-34ec-429b-864f-09bfcd32830e", "Set this to 'Yes' to export Warehouse Order eDocs data."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
					false);
				});
			}
		}

		#endregion

		#region Include Warehouse Receive Confirmation eDocs

		public BooleanRegistryItem IncludeWhsReceipteDocs
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeWhsReceipteDocs", delegate
				{
					return new BooleanRegistryItem(
					"IncludeWhsReceipteDocs",
					Categories.System_DataExportSettings,
					ResString.GetMultilingualString("465e8e84-6de0-4dbc-bf38-e605529eaa43", "Include eDocs when Exporting Warehouse Receive XML"),
					ResString.GetMultilingualString("d0225f04-7631-44a3-ac42-a3c82214c03e", "Set this to 'Yes' to export Warehouse Receive eDocs data."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
					false);
				});
			}
		}

		#endregion

		#region Include Warehouse Adjustment eDocs

		public BooleanRegistryItem IncludeWhsAdjustmenteDocs
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeWhsAdjustmenteDocs", delegate
				{
					return new BooleanRegistryItem(
					"IncludeWhsAdjustmenteDocs",
					Categories.System_DataExportSettings,
					ResString.GetMultilingualString("cbf80faa-366a-48ec-b14f-a099a04c719e", "Include eDocs when Exporting Warehouse Adjustment XML"),
					ResString.GetMultilingualString("40a6a28f-f43f-411d-873a-17692e862e1c", "Set this to 'Yes' to export Warehouse Adjustment eDocs data."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
					false);
				});
			}
		}

		#endregion

		#region Contact for eDoc Export

		public GuidRegistryItem ContactForEDocExport
		{
			get
			{
				return GetItem<GuidRegistryItem>("ContactForEDocExport", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ContactForEDocExport",
						Categories.System_DataExportSettings,
						ResString.GetMultilingualString("1F872C42-45E3-43b4-9168-506810258F85", "Contact for eDoc Export"),
						ResString.GetMultilingualString("3EABC2B8-52DE-4b93-8E1D-DDB69F768841", "The contact will be used as login to WebTracker for eDoc export."),
						RegistryStorageFlags.System,
						RegistryOptions.Default);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgContact);
					return result;
				});
			}
		}

		#endregion

		#region Simplified XML

		public BooleanRegistryItem SimpleXMLExportFormat
		{
			get
			{
				return GetItem("SimpleXMLExportFormat", delegate
				{
					return new BooleanRegistryItem(
					"SimpleXMLExportFormat",
					Categories.System_DataExportSettings,
					ResString.GetMultilingualString("3d94533e-df72-42b0-a308-766a0bebbe4a", "Use Light-Weight XML"),
					ResString.GetMultilingualString("591d335d-839b-4189-9a5b-281e8002630f", "If enabled, changes certain XML structures to be more condensed and clear."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					true);
				});
			}
		}

		#endregion

		#region Include Local Value and Exchange Rate when Exporting Billing

		public BooleanRegistryItem IncludeLocValAndExRateWhenExportingBilling
		{
			get
			{
				return GetItem("IncludeLocValAndExRateWhenExportingBilling", delegate
				{
					return new BooleanRegistryItem(
					"IncludeLocValAndExRateWhenExportingBilling",
					Categories.System_DataExportSettings,
					ResString.GetMultilingualString("243d629b-e08c-4362-8a01-fd34b355c2b1", "Include Local Amount and Exchange Rate in Billing Information Export"),
					ResString.GetMultilingualString("c3838df9-84ee-4b55-adee-aeb007488a33", @"When this registry is set to 'Yes', {0} will include the local sell and cost values, as well as the cost and sell exchange rates for each charge line in the exported XML.
Billing information can be sent with operations jobs such as shipments when the relevant registry item is enabled (for shipments, see the registry setting System > Data Export Settings > Shipment Export > Include Billing Information in XML File)", Constants.ProductName),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
					false);
				});
			}
		}

		#endregion

		#region Export Directories

		public StringRegistryItem BookingExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("BookingExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"BookingExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("8ae98a5b-c5bc-468b-8434-56f892144267", "Bookings"),
						ResString.GetMultilingualString("3347c432-d77f-43ab-9d5a-381eb8346ee6", "The folder specified here should contain any Booking XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem ConsolExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ConsolExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ConsolExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("6788ce0d-9a0a-42bd-b730-5e1de7c39b11", "Consols"),
						ResString.GetMultilingualString("0df87c5f-00b8-4530-a91e-eecbffcbb56c", "The folder specified here should contain any Consol XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem CFSLoadListConsolDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("CFSLoadListConsolDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"CFSLoadListConsolDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("93211a3f-5d62-49e0-9266-53c973359ab9", "Load Lists"),
						ResString.GetMultilingualString("b999d11a-5b52-440a-8fb6-5dc89dbf51a2", "The folder specified here should contain any Load List XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem ALPOExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ALPOExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ALPOExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("3c51c75f-97fa-4286-b0e4-e01e7d8785f1", "ALPO"),
						ResString.GetMultilingualString("96d360e5-d836-447a-b3d6-7409dd4020d0", "The folder specified here should contain any ALPO XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden :
							(Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Germany ? RegistryOptions.PreserveTestValue : RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public StringRegistryItem ATLASExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ATLASExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ATLASExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("74e090a4-ef3b-4e4b-b394-bab31a204ee1", "ATLAS"),
						ResString.GetMultilingualString("60d2e181-7ace-44fc-bb47-1e73e3ae459c", "The folder specified here should contain any ATLAS XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden :
							(Env.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Germany ? RegistryOptions.Default : RegistryOptions.IsHidden));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public StringRegistryItem ShipmentExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ShipmentsExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ShipmentsExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("be1aa4e8-7f23-41be-b032-56dd97d395bc", "Shipments"),
						ResString.GetMultilingualString("3db54cf0-1b52-46b8-a11e-4a8dcf665f28",
							"The folder specified here should contain any Shipment XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem ShipmentAsCustomDeclarationExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("ShipmentAsCustomDeclarationExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"ShipmentAsCustomDeclarationExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("f04ff7d4-67aa-4ed5-b468-0d8032526d58", "Shipments as Customs Declarations"),
						ResString.GetMultilingualString("ed40c840-f6ea-4a6c-b207-b3104fea6945", "The folder specified here should contain any Shipment as Customs Declaration XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem CustomDeclarationExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("CustomDeclarationExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"CustomDeclarationExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("ac268f49-d404-4129-9875-2225c17882a4", "Customs Declarations"),
						ResString.GetMultilingualString("4a5462d5-3f31-4b4b-afe6-8281e16c52d9", "The folder specified here should contain any Customs Declaration XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem OrderExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("OrderExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"OrderExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("f3d3a55d-325c-4362-9ab1-72e39167dc08", "Orders"),
						ResString.GetMultilingualString("306e650f-226e-437a-8e1b-1920fe32562a",
							"The folder specified here should contain any Order XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem WarehouseExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("WarehouseExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"WarehouseExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("b268dbd8-bb73-4fbe-a448-80712b027a53", "Warehouse"),
						ResString.GetMultilingualString("a252ed2c-eccf-4fc2-b0d5-9e9d5e740d88",
							"The folder specified here should contain any Warehouse XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem HVLVBookingHeaderExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("HVLVBookingHeaderExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"HVLVBookingHeaderExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("1fd1bfdb-ef40-4836-b6aa-6d647955d9d3", "HVLV Booking Headers"),
						ResString.GetMultilingualString("8eccc2ec-5a31-4de7-aa78-56d5290b86d1", "The folder specified here should contain any HVLV Booking Header XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem GateBookingExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("GateBookingExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"GateBookingExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("69fbd09b-cf57-4fa5-b6a2-b8179cc239be", "Gate Bookings"),
						ResString.GetMultilingualString("b566d61d-8148-432b-9f88-b7fcf9d3f28e", "The folder specified here should contain any Gate Booking XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		public StringRegistryItem GateVehicleMovementExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("GateVehicleMovementExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"GateVehicleMovementExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("1959a63c-42b8-4e3b-b733-794351867f49", "Gate Vehicle Movements"),
						ResString.GetMultilingualString("c9f8340f-d065-4049-b905-00997d0a9d2a", "The folder specified here should contain any Gate Vehicle Movement XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#region Debtor Outstanding Balances

		public StringRegistryItem DebtorOutstandingBalancesExportDirectory
		{
			get
			{
				return GetItem<StringRegistryItem>("DebtorOutstandingBalancesExportDirectory",
					() => new LicencedStringRegistryItem(
						() => eHubMessagingRegistry.Instance.HasInterfaceConnector,
						"DebtorOutstandingBalancesExportDirectory",
						Categories.System_DataExportSettings_ExportDirectories,
						ResString.GetMultilingualString("bab1a3cd-2744-476c-925e-606234f3e7f7", "Debtor's Outstanding Balances"),
						ResString.GetMultilingualString("7be00ef2-b163-485d-95ff-4c736012a1c4",
							"The folder specified here should contain any Debtor 's Outstanding Balances XML files that are to be exported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser)
					});
			}
		}

		#endregion

		#endregion

		#region Export To Excel Format
		public CodePairRegistryItem ExportToExcelFormat
		{
			get
			{
				return GetItem("ExportToExcelFormat",
					() => new CodePairRegistryItem(
							"ExportToExcelFormat",
							Categories.System_UI,
							ResString.GetMultilingualString("AEA27223-1B40-44CE-80F4-4F6387D34039", "Export To Excel Format"),
							ResString.GetMultilingualString("63CD59B5-DF5F-4DC1-9161-ED25F3CE2CA4", "The file format of the Excel spreadsheet when Export To Excel is used. The default format is XLSX."),
							new CodeDescriptionPairListProvider(() => ExportToExcelFormatOptions()),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryOptions.Default,
							ExportToExcelFormatOptions().DefaultCode)
					);
			}
		}

		internal CodeDescriptionPairList ExportToExcelFormatOptions()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.ExportToExcelFormats.Code.Xls, Constants.ExportToExcelFormats.Description.Xls);
			result.AddPair(Constants.ExportToExcelFormats.Code.Xlsx, Constants.ExportToExcelFormats.Description.Xlsx);
			result.DefaultCode = Constants.ExportToExcelFormats.Code.Xlsx;
			return result;
		}

		#endregion

		#region Audit Logs

		public CachedRegistryItem<EnableAddEditAndDeleteLogsItemCollection> EnableAddEditAndDeleteLogsItemsRegistryItem
		{
			get
			{
				return GetItem("EnableAddEditAndDeleteLogsItems", delegate
				{
					return new CachedRegistryItem<EnableAddEditAndDeleteLogsItemCollection>("AddEditAndDeleteLogsEnabledLookup", new EnableAddEditAndDeleteLogsItemCollectionRegistryItem(
						"EnableAddEditAndDeleteLogsItems",
						Categories.System_AuditLogs,
						ResString.GetMultilingualString("e6a5c4a2-fe41-4ba8-9df3-6f3007205f12", "Enable Add/Edit/Delete Logs"),
						ResString.GetMultilingualString("a1a4dbfa-1e90-4b1f-8c78-07b983bd6332", "Control whether or not Audit Logs for add/edit/delete record operations is created for a given table."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						EnableAddEditAndDeleteLogsItemCollection.DefaultValue));
				});
			}
		}

		public BooleanRegistryItem EnableEnhancedLogging
		{
			get
			{
				return GetItem("EnableEnhancedLogging", delegate
				{
					return new BooleanRegistryItem(
						"EnableEnhancedLogging",
						Categories.System_AuditLogs,
						ResString.GetMultilingualString("f0672b46-9f12-41e8-a2d2-ee7122a88069", "Enable Enhanced Logging"),
						ResString.GetMultilingualString("4d95670d-af70-4a21-80c4-cf9b364ab5e6", "When enabled the Change Logs in the Registry will provide additional details of the changes made to Registry settings."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL)? AuditLogsEnabledFor(string tableName)
		{
			var list = EnableAddEditAndDeleteLogsItemsRegistryItem.GetCacheValue();
			if (list == null)
			{
				list = EnableAddEditAndDeleteLogsItemsRegistryItem.Value;
				EnableAddEditAndDeleteLogsItemsRegistryItem.AddCachedValue(list, DateTimeOffset.Now.AddSeconds(10));
			}
			var item = list.FindByTableName(tableName);
			return item == null ? null : item.GetAuditConfiguration();
		}
		#endregion

		#region Identity Provider

		public StringRegistryItem IdentityProviderClientID
		{
			get
			{
				return GetItem("IdentityProviderClientID", delegate
				{
					var result = new StringRegistryItem(
						"IdentityProviderClientID",
						Categories.System_IdentityProvider,
						(NoResString)"Identity Provider Client ID",
						(NoResString)"It's a permanent id coming from Azure application which represents Identity Provider.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"7a67d4a5-7751-4a64-87b6-d06802844436");
					return result;
				});
			}
		}

		public IntRegistryItem IdentityProviderTimeoutInSeconds
		{
			get
			{
				return GetItem("IdentityProviderTimeoutInSeconds", delegate
				{
					return new IntRegistryItem(
						"IdentityProviderTimeoutInSeconds",
						Categories.System_IdentityProvider_CargoWiseUserManagement,
						(NoResString)"Identity Provider Configuration Request Timeout",
						(NoResString)"Timeout is seconds when Identity Provider requests are executed.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: 5,
						minValue: 1,
						maxValue: 300
					);
				});
			}
		}

		#region CargoWise IdP User Synchronization

		public DateTimeRegistryItem IdpUserImportHighWaterMark
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("IdpUserImportHighWaterMark", delegate
				{
					var result = new DateTimeRegistryItem(
						"IdpUserImportHighWaterMark",
						Categories.System_IdentityProvider_CargoWiseUserSynchronization,
						(NoResString)"Identity Provider User Import High Water Mark",
						(NoResString)"Water mark for the IDP service task.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached | RegistryOptions.IsHidden);
					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);
					return result;
				});
			}
		}

		public StringRegistryItem IdpUserSynchronisationEndpoint
		{
			get
			{
				return GetItem("IdpUserSynchronisationEndpoint", delegate
				{
					return new StringRegistryItem(
						"IdpUserSynchronisationEndpoint",
						Categories.System_IdentityProvider_CargoWiseUserSynchronization,
						(NoResString)"Identity Provider Endpoint for creating and updating users",
						(NoResString)"A support only registry for specifying the link to the user create/update Identity Provider endpoint.",
						RegistryStorageFlags.System,
						IsUATOrDevSystem() ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden
					);
				});
			}
		}

		public BooleanRegistryItem IdpUserSynchronisationEnabled
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IdpUserSynchronisationEnabled", delegate
				{
					return new BooleanRegistryItem(
						"IdpUserSynchronisationEnabled",
						Categories.System_IdentityProvider_CargoWiseUserSynchronization,
						(NoResString)"Enable Synchronization of Staff records into the Identity Provider",
						(NoResString)"If this registry is enabled, Staff records will automatically sync with the Identity Provider.",
						RegistryStorageFlags.System,
						IsUATOrDevSystem() ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						false
						);
				});
			}
		}

		#endregion

		#endregion

		#region Staff

		#region Staff Employment Types

		public CodeDescriptionBoolRegistryItem StaffEmploymentTypes
		{
			get
			{
				return GetItem("StaffEmploymentTypes", delegate
				{
					var defaultValue = new CodeDescriptionBoolCollection { CodeMaxLength = 3 };

					defaultValue.Add(DefaultStaffEmploymentTypes.Codes.PermanentFullTime, DefaultStaffEmploymentTypes.Descriptions.PermanentFullTime, true);
					defaultValue.Add(DefaultStaffEmploymentTypes.Codes.PermanentPartTime, DefaultStaffEmploymentTypes.Descriptions.PermanentPartTime, false);
					defaultValue.Add(DefaultStaffEmploymentTypes.Codes.Casual, DefaultStaffEmploymentTypes.Descriptions.Casual, false);
					defaultValue.Add(DefaultStaffEmploymentTypes.Codes.Contractor, DefaultStaffEmploymentTypes.Descriptions.Contractor, false);
					defaultValue.Add(DefaultStaffEmploymentTypes.Codes.Student, DefaultStaffEmploymentTypes.Descriptions.Student, false);
					defaultValue.Add(DefaultStaffEmploymentTypes.Codes.Temp, DefaultStaffEmploymentTypes.Descriptions.Temp, false);
					defaultValue.Add(DefaultStaffEmploymentTypes.Codes.Director, DefaultStaffEmploymentTypes.Descriptions.Director, false);
					defaultValue.Add(DefaultStaffEmploymentTypes.Codes.Other, DefaultStaffEmploymentTypes.Descriptions.Other, false);

					return new CodeDescriptionBoolRegistryItem(
						"StaffEmploymentTypes",
						Categories.System_Staff,
						ResString.GetMultilingualString("b85213a0-b58c-442c-a2ba-51b22be369d7", "Staff Employment Types"),
						ResString.GetMultilingualString("bd2a3ad6-99ec-4688-9e0c-74c6c0e6741d", "A list of the ways a staff member may be employed in this organization."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("ea294007-7ab7-407d-8a50-28a493702e35", "Full Time")),
						defaultValue);
				});
			}
		}

		#endregion

		#region Email Types

		public CodeDescriptionPairListRegistryItem StaffEmailTypeList
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("StaffEmailTypeList", delegate
				{
					var defaultValue = new CodeDescriptionPairList();

					return new CodeDescriptionPairListRegistryItem(
						"StaffEmailTypeList",
						Categories.System_Staff,
						ResString.GetMultilingualString("4D375EAD-548B-4D79-ADA8-E684149B283A", "Staff Email Type List"),
						ResString.GetMultilingualString("0E13A691-D8E0-4A2F-B50E-26091C3B8253", "A list of email types used for staff details."),
						20,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						defaultValue)
					{ DataType = new EmailAddressTypeListRegistryDataType(20) };
				});
			}
		}

		#endregion

		#region Staff Leave Types

		public CodeDescriptionBoolRegistryItem StaffLeaveTypes
		{
			get
			{
				return GetItem<CodeDescriptionBoolRegistryItem>("StaffLeaveTypes", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(3);
					defaultValue.Add("ANN", ResString.GetMultilingualString("ff97f05c-1845-4f3a-9bb1-6d83be99f4a0", "Annual Leave"), false);
					defaultValue.Add("SIC", ResString.GetMultilingualString("ced0b4bb-14c8-4811-a17d-313a01db40cf", "Sick Leave"), false);
					defaultValue.Add("CAS", ResString.GetMultilingualString("9dcf9170-deea-43f6-bd6e-59226f63cc04", "Casual Work Days"), false);
					defaultValue.Add("MAT", ResString.GetMultilingualString("680b9b2e-601a-4c88-b879-7a338219db05", "Maternity Leave"), false);
					defaultValue.Add("COM", ResString.GetMultilingualString("fbcf0d22-48c1-4fec-b70c-a94c5727d2cd", "Compassionate Leave"), false);
					defaultValue.Add("JUR", ResString.GetMultilingualString("dda188b1-0ea9-4446-a117-e522acc642f3", "Jury Duty"), false);
					defaultValue.Add("TIL", ResString.GetMultilingualString("d61c33ca-8243-4703-9f67-ce11e4b28479", "Time in Lieu"), false);
					defaultValue.Add("WHM", ResString.GetMultilingualString("a77eea6c-96ea-435d-a000-aaf244874509", "Working from Home"), false);
					defaultValue.Add("DEF", ResString.GetMultilingualString("ff18a1b3-6994-4440-b045-2bbf2a7219d3", "Defense Service Leave"), false);
					defaultValue.Add("EMR", ResString.GetMultilingualString("77db4c57-4c87-44e3-96f0-9006a4b5061c", "Emergency Services Leave"), false);
					defaultValue.Add("FAM", ResString.GetMultilingualString("dae951e7-4799-4834-a461-8de864b6e86e", "Family and Carers Leave"), false);
					defaultValue.Add("LWP", ResString.GetMultilingualString("7eca98f6-57e8-40a2-9e94-69bda9ba4acd", "Leave without Pay"), false);
					defaultValue.Add("BLD", ResString.GetMultilingualString("6d9ff339-97fe-421b-8e77-8adfde1d9c27", "Blood Donors Leave"), false);
					defaultValue.Add("OVS", ResString.GetMultilingualString("1ba5cb4b-b13e-411d-b98c-fd438803cd9c", "Overseas Business"), false);
					defaultValue.Add("INT", ResString.GetMultilingualString("a6f62948-f42d-40c4-8987-b72d9f60a307", "Interstate Business"), false);
					defaultValue.Add("TRN", ResString.GetMultilingualString("6f8192bf-643e-45fd-96b7-75a9f640cdcf", "On Training Course"), false);
					defaultValue.Add("CON", ResString.GetMultilingualString("6a44edfc-2bb0-4128-be6c-d43bdbd85a65", "Attending Conference"), false);
					defaultValue.Add("OTH", ResString.GetMultilingualString("f8437cfc-e38b-46b8-ae50-753bee3ea051", "Other Leave Type (Please specify in comment)"), false);

					return new CodeDescriptionBoolRegistryItem(
						"StaffLeaveTypes",
						Categories.System_Staff,
						ResString.GetMultilingualString("d522ce2b-a366-4900-8181-2fde92703244", "Staff Leave Types"),
						ResString.GetMultilingualString("45cc9a9c-02a2-48ce-a2ac-d3c86cfa59e9", "A list of leave types that apply to staff members."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("a7f0ac98-7580-41a1-afcd-59dc69bec6e2", "Is Working Away"),
						defaultValue
						);
				});
			}
		}

		#endregion

		#region Staff Leave Types Alerts

		public const int LeaveTypeCodeMaxLength = 3;

		public CodeDescriptionPairListRegistryItem StaffLeaveTypeAlerts
		{
			get
			{
				return GetItem("StaffLeaveTypeAlerts", delegate
				{
					var defaultValue = new CodeDescriptionPairList();

					return new CodeDescriptionPairListRegistryItem(
						"StaffLeaveTypeAlerts",
						Categories.System_Staff,
						ResString.GetMultilingualString("4438FFFA-27B9-4DBB-A894-2DE6C7AF782C", "Staff Leave Type Alerts"),
						ResString.GetMultilingualString("C2C1AD1F-D1CF-4A7A-847B-5E8EB1C30DC4", "A list of alerts to display when the user creates leave of the corresponding type."),
						LeaveTypeCodeMaxLength,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultValue)
					{ DataType = new StaffLeaveTypeAlertsDataType(LeaveTypeCodeMaxLength) };
				});
			}
		}

#if DEBUG
		public
#endif
		class StaffLeaveTypeAlertsDataType : CodeDescriptionPairListRegistryDataType
		{
			public StaffLeaveTypeAlertsDataType(int codeMaxLength)
				: base(codeMaxLength)
			{ }

			protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
				var registryStaffLeaveTypes = Instance.StaffLeaveTypes.Value.Select(slt => (slt as CodeDescriptionBool).Code.ToString());

				for (var i = 0; i < proposedValue.Count; i++)
				{
					if (string.IsNullOrEmpty(proposedValue[i].Code))
					{
						throw new RegistryValidationException(Res.GetString("993A7EDB-6B6D-4C3E-B9DD-1752D4FA531D", "Please enter a Leave Type code at line {0}", i + 1));
					}
					else if (!registryStaffLeaveTypes.Contains(proposedValue[i].Code))
					{
						throw new RegistryValidationException(Res.GetString("8691B216-48C8-4BDD-B702-642C0CA723F0", "The following is not a valid Leave Type : {0}", proposedValue[i].Code));
					}
					else if (string.IsNullOrEmpty(proposedValue[i].Description.Trim()))
					{
						throw new RegistryValidationException(Res.GetString("17B035AA-58FF-4E2A-A113-A6649F311E67", "Please enter an Alert for Leave Type {0}", proposedValue[i].Code));
					}
				}
			}
		}

		#endregion

		#region Enable Validate Leave on Staff Edit

		public BooleanRegistryItem EnableValidateLeaveOnStaffEdit
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableValidateLeaveOnStaffEdit", delegate
				{
					return new BooleanRegistryItem(
						"EnableValidateLeaveOnStaffEdit",
						Categories.System_Staff,
						ResString.GetMultilingualString("d354bb5a-a828-4033-bda7-c8da8502e13c", "Enable Validate Leave on Staff Edit"),
						ResString.GetMultilingualString("fc057f89-ecff-418c-8a35-34875bd4cf49", "If this registry is enabled, this feature will validate Days field on Staff Leave Records."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						true
						);
				});
			}
		}

		#endregion

		#region Staff Reporting Roles

		public StaffReportingRoleRegistryItem StaffReportingRoles
		{
			get
			{
				return GetItem<StaffReportingRoleRegistryItem>("StaffReportingRoles", delegate
				{
					var defaultValue = new StaffReportingRoleCollection();
					defaultValue.Add(DefaultStaffReportingRoles.Codes.DirectManager, DefaultStaffReportingRoles.Descriptions.DirectManager, enabled: true, isMandatory: false, sharedRoleAllowed: false);

					return new StaffReportingRoleRegistryItem(
						"StaffReportingRoles",
						Categories.System_Staff,
						ResString.GetMultilingualString("882d53f3-47d9-45ba-b1bf-352beafe2b76", "Staff Reporting Roles"),
						ResString.GetMultilingualString("6219b767-528b-4418-a04f-0ca901d9839b", "A list of management roles which staff members can perform."),
						RegistryStorageFlags.System,
						defaultValue
						);
				});
			}
		}

		#endregion

		#region Two Factor Authentication Types
		public CodePairRegistryItem TwoFactorAuthenticationTypes
		{
			get
			{
				return GetItem<CodePairRegistryItem>("TwoFactorAuthenticationTypes", delegate
				{
					return new CodePairRegistryItem(
					"TwoFactorAuthenticationTypes",
					Categories.System_Staff,
					ResString.GetMultilingualString("1D2153A1-199E-4759-8786-9ABAED3F5A79", "Two Factor Authentication Types"),
					ResString.GetMultilingualString("E856F071-3F27-482C-8335-4A098E833DEB", "A list of ways that the system sends the two factor authentication codes"),
					new CodeDescriptionPairListProvider(() => TwoFactorAuthenticationOptions()),
					RegistryStorageFlags.System,
					TwoFactorAuthenticationOptions().DefaultCode);
				});
			}
		}

		internal CodeDescriptionPairList TwoFactorAuthenticationOptions()
		{
			var twoFactorAuthenticationTypes = new CodeDescriptionPairList();
			twoFactorAuthenticationTypes.AddPair((NoResString)"None", (NoResString)"None");
			twoFactorAuthenticationTypes.AddPair((NoResString)"Email", (NoResString)"Email");

			twoFactorAuthenticationTypes.DefaultCode = (NoResString)"None";
			return twoFactorAuthenticationTypes;
		}

		public BooleanRegistryItem MakeTwoFactorAuthenticationEmailHTML
		{
			get
			{
				return GetItem<BooleanRegistryItem>("MakeTwoFactorAuthenticationEmailHTML", delegate
				{
					return new BooleanRegistryItem(
					"MakeTwoFactorAuthenticationEmailHTML",
					Categories.System_Staff,
					(NoResString)"Make Two Factor Authentication Email HTML",
					(NoResString)"Sends the Two Factor Authentication Email as HTML",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
				});
			}
		}

		#endregion

		#region Staff Certificate Types

		public OverrideImmuneCodeDescriptionBoolRegistryItem StaffCertificateTypes
		{
			get
			{
				return GetItem<OverrideImmuneCodeDescriptionBoolRegistryItem>("StaffCertificateTypes", delegate
				{
					#region Default Values

					var defaultValue = new OverrideImmuneCodeDescriptionBoolCollection();

					#region Certificate Types

					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.APP, ResString.GetMultilingualString("61679b5a-c366-428d-bb89-7bfedbca4e72", "Airport Pass"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK, ResString.GetMultilingualString("f5a8d330-06c2-4e98-933b-bdfeb4720d36", "Broker"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.DBH, ResString.GetMultilingualString("f483c485-3089-4b52-840f-b8689eb3b8e4", "DBH user code"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.DGN, ResString.GetMultilingualString("a4110706-89a3-4fce-ab24-4f5ad84f25c6", "Dangerous Goods"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.DTA, ResString.GetMultilingualString("b55e1a2c-6591-435b-8807-039ac65d34d1", "AWB Security Training Accreditation Number"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.FIN, ResString.GetMultilingualString("40786082-bdf5-4833-83cd-9c000c5fbef1", "Foreign Identification Number"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.FKL, ResString.GetMultilingualString("cf22aae6-2db3-45e5-bab5-c0058e86b583", "Fork Lift"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.IAT, ResString.GetMultilingualString("fe399de1-a026-4fec-9f9c-2c631013ebbc", "IATA"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.MSC, ResString.GetMultilingualString("810e157d-e93f-4247-a768-3ba45e9f5ce4", "Miscellaneous"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.NID, ResString.GetMultilingualString("aceccc9a-047b-48ec-9ece-dbcb5beda054", "National Identity Document"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.TFN, ResString.GetMultilingualString("c6dcd3d4-fd0a-4530-ac26-dc68e72a143e", "Tax File Number"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.TRK, ResString.GetMultilingualString("df03b361-bb05-4332-b44c-9c9069647cdf", "Truck (Specify Type)"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.WKP, ResString.GetMultilingualString("e2de92f3-477d-4682-969d-0a4a3f1bb748", "Work Permit"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.BKG, ResString.GetMultilingualString("9F4F9D11-4F12-471E-AEF3-418E3862A07B", "Background Check"), true);
					defaultValue.Add(Constants.StaffDefaultCertificateIDAndTrainingTypes.PID, ResString.GetMultilingualString("c3be2e77-9192-46a5-9996-36aa92d030c0", "Payroll ID"), true);

					#endregion

					#region ID and training types

					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.BCT, ResString.GetMultilingualString("8A8BCFD4-0FF8-4527-8C29-BD2837AE84A9", "Birth Certificate"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CON, ResString.GetMultilingualString("302EC4F8-EAE6-46BE-93BD-9DEC81B25473", "Certificate of naturalization"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CDN, ResString.GetMultilingualString("24C58028-4E9F-4634-A550-266781C1F076", "Citizenship document number"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CDL, ResString.GetMultilingualString("8CD6502C-2F22-4FF4-9FEE-50E92433BA27", "Commercial driver's license"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.REP, ResString.GetMultilingualString("60AC272F-CC38-4722-BF96-031CD4FD575F", "DHS Re-entry permit"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.RTD, ResString.GetMultilingualString("53FC09B9-D737-4BE3-A95F-6E8CD963FAE9", "DHS Refugee travel document"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CAR, ResString.GetMultilingualString("606FB12F-E9A7-4B06-9482-02BB945E3EAE", "Driving license (national)"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.EDL, ResString.GetMultilingualString("9E4C5709-5CCE-4C0B-ACD8-54E00CBBB286", "Enhanced driver's license (EDI) ID"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.HZM, ResString.GetMultilingualString("95CD8BD7-D4D5-464A-8822-A534053C1D49", "Hazardous materials endorsement"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.LVC, ResString.GetMultilingualString("485BA9BF-E6BA-4A8D-AE47-2FCD4FF7CAE5", "Laser visa border crossing card"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.MID, ResString.GetMultilingualString("C3AF9001-BF4D-4FD6-8B8D-D04C7D924282", "Military ID document"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.NAI, ResString.GetMultilingualString("75F8D766-DB2B-4CCF-BE5A-1BF4C38E2B57", "Native American Indian/INAC"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.NEX, ResString.GetMultilingualString("C9D2BCC7-F6C9-42D7-98E7-1E978BD8652D", "NEXUS card"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.OTD, ResString.GetMultilingualString("9A4DE4EB-6335-4AE5-82FA-F0A118BF232E", "Other travel document"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.PAS, ResString.GetMultilingualString("172F737F-BB0F-47ED-B74D-7E145A80689A", "Passport"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.PR1, ResString.GetMultilingualString("54F5B900-8281-4DD5-BC56-E62A439E52EB", "Permanent resident card (1998-2003) C1"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.PR2, ResString.GetMultilingualString("813B89D9-382E-4B12-A34C-06F6BF402FFC", "Permanent resident card C2"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.SEN, ResString.GetMultilingualString("DE1E4E03-9E13-46A3-BD2A-4F5C447A5EF9", "Secure Electronic Network for Traveler's Rapid Inspection (SENTRI) Card"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.AR1, ResString.GetMultilingualString("DBCFC438-90A4-429D-98EA-516FE12D16B1", "U.S. alien registration card A1"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.AR2, ResString.GetMultilingualString("7EB6269B-F970-47E9-96AC-78BAAEEC6D1A", "U.S. alien registration card A2"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.MMD, ResString.GetMultilingualString("3630D96B-8FBD-4222-B7FE-2AD35FFE6118", "U.S. merchant mariner Document ID"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.USP, ResString.GetMultilingualString("F695BC02-0A7A-448E-8D1F-22CCB1E32925", "U.S. passport card"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CO1, ResString.GetMultilingualString("B80EEB21-CA96-4585-9426-5FA172ADBF49", "UK Cargo Operative"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CO2, ResString.GetMultilingualString("DB2874AF-2848-4E09-96C1-F82CD76482DB", "UK Cargo Operative Screening"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CO3, ResString.GetMultilingualString("A08AFC6C-942E-4748-A2CC-BB03DE2511D8", "UK Cargo Operative Screening - Refresher"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CS1, ResString.GetMultilingualString("AD50D398-F3A4-432F-91D5-420EBBDCA359", "UK Cargo Supervisor"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CS2, ResString.GetMultilingualString("EBB51A67-6565-4A13-BAE9-9C49EE8BBF2A", "UK Cargo Supervisor - Refresher"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CM1, ResString.GetMultilingualString("CDA4F944-B8EC-489C-9E2A-FE55A4BD49B5", "UK Cargo Manager"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.VIM, ResString.GetMultilingualString("29E1AE47-0A5D-44F3-906B-00B967E1987B", "Visa immigrant"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.VNI, ResString.GetMultilingualString("BA97E94F-D32E-4E41-98C4-4F9AF4DCE579", "Visa non-immigrant"), true);

					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.ACE, ResString.GetMultilingualString("501E9588-A2DA-46EB-B2B7-C3B0C4869D92", "ACE id"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.APC, ResString.GetMultilingualString("425B5E87-53AB-4B49-BC9E-EE264CBDC872", "ACE proximity card ID"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.CNO, ResString.GetMultilingualString("1B8D7A11-30D5-4848-81E9-C3CD89F7E206", "CN e-port operator card ID"), true);
					defaultValue.AddSystemDefined(Constants.StaffDefaultCertificateIDAndTrainingTypes.COD, ResString.GetMultilingualString("9D0AF6A6-C303-4163-8025-9848D6C085F8", "IT Italian Registration Number"), true);

					#endregion

					#endregion

					return new OverrideImmuneCodeDescriptionBoolRegistryItem(
							"StaffCertificateTypes",
							Categories.System_Staff,
							ResString.GetMultilingualString("33dc5ad4-d428-478c-98bc-38bbabe45477", "Staff Certificate Types"),
							ResString.GetMultilingualString("1b31eca8-0a6f-4ba1-82d3-55c00ffdf57c", "A customizable list of certificate, ID and training types for staff. The list also contains system defined default types."),
							RegistryStorageFlags.System,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("5612bf86-6ec3-4363-85db-05fe0f359ce4", "Enabled")),
							defaultValue);
				});
			}
		}

		#endregion

		#region OpenID Connect

		public OIDCConfigRegistryItem WinzorOIDCConfig
		{
			get
			{
				return GetItem("WinzorOIDCConfig", delegate
				{
					return new OIDCConfigRegistryItem(
						"WinzorOIDCConfig",
						Categories.System_Staff_OIDCAuthentication,
						ResString.GetMultilingualString("D6E34E2F-07E4-43E7-9362-34021C807AF6", "Web Version OpenID Connect Settings override"),
						ResString.GetMultilingualString("FFC8C899-28D2-44E8-BB3F-BE2B51334190", "CargoWise Web Version OpenID Connect Authentication settings override. If this is not set, the OpenID Connect Settings registry item will be used instead."),
						isWinzorConfig: true,
						RegistryStorageFlags.System,
						GetRegistryOptionsForOIDCConfig(),
						Business.OIDCConfig.DefaultValue);
				});
			}
		}

		public OIDCConfigRegistryItem OIDCConfig
		{
			get
			{
				return GetItem("OIDCConfig", delegate
				{
					return new OIDCConfigRegistryItem(
						"OIDCConfig",
						Categories.System_Staff_OIDCAuthentication,
						ResString.GetMultilingualString("B7B695B9-8D00-41DA-BDF8-D1AFF45CB550", "OpenID Connect Settings"),
						ResString.GetMultilingualString("58D0DBFA-18B6-458F-ACD6-897DE3C0604F", "Configuration for OpenID Connect Authentication."),
						RegistryStorageFlags.System,
						GetRegistryOptionsForOIDCConfig(),
						Business.OIDCConfig.DefaultValue);
				});
			}
		}

		public StringRegistryItem OIDCClientIDForWebApplications
		{
			get
			{
				return GetItem("OIDCClientIDForWebApplications", delegate
				{
					var result = new StringRegistryItem(
						"OIDCClientIDForWebApplications",
						Categories.System_Staff_OIDCAuthentication,
						ResString.GetMultilingualString("531C586F-75F8-4E9E-954B-5F4C32048EAA", "OIDC Client ID For Web Applications"),
						ResString.GetMultilingualString("3F3F53D2-3EC1-43C4-A34A-8C9B2B1CF55F", "It's the client ID for web applications in OpenID Connect login."),
						RegistryStorageFlags.System,
						RegistryOptions.IsReadOnly,
						string.Empty);
					return result;
				});
			}
		}

		RegistryOptions GetRegistryOptionsForOIDCConfig()
		{
			if (!EnvProxy.IsHostedWithCargowise || (Env.CurrentUser?.IsSupportUser ?? false))
			{
				return RegistryOptions.Default;
			}

			return RegistryOptions.IsReadOnly;
		}

		public BooleanRegistryItem IsOIDCFederatedWithWTG
		{
			get
			{
				return GetItem("IsOIDCFederatedWithWTG", delegate
				{
					return new BooleanRegistryItem(
						"IsOIDCFederatedWithWTG",
						Categories.System_Staff_OIDCAuthentication,
						ResString.GetMultilingualString("651FABE9-33BD-4ADE-93F0-6906D1A65F62", "Use WTG B2C for Identity Federation"),
						ResString.GetMultilingualString("C5F1DD5B-9F88-4A81-8400-FF58331B749D", "When this value is \"Yes\" it indicates that WiseTech Global web apps will federate identities with the WiseTech Global Azure B2C server. Overriding the value to \"No\" will disable federated identities using the WiseTech Global B2C"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableSupportUserLogin
		{
			get
			{
				return GetItem("EnableSupportUserLogin", delegate
				{
					return new BooleanRegistryItem(
						"EnableSupportUserLogin",
						Categories.System_Staff_OIDCAuthentication,
						ResString.GetMultilingualString("884C13AA-52E2-4B61-8A03-2B95B9E84266", "Enable support user login when OIDC authentication is enabled."),
						ResString.GetMultilingualString("1F7827DF-F6DE-41A6-9F8F-68C7005B423F", "Overriding this setting to Yes will enable the CW1 Support user to log into your system when OIDC authentication is enabled. To remove access, remember to set this setting back to No."),
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController,
						true);
				});
			}
		}

		public BooleanRegistryItem RevertToUsernameAndPasswordAuthentication
		{
			get
			{
				return GetItem("RevertToUsernameAndPasswordAuthentication", delegate
				{
					return new BooleanRegistryItem(
						"RevertToUsernameAndPasswordAuthentication",
						Categories.System_OIDC,
						(NoResString)"Revert to Username And Password Authentication",
						(NoResString)"Overriding this setting to Yes will enable reverting to use username and password authentication for WiseCloud Accessor. Note: When this setting is enabled, the WiseCloud Accessor Client and RDP Plugin on client terminals will be automatically updated to use username and password authentication if token-based authentication is disabled.",
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise || IsEdiProd ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		public BooleanRegistryItem WiseCloudAccessorTokenBasedAccess
		{
			get
			{
				return GetItem("WCATokenBasedAccess", delegate
				{
					return new BooleanRegistryItem(
						"WCATokenBasedAccess",
						Categories.System_OIDC,
						(NoResString)"WiseCloud Accessor Token Based Access",
						(NoResString)"Overriding this setting to Yes will enable token-based authentication for WiseCloud Accessor. Note: When this setting is enabled, the WiseCloud Accessor Client and RDP Plugin on client terminals will be automatically updated to use token-based authentication.",
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise || IsEdiProd ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		public StringRegistryItem DomainHint
		{
			get
			{
				return GetItem("DomainHint", delegate
				{
					var dataType = new StringRegistryDataType(true, FormattableString.Invariant($"WC_{RawDataRegistry.Instance.SystemEnterpriseCode.Value}"));

					return new StringRegistryItem(
						"DomainHint",
						Categories.System_OIDC,
						ResString.GetMultilingualString("431b0de6-d788-4002-8796-61a9797ea3fe", "Domain Hint"),
						EnvProxy.IsHostedWithCargowise || IsEdiProd ? HintOfDomainHintForHostedSystem : HintOfDomainHintForSelfHostedSystem,
						dataType,
						null,
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise || IsEdiProd
							? RegistryOptions.IsOnlyForSupport
							: RegistryOptions.Default,
						(NoResString)"Azure");
				});
			}
		}

		ResourceString HintOfDomainHintForSelfHostedSystem => ResString.GetMultilingualString("1DD6AB11-7043-4D3A-9E8F-192639767A2B", "The Domain Hint for self-hosted customers is used if you are federating identities to WiseTech Global's {0} server for CargoWise logins. If you are not federating to the WiseTech {0} please leave this as disabled.", "Azure AD B2C");

		ResourceString HintOfDomainHintForHostedSystem => ResString.GetMultilingualString("4747f7cf-81ac-4fbe-9223-1fcf14971ad3",
			@"This is a setting for WiseCloud customers who require special configuration in the {0} custom policy file.

Overriding this value will need to create a copy of a {1} configuration for the specific customer, and the value in this registry setting will be included in the {2} in the custom policy file.

This text box is always read-only by design. Overriding the default option will automatically use the prefix {3} together with the customer's three letter Enterprise Code.

This combination of {3} + three letter Enterprise Code is used to locate the custom configuration. If no customization is required, please do not override this setting.",
							"Azure AD B2C",
							"B2C",
							"<Domain> and <DisplayName>",
							"'WC_'");

		public StringArrayRegistryItem AppDomainMapping
		{
			get
			{
				return GetItem<StringArrayRegistryItem>("AppDomainMapping", delegate
				{
					var result = new StringArrayRegistryItem(
						"AppDomainMapping",
						Categories.System_OIDC,
						ResString.GetMultilingualString("44BD02C2-9D19-4C9D-98B9-3D27FD3CF631", "App Domain Mapping"),
						ResString.GetMultilingualString("58A0A9F2-80FE-48BA-BED5-33777A86FFDA", "This setting is used to enable OpenID Connect (OIDC) authentication for WiseTech Global web applications that do not have a connection to the {0} database. Add your company domain names to this list using the format @your-domain.com. Data entered here will be synchronized to a central service that will be used for the OIDC authentication flow.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System);
					result.DataType.Validating += ValidateRedirectedEmailDomains;

					return result;
				});
			}
		}

		void ValidateRedirectedEmailDomains(object sender, RegistryDataTypeValidatingEventArgs<string[]> e)
		{
			if (e.ProposedValue.IsNullOrEmpty())
			{
				return;
			}

			var expression = (NoResString)@"^@((?!-)[a-zA-Z\d\-]+(?<!-)\.)+[a-zA-Z]{2,}$";
			var re = new Regex(expression);
			foreach (var emailAddress in e.ProposedValue)
			{
				if (!re.IsMatch(emailAddress))
				{
					throw new RegistryValidationException(ResString.GetMultilingualString("A198166F-8E2F-4BE0-8FCC-6C06075665B6", "Please enter valid email domain. E.g. {0}", "@your-domain.com"));
				}
			}
		}

		#endregion

		#endregion

		#region Groups

		public CodeDescriptionPairListRegistryItem GroupCategoryList
		{
			get
			{
				return GetItem("GroupCategoryList", () =>
					new CodeDescriptionPairListRegistryItem(
						name: "GroupCategoryList",
						category: Categories.System_Groups,
						caption: ResString.GetMultilingualString("c4e9b46e-8112-40f9-a82e-11a95f2ca5cd", "Categories"),
						hint: ResString.GetMultilingualString("4db02374-8f0a-4ce3-87b2-76d546a235be", "The values that can be selected in the Category field within the Group module."),
						maxCodeLength: 3,
						storage: RegistryStorageFlags.System,
						defaultValue: new CodeDescriptionPairList())
				);
			}
		}

		public CodeDescriptionPairListRegistryItem DefectReportGroupCategoryList
		{
			get
			{
				return GetItem("DefectReportGroupCategories", () =>
					new CodeDescriptionPairListRegistryItem(
						name: "DefectReportGroupCategories",
						category: Categories.System_Groups,
						caption: ResString.GetMultilingualString("5d6cf83e-267a-46c7-bbbc-9c9dfbeac09c", "Defect Report Included Categories"),
						hint: ResString.GetMultilingualString("0733533a-a4cb-4530-a75b-60d8ef080f39", "The categories of Groups that will be included in the Defects Report."),
						maxCodeLength: 3,
						editorInfo: new CodeDescriptionPairListEditorInfo(showCodeColumn: true, showDescriptionColumn: false),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: new CodeDescriptionPairList(),
						useDefaultDefaultValue: true)
				);
			}
		}

		#endregion

		#region UserIdleWorkerEnabled
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem UserIdleWorkerEnabled
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UserIdleWorkerEnabledDevOnly", delegate
				{
					return new BooleanRegistryItem(
						"UserIdleWorkerEnabledDevOnly",
						Categories.System_Framework,
						(NoResString)"User Idle Worker Enabled (Developer Only)",
						(NoResString)"The User Idle Worker improves system responsiveness by performing tasks when the user is idle momentarily.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		bool ISystemDataRegistry.UserIdleWorkerEnabled
		{
			get { return UserIdleWorkerEnabled.Value; }
		}

		#endregion
		#endregion

		#region Workflow

		#region SuppressResourceStringsCheckRegion

		public DateTimeRegistryItem WorkflowExceptionGenerationHWM
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("WorkflowExceptionGenerationHWM", delegate
				{
					return new DateTimeRegistryItem(
						"WorkflowExceptionGenerationHWM",
						Categories.System_Workflow,
						(NoResString)"Workflow Exception Generator High Water Mark",
						(NoResString)"Workflow Exception Generator High Water Mark",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached | RegistryOptions.IsOnlyForDevelopers);
				});
			}
		}

		public DateTimeRegistryItem WorkflowFieldChangeTriggerHWM
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("WorkflowFieldChangeTriggerHWM", delegate
				{
					return new DateTimeRegistryItem(
						"WorkflowFieldChangeTriggerHWM",
						Categories.System_Workflow,
						(NoResString)"Workflow Field Change Trigger High Water Mark",
						(NoResString)"Workflow Field Change Trigger High Water Mark",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached | RegistryOptions.IsOnlyForDevelopers);
				});
			}
		}

		public BooleanRegistryItem WorkflowFieldChangeTriggersEnabled
		{
			get
			{
				return GetItem<BooleanRegistryItem>("WorkflowFieldChangeTriggersEnabled", delegate
				{
					return new BooleanRegistryItem(
						"WorkflowFieldChangeTriggersEnabled",
						Categories.System_Workflow,
						(NoResString)"Workflow Field Change Triggers Enabled",
						(NoResString)"Workflow Field Change Triggers Enabled",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public IntRegistryItem WorkflowFieldChangeClearProcessedLogsAfterHours
		{
			get
			{
				return GetItem("WorkflowFieldChangeClearProcessedLogsAfterHours", delegate
				{
					return new IntRegistryItem(
						"WorkflowFieldChangeClearProcessedLogsAfterHours",
						Categories.System_Workflow,
						ResString.GetMultilingualString("4abcd5a8-5b53-40fe-ae7b-c869bf467759", "Clear processed Workflow Field Change Logs"),
						ResString.GetMultilingualString("c262621e-13af-4a40-bb22-e0d15bf0f07f", "The duration for which processed logs will be kept by the system in hours."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						24 * 7, 1, 1000);
				});
			}
		}
		#endregion

		#endregion

		#region Number Of Factories Needed For Warning Report

		#region SuppressResourceStringsCheckRegion

		public IntRegistryItem NumberOfFactoriesNeededForWarningReport
		{
			get
			{
				return GetItem("NumberOfFactoriesNeededForWarningReport", delegate
				{
					return new IntRegistryItem(
						"NumberOfFactoriesNeededForWarningReport",
						Categories.System_Miscellaneous,
						(NoResString)"Number Of Factories Needed For Warning Report",
						(NoResString)"Defines the number of factories that need be counted for before sending an internal report. 0 means no report will be sent.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						0);
				});
			}
		}

		#endregion

		#endregion

		#region MDMAdministrationPanel

		public BooleanRegistryItem MdmAdministrationPanelEnabled
		{
			get
			{
				return GetItem("MdmAdministrationPanelEnabled", delegate
				{
					return new BooleanRegistryItem(
						"MdmAdministrationPanelEnabled",
						Categories.MDM_Administration,
						(NoResString)"Administration Panel Enabled",
						(NoResString)"Enable the Administration Panel.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem MdmAdministrationPanelAddressTabEnabled
		{
			get
			{
				return GetItem("MdmAdministrationPanelAddressTabEnabled", delegate
				{
					return new BooleanRegistryItem(
						"MdmAdministrationPanelAddressTabEnabled",
						Categories.MDM_Administration,
						(NoResString)"Administration Panel Address Tab Enabled",
						(NoResString)"Enable the Administration Panel Address Tab.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem MdmAdministrationPanelDuplicatesTabEnabled
		{
			get
			{
				return GetItem("MdmAdministrationPanelDuplicatesTabEnabled", delegate
				{
					return new BooleanRegistryItem(
						"MdmAdministrationPanelDuplicatesTabEnabled",
						Categories.MDM_Administration,
						(NoResString)"Administration Panel Duplicates Tab Enabled",
						(NoResString)"Enable the Administration Panel Duplicates Tab.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem MdmAdministrationPanelOrganizationDuplicatesTabEnabled
		{
			get
			{
				return GetItem("MdmAdministrationPanelOrganizationDuplicatesTabEnabled", delegate
				{
					return new BooleanRegistryItem(
						"MdmAdministrationPanelOrganizationDuplicatesTabEnabled",
						Categories.MDM_Administration,
						(NoResString)"Administration Panel Organization Duplicates Tab Enabled",
						(NoResString)"Enable the Administration Panel Organization Duplicates Tab.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem MdmAdministrationPanelPersonDuplicatesTabEnabled
		{
			get
			{
				return GetItem("MdmAdministrationPanelPersonDuplicatesTabEnabled", delegate
				{
					return new BooleanRegistryItem(
						"MdmAdministrationPanelPersonDuplicatesTabEnabled",
						Categories.MDM_Administration,
						(NoResString)"Administration Panel Person Duplicates Tab Enabled",
						(NoResString)"Enable the Administration Panel Person Duplicates Tab.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem MdmAdministrationPanelDashBoardEnabled
		{
			get
			{
				return GetItem("MdmAdministrationPanelDashBoardTabEnabled", delegate
				{
					return new BooleanRegistryItem(
						"MdmAdministrationPanelDashBoardTabEnabled",
						Categories.MDM_Administration,
						(NoResString)"Administration Panel Dashboard Tab Enabled",
						(NoResString)"Enable the Administration Panel Dashboard Tab.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public IntRegistryItem MaximumNumberOfAddressesForMDMAddressGrid
		{
			get
			{
				return GetItem("MaximumNumberOfAddressesForMDMAddressGrid", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfAddressesForMDMAddressGrid",
						Categories.MDM_Administration,
						ResString.GetMultilingualString("a0b5b126-8a8e-fcbd-4688-c55c31e657ba", "Maximum Number Of Records To Display on Address Grid"),
						ResString.GetMultilingualString("d8ff13aa-3697-2ab0-4b51-b17a6e2594bb", "The maximum number of records to display on the address filter grid (from 1 to 1000)."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						100, 1, 1000);
				});
			}
		}

		public IntRegistryItem MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid
		{
			get
			{
				return GetItem("MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfOrganizationsForMDMDuplicatesOrganizationsGrid",
						Categories.MDM_Administration,
						ResString.GetMultilingualString("19b5de8d-c68e-a281-45aa-14969ffe6963", "Maximum Number Of Records To Display On Duplicates Organizations Grid"),
						ResString.GetMultilingualString("1b61455d-b018-4383-428c-a1e1a9c54992", "The maximum number of records to display on the duplicates Organizations filter grid (from 1 to 1000)."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						100, 1, 1000);
				});
			}
		}

		public BooleanRegistryItem BackgroundValidationSuspended
		{
			get
			{
				return GetItem("BackgroundValidationSuspended", delegate
				{
					return new BooleanRegistryItem(
						"BackgroundValidationSuspended",
						Categories.MDM_Administration,
						(NoResString)"Background Validation Suspended",
						(NoResString)"Suspend background validation when open Administration Panel Address Tab.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Persons
		public BooleanRegistryItem PersonsEnableDuplicateDetection
		{
			get
			{
				return GetItem("PersonsEnableDuplicateDetection", delegate
				{
					return new BooleanRegistryItem(
						"PersonsEnableDuplicateDetection",
						Categories.Persons_Duplicate_Detection,
						(NoResString)"Enable Duplicate Detection",
						(NoResString)"When this registry is set to 'Yes', the system will show a warning when it detects that a potential duplicate is being added.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public IntRegistryItem PersonsDuplicateDetectionTimeout
		{
			get
			{
				return GetItem("PersonsDuplicateDetectionTimeout", delegate
				{
					const int defaultValue = 10;
					const int minValue = 1;
					const int maxValue = 300;

					return new IntRegistryItem(
						"PersonsDuplicateDetectionTimeout",
						Categories.Persons_Duplicate_Detection,
						ResString.GetMultilingualString("dd045958-1544-45a3-8556-2e1c62a4d307", "Duplicate Detection Timeout"),
						ResString.GetMultilingualString("85a4d897-bc66-4d92-a9d5-8add0642081f", "Time in seconds representing timeout for duplicate detection (from 1 to 300)."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue,
						minValue,
						maxValue);
				});
			}
		}

		public IntRegistryItem PersonsMaximumPotentialTargets
		{
			get
			{
				return GetItem("PersonsMaximumPotentialTargets", delegate
				{
					const int defaultValue = 150;
					const int minValue = 50;
					const int maxValue = 400;

					return new IntRegistryItem(
						"PersonsMaximumPotentialTargets",
						Categories.Persons_Duplicate_Detection,
						ResString.GetMultilingualString("8608af84-b95b-4b50-ba24-25c1fb8d98f3", "Maximum Potential Targets"),
						ResString.GetMultilingualString("84b93723-69b6-43d4-a511-1850a245315f", "The maximum number of potential targets that are loaded during duplicate detection (from 50 to 400)."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue,
						minValue,
						maxValue);
				});
			}
		}

		public BooleanRegistryItem PersonsExcludeInactivePotentialDuplicates
		{
			get
			{
				return GetItem("PersonsExcludeInactivePotentialDuplicates", delegate
				{
					return new BooleanRegistryItem(
						"PersonsExcludeInactivePotentialDuplicates",
						Categories.Persons_Duplicate_Detection,
						ResString.GetMultilingualString("AF703DF2-D9FF-495B-8BED-FBE3E73D25BE", "Exclude Inactive Potential Duplicate Records"),
						ResString.GetMultilingualString("2D55E4C6-9D00-4F2C-A2C1-D95868BDE000", @"When this registry is set to 'Yes', The system will exclude all inactive records from the list of potential duplicate results."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public CodePairRegistryItem PersonsDeduplicationMinimumConfidenceResult
		{
			get
			{
				return GetItem<CodePairRegistryItem>("PersonsDeduplicationMinimumConfidenceResult", delegate
				{
					var deDuplicationMinimumConfidenceRatingListProvider = new CodeDescriptionPairListProvider(() => new DeDuplicationMinimumConfidenceRating());
					return new CodePairRegistryItem(
						"PersonsDeduplicationMinimumConfidenceResult",
						Categories.Persons_Duplicate_Detection,
						ResString.GetMultilingualString("DD598AF4-17FE-4B01-84A3-CAFAF4C08D88", "Minimum Confidence Rating"),
						ResString.GetMultilingualString("F2C5C29F-E12F-4638-8349-8980677FF3AB", "Only potential duplicates where the confidence is higher than the selected value will be shown. If Medium is selected only High confidence results will be shown."),
						deDuplicationMinimumConfidenceRatingListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(deDuplicationMinimumConfidenceRatingListProvider, true),
						RegistryStorageFlags.All,
						RegistryOptions.Default,
						DeDuplicationMinimumConfidenceRating.Codes.Low,
						false);
				});
			}
		}

		public IntRegistryItem PersonsRepeatedValueLimit
		{
			get
			{
				return GetItem("PersonsRepeatedValueLimit", delegate
				{
					return new IntRegistryItem(
						"PersonsRepeatedValueLimit",
						Categories.Persons_Duplicate_Detection,
						ResString.GetMultilingualString("8b8ba069-e8b5-8995-45a7-ec059bd15233", "Repeated Pattern Limit"),
						ResString.GetMultilingualString("59f3ad7a-92eb-16b1-4ce6-bb98755ed333", "Any pattern that is repeated more than the specified number of times will be excluded from duplicate detection. The value can be set between 1 and 200."),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						10,
						1,
						200);
				});
			}
		}

		#region Person Merge

		public PersonMergePreviewItemCollectionRegistryItem PersonMergePreviewItemsRegistryItem
		{
			get
			{
				return GetItem("PersonMergePreviewItems", delegate
				{
					return new PersonMergePreviewItemCollectionRegistryItem(
						"PersonMergePreviewItems",
						Categories.Persons_PersonMerge,
						ResString.GetMultilingualString("c1aa728a-a8e3-4117-8299-8c68c330bc9d", "Person Merge Preview Items"),
						ResString.GetMultilingualString("bbbc1e1e-1d73-4361-6e44-e593adeddd0c", "This registry item specifies the order and visibility for properties that should be displayed on the Person Merge Preview form."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						PersonMergePreviewItemCollection.DefaultValue);
				});
			}
		}

		public NotificationEmailTemplateRegistryItem PersonMergeWithPasswordNotificationEmailTemplate
		{
			get
			{
				return GetItem("PersonMergeWithPasswordNotificationEmailTemplate", delegate
				{
					return new NotificationEmailTemplateRegistryItem("PersonMergeWithPasswordNotificationEmailTemplate",
						Categories.Persons_PersonMerge,
						ResString.GetMultilingualString("7b6155a9-8af5-4fe0-802f-93290d56ded4", "Person Merge With Password Notification Email Template"),
						ResString.GetMultilingualString("c7e1d9a0-7eb5-4a37-aa02-827fcc31cbe5", "Template that will be used to notify users their accounts have been merged under a single password."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocPersonMergeEmailSender>(),
						(NoResString)"(*PersonName*) Web Access Accounts Merged",
						PersonMergeWithPasswordNotificationEmailTemplateDefaultBody);
				});
			}
		}

		ZString PersonMergeWithPasswordNotificationEmailTemplateDefaultBody => (NoResString)@"<font face=""Arial"">
	<div>
		<strong><br></strong>
	</div>
	<div>
		<strong>(*PersonName*) Web Access Accounts Merged</strong>
	</div>
	<div>
		<br>
	</div>
	<div>
		<font face=""Arial"" size=""2""/>
		<font face=""Arial"" size=""2"">Hi (*PersonName*),<br></font>
	</div>
	<div>
		<br>
			<font face=""Arial"" size=""2""/>
			<font face=""Arial"" size=""2"">
				Your login accounts for the below organizations have been merged by a Personal Recovery Email match or via an administrative action taken by a specialist.
				<br>
				<br>
				Your existing password previously set for the following accounts will be retained as your primary password:
				<br>
				<br>
				(*AccountsWithRetainedPasswordTable*)
				<br>
				<br>
				The following accounts have now been merged into your Person user account, meaning all of the following accounts can now be accessed using the retained primary password:
				<br>
				<br>
				(*MergedAccountsTable*)
				<br>
				<br>
				If you do not remember your password and wish to set a new one, you can do so using the reset password functionality via our website.
				<br>
			</font>
	</div>
	<div>
		<font face=""Arial"" size=""2"">
			<Br>
			<font face=""Arial"" size=""2"">
				<strong>Important</strong>:
				<div>
					<font size=""2"">This email is a system generated email, initiated by a system merge of your system user account, a request action taken by you through a self identification/merge process, or by a specialist.</font>
				</div>
			</font>
		</font>
	</div>
</font>";

		#endregion

		#region Employment Types

		public CodeDescriptionPairListRegistryItem PersonEmploymentTypes
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("PersonEmploymentTypes", delegate
				{
					var defaultValue = new CodeDescriptionPairList();

					defaultValue.AddPair(DefaultStaffEmploymentTypes.Codes.PermanentFullTime, DefaultStaffEmploymentTypes.Descriptions.PermanentFullTime);
					defaultValue.AddPair(DefaultStaffEmploymentTypes.Codes.PermanentPartTime, DefaultStaffEmploymentTypes.Descriptions.PermanentPartTime);
					defaultValue.AddPair(DefaultStaffEmploymentTypes.Codes.Casual, DefaultStaffEmploymentTypes.Descriptions.Casual);
					defaultValue.AddPair(DefaultStaffEmploymentTypes.Codes.Contractor, DefaultStaffEmploymentTypes.Descriptions.Contractor);
					defaultValue.AddPair(DefaultStaffEmploymentTypes.Codes.Student, DefaultStaffEmploymentTypes.Descriptions.Student);
					defaultValue.AddPair(DefaultStaffEmploymentTypes.Codes.Temp, DefaultStaffEmploymentTypes.Descriptions.Temp);
					defaultValue.AddPair(DefaultStaffEmploymentTypes.Codes.Director, DefaultStaffEmploymentTypes.Descriptions.Director);
					defaultValue.AddPair(DefaultStaffEmploymentTypes.Codes.Other, DefaultStaffEmploymentTypes.Descriptions.Other);

					return new CodeDescriptionPairListRegistryItem(
						"PersonEmploymentTypes",
						Categories.Persons,
						ResString.GetMultilingualString("2a4e27cf-e796-4aba-affb-a0ef15550010", "Person Employment Types"),
						ResString.GetMultilingualString("f6c27cb2-c694-47f7-bad5-a6adc38247c4", "A list of the ways a person may be employed."),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultValue);
				});
			}
		}

		#endregion

		#endregion

		#region PersonIntelligenceModule

		public BooleanRegistryItem PersonIntelligenceModuleEnabled
		{
			get
			{
				return GetItem("PersonIntelligenceModuleEnabled", delegate
				{
					return new BooleanRegistryItem(
						"PersonIntelligenceModuleEnabled",
						Categories.Persons,
						ResString.GetMultilingualString("FB465EFC-9873-45EA-8283-402A62F632CC", "Person Intelligence Module Enabled"),
						ResString.GetMultilingualString("5ED21A17-2CAC-4396-B512-0290F0F3D124", "Enable the Person Intelligence Module."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}
		#endregion

		#region Geography

		public CodeDescriptionPairListRegistryItem UserDefinedGeographyType
		{
			get
			{
				return GetItem("UserDefinedGeographyType", delegate
				{
					return new CodeDescriptionPairListRegistryItem(new RegistryItemImpl(
						"UserDefinedGeographyType",
						Categories.Geography,
						ResString.GetMultilingualString("211ED2FB-4ACE-4737-9ABE-5B0DC36FD2B5", "User Defined Geography Type"),
						ResString.GetMultilingualString("2B5E451A-457A-4C25-9860-B98EEE79126C", "Allow user to customize the geography type. The user defined geography code should start with 'U' and consist of 4 numbers or letters."),
						new UserDefinedGeographyTypeRegistryDataType()
						{
							AllowDuplicateCodes = false,
							AllowEmptyCodes = false,
						},
						new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						new CodeDescriptionPairList()
					), true, new CodeDescriptionPairList());
				});
			}
		}

		public IntRegistryItem MaximumGeographyPointNumberLimit
		{
			get
			{
				return GetItem("MaximumGeographyPointNumberLimit", delegate
				{
					return new IntRegistryItem(
						"MaximumGeographyPointNumberLimit",
						Categories.Geography,
						(NoResString)"Maximum Geography Point Number Limit",
						(NoResString)"Limit the maximum point number of a KML file.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						10000)
					{ DataType = new IntRegistryDataType(1, 10000) };
				});
			}
		}

		public BooleanRegistryItem GeographyUnreleasedFunctions
		{
			get
			{
				return GetItem("GeographyUnreleasedFunctions", delegate
				{
					return new BooleanRegistryItem(
						"GeographyUnreleasedFunctions",
						Categories.Geography,
						(NoResString)"Enable Geography Unreleased Functions",
						(NoResString)"When turned on, the user can use geography unreleased functions, such as create new shape geography.",
						RegistryStorageFlags.System,
						IsEdiProd ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Enrichment

		public StringRegistryItem EnrichmentWebServiceAddress
		{
			get
			{
				return GetItem("EnrichmentWebServiceAddress", delegate
				{
					return new StringRegistryItem(
						"EnrichmentWebServiceAddress",
						Categories.Enrichment,
						(NoResString)"Enrichment Web Service Address",
						(NoResString)"URL for the Enrichment Web Service.",
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Url),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						string.Empty);
				});
			}
		}

		#endregion

		#region Resource Types

		public CodeDescriptionBoolRegistryItem ResourceTypes
		{
			get
			{
				return GetItem<CodeDescriptionBoolRegistryItem>("ResourceTypes", delegate
				{
					CodeDescriptionBoolCollection defaultValue = new CodeDescriptionBoolCollection(3);
					defaultValue.Add("CPU", ResString.GetMultilingualString("982a6e84-6b8e-4685-b4c7-723c7cc8f7e1", "Computer"), false);
					defaultValue.Add("LAP", ResString.GetMultilingualString("4cb80d27-2811-436e-b37d-5ca6b0464675", "Laptop"), false);
					defaultValue.Add("PRJ", ResString.GetMultilingualString("d5bac2cb-5d38-4974-bd0d-1e7a61c29a81", "Computer Projector"), false);
					defaultValue.Add("OPR", ResString.GetMultilingualString("ca6c1ef7-de65-4632-ba29-f1f39a19b4be", "Overhead Projector"), false);
					defaultValue.Add("PRS", ResString.GetMultilingualString("1c66cedb-deb3-4acb-a66d-b76571dec143", "Projector Screen"), false);
					defaultValue.Add("PRN", ResString.GetMultilingualString("b0ea9241-2350-43a0-a082-7854d47e1f06", "Printer"), false);
					defaultValue.Add("ROM", ResString.GetMultilingualString("e0b821e7-5fa9-46ea-932f-755838b06814", "Meeting Room"), true);

					return new CodeDescriptionBoolRegistryItem(
						"ResourceTypes",
						Categories.System_Resources,
						ResString.GetMultilingualString("541f1e7e-be2e-420a-bed6-789f542239f6", "Resource Types"),
						ResString.GetMultilingualString("fbf8c0f2-e8a1-46e2-a440-38174e1563b3", "A list of resource types."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("34389bf2-a408-4e7a-9673-90a9ee56588f", "Meeting Location")),
						defaultValue);
				});
			}
		}

		#endregion

		#region Custom Notes

		public CustomNoteTypesRegistryItem CustomNotes
		{
			get
			{
				return GetItem<CustomNoteTypesRegistryItem>("CustomNotes", delegate
				{
					return new CustomNoteTypesRegistryItem(
					"CustomNotes",
					Categories.System_UI,
					ResString.GetMultilingualString("bd0ccdd8-da97-44e1-950e-f442f666dd90", "Custom Defined Note Types"),
					CustomNoteTypeDescription,
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue);
				});
			}
		}

		public static MultilingualString CustomNoteTypeDescription
		{
			get { return ResString.GetMultilingualString("d9fc9ec7-0f0b-4aca-b411-42d8a419b8b3", "This module allows you to specify a list of your own note types that {0} operators can select, instead of using the \"Custom Description\" flag on a note. You can define any new custom note types here by selecting a module from the tree on top, and entering the new note-types in the grid shown at the bottom.\r\n\r\n Custom note types can only be defined at a module level for all countries/regions installed on this system. Note that for Brokerage modules (i.e: Operations --> Customs) note types can be defined individually by country/region as well. (e.g.: Operations --> Customs --> Customs Declaration --> All Countries/Regions --> Australia).\r\n\r\n The information that can be specified for each note type is as follows:\r\n ' Description: This is the name of the note as displayed in the list of notes available for a module.\r\n ' Visibility: This defines the default visibility of any new notes entered of this note type.\r\n ' Text Only: This flag indicates whether the operator is allowed to embed pictures, formatting, documents and other extra information into the note.\r\n ' Is Appending Note: If set, this flag will provide the note with special behavior allowing operators to append details to a single note, but not edit any already existing note information. This allows operators to append information to a note, where creating multiple notes of the same not type is not allowed. (nb: If set, the note MUST be a text-only note type).\r\n ' Non-Editable: If set, this flag will not allow operators to edit a note once it has been saved, preventing operators from changing the information in a note after saving for the first time.", Core.Constants.ProductName); }
		}

		#endregion

		#region Customer Service

		public CodeDescriptionPairListRegistryItem CustomerStatuses
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("CustomerStatuses", delegate
				{
					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();

					defaultValue.AddPair("UDF", ResString.GetMultilingualString("Registry|CustomerStatuses|UndefinedStatusDescription", "You can configure these statuses in the registry at {0}", "System > Registry > System > Customer Service > Customer Statuses"));

					return new CodeDescriptionPairListRegistryItem(
						"CustomerStatuses",
						Categories.System_CustomerService,
						ResString.GetMultilingualString("33e410ab-1963-4610-8138-25283a1dc111", "Customer Statuses"),
						ResString.GetMultilingualString("432aedfe-b735-449f-9f3c-c1c91f3ba0bc", "A list of customer statuses which can be set in Customer Service Request"),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						defaultValue);
				});
			}
		}

		public CodePairRegistryItem CustomerServiceRequestNotificationRecipientsSetting
		{
			get
			{
				return GetItem<CodePairRegistryItem>("CustomerServiceRequestNotificationRecipientsSetting", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var lookupList = new CodeDescriptionPairList();
						lookupList.AddPair("APP", ResString.GetMultilingualString("416127e4-9c90-45f3-962f-311889d6b259", "Approved By Staff"));
						lookupList.AddPair("RPT", ResString.GetMultilingualString("8ee76eff-6438-4eec-9e00-10503a4b2559", "Reported By Staff"));
						lookupList.AddPair("ALL", ResString.GetMultilingualString("a5d0560a-c2ea-4e66-86df-1eea43e47f4f", "All parties"));
						lookupList.AddPair("THI", ResString.GetMultilingualString("474fcbf8-11ee-4f82-bcfd-3ffca7ed2f14", "Third Party Notify Only"));
						return lookupList;
					});

					return new CodePairRegistryItem(
						"CustomerServiceRequestNotificationRecipientsSetting",
						Categories.System_CustomerService,
						(NoResString)"Customer Service Request Notification Recipients Setting",
						(NoResString)"This setting allows you to set recipient(s) to receive notification of a customer service request.\r\n\r\nNote: If you are setting \"Third Party Notify Only\" as a default, the field \"Third Party Notify\" on the service request will be Mandatory. With any other set default, the \"Third Party Notify\" specified on service request will always receive a notification.",
						lookUpListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						"ALL");
				});
			}
		}

		public StringRegistryItem ELearningUrl
		{
			get
			{
				return GetItem("ELearningUrl", delegate
				{
					var result = new StringRegistryItem(
						"ELearningUrl",
						Categories.System_CustomerService,
						(NoResString)"eLearning URL",
						(NoResString)"URL of the eLearning site.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						"http://www.cargowise.com/eLearning.aspx");

					return result;
				});
			}
		}

		public StringRegistryItem HowToDocumentERequestIncidentUrl
		{
			get
			{
				return GetItem("HowToDocumentERequestIncidentUrl", delegate
				{
					var result = new StringRegistryItem(
						"HowToDocumentERequestIncidentUrl",
						Categories.System_CustomerService,
						(NoResString)"How To Document eRequest Incident URL",
						(NoResString)"URL of how to document eRequest incident site.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						"http://www.cargowise.com/Documents/UserGuides/HowTo/How-To%20document%20your%20eRequest%20Incident.pdf");

					return result;
				});
			}
		}

		#endregion

		#region ReportConcurrencyErrors
		#region SuppressResourceStringsCheckRegion

		bool ISystemDataRegistry.ReportConcurrencyErrors
		{
			get { return ReportConcurrencyErrors.Value; }
		}

		public BooleanRegistryItem ReportConcurrencyErrors
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ReportConcurrencyErrors", delegate
				{
					return new BooleanRegistryItem(
						"ReportConcurrencyErrors",
						Categories.System_Database_UserOptions,
						(NoResString)"Report Concurrency Errors",
						(NoResString)"Report data concurrency errors to CargoWise.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		#endregion
		#endregion

		#region Settings for the reporting database

		string[] ISystemDataRegistry.GetReportingDbServerNames()
		{
			return ReportingDbServerNames.Value;
		}

		bool ISystemDataRegistry.UseReportingDbServerNames => UseReportingDbServerNames.Value;

		public BooleanRegistryItem UseReportingDbServerNames
		{
			get
			{
				return GetItem("UseReportingDbServerNames", delegate
				{
					var result = new BooleanRegistryItem(
					"UseReportingDbServerNames",
					Categories.System_Reports,
					(NoResString)"Use reporting DB server name",
					(NoResString)"Use reporting DB server name",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden,
					!EnvProxy.IsHostedWithCargowise
					);

					return result;
				});
			}
		}

		public StringArrayRegistryItem ReportingDbServerNames
		{
			get
			{
				return GetItem<StringArrayRegistryItem>("ReportingDBServerName", delegate
				{
					var result = new StringArrayRegistryItem(
						"ReportingDBServerName",
						Categories.System_Reports,
						ResString.GetMultilingualString("d939ab87-bde9-4c61-a714-1c98b7901527", "Reporting databases full server names"),
						ResString.GetMultilingualString("5656DA0A-E961-429F-84A2-BB544E0E5A8E", "The full server names of the reporting databases, including the instance names if applicable."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted);
					result.DataType.MaximumLength = 128;
					return result;
				});
			}
		}

		public IntRegistryItem MaxActiveScheduledReportsWarningThreshold
		{
			get
			{
				return GetItem("MaxActiveScheduledReportsWarningThreshold", delegate
				{
					return new IntRegistryItem(
						"MaxActiveScheduledReportsWarningThreshold",
						Categories.System_Reports,
						ResString.GetMultilingualString("08e603f8-b611-47a8-8b56-0d3226df44fa", "Maximum Active Scheduled Reports Warning Threshold"),
						ResString.GetMultilingualString("4bb7bf66-99c6-4b77-ac68-794a79944bb7", @"This setting manages how many Scheduled Reports have been set with a similar Next Run Time.
It is possible to experience processing delays when there are too many reports with similar run times."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						0);
				});
			}
		}

		TimeSpan ISystemDataRegistry.ReportingDbServerThreshold
		{
			get { return ReportingDbServerThreshold; }
		}

		public TimeSpan ReportingDbServerThreshold
		{
			get
			{
				return TimeSpan.FromMinutes(ReportingDbServerThresholdCore.Value);
			}
		}

		public IntRegistryItem ReportingDbServerThresholdCore
		{
			get
			{
				return GetItem<IntRegistryItem>("ReportingDbServerThreshold", delegate
				{
					return new IntRegistryItem(
					"ReportingDbServerThreshold",
					Categories.System_Reports,
					ResString.GetMultilingualString("d9b7ab4f-4301-4f95-85db-4debe055603b", "Old data threshold"),
					ResString.GetMultilingualString("414336b7-d7f0-4083-abc6-1ffbc49e71a3", @"The threshold in minutes of how old the data can be on the replica server. Set '0' for using primary server."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					10);
				});
			}
		}

		int ISystemDataRegistry.ReportMaxConnections
		{
			get { return ReportMaxConnections.Value; }
		}

		public IntRegistryItem ReportMaxConnections
		{
			get
			{
				return GetItem<IntRegistryItem>("MaximumConcurrentReports", delegate
				{
					return new IntRegistryItem(new RegistryItemImpl(
					"MaximumConcurrentReports",
					Categories.System_Reports,
					ResString.GetMultilingualString("76332298-5707-41d2-9e9d-27970c35de12", "Maximum concurrent running reports"),
					ResString.GetMultilingualString("d022a54c-6fb2-4cc5-8613-2465570f6f86", @"This is the maximum number of reports allowed to run concurrently. '0' denotes unlimited.
Note: This limit affects reports both run locally and generated via the SRR service task.
You can control whether reports are generated locally or via the service task with Registry Setting: Documents > Background Report Delivery.
Previewing of a report will contribute towards this limit."),
					new ReportMaxConnectionsDataType(),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					0));
				});
			}
		}

		public IntRegistryItem SRRMaximumTimeElapsed
		{
			get
			{
				return GetItem<IntRegistryItem>("SRRMaximumTimeElapsed", delegate
				{
					return new IntRegistryItem(new RegistryItemImpl(
					"SRRMaximumTimeElapsed",
					Categories.System_Reports,
					ResString.GetMultilingualString("c60986c6-cd5a-459e-9ac9-e68f614e5e2a", "Scheduled Report Timeout"),
					ResString.GetMultilingualString("9cfd112d-2ae6-4a64-b198-0ec90461d659", @"Maximum amount of time (in seconds) a Scheduled Report is allowed to run for before being aborted. Set to 0 to disable."),
					new ReportMaxConnectionsDataType(),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					0));
				});
			}
		}

		public IntRegistryItem SRRMaximumMemory
		{
			get
			{
				return GetItem<IntRegistryItem>("SRRMaximumMemory", delegate
				{
					return new IntRegistryItem(new RegistryItemImpl(
					"SRRMaximumMemory",
					Categories.System_Reports,
					ResString.GetMultilingualString("83690394-930c-4d10-bc16-49b3d73c09cf", "Scheduled Report Memory Limit (GB)"),
					ResString.GetMultilingualString("187b1e88-e06c-4d68-ae7d-25deb0379a63", @"Maximum amount of memory (in gigabytes) a Scheduled Report is allowed to consume before being aborted. Set to 0 to disable."),
					new ReportMaxConnectionsDataType(),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					0));
				});
			}
		}

		public IntRegistryItem SRRMaximumRetryCount
		{
			get
			{
				return GetItem<IntRegistryItem>("SRRMaximumRetryCount", delegate
				{
					return new IntRegistryItem(new RegistryItemImpl(
					"SRRMaximumRetryCount",
					Categories.System_Reports,
					ResString.GetMultilingualString("a4b95112-ef65-48b3-aa59-7da948a59346", "Scheduled Report Failure Threshold"),
					ResString.GetMultilingualString("247caab6-ab2c-4d0e-b85f-b4ec22823113", @"Maximum number of times that a report can fail before it is disabled. Set to 0 to disable."),
					new ReportMaxConnectionsDataType(),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					3));
				});
			}
		}

		public CodePairRegistryItem SRRErrorNotificationOptions
		{
			get
			{
				return GetItem("SRRErrorNotificationOptions", () =>
				{
					return new CodePairRegistryItem("SRRErrorNotificationOptions",
						Categories.System_Reports,
						ResString.GetMultilingualString("983573F6-E532-4922-B730-B82315A3829F", "Error Notification Options"),
						ResString.GetMultilingualString("A4F5A17A-BC37-415D-9D65-527080458696", "This setting determines the recipients for error notifications for reports."),
						OLookUpEditType.SRRErrorNotificationOptions,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Enterprise.Core.Constants.ErrorNotificationOptions.Code.DEF)
					{
						DataType = new SRRErrorNotificationOptionsRegistryDataType()
					};
				});
			}
		}

		public CodeDescriptionBoolRegistryItem SRRErrorNotificationStaffRoles
		{
			get
			{
				return GetItem<CodeDescriptionBoolRegistryItem>("SRRErrorNotificationStaffRoles", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"SRRErrorNotificationStaffRoles",
						Categories.System_Reports,
						ResString.GetMultilingualString("AEB6888D-75EF-41D3-A6E1-05A7D490A595", "Error Notification Staff Roles"),
						ResString.GetMultilingualString("E57947D0-3C8B-4182-8F97-6D4ECF361FE0", "The staff roles who will receive error notification emails."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("7F32145C-D521-4F1D-8177-AF24D1A5F465", "Send Notification To"), true, true),
						GetStaffRolesDefaultValue());
				});
			}
		}

		CodeDescriptionBoolDisallowNewCollection GetStaffRolesDefaultValue() => StaffRolesNotificationHelper.GetRoles();

		public GuidRegistryItem SRRErrorNotificationGroups
		{
			get
			{
				return GetItem("SRRErrorNotificationGroups", delegate
				{
					return new GuidRegistryItem(
						"SRRErrorNotificationGroups",
						Categories.System_Reports,
						ResString.GetMultilingualString("6A1D2308-9799-454C-8AFE-95FCE8FF9C1D", "Error Notification Groups"),
						ResString.GetMultilingualString("12E2D05F-2858-443F-8EF0-A8256A316E9B", "The Group that will be notified for error notifications related to reports."),
						RegistryStorageFlags.System)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					};
				});
			}
		}

		#endregion

		#region DB Health Check

		public DbHealthWarningListRegistryItem LastDbHealthCheckWarningList
		{
			get
			{
				return GetItem<DbHealthWarningListRegistryItem>("LastDbHealthCheckWarningList", delegate
				{
					DbHealthWarningListRegistryItem result = new DbHealthWarningListRegistryItem(
						"LastDbHealthCheckWarningList",
						Categories.System_Database,
						ResString.GetMultilingualString("3974837e-c50b-459b-881c-44840d26a666", "Last Database Health Check warning list"),
						ResString.GetMultilingualString(
							"80DBC164-493A-41E6-A0E5-F4AD8159DF7B",
							"List of warnings of the last executed Database Health Check. " +
							"A notification email is sent if there are non-acknowledged warnings or once a month if they're all acknowledged.\r\n\r\n" +
							"*** Ignoring health warnings is a breach of the support agreement.\r\n*** Some warnings cannot be acknowledged."),
						new DbHealthWarningRegistryCollection());

					return result;
				});
			}
		}

		public BooleanRegistryItem RefDbNamesCacheIsDirty
		{
			get
			{
				return GetItem("RefDbNamesCacheIsDirty", delegate
				{
					return new BooleanRegistryItem(
						"RefDbNamesCacheIsDirty",
						Categories.System_Database,
						(NoResString)"Reference database name cache is dirty.",
						(NoResString)"Reference database name cache becomes dirty and should be refreshed.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.NotCached,
						false);
				});
			}
		}

		#endregion

		#region On Demand Licence Usage

		public const int InitialLicenceUsageReportYear = 1950;

		public DateTimeRegistryItem OnDemandLicenceUsageReportDate
		{
			get
			{
				// This registry item intentionally has an irrelevant key and description so it is not obvious in the DB.
				return GetItem<DateTimeRegistryItem>("WarehouseCountPackages", delegate
				{
					return new DateTimeRegistryItem(
						"WarehouseCountPackages",
						Categories.System_License,
						(NoResString)"Warehouse Count And Packages",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached,
						new DateTime(InitialLicenceUsageReportYear, 1, 1));
				});
			}
		}

		#endregion

		#region User Account Last Report Time

		public const int InitialUserAccountLastReportYear = 1950;

		public DateTimeRegistryItem UserAccountLastReportTime
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("UserAccountLastReportTime", delegate
				{
					return new DateTimeRegistryItem(
						"UserAccountLastReportTime",
						Categories.System_License,
						(NoResString)"User Account Last Report Time",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached,
						new DateTime(InitialUserAccountLastReportYear, 1, 1));
				});
			}
		}

		#endregion

		#region Is First Time Sending Staff Report

		public BooleanRegistryItem IsFirstTimeSendingStaffReport
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IsFirstTimeSendingStaffReport", () =>
				{
					return new BooleanRegistryItem(
						"IsFirstTimeSendingStaffReport",
						Categories.System_Staff,
						(NoResString)"Set Is First Time Sending Staff Report",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		#endregion

		#region Process Controller

		#region SuppressResourceStringsCheckRegion

		public TimeSpan ServiceTaskUnloadTimeout => TimeSpan.FromSeconds(ServiceTaskUnloadTimeoutInSeconds.Value);

		public IntRegistryItem ServiceTaskUnloadTimeoutInSeconds
		{
			get
			{
				return GetItem("ServiceTaskUnloadTimeoutInSeconds", () =>
					new IntRegistryItem(
						"ServiceTaskUnloadTimeoutInSeconds",
						Categories.System_ProcessController,
						(NoResString)"Service Task Unload Timeout (in seconds)",
						(NoResString)"Time in seconds until idle service task runner will be disposed.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						60,
						0,
						3600)
					);
			}
		}

		#endregion

		public IntRegistryItem ServiceTaskHostTerminatorFrequency
		{
			get
			{
				return GetItem("ServiceTaskHostTerminatorFrequency", () =>
					new IntRegistryItem(
						"ServiceTaskHostTerminatorFrequency",
						Categories.System_ProcessController,
						ResString.GetMultilingualString("DFF74E41-111B-46BA-897A-56895602A59F", "Service Task Host Terminator Frequency"),
						ResString.GetMultilingualString("066A9A80-B38A-427A-A60A-5DF73BBAAB41", "Sets how often (in minutes) a service task host looks for other service hosts that may have crashed."),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						60, 1, int.MaxValue)
					);
			}
		}

		#region SuppressResourceStringsCheckRegion

		public IntRegistryItem BusyRunnerWaitTimeInSeconds
		{
			get
			{
				return GetItem("BusyRunnerWaitTime", () =>
					new IntRegistryItem(
						"BusyRunnerWaitTime",
						Categories.System_ProcessController,
						(NoResString)"Busy Runner wait time (in seconds)",
						(NoResString)"Maximum time in seconds to wait for a busy runner to become idle before creating another process.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						5,
						0,
						3600)
					);
			}
		}

		public IntRegistryItem SecondaryProcessSpinUpDelayInSeconds
		{
			get
			{
				return GetItem("SecondaryProcessSpinUpDelay", () =>
					new IntRegistryItem(
						"SecondaryProcessSpinUpDelay",
						Categories.System_ProcessController,
						(NoResString)"Secondary Process SpinUp Delay (in seconds)",
						(NoResString)"Minimum time in seconds to wait for a task to complete processing before spinning up secondaries (if allowed by task extended configuration).",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						5,
						0,
						3600)
					);
			}
		}

		public IntRegistryItem ServiceHostHttpRequestTimeoutInMilliseconds
		{
			get
			{
				return GetItem("ServiceHostHttpRequestTimeoutInMilliseconds", () =>
					new IntRegistryItem(
						"ServiceHostHttpRequestTimeoutInMilliseconds",
						Categories.System_ProcessController,
						(NoResString)"Service Host requests timeout (in milliseconds)",
						(NoResString)"Maximum time in milliseconds to wait for a reply from Service Host to CW1 HTTP request.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						30000,
						0,
						60000)
				);
			}
		}

		bool ISystemDataRegistry.ServiceTaskBusinessObjectBindingEnabled
		{
			get { return ServiceTaskBusinessObjectBindingEnabled.Value; }
		}

		public BooleanRegistryItem ServiceTaskBusinessObjectBindingEnabled
		{
			get
			{
				return GetItem("ServiceTaskBusinessObjectBindingEnabled", () =>
					new BooleanRegistryItem(
						"ServiceTaskBusinessObjectBindingEnabled",
						Categories.System_ProcessController,
						(NoResString)"Enable Business Object to Service Task Binding",
						(NoResString)"Service Tasks can be invoked either by schedule or by a special mapping when the BusinessObjectFactory containing these objects are saved. If this option is enabled, when relevant records are modified, a request will be sent to Process Controller(s) in order to schedule Service Tasks execution associated with modified records",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						!Globals.IsDebugMode));
			}
		}

		bool ISystemDataRegistry.ServiceTaskParallelWebRequestsEnabled
		{
			get { return ServiceTaskParallelWebRequestsEnabled.Value; }
		}

		public BooleanRegistryItem ServiceTaskParallelWebRequestsEnabled
		{
			get
			{
				return GetItem("ServiceTaskParallelWebRequestsEnabled", () =>
					new BooleanRegistryItem(
						"ServiceTaskParallelWebRequestsEnabled",
						Categories.System_ProcessController,
						(NoResString)"Enable sending web requests in parallel for service task status information",
						(NoResString)"When there are many process controllers in a system, the scheduling of service tasks can be performed significantly faster if we request the status information from different process controllers in parallel.  This option should only be disabled if we are experiencing critical issues with the microsoft Parallel task framework.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true));
			}
		}

		bool ISystemDataRegistry.AsynchronousBilling
		{
			get { return AsynchronousBilling.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public BooleanRegistryItem AsynchronousBilling
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AsynchronousBilling", () =>
				{
					return new BooleanRegistryItem(
						"AsynchronousBilling",
						Categories.System_License,
						(NoResString)"Enable asynchronous billing",
						(NoResString)"Specifies whether or not billing is saved in a background thread.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableLicenseAgreementPopup
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableLicenseAgreementPopup", () =>
				{
					return new BooleanRegistryItem(
						"EnableLicenseAgreementPopup",
						Categories.System_License,
						(NoResString)"Enable license agreement popup",
						(NoResString)"Show the license agreement popup on start-up. This is for testing only.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem ShowQueryStackTraceInProcessControllerEnabled
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShowQueryStackTraceInProcessControllerEnabled", delegate
				{
					return new BooleanRegistryItem(
					"ShowQueryStackTraceInProcessControllerEnabled",
					Categories.System_ProcessController,
					(NoResString)"Enable Query Stack Trace In Process Controllers",
					null,
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsHidden,
					false);
				});
			}
		}

		public DateTimeRegistryItem LastYearlySyncTLS
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("LastYearlySyncTLS", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
						"LastYearlySyncTLS",
						Categories.System_ProcessController,
						(NoResString)"Last yearly sunc data done by TLS service tasks",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						DateTime.MinValue);

					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Message Interception
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem AllowMessageModificationBeforeSending
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowMessageModificationBeforeSending", delegate
				{
					return new BooleanRegistryItem(
						"AllowMessageModificationBeforeSending",
						Categories.System_Messaging,
						(NoResString)"Allow Message Modification",
						(NoResString)"This registry item specifies whether or not users are allowed to modify a message before it is sent.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public GuidRegistryItem MessageModificationBeforeSendingAuthorisationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("MessageModificationBeforeSendingAuthorisationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"MessageModificationBeforeSendingAuthorisationGroup",
						Categories.System_Messaging,
						(NoResString)"Message Modification Authorization Group",
						(NoResString)"The group that are allowed to modify a message before it is sent.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsValueOptional);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion
		#endregion

		#region AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines

		public BooleanRegistryItem AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines", delegate
				{
					return new BooleanRegistryItem(
						"AutoPopulateInvoicePackagingAndContainerDetailsFromOrderLines",
						Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("49F1BA43-0CD2-4CF7-A9FD-10881A261044", "Allow auto-population of packaging and container details."),
						ResString.GetMultilingualString("F3B3C972-0D0D-42B3-A5F2-C8A7F5FB0FFC", "When set to 'Yes' auto-populate packaging details (Box 31) and containers allocated (by placing a tick in the 'Is For Invoice' tick box) when importing invoice lines from order lines."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion
		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem EdiProdLicenceIdentifier
		{
			get
			{
				return GetItem<StringRegistryItem>("ediProdLicenceIdentifier", delegate
				{
					return new StringRegistryItem(
						"ediProdLicenceIdentifier",
						Categories.System_License,
						(NoResString)"ediProd License Identifier",
						(NoResString)"Enter the name of ediProd license identifier.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"EDIAUSSYD");
				});
			}
		}

		#endregion

		#region Main Form

		public StringRegistryItem CompanyShortNameForNews
		{
			get
			{
				return GetItem("CompanyShortNameForNews",
					() => new StringRegistryItem(
						"CompanyShortNameForNews",
						Categories.System_NewsAnnouncements,
						ResString.GetMultilingualString("173966cc-5d90-487c-85b4-89da80f2b48e", "Short Company Name"),
						ResString.GetMultilingualString("a1268d11-17c9-40b1-b081-d90bdbb3fe6e", "Specify the name of your company to appear in the News sections on the application main screen in place of your full company name."),
						RegistryStorageFlags.Company)
					);
			}
		}

		public NewsAnnouncementSectionTypeRegistryItem NewsSectionTypes
		{
			get
			{
				return GetItem<NewsAnnouncementSectionTypeRegistryItem>("NewsSectionTypes", delegate
				{
					var defaultValue = new NewsAnnouncementSectionTypeCollection();
					foreach (CodeDescriptionPair item in new NewsSectionTypeList())
					{
						defaultValue.Add(new NewsAnnouncementSectionType()
						{
							Code = item.Code,
							Description = item.MultilingualDescription,
							OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime,
							SystemDefined = true,
						});
					}
					return new NewsAnnouncementSectionTypeRegistryItem(
						"NewsSectionTypes",
						Categories.System_NewsAnnouncements,
						ResString.GetMultilingualString("e9cf0b82-10f3-4e6e-90ba-71e2f6c2d332", "Section Types"),
						ResString.GetMultilingualString("f3d8c4fc-e339-4876-807d-9dfe1465f0bd",
							@"A list of News & Announcements sections that can be placed on the main screen of the application, in addition to the system defined list.
The user defined News & Announcements sections can either be sorted by 'Published Time' or in 'Alpha/Numeric' order. There is a display limitation of {0} items per section. With 'Published Time', the most recent {0} items are displayed. With 'Alpha/Numeric', the first {0} items are displayed in ascending Alpha/Numeric order.", NewsMaximumRows),
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}
		public const int NewsMaximumRows = 50;

		public NewsSectionRegistryItem NewsSectionLayouts
		{
			get
			{
				return GetItem<NewsSectionRegistryItem>("NewsSectionLayouts", delegate
				{
					var defaultValue = new NewsSectionCollection();
					defaultValue.Add(GetNewsSection(NewsSectionLayoutIDs.TopLeft, NewsSectionTypeList.Codes.ClientNews, true, false));
					defaultValue.Add(GetNewsSection(NewsSectionLayoutIDs.TopMiddle, NewsSectionTypeList.Codes.ClientAnnouncements, true, false));
					defaultValue.Add(GetNewsSection(NewsSectionLayoutIDs.TopRight, NewsSectionTypeList.Codes.ClientStaffNews, true, false));
					defaultValue.Add(GetNewsSection(NewsSectionLayoutIDs.BottomLeft, NewsSectionTypeList.Codes.ProductUpdates, true, false));
					defaultValue.Add(GetNewsSection(NewsSectionLayoutIDs.BottomMiddle, NewsSectionTypeList.Codes.WiseLearningUpdates, false, false));
					defaultValue.Add(GetNewsSection(NewsSectionLayoutIDs.BottomRight, NewsSectionTypeList.Codes.WiseNews, false, false));

					return new NewsSectionRegistryItem(
						"NewsSectionLayouts",
						Categories.System_NewsAnnouncements,
						ResString.GetMultilingualString("e9cf0b82-10f3-4e6e-90ba-71e2f6c2d441", "Section Layout"),
						ResString.GetMultilingualString("f3d8c4fc-e339-4876-807d-9dfe1465f1ca", "Defines the layout of the news & announcement sections on the application main screen."),
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}

		public FindWindowQueryCostsRegistryItem FindWindowQueryGovernorCosts
		{
			get
			{
				const string key = "FindWindowQueryGovernorCosts";
				return GetItem(key, () => new FindWindowQueryCostsRegistryItem(
					key,
					RawDataRegistry.Categories.System_Database_UserOptions,
					ResString.GetMultilingualString("fc774346-0f04-465d-be9b-976b846bc838", "Estimated query cost limit"),
					ResString.GetMultilingualString("c23dee20-b73e-4825-96d7-79651552f289", @"Specify two query cost limits for find screens.
Below the low value query will run.
Between these two values confirmation will be required to run the query.
Above the high value requires security right to be allocated.
Zero in value means unlimited execution time.
Add security right ""Specialized Rights\Allow to Run Query with Unlimited Estimated Cost on Find Screens"" to run queries with estimated cost more then these limits."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					new FindWindowQueryCosts()));
			}
		}

		NewsSection GetNewsSection(string layoutPanelID, string sectionID, bool hideReadItems, bool mandatoryToRead)
		{
			var result = new NewsSection();
			using (result.GetValidationSuspender())
			{
				result.LayoutPanelID = layoutPanelID;
				result.SectionID = sectionID;
				result.HideReadItems = hideReadItems;
				result.MandatoryToRead = mandatoryToRead;
			}
			return result;
		}

		public static class NewsSectionLayoutIDs
		{
			public const string TopLeft = "Top-Left";
			public const string TopMiddle = "Top-Middle";
			public const string TopRight = "Top-Right";
			public const string BottomLeft = "Bottom-Left";
			public const string BottomMiddle = "Bottom-Middle";
			public const string BottomRight = "Bottom-Right";
		}

		#endregion

		#region Product Registration

		public StringRegistryItem ProductRegistrationServiceUri
		{
			get
			{
				return GetItem("ProductRegistrationServiceUri",
					() => new StringRegistryItem(
						"ProductRegistrationServiceUri",
						Categories.System_License,
						(NoResString)"Product Registration Service URI",
						(NoResString)"Specify the URL for the Product Registration Service",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://webservices-ausyd.cargowise.net/Registration/")
					);
			}
		}

		#endregion

		#region STL = Seat+Transaction Licence
		#region SuppressResourceStringsCheckRegion

		public const string StlCollectorHighWaterMarkPrefix = "WaterBillDate";

		public DateTimeRegistryItem StlCollectorHighWaterMark
		{
			get
			{
				return GetItem(StlCollectorHighWaterMarkPrefix, delegate
				{
					return new DateTimeRegistryItem(
						StlCollectorHighWaterMarkPrefix,
						Categories.System_STLHighWaterMarks,
						(NoResString)"Legacy System Wide Setting",
						(NoResString)"This setting has been superseded by separate settings for each STL item being collected",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		public DateTimeRegistryItem LastWatermarkResetTime
		{
			get
			{
				return GetItem("LastWatermarkResetTime", delegate
				{
					return new DateTimeRegistryItem(
						"LastWatermarkResetTime",
						Categories.System_STLHighWaterMarks,
						(NoResString)"Last Watermark Reset Time",
						(NoResString)"The date a local watermark reset was last performed",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}
		
		public DateTimeRegistryItem GetStlCollectorHighWaterMark(string code, string feature, MultilingualString category = null)
		{
			category ??= Categories.System_STLHighWaterMarks;

			var key = StlCollectorHighWaterMarkPrefix + code;
			return GetItem(key, delegate
			{
				return new DateTimeRegistryItem(
					key,
					category,
					(NoResString)feature,
					(NoResString)"The start date of the next STL data collection.",
					new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short),
					RegistryStorageFlags.System,
					RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport,
					DateTime.MinValue,
					false);
			});
		}

		public IntRegistryItem HighWaterMarkExceptionThresholdInHours
		{
			get
			{
				return GetItem<IntRegistryItem>("HighWaterMarkExceptionThresholdInHours", delegate
				{
					return new IntRegistryItem(
						"HighWaterMarkExceptionThresholdInHours",
						Categories.System_STLHighWaterMarks,
						(NoResString)"High Water Mark Exception Threshold",
						(NoResString)"The maximum allowed time between successful data collections before an exception is thrown.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport,
						5,
						0,
						60);
				});
			}
		}

		public StringRegistryItem UsageBillingGateway
		{
			get
			{
				return GetItem<StringRegistryItem>("UsageBillingGateway", delegate
				{
					var caption = (NoResString)"Usage Billing Gateway Server Address";
					var hint = (NoResString)"This is the address of the usage billing gateway server.";
					var result = new eHubGatewayRegistryItem(
						"UsageBillingGateway",
						Categories.System_STL,
						caption,
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						"ehub-usage.wisegrid.net",
						isProduction: true);
					return result;
				});
			}
		}

		public StringRegistryItem UsageBillingTestGateway
		{
			get
			{
				return GetItem<StringRegistryItem>("UsageBillingTestGateway", delegate
				{
					var caption = (NoResString)"Usage Billing Test Gateway Server Address";
					var hint = (NoResString)"This is the address of the usage billing gateway test server.";
					var result = new eHubGatewayRegistryItem(
						"UsageBillingTestGateway",
						Categories.System_STL,
						caption,
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						"ehub-usage-test.wisegrid.net",
						isProduction: false);
					return result;
				});
			}
		}

		public DateTimeRegistryItem USSOutageStartTime
		{
			get
			{
				return GetItem("USSOutageStartTime", delegate
				{
					return new DateTimeRegistryItem(
						"USSOutageStartTime",
						Categories.System_STL,
						(NoResString)"USS Service Task Outage Start Time",
						(NoResString)"The time the current outage for the USS service task started",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		#endregion
		#endregion

		#region FTPDestinationOverride

		public FTPDestinationOverrideRegistryItem FTPDestinationOverride
		{
			get
			{
				return GetItem<FTPDestinationOverrideRegistryItem>("FTPDestinationOverride", delegate
				{
					return new FTPDestinationOverrideRegistryItem(
						"FTPDestinationOverride",
						Categories.System_Testing,
						ResString.GetMultilingualString("E255E632-BFB9-4F34-B45D-BE6313E7DEEB", "FTP Destination Override"),
						ResString.GetMultilingualString("8059B4EB-D5CA-41F0-BE3B-52D43E256F9B", "Enter a FTP address here to force all scheduled reports with a delivery method of FTP to be sent to this FTP address. This is to prevent scheduled reports from being delivered to client FTP systems out of the non-production environment, while still being able to test the FTP scheduled report delivery process."),
						RegistryStorageFlags.System,
						EnvProxy.Instance.IsProductionSystem ? RegistryOptions.PreserveTestValue : (RegistryOptions.PreserveTestValue | RegistryOptions.MustOverrideDefaultValue),
						new FTPDestinationOverrideInfo());
				});
			}
		}

		#endregion

		#region Locations

		public IntRegistryItem TimeZoneOffsetCachePeriod
		{
			get
			{
				return GetItem<IntRegistryItem>("TimeZoneOffsetCachePeriod",
				() => new IntRegistryItem(
						"TimeZoneOffsetCachePeriod",
						Categories.System_Database_UserOptions,
						ResString.GetMultilingualString("6111ebfd-4a4c-438a-9b16-fcea46da58f7", "Number of Months to Cache Time Zone Data"),
						ResString.GetMultilingualString("2d04cd40-cbe0-4392-8fc5-3b8963b4bc73", "Sets the number of months in the future that time zone offset information will be cached. This value will also be used as the expiry date for cached values. For example, if the value is set to 24 and the Time Zone Offset Cache service task runs on 1 July 2020, Time Zone data will be calculated for the time span between 1 July 2020 and 1 July 2022. The service task will also delete cache entries with an end date falling before 1 July 2018."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						24,
						12,
						60)
						);
			}
		}

		#endregion

		#region Google Maps

		public StringRegistryItem GoogleMapsClientId
		{
			get
			{
				return GetItem("GoogleMapsClientId", () => new StringRegistryItem(
					"GoogleMapsClientId",
					Categories.System_GoogleMaps,
					(NoResString)"Google Maps Client ID",
					(NoResString)"The client ID to be used for Google Maps services.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					"gme-translogix"));
			}
		}

		public StringRegistryItem GoogleMapsAPIKey
		{
			get
			{
				return GetItem("GoogleMapsAPIKey", () => new StringRegistryItem(
					"GoogleMapsAPIKey",
					Categories.System_GoogleMaps,
					(NoResString)"Google Maps API Key for Web",
					(NoResString)"The API key to be used for Google Maps API services accessible from Web.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					"AIzaSyAxgGMeU4YHsfnBxGVOrLRrkPaaQumFrtI"));
			}
		}

		public StringRegistryItem GoogleMapsAPIKeyForServiceTasks
		{
			get
			{
				return GetItem(nameof(GoogleMapsAPIKeyForServiceTasks), () => new StringRegistryItem(
					nameof(GoogleMapsAPIKeyForServiceTasks),
					Categories.System_GoogleMaps,
					(NoResString)"Google Maps API Key for service tasks",
					(NoResString)"The API key to be used for Google Maps API services accessible from service tasks.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
					"AIzaSyCt2OV3miUwsCkfAV_M8nJzMB2Fiug4MpI"));
			}
		}

		#endregion

		#region Production Rules

		public IntRegistryItem MaxNumberOfAttemptsForRuleProcessing
		{
			get
			{
				return GetItem(nameof(MaxNumberOfAttemptsForRuleProcessing), () =>
					new IntRegistryItem(
						nameof(MaxNumberOfAttemptsForRuleProcessing),
						Categories.System_ProductionRules,
						ResString.GetMultilingualString("b61bfd0c-735e-4c0d-90b6-3b418efc5dc3", "Max No. of Attempts for Production Rule Processing"),
						ResString.GetMultilingualString("54abb5b0-74e4-4311-93ad-b8c5ce66b6bc", "An email will be sent to appropriate groups when production rules are disabled for exceeding the maximum number of attempts."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: 10,
						minValue: 1,
						maxValue: 1000)
					);
			}
		}

		public IntRegistryItem RulesEngineSessionFactoryCacheMinutes
		{
			get
			{
				return GetItem(nameof(RulesEngineSessionFactoryCacheMinutes), delegate
				{
					return new IntRegistryItem(
						nameof(RulesEngineSessionFactoryCacheMinutes),
						Categories.System_ProductionRules,
						ResString.GetMultilingualString("e81770a9-ec16-4fe1-a812-901a3cc2f4e2", "Production Rules Engine Cache Expiration Time in Minutes"),
						ResString.GetMultilingualString("101b20c1-1818-4608-80db-cff811333d8d", "Production rules will be cached and expire after configured minutes."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: Globals.IsTest ? 0 : 20,
						minValue: 0,
						maxValue: 60);
				});
			}
		}

		#endregion

		#region Maximum Number of Records to Show in Display Grids
		public IntRegistryItem MaxNumberOfRecordsToShowInDisplayGrids
		{
			get
			{
				return GetItem("MaxNumberOfRecordsToShowInDisplayGrids", () =>
					new IntRegistryItem(
						"MaxNumberOfRecordsToShowInDisplayGrids",
						Categories.PhysicalServer_DisplayGrid,
						ResString.GetMultilingualString("b668a61f-ac84-4f21-adb9-3eb06224f6c2", "Max No. of Records to Show"),
						ResString.GetMultilingualString("a49410af-a460-4e92-bc55-33ee4ed71719", "An error icon will show on a Display Grid if a search returns more than the maximum number of results. Results will not be displayed if this happens."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.All,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						1000,
						1,
						15000)
					);
			}
		}

		#endregion

		#region Error Reporting

		public StringRegistryItem ErrorReportingServiceUri
		{
			get
			{
				return GetItem("ErrorReportingServiceUri", () =>
					new StringRegistryItem(
						"ErrorReportingServiceUri",
						Categories.System_Miscellaneous,
						(NoResString)"Error Reporting Service URI",
						(NoResString)"If set, error reports will be redirected to the supplied service URI, instead of the default error reporting service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: WTG.ErrorReporting.WellKnownServiceUris.Production)
					{
						DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false }
					});
			}
		}

		public BooleanRegistryItem ErrorReportingFormsRunningInBackground
		{
			get
			{
				return GetItem("ErrorReportingFormsRunningInBackground", () =>
					new BooleanRegistryItem(
						"ErrorReportingFormsRunningInBackground",
						Categories.System_Miscellaneous,
						(NoResString)"Error Reporting Forms Running In Background When Not Allowed",
						(NoResString)"If set, error reports will be generated for the cases when forms run in background when it is not allowed.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false));
			}
		}

		#endregion

		#region Activity Log Maximum Past Years

		public IntRegistryItem ActivityLogMaximumPastYears
		{
			get
			{
				return GetItem("ActivityLogMaximumPastYears",
					() => new IntRegistryItem(
							"ActivityLogMaximumPastYears",
							Categories.System_Staff_ActivityLogging,
							ResString.GetMultilingualString("7AEB880D-3C00-406E-B731-CF6C600A816D", "Activity Log Maximum Past Years"),
							ResString.GetMultilingualString("AD75A592-F6FE-4DF1-B9D1-A80EC4479D69", "Defines the maximum number of years that the details on the Activity Log of a Staff record can be searched. 0 means no time limit."),
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
							defaultValue: 10,
							minValue: 0,
							maxValue: DateRangeValidation.MaximumPastYears)
					);
			}
		}

		int ISystemDataRegistry.ActivityLogMaximumPastYears
		{
			get { return ActivityLogMaximumPastYears.Value; }
		}

		#endregion

		#region System_TransportZones
		public BooleanRegistryItem DisableNonVerifiablePostcodeWarning
		{
			get
			{
				return GetItem("DisableNonVerifiablePostcodeWarning", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"DisableNonVerifiablePostcodeWarning",
						Categories.System_TransportZones,
						ResString.GetMultilingualString("b668a61f-ac84-4f23-adb9-3eb06224f6c2", "Disable Non-verifiable Postcode Warning."),
						ResString.GetMultilingualString("3432165a-9963-4693-8e8c-15dc5283b980", "Set this to \"Yes\" to disable the warning when importing a non-verifiable postcode on Transport Zone Sets."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
					return result;
				});
			}
		}

		public BooleanRegistryItem DisableNonVerifiableCityTownWarning
		{
			get
			{
				return GetItem("DisableNonVerifiableCityTownWarning", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"DisableNonVerifiableCityTownWarning",
						Categories.System_TransportZones,
						ResString.GetMultilingualString("b44dad40-327d-41cc-9b96-1a437a1d557b", "Disable Non-verifiable City/Town Warning."),
						ResString.GetMultilingualString("3bf088c8-b293-4495-b965-70c8f29e1742", "Set this to \"Yes\" to disable the warning when importing a non-verifiable city/town on Transport Zone Sets."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
					return result;
				});
			}
		}

		#endregion

		#region SQL Server System Configurations

		public SqlSystemConfigurationRegistryItem SqlSystemConfigurations
		{
			get
			{
				return GetItem("SqlSystemConfigurations", delegate
				{
					return new SqlSystemConfigurationRegistryItem(
						"SqlSystemConfigurations",
						RawDataRegistry.Categories.System,
						(NoResString)"SQL Server Configurations",
						(NoResString)"SQL Server Configurations Proposed Values",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden);
				});
			}
		}

		#endregion

		#region ArchiveManager

		public BooleanRegistryItem BiIsRequiredDeleteOrphanSubscriber
		{
			get
			{
				return GetItem("BiIsRequiredDeleteOrphanSubscriber", delegate
				{
					return new BooleanRegistryItem(
						"BiIsRequiredDeleteOrphanSubscriber",
						Categories.System_ArchiveManager,
						(NoResString)"Is Required for Delete Orphan Subscriber",
						(NoResString)"This enables/disables the Delete Orphan Subscriber (DOS).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableOfflineArchiving
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableOfflineArchiving", delegate
				{
					return new BooleanRegistryItem(
						"EnableOfflineArchiving",
						Categories.System_ArchiveManager,
						ResString.GetMultilingualString("029525C5-5FDF-455B-98E5-0A549F53F9ED", "Enable Offline Archiving"),
						ResString.GetMultilingualString("3BE16D35-23BA-4D75-B346-6699F1D02849", "Offline Archiving is the process where documents that have been archived are stored in a location other than on the database server. The Offline Archiving process is only available for self-hosted clients."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
						!EnvProxy.IsHostedWithCargowise);
				});
			}
		}

		public CodePairRegistryItem OnlineArchiveDocumentFormat
		{
			get
			{
				return GetItem("OnlineArchiveDocumentFormat", () =>
				{
					return new CodePairRegistryItem("OnlineArchiveDocumentFormat",
						Categories.System_ArchiveManager,
						ResString.GetMultilingualString("502CF472-7EB8-497D-80C6-9E4B0C68F857", "Generated Documents Format"),
						ResString.GetMultilingualString("055F45E8-AF78-4F9F-BFE3-D2028E9F4DF0", "Documents that are generated during the archiving process will be stored in this format."),
						DocumentFileFormatListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Enterprise.Core.Constants.FileFormats.TIF);
				});
			}
		}

		public BooleanRegistryItem IncludeTimeTakenInTheARCLogs
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeTimeTakenInTheARCLogs", delegate
				{
					return new BooleanRegistryItem(
						"IncludeTimeTakenInTheARCLogs",
						Categories.System_ArchiveManager,
						ResString.GetMultilingualString("FB4BA009-28AC-47E8-8675-53B0F2862C13", "Include Time Taken In The ARC Logs"),
						ResString.GetMultilingualString("1F9D7E45-F39E-4E8E-998F-DC20903FA4D8", "When enabled, the ARC service Task logs will include the time taken for different operations in processing the archive schedule jobs."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						true);
				});
			}
		}

		public IntRegistryItem LoadArchiveSetTimeout
		{
			get
			{
				return GetItem<IntRegistryItem>("LoadArchiveSetTimeout", () => new IntRegistryItem(
						"LoadArchiveSetTimeout",
						Categories.System_ArchiveManager,
						ResString.GetMultilingualString("88467E10-017C-4794-BFAB-EBF5F7427AB2", "Load Archive Set Timeout"),
						ResString.GetMultilingualString("2B71CF14-BE78-487D-8B2A-DA34DB1B6854", "Time in minutes that the application will wait for a successful load of Archive Set before timing out. Accepted values are from 0 to 120. 0 means no time limit."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						30,
						0,
						120)
				);
			}
		}

		public IntRegistryItem LoadArchiveSetBatchTimeout
		{
			get
			{
				return GetItem<IntRegistryItem>("LoadArchiveSetBatchTimeout", () => new IntRegistryItem(
						"LoadArchiveSetBatchTimeout",
						Categories.System_ArchiveManager,
						ResString.GetMultilingualString("6630973B-9375-4570-8C85-58A5D0BCF016", "Load Archive Set Batch Timeout"),
						ResString.GetMultilingualString("85FD3984-8289-4D85-9100-59753650124A", "Time in minutes that the application will wait for a successful load of Archive Set Batch before timing out. Accepted values are from 0 to 120. 0 means no time limit."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						10,
						0,
						120)
				);
			}
		}

		#region InactiveOperationalJobsArchiveSystem

		public IntRegistryItem InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum
		{
			get
			{
				return GetItem<IntRegistryItem>("InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum", delegate
				{
					return new IntRegistryItem(
						"InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum",
						Categories.System_ArchiveManager_InactiveOperationalJobsArchiveSystem,
						ResString.GetMultilingualString("F46F1D57-5BCD-4974-A834-3F6C8BE0601A", "On or Before Minimum"),
						ResString.GetMultilingualString("F36224DD-D869-4F40-90A5-E99DDC5DDD93", "This registry allows you to define the minimum number of years that a record must have been in the system before it can be archived using the Inactive Operational Jobs Archive System (IPS). \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						7,
						0,
						100);
				});
			}
		}

		#endregion

		#region OperationalJobsArchiveSystem

		public IntRegistryItem ArchiveRecordsOnOrBeforeMinimum
		{
			get
			{
				return GetItem<IntRegistryItem>("ArchiveRecordsOnOrBeforeMinimum", delegate
				{
					return new IntRegistryItem(
						"ArchiveRecordsOnOrBeforeMinimum",
						Categories.System_ArchiveManager_OperationalJobsArchiveSystem,
						ResString.GetMultilingualString("2DEA5390-0618-495C-8B2F-19F0821A7310", "On or Before Minimum"),
						ResString.GetMultilingualString("0D35AC2E-E22D-4FED-8410-2194E2E3E0FD", "This registry allows you to define the minimum number of years that a record must have been in the system before it can be archived using the Operations Jobs Archive (OPS) system. \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						7,
						0,
						100);
				});
			}
		}

		public IntRegistryItem BatchSizeControl
		{
			get
			{
				return GetItem<IntRegistryItem>("BatchSizeControl", delegate
				{
					return new IntRegistryItem(
						"BatchSizeControl",
						Categories.System_ArchiveManager,
						ResString.GetMultilingualString("36CA86D9-291F-4706-98FD-B03C335724AA", "Set Batch Size for Archiving and Purging Operational Jobs"),
						ResString.GetMultilingualString("72B50C16-9F79-4058-9711-B9A703387D24", "The maximum number of records to be processed in one batch by Archive Manager. This includes schedules for Operational Jobs Archive System (OPS), Inactive Operational Jobs Archive System (IPS), and Purge Documents and Records (PDR)."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						50,
						1,
						500);
				});
			}
		}

		#endregion

		#region PurgeArchivedRecordsSystem
		public IntRegistryItem PurgeArchivedRecordsOnOrBeforeMinimum
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeArchivedRecordsOnOrBeforeMinimum", delegate
				{
					return new IntRegistryItem(
						"PurgeArchivedRecordsOnOrBeforeMinimum",
						Categories.System_ArchiveManager_PurgeArchivedRecordsSystem,
						ResString.GetMultilingualString("C7FE4196-7AE3-490C-8166-F177E9CF88CA", "On or Before Minimum"),
						ResString.GetMultilingualString("699A1AA9-2073-4140-93B7-15F1DE206222", "This registry allows you to define the minimum number of years that a record must have been archived before it can be deleted when using the Purge Archived Records (PAR) system."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						7,
						0,
						100);
				});
			}
		}

		public IntRegistryItem PurgeArchivedRecordsBatchSizeControl
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeArchivedRecordsBatchSizeControl", delegate
				{
					return new IntRegistryItem(
						"PurgeArchivedRecordsBatchSizeControl",
						Categories.System_ArchiveManager_PurgeArchivedRecordsSystem,
						ResString.GetMultilingualString("C44970EC-FF4D-4B7C-A56C-6DB9A67CFBDC", "Set Batch Size"),
						ResString.GetMultilingualString("A7BC8CDF-A670-4085-9BCE-89F55B508DE0", "The maximum number of records in the Archived Records module to be processed by the Archive Manager in one batch using the Purge Archived Records System (PAR)."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						200,
						1,
						10000);
				});
			}
		}

		#endregion

		#region StandaloneRecordsArchiveSystem

		public IntRegistryItem StandaloneRecordsOnOrBeforeMinimum
		{
			get
			{
				return GetItem<IntRegistryItem>("StandaloneRecordsOnOrBeforeMinimum", delegate
				{
					return new IntRegistryItem(
						"StandaloneRecordsOnOrBeforeMinimum",
						Categories.System_ArchiveManager_StandaloneRecordsArchiveSystem,
						ResString.GetMultilingualString("2DEA5390-0618-495C-8B2F-19F0821A7310", "On or Before Minimum"),
						ResString.GetMultilingualString("C78B911B-1447-4215-A796-3E06A7A2D639", "This registry allows you to define the minimum number of years that a record must have been in the system before it can be archived using the Standalone Records Archive System (STA). \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						7,
						0,
						100);
				});
			}
		}

		public IntRegistryItem StandaloneRecordsBatchSizeControl
		{
			get
			{
				return GetItem<IntRegistryItem>("StandaloneRecordsBatchSizeControl", delegate
				{
					return new IntRegistryItem(
						"StandaloneRecordsBatchSizeControl",
						Categories.System_ArchiveManager_StandaloneRecordsArchiveSystem,
						ResString.GetMultilingualString("C44970EC-FF4D-4B7C-A56C-6DB9A67CFBDC", "Set Batch Size"),
						ResString.GetMultilingualString("01515287-0F1A-402C-9954-E3640C39ADAB", "The maximum number of records to be processed in one batch by Archive Manager. This includes schedule for Standalone Records Archive System (STA)."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						200,
						1,
						10000);
				});
			}
		}

		#endregion

		#region PurgeDocumentsAndRecordsSystem

		public IntRegistryItem PurgeDocumentsAndRecordsOnOrBeforeMinimum
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeDocumentsAndRecordsOnOrBeforeMinimum", delegate
				{
					return new IntRegistryItem(
						"PurgeDocumentsAndRecordsOnOrBeforeMinimum",
						Categories.System_ArchiveManager_PurgeDocumentsAndRecordsSystem,
						ResString.GetMultilingualString("ECA3343F-47E5-4A0D-8E9A-BB443D137505", "On or Before Minimum"),
						ResString.GetMultilingualString("F5CA89E2-3863-432E-8291-BD01A4BA071F", "This registry allows you to define the minimum number of years that a record must have been in the system before it can be purged using the Purge Documents and Records (PDR) system. \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before purging."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						7,
						0,
						100);
				});
			}
		}

		#endregion

		#region PurgeDocumentsOfOperationalRecordsSystem

		public IntRegistryItem PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum", delegate
				{
					return new IntRegistryItem(
						"PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum",
						Categories.System_ArchiveManager_PurgeDocumentsOfOperationalRecordsSystem,
						ResString.GetMultilingualString("A3D19635-27C2-4825-B8EA-211DD79B597B", "On or Before Minimum"),
						ResString.GetMultilingualString("B0EFABD2-041F-486F-8914-31C2E7DC700F", "This registry allows you to define the minimum number of years that a record must have been in the system before their related documents can be deleted using the Purge Documents of Operational Records System (PDO). \r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before purging."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						10,
						0,
						100);
				});
			}
		}

		public IntRegistryItem PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl
		{
			get
			{
				return GetItem<IntRegistryItem>("PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl", delegate
				{
					return new IntRegistryItem(
						"PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl",
						Categories.System_ArchiveManager_PurgeDocumentsOfOperationalRecordsSystem,
						ResString.GetMultilingualString("915A1BA9-912A-498F-9CC3-96C3D6BD0ACC", "Set Batch Size"),
						ResString.GetMultilingualString("34BEF132-43BA-4A61-91A2-5D9F19D1E723", "The maximum number of records to be processed in one batch by Archive Manager."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						50,
						1,
						10000);
				});
			}
		}

		#endregion

		#region HVLVArchiveSystem

		public IntRegistryItem HVLVArchiveSystemOnOrBeforeMinimum
		{
			get
			{
				return GetItem<IntRegistryItem>("HVLVArchiveSystemOnOrBeforeMinimum", delegate
				{
					return new IntRegistryItem(
						"HVLVArchiveSystemOnOrBeforeMinimum",
						Categories.System_ArchiveManager_HVLVArchiveSystem,
						ResString.GetMultilingualString("DA8CA4C6-1C17-431B-A124-6FB67D2E9D06", "On or Before Minimum"),
						ResString.GetMultilingualString("DF9686F3-3EF4-4366-871D-3E37A46E9794", "This registry setting specifies how many years from the Shipment's arrival date HVLV Consignments and Items will remain before they are marked as archived via HVLV Archive System (HAR)." +
						"\r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						1,
						0,
						100);
				});
			}
		}

		public IntRegistryItem HVLVArchiveSystemBatchSizeControl
		{
			get
			{
				return GetItem<IntRegistryItem>("HVLVArchiveSystemBatchSizeControl", delegate
				{
					return new IntRegistryItem(
						"HVLVArchiveSystemBatchSizeControl",
						Categories.System_ArchiveManager_HVLVArchiveSystem,
						ResString.GetMultilingualString("C8FAE878-A867-4037-921F-CA9E358D98F4", "Set Batch Size"),
						ResString.GetMultilingualString("312F4874-53E3-467E-A1E5-E0CF7C1C73D9", "The maximum number of shipments to be processed in one batch by Archive Manager using the HVLV Archive System (HAR)."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						100,
						1,
						100);
				});
			}
		}

		public CodePairRegistryItem ArchivingFileFormat
		{
			get
			{
				return GetItem("ArchivingFileFormat", () =>
				{
					return new CodePairRegistryItem("ArchivingFileFormat",
						Categories.System_ArchiveManager_HVLVArchiveSystem,
						ResString.GetMultilingualString("A3CE76E3-428E-4936-BCFE-5EA77F86674D", "Archiving File Format"),
						ResString.GetMultilingualString("779BADB8-C4A5-4431-BE12-9B062B9AFC8F", "Select the file format that will be stored in the shipment eDocs once HVLV data has been archived by the HVLV Archive System (HAR)."),
						new HVLVArchivalFileFormats().GetCodeDescriptionPairListProvider(),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Constants.FileFormats.CSV);
				});
			}
		}

		#endregion

		#region ExpiredRatesArchiveSystem

		public BooleanRegistryItem ExposeExpiredRatesArchiveSystem
		{
			get
			{
				return GetItem("ExposeExpiredRatesArchiveSystem", () =>
				{
					return new BooleanRegistryItem(
						"ExposeExpiredRatesArchiveSystem",
						Categories.System_ArchiveManager_PurgeExpiredRatesSystem,
						(NoResString)"Expose RED",
						(NoResString)"Whether to make the Expired Rates Archive System visible.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public IntRegistryItem ExpiredRatesArchiveSystemOnOrBeforeMinimum
		{
			get
			{
				return GetItem("ExpiredRatesArchiveSystemOnOrBeforeMinimum", () =>
				{
					return new IntRegistryItem(
						"ExpiredRatesArchiveSystemOnOrBeforeMinimum",
						Categories.System_ArchiveManager_PurgeExpiredRatesSystem,
						(NoResString)"On or Before Minimum",
						(NoResString)"This registry setting specifies how many years a rate must have been expired for before being eligible for archiving under the Expired Rates Archive System. A value of 0 means it will be targeted for archiving immediately.\r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						7,
						0,
						100
					);
				});
			}
		}

		public IntRegistryItem ExpiredRatesArchiveSystemBatchSizeControl
		{
			get
			{
				return GetItem("ExpiredRatesArchiveSystemBatchSizeControl", () =>
				{
					return new IntRegistryItem(
						"ExpiredRatesArchiveSystemBatchSizeControl",
						Categories.System_ArchiveManager_PurgeExpiredRatesSystem,
						(NoResString)"Set Batch Size",
						(NoResString)"The maximum number of expired rates to be processed in one batch by Archive Manager using the Expired Rates Archive System (RED).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						100,
						1,
						5000
					);
				});
			}
		}

		#endregion

		#region ActivityLogsArchiveSystem

		public BooleanRegistryItem ExposeActivityLogsArchiveSystem
		{
			get
			{
				return GetItem("ExposeActivityLogsArchiveSystem", () =>
				{
					return new BooleanRegistryItem(
						"ExposeActivityLogsArchiveSystem",
						Categories.System_ArchiveManager_PurgeActivityLogsSystem,
						(NoResString)"Expose PAL",
						(NoResString)"Whether to make the Activity Logs Purge System visible.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public IntRegistryItem ActivityLogsArchiveSystemOnOrBeforeMinimum
		{
			get
			{
				return GetItem("ActivityLogsArchiveSystemOnOrBeforeMinimum", () =>
				{
					return new IntRegistryItem(
						"ActivityLogsArchiveSystemOnOrBeforeMinimum",
						Categories.System_ArchiveManager_PurgeActivityLogsSystem,
						(NoResString)"On or Before Minimum",
						(NoResString)"This registry specifies allows you to determine the minimum number of years that a record must have been in the system before it can be purged using the Purge Activity Logs (PAL).\r\n\r\nWarning: Please make sure to confirm the data retention requirements for this data before archiving.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						7,
						1,
						100
					);
				});
			}
		}

		#endregion

		#endregion

		#region eConversations

		public NotificationEmailTemplateRegistryItem EConversationMessageEmailTemplate
		{
			get
			{
				return GetItem("EConversationMessageEmailTemplate", delegate
				{
					var docSourceType = ObjectFactory.GetType<DocumentWrappers.IDocEConversationMessageNotification>();

					return new EConversationMessageEmailTemplateRegistryItem(
						"EConversationMessageEmailTemplate",
						Categories.System_EConversations,
						ResString.GetMultilingualString("bd4c2a64-f20c-45a7-b67f-6fd0f1552356", "eConversation Message Email Template"),
						ResString.GetMultilingualString("03722d5a-da74-4c2c-a5df-ae2da317cc77", "Configure the template for eConversation message notification emails."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						docSourceType);
				});
			}
		}

		#endregion

		#region Log Shrink After Upgrade

		public IntRegistryItem MaximumAllowedVLFsCount
		{
			get
			{
				return GetItem("MaximumAllowedVLFsCount", () => new IntRegistryItem(
					name: "MaximumAllowedVLFsCount",
					category: Categories.System_Database_LogShrinkAfterUpgrade,
					caption: ResString.GetMultilingualString("eb5c9f66-2524-4bc9-a43e-ace8022865d8", "Maximum Allowed Virtual Log Files"),
					hint: ResString.GetMultilingualString("9d51e9ac-e9a8-4a5f-ad25-62abaf9d37cd", "This setting determines the threshold for the maximum number of Virtual Log Files (VLFs) allowed in a database log file. When the number of VLFs exceeds this threshold, the log file will be considered for shrinking during the upgrade process."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted,
					100
				));
			}
		}

		public IntRegistryItem MaximumLogToTwoWeekBackupPercentage
		{
			get
			{
				return GetItem("MaximumLogToTwoWeekBackupPercentage", () => new IntRegistryItem(
					name: "MaximumLogToTwoWeekBackupPercentage",
					category: Categories.System_Database_LogShrinkAfterUpgrade,
					caption: ResString.GetMultilingualString("2afe2cf8-aebb-4ea7-95db-dd7ec6b3df0d", "Maximum Log File to Backup Size Percentage"),
					hint: ResString.GetMultilingualString("a64bc2fe-0137-4bc4-be43-c886487c72a1", "This setting defines the maximum size of the log file in relation to the largest backup made in the last two weeks, measured as a percentage. If the size of the current log file goes beyond this set percentage, the log file will be eligible for shrinking."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted,
					120
				));
			}
		}

		#endregion

		#region NTP Time Servers

		public StringArrayRegistryItem NtpTimeServers
		{
			get
			{
				return GetItem("NtpTimeServers", () =>
				{
					return new StringArrayRegistryItem(
						"NtpTimeServers",
						Categories.System_Time,
						ResString.GetMultilingualString("2467822A-30BB-4E66-BB3C-256523B7E9DF", "Time Server List"),
						ResString.GetMultilingualString("CB806B73-7257-4F09-A59E-88231A2200D0", "List of Time Servers"),
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsHidden
					);
				});
			}
		}

		#endregion

		#region Security Override

		public BooleanRegistryItem EnableSecurityOverrideToken
		{
			get
			{
				return GetItem("EnableSecurityOverrideToken", delegate
				{
					return new BooleanRegistryItem(
						"EnableSecurityOverrideToken",
						Categories.System_SecurityOverride,
						ResString.GetMultilingualString("C0F1A4D5-3E8B-4A2C-9E7F-6D3B2F1A0E5C", "Enable Security Override Token"),
						ResString.GetMultilingualString("A6D7F8B2-9E4C-4D3B-BE5A-8F1D7B0C8E5F", "When OIDC is enabled, setting this registry to 'Yes' allows authorized users to generate a 6 digits, one time use token for security overrides."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public IntRegistryItem SecurityOverrideTokenExpiryTime
		{
			get
			{
				return GetItem("SecurityOverrideTokenExpiryTime", delegate
				{
					return new IntRegistryItem(
						"SecurityOverrideTokenExpiryTime",
						Categories.System_SecurityOverride,
						ResString.GetMultilingualString("E269EAF0-6C0A-4DCB-85D9-B4285C37FDA8", "Security Override Token Expiry Time"),
						ResString.GetMultilingualString("C05DB0E5-4330-4A75-8F9D-157FC39A937E", "This value specifies the expiration time of the security override authorization token after it is generated by CargoWise. Updating this value will adjust authorization token’s validity period."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						5,
						5,
						30);
				});
			}
		}

		#endregion

		public DropDownCodeDescriptionBoolRegistryItem ManagerSecurityMapping
		{
			get
			{
				return GetItem("ManagerSecurityMapping", delegate
				{
					var defaultList = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);

					defaultList.Add("MANAGERSECURITY1", ResString.GetMultilingualString("868BC1F6-CF63-45C6-B229-95AAD816BC09", "DRM"));

					return new DropDownCodeDescriptionBoolRegistryItem
					(
						"ManagerSecurityMapping",
						Categories.System_Staff_HRMS,
						ResString.GetMultilingualString("B2E54F5B-5232-4036-BE6C-AD49101C7A4B", "Manager Security Mapping"),
						ResString.GetMultilingualString("0EBAF3B0-ADD7-454B-99ED-AE9AD37ADC34", "When enabled, Manager security roles can be added to HRM by mapping manager codes from System -> Staff -> Staff Reporting Roles to '{0}' where X is 1, 2, or 3. Manager security roles cannot be mapped to more than one code, but one code can be mapped to multiple security roles", "ManagerSecurityX"),
						RegistryStorageFlags.System,
						ResString.GetMultilingualString("A8A04B8B-2DD1-451E-9AAD-6E830D4E7E79", "Enabled"),
						defaultList,
						16,
						ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup,
						ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup
					);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem PersonalRelationshipsList
		{
			get
			{
				return GetItem("PersonalRelationshipsList", delegate
				{
					var defaultValue = new CodeDescriptionPairList();

					defaultValue.AddPair("WIF", ResString.GetMultilingualString("8fda6d24-d0de-48df-8a9e-1d9f4f6c3fbb", "Wife"));
					defaultValue.AddPair("HUS", ResString.GetMultilingualString("c421e7f1-2f5f-43e2-b2b4-1ed849d51a16", "Husband"));
					defaultValue.AddPair("SON", ResString.GetMultilingualString("4f1f6c6e-bdc1-48cf-8d3e-d2d3d41e45a3", "Son"));
					defaultValue.AddPair("DAU", ResString.GetMultilingualString("a92a4c5d-4bfe-4d1b-88e6-929a66fecd94", "Daughter"));
					defaultValue.AddPair("PRT", ResString.GetMultilingualString("fbe8b1cd-657f-4044-9af8-cb75fb541983", "Partner"));
					defaultValue.AddPair("BRO", ResString.GetMultilingualString("b8a1db1f-34e1-4865-9533-2c96f6e964ba", "Brother"));
					defaultValue.AddPair("SIS", ResString.GetMultilingualString("5ae1c0a7-1f15-45c5-bd72-7d8cf74b3071", "Sister"));
					defaultValue.AddPair("SIB", ResString.GetMultilingualString("7f35a1eb-37c3-4a77-bc2a-06f42dce9c4d", "Sibling"));
					defaultValue.AddPair("MOT", ResString.GetMultilingualString("b57d6f22-1b6b-44d4-85a4-c2149646bde8", "Mother"));
					defaultValue.AddPair("FAT", ResString.GetMultilingualString("f0629f7c-6cf3-4966-bc93-6cb2c09c2d23", "Father"));
					defaultValue.AddPair("AUN", ResString.GetMultilingualString("1b3b5e64-7581-4c66-9e5a-bbe5b8ae9b7d", "Aunty"));
					defaultValue.AddPair("UNC", ResString.GetMultilingualString("b91b6491-4f6f-4b56-8e29-f9fd4c1e3d35", "Uncle"));
					defaultValue.AddPair("NEP", ResString.GetMultilingualString("98c5c994-bb82-424d-b1ab-7d64e7e2f128", "Nephew"));
					defaultValue.AddPair("NIE", ResString.GetMultilingualString("1b69b2e7-0ae3-46f8-8e4f-08b88e0c517c", "Niece"));
					defaultValue.AddPair("GUR", ResString.GetMultilingualString("e6b8a6e2-bf3b-45f3-bc47-4d2d86c1c4ae", "Guardian"));
					defaultValue.AddPair("PAR", ResString.GetMultilingualString("81a1e28b-9a1f-43ad-bf41-4ec678a781f4", "Parent"));
					defaultValue.AddPair("GPR", ResString.GetMultilingualString("bf98cf33-f765-4e4d-9864-bef7cc7e1c7f", "Grand Parent"));
					defaultValue.AddPair("COU", ResString.GetMultilingualString("c4967df5-ff1c-46de-a8d9-c03d3fe05425", "Cousin"));
					defaultValue.AddPair("FRI", ResString.GetMultilingualString("f9a2be73-6c9b-45db-bcb5-dc47c582b84b", "Friend"));

					return new CodeDescriptionPairListRegistryItem(
						"PersonalRelationshipsList",
						Categories.System_Staff_HRMS,
						ResString.GetMultilingualString("165eeae5-cc71-4f33-935e-bf60c9241d4b", "Personal Relationships"),
						ResString.GetMultilingualString("54e26c26-1857-4c5b-9642-564ad0a61c5a", "A list of personal relationships."),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue);
				});
			}
		}

		#region EnableCertificateManagementServiceTask

		public BooleanRegistryItem EnableCertificateManagementServiceTask
		{
			get
			{
				return GetItem("EnableCertificateManagementServiceTask", () =>
				{
					return new BooleanRegistryItem(
						"EnableCertificateManagementServiceTask",
						Categories.System_SystemToSystemTrust,
						(NoResString)"Enable System To System Trust Certificate Management Service Task",
						(NoResString)"Specifies if the System To System Trust Certificate Management Task is enabled.",
						RegistryStorageFlags.System,
						IsUATOrDevSystem() ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						!IsUATOrDevSystem());
				});
			}
		}

		bool IsUATOrDevSystem()
		{
			var productReg = ObjectFactory.Get<IProductRegistration>();
			return productReg.IsWiseTechGlobalInternalUATSystem() || productReg.IsWiseTechGlobalInternalDeveloperSystem();
		}

		#endregion

		public SystemToSystemTrustRegistryItem SystemToSystemCertificate
		{
			get
			{
				return GetItem("SystemToSystemCertificate", delegate
				{
					return new SystemToSystemTrustRegistryItem(
						"SystemToSystemCertificate",
						Categories.System_SystemToSystemTrust,
						(NoResString)"System to System Certificate",
						(NoResString)"This value contains the System to System Trust certificate information.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.PreserveTestValue
					);
				});
			}
		}

		public string AzureApplicationClientId => string.IsNullOrEmpty(SystemToSystemCertificate.Value.ClientId) ? (NoResString)"Not issued" : SystemToSystemCertificate.Value.ClientId;

		public StringRegistryItem EDIClientID
		{
			get
			{
				return GetItem("EDIClientID", delegate
				{
					var result = new StringRegistryItem(
						"EDIClientID",
						Categories.System_SystemToSystemTrust,
						(NoResString)"EDI Client ID",
						(NoResString)"It's a permanent id coming from Azure application which represents EDI.",
						RegistryStorageFlags.System,
						IsUATOrDevSystem() ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						"9a6ebfef-9638-4fdd-95bb-5394926c0422");
					return result;
				});
			}
		}

		public CodePairRegistryItem SystemToSystemTrustDataProtectionMechanism
		{
			get
			{
				return GetItem("SystemToSystemTrustDataProtectionMechanism", delegate
				{
					var result = new CodePairRegistryItem(
						"SystemToSystemTrustDataProtectionMechanism",
						Categories.System_SystemToSystemTrust_DataProtection,
						ResString.GetMultilingualString("ED55DCE0-9A77-4D6F-9029-7C75BE376932", "Data Protection Mechanism"),
						ResString.GetMultilingualString(
							"89E7DB8B-9FE6-4E11-8206-06CEBF4C33BC",
							@"Select the mechanism used for data protection of the System-to-System Trust private key, where:

{0} - means that the data protection is not activated.
{1} - means that DPAPI-NG data protection will be used. This requires additional setup, including setting up the {2} registry item.

Note: Changing this value can break the System-to-System Trust functionality by disassociating the private key and reliant services from the group.",
							DataProtectionMechanisms.Codes.None,
							DataProtectionMechanisms.Codes.ActiveDirectory,
							nameof(SystemToSystemTrustDataProtectionGroup)),
						new CodeDescriptionPairListProvider(() => new DataProtectionMechanisms()),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
						DataProtectionMechanisms.Codes.None);
					return result;
				});
			}
		}

		public SecurityIdentifierRegistryItem SystemToSystemTrustDataProtectionGroup
		{
			get
			{
				return GetItem("SystemToSystemTrustDataProtectionGroup", delegate
				{
					var result = new SecurityIdentifierRegistryItem(
						"SystemToSystemTrustDataProtectionGroup",
						Categories.System_SystemToSystemTrust_DataProtection,
						ResString.GetMultilingualString("39450D7C-8ACD-445F-89CF-A422017D15E2", "Data Protection Group"),
						ResString.GetMultilingualString(
							"E51DA07A-12CC-4C21-ADC1-464467AF7F5E",
							@"This is the AD group used to encrypt and decrypt protected data for System-to-System Trust.

Note: Changing this value can break the System-to-System Trust functionality by disassociating the private key and reliant services from the group."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
						new SecurityIdentifier(WellKnownSidType.WorldSid, null));
					return result;
				});
			}
		}

		#region Scim

		public BooleanRegistryItem EnableScimService
		{
			get
			{
				return GetItem("EnableScimService", delegate
				{
					return new BooleanRegistryItem(
						"EnableScimService",
						Categories.System_SCIM,
						(NoResString)"Enable Scim Service",
						(NoResString)$@"Enabling this registry item will enable the SCIM functionality for this {BrandingFactory.Instance.ProductName} system. This registry is used by various pieces of SCIM functionality to drive system behaviour. Only the I&S team should enable this setting because customers first need to have signed the Early Access Addendum.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		readonly StaffColumnToGroupNamesMappingUpdateAction updater = new StaffColumnToGroupNamesMappingUpdateAction();
		public StaffColumnToGroupDescriptionScimMappingRegistryItem StaffColumnToGroupNamesMapping
		{
			get
			{
				return GetItem("StaffColumnToGroupNamesMapping", delegate
				{
					var item = new StaffColumnToGroupDescriptionScimMappingRegistryItem(
						"StaffColumnToGroupNamesMapping",
						Categories.System_SCIM,
						ResString.GetMultilingualString("FADED85B-95F6-46F4-B5E3-1372290994C2", "Staff Column To Group Names Mapping"),
						ResString.GetMultilingualString("053321B7-863E-4F77-A402-03665D683E81", "This registry setting provides the mapping between Group Descriptions and Staff Field values. Group membership provides staff with a corresponding flag value upon SCIM import or update only. If you currently have any Staff open forms, please re-open them for these changes to take effect. Modifying this registry will affect corresponding group and staff records. Saving might take several minutes."),
						RegistryStorageFlags.System,
						RegistryOptions.Default);

					updater.UpdateCurrentScimMappingCollection(item.Value);

					item.OnUpdateAction = updater.Update;

					return item;
				});
			}
		}

		public CodePairRegistryItem ScimAuthenticationMethod
		{
			get
			{
				return GetItem("ScimAuthenticationMethod", delegate
				{
					var registryItem = new CodePairRegistryItem(
						"ScimAuthenticationMethod",
						Categories.System_SCIM_Authentication,
						ResString.GetMultilingualString("F30BFA85-8774-48E7-8C8E-D1D9775271F8", "SCIM Authentication Method"),
						ResString.GetMultilingualString("759D30F8-7E84-488B-B6B9-B22BBDC8E997", "Authentication method used by the SCIM listener endpoint."),
						ScimAuthenticationTypeProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						ScimConstants.AuthenticationCodes.AzureEntra)
					{
					};

					return registryItem;
				});
			}
		}

		ICodeDescriptionPairListProvider ScimAuthenticationTypeProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair(ScimConstants.AuthenticationCodes.AzureEntra, ScimConstants.AuthenticationDescriptions.AzureEntra),
						new CodeDescriptionPair(ScimConstants.AuthenticationCodes.ApiToken, ScimConstants.AuthenticationDescriptions.ApiToken),
					};
				});
			}
		}

		public StringRegistryItem ScimApiTokenAuthentication
		{
			get
			{
				return GetItem("ScimApiTokenAuthentication", delegate
				{
					var item = new StringRegistryItem(
						"ScimApiTokenAuthentication",
						Categories.System_SCIM_Authentication,
						ResString.GetMultilingualString("479AE0E6-0986-4AFE-874D-3B1CE90E3B4F", "SCIM API Token Authentication"),
						ResString.GetMultilingualString("FDBA6BA1-AD38-4029-BA37-EACFF9959A46", "SCIM API Token Authentication settings used by the SCIM listener endpoint.\r\nFor security reasons, the API key is only visible once when generated, a new key must be generated if previous key is lost."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController);

					item.DataType = new ScimApiTokenAuthenticationDataType();
					item.EditorInfo = null;
					return item;
				});
			}
		}

		public BooleanRegistryItem ScimClearPrivilegeFlagsOnMatching
		{
			get
			{
				return GetItem("ScimClearPrivilegeFlagsOnMatching", delegate
				{
					return new BooleanRegistryItem(
						"ScimClearPrivilegeFlagsOnMatching",
						Categories.System_SCIM,
						ResString.GetMultilingualString("1FB9A738-53F4-40AA-993D-E170B5E985A9", "Clear Privilege Flags On Matching"),
						ResString.GetMultilingualString("F57510B6-C5AF-438A-89F1-98920C2C5D42", "If enabled, this registry will clear following flags upon matching an external user to an existing user: Is Controller, Is Database Developer, Is Database Reader, Is Backup Operator."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						true);
				});
			}
		}

		public CodePairRegistryItem ScimSafeListType
		{
			get
			{
				return GetItem("ScimSafelistType", delegate
				{
					var registryItem = new CodePairRegistryItem(
						"ScimSafelistType",
						Categories.System_SCIM,
						ResString.GetMultilingualString("91C191C4-8AB7-48E9-BDAE-6E13B1A653EB", "Safelist Type"),
						ResString.GetMultilingualString("C24937D8-2295-4F4B-A5E2-F2A865FD3A1D", "Safelist Type used by SCIM service"),
						ScimSafeListTypeTypeProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						ScimConstants.SafelistTypes.None)
					{
					};

					return registryItem;
				});
			}
		}

		ICodeDescriptionPairListProvider ScimSafeListTypeTypeProvider
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					return new CodeDescriptionPairList
					{
						new CodeDescriptionPair(ScimConstants.SafelistTypes.None, ScimConstants.SafelistTypeDescriptions.None),
						new CodeDescriptionPair(ScimConstants.SafelistTypes.AzureEntra, ScimConstants.SafelistTypeDescriptions.AzureEntra),
					};
				});
			}
		}

		public BooleanRegistryItem ScimReturnAllGroupMembershipsForStaff
		{
			get
			{
				return GetItem("ScimReturnAllGroupMembershipsForStaff", delegate
				{
					return new BooleanRegistryItem(
						"ScimReturnAllGroupMembershipsForStaff",
						Categories.System_SCIM,
						ResString.GetMultilingualString("61A2E1F1-FEBE-43FF-A2FD-7DF1CF7EF3D5", "Show All Group Memberships For Staff"),
						ResString.GetMultilingualString("D160FAD0-538D-49C7-B0D4-51851AB4D920", "If enabled, user.groups collection will contain all groups regardless of whether or not they were provisioned by SCIM or created locally."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						false);
				});
			}
		}

		public BooleanRegistryItem ScimClearRoleFlagsOnMatching
		{
			get
			{
				return GetItem("ScimClearRoleFlagsOnMatching", delegate
				{
					return new BooleanRegistryItem(
						"ScimClearRoleFlagsOnMatching",
						Categories.System_SCIM,
						ResString.GetMultilingualString("AB12B024-3109-404B-B9E6-CA23A11890C9", "Clear Role Flags On Matching"),
						ResString.GetMultilingualString("9CCD7C7C-DBDE-4052-9B90-97C9BEA12B82", "If enabled, this registry will clear following flags upon matching an external user to an existing user: Is Sales Rep, Is Driver."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						false);
				});
			}
		}

		public BooleanRegistryItem ScimAllowLocalEditing
		{
			get
			{
				return GetItem("ScimAllowLocalEditing", delegate
				{
					return new BooleanRegistryItem(
						"ScimAllowLocalEditing",
						Categories.System_SCIM,
						ResString.GetMultilingualString("FA9D7C50-425E-4D5E-A0EA-39B88C5F43CF", "Allow Local Editing of SCIM users and groups"),
						ResString.GetMultilingualString("ABCF2B16-67B5-478A-8061-98851A148580", "If enabled, users and groups originating from an external Identity Provider and provisioned by SCIM become locally editable. Please note, this is not recommended unless your external Identity Provider is able to perform a sync via available SCIM endpoints. If not synchronised back to the Identity Provider, any future provisions may override local changes and cause data inconsistency."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						false);
				});
			}
		}

		#endregion

		#region Web Services

		public WebServicesConfigRegistryItem WebServicesConfigure
		{
			get
			{
				return GetItem<WebServicesConfigRegistryItem>("WebServicesConfigure", delegate
				{
					return new WebServicesConfigRegistryItem(
						"WebServicesConfigure",
						Categories.System_WebServices,
						(NoResString)"Configure Web Services",
						(NoResString)"Configurable Web Services. Changes here will be actioned automatically by the system. \r\n\r\nNote: Web services may appear as not modifiable if they have special requirements. Please raise an eRequest if you would like to request a change.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyForSupport,
						WebServicesConfigCollection.DefaultValue
						);
				});
			}
		}

		#endregion

		public BooleanRegistryItem AllowNonSupportDiagnosticsProfiling
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AllowNonSupportDiagnosticsProfiling", delegate
				{
					return new BooleanRegistryItem(
						"AllowNonSupportDiagnosticsProfiling",
						Categories.System_Diagnostics,
						(NoResString)"Allow Non-Support Diagnostics Profiling",
						(NoResString)"Setting this to yes enables non-support users on hosted systems to run dotTrace and dotMemory profiling tools.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}
	}
}
