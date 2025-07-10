using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.Integration.Licensing;
using Enterprise.MailManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using WTG.DevTools.Definitions;
using Constants = Enterprise.Core.Constants;
using RegistryOptions = Enterprise.Integration.RegistryOptions;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public sealed partial class RawDataRegistry : RegistryItemSet
	{
		public static RawDataRegistry Instance
		{
			get { return DataRegistry.Instance.RawRegistry; }
		}

		public override bool IsForProductivityWise => true;

		public static class EntityFrameworkRegistryDefaults
		{
			public static readonly bool ApplyIsNotNullToJoinOnFK;
			public static readonly bool DefaultToForceSeek;
			public static readonly bool ParameterizeInsertAndUpdateStatements = true;
			public static readonly bool ConcatenateMultipleFetchHintTypes;
			public static readonly bool LightValidationEnabled = true;
			public static readonly bool RunSelectTopNAsRowNumberQuery;
			public static readonly bool ReportCrossThreadFactoryAccess = true;
			public static readonly bool SuppressDbFilesHealthCheckNotificationsForHostedSystems = true;
			public static readonly string DebugBusinessObjectType = "";
			public static readonly bool ApplyOptionRecompile = true;
			public static readonly string SMTPEhloDomainForWiseGlobal = "wisetechglobal.com";
			public static readonly string FieldsToLiteralize = "AH_TransactionType, VX_ParentTableCode, VJ_TableName";
			public static readonly string TVPRule = "5,10,20,50,100";
			public static readonly int UberFactoryTimeoutPeriod = 600;
			public static readonly int MaximumParametersPerFetchHint = 64;
			public static readonly int DefaultHeartbeatPulseInMs = 120000;
		}

		#region Categories

		public abstract class Categories
		{
			public static MultilingualString eServices { get { return ResString.GetMultilingualString("6820e9e8-5f83-46b8-a85d-ec35d529fa73", "eServices"); } }
			public static MultilingualString eServices_NativeXML { get { return CombineCategories(eServices, ResString.GetMultilingualString("63514F5D-F360-4643-AF85-DE27014DBC70", "Native XML")); } }

			public static MultilingualString System { get { return ResString.GetMultilingualString("4D7C40B9-527C-4323-A6E9-B5B0A50899F6", "System"); } }
			public static MultilingualString System_ASSESS { get { return CombineCategories(System, ResString.GetMultilingualString("90211455-A829-4453-8C97-6478DCCB2685", "ASSESS")); } }
			public static MultilingualString System_Calendar { get { return CombineCategories(System, ResString.GetMultilingualString("50E1F2E5-4576-4649-83D2-248488EE5BA3", "Calendar")); } }
			public static MultilingualString System_Certificates { get { return CombineCategories(System, ResString.GetMultilingualString("0447FF26-DAE3-4F29-9AC2-C6FC869DCA40", "Certificates")); } }
			public static MultilingualString System_Database { get { return CombineCategories(System, ResString.GetMultilingualString("A2C715E8-26F5-4f80-9E2A-9D7DA2288B67", "Database")); } }
			public static MultilingualString System_Database_Backup { get { return CombineCategories(System_Database, ResString.GetMultilingualString("268C8301-6EDC-4908-91A8-1D3C1D0DBBCA", "Backup")); } }
			public static MultilingualString System_Database_DBCC { get { return CombineCategories(System_Database, ResString.GetMultilingualString("4AB03535-8860-4F68-832B-FF9017E3FC12", "DBCC")); } }
			public static MultilingualString System_Database_ISU { get { return CombineCategories(System_Database, ResString.GetMultilingualString("98459ef5-c1a6-40a0-a630-c28d0cc5377a", "ISU")); } }
			public static MultilingualString System_Database_GRC { get { return CombineCategories(System_Database, ResString.GetMultilingualString("1250448F-5453-476D-A344-0B1DFD2029C2", "GRC")); } }
			public static MultilingualString System_Database_UserOptions { get { return CombineCategories(System_Database, ResString.GetMultilingualString("7868264A-D3A9-45C3-BBAF-65AD4683A777", "User Options")); } }
			public static MultilingualString System_Database_Version { get { return CombineCategories(System_Database, ResString.GetMultilingualString("FD15536B-43B5-499B-BF8D-7B4C75C29800", "Version")); } }
			public static MultilingualString System_Database_AlwaysOn { get { return CombineCategories(System_Database, ResString.GetMultilingualString("CC6F24B9-38C2-47E8-A9BD-241AD6635A8A", "Always On")); } }
			public static MultilingualString System_DataExportSettings { get { return CombineCategories(System, ResString.GetMultilingualString("383F7505-9C2F-4965-A208-B764753E8E36", "Data Export Settings")); } }
			public static MultilingualString System_DataImportSettings { get { return CombineCategories(System, ResString.GetMultilingualString("86A2301C-76F4-4262-8F13-4BB71A9EBD61", "Data Import Settings")); } }
			public static MultilingualString System_Email { get { return CombineCategories(System, ResString.GetMultilingualString("570D4AE6-62D6-41d4-9577-0473D8C4BC35", "Email")); } }
			public static MultilingualString System_FormTopCaption { get { return CombineCategories(System, ResString.GetMultilingualString("f56b2b55-d748-45c3-9adb-76b8afb38850", "Form Top Caption")); } }
			public static MultilingualString System_Framework { get { return CombineCategories(System, ResString.GetMultilingualString("B0857976-1FB6-4191-B70D-F08E5780F648", "Framework")); } }
			public static MultilingualString System_FTPService { get { return CombineCategories(System, ResString.GetMultilingualString("50351e90-7bf0-4138-b6c5-3ef70dbbc890", "FTP Service")); } }
			public static MultilingualString System_GoogleMaps { get { return CombineCategories(System, ResString.GetMultilingualString("606D94B6-7581-4874-BD8B-550B51494EFC", "Google Maps")); } }
			public static MultilingualString System_Language { get { return CombineCategories(System, ResString.GetMultilingualString("C9B25AE2-9997-4939-8F4E-088E1019C113", "Language")); } }
			public static MultilingualString System_License { get { return CombineCategories(System, ResString.GetMultilingualString("44477EDB-886A-462C-8207-AA07CD728C8A", "License")); } }
			public static MultilingualString System_Messaging { get { return CombineCategories(System, ResString.GetMultilingualString("0DD1628E-5EF6-481F-8F67-331A726C5CAD", "Messaging")); } }
			public static MultilingualString System_Miscellaneous { get { return CombineCategories(System, ResString.GetMultilingualString("EE6B356E-A6FA-45F6-BD36-A31EB283B040", "Misc")); } }
			public static MultilingualString System_NumberFountain { get { return CombineCategories(System, ResString.GetMultilingualString("0ED61FF8-9305-4038-8DD5-58B8B1DC3958", "Number Fountain")); } }
			public static MultilingualString System_ArchiveManager { get { return CombineCategories(System, ResString.GetMultilingualString("6F0A7566-8277-47C4-ABDC-4EEADF7884C2", "Archive Manager")); } }
			public static MultilingualString System_OIDC { get { return CombineCategories(System, ResString.GetMultilingualString("692f074e-51c0-456a-a2fa-b9e1de605f72", "OpenID Connect")); } }
			public static MultilingualString System_ProcessController { get { return CombineCategories(System, ResString.GetMultilingualString("D6F4B630-070B-41c0-BD1B-7C58C1419EEC", "Process Controller")); } }
			public static MultilingualString System_RemoteApp { get { return CombineCategories(System, ResString.GetMultilingualString("3a63c0e4-522f-4e55-9d37-57bd807551d9", "RemoteApp")); } }
			public static MultilingualString System_SCIM { get { return CombineCategories(System, ResString.GetMultilingualString("59C65189-8CE1-4A3F-AFAD-7E1117A4C3BD", "SCIM")); } }
			public static MultilingualString System_SCIM_Logging { get { return CombineCategories(System_SCIM, ResString.GetMultilingualString("02E36AF3-2C17-4C97-9C0E-4D39DC708A57", "Logging")); } }
			public static MultilingualString System_SCIM_Authentication { get { return CombineCategories(System_SCIM, ResString.GetMultilingualString("88E925A5-98E9-40CF-A0C6-0B48C0818AA2", "Authentication")); } }
			public static MultilingualString System_Staff { get { return CombineCategories(System, ResString.GetMultilingualString("AE9946FF-65F9-44e9-BF70-ABF369EB1345", "Staff")); } }
			public static MultilingualString System_Staff_OIDCAuthentication { get { return CombineCategories(System_Staff, ResString.GetMultilingualString("d90fb4e6-c3d2-4215-a8b1-79cfbe533c4d", "OpenID Connect Authentication")); } }
			public static MultilingualString System_Staff_HRMS { get { return CombineCategories(System_Staff, ResString.GetMultilingualString("deee1874-3c92-4b18-9dcf-f8a905458b67", "HRMS")); } }
			public static MultilingualString System_Staff_CWSupportLogin { get { return CombineCategories(System_Staff, ResString.GetMultilingualString("BF6E87AC-0C79-46C8-87F2-D5B9219F6768", "Support User Login")); } }
			public static MultilingualString System_Staff_ActivityLogging { get { return CombineCategories(System_Staff, ResString.GetMultilingualString("76b7016f-1b40-48c7-8d47-7415a53bc86f", "Activity Logging")); } }
			public static MultilingualString System_SystemToSystemTrust { get { return CombineCategories(System, ResString.GetMultilingualString("DE649956-CA3C-4780-95D6-35BD48332707", "System-to-System Trust")); } }
			public static MultilingualString System_SystemToSystemTrust_DataProtection { get { return CombineCategories(System_SystemToSystemTrust, ResString.GetMultilingualString("70238A15-B735-491D-B120-F15E57152030", "Data Protection")); } }
			public static MultilingualString System_Groups { get { return CombineCategories(System, ResString.GetMultilingualString("f578598b-7490-476d-b21c-5d3affb92d19", "Groups")); } }
			public static MultilingualString System_Testing { get { return CombineCategories(System, ResString.GetMultilingualString("3DCF4F33-7766-42b9-AC9F-D70BEDF1336B", "Testing")); } }
			public static MultilingualString System_UI { get { return CombineCategories(System, ResString.GetMultilingualString("13C2E2E9-019A-4395-A35A-6800A84C5C57", "UI")); } }
			public static MultilingualString System_UI_RegionNumberFormat { get { return CombineCategories(System_UI, ResString.GetMultilingualString("97451390-48B3-47C1-AE01-2ED39ADEB050", "Region Number Format")); } }
			public static MultilingualString System_UniversalCopy { get { return CombineCategories(System, ResString.GetMultilingualString("9e5303e8-418c-4df7-a21b-14b3849729ef", "Universal Copy")); } }
			public static MultilingualString System_Upgrade { get { return CombineCategories(System, ResString.GetMultilingualString("64AD2033-E0C4-4D05-9B52-94A5A5E7048C", "Upgrade")); } }
			public static MultilingualString System_WiseCloud { get { return CombineCategories(System, ResString.GetMultilingualString("c7a8414e-28b5-481d-8feb-ece504728292", "Wise Cloud")); } }
			public static MultilingualString System_Workflow { get { return CombineCategories(System, ResString.GetMultilingualString("2C4EDBB1-9250-4FDC-A226-A4F553572B02", "Workflow")); } }
			public static MultilingualString System_AuditLogs { get { return CombineCategories(System, ResString.GetMultilingualString("803CFFE4-C02F-46E2-9489-03655878F1DB", "Audit Logs")); } }
			public static MultilingualString System_IdentityProvider { get { return CombineCategories(System, ResString.GetMultilingualString("F6AB9B8E-1C14-456F-85C8-82BEF3E11FB4", "Identity Provider")); } }
			public static MultilingualString System_IdentityProvider_CargoWiseUserSynchronization { get { return CombineCategories(System_IdentityProvider, ResString.GetMultilingualString("29B96651-6404-4B2F-9F29-FE209437F7DF", "CargoWise User Synchronization")); } }
			public static MultilingualString System_IdentityProvider_CargoWiseUserManagement { get { return CombineCategories(System_IdentityProvider, ResString.GetMultilingualString("F73EF879-609A-4CF7-A832-50EF4DDC8E10", "CargoWise User Management")); } }
			public static MultilingualString PhysicalServer { get { return ResString.GetMultilingualString("4B12DED4-528D-4f3e-9023-67196ABF9CFF", "Physical Server"); } }
			public static MultilingualString PhysicalServer_Mail { get { return CombineCategories(PhysicalServer, ResString.GetMultilingualString("21E5727A-9AC9-416C-88A4-64B788524E81", "Mail")); } }
			public static MultilingualString PhysicalServer_MailIn { get { return CombineCategories(PhysicalServer_Mail, ResString.GetMultilingualString("21E5727A-9AC9-416C-88A4-64B788524E82", "Incoming")); } }
			public static MultilingualString PhysicalServer_MailOut { get { return CombineCategories(PhysicalServer_Mail, ResString.GetMultilingualString("21E5727A-9AC9-416C-88A4-64B788524E83", "Outgoing")); } }
			public static MultilingualString PhysicalServer_Mail_OAuth2 { get { return CombineCategories(PhysicalServer_Mail, ResString.GetMultilingualString("21E5727A-9AC9-416C-88A4-64B788524E88", "{0} 2.0", "OAuth")); } }
			public static MultilingualString PhysicalServer_Mail_OAuth2_M365 { get { return CombineCategories(PhysicalServer_Mail_OAuth2, ResString.GetMultilingualString("317C6D46-9092-48D0-ADF6-92C42E1E2EA7", "Microsoft 365")); } }
			public static MultilingualString PhysicalServer_Mail_OAuth2_M365In { get { return CombineCategories(PhysicalServer_Mail_OAuth2_M365, ResString.GetMultilingualString("9E79CDFA-6C16-46F0-A8B3-25BD01D59423", "Incoming")); } }
			public static MultilingualString PhysicalServer_Mail_OAuth2_M365Out { get { return CombineCategories(PhysicalServer_Mail_OAuth2_M365, ResString.GetMultilingualString("FFF6AE0F-F6B9-497E-AAEC-07708452341C", "Outgoing")); } }
			public static MultilingualString PhysicalServer_Mail_OAuth2_Gmail { get { return CombineCategories(PhysicalServer_Mail_OAuth2, ResString.GetMultilingualString("309C9B2E-9037-4BFA-AA38-1978C7B676F3", "Google Mail")); } }
			public static MultilingualString PhysicalServer_Mail_OAuth2_GmailIn { get { return CombineCategories(PhysicalServer_Mail_OAuth2_Gmail, ResString.GetMultilingualString("539CE37C-3019-4B9C-B6B4-0A72FE3FF38D", "Incoming")); } }
			public static MultilingualString PhysicalServer_Mail_OAuth2_GmailOut { get { return CombineCategories(PhysicalServer_Mail_OAuth2_Gmail, ResString.GetMultilingualString("FA0EF39D-8244-439C-A035-5F8D3D957C6E", "Outgoing")); } }
			public static MultilingualString PhysicalServer_POP3 { get { return CombineCategories(PhysicalServer_MailIn, ResString.GetMultilingualString("E0EFB64C-0F39-483F-84DD-02F3E445E528", "POP3")); } }
			public static MultilingualString PhysicalServer_IMAP { get { return CombineCategories(PhysicalServer_MailIn, ResString.GetMultilingualString("E0EFB64C-0F39-483F-84DD-02F3E445E529", "IMAP")); } }
			public static MultilingualString PhysicalServer_SMTP { get { return CombineCategories(PhysicalServer_MailOut, ResString.GetMultilingualString("bc4df353-3ed3-4636-9b99-b67c99c0cec0", "SMTP")); } }
			public static MultilingualString PhysicalServer_DisplayGrid { get { return CombineCategories(PhysicalServer, ResString.GetMultilingualString("abbf506c-93f2-4718-af35-f6d786102d08", "Display Grid")); } }
			public static MultilingualString AutoRating { get { return ResString.GetMultilingualString("e3bb4f41-de0a-4ab1-ba00-b5da64c41798", "AutoRating"); } }
			public static MultilingualString AutoRating_GenericContainerClasses { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("607ab8e4-b8fd-4b23-a7f0-beb15865d384", "Generic Container Classes")); } }
			public static MultilingualString AutoRating_Calculation { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("059f2e44-5f51-421d-acbb-d2c8dad14dff", "Calculation")); } }
			public static MultilingualString AutoRating_Calculation_StorageandTimeRating { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("62dfdcad-ec7e-4100-bc6d-1c45228e3002", "Storage and Time Rating")); } }
			public static MultilingualString AutoRating_Calculation_PortTransportZoneDistanceRating { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("bd0a76d5-6d47-40b7-8643-4c149b345e8c", "Port Transport Zone Distance Rating")); } }
			public static MultilingualString AutoRating_Calculation_AgencyCalculator { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("660a6cda-3dfa-4d68-8281-bdd71667bcd3", "Agency Calculator")); } }
			public static MultilingualString AutoRating_ChargeCodes { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("4e58c646-742b-4401-8c20-2dc883938084", "Charge Codes")); } }
			public static MultilingualString AutoRating_ChargeCodes_Freight { get { return CombineCategories(AutoRating_ChargeCodes, ResString.GetMultilingualString("3a9df10c-90fa-4b4e-8cce-c5b059baec26", "Freight")); } }
			public static MultilingualString AutoRating_ValidityandNotificationPeriods { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("2ad093f7-2f8d-4401-9306-328e859836a9", "Validity and Notification Periods")); } }
			public static MultilingualString AutoRating_ChargeCodeGroups { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("8b64f0f1-d513-4f76-9682-0e9359a8923b", "Charge Code Groups")); } }
			public static MultilingualString AutoRating_CostMarkups { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("ae95aa47-e5a3-478e-9858-9b1d5051fc0d", "Cost Markups")); } }
			public static MultilingualString AutoRating_PortTransportZoneDistanceRating { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("40f45b69-0163-4c23-bb85-035d3bfcb205", "Port Transport Zone Distance Rating")); } }
			public static MultilingualString AutoRating_EmailNotification { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("62f6387d-3ddf-404f-910e-9151e3749468", "Email Notification")); } }
			public static MultilingualString Freight { get { return ResString.GetMultilingualString("3a9df10c-90fa-4b4e-8cce-c5b059baec26", "Freight"); } }
			public static MultilingualString Freight_Shipment { get { return CombineCategories(Freight, ResString.GetMultilingualString("5348f302-d6f5-42d9-9ef0-39c19229cb97", "Shipment")); } }
			public static MultilingualString Freight_Shipment_Packages { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("8de9537b-f4f7-4d10-9b50-6ecc335ab0c6", "Packages")); } }
			public static MultilingualString Freight_Shipment_EffectiveDateForMandatoryControllingCustomer { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("006CC91B-0216-4678-BAAD-8A3547FBCB11", "Effective Date for Mandatory Controlling Customer")); } }
			public static MultilingualString Freight_Shipment_EffectiveDateForMandatoryControllingAgent { get { return CombineCategories(Freight_Shipment, ResString.GetMultilingualString("96FCCB93-8374-4D3F-89AC-32274D684A9E", "Effective Date for Mandatory Controlling Agent")); } }
			public static MultilingualString Freight_HouseBills { get { return CombineCategories(Freight, ResString.GetMultilingualString("e7c594c3-1c7f-435c-98a6-2dd3c62638b1", "House Bills")); } }
			public static MultilingualString Freight_AWB { get { return CombineCategories(Freight, ResString.GetMultilingualString("2AFA89C9-F6B9-4ca1-9C36-A9BD59AB7B43", "AWB")); } }
			public static MultilingualString Freight_AWB_SecurityStatementToUSA { get { return CombineCategories(Freight_AWB, ResString.GetMultilingualString("CDAA541A-BE63-4AF4-9C7F-6A9507582364", "Security Statement to the USA")); } }
			public static MultilingualString Freight_AWB_MAWB { get { return CombineCategories(Freight_AWB, ResString.GetMultilingualString("94D9BBFC-FAED-498c-B5F6-881C3904B434", "MAWB")); } }
			public static MultilingualString Freight_AWB_HAWB { get { return CombineCategories(Freight_AWB, ResString.GetMultilingualString("EE902F4E-0ACB-4836-A0A0-32F09C277596", "HAWB")); } }
			public static MultilingualString Freight_AWB_MAWB_IssuingCarrierAgent { get { return CombineCategories(Freight_AWB_MAWB, ResString.GetMultilingualString("e7e20c6b-ae1e-4d4a-b8f5-a7e7d95b39b0", "Issuing Carrier Agent")); } }
			public static MultilingualString Freight_AWB_MAWB_DotMatrix { get { return CombineCategories(Freight_AWB_MAWB, ResString.GetMultilingualString("76D4062D-0BB3-4d22-96B6-F2777E40C3E7", "Dot Matrix")); } }
			public static MultilingualString Freight_AWB_MAWB_AsAgreed { get { return CombineCategories(Freight_AWB_MAWB, ResString.GetMultilingualString("76D4062D-0BB3-4d22-96B6-F2777E40FFE7", "As Agreed")); } }
			public static MultilingualString Freight_AWB_HAWB_DotMatrix { get { return CombineCategories(Freight_AWB_HAWB, ResString.GetMultilingualString("D3BD6C82-B3BD-4d2d-ADAE-B13E710917C2", "Dot Matrix")); } }
			public static MultilingualString Freight_AWB_HAWB_AsAgreed { get { return CombineCategories(Freight_AWB_HAWB, ResString.GetMultilingualString("6af6e6f8-601d-81ad-48a4-116f805fd279", "As Agreed")); } }
			public static MultilingualString Freight_AWB_HAWB_Laser { get { return CombineCategories(Freight_AWB_HAWB, ResString.GetMultilingualString("0D1C5B9A-146C-47c6-9111-CB9A87496DCE", "Laser")); } }
			public static MultilingualString Freight_Container { get { return CombineCategories(Freight, ResString.GetMultilingualString("204f3bde-0668-483f-a244-2b3712ba366f", "Container")); } }
			public static MultilingualString Freight_Notifications { get { return CombineCategories(Freight, ResString.GetMultilingualString("B0CDA98C-9218-4bd0-A61C-D0A1BD9175FA", "Notifications")); } }
			public static MultilingualString Freight_Notifications_Shipment { get { return CombineCategories(Freight_Notifications, ResString.GetMultilingualString("46E63641-7B62-4527-AB36-3B7AF8CD7993", "Shipment")); } }
			public static MultilingualString Freight_PRAMessaging { get { return CombineCategories(Freight, ResString.GetMultilingualString("8203d4fa-1560-4609-b111-56df8f7abe82", "PRA Messaging")); } }
			public static MultilingualString Freight_PackLine { get { return CombineCategories(Freight, ResString.GetMultilingualString("f13acd1e-035f-442e-926e-930774875ddd", "Pack Line")); } }
			public static MultilingualString Freight_PackLine_CustomAttribute1 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("e6b7b8ad-7295-4728-ad0c-7d18523b070a", "Custom Attribute 1")); } }
			public static MultilingualString Freight_PackLine_CustomAttribute2 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("4ec9f68d-75df-4566-872e-39e424033912", "Custom Attribute 2")); } }
			public static MultilingualString Freight_PackLine_CustomAttribute3 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("2a517515-4d96-4f35-8372-22bea68acda5", "Custom Attribute 3")); } }
			public static MultilingualString Freight_PackLine_CustomAttribute4 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("69666df0-b0e5-44b5-bdd9-9f2cd3cace06", "Custom Attribute 4")); } }
			public static MultilingualString Freight_PackLine_CustomDecimal1 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("9b7bb186-3d71-4bb5-ae4d-2180cc093914", "Custom Decimal 1")); } }
			public static MultilingualString Freight_PackLine_CustomDecimal2 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("ab7d674c-a6ed-48da-917d-f85079d8146f", "Custom Decimal 2")); } }
			public static MultilingualString Freight_PackLine_CustomDate1 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("28d9c3a3-422a-4b60-8fd6-acfebb5389f1", "Custom Date 1")); } }
			public static MultilingualString Freight_PackLine_CustomFlag1 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("864ee571-89da-45be-9ab4-3e073129751d", "Custom Flag 1")); } }
			public static MultilingualString Freight_PackLine_CustomDate2 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("e02e0919-f43e-4f7b-a147-1d4ec4b3c2fc", "Custom Date 2")); } }
			public static MultilingualString Freight_PackLine_CustomFlag2 { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("f88ee544-e524-467b-9f08-456c2cf42520", "Custom Flag 2")); } }
			public static MultilingualString Freight_PackLine_CustomHeading { get { return CombineCategories(Freight_PackLine, ResString.GetMultilingualString("f2d276c8-9c69-43a4-b4a7-cb6a0b054614", "Custom Heading")); } }
			public static MultilingualString Freight_CFX { get { return CombineCategories(Freight, ResString.GetMultilingualString("7cd8590a-eef7-4526-9c4f-cd3d087dfd41", "CFX")); } }
			public static MultilingualString Freight_CFX_ImportAir { get { return CombineCategories(Freight_CFX, ResString.GetMultilingualString("dfbd3188-8056-47d0-9e29-f4e29b2dece3", "Import Air")); } }
			public static MultilingualString Freight_CFX_ExportAir { get { return CombineCategories(Freight_CFX, ResString.GetMultilingualString("d1ddc835-3e88-4177-b8b9-a0c34f12c775", "Export Air")); } }
			public static MultilingualString Freight_CFX_ImportSea { get { return CombineCategories(Freight_CFX, ResString.GetMultilingualString("eedecf2d-9017-4bd5-9b7b-a22dd6d83ce7", "Import Sea")); } }
			public static MultilingualString Freight_CFX_ExportSea { get { return CombineCategories(Freight_CFX, ResString.GetMultilingualString("1ffe11fe-e20c-4f92-ae79-b064b8285452", "Export Sea")); } }
			public static MultilingualString PortTransport { get { return ResString.GetMultilingualString("ad7f8b1d-bbc6-4d0f-8c0f-7e77bc8e560c", "Port Transport"); } }
			public static MultilingualString Documents { get { return ResString.GetMultilingualString("6524f324-2007-43d6-b700-344bb5e402d3", "Documents"); } }
			public static MultilingualString Documents_Customs { get { return CombineCategories(Documents, ResString.GetMultilingualString("8b4395e1-4ff7-47af-9ef6-386fd07fe309", "Customs")); } }
			public static MultilingualString Documents_DigitalDocs { get { return CombineCategories(Documents, ResString.GetMultilingualString("6a899b81-652f-475f-883a-8317462c23d9", "Digital Docs")); } }
			public static MultilingualString Documents_DigitalDocs_Certification { get { return CombineCategories(Documents_DigitalDocs, ResString.GetMultilingualString("f2d387b2-3960-4ffb-a7a1-2e12f1754408", "Certification")); } }
			public static MultilingualString Documents_LandTransport { get { return CombineCategories(Documents, ResString.GetMultilingualString("f536f3ae-9a52-4a29-8912-bc1873f5b8f5", "Land Transport")); } }
			public static MultilingualString Documents_LandTransport_RequestForService { get { return CombineCategories(Documents_LandTransport, ResString.GetMultilingualString("5bad1150-349f-4f5d-bf0f-f054108d4851", "Request for Service")); } }
			public static MultilingualString Documents_LandTransport_AuthorizationForService { get { return CombineCategories(Documents_LandTransport, ResString.GetMultilingualString("2bb47998-1c1f-4ff9-9d15-297c6dd6916e", "Authorization for Service")); } }
			public static MultilingualString Documents_Forwarding { get { return CombineCategories(Documents, ResString.GetMultilingualString("9f387d4a-7c3c-447b-9abe-45f75dc15f52", "Forwarding")); } }
			public static MultilingualString Documents_Forwarding_Shipment { get { return CombineCategories(Documents_Forwarding, ResString.GetMultilingualString("9cab2949-bb39-4b97-9868-c0747fa1b8eb", "Shipment")); } }
			public static MultilingualString Documents_Forwarding_Shipment_AgentsInstruction { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("3cf84ff6-14c2-42bc-a2d2-53e802f6f789", "Agents Instruction")); } }
			public static MultilingualString Documents_Forwarding_Shipment_AgentsInstruction_Air { get { return CombineCategories(Documents_Forwarding_Shipment_AgentsInstruction, ResString.GetMultilingualString("3BAA5C3E-D91C-468d-B496-E2593E37E4E1", "Air")); } }
			public static MultilingualString Documents_Forwarding_Shipment_AgentsInstruction_Sea { get { return CombineCategories(Documents_Forwarding_Shipment_AgentsInstruction, ResString.GetMultilingualString("0C114243-CB92-4a15-8958-591C3C958B28", "Sea")); } }
			public static MultilingualString Documents_Forwarding_Shipment_CartageAdvice { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("a700f9dd-d8bc-4ac4-8dbd-969d77be6edc", "Cartage Advice")); } }
			public static MultilingualString Documents_Forwarding_Shipment_CartageAdvice_Export { get { return CombineCategories(Documents_Forwarding_Shipment_CartageAdvice, ResString.GetMultilingualString("50e94d0b-c3c6-4ec7-b9dd-5e7a45aec8ea", "Export")); } }
			public static MultilingualString Documents_Forwarding_Shipment_CartageAdvice_Import { get { return CombineCategories(Documents_Forwarding_Shipment_CartageAdvice, ResString.GetMultilingualString("915ea0a0-3134-4def-8a1f-05402ba270cb", "Import")); } }
			public static MultilingualString Documents_Forwarding_Shipment_TimeSlotRequest { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("eca62c60-2051-4be8-8156-394acc3b0690", "Time Slot Request")); } }
			public static MultilingualString Documents_Forwarding_Shipment_TimeSlotRequest_Export { get { return CombineCategories(Documents_Forwarding_Shipment_TimeSlotRequest, ResString.GetMultilingualString("D8B78655-04D4-4a28-920D-77125E43CC94", "Export")); } }
			public static MultilingualString Documents_Forwarding_Shipment_TimeSlotRequest_Import { get { return CombineCategories(Documents_Forwarding_Shipment_TimeSlotRequest, ResString.GetMultilingualString("14DDEAD8-F4ED-4125-9644-73919D9CD956", "Import")); } }
			public static MultilingualString Documents_Forwarding_Shipment_Ausfuhrbescheinigung { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("C44666C5-DD3C-4a98-AA11-7D4E247EA068", "Ausfuhrbescheinigung")); } }
			public static MultilingualString Documents_Forwarding_Shipment_BookingConfirmation { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("88e75968-85c7-4bcf-8479-2a184ff54cbe", "Booking Confirmation")); } }
			public static MultilingualString Documents_Forwarding_Shipment_ShipperDepartureNotice { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("b6e736f2-b7c0-4a9e-89e2-2993af7375b3", "Shipper Departure Notice")); } }
			public static MultilingualString Documents_Forwarding_Shipment_BillofLading { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("1f3b9aff-c4dd-4ea4-a28e-e9a64ab17d44", "Bill of Lading")); } }
			public static MultilingualString Documents_Forwarding_Shipment_CoLoadMasterManifest { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("3e64eba4-5af3-455e-a715-1debf75d29e8", "Co-Load Master Manifest")); } }
			public static MultilingualString Documents_Forwarding_Shipment_WeightsAndMeasurements { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("7531329b-8cce-4899-8806-adc708208864", "Weights And Measurements")); } }
			public static MultilingualString Documents_Forwarding_Shipment_WeightsAndMeasurements_ExportSea { get { return CombineCategories(Documents_Forwarding_Shipment_WeightsAndMeasurements, ResString.GetMultilingualString("223ce071-8da8-4d2d-8b85-1292952ff863", "Export Sea")); } }
			public static MultilingualString Documents_Forwarding_Shipment_WeightsAndMeasurements_ExportAir { get { return CombineCategories(Documents_Forwarding_Shipment_WeightsAndMeasurements, ResString.GetMultilingualString("9c4110eb-c8d8-46f9-90cb-e5bf70d6783c", "Export Air")); } }
			public static MultilingualString Documents_Forwarding_Shipment_AWBSecurityDeclaration { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("7a5ed0ba-067e-4e70-a531-bbcc35abb811", "AWB Security Declaration")); } }
			public static MultilingualString Documents_Forwarding_Shipment_DeliveryNote { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("10f460ef-4b91-4568-ad16-af5d15871241", "Delivery Note")); } }
			public static MultilingualString Documents_Forwarding_Shipment_RequestforMissingDocuments { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("e0dba87a-874c-44d0-8cdc-ada7c92609fe", "Request for Missing Documents")); } }
			public static MultilingualString Documents_Forwarding_Consol { get { return CombineCategories(Documents_Forwarding, ResString.GetMultilingualString("1f91e51d-6a38-47c5-9908-5ea880064184", "Consol")); } }
			public static MultilingualString Documents_Forwarding_Consol_ManifestConsol { get { return CombineCategories(Documents_Forwarding_Consol, ResString.GetMultilingualString("20812f84-0d0d-42ac-817c-644c794f4835", "Manifest Consol")); } }
			public static MultilingualString Documents_Forwarding_Consol_ManifestConsol_Import { get { return CombineCategories(Documents_Forwarding_Consol_ManifestConsol, ResString.GetMultilingualString("ec9edafd-76d8-458a-b53d-79f2a753a93d", "Import")); } }
			public static MultilingualString Documents_Forwarding_Consol_ManifestConsol_Export { get { return CombineCategories(Documents_Forwarding_Consol_ManifestConsol, ResString.GetMultilingualString("4f813abc-f096-482b-91b8-c5871014abd3", "Export")); } }
			public static MultilingualString Documents_Forwarding_Consol_ForwardingInstruction { get { return CombineCategories(Documents_Forwarding_Consol, ResString.GetMultilingualString("e00cecaa-184f-4a1b-8566-2bce4e7a984e", "Forwarding Instruction")); } }
			public static MultilingualString Documents_Forwarding_Consol_ConsolShipperDepartureNotice { get { return CombineCategories(Documents_Forwarding_Consol, ResString.GetMultilingualString("ed34aed1-8a85-4d1e-8d78-1332e71c511e", "Consol Shipper Departure Notice")); } }
			public static MultilingualString Documents_Forwarding_Consol_CargoLoadList { get { return CombineCategories(Documents_Forwarding_Consol, ResString.GetMultilingualString("296df232-eefd-4c69-8240-ff6429484cb2", "Cargo Load List")); } }
			public static MultilingualString Documents_Forwarding_Customs { get { return CombineCategories(Documents_Forwarding, ResString.GetMultilingualString("cb1c1617-c309-451b-8639-0a6c6496e946", "Customs")); } }
			public static MultilingualString Documents_Forwarding_Customs_RequestforMissingDocuments { get { return CombineCategories(Documents_Forwarding_Customs, ResString.GetMultilingualString("e0dba87a-874c-44d0-8cdc-ada7c92609fe", "Request for Missing Documents")); } }
			public static MultilingualString Documents_Forwarding_Customs_CartageAdvice { get { return CombineCategories(Documents_Forwarding_Customs, ResString.GetMultilingualString("211fb054-eb92-47e3-b60f-8373deafdd8f", "Cartage Advice")); } }
			public static MultilingualString Documents_QuotationsandRates { get { return CombineCategories(Documents, ResString.GetMultilingualString("9b43af51-c1af-4c9b-998a-9855f1be4ef0", "Quotations and Rates")); } }
			public static MultilingualString Documents_QuotationsandRates_TrailingPages { get { return CombineCategories(Documents_QuotationsandRates, ResString.GetMultilingualString("0cf1732f-e98b-4034-bfdc-7630cf0af60e", "Trailing Pages")); } }
			public static MultilingualString Documents_Organization { get { return CombineCategories(Documents, ResString.GetMultilingualString("4c15bd6c-eebe-4cde-82cb-3cf6e39e0b78", "Organization")); } }
			public static MultilingualString Documents_Organization_RoutingOrder { get { return CombineCategories(Documents_Organization, ResString.GetMultilingualString("b82ef96e-601f-4a0b-b6b3-5baf399e175c", "Routing Order")); } }
			public static MultilingualString Documents_Organization_AgentReplacementRoutingOrder { get { return CombineCategories(Documents_Organization, ResString.GetMultilingualString("9b42bdb9-e40d-4d1f-a8bf-cb52714c9226", "Agent Replacement Routing Order")); } }
			public static MultilingualString Documents_Organization_RoutingRecommendation { get { return CombineCategories(Documents_Organization, ResString.GetMultilingualString("cdd25da2-35e2-41d6-b9c7-e101d995fb3b", "Routing Recommendation")); } }
			public static MultilingualString Documents_PortTransport { get { return CombineCategories(Documents, ResString.GetMultilingualString("9cb0b61d-8cc3-46df-b8a9-f49cc81aeabf", "Port Transport")); } }
			public static MultilingualString Documents_PortTransport_TimeSlotConfirmation { get { return CombineCategories(Documents_PortTransport, ResString.GetMultilingualString("8ec971fd-bef3-4005-b9a1-c068114f5dd8", "Time Slot Confirmation")); } }
			public static MultilingualString Documents_PortTransport_CartageAdvice { get { return CombineCategories(Documents_PortTransport, ResString.GetMultilingualString("a4271553-c422-4e95-b677-f454eac2fdb2", "Cartage Advice")); } }
			public static MultilingualString Documents_UserSignOff { get { return CombineCategories(Documents, ResString.GetMultilingualString("6b4cbf30-73a9-4e54-81db-44a592c9fdef", "User Sign Off")); } }
			public static MultilingualString Documents_LinerAgency { get { return CombineCategories(Documents, ResString.GetMultilingualString("2fc3d93e-c77b-4586-8aad-fdbecb6c4871", "Liner & Agency")); } }
			public static MultilingualString Documents_LinerAgency_BookingConfirmation { get { return CombineCategories(Documents_LinerAgency, ResString.GetMultilingualString("c19ca2d2-85e1-49fd-a742-1aa93b90f95d", "Booking Confirmation")); } }
			public static MultilingualString Documents_LinerAgency_ArrivalNotice { get { return CombineCategories(Documents_LinerAgency, ResString.GetMultilingualString("6dba7640-635d-4b69-b10f-490acc2a6fd8", "Arrival Notice")); } }
			public static MultilingualString Documents_LinerAgency_ContainerRelease { get { return CombineCategories(Documents_LinerAgency, ResString.GetMultilingualString("5ed5dfa5-437c-4913-967a-f43ab0f567d5", "Container Release")); } }
			public static MultilingualString Documents_Booking { get { return CombineCategories(Documents, ResString.GetMultilingualString("23bb310f-6ccc-42fd-9f7f-eaf974c28813", "Booking")); } }
			public static MultilingualString Documents_Booking_BookingConfirmation { get { return CombineCategories(Documents_Booking, ResString.GetMultilingualString("995f6ac8-4011-4337-8c74-7288e8107b93", "Booking Confirmation")); } }
			public static MultilingualString Documents_Booking_CartageAdvice { get { return CombineCategories(Documents_Booking, ResString.GetMultilingualString("1a62b664-fce8-40ca-a13e-ee4181b82d0b", "Cartage Advice")); } }
			public static MultilingualString Documents_PrintOptions { get { return CombineCategories(Documents, ResString.GetMultilingualString("86ffb1fe-c889-41dd-b976-869854ad3824", "Print Options")); } }
			public static MultilingualString Documents_PrintOptions_PrintJobTimeout { get { return CombineCategories(Documents_PrintOptions, ResString.GetMultilingualString("67174fce-93b2-4c0a-916f-243c8d3c12cd", "Print Job Timeout")); } }
			public static MultilingualString Documents_CFS { get { return CombineCategories(Documents, ResString.GetMultilingualString("8a5d4b01-c8ad-4a41-ae48-3379f6ced206", "CFS")); } }
			public static MultilingualString Documents_CFS_CartageAdvice { get { return CombineCategories(Documents_CFS, ResString.GetMultilingualString("65692baf-8750-4526-a7be-b331f3c45fc9", "Cartage Advice")); } }
			public static MultilingualString Documents_Orders { get { return CombineCategories(Documents, ResString.GetMultilingualString("bbed9b2e-5b5f-4bc7-a97d-d2662706c0dc", "Orders")); } }
			public static MultilingualString Documents_Orders_ImportOrderAdvice { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("114bba56-c92f-42b1-b964-a787ee75897d", "Import Order Advice")); } }
			public static MultilingualString Documents_Orders_ExportOrderAdvice { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("d5c66b44-094e-4dbe-a753-245df19a3503", "Export Order Advice")); } }
			public static MultilingualString Documents_Orders_ImportOrderNotification { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("654c7833-5948-4d3d-bdfd-5f0b9d9999ac", "Import Order Notification")); } }
			public static MultilingualString Documents_Orders_ExportOrderNotification { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("2115b0e9-c9a3-4b07-ac8c-4ab5b070d70e", "Export Order Notification")); } }
			public static MultilingualString Documents_Orders_ImportOrderStatus { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("6768c2a7-0f84-497d-9edb-454eb98712e5", "Import Order Status")); } }
			public static MultilingualString Documents_Orders_ExportOrderStatus { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("96dc6d15-19a2-46ab-9990-7b5f44f6f996", "Export Order Status")); } }
			public static MultilingualString Documents_Orders_ImportShippedOnBoardAdvice { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("94917c21-9901-4878-b20c-771a3aa5e3b6", "Import Shipped On Board Advice")); } }
			public static MultilingualString Documents_Orders_ImportAmendmentToBooking { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("6474304a-da77-43e7-b8e7-1eac01afb828", "Import Amendment To Booking")); } }
			public static MultilingualString Customs { get { return ResString.GetMultilingualString("cb1c1617-c309-451b-8639-0a6c6496e946", "Customs"); } }
			public static MultilingualString Customs_DeclarationAuditing { get { return CombineCategories(Customs, ResString.GetMultilingualString("2EAC57A1-6904-4F34-A6E4-3CB432A3D492", "Customs Declaration Auditing")); } }
			public static MultilingualString Customs_CountryOrRegion { get { return CombineCategories(Customs, ResString.GetMultilingualString("D48E1329-DC66-4054-B88B-814EB6DC19A5", "Country or Region Specific")); } }
			public static MultilingualString Customs_InDevelopment { get { return CombineCategories(Customs, ResString.GetMultilingualString("C81D8A1D-6211-4CA4-A983-264281CDA0EE", "In Development")); } }
			public static MultilingualString Customs_Integration { get { return CombineCategories(Customs, ResString.GetMultilingualString("71a3df2a-ba37-48b9-9973-e550ccfd7d3a", "Integration")); } }
			public static MultilingualString Customs_Integration_UnitedKingdom { get { return CombineCategories(Customs_Integration, ResString.GetMultilingualString("{C476E6E2-F1BA-46F9-BC8A-7F655686A807}", "United Kingdom")); } }
			public static MultilingualString Customs_Australia { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("f2187849-2412-42d1-95d9-49b18cfe1d19", "Australia")); } }
			public static MultilingualString Customs_Australia_CMR { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("D27D1476-6479-4228-A898-BB89E616791D", "CMR")); } }
			public static MultilingualString Customs_Australia_ReferenceFiles { get { return CombineCategories(Customs_Australia_CMR, ResString.GetMultilingualString("74B64C38-D733-45FC-AE3B-51237F7AB16B", "Reference Files")); } }
			public static MultilingualString Customs_Australia_Testing { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("3BAFE144-840B-4b7e-87C6-BC02C175AD59", "Testing")); } }
			public static MultilingualString Customs_Australia_AirCargo { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("625a0863-a6c5-47fd-b83e-ce51199a073d", "Air Cargo")); } }
			public static MultilingualString Customs_Australia_SeaCargo { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("d604a508-8153-4e86-b538-bcedce828b81", "Sea Cargo")); } }
			public static MultilingualString Customs_Australia_Legacy { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("a2758ff3-bcb1-4643-b6aa-287c9ef56522", "Legacy")); } }
			public static MultilingualString Customs_Australia_ImportDeclaration { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("eedd32b7-5fab-46f7-8822-a53ac08fbb0a", "Import Declaration")); } }
			public static MultilingualString Customs_Australia_ExportDeclaration { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("12e18bd1-7727-4662-9de8-8b22c896eb6f", "Export Declaration")); } }
			public static MultilingualString Customs_Australia_ExportManifest { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("c9c66a55-7c20-4417-b3d3-8be8080b7246", "Export Manifest")); } }
			public static MultilingualString Customs_Australia_Underbond { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("8DA328B8-7DB3-40c8-9D75-544AA95172DF", "Underbond")); } }
			public static MultilingualString Customs_Australia_CargoStatus { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("0ea5d144-b474-4717-ad4e-ab5fe24d1948", "Cargo Status")); } }
			public static MultilingualString Customs_Australia_NEXDOCS { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("C1C5D09E-54F2-4EF1-80FB-9273D6767398", "NEXDOCS")); } }
			public static MultilingualString Customs_Australia_COLS { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("806FAB38-01B4-45CE-B61C-1CB4ABBAF082", "COLS")); } }
			public static MultilingualString Customs_ConsolidatedEntries { get { return CombineCategories(Customs, ResString.GetMultilingualString("F285B5C4-F142-4545-801B-B362C3055135", "Consolidated Entries")); } }
			public static MultilingualString Customs_EuropeanUnionCommon { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("661A688F-AAE7-40AB-B4C6-65E2C58D88EF", "European Union (common)")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_EMCS { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("76D631C3-AED1-4EAE-932A-F0B166084F57", "EMCS")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_Export { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("625D8BC3-B545-43A9-96E2-55991E1A745D", "Export")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_ICS2 { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("AC75A6B7-7DC6-483F-8984-ABB9CA7C223E", "ICS2")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_Intrastat { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("A6C19DF0-874D-4DBE-9DEA-3D8650DD941E", "Intrastat")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_Import { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("8E59E6BB-D9F3-4EED-ACC9-6900668F0C43", "Import")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_InwardProcessing { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("A8556210-0A71-4E6C-B2C6-EF746AF664BC", "Inward Processing")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_TemporaryStorage { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("9CE4A505-8AAB-4361-8D6F-CC190D4FF73B", "Temporary Storage")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_FiscalRepresentation { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("F28EAAE5-EE03-4C92-9DC9-1AAAC6FB6EF0", "Fiscal Representation")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_NCTS { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("949B7DF0-77F5-481E-9F9B-DBA9F8EB518F", "NCTS")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_ExitControl { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("DCC182B9-CE55-44B9-8DA6-47A334A7664D", "Exit Control")); } }
			public static MultilingualString Customs_EuropeanUnionCommon_H7 { get { return CombineCategories(Customs_EuropeanUnionCommon, ResString.GetMultilingualString("1FD63859-6BB8-431A-8421-83D773B772B7", "Low Value")); } }
			public static MultilingualString Customs_Germany { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("6A2063D6-3613-4792-9D60-5E8AFEB63626", "Germany")); } }
			public static MultilingualString Customs_Italy { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("8D8265F5-FE60-4397-A57C-8ACED63951B0", "Italy")); } }
			public static MultilingualString Customs_Japan { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("737E212F-0479-4C5C-B59E-06E7E121EAA2", "Japan")); } }
			public static MultilingualString Customs_SouthAfrica { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("5e100e04-2ac6-4939-8b17-cf6fe5ed886f", "South Africa")); } }
			public static MultilingualString Customs_Switzerland { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("2EBED29F-4129-4094-BBBA-FEC7D8E9E5D8", "Switzerland")); } }
			public static MultilingualString Customs_Packages { get { return CombineCategories(Customs, ResString.GetMultilingualString("71bdd9d3-a554-45f1-8e28-55265d30d9ee", "Packages")); } }
			public static MultilingualString Customs_Products { get { return CombineCategories(Customs, Products); } }
			public static MultilingualString Customs_Malaysia { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("c3b57319-ba10-4833-8a9e-949cc5db9c24", "Malaysia")); } }
			public static MultilingualString Customs_UnitedArabEmirates { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("32e5ba62-c848-42f5-80c1-51f6e851c5dd", "United Arab Emirates")); } }
			public static MultilingualString Customs_Netherlands { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("29159279-8670-4421-9B8B-C3B300981566", "Netherlands")); } }
			public static MultilingualString Customs_Norway { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("B809C03E-6F3A-41DD-8A37-684B34F24B0C", "Norway")); } }
			public static MultilingualString Customs_Belgium { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("4731D280-06B3-4E58-9F79-4AB7B91F7BFD", "Belgium")); } }
			public static MultilingualString Customs_Poland { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("F2E9441A-10F3-45EE-84FA-11F58D13D05D", "Poland")); } }
			public static MultilingualString Notification { get { return ResString.GetMultilingualString("aed55c44-9c44-43c0-a785-61f9ac3424b1", "Notification"); } }
			public static MultilingualString SalesMarketing { get { return ResString.GetMultilingualString("d5afd331-0ebe-4c28-9f7e-4a41739848a1", "Sales & Marketing"); } }
			public static MultilingualString SalesMarketing_ClientIntelligence { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("7fdb0b79-2290-4f23-97aa-2f2c618e50a9", "Client Intelligence")); } }
			public static MultilingualString SalesMarketing_ClientIntelligence_MarketingFlags { get { return CombineCategories(SalesMarketing_ClientIntelligence, ResString.GetMultilingualString("9910e75a-346f-4251-85f6-8208f371c024", "Marketing Flags")); } }
			public static MultilingualString SalesMarketing_CompetitorIntelligence { get { return CombineCategories(SalesMarketing, ResString.GetMultilingualString("c2c58ceb-2cd4-4d07-9c94-7ffcda6b5251", "Competitor Intelligence")); } }
			public static MultilingualString Optimization { get { return ResString.GetMultilingualString("23313E6C-DF0C-47e6-9FEA-D7DC4F9502FE", "Optimization"); } }
			public static MultilingualString Optimization_FetchHints { get { return CombineCategories(Optimization, ResString.GetMultilingualString("8D874F64-F392-46BF-8D40-28B1156EA50A", "Fetch Hints")); } }
			public static MultilingualString Optimization_FetchHints_ProcessTask { get { return CombineCategories(Optimization_FetchHints, ResString.GetMultilingualString("1B20AB00-4EB7-4FA2-8029-09771ED8A4FE", "Process Tasks")); } }
			public static MultilingualString Organizations { get { return CombineCategories(MasterData, ResString.GetMultilingualString("c000cbfe-654d-49b6-bdee-92c62224dd16", "Organizations")); } }
			public static MultilingualString Organizations_Codes { get { return CombineCategories(Organizations, ResString.GetMultilingualString("E1763FA4-F4BF-4966-A922-6D675D462855", "Codes")); } }
			public static MultilingualString Organizations_PatternMatch { get { return CombineCategories(Organizations, ResString.GetMultilingualString("207db182-b67d-40bb-8901-dff80a9b5ba6", "UXML Organization Matching")); } }
			public static MultilingualString Organizations_TempOrganizationRequiredFields { get { return CombineCategories(Organizations, ResString.GetMultilingualString("b6f65236-bf76-4b30-a85d-f3e15a1e1bf5", "Temp. Organization Required Fields")); } }
			public static MultilingualString Organizations_OrganizationRequiredFields { get { return CombineCategories(Organizations, ResString.GetMultilingualString("cb106c37-3ad8-41b8-b6fb-4c0ce24ec228", "Organization Required Fields")); } }
			public static MultilingualString Organizations_BuyerSupplierRelationships { get { return CombineCategories(Organizations, ResString.GetMultilingualString("f7265577-96b4-4a66-b1bc-c32c62ee6ebe", "Buyer Supplier Relationships")); } }
			public static MultilingualString Organizations_DataExchange { get { return CombineCategories(Organizations, ResString.GetMultilingualString("3D8FBADB-9270-4B09-900A-D4099371B7C3", "Data Exchange")); } }
			public static MultilingualString Organizations_DataExchange_Dynamics365 { get { return CombineCategories(Organizations_DataExchange, ResString.GetMultilingualString("B1C6969E-047F-4EA5-9394-69E414F03D21", "Dynamics 365")); } }
			public static MultilingualString Organizations_Shipment { get { return CombineCategories(Organizations, ResString.GetMultilingualString("E39D9443-F1B5-438a-B479-A9AB84A8317C", "Shipment")); } }
			public static MultilingualString Organizations_Rating { get { return CombineCategories(Organizations, ResString.GetMultilingualString("a8637830-a12a-4121-99a3-e7b967492c9f", "Rating")); } }
			public static MultilingualString Organizations_CodeLists { get { return CombineCategories(Organizations, ResString.GetMultilingualString("9e7b50a3-2b68-48ea-a7bf-253c396ddfbe", "Code Lists")); } }
			public static MultilingualString Organizations_ExternalValidationService { get { return CombineCategories(Organizations, ResString.GetMultilingualString("845A2246-1AA8-416F-B086-FF47BF40C77D", "External Validation Service")); } }
			public static MultilingualString Organizations_AddressValidationService { get { return CombineCategories(Organizations, ResString.GetMultilingualString("2EF86165-F117-4873-B1CA-0B607A8D815B", "Address Validation Service")); } }
			public static MultilingualString Operations { get { return ResString.GetMultilingualString("c001cbfe-654d-49b6-bbee-92c62224dd16", "Operations"); } }
			public static MultilingualString Operations_JobAddress { get { return CombineCategories(Operations, ResString.GetMultilingualString("c001cbfe-654d-49b6-bbee-92c62224dd17", "Job Address")); } }
			public static MultilingualString CFS { get { return ResString.GetMultilingualString("905fd05f-8a3d-4cd6-9311-cc8cd01b1b74", "CFS"); } }
			public static MultilingualString CFS_SeaFreight { get { return CombineCategories(CFS, ResString.GetMultilingualString("fe51b632-30d0-416f-8b39-180e1e74bd4f", "Sea Freight")); } }
			public static MultilingualString CFS_SeaFreight_General { get { return CombineCategories(CFS_SeaFreight, ResString.GetMultilingualString("fff120d4-763b-49c6-b4f4-62f38ad4137e", "General")); } }
			public static MultilingualString CFS_SeaFreight_DangerousGoods { get { return CombineCategories(CFS_SeaFreight, ResString.GetMultilingualString("3d10cee8-1123-4156-869e-e5df13c3c53c", "Dangerous Goods")); } }
			public static MultilingualString CFS_AirFreight { get { return CombineCategories(CFS, ResString.GetMultilingualString("ce4fa6a5-3c00-4752-8851-a4430d2491df", "Air Freight")); } }
			public static MultilingualString CFS_AirFreight_General { get { return CombineCategories(CFS_AirFreight, ResString.GetMultilingualString("9056df56-dd7f-4ddb-a1b1-69d9af59a3ab", "General")); } }
			public static MultilingualString CFS_AirFreight_DangerousGoods { get { return CombineCategories(CFS_AirFreight, ResString.GetMultilingualString("19475fb5-98d7-4d92-9f5c-39cb9ef1a6c8", "Dangerous Goods")); } }
			public static MultilingualString BorderWise { get { return ResString.GetMultilingualString("43926CF7-1619-44ea-8C63-CB5EF38ECBC4", "BorderWise"); } }
			public static MultilingualString Compliance { get { return ResString.GetMultilingualString("F3F63AE7-7197-40B0-BBFB-E2F31DFB1E99", "Compliance"); } }
			public static MultilingualString Compliance_Customs { get { return CombineCategories(Compliance, ResString.GetMultilingualString("2B5FA060-0B72-4DEF-901A-59B81EEB5817", "Customs")); } }
			public static MultilingualString WebAndVisibility { get { return ResString.GetMultilingualString("13c137cf-7067-4ff5-9f56-273c5dca76a9", "Web and Visibility"); } }
			public static MultilingualString Orders { get { return ResString.GetMultilingualString("0540f6cd-01aa-4683-9bd8-12aaec9f8968", "Orders"); } }
			public static MultilingualString ReferenceFiles { get { return ResString.GetMultilingualString("03286989-f253-4595-b597-c38a6429ca22", "Reference Files"); } }
			public static MultilingualString Forms { get { return ResString.GetMultilingualString("AEE46A34-C831-45dc-8A23-95AD90098BE9", "Forms"); } }
			public static MultilingualString PasswordControl { get { return ResString.GetMultilingualString("9974666d-d601-474d-a181-f71011c771dc", "Password Control"); } }
			public static MultilingualString Debug { get { return (NoResString)"Debug"; } } // Developer only registry category
			public static MultilingualString Warehouse { get { return ResString.GetMultilingualString("db5b39ca-f778-42be-ac23-9b7d7901b5db", "Warehouse"); } }
			public static MultilingualString Warehouse_General { get { return CombineCategories(Warehouse, ResString.GetMultilingualString("c984f621-7ad5-4be6-b0e3-078225abea45", "General")); } }
			public static MultilingualString Warehouse_Packages { get { return ResString.GetMultilingualString("387d4be5-8d39-4663-9dad-d05a6131271e", "Packages"); } }
			public static MultilingualString Warehouse_Products { get { return CombineCategories(Warehouse, Products); } }
			public static MultilingualString Accounting { get { return ResString.GetMultilingualString("6d21543f-5917-4382-8815-b1be5b82f534", "Accounting"); } }
			public static MultilingualString Accounting_JobInvoicing { get { return CombineCategories(Accounting, ResString.GetMultilingualString("5b8948e4-16a1-40c6-bbd5-474d8b49e330", "Job Invoicing")); } }
			public static MultilingualString WorkflowManager { get { return ResString.GetMultilingualString("F63AED76-7A26-4c67-9504-25830A992F18", "Workflow Manager"); } }
			public static MultilingualString WorkflowManager_QualityIteration { get { return CombineCategories(WorkflowManager, ResString.GetMultilingualString("467D6FE0-2428-4A29-96FF-76AC9D1E16F9", "Quality Iterations")); } }
			public static MultilingualString ProductivityTools { get { return ResString.GetMultilingualString("e6483210-f0f8-49ec-901d-8cd62db7858e", "Productivity Tools"); } }
			public static MultilingualString Products { get { return ResString.GetMultilingualString("96fb8c85-ed92-463d-9942-61cd17ca819e", "Products"); } }
			public static MultilingualString Transport { get { return ResString.GetMultilingualString("F6EDFA30-F2BF-4EC1-91E6-2FC93C514FE4", "Transport"); } }
			public static MultilingualString Transport_CarrierMessagingBuss { get { return CombineCategories(Transport, ResString.GetMultilingualString("62B805E1-14CB-4914-987D-E74FE1ECE1FF", "Carrier Messaging Buss")); } }
			public static MultilingualString Transport_Equipment { get { return CombineCategories(Transport, ResString.GetMultilingualString("AC9C8162-562D-426A-A2C2-EBBD07BD200D", "Equipment")); } }
			public static MultilingualString Transport_RTUS { get { return CombineCategories(Transport, ResString.GetMultilingualString("7FABF34F-FFB7-4DAB-BE5F-E7D2A9A2F183", "RTUS")); } }
			public static MultilingualString Transport_TransportBookings { get { return CombineCategories(Transport, ResString.GetMultilingualString("86E5FF01-4305-4A10-AAFC-C3137BD7165F", "Transport Bookings")); } }
			public static MultilingualString Transport_LandAndPortTransport { get { return CombineCategories(Transport, ResString.GetMultilingualString("3A790A97-9BEF-4E9D-BAD3-532A3223845C", "Land & Port Transport")); } }
			public static MultilingualString Transport_TransportBookings_ServiceTask { get { return CombineCategories(Transport_TransportBookings, ResString.GetMultilingualString("47518631-8530-4ce5-9c3a-1c813ea844f7", "Auto-Creation Service Task for Transport Jobs")); } }
			public static MultilingualString Transport_LandAndPortTransport_LandTransport { get { return CombineCategories(Transport_LandAndPortTransport, ResString.GetMultilingualString("D9043147-59BC-4C6C-9C3B-31477C9FD747", "Land Transport")); } }
			public static MultilingualString Transport_LandAndPortTransport_LandTransport_Optimization { get { return CombineCategories(Transport_LandAndPortTransport_LandTransport, ResString.GetMultilingualString("b285a150-20d0-4d89-a8c7-91141327b90d", "Optimization")); } } // Developer only registry category
			public static MultilingualString Transport_LandAndPortTransport_LandTransport_Mobility { get { return CombineCategories(Transport_LandAndPortTransport_LandTransport, ResString.GetMultilingualString("3786FA7F-E5FC-4D6F-B72E-B628FAC59691", "Mobility")); } }
			public static MultilingualString Transport_LandAndPortTransport_PortTransport { get { return CombineCategories(Transport_LandAndPortTransport, ResString.GetMultilingualString("5390640F-BDBB-4378-B66A-18DFADEFACBD", "Port Transport")); } }
			public static MultilingualString MasterData { get { return ResString.GetMultilingualString("5f128d0e-0f53-4963-835c-eb3a6f13b8c3", "Master Data"); } }
			public static MultilingualString Recruiter { get { return ResString.GetMultilingualString("f0ca77a0-0428-41db-b55c-15aa02f439cb", "Recruiter"); } }
			public static MultilingualString CargoWiseWebVersion { get { return CombineCategories(WebAndVisibility, ResString.GetMultilingualString("D8477E9E-5894-4182-B300-7E469010660C", "CargoWise Web Version")); } }

			#region Import / Export Sub Catagories

			public static MultilingualString BookingConfirmation { get { return ResString.GetMultilingualString("d7a6cb27-14ec-4e52-bae2-1f7c790ca368", "Booking Confirmation"); } }
			public static MultilingualString ShipperDepartureNotice { get { return ResString.GetMultilingualString("c7e5c158-0513-4f6b-909c-fc3d6162cdcc", "Shipper Departure Notice"); } }
			public static MultilingualString AgentDepartureNotice { get { return ResString.GetMultilingualString("38a0a3d7-6087-4c53-a97e-54fe598730ca", "Agent Departure Notice"); } }
			public static MultilingualString ArrivalNotice { get { return ResString.GetMultilingualString("df193296-d41c-4423-9abc-593e1b718d14", "Arrival Notice"); } }
			public static MultilingualString PreAlert { get { return ResString.GetMultilingualString("38581275-1b7b-426c-83f1-0c83a0e17920", "Pre Alert"); } }
			public static MultilingualString UltimateConsigneePreAlert { get { return ResString.GetMultilingualString("690ccd59-9299-4a94-84b0-ebaa2b7691e1", "Ultimate Consignee Pre Alert"); } }
			public static MultilingualString UltimateConsigneeArrivalNotice { get { return ResString.GetMultilingualString("0ec92e40-0c95-42ea-8ae4-116ee637594e", "Ultimate Consignee Arrival Notice"); } }
			public static MultilingualString ShippingAdvice { get { return ResString.GetMultilingualString("ad2ff60b-6090-4090-8c35-b219b6f3d218", "Shipping Advice"); } }
			public static MultilingualString DeliveryOrder { get { return ResString.GetMultilingualString("6504124c-3131-410f-8a25-773cce2c6e1b", "Delivery Order"); } }
			public static MultilingualString OutturnReport { get { return ResString.GetMultilingualString("951b7605-576e-4d51-bf67-ce509f8ee699", "Outturn Report"); } }
			public static MultilingualString LetterToOverseasAgent { get { return ResString.GetMultilingualString("29ce11de-b8fc-420e-b93d-252a7443b6c0", "Letter To Overseas Agent"); } }

			#endregion
		}

		#endregion

		#region System

		#region SuppressResourceStringsCheckRegion

		#region Native XML Support Date

		internal DateTimeRegistryItem NativeXMLSupportTillDate
		{
			get
			{
				return GetItem("NativeXMLSupportTillDate", delegate
				{
					var result = new DateTimeRegistryItem(
						"NativeXMLSupportTillDate",
						Categories.eServices_NativeXML,
						(NoResString)"Shipment/Declaration/Order Deprecated",
						(NoResString)"The last date that the system will support Native XML for Shipments, Declarations, and Orders. These were replaced by Universal Shipment XML in all 3 modules late in 2012.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short), RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport, DateTime.MinValue, false);
					result.DataType = new NativeXMLSupportTillDateDataType();
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region FTP

		public IntRegistryItem FTPConnectionTimeout
		{
			get
			{
				return GetItem("FTPConnectionTimeout", delegate
				{
					return new IntRegistryItem(
						"FTPConnectionTimeout",
						Categories.System_FTPService,
						ResString.GetMultilingualString("733BE002-E51F-44FD-BA3A-A4BAD941DDEE", "FTP Request Time-out"),
						ResString.GetMultilingualString("CE3D8C8C-8DD2-4FE0-BDDD-417DBC382830", "This Registry value specifies the number of minutes the application will wait for a response from the FTP server. If the server does not respond within the time-out period, the logs will display a web exception error indicating a time-out occurred."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						6
						);
				});
			}
		}

		public IntRegistryItem FTPReadTimeout
		{
			get
			{
				return GetItem("FTPReadTimeout", delegate
				{
					return new IntRegistryItem(
						"FTPReadTimeout",
						Categories.System_FTPService,
						ResString.GetMultilingualString("A0EE1F0B-D49F-4B5E-A346-87ADDA3B567A", @"FTP Read/Write Time-out"),
						ResString.GetMultilingualString("A4A007E8-8B82-4E3D-B923-2BA5D030A657", "This Registry value specifies the time-out period in minutes, for the Read and Write method of the FTP client. The Read method is used to read the data on the FTP client, and the Write method is used to write to the stream returned to the application. If the time-out period is exceeded, the logs will display a web exception error indicating a time-out occurred."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						5
						);
				});
			}
		}

		#endregion

		#region Import Wizard

		public StringArrayRegistryItem ProperCaseExcludeList
		{
			get
			{
				return GetItem("ProperCaseExcludeList", delegate
				{
					return new StringArrayRegistryItem(
						"ProperCaseExcludeList",
						Categories.System_Language,
						(NoResString)"ProperCase Exclude List", // This is a hidden registry item
						(NoResString)"Words to exclude when converting to ProperCase", // This is a hidden registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						Array.Empty<string>());
				});
			}
		}

		#endregion

		#region OperationalActionsRecordBatchSize

		public IntRegistryItem OperationalActionsRecordBatchSize
		{
			get
			{
				return GetItem("OperationalActionsRecordBatchSize", () =>
					new IntRegistryItem(
						"OperationalActionsRecordBatchSize",
						Categories.System_UI,
						ResString.GetMultilingualString("4587f836-c659-498a-b28b-75a478a76bc8", "Operational Actions Record Batch Size"),
						ResString.GetMultilingualString("0a5131cc-a66b-4c46-895e-1ea726cc94ea", "The number of records to process at any one time when running Operational Actions."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						CargoWise.EntityFramework.ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION,
						2,
						CargoWise.EntityFramework.ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION)
					);
			}
		}

		#endregion

		public BooleanRegistryItem ShowWarningPopupBeforeLaunchingURL
		{
			get
			{
				return GetItem("ShowWarningPopupBeforeLaunchingURL", delegate
				{
					return new BooleanRegistryItem("ShowWarningPopupBeforeLaunchingURL",
					Categories.System,
					ResString.GetMultilingualString("CEFA6663-5A32-4BFA-9C6B-957841F9CF86", "Enable a security warning pop-up before opening an unsafe link."),
					ResString.GetMultilingualString("6E3F2D95-0606-4A3F-BD5E-3FD3BF3FD06A", "This registry will allow security warning pop-up when user clicks an unsafe link."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					false);
				});
			}
		}

		public BooleanRegistryItem DisableRobotDetection
		{
			get
			{
				return GetItem("DisableRobotDetection", delegate
				{
					return new BooleanRegistryItem(
					name: "DisableRobotDetection",
					category: Categories.System,
					caption: (NoResString)"Disable Robot Detection", // This is a hidden registry item
					hint: (NoResString)"Disable the detection for UI RPA.", // This is a hidden registry item
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForSupport,
					defaultValue: false);
				});
			}
		}

		public BooleanRegistryItem EnableRPAForWiseCloud
		{
			get
			{
				return GetItem("EnableRPAForWiseCloud", delegate
				{
					return new BooleanRegistryItem(
					name: "EnableRPAForWiseCloud",
					category: Categories.System,
					caption: (NoResString)"Enable RPA Support", // This is a hidden registry item
					hint: (NoResString)"Allows Wisecloud Users to set the Is Robot flag on users and access RPA support. Do not enable this registry item in WiseCloud without CTO approval. Current WiseCloud infrastructure will not allow RPA without changes to the structure of our security model. If this is turned on in a hosted system without security changes to WiseCloud, the client will be billed for something they cannot use.", // This is a hidden registry item
					storage: RegistryStorageFlags.System,
					options: RegistryOptions.IsOnlyForSupport,
					defaultValue: false);
				});
			}
		}

		public IntRegistryItem StaffDetailsUpdateFrequency
		{
			get
			{
				return GetItem("StaffDetailsUpdateFrequency", delegate
				{
					return new IntRegistryItem(
						"StaffDetailsUpdateFrequency",
						Categories.System_Staff,
						ResString.GetMultilingualString("fdf49d29-b9f9-4c5d-9bf8-b396ef9f8789", "Staff Details Update Frequency"),
						ResString.GetMultilingualString("6b951836-6916-44f9-961e-e75ffa3307a1", "Specifies the number of days to elapse before each staff member is asked to check and update their staff details. If this is set to 0, staff members will not be reminded to update their profile."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						30);
				});
			}
		}

		public BinaryRegistryItem CWSupportLoginTokenCertificate
		{
			get
			{
				return GetItem(IdentityRegistry.CWSupportLoginTokenCertificateRegistryKey, delegate
				{
					return new BinaryRegistryItem(
						IdentityRegistry.CWSupportLoginTokenCertificateRegistryKey,
						Categories.System_Staff_CWSupportLogin,
						(NoResString)"CWSupport Login Token Certificate",
						(NoResString)"This certificate is used to validate CWSupport account login token.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.IsReadOnly,
						IdentityRegistry.GetCWSupportLoginTokenCertificateDefaultValue())
					{
						EditorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo()
					};
				});
			}
		}

		public StringRegistryItem UserContextErrorEmailTimestamps
		{
			get
			{
				return GetItem("UserContextErrorEmailTimestamps",
					() => new StringRegistryItem(
						"UserContextErrorEmailTimestamps",
						Categories.System_Miscellaneous,
						null,
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						string.Empty));
			}
		}

		public IntRegistryItem UserContextErrorEmailInterval
		{
			get
			{
				return GetItem("UserContextErrorEmailInterval",
					() => new IntRegistryItem(
						"UserContextErrorEmailInterval",
						Categories.System_Email,
						ResString.GetMultilingualString("81E12D66-5AA0-46AB-B1AC-9A8D24D0F1C8", "User Context Error Email Send Interval"),
						ResString.GetMultilingualString("54A0FF8C-EB33-4292-BE85-199FADC19106", "The interval in hours between sending two repeating error notification emails for an invalid user name setup in background processes."),
						RegistryStorageFlags.System,
						24));
			}
		}

		public BooleanRegistryItem RemoveNonAsciiCharactersInEmailMessageHeader
		{
			get
			{
				return GetItem("RemoveNonAsciiCharactersInEmailMessageHeader",
					() => new BooleanRegistryItem(
					"RemoveNonAsciiCharactersInEmailMessageHeader",
					Categories.System_Email,
					ResString.GetMultilingualString("55BDC0FB-3AEC-4E14-BAAC-B244FD6BF41F", "Remove NON-ASCII Characters in Email's Message Header"),
					ResString.GetMultilingualString("AAABFFDC-FCE7-4097-8AF0-30EE8523DBFC", @"The flag is to indicate that the NON-ASCII characters in email's message header should be removed or not.
Only enable this option if you see error like '{0}' in the OMS service task log.", "This message requires SMTPUTF8 extension and this SMTP server doesn't support it."),
					RegistryStorageFlags.System,
					false
				));
			}
		}

		#region .NET Version Switching
		public CodePairRegistryItem DefaultDotNetVersionOnLaunch
		{
			get
			{
				return GetItem("DEFAULT_DOTNET_VERSION_ON_LAUNCH", () =>
				{
					var hint = (NoResString)"""
					Specify the .NET version for the CargoWise application to use during launch. Example: .NET Framework 4.8, .NET Core 8.

					Note:
					 - This option is only visible and applicable in the ALP release.
					""";
					var targetTypeList = new DotNetBuildVersionTargetTypeList();
					return new CodePairRegistryItem(
						name: "DEFAULT_DOTNET_VERSION_ON_LAUNCH",
						category: Categories.System_Framework,
						caption: (NoResString)"Default .NET Version on launch",
						hint: hint,
						lookUpList: new CodeDescriptionPairListProvider(() => targetTypeList),
						defaultValue: targetTypeList.DefaultCode,
						storage: RegistryStorageFlags.System,
						options: GetRegistryOptionForNetVersionSwitch());
				});
			}
		}

		public BooleanRegistryItem EnableDotNetVersionSwitchMenu
		{
			get
			{
				return GetItem("ENABLE_DOTNET_VERSION_SWITCH_MENU", () =>
				{
					var hint = (NoResString)"""
					The flag is to enable menu option to launch a different .NET Version.

					Note:
					 - This option is only visible and applicable in the ALP release.
					""";
					return new BooleanRegistryItem(
						name: "ENABLE_DOTNET_VERSION_SWITCH_MENU",
						category: Categories.System_Framework,
						caption: (NoResString)"Enable launch .NET Version menu",
						hint: hint,
						storage: RegistryStorageFlags.System,
						defaultValue: false,
						options: GetRegistryOptionForNetVersionSwitch());
				});
			}
		}

		public RegistryOptions GetRegistryOptionForNetVersionSwitch()
		{
			return ReleaseInfo.Instance.ReleaseRing == ReleaseRings.Codes.ALP
				? RegistryOptions.IsOnlyForSupport
				: RegistryOptions.IsHidden;
		}
		#endregion

		#region .Net Version Pre Upgrade Check
		internal StringRegistryItem DotNetPreUpgradeCheckWhitelist
		{
			get
			{
				return GetItem("DOTNET_PRE_UPGRADE_CHECK_WHITE_LIST", delegate
				{
					return new StringRegistryItem(
						"DOTNET_PRE_UPGRADE_CHECK_WHITE_LIST",
						Categories.System_Framework,
						ResString.GetMultilingualString("43C88934-B33C-4E7C-BCE9-9BF6A67D37C3", ".NET Pre-Upgrade Validation Machine Ignore List"),
						ResString.GetMultilingualString("1D6046FC-DFBF-4932-982E-BF69B47FF055", "Specify the machines that you want to ignore during the .NET Pre-Upgrade Validation. These should be fully-qualified domain names (FQDN), separated by commas, is case-insensitive and ignores spaces. Example: machine.domain, machine.domain.zone"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						string.Empty
						);
				});
			}
		}

		internal BooleanRegistryItem EnableDotNetPreUpgradeCheck
		{
			get
			{
				return GetItem("ENABLE_DOTNET_PRE_UPGRADE_CHECK", delegate
				{
					return new BooleanRegistryItem(
						"ENABLE_DOTNET_PRE_UPGRADE_CHECK",
						Categories.System_Framework,
						ResString.GetMultilingualString("D65FB36A-6DAD-4DC1-BD91-D13919B6D7A3", "Enable .NET Pre-Upgrade Validation"),
						ResString.GetMultilingualString("E9A3B25D-D18D-422A-85A5-13EBC778FD4B", "Before installing an upgrade of CargoWise, validates that the required versions of .NET are installed. WARNING: If you disable this, CargoWise may not run after an upgrade. Only disable it temporarily if you know you have the correct versions of .NET installed on all computers that need it."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						true);
				});
			}
		}

		internal BooleanRegistryItem EnableEDIMessageInterpreter
		{
			get
			{
				return GetItem("EnableEDIMessageInterpreter", delegate
				{
					return new BooleanRegistryItem(
						"EnableEDIMessageInterpreter",
						Categories.System_Messaging,
						(NoResString)"Enable EDIMessage Interpreter", // it's just for internal usage so it should not be located
						(NoResString)"This registry item let you enable or disable completely the EDIMessage Interpreter module.", // it's just for internal usage so it should not be located
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		internal StringRegistryItem ExpectedClientDLL
		{
			get
			{
				return GetItem("EXPECTED_CLIENT_DLL", delegate
				{
					return new StringRegistryItem("EXPECTED_CLIENT_DLL",
						RawDataRegistry.Categories.System_License,
						(NoResString)"Expected Client-Specific DLL",
						(NoResString)"The client-specific DLL expected by this installation of this application.",
						new ExpectedClientDLLRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						null);
				});
			}
		}

		internal BooleanRegistryItem ReportAllNonCriticalSqlErrors
		{
			get
			{
				return GetItem("ReportAllNonCriticalSqlErrors", delegate
				{
					return new BooleanRegistryItem(
						"ReportAllNonCriticalSqlErrors",
						Categories.System_Database_UserOptions,
						(NoResString)"Report all non-critical SQL Server errors",
						(NoResString)"Report back non-critical SQL Server errors even if they have an associated user-friendly message.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		#region System\Database

		#region SuppressResourceStringsCheckRegion

		internal IntRegistryItem DatabaseMajorSchemaVersion
		{
			get
			{
				return GetItem(DbRegistry.DatabaseMajorSchemaVersion.ItemName, delegate
				{
					return new IntRegistryItem(
						DbRegistry.DatabaseMajorSchemaVersion.ItemName,
						Categories.System_Database_Version,
						(NoResString)"Major Schema Version",
						(NoResString)"Major version of the database schema. Used by the database upgrade engine to check if the database schema is in sync with the application. Major version downgrades are not allowed.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IRegistryItem DatabaseMinorSchemaVersion
		{
			get
			{
				return GetItem(DbRegistry.DatabaseMinorSchemaVersion.ItemName, delegate
				{
					return new IntRegistryItem(
						DbRegistry.DatabaseMinorSchemaVersion.ItemName,
						Categories.System_Database_Version,
						(NoResString)"Minor Schema Version",
						(NoResString)"Minor version of the database schema. Minor version can be both upgraded and downgraded.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IRegistryItem DatabaseMajorScriptVersion
		{
			get
			{
				return GetItem(DbRegistry.DatabaseMajorScriptVersion.ItemName, delegate
				{
					return new IntRegistryItem(
						DbRegistry.DatabaseMajorScriptVersion.ItemName,
						Categories.System_Database_Version,
						(NoResString)"Major Script Version",
						(NoResString)"Major Version of the database scripts (Views, Procedures, Functions & Triggers).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IRegistryItem DatabaseMinorScriptVersion
		{
			get
			{
				return GetItem(DbRegistry.DatabaseMinorScriptVersion.ItemName, delegate
				{
					return new IntRegistryItem(
						DbRegistry.DatabaseMinorScriptVersion.ItemName,
						Categories.System_Database_Version,
						(NoResString)"Minor Script Version",
						(NoResString)"Minor Version of the database scripts (Views, Procedures, Functions & Triggers).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IRegistryItem DatabaseMajorAnalyticsReportProjectUpgradeVersion
		{
			get
			{
				return GetItem("DatabaseMajorAnalyticsReportProjectUpgradeVersion", delegate
				{
					return new IntRegistryItem(
						"DatabaseMajorAnalyticsReportProjectUpgradeVersion",
						Categories.System_Database_Version,
						(NoResString)"Major Analytics Report Project Upgrade Version",
						(NoResString)"Major Version of Analytics reports.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IRegistryItem DatabaseMinorAnalyticsReportProjectUpgradeVersion
		{
			get
			{
				return GetItem("DatabaseMinorAnalyticsReportProjectUpgradeVersion", delegate
				{
					return new IntRegistryItem(
						"DatabaseMinorAnalyticsReportProjectUpgradeVersion",
						Categories.System_Database_Version,
						(NoResString)"Minor Analytics Report Project Upgrade Version",
						(NoResString)"Minor Version of Analytics reports.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IRegistryItem DatabaseMajorAuditReportProjectUpgradeVersion
		{
			get
			{
				return GetItem("DatabaseMajorAuditReportProjectUpgradeVersion", delegate
				{
					return new IntRegistryItem(
						"DatabaseMajorAuditReportProjectUpgradeVersion",
						Categories.System_Database_Version,
						(NoResString)"Major Audit Report Project Upgrade Version",
						(NoResString)"Major Version of Audit reports.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IRegistryItem DatabaseMinorAuditReportProjectUpgradeVersion
		{
			get
			{
				return GetItem("DatabaseMinorAuditReportProjectUpgradeVersion", delegate
				{
					return new IntRegistryItem(
						"DatabaseMinorAuditReportProjectUpgradeVersion",
						Categories.System_Database_Version,
						(NoResString)"Minor Audit Report Project Upgrade Version",
						(NoResString)"Minor Version of Audit reports.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IntRegistryItem DatabaseMajorTransformationVersion
		{
			get
			{
				return GetItem("DatabaseMajorTransformationVersion", delegate
				{
					return new IntRegistryItem(
						"DatabaseMajorTransformationVersion",
						Categories.System_Database_Version,
						(NoResString)"Major Transformation Version",
						(NoResString)"Major Version of the database transformation.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IntRegistryItem DatabaseMinorTransformationVersion
		{
			get
			{
				return GetItem("DatabaseMinorTransformationVersion", delegate
				{
					return new IntRegistryItem(
						"DatabaseMinorTransformationVersion",
						Categories.System_Database_Version,
						(NoResString)"Minor Transformation Version",
						(NoResString)"Minor Version of the database transformation.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IntRegistryItem DatabaseMajorClrAssembliesVersion
		{
			get
			{
				return GetItem("DatabaseMajorClrAssembliesVersion", delegate
				{
					return new IntRegistryItem(
						"DatabaseMajorClrAssembliesVersion",
						Categories.System_Database,
						(NoResString)"Major CLR Version",
						(NoResString)"Major Version of the database CLR assemblies.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IntRegistryItem DatabaseMinorClrAssembliesVersion
		{
			get
			{
				return GetItem("DatabaseMinorClrAssembliesVersion", delegate
				{
					return new IntRegistryItem(
						"DatabaseMinorClrAssembliesVersion",
						Categories.System_Database,
						(NoResString)"Minor CLR Version",
						(NoResString)"Minor Version of the database CLR assemblies.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IntRegistryItem DatabaseSystemDataVersionMajor
		{
			get
			{
				return GetItem("DatabaseSystemDataVersionMajor", delegate
				{
					return new IntRegistryItem(
						"DatabaseSystemDataVersionMajor",
						Categories.System_Database_Version,
						(NoResString)"Major System Data Version",
						(NoResString)"Major Version of the system data shipped by the application. Also used by the database upgrade engine to check if system data is in sync with the application.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		internal IntRegistryItem DatabaseSystemDataVersionMinor
		{
			get
			{
				return GetItem("DatabaseSystemDataVersionMinor", delegate
				{
					return new IntRegistryItem(
						"DatabaseSystemDataVersionMinor",
						Categories.System_Database_Version,
						(NoResString)"Minor System Data Version",
						(NoResString)"Minor Version of the system data shipped by the application. Also used by the database upgrade engine to check if system data is in sync with the application.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.IsReadOnly);
				});
			}
		}

		public StringRegistryItem ComplianceVersionNumber
		{
			get
			{
				return GetItem("ComplianceVersionNumber", delegate
				{
					return new StringRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
							"ComplianceVersionNumber",
							Categories.System_Database_Version,
							(NoResString)FormattableString.Invariant($"Compliance Version"), // Developer only message. No need to use ResString.
							(NoResString)FormattableString.Invariant($"This registry displays the latest version that is compliant with the requirements for use in a country."), // Developer only message. No need to use ResString.
							RegistryDataTypes.StringType,
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport,
							ComplianceVersionNumberDefaultValueGetter)
					);
				});
			}
		}

		object ComplianceVersionNumberDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var result = string.Empty;
			var company = companyPK == EnvProxy.Instance.CurrentCompany.PK ?
				EnvProxy.Instance.CurrentCompany : (ICompany)new BusinessObjectFactory().Load(ObjectFactory.GetType("IGlbCompany"), companyPK);
			var countryCode = company?.Country?.Code ?? string.Empty;

			var complianceInfo = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(countryCode);
			if (complianceInfo != null)
			{
				result = complianceInfo.GetComplianceVersionNo();
			}
			return result;
		}

		internal StringRegistryItem OnlineTransformationStatus
		{
			get
			{
				return GetItem(nameof(OnlineTransformationStatus), () =>
					new StringRegistryItem(
						nameof(OnlineTransformationStatus),
						Categories.System_Database,
						(NoResString)"Status of online transformation tasks",
						(NoResString)"Status of online transformation tasks, use by OnlineTransformationProvider",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						null)
				);
			}
		}

		#region SuppressDbFilesHealthCheckNotificationsForHostedSystems

		public BooleanRegistryItem SuppressDbFilesHealthCheckNotificationsForHostedSystems
		{
			get
			{
				return GetItem("SuppressDbFilesHealthCheckNotificationsForHostedSystems", delegate
				{
					return new BooleanRegistryItem("SuppressDbFilesHealthCheckNotificationsForHostedSystems",
						Categories.System_Database,
						ResString.GetMultilingualString("65342099-4E53-4E2D-B805-5CBDEE67CC72", "Suppress DB files health check notifications for hosted systems"),
						ResString.GetMultilingualString("A0EF2AAB-D098-48BA-8B1E-E4FAEE417CB6", @"If turned on, Database Health Check service task won't send a notification if database data file(s) and database log file(s) are located on the same drive."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						EntityFrameworkRegistryDefaults.SuppressDbFilesHealthCheckNotificationsForHostedSystems);
				});
			}
		}

		#endregion

		internal IntRegistryItem HeartbeatDurationSeconds
		{
			get
			{
				return GetItem("HeartbeatDuration", delegate
				{
					return new IntRegistryItem(
						name: "HeartbeatDuration",
						category: Categories.System_Database,
						caption: ResString.GetMultilingualString("d9d683d3-e1b1-439e-b754-6f204143dabf", "Heartbeat duration"),
						hint: ResString.GetMultilingualString("1a91e5dd-72cf-45f4-8a64-546b3b5904e8", "The number of seconds that a heartbeat will survive without being refreshed. Refresh is performed automatically at 40% of this value."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForCargoWise,
						defaultValue: 2700,
						minValue: 1500,
						maxValue: 7200
						);
				});
			}
		}

		internal IRegistryItem LockTimeout
		{
			get
			{
				return GetItem(DbRegistry.LockTimeout.ItemName, delegate
				{
					return new IntRegistryItem(
						name: DbRegistry.LockTimeout.ItemName,
						category: Categories.System_Database_UserOptions,
						caption: ResString.GetMultilingualString("5a54c9c1-2427-47a3-ae42-2aa1741127d3", "Lock Timeout"),
						hint: ResString.GetMultilingualString("1cfb4048-d979-4807-959b-b25e67c2338e", "The number of milliseconds that a connection waits for a blocking lock to go away before an error is returned from SQL Server to the application.\r\n\r\nNote:\r\n\t0 means to don't wait\r\n\t-1 means to wait indefinitely"),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForController,
						defaultValue: DbConnection.LockTimeout.Default,
						minValue: -1,
						maxValue: int.MaxValue
						);
				});
			}
		}

		#endregion

		internal IntRegistryItem ServiceTaskHeartbeatDurationSeconds
		{
			get
			{
				return GetItem("ServiceTaskHeartbeatDuration", delegate
				{
					return new IntRegistryItem(
						name: "ServiceTaskHeartbeatDuration",
						category: Categories.System_ProcessController,
						caption: ResString.GetMultilingualString("85c3a5ab-9429-452c-a473-097b1d9eb028", "Service Task Heartbeat Duration"), // hidden registry
						hint: ResString.GetMultilingualString("a656ae61-a317-4f74-9272-bbc9a69c30c9", "The number of seconds that a heartbeat will survive without being refreshed for single instance service tasks. Refresh is performed automatically at 40% of this value."), // hidden registry
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForCargoWise,
						defaultValue: 900,
						minValue: 60,
						maxValue: 1800
					);
				});
			}
		}

		internal BooleanRegistryItem RemoveOldUpgradePackages
		{
			get
			{
				return GetItem("RemoveOldUpgradePackages", delegate
				{
					return new BooleanRegistryItem(
						"RemoveOldUpgradePackages",
						Categories.System_Database,
						ResString.GetMultilingualString("AE88224C-D86F-4a21-B479-35F2390A2752", "Remove Old Upgrade Packages"),
						ResString.GetMultilingualString("D8CEEEA1-D220-48d0-AF9B-A28084999590", "Automatically remove old upgrade packages from the database when the system is upgraded."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#region DatabaseRecoveryModel

		public CodePairRegistryItem DatabaseRecoveryModel
		{
			get
			{
				return GetItem("DatabaseRecoveryModel", delegate
				{
					var options = RegistryOptions.PreserveTestValue | RegistryOptions.NotCached;
					if (EnvProxy.Instance.IsProductionSystem || Db.Connection.IsAlwaysOnEnabled || DataUtils.IsWiseTechGlobalDatabaseServer(Db.Connection))
					{
						options |= RegistryOptions.IsHidden;
					}

					return new CodePairRegistryItem(
						"DatabaseRecoveryModel",
						Categories.System_Database,
						ResString.GetMultilingualString("CFC4102E-B6D9-491C-881A-C00536F528CC", "Recovery Model of Databases"),
						ResString.GetMultilingualString("D2D6733D-3EDF-4CE5-BF60-BC64DDF78AD6", "Recovery model controls how transactions are logged, whether the transaction log requires backing up, and what kinds of restore operations are available."),
						new CodeDescriptionPairListProvider(() => GetDatabaseRecoveryModelList()),
						RegistryStorageFlags.System,
						options,
						GetDatabaseRecoveryModelList().DefaultCode);
				});
			}
		}

		internal CodeDescriptionPairList GetDatabaseRecoveryModelList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.DatabaseRecoveryModel.Codes.Simple, Constants.DatabaseRecoveryModel.Descriptions.Simple);
			result.AddPair(Constants.DatabaseRecoveryModel.Codes.Full, Constants.DatabaseRecoveryModel.Descriptions.Full);
			result.DefaultCode = (Globals.IsDebugMode ? Constants.DatabaseRecoveryModel.Codes.Simple : Constants.DatabaseRecoveryModel.Codes.Full);

			return result;
		}

		#endregion

		internal BooleanRegistryItem KeepOnlyLatestVersion
		{
			get
			{
				return GetItem("KeepOnlyLatestVersion", delegate
				{
					return new BooleanRegistryItem(
						"KeepOnlyLatestVersion",
						Categories.System_Database,
						ResString.GetMultilingualString("56A6BD60-02D1-4BBB-985F-62EA909B7B61", "Keep Only Latest Version"),
						ResString.GetMultilingualString("E7A77817-8C6A-4324-B652-E2C3A560BEE1", "Automatically remove old upgrade packages from the database when the latest package is downloaded."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#region Backup

		public StringRegistryItem BackupDirectoryPath
		{
			get
			{
				return GetItem("BackupFilePath", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"BackupFilePath",
						Categories.System_Database_Backup,
						ResString.GetMultilingualString("a8ef7258-d197-4501-9689-33058c670568", "Backup/Directory Path"),
						ResString.GetMultilingualString("da262ae4-5c5b-4634-9940-6a4140c52184", "Folder where database backup files are created (relative to DB server)"),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise) ? RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue : RegistryOptions.PreserveTestValue,
						"");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public BooleanRegistryItem BackupSystemDatabases
		{
			get
			{
				return GetItem("BackupSystemDatabases", delegate
				{
					return new BooleanRegistryItem(
						"BackupSystemDatabases",
						Categories.System_Database_Backup,
						ResString.GetMultilingualString("5C7BF906-3201-4220-8175-DFA6A624090B", "Backup System Databases"),
						ResString.GetMultilingualString("4BCF809F-9AC6-475C-B90C-E9748392C74F", "Option to back up master, msdb and model databases."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise) ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem BackupReferenceDatabases
		{
			get
			{
				return GetItem("BackupReferenceDatabases", delegate
				{
					return new BooleanRegistryItem(
						"BackupReferenceDatabases",
						Categories.System_Database_Backup,
						ResString.GetMultilingualString("C6CB16FF-9E6A-433A-BD43-2F9357104C9C", "Backup Reference Databases"),
						ResString.GetMultilingualString("89178485-7CAD-452C-A00B-E90C9FA8C4E6", "Option to back up the reference databases."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		internal BooleanRegistryItem SkipBackupOfNonMainDatabasesWithoutActivity
		{
			get
			{
				return GetItem("SkipBackupOfNonMainDatabasesWithoutActivity", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"SkipBackupOfNonMainDatabasesWithoutActivity",
						Categories.System_Database_Backup,
						ResString.GetMultilingualString("AB79F086-F956-444B-BA67-2E4C6AE96086", "Skip backup of inactive databases except main"),
						ResString.GetMultilingualString("16F0D2BC-C4FE-45BA-8ACD-4AFA9F042F44", "If set to 'Yes' the application backup will skip databases except, for the main database, that have had no activity since the last backup."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
					return result;
				});
			}
		}

		internal BooleanRegistryItem UseDbBackupCompression
		{
			get
			{
				return GetItem("UseDbBackupCompression", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"UseDbBackupCompression",
						Categories.System_Database_Backup,
						ResString.GetMultilingualString("80DA8474-5455-4822-B69C-46A0739763AB", "Use Database Backup Compression"),
						ResString.GetMultilingualString("6A0DB241-59BF-4942-9C1C-A162E459F785", "If Yes, database backups will be taken with compression. This reduces disk I/O, network traffic and storage space, but the backup process is more CPU intensive in the database server."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
					return result;
				});
			}
		}

		internal BooleanRegistryItem BackupIncludesCheckSum
		{
			get
			{
				return GetItem("BackupIncludesCheckSum", delegate
				{
					return new BooleanRegistryItem(
						"BackupIncludesCheckSum",
						Categories.System_Database_Backup,
						ResString.GetMultilingualString("80DA8474-5455-1234-B69C-46A0739763AB", "Backups include 'with checksum'"),
						ResString.GetMultilingualString("6A0DB241-59BF-5678-9C1C-A162E459F785", "When backing up a database the backup will include a checksum."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		internal IntRegistryItem DaysOfLogBackupToKeep
		{
			get
			{
				return GetItem("DaysOfLogBackupToKeep", delegate
				{
					return new IntRegistryItem(
						"DaysOfLogBackupToKeep",
						Categories.System_Database_Backup,
						ResString.GetMultilingualString("BF411F1C-5D95-4369-9F73-CDA3C1F11541", "Days of log backup to keep"),
						ResString.GetMultilingualString("A3E3B5F6-1587-4173-9B88-9947C843E8AD", "The number of days of log backup to keep. Log backups older than this number of days will be deleted by the Database Log Backup Service task, except where a more recent full backup doesn't exist."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						10);
				});
			}
		}

		internal BooleanRegistryItem SkipLogShrinkingAndBackupInUpgrade
		{
			get
			{
				return GetItem("SkipLogShrinkingAndBackupInUpgrade", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"SkipLogShrinkingAndBackupInUpgrade",
						Categories.System_Database_Backup,
						ResString.GetMultilingualString("B9D3264B-7C76-464C-8DD7-324B1BFA9D81", "Skip both backup and shrink log operation in upgrade"),
						ResString.GetMultilingualString("D443D01C-CFB2-4A0F-A3AE-A6D30E15849B", "If set to 'Yes', the application upgrade will skip both the backup and the shrink of the log file."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false);
					return result;
				});
			}
		}

		public IntRegistryItem LogDeltaSizeInKbToBackupDirectly
		{
			get
			{
				return GetItem("LogDeltaSizeInKbToBackupDirectly", delegate
				{
					return new IntRegistryItem(
						"LogDeltaSizeInKbToBackupDirectly",
						Categories.System_Database_Backup,
						(NoResString)"Log delta size in KB to backup directly", // hidden registry
						(NoResString)"If the log size since last log backup is greater than this number, the backup will be consdered needed.", // hidden registry
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						200);
				});
			}
		}

		#endregion // Backup

		#region DBCC

		internal IntRegistryItem DbccInitialTimeout
		{
			get
			{
				return GetItem("DbccInitialTimeout", delegate
				{
					return new IntRegistryItem(
						name: "DbccInitialTimeout",
						category: Categories.System_Database_DBCC,
						caption: ResString.GetMultilingualString("0E11F140-5A78-42F7-9768-652E8FC8AB6C", "Initial timeout"),
						hint: ResString.GetMultilingualString("FE6FC67D-B921-4041-BDCA-B53D0EBF99C7", "Total timeout in milliseconds for all DBCCs."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 7200000,
						minValue: 300000,
						maxValue: int.MaxValue
						);
				});
			}
		}

		internal IntRegistryItem DbccUserWaitThreshold
		{
			get
			{
				return GetItem("DbccUserWaitThreshold", delegate
				{
					return new IntRegistryItem(
						name: "DbccUserWaitThreshold",
						category: Categories.System_Database_DBCC,
						caption: ResString.GetMultilingualString("E9D9022F-C94E-435C-9564-3DDDC2E191FE", "User wait threshold"),
						hint: ResString.GetMultilingualString("2B6DC946-274B-4869-8577-25A49114275A", "The number of milliseconds the DBCC CHECKTABLE is allowed to block any user."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 5000,
						minValue: 0,
						maxValue: int.MaxValue
						);
				});
			}
		}

		internal IntRegistryItem DbccServiceTaskWaitThreshold
		{
			get
			{
				return GetItem("DbccServiceTaskWaitThreshold", delegate
				{
					return new IntRegistryItem(
						name: "DbccServiceTaskWaitThreshold",
						category: Categories.System_Database_DBCC,
						caption: ResString.GetMultilingualString("501F6224-F209-4987-A373-0E21B317411F", "Service task wait threshold"),
						hint: ResString.GetMultilingualString("A0AC7F57-EFE5-4902-ACDD-7B141E0CF56D", "The number of milliseconds the DBCC CHECKTABLE is allowed to block any Service Task."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 240000,
						minValue: 0,
						maxValue: int.MaxValue
						);
				});
			}
		}

		internal IntRegistryItem DbccPollingInterval
		{
			get
			{
				return GetItem("DbccPollingInterval", delegate
				{
					return new IntRegistryItem(
						name: "DbccPollingInterval",
						category: Categories.System_Database_DBCC,
						caption: ResString.GetMultilingualString("C14AA5F3-C16E-44B5-9771-0299A9E25F5F", "Polling interval"),
						hint: ResString.GetMultilingualString("E4CE0ABE-D5B4-4A6B-B430-AA3AD3558397", "Polling interval in milliseconds for periodically checking blocked connections during running DBCC CHECKTABLE with TABLOCK option."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 3500,
						minValue: 500,
						maxValue: 300000
						);
				});
			}
		}

		internal BooleanRegistryItem DbccRunCheckdbWithPhysicalOnly
		{
			get
			{
				return GetItem("DbccRunCheckdbWithPhysicalOnly", delegate
				{
					return new BooleanRegistryItem(
						name: "DbccRunCheckdbWithPhysicalOnly",
						category: Categories.System_Database_DBCC,
						caption: ResString.GetMultilingualString("03A80B51-B158-47C0-9BE9-DA27ED1D9F2B", "Run CHECKDB with PHYSICAL_ONLY"),
						hint: ResString.GetMultilingualString("44609163-E4BB-49E9-8ADE-4231F1267238", "Run \"DBCC CHECKDB WITH PHYSICAL_ONLY\" statement on every database (within client database set) on primary replica only."),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: false
						);
				});
			}
		}

		internal BooleanRegistryItem DbccRunSecondariesInParallel
		{
			get
			{
				return GetItem("DbccRunSecondariesInParallel", delegate
				{
					return new BooleanRegistryItem(
						name: "DbccRunSecondariesInParallel",
						category: Categories.System_Database_DBCC,
						caption: ResString.GetMultilingualString("0FDDCCFF-3CF0-4D38-A665-F3D4EDA0D9A7", "Run secondaries in parallel"),
						hint: ResString.GetMultilingualString("8BC2F018-BAA8-4B88-B7C4-7C7AF6CF1C50", "Controls whether database consistency checks on all secondary replicas should be run simultaneously."),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: false
						);
				});
			}
		}

		internal BooleanRegistryItem DbccRunSecondariesByParts
		{
			get
			{
				return GetItem("DbccRunSecondariesByParts", delegate
				{
					return new BooleanRegistryItem(
						name: "DbccRunSecondariesByParts",
						category: Categories.System_Database_DBCC,
						caption: ResString.GetMultilingualString("395428D5-376F-4F24-A12B-F7BA27D0D7C0", "Run secondaries by parts"),
						hint: ResString.GetMultilingualString("E8F5682A-14D9-4A25-8E01-A53B472605E5", "Controls whether database consistency checks on all secondary replicas should be run by parts or in one go (using DBCC CHECKDB statement)."),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: false
						);
				});
			}
		}

		#endregion // DBCC

		#region ISU

		internal IntRegistryItem ISU_Rebuild_ThresholdPercentage
		{
			get
			{
				return GetItem("ISU_Rebuild_ThresholdPercentage", delegate
				{
					return new IntRegistryItem(
						name: "ISU_Rebuild_ThresholdPercentage",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("9644933A-E1D4-401A-A00F-D13302EE848D", "Rebuild Threshold"),
						hint: ResString.GetMultilingualString("A715AD24-AAC9-4D83-B175-CCC69CDAF935", "Rebuild Threshold exclusive in percentage."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 40,
						minValue: 0,
						maxValue: 100
						);
				});
			}
		}

		internal IntRegistryItem ISU_Rebuild_MaxDopPercentage
		{
			get
			{
				return GetItem("ISU_Rebuild_MaxDopPercentage", delegate
				{
					return new IntRegistryItem(
						name: "ISU_Rebuild_MaxDopPercentage",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("74A550F2-430D-4111-9445-FEB91670A19E", "Rebuild MAXDOP"),
						hint: ResString.GetMultilingualString("FE327A4E-EA24-4979-8AC7-113F06237166", "Specifies percentage of all available processors for \"max degree of parallelism\" option used in a parallel plan execution during rebuild"),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 25,
						minValue: 1,
						maxValue: 100
						);
				});
			}
		}

		internal BooleanRegistryItem ISU_Rebuild_Online
		{
			get
			{
				return GetItem("ISU_Rebuild_Online", delegate
				{
					return new BooleanRegistryItem(
						name: "ISU_Rebuild_Online",
						category: Categories.System_Database_ISU,
						caption: (NoResString)"Rebuild on-line", // support only information
						hint: (NoResString)"Specifies whether an index should be rebuilt on-line or off-line.", // support only information
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: true
						);
				});
			}
		}

		internal BooleanRegistryItem ISU_Rebuild_UseObserver
		{
			get
			{
				return GetItem("ISU_Rebuild_UseObserver", delegate
				{
					return new BooleanRegistryItem(
						name: "ISU_Rebuild_UseObserver",
						category: Categories.System_Database_ISU,
						caption: (NoResString)"Rebuild using observer", // support only information
						hint: (NoResString)"Specifies whether an observer can be used during rebuild", // support only information
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: false
						);
				});
			}
		}

		internal IntRegistryItem ISU_Rebuild_MaxWaitInMinutes
		{
			get
			{
				return GetItem("ISU_Rebuild_MaxWaitInMinutes", delegate
				{
					return new IntRegistryItem(
						name: "ISU_Rebuild_MaxWaitInMinutes",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("6FB631E0-6979-472F-AE87-4CC451970B74", "Rebuild Max Wait in minutes"),
						hint: ResString.GetMultilingualString("EF85804A-520B-42CC-982D-30B58DBAE2F6", "The wait time (in minutes) that the on-line index rebuild locks will wait with low priority when executing the rebuild. If the operation is blocked for the time specified, one of the \"Rebuild Abort After Wait\" actions will be executed"),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 1,
						minValue: 0,
						maxValue: 120
						);
				});
			}
		}

		internal CodePairRegistryItem ISU_Rebuild_AbortAfterWait
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() => new OnlineIndexRebuildLowPriorityAbortAfterWaitList());

				return GetItem("ISU_Rebuild_AbortAfterWait", delegate
				{
					return new CodePairRegistryItem(
						name: "ISU_Rebuild_AbortAfterWait",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("675973CC-3A43-449B-AC72-F7C706F6FF38", "Rebuild Abort After Wait"),
						hint: ResString.GetMultilingualString("D2184585-D992-492B-9E5F-E12EA6C8FD38", "The action which will be executed if the on-line rebuild operation is blocked for the time specified in \"Rebuild Max Wait In Minutes\""),
						lookUpList: listProvider,
						allowBlank: false,
						validate: false,
						editorInfo: new ComboBoxRegistryEditorInfo(listProvider),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Blockers,
						useDefaultDefaultValue: false
						);
				});
			}
		}

		internal IntRegistryItem ISU_Reorganize_ThresholdPercentage
		{
			get
			{
				return GetItem("ISU_Reorganize_ThresholdPercentage", delegate
				{
					return new IntRegistryItem(
						name: "ISU_Reorganize_ThresholdPercentage",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("2F3E63F9-ACBE-490E-9960-824B255A498B", "Reorganize Threshold"),
						hint: ResString.GetMultilingualString("3ABE2AEA-3F75-47DD-9F95-665AEED3984D", "Reorganize Threshold exclusive in percentage."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 10,
						minValue: 0,
						maxValue: 100
						);
				});
			}
		}

		internal IntRegistryItem ISU_Reorganize_MaxConcurrentProcessesPercentage
		{
			get
			{
				return GetItem("ISU_Reorganize_MaxConcurrentProcessesPercentage", delegate
				{
					return new IntRegistryItem(
						name: "ISU_Reorganize_MaxConcurrentProcessesPercentage",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("6906CDA3-2287-4D32-9555-83822554D071", "Reorganize Max Concurrent Processes"),
						hint: ResString.GetMultilingualString("D3439CB0-7FCB-4441-8322-21665B8AF7A3", "Reorganize Max Concurrent Processes in percentage."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 25,
						minValue: 1,
						maxValue: 100
						);
				});
			}
		}

		internal IntRegistryItem ISU_Reorganize_PeriodInDays
		{
			get
			{
				return GetItem("ISU_Reorganize_PeriodInDays", delegate
				{
					return new IntRegistryItem(
						name: "ISU_Reorganize_PeriodInDays",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("1A8B905A-4618-4A6E-8547-2A238C76491A", "Reorganize Period in days"),
						hint: ResString.GetMultilingualString("82605DC6-BB1E-4C19-A647-6B67549ED68F", "Reorganize Period in days."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 7,
						minValue: 0,
						maxValue: int.MaxValue
						);
				});
			}
		}

		internal IntRegistryItem ISU_MinimumIndexPageCount
		{
			get
			{
				return GetItem("ISU_MinimumIndexPageCount", delegate
				{
					return new IntRegistryItem(
						name: "ISU_MinimumIndexPageCount",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("1EA908EB-C3B2-4A13-B398-BA0FA3CF3A58", "Minimum Index Page Count"),
						hint: ResString.GetMultilingualString("097E1D66-3794-46EC-8B43-572F0DC98F46", "Minimum Index Page Count."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 50,
						minValue: 1,
						maxValue: int.MaxValue
						);
				});
			}
		}

		internal IntRegistryItem ISU_MaxBacklogWaitTime_InMinutes
		{
			get
			{
				return GetItem(nameof(ISU_MaxBacklogWaitTime_InMinutes), delegate
				{
					return new IntRegistryItem(
						name: nameof(ISU_MaxBacklogWaitTime_InMinutes),
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("EC937616-EF9C-487D-83CC-7FBE25C3001B", "Maximum Backlog Wait Time (minutes)"),
						hint: ResString.GetMultilingualString("6010E1A5-1A3A-4DBB-877A-D07CC23D370B", "Maximum time to wait for backlog to clear before terminating an ISU service task"),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 60,
						minValue: 1,
						maxValue: 600
						);
				});
			}
		}

		internal IntRegistryItem ISU_EmptyStatisticsRowsThreshold
		{
			get
			{
				return GetItem(nameof(ISU_EmptyStatisticsRowsThreshold), delegate
				{
					return new IntRegistryItem(
						name: nameof(ISU_EmptyStatisticsRowsThreshold),
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("8CDD65BE-66A5-4CD2-92FF-B5DDE88B3321", "Filtered Indexes automatic statistics threshold (rows)"),
						hint: ResString.GetMultilingualString("08C308A0-5306-470C-9CE3-A5BB451C2D64", "Auto Statistics Update is turned OFF/ON for filtered indexes according to their SMALL/BIG volume (in number of rows)"),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 1000,
						minValue: 1,
						maxValue: 1000000
					);
				});
			}
		}

		internal BooleanRegistryItem ISU_DisableAutoStatisticsDuringUpgrade
		{
			get
			{
				return GetItem("ISU_DisableAutoStatisticsDuringUpgrade", delegate
				{
					return new BooleanRegistryItem(
						name: "ISU_DisableAutoStatisticsDuringUpgrade",
						category: Categories.System_Database_ISU,
						caption: ResString.GetMultilingualString("8f073ab7-1f77-4ca6-a0e9-90bba26fc348", "Disable auto statistics during database upgrade"),
						hint: ResString.GetMultilingualString("967c0a66-ed84-4de0-a8a6-9905cf099875", "Specifies if auto statistics during database upgrade should be disabled."),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: true
					);
				});
			}
		}

		#endregion // ISU

		#region Use modern Sql security system

		public BooleanRegistryItem UseModernSqlSecuritySystem
		{
			get
			{
				return GetItem("UseModernSqlSecuritySystem", delegate
				{
					return new BooleanRegistryItem(
						name: "UseModernSqlSecuritySystem",
						category: Categories.System_Database,
						caption: ResString.GetMultilingualString("257C054D-1E96-4368-93E5-2E46EF1CD7B4", "Use modern SQL security system"),
						hint: ResString.GetMultilingualString("5E52372D-0406-4B55-BF58-DCB3E9A6FB8B", "Use modern SQL security system"),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
						defaultValue: Globals.IsTest
						);
				});
			}
		}

		#endregion Use modern Sql security system

		#region Always On Replica Cache

		public BooleanRegistryItem UseAlwaysOnReplicaCache
		{
			get
			{
				return GetItem("UseAlwaysOnReplicaCache", delegate
				{
					var result = new BooleanRegistryItem(
					name: "UseAlwaysOnReplicaCache",
					category: Categories.System_Database_AlwaysOn,
					caption: ResString.GetMultilingualString("DB789C77-9D95-43C6-A251-A43CD19C6588", "Use Always On Replica Cache"),
					hint: ResString.GetMultilingualString("87739076-9678-43A1-89F7-FCABD6C64B49", "Use cache for Always On info to reduce WSFC hit."),
					storage: RegistryStorageFlags.System,
					options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.PreserveTestValue,
					defaultValue: true
					);

					return result;
				});
			}
		}

		public StringArrayRegistryItem AlwaysOnReplicaCachedInfos
		{
			get
			{
				return GetItem("AlwaysOnReplicaCachedInfo", delegate
				{
					var result = new StringArrayRegistryItem(
						name: "AlwaysOnReplicaCachedInfo",
						category: Categories.System_Database_AlwaysOn,
						caption: ResString.GetMultilingualString("934F67FA-0063-42FA-BFA9-4309A18BF410", "Always On Replica Cached Info"),
						hint: ResString.GetMultilingualString("0FBAB085-1E97-4310-8806-B16CC33B2D39", "Cached Always On info for all replicas."),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		public StringArrayRegistryItem AvailabilityGroupInfo
		{
			get
			{
				return GetItem("AvailabilityGroupInfo", delegate
				{
					var result = new StringArrayRegistryItem(
						"AvailabilityGroupInfo",
						Categories.System_Database_AlwaysOn,
						ResString.GetMultilingualString("1310E9E2-D786-4BC0-B71F-742E106512F9", "Availability Group Info"),
						ResString.GetMultilingualString("3BC9CD66-02DF-4DF4-B09C-487B85A69239", "Availability group info including group name, group Id, replica Id and listener IP addresses if available are shown below in each row respectively."),
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController) | RegistryOptions.IsReadOnly);
					result.DataType.MaximumLength = 128;
					return result;
				});
			}
		}

		#endregion //Always On Replica Cache

		internal IntRegistryItem PurgeDataTimeoutLimit
		{
			get
			{
				return GetItem(
					"PurgeDataTimeoutLimit",
					() => new IntRegistryItem(
						"PurgeDataTimeoutLimit",
						Categories.System_Database_UserOptions,
						ResString.GetMultilingualString("f5739d27-19ee-44b7-b847-d827a8793a0f", "Purge Data Timeout Limit"),
						ResString.GetMultilingualString("524b76bd-8a94-48e8-a794-5a230a6c6497", "Timeout limit for Purge Data operation in minutes. Minimum value is 5 minutes, maximum is 1440 minutes (equals to 24 hours)."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						30,
						5,
						1440)
					);
			}
		}

		internal CodePairRegistryItem PurgeDataRunStatusFlag
		{
			get
			{
				return GetItem("PurgeDataRunStatusFlag", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair("RUN", (NoResString)nameof(Constants.PurgeDataRunStatusFlagCodes.Running));
						list.AddPair("CMP", (NoResString)nameof(Constants.PurgeDataRunStatusFlagCodes.Completed));
						list.AddPair("ERR", (NoResString)nameof(Constants.PurgeDataRunStatusFlagCodes.Error));
						return list;
					});
					return new CodePairRegistryItem(name: "PurgeDataRunStatusFlag",
						category: Categories.System_Database_UserOptions,
						null,
						null,
						lookUpListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						"CMP");
				});
			}
		}

		internal IntRegistryItem GhostRecordCleanerLockTimeoutSeconds
		{
			get
			{
				return GetItem(
					"GhostRecordCleanerLockTimeoutSeconds",
					() => new IntRegistryItem(
						name: "GhostRecordCleanerLockTimeoutSeconds",
						category: Categories.System_Database_GRC,
						caption: ResString.GetMultilingualString("961dd4d5-7804-4f2b-adae-49006ab82c3e", "Ghost Record Clean-up Fetch Timeout Seconds"),
						hint: ResString.GetMultilingualString("04007573-3ece-4bd0-b40b-592767556f05", "The lock timeout in seconds for querying number of ghost records in GRC service task. Minimum value is 3 seconds, maximum is 60 seconds."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 10,
						minValue: 3,
						maxValue: 60)
					);
			}
		}

		public IntRegistryItem GhostRecordCleanUpRebuildWaitMinutes
		{
			get
			{
				return GetItem(
					"GhostRecordCleanUpRebuildWaitMinutes",
					() => new IntRegistryItem(
						name: "GhostRecordCleanUpRebuildWaitMinutes",
						category: Categories.System_Database_GRC,
						caption: ResString.GetMultilingualString("fdfbe5e0-fdc3-4b3d-87d2-5ab66f720fe4", "Ghost Record Clean-up Rebuild Index Wait Minutes"),
						hint: ResString.GetMultilingualString("dc0b1cc1-b595-44d6-b774-d3ffd8ea1764", "The low priority lock timeout in minutes for the rebuild index statement waiting for blockers on the respective table in GRC service task. Minimum value is 1 minute, maximum is 60 minutes."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 1,
						minValue: 1,
						maxValue: 60)
					);
			}
		}

		public IntRegistryItem GhostRecordCleanUpThreshold
		{
			get
			{
				return GetItem(
					"GhostRecordCleanUpThreshold",
					() => new IntRegistryItem(
						name: "GhostRecordCleanUpThreshold",
						category: Categories.System_Database_GRC,
						caption: ResString.GetMultilingualString("8fde6e93-3aab-4cb6-aebf-f168ed189445", "Ghost Record Clean-up Threshold"),
						hint: ResString.GetMultilingualString("68aeb0cf-3ff1-4af7-9dea-065d1e4fac1f", "Specifies the threshold for the number of ghost records at which the GRC service task will rebuild an index."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 5000,
						minValue: 0,
						maxValue: int.MaxValue)
				);
			}
		}

		public IntRegistryItem GhostRecordProcessingThreadInterval
		{
			get
			{
				return GetItem(
					"GhostRecordProcessingThreadInterval",
					() => new IntRegistryItem(
						name: "GhostRecordProcessingThreadInterval",
						category: Categories.System_Database_GRC,
						caption: (NoResString)"Ghost Record Clean-up Processing Thread Interval",
						hint: (NoResString)"Seconds interval for checking whether to add another thread to help clean-up ghost records",
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsHidden,
						defaultValue: 5,
						minValue: 1,
						maxValue: 60)
					);
			}
		}

		public IntRegistryItem GhostRecordMaxProcessingThreadCount
		{
			get
			{
				return GetItem(
					"GhostRecordMaxProcessingThreadCount",
					() => new IntRegistryItem(
						name: "GhostRecordMaxProcessingThreadCount",
						category: Categories.System_Database_GRC,
						caption: (NoResString)"Ghost Record Clean-up Max Thread Count",
						hint: (NoResString)"Specifies the max amount of threads that can be actioned to clean up ghost records",
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsHidden,
						defaultValue: 10,
						minValue: 1,
						maxValue: 20)
					);
			}
		}

		public BooleanRegistryItem GhostRecordCleanupOnlineRebuild
		{
			get
			{
				return GetItem(
					"GhostRecordCleanupOnlineRebuild",
					() => new BooleanRegistryItem(
						name: "GhostRecordCleanupOnlineRebuild",
						category: Categories.System_Database_GRC,
						caption: ResString.GetMultilingualString("66F043DC-A025-42D7-AC91-B63D552ADB7D", "Ghost Record Clean-up Online rebuild"),
						hint: ResString.GetMultilingualString("75C3A87B-DA7B-4419-91A7-634CCCCB3584", "Ghost Record Clean-up uses online rebuild as default. The offline rebuild is faster and more likely to succeed, minimizing the chances of a lock request time-out. Please disable the online rebuild when the GRC is consistently unable to rebuild indexes or when you are experiencing lock request time-outs"),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: true)
				);
			}
		}

		public IntRegistryItem GhostRecordCleanupKillBlockersThreshold
		{
			get
			{
				return GetItem(
					"GhostRecordCleanupKillBlockersThreshold",
					() => new IntRegistryItem(
						name: "GhostRecordCleanupKillBlockersThreshold",
						category: Categories.System_Database_GRC,
						caption: ResString.GetMultilingualString("A299849C-CABA-48DF-A6CE-5D78D9558B62", "Ghost Record Clean-up Kill Blockers Threshold"),
						hint: ResString.GetMultilingualString("D56913E0-7660-414D-A19F-60B19F214A50", "To ensure the Ghost Record Clean-up service succeeds, blocking statements will be terminated. This registry setting determines when termination begins, defined as a percentage of the lock request timeout. Setting this value to -1 will disable the automatic killing of blockers."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: -1,
						minValue: -1,
						maxValue: 100)
				);
			}
		}

		public IntRegistryItem GhostRecordCleanupCommandTimeOutThreshold
		{
			get
			{
				return GetItem(
					"GhostRecordCleanupCommandTimeOutThreshold",
					() => new IntRegistryItem(
						name: "GhostRecordCleanupCommandTimeOutThreshold",
						category: Categories.System_Database_GRC,
						caption: ResString.GetMultilingualString("52EA71A4-350E-4A57-BE71-FAC34E83B5C3", "Ghost Record Clean-up Command Time-Out"),
						hint: ResString.GetMultilingualString("CDC81C93-6298-42FA-B251-6B61DA13920B", "To prevent the Ghost Record Clean-up service from causing lock request time-outs, transactions will be timed out if they run too long. This registry setting defines when the GRC transaction is terminated, based on a percentage of the lock request timeout. Setting this value to 0 will run long-running REBUILD commands with infinite timeout."),
						editorInfo: new NumericRegistryEditorInfo(0),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 0,
						minValue: 0,
						maxValue: 1000)
				);
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal BooleanRegistryItem OnlySupportsSqlServerEnterpriseEdition
		{
			get
			{
				return GetItem("OnlySupportsSqlServerEnterpriseEdition", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"OnlySupportsSqlServerEnterpriseEdition",
						Categories.System_Database_UserOptions,
						(NoResString)"Supports only MS SQL Server Enterprise Edition",
						(NoResString)"If Yes, the application can only run on Enterprise edition of MS SQL Server.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.IsReadOnly,
						true);
					return result;
				});
			}
		}

		#endregion

		internal IntRegistryItem ZSqlSaverRowsToPostPerSqlStatement
		{
			get
			{
				return GetItem(
					"ZSqlSaverRowsToPostPerSqlStatement",
					() => new IntRegistryItem(
						"ZSqlSaverRowsToPostPerSqlStatement",
						Categories.System_Database_UserOptions,
						ResString.GetMultilingualString("c7d65cfd-d4e2-48a2-83cc-9fe412a2d12d", "Rows to Post per Database Statement"),
						ResString.GetMultilingualString("48ebc13e-ac85-4a62-a8be-53ea72241bdb", "Sets the maximum batch size of rows to save to the database in each statement."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						500,
						1,
						1000)
					);
			}
		}

		#endregion

		public BooleanRegistryItem ParameterizeInsertAndUpdateStatements
		{
			get
			{
				return GetItem("ParameterizeInsertAndUpdateStatements", delegate
				{
					return new BooleanRegistryItem("ParameterizeInsertAndUpdateStatements"
						, Categories.Optimization
						, ResString.GetMultilingualString("FE09031A-3E70-40C1-B1CA-4C1E13941AC5", "Parameterize Insert/Update statements")
						, ResString.GetMultilingualString("405A6302-EFDA-45CF-AA1F-6A67E6610489", "Parameterize Insert/Update statements for each Save operation to reduce recompilations of execution plans.")
						, RegistryStorageFlags.System
						, EntityFrameworkRegistryDefaults.ParameterizeInsertAndUpdateStatements
						);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public IRegistryItem FieldsToLiteralize
		{
			get
			{
				return GetItem("FieldsToLiteralize", delegate
				{
					return new StringRegistryItem("FieldsToLiteralize"
						, category: Categories.Optimization
						, caption: (NoResString)"Fields to literalize"
						, hint: (NoResString)"Fields to literalize"
						, storage: RegistryStorageFlags.System
						, options: RegistryOptions.IsOnlyForSupport
						, defaultValue: EntityFrameworkRegistryDefaults.FieldsToLiteralize
						);
				});
			}
		}

		public StringRegistryItem TVPRule
		{
			get
			{
				return GetItem("TVPRule", delegate
				{
					return new StringRegistryItem("TVPRule"
						, category: Categories.Optimization
						, caption: (NoResString)"Convert sets of values to TVP query instead of IN syntax"
						, hint: (NoResString)"First value is minimum predicates to convert, then each each value represents the bucketization to apply to generate multiple SQL plans."
						, storage: RegistryStorageFlags.System
						, options: RegistryOptions.IsOnlyForSupport
						, defaultValue: EntityFrameworkRegistryDefaults.TVPRule
						);
				});
			}
		}

		public IntRegistryItem PasteDebounceMs
		{
			get
			{
				return GetItem("PasteDebounceMs", delegate
				{
					return new IntRegistryItem(
							"PasteDebounceMs",
							Categories.Optimization,
							caption: (NoResString)"Paste Debounce (ms)",
							hint: (NoResString)"How many milliseconds must elapse between consecutive paste attempts for ZTextBox and ZRichTextBox. Workaround for a long standing double paste RDP bug. Set to 0 to disable this feature.",
							null,
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							25, 0, 1000
							);
				});
			}
		}

		#endregion // SuppressResourceStringsCheckRegion

		internal IntRegistryItem MaximumParametersPerFetchHint
		{
			get
			{
				return GetItem("MaximumParametersPerFetchHint", delegate
				{
					return new IntRegistryItem(
							"MaximumParametersPerFetchHint",
							Categories.Optimization,
							ResString.GetMultilingualString("23cb25a1-3bc5-4a4d-9552-bf9278b677a7", "Maximum Parameters Per Fetch Hint"),
							ResString.GetMultilingualString("6f71648a-ce23-4d6b-9a27-e884fce32535", "Controls the number of parameters per fetch hint. Default aligns with SQL maximum that avoids table scans when multiple indexes are utilized in a query."),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							EntityFrameworkRegistryDefaults.MaximumParametersPerFetchHint, 1, 1024
							);
				});
			}
		}

		/// There's a bug that makes query statistics invalid ATM.
		/// To try to work around the bug, we can force FORCESEEK on certain simple queries (for example, single column foreign key query).
		internal BooleanRegistryItem DefaultToForceSeek
		{
			get
			{
				return GetItem("DefaultToForceSeek", delegate
				{
					return new BooleanRegistryItem(
							"DefaultToForceSeek",
							Categories.Optimization,
							(NoResString)"Default to Force Seek", // Support only Registry Item
							(NoResString)"Use Force Seek flag for certain kinds of simple queries (for example, single column foreign key query).", // Support only Registry Item
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							EntityFrameworkRegistryDefaults.DefaultToForceSeek
							);
				});
			}
		}

		internal BooleanRegistryItem ApplyIsNotNullToJoinOnFK
		{
			get
			{
				return GetItem("ApplyIsNotNullToJoinOnFK", delegate
				{
					return new BooleanRegistryItem(
						name: "ApplyIsNotNullToJoinOnFK",
						category: Categories.Optimization,
						caption: (NoResString)"Apply IS NOT NULL to join on foreign keys", // Support only Registry Item
						hint: (NoResString)"Joining to nullable foreign keys may be faster when IS NOT NULL is present.", // Support only Registry Item
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: EntityFrameworkRegistryDefaults.ApplyIsNotNullToJoinOnFK
						);
				});
			}
		}

		/// Performance problems were found when submitting too many different types of queries
		/// By separating, they become index seeks in some cases, instead of table scans
		internal BooleanRegistryItem ConcatenateMultipleFetchHintTypes
		{
			get
			{
				return GetItem("ConcatenateMultipleFetchHintTypes", delegate
				{
					return new BooleanRegistryItem(
							"ConcatenateMultipleFetchHintTypes",
							Categories.Optimization,
							ResString.GetMultilingualString("23cb25a1-3bc5-4a4d-9552-bf9278b777a7", "Concatenate Multiple Fetch Hint Types"),
							ResString.GetMultilingualString("6f71648a-ce23-4d6b-9a27-e884fce42535", "Indicates whether the application will join multiple fetch hints using an 'or' statement."),
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							EntityFrameworkRegistryDefaults.ConcatenateMultipleFetchHintTypes
							);
				});
			}
		}

		#region RegFonts

		public IRegistryItem SystemFontRegItem
		{
			get
			{
				return GetItem("SystemFont", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"SystemFont",
						Categories.System_UI,
						(NoResString)"Font Used for the System ", // Support only Registry Item
						(NoResString)"The text for the System will be displayed using the selected font.", // Support only Registry Item
						FontList,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						OFont.DefaultFontName
					);

					return result;
				});
			}
		}

		public IRegistryItem NavigationMenuFontRegItem
		{
			get
			{
				return GetItem("NavigationMenuFont", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"NavigationMenuFont",
						Categories.System_UI,
						(NoResString)"Font Used for the Navigation Menu", // Support only Registry Item
						(NoResString)"The text in the Navigation Menu will be displayed using the selected font.", // Support only Registry Item
						FontList,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						OFont.DefaultNavigationMenuFontName
					);

					return result;
				});
			}
		}

		public IRegistryItem NewsAnnouncementFontRegItem
		{
			get
			{
				return GetItem("NewsAnnouncementFont", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"NewsAnnouncementFont",
						Categories.System_UI,
						(NoResString)"Font Used for the News & Announcements", // Support only Registry Item
						(NoResString)"The text in the News & Announcements will be displayed using the selected font.", // Support only Registry Item
						FontList,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						OFont.DefaultNewsAnnouncementFontName
					);

					return result;
				});
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		internal ICodeDescriptionPairListProvider FontList
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					CodeDescriptionPairList fonts = new CodeDescriptionPairList();

					foreach (FontFamily font in System.Drawing.FontFamily.Families)
					{
						fonts.Add(new CodeDescriptionPair(font.Name, (NoResString)$"{font.Name} Font")); // Font names are same globally
					}

#if DEBUG
					fontListIsCreated = true;
#endif
					return fonts;
				});
			}
		}

#if DEBUG
		internal bool fontListIsCreated;
#endif

		#endregion

		#region GraphicRenderingEngine

		public IRegistryItem GraphicRenderingEngineRegItem
		{
			get
			{
				return GetItem("GraphicRenderingEngine", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"GraphicRenderingEngine",
						Categories.System_UI,
						(NoResString)"Graphic Rendering Engine", // Support only Registry Item
						(NoResString)@"This registry setting determines the graphic rendering engine.

GDI+ has advantages with anti-aliased output and support of alpha channel which makes it useful to create high-quality imagery.
However for most instances GDI works better, performing real-time rendering and offering higher performance with imagery.", // Support only Registry Item
						TextRendererTypeList,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						default(TextRendererType).ToString()
					);

					return result;
				});
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		ICodeDescriptionPairListProvider TextRendererTypeList
		{
			get
			{
				return new CodeDescriptionPairListProvider(() =>
				{
					CodeDescriptionPairList rendererType = new CodeDescriptionPairList();

					foreach (TextRendererType renderer in Enum.GetValues(typeof(TextRendererType)))
					{
						rendererType.Add(new CodeDescriptionPair(renderer.ToString(), (NoResString)$"{renderer.ToString()} Rendering Engine")); // Font names are same globally
					}

					return rendererType;
				});
			}
		}

		#endregion

		#region ForceDialogRenderingOverTS

		public BooleanRegistryItem ForceDialogRenderingOverTS
		{
			get
			{
				return GetItem(ForceDialogRenderingOverTSRegistryName, () =>
				{
					return new BooleanRegistryItem(
						ForceDialogRenderingOverTSRegistryName,
						Categories.System_UI,
						ResString.GetMultilingualString("79db56d7-836c-4d5e-b3a2-3ac872890a56", "Force Dialogs Full Rendering When Run Over Terminal Service"),
						ResString.GetMultilingualString("1a4cf398-428d-4bec-93c9-38bf4988f37a",
@"When application runs over Terminal Service (e.g. Remote Desktop), some dialog windows are not rendered properly when open.

Enable this option to make following dialogs to perform necessary actions to ensure they are fully rendered. This may cause quick flickering of the dialog window when it opens.
Following dialog windows currently support this option: Document Delivery."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						true
						);
				});
			}
		}

		const string ForceDialogRenderingOverTSRegistryName = "ForceDialogRenderingOverTS";

		#endregion

		public DecimalRegistryItem TextOnlyNoteFontSize
		{
			get
			{
				return GetItem("TextOnlyNoteFontSize", delegate
				{
					return new DecimalRegistryItem(
						"TextOnlyNoteFontSize",
						Categories.System_UI,
						ResString.GetMultilingualString("F94445AB-0873-468E-8B45-15C2D0FCDB6B", "Text-Only Note Font Size"),
						ResString.GetMultilingualString("EDA9B411-77F3-43C3-8FAE-0E393190B902", "The font size used for a text-only note's body when being edited in the application."),
						new NumericRegistryEditorInfo(2),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						10.25m,
						6,
						40);
				});
			}
		}

		public BooleanRegistryItem CalendarIntegration
		{
			get
			{
				return GetItem("CalendarIntegration", delegate
				{
					return new BooleanRegistryItem(
						"CalendarIntegration",
						Categories.System_Calendar,
						ResString.GetMultilingualString("975e5d36-1b32-4e30-9773-59fcd87ef657", "Calendar Integration"),
						ResString.GetMultilingualString("212a4f41-a158-4d4e-94f0-dcb62eb31b9a", "Specifies whether the application will integrate with Lotus Notes / Microsoft Outlook Calendar system for reminders."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		internal BooleanRegistryItem CalendarInvitationSolutionForOrganisers
		{
			get
			{
				return GetItem("CalendarInvitationSolutionForLotusNotesUsers", delegate
				{
					return new BooleanRegistryItem(
						"CalendarInvitationSolutionForLotusNotesUsers",
						Categories.System_Calendar,
						ResString.GetMultilingualString("622026bc-6492-4cac-9260-b125297cff6e", "Calendar Invitation Solution For Organizers"),
						ResString.GetMultilingualString("9d204dd6-efeb-43ca-b218-b31fe8c1e573",
@"When Calendar invitations are being delivered through the application on behalf of mail accounts managed through Lotus Notes amongst other mail clients, they can be rejected and not received by Organizers because the invitations did not originate from its own software.  This behavior may also be experienced using SMTP servers when the system SMTP server is authenticated. This is caused because the SMTP is able to recognize that the Organizer is the sender therefore states 'As the meeting organizer, you do not need to respond to the meeting'.

In some SMTP servers, if the SMTP server is not authenticated, it is possible to send calendar invitations on behalf of an Organizer to self and the user is able to accept or decline their own invitations.

By setting this option to 'Yes', regardless of your SMTP server configuration, Calendar invitations sent to the Organizer will be received using the system email address, with the Organizer's Staff name displayed as the Sender.  In these cases, you should manage your schedules and cancellations through the application as replies will be received by the system.  All other calendar invitations recipients where the recipient is not the Organizer, will receive the invitation from the actual Organizers email address and any replies will be received by the Organizers mail client accordingly.

If you are using a mail server which is not authenticated or you are using an SMTP which when authenticated does not reject calendar invitations created on behalf of mail clients by Organizers,  you should set this registry to 'No'."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		internal BooleanRegistryItem DisplayUtcOffset
		{
			get
			{
				return GetItem("DisplayUtcOffset", delegate
				{
					return new BooleanRegistryItem(
						"DisplayUtcOffset",
						Categories.System_UI,
						ResString.GetMultilingualString("B9B9AA00-BF40-4AFF-90BE-859E94615888", "Display GMT Offset for DateTimeOffset fields"),
						ResString.GetMultilingualString("DD5A1F14-3569-4933-9207-3B78CCCE130A", "Specifies whether the GMT Offset is displayed for DateTimeOffset fields."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public IntRegistryItem ClientDocumentVersion
		{
			get
			{
				return GetItem("ClientDocumentVersion", delegate
				{
					return new IntRegistryItem(new PhysicalServerRegistryItem(
						"ClientDocumentVersion",
						null, null, null,
						RegistryDataTypes.IntType,
						RegistryOptions.IsHidden));
				});
			}
		}

		public IRegistryItem ClientDocumentName
		{
			get
			{
				return GetItem("ClientDocumentName", delegate
				{
					return new PhysicalServerRegistryItem(
						"ClientDocumentName",
						Categories.Documents,
						(NoResString)"Client Documents",  // Support only registry item
						(NoResString)"Client Documents Name (Support Only). Changes will be applied on the next upgrade.", // Support only registry item
						RegistryDataTypes.StringType,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		internal IRegistryItem UserRunningUpgrade
		{
			get
			{
				return GetItem("UserRunningUpgrade", delegate
				{
					return new PhysicalServerRegistryItem(
						"UserRunningUpgrade",
						null, null, null,
						RegistryDataTypes.GuidType,
						RegistryOptions.IsHidden);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ResidencyStatus
		{
			get
			{
				return GetItem("ResidencyStatus", delegate
				{
					CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("CTZ", ResString.GetMultilingualString("b3dd63d9-dd7c-4682-9801-5c55476f5efe", "Citizen"));
					defaultList.AddPair("RES", ResString.GetMultilingualString("26541e00-0160-4e68-841b-967a94b2053e", "Permanent Resident"));
					defaultList.AddPair("TMP", ResString.GetMultilingualString("5534d801-070e-4d42-ab0f-727e4cd25771", "Temporary Visa Holder"));
					defaultList.AddPair("STD", ResString.GetMultilingualString("73b3763b-d7bb-456b-9aee-ae91ddb3d79d", "Student Visa Holder"));

					return new CodeDescriptionPairListRegistryItem
					(
						"ResidencyStatus",
						Categories.System_Staff_HRMS,
						ResString.GetMultilingualString("aafa926f-10e5-4737-83ed-bf43e04efee7", "Residency Status"),
						ResString.GetMultilingualString("f35bcac9-8cec-4cf9-bbf7-c682a305b4da", "If enabled, Residency Status can be customizable"),
						3,
						RegistryStorageFlags.System,
						defaultList
					);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem DepartureReason
		{
			get
			{
				return GetItem("DepartureReason", delegate
				{
					CodeDescriptionPairList defaultList = new CodeDescriptionPairList();

					defaultList.AddPair("RSN", ResString.GetMultilingualString("23506cbc-22f7-4b20-ab95-cac16cbd3f68", "Resignation"));
					defaultList.AddPair("RTA", ResString.GetMultilingualString("de8145ca-9ae9-4e13-bc63-51eaa28ad260", "Retirement - Age"));
					defaultList.AddPair("RTH", ResString.GetMultilingualString("2e9159c7-bfb7-4edc-a1e1-66d0225629c9", "Retirement - Health"));

					defaultList.AddPair("DML", ResString.GetMultilingualString("20d13df8-be5f-41bf-ab8f-7f80d5261773", "Dismissal"));
					defaultList.AddPair("RDY", ResString.GetMultilingualString("91565d6d-37f7-4aa0-abdf-8553c7ed52ae", "Redundancy"));
					defaultList.AddPair("DIS", ResString.GetMultilingualString("7e8cd80f-8fe8-46e7-b718-260e8d207532", "Death in Service"));
					defaultList.AddPair("OTH", ResString.GetMultilingualString("5e910546-0862-44d0-bfda-b633e2bb196d", "Other - See Comments"));

					return new CodeDescriptionPairListRegistryItem
					(
						"DepartureReason",
						Categories.System_Staff_HRMS,
						ResString.GetMultilingualString("daa53a8f-5e6b-43d2-bd9c-bb2e81d89ec7", "Departure Reason"),
						ResString.GetMultilingualString("513bf46a-c84f-4c37-959e-80a31b989dd4", "If enabled, departure reason can be customizable"),
						3,
						RegistryStorageFlags.System,
						defaultList
					);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem LanguageSkillLevel
		{
			get
			{
				return GetItem("LanguageSkillLevel", delegate
				{
					CodeDescriptionPairList defaultList = new CodeDescriptionPairList();
					defaultList.AddPair("NAT", ResString.GetMultilingualString("06b844b5-03a1-411f-82e6-01fc006967f1", "Natural Language"));
					defaultList.AddPair("POR", ResString.GetMultilingualString("4fb7279f-39b8-4776-8558-5b4893e8f750", "Poor"));
					defaultList.AddPair("AVG", ResString.GetMultilingualString("b30c0e38-a196-40c4-b89f-70267b4af42b", "Average"));
					defaultList.AddPair("EXC", ResString.GetMultilingualString("34c8b53d-746d-4a76-b5ff-0622bb1f02ce", "Excellent"));

					return new CodeDescriptionPairListRegistryItem
					(
						"LanguageSkillLevel",
						Categories.System_Staff_HRMS,
						ResString.GetMultilingualString("41eb4b49-7a66-4283-91ac-53949baecc4c", "Language Skill Level"),
						ResString.GetMultilingualString("ba384844-51a6-4f2b-9bd7-8a891f5d6e96", "If enabled, Language Skill Level can be customizable"),
						3,
						RegistryStorageFlags.System,
						defaultList
					);
				});
			}
		}

		public StringRegistryItem StandardWorkingHours
		{
			get
			{
				return GetItem("StandardWorkingHours", delegate
				{
					StringRegistryItem result = new StringRegistryItem("StandardWorkingHours",
						Categories.System_Staff_HRMS,
						ResString.GetMultilingualString("c41de809-925f-4018-ad53-423a074cd190", "Standard Working Hours"),
						ResString.GetMultilingualString("54195772-7773-4fbc-aa69-dd19156ad3ac",
@"Describes the standard weekly working hours for staff employed full-time at the provided branch or company. Working hours must be specified in HH:mm format."),
						new DurationRegistryDataType(),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"40:00");
					return result;
				});
			}
		}

		internal IRegistryItem UserEventTrackingEnterprise
		{
			get
			{
				return GetItem("UserEventTrackingEnterprise", delegate
				{
					return new BooleanRegistryItem(
						"UserEventTrackingEnterprise",
						Categories.System_Staff_ActivityLogging,
						ResString.GetMultilingualString("6d2407cd-1350-4ea2-b7fe-dab0ae39096d", "Internal application Activity Tracking"),
						ResString.GetMultilingualString("b10a279f-376a-4626-b236-78be14a77605",
@"If turned on all user activity, including Window Titles, activity duration and activity statistics, within the application is tracked.

Note: Ensure you have obtained consent of individuals to track their application usage prior to setting this up in order to comply with GDPR Regulations that applies to all EU residents."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		internal IRegistryItem UserEventTrackingExternal
		{
			get
			{
				return GetItem("UserEventTrackingExternal", delegate
				{
					return new BooleanRegistryItem(
						"UserEventTrackingExternal",
						Categories.System_Staff_ActivityLogging,
						ResString.GetMultilingualString("d689ab66-f62b-4815-b725-5413528c39bd", "Activity Tracking - External"),
						ResString.GetMultilingualString("58ec49be-93f1-4fdf-94c2-a417302eb8b8", "If turned on, all user activity is tracked - even that that occurs outside the application. Window Titles and activity duration is logged against the staff member."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default,
						false);
				});
			}
		}

		#region User Help Mode

		internal StringRegistryItem UserHelpMode
		{
			get
			{
				return GetItem("HelpMode", delegate
				{
					return new StringRegistryItem("HelpMode", Categories.System_Miscellaneous, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, nameof(HelpMode.Standard));
				});
			}
		}

		#endregion

		#region MegabytesOfManagedMemoryBeforeAutomaticCollection

		public IntRegistryItem MegabytesOfManagedMemoryBeforeAutomaticCollection
		{
			get
			{
				return GetItem("MegabytesOfManagedMemoryBeforeAutomaticCollection", delegate
				{
					return new IntRegistryItem("MegabytesOfManagedMemoryBeforeAutomaticCollection",
						Categories.Optimization,
						ResString.GetMultilingualString("fad79f3c-9714-4142-a131-fa8c08013f5f", "Managed memory before collect"),
						ResString.GetMultilingualString("990e4b5c-66ad-42bd-84f4-cd3d83420ff0", "Enter the number of megabytes of managed memory that may be allocated before the system attempts to recover memory"),
						new NumericRegistryEditorInfo(0), RegistryStorageFlags.System, RegistryOptions.Default, 100, 0, 65535);
				});
			}
		}

		#endregion

		#region TrainingModeEnabled

		public BooleanRegistryItem TrainingModeEnabled
		{
			get
			{
				return GetItem("TrainingModeEnabled", delegate
				{
					return new BooleanRegistryItem("TrainingModeEnabled", Categories.System_Miscellaneous, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
				});
			}
		}

		#endregion

		public BooleanRegistryItem ReportStatisticsLogExecutionPlan
		{
			get
			{
				return GetItem("ReportStatisticsLogExecutionPlan", delegate
				{
					return new BooleanRegistryItem("ReportStatisticsLogExecutionPlan", Categories.System_Database_UserOptions, ResString.GetMultilingualString("be9b140b-39d2-42cb-82f8-d479fe57abdc", "Report Statistics - Log Execution Plan"),
						ResString.GetMultilingualString("d9515479-dc89-4adf-899c-29468e328e6e", "When enabled, the execution plan used for the report will be stored in the Execution Plan field on the Report Statistics tab."), RegistryStorageFlags.System, false);
				});
			}
		}

		public IntRegistryItem PurgeReportStatisticsLogs
		{
			get
			{
				return GetItem("PurgeReportStatisticsLogs", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
							name: "PurgeReportStatisticsLogs",
							category: Categories.System_Database_UserOptions,
							caption: ResString.GetMultilingualString("4ee674ee-2dab-4342-9038-b6497b494d1c", "Purge Report Statistics Logs older than n days"),
							hint: ResString.GetMultilingualString("81258b16-99be-419f-bbb3-f413445a8385", "How old a Report Statistics Log must be (in days) before the Purge Report Statistics Logs (PRS) service task will delete it."),
							editorInfo: new NumericRegistryEditorInfo(0),
							storage: RegistryStorageFlags.System,
							options: RegistryOptions.Default,
							defaultValue: 60,
							minValue: 0,
							maxValue: 200000
						);

					return result;
				});
			}
		}

		public IntRegistryItem InternalApplicationActivityTrackingInterval
		{
			get
			{
				return GetItem("InternalApplicationActivityTrackingInterval", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
							name: "InternalApplicationActivityTrackingInterval",
							category: Categories.System_Staff_ActivityLogging,
							caption: ResString.GetMultilingualString("9BB6C367-29D1-471A-AAFC-7734FCD90EC3", "Internal application Activity Tracking Interval"),
							hint: ResString.GetMultilingualString("03FC5AB2-2646-4CAD-B484-A06321DA82BF", "Customizes the interval of when the Activity Logs are updated.  The minimum value is 10 seconds and the maximum value is 600 seconds."),
							editorInfo: new NumericRegistryEditorInfo(0),
							storage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
							options: RegistryOptions.Default,
							defaultValue: 300,
							minValue: 10,
							maxValue: 600
						);

					return result;
				});
			}
		}

		#region DisplayGMTOffsetOnF5UserInfo

		public BooleanRegistryItem DisplayGMTOffsetOnF5UserInfo
		{
			get
			{
				return GetItem("DisplayGMTOffsetOnF5UserInfo", delegate
				{
					return new BooleanRegistryItem("DisplayGMTOffsetOnF5UserInfo", Categories.System_UI, ResString.GetMultilingualString("F8F4AAE1-8AB7-4E68-89D8-81FFFDD76680", "Display GMT Offset on F5 User Info"),
						ResString.GetMultilingualString("3D483DA6-DD2E-45D1-B254-D887E84BEC87", "Specifies whether the GMT Offset will be included in the User Info when F5 is pressed."), RegistryStorageFlags.System, true);
				});
			}
		}

		#endregion

		#region ShowDialogForReopeningFormsLeftAfterRestart

		public BooleanRegistryItem RememberOpenedFormsAfterUpgrade
		{
			get
			{
				return GetItem("RememberOpenedFormsAfterUpgrade", delegate
				{
					return new BooleanRegistryItem("RememberOpenedFormsAfterUpgrade",
						Categories.System_UI, ResString.GetMultilingualString("AA35FDB3-10B2-4ED3-BF74-FF6249D6EFA9", "Remember Opened Forms After Upgrade"),
						ResString.GetMultilingualString("F0DB6472-814D-4049-978F-8BF805EB3D2D", "When an Active User has been disconnected for a database upgrade, the forms that were opened will be reloaded when the user logs in."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region ShowSaveProgressBox

		public BooleanRegistryItem ShowSaveProgressBox
		{
			get
			{
				return GetItem("ShowSaveProgressBox", delegate
				{
					return new BooleanRegistryItem("ShowSaveProgressBox", Categories.System_UI, ResString.GetMultilingualString("07938ac8-cd25-4740-b7b4-950daffe13fb", "Show Save Progress Box"), ResString.GetMultilingualString("7694dc3c-f479-4603-ba43-e3c99391b1bc", "Show a progress box when the user clicks save."), RegistryStorageFlags.System, true);
				});
			}
		}

		public BooleanRegistryItem ShowCodeAtCompanyAndBranchName
		{
			get
			{
				return GetItem("ShowCodeAtCompanyAndBranchName", delegate
				{
					return new BooleanRegistryItem("ShowCodeAtCompanyAndBranchName", Categories.System_UI, ResString.GetMultilingualString("4AC5AC8E-5EFD-40ED-A3D0-196EE769CB4E", "Show Company and Branch codes on Registry Fallback Levels"), ResString.GetMultilingualString("E41B6E47-8C3B-45B3-82DD-B54DA7667877", "When enabled, Company and Branch codes will display on the Registry fallback levels."), RegistryStorageFlags.System, false);
				});
			}
		}

		#endregion

		#region LightValidationEnabled

		public BooleanRegistryItem LightValidationEnabled
		{
			get
			{
				return GetItem("LightValidationEnabled", delegate
				{
					return new BooleanRegistryItem("LightValidationEnabled"
						, Categories.System_Framework
						, ResString.GetMultilingualString("e6c77da2-b886-48cf-94bd-f42366bc14fa", "Enable Light Validation")
						, ResString.GetMultilingualString("fef68037-8924-4a52-b469-b605065c509a", "Use faster validation that only checks records with changes during save.")
						, RegistryStorageFlags.System
						, EntityFrameworkRegistryDefaults.LightValidationEnabled
						);
				});
			}
		}

		#endregion

		#region ApplyOptionRecompile

		public BooleanRegistryItem ApplyOptionRecompile
		{
			get
			{
				return GetItem("ApplyOptionRecompile", delegate
				{
					return new BooleanRegistryItem("ApplyOptionRecompile",
						Categories.System_Database_UserOptions,
						ResString.GetMultilingualString("4E4E9B56-DA25-4C5B-9B33-0979CC8F443B", "Apply OPTION(RECOMPILE) to find screen"),
						ResString.GetMultilingualString("7DD29A85-7903-4804-ABF1-9CA711385536", @"If turned on, add 'option (recompile)' to the SQL query if the query is from a find screen and the keyword 'like' is used."),
						RegistryStorageFlags.System,
						EntityFrameworkRegistryDefaults.ApplyOptionRecompile);
				});
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem LegacyEncryptedSystemRegistrationKey
		{
			get
			{
				return GetItem("FreightNotesHeaderLength", delegate
				{
					return new StringRegistryItem("FreightNotesHeaderLength", Categories.System_Miscellaneous, (NoResString)"System Expiry Date", (NoResString)"Application System Expiry Date", RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, "");
				});
			}
		}

		public StringRegistryItem EncryptedRegistrationKey
		{
			get
			{
				return GetItem("FreightNotesRegistration", delegate
				{
					return new StringRegistryItem("FreightNotesRegistration", Categories.System_Miscellaneous, (NoResString)"System Registration", (NoResString)"System Registration", RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, "");
				});
			}
		}

		#endregion

		[RegistryItemReservedNameTestExclude]
		internal BinaryRegistryItem GridLayout
		{
			get
			{
				return GetItem("GridLayout", delegate
				{
					return new BinaryRegistryItem("GridLayout", null, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden);
				});
			}
		}

		/// <summary>
		/// This is only a temporary solution to stop unimplemented modules from shown.
		/// </summary>
		internal BooleanRegistryItem ShowUnimplementedModule
		{
			get
			{
				return GetItem("ShowUnimplementedModule", delegate
				{
					return new BooleanRegistryItem("ShowUnimplementedModule", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden, true);
				});
			}
		}

		internal BinaryRegistryItem EagleMailPrivateKey
		{
			get
			{
				return GetItem("EagleMailPrivateKey", delegate
				{
					return new BinaryRegistryItem("EagleMailPrivateKey", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden);
				});
			}
		}

		internal StringRegistryItem EagleMailPrivateKeyPassword
		{
			get
			{
				return GetItem("EagleMailPrivateKeyPassword", delegate
				{
					return new StringRegistryItem("EagleMailPrivateKeyPassword", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden);
				});
			}
		}

		internal BinaryRegistryItem EagleMailPublicCertificate
		{
			get
			{
				return GetItem("EagleMailPublicCertificate", delegate
				{
					return new BinaryRegistryItem("EagleMailPublicCertificate", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden);
				});
			}
		}

		[RegistryItemReservedNameTestExclude]
		internal IRegistryItem FilterCriteria
		{
			get
			{
				return GetItem("FilterCriteria", delegate
				{
					return new StringRegistryItem("FilterCriteria", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem CurrentCheckedOutClientName
		{
			get
			{
				return GetItem("CurrentCheckedOutClientName", delegate
				{
					return new StringRegistryItem("CurrentCheckedOutClientName", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden);
				});
			}
		}

		internal PhysicalServerRegistryItem ediTariffInstallationDirectory
		{
			get
			{
				return GetItem("ediTariffInstallationDirectory", delegate
				{
					return new PhysicalServerRegistryItem("ediTariffInstallationDirectory",
						Categories.BorderWise,
						ResString.GetMultilingualString("7e1c1353-2f8a-4470-ae58-cac5281a2aa9", "ediTariff Installation Directory"),
						ResString.GetMultilingualString("6bd559fa-a68e-46de-883c-de86fe57c88b", "The location for ediTariff. If not specified, the application will attempt to find ediTariff in the most commonly installed locations."),
						RegistryDataTypes.StringType,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public CodePairRegistryItem ExternalBorderComplianceTool
		{
			get
			{
				return GetItem("ExternalBorderComplianceTool", () =>
					new CodePairRegistryItem(
						name: "ExternalBorderComplianceTool",
						category: Categories.BorderWise,
						caption: ResString.GetMultilingualString("c731664b-b130-4052-8eb8-fa81aaa08132", "External Border Compliance Tool"),
						hint: ResString.GetMultilingualString("8ae680eb-afb4-4327-a44d-31a1578cee43", "Select which external tool, if any, to be used for looking up tariffs."),
						lookUpList: new CodeDescriptionPairListProvider(() => new ExternalBorderComplianceToolList()),
						storage: RegistryStorageFlags.System | RegistryStorageFlags.Company,
						options: DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						defaultValue: ExternalBorderComplianceToolList.Codes.BorderWiseWeb));
			}
		}

		public StringRegistryItem BorderWiseUmpApiBaseAddress
		{
			get
			{
				return GetItem("BorderWiseUmpApiBaseAddress",
					() => new StringRegistryItem(
						"BorderWiseUmpApiBaseAddress",
						Categories.BorderWise,
						ResString.GetMultilingualString("7ad2fe2f-d827-4fb9-b112-e43a8143d2ed", "BorderWise UMP API Base Address"),
						ResString.GetMultilingualString("d1e84185-71bf-4777-8a18-5ccc9a26fa4c", "Base address of BorderWise User Management Portal API."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						"https://ump.borderwise.com/api"));
			}
		}

		public StringRegistryItem BorderWiseWebAddress
		{
			get
			{
				return GetItem("BorderWiseWebAddress",
					() => new StringRegistryItem(
						"BorderWiseWebAddress",
						Categories.BorderWise,
						(NoResString)"BorderWise Web Address", // Developer-only registry item
						(NoResString)"Address of BorderWise Web application.", // Developer-only registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						"https://app.borderwise.com"));
			}
		}

		public BooleanRegistryItem BorderWiseEnableWebSocketClient
		{
			get
			{
				return GetItem("BorderWiseEnableWebSocketClient",
					() => new BooleanRegistryItem(
						"BorderWiseEnableWebSocketClient",
						Categories.BorderWise,
						ResString.GetMultilingualString("8592C655-92AA-421A-878F-01500E621C62", "BorderWise Enable Web Socket Client"),
						ResString.GetMultilingualString("9E795C85-9543-4833-AB45-A16DCC3EAA30", "Enable Web Socket Client Communication To BorderWise Hub"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
					true));
			}
		}

		public BooleanRegistryItem BorderWiseEnableMultilineTariffClassification
		{
			get
			{
				return GetItem("BorderWiseEnableMultilineTariffClassification",
					() => new BooleanRegistryItem(
						"BorderWiseEnableMultilineTariffClassification",
						Categories.BorderWise,
						ResString.GetMultilingualString("2e40c82f-804d-4ed9-aadf-51622e0a4213", "BorderWise Enable Multi-line Tariff Classification"),
						ResString.GetMultilingualString("a8aa3c82-6cc8-480b-bb5a-8548b7ee3d4a", "Enable Web Socket Client Communication To Use Multi-line Tariff Classification"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true));
			}
		}

		public BooleanRegistryItem BorderWiseEnableMultilineTariffClassificationServiceTask
		{
			get
			{
				return GetItem("BorderWiseEnableMultilineTariffClassificationServiceTask",
					() => new BooleanRegistryItem(
						"BorderWiseEnableMultilineTariffClassificationServiceTask",
						Categories.BorderWise,
						(NoResString)"BorderWise Enable ServiceTask for Multi-line Tariff Classification",
						(NoResString)"Enable ServiceTask for Multi-line Tariff Classification",
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public StringRegistryItem BorderWiseWebSocketHubUrl
		{
			get
			{
				return GetItem("BorderWiseWebSocketHubUrl",
					() => new StringRegistryItem(
						"BorderWiseWebSocketHubUrl",
						Categories.BorderWise,
						(NoResString)"BorderWise Web Socket Hub Url", // Developer-only registry item
						(NoResString)"Address of BorderWise Web Socket Hub.", // Developer-only registry item
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport,
						"wss://app.borderwise.com/api/ws/hub"));
			}
		}

		public StringRegistryItem BorderWiseApiKey
		{
			get
			{
				return GetItem("BorderWiseApiKeyValue",
					() => new StringRegistryItem(
						"BorderWiseApiKeyValue",
						Categories.BorderWise,
						(NoResString)"BorderWise Api key value", // Developer-only registry item
						(NoResString)"Subscription key value for BorderWise Api.", // Developer-only registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						 "NjAyMDRmN2MtMjA1OC00YjY4LWI1ZTEtZmQ0NDY5ZGE3ZTA2"));
			}
		}

		internal BooleanRegistryItem AutoPopulateHAWBsOnConsol
		{
			get
			{
				return GetItem("AutoPopulateHAWBsOnConsol", delegate
				{
					return new BooleanRegistryItem("AutoPopulateHAWBsOnConsol", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("ed2071f7-27e4-4dd8-a441-b442a944c03e", "Auto Populate HAWBs on Consol"), null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem StaffMembershipTypeList
		{
			get
			{
				return GetItem("StaffMembershipTypeList", delegate
				{
					CodeDescriptionPairList membershipTypeList = new MembershipTypeList();
					return new CodeDescriptionPairListRegistryItem("StaffMembershipTypeList", Categories.System_Staff, ResString.GetMultilingualString("a5933210-1389-46cb-907a-a671463bcf29", "Staff Membership Type List"), ResString.GetMultilingualString("7c1dc0c4-8237-4389-b733-4614d4384baa", "List of codes used for staff membership types on Group form."), 3, RegistryStorageFlags.System, membershipTypeList);
				});
			}
		}

		public TableRowCountRegistryItem TableRowCount
		{
			get
			{
				return GetItem("TableRowCountWithUtcTime", delegate
				{
					return new TableRowCountRegistryItem(
					"TableRowCountWithUtcTime",
					Categories.System_Miscellaneous,
					null,
					null,
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden,
					new TableRowCountDataType());
				});
			}
		}

		internal IRegistryItem RelatedNoteReadDelay
		{
			get
			{
				return GetItem("RelatedNoteReadDelay", delegate
				{
					return new IntRegistryItem(
						"RelatedNoteReadDelay", Categories.System_UI, ResString.GetMultilingualString("53c824fb-f46f-431f-9b61-a42a4016bb1b", "Related-Note Read Delay"),
						ResString.GetMultilingualString("69111105-dff7-45d8-a653-808277f8e738", "The number of seconds a user must have a related note selected before it is automatically marked as 'read'. Set a value of 0 for no delay."),
						new NumericRegistryEditorInfo(0), RegistryStorageFlags.Company, RegistryOptions.Default, 2, 0, 10);
				});
			}
		}

		internal IRegistryItem DateTimeStaffFormWasLastShown
		{
			get
			{
				return GetItem("DateTimeStaffFormWasLastShown", delegate
				{
					return new DateTimeRegistryItem("DateTimeStaffFormWasLastShown", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal BooleanRegistryItem DisableEdocsFileTypeValidation
		{
			get
			{
				return GetItem("DisableEdocsFileTypeValidation", delegate
				{
					return new BooleanRegistryItem("DisableEdocsFileTypeValidation", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden, false);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal BooleanRegistryItem UCMoveLinkableOnlyEntitiesToProperties
		{
			get
			{
				return GetItem(
					"UCMoveLinkableOnlyEntitiesToProperties",
					() => new BooleanRegistryItem(
						"UCMoveLinkableOnlyEntitiesToProperties",
						Categories.System_UniversalCopy,
						ResString.GetMultilingualString("8154b29e-d0d3-4056-8673-38c138c3000a", "Move Link-only Related Entities to Properties"),
						ResString.GetMultilingualString("a64daa54-1b0c-498b-b58c-316abbe7580a", "Move all related entities that can only be linked from Related Records tree to corresponding properties lists."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						true)
					);
			}
		}

		public CodePairRegistryItem TemplateRecordValidation
		{
			get
			{
				return GetItem("TemplateRecordValidation", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var lookUpList = new CodeDescriptionPairList();
						lookUpList.AddPair(TemplateRecordValidationCodes.StandardValidation, ResString.GetMultilingualString("f731f765-0868-4e62-b2dc-fed117f23de1", "Standard Validation"));
						lookUpList.AddPair(TemplateRecordValidationCodes.NoValidation, ResString.GetMultilingualString("64755c40-1a99-419e-b72f-d918f08391f6", "No Validation"));
						lookUpList.AddPair(TemplateRecordValidationCodes.IgnoreAndSave, ResString.GetMultilingualString("1da1d427-0f1f-4572-89c1-165039e918c9", "Ignore Validation on Save"));
						return lookUpList;
					});

					return new CodePairRegistryItem(
						"TemplateRecordValidation",
						Categories.System_UniversalCopy,
						ResString.GetMultilingualString("f476943b-aa7d-4898-8311-91e9c321659c", "Template Records Validation"),
						ResString.GetMultilingualString("acfecdc6-0b85-474e-af81-576003c94fff", "This setting allows you to set validation behavior for template records. Default is STD - standard validation. If NON is selected - validation on template records will be completely disabled. If IGN is selected - validation is performed, but during save user will be given an option to save template record even with validation errors."),
						lookUpListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"STD");
				});
			}
		}

		public static class TemplateRecordValidationCodes
		{
			public const string StandardValidation = "STD";
			public const string NoValidation = "NON";
			public const string IgnoreAndSave = "IGN";
		}

		public CodePairRegistryItem DefaultRelatedCopyScheduleDeactivationBehavior
		{
			get
			{
				return GetItem("DefaultRelatedCopyScheduleDeactivationBehavior", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var lookUpList = new CodeDescriptionPairList();
						lookUpList.AddPair(DefaultRelatedCopyScheduleDeactivationBehaviorCodes.DoNotDeactivateAnyCopySchedule, ResString.GetMultilingualString("fb113a5e-1f39-402b-a1e1-915d64740514", "Do not deactivate any copy schedule"));
						lookUpList.AddPair(DefaultRelatedCopyScheduleDeactivationBehaviorCodes.DeactivateAllCopySchedules, ResString.GetMultilingualString("1de4f1a1-8e5b-4d84-ac55-040aaf0cf8b1", "Deactivate all copy schedules"));
						return lookUpList;
					});

					return new CodePairRegistryItem(
						"DefaultRelatedCopyScheduleDeactivationBehavior",
						Categories.System_UniversalCopy,
						ResString.GetMultilingualString("c610e8c4-d5db-4a8a-b097-44b4937c6d4e", "Default Related Copy Schedule Deactivation Behavior"),
						ResString.GetMultilingualString("3a024ba8-38db-44f2-b0ad-77d8de578185", "This setting controls whether copy schedules attached to workflows with tasks that are being canceled by automated processes are deactivated or not."),
						lookUpListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"DND");
				});
			}
		}

		public static class DefaultRelatedCopyScheduleDeactivationBehaviorCodes
		{
			public const string DoNotDeactivateAnyCopySchedule = "DND";
			public const string DeactivateAllCopySchedules = "DEA";
		}

		#endregion

		#region Region Number Format

		internal NumberGroupSeparatorRegistryItem NumberGroupSeparator
		{
			get
			{
				return GetItem("NumberGroupSeparator", () =>
				{
					var result = new NumberGroupSeparatorRegistryItem("NumberGroupSeparator",
						Categories.System_UI_RegionNumberFormat,
						ResString.GetMultilingualString("0B670824-55AC-4CF4-9FB6-15DD8F30BF0B", "Number Group Separator"),
						ResString.GetMultilingualString("C6713E01-7DAB-4852-8FCF-B9EA21DEF6B4", "The string that separates groups of digits to the left of the decimal in numeric values."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory);
					return result;
				});
			}
		}

		internal NumberDecimalSeparatorRegistryItem NumberDecimalSeparator
		{
			get
			{
				return GetItem("NumberDecimalSeparator", () =>
				{
					var result = new NumberDecimalSeparatorRegistryItem("NumberDecimalSeparator",
						Categories.System_UI_RegionNumberFormat,
						ResString.GetMultilingualString("081C727C-44B1-49A3-9BB3-8E6BFE7EF615", "Number Decimal Separator"),
						ResString.GetMultilingualString("4836CE2F-C03B-4547-9D1A-C31BC370B6B7", "The string to use as the decimal separator in numeric values."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory);
					return result;
				});
			}
		}

		internal NumberGroupSizesStringListRegistryItem NumberGroupSizes
		{
			get
			{
				return GetItem("NumberGroupSizes", () =>
				{
					var result = new NumberGroupSizesStringListRegistryItem("NumberGroupSizes",
						Categories.System_UI_RegionNumberFormat,
						ResString.GetMultilingualString("E6326C39-3D25-4E8C-BE7D-575A3789ECFF", "Number Group Sizes"),
						ResString.GetMultilingualString("6C4B375A-3109-422E-99BC-0B864B2480BC", "The number of digits in each group to the left of the decimal in numeric values, multiple values separated by commas."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default);
					return result;
				});
			}
		}

		internal CurrencyGroupSeparatorRegistryItem CurrencyGroupSeparator
		{
			get
			{
				return GetItem("CurrencyGroupSeparator", () =>
				{
					var result = new CurrencyGroupSeparatorRegistryItem("CurrencyGroupSeparator",
						Categories.System_UI_RegionNumberFormat,
						ResString.GetMultilingualString("F53A7773-13A3-4D0F-B503-47C5417769FF", "Currency Group Separator"),
						ResString.GetMultilingualString("7D311054-F591-492D-A511-6315AD6A8D46", "The string that separates groups of digits to the left of the decimal in currency values."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory);
					return result;
				});
			}
		}

		internal CurrencyDecimalSeparatorRegistryItem CurrencyDecimalSeparator
		{
			get
			{
				return GetItem("CurrencyDecimalSeparator", () =>
				{
					var result = new CurrencyDecimalSeparatorRegistryItem("CurrencyDecimalSeparator",
						Categories.System_UI_RegionNumberFormat,
						ResString.GetMultilingualString("7DFAD361-48D6-448B-A833-B22FADB3F18A", "Currency Decimal Separator"),
						ResString.GetMultilingualString("86630E4E-6F61-4632-96B8-29E2AF49A2AD", "The string to use as the decimal separator in currency values."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory);
					return result;
				});
			}
		}

		internal CurrencyGroupSizesStringListRegistryItem CurrencyGroupSizes
		{
			get
			{
				return GetItem("CurrencyGroupSizes", () =>
				{
					var result = new CurrencyGroupSizesStringListRegistryItem("CurrencyGroupSizes",
						Categories.System_UI_RegionNumberFormat,
						ResString.GetMultilingualString("D8FA6909-17EE-42C7-A7F7-9B8F77200587", "Currency Group Sizes"),
						ResString.GetMultilingualString("C2FAB6CC-CBBD-46A3-AFAC-4FA63E865618", "The number of digits in each group to the left of the decimal in currency values, multiple values separated by commas."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default);
					return result;
				});
			}
		}

		#endregion

		#region Physical Server Items

		public class PhysicalServerRegistryItem : RegistryItemImpl
		{
			public PhysicalServerRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType)
				: base(name, category, caption, hint, dataType, RegistryStorageFlags.System, RegistryOptions.Default)
			{
			}

			public PhysicalServerRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryOptions options)
				: base(name, category, caption, hint, dataType, RegistryStorageFlags.System, options)
			{
			}

			public PhysicalServerRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, object defaultValue)
				: base(name, category, caption, hint, dataType, RegistryStorageFlags.System, RegistryOptions.Default, defaultValue)
			{
			}

			public PhysicalServerRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryOptions options, object defaultValue)
				: base(name, category, caption, hint, dataType, RegistryStorageFlags.System, options, defaultValue)
			{
			}

			public PhysicalServerRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags flags, RegistryOptions options, object defaultValue)
				: base(name, category, caption, hint, dataType, flags, options, defaultValue)
			{
			}

			public PhysicalServerRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryOptions options, object defaultValue)
				: base(name, category, caption, hint, dataType, editorInfo, RegistryStorageFlags.System, options, defaultValue)
			{
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem PhysicalServerID
		{
			get
			{
				return GetItem("PhysicalServerID", delegate
				{
					return new PhysicalServerRegistryItem("PhysicalServerID", Categories.PhysicalServer, (NoResString)"Physical Server ID", (NoResString)"Physical Server ID", new StringRegistryDataType(0, 3), RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue, "DEM");
				});
			}
		}

		public StringRegistryItem SystemEnterpriseCode
		{
			get
			{
				return GetItem("EnterpriseCode", delegate
				{
					return new StringRegistryItem("EnterpriseCode", Categories.System_License, (NoResString)"Enterprise code", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsReadOnly, "");
				});
			}
		}

		public IntRegistryItem LastProductRegistrationRemoteVerifyResult
		{
			get
			{
				return GetItem("LastProductRegistrationRemoteVerifyResult", delegate
				{
					return new IntRegistryItem("LastProductRegistrationRemoteVerifyResult", Categories.System_License,
						null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.NotLogged, (int)Enterprise.Integration.Licensing.ProductRegistrationVerifyResult.OK);
				});
			}
		}

		#endregion

		internal RegistryOptions GetSupportOnlyOrClientEditableOptionForHostedSystems()
		{
			var registry = new RawDataRegistry();

			if ((bool)registry.AllowHostedClientAccessToEmailSettings.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				return RegistryOptions.Default;
			}
			else
			{
				return RegistryOptions.IsOnlyEditableBySupportIfHosted;
			}
		}

		internal void Ms365AppSecretForIncoming_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			Ms365OAuth2AppTokenForIncoming.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
		}

		internal void Ms365AppSecretForOutgoing_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			Ms365OAuth2AppTokenForOutgoing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
		}

		#region OAuth 2.0

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is proper noun")]
		public const string oAuth = "OAuth 2.0";
		readonly ResourceString clientSecretHint = ResString.GetMultilingualString("CF6525E7-FE44-4C60-8B8A-1B5AD655A7F9", @"This is the Client Secret that has been registered in the Azure platform.  The Client Secret is the value in the Value column of Certificates & secrets settings in Azure.  For added security, the Client Secret will be encrypted when the Registry is saved.

Important: Please consider the expiry period of the Client Secret in the Certificates & secrets page of your App registration in Azure as an expired Client Secret can lead to authentication issues.  Please refer to the Registering App in Microsoft Azure Technical Guide at https://myaccount.cargowise.com/Home/CargoWise/TechnicalGuides.aspx for further details on registering an App in Azure.");

		#region MicroSoft 365

		public CodePairRegistryItem UseOAuth2ForIncoming
		{
			get
			{
				return GetItem("UseOAuth2ForIncoming", () =>
					new CodePairRegistryItem(
						name: "UseOAuth2ForIncoming",
						category: Categories.PhysicalServer_MailIn,
						caption: ResString.GetMultilingualString("20283B17-A312-4A11-80E4-6D3409C90649", "Enable {0} authentication", oAuth),
						hint: ResString.GetMultilingualString("8523F78E-0975-4D15-ADF6-E84B5B7EC753", "When enabled, {0} authentication will be used.", oAuth),
						lookUpList: new CodeDescriptionPairListProvider(() => new OAuth2TypeList()),
						storage: RegistryStorageFlags.System,
						options: GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue));
			}
		}

		public StringRegistryItem Ms365ApplicationIdForIncoming
		{
			get
			{
				return GetItem("Ms365ApplicationIdForIncoming", () =>
				{
					var result = new StringRegistryItem("Ms365ApplicationIdForIncoming",
						Categories.PhysicalServer_Mail_OAuth2_M365In,
						ResString.GetMultilingualString("FA388B90-1727-4B0B-BDC3-EF0FF9921AD6", "Application ID"),
						ResString.GetMultilingualString("E78EE6E4-59E5-460F-8E14-31104F1B95BB", @"This is the Application ID registered in the Azure platform. It should be a unique identifier like '{0}'.

Important: You are required to register your own Application ID under App registrations blade in Azure AD, then enter the ID in this registry item before using the {1} authentication.", "acc8304d-88d3-4caa-8452-2be8061d7fba", oAuth),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);
					return result;
				});
			}
		}

		public StringRegistryItem Ms365AppSecretForIncoming
		{
			get
			{
				return GetItem("Ms365AppSecretForIncoming", () =>
				{
					var result = new StringRegistryItem("Ms365AppSecretForIncoming",
						Categories.PhysicalServer_Mail_OAuth2_M365In,
						ResString.GetMultilingualString("F6E9842A-77EF-4110-B232-FA1F54D7F4B2", "Client Secret"),
						clientSecretHint,
						new StringRegistryDataType(true),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty)
					{
						OnUpdateAction = Ms365AppSecretForIncoming_OnUpdateAction
					};
					return result;
				});
			}
		}

		public BooleanRegistryItem UseGraphApiForIncoming
		{
			get
			{
				return GetItem("UseGraphApiForIncoming", delegate
				{
					return new BooleanRegistryItem(
						new PhysicalServerRegistryItem("UseGraphApiForIncoming",
							Categories.PhysicalServer_Mail_OAuth2_M365In,
							ResString.GetMultilingualString("3B5FDA4B-8E9E-46AA-9289-1E2EC70DE8C8", "Use Graph API"),
							ResString.GetMultilingualString("1908D8C9-4BB4-418A-B115-B1AAABE3240A", "When enabled, Microsoft Graph API will be used."),
							RegistryDataTypes.BoolType,
							GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
							false));
				});
			}
		}

		public Ms365OAuth2TokenRegistryItem Ms365OAuth2TokenForIncoming
		{
			get
			{
				return GetItem("Ms365OAuth2TokenForIncoming", delegate
				{
					return new Ms365OAuth2TokenRegistryItem("Ms365OAuth2TokenForIncoming", Categories.PhysicalServer_Mail_OAuth2_M365In,
						GetTokenRegistryCaption(EmailType.Incoming),
						GetTokenRegistryHint(EmailType.Incoming),
						EmailType.Incoming,
						Ms365OAuth2TenantId,
						Ms365ApplicationIdForIncoming,
						UseGraphApiForIncoming,
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue
					);
				});
			}
		}

		public BinaryRegistryItem Ms365OAuth2AppTokenForIncoming
		{
			get
			{
				return GetItem("Ms365OAuth2AppTokenForIncoming", delegate
				{
					return new BinaryRegistryItem("Ms365OAuth2AppTokenForIncoming", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden);
				});
			}
		}

		public CodePairRegistryItem UseOAuth2ForOutgoing
		{
			get
			{
				return GetItem("UseOAuth2ForOutgoing", () =>
					new CodePairRegistryItem(
						name: "UseOAuth2ForOutgoing",
						category: Categories.PhysicalServer_MailOut,
						caption: ResString.GetMultilingualString("20283B17-A312-4A11-80E4-6D3409C90649", "Enable {0} authentication", oAuth),
						hint: ResString.GetMultilingualString("8523F78E-0975-4D15-ADF6-E84B5B7EC753", "When enabled, {0} authentication will be used.", oAuth),
						lookUpList: new CodeDescriptionPairListProvider(() => new OAuth2TypeList()),
						storage: RegistryStorageFlags.System,
						options: GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue));
			}
		}

		public StringRegistryItem Ms365ApplicationIdForOutgoing
		{
			get
			{
				return GetItem("Ms365ApplicationIdForOutgoing", () =>
				{
					var result = new StringRegistryItem("Ms365ApplicationIdForOutgoing",
						Categories.PhysicalServer_Mail_OAuth2_M365Out,
						ResString.GetMultilingualString("FA388B90-1727-4B0B-BDC3-EF0FF9921AD6", "Application ID"),
						ResString.GetMultilingualString("E78EE6E4-59E5-460F-8E14-31104F1B95BB", @"This is the Application ID registered in the Azure platform. It should be a unique identifier like '{0}'.

Important: You are required to register your own Application ID under App registrations blade in Azure AD, then enter the ID in this registry item before using the {1} authentication.", "4a8997bb-7e41-4980-adb2-7948aa45a623", oAuth),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);
					return result;
				});
			}
		}

		public StringRegistryItem Ms365AppSecretForOutgoing
		{
			get
			{
				return GetItem("Ms365AppSecretForOutgoing", () =>
				{
					var result = new StringRegistryItem("Ms365AppSecretForOutgoing",
						Categories.PhysicalServer_Mail_OAuth2_M365Out,
						ResString.GetMultilingualString("D744E083-0B2D-4C44-AD2C-650152DBAFD7", "Client Secret"),
						clientSecretHint,
						new StringRegistryDataType(true),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty)
					{
						OnUpdateAction = Ms365AppSecretForOutgoing_OnUpdateAction
					};
					return result;
				});
			}
		}

		public BooleanRegistryItem UseGraphApiForOutgoing
		{
			get
			{
				return GetItem("UseGraphApiForOutgoing", delegate
				{
					return new BooleanRegistryItem(
						new PhysicalServerRegistryItem("UseGraphApiForOutgoing",
							Categories.PhysicalServer_Mail_OAuth2_M365Out,
							ResString.GetMultilingualString("3B5FDA4B-8E9E-46AA-9289-1E2EC70DE8C8", "Use Graph API"),
							ResString.GetMultilingualString("1908D8C9-4BB4-418A-B115-B1AAABE3240A", "When enabled, Microsoft Graph API will be used."),
							RegistryDataTypes.BoolType,
							GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
							false));
				});
			}
		}

		public Ms365OAuth2TokenRegistryItem Ms365OAuth2TokenForOutgoing
		{
			get
			{
				return GetItem("Ms365OAuth2TokenForOutgoing", delegate
				{
					return new Ms365OAuth2TokenRegistryItem("Ms365OAuth2TokenForOutgoing", Categories.PhysicalServer_Mail_OAuth2_M365Out,
						GetTokenRegistryCaption(EmailType.Outgoing),
						GetTokenRegistryHint(EmailType.Outgoing),
						EmailType.Outgoing,
						Ms365OAuth2TenantId,
						null,
						null,
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue
					);
				});
			}
		}

		public BinaryRegistryItem Ms365OAuth2AppTokenForOutgoing
		{
			get
			{
				return GetItem("Ms365OAuth2AppTokenForOutgoing", delegate
				{
					return new BinaryRegistryItem("Ms365OAuth2AppTokenForOutgoing", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden);
				});
			}
		}

		public StringRegistryItem Ms365OAuth2TenantId
		{
			get
			{
				return GetItem("Ms365OAuth2TenantId", () =>
				{
					var result = new StringRegistryItem("Ms365OAuth2TenantId",
						Categories.PhysicalServer_Mail_OAuth2_M365,
						ResString.GetMultilingualString("92A3FAFB-EC95-45DB-BC95-FD2420403797", "Tenant ID"),
						ResString.GetMultilingualString("AB912758-458F-40F9-99FF-F111B716377F", "The Tenant ID is used to identify the organization when authenticating the user. If your account type is Single tenant, enter in the Tenant ID. When left blank, the common authority will be used."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);

					return result;
				});
			}
		}

		static MultilingualString GetTokenRegistryCaption(EmailType mailType)
		{
			return ResString.GetMultilingualString("FC7D1FE7-BA17-4BFA-B1F3-B72CC0BA4172", "{0} Access Token for {1}",
				oAuth,
				mailType == EmailType.Incoming
					? Res.GetString("D9CD77CF-FDEC-4F7F-B411-1EDC17C06D01", "Incoming")
					: Res.GetString("1134072A-4C9E-4166-9B64-3E5B1E88C4FE", "Outgoing"));
		}

		static MultilingualString GetTokenRegistryHint(EmailType mailType)
		{
			return ResString.GetMultilingualString("D4E7C0AA-1E8F-44FE-AF2C-3C34C257659C",
				@"This setting stores the {0} Access Token that the {1} Mail Service Task will use during authentication.

Click on Grant Permissions to generate and store the {0} Access Token. If a token has already been generated, clicking on Grant Permissions will renew it. Click on the Clear button to clear the cached token.",
				oAuth,
				mailType == EmailType.Incoming
					? Res.GetString("369BED4E-98DB-49A1-80D2-DA05BA99AEB1", "Inbound")
					: Res.GetString("54ED69C3-9A38-40E9-9012-25B50CB6BE4A", "Outbound"));
		}

		#endregion

		#region Google Mail

		public StringRegistryItem GmailDelegatedMailForIncoming
		{
			get
			{
				return GetItem("GmailDelegatedMailForIncoming", () =>
				{
					var result = new StringRegistryItem("GmailDelegatedMailForIncoming",
						Categories.PhysicalServer_Mail_OAuth2_GmailIn,
						ResString.GetMultilingualString("523ABD52-9295-493B-89C2-DE547902480A", "User Email"),
						ResString.GetMultilingualString("87628447-1EC9-40C8-8B5E-F852DB9B1C54", "Input the mailbox which you want to access, it will be delegated by service account."),
						new StringRegistryDataType(true),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue |
						RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);
					return result;
				});
			}
		}

		public GmailOAuth2JsonFileRegistryItem GmailServiceAccountKeyForIncoming
		{
			get
			{
				return GetItem("GmailServiceAccountKeyForIncoming", () =>
				{
					var result = new GmailOAuth2JsonFileRegistryItem("GmailServiceAccountKeyForIncoming",
						Categories.PhysicalServer_Mail_OAuth2_GmailIn,
						ResString.GetMultilingualString("4F6ADD9E-B723-45BB-A60C-E1A5875C3C34", "Service Account Key"),
						ResString.GetMultilingualString("17809B8F-0D7D-44BE-B8C4-E56906C3C505", "Please choose key file (*.json). The key file can be created under the service account."),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		public StringRegistryItem GmailDelegatedMailForOutgoing
		{
			get
			{
				return GetItem("GmailDelegatedMailForOutgoing", () =>
				{
					var result = new StringRegistryItem("GmailDelegatedMailForOutgoing",
						Categories.PhysicalServer_Mail_OAuth2_GmailOut,
						ResString.GetMultilingualString("523ABD52-9295-493B-89C2-DE547902480A", "User Email"),
						ResString.GetMultilingualString("87628447-1EC9-40C8-8B5E-F852DB9B1C54", "Input the mailbox which you want to access, it will be delegated by service account."),
						new StringRegistryDataType(true),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue |
						RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);
					return result;
				});
			}
		}

		public GmailOAuth2JsonFileRegistryItem GmailServiceAccountKeyForOutgoing
		{
			get
			{
				return GetItem("GmailServiceAccountKeyForOutgoing", () =>
				{
					var result = new GmailOAuth2JsonFileRegistryItem("GmailServiceAccountKeyForOutgoing",
						Categories.PhysicalServer_Mail_OAuth2_GmailOut,
						ResString.GetMultilingualString("4F6ADD9E-B723-45BB-A60C-E1A5875C3C34", "Service Account Key"),
						ResString.GetMultilingualString("17809B8F-0D7D-44BE-B8C4-E56906C3C505", "Please choose key file (*.json). The key file can be created under the service account."),
						RegistryStorageFlags.System,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		#endregion

		#endregion

		public IRegistryItem UseVerboseProtocolLogging
		{
			get
			{
				return GetItem("UseVerboseProtocolLogging", delegate
				{
					return new BooleanRegistryItem(
						new PhysicalServerRegistryItem("UseVerboseProtocolLogging",
							Categories.PhysicalServer_SMTP,
							ResString.GetMultilingualString("D7AE9435-4FC9-4904-8ECE-7609033B9D8F", "Enable verbose protocol logging"),
							ResString.GetMultilingualString("0AE5080B-C686-4A5B-9026-0E76E32F491A", @"When enabled, protocol logging will be available in the Log File of the Outbound Mail Service task.
Warning:  This will generate a large volume of entries into the Log Files.  It is advised to enable this for debugging purposes only.
Note:  The protocol logs are generated at the Debug level, which requires the System > Process Controller > Logging > Enable verbose logging to be enabled. In WiseCloud hosted systems, this can only be enabled by WiseCloud support."),
							RegistryDataTypes.BoolType,
							GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.Default,
							false));
				});
			}
		}

		public IRegistryItem UseMailKitPOP3AndIMAPProtocolLogging
		{
			get
			{
				return GetItem("UseMailKitPOP3AndIMAPProtocolLogging", delegate
				{
					return new BooleanRegistryItem(
						new PhysicalServerRegistryItem("UseMailKitPOP3AndIMAPProtocolLogging",
							Categories.PhysicalServer_MailIn,
							ResString.GetMultilingualString($"CAB4B13B-4B07-4C7F-BA6B-CF63859B2C3D", "Enable verbose protocol logging"),
							ResString.GetMultilingualString("0E987E49-D875-47B0-8246-1A7149C69309", @"When enabled, protocol logging will be included in the Debug logs for the POP3 and IMAP protocols.

Warning:  Protocol logging will generate a large volume of logs and should be enabled for debugging purposes only.  Additionally, the System > Process Controller > Logging > Enable verbose logging Registry setting needs to be enabled as well."),
							RegistryDataTypes.BoolType,
							GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.Default,
							false));
				});
			}
		}

		//Protip: never ever try to change caching on any values here.
		public IRegistryItem SMTPServer
		{
			get
			{
				return GetItem("Physical_System_MailServer", delegate
				{
					return new PhysicalServerRegistryItem("Physical_System_MailServer",
						Categories.PhysicalServer_SMTP,
						ResString.GetMultilingualString("14cf640e-d6b3-44f3-80ad-a5c9b6c6af51", "SMTP Server"),
						ResString.GetMultilingualString("12792F44-7858-408A-B4B7-B3F585149E38", "Address of the SMTP Server. \r\n\r\nModifying this will affect any documents with an email sender override"),
						new StringRegistryDataType(0, 250),
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue)
					{
						OnUpdateAction = Physical_System_MailServer_OnUpdateAction
					};
				});
			}
		}

		void Physical_System_MailServer_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			RescheduleMailTask("OMS", ZDateTime.UtcNow.AddSeconds((int)RegistryRefresh.FrequencyInSeconds));
		}

		void RescheduleMailTask(string code, ZDateTime nextRuntimeUTC)
		{
			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskNextRuntime(code, ZDateTime.Truncate(nextRuntimeUTC, TimeSpan.TicksPerSecond).UtcToDateTimeOffset().ToDateTimeOffsetSafe());
		}

		public IRegistryItem SMTPPort
		{
			get
			{
				return GetItem("SMTPPort", delegate
				{
					return new PhysicalServerRegistryItem("SMTPPort", Categories.PhysicalServer_SMTP, ResString.GetMultilingualString("0ade7414-2292-452a-87dd-7d9447e0b136", "SMTP Port"), ResString.GetMultilingualString("0ade7414-2292-452a-87dd-7d9447e0b136", "SMTP Port"), RegistryDataTypes.IntType, GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue, 25);
				});
			}
		}

		public IRegistryItem SMTPUsername
		{
			get
			{
				return GetItem("SMTPUsername", delegate
				{
					return new PhysicalServerRegistryItem("SMTPUsername", Categories.PhysicalServer_SMTP, ResString.GetMultilingualString("3c65622c-059a-413a-bb09-8bf7976242e7", "SMTP Username"), ResString.GetMultilingualString("52c9cb4a-d3cb-4c24-bdd7-1ca9f330ef31", "The username needed to log onto your SMTP server.  Only enter this if your SMTP server requires authentication."), RegistryDataTypes.StringType, GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue);
				});
			}
		}

		public IRegistryItem SMTPPassword
		{
			get
			{
				return GetItem("SMTPPassword", delegate
				{
					IRegistryItem result = new PhysicalServerRegistryItem("SMTPPassword", Categories.PhysicalServer_SMTP, ResString.GetMultilingualString("4fb490a0-f580-43a3-8ca8-65893be5cd39", "SMTP User Password"), ResString.GetMultilingualString("bdf2eabd-69e8-4d86-97e8-71310308412a", "The password needed to log onto your SMTP server.  Only enter this if your SMTP server requires authentication."), RegistryDataTypes.StringType, GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsPasswordVisibleForControllerUser);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		internal IRegistryItem SMTPEhloDomain
		{
			get
			{
				return GetItem("SMTPEhloDomain", delegate
				{
					return new PhysicalServerRegistryItem(
						"SMTPEhloDomain",
						Categories.PhysicalServer_SMTP,
						ResString.GetMultilingualString("AF889ABF-9CB6-4527-94BB-51B90F375766", "Domain used in SMTP HELO/EHLO command"),
						ResString.GetMultilingualString("02E0B237-3E31-4AD4-BCF2-B07A8CB09409", "If this registry item is not empty, the SMTP HELO/EHLO domain command will use it when sending an email."),
						RegistryDataTypes.StringType,
						GetSupportOnlyOrClientEditableOptionForHostedSystems(),
						EnvProxy.IsHostedWithCargowise ? EntityFrameworkRegistryDefaults.SMTPEhloDomainForWiseGlobal : "");
				});
			}
		}

		internal IRegistryItem SMTPDefaultReturnEmailAddress
		{
			get
			{
				return GetItem("SMTPDefaultReturnEmailAddress", delegate
				{
					return new PhysicalServerRegistryItem(
						"SMTPDefaultReturnEmailAddress",
						Categories.PhysicalServer_SMTP,
						ResString.GetMultilingualString("dfafb4cc-b140-4741-99ba-75c579f22b84", "Default Return Email Address"),
						ResString.GetMultilingualString("dabcfc85-f5b2-445a-a6bf-d9912af6077b", "This email address will be used as the default reply address when emails are sent via the system.\r\n\r\nE.g. {0}", "email_name@domain_name.com"),
						new EmailStringRegistryDataType(),
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
						(string)MailboxEmailAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				});
			}
		}

		internal IRegistryItem SMTPDefaultDoNotReplyEmailAddress
		{
			get
			{
				return GetItem("SMTPDefaultDoNotReplyEmailAddress", delegate
				{
					return new PhysicalServerRegistryItem(
						"SMTPDefaultDoNotReplyEmailAddress",
						Categories.PhysicalServer_SMTP,
						ResString.GetMultilingualString("4d09ba6e-33a4-4f88-b988-eab2d8b2cf6d", "Default Do Not Reply Email Address"),
						ResString.GetMultilingualString("522a9fb5-3d4e-40c7-81fb-8f437772ff7e", "This email address will be used as the default do not reply email address when emails are sent via the system.\r\n\r\nE.g. {0}", "email_name@domain_name.com"),
						new EmailStringRegistryDataType(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						GetSupportOnlyOrClientEditableOptionForHostedSystems(),
						DefaultDoNotReplyEmailAddress);
				});
			}
		}

		public const string DefaultDoNotReplyEmailAddress = "PleaseDoNotReply@wisetechglobal.com";

		internal IRegistryItem MaximumNumberOfMailItemsToSendInABatch
		{
			get
			{
				return GetItem("MaximumNumberOfMailItemsToSendInABatch", delegate
				{
					return new PhysicalServerRegistryItem("MaximumNumberOfMailItemsToSendInABatch", Categories.PhysicalServer_SMTP, ResString.GetMultilingualString("e7b0280d-03e7-4549-8666-01689e0682c7", "Maximum Number Of Mail Items To Send In One Batch"), ResString.GetMultilingualString("c6a20e1b-6ad1-4457-9254-1bcec0550dc4", "The maximum number of emails to send in one batch."), new IntRegistryDataType(1, 1000), GetSupportOnlyOrClientEditableOptionForHostedSystems(), 1000);
				});
			}
		}

		public IRegistryItem AllowEmailsToBeSentFromUsersAddress
		{
			get
			{
				return GetItem("AllowEmailsToBeSentFromUsersAddress", delegate
				{
					return new BooleanRegistryItem(new PhysicalServerRegistryItem("AllowEmailsToBeSentFromUsersAddress", Categories.PhysicalServer_SMTP, ResString.GetMultilingualString("ed8fb175-66f6-4d8f-b7f1-2c677b6305fa", "Allow Emails To Be Sent From User's Address"), ResString.GetMultilingualString("760a1dd3-f5c7-4a40-ab85-1d2713138189", "This flag is to indicate that the from field in outgoing emails can be set to the current user. \r\nIf this flag is not set, the from address will always be the registry item Mailbox Email Address.\r\nOnly enable this option if your SMTP server doesn't check the validity of the from address."), RegistryDataTypes.BoolType, GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue, true));
				});
			}
		}

		internal IRegistryItem SMTPServerTimeout
		{
			get
			{
				return GetItem(
					"SMTPServerTimeout",
					delegate
					{
						return new PhysicalServerRegistryItem(
							"SMTPServerTimeout",
							Categories.PhysicalServer_SMTP,
							ResString.GetMultilingualString("30a257d5-11af-4c41-a3a1-2acb43246da1", "SMTP Server Timeout"),
							ResString.GetMultilingualString("8c65c3e5-41a4-4050-907d-1ebc7016faa1",
								"A period of time, specified in seconds, during which program will wait for an answer from SMTP server for success operation."),
							new IntRegistryDataType(0, 600),
							GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
							30);
					});
			}
		}

		public IRegistryItem SMTPSecureConnection
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

				return GetItem(
					"SMTPSecureConnection",
					delegate
					{
						return new CodePairWithAdditionalEventRegistryItem(
							"SMTPSecureConnection",
							Categories.PhysicalServer_SMTP,
							ResString.GetMultilingualString("56A72E76-1A5B-4039-84D7-DB8DDADFE0A0", "SMTP Server Secure Connection"),
							ResString.GetMultilingualString("0942E31F-D022-48EC-A8CE-2349A91FAFA7", "The type of secure connection to use to connect to the SMTP server"),
							listProvider,
							false, true, new ComboBoxRegistryEditorInfo(listProvider),
							(registryItem) =>
							{
								return Utilities.IsConnectionAndPortChecked(registryItem, SMTPPort);
							},
							RegistryStorageFlags.System,
							GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
							SecureConnectionTypes.None,
							false);
					});
			}
		}

		public IRegistryItem AttachmentEncodingFormat
		{
			get
			{
				return GetItem(
					"AttachmentEncodingFormat",
					delegate
					{
						var defaultListProvider = new CodeDescriptionPairListProvider(() => new AttachmentEncodingFormats());

						return new CodePairRegistryItem(
							"AttachmentEncodingFormat",
							Categories.PhysicalServer_MailOut,
							ResString.GetMultilingualString("66131830-0060-4D26-B7DB-7EE4EC4DBCDA", "Attachment Encoding Format"),
							ResString.GetMultilingualString("68A2B1C8-32CE-4527-9C2F-317A364C8CBB", "This option specifies the encoding format used for attachments"),
							defaultListProvider,
							false, true, new ComboBoxRegistryEditorInfo(defaultListProvider),
							RegistryStorageFlags.System,
							GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
							AttachmentEncodingFormats.RFC2047,
							false);
					});
			}
		}

		internal IRegistryItem MailRetrievalProtocol
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() => new MailRetrievalProtocols());

				return GetItem("MailRetrievalProtocol", () => new CodePairWithAdditionalEventRegistryItem(
					"MailRetrievalProtocol",
					Categories.PhysicalServer_MailIn,
					ResString.GetMultilingualString("6B738D70-9EEA-4C5F-8786-1ABB9403C135", "Mail Retrieval Protocol"),
					ResString.GetMultilingualString("5DD23D0B-EF40-4C03-B796-B37773C0F718", "Protocol used to retrieve incoming mail"),
					listProvider,
					false, true, new ComboBoxRegistryEditorInfo(listProvider),
					(registryItem) =>
					{
						return Utilities.IsProtocolAndPortChecked(registryItem, MailServerPort);
					},
					RegistryStorageFlags.System,
					GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
					MailRetrievalProtocols.POP3,
					false));
			}
		}

		public IRegistryItem MailServer
		{
			get
			{
				return GetItem("MailServer", delegate // Registry item key
				{
					return new PhysicalServerRegistryItem("MailServer", // Registry item key
						Categories.PhysicalServer_MailIn,
						ResString.GetMultilingualString("061e7fe6-7b8e-4b72-bcfd-3f72d421f6de", "Mail Server"),
						ResString.GetMultilingualString("061e7fe6-7b8e-4b72-bcfd-3f72d421f6de", "Mail Server"),
						RegistryDataTypes.StringType,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue)
					{
						OnUpdateAction = MailServer_OnUpdateAction
					};
				});
			}
		}

		void MailServer_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
		{
			RescheduleMailTask("IMS", ZDateTime.UtcNow.AddSeconds((int)RegistryRefresh.FrequencyInSeconds));
		}

		public IRegistryItem MailServerPort
		{
			get
			{
				return GetItem("MailServerPort", delegate
				{
					return new PhysicalServerRegistryItem("MailServerPort", Categories.PhysicalServer_MailIn, ResString.GetMultilingualString("d2f662eb-ab65-4998-8187-9172646627cf", "Mail Server Port"), ResString.GetMultilingualString("d2f662eb-ab65-4998-8187-9172646627cf", "Mail Server Port"), RegistryDataTypes.IntType, GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue, 110);
				});
			}
		}

		public IRegistryItem MailboxUserName
		{
			get
			{
				return GetItem("MailboxUserName", delegate
				{
					return new PhysicalServerRegistryItem("MailboxUserName",
						Categories.PhysicalServer_MailIn,
						ResString.GetMultilingualString("e12181b1-c161-49be-8fdf-3ef707bb4d5d", "Mailbox User Name"),
						ResString.GetMultilingualString("5c3fd46b-8a5a-4638-a402-ed37ebb2a9a0", "When OAuth 2.0 and Graph API are enabled, this account will be used to download email messages from the mail server. When OAuth 2.0 and Graph API are not enabled, this value of this setting will not be used. \r\n\r\nE.g. {0}", "user@example.com"),
						RegistryDataTypes.StringType,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Accessed before login")]
		public IRegistryItem MailboxDisplayName
		{
			get
			{
				return GetItem("MailboxDisplayName", delegate
				{
					return new RegistryItemImplWithDynamicDefaultValue(
						"MailboxDisplayName",
						Categories.PhysicalServer_MailIn,
						ResString.GetMultilingualString("6dc032b5-9a54-48d6-b6fd-d801913575d8", "Mailbox Display Name"),
						ResString.GetMultilingualString("5182a447-aeb6-4000-b69d-dbb488353c6c", "This is the friendly name that will be displayed in the from field when people receive mail sent from the application."),
						RegistryDataTypes.StringType,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
						(Guid companyPK, Guid branchPK, Guid departmentPK) =>
						{
							ICompany currentCompany = null;
							if (companyPK != Guid.Empty)
							{
								currentCompany = (ICompany)new BusinessObjectFactory().Load(ObjectFactory.GetType("IGlbCompany"), companyPK);
							}
							if (currentCompany == null)
							{
								currentCompany = EnvProxy.Instance.CurrentCompany;
							}
							return (currentCompany == null) ? "Mailbox" : currentCompany.Name;
						});
				});
			}
		}

		public IRegistryItem MailboxPassword
		{
			get
			{
				return GetItem("MailboxPassword", delegate
				{
					IRegistryItem result = new PhysicalServerRegistryItem("MailboxPassword", Categories.PhysicalServer_MailIn, ResString.GetMultilingualString("58b73347-a338-4495-b314-0e31e1d20d02", "Mailbox Password"), ResString.GetMultilingualString("58b73347-a338-4495-b314-0e31e1d20d02", "Mailbox Password"), RegistryDataTypes.StringType, GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue | RegistryOptions.IsPasswordVisibleForControllerUser);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		public IRegistryItem MailboxEmailAddress
		{
			get
			{
				return GetItem("MailboxEmailAddress", delegate
				{
					return new PhysicalServerRegistryItem(
						"MailboxEmailAddress",
						Categories.PhysicalServer_MailIn,
						ResString.GetMultilingualString("bfe1573a-45f5-4dc3-bd65-02a0ea63f054", "Mailbox Email Address"),
						ResString.GetMultilingualString("eb0f8b20-677e-4dbc-a4f1-f61d15a2391a", "This email address will be used to authenticate to the POP3/IMAP server. Additionally, this email address will show as the From sender when emails are sent from the system email address. \r\n\r\nE.g. {0}", "pop3@domain_name.com"),
						new EmailStringRegistryDataType(),
						GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue);
				});
			}
		}

		internal IRegistryItem IMAPSecureConnectionType
		{
			get
			{
				return GetItem(
					"IMAPSecureConnection",
					delegate
					{
						var listProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

						return new CodePairWithAdditionalEventRegistryItem(
							"IMAPSecureConnection",
							Categories.PhysicalServer_IMAP,
							ResString.GetMultilingualString("7808D5EA-183F-4ECC-953F-ACEEF8D864AA", "IMAP Server Secure Connection"),
							ResString.GetMultilingualString("F7C7BF26-118A-4229-AF2F-0C4CBDD91441", "The type of secure connection to use to connect to the IMAP mail server"),
							listProvider,
							false, true, new ComboBoxRegistryEditorInfo(listProvider),
							(registryItem) =>
							{
								return Utilities.IsConnectionAndPortChecked(registryItem, MailServerPort);
							},
							RegistryStorageFlags.System,
							GetSupportOnlyOrClientEditableOptionForHostedSystems(),
							SecureConnectionTypes.TLS,
							false);
					});
			}
		}

		internal IRegistryItem POP3SecureConnectionType
		{
			get
			{
				return GetItem(
					"POP3SecureConnection",
					delegate
					{
						var listProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

						return new CodePairWithAdditionalEventRegistryItem(
							"POP3SecureConnection",
							Categories.PhysicalServer_POP3,
							ResString.GetMultilingualString("C4E10D3B-2A4A-448E-866D-920242DDE9FB", "POP3 Server Secure Connection"),
							ResString.GetMultilingualString("DAE11A3A-47B1-4E85-8718-AD240D89BB00", "The type of secure connection to use to connect to the POP3 mail server"),
							listProvider,
							false, true, new ComboBoxRegistryEditorInfo(listProvider),
							(registryItem) =>
							{
								return Utilities.IsConnectionAndPortChecked(registryItem, MailServerPort);
							},
							RegistryStorageFlags.System,
							GetSupportOnlyOrClientEditableOptionForHostedSystems() | RegistryOptions.PreserveTestValue,
							SecureConnectionTypes.None,
							false);
					});
			}
		}

		#region Web

		public IRegistryItem WebBranch
		{
			get
			{
				return GetItem("WebBranch", delegate
				{
					var factory = new BusinessObjectFactory { RefreshEnabled = false };
					factory.SuspendValidation();
					return (IRegistryItem)new GuidRegistryItem(new WebBrachRegistryItemImpl("WebBranch",
						Categories.WebAndVisibility,
						ResString.GetMultilingualString("9992fca6-0fe5-459e-9400-9c0e9be26743", "Web Branch"),
						ResString.GetMultilingualString("e85334b3-fa5a-487a-b824-b298c2093177", @"This is the branch used for the user login context in all web applications.

The branch's company's web address and company name are also used as the hyperlink and tool tip for the Web logo images, respectively."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						EnvProxy.GetAnyBranchWithWebAddress(factory)));
				});
			}
		}

		internal IRegistryItem WebDepartment
		{
			get
			{
				return GetItem("WebDepartment", delegate
				{
					var defaultDepartment = (IDepartment)new BusinessObjectFactory().LoadFromNaturalKey(ObjectFactory.GetType("IGlbDepartment"), GlbDepartmentSchema.GE_Code, "BRN");
					Guid defaultDepartmentPk = defaultDepartment != null ? defaultDepartment.PK : Guid.Empty;

					return (IRegistryItem)new GuidRegistryItem("WebDepartment",
						Categories.WebAndVisibility,
						ResString.GetMultilingualString("ce69b296-7ce4-431e-80a5-9a5fd546d0c8", "Web Department"),
						ResString.GetMultilingualString("57478474-7295-4d6e-9c92-0ee77c9c52f2", @"This is the department used for the user login context in all web applications."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultDepartmentPk);
				});
			}
		}

		internal IRegistryItem WebHyperlinksEnabled
		{
			get
			{
				return GetItem("WebHyperlinksEnabled", delegate
				{
					return (IRegistryItem)new BooleanRegistryItem(
						"WebHyperlinksEnabled",
						Categories.WebAndVisibility,
						ResString.GetMultilingualString(
							"98ebf9ae-49ea-4b7e-8862-3e324843b91d",
							"Web Hyperlinks Enabled"),
						ResString.GetMultilingualString(
							"3860f190-dbb3-4657-80a0-f054db553779",
							"When copying hyperlinks to the clipboard, copy plain web hyperlinks ({0}) instead of rich CargoWise hyperlinks ({1})",
							"https:",
							"edient:"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: false);
				});
			}
		}

		public IRegistryItem AddDatabaseInfoToEdientUrls
		{
			get
			{
				return GetItem("AddDatabaseInfoToEdientUrls", delegate
				{
					return (IRegistryItem)new BooleanRegistryItem(
						"AddDatabaseInfoToEdientUrls",
						Categories.System_Framework,
						(NoResString)"Add database info to hyperlinks",
						(NoResString)"When copying hyperlinks to the clipboard, add database name and database server name to the link",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false);
				});
			}
		}

		public StringArrayRegistryItem ResourceStringUsageTrustedDomains
		{
			get
			{
				return GetItem("ResourceStringUsageTrustedDomains", delegate
				{
					var result = new StringArrayRegistryItem(
						"ResourceStringUsageTrustedDomains",
						Categories.WebAndVisibility,
						(NoResString)"Resource String Usage Trusted Domains",
						(NoResString)"Resource String Usage Trusted Domains.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		#endregion

		internal IRegistryItem MaxRecommendedNumberOfRecordsToShowInDisplayGrids
		{
			get
			{
				return GetItem("MaxRecommendedNumberOfRecordsToShowInDisplayGrids", delegate
				{
					return new PhysicalServerRegistryItem("MaxRecommendedNumberOfRecordsToShowInDisplayGrids", Categories.PhysicalServer_DisplayGrid, ResString.GetMultilingualString("05d64c5e-5146-4d3c-ad5e-ab5bbe058d88", "Max Recommended No. of Records"), ResString.GetMultilingualString("c3c82949-7490-46fb-8290-ac9e370643e3", "A warning icon will appear on a Display Grid if a search returns more than the maximum recommended number of results."), new IntRegistryDataType(1, 1000), RegistryOptions.IsOnlyEditableBySupportIfHosted, 100);
				});
			}
		}

		internal BooleanRegistryItem ShowExactRowCountOnExcessResult
		{
			get
			{
				return GetItem("ShowExactRowCountOnExcessResult", delegate
				{
					return new BooleanRegistryItem(
							"ShowExactRowCountOnExcessResult",
							Categories.PhysicalServer_DisplayGrid,
							ResString.GetMultilingualString("5cf6fec6-c591-4a51-b575-f35c083d8b66", "Show Exact Row Count on Excess Result"),
							ResString.GetMultilingualString("2a5171f9-4464-4542-8211-a5541803e278", "Indicates whether the application will return the exact row count when running a large filter grid query."),
							RegistryStorageFlags.System,
							RegistryOptions.Default | RegistryOptions.IsOnlyEditableBySupportIfHosted,
							false);
				});
			}
		}

		internal IRegistryItem RunSearchOnEnteringAModule
		{
			get
			{
				return GetItem("RunSearchOnEnteringAModule", delegate
				{
					return new BooleanRegistryItem(new PhysicalServerRegistryItem("RunSearchOnEnteringAModule", Categories.PhysicalServer_DisplayGrid, ResString.GetMultilingualString("bbd94a28-0275-4cd7-86d7-0b89b0605b1c", "Run Search On Entering A Module"), ResString.GetMultilingualString("2d9dd196-38f5-4418-a6f4-503e7f34a821", "Setting this to 'No' can improve the speed on entering a module."), RegistryDataTypes.BoolType, RegistryOptions.IsOnlyEditableBySupportIfHosted, false));
				});
			}
		}

		internal IRegistryItem AutoRunSearchFromFindBox
		{
			get
			{
				return GetItem("AutoRunSearchFromFindBox", delegate
				{
					return new BooleanRegistryItem(new PhysicalServerRegistryItem("AutoRunSearchFromFindBox", Categories.PhysicalServer_DisplayGrid, ResString.GetMultilingualString("feeebaf6-9934-4919-9c92-06decb54e9d7", "Auto Run Search from Find Box"), ResString.GetMultilingualString("df93a908-a86d-42fe-b408-8b4ea2b4a75e", "Setting this to 'Yes' to find an object from a find box when F4 or find button is pressed by automatically executing the find and placing focus on the first line."), RegistryDataTypes.BoolType, RegistryOptions.IsOnlyEditableBySupportIfHosted, true));
				});
			}
		}

		internal IRegistryItem RunSelectTopNAsRowNumberQuery
		{
			get
			{
				return GetItem("RunSelectTopNAsRowNumberQuery", delegate
				{
					return new BooleanRegistryItem(new PhysicalServerRegistryItem("RunSelectTopNAsRowNumberQuery"
						, Categories.PhysicalServer
						, ResString.GetMultilingualString("185bf02e-bbae-4f76-b4a7-66bdd1f59a67", "Run 'Top N' Select Statements alternate syntax?")
						, ResString.GetMultilingualString("fdd8eddd-b56a-44de-9df1-7a8dd277dc9d", "This option allows queries to be generated using the Row_Number syntax instead of Top N and may alleviate some performance problems. This feature should be tested before turning on on permanent basis.")
						, RegistryDataTypes.BoolType
						, RegistryOptions.IsOnlyEditableBySupportIfHosted
						, EntityFrameworkRegistryDefaults.RunSelectTopNAsRowNumberQuery
						));
				});
			}
		}

#if DEBUG
		public
#else
		internal
#endif
		BooleanRegistryItem ReportCrossThreadFactoryAccess
		{
			get
			{
				return GetItem("ReportCrossThreadFactoryAccess",
					() => new BooleanRegistryItem(
						new PhysicalServerRegistryItem("ReportCrossThreadFactoryAccess",
							Categories.PhysicalServer,
							(NoResString)"Report Cross Thread Factory Access", // For developers only
							(NoResString)"The business object factory is single threaded. This option detects if a factory is accessed by multiple threads on an ongoing basis - an example would be: thread 1, thread 2, thread 1. Sequential access is not reported - an example would be: thread 1, thread 2, thread 3.", // For developers only
							RegistryDataTypes.BoolType, RegistryOptions.IsOnlyForDevelopers,
							EntityFrameworkRegistryDefaults.ReportCrossThreadFactoryAccess
							)));
			}
		}

		internal IRegistryItem AllowHostedClientAccessToEmailSettings
		{
			get
			{
				return GetItem("AllowHostedClientWriteAccessToEmailSettings", delegate
				{
					return new BooleanRegistryItem(new PhysicalServerRegistryItem("AllowHostedClientWriteAccessToEmailSettings", Categories.PhysicalServer,
																(NoResString)"Allow Hosted Client Write Access To POP3/SMTP?", // For support only
																(NoResString)"Setting this option to YES will allow hosted clients to change their POP3/SMTP settings.", // For support only
																RegistryDataTypes.BoolType,
																EnvProxy.IsHostedWithCargowise ? (RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForSupport) : RegistryOptions.IsHidden, true));
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public DateTimeRegistryItem EnterpriseCDDate
		{
			get
			{
				return GetItem("EnterpriseCDDate", delegate
				{
					return new DateTimeRegistryItem("EnterpriseCDDate", Categories.PhysicalServer, (NoResString)"CD Date", (NoResString)"The version date of the the application installation CD. Release Notes prior to this date will not be displayed.", RegistryStorageFlags.System, RegistryOptions.NotLogged | RegistryOptions.IsOnlyForDevelopers, DateTime.MinValue);
				});
			}
		}

		#endregion

		#endregion

		#region ProductivityWise (working title)

		internal BooleanRegistryItem ProductivityWiseModeEnabled => GetItem("ProductivityWiseModeEnabled", () => new BooleanRegistryItem(
			name: "ProductivityWiseModeEnabled",
			category: Categories.System_Workflow,
			caption: (NoResString)"ProductivityWise Mode Enabled", // Support-only registry item
			hint: (NoResString)"Causes this application to be shown in ProductivityWise (working title) mode. This will hide all freight forwarding modules and associated functionality, resulting in a cut-down system that can be used for Projects, Work Items and Incident Management, supported by Workflow, PAVE and other architecture features.", // Support-only registry item
			storage: RegistryStorageFlags.System,
			options: RegistryOptions.IsOnlyForSupport,
			defaultValue: BrandingFactory.Instance is ProductivityWiseBranding
			));

#if DEBUG
		public IDisposable SetTemporaryProductivityWiseModeEnabledForTest(bool enabled)
		{
			return ProductivityWiseModeEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled);
		}
#endif

		#endregion

		#region Number Fountain

		public StringRegistryItem MultiSearchSeparator
		{
			get
			{
				return GetItem("MultiSearchSeparator", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"MultiSearchSeparator",
						Categories.System_NumberFountain,
						ResString.GetMultilingualString("c1aaacc3-ca63-49bb-acd4-be68b89d7471", "Multi-Search Separator"),
						ResString.GetMultilingualString("df464961-62e3-4a15-a079-1be7b1b06088", "Separator character used to separate multiple values when using Multi-Search in a filter. (Set to the empty string to disable.)"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						",");
					return result;
				});
			}
		}

		#endregion // Number Fountain

		#region Upgrade

		internal IntRegistryItem UpgradeObserverMaxWaitInSeconds
		{
			get
			{
				return GetItem("UpgradeObserverMaxWaitInSeconds", delegate
				{
					return new IntRegistryItem(
						name: "UpgradeObserverMaxWaitInSeconds",
						category: Categories.System_Upgrade,
						caption: ResString.GetMultilingualString("C1C896BC-767C-4B09-8BF1-BEE863EF5C43", "Observer max wait in seconds"),
						hint: ResString.GetMultilingualString("B502C76D-705A-4C5C-BCBF-364EB2E25F9C", "The wait time (in seconds) that the on-line upgrade will wait when it is blocked by user process. If the operation is blocked for the time specified, the blocker will be killed by upgrade process.\r\n\r\nNote:\r\n\t0 Don't wait\r\n\t-1 Don't use observer"),
						storage: RegistryStorageFlags.System,
						options: EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController,
						defaultValue: 60,
						minValue: -1,
						maxValue: 3600
						);
				});
			}
		}

		#endregion // Upgrade

		#endregion // System

		#region External Validation Service

		public BooleanRegistryItem EnableExternalValidationService
		{
			get
			{
				return GetItem("EnableExternalValidationService", delegate
				{
					return new BooleanRegistryItem(
						"EnableExternalValidationService",
						Categories.Organizations_ExternalValidationService,
						ResString.GetMultilingualString("C197E602-2CC0-4235-828A-093098BC1DB8", "Enable External Validation Service"),
						ResString.GetMultilingualString("7577ED54-4881-4FB9-93F1-CF3ABC11D512", "Enables or disables external validation service."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public ServiceUrlRegistryItem ExternalValidationServiceUrl
		{
			get
			{
				return GetItem("ExternalValidationServiceUrl", delegate
				{
					return new ServiceUrlRegistryItem(
						"ExternalValidationServiceUrl",
						Categories.Organizations_ExternalValidationService,
						ResString.GetMultilingualString("28634830-A8E6-4969-A459-3AFA66E2ABCB", "External Validation Service URL"),
						ResString.GetMultilingualString("682066CD-2F6E-46D6-B2C8-C50392742CF7", "Configures the external validation service URL."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		public IntRegistryItem ExternalValidationServiceTimeout
		{
			get
			{
				return GetItem("ExternalValidationServiceTimeout", delegate
				{
					return new IntRegistryItem(
						"ExternalValidationServiceTimeout",
						Categories.Organizations_ExternalValidationService,
						ResString.GetMultilingualString("86DE57EA-BC00-42C9-9014-90632FB46248", "External Validation Service Timeout"),
						ResString.GetMultilingualString("F99BD00D-A245-488D-B817-EF0B5DABE141", "Configures maximum allowed time length in seconds for waiting till external validation process completes and web service returns result. 0 means no time limit."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						60);
				}
				);
			}
		}

		#endregion

		#region Address Validation

		public IntRegistryItem AddressValidationWebServiceTimeout
		{
			get
			{
				return GetItem("AddressValidationWebServiceTimeout", delegate
				{
					return new IntRegistryItem("AddressValidationWebServiceTimeout",
						Categories.Organizations_AddressValidationService,
						(NoResString)"Address Validation Web Service Timeout",// EDISupport Only Registry Item
						(NoResString)"Address Validation Web Service Timeout (In Seconds)",// EDISupport Only Registry Item
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						10)
					{ DataType = new IntRegistryDataType(1, 30) };
				});
			}
		}

		public BooleanRegistryItem EnableAddressValidationWebService
		{
			get
			{
				return GetItem("EnableAddressValidationWebService", delegate
				{
					return new BooleanRegistryItem(
						"EnableAddressValidationWebService",
						Categories.Organizations_AddressValidationService,
						(NoResString)"Enable Address Validation Web Service",// EDISupport Only Registry Item
						(NoResString)"Enable Address Validation Web Service", RegistryStorageFlags.System | RegistryStorageFlags.Company,// EDISupport Only Registry Item
						RegistryOptions.IsOnlyForSupport,
						!Globals.IsTest);
				});
			}
		}

		#endregion

		#region Freight

		#region Freight Registry Items

		#region Freight/AWB Registry Items

		internal IRegistryItem HAWBDimensionsDefault
		{
			get
			{
				return GetItem("HAWBDimensionsDefault", delegate
				{
					return new CodePairRegistryItem("HAWBDimensionsDefault",
						Categories.Freight_AWB_HAWB,
						ResString.GetMultilingualString("c7d05e7f-e632-4893-8b71-371d68cd40f7", "HAWB Dimensions Default"),
						ResString.GetMultilingualString("f3f7fd12-b88a-43ad-907c-b10607081557", "This governs how the HAWB volume/dimensions are defaulted"),
						OLookUpEditType.AWBDimensions, RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.AWB.Dimensions.DEF);
				});
			}
		}

		internal IRegistryItem MAWBDimensionsDefault
		{
			get
			{
				return GetItem("MAWBDimensionsDefault", delegate
				{
					return new CodePairRegistryItem("MAWBDimensionsDefault",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("65375868-d77f-4360-b5bf-5a71704bd4b1", "MAWB Dimensions Default"),
						ResString.GetMultilingualString("2aeeaf93-9f43-49ea-b87f-dfff1c63b053", "This governs how the MAWB volume/dimensions are defaulted"),
						OLookUpEditType.AWBDimensions, RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.AWB.Dimensions.DEF);
				});
			}
		}

		internal IRegistryItem ShipperAddressDefaultsTo
		{
			get
			{
				return GetItem("ShipperAddressDefaultsTo", delegate
				{
					return new CodePairRegistryItem("ShipperAddressDefaultsTo",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("ed750b56-74cc-4e86-b28f-edfa4c5cce86", "Shipper Address Defaults To"),
						ResString.GetMultilingualString("f5735564-b0b1-412e-9c19-a5ad26056464", "This governs which address appears in the Shipper address on the AWB by default"),
						new CodeDescriptionPairListProvider(() => new EXDocumentAddressPreferenceCodeDescriptionPairList()),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						OrgConstants.AddressType.Documentary);
				});
			}
		}

		internal IRegistryItem ConsigneeAddressDefaultsTo
		{
			get
			{
				return GetItem("ConsigneeAddressDefaultsTo", delegate
				{
					return new CodePairRegistryItem("ConsigneeAddressDefaultsTo",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("bb02b8c0-21d3-4232-8461-0991d00435c9", "Consignee Address Defaults To"),
						ResString.GetMultilingualString("5c2e6d26-b469-41be-9e55-6da987ed7401", "This governs which address appears in the Consignee address on the AWB by default"),
						new CodeDescriptionPairListProvider(() => new IMDocumentAddressPreferenceCodeDescriptionPairList()),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						OrgConstants.AddressType.Documentary);
				});
			}
		}

		internal IRegistryItem IssuingCarrierAgentName
		{
			get
			{
				return GetItem("IssuingCarrierAgentName", delegate
				{
					return new StringRegistryItem("IssuingCarrierAgentName",
						Categories.Freight_AWB_MAWB_IssuingCarrierAgent,
						ResString.GetMultilingualString("2553b50f-0c8b-41e2-9274-89aa406316bc", "Name"), ResString.GetMultilingualString("66f069d7-55e6-4e3e-8e9a-c438b0fd4c36", "Enter the Issuing Carrier Agent Name"), RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem IssuingCarrierAgentCity
		{
			get
			{
				return GetItem("IssuingCarrierAgentCity", delegate
				{
					return new StringRegistryItem("IssuingCarrierAgentCity",
						Categories.Freight_AWB_MAWB_IssuingCarrierAgent,
						ResString.GetMultilingualString("4382c866-feb0-4018-bf51-ec5ff0fc6688", "City"), ResString.GetMultilingualString("6230951c-a5ed-46be-8bf2-b9b02971e353", "Enter the Issuing Carrier Agent City"), RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem IssuingCarrierAgentIATACode
		{
			get
			{
				return GetItem("IssuingCarrierAgentIATACode", delegate
				{
					return new StringRegistryItem("IssuingCarrierAgentIATACode",
						Categories.Freight_AWB_MAWB_IssuingCarrierAgent,
						ResString.GetMultilingualString("a3812482-baba-4fc4-a5d1-6bee1bcf47dc", "IATA Code"), ResString.GetMultilingualString("77d87427-72af-4903-8838-1d53c6ee1ce4", "Enter the Issuing Carrier Agent IATA Code"), RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem IssuingCarrierAgentAccountNumber
		{
			get
			{
				return GetItem("IssuingCarrierAgentAccountNumber", delegate
				{
					return new StringRegistryItem("IssuingCarrierAgentAccountNumber",
						Categories.Freight_AWB_MAWB_IssuingCarrierAgent,
						ResString.GetMultilingualString("4d182334-db2c-4c56-94a3-98e162d3503e", "Account Number"), ResString.GetMultilingualString("79f4abdf-8f44-439f-bd6e-664a0751aaab", "Enter the Issuing Carrier Agent Account Number"), RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem AllowAutoCalculationOfTax
		{
			get
			{
				return GetItem("AllowAutoCalculationOfTax", delegate
				{
					return new BooleanRegistryItem("AllowAutoCalculationOfTax",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("02b8384d-f574-49f5-9b33-b0096ef1aef2", @"Allow Auto Calculation of Tax for domestic AWBs"),
						ResString.GetMultilingualString("328e326d-f09a-4da9-9031-fb4bf0abd722", "This determines whether tax is automatically calculated for domestic AWBs."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem AllowAsAgreed
		{
			get
			{
				return GetItem("AllowAsAgreed", delegate
				{
					return new BooleanRegistryItem("AllowAsAgreed",
									Categories.Freight_AWB_MAWB_AsAgreed,
									ResString.GetMultilingualString("63ade96c-e454-4963-9e47-5397f5ca94cf", @"Allow selection of As Agreed on MAWB"),
									ResString.GetMultilingualString("f0b221f6-d720-4356-8095-1812d1c10d37", "If this is checked, the As Agreed can be selected for MAWBs."),
									RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem SelectFHLByDefault
		{
			get
			{
				return GetItem("SelectFHLByDefault", delegate
				{
					return new BooleanRegistryItem("SelectFHLByDefault",
									Categories.Freight_AWB_MAWB,
									ResString.GetMultilingualString("52031463-861f-4c55-aa21-f57da70d779a", @"Select FHL By Default on MAWB"),
									ResString.GetMultilingualString("0bb249ae-bed0-40aa-8dd9-258392e86c6d", "Set this Registry to 'Yes' for the 'Send FHL' option to be checked by default when the 'Send FWB' option is checked, regardless of origin/destination for Air Consols that are not direct.\r\n\r\nIf the Registry is not overridden, the 'Send FHL' option will be checked by default when the 'Send FWB' option is checked, for Air Consols that are not direct, originating/destined to/transhipping countries/regions in the prescribed list.\r\n\r\nRefer to the warning message visible on the Send FWB message for countries/regions in the prescribed list."),
									RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		internal IRegistryItem MAWBPaperType
		{
			get
			{
				return GetItem("MAWBPaperType", delegate
				{
					return new CodePairRegistryItem("MAWBPaperType",
						Categories.Freight_AWB_MAWB_DotMatrix,
						ResString.GetMultilingualString("f53e6e42-916d-4ae4-9dfa-e609e8b2d4de", "Paper Type"), ResString.GetMultilingualString("445b0793-5172-473d-b329-f6f1dbe4cfd0", "Select the Master Air Waybill Paper Type"),
						OLookUpEditType.AirWaybillPaperTypes, RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.AWB.PaperTypes.Iata);
				});
			}
		}

		internal IRegistryItem HAWBPaperType
		{
			get
			{
				return GetItem("HAWBPaperType", delegate
				{
					return new CodePairRegistryItem("HAWBPaperType",
						Categories.Freight_AWB_HAWB_DotMatrix,
						ResString.GetMultilingualString("f53e6e42-916d-4ae4-9dfa-e609e8b2d4de", "Paper Type"), ResString.GetMultilingualString("ee9b4305-6113-47fb-8ae0-af5e0d3629d7", "Select the House AWB Paper Type"),
						OLookUpEditType.AirWaybillPaperTypes, RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.AWB.PaperTypes.Iata);
				});
			}
		}

		internal IRegistryItem PrintAsAgreedOnFirstSetHAWB
		{
			get
			{
				return GetItem("PrintAsAgreedOnFirstSet", delegate
				{
					return new CodePairRegistryItem("PrintAsAgreedOnFirstSet",
						Categories.Freight_AWB_HAWB_AsAgreed,
						ResString.GetMultilingualString("0b986b7a-d507-4bae-88c1-474b371ee2d6", "Print '{0}' On First Set", "AsAgreed"),
						ResString.GetMultilingualString("b14fd348-8e18-4d2f-8d75-2cec3e5fd30e", "This determines whether '{0}' replaces the charges on the first set of House AWB documents by default", "AsAgreed"),
						OLookUpEditType.AWBAsAgreedFirstSetType,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.AWB.AsAgreedTypes.Codes.None);
				});
			}
		}

		internal IRegistryItem PrintAsAgreedOnSecondSetHAWB
		{
			get
			{
				return GetItem("PrintAsAgreedOnSecondSet", delegate
				{
					return new CodePairRegistryItem("PrintAsAgreedOnSecondSet",
						Categories.Freight_AWB_HAWB_AsAgreed,
						ResString.GetMultilingualString("0a898ff2-e90c-46bc-b4dd-dde353a63125", "Print '{0}' On Second Set", "AsAgreed"),
						ResString.GetMultilingualString("460a09f9-24fe-4c26-a31e-09729867744c", "This determines whether '{0}' replaces the charges on the second set of House AWB documents by default", "AsAgreed"),
						OLookUpEditType.AWBAsAgreedSecondSetType,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.AWB.AsAgreedTypes.Codes.None);
				});
			}
		}

		internal IRegistryItem PrintAsAgreedOnFirstSetMAWB
		{
			get
			{
				return GetItem("PrintAsAgreedOnFirstSetMAWB", delegate
				{
					return new CodePairRegistryItem("PrintAsAgreedOnFirstSetMAWB",
						Categories.Freight_AWB_MAWB_AsAgreed,
						ResString.GetMultilingualString("0b986b7a-d507-4bae-88c1-474b371ee2d6", "Print '{0}' On First Set", "AsAgreed"),
						ResString.GetMultilingualString("b14fd348-8e18-4d2f-8d75-2cec3e5fd15e", "This determines whether '{0}' replaces the charges on the first set of Master AWB documents by default", "AsAgreed"),
						OLookUpEditType.AWBAsAgreedFirstSetType,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.AWB.AsAgreedTypes.Codes.None);
				});
			}
		}

		internal IRegistryItem PrintAsAgreedOnSecondSetMAWB
		{
			get
			{
				return GetItem("PrintAsAgreedOnSecondSetMAWB", delegate
				{
					return new CodePairRegistryItem("PrintAsAgreedOnSecondSetMAWB",
						Categories.Freight_AWB_MAWB_AsAgreed,
						ResString.GetMultilingualString("0a898ff2-e90c-46bc-b4dd-dde353a63125", "Print '{0}' On Second Set", "AsAgreed"),
						ResString.GetMultilingualString("460a09f9-24fe-4c26-a31e-09729867745d", "This determines whether '{0}' replaces the charges on the second set of Master AWB documents by default", "AsAgreed"),
						OLookUpEditType.AWBAsAgreedSecondSetType,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.AWB.AsAgreedTypes.Codes.None);
				});
			}
		}

		internal IRegistryItem ShowChargeCodeForOtherChargesInHAWBScreen
		{
			get
			{
				return GetItem("ShowChargeCodeForOtherChargesInHAWBScreen", delegate
				{
					return new BooleanRegistryItem("ShowChargeCodeForOtherChargesInHAWBScreen",
						Categories.Freight_AWB_HAWB, ResString.GetMultilingualString("9870df3f-bb22-444a-b543-a8ad6c7cc968", "Show IATA code field in other charges grid on the HAWB form"),
						ResString.GetMultilingualString("a640af26-676e-46a5-85a8-bcf756727045", "When set up to 'Yes' the IATA code column will be available to select an IATA code in the other charges grid on the HAWB form and only the IATA code description will print on the HAWB.\r\n\r\nWhen set up to 'No' the IATA code column will not be available to select an IATA code in the other charges grid on the HAWB form, the IATA code description will print on the HAWB."), RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		internal IRegistryItem AllowShortGoodsDescriptionOverrideforFHL
		{
			get
			{
				return GetItem("AllowShortGoodsDescriptionOverrideforFHL", delegate
				{
					return new BooleanRegistryItem("AllowShortGoodsDescriptionOverrideforFHL",
						Categories.Freight_AWB_HAWB, ResString.GetMultilingualString("bb06346d-01a2-4f82-b37e-423398ad116a", "Short Goods Description Override for FHL purposes"),
						ResString.GetMultilingualString("7086a2e7-4c79-46d0-8f9b-adc61d3d15f8", "Change the default value of this registry if you want to include first 15 characters of short goods description (with fallback to the long description) to the AWB form for FHL/AMS messaging purposes (this field will not be printed on HAWB document)."), RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Freight/PRA Messaging Registry Items

		internal IRegistryItem PRAMessagingSeaFreightDangerousGoodsContact
		{
			get
			{
				return GetItem("PRAMessagingSeaFreightDangerousGoodsContact", delegate
				{
					IRegistryItem result = new GuidRegistryItem("PRAMessagingSeaFreightDangerousGoodsContact",
						Categories.Freight_PRAMessaging, ResString.GetMultilingualString("d5fab2aa-627f-4ecf-8de4-f66c4d77fa43", "Sea Freight Dangerous Goods Contact"),
						ResString.GetMultilingualString("8168a3f0-55e2-4c44-b019-04d201a82183", "A technical contact the shipping companies can contact in the event of a Dangerous Goods Emergency."),
						new ContactRegistryEditorInfo(), RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, Guid.Empty);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AcknowledgementEmailGroup
		{
			get
			{
				return GetItem("AcknowledgementEmailGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem("AcknowledgementEmailGroup",
						Categories.Freight_PRAMessaging, ResString.GetMultilingualString("bf172e1a-dadd-4833-b93a-f9e54d0af150", "Acknowledgement Email Group"), null,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, Guid.Empty);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AcknowledgementEmailMode
		{
			get
			{
				return GetItem("AcknowledgementEmailMode", delegate
				{
					IRegistryItem result = new CodePairRegistryItem("AcknowledgementEmailMode",
						Categories.Freight_PRAMessaging, ResString.GetMultilingualString("3e9294b5-f7ad-4ad8-9ed3-cc99f377758e", "Acknowledgement Email Mode"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ImpedimentEmailGroup
		{
			get
			{
				return GetItem("ImpedimentEmailGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem("ImpedimentEmailGroup",
						Categories.Freight_PRAMessaging, ResString.GetMultilingualString("0b3cd392-f859-4e33-9248-d8b1e5552dcd", "Impediment Email Group"), null,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, Guid.Empty);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ImpedimentEmailMode
		{
			get
			{
				return GetItem("ImpedimentEmailMode", delegate
				{
					IRegistryItem result = new CodePairRegistryItem("ImpedimentEmailMode", Categories.Freight_PRAMessaging,
						ResString.GetMultilingualString("fb80c144-f86e-4d17-b0d2-372773496e1d", "Impediment Email Mode"), null, OLookUpEditType.EmailTo, false, RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ErrorEmailGroup
		{
			get
			{
				return GetItem("ErrorEmailGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem("ErrorEmailGroup",
						Categories.Freight_PRAMessaging, ResString.GetMultilingualString("517b7ca1-3dd5-4153-acd7-e12e5f191185", "Error Email Group"), null,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, Guid.Empty);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ErrorEmailMode
		{
			get
			{
				return GetItem("ErrorEmailMode", delegate
				{
					IRegistryItem result = new CodePairRegistryItem("ErrorEmailMode",
						Categories.Freight_PRAMessaging, ResString.GetMultilingualString("9bb7b7f4-9222-4e39-a5a9-0f7f0a7d720c", "Error Email Mode"), null, OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem PRATestMode
		{
			get
			{
				return GetItem("PRATestMode", delegate
				{
					IRegistryItem result = new BooleanRegistryItem("PRATestMode",
						Categories.Freight_PRAMessaging,
						(NoResString)"Test Mode",
						null, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport, false);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		#endregion

		internal IRegistryItem ShipmentScreenLayout
		{
			get
			{
				return GetItem("ShipmentScreenLayout", delegate
				{
					return new CodePairRegistryItem("ShipmentScreenLayout", Categories.Freight_Shipment, ResString.GetMultilingualString("2f6638fd-c4a4-4d89-9c31-39e2babc7db9", "Shipment Screen Layout"),
						ResString.GetMultilingualString("e47c2a06-cd06-43e1-b481-828642a0da61", "Configure the order in which Consignor & Consignee are displayed"), OLookUpEditType.ShipmentScreenLayout, false, true,
						new ComboBoxRegistryEditorInfo(OLookUpEditType.ShipmentScreenLayout, true), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.ShipmentScreenOptions.Consignor, false);
				});
			}
		}

		public CodePairRegistryItem ShipmentOSMGSecurityLevel
		{
			get
			{
				return GetItem("ShipmentOSMGSecurityLevel", delegate
				{
					return new CodePairRegistryItem("ShipmentOSMGSecurityLevel",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("54ef13b0-5340-49aa-8566-6bbdcea84b75", "Org. Security Groups - Security Level"),
						ResString.GetMultilingualString("ba83d4c1-3fd7-4237-a033-fd8dcd92f7a3", @"Configure the level of security to apply to Organization Security Groups.
This is used for users that have the security right '{0}' denied.
When the default value '{1}' is selected, these users will need to be part of the security group of at least one primary organization involved in the shipment.
When the value is set to '{2}', these users will need to be part of the security group of all primary organizations on the shipment. Task and staff assignments will be ignored.",
							Constants.CRMSecurityCaptions.IgnoreOSMG,
							Constants.OSMGSecurityLevels.Standard,
							Constants.OSMGSecurityLevels.Enhanced),
						OLookUpEditType.OSMGSecurityLevel,
						allowBlank: false,
						validate: true,
						new ComboBoxRegistryEditorInfo(OLookUpEditType.OSMGSecurityLevel, true),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.OSMGSecurityLevels.Standard,
						useDefaultDefaultValue: false);
				});
			}
		}

		internal IRegistryItem FreightWeightUnit
		{
			get
			{
				return GetItem("FreightWeightUnit", delegate
				{
					return new CodePairRegistryItem("FreightWeightUnit", Categories.Freight_Shipment_Packages, ResString.GetMultilingualString("0ae04a99-2a05-4c8e-b5d0-30f417f9a134", "Freight Weight Unit"), ResString.GetMultilingualString("1efbdb69-3d4c-4db1-be7f-6260ec1ed2b6", "The Weight Unit to use for Freight."), OLookUpEditType.Weight, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, "KG");
				});
			}
		}

		internal IRegistryItem FreightVolumeUnit
		{
			get
			{
				return GetItem("FreightVolumeUnit", delegate
				{
					return new CodePairRegistryItem("FreightVolumeUnit", Categories.Freight_Shipment_Packages, ResString.GetMultilingualString("87074ca4-d9dc-4d16-b65a-9076d89273e7", "Freight Volume Unit"), ResString.GetMultilingualString("25d69869-0e47-4e69-bef4-66f6b92585b6", "The Volume Unit to use for Freight."), OLookUpEditType.Volume, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, "M3");
				});
			}
		}

		internal IRegistryItem ServiceLevel
		{
			get
			{
				return GetItem("ServiceLevel", delegate
				{
					Guid defaultServiceLevel = new Guid("DE20E49F-224F-4EA0-9523-E07B26CB4841"); //STD
					IRegistryItem result = new GuidRegistryItem("ServiceLevel", Categories.Freight_Shipment, ResString.GetMultilingualString("94452350-d045-445d-bbb0-d8323f0f3024", "Service Level"), ResString.GetMultilingualString("94452350-d045-445d-bbb0-d8323f0f3024", "Service Level"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, defaultServiceLevel);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefServiceLevel);
					return result;
				});
			}
		}

		[SuppressMessage("Style", "CW1161:Do not use constant string literals - use resource strings instead.", Justification = "Database query")]
		internal IRegistryItem CommodityCode
		{
			get
			{
				return GetItem("CommodityCode", delegate
				{
					object defaultCommodityCodeObj = Db.Connection.ExecuteScalar(
						"select " + RefCommodityCodeSchema.PK.Name +
						" from " + RefCommodityCodeSchema.Constants.SqlSchemaName + "." + RefCommodityCodeSchema.Constants.TableName +
						" where " + RefCommodityCodeSchema.RH_Code.Name + " = @code",
						cmd => cmd.AddParameterBasedOnDbColumn("@code", "GEN", RefCommodityCodeSchema.RH_Code));// rolling back 15543
					Guid defaultCommodityCode = defaultCommodityCodeObj != null ? (Guid)defaultCommodityCodeObj : Guid.Empty;
					IRegistryItem result = new GuidRegistryItem("CommodityCode", Categories.Freight_PackLine, ResString.GetMultilingualString("80bec146-87b8-4e7a-bb25-270367bd3e62", "Commodity Code"), ResString.GetMultilingualString("80bec146-87b8-4e7a-bb25-270367bd3e62", "Commodity Code"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, defaultCommodityCode);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefCommodityCode);
					result.Options = DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsValueOptional;
					return result;
				});
			}
		}

		internal IRegistryItem ConsolPaymentTerm
		{
			get
			{
				return GetItem("ConsolPaymentTerm", delegate
				{
					return new CodePairRegistryItem("ConsolPaymentTerm", Categories.Freight, ResString.GetMultilingualString("1352741f-15e5-43e6-a0a5-9b96144b0e57", "Payment Term"), ResString.GetMultilingualString("f9676027-d3ab-4ec6-852c-525f9f033c36", "Default Payment Term for Consols"), OLookUpEditType.PaymentType, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem IsExpress
		{
			get
			{
				return GetItem("IsExpress", delegate
				{
					return new BooleanRegistryItem("IsExpress", Categories.Freight, ResString.GetMultilingualString("46638629-5875-460f-99e7-557884013560", "Is Express Courier"), ResString.GetMultilingualString("971a3729-439b-4160-be5f-b620bb0b6fda", "Is this organization an Express Courier?"), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		internal IRegistryItem AllowManualShipmentNumberEntry
		{
			get
			{
				return GetItem("ManualShipmentNumbers", delegate
				{
					return new BooleanRegistryItem("ManualShipmentNumbers", Categories.Freight_Shipment, ResString.GetMultilingualString("30583abc-fd52-4f84-b062-3cfc3546827d", "Allow Manual Shipment Number Entry"), ResString.GetMultilingualString("512df606-a634-4d10-b7ee-c4ffe5bce1d2", "Do you wish to allow forwarding shipment numbers to be set manually?"), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		internal IRegistryItem ManifestClientID
		{
			get
			{
				return GetItem("ManifestClientID", delegate
				{
					return new StringRegistryItem(
						"ManifestClientID",
						Categories.Customs_Australia,
						ResString.GetMultilingualString("c199c569-613a-4d13-95cc-f68789b11c57", "Sea Cargo Manifest Client ID"),
						ResString.GetMultilingualString("344294e8-3e7b-45c1-a999-9d8c73a48416", "This ID is used for shipments where the arrival consol's receiving forwarder does not have a Local Manifest ID custom code specified, and the receiving forwarder is the current branch's organization proxy, or the current company's organization proxy."),
						new ManifestClientIDDataType(),
						null,
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ContainerHandlingRateClassList
		{
			get
			{
				return GetItem("ContainerHandlingRateClassList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"ContainerHandlingRateClassList",
						Categories.AutoRating_GenericContainerClasses,
						ResString.GetMultilingualString("6243753c-1531-440c-a1b5-f049d4d00201", "Container Handling Charges Classes"),
						ResString.GetMultilingualString("f1e40377-5fe2-42f7-a3f2-abeaa0c3f2bc", "Provides the ability to specify origin and destination charges in the system for an entire Container Class rather than a specific container type."),
						4,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new ContainerISOTypeGroupList());
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ContainerFreightRateClassList
		{
			get
			{
				return GetItem("ContainerFreightRateClassList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"ContainerFreightRateClassList",
						Categories.AutoRating_GenericContainerClasses,
						ResString.GetMultilingualString("a41f6d01-6159-4cde-b574-ba26607e7f54", "Container Freight Charges Classes"),
						ResString.GetMultilingualString("6431d221-271c-41cb-b9a3-c4e07de37ba9", "Provides the ability to specify freight charges in the system for an entire Container Class rather than a specific container type."),
						4,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new ContainerISOTypeGroupList());
				});
			}
		}

		public CodeDescriptionPairListRegistryItem FCLEquipmentNeeded
		{
			get
			{
				return GetItem("FCLEquipmentNeeded", delegate
				{
					CodeDescriptionPairList emptyList = new CodeDescriptionPairList();
					return new CodeDescriptionPairListRegistryItem("FCLEquipmentNeeded", Categories.Freight_Shipment, ResString.GetMultilingualString("b01d190b-ba20-4fd0-a636-db58fe707aa2", "FCL Port Transport Drop Modes"), null, 3, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, emptyList);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem LCLAIREquipmentNeeded
		{
			get
			{
				return GetItem("LCLAIREquipmentNeeded", delegate
				{
					CodeDescriptionPairList emptyList = new CodeDescriptionPairList();
					return new CodeDescriptionPairListRegistryItem("LCLAIREquipmentNeeded", Categories.Freight_Shipment, ResString.GetMultilingualString("4185da11-d7f5-45bf-ae3c-6fe5a6361c5e", "LCL/AIR Port Transport Drop Modes"), null, 3, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, emptyList);
				});
			}
		}

		public IRegistryItem HouseBillOfLadingLogo
		{
			get
			{
				return GetItem("HouseBillOfLadingLogo", delegate
				{
					return new ImageRegistryItem("HouseBillOfLadingLogo", Categories.Freight_HouseBills, ResString.GetMultilingualString("7895ce9a-dd25-4130-9b13-5886b8b29b42", "House Bill Of Lading Logo"), null, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ContainerStorageClass
		{
			get
			{
				return GetItem("ContainerStorageClass", delegate
				{
					CodeDescriptionPairList defaultStorageClass = new CodeDescriptionPairList();
					defaultStorageClass.AddPair("20F", ResString.GetMultilingualString("50a0d221-4827-4970-bb7a-a314dd69614a", "Twenty Foot Equivalent Unit"));
					defaultStorageClass.AddPair("20R", ResString.GetMultilingualString("c5078625-629e-4900-8c34-38d4539a9f40", "Twenty Foot Reefer"));
					defaultStorageClass.AddPair("20H", ResString.GetMultilingualString("3d58121f-d902-4a7c-bbf3-dde9abff863a", "Twenty Foot High Cube"));
					defaultStorageClass.AddPair("40F", ResString.GetMultilingualString("da781142-06f6-4641-86d8-8f3c237cc406", "Forty Foot Equivalent Unit"));
					defaultStorageClass.AddPair("40R", ResString.GetMultilingualString("6565281f-6f61-4259-bb7f-281293407094", "Forty Foot Reefer"));
					defaultStorageClass.AddPair("40H", ResString.GetMultilingualString("87c26d04-5d90-47a2-8ea6-84d5a773808e", "Forty Foot High Cube"));
					defaultStorageClass.AddPair("45F", ResString.GetMultilingualString("c71a23b1-937c-4c3b-9ab1-c129813d7d32", "Forty Five Foot"));
					defaultStorageClass.AddPair("GEN", ResString.GetMultilingualString("8ea5c0b4-410e-4fcb-9685-c49deefa9746", "Genset"));
					return new CodeDescriptionPairListRegistryItem("ContainerStorageClass", Categories.Freight_Container, ResString.GetMultilingualString("ff882b93-722e-497d-92e0-f06a0d9c2de0", "Container Storage Class"), ResString.GetMultilingualString("b38149c8-fee6-473b-8f58-02f9d28c6912", "Storage class list used for rating of containers."), 3, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, defaultStorageClass);
				});
			}
		}

		internal IRegistryItem PackLineCustomAttribute1Caption
		{
			get
			{
				return GetItem("PackLineCustomAttribute1Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomAttribute1Caption", Categories.Freight_PackLine_CustomAttribute1, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomAttribute1Hint
		{
			get
			{
				return GetItem("PackLineCustomAttribute1Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomAttribute1Hint", Categories.Freight_PackLine_CustomAttribute1, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomAttribute2Caption
		{
			get
			{
				return GetItem("PackLineCustomAttribute2Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomAttribute2Caption", Categories.Freight_PackLine_CustomAttribute2, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomAttribute2Hint
		{
			get
			{
				return GetItem("PackLineCustomAttribute2Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomAttribute2Hint", Categories.Freight_PackLine_CustomAttribute2, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomAttribute3Caption
		{
			get
			{
				return GetItem("PackLineCustomAttribute3Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomAttribute3Caption", Categories.Freight_PackLine_CustomAttribute3, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomAttribute3Hint
		{
			get
			{
				return GetItem("PackLineCustomAttribute3Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomAttribute3Hint", Categories.Freight_PackLine_CustomAttribute3, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomAttribute4Caption
		{
			get
			{
				return GetItem("PackLineCustomAttribute4Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomAttribute4Caption", Categories.Freight_PackLine_CustomAttribute4, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomAttribute4Hint
		{
			get
			{
				return GetItem("PackLineCustomAttribute4Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomAttribute4Hint", Categories.Freight_PackLine_CustomAttribute4, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomDecimal1Caption
		{
			get
			{
				return GetItem("PackLineCustomDecimal1Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomDecimal1Caption", Categories.Freight_PackLine_CustomDecimal1, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomDecimal1Hint
		{
			get
			{
				return GetItem("PackLineCustomDecimal1Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomDecimal1Hint", Categories.Freight_PackLine_CustomDecimal1, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomDecimal2Caption
		{
			get
			{
				return GetItem("PackLineCustomDecimal2Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomDecimal2Caption", Categories.Freight_PackLine_CustomDecimal2, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomDecimal2Hint
		{
			get
			{
				return GetItem("PackLineCustomDecimal2Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomDecimal2Hint", Categories.Freight_PackLine_CustomDecimal2, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomDate1Caption
		{
			get
			{
				return GetItem("PackLineCustomDate1Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomDate1Caption", Categories.Freight_PackLine_CustomDate1, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomDate1Hint
		{
			get
			{
				return GetItem("PackLineCustomDate1Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomDate1Hint", Categories.Freight_PackLine_CustomDate1, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomFlag1Caption
		{
			get
			{
				return GetItem("PackLineCustomFlag1Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomFlag1Caption", Categories.Freight_PackLine_CustomFlag1, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomFlag1Hint
		{
			get
			{
				return GetItem("PackLineCustomFlag1Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomFlag1Hint", Categories.Freight_PackLine_CustomFlag1, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomDate2Caption
		{
			get
			{
				return GetItem("PackLineCustomDate2Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomDate2Caption", Categories.Freight_PackLine_CustomDate2, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomDate2Hint
		{
			get
			{
				return GetItem("PackLineCustomDate2Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomDate2Hint", Categories.Freight_PackLine_CustomDate2, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomFlag2Caption
		{
			get
			{
				return GetItem("PackLineCustomFlag2Caption", delegate
				{
					return new StringRegistryItem("PackLineCustomFlag2Caption", Categories.Freight_PackLine_CustomFlag2, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomFlag2Hint
		{
			get
			{
				return GetItem("PackLineCustomFlag2Hint", delegate
				{
					return new StringRegistryItem("PackLineCustomFlag2Hint", Categories.Freight_PackLine_CustomFlag2, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem PackLineCustomHeadingCaption
		{
			get
			{
				return GetItem("PackLineCustomHeadingCaption", delegate
				{
					return new StringRegistryItem("PackLineCustomHeadingCaption", Categories.Freight_PackLine_CustomHeading, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default text for non-multilingual registry item should not be localized")]
		internal IRegistryItem PackLineCustomHeadingHint
		{
			get
			{
				return GetItem("PackLineCustomHeadingHint", delegate
				{
					return new StringRegistryItem("PackLineCustomHeadingHint", Categories.Freight_PackLine_CustomHeading, ResString.GetMultilingualString("e264d93b-897a-4b1e-9ebe-51b02f38ca66", "Hint"), null, RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.");
				});
			}
		}

		internal IRegistryItem OuterPacklinesMeasurementDefaultUnit
		{
			get
			{
				return GetItem("OuterPacklinesMeasurementDefaultUnit", delegate
				{
					IRegistryItem result = new StringRegistryItem("OuterPacklinesMeasurementDefaultUnit", Categories.Freight_PackLine, ResString.GetMultilingualString("22ac288e-d39f-449b-9c11-b7f57d3a6b53", "Outer Packlines Measurement Default Unit"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, Constants.Length.Metres);
					result.EditorInfo = new ComboBoxRegistryEditorInfo(OLookUpEditType.Length);
					return result;
				});
			}
		}

		internal IRegistryItem InnerPacklinesMeasurementDefaultUnit
		{
			get
			{
				return GetItem("InnerPacklinesMeasurementDefaultUnit", delegate
				{
					IRegistryItem result = new StringRegistryItem("InnerPacklinesMeasurementDefaultUnit", Categories.Freight_PackLine, ResString.GetMultilingualString("1e17d897-30b0-40a9-81b4-993aae3c13db", "Inner Packlines Measurement Default Unit"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, Constants.Length.Centimetres);
					result.EditorInfo = new ComboBoxRegistryEditorInfo(OLookUpEditType.Length);
					return result;
				});
			}
		}

#if DEBUG
		internal BooleanRegistryItem ExportGridLayoutDetails
		{
			get
			{
				return GetItem("ExportGridLayoutDetails", delegate
				{
					return new BooleanRegistryItem(
						(NoResString)"ExportGridLayoutDetails",
						Categories.System_DataExportSettings,
						(NoResString)"Allow Export Grid Details",
						(NoResString)"Allow Developers to export the grid layout information.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}
#endif

		#endregion

		#endregion

		#region Products

		internal IRegistryItem PackageWeightUnit
		{
			get
			{
				return GetItem("PackageWeightUnit", delegate
				{
					MultilingualString[] categories =
					{
						Categories.Customs_Packages,
						Categories.Warehouse_Packages,
					};
					return new CodePairRegistryItem(
						"PackageWeightUnit",
						categories,
						ResString.GetMultilingualString("9db004da-2998-466f-b369-74ffb97a955d", "Weight Unit"),
						ResString.GetMultilingualString("44eec4bb-5309-4343-90af-74ebd9828435", "The Weight Unit to use for Packages."),
						OLookUpEditType.Weight,
						RegistryStorageFlags.System | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"KG");
				});
			}
		}

		internal IRegistryItem PackageVolumeUnit
		{
			get
			{
				return GetItem("PackageVolumeUnit", delegate
				{
					MultilingualString[] categories =
					{
						Categories.Customs_Packages,
						Categories.Warehouse_Packages,
					};
					return new CodePairRegistryItem(
						"PackageVolumeUnit",
						categories,
						ResString.GetMultilingualString("0294ab4d-3ebc-4b62-89f4-9ea4583b03eb", "Volume Unit"),
						ResString.GetMultilingualString("dafc4831-cdef-4b91-a840-b9011b434227", "The Volume Unit to use for Packages."),
						OLookUpEditType.Volume,
						RegistryStorageFlags.System | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"M3");
				});
			}
		}

		internal IRegistryItem DefaultStockUnits
		{
			get
			{
				return GetItem("DefaultStockUnits", delegate
				{
					MultilingualString[] categories =
					{
						Categories.Customs_Products, Categories.Warehouse_Products
					};
					return new StringRegistryItem("DefaultStockUnits", categories, ResString.GetMultilingualString("558431d1-88fd-4d20-9a17-eb6e27a06db9", "Default Stock Unit"), ResString.GetMultilingualString("2bc18138-4349-4fd1-80a7-e736e259f3c7", "Initial value set in Stock Unit field when a product is added."), new StringRegistryDataType(), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, "UNT");
				});
			}
		}

		public BooleanRegistryItem UnitConversionPackTypesValidation
		{
			get
			{
				return GetItem(nameof(UnitConversionPackTypesValidation), () =>
				{
					MultilingualString[] categories =
					{
						Categories.Customs_Products, Categories.Warehouse_Products
					};

					return new BooleanRegistryItem(nameof(UnitConversionPackTypesValidation), categories, ResString.GetMultilingualString("00d6ee2d-94c2-493c-9138-d3b23b37fc4e", "Unit Conversion Pack Types Validation"), ResString.GetMultilingualString("2e52abb7-d31f-4ef6-8c05-3659aa71a451", @"Enabling this setting will display an error if the entered pack type or parent pack type of a unit conversion is not defined in the list of valid pack types.

Enabling this setting will also reject products with invalid pack types during importing CSV or Native XML files."), null, RegistryStorageFlags.System, RegistryOptions.Default, false);
				});
			}
		}

		#endregion

		#region Order Management

		internal IRegistryItem OrderLineContainersVisible
		{
			get
			{
				return GetItem("OrderLineContainersVisible", delegate
				{
					return new BooleanRegistryItem("OrderLineContainersVisible", null, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden, false);
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem OrderLineStatusList
		{
			get
			{
				return GetItem("OrderLineStatusListXmlData", delegate
				{
					return new CodeDescriptionPairListRegistryItem("OrderLineStatusListXmlData", Categories.Orders, ResString.GetMultilingualString("468bbb52-0f4d-4e1a-b446-799238563ecf", "Order Line Status List"), null, 3, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, new CodeDescriptionPairList());
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem OrderHeaderStatusList
		{
			get
			{
				return GetItem("OrderHeaderStatusListXmlData", delegate
				{
					return new CodeDescriptionPairListRegistryItem("OrderHeaderStatusListXmlData", Categories.Orders, ResString.GetMultilingualString("1535f6ea-b687-4c25-a884-d219fcae04ec", "Order Header Status List"), null, 3, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, new CodeDescriptionPairList());
				});
			}
		}

		#endregion

		#region Documents

		public BooleanRegistryItem UseJSEngineForDocumentMacroEvaluation
		{
			get
			{
				return GetItem("UseJSEngineForDocumentMacroEvaluation", () => new BooleanRegistryItem(
					"UseJSEngineForDocumentMacroEvaluation",
					Categories.Documents,
					(NoResString)"Use JS engine for macro evaluation", // Support only Registry Item
					(NoResString)"This registry determines whether JS engine should be used to evaluate macro binary expressions.", // Support only Registry Item
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public BooleanRegistryItem UseExpressionCachingForFormBuilderMacroEngine
		{
			get
			{
				return GetItem("UseExpressionCachingForFormBuilderMacroEngine", () => new BooleanRegistryItem(
					"UseExpressionCachingForFormBuilderMacroEngine",
					Categories.Documents,
					(NoResString)"Cache expressions for form builder macros", // Support only Registry Item
					(NoResString)"This registry determines whether catching will be used for form builder macros. This functionality is initialized at application startup so any changes to this setting require restart.", // Support only Registry Item
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));
			}
		}

		public IntRegistryItem JSEngineMaxStatements
		{
			get
			{
				return GetItem("JSEngineMaxStatements", () => new IntRegistryItem(
					"JSEngineMaxStatements",
					Categories.System,
					(NoResString)"Maximum JS Engine Statements", // Support only Registry Item
					(NoResString)"This controls how many executions the JS engine can do when evaluating macros.", // Support only Registry Item
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					10_000));
			}
		}

		public BooleanRegistryItem UseJSEngineForTriggerConditionsEvaluation
		{
			get
			{
				return GetItem("UseJSEngineForTriggerConditionsEvaluation", () => new BooleanRegistryItem(
					"UseJSEngineForTriggerConditionsEvaluation",
					Categories.Documents,
					(NoResString)"Use JS engine for trigger conditions evaluation", // Support only Registry Item
					(NoResString)"This registry determines whether JS engine should be used to evaluate trigger conditions.", // Support only Registry Item
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));
			}
		}

		public BooleanRegistryItem UseJSEngineForAutoRatingConditionsEvaluation
		{
			get
			{
				return GetItem("UseJSEngineForAutoRatingConditionsEvaluation", () => new BooleanRegistryItem(
					"UseJSEngineForAutoRatingConditionsEvaluation",
					Categories.Documents,
					(NoResString)"Use JS engine for autorating conditions evaluation", // Support only Registry Item
					(NoResString)"This registry determines whether JS engine should be used to evaluate autorating user conditions.", // Support only Registry Item
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));
			}
		}

		internal IRegistryItem ExcelPrinterInternationalFormats
		{
			get
			{
				return GetItem("ExcelPrinterInternationalFormats", delegate
				{
					IRegistryItem result = new StringRegistryItem("ExcelPrinterInternationalFormats", Categories.Documents, ResString.GetMultilingualString("48b0fdab-083b-4dd1-af65-46578e8c16d8", "Excel Printer Name International Formats"), ResString.GetMultilingualString("cdc77d44-b8b7-41d0-837b-d1a497bd306d", @"This is only applicable for non-English versions of Microsoft Excel/Microsoft Windows.

In English, Excel printer names look like 'Printer XYZ on LPT1:'. To override this for individual servers, enter a string using a format like:
{0}
In this example, the printer would be named 'Printer {1}:' (suitable for French Excel).", "SERVER NAME:$PRINTER sur $PORT", "XYZ sur LPT1"), RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public IRegistryItem DeliverReportsInBackground
		{
			get
			{
				return GetItem("DeliverReportsInBackground", delegate
				{
					return new BooleanRegistryItem("DeliverReportsInBackground", Categories.Documents, ResString.GetMultilingualString("510302de-072c-4186-950e-e6251e57c54e", "Background Report Delivery"),
						ResString.GetMultilingualString("41c2349c-939d-466e-b4bd-592781fea028", "If this option is set to 'Yes', then all reports will be rendered and delivered in the background. This means that you can continue working while the report is being delivered.\r\nIf this option is 'No', then reports are rendered in the foreground and for long running reports, you will not be able to use the application while the report is being created."),
						RegistryStorageFlags.System, true);
				});
			}
		}

		internal IRegistryItem DocumentDefaultDestination
		{
			get
			{
				return GetItem("DocumentDefaultDestination", delegate
				{
					return new BinaryRegistryItem("DocumentDefaultDestination", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem CoverPageText
		{
			get
			{
				return GetItem("CoverPageText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("CoverPageText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("b08d6370-7604-4262-ae58-333a873e2d7a", "Cover Page Text - Existing Clients"), ResString.GetMultilingualString("cd385276-7b57-404c-851d-5638286e58e2", "Specifies the default text to place on quotation cover pages for existing clients."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem CoverPageTextOneOff
		{
			get
			{
				return GetItem("CoverPageTextOneOff", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("CoverPageTextOneOff", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("9ce01852-33c8-4d88-931d-50f731937602", "Cover Page Text - One Off Quotes - Existing Clients"), ResString.GetMultilingualString("3e1847c4-88c0-40fb-b127-5cb3be469f35", "Specifies the default text to place on one off quotation cover pages for existing clients."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem CoverPageTextNew
		{
			get
			{
				return GetItem("CoverPageTextNew", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("CoverPageTextNew", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("a8ddd2c6-151a-4b90-af93-5d1634906122", "Cover Page Text - New Clients"), ResString.GetMultilingualString("606440fa-de40-448e-bd1a-5b31e1f42aea", "Specifies the default text to place on quotation cover pages for new clients."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem CoverPageTextOneOffNew
		{
			get
			{
				return GetItem("CoverPageTextOneOffNew", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("CoverPageTextOneOffNew", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("21940d38-f430-4db0-a08e-b17cefb658b9", "Cover Page Text - One Off Quotes - New Clients"), ResString.GetMultilingualString("e826528e-a702-4e40-85e5-080ac01025f8", "Specifies the default text to place on one off quotation cover pages for new clients."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public IRegistryItem DocumentOpeningText
		{
			get
			{
				return GetItem("DocumentOpeningText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("DocumentOpeningText", Categories.Documents, ResString.GetMultilingualString("0d85bfcc-d038-4e97-b99b-650a9d0b4d59", "Default Opening Text"), ResString.GetMultilingualString("0d85bfcc-d038-4e97-b99b-650a9d0b4d59", "Default Opening Text"), RegistryStorageFlags.All);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public IRegistryItem DocumentClosingText
		{
			get
			{
				return GetItem("DocumentClosingText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("DocumentClosingText", Categories.Documents, ResString.GetMultilingualString("9ebb4fa9-52cb-4902-b0cc-03c7d597ef76", "Default Closing Text"), ResString.GetMultilingualString("9ebb4fa9-52cb-4902-b0cc-03c7d597ef76", "Default Closing Text"), RegistryStorageFlags.All);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}
		
		internal IRegistryItem ShowSystemGeneratedContactsOnOrgDocuments
		{
			get
			{
				return GetItem(
					"ShowSystemGeneratedContactsOnOrgDocuments",
					delegate
					{
						return new BooleanRegistryItem(
							"ShowSystemGeneratedContactsOnOrgDocuments",
							Categories.Documents_Organization,
							ResString.GetMultilingualString("d19601e8-ed3b-429e-a816-9277421e098a", "Show System-generated Contacts in Documents"),
							ResString.GetMultilingualString("d19601e8-ed3b-429e-a816-9277421e098a", "Show System-generated Contacts in Documents"),
							RegistryStorageFlags.All,
							false);
					});
			}
		}

		internal ImportAirTextRegistryItem ImportAirPreAlertOpeningText
		{
			get
			{
				return GetItem("ImportAirPreAlertOpeningText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirPreAlertOpeningText", Categories.PreAlert, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("bf8f2606-ac3b-432e-aff4-9520471f8506", "Pre Alert Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirPreAlertClosingText
		{
			get
			{
				return GetItem("ImportAirPreAlertClosingText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirPreAlertClosingText", Categories.PreAlert, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("32e23c90-51ae-482f-b1a2-f55b14dd6c7d", "Pre Alert Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public ImportAirTextRegistryItem ImportAirArrivalNoticeOpeningText
		{
			get
			{
				return GetItem("ImportAirArrivalNoticeOpeningText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirArrivalNoticeOpeningText", Categories.ArrivalNotice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("005544e0-e044-4365-b9ac-6a8fba6a97e6", "Arrival Notice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public ImportAirTextRegistryItem ImportAirArrivalNoticeClosingText
		{
			get
			{
				return GetItem("ImportAirArrivalNoticeClosingText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirArrivalNoticeClosingText", Categories.ArrivalNotice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("2c913bb5-44f7-4175-9e69-910dff4f2684", "Arrival Notice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirUltimateConsigneePreAlertOpeningText
		{
			get
			{
				return GetItem("ImportAirUltimateConsigneePreAlertOpeningText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirUltimateConsigneePreAlertOpeningText", Categories.UltimateConsigneePreAlert, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("1664d473-89b4-408f-b945-4468e132d77f", "Ultimate Consignee Pre Alert Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirUltimateConsigneePreAlertClosingText
		{
			get
			{
				return GetItem("ImportAirUltimateConsigneePreAlertClosingText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirUltimateConsigneePreAlertClosingText", Categories.UltimateConsigneePreAlert, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("2da25af2-6c07-4251-bbd5-9f8afde86b95", "Ultimate Consignee Pre Alert Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirUltimateConsigneeArrivalNoticeOpeningText
		{
			get
			{
				return GetItem("ImportAirUltimateConsigneeArrivalNoticeOpeningText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirUltimateConsigneeArrivalNoticeOpeningText", Categories.UltimateConsigneeArrivalNotice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("d82979cb-617e-4b93-a58f-16e9ad487969", "Ultimate Consignee Arrival Notice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirUltimateConsigneeArrivalNoticeClosingText
		{
			get
			{
				return GetItem("ImportAirUltimateConsigneeArrivalNoticeClosingText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirUltimateConsigneeArrivalNoticeClosingText", Categories.UltimateConsigneeArrivalNotice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("fec46cf7-2f5a-4e82-9a4f-350315bce4f0", "Ultimate Consignee Arrival Notice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirShippingAdviceOpeningText
		{
			get
			{
				return GetItem("ImportAirShippingAdviceOpeningText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirShippingAdviceOpeningText", Categories.ShippingAdvice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("da9df62f-494b-4e43-a2ee-f3c9c744c9ee", "Shipping Advice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirShippingAdviceClosingText
		{
			get
			{
				return GetItem("ImportAirShippingAdviceClosingText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirShippingAdviceClosingText", Categories.ShippingAdvice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("e38600b5-f81e-4084-a598-8e9ce054ba27", "Shipping Advice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirFreightDeliveryOrderOpeningText
		{
			get
			{
				return GetItem("ImportAirFreightDeliveryOrderOpeningText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirFreightDeliveryOrderOpeningText", Categories.DeliveryOrder, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("57982549-afe4-4765-9136-66196398b87c", "Delivery Order Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirFreightDeliveryOrderClosingText
		{
			get
			{
				return GetItem("ImportAirFreightDeliveryOrderClosingText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirFreightDeliveryOrderClosingText", Categories.DeliveryOrder, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("8e28dc53-279a-48f6-ab31-f99f633daa1b", "Delivery Order Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirOutturnReportOpeningText
		{
			get
			{
				return GetItem("ImportAirOutturnReportOpeningText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirOutturnReportOpeningText", Categories.OutturnReport, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("56eee9be-1bd5-4903-ad2f-142f9ae522aa", "Outturn Report Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportAirTextRegistryItem ImportAirOutturnReportClosingText
		{
			get
			{
				return GetItem("ImportAirOutturnReportClosingText", delegate
				{
					return new ImportAirTextRegistryItem("ImportAirOutturnReportClosingText", Categories.OutturnReport, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("2dc057e0-a9c6-490e-ab87-e6e4ae56fca4", "Outturn Report Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaFreightPreAlertOpeningText
		{
			get
			{
				return GetItem("ImportSeaFreightPreAlertOpeningText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaFreightPreAlertOpeningText", Categories.PreAlert, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("bf8f2606-ac3b-432e-aff4-9520471f8506", "Pre Alert Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaFreightPreAlertClosingText
		{
			get
			{
				return GetItem("ImportSeaFreightPreAlertClosingText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaFreightPreAlertClosingText", Categories.PreAlert, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("32e23c90-51ae-482f-b1a2-f55b14dd6c7d", "Pre Alert Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public ImportSeaTextRegistryItem ImportSeaFreightArrivalNoticeOpeningText
		{
			get
			{
				return GetItem("ImportSeaFreightArrivalNoticeOpeningText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaFreightArrivalNoticeOpeningText", Categories.ArrivalNotice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("005544e0-e044-4365-b9ac-6a8fba6a97e6", "Arrival Notice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public ImportSeaTextRegistryItem ImportSeaFreightArrivalNoticeClosingText
		{
			get
			{
				return GetItem("ImportSeaFreightArrivalNoticeClosingText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaFreightArrivalNoticeClosingText", Categories.ArrivalNotice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("2c913bb5-44f7-4175-9e69-910dff4f2684", "Arrival Notice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaFreightDeliveryOrderOpeningText
		{
			get
			{
				return GetItem("ImportSeaFreightDeliveryOrderOpeningText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaFreightDeliveryOrderOpeningText", Categories.DeliveryOrder, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("57982549-afe4-4765-9136-66196398b87c", "Delivery Order Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaFreightDeliveryOrderClosingText
		{
			get
			{
				return GetItem("ImportSeaFreightDeliveryOrderClosingText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaFreightDeliveryOrderClosingText", Categories.DeliveryOrder, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("8e28dc53-279a-48f6-ab31-f99f633daa1b", "Delivery Order Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaUltimateConsigneePreAlertOpeningText
		{
			get
			{
				return GetItem("ImportSeaUltimateConsigneePreAlertOpeningText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaUltimateConsigneePreAlertOpeningText", Categories.UltimateConsigneePreAlert, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("1664d473-89b4-408f-b945-4468e132d77f", "Ultimate Consignee Pre Alert Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaUltimateConsigneePreAlertClosingText
		{
			get
			{
				return GetItem("ImportSeaUltimateConsigneePreAlertClosingText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaUltimateConsigneePreAlertClosingText", Categories.UltimateConsigneePreAlert, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("2da25af2-6c07-4251-bbd5-9f8afde86b95", "Ultimate Consignee Pre Alert Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaUltimateConsigneeArrivalNoticeOpeningText
		{
			get
			{
				return GetItem("ImportSeaUltimateConsigneeArrivalNoticeOpeningText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaUltimateConsigneeArrivalNoticeOpeningText", Categories.UltimateConsigneeArrivalNotice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("d82979cb-617e-4b93-a58f-16e9ad487969", "Ultimate Consignee Arrival Notice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaUltimateConsigneeArrivalNoticeClosingText
		{
			get
			{
				return GetItem("ImportSeaUltimateConsigneeArrivalNoticeClosingText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaUltimateConsigneeArrivalNoticeClosingText", Categories.UltimateConsigneeArrivalNotice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("fec46cf7-2f5a-4e82-9a4f-350315bce4f0", "Ultimate Consignee Arrival Notice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaOutturnReportClosingText
		{
			get
			{
				return GetItem("ImportSeaOutturnReportClosingText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaOutturnReportClosingText", Categories.OutturnReport, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("2dc057e0-a9c6-490e-ab87-e6e4ae56fca4", "Outturn Report Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}
		internal ImportSeaTextRegistryItem ImportSeaOutturnReportOpeningText
		{
			get
			{
				return GetItem("ImportSeaOutturnReportOpeningText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaOutturnReportOpeningText", Categories.OutturnReport, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("56eee9be-1bd5-4903-ad2f-142f9ae522aa", "Outturn Report Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaShippingAdviceOpeningText
		{
			get
			{
				return GetItem("ImportSeaShippingAdviceOpeningText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaShippingAdviceOpeningText", Categories.ShippingAdvice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("da9df62f-494b-4e43-a2ee-f3c9c744c9ee", "Shipping Advice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ImportSeaTextRegistryItem ImportSeaShippingAdviceClosingText
		{
			get
			{
				return GetItem("ImportSeaShippingAdviceClosingText", delegate
				{
					return new ImportSeaTextRegistryItem("ImportSeaShippingAdviceClosingText", Categories.ShippingAdvice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("e38600b5-f81e-4084-a598-8e9ce054ba27", "Shipping Advice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportAirTextRegistryItem ExportAirBookingConfirmationOpeningText
		{
			get
			{
				return GetItem("ExportAirBookingConfirmationOpeningText", delegate
				{
					return new ExportAirTextRegistryItem("ExportAirBookingConfirmationOpeningText", Categories.BookingConfirmation, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("5c23b48c-ce88-4981-845b-060fb3660c50", "Booking Confirmation Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportAirTextRegistryItem ExportAirBookingConfirmationClosingText
		{
			get
			{
				return GetItem("ExportAirBookingConfirmationClosingText", delegate
				{
					return new ExportAirTextRegistryItem("ExportAirBookingConfirmationClosingText", Categories.BookingConfirmation, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("3965c02d-9945-4e91-b7ff-3f147638a2c0", "Booking Confirmation Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportAirTextRegistryItem ExportAirFreightShipperDepartureNoticeOpeningText
		{
			get
			{
				return GetItem("ExportAirFreightShipperDepartureNoticeOpeningText", delegate
				{
					return new ExportAirTextRegistryItem("ExportAirFreightShipperDepartureNoticeOpeningText", Categories.ShipperDepartureNotice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("f7af360f-490b-4310-99eb-c3aa49a39858", "Shipper Departure Notice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportAirTextRegistryItem ExportAirFreightShipperDepartureNoticeClosingText
		{
			get
			{
				return GetItem("ExportAirFreightShipperDepartureNoticeClosingText", delegate
				{
					return new ExportAirTextRegistryItem("ExportAirFreightShipperDepartureNoticeClosingText", Categories.ShipperDepartureNotice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("3c00f42f-f6ab-48a8-b244-725359278cbf", "Shipper Departure Notice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportAirTextRegistryItem ExportAirLetterToOverseasAgentOpeningText
		{
			get
			{
				return GetItem("ExportAirLetterToOverseasAgentOpeningText", delegate
				{
					return new ExportAirTextRegistryItem("ExportAirLetterToOverseasAgentOpeningText", Categories.LetterToOverseasAgent, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("0579dfe6-50ba-47b9-b4e2-3593ba54b81a", "Letter To Overseas Agent Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportAirTextRegistryItem ExportAirLetterToOverseasAgentClosingText
		{
			get
			{
				return GetItem("ExportAirLetterToOverseasAgentClosingText", delegate
				{
					return new ExportAirTextRegistryItem("ExportAirLetterToOverseasAgentClosingText", Categories.LetterToOverseasAgent, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("33829224-3826-4344-8fe0-48416666d08d", "Letter To Overseas Agent Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportAirTextRegistryItem ExportAirFreightAgentDepartureNoticeOpeningText
		{
			get
			{
				return GetItem("ExportAirFreightAgentDepartureNoticeOpeningText", delegate
				{
					return new ExportAirTextRegistryItem("ExportAirFreightAgentDepartureNoticeOpeningText", Categories.AgentDepartureNotice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("f4c23802-3064-4e3c-b6d6-d0643b2d6452", "Agent Departure Notice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportAirTextRegistryItem ExportAirFreightAgentDepartureNoticeClosingText
		{
			get
			{
				return GetItem("ExportAirFreightAgentDepartureNoticeClosingText", delegate
				{
					return new ExportAirTextRegistryItem("ExportAirFreightAgentDepartureNoticeClosingText", Categories.AgentDepartureNotice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("c4097579-649f-4efa-a803-bc5b8d3497be", "Agent Departure Notice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal MultilingualStringRegistryItem AirAgentsInstructionOpeningText
		{
			get
			{
				return GetItem("ExportAirAgentsInstructionOpeningText", delegate
				{
					MultilingualStringRegistryItem result = new DocumentOpenCloseTextRegistryItem("ExportAirAgentsInstructionOpeningText", Categories.Documents_Forwarding_Shipment_AgentsInstruction_Air, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("190999fa-bba6-4c37-9580-825a5adfbe71", "Agents Instruction Opening Text for Air."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.DepartmentsAllowed = DepartmentFlags.Air;
					return result;
				});
			}
		}

		internal MultilingualStringRegistryItem AirAgentsInstructionClosingText
		{
			get
			{
				return GetItem("ExportAirAgentsInstructionClosingText", delegate
				{
					MultilingualStringRegistryItem result = new DocumentOpenCloseTextRegistryItem("ExportAirAgentsInstructionClosingText", Categories.Documents_Forwarding_Shipment_AgentsInstruction_Air, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("25398745-f017-4c92-b5b0-ad281501e154", "Agents Instruction Closing Text for Air."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.DepartmentsAllowed = DepartmentFlags.Air;
					return result;
				});
			}
		}

		internal ExportSeaTextRegistryItem ExportSeaBookingConfirmationOpeningText
		{
			get
			{
				return GetItem("ExportSeaBookingConfirmationOpeningText", delegate
				{
					return new ExportSeaTextRegistryItem("ExportSeaBookingConfirmationOpeningText", Categories.BookingConfirmation, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("5c23b48c-ce88-4981-845b-060fb3660c50", "Booking Confirmation Opening Text"),
						 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportSeaTextRegistryItem ExportSeaBookingConfirmationClosingText
		{
			get
			{
				return GetItem("ExportSeaBookingConfirmationClosingText", delegate
				{
					return new ExportSeaTextRegistryItem("ExportSeaBookingConfirmationClosingText", Categories.BookingConfirmation, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("3965c02d-9945-4e91-b7ff-3f147638a2c0", "Booking Confirmation Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportSeaTextRegistryItem ExportSeaFreightShipperDepartureNoticeOpeningText
		{
			get
			{
				return GetItem("ExportSeaFreightShipperDepartureNoticeOpeningText", delegate
				{
					return new ExportSeaTextRegistryItem("ExportSeaFreightShipperDepartureNoticeOpeningText", Categories.ShipperDepartureNotice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("f7af360f-490b-4310-99eb-c3aa49a39858", "Shipper Departure Notice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportSeaTextRegistryItem ExportSeaFreightShipperDepartureNoticeClosingText
		{
			get
			{
				return GetItem("ExportSeaFreightShipperDepartureNoticeClosingText", delegate
				{
					return new ExportSeaTextRegistryItem("ExportSeaFreightShipperDepartureNoticeClosingText", Categories.ShipperDepartureNotice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("3c00f42f-f6ab-48a8-b244-725359278cbf", "Shipper Departure Notice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportSeaTextRegistryItem ExportSeaFreightAgentDepartureNoticeOpeningText
		{
			get
			{
				return GetItem("ExportSeaFreightAgentDepartureNoticeOpeningText", delegate
				{
					return new ExportSeaTextRegistryItem("ExportSeaFreightAgentDepartureNoticeOpeningText", Categories.AgentDepartureNotice, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("f4c23802-3064-4e3c-b6d6-d0643b2d6452", "Agent Departure Notice Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportSeaTextRegistryItem ExportSeaFreightAgentDepartureNoticeClosingText
		{
			get
			{
				return GetItem("ExportSeaFreightAgentDepartureNoticeClosingText", delegate
				{
					return new ExportSeaTextRegistryItem("ExportSeaFreightAgentDepartureNoticeClosingText", Categories.AgentDepartureNotice, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("c4097579-649f-4efa-a803-bc5b8d3497be", "Agent Departure Notice Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportSeaTextRegistryItem ExportSeaLetterToOverseasAgentOpeningText
		{
			get
			{
				return GetItem("ExportSeaLetterToOverseasAgentOpeningText", delegate
				{
					return new ExportSeaTextRegistryItem("ExportSeaLetterToOverseasAgentOpeningText", Categories.LetterToOverseasAgent, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("0579dfe6-50ba-47b9-b4e2-3593ba54b81a", "Letter To Overseas Agent Opening Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal ExportSeaTextRegistryItem ExportSeaLetterToOverseasAgentClosingText
		{
			get
			{
				return GetItem("ExportSeaLetterToOverseasAgentClosingText", delegate
				{
					return new ExportSeaTextRegistryItem("ExportSeaLetterToOverseasAgentClosingText", Categories.LetterToOverseasAgent, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("33829224-3826-4344-8fe0-48416666d08d", "Letter To Overseas Agent Closing Text"),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal MultilingualStringRegistryItem SeaAgentsInstructionOpeningText
		{
			get
			{
				return GetItem("ExportSeaAgentsInstructionOpeningText", delegate
				{
					MultilingualStringRegistryItem result = new DocumentOpenCloseTextRegistryItem("ExportSeaAgentsInstructionOpeningText", Categories.Documents_Forwarding_Shipment_AgentsInstruction_Sea, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("f7657f1b-b3b6-4dbb-832c-bbcd2852303d", "Agents Instruction Opening Text for Sea."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.DepartmentsAllowed = DepartmentFlags.Sea;
					return result;
				});
			}
		}

		internal MultilingualStringRegistryItem SeaAgentsInstructionClosingText
		{
			get
			{
				return GetItem("ExportSeaAgentsInstructionClosingText", delegate
				{
					MultilingualStringRegistryItem result = new DocumentOpenCloseTextRegistryItem("ExportSeaAgentsInstructionClosingText", Categories.Documents_Forwarding_Shipment_AgentsInstruction_Sea, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("1b6c4907-d8f4-45f7-adc6-dcd3f3ec05ea", "Agents Instruction Closing Text for Sea."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.DepartmentsAllowed = DepartmentFlags.Sea;
					return result;
				});
			}
		}

		internal IRegistryItem CartageAdviceExportOpeningText
		{
			get
			{
				return GetItem("CartageAdviceExportOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CartageAdviceExportOpeningText", Categories.Documents_Forwarding_Shipment_CartageAdvice_Export, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("afb28195-a224-4ab9-aa3e-fc411be1b00f", "Cartage Advice Export Opening Text."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CartageAdviceExportClosingText
		{
			get
			{
				return GetItem("CartageAdviceExportClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CartageAdviceExportClosingText", Categories.Documents_Forwarding_Shipment_CartageAdvice_Export, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("c06de0b1-af05-4b61-83ed-da2d5ba29d2e", "Cartage Advice Export Closing Text."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CartageAdviceImportOpeningText
		{
			get
			{
				return GetItem("CartageAdviceImportOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CartageAdviceImportOpeningText", Categories.Documents_Forwarding_Shipment_CartageAdvice_Import, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("76861e33-07db-4465-af78-a5f6d13826b7", "Cartage Advice Import Opening Text."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CartageAdviceImportClosingText
		{
			get
			{
				return GetItem("CartageAdviceImportClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CartageAdviceImportClosingText", Categories.Documents_Forwarding_Shipment_CartageAdvice_Import, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("48f7128f-64f1-4e5d-a00c-1b82085a6156", "Cartage Advice Import Closing Text."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CartageAdviceTimeSlotRequestExportOpeningText
		{
			get
			{
				return GetItem("CartageAdviceTimeSlotRequestExportOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CartageAdviceTimeSlotRequestExportOpeningText",
						Categories.Documents_Forwarding_Shipment_TimeSlotRequest_Export,
						ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"),
						ResString.GetMultilingualString("c3e6723d-1ca0-40a5-8f86-c49b9ea6733b", "The text that will be displayed as the opening text in the Time Slot Request document to be sent to the Local Transport Company in order for them to organize a time slot for the delivery of containers related to a FCL shipment."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("3b418961-474b-4f90-b3c0-c40a6413d7be", "Please book time slot(s) for the container(s) listed below."));
				});
			}
		}

		internal IRegistryItem CartageAdviceTimeSlotRequestImportOpeningText
		{
			get
			{
				return GetItem("CartageAdviceTimeSlotRequestImportOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CartageAdviceTimeSlotRequestImportOpeningText",
						Categories.Documents_Forwarding_Shipment_TimeSlotRequest_Import,
						ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"),
						ResString.GetMultilingualString("32f347a5-a010-461b-9039-2a769bf7f43b", "The text that will be displayed as the opening text in the Time Slot Request document to be sent to the Local Transport Company in order for them to organize a time slot for the collection of containers related to a FCL/BCN shipment."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("3b418961-474b-4f90-b3c0-c40a6413d7be", "Please book time slot(s) for the container(s) listed below."));
				});
			}
		}

		internal IRegistryItem CartageAdviceTimeSlotConfirmationOpeningText
		{
			get
			{
				return GetItem("CartageAdviceTimeSlotConfirmationOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CartageAdviceTimeSlotConfirmationOpeningText", Categories.Documents_PortTransport_TimeSlotConfirmation, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("624f12bf-5dd2-488d-944b-7c5c601a8efa", "The text that will be displayed as the opening text in the Time Slot Confirmation document."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CartageAdviceTimeSlotConfirmationClosingText
		{
			get
			{
				return GetItem("CartageAdviceTimeSlotConfirmationClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CartageAdviceTimeSlotConfirmationClosingText", Categories.Documents_PortTransport_TimeSlotConfirmation, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("c3db6a7d-df78-4ff4-8d27-d6e0b547fb36", "The text that will be displayed as the closing text in the Time Slot Confirmation document."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ConsignmentRequestForServiceOpeningText
		{
			get
			{
				return GetItem("ConsignmentRequestForServiceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ConsignmentRequestForServiceOpeningText", Categories.Documents_LandTransport_RequestForService, ResString.GetMultilingualString("a4cabf32-06ae-4ac6-86d6-af1ad72749e7", "Opening Text"), ResString.GetMultilingualString("a06f4256-4360-437b-9fcd-9526fd686de3", "The text that will be displayed as the opening text in the Consignment Request for Service document."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ConsignmentRequestForServiceClosingText
		{
			get
			{
				return GetItem("ConsignmentRequestForServiceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ConsignmentRequestForServiceClosingText", Categories.Documents_LandTransport_RequestForService, ResString.GetMultilingualString("7cf0d2d7-7d5d-45d4-86ab-d8bfb5b88b47", "Closing Text"), ResString.GetMultilingualString("0377767b-1ecd-4510-984e-507c752837a6", "The text that will be displayed as the closing text in the Consignment Request for Service document."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ConsignmentAuthorizationForServiceOpeningText
		{
			get
			{
				return GetItem("ConsignmentAuthorizationForServiceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ConsignmentAuthorizationForServiceOpeningText", Categories.Documents_LandTransport_AuthorizationForService, ResString.GetMultilingualString("a27333b1-2847-4c2c-8316-41b343d4723c", "Opening Text"), ResString.GetMultilingualString("90d88f1f-3f25-44a4-ab2a-b67dd3107237", "The text that will be displayed as the opening text in the Consignment Request for Authorization document."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ConsignmentAuthorizationForServiceClosingText
		{
			get
			{
				return GetItem("ConsignmentAuthorizationForServiceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ConsignmentAuthorizationForServiceClosingText", Categories.Documents_LandTransport_AuthorizationForService, ResString.GetMultilingualString("ca44bae0-e407-4a94-8ef4-1615a401dbae", "Closing Text"), ResString.GetMultilingualString("3786259a-1986-4994-a64a-88ff0c4b4a11", "The text that will be displayed as the closing text in the Consignment Request for Authorization document."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ExportCertificationOpeningText
		{
			get
			{
				return GetItem("ExportCertificationOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ExportCertificationOpeningText",
						Categories.Documents_Forwarding_Shipment_Ausfuhrbescheinigung,
						ResString.GetMultilingualString("1997cf0f-fd28-4aa7-b332-f958e3a1f10b", "Electronic Signature Clause"),
						ResString.GetMultilingualString("fc4934f6-9085-42a9-af97-dd41704ecf65", "This text will be displayed next to the electronic signature in the Export Certification document."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ExportCertificationClosingText
		{
			get
			{
				return GetItem("ExportCertificationClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ExportCertificationClosingText",
						Categories.Documents_Forwarding_Shipment_Ausfuhrbescheinigung,
						RegistryConstants.Strings.ClosingText,
						ResString.GetMultilingualString("b3036c84-2b3d-4c0f-85e2-d5cd914ea19c", "The text that will be displayed as the closing text in the Export Certification document."),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal MultilingualStringRegistryItem SignOffText
		{
			get
			{
				return GetItem("SignOffText", delegate
				{
					return new MultilingualStringRegistryItem("SignOffText", Categories.Documents_UserSignOff, ResString.GetMultilingualString("e41c5d6d-f281-4857-94ca-60d0d839349e", "Sign Off"), ResString.GetMultilingualString("3f70b815-3e32-4867-8921-054e04e16a40", "Sign Off Text"), RegistryStorageFlags.All,
						ResString.GetMultilingualString("FF2EDDB6-E751-4131-8784-D662E5877B38", "Yours Sincerely,"));
				});
			}
		}

		internal IRegistryItem DisbursementNoteTitle
		{
			get
			{
				return GetItem("DisbursementNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("DisbursementNoteTitle", Categories.Documents, ResString.GetMultilingualString("b3976ee9-3d31-48b1-9176-9e034c4110d4", "Disbursement Note Title"), ResString.GetMultilingualString("7ABE1393-BB94-4B62-998F-527302523BCE", "Disbursement Note Title"), RegistryStorageFlags.All, ResString.GetMultilingualString("904bb5ba-c3db-45c1-b70d-492ff4f567ac", "Note"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem PreAlertWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Pre AlertWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("50781efb-5439-4d48-b00a-929908150767", "Pre Alert"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ArrivalNoticeWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Arrival NoticeWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("da954193-ded9-4d1f-916f-be5aa2901049", "Arrival Notice"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ShippingAdviceWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Shipping AdviceWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("efb3405e-f6d2-426a-9247-1b7dc03aec25", "Shipping Advice"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem DeliveryOrderWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Delivery OrderWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("d847afbf-9325-4d9e-8ff1-3cbe0f6bb4ce", "Delivery Order"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem OutturnReportWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Outturn ReportWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("e0241857-2647-4827-97fd-7835110541fe", "Outturn Report"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem BookingConfirmationWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Booking ConfirmationWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("88e75968-85c7-4bcf-8479-2a184ff54cbe", "Booking Confirmation"));
				});
			}
		}

		internal IRegistryItem ShowChargesOnForwardingBookingConfirmation
		{
			get
			{
				return GetItem("ShowChargesOnForwardingBookingConfirmation", delegate
				{
					return new BooleanRegistryItem("ShowChargesOnForwardingBookingConfirmation", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("1EB9B6D6-2630-4316-A477-98ECE41AA3BA", "Show Charges"), ResString.GetMultilingualString("6b0f17a3-f228-46ff-bc05-8cf53273cc2c", "Show charges on booking confirmations printed from Forwarding"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		internal IRegistryItem ShowChargesOnBookingsBookingConfirmation
		{
			get
			{
				return GetItem("ShowChargesOnBookingsBookingConfirmation", () =>

					new BooleanRegistryItem("ShowChargesOnBookingsBookingConfirmation", Categories.Documents_Booking_BookingConfirmation, ResString.GetMultilingualString("8c3f3cdd-1137-4017-949d-efd3585c3627", "Show Charges (Legacy Documents)"), ResString.GetMultilingualString("5baca46a-0e5d-46e9-9150-225ad240cb59", "Show charges on booking confirmations printed from Export Bookings"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false));
			}
		}

		internal IRegistryItem ShowMarksAndNumbersForFCL
		{
			get
			{
				return GetItem("ShowMarksAndNumbersForFCL", delegate
				{
					return new BooleanRegistryItem("ShowMarksAndNumbersForFCL", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("40f3d652-8e8e-496f-8a68-8dbd90232b22", "Show Marks And Numbers For FCL"), ResString.GetMultilingualString("4e187299-0a39-4d2d-9b33-04ddfbe9b54d", "The industry standard is not to display marks & numbers on FCL booking confirmations."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem ShowMarksAndNumbersForNonFCL
		{
			get
			{
				return GetItem("ShowMarksAndNumbersForNonFCL", delegate
				{
					return new BooleanRegistryItem("ShowMarksAndNumbersForNonFCL", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("79c0120c-077f-49b9-86e2-f19cfd0a0708", "Show Marks And Numbers For Non-FCL"), ResString.GetMultilingualString("d495dba4-3539-4ba5-9e98-ba3f0e514d55", "The industry standard is not to display marks & numbers on Non-FCL booking confirmations."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem ShowMarksAndNumbersForLSE
		{
			get
			{
				return GetItem("ShowMarksAndNumbersForLSE", delegate
				{
					return new BooleanRegistryItem("ShowMarksAndNumbersForLSE", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("3b934f79-fcfd-4903-ba90-518f838ed0c8", "Show Marks And Numbers For LSE"), ResString.GetMultilingualString("6c76d5c0-59d3-4136-aabf-bd716e2abf90", "The industry standard is not to display marks & numbers on LSE booking confirmations."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem ShowMarksAndNumbersForNonLSE
		{
			get
			{
				return GetItem("ShowMarksAndNumbersForNonLSE", delegate
				{
					return new BooleanRegistryItem("ShowMarksAndNumbersForNonLSE", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("291d5c8c-9c1f-46b1-ac4c-2fa7eff37534", "Show Marks And Numbers For Non-LSE"), ResString.GetMultilingualString("a71488ee-068a-4728-8ffb-c6051011f1ca", "The industry standard is not to display marks & numbers on Non-LSE booking confirmations."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem MarksAndNumbersHeadingForFCL
		{
			get
			{
				return GetItem("MarksAndNumbersHeadingForFCL", delegate
				{
					return new MultilingualStringRegistryItem("MarksAndNumbersHeadingForFCL", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("58236e8e-176b-45e4-8ad9-f212c95b8483", "Marks And Numbers Heading For FCL"), ResString.GetMultilingualString("8b69edcc-74c1-44e0-9662-b894b02e8c32", "Booking Confirmation Marks And Numbers Heading For FCL"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("c32d3344-fbe5-4285-a006-e4f93e223e8e", "MARKS AND NUMBERS"));
				});
			}
		}

		internal IRegistryItem MarksAndNumbersHeadingForNonFCL
		{
			get
			{
				return GetItem("MarksAndNumbersHeadingForNonFCL", delegate
				{
					return new MultilingualStringRegistryItem("MarksAndNumbersHeadingForNonFCL", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("8bdc33b4-c44e-4897-bd67-4a3a7921cb60", "Marks And Numbers Heading For Non-FCL"), ResString.GetMultilingualString("7e14859d-56d4-418f-b9c9-8a670e85c7c1", "Booking Confirmation Marks And Numbers Heading For Non-FCL"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("c32d3344-fbe5-4285-a006-e4f93e223e8e", "MARKS AND NUMBERS"));
				});
			}
		}

		internal IRegistryItem MarksAndNumbersHeadingForLSE
		{
			get
			{
				return GetItem("MarksAndNumbersHeadingForLSE", delegate
				{
					return new MultilingualStringRegistryItem("MarksAndNumbersHeadingForLSE", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("13d34783-b8ae-4ab3-a39a-7f11a7f7bad8", "Marks And Numbers Heading For LSE"), ResString.GetMultilingualString("e96c629b-469e-419c-91b4-1641497ef7fa", "Booking Confirmation Marks And Numbers Heading For LSE"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("c32d3344-fbe5-4285-a006-e4f93e223e8e", "MARKS AND NUMBERS"));
				});
			}
		}

		internal IRegistryItem MarksAndNumbersHeadingForNonLSE
		{
			get
			{
				return GetItem("MarksAndNumbersHeadingForNonLSE", delegate
				{
					return new MultilingualStringRegistryItem("MarksAndNumbersHeadingForNonLSE", Categories.Documents_Forwarding_Shipment_BookingConfirmation, ResString.GetMultilingualString("cce95823-1ea9-498e-8177-6c13b355ec20", "Marks And Numbers Heading For Non-LSE"), ResString.GetMultilingualString("6493a4f3-5131-461e-970b-7b486496daf2", "Booking Confirmation Marks And Numbers Heading For Non-LSE"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("c32d3344-fbe5-4285-a006-e4f93e223e8e", "MARKS AND NUMBERS"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ShipperDepartureNoticeWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Shipper Departure NoticeWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("b6e736f2-b7c0-4a9e-89e2-2993af7375b3", "Shipper Departure Notice"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem AgentsInstructionNoticeWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Agents InstructionWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("3cf84ff6-14c2-42bc-a2d2-53e802f6f789", "Agents Instruction"));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem CartageAdviceImportWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Cartage Advice/ImportWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(CombineCategories(ResString.GetMultilingualString("a700f9dd-d8bc-4ac4-8dbd-969d77be6edc", "Cartage Advice"), ResString.GetMultilingualString("915ea0a0-3134-4def-8a1f-05402ba270cb", "Import")));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem CartageAdviceExportWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Cartage Advice/ExportWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(CombineCategories(ResString.GetMultilingualString("a700f9dd-d8bc-4ac4-8dbd-969d77be6edc", "Cartage Advice"), ResString.GetMultilingualString("50e94d0b-c3c6-4ec7-b9dd-5e7a45aec8ea", "Export")));
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		public IRegistryItem BillOfLadingWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Bill of LadingWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("1f3b9aff-c4dd-4ea4-a28e-e9a64ab17d44", "Bill of Lading"));
				});
			}
		}

		internal IRegistryItem WeightMinimumDecimalPlacesToDisplay
		{
			get
			{
				return GetItem("WeightMinimumDecimalPlacesToDisplay", delegate
				{
					return new IntRegistryItem("WeightMinimumDecimalPlacesToDisplay", Categories.Documents_Forwarding_Shipment_BillofLading, ResString.GetMultilingualString("57d4be5a-7e70-4f95-9ef9-b150c0e35af4", "Weight Minimum Decimal Places To Display"), null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 0, 0, 3);
				});
			}
		}

		internal IRegistryItem VolumeMinimumDecimalPlacesToDisplay
		{
			get
			{
				return GetItem("VolumeMinimumDecimalPlacesToDisplay", delegate
				{
					return new IntRegistryItem("VolumeMinimumDecimalPlacesToDisplay", Categories.Documents_Forwarding_Shipment_BillofLading, ResString.GetMultilingualString("2b284edd-fa61-468d-b4c4-c46619c59ea4", "Volume Minimum Decimal Places To Display"), null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 0, 0, 3);
				});
			}
		}

		internal IRegistryItem DisplayTareAndGrossWeightOnHBOL
		{
			get
			{
				return GetItem("DisplayTareAndGrossWeightOnHBOL", delegate
				{
					return new BooleanRegistryItem("DisplayTareAndGrossWeightOnHBOL", Categories.Documents_Forwarding_Shipment_BillofLading, ResString.GetMultilingualString("7cc4061d-386e-4bab-8190-0cb8fc219801", "Display Tare and Gross Weight on House Bill"), ResString.GetMultilingualString("994ce0ff-1f50-433f-a0a3-58a746367193", "Display Tare and Gross Weight on House Bill's for FCL Shipments"), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem AirWaybillHAWBWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("HAWBWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("EE902F4E-0ACB-4836-A0A0-32F09C277596", "HAWB"), Categories.Freight_AWB, ResString.GetMultilingualString("3d79ac52-b7ad-4cd9-aec0-05fcc8f51a8f", "Choose the type of weight and volume to display in this document when Freight is not calculated by means of AutoRating"));
				});
			}
		}

		internal IRegistryItem HAWBDefaultShipperText
		{
			get
			{
				return GetItem("HAWBDefaultShipperText", delegate
				{
					return new StringRegistryItem("HAWBDefaultShipperText",
									Categories.Freight_AWB_HAWB,
									ResString.GetMultilingualString("57e33dd6-2d05-4b1c-8573-ec4c73c3e47a", "Default Shipper Text"), ResString.GetMultilingualString("84503660-9147-47ed-9917-2777535d1b64", "Default text entered into signature box for the Shipper on the AWB."),
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem HAWBDefaultCarrierText
		{
			get
			{
				return GetItem("HAWBDefaultCarrierText", delegate
				{
					return new StringRegistryItem("HAWBDefaultCarrierText",
									Categories.Freight_AWB_HAWB,
									ResString.GetMultilingualString("9b3a11c0-fde2-40c0-9ddd-cda6f85d2595", "Default Carrier Text"),
									ResString.GetMultilingualString("5413cd6d-bc7c-4ac7-8c9e-1c7ec262d642", "Carrier text entered into with signature box for the Carrier. This text will be followed by the Airline Name."),
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem AirWaybillMAWBWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("MAWBWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("94D9BBFC-FAED-498c-B5F6-881C3904B434", "MAWB"), Categories.Freight_AWB);
				});
			}
		}

		internal IRegistryItem MAWBDefaultShipperText
		{
			get
			{
				return GetItem("MAWBDefaultShipperText", delegate
				{
					return new StringRegistryItem("MAWBDefaultShipperText",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("57e33dd6-2d05-4b1c-8573-ec4c73c3e47a", "Default Shipper Text"),
						ResString.GetMultilingualString("84503660-9147-47ed-9917-2777535d1b64", "Default text entered into signature box for the Shipper on the AWB."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem MAWBDefaultCarrierText
		{
			get
			{
				return GetItem("MAWBDefaultCarrierText", delegate
				{
					return new StringRegistryItem("MAWBDefaultCarrierText",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("9b3a11c0-fde2-40c0-9ddd-cda6f85d2595", "Default Carrier Text"),
						ResString.GetMultilingualString("5413cd6d-bc7c-4ac7-8c9e-1c7ec262d642", "Carrier text entered into with signature box for the Carrier. This text will be followed by the Airline Name."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem CoLoadMasterManifestWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Co-Load Master ManifestWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("3e64eba4-5af3-455e-a715-1debf75d29e8", "Co-Load Master Manifest"));
				});
			}
		}

		internal IRegistryItem CoLoadMasterManifestDisplayVolumeWhenAir
		{
			get
			{
				return GetItem("CoLoadMasterManifestDisplayVolumeWhenAir", delegate
				{
					return new BooleanRegistryItem("CoLoadMasterManifestDisplayVolumeWhenAir", Categories.Documents_Forwarding_Shipment_CoLoadMasterManifest, ResString.GetMultilingualString("99dd7a96-e1c0-4f75-a271-44d84351bc3a", "Display Volume When Air"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem CoLoadMasterManifestDisplayChargeableWhenAir
		{
			get
			{
				return GetItem("CoLoadMasterManifestDisplayChargeableWhenAir", delegate
				{
					return new BooleanRegistryItem("CoLoadMasterManifestDisplayChargeableWhenAir", Categories.Documents_Forwarding_Shipment_CoLoadMasterManifest, ResString.GetMultilingualString("fde4619a-3e07-402e-99f5-62d7e9935835", "Display Chargeable When Air"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem CoLoadMasterManifestDisplayChargeableWhenSea
		{
			get
			{
				return GetItem("CoLoadMasterManifestDisplayChargeableWhenSea", delegate
				{
					return new BooleanRegistryItem("CoLoadMasterManifestDisplayChargeableWhenSea", Categories.Documents_Forwarding_Shipment_CoLoadMasterManifest, ResString.GetMultilingualString("1594890a-7f8b-457f-81cc-2d0ee48e7a58", "Display Chargeable When Sea"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ConsolManifestConsolImportWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Manifest Consol/ImportWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(CombineCategories(ResString.GetMultilingualString("20812f84-0d0d-42ac-817c-644c794f4835", "Manifest Consol"), ResString.GetMultilingualString("ec9edafd-76d8-458a-b53d-79f2a753a93d", "Import")), Categories.Documents_Forwarding_Consol);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ConsolManifestConsolExportWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Manifest Consol/ExportWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(CombineCategories(ResString.GetMultilingualString("20812f84-0d0d-42ac-817c-644c794f4835", "Manifest Consol"), ResString.GetMultilingualString("4f813abc-f096-482b-91b8-c5871014abd3", "Export")), Categories.Documents_Forwarding_Consol);
				});
			}
		}

		internal IRegistryItem ConsolManifestConsolExportDisplayVolumewhenAir
		{
			get
			{
				return GetItem("DisplayVolumewhenAir", delegate
				{
					return new BooleanRegistryItem("DisplayVolumewhenAir", Categories.Documents_Forwarding_Consol_ManifestConsol_Export, ResString.GetMultilingualString("99dd7a96-e1c0-4f75-a271-44d84351bc3a", "Display Volume When Air"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem ConsolManifestConsolExportDisplayChargeableWhenAir
		{
			get
			{
				return GetItem("DisplayChargeableWhenAir", delegate
				{
					return new BooleanRegistryItem("DisplayChargeableWhenAir", Categories.Documents_Forwarding_Consol_ManifestConsol_Export, ResString.GetMultilingualString("fde4619a-3e07-402e-99f5-62d7e9935835", "Display Chargeable When Air"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem ConsolManifestConsolExportDisplayChargeableWhenSea
		{
			get
			{
				return GetItem("DisplayChargeableWhenSea", delegate
				{
					return new BooleanRegistryItem("DisplayChargeableWhenSea", Categories.Documents_Forwarding_Consol_ManifestConsol_Export, ResString.GetMultilingualString("1594890a-7f8b-457f-81cc-2d0ee48e7a58", "Display Chargeable When Sea"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ConsolForwardingInstructionWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Forwarding InstructionWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("e00cecaa-184f-4a1b-8566-2bce4e7a984e", "Forwarding Instruction"), Categories.Documents_Forwarding_Consol);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ConsolShipperDepartureNoticeWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Consol Shipper Departure NoticeWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("ed34aed1-8a85-4d1e-8d78-1332e71c511e", "Consol Shipper Departure Notice"), Categories.Documents_Forwarding_Consol);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ConsolAgentDepartureNoticeWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Agent Departure NoticeWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(Categories.AgentDepartureNotice, Categories.Documents_Forwarding_Consol);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ConsolCargoLoadListWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Cargo Load ListWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(ResString.GetMultilingualString("296df232-eefd-4c69-8240-ff6429484cb2", "Cargo Load List"), Categories.Documents_Forwarding_Consol);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Registry Item Key")]
		internal IRegistryItem ConsolLetterToOverseasAgentWeightAndVolumeDisplay
		{
			get
			{
				return GetItem("Letter To Overseas AgentWeightAndVolumeDisplay", delegate
				{
					return new WeightAndVolumeDisplayRegistryItem(Categories.LetterToOverseasAgent, Categories.Documents_Forwarding_Consol);
				});
			}
		}

		internal IRegistryItem PrintScannedUserSignatureOnQuotationDocs
		{
			get
			{
				return GetItem("PrintScannedUserSignatureOnQuotationDocs", delegate
				{
					return new BooleanRegistryItem("PrintScannedUserSignatureOnQuotationDocs", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("1af8f3fa-8457-4ff7-9449-a30fdadced6c", "Print Scanned User Signature"), ResString.GetMultilingualString("c50c08e0-6d2d-4bf9-bf1c-3566fc393128", "Indicates whether the signature stored on the staff profile will be printed on the Quotation Cover Page document."), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		#region UseFormBuilderHouseBills

		public BooleanRegistryItem UseFormBuilderHouseBills
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseFormBuilderHouseBills", () =>
					new BooleanRegistryItem(
						"UseFormBuilderHouseBills",
						Categories.Freight_HouseBills,
						ResString.GetMultilingualString("961455fc-dc85-407c-93dc-1ab85ecc9a0b", "Use new Form Builder House Bills"),
						ResString.GetMultilingualString("94a0293f-dae4-4cae-94fe-c19cedfdd803", "If turned on CW1 will use new Form Builder house bills."),
						RegistryStorageFlags.System,
						true));
			}
		}

		#endregion

		internal IRegistryItem NotifyPartyDefaultText
		{
			get
			{
				return GetItem("NotifyPartyDefaultText", delegate
				{
					return new MultilingualStringRegistryItem(
						"NotifyPartyDefaultText",
						Categories.Documents_Forwarding_Shipment_BillofLading,
						ResString.GetMultilingualString("ca636e17-b42b-4e93-bcd5-75f95dc11c52", "Notify Party Default Text"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled
							? RegistryOptions.IsHidden
							: (UseFormBuilderHouseBills.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default)
					);
				});
			}
		}

		internal IRegistryItem HAWBLogo
		{
			get
			{
				return GetItem("HAWBLogo", delegate
				{
					return new ImageRegistryItem("HAWBLogo",
						Categories.Freight_AWB_HAWB_Laser,
						ResString.GetMultilingualString("a31b412f-c258-47c0-b20d-c55039704af1", "Logo"), ResString.GetMultilingualString("4ef07b9b-d5f7-42d6-b29e-9eb1f968d1ae", "Choose a logo to be used to populate the top right of the Laser House AWB Document"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML Registry Data")]
		internal IRegistryItem HAWBDocumentTitles
		{
			get
			{
				return GetItem("HAWBDocumentTitles", delegate
				{
					string defaultHAWBDocumentTitlesData =
												@"<NewDataSet>

					<ListTable>
						<Name>Original 1 - (for Issuing Carrier)</Name>
						<Title>Original 1 - (for Issuing Carrier)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Original 2 - (for Consignee)</Name>
						<Title>Original 2 - (for Consignee)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Original 3 - (for Shipper)</Name>
						<Title>Original 3 - (for Shipper)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 4 - (Delivery Receipt)</Name>
						<Title>Copy 4 - (Delivery Receipt)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 5 - (Extra Copy)</Name>
						<Title>Copy 5 - (Extra Copy)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 6 - (Extra Copy)</Name>
						<Title>Copy 6 - (Extra Copy)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 7 - (Extra Copy)</Name>
						<Title>Copy 7 - (Extra Copy)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 8 - (for Agent)</Name>
						<Title>Copy 8 - (for Agent)</Title>
						<Printed>true</Printed>
					</ListTable>
				</NewDataSet>";

					byte[] defaultHAWBDocumentTitlesBytes = System.Text.Encoding.ASCII.GetBytes(defaultHAWBDocumentTitlesData);

					IRegistryItem result = new BinaryRegistryItem("HAWBDocumentTitles",
						Categories.Freight_AWB_HAWB_Laser,
						ResString.GetMultilingualString("1664d701-d2ad-448c-be3d-eb056c473c87", "Document Titles"), ResString.GetMultilingualString("3b4dd972-33cd-4e51-8b76-4aae263606c5", "The titles for each copy of the Laser House AWB document to be printed."), RegistryStorageFlags.Branch | RegistryStorageFlags.Company | RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, defaultHAWBDocumentTitlesBytes);
					result.EditorInfo = new HAWBDocumentPivotRegistryEditorInfo();
					return result;
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML Registry Data")]
		internal IRegistryItem MAWBDocumentTitles
		{
			get
			{
				return GetItem("MAWBDocumentTitles", delegate
				{
					var defaultMAWBDocumentTitlesData =
												@"<NewDataSet>

					<ListTable>
						<Name>Original 1 - (for Issuing Carrier)</Name>
						<Title>Original 1 - (for Issuing Carrier)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Original 2 - (for Consignee)</Name>
						<Title>Original 2 - (for Consignee)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Original 3 - (for Shipper)</Name>
						<Title>Original 3 - (for Shipper)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 4 - (Delivery Receipt)</Name>
						<Title>Copy 4 - (Delivery Receipt)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 5 - (Extra Copy)</Name>
						<Title>Copy 5 - (Extra Copy)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 6 - (Extra Copy)</Name>
						<Title>Copy 6 - (Extra Copy)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 7 - (Extra Copy)</Name>
						<Title>Copy 7 - (Extra Copy)</Title>
						<Printed>true</Printed>
					</ListTable>
					<ListTable>
						<Name>Copy 8 - (for Agent)</Name>
						<Title>Copy 8 - (for Agent)</Title>
						<Printed>true</Printed>
					</ListTable>
				</NewDataSet>";

					var defaultMAWBDocumentTitlesBytes = System.Text.Encoding.ASCII.GetBytes(defaultMAWBDocumentTitlesData);

					var result = new BinaryRegistryItem("MAWBDocumentTitles",
						Categories.Freight_AWB_MAWB,
						ResString.GetMultilingualString("42b383e3-e425-4ce3-84b8-974de6e37050", "Document Titles"), ResString.GetMultilingualString("a079c6db-b16d-4152-bd92-4946fa8ae699", "The titles for each copy of the Laser Master AWB document to be printed."), RegistryStorageFlags.Branch | RegistryStorageFlags.Company | RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, defaultMAWBDocumentTitlesBytes);
					result.EditorInfo = new MAWBDocumentPivotRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem RateLineSpacing
		{
			get
			{
				return GetItem("RateLineSpacing", delegate
				{
					return new IntRegistryItem("RateLineSpacing", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("669d115a-8381-4d45-92cb-706d52b8db12", "Line Spacing between Charge Codes"), ResString.GetMultilingualString("633b33b2-e829-4867-aaac-b06cef2ce3c3", "The number of lines to leave as spacing between each different charge code."), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 0);
				});
			}
		}

		internal IRegistryItem ExcelPasswordForOpening
		{
			get
			{
				return GetItem("ExcelPasswordForOpening", delegate
				{
					string defaultPassword = GetDefaultValueOfExcelPassword();
					IRegistryItem result = new StringRegistryItem("ExcelPasswordForOpening", Categories.Documents, ResString.GetMultilingualString("9a85149a-c3f9-4a8b-a271-5e235b992b59", "Excel Password Required for Opening"), ResString.GetMultilingualString("0ea8562a-f041-44c4-b913-fe3bb022213c", "This password will be required for opening a sheet on a password protected tab of a document or report delivered in Excel format."), RegistryStorageFlags.Company, defaultPassword);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					result.Options = RegistryOptions.IsPasswordVisibleForControllerUser;
					return result;
				});
			}
		}

		internal IRegistryItem ExcelPasswordForModifying
		{
			get
			{
				return GetItem("ExcelPasswordForModifying", delegate
				{
					var defaultPassword = GetDefaultValueOfExcelPassword();
					IRegistryItem result = new StringRegistryItem("ExcelPasswordForModifying", Categories.Documents, ResString.GetMultilingualString("CD16B793-A967-4E21-A754-E5AB046EA7F8", "Excel Password Required for Modifying"), ResString.GetMultilingualString("45A2CA61-5918-4524-AB1C-038995E4876C", "This password will be required for modifying a sheet on a password protected tab of a document or report delivered in Excel format."), RegistryStorageFlags.Company, defaultPassword);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					result.Options = RegistryOptions.IsPasswordVisibleForControllerUser;
					return result;
				});
			}
		}

		string GetDefaultValueOfExcelPassword()
		{
			var productRegistration = ObjectFactory.Get<IProductRegistration>().Key;
			var strValue = productRegistration.EnterpriseCode + productRegistration.ServerCode;
			return strValue.GetHashCode().ToString().Substring(1, 6);
		}

		#region Organisation Documents

		internal IRegistryItem RoutingOrderOpeningText
		{
			get
			{
				return GetItem("RoutingOrderOpeningText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("RoutingOrderOpeningText",
						Categories.Documents_Organization_RoutingOrder,
						ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"),
						ResString.GetMultilingualString("bd900434-e476-4a6f-a913-7a1a3c8a8f79", "The default opening text that will appear on the Routing Order document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("7cbb5642-49e1-42bc-b2a7-255fb675d37b", "Please note that all freight consignments of goods handled for our account are forwarded through the intermediary of:"));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem RoutingOrderClosingText
		{
			get
			{
				return GetItem("RoutingOrderClosingText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("RoutingOrderClosingText",
						Categories.Documents_Organization_RoutingOrder,
						ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"),
						ResString.GetMultilingualString("defda0cc-ad32-4000-9914-8c86ac1d6774", "The default closing text that will appear on the Routing Order document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("f258a043-3e3e-4746-9fb4-3dc07ae780dd", "We have experienced that this office provides a high quality, reliable and competitive service. We recommended them for your future international trade and logistics tasks."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem AgentReplacementRoutingOrderOpeningText
		{
			get
			{
				return GetItem("AgentReplacementRoutingOrderOpeningText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("AgentReplacementRoutingOrderOpeningText",
						Categories.Documents_Organization_AgentReplacementRoutingOrder,
						ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"),
						ResString.GetMultilingualString("ed41c958-8a3e-4392-8587-a0eb3c299e2f", "The default opening text that will appear on the Agent Replacement Routing Order document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem AgentReplacementRoutingOrderClosingText
		{
			get
			{
				return GetItem("AgentReplacementRoutingOrderClosingText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("AgentReplacementRoutingOrderClosingText",
						Categories.Documents_Organization_AgentReplacementRoutingOrder,
						ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"),
						ResString.GetMultilingualString("99ed7224-9dc6-4e35-bed9-f4831abe32a3", "The default closing text that will appear on the Agent Replacement Routing Order document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem RoutingRecommendationOpeningText
		{
			get
			{
				return GetItem("RoutingRecommendationOpeningText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("RoutingRecommendationOpeningText",
						Categories.Documents_Organization_RoutingRecommendation,
						ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("e7924583-ef73-4434-a92f-fbb72e0b760d", "The default opening text that will appear on the Routing Recommendation document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("a3c378b1-f405-4a3e-bc22-355b92efa8b3", "We would like to take this opportunity to introduce you to our forwarding partner and recommend their use for future shipments."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem RoutingRecommendationClosingText
		{
			get
			{
				return GetItem("RoutingRecommendationClosingText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("RoutingRecommendationClosingText",
						Categories.Documents_Organization_RoutingRecommendation,
						ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"),
						ResString.GetMultilingualString("d8460fe1-8e7e-4c5c-abd1-4fa3c9e7873d", "The default closing text that will appear on the Routing Recommendation document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("f258a043-3e3e-4746-9fb4-3dc07ae780dd", "We have experienced that this office provides a high quality, reliable and competitive service. We recommended them for your future international trade and logistics tasks."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region Customs Documents

		internal IRegistryItem CustomsRequestForMissingDocumentsOpeningText
		{
			get
			{
				return GetItem("RequestForMissingDocumentsOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("RequestForMissingDocumentsOpeningText", Categories.Documents_Forwarding_Customs_RequestforMissingDocuments, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("d95cbe18-01f6-4749-9dc4-17a004f622d6", "Opening text for 'Request for Missing Documents' document in Customs."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CustomsRequestForMissingDocumentsClosingText
		{
			get
			{
				return GetItem("RequestForMissingDocumentsClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("RequestForMissingDocumentsClosingText", Categories.Documents_Forwarding_Customs_RequestforMissingDocuments, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("4eec7707-1cb3-4f69-a0f7-21c5e5b52342", "Closing text for 'Request for Missing Documents' document in Customs."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CustomsRequestForMissingDocumentsClause
		{
			get
			{
				return GetItem("CustomsRequestForMissingDocumentsClause", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("CustomsRequestForMissingDocumentsClause", Categories.Documents_Forwarding_Customs_RequestforMissingDocuments, ResString.GetMultilingualString("00b1289b-3db9-4872-8a63-079a648a2d65", "Clause"), ResString.GetMultilingualString("9ed5fb5c-4d67-45ca-9d43-4027eab12e91", "Clause for 'Request for Missing Documents' document in Customs."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, RegistryConstants.Strings.DefaultRequestForMissingDocumentsClause);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		internal IRegistryItem SeaWeightsAndMeasurementsOpeningText
		{
			get
			{
				return GetItem("SeaWeightsAndMeasurementsOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("SeaWeightsAndMeasurementsOpeningText", Categories.Documents_Forwarding_Shipment_WeightsAndMeasurements_ExportSea, RegistryConstants.Strings.OpeningText, null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem SeaWeightsAndMeasurementsClosingText
		{
			get
			{
				return GetItem("SeaWeightsAndMeasurementsClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("SeaWeightsAndMeasurementsClosingText", Categories.Documents_Forwarding_Shipment_WeightsAndMeasurements_ExportSea, RegistryConstants.Strings.ClosingText, null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem AirWeightsAndMeasurementsOpeningText
		{
			get
			{
				return GetItem("AirWeightsAndMeasurementsOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("AirWeightsAndMeasurementsOpeningText", Categories.Documents_Forwarding_Shipment_WeightsAndMeasurements_ExportAir, RegistryConstants.Strings.OpeningText, null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem AirWeightsAndMeasurementsClosingText
		{
			get
			{
				return GetItem("AirWeightsAndMeasurementsClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("AirWeightsAndMeasurementsClosingText", Categories.Documents_Forwarding_Shipment_WeightsAndMeasurements_ExportAir, RegistryConstants.Strings.ClosingText, null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem AWBSecurityDeclarationOpeningText
		{
			get
			{
				return GetItem("AWBSecurityDeclarationOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("AWBSecurityDeclarationOpeningText", Categories.Documents_Forwarding_Shipment_AWBSecurityDeclaration, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("8389aebf-9f16-4e90-890c-e392fdf32ce8", "Default opening text for AWB Security Declaration."), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem PDFTIFColourDepth
		{
			get
			{
				return GetItem("PDFTIFColourDepth", delegate
				{
					IRegistryItem result = new StringRegistryItem("PDFTIFColourDepth", Categories.Documents_PrintOptions, ResString.GetMultilingualString("51045175-1830-48a3-9aa8-5fd76a37da5f", "Emailed TIF file color depth"), ResString.GetMultilingualString("e71dbeae-d35a-4f2c-bb66-d39390effee2", "TIFs will be emailed out of the application at the selected color depth. Select 256 color if you would like your company logo to be delivered in color. Black and white delivery will result in a smaller file size."), RegistryStorageFlags.System, Constants.ColourDepth.Colour256);
					result.EditorInfo = new ComboBoxRegistryEditorInfo(OLookUpEditType.ColourDepth, false);
					return result;
				});
			}
		}

		internal IRegistryItem PDFTIFResolution
		{
			get
			{
				return GetItem("PDFTIFResolution", delegate
				{
					return new IntRegistryItem("PDFTIFResolution", Categories.Documents_PrintOptions, ResString.GetMultilingualString("1f431601-4bd8-44f7-a1c1-939ce477e2d6", "PDF/TIF Resolution"), ResString.GetMultilingualString("c1af9c8a-a9ea-4e61-a9d0-fb588c9068ec", "dots per inch"), RegistryStorageFlags.System, RegistryOptions.Default, 200, 1, int.MaxValue);
				});
			}
		}

		internal IRegistryItem CFSCartageAdviceOpeningText
		{
			get
			{
				return GetItem("CFSCartageAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CFSCartageAdviceOpeningText", Categories.Documents_CFS_CartageAdvice, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("c45d9c31-6d6f-4009-b420-c0ef96c54e5a", "Cartage Advice Opening Text"), RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CFSCartageAdviceClosingText
		{
			get
			{
				return GetItem("CFSCartageAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CFSCartageAdviceClosingText", Categories.Documents_CFS_CartageAdvice, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("8e5fbdd7-7507-4c45-92ba-4c2c0d19daee", "Cartage Advice Closing Text"),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem BookingCartageAdviceOpeningText
		{
			get
			{
				return GetItem("BookingCartageAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("BookingCartageAdviceOpeningText", Categories.Documents_Booking_CartageAdvice, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("c45d9c31-6d6f-4009-b420-c0ef96c54e5a", "Cartage Advice Opening Text"),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem BookingCartageAdviceClosingText
		{
			get
			{
				return GetItem("BookingCartageAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("BookingCartageAdviceClosingText", Categories.Documents_Booking_CartageAdvice, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("8e5fbdd7-7507-4c45-92ba-4c2c0d19daee", "Cartage Advice Closing Text"),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem LocalCartageCartageAdviceOpeningText
		{
			get
			{
				return GetItem("LocalCartageCartageAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("LocalCartageCartageAdviceOpeningText", Categories.Documents_PortTransport_CartageAdvice, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("c45d9c31-6d6f-4009-b420-c0ef96c54e5a", "Cartage Advice Opening Text"),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem LocalCartageCartageAdviceClosingText
		{
			get
			{
				return GetItem("LocalCartageCartageAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("LocalCartageCartageAdviceClosingText", Categories.Documents_PortTransport_CartageAdvice, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("8e5fbdd7-7507-4c45-92ba-4c2c0d19daee", "Cartage Advice Closing Text"),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CustomsDeclarationCartageAdviceOpeningText
		{
			get
			{
				return GetItem("CustomsDeclarationCartageAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CustomsDeclarationCartageAdviceOpeningText", Categories.Documents_Forwarding_Customs_CartageAdvice, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("c45d9c31-6d6f-4009-b420-c0ef96c54e5a", "Cartage Advice Opening Text"),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem CustomsDeclarationCartageAdviceClosingText
		{
			get
			{
				return GetItem("CustomsDeclarationCartageAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("CustomsDeclarationCartageAdviceClosingText", Categories.Documents_Forwarding_Customs_CartageAdvice, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("8e5fbdd7-7507-4c45-92ba-4c2c0d19daee", "Cartage Advice Closing Text"),
						RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ShipmentRequestForMissingDocumentsOpeningText
		{
			get
			{
				return GetItem("ShipmentRequestForMissingDocumentsOpeningText", delegate
				{
					return new MultilingualStringRegistryItem("ShipmentRequestForMissingDocumentsOpeningText", Categories.Documents_Forwarding_Shipment_RequestforMissingDocuments, RegistryConstants.Strings.OpeningText, ResString.GetMultilingualString("b908deea-ccb2-4429-b3a8-5f5d22cae088", "Opening text for 'Request for Missing Documents' document in Shipment."), new StringRegistryDataType(), new TextRegistryEditorInfo(TextEditorType.Memo), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, null);
				});
			}
		}

		internal IRegistryItem ShipmentRequestForMissingDocumentsClosingText
		{
			get
			{
				return GetItem("ShipmentRequestForMissingDocumentsClosingText", delegate
				{
					return new MultilingualStringRegistryItem("ShipmentRequestForMissingDocumentsClosingText", Categories.Documents_Forwarding_Shipment_RequestforMissingDocuments, RegistryConstants.Strings.ClosingText, ResString.GetMultilingualString("4ca555e5-0435-4c1a-bbca-38d9327eabc5", "Closing text for 'Request for Missing Documents' document in Shipment."), new StringRegistryDataType(), new TextRegistryEditorInfo(TextEditorType.Memo), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, null);
				});
			}
		}

		internal IRegistryItem ShipmentRequestForMissingDocumentsClause
		{
			get
			{
				return GetItem("RequestForMissingDocumentsInstruction", delegate
				{
					return new MultilingualStringRegistryItem("RequestForMissingDocumentsInstruction", Categories.Documents_Forwarding_Shipment_RequestforMissingDocuments, ResString.GetMultilingualString("00b1289b-3db9-4872-8a63-079a648a2d65", "Clause"), ResString.GetMultilingualString("be61df23-6d6a-4aa8-a223-212e58362bde", "Clause for 'Request for Missing Documents' document in Shipment."), new StringRegistryDataType(), new TextRegistryEditorInfo(TextEditorType.Memo), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, RegistryConstants.Strings.DefaultRequestForMissingDocumentsClause);
				});
			}
		}

		internal IRegistryItem AuthorisationReleaseClause
		{
			get
			{
				return GetItem("AuthorisationReleaseClause", delegate
				{
					return new MultilingualStringRegistryItem("AuthorisationReleaseClause", Categories.Documents_Forwarding_Consol, ResString.GetMultilingualString("a6e85565-b05a-4343-871c-2b334ce5bc20", "Authorization Release Clause"), ResString.GetMultilingualString("96e5150c-ea26-4acb-b674-79765b8b0228", "Text to be inserted in an 'Authorization Release' document in Consol."), new StringRegistryDataType(), new TextRegistryEditorInfo(TextEditorType.Memo), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("260f5124-0b7a-40dc-a9b6-5049431d9776", "WE WILL SUBMIT INWARD PERMIT(S) TO YOU ONCE AVAILABLE.\r\nIN VIEW OF THE ABOVE, WE HEREBY UNDERTAKE AND AGREE TO INDEMNIFY YOU FULLY AGAINST ALL CONSEQUENCES AND LIABILITIES THAT MAY ARISE DIRECTLY OR INDIRECTLY."));
				});
			}
		}

		#region Quotation Document

		internal IRegistryItem QuoteDocumentHeading
		{
			get
			{
				return GetItem("QuoteDocumentHeading", delegate
				{
					return new MultilingualStringRegistryItem("QuoteDocumentHeading", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("c08b54b6-60d8-4ab7-b68a-5cfd025b6092", "Quotation Documentation Title"), ResString.GetMultilingualString("8964eb59-ce4d-4bf7-8bba-8dea27f463e3", "The title text that should appear on quotation documentation."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("7943c39c-36b4-4326-bc0e-063bfa6d3b68", "Quotation"));
				});
			}
		}

		internal IRegistryItem OneOffQuoteDocumentHeading
		{
			get
			{
				return GetItem("OneOffQuoteDocumentHeading", delegate
				{
					return new MultilingualStringRegistryItem("OneOffQuoteDocumentHeading", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("8a743213-5a56-4573-ab2f-159ba6b71a91", "Quotation Documentation Title (One Off Quotes)"), ResString.GetMultilingualString("ad7f8bca-4edb-46c0-a85d-a793eeb3b9c1", "The title text that should appear on one off quotation documentation."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("7943c39c-36b4-4326-bc0e-063bfa6d3b68", "Quotation"));
				});
			}
		}

		internal IRegistryItem QuotationDocumentLogo
		{
			get
			{
				return GetItem("QuotationDocumentLogo", delegate
				{
					return new ImageRegistryItem("QuotationDocumentLogo", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("1654dcc7-f082-4afd-a235-2c0e5912bf80", "Quotation Document Logo"), ResString.GetMultilingualString("7bc3b1ff-84c3-4088-9422-8fde63a5a965", "Logo to be displayed on top of the Quotation Document."), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteHeaderText
		{
			get
			{
				return GetItem("QuoteHeaderText", delegate
				{
					return new MultilingualStringRegistryItem("QuoteHeaderText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("f5c8d914-c674-4c6d-ae90-2b2bcd5485fb", "Quote Entry Header Text"), null, new StringRegistryDataType(0, 80), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, null);
				});
			}
		}

		internal IRegistryItem QuoteCoverPageFooterText
		{
			get
			{
				return GetItem("QuoteCoverPageFooterText", delegate
				{
					return new MultilingualStringRegistryItem("QuoteCoverPageFooterText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("344c47ea-b16c-4bb9-9ef2-04b0d7889a95", "Cover Page Footer Text"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("f8d686b1-644d-4ff9-ac88-8752cc251efd", "This quotation is subject to our Standard Terms and Conditions which are available on request."));
				});
			}
		}

		internal IRegistryItem GRIUpdateCoverPageText
		{
			get
			{
				return GetItem("GRIUpdateCoverPageText", delegate
				{
					IRegistryItem result = new MultilingualStringRegistryItem("GRIUpdateCoverPageText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("a12c067e-b971-4da9-8ccf-148072667f5d", "Rate Update Cover Page Text"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		internal IRegistryItem GRIUpdateFooterPageText
		{
			get
			{
				return GetItem("GRIUpdateFooterPageText", delegate
				{
					return new MultilingualStringRegistryItem("GRIUpdateFooterPageText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("ea913385-3a2e-4684-91af-6ad90baa82f3", "Rate Update Footer Page Text"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#region Trailing Pages

		#region Terms and Conditions

		internal IRegistryItem QuoteTCPage1
		{
			get
			{
				return GetItem("QuoteTCPage1", delegate
				{
					return new ImageRegistryItem("QuoteTCPage1", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("15e340f6-4e62-49ee-9f03-e845afa84883", "Trailing Page 1"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteTCPage2
		{
			get
			{
				return GetItem("QuoteTCPage2", delegate
				{
					return new ImageRegistryItem("QuoteTCPage2", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("3363d6d9-0d8d-4047-8339-c277474982e1", "Trailing Page 2"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteTCPage3
		{
			get
			{
				return GetItem("QuoteTCPage3", delegate
				{
					return new ImageRegistryItem("QuoteTCPage3", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("d303e5e8-6669-4a26-b791-7cc5d586606b", "Trailing Page 3"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteTCPage4
		{
			get
			{
				return GetItem("QuoteTCPage4", delegate
				{
					return new ImageRegistryItem("QuoteTCPage4", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("48e6949c-4a3f-4dee-990b-93b49ac0106c", "Trailing Page 4"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteTCPage5
		{
			get
			{
				return GetItem("QuoteTCPage5", delegate
				{
					return new ImageRegistryItem("QuoteTCPage5", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("ba736821-2107-463e-94c3-cdc2f9da5bd0", "Trailing Page 5"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Other Pages

		internal IRegistryItem QuoteOtherPage1
		{
			get
			{
				return GetItem("QuoteOtherPage1", delegate
				{
					return new ImageRegistryItem("QuoteOtherPage1", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("ed1483e1-e8b6-429f-a092-b13fca98c69b", "Trailing Page 6"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteOtherPage2
		{
			get
			{
				return GetItem("QuoteOtherPage2", delegate
				{
					return new ImageRegistryItem("QuoteOtherPage2", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("a7d7a57a-1a23-4089-945c-a24b3c9330c9", "Trailing Page 7"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteOtherPage3
		{
			get
			{
				return GetItem("QuoteOtherPage3", delegate
				{
					return new ImageRegistryItem("QuoteOtherPage3", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("5b99145c-a3f9-4e6e-bcb5-b39ca28cd86e", "Trailing Page 8"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteOtherPage4
		{
			get
			{
				return GetItem("QuoteOtherPage4", delegate
				{
					return new ImageRegistryItem("QuoteOtherPage4", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("3228be92-002b-43c9-8426-e6f98bfd7ead", "Trailing Page 9"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem QuoteOtherPage5
		{
			get
			{
				return GetItem("QuoteOtherPage5", delegate
				{
					return new ImageRegistryItem("QuoteOtherPage5", Categories.Documents_QuotationsandRates_TrailingPages, ResString.GetMultilingualString("bcab7a2a-2497-4841-8da0-d3b11b24db18", "Trailing Page 10"), null, RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#endregion

		internal IRegistryItem NotChargedText
		{
			get
			{
				return GetItem("NotChargedText", delegate
				{
					return new MultilingualStringRegistryItem("NotChargedText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("a12f84cc-0724-4578-b2de-9a9d68dc46f3", "Not Charged Text"), ResString.GetMultilingualString("36f6ed4c-df2f-4930-b004-b88dc0fa1b94", "This is the text that will appear on the quotation when user rates something at $0."), RegistryStorageFlags.Company | RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("58e65aea-6fc3-4912-ac4d-ab9b89cdde4e", "Not Charged"));
				});
			}
		}

		internal IRegistryItem FlatFeeText
		{
			get
			{
				return GetItem("FlatFeeText", delegate
				{
					return new MultilingualStringRegistryItem("FlatFeeText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("f87d41b2-3ff3-45a7-a196-08fdc5eb9e9b", "Flat / Base Fee Units Text"), ResString.GetMultilingualString("c131b293-4abc-4d62-8776-c05e97e50dbe", "This is the text that will appear next to any charges that are FLAT or BASE fees."), RegistryStorageFlags.Company | RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem PublishedAgentsHeadingText
		{
			get
			{
				return GetItem("PublishedAgentsHeadingText", delegate
				{
					return new MultilingualStringRegistryItem("PublishedAgentsHeadingText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("32f9a985-06b6-480b-99ba-af1b08a7fb80", "Published Agents Listing Heading Text"), ResString.GetMultilingualString("819c6ce5-ca67-41b7-a224-3134d22e82ad", "This is the text that will appear as the heading on the list of published agents printed with the Quotation document."), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, ResString.GetMultilingualString("3de16ea1-b371-4f94-b0f9-5340fec30b8f", "Recommended Agents"));
				});
			}
		}

		internal IRegistryItem IncludeCFXOnQuotation
		{
			get
			{
				return GetItem("IncludeCFXOnQuotation", delegate
				{
					return new BooleanRegistryItem("IncludeCFXOnQuotation", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("fcf17694-ccca-4648-821d-2d9c8c8e5414", "Include Currency Uplift (CFX) on Quotation Documentation"), ResString.GetMultilingualString("f142f6fa-cf88-4b03-bfe2-7f91526672e4", "This will determine whether the client's CFX information will be printed on the Quotation document"), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		#endregion

		#region Order Documents

		internal IRegistryItem ImportOrderAdviceOpeningText
		{
			get
			{
				return GetItem("ImportOrderAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportOrderAdviceOpeningText", Categories.Documents_Orders_ImportOrderAdvice, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("e0994416-ddd1-4837-a6c5-5be3a87d8d06", "Import Order Advice Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportOrderAdviceClosingText
		{
			get
			{
				return GetItem("ImportOrderAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportOrderAdviceClosingText", Categories.Documents_Orders_ImportOrderAdvice, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("8cee7fcb-a32c-4e14-83e2-53da8a569acb", "Import Order Advice Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ExportOrderAdviceOpeningText
		{
			get
			{
				return GetItem("ExportOrderAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ExportOrderAdviceOpeningText", Categories.Documents_Orders_ExportOrderAdvice, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("179e6644-c97e-49a1-8d52-0db3e07a8c26", "Export Order Advice Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ExportOrderAdviceClosingText
		{
			get
			{
				return GetItem("ExportOrderAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ExportOrderAdviceClosingText", Categories.Documents_Orders_ExportOrderAdvice, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("276d24f2-fecd-4402-9fed-4942971a130b", "Export Order Advice Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportOrderNotificationOpeningText
		{
			get
			{
				return GetItem("ImportOrderNotificationOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportOrderNotificationOpeningText", Categories.Documents_Orders_ImportOrderNotification, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("472af01e-eb9e-4ab9-a8da-62f06acc05da", "Import Order Notification Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportOrderNotificationClosingText
		{
			get
			{
				return GetItem("ImportOrderNotificationClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportOrderNotificationClosingText", Categories.Documents_Orders_ImportOrderNotification, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("81e6218e-1589-4b7b-8e4f-78a2fd24f19e", "Import Order Notification Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ExportOrderNotificationOpeningText
		{
			get
			{
				return GetItem("ExportOrderNotificationOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ExportOrderNotificationOpeningText", Categories.Documents_Orders_ExportOrderNotification, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("f39da3b4-3dac-434d-a901-01577dbedb70", "Export Order Notification Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ExportOrderNotificationClosingText
		{
			get
			{
				return GetItem("ExportOrderNotificationClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ExportOrderNotificationClosingText", Categories.Documents_Orders_ExportOrderNotification, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("cbaca1e6-6132-4f81-b9d2-c0bc97a55ce9", "Export Order Notification Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportOrderStatusOpeningText
		{
			get
			{
				return GetItem("ImportOrderStatusOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportOrderStatusOpeningText", Categories.Documents_Orders_ImportOrderStatus, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("f91c2152-839b-47f4-9c11-3a65052ee1ac", "Import Order Status Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportOrderStatusClosingText
		{
			get
			{
				return GetItem("ImportOrderStatusClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportOrderStatusClosingText", Categories.Documents_Orders_ImportOrderStatus, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("d9fd803c-b457-4d22-b4ca-03005c444ccd", "Import Order Status Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ExportOrderStatusOpeningText
		{
			get
			{
				return GetItem("ExportOrderStatusOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ExportOrderStatusOpeningText", Categories.Documents_Orders_ExportOrderStatus, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("2416dd5a-2c94-4d91-b1b7-8a397e6ab3bc", "Export Order Status Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ExportOrderStatusClosingText
		{
			get
			{
				return GetItem("ExportOrderStatusClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ExportOrderStatusClosingText", Categories.Documents_Orders_ExportOrderStatus, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("2404a426-416b-49a6-99c4-9b848f94ca61", "Export Order Status Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportShippedOnBoardAdviceOpeningText
		{
			get
			{
				return GetItem("ImportShippedOnBoardAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportShippedOnBoardAdviceOpeningText", Categories.Documents_Orders_ImportShippedOnBoardAdvice, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("0e0fa8b0-7d05-45b5-bfdc-816eaeae7ba3", "Import Shipped On Board Advice Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportShippedOnBoardAdviceClosingText
		{
			get
			{
				return GetItem("ImportShippedOnBoardAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportShippedOnBoardAdviceClosingText", Categories.Documents_Orders_ImportShippedOnBoardAdvice, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("32f8292f-2290-42bc-bb44-2dbf9ac49f86", "Import Shipped On Board Advice Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportAmendmentToBookingOpeningText
		{
			get
			{
				return GetItem("ImportAmendmentToBookingOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportAmendmentToBookingOpeningText", Categories.Documents_Orders_ImportAmendmentToBooking, ResString.GetMultilingualString("4f3e194b-ba15-4cf4-b5c9-754537bfe145", "Opening Text"), ResString.GetMultilingualString("c6128386-6d82-4bee-b91f-3e66e50ea4b2", "Import Amendment To Booking Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		internal IRegistryItem ImportAmendmentToBookingClosingText
		{
			get
			{
				return GetItem("ImportAmendmentToBookingClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("ImportAmendmentToBookingClosingText", Categories.Documents_Orders_ImportAmendmentToBooking, ResString.GetMultilingualString("9bd8c743-09ea-41f5-a74a-33671be2acdd", "Closing Text"), ResString.GetMultilingualString("a35792d0-ac5e-4274-806d-aa7908c95df5", "Import Amendment To Booking Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Template cache timeout

		public IntRegistryItem DocumentCustomisationVersionNumber
		{
			get
			{
				return GetItem("DocumentCustomisationVersionNumber", () =>
				{
					return new IntRegistryItem("DocumentCustomisationVersionNumber",
						Categories.Documents,
						(NoResString)"Version number for document customisations", // This is a hidden registry item
						(NoResString)"This number is incremented whenever anyone saves the document customisation form, indicating that other application systems on this database should drop their template caches.", // This is a hidden registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#endregion

		#region Customs

		#region Common

		#region Consolidated Entries

		#region Enable Consolidated Entries
		public BooleanRegistryItem EnableConsolidatedEntries
		{
			get
			{
				return GetItem("EnableConsolidatedEntries", delegate
				{
					return new BooleanRegistryItem("EnableConsolidatedEntries",
						Categories.Customs_ConsolidatedEntries,
						ResString.GetMultilingualString("59A5C573-79DA-40BB-BFD6-1DFEF4AF6CBD", "Enable Consolidated Entries"),
						ResString.GetMultilingualString("7E80A5EF-9C22-40A3-8EBA-FE1CD74A00B4", "Set to Yes to enable Consolidated Entries for the system."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}
		#endregion

		#endregion

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem EnableCustomsDiagnostics
		{
			get
			{
				return GetItem("EnableCustomsDiagnostics1007", delegate
				{
					return new BooleanRegistryItem(
						"EnableCustomsDiagnostics1007",
						Categories.Customs,
						(NoResString)"Enable Customs Diagnostics",
						(NoResString)"Set this to 'Yes' to log diagnostic information and send a developer error when certain events occur.",
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		internal IRegistryItem CommercialInvoiceLineMergeMethod
		{
			get
			{
				return GetItem("CommercialInvoiceLineMergeMethod", delegate
				{
					return new MergeMethodRegistryItem(
						"CommercialInvoiceLineMergeMethod",
						Categories.Customs,
						ResString.GetMultilingualString("abe244da-e93d-4581-906e-bb941eda6040", "Commercial Invoice Line Merge Method"),
						ResString.GetMultilingualString("9fd49e70-d5f9-4e13-9e4c-49fe0b384819", "The way a commercial invoice is merged to produce the entry lines"),
						new CountrySepecificMergeMethodListProvider(OLookUpEditType.CommercialInvoiceMergeMethod),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge);
				});
			}
		}

		internal BooleanRegistryItem LandedCostingFallbackExRatesToJobInvoicing
		{
			get
			{
				return GetItem("LandedCostingFallbackExRatesToJobInvoicing", delegate
				{
					return new BooleanRegistryItem("LandedCostingFallbackExRatesToJobInvoicing", Categories.Customs, ResString.GetMultilingualString("dea4f838-56ba-43b8-bb84-c1a2c660285e", "Fallback Exchange Rates to Job Invoicing"), ResString.GetMultilingualString("36b4d5ea-c535-4ec2-b11d-2106cc6c892e", "Choose yes to use exchange rates from the job invoicing tab instead of from the invoice header"), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		internal IntRegistryItem MessagesPerInterchange
		{
			get
			{
				return GetItem("MessagesPerInterchange", delegate
				{
					return new IntRegistryItem("MessagesPerInterchange", Categories.Customs, ResString.GetMultilingualString("2051c4f9-3724-46ff-b8e9-1f4dd9ab5367", "Messages Per Interchange"), ResString.GetMultilingualString("24a4b7fb-2749-4090-b0bf-607616b47324", "Maximum number of Messages per Interchange."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 50);
				});
			}
		}

		internal IntRegistryItem InterchangesPerRun
		{
			get
			{
				return GetItem("InterchangesPerRun", delegate
				{
					return new IntRegistryItem("InterchangesPerRun", Categories.Customs, ResString.GetMultilingualString("21e06573-c0b6-45fb-afe7-b99039046ecd", "Interchanges per Run"), ResString.GetMultilingualString("edcdc2e6-2cd7-4d11-ac81-566616b8ccad", "Maximum number of Interchanges processed per Batch Processor run."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 10);
				});
			}
		}

		#endregion

		#region Australia

		internal IRegistryItem BrokerageID
		{
			get
			{
				return GetItem("BrokerageID", delegate
				{
					IRegistryItem result = new StringRegistryItem("BrokerageID", Categories.Customs_Australia, ResString.GetMultilingualString("ba567f1f-ac63-4776-b221-db23d4910959", "Brokerage ID"), ResString.GetMultilingualString("32b27f92-c906-47bc-a0ff-1db3d0bf1e26", "The customs brokerage license number."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem RequestOfficialCustomsPaymentReceipt
		{
			get
			{
				return GetItem("RequestOfficialCustomsPaymentReceipt", delegate
				{
					return new BooleanRegistryItem("RequestOfficialCustomsPaymentReceipt", Categories.Customs_Australia, ResString.GetMultilingualString("7bf638c7-7e04-4565-858f-5993153596b4", "Request Official Customs Payment Receipt"), ResString.GetMultilingualString("9e186795-0503-41c3-bfb3-ea45f6328442", "Enables the CMR automatic request for Customs Official Receipt whenever a payment, or lodge with payment, message is sent to Customs.  The Official Receipt will be generated by Customs and sent to the text Email address specified on the default user site defined at Customs. If this Email address is the same as the application Email address (in the user site) then the Receipt will be delivered into eDocs of the relevant Job."), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem UpdateCMRReferenceFilesWhenModifiedByCustoms
		{
			get
			{
				return GetItem("UpdateCMRReferenceFilesWhenModifiedByCustoms", delegate
				{
					return new BooleanRegistryItem("UpdateCMRReferenceFilesWhenModifiedByCustoms", Categories.Customs_Australia, ResString.GetMultilingualString("fa9ea1f4-e65e-4635-80d9-6a1d11200d67", "Update CMR Reference Files whenever they are modified by Customs"), ResString.GetMultilingualString("b5154feb-bb72-4bb4-a6ca-551dec3759cb", "Enables the CMR Reference Files auto-update mode which checks every 15 minutes on the Customs web site for the last modified time of the CMR Reference Files package. The package will be automatically downloaded and imported whenever any modification is discovered."), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		public IRegistryItem InterchangeResendDelay
		{
			get
			{
				return GetItem("InterchangeResendDelay", delegate
				{
					IRegistryItem result = new IntRegistryItem("InterchangeResendDelay", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("9644715d-94ff-4f90-92cf-1d4b9bace33e", "Interchange Resend Delay"), ResString.GetMultilingualString("73ee0434-b5e7-443a-a896-e8dd1b0358ed", "This is the time period in minutes that the batch processor will wait before resending an unacknowledged interchange to Australian Customs"), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 30);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem InterchangeMaxSends
		{
			get
			{
				return GetItem("InterchangeMaxSends", delegate
				{
					IRegistryItem result = new IntRegistryItem("InterchangeMaxSends", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("c4f80972-de3b-402f-aa77-2fba8dee8f0a", "Interchange Maximum Sends"), ResString.GetMultilingualString("872eb06c-dc72-4fb3-baa8-3eb16c0ecab0", "This is the maximum number of times an unacknowledged interchange will be sent to Australian Customs before the batch processor gives up and flags an error on the interchange"), null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 3, 1, 3);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public BinaryRegistryItem AUCCompanyCertificateData
		{
			get
			{
				return GetItem("AUCCompanyCertificateData", delegate
				{
					BinaryRegistryItem result = new BinaryRegistryItem("AUCCompanyCertificateData", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("1eb59d04-41d1-42b4-a5ec-786c9a64e926", "Company Key File"), ResString.GetMultilingualString("13c94be7-ded6-4c2b-bfa5-120d54b404a6", "The certificate to use for signing data sent to Customs or decrypting data received from Customs."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new FileUpLoaderX509CertificateRegistryEditorInfo(AUCCompanyCertificatePassword);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem AUCCompanyCertificatePassword
		{
			get
			{
				return GetItem("AUCCompanyCertificatePassword", delegate
				{
					IRegistryItem result = new StringRegistryItem("AUCCompanyCertificatePassword", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("78cce8b3-5e19-4d39-aa4a-52ace130d9f5", "Private Key Password"), ResString.GetMultilingualString("e8b93c14-dba9-41aa-ac15-d8ac036b34bb", "The password used to access the private key."), new CertificatePasswordValidator(), null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, "");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem AUCCarrierMovementAdviceGroup
		{
			get
			{
				return GetItem("AUCCarrierMovementAdviceGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem("AUCCarrierMovementAdviceGroup", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("e740369e-3026-46c1-9522-fd17f0e652a6", "Carrier Movement Advice Email Group"), ResString.GetMultilingualString("3c479f3e-9111-43ad-b29b-16be64d8d9a2", "The staff group that carrier movement advice messages received from Customs will be forwarded to."), new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Guid.Empty);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem AUCAlertCarrierMovementLoadStatus
		{
			get
			{
				return GetItem("AUCAlertCarrierMovementLoadStatus", delegate
				{
					IRegistryItem result = new BooleanRegistryItem("AUCAlertCarrierMovementLoadStatus", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("ba5c1eff-9de8-46e4-a80e-078f9933bee9", "Alert Carrier Movement Load Status"), ResString.GetMultilingualString("0b41a2ba-b5b3-419d-b475-b826810cac91", "Do you want to receive email notification of LOAD statuses for CANs received by a CTO."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, false);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem AUCHVLVSpecialReporterNumber
		{
			get
			{
				return GetItem("AUCSpecialReporterNumber", delegate
				{
					IRegistryItem result = new StringRegistryItem("AUCSpecialReporterNumber", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("3b175dd0-4a6f-4e9b-b3fe-959b0bfe70f8", "HVLV Special Reporter Number"), ResString.GetMultilingualString("aabc2a02-075f-461a-a517-51178ee6b439", "This number is a unique number generated by AU Customs, and is given to a HVLV Special Reporter to be used when reporting abbreviated cargo reports."), new AlphaNumericCodeRegistryDataType(6, 6), null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, "");
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem AUCRemailSpecialReporterNumber
		{
			get
			{
				return GetItem("AUCRemailSpecialReporterNumber", delegate
				{
					var result = new StringRegistryItem("AUCRemailSpecialReporterNumber", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("AD0E4BF8-ECBA-4AD0-8ECF-CE8326373EE8", "Re-mail Special Reporter Number"), ResString.GetMultilingualString("6CC449AF-C3B7-4575-A749-6B6DF5FD4FB4", "This number is a unique number generated by AU Customs, and is given to a Re-mail Special Reporter to be used when reporting abbreviated cargo reports."), new AlphaNumericCodeRegistryDataType(6, 6), null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, "");
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem AUCAutoSendUnderbondOnCARST
		{
			get
			{
				return GetItem("AUCAutoSendUnderbondOnCARST", delegate
				{
					IRegistryItem result = new BooleanRegistryItem("AUCAutoSendUnderbondOnCARST", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("09fdbe58-8c32-4078-9b78-dddf8255a215", "Auto Send Underbond On CARST"), ResString.GetMultilingualString("04fdf63a-8609-45be-98d3-91bb9872d475", "Do you want to automatically send underbond records that already exist when receiving a valid CARST?"), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem AUCustomsTolerancePercentage
		{
			get
			{
				return GetItem("AUCustomsTolerancePercentage", delegate
				{
					IRegistryItem result = new IntRegistryItem("AUCustomsTolerancePercentage", Categories.Customs_Australia_Legacy, (NoResString)"Tolerance Percentage", (NoResString)"The percentage that the actual duty can vary from the estimated duty before a declaration will be rejected by Customs.", null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers, 0, 0, 100);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AUCustomsToleranceAmount
		{
			get
			{
				return GetItem("AUCustomsToleranceAmount", delegate
				{
					IRegistryItem result = new DecimalRegistryItem("AUCustomsToleranceAmount", Categories.Customs_Australia_Legacy, (NoResString)"Tolerance Amount", (NoResString)"The dollar amount that the actual duty can vary from the estimated duty before a declaration will be rejected by Customs.", RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers);
					result.EditorInfo = new NumericRegistryEditorInfo(2);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		internal IRegistryItem AUCustomsRefundToleranceAmount
		{
			get
			{
				return GetItem("AUCustomsRefundToleranceAmount", delegate
				{
					IRegistryItem result = new DecimalRegistryItem("AUCustomsRefundToleranceAmount", Categories.Customs_Australia, ResString.GetMultilingualString("c02ab5e0-05d0-4cac-9ef6-e0c569b8a2ca", "Minimum Amount for a Refund"), ResString.GetMultilingualString("87a383b1-ed0d-4da0-b563-98263a04a0f8", "The dollar amount that must be exceeded before Customs will issue a refund."), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 2);
					result.EditorInfo = new NumericRegistryEditorInfo(2);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem AUCustomsSenderID
		{
			get
			{
				return GetItem("AUCustomsSenderID", delegate
				{
					IRegistryItem result = new StringRegistryItem("AUCustomsSenderID", Categories.Customs_Australia_Legacy, (NoResString)"Customs Mailbox", (NoResString)"The identifier used to identify your company when sending messages from the Air Cargo or Sea Cargo Forwarder modules to Australian Customs.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AUCustomsSeaCargoDepotMailbox
		{
			get
			{
				return GetItem("AUCustomsSeaCargoDepotMailbox", delegate
				{
					IRegistryItem result = new StringRegistryItem("AUCustomsSeaCargoDepotMailbox", Categories.Customs_Australia_Legacy, (NoResString)"Sea Cargo Depot Mailbox", (NoResString)"The identifier used to identify your company when sending messages from the Sea Cargo Depot module to Australian Customs.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem AUCustomsEdificeSenderID
		{
			get
			{
				return GetItem("AUCustomsEdificeSenderID", delegate
				{
					IRegistryItem result = new StringRegistryItem("AUCustomsEdificeSenderID", Categories.Customs_Australia_Legacy, (NoResString)"Edifice Mailbox", (NoResString)"The identifier used to identify your company when sending Edifice messages to Australian Customs.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AUCustomsCompileSiteID
		{
			get
			{
				return GetItem("AUCustomsCompileSiteID", delegate
				{
					IRegistryItem result = new StringRegistryItem("AUCustomsCompileSiteID", Categories.Customs_Australia_Legacy, (NoResString)"Compile Site ID", (NoResString)"This is the number allocated by customs for this physical location.", new AlphaNumericCodeRegistryDataType(6, 6), null, RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers, "");
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		public IRegistryItem CMRTestMode
		{
			get
			{
				return GetItem("CMRTestMode", delegate
				{
					IRegistryItem result = new BooleanRegistryItem("CMRTestMode", Categories.Customs_Australia_Testing, ResString.GetMultilingualString("929738fa-9a64-4f14-b42a-ab786e2dc057", "CMR Test Mode"), ResString.GetMultilingualString("ba3fe201-f5dc-4455-afea-cf6f2858df3c", "Should CMR messages be sent to the test rather than production system?"), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AUImportsMessagingMode
		{
			get
			{
				return GetItem("AUImportsMessagingMode", delegate
				{
					IRegistryItem result = new CodePairRegistryItem("AUImportsMessagingMode", Categories.Customs_Australia_CMR, ResString.GetMultilingualString("7b05df6f-a281-4557-80a4-4ec639558d94", "Imports Messaging Mode Override"), ResString.GetMultilingualString("ce568c4b-3e55-41ff-9d1b-7f42572ba59a", "How should all Australian Customs import messages be sent?"), OLookUpEditType.AUImportMessagingMode, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, Constants.AUCustoms.ImportMessagingMode.Default);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AUCustomsEFTReceiptPrinterOverride
		{
			get
			{
				return GetItem("AUCustomsEFTReceiptPrinterOverride", delegate
				{
					IRegistryItem result = new StringRegistryItem("AUCustomsEFTReceiptPrinterOverride", Categories.Customs_Australia, null, null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsHidden);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem AUCustomsAirCargoTestMode
		{
			get
			{
				return GetItem("AUCustomsAirCargoTestMode", delegate
				{
					IRegistryItem result = new BooleanRegistryItem("AUCustomsAirCargoTestMode", Categories.Customs_Australia_Legacy, (NoResString)"Air Cargo Test Mode", (NoResString)"Air Cargo messages be sent to the test rather than production system?", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers, false);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AUCustomsSeaCargoTestMode
		{
			get
			{
				return GetItem("AUCustomsSeaCargoTestMode", delegate
				{
					IRegistryItem result = new BooleanRegistryItem("AUCustomsSeaCargoTestMode", Categories.Customs_Australia_Legacy, (NoResString)"Sea Cargo Test Mode", (NoResString)"Sea Cargo messages be sent to the test rather than production system?", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers, false);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		internal IRegistryItem AQISMessagingTestMode
		{
			get
			{
				return GetItem("AQISMessagingTestMode", delegate
				{
					IRegistryItem result = new BooleanRegistryItem("AQISMessagingTestMode", Categories.Customs_Australia_Testing, ResString.GetMultilingualString("020cb370-475a-4576-85ba-53c224aec638", "Quarantine Messaging Test Mode"), ResString.GetMultilingualString("17c4074d-a454-4171-8b36-6f3e5afb0c32", "Should Quarantine messages be sent to the test rather than production system?"), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem CustomsPaymentBankAccount
		{
			get
			{
				return GetItem("CustomsPaymentBankAccount", delegate
				{
					IRegistryItem result = new GuidRegistryItem("CustomsPaymentBankAccount", Categories.Customs_Australia, ResString.GetMultilingualString("345f624f-b03e-48be-8709-625208000faf", "Payment Bank Account"), ResString.GetMultilingualString("b06fb10a-cefb-4291-9d9a-7e810715d245", "These are for payment messages sent to customs."), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccBankAccount);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem CustomsSecondPaymentBankAccount
		{
			get
			{
				return GetItem("CustomsSecondPaymentBankAccount", delegate
				{
					IRegistryItem result = new GuidRegistryItem("CustomsSecondPaymentBankAccount", Categories.Customs_Australia, ResString.GetMultilingualString("ec5daeb9-6291-4e0f-8b16-e9adab56e2eb", "Second Payment Bank Account"), ResString.GetMultilingualString("b06fb10a-cefb-4291-9d9a-7e810715d245", "These are for payment messages sent to customs."), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccBankAccount);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem LastDateCertificatesChecked
		{
			get
			{
				return GetItem("LastDateCertificatesChecked", delegate
				{
					return new DateTimeRegistryItem("LastDateCertificatesChecked", Categories.Customs_Australia, null, null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
				});
			}
		}

		internal IRegistryItem LocalCustomsBranchIdentifier
		{
			get
			{
				return GetItem("LocalCustomsBranchIdentifier", delegate
				{
					IRegistryItem result = new StringRegistryItem("LocalCustomsBranchIdentifier", Categories.Customs_Australia, ResString.GetMultilingualString("28757f42-0eea-4001-bb0e-50dfa7ca366a", "Local Customs Branch Identifier"), ResString.GetMultilingualString("ac0979cb-6ea8-4046-8ca7-fb42be69ff8c", "A Branch Identifier is a facility within the Client Register which enables clients to identify specific areas of their organization in dealings with Customs. A Branch Id is always linked with the Client ID. A Branch will belong to a single client, and will be uniquely identified and associated with a name and address."), RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.DataType = new StringRegistryDataType(0, 6);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal StringRegistryItem PreLodgementLicenceCode
		{
			get
			{
				return GetItem("PreLodgementLicenceCode", delegate
				{
					StringRegistryItem result = new StringRegistryItem("PreLodgementLicenceCode", Categories.Customs_Australia, ResString.GetMultilingualString("aaaeb2dd-b25f-4a5d-bf93-1a31d3eacde4", "Pre-Lodgement License Code"), ResString.GetMultilingualString("53b1a256-0029-4810-99a1-2ed40e144bbe", "The pre-lodgement license code is used when a user wants to send an import declaration as a pre-lodgement message and they do not have a Broker License."), new StringRegistryDataType(0, 5), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal CodePairRegistryItem AgentsReferenceDefaulting
		{
			get
			{
				return GetItem("AgentsReferenceDefaulting", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"AgentsReferenceDefaulting", Categories.Customs_Australia, ResString.GetMultilingualString("27e68bfb-a75f-4844-976a-52c3df2ee29a", "Agents Reference Defaulting"), ResString.GetMultilingualString("2e4fd567-24ae-466c-a066-56a8e7a24a10", "Default for Agents Reference field on the Declaration screen"),
						OLookUpEditType.AgentsReferenceDefaulting, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.AgentsReferenceDefaulting.DEF);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#region AirCargo

		#region Item Declaration

		internal IRegistryItem AirCargoSendErrors
		{
			get
			{
				return GetItem("AirCargoSendErrors", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"AirCargoSendErrors", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("b9e1bf07-85d5-4f48-ac96-10a9abd81ee8", "Send Air Cargo Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem AirCargoSendErrorsToGroup
		{
			get
			{
				return GetItem("AirCargoSendErrorsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"AirCargoSendErrorsToGroup", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("1ab40c7f-a295-4384-b7ae-a1685d990917", "Group To Send Air Cargo Errors To"), null, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AirCargoSendAcknowledgements
		{
			get
			{
				return GetItem("AirCargoSendAcknowledgements", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"AirCargoSendAcknowledgements", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("ff62a6e1-e83c-449f-adc8-4c9d159c699f", "Send Air Cargo Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AirCargoSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("AirCargoSendAcknowledgementsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"AirCargoSendAcknowledgementsToGroup", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("e1b943fa-88bd-4cee-8c35-1278962d708b", "Group To Send Air Cargo Acknowledgements To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AirCargoSendImpediments
		{
			get
			{
				return GetItem("AirCargoSendImpediments", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"AirCargoSendImpediments", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("3134eaf0-ab0a-4c27-9799-c6c3570e4b08", "Send Air Cargo Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem AirCargoSendImpedimentsToGroup
		{
			get
			{
				return GetItem("AirCargoSendImpedimentsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"AirCargoSendImpedimentsToGroup", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("18ec7979-1582-4a99-a31e-7f894eae239d", "Group To Send Air Cargo Impediments To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem AirCargoShipmentType
		{
			get
			{
				return GetItem("AirCargoShipmentType", delegate
				{
					CodeDescriptionPairList airCargoShipmentTypeList = new CodeDescriptionPairList();
					airCargoShipmentTypeList.Add(new CodeDescriptionPair("STD", ResString.GetMultilingualString("39403102-50b3-4c1e-8f40-87c1ff09ae5a", "Standard Shipment")));
					airCargoShipmentTypeList.Add(new CodeDescriptionPair("EXP", ResString.GetMultilingualString("bf31bd2e-d013-4747-a1ad-cd62db419110", "Express Shipment")));
					airCargoShipmentTypeList.Add(new CodeDescriptionPair("DOC", ResString.GetMultilingualString("11e68fe8-85d0-4bb3-bf1e-3480789784f4", "Documents Only")));
					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"AirCargoShipmentType", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("b1e109a4-362e-4422-8ca3-bd3d8507954b", "Shipment Type"), ResString.GetMultilingualString("6f6b20d6-6e79-46a7-9717-6a9e75450bba", "The list of shipment types that appear on the Air Cargo form for Express Couriers."),
						3, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, airCargoShipmentTypeList);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem AirCargoCommercialStatus
		{
			get
			{
				return GetItem("AirCargoCommercialStatus", delegate
				{
					CodeDescriptionPairList airCargoCommercialStatusTypeList = new CodeDescriptionPairList();
					airCargoCommercialStatusTypeList.Add(new CodeDescriptionPair("UDF", ResString.GetMultilingualString("9482aee5-102e-4599-aa25-6ddb63a46160", "Undefined - You can modify this in the System Registry, under Customs / Australia /Air Cargo")));
					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"AirCargoCommercialStatus", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("26871e04-e646-4539-b608-b592f84f17b6", "Commercial Status"), ResString.GetMultilingualString("68d1587f-e7b9-4187-a212-b37b2442c4d5", "The Commercial Status of a Air Cargo Report is a user definable list of commercial status items that may be used at the clients discretion."),
						3, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, airCargoCommercialStatusTypeList);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem HVLVAirCargoSendErrors
		{
			get
			{
				return GetItem("HVLVAirCargoSendErrors", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"HVLVAirCargoSendErrors", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("d7e52247-1aeb-441a-9cd0-616f5f3f57a0", "Send HVLV Air Cargo Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem HVLVAirCargoSendAcknowledgements
		{
			get
			{
				return GetItem("HVLVAirCargoSendAcknowledgements", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"HVLVAirCargoSendAcknowledgements", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("530af0c2-f2ad-400e-b59a-4623ffb0757b", "Send HVLV Air Cargo Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem HVLVAirCargoSendImpediments
		{
			get
			{
				return GetItem("HVLVAirCargoSendImpediments", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"HVLVAirCargoSendImpediments", Categories.Customs_Australia_AirCargo, ResString.GetMultilingualString("b3c810b6-dd37-4601-8bb7-f7903b34d168", "Send HVLV Air Cargo Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Edifice

		#region Item Declaration

		internal IRegistryItem EdificeSendErrors
		{
			get
			{
				return GetItem("EdificeSendErrors", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"EdificeSendErrors", Categories.Customs_Australia_ImportDeclaration, ResString.GetMultilingualString("bb98dad1-f433-4770-b6a7-b8d40c41179f", "Send Import Declaration Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem EdificeSendErrorsToGroup
		{
			get
			{
				return GetItem("EdificeSendErrorsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"EdificeSendErrorsToGroup", Categories.Customs_Australia_ImportDeclaration, ResString.GetMultilingualString("3ab24734-f0a1-4aeb-8b52-e14a81ddf6b4", "Group To Send Import Declaration Errors To"), null, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem EdificeSendAcknowledgements
		{
			get
			{
				return GetItem("EdificeSendAcknowledgements", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"EdificeSendAcknowledgements", Categories.Customs_Australia_ImportDeclaration, ResString.GetMultilingualString("a208f96b-ac45-4c97-8f95-00a27b31e30a", "Send Import Declaration Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem EdificeSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("EdificeSendAcknowledgementsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"EdificeSendAcknowledgementsToGroup", Categories.Customs_Australia_ImportDeclaration, ResString.GetMultilingualString("6865ffed-4bbd-46d8-be06-7ddee3c47b2a", "Group To Send Import Declaration Acknowledgements To"), null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem EdificeSendImpediments
		{
			get
			{
				return GetItem("EdificeSendImpediments", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"EdificeSendImpediments", Categories.Customs_Australia_ImportDeclaration, ResString.GetMultilingualString("87d729df-548e-4204-8c5a-9a1169e33c68", "Send Import Declaration Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem EdificeSendImpedimentsToGroup
		{
			get
			{
				return GetItem("EdificeSendImpedimentsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"EdificeSendImpedimentsToGroup", Categories.Customs_Australia_ImportDeclaration, ResString.GetMultilingualString("fe38ceb6-8947-4c05-ad5e-84ac55f7e30f", "Group To Send Import Declaration Impediments To"), null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region ExportDeclaration

		#region Item Declaration

		internal IRegistryItem ExportDeclarationSendErrors
		{
			get
			{
				return GetItem("Exit1SendErrors", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"Exit1SendErrors", Categories.Customs_Australia_ExportDeclaration, ResString.GetMultilingualString("4b28403e-9674-4774-bb7e-8b4e592cdec3", "Send Export Declaration Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportDeclarationSendErrorsToGroup
		{
			get
			{
				return GetItem("Exit1SendErrorsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"Exit1SendErrorsToGroup", Categories.Customs_Australia_ExportDeclaration, ResString.GetMultilingualString("6c3ff165-a225-431e-8bea-ed428f51fb9f", "Group To Send Export Declaration Errors To"), null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportDeclarationSendAcknowledgements
		{
			get
			{
				return GetItem("Exit1SendAcknowledgements", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"Exit1SendAcknowledgements", Categories.Customs_Australia_ExportDeclaration, ResString.GetMultilingualString("bb067d1a-cfc5-4aff-831c-fea10eb20b2b", "Send Export Declaration Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportDeclarationSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("Exit1SendAcknowledgementsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"Exit1SendAcknowledgementsToGroup", Categories.Customs_Australia_ExportDeclaration, ResString.GetMultilingualString("0f7af57c-b7fa-46c1-8793-ff1b039ad56a", "Group To Send Export Declaration Acknowledgements To"), null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportDeclarationSendImpediments
		{
			get
			{
				return GetItem("Exit1SendImpediments", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"Exit1SendImpediments", Categories.Customs_Australia_ExportDeclaration, ResString.GetMultilingualString("bbb434e7-9823-4ba9-b106-b0d851446e79", "Send Export Declaration Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportDeclarationSendImpedimentsToGroup
		{
			get
			{
				return GetItem("Exit1SendImpedimentsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"Exit1SendImpedimentsToGroup", Categories.Customs_Australia_ExportDeclaration, ResString.GetMultilingualString("c33718c0-6280-4b3d-a84f-f612766dfa30", "Group To Send Export Declaration Impediments To"), null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem NEXDOCSDisableQRPView
		{
			get
			{
				return GetItem("DisableQRPView", () => new BooleanRegistryItem(
					"DisableQRPView",
					Categories.Customs_Australia_NEXDOCS,
					(NoResString)"Disable QRP Document Menu Options", // Support only Registry Item
					(NoResString)"Disables the menu options for Viewing a QRP Document.", // Support only Registry Item
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true
				));
			}
		}

		#endregion

		#endregion

		#region Exit2

		#region Item Declaration

		internal IRegistryItem ExportManifestSendErrors
		{
			get
			{
				return GetItem("Exit2SendErrors", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"Exit2SendErrors", Categories.Customs_Australia_ExportManifest, ResString.GetMultilingualString("36723718-f5d6-4613-8922-c191ce2e641d", "Send Export Manifest Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportManifestSendErrorsToGroup
		{
			get
			{
				return GetItem("Exit2SendErrorsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"Exit2SendErrorsToGroup", Categories.Customs_Australia_ExportManifest, ResString.GetMultilingualString("bc0dddfb-e1fe-4d52-a9cb-ab02aa80cd7b", "Group To Send Export Manifest Errors To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportManifestSendAcknowledgements
		{
			get
			{
				return GetItem("Exit2SendAcknowledgements", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"Exit2SendAcknowledgements", Categories.Customs_Australia_ExportManifest, ResString.GetMultilingualString("d76cae5f-e5f2-44e1-9bc4-bd8202f984a1", "Send Export Manifest Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportManifestSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("Exit2SendAcknowledgementsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"Exit2SendAcknowledgementsToGroup", Categories.Customs_Australia_ExportManifest, ResString.GetMultilingualString("31a847a0-05a5-462a-8b72-91c41612f84a", "Group To Send Export Manifest Acknowledgements To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportManifestSendImpediments
		{
			get
			{
				return GetItem("Exit2SendImpediments", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"Exit2SendImpediments", Categories.Customs_Australia_ExportManifest, ResString.GetMultilingualString("8d63aef0-6f8e-4463-b388-76bdb2340aea", "Send Export Manifest Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem ExportManifestSendImpedimentsToGroup
		{
			get
			{
				return GetItem("Exit2SendImpedimentsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"Exit2SendImpedimentsToGroup", Categories.Customs_Australia_ExportManifest, ResString.GetMultilingualString("90f8105f-e209-43b4-8ade-a1853d17ff1f", "Group To Send Export Manifest Impediments To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IntRegistryItem MaximumConsignmentsLines
		{
			get
			{
				return GetItem("MaximumConsignmentsLines", delegate
				{
					return new IntRegistryItem(
						"MaximumConsignmentsLines",
						Categories.Customs_Australia_ExportManifest,
						ResString.GetMultilingualString("9A705BFC-CCD2-4270-BA92-B4BE2FB9C720", "Maximum Consignments Lines"),
						ResString.GetMultilingualString("B079BE60-2984-44B3-B109-2E7F2D948028", "Maximum Consignments Lines that can be sent in an Export Manifest Message."),
						null,
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController,
						4000,
						1,
						9999);
				});
			}
		}

		#endregion

		#endregion

		#region SeaCargo

		#region Item Declaration

		internal IRegistryItem SeaCargoSendErrors
		{
			get
			{
				return GetItem("SeaCargoSendErrors", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"SeaCargoSendErrors", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("854ac38e-2c28-4db1-b34e-2f608d5581a1", "Send Sea Cargo Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		public IRegistryItem SeaCargoSendErrorsToGroup
		{
			get
			{
				return GetItem("SeaCargoSendErrorsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"SeaCargoSendErrorsToGroup", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("e8979a56-2015-446e-a6ea-cbde4753a250", "Group To Send Sea Cargo Errors To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem SeaCargoSendAcknowledgements
		{
			get
			{
				return GetItem("SeaCargoSendAcknowledgements", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"SeaCargoSendAcknowledgements", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("c37d3a94-66fb-4d79-920c-b08c62327ef7", "Send Sea Cargo Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem SeaCargoSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("SeaCargoSendAcknowledgementsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"SeaCargoSendAcknowledgementsToGroup", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("7aa7a512-05d7-43f3-a616-96ee8f6af05c", "Group To Send Sea Cargo Acknowledgements To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem SeaCargoSendImpediments
		{
			get
			{
				return GetItem("SeaCargoSendImpediments", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"SeaCargoSendImpediments", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("1a4d12b7-34d7-4e2f-a961-e066bef09eb1", "Send Sea Cargo Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem SeaCargoSendImpedimentsToGroup
		{
			get
			{
				return GetItem("SeaCargoSendImpedimentsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"SeaCargoSendImpedimentsToGroup", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("ff886139-c232-493c-9790-f6de4ad048b2", "Group To Send Sea Cargo Impediments To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem SeaCargoCommercialStatus
		{
			get
			{
				return GetItem("SeaCargoCommercialStatus", delegate
				{
					CodeDescriptionPairList seaCargoCommercialStatusTypeList = new CodeDescriptionPairList();
					seaCargoCommercialStatusTypeList.Add(new CodeDescriptionPair("UDF", ResString.GetMultilingualString("84404501-fb65-4f4a-ae8f-c43c21ff27c1", "Undefined - You can modify this in the System Registry, under Customs / Australia /Sea Cargo")));
					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"SeaCargoCommercialStatus", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("26871e04-e646-4539-b608-b592f84f17b6", "Commercial Status"), ResString.GetMultilingualString("30085610-a879-4631-8e59-f0e0b04f889d", "The Commercial Status of a Sea Cargo Report is a user definable list of commercial status items that may be used at the clients discretion."),
						3, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, seaCargoCommercialStatusTypeList);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem HVLVSeaCargoSendErrors
		{
			get
			{
				return GetItem("HVLVSeaCargoSendErrors", () =>
				{
					IRegistryItem result = new CodePairRegistryItem(
						"HVLVSeaCargoSendErrors", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("f1bca179-20c5-41eb-b9dd-346e38b2a2b1", "Send HVLV Sea Cargo Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem HVLVSeaCargoSendAcknowledgements
		{
			get
			{
				return GetItem("HVLVSeaCargoSendAcknowledgements", () =>
				{
					IRegistryItem result = new CodePairRegistryItem(
						"HVLVSeaCargoSendAcknowledgements", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("825bd34c-0ded-4f19-804a-eb242ab77152", "Send HVLV Sea Cargo Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem HVLVSeaCargoSendImpediments
		{
			get
			{
				return GetItem("HVLVSeaCargoSendImpediments", () =>
				{
					IRegistryItem result = new CodePairRegistryItem(
						"HVLVSeaCargoSendImpediments", Categories.Customs_Australia_SeaCargo, ResString.GetMultilingualString("f5f906ae-6817-4602-accb-6cf54dd210fe", "Send HVLV Sea Cargo Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Underbonds

		#region Item Declaration

		internal IRegistryItem UnderbondSendErrors
		{
			get
			{
				return GetItem("UnderbondSendErrors", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"UnderbondSendErrors", Categories.Customs_Australia_Underbond, ResString.GetMultilingualString("4f4dccb0-eb40-4997-9643-2069bc1ad482", "Send Underbond Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem UnderbondSendErrorsToGroup
		{
			get
			{
				return GetItem("UnderbondSendErrorsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"UnderbondSendErrorsToGroup", Categories.Customs_Australia_Underbond, ResString.GetMultilingualString("dc97bb61-74a9-4cb9-9fd2-89fbd73fc748", "Group To Send Underbond Errors To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem UnderbondSendAcknowledgements
		{
			get
			{
				return GetItem("UnderbondSendAcknowledgements", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"UnderbondSendAcknowledgements", Categories.Customs_Australia_Underbond, ResString.GetMultilingualString("2a6eda48-be4a-4a17-b523-015facb9a5c0", "Send Underbond Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem UnderbondSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("UnderbondSendAcknowledgementsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"UnderbondSendAcknowledgementsToGroup", Categories.Customs_Australia_Underbond, ResString.GetMultilingualString("53dab728-1c94-4389-a443-c51841f2716f", "Group To Send Underbond Acknowledgements To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem UnderbondSendImpediments
		{
			get
			{
				return GetItem("UnderbondSendImpediments", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"UnderbondSendImpediments", Categories.Customs_Australia_Underbond, ResString.GetMultilingualString("743cebc1-0829-4aa3-b05d-5649085d73ee", "Send Underbond Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem UnderbondSendImpedimentsToGroup
		{
			get
			{
				return GetItem("UnderbondSendImpedimentsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"UnderbondSendImpedimentsToGroup", Categories.Customs_Australia_Underbond, ResString.GetMultilingualString("459d871a-e462-44c7-9a37-9ea40c60460e", "Group To Send Underbond Impediments To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region CargoStatus

		#region Item Declaration

		internal IRegistryItem CargoStatusSendErrors
		{
			get
			{
				return GetItem("CargoStatusSendErrors", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"CargoStatusSendErrors", Categories.Customs_Australia_CargoStatus, ResString.GetMultilingualString("4cfadcc7-b114-4bd3-941d-9ee7acf3e748", "Send Cargo Status Errors"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem CargoStatusSendErrorsToGroup
		{
			get
			{
				return GetItem("CargoStatusSendErrorsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"CargoStatusSendErrorsToGroup", Categories.Customs_Australia_CargoStatus, ResString.GetMultilingualString("16d6e8c7-9565-4180-8a05-d0db63b05565", "Group To Send Cargo Status Errors To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem CargoStatusSendAcknowledgements
		{
			get
			{
				return GetItem("CargoStatusSendAcknowledgements", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"CargoStatusSendAcknowledgements", Categories.Customs_Australia_CargoStatus, ResString.GetMultilingualString("4dbfe8fb-552a-4f42-8e7d-12085d24e2e9", "Send Cargo Status Acknowledgements"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem CargoStatusSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("CargoStatusSendAcknowledgementsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"CargoStatusSendAcknowledgementsToGroup", Categories.Customs_Australia_CargoStatus, ResString.GetMultilingualString("d9a4c574-dbf9-498a-9cb6-f5ff18ff2a35", "Group To Send Cargo Status Acknowledgements To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem CargoStatusSendImpediments
		{
			get
			{
				return GetItem("CargoStatusSendImpediments", delegate
				{
					IRegistryItem result = new CodePairRegistryItem(
						"CargoStatusSendImpediments", Categories.Customs_Australia_CargoStatus, ResString.GetMultilingualString("6ffc3e07-e728-4a40-9d47-ba194519fcf9", "Send Cargo Status Impediments"), null,
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, Constants.EmailTo.StaffMemberAndNominatedGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal IRegistryItem CargoStatusSendImpedimentsToGroup
		{
			get
			{
				return GetItem("CargoStatusSendImpedimentsToGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"CargoStatusSendImpedimentsToGroup", Categories.Customs_Australia_CargoStatus, ResString.GetMultilingualString("aa3ad9c4-e984-410c-9cf1-7840cc95e03d", "Group To Send Cargo Status Impediments To"), null,
						RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Malaysia

		internal IRegistryItem MYCustomsUseRankAlphaForK4K5
		{
			get
			{
				return GetItem("MYCustomsUseRankAlphaForK4K5", delegate
				{
					return new BooleanRegistryItem(new MYRegistryItem("MYCustomsUseRankAlphaForK4K5", Categories.Customs_Malaysia, ResString.GetMultilingualString("76e09387-738e-4ae3-8e60-da5a2f0cee84", "Use Rank Alpha for K4 K5 messages"), ResString.GetMultilingualString("76e09387-738e-4ae3-8e60-da5a2f0cee84", "Use Rank Alpha for K4 K5 messages"), RegistryDataTypes.BoolType, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, false));
				});
			}
		}

		internal IRegistryItem MYMessageOutputDirectory
		{
			get
			{
				return GetItem("MYCustomsRankAlphaExportDirectory", delegate
				{
					return new MYRegistryItem("MYCustomsRankAlphaExportDirectory", Categories.Customs_Malaysia, ResString.GetMultilingualString("b36a31cf-af5f-4e5c-9057-498e0a8eccd7", "Customs Message Output Directory"), ResString.GetMultilingualString("b36a31cf-af5f-4e5c-9057-498e0a8eccd7", "Customs Message Output Directory"), RegistryDataTypes.StringType, RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		internal IRegistryItem MYCustomsImportContinuingPermission
		{
			get
			{
				return GetItem("MYCustomsImportContinuingPermission", delegate
				{
					return new MYRegistryItem("MYCustomsImportContinuingPermission", Categories.Customs_Malaysia, ResString.GetMultilingualString("3df41016-b8d6-4554-82ff-1dfb5ef0233d", "Import Continuing Permission"), ResString.GetMultilingualString("b1b56a0a-5d7b-4858-9854-b5cbb8e9ce89", "Enter a 20 characters long permission number"), new StringRegistryDataType(20, 20), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem MYIsTestMode
		{
			get
			{
				return GetItem("MYIsTestMode", delegate
				{
					return new BooleanRegistryItem(new MYRegistryItem("MYIsTestMode", Categories.Customs_Malaysia, (NoResString)"Is Test Mode", null, RegistryDataTypes.BoolType, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue, false));
				});
			}
		}

		#endregion

		internal IRegistryItem MYUse2006Version
		{
			get
			{
				return GetItem("MYUse2006Version", delegate
				{
					return new BooleanRegistryItem(new MYRegistryItem("MYUse2006Version", Categories.Customs_Malaysia, ResString.GetMultilingualString("9cc773df-09bd-477f-bd8d-51e0f0e0a635", "Use 2006 e-Manifest Specification"), null, RegistryDataTypes.BoolType, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, false));
				});
			}
		}

		internal IRegistryItem MYCustomsSenderID
		{
			get
			{
				return GetItem("MYCustomsSenderID", delegate
				{
					return new MYRegistryItem("MYCustomsSenderID", Categories.Customs_Malaysia, ResString.GetMultilingualString("59ae163a-88f6-404b-be59-d222a3b755cf", "Customs Mailbox"), ResString.GetMultilingualString("ab3a0ac7-076b-44c0-9151-84863ebb989a", "The identifier used to identify your company when sending K4/K5 messages to Malaysian Customs."), RegistryDataTypes.StringType, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		internal IRegistryItem MYDagangPassword
		{
			get
			{
				return GetItem("MYDagangPassword", delegate
				{
					IRegistryItem result = new MYRegistryItem("MYDagangPassword", Categories.Customs_Malaysia, ResString.GetMultilingualString("3fd0befd-8419-433a-8dd9-d0b61a048fa1", "Dagang Password"), null, RegistryDataTypes.StringType, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem MYSenderPassword
		{
			get
			{
				return GetItem("MYSenderPassword", delegate
				{
					IRegistryItem result = new MYRegistryItem("MYSenderPassword", Categories.Customs_Malaysia, (NoResString)"Sender Password", null, RegistryDataTypes.StringType, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		#endregion

		class MYRegistryItem : RegistryItemImpl
		{
			public MYRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, category, caption, hint, dataType, storage)
			{
			}

			public MYRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, object defaultValue)
				: base(name, category, caption, hint, dataType, storage, defaultValue)
			{
			}

			public MYRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, dataType, storage, options)
			{
			}

			public MYRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
				: base(name, category, caption, hint, dataType, storage, options, defaultValue)
			{
			}

			public override IEnumerable<Guid> CountryFilterPKs
			{
				get { return RegistryItemSet.CountryFilterPKs.Malaysia; }
			}
		}

		#endregion

		#region South Africa

		class ZARegistryItem : RegistryItemImpl
		{
			public ZARegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, category, caption, hint, dataType, storage)
			{
			}

			public ZARegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, dataType, storage, options)
			{
			}

			public ZARegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, object defaultValue)
				: base(name, category, caption, hint, dataType, storage, defaultValue)
			{
			}

			public ZARegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
				: base(name, category, caption, hint, dataType, storage, options, defaultValue)
			{
			}

			public override IEnumerable<Guid> CountryFilterPKs
			{
				get { return RegistryItemSet.CountryFilterPKs.SouthAfrica; } // ZA
			}
		}

		internal IRegistryItem ZAIsTestMode
		{
			get
			{
				return GetItem("ZAIsTestMode", delegate
				{
					return new BooleanRegistryItem(new ZARegistryItem("ZAIsTestMode", Categories.Customs_SouthAfrica, ResString.GetMultilingualString("5bd315c3-f0ec-48f0-b5af-06210ec65ca0", "Is Test Mode"), null, RegistryDataTypes.BoolType, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsHidden : DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, false));
				});
			}
		}

		#endregion

		#region United Arab Emirates

		class AERegistryItem : RegistryItemImpl
		{
			public AERegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, category, caption, hint, dataType, storage)
			{
			}

			public AERegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, dataType, storage, options)
			{
			}

			public AERegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, object defaultValue)
				: base(name, category, caption, hint, dataType, storage, defaultValue)
			{
			}

			public AERegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
				: base(name, category, caption, hint, dataType, storage, options, defaultValue)
			{
			}

			public override IEnumerable<Guid> CountryFilterPKs
			{
				get { return RegistryItemSet.CountryFilterPKs.UnitedArabEmirates; }
			}
		}

		internal IRegistryItem AECourierID
		{
			get
			{
				return GetItem("AECourierID", delegate
				{
					return new StringRegistryItem(new AERegistryItem("AECourierID", Categories.Customs_UnitedArabEmirates, ResString.GetMultilingualString("300adc47-cb09-4e41-9ea8-388b5dcfc755", "Clearing Agent Code"), ResString.GetMultilingualString("55076be2-895a-44b4-8bf4-f8db43c1bb98", "The Dubai Customs Clearing Agent Code."), new StringRegistryDataType(0, 6), RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, ""));
				});
			}
		}

		#endregion

		#endregion

		#region ReferenceFiles

		#region Registry Item Properties

		internal IRegistryItem EquipmentGroup
		{
			get
			{
				return GetItem("EquipmentGroup", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"EquipmentGroup",
						Categories.ReferenceFiles,
						ResString.GetMultilingualString("ba7f639d-09ab-406f-af55-7903c356eedc", "Equipment Group"),
						ResString.GetMultilingualString("04ba18f7-77e2-42d2-9083-df945c47bacc", "List of available equipment groups"),
						3,
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#endregion

		#region Rating

		#region Registry Item Properties

		internal IRegistryItem FreightSearchPriorities
		{
			get
			{
				return GetItem("FreightSearchPriorities", delegate
				{
					const string DefaultFreightPriorities = "TI_RH_NKCommodityCode,TI_RS_NKServiceLevel_NI,TI_OH_TransportProvider,TI_ViaLRC,TI_HBLDeliveryMode,";
					IRegistryItem result = new StringRegistryItem(
						"FreightSearchPriorities",
						Categories.AutoRating_Calculation,
						ResString.GetMultilingualString("9acb17e0-c3f0-4e03-b56e-014d6d308dec", "Freight Rates Search Priorities"),
						ResString.GetMultilingualString("4a7b916e-3b1f-4356-a561-be2e098ae53a", "Search priorities for the AutoRating system"),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						DefaultFreightPriorities);
					result.EditorInfo = new AutoRatingPriorityRegistryEditorInfo();
					return result;
				});
			}
		}
		
		public ChargeCodeRegistryItem FreightChargeCode
		{
			get
			{
				return GetItem("FreightChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem(
						"FreightChargeCode",
						Categories.AutoRating_ChargeCodes_Freight,
						ResString.GetMultilingualString("120cde16-aa8a-43c0-9294-13b501bbf04a", "Freight Charge Code"),
						ResString.GetMultilingualString("120cde16-aa8a-43c0-9294-13b501bbf04a", "Freight Charge Code"),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						RegistryConstants.Strings.DefaultFreightChargeCode);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		internal IRegistryItem GRINotificationLastRunDate
		{
			get
			{
				return GetItem("GRINotificationLastRunDate", delegate
				{
					return new DateTimeRegistryItem(
						"GRINotificationLastRunDate",
						Categories.AutoRating,
						(NoResString)"GRI Notification Last Run Date",
						(NoResString)"Internal registry item used to keep track of last GRI Notification run date",
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden);
				});
			}
		}

		#endregion

		internal IRegistryItem RateValidityPeriod
		{
			get
			{
				return GetItem("RateValidityPeriod", delegate
				{
					IRegistryItem result = new IntRegistryItem(
						"RateValidityPeriod",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("65cabe71-5850-467e-98cf-f52ac7fcebfd", "Rate Validity Period (Months)"),
						ResString.GetMultilingualString("cf8fef8d-1d23-4307-9f17-00cbc7b7fa67", "The default validity for a new Client rate. \r\n Leave field blank to specify infinite validity."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						6);
					result.EditorInfo = new RateValidityRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem CostRateValidityPeriod
		{
			get
			{
				return GetItem("CostRateValidityPeriod", delegate
				{
					IRegistryItem result = new IntRegistryItem(
						"CostRateValidityPeriod",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("4f43d536-7d3f-4b7e-9e0d-b2694b26f8b3", "Cost Rate Validity Period (Months)"),
						ResString.GetMultilingualString("1077be71-a1c3-473e-bdf7-dd32c3c0a827", "The default validity for a new Buy rate. \r\n Leave field blank to specify infinite validity."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						6);
					result.EditorInfo = new RateValidityRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem GlobalTariffValidityPeriod
		{
			get
			{
				return GetItem("GlobalTariffValidityPeriod", delegate
				{
					IRegistryItem result = new IntRegistryItem(
						"GlobalTariffValidityPeriod",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("c46a13d3-9001-4882-8199-096aa273a42b", "Company Tariff Validity Period (Months)"),
						ResString.GetMultilingualString("2a31aff3-ab4c-4f88-bac9-ed161eba1592", "The default validity for a new Company Tariff. \r\n Leave field blank to specify infinite validity."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						6);
					result.EditorInfo = new RateValidityRegistryEditorInfo();
					return result;
				});
			}
		}

		internal QuoteValidityRegistryItem QuoteValidityPeriod
		{
			get
			{
				return GetItem("QuoteValidityPeriod", delegate
				{
					QuoteValidityRegistryItem result = new QuoteValidityRegistryItem(
						"QuoteValidityPeriod",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("b7d5fcd6-7fba-428b-8894-521f859200e8", "Quotation Validity Period (Months)"),
						ResString.GetMultilingualString("14082f8e-8b2f-434e-b871-6120f95bbff6", "The default validity for a new quotation."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						1);
					result.EditorInfo = new QuoteValidityRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem ExpiredRateNotificationPeriod
		{
			get
			{
				return GetItem("ExpiredRateNotificationPeriod", delegate
				{
					return new IntRegistryItem(
						"ExpiredRateNotificationPeriod",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("5ab7b709-721c-4996-8f20-caa18b28c17f", "Expired Rate Notification Period (Days)"),
						ResString.GetMultilingualString("7449c1c9-392d-4b42-bf6a-fc198a7b2313", "The number of days after a rate expires, that AutoRating should still warn users that this rate had entries that matched the job criteria, but the rate entry has expired."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						7);
				});
			}
		}

		internal IRegistryItem ExpiringRateNotificationPeriod
		{
			get
			{
				return GetItem("ExpiringRateNotificationPeriod", delegate
				{
					return new IntRegistryItem(
						"ExpiringRateNotificationPeriod",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("f38d31b2-4280-474a-ae49-43e78ef18219", "Expiring Rate Notification Period (Days)"),
						ResString.GetMultilingualString("4c1472f3-a0a9-4c8a-9bbf-fe9cd8c7633c", "The number of days before a rate expires that AutoRating should still warn users that this rate has entries that matched and will expire."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						90);
				});
			}
		}

		internal IRegistryItem PermanentlyDeleteRatesExpiredPeriod
		{
			get
			{
				return GetItem("PermanentlyDeleteRatesExpiredPeriod", delegate
				{
					return new DeleteExpiredRatesRegistryItem(
						"PermanentlyDeleteRatesExpiredPeriod",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("e38d6226-08d2-4055-b41b-612ee07bbf1e", "Permanently Delete Rates Expired in Period (Years)"),
						ResString.GetMultilingualString("f6c08fc6-47d5-4ec9-bf92-fbbebbdeaf0a", "Specifies the Period that Rates have been Expired since to be Permanently Deleted using Service Task 'RED - Permanently Delete Expired Rates from CW Database'\r\n" +
																								"\r\nA value of Zero means no expired rate to be deleted by the Service Task." +
																								"\r\nMaximum Value for Batch Size is 5000"),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new DeleteExpiredRates { ExpiredRatesPeriodInYears = DeleteExpiredRatesDefaultValue.ExpiredRatesPeriodInYears, BatchSize = DeleteExpiredRatesDefaultValue.BatchSize });
				});
			}
		}

		internal IRegistryItem FreightRatedCodes
		{
			get
			{
				return GetItem("FreightRatedCodes", delegate
				{
					return new StringRegistryItem(
						"FreightRatedCodes",
						Categories.AutoRating_ChargeCodeGroups,
						ResString.GetMultilingualString("c9af3c65-ea73-4368-92c6-c6fab6c23098", "Freight Rated Charge Code Groups"),
						ResString.GetMultilingualString("005416c7-1cb2-4a7b-8701-08d96aa1ae59", "This specifies the charge code groups that are rated when AutoRating from a Shipment."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"ORG,LOD,UNL,DST,INS,FRT"); // Registry Value
				});
			}
		}

		internal IRegistryItem BrokerageRatedCodes
		{
			get
			{
				return GetItem("BrokerageRatedCodes", delegate
				{
					return new StringRegistryItem(
						"BrokerageRatedCodes",
						Categories.AutoRating_ChargeCodeGroups,
						ResString.GetMultilingualString("6681f286-f57b-4964-9581-d59da442a93e", "Import Brokerage Rated Charge Code Groups"),
						ResString.GetMultilingualString("e8113bc1-959c-4207-bcbb-e2d529b45ae6", "This specifies the charge code groups that are rated when AutoRating from a Import Customs Brokerage job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"BRK,BON,CDS"); // Registry Value
				});
			}
		}

		public IRegistryItem BuyersConsolApportionedCodes
		{
			get
			{
				return GetItem("BuyersConsolApportionedCodes", delegate
				{
					return new StringRegistryItem(
						"BuyersConsolApportionedCodes",
						Categories.AutoRating_ChargeCodeGroups,
						ResString.GetMultilingualString("dd62a464-1265-431a-bf56-c96fe3fea7fe", "Buyers Consol Apportioned Charge Code Groups"),
						ResString.GetMultilingualString("8abd9023-e6ed-44bf-b65d-16a3b37390b1", "This specifies the charge code groups that should be apportioned across the Buyers Consol when rating Buyers Consol shipments."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"LOD,UNL,DST,INS,FRT"); // Registry Value
				});
			}
		}

		public IRegistryItem ShippersConsolApportionedCodes
		{
			get
			{
				return GetItem("ShippersConsolApportionedCodes", delegate
				{
					return new StringRegistryItem(
						"ShippersConsolApportionedCodes",
						Categories.AutoRating_ChargeCodeGroups,
						ResString.GetMultilingualString("cdd20a0c-b502-4a08-8ce9-2db2403f3d3d", "Shippers Consol Apportioned Charge Code Groups"),
						ResString.GetMultilingualString("0c9ef102-cfe0-418a-b126-b246f6dd29a3", "This specifies the charge code groups that should be apportioned across the Shippers Consol when rating Shippers Consol shipments."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"LOD,UNL,ORG,INS,FRT"); // Registry Value
				});
			}
		}

		internal IRegistryItem OriginBrokerageRatedCodes
		{
			get
			{
				return GetItem("OriginBrokerageRatedCodes", delegate
				{
					return new StringRegistryItem(
						"OriginBrokerageRatedCodes",
						Categories.AutoRating_ChargeCodeGroups,
						ResString.GetMultilingualString("512c9e62-32fc-4c44-91f7-4ec398a84db8", "Export Brokerage Rated Charge Code Groups"),
						ResString.GetMultilingualString("ef1b0e42-9117-4614-88ef-dea123b4adfd", "This specifies the charge code groups that are rated when AutoRating from a Export Customs Brokerage job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"OBR,OBO"); // Registry Item Value
				});
			}
		}

		internal IRegistryItem MarkUpPercentages
		{
			get
			{
				return GetItem("MarkUpPercentages", delegate
				{
					IRegistryItem result = new StringRegistryItem(
						"MarkUpPercentages",
						Categories.AutoRating_CostMarkups,
						ResString.GetMultilingualString("0ccb29e1-d3d5-4cc3-87a1-c2042d8169f1", "Default Markup on Costs"),
						ResString.GetMultilingualString("87336bc6-98a1-499c-a641-0906cdc428f5", "The default markup percentage to use when the Cost Based Calculator is used to specify a sell rate."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");
					result.EditorInfo = new MarkUpPercentagesRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem MinimumMarkUpPercentages
		{
			get
			{
				return GetItem("MinimumMarkUpPercentages", delegate
				{
					IRegistryItem result = new StringRegistryItem(
						"MinimumMarkUpPercentages",
						Categories.AutoRating_CostMarkups,
						ResString.GetMultilingualString("485c9b20-b4cf-4ebf-a31b-b91e8ce452bb", "Minimum Markup on Costs"),
						ResString.GetMultilingualString("5bc91300-6711-42a0-a6f2-504104704908", "Minimum markup percentage to use when the Cost Based Calculator is used to specify a sell rate."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");
					result.EditorInfo = new MarkUpPercentagesRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem QuoteEndDateMandatory
		{
			get
			{
				return GetItem("QuoteEndDateMandatory", delegate
				{
					return new BooleanRegistryItem(
						"QuoteEndDateMandatory",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("a05586f1-5775-4045-8633-05f46416f7c3", "Quotation End Date Mandatory"),
						ResString.GetMultilingualString("06b1be2c-414a-4cc9-8ad0-157619421ab9", "Specifies whether the quotation end date is a mandatory field."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		internal IRegistryItem WebRateValidityPeriod
		{
			get
			{
				return GetItem("WebRateValidityPeriod", delegate
				{
					return new IntRegistryItem(
						"WebRateValidityPeriod",
						Categories.AutoRating_ValidityandNotificationPeriods,
						ResString.GetMultilingualString("1effe728-e6f6-4a5b-a646-96b724300ad1", "Web Rate Validity Period (Months)"),
						ResString.GetMultilingualString("15486d82-c8be-46b9-9825-b5b6007efc32", "The default validity for a new client rate on the web site."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						6,
						1,
						int.MaxValue);
				});
			}
		}

		internal IRegistryItem StorageCalculationPeriod
		{
			get
			{
				return GetItem("StorageCalculationPeriod", delegate
				{
					return new CodePairRegistryItem(
						"StorageCalculationPeriod",
						Categories.AutoRating_Calculation_StorageandTimeRating,
						ResString.GetMultilingualString("db70aff1-8ee9-4794-b1a1-a4560b8e6353", "Storage Calculation Period"),
						ResString.GetMultilingualString("6a0d0bec-e55d-40d9-8bde-22ab98ea2e41", "Defines the period by which periodic storage calculations are based."),
						OLookUpEditType.StorageCalculationPeriod,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Constants.StorageCalculationPeriods.Weekly);
				});
			}
		}

		internal IRegistryItem ExcludeHolidaysInTimeRating
		{
			get
			{
				return GetItem("ExcludeHolidaysInTimeRating", delegate
				{
					return new BooleanRegistryItem(
						"ExcludeHolidaysInTimeRating",
						Categories.AutoRating_Calculation_StorageandTimeRating,
						ResString.GetMultilingualString("42c5f774-1b82-40cc-b858-efe56d03b05f", "Exclude Holidays / Weekends in Storage Rating"),
						ResString.GetMultilingualString("baf8d8b1-617c-492b-ad62-d463ade6a2fc", "Indicates whether holidays and weekends should be counted when calculating storage."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		internal IRegistryItem IncludeCFSFreeStorageDaysInCalculation
		{
			get
			{
				return GetItem("IncludeCFSFreeStorageDaysInCalculation", delegate
				{
					return new BooleanRegistryItem(
						"IncludeCFSFreeStorageDaysInCalculation",
						Categories.AutoRating_Calculation_StorageandTimeRating,
						ResString.GetMultilingualString("36e44b53-e79b-4c02-a588-c9012cc97822", "Include CFS Free Storage Days in Calculation"),
						ResString.GetMultilingualString("1c258e54-a472-4cc7-a80c-35370d8b6cb6", "Indicates whether Free Storage Days are included in the total number of storage days for CFS storage calculation."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		internal IRegistryItem DefaultCTOPostCodeAir
		{
			get
			{
				return GetItem("DefaultCTOPostCodeAir", delegate
				{
					return new StringRegistryItem(
						"DefaultCTOPostCodeAir",
						Categories.AutoRating_Calculation_PortTransportZoneDistanceRating,
						ResString.GetMultilingualString("78126f4b-5518-4531-a09c-245b8aea1d08", "CTO Default Postcode - Air"),
						ResString.GetMultilingualString("cdc278fc-2a57-4a67-9d7d-189e653cb398", "This specifies the default CTO postcode to assume when performing Port Transport zone rating, if no CTO address is specified on the Consol."),
						RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		internal IRegistryItem DefaultCTOPostCodeSea
		{
			get
			{
				return GetItem("DefaultCTOPostCodeSea", delegate
				{
					return new StringRegistryItem(
						"DefaultCTOPostCodeSea",
						Categories.AutoRating_Calculation_PortTransportZoneDistanceRating,
						ResString.GetMultilingualString("7cacb5be-f2a8-483c-bee7-266a365fb911", "CTO Default Postcode - Sea"),
						ResString.GetMultilingualString("A2D51BD2-B354-4E54-A93A-006967846206", "This specifies the default CTO postcode to assume when performing Port Transport zone rating, if no CTO address is specified on the Consol."),
						RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		internal IRegistryItem DefaultCTOPostCodeRail
		{
			get
			{
				return GetItem("DefaultCTOPostCodeRail", delegate
				{
					return new StringRegistryItem(
						"DefaultCTOPostCodeRail",
						Categories.AutoRating_Calculation_PortTransportZoneDistanceRating,
						ResString.GetMultilingualString("83cbb7f3-b1c7-49b9-ad7f-a38cf6be9399", "CTO Default Postcode - Rail"),
						ResString.GetMultilingualString("24ba2d6a-cb03-494c-859d-3f04927b8f91", "This specifies the default CTO postcode to assume when performing Port Transport zone rating, if no CTO address is specified on the Consol."),
						RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		internal IRegistryItem DefaultCFSPostCodeRoad
		{
			get
			{
				return GetItem("DefaultCFSPostCodeRoad", delegate
				{
					return new StringRegistryItem(
						"DefaultCFSPostCodeRoad",
						Categories.AutoRating_Calculation_PortTransportZoneDistanceRating,
						ResString.GetMultilingualString("9c3cbe88-c364-4c4f-87ae-e8fbabefeb3f", "CFS Default Postcode - Road"),
						ResString.GetMultilingualString("BFA8881E-91CF-4E82-944D-5237013AB1C3", "This specifies the default CFS postcode to assume when performing Port Transport zone rating, if no CFS address is specified on the Shipment or Consol."),
						RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		internal IRegistryItem UseDistanceCalculcationService
		{
			get
			{
				return GetItem("UseDistanceCalculcationService", delegate
				{
					return new BooleanRegistryItem(
						"UseDistanceCalculcationService",
						Categories.AutoRating_PortTransportZoneDistanceRating,
						ResString.GetMultilingualString("3fb12c42-466c-470f-8281-8291ddcc7d10", "Use CargoWise Distance Calculation Service"),
						ResString.GetMultilingualString("6b7fa8c7-b72a-4d94-9f17-fbaf874107c6", "This specifies whether the CargoWise Distance Calculation Service should be used to calculate relevant road distances."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		internal IRegistryItem AgencyCalcLinesFirstPageImport
		{
			get
			{
				return GetItem("AgencyCalcLinesFirstPageImport", delegate
				{
					return new IntRegistryItem(
						"AgencyCalcLinesFirstPageImport",
						Categories.AutoRating_Calculation_AgencyCalculator,
						ResString.GetMultilingualString("f1c1cff1-a12a-4a8b-a8d0-1566463847e2", "Number of Lines on First Page - Import"),
						ResString.GetMultilingualString("3220eda4-ada1-47ad-b21c-0b48a284a729", "Specifies how many entry lines exist on the first entry page, for imports. This number will be used during the calculation of agency charges that are per Entry Page."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						1);
				});
			}
		}

		internal IRegistryItem AgencyCalcLinesFirstPageExport
		{
			get
			{
				return GetItem("AgencyCalcLinesFirstPageExport", delegate
				{
					return new IntRegistryItem(
						"AgencyCalcLinesFirstPageExport",
						Categories.AutoRating_Calculation_AgencyCalculator,
						ResString.GetMultilingualString("814a7724-eab8-4b4e-8739-d670aad7468a", "Number of Lines on First Page - Export"),
						ResString.GetMultilingualString("c5667185-5bb4-41c6-bec9-1e2c270e05f0", "Specifies how many entry lines exist on the first entry page, for exports. This number will be used during the calculation of agency charges that are per Entry Page."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						1);
				});
			}
		}

		internal IRegistryItem AgencyCalcLinesAdditionalPageImport
		{
			get
			{
				return GetItem("AgencyCalcLinesAdditionalPageImport", delegate
				{
					return new IntRegistryItem(
						"AgencyCalcLinesAdditionalPageImport",
						Categories.AutoRating_Calculation_AgencyCalculator,
						ResString.GetMultilingualString("29c16519-c0a4-4876-b4f1-d03d746f3773", "Number of Lines on Additional Pages - Import"),
						ResString.GetMultilingualString("d855df81-21b3-49e7-97d8-ad7af80cadba", "Specifies how many entry lines exist on each additional entry page, for imports. This number will be used during the calculation of agency charges that are per Entry Page."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						5);
				});
			}
		}

		internal IRegistryItem AgencyCalcLinesAdditionalPageExport
		{
			get
			{
				return GetItem("AgencyCalcLinesAdditionalPageExport", delegate
				{
					return new IntRegistryItem(
						"AgencyCalcLinesAdditionalPageExport",
						Categories.AutoRating_Calculation_AgencyCalculator,
						ResString.GetMultilingualString("776ae5d0-4da4-4ba7-9d5d-9b6f7b782671", "Number of Lines on Additional Pages - Export"),
						ResString.GetMultilingualString("98933cc2-09ce-4a36-8e27-1c09e3b774eb", "Specifies how many entry lines exist on each additional entry page, for exports. This number will be used during the calculation of agency charges that are per Entry Page."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						3);
				});
			}
		}

		internal IRegistryItem TACTRateImportPartitionSize
		{
			get
			{
				return GetItem("TACTRateImportPartitionSize", delegate
				{
					return new IntRegistryItem(
						"TACTRateImportPartitionSize",
						Categories.AutoRating,
						ResString.GetMultilingualString("4b430a85-8083-45bc-bd0d-723bcfe50f76", "Partition size for TACT rates import"),
						ResString.GetMultilingualString("3fd7740b-e97a-440d-b2f6-af494086cfe3", "Specifies how many lines included in one partition when importing from full TACT rate plain text file. Set smaller partition size to reduce memory usage, but it will take longer time to import."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						50000);
				});
			}
		}

		#endregion

		#endregion

		#region Notification

		public IRegistryItem NotificationGroup
		{
			get
			{
				return GetItem("NotificationGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"NotificationGroup",
						Categories.Notification,
						ResString.GetMultilingualString("a9468804-7f78-4db3-a89a-d344303aa298", "Company Notification Group"),
						ResString.GetMultilingualString("78b09a50-d020-4bac-a13f-256ee551459e", "Group to notify with email."),
						RegistryStorageFlags.All,
						RegistryOptions.Default,
						RegistryConstants.GroupPKs.Notification
						);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Infrastructure Errors Notification Group

		public GuidRegistryItem InfrastructureErrorsNotificationGroup
		{
			get
			{
				return GetItem("InfrastructureErrorsNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"InfrastructureErrorsNotificationGroup",
						Categories.Notification,
						ResString.GetMultilingualString("34104a5f-894d-44bc-939b-ddef1afa67c2", "Infrastructure Errors Notification Group"),
						ResString.GetMultilingualString("db523483-8a06-446c-9818-f6608ff23544", "The staff group that will be notified about SQL errors in service tasks related to network outages, database server outages and deadlocks/timeouts in SQL statements - as opposed to application/data errors."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						EnvProxy.IsHostedWithCargowise && RegistryConstants.GroupPKs.HostingSupport != Guid.Empty ? RegistryConstants.GroupPKs.HostingSupport : RegistryConstants.GroupPKs.Notification);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Organisation

		public IRegistryItem OrgShowConsigneeConsignorTab
		{
			get
			{
				return GetItem("OrgShowConsigneeConsignorTab", delegate
				{
					return new BooleanRegistryItem("OrgShowConsigneeConsignorTab", Categories.SalesMarketing_ClientIntelligence, ResString.GetMultilingualString("8c11ff87-6218-4afd-ad22-777988865904", "Show Consignee and Consignor Tabs"), ResString.GetMultilingualString("4c6d7710-9bf7-4ad6-946b-38fb0f6a996b", "Show the Consignee and Consignor tabs on the Client Intelligence screen"), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		public IRegistryItem OrgShowARTab
		{
			get
			{
				return GetItem("OrgShowARTab", delegate
				{
					return new BooleanRegistryItem("OrgShowARTab", Categories.SalesMarketing_ClientIntelligence, ResString.GetMultilingualString("6d263311-2dc9-4020-b6be-576262ea81f1", "Show Receivables Tab"), ResString.GetMultilingualString("4637789c-00da-4f3f-b2eb-6663fd88c667", "Show the Receivables tab on the Client Intelligence screen"), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		public BooleanRegistryItem AllowCityPostcodeValidation
		{
			get
			{
				return GetItem("AllowCityPostcodeValidation", delegate
				{
					var result = new BooleanRegistryItem(
						"AllowCityPostcodeValidation",
						Categories.Freight_Shipment,
						(NoResString)"Allow City/Postcode Validation", // Support only registry item
						(NoResString)"Set this to \"yes\" to turn on City / Postcode validation when editing / saving an eTail / eManifest address.", // Support only registry item
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#region Job Addresses

		public BooleanRegistryItem JobAddressValidation_CityMandatory
		{
			get
			{
				return GetItem("JobAddressValidation_CityMandatory", delegate
				{
					return new BooleanRegistryItem(
						"JobAddressValidation_CityMandatory",
						Categories.Operations_JobAddress,
						ResString.GetMultilingualString("8df3cd0c-fb2f-45c6-9ff0-b1630d33d0c7", "City Mandatory"),
						ResString.GetMultilingualString("f56eb5e9-ab41-462f-85b5-80d4370f2199", "Specifies whether the City on an job address is mandatory."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem JobAddressValidation_CountryMandatory
		{
			get
			{
				return GetItem("JobAddressValidation_CountryMandatory", delegate
				{
					return new BooleanRegistryItem(
						"JobAddressValidation_CountryMandatory",
						Categories.Operations_JobAddress,
						ResString.GetMultilingualString("97e400e7-f6ab-467b-9aa7-2a8032911206", "Country/Region Mandatory"),
						ResString.GetMultilingualString("b14fa68f-ea3a-46fd-9546-3680df6feeb1", "Specifies whether the Country/Region on an job address is mandatory."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem JobAddressValidation_UsePostcodeRules
		{
			get
			{
				return GetItem("JobAddressValidation_UsePostcodeRules", delegate
				{
					return new BooleanRegistryItem(
						"JobAddressValidation_UsePostcodeRules",
						Categories.Operations_JobAddress,
						ResString.GetMultilingualString("26e312a4-7e23-4e4e-ab9f-27f4fbaa7e65", "Postcode Validation Rule affects Job Addresses"),
						ResString.GetMultilingualString("197c36ae-9325-4ffd-8564-fd4f3d0ccfa9", "If enabled, postcode validation on job addresses will follow the rule defined on the country/region. If disabled, postcodes will not be mandatory on job addresses."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem JobAddressValidation_UseStateRules
		{
			get
			{
				return GetItem("JobAddressValidation_UseStateRules", delegate
				{
					return new BooleanRegistryItem(
						"JobAddressValidation_UseStateRules",
						Categories.Operations_JobAddress,
						ResString.GetMultilingualString("69b4766d-aa7c-42bf-b80a-6eb38fb3ef81", "State/Province Validation Rule affects Job Addresses"),
						ResString.GetMultilingualString("bee616e3-2fe5-447e-b8c3-a0150437f56e", "If enabled, state validation on job addresses will follow the rule defined on the country/region. If disabled, states will not be mandatory on job addresses."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}
		public BooleanRegistryItem JobAddress_ShowUNLOCO
		{
			get
			{
				return GetItem("JobAddress_ShowUNLOCO", delegate
				{
					return new BooleanRegistryItem(
						"JobAddress_ShowUNLOCO",
						Categories.Operations_JobAddress,
						(NoResString)"Show UNLOCO on Job Addresses", // This is a support only flag
						(NoResString)"If enabled, show UNLOCO on job addresses. If disabled, will hide UNLOCO on job addresses.", // This is a support only flag
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		internal int OrgUserFlagCount
		{
			get { return 32; }
		}

		internal MultilingualStringRegistryItem OrgUserFlag1Label
		{
			get
			{
				return GetItem("OrgUserFlag1Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag1Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("8a838736-84dc-4b3c-baea-e4b2a4e36b23", "User Flag 01 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag1", "User Flag 01"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag2Label
		{
			get
			{
				return GetItem("OrgUserFlag2Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag2Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("412a03cc-40c9-4fd5-9242-7adabea806a4", "User Flag 02 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag2", "User Flag 02"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag3Label
		{
			get
			{
				return GetItem("OrgUserFlag3Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag3Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("90ff820d-3b6d-4289-b703-e3ee9052508d", "User Flag 03 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag3", "User Flag 03"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag4Label
		{
			get
			{
				return GetItem("OrgUserFlag4Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag4Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("195e5963-8f82-48e6-beb7-368043cb2d2f", "User Flag 04 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag4", "User Flag 04"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag5Label
		{
			get
			{
				return GetItem("OrgUserFlag5Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag5Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("ab50089f-ebd5-4d67-8319-385a3106b027", "User Flag 05 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag5", "User Flag 05"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag6Label
		{
			get
			{
				return GetItem("OrgUserFlag6Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag6Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("e52621bc-6ecd-407e-a303-1fae5658b083", "User Flag 06 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag6", "User Flag 06"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag7Label
		{
			get
			{
				return GetItem("OrgUserFlag7Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag7Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("02dd0f3f-0869-4411-b76c-90958b640e05", "User Flag 07 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag7", "User Flag 07"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag8Label
		{
			get
			{
				return GetItem("OrgUserFlag8Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag8Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("3d02cd7a-d5b3-4f82-8c6b-286120fde0b5", "User Flag 08 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag8", "User Flag 08"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag9Label
		{
			get
			{
				return GetItem("OrgUserFlag9Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag9Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("b5a8899a-3cb7-43c2-9f2a-f21ca9370cef", "User Flag 09 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag9", "User Flag 09"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag10Label
		{
			get
			{
				return GetItem("OrgUserFlag10Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag10Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("d98b1405-f239-49c0-89c0-b1d7f7b861d9", "User Flag 10 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag10", "User Flag 10"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag11Label
		{
			get
			{
				return GetItem("OrgUserFlag11Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag11Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("d6ed6a9e-3c1d-4d5e-888d-f0997bcf666d", "User Flag 11 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag11", "User Flag 11"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag12Label
		{
			get
			{
				return GetItem("OrgUserFlag12Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag12Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("81d73c31-4c6a-4397-9b59-6a25d91609a0", "User Flag 12 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag12", "User Flag 12"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag13Label
		{
			get
			{
				return GetItem("OrgUserFlag13Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag13Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("04a1a111-b8a9-436b-9706-11c9ef53f7c7", "User Flag 13 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag13", "User Flag 13"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag14Label
		{
			get
			{
				return GetItem("OrgUserFlag14Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag14Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("0a1e6cf6-599f-49bb-b76d-61d9a05acc08", "User Flag 14 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag14", "User Flag 14"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag15Label
		{
			get
			{
				return GetItem("OrgUserFlag15Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag15Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("54e9bf04-0fc0-4a74-9be5-9cadbd67c106", "User Flag 15 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag15", "User Flag 15"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag16Label
		{
			get
			{
				return GetItem("OrgUserFlag16Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag16Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("27ba02d8-f2fa-45dd-8ffd-c1960e871b1f", "User Flag 16 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag16", "User Flag 16"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag17Label
		{
			get
			{
				return GetItem("OrgUserFlag17Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag17Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("f3dc5594-852f-4dae-9893-edefc8ab3a4e", "User Flag 17 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag17", "User Flag 17"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag18Label
		{
			get
			{
				return GetItem("OrgUserFlag18Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag18Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("81165953-f95e-4ebe-88d1-75e424b7e7b8", "User Flag 18 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag18", "User Flag 18"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag19Label
		{
			get
			{
				return GetItem("OrgUserFlag19Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag19Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("a187d1c7-f618-4a37-a469-d39bdfb1f876", "User Flag 19 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag19", "User Flag 19"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag20Label
		{
			get
			{
				return GetItem("OrgUserFlag20Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag20Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("91916dac-02db-4896-bf92-155fb126304c", "User Flag 20 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag20", "User Flag 20"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag21Label
		{
			get
			{
				return GetItem("OrgUserFlag21Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag21Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("7c03fce9-a282-46f9-911a-494bbd5bca39", "User Flag 21 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag21", "User Flag 21"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag22Label
		{
			get
			{
				return GetItem("OrgUserFlag22Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag22Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("1fe8f032-c5c7-4dc1-b316-cc4643781e63", "User Flag 22 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag22", "User Flag 22"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag23Label
		{
			get
			{
				return GetItem("OrgUserFlag23Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag23Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("069a4ac6-05e9-4d3c-886a-8821be09687b", "User Flag 23 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag23", "User Flag 23"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag24Label
		{
			get
			{
				return GetItem("OrgUserFlag24Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag24Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("7bce3284-c0e8-48ff-8be1-cea0566645f9", "User Flag 24 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag24", "User Flag 24"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag25Label
		{
			get
			{
				return GetItem("OrgUserFlag25Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag25Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("B9CB56A1-3F0A-4B89-A6E4-7AA39CE51269", "User Flag 25 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag25", "User Flag 25"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag26Label
		{
			get
			{
				return GetItem("OrgUserFlag26Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag26Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("D1DCE754-0487-4F18-952D-38950E82BF84", "User Flag 26 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag26", "User Flag 26"));
				});
			}
		}
		internal MultilingualStringRegistryItem OrgUserFlag27Label
		{
			get
			{
				return GetItem("OrgUserFlag27Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag27Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("CFFC067D-B5A4-46E8-B942-3C08DCEA94F8", "User Flag 27 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag27", "User Flag 27"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag28Label
		{
			get
			{
				return GetItem("OrgUserFlag28Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag28Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("02BE4B53-076F-4E17-BEE2-D316695DD8FA", "User Flag 28 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag28", "User Flag 28"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag29Label
		{
			get
			{
				return GetItem("OrgUserFlag29Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag29Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("B074E7FE-1A5D-4FAC-B54D-2AA27B627EAE", "User Flag 29 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag29", "User Flag 29"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag30Label
		{
			get
			{
				return GetItem("OrgUserFlag30Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag30Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("7C93A66E-E88C-47EB-B884-EA182527A005", "User Flag 30 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag30", "User Flag 30"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag31Label
		{
			get
			{
				return GetItem("OrgUserFlag31Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag31Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("18028115-1C04-4D5D-9097-D7419566CDC9", "User Flag 31 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag31", "User Flag 31"));
				});
			}
		}

		internal MultilingualStringRegistryItem OrgUserFlag32Label
		{
			get
			{
				return GetItem("OrgUserFlag32Label", delegate
				{
					return new MultilingualStringRegistryItem("OrgUserFlag32Label", Categories.SalesMarketing_ClientIntelligence_MarketingFlags, ResString.GetMultilingualString("B3970EAB-9A0F-4B22-B13C-79E0A7A83A19", "User Flag 32 Label"), ResString.GetMultilingualString("15276ea0-f42b-4c32-925e-2d108b1714f6", "This will be displayed in the label on the Sales tab page."), new StringRegistryDataType(0, 20), null, RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("FieldCustomization.UserFlag32", "User Flag 32"));
				});
			}
		}
		//Temp A/R, Temp A/P, Temp Consignee, Temp Consignor

		public IRegistryItem TempOrgDebtorRequiredFields
		{
			get
			{
				return GetItem("TempOrgDebtorRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("TempOrgDebtorRequiredFields", Categories.Organizations_TempOrganizationRequiredFields, ResString.GetMultilingualString("0cb34dac-adfc-4ea7-abf3-470ab37ba89d", "Temporary Debtor Required Fields"), ResString.GetMultilingualString("26e17494-5269-4bd4-a7ee-eccdff6c6f1b", "Required fields for temporary debtor organizations."), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem TempOrgCreditorRequiredFields
		{
			get
			{
				return GetItem("TempOrgCreditorRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("TempOrgCreditorRequiredFields", Categories.Organizations_TempOrganizationRequiredFields, ResString.GetMultilingualString("3715d9d3-f388-4269-b0fa-01ad60b0cea9", "Temporary Creditor Required Fields"), ResString.GetMultilingualString("5c314f20-6e12-47dc-89a6-d4cffe48e0af", "Required fields for temporary creditor organizations."), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem TempOrgConsigneeRequiredFields
		{
			get
			{
				return GetItem("TempOrgConsigneeRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("TempOrgConsigneeRequiredFields", Categories.Organizations_TempOrganizationRequiredFields, ResString.GetMultilingualString("ec173f14-6de4-4f85-8733-f13e8545fa34", "Temporary Consignee Required Fields"), ResString.GetMultilingualString("c6c4ae11-1331-4c69-9b12-b1b29bb13b33", "Required fields for temporary consignee organizations."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem TempOrgSalesRequiredFields
		{
			get
			{
				return GetItem("TempOrgSalesRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("TempOrgSalesRequiredFields", Categories.Organizations_TempOrganizationRequiredFields, ResString.GetMultilingualString("a51d2c76-2b0b-4d8a-8c73-931876902d6c", "Temporary Sales Required Fields"), ResString.GetMultilingualString("42a5a309-bb21-4a7b-adb2-4f4b818ba20b", "Required fields for temporary sales organizations."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		public BooleanRegistryItem MakeSalesRepMandatoryForTempOrganization
		{
			get
			{
				return GetItem("MakeSalesRepMandatoryForTempOrganization", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem("MakeSalesRepMandatoryForTempOrganization",
						Categories.Organizations_TempOrganizationRequiredFields,
						ResString.GetMultilingualString("6e6c5ca3-3ef5-4400-bb6b-b3c78e755e46", "Make Sales Rep Mandatory"),
						ResString.GetMultilingualString("713c13ef-73d0-42f8-a697-148559fcfb44",
						@"If registry is turned on, when saving changes on temporary organizations (new or edits), if it does not have a sales rep record in staff assignment, it will auto add in a Sales rep record but leaving Initials field blank with an error, so that it cannot be saved until it is entered."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
					return result;
				});
			}
		}

		internal IRegistryItem TempOrgConsignorRequiredFields
		{
			get
			{
				return GetItem("TempOrgConsignorRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("TempOrgConsignorRequiredFields", Categories.Organizations_TempOrganizationRequiredFields, ResString.GetMultilingualString("f723919f-bd02-4813-a8cd-5bd765ce131d", "Temporary Consignor Required Fields"), ResString.GetMultilingualString("82ea5ca8-cd12-4ed3-8d81-eeb77e08d370", "Required fields for temporary consignor organizations."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgBrokerRequiredFields
		{
			get
			{
				return GetItem("OrgBrokerRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgBrokerRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("1af2cf74-11ec-4ca6-9971-7a229e2fb505", "Broker Required Fields"), ResString.GetMultilingualString("33183eaa-e3d6-4836-9820-67d53cece68b", "Required fields for Broker organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgCarrierRequiredFields
		{
			get
			{
				return GetItem("OrgCarrierRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgCarrierRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("ef98e399-ec09-40ad-b30e-e74a4d77ea40", "Carrier Required Fields"), ResString.GetMultilingualString("fc1db710-7b54-4b65-9611-ebdb7a72a7d0", "Required fields for Carrier organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgConsigneeRequiredFields
		{
			get
			{
				return GetItem("OrgConsigneeRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgConsigneeRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("46ca7225-146d-408c-b647-4fa79005da63", "Consignee Required Fields"), ResString.GetMultilingualString("6d2961c7-48b3-41dd-a775-62eef5f17233", "Required fields for Consignee organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgConsignorRequiredFields
		{
			get
			{
				return GetItem("OrgConsignorRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgConsignorRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("3e10ea17-fba1-461b-b01f-f59b519d3b70", "Consignor Required Fields"), ResString.GetMultilingualString("e6867fa4-02da-4d6f-a2f8-7d215b754ba6", "Required fields for Consignor organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgCompetitorRequiredFields
		{
			get
			{
				return GetItem("OrgCompetitorRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgCompetitorRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("1bb9e08c-8572-4c0e-847d-d602fcebf240", "Competitor Required Fields"), ResString.GetMultilingualString("b0e0d47a-b25a-49c8-aee3-641d547de51a", "Required fields for Competitor organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgContainerYardRequiredFields
		{
			get
			{
				return GetItem("OrgContainerYardRequiredFields", delegate
				{
					var result = new BinaryRegistryItem("OrgContainerYardRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("90899c6b-5d68-4c25-8e8d-69df94217953", "Container Yard Required Fields"), ResString.GetMultilingualString("2484d769-90b7-4adf-ab9d-0a3e846eeab4", "Required fields for Container Yard organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgCreditorRequiredFields
		{
			get
			{
				return GetItem("OrgCreditorRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgCreditorRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("f40e16ea-d616-43b1-a498-9dce1d36b604", "Creditor Required Fields"), ResString.GetMultilingualString("66eeb7bd-c052-4815-9338-fd2a78619a1f", "Required fields for Creditor organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgCTORequiredFields
		{
			get
			{
				return GetItem("OrgCTORequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgCTORequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("931e63bc-17e1-444d-9436-1f23632853b6", "CTO Required Fields"), ResString.GetMultilingualString("048df0b6-75e2-4bc0-8f99-b0e2cb774d65", "Required fields for CTO organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		public IRegistryItem OrgDebtorRequiredFields
		{
			get
			{
				return GetItem("OrgDebtorRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgDebtorRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("70db1328-aecc-4e41-aa7c-db627d9f538e", "Debtor Required Fields"), ResString.GetMultilingualString("5935df91-a441-41bf-84b9-d75ffe3e603a", "Required fields for Debtor organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgForwarderRequiredFields
		{
			get
			{
				return GetItem("OrgForwarderRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgForwarderRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("80c20441-4609-4536-8104-89a9452355e3", "Forwarder Required Fields"), ResString.GetMultilingualString("27693221-3293-474a-a2c0-ca426110e6fb", "Required fields for Forwarder organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgPackDepotRequiredFields
		{
			get
			{
				return GetItem("OrgPackDepotRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgPackDepotRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("52fb7c60-0537-44c6-ae52-1578111bea7b", "Pack Depot Required Fields"), ResString.GetMultilingualString("48c8c6a1-9656-4bcd-9599-72eefc381174", "Required fields for Pack Depot organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgSalesLeadRequiredFields
		{
			get
			{
				return GetItem("OrgSalesLeadRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgSalesLeadRequiredFields", Categories.SalesMarketing_ClientIntelligence, ResString.GetMultilingualString("2e4e59b1-d167-42e8-a676-a32b4f1d8faa", "Sales Lead Required Fields"), ResString.GetMultilingualString("1a3f06d3-eb95-43b4-a442-84600ee16dbd", "Required fields for Sales Lead organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgTransportClientRequiredFields
		{
			get
			{
				return GetItem("OrgTransportClientRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgTransportClientRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("17b6c895-df1d-4460-b638-0f191515e9a5", "Transport Client Required Fields"), ResString.GetMultilingualString("78de7c99-981a-4b59-9633-d072e9ee15d8", "Required fields for Transport Client organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		internal IRegistryItem OrgWarehouseRequiredFields
		{
			get
			{
				return GetItem("OrgWarehouseRequiredFields", delegate
				{
					IRegistryItem result = new BinaryRegistryItem("OrgWarehouseRequiredFields", Categories.Organizations_OrganizationRequiredFields, ResString.GetMultilingualString("7499c267-384d-479e-b289-ca48900050ee", "Warehouse Required Fields"), ResString.GetMultilingualString("ead2b1c7-a654-4041-9ed0-65937cb17db8", "Required fields for Warehouse organizations. Address 2 and City settings will not apply to addresses validated via the address validation web service."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new OrgRequiredFieldsRegistryEditorInfo();
					return result;
				});
			}
		}

		public IRegistryItem CanUserEditOrganisationCode
		{
			get
			{
				return GetItem("CanUserEditOrganisationCode", delegate
				{
					return new BooleanRegistryItem("CanUserEditOrganisationCode", Categories.Organizations_Codes, ResString.GetMultilingualString("7b821584-706b-4429-864d-cc246118fe1b", "Organization Codes can be edited"), ResString.GetMultilingualString("bb4f106c-637b-4058-9518-bf56801a2a27", "Can users edit organization codes?"), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false);
				});
			}
		}

		public BooleanRegistryItem EnableDynamics365Feature
		{
			get
			{
				return GetItem("EnableDynamics365Feature", () =>
					new BooleanRegistryItem("EnableDynamics365Feature",
						Categories.Organizations_DataExchange_Dynamics365,
						(NoResString)"Enable Dynamics365 Feature",
						(NoResString)"This registry setting controls the organizations and the relevant country specific information that will be exported to Dynamics 365, based on the login companies that are enabled",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		internal IRegistryItem UseBuyerSupplierRelationships
		{
			get
			{
				return GetItem("UseBuyerSupplierRelationships", delegate
				{
					return new BooleanRegistryItem("UseBuyerSupplierRelationships", Categories.Organizations_BuyerSupplierRelationships, ResString.GetMultilingualString("080af7d6-3eb8-4268-b10d-d2872dadad6c", "Use Buyer/Supplier Relationships"), ResString.GetMultilingualString("b55ff482-d67f-4f09-9726-2d5cdbe8574a", "Use Buyer/Supplier relationships for data entry."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem PromptToSaveBuyerSupplier
		{
			get
			{
				return GetItem("PromptToSaveBuyerSupplier", delegate
				{
					return new BooleanRegistryItem("PromptToSaveBuyerSupplier", Categories.Organizations_BuyerSupplierRelationships, ResString.GetMultilingualString("50ee1230-c8a1-4af7-8fcf-3fe62c97f326", "Prompt to save Buyer/Supplier Relationship"), ResString.GetMultilingualString("d9303b8f-14b9-4128-8e32-c567f77e3abe", "Prompt to save Buyer/Supplier relationship if relationship does not exist."), RegistryStorageFlags.System, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, true);
				});
			}
		}

		internal IRegistryItem GlobalTariffDefault
		{
			get
			{
				return GetItem("GlobalTariffDefault", delegate
				{
					return new IntRegistryItem("GlobalTariffDefault", Categories.Organizations_Rating, ResString.GetMultilingualString("611c280c-eb98-4bec-b67a-2ae65a86272c", "Default Company Tariff Level"), ResString.GetMultilingualString("15a436bf-1ac8-4d9b-ad52-e1a55309c9b1", "The default Company Tariff Level to use for organizations"), null, RegistryStorageFlags.System | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 0, 0, 255);
				});
			}
		}

		internal IRegistryItem OrgAllowMixedCase
		{
			get
			{
				return GetItem("OrgAllowMixedCase", delegate
				{
					return new BooleanRegistryItem("OrgAllowMixedCase", Categories.Organizations, ResString.GetMultilingualString("88d58463-6002-4d1e-abfa-76f9e7f84e9c", "Allow Mixed Case"), ResString.GetMultilingualString("14b00bcb-58a6-46d1-823f-9420beb0e96b", "Allow upper and lower case to be entered for Organization details"), RegistryStorageFlags.System, false);
				});
			}
		}

		internal IRegistryItem NextCallFollowUpDays
		{
			get
			{
				return GetItem("NextCallFollowUpDays", delegate
				{
					return new IntRegistryItem("NextCallFollowUpDays", Categories.Organizations, ResString.GetMultilingualString("4cbd382a-7578-48f8-a078-91e9cd2d4f60", "Next Call Follow Up Days"), ResString.GetMultilingualString("600e5ff5-e4c3-4b75-bbb8-f90dbe6c7f90", "The default number of days that a call should be followed up after."), RegistryStorageFlags.System, 14);
				});
			}
		}

		internal IRegistryItem OrgUsePhoneNumberFormatting
		{
			get
			{
				return GetItem("OrgUsePhoneNumberFormatting", delegate
				{
					return new BooleanRegistryItem("OrgUsePhoneNumberFormatting", Categories.Organizations, ResString.GetMultilingualString("7bb465eb-0fe6-4139-8445-87ba072c2adc", "Use Phone Number Formatting"), ResString.GetMultilingualString("17c7498e-243a-41dd-8fe9-a4a55b0f7218", "Automatically format phone numbers into international format."), RegistryStorageFlags.System, false);
				});
			}
		}

		internal IRegistryItem DowngradeInvalidPhoneNumbersToAWarning
		{
			get
			{
				return GetItem("DowngradeInvalidPhoneNumbersToAWarning", delegate
				{
					return new BooleanRegistryItem(
						"DowngradeInvalidPhoneNumbersToAWarning",
						Categories.Organizations,
						ResString.GetMultilingualString("938fd9fa-17e2-32bd-41ee-d05bd4ac33be", "Downgrade Invalid Phone Numbers to a Warning"), // IsOnlyForCargoWise.
						ResString.GetMultilingualString("bfbe74f2-b93d-fb91-4813-43694409c570", "Validate invalid phone numbers as warnings instead of errors."), // IsOnlyForCargoWise.
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		internal IRegistryItem NumericValuesOnlyForPhoneNumberFields
		{
			get
			{
				return GetItem("NumericValuesOnlyForPhoneNumberFields", delegate
				{
					return new BooleanRegistryItem(
							"NumericValuesOnlyForPhoneNumberFields",
							Categories.Organizations,
							ResString.GetMultilingualString("9B42E9CF-5F26-417A-8713-D144776BA9A7", "Numeric Values Only For Phone Number Fields"),
							ResString.GetMultilingualString("FCA41F90-4946-4295-974C-1430527AE76D",
							"If this registry is turned off and then the system would accept any value in phone number when click 'Accept as Entered', otherwise it would only accept numeric characters and '+','-' characters."),
							RegistryStorageFlags.System,
							false);
				});
			}
		}

		internal IRegistryItem DefaultNoteContextFromCurrentlyLoggedInDepartment
		{
			get
			{
				return GetItem("DefaultNoteContextFromCurrentlyLoggedInDepartment",
					() => new BooleanRegistryItem("DefaultNoteContextFromCurrentlyLoggedInDepartment", Categories.Organizations, ResString.GetMultilingualString("AB74C2E9-A24D-484B-84E5-E8C124D827E0", "Default Note Context from Currently Logged in Department"), ResString.GetMultilingualString("37CDE546-AE97-4B10-A074-D25960766564", "If this registry is turned on then newly created note will get the default value of context from the logged in department."), RegistryStorageFlags.System, false));
			}
		}

		internal IRegistryItem DefaultNoteCompanyFromCurrentlyLoggedInCompany
		{
			get
			{
				return GetItem("DefaultNoteCompanyFromCurrentlyLoggedInCompany",
					() => new BooleanRegistryItem("DefaultNoteCompanyFromCurrentlyLoggedInCompany", Categories.Organizations, ResString.GetMultilingualString("2CD160D9-210D-4A86-A895-BA70C247B6DF", "Default Note Company from Currently Logged  in Company"), ResString.GetMultilingualString("B522F160-BD33-4BF4-A05B-892B9E829668", "If this registry is turned on then newly created note will get the default value of company from the logged in company."), RegistryStorageFlags.System, false));
			}
		}

		internal OrgListRegistryItem PayablesCreditAgreedPaymentMethodsList
		{
			get
			{
				return GetItem("PayablesCreditAgreedPaymentMethodsList", delegate
				{
					var item = new OrgListRegistryItem("PayablesCreditAgreedPaymentMethodsList",
						ResString.GetMultilingualString("eabbcb04-1169-470b-b84e-9b006e49ea80", "Payables Credit Agreed Payment Methods"),
						ResString.GetMultilingualString("c29d224d-4f08-46e7-b560-e8c76e40b19d",
						"The list of valid payment methods that can be assigned against a Payables Organization to recording the settlement method agreed as part of the credit arrangements negotiated with suppliers."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						GetDefaultCreditAgreedPaymentMethods(false),
						PayablesDefaultCreditAgreedPaymentMethodsDefaultValueGetter);
					item.OnBuildLogReference += FindDeletedItemsForCodeDescriptionPairList;
					return item;
				});
			}
		}

		object PayablesDefaultCreditAgreedPaymentMethodsDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK) => CreditAgreedPaymentMethodsDefaultValueGetter(companyPK, GetDefaultCreditAgreedPaymentMethods(false));

		object CreditAgreedPaymentMethodsDefaultValueGetter(Guid companyPK, CodeDescriptionPairList list)
		{
			if (companyPK == Guid.Empty || !ObjectFactory.Get<IAccounting>().IsEPaymentFunctionalityEnabledForAnyProvider(companyPK))
			{
				list.RemoveCode(OrgConstants.CreditAgreedPaymentMethods.Code.EPayment);
			}
			return list;
		}

		static CodeDescriptionPairList GetDefaultCreditAgreedPaymentMethods(bool isReceivables)
		{
			var defaultCreditAgreedPaymentMethods = new CodeDescriptionPairList();
			defaultCreditAgreedPaymentMethods.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck, OrgDescriptions.CreditAgreedPaymentMethods.BusinessCheck);
			defaultCreditAgreedPaymentMethods.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard, OrgDescriptions.CreditAgreedPaymentMethods.CreditCard);
			defaultCreditAgreedPaymentMethods.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer, OrgDescriptions.CreditAgreedPaymentMethods.BankTransfer);
			defaultCreditAgreedPaymentMethods.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck, OrgDescriptions.CreditAgreedPaymentMethods.CashAndBankCheck);
			defaultCreditAgreedPaymentMethods.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard, OrgDescriptions.CreditAgreedPaymentMethods.DebitCard);
			if (isReceivables)
			{
				defaultCreditAgreedPaymentMethods.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest, OrgDescriptions.CreditAgreedPaymentMethods.CollectionRequest);
			}
			defaultCreditAgreedPaymentMethods.AddPair(OrgConstants.CreditAgreedPaymentMethods.Code.EPayment, OrgDescriptions.CreditAgreedPaymentMethods.EPayment);
			return defaultCreditAgreedPaymentMethods;
		}

		public static string FindDeletedItemsForCodeDescriptionPairList(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			return FindDeletedItemsForCodeDescriptionPairList(args.OriginalValue, args.NewValue);
		}

		internal static string FindDeletedItemsForCodeDescriptionPairList(object oldValue, object newValue)
		{
			string result = "";
			string oldCodesAsString = "";
			string newCodesAsString = "";
			ReadOnlyCodeDescriptionPairList oldCodeDescriptionPairList = oldValue as ReadOnlyCodeDescriptionPairList;
			ReadOnlyCodeDescriptionPairList newCodeDescriptionPairList = newValue as ReadOnlyCodeDescriptionPairList;

			if (oldCodeDescriptionPairList != null)
			{
				oldCodesAsString = oldCodeDescriptionPairList.CodesAsString;
			}
			if (newCodeDescriptionPairList != null)
			{
				newCodesAsString = newCodeDescriptionPairList.CodesAsString;
			}
			if (!string.IsNullOrEmpty(oldCodesAsString))
			{
				List<string> oldCodesList = new List<string>(oldCodesAsString.Split(','));
				List<string> newCodesList = new List<string>(newCodesAsString.Split(','));

				var deletedCodes = from code in oldCodesList
								   where !newCodesList.Contains(code)
								   select string.Format("[{0} - {1}] ", code, oldCodeDescriptionPairList.GetDescriptionFromCode(code));

				if (deletedCodes.Any())
				{
					result = Res.GetString("76e3dfa9-82c1-46f3-92ed-8a68a897f69d", "Deleted: {0}", deletedCodes.Aggregate((current, next) => current + next));
				}
			}
			return result.Length > 0 ? result.Substring(0, Math.Min(result.Length, 128)) : result;
		}

		#region OrganisationFormLists

		internal OrgListRegistryItem AddressAccessPointList
		{
			get
			{
				return GetItem("AddressAccessPoint", delegate
				{
					CodeDescriptionPairList addressAccessPointList = new CodeDescriptionPairList();
					addressAccessPointList.AddPair(OrgConstants.AccessPoint.Code.Dock, OrgDescriptions.AccessPoint.Dock);
					addressAccessPointList.AddPair(OrgConstants.AccessPoint.Code.Rack, OrgDescriptions.AccessPoint.Rack);
					addressAccessPointList.AddPair(OrgConstants.AccessPoint.Code.Interior, OrgDescriptions.AccessPoint.Interior);
					addressAccessPointList.AddPair(OrgConstants.AccessPoint.Code.InteriorViaElevator, OrgDescriptions.AccessPoint.InteriorViaElevator);
					addressAccessPointList.AddPair(OrgConstants.AccessPoint.Code.InteriorViaStairs, OrgDescriptions.AccessPoint.InteriorViaStairs);
					addressAccessPointList.AddPair(OrgConstants.AccessPoint.Code.Other, OrgDescriptions.AccessPoint.Other);

					return new OrgListRegistryItem("AddressAccessPoint", ResString.GetMultilingualString("6ec76918-9312-4fa2-9bb9-7d0702473fde", "Address Access Point"), ResString.GetMultilingualString("0e508f3b-eafb-4e6f-a83c-fb920c6366de", "The access points at which an organization's premises can be entered."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, addressAccessPointList);
				});
			}
		}

		internal OrgListRegistryItem AddressCommunicationRequiredList
		{
			get
			{
				return GetItem("AddressCommunicationRequired", delegate
				{
					CodeDescriptionPairList addressCommunicationRequiredList = new CodeDescriptionPairList();
					addressCommunicationRequiredList.AddPair(OrgConstants.CommunicationRequired.Code.Appointment, OrgDescriptions.CommunicationRequired.Appointment);
					addressCommunicationRequiredList.AddPair(OrgConstants.CommunicationRequired.Code.CallBefore, OrgDescriptions.CommunicationRequired.CallBefore);
					addressCommunicationRequiredList.AddPair(OrgConstants.CommunicationRequired.Code.NotifyBefore, OrgDescriptions.CommunicationRequired.NotifyBefore);
					addressCommunicationRequiredList.AddPair(OrgConstants.CommunicationRequired.Code.SeeNotes, OrgDescriptions.CommunicationRequired.SeeNotes);
					return new OrgListRegistryItem("AddressCommunicationRequired", ResString.GetMultilingualString("97a8f025-1694-487c-9c6f-a3526ac1cc1a", "Address Communication Required"), ResString.GetMultilingualString("34585656-49f7-45b7-932b-49169038f826", "Communication required with a premises prior to the delivery or pickup of goods."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, addressCommunicationRequiredList);
				});
			}
		}

		internal OrgListRegistryItem AddressDockHeightList
		{
			get
			{
				return GetItem("AddressDockHeight", delegate
				{
					CodeDescriptionPairList addressDockHeightList = new CodeDescriptionPairList();
					addressDockHeightList.AddPair(OrgConstants.DockHeight.Code.Standard, OrgDescriptions.DockHeight.Standard);
					addressDockHeightList.AddPair(OrgConstants.DockHeight.Code.NonStandard, OrgDescriptions.DockHeight.NonStandard);
					addressDockHeightList.AddPair(OrgConstants.DockHeight.Code.Other, OrgDescriptions.DockHeight.Other);
					return new OrgListRegistryItem("AddressDockHeight", ResString.GetMultilingualString("56e1e0a9-bb7a-4c68-99b9-4bb1f6d99dbc", "Address Dock Height"), ResString.GetMultilingualString("12457925-50db-4d4d-bb28-e62f5362aacf", "Dock height at a warehouse premises."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, addressDockHeightList);
				});
			}
		}

		internal OrgListRegistryItem AddressContainerHandlingList
		{
			get
			{
				return GetItem("AddressContainerHandling", delegate
				{
					CodeDescriptionPairList addressContainerHandlingList = new CodeDescriptionPairList();
					addressContainerHandlingList.AddPair(OrgConstants.ContainerHandling.Code.PackAndUnpack, OrgDescriptions.ContainerHandling.PackAndUnpack);
					addressContainerHandlingList.AddPair(OrgConstants.ContainerHandling.Code.PackOnly, OrgDescriptions.ContainerHandling.PackOnly);
					addressContainerHandlingList.AddPair(OrgConstants.ContainerHandling.Code.UnpackOnly, OrgDescriptions.ContainerHandling.UnpackOnly);
					addressContainerHandlingList.AddPair(OrgConstants.ContainerHandling.Code.NoPackOrUnpack, OrgDescriptions.ContainerHandling.NoPackOrUnpack);
					addressContainerHandlingList.AddPair(OrgConstants.ContainerHandling.Code.DropAndPull, OrgDescriptions.ContainerHandling.DropAndPull);
					addressContainerHandlingList.AddPair(OrgConstants.ContainerHandling.Code.Ask, OrgDescriptions.ContainerHandling.Ask);
					addressContainerHandlingList.AddPair(OrgConstants.ContainerHandling.Code.Other, OrgDescriptions.ContainerHandling.Other);
					return new OrgListRegistryItem("AddressContainerHandling", ResString.GetMultilingualString("fb78f198-d8a2-477b-b071-c8a84e8137e2", "Address Container Handling"), ResString.GetMultilingualString("d95edc4e-96d9-43cd-84a6-c852ef974f57", "Container handling capabilities of a warehouse premises."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, addressContainerHandlingList);
				});
			}
		}

		internal OrgListRegistryItem AddressLabourRequiredList
		{
			get
			{
				return GetItem("AddressLabourRequired", delegate
				{
					CodeDescriptionPairList addressLabourRequiredList = new CodeDescriptionPairList();
					addressLabourRequiredList.AddPair(OrgConstants.LabourRequired.Code.Yes, OrgDescriptions.LabourRequired.Yes);
					addressLabourRequiredList.AddPair(OrgConstants.LabourRequired.Code.No, OrgDescriptions.LabourRequired.No);
					addressLabourRequiredList.AddPair(OrgConstants.LabourRequired.Code.Ask, OrgDescriptions.LabourRequired.Ask);
					addressLabourRequiredList.AddPair(OrgConstants.LabourRequired.Code.FCLOnly, OrgDescriptions.LabourRequired.FCLOnly);
					addressLabourRequiredList.AddPair(OrgConstants.LabourRequired.Code.OutOfGauge, OrgDescriptions.LabourRequired.OutOfGauge);
					addressLabourRequiredList.AddPair(OrgConstants.LabourRequired.Code.HeavyPieces, OrgDescriptions.LabourRequired.HeavyPieces);
					addressLabourRequiredList.AddPair(OrgConstants.LabourRequired.Code.Other, OrgDescriptions.LabourRequired.Other);
					return new OrgListRegistryItem("AddressLabourRequired", ResString.GetMultilingualString("e4d59c0a-e656-4b40-b2dc-e054eec6a154", "Address Labor Required"), ResString.GetMultilingualString("019db500-42c1-4cd4-8096-a0b69acdb808", "Additional labor required at the premises for the pickup and delivery of goods."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, addressLabourRequiredList);
				});
			}
		}

		internal OrgListRegistryItem DeliveryRoutesList
		{
			get
			{
				return GetItem("DeliveryRoutes", delegate
				{
					var deliveryRoutesList = new CodeDescriptionPairList();
					return new OrgListRegistryItem("DeliveryRoutes", ResString.GetMultilingualString("49849E35-3DD3-4483-8169-F09DA1DF591A", "Delivery Routes"), ResString.GetMultilingualString("346CE9D2-DDD2-4E41-A483-2EA5982C7A2F", "A Delivery Route is used to create a list of organization addresses for the delivery of goods."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, deliveryRoutesList);
				});
			}
		}

		internal OrgListRegistryItem AgentCategoryList
		{
			get
			{
				return GetItem("AgentCategory", delegate
				{
					CodeDescriptionPairList agentCategoryList = new CodeDescriptionPairList();
					agentCategoryList.AddPair(Constants.AccountsCategory.Standard, ResString.GetMultilingualString("Common|AgentCategoryList|Standard", "Standard accounts relationship"));

					return new OrgListRegistryItem("AgentCategory", ResString.GetMultilingualString("dab82b58-f2a5-4028-b910-4f22be1c704e", "Agent Category List"), ResString.GetMultilingualString("751f16b5-2bd6-47a4-a6c3-b2800f64bd6c", "The list of valid entries for category of Agent."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, agentCategoryList);
				});
			}
		}

		internal OrgListRegistryItem ARCreditRatingList
		{
			get
			{
				return GetItem("ARCreditRating", delegate
				{
					CodeDescriptionPairList aRCreditRatingList = new CodeDescriptionPairList();

					aRCreditRatingList.AddPair(OrgConstants.ARCreditRating.Code.VeryLowRisk, ResString.GetMultilingualString("MasterFiles|ARCreditRatingList|VeryLowRisk", "Very Low Credit Risk"));
					aRCreditRatingList.AddPair(OrgConstants.ARCreditRating.Code.LowRisk, ResString.GetMultilingualString("MasterFiles|ARCreditRatingList|LowRisk", "Low Credit Risk"));
					aRCreditRatingList.AddPair(OrgConstants.ARCreditRating.Code.Normal, ResString.GetMultilingualString("MasterFiles|ARCreditRatingList|Normal", "Normal Credit Risk"));
					aRCreditRatingList.AddPair(OrgConstants.ARCreditRating.Code.HighRisk, ResString.GetMultilingualString("MasterFiles|ARCreditRatingList|HighRisk", "High Credit Risk"));
					aRCreditRatingList.AddPair(OrgConstants.ARCreditRating.Code.VeryHighRisk, ResString.GetMultilingualString("MasterFiles|ARCreditRatingList|VeryHighRisk", "Very High Risk"));

					return new OrgListRegistryItem("ARCreditRating", ResString.GetMultilingualString("89ef107c-e6ee-4f21-883a-adf43d583e2b", "Receivables Credit Rating List"), ResString.GetMultilingualString("cb282503-c55f-4395-9cf3-edc0fb83dcfb", "The Accounts Receivables credit rating list."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, aRCreditRatingList);
				});
			}
		}

		internal OrgListRegistryItem CompetitorActivityList
		{
			get
			{
				return GetItem("CompetitorActivity", delegate
				{
					CodeDescriptionPairList competitorActivityList = new CodeDescriptionPairList();
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.Freight, ResString.GetMultilingualString("Commmon|CompetitorActivity|Freight", "Freight"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.Brokerage, ResString.GetMultilingualString("Commmon|CompetitorActivity|Brokerage", "Brokerage"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.FrtAndBrk, ResString.GetMultilingualString("Commmon|CompetitorActivity|FrtAndBrk", "Freight and Brokerage"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.Transport, ResString.GetMultilingualString("Commmon|CompetitorActivity|Transport", "Transport"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.FrtAndTrn, ResString.GetMultilingualString("Commmon|CompetitorActivity|FrtAndTrn", "Freight and Transport"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.BrkAndTrn, ResString.GetMultilingualString("Commmon|CompetitorActivity|BrkAndTrn", "Brokerage and Transport"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.TrnBrkFrt, ResString.GetMultilingualString("Commmon|CompetitorActivity|TrnBrkFrt", "Freight, Brokerage and Transport"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.Warehouse, ResString.GetMultilingualString("Commmon|CompetitorActivity|Warehouse", "Warehouse"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.FrtAndWhs, ResString.GetMultilingualString("Commmon|CompetitorActivity|FrtAndWhs", "Freight and Warehouse"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.BrkAndWhs, ResString.GetMultilingualString("Commmon|CompetitorActivity|BrkAndWhs", "Brokerage and Warehouse"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.TrnAndWhs, ResString.GetMultilingualString("Commmon|CompetitorActivity|TrnAndWhs", "Transport and Warehouse"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.FrtBrkWhs, ResString.GetMultilingualString("Commmon|CompetitorActivity|FrtBrkWhs", "Freight, Brokerage and Warehouse"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.FrtTrnWhs, ResString.GetMultilingualString("Commmon|CompetitorActivity|FrtTrnWhs", "Freight, Transport and Warehouse"));
					competitorActivityList.AddPair(Constants.Sales.CompetitorActivity.All, ResString.GetMultilingualString("Commmon|CompetitorActivity|All", "Freight, Brokerage, Transport and Warehouse"));
					return new OrgListRegistryItem("CompetitorActivity", ResString.GetMultilingualString("66913ab9-fb45-4c53-a887-f4688a8eece4", "Competitor Activity List"), ResString.GetMultilingualString("44252467-41ea-4cb5-b93f-9e330ab5ebb8", "The list of valid entries for activities of Competitors."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, competitorActivityList);
				});
			}
		}

		internal OrgListRegistryItem CompetitorCategoryList
		{
			get
			{
				return GetItem("CompetitorCategory", delegate
				{
					CodeDescriptionPairList competitorCategoryList = new CodeDescriptionPairList();
					competitorCategoryList.AddPair(Constants.AccountsCategory.Standard, ResString.GetMultilingualString("Commmon|CompetitorCategoryList|Standard", "Standard accounts relationship"));

					return new OrgListRegistryItem("CompetitorCategory", ResString.GetMultilingualString("b23e45f4-f677-4690-bf74-1fc35f2aaf1b", "Competitor Category List"), ResString.GetMultilingualString("8d84dd5d-7f22-4a11-b78f-9dd9ac17bd16", "The list of valid entries for category of Competitors."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, competitorCategoryList);
				});
			}
		}

		internal OrgListRegistryItem ExporterCategoryList
		{
			get
			{
				return GetItem("ExporterCategory", delegate
				{
					CodeDescriptionPairList exporterCategoryList = new CodeDescriptionPairList();
					exporterCategoryList.AddPair(Constants.AccountsCategory.Standard, ResString.GetMultilingualString("Commmon|ExporterCategoryList|Standard", "Standard accounts relationship"));

					return new OrgListRegistryItem("ExporterCategory", ResString.GetMultilingualString("1cf18fec-149b-4a0f-aa0d-6e64e5306a76", "Exporter Category List"), ResString.GetMultilingualString("5491d80f-a1a0-4c48-a035-505a7c50c75d", "The list of valid entries for category of Exporter."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, exporterCategoryList);
				});
			}
		}

		internal OrgListRegistryItem ImporterCategoryList
		{
			get
			{
				return GetItem("ImporterCategory", delegate
				{
					CodeDescriptionPairList importerCategoryList = new CodeDescriptionPairList();
					importerCategoryList.AddPair(Constants.AccountsCategory.Standard, ResString.GetMultilingualString("Common|ImporterCategoryList|Standard", "Standard accounts relationship"));

					return new OrgListRegistryItem("ImporterCategory", ResString.GetMultilingualString("d7c144a7-1d40-4182-8d1e-998a520e1572", "Importer Category List"), ResString.GetMultilingualString("0bb1d2f1-6765-4b5b-94b7-f7438eeb1f8b", "The list of valid entries for category of Importer."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, importerCategoryList);
				});
			}
		}

		internal OrgListRegistryItem PayablesCategoryList
		{
			get
			{
				return GetItem("PayablesCategory", delegate
				{
					CodeDescriptionPairList payablesCategoryList = new CodeDescriptionPairList();
					payablesCategoryList.AddPair(Constants.AccountsCategory.Standard, ResString.GetMultilingualString("Commmon|PayablesCategoryList|Standard", "Standard accounts relationship"));
					payablesCategoryList.AddPair(Constants.AccountsCategory.Key, ResString.GetMultilingualString("Commmon|PayablesCategoryList|Key", "Key Supplier"));
					payablesCategoryList.AddPair(Constants.AccountsCategory.Significant, ResString.GetMultilingualString("Commmon|PayablesCategoryList|Significant", "Significant Supplier"));
					payablesCategoryList.AddPair(Constants.AccountsCategory.Alternative, ResString.GetMultilingualString("Commmon|PayablesCategoryList|Alternative", "Alternative Supplier only"));

					return new OrgListRegistryItem("PayablesCategory", ResString.GetMultilingualString("f25499ed-3b43-4770-9b2f-8b0e74ced470", "Payables Category List"), ResString.GetMultilingualString("9f1e6b64-a2c5-4676-af03-9edfdd46b126", "The list of valid entries for category of Accounts Payable clients."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, payablesCategoryList);
				});
			}
		}

		internal OrgListRegistryItem ReceivablesCategoryList
		{
			get
			{
				return GetItem("ReceivablesCategory", delegate
				{
					CodeDescriptionPairList receivablesCategoryList = new CodeDescriptionPairList();
					receivablesCategoryList.AddPair(Constants.AccountsCategory.Standard, ResString.GetMultilingualString("Accounting|ReceivablesCategoryList|Standard", "Standard accounts relationship"));
					receivablesCategoryList.AddPair(Constants.AccountsCategory.Key, ResString.GetMultilingualString("Accounting|ReceivablesCategoryList|Key", "Key Client"));
					receivablesCategoryList.AddPair(Constants.AccountsCategory.Significant, ResString.GetMultilingualString("Accounting|ReceivablesCategoryList|Significant", "Significant client"));

					return new OrgListRegistryItem("ReceivablesCategory", ResString.GetMultilingualString("cac89c1b-6277-422d-84a8-5f7d3c556b2f", "Receivables Category List"), ResString.GetMultilingualString("35ee7b9a-46a9-4e37-b979-eb5362b77952", "The list of valid entries for category of Accounts Receivable clients."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, receivablesCategoryList);
				});
			}
		}

		internal OrgListRegistryItem CarrierCategoryList
		{
			get
			{
				return GetItem("CarrierCategoryList", delegate
				{
					CodeDescriptionPairList carrierCategoryCodeDescriptionPairList = new CodeDescriptionPairList();
					carrierCategoryCodeDescriptionPairList.AddPair(Constants.CarrierCategory.PreferredAll, ResString.GetMultilingualString("Common|CarrierCategoryList|PreferredAll", "Preferred carrier for all business units."));
					carrierCategoryCodeDescriptionPairList.AddPair(Constants.CarrierCategory.PreferredThisCountry, ResString.GetMultilingualString("Common|CarrierCategoryList|PreferredThisCountry", "Preferred carrier for all this country/region."));
					carrierCategoryCodeDescriptionPairList.AddPair(Constants.CarrierCategory.Secondary, ResString.GetMultilingualString("Common|CarrierCategoryList|Secondary", "Secondary preference."));
					carrierCategoryCodeDescriptionPairList.AddPair(Constants.CarrierCategory.Occasional, ResString.GetMultilingualString("Common|CarrierCategoryList|Occasional", "Use occasionally on miscellaneous routes."));
					carrierCategoryCodeDescriptionPairList.AddPair(Constants.CarrierCategory.DoNotUse, ResString.GetMultilingualString("Common|CarrierCategoryList|DoNotUse", "Do not use this carrier without management approval."));
					return new OrgListRegistryItem("CarrierCategoryList", ResString.GetMultilingualString("ae0b3667-0701-4399-b09d-d0139d8e203b", "Carrier Category List"), ResString.GetMultilingualString("e0216579-ad07-44c6-92e8-41dc313f5556", "The list of valid entries for preferred carriers."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, carrierCategoryCodeDescriptionPairList);
				});
			}
		}

		internal OrgListRegistryItem ServicesCategoryList
		{
			get
			{
				return GetItem("ServicesCategoryList", delegate
				{
					CodeDescriptionPairList servicesCategoryCodeDescriptionPairList = new CodeDescriptionPairList();
					servicesCategoryCodeDescriptionPairList.AddPair(Constants.ServicesCategory.Preferred, ResString.GetMultilingualString("Commmon|ServicesCategoryList|Preferred", "Preferred Service Provider for all business units."));
					servicesCategoryCodeDescriptionPairList.AddPair(Constants.ServicesCategory.Secondary, ResString.GetMultilingualString("Commmon|ServicesCategoryList|Secondary", "Secondary preference."));
					servicesCategoryCodeDescriptionPairList.AddPair(Constants.ServicesCategory.Occasional, ResString.GetMultilingualString("Commmon|ServicesCategoryList|Occasional", "Use occasionally for miscellaneous work."));
					servicesCategoryCodeDescriptionPairList.AddPair(Constants.ServicesCategory.DoNotUse, ResString.GetMultilingualString("Commmon|ServicesCategoryList|DoNotUse", "Do not use this service provider without management approval."));
					return new OrgListRegistryItem("ServicesCategoryList", ResString.GetMultilingualString("96c5aa10-ed91-4ffd-bf6f-6de66c89fd5d", "Services Category List"), ResString.GetMultilingualString("34371c06-8fed-41c1-8823-882193d5b924", "The list of valid entries for preferred service providers."), DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, servicesCategoryCodeDescriptionPairList);
				});
			}
		}

		internal OrgListRegistryItem OrgListOfContactAllocations
		{
			get
			{
				return GetItem("OrgListOfContactAllocations", delegate
				{
					var item = new OrgListRegistryItem("OrgListOfContactAllocations",
						ResString.GetMultilingualString("6997328A-3864-43D8-B878-8ED1D9E0A887", "List of Contact Allocations"),
						ResString.GetMultilingualString("24CF015B-3AC3-4C45-AD89-35771286405E", "This is a list of external organizations that need a Contact point from within an organization."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ContactAllocationsList);
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowEmptyCodes = false;
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowDuplicateCodes = false;
					((CodeDescriptionPairListRegistryDataType)item.DataType).IsEmptyListAllowed = false;
					((CodeDescriptionPairListRegistryDataType)item.DataType).KeepDefaultValues = true;
					return item;
				});
			}
		}

		ReadOnlyCodeDescriptionPairList ContactAllocationsList
		{
			get
			{
				if (contactAllocationsList == null)
				{
					var allocationList = new CodeDescriptionPairList();

					allocationList.AddPair(OrgConstants.ContactAllocationType.CAPGA, ResString.GetMultilingualString("05157220-FDBF-49C6-A99A-851B79FFAED0", "CA PGA"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.CNCUS, ResString.GetMultilingualString("77A2763A-6B80-4C82-A54A-1081B7E9340C", "China Customs/CIQ"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.HAZ, ResString.GetMultilingualString("42317B87-ECD1-4D6A-8B07-6865052A5485", "D/G - Dangerous Goods"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.CUS, ResString.GetMultilingualString("{DAB3F0B7-64BC-4EC0-AA89-61F44776D28B}", "Customs"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.CEOForKRCustoms, ResString.GetMultilingualString("A4A65D9D-8419-4C94-8DFA-79021C41B20F", "Korea Company Representative"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.KRS, ResString.GetMultilingualString("2FEBE106-8080-4A04-9FC8-E29F78F2711A", "Korea Secondary Company Representative"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.ValuationAuthorityForKRCustoms, ResString.GetMultilingualString("2B4F91E2-2D5D-43E2-8E3E-CE694B30B166", "KR Customs Valuation Authority Contact"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.BRForeignOperator, ResString.GetMultilingualString("6B53C19B-C3BC-4A4C-B2B5-47EC5A054E6E", "BR Foreign Operator Responsible Contact"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.NZBiosecurity, ResString.GetMultilingualString("B67FE815-CA4D-4DB3-A736-7136E0FC3CEF", "MPI Biosecurity"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.NZCustoms, ResString.GetMultilingualString("027E0A31-A667-4251-98DD-4FD3708E499B", "New Zealand Customs Service"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.USFSV, ResString.GetMultilingualString("E26D9053-71C6-40CC-8E27-29966587D896", "US FDA Foreign Supplier Verification Program Contact"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.USPGA, ResString.GetMultilingualString("ED1FB744-EC85-4028-9541-694820405635", "US PGA Contact"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.VAT, ResString.GetMultilingualString("E9178B65-2D9A-46B3-A687-81B955928E1B", "Customs VAT Reporting"));
					allocationList.AddPair(OrgConstants.ContactAllocationType.CIV, ResString.GetMultilingualString("0F496834-7136-43C2-8315-4519A7C8888A", "Commercial Invoice Party (Exporter)"));
					allocationList.Sort();
					contactAllocationsList = allocationList;
				}

				return contactAllocationsList;
			}
		}
		ReadOnlyCodeDescriptionPairList contactAllocationsList;

		internal OrgListRegistryItem OrgListOfInterests
		{
			get
			{
				return GetItem("OrgListOfInterests", delegate
				{
					CodeDescriptionPairList interestsList = new CodeDescriptionPairList();

					interestsList.AddPair("AQS", ResString.GetMultilingualString("79515f0b-5a4a-4a80-9021-20883a5dbe87", "Aqua Sports"));
					interestsList.AddPair("ART", ResString.GetMultilingualString("e54a30b4-ff66-4bf6-bba8-7de3a72a96c2", "Arts"));
					interestsList.AddPair("BAS", ResString.GetMultilingualString("51feee11-fbb1-4a8d-ac22-d809c6f5c1ad", "Baseball"));
					interestsList.AddPair("BAB", ResString.GetMultilingualString("681b0768-74b7-49a2-a60f-0b2b2acb7a65", "Basketball"));
					interestsList.AddPair("BOW", ResString.GetMultilingualString("bd0f0c0a-023e-46c1-8f9b-4e3b2f0da8fd", "Bowling"));
					interestsList.AddPair("COM", ResString.GetMultilingualString("52b9bf33-639d-4eae-a382-41670beb4544", "Computing"));
					interestsList.AddPair("CON", ResString.GetMultilingualString("d469bf8d-5c45-414a-af50-6f0383c40efc", "Concerts"));
					interestsList.AddPair("CRI", ResString.GetMultilingualString("bf9cfe64-88b3-4de7-a460-cbee7b9b0088", "Cricket"));
					interestsList.AddPair("CYC", ResString.GetMultilingualString("527d3461-8d24-4634-aa37-5284555730f0", "Cycling"));
					interestsList.AddPair("DAN", ResString.GetMultilingualString("57d3dc2c-864f-4293-9240-0f5c8e17a4be", "Dancing"));
					interestsList.AddPair("DIV", ResString.GetMultilingualString("1efe8d56-6038-4cea-b6ec-05cde83efb7e", "Diving"));
					interestsList.AddPair("FAS", ResString.GetMultilingualString("19e1274b-9ec4-4897-b274-325af3cf66a9", "Fashion"));
					interestsList.AddPair("FIS", ResString.GetMultilingualString("a9889a59-82c3-463c-aa5c-a57b27abb168", "Fishing"));
					interestsList.AddPair("FWI", ResString.GetMultilingualString("27792042-4a99-4dea-8069-239b62d6cd63", "Food and Wine"));
					interestsList.AddPair("ARL", ResString.GetMultilingualString("80731ec8-d488-4aae-aba3-5395903bee2a", "Football (Australian Rules)"));
					interestsList.AddPair("NRL", ResString.GetMultilingualString("04361066-60f6-46a5-8407-7d5c8c9189e0", "Football (Rugby League)"));
					interestsList.AddPair("ARU", ResString.GetMultilingualString("bc1c2d73-48f6-477f-8459-38be4d71e271", "Football (Rugby Union)"));
					interestsList.AddPair("GLF", ResString.GetMultilingualString("d4fb088f-d426-4161-abd9-e438123561a9", "Golf"));
					interestsList.AddPair("GYM", ResString.GetMultilingualString("80899794-b580-4c68-800d-62df24209270", "Gymnastics"));
					interestsList.AddPair("HKY", ResString.GetMultilingualString("ee23a166-8e1c-4d4e-973d-fa3137ff10b5", "Hockey"));
					interestsList.AddPair("HRS", ResString.GetMultilingualString("f6dbb7e4-a8ac-4c81-8dbe-d36b5cea9229", "Horse Racing"));
					interestsList.AddPair("INV", ResString.GetMultilingualString("a7491b47-9338-4d23-8c26-b0f77f6bf7ff", "Investments"));
					interestsList.AddPair("THT", ResString.GetMultilingualString("76a03e50-a3d4-4c7d-a5ad-6e59e5f49060", "Live Theater"));
					interestsList.AddPair("MAA", ResString.GetMultilingualString("e0b19f95-aaa7-4f82-b496-84ffa8d513c0", "Martial Arts"));
					interestsList.AddPair("MTR", ResString.GetMultilingualString("24950a71-be6d-4869-b66e-8a82ce25bdfd", "Motorsports"));
					interestsList.AddPair("MOV", ResString.GetMultilingualString("7830e27a-9374-4524-a09e-f542d2b14411", "Movies"));
					interestsList.AddPair("MUS", ResString.GetMultilingualString("f1ed6e70-a69c-424c-aeb2-be521db773fa", "Music"));
					interestsList.AddPair("MUC", ResString.GetMultilingualString("db81e21e-2991-4e05-aa48-2ba8d1ba40d2", "Musicals"));
					interestsList.AddPair("NET", ResString.GetMultilingualString("c4f4678f-76ca-4962-9ce3-812a1119796f", "Netball"));
					interestsList.AddPair("PNT", ResString.GetMultilingualString("94f66ad2-b4b3-461a-8f3b-d662fb0b036f", "Paintball"));
					interestsList.AddPair("PHT", ResString.GetMultilingualString("7dbdd429-4271-4202-aa06-5ca779279e72", "Photography"));
					interestsList.AddPair("RED", ResString.GetMultilingualString("C3ED57BD-9FAE-44f4-8A07-3B59DFBCC863", "Reading/Books"));
					interestsList.AddPair("RES", ResString.GetMultilingualString("d99a4393-a0e8-4b25-a31f-e9cee823bd03", "Real Estate"));
					interestsList.AddPair("RUN", ResString.GetMultilingualString("f37b82bc-382c-4558-ae8a-27d36abaeb5b", "Running"));
					interestsList.AddPair("SAI", ResString.GetMultilingualString("f7cfffd2-5df7-4b12-880c-c8e1a19bb835", "Sailing"));
					interestsList.AddPair("SHO", ResString.GetMultilingualString("93f5eb4e-3f80-4061-924e-84b36270a2fa", "Shopping"));
					interestsList.AddPair("SKA", ResString.GetMultilingualString("cc4875b2-3853-4bb3-b057-d41307ee3309", "Skating"));
					interestsList.AddPair("SNO", ResString.GetMultilingualString("03c8548e-b775-4d46-825c-a32ec6364bfe", "Snowboarding"));
					interestsList.AddPair("SOC", ResString.GetMultilingualString("9897b1b0-3331-452c-8df3-ad947841193b", "Soccer"));
					interestsList.AddPair("SNL", ResString.GetMultilingualString("808d54f2-e7f6-4477-b092-e34a4715d478", "Social Networking Link"));
					interestsList.AddPair("SQU", ResString.GetMultilingualString("a270cc1e-a17d-4fe5-85d5-f1c419e0ab81", "Squash"));
					interestsList.AddPair("SUR", ResString.GetMultilingualString("2f25d338-f98d-42f3-a8aa-e1f86c31c41f", "Surfing"));
					interestsList.AddPair("SWI", ResString.GetMultilingualString("e5892020-6c44-4d32-af62-92d1a3b23b4d", "Swimming"));
					interestsList.AddPair("TEN", ResString.GetMultilingualString("e8075c96-c974-41f1-a27c-adf7dd4a9c24", "Tennis"));
					interestsList.AddPair("TRI", ResString.GetMultilingualString("1e740c26-d0c1-40ef-8999-73af9f5467b4", "Triathlon"));
					interestsList.AddPair("ULF", ResString.GetMultilingualString("ce50901a-e2ee-4bb0-8c60-a575fd6783e9", "Ultimate Frisbee"));
					interestsList.AddPair("VDO", ResString.GetMultilingualString("a646cc71-7685-40ba-825a-33a374fe1e27", "Video Games"));
					interestsList.AddPair("VLB", ResString.GetMultilingualString("3a1efd74-bfc0-41a9-9914-94762baddd15", "Volleyball"));
					interestsList.AddPair("WAL", ResString.GetMultilingualString("d3e5c2fd-36d1-42be-82a2-5f908ce37439", "Walking"));

					var item = new OrgListRegistryItem("OrgListOfInterests",
						ResString.GetMultilingualString("0e792161-b724-4550-b5c4-07916bc555f8", "List of Contact Attributes & Interests"),
						ResString.GetMultilingualString("f76dba08-37b4-4292-916a-2efe30166ba0", "This is a list of attributes & interests that can be specified against each contact on an organization."),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						interestsList);
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowEmptyCodes = false;
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowDuplicateCodes = false;
					return item;
				});
			}
		}

		internal OrgListRegistryItem OrgStaffMemberAssignmentRoles
		{
			get
			{
				return GetItem("OrgStaffMemberAssignmentRoles", delegate
				{
					var item = new OrgListRegistryItem("OrgStaffMemberAssignmentRoles",
						ResString.GetMultilingualString("5c82366c-c05d-4786-afe8-687818900651", "Staff Member Assignment Roles"),
						ResString.GetMultilingualString("9E362BD4-4035-4C1C-AF63-1EBD0AC32A43", @"The list of valid entries for Staff Member Assignment Roles.

Please note that you will need to close and re-open the registry before these changes are reflected in other parts of the registry."),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						new StaffAssignmentRoles());

					var dataType = (CodeDescriptionPairListRegistryDataType)item.DataType;
					dataType.AllowEmptyDescriptions = false;
					dataType.AllowDuplicateCodes = false;

					return item;
				});
			}
		}

		#endregion

		#endregion

		#region Sales & Marketing

		internal CodeDescriptionPairListRegistryItem SalesCategoryList
		{
			get
			{
				return GetItem("SalesCategory", delegate
				{
					CodeDescriptionPairList salesCategoryList = new CodeDescriptionPairList();
					salesCategoryList.AddPair(Constants.AccountsCategory.Standard, ResString.GetMultilingualString("Common|SalesCategoryList|Standard", "Standard accounts relationship"));

					return new CodeDescriptionPairListRegistryItem(
							"SalesCategory",
							Categories.SalesMarketing_ClientIntelligence,
							ResString.GetMultilingualString("76b30a7e-f796-4beb-a31f-aaacb03a8e51", "Sales Category List"),
							ResString.GetMultilingualString("ecc313f4-c7ea-4092-b82b-b69ffdbf449b", "The list of valid entries for category of Sales Lead clients."),
							3,
							RegistryStorageFlags.System,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							salesCategoryList);
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem SalesEffectOnCostList
		{
			get
			{
				return GetItem("SalesEffectOnCost", delegate
				{
					CodeDescriptionPairList salesEffectOnCostList = new CodeDescriptionPairList();
					salesEffectOnCostList.AddPair(Constants.Sales.EffectOnCosts.LrgDecrease, ResString.GetMultilingualString("Commmon|SalesEffectOnCosts|LrgDecrease", "Large decrease in costs"));
					salesEffectOnCostList.AddPair(Constants.Sales.EffectOnCosts.MedDecrease, ResString.GetMultilingualString("Commmon|SalesEffectOnCosts|MedDecrease", "Medium decrease in costs"));
					salesEffectOnCostList.AddPair(Constants.Sales.EffectOnCosts.SmlDecrease, ResString.GetMultilingualString("Commmon|SalesEffectOnCosts|SmlDecrease", "Small decrease in costs"));
					salesEffectOnCostList.AddPair(Constants.Sales.EffectOnCosts.Negligible, ResString.GetMultilingualString("Commmon|SalesEffectOnCosts|Negligible", "Negligible effect on costs"));
					salesEffectOnCostList.AddPair(Constants.Sales.EffectOnCosts.SmlIncrease, ResString.GetMultilingualString("Commmon|SalesEffectOnCosts|SmlIncrease", "Small increase in costs"));
					salesEffectOnCostList.AddPair(Constants.Sales.EffectOnCosts.MedIncrease, ResString.GetMultilingualString("Commmon|SalesEffectOnCosts|MedIncrease", "Medium increase in costs"));
					salesEffectOnCostList.AddPair(Constants.Sales.EffectOnCosts.LrgIncrease, ResString.GetMultilingualString("Commmon|SalesEffectOnCosts|LrgIncrease", "Large increase in costs"));

					return new CodeDescriptionPairListRegistryItem(
							"SalesEffectOnCost",
							Categories.SalesMarketing_ClientIntelligence,
							ResString.GetMultilingualString("7ef37c45-4bbc-4fc6-b6f5-b7611b66be7f", "Sales Effect On Cost List"),
							ResString.GetMultilingualString("f7398145-15d2-489d-96a2-bb71104484b2", "The list of valid entries for effect Sales Leads clients will have on costs."),
							3,
							RegistryStorageFlags.System,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							salesEffectOnCostList);
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem SalesGrowthOutlookList
		{
			get
			{
				return GetItem("SalesGrowthOutlook", delegate
				{
					CodeDescriptionPairList salesGrowthOutlookList = new CodeDescriptionPairList();
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.LIE, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|LIE", "Large increase in business due to customer expansion."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.LIN, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|LIN", "Large increase in business due to winning business from competitor."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.SIE, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|SIE", "Small increase in business due to customer expansion."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.SIN, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|SIN", "Small increase in business due to winning business from competitor."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.NOR, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|NOR", "Business will remain roughly the same."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.SDC, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|SDC", "Small decrease in business due to customer contraction."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.SDL, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|SDL", "Small decrease in business due to losing business to competitor."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.LDC, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|LDC", "Large decrease in business due to customer contraction."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.CLC, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|CLC", "Complete loss of business due to customer contraction."));
					salesGrowthOutlookList.AddPair(Constants.Sales.GrowthOutlook.CLL, ResString.GetMultilingualString("MasterFiles|SalesGrowthOutlookList|CLL", "Complete loss of business due to losing business to competitor."));

					return new CodeDescriptionPairListRegistryItem(
							"SalesGrowthOutlook",
							Categories.SalesMarketing_ClientIntelligence,
							ResString.GetMultilingualString("dd92d8dd-fe90-4dd5-8058-ea42eed9146b", "Sales Growth Outlook List"),
							ResString.GetMultilingualString("2cffec68-c75e-423d-b06a-f2d9a91b5a7b", "The list of valid entries for growth outlook of Sales Leads."),
							3,
							RegistryStorageFlags.System,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							salesGrowthOutlookList);
				});
			}
		}

		internal CodeDescriptionPairListRegistryItem SalesTerritoryList
		{
			get
			{
				return GetItem("SalesTerritoryList", delegate
				{
					CodeDescriptionPairList salesTerritoryCodeDescriptionPairList = new CodeDescriptionPairList();
					salesTerritoryCodeDescriptionPairList.AddPair(Constants.Sales.Territory.Branch, ResString.GetMultilingualString("Commmon|SalesTerritoryList|Branch", "Territory is the branch of this client."));

					return new CodeDescriptionPairListRegistryItem(
							"SalesTerritoryList",
							Categories.SalesMarketing_ClientIntelligence,
							ResString.GetMultilingualString("067c5e07-9ac6-4dfd-853e-8b479a39a84f", "Sales Territory List"),
							ResString.GetMultilingualString("8dfffacc-6f2c-4cc4-91f4-260db38b1df5", "The list of valid sales territories."),
							3,
							RegistryStorageFlags.System,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							salesTerritoryCodeDescriptionPairList);
				});
			}
		}

		#region Competitor Intelligence

		internal CodeDescriptionPairListRegistryItem SalesStyleList
		{
			get
			{
				return GetItem("SalesStyle", delegate
				{
					CodeDescriptionPairList salesStyleList = new CodeDescriptionPairList();
					salesStyleList.AddPair(Constants.Sales.Style.PriceBased, ResString.GetMultilingualString("Commmon|SalesStyle|PriceBased", "Price based"));
					salesStyleList.AddPair(Constants.Sales.Style.ServiceBase, ResString.GetMultilingualString("Commmon|SalesStyle|ServiceBase", "Service based"));  // This is supported on all platforms
					salesStyleList.AddPair(Constants.Sales.Style.PriceAndService, ResString.GetMultilingualString("Commmon|SalesStyle|PriceAndService", "Price and service based"));
					salesStyleList.AddPair(Constants.Sales.Style.Integration, ResString.GetMultilingualString("Commmon|SalesStyle|Integration", "Integration"));
					salesStyleList.AddPair(Constants.Sales.Style.Technology, ResString.GetMultilingualString("Commmon|SalesStyle|Technology", "Technology"));
					salesStyleList.AddPair(Constants.Sales.Style.Size, ResString.GetMultilingualString("Commmon|SalesStyle|Size", "Size"));
					salesStyleList.AddPair(Constants.Sales.Style.Coverage, ResString.GetMultilingualString("Commmon|SalesStyle|Coverage", "Coverage"));
					salesStyleList.AddPair(Constants.Sales.Style.Specialist, ResString.GetMultilingualString("Commmon|SalesStyle|Specialist", "Specialist"));
					salesStyleList.AddPair(Constants.Sales.Style.Other, ResString.GetMultilingualString("Commmon|SalesStyle|Other", "Other - see notes"));

					return new CodeDescriptionPairListRegistryItem("SalesStyle",
							Categories.SalesMarketing_CompetitorIntelligence,
							ResString.GetMultilingualString("78dde6de-dd01-4140-aba3-d19710e1285a", "Sales Style List"),
							ResString.GetMultilingualString("ea2dae2c-7d70-428b-9e43-39572b0fe62b", "The list of valid entries for style of Sales Leads."),
							3,
							RegistryStorageFlags.System,
							DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
							salesStyleList);
				});
			}
		}

		#endregion

		#endregion

		#region User Interface

		internal IRegistryItem NavBarState
		{
			get
			{
				return GetItem("NavBarState", delegate
				{
					return new StringRegistryItem("NavBarState", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, nameof(NavBarStyles.OutlookBar));
				});
			}
		}

		internal IRegistryItem TimeLineView
		{
			get
			{
				return GetItem("TimeLineViewState", delegate
				{
					return new StringRegistryItem("TimeLineViewState", null, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden, "TimeLine");
				});
			}
		}

		internal IRegistryItem ShowUnshippedOrders
		{
			get
			{
				return GetItem("ShowUnshippedOrdersMode", delegate
				{
					return new StringRegistryItem("ShowUnshippedOrdersMode", null, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden, "DoNotShow");
				});
			}
		}

		internal IRegistryItem WebUserDefaultSettingsForDocAddress
		{
			get
			{
				return GetItem("WebUserDefaultSettingsForDocAddress", delegate
				{
					return new StringRegistryItem("WebUserDefaultSettingsForDocAddress", null, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden, "");
				});
			}
		}

		internal IRegistryItem LastAccessedModule
		{
			get
			{
				return GetItem("LastAccessedModule", delegate
				{
					return new StringRegistryItem("LastAccessedModule", Categories.System_UI, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		public IntRegistryItem MaximumNumberOfRecentItems
		{
			get
			{
				return GetItem("MaximumNumberOfRecentItems", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfRecentItems",
						Categories.System_UI,
						ResString.GetMultilingualString("e4b0c7f7-15d0-486b-a56a-590c340cf480", "Maximum Number of Recent Items"),
						ResString.GetMultilingualString("19370f07-fedd-47e1-9b0d-959f946034d2", "Number of items to show in the Recent Items list."),
						RegistryStorageFlags.Company, 30);
				});
			}
		}

		public IntRegistryItem MaximumNumberOfFavorites
		{
			get
			{
				return GetItem("MaximumNumberOfFavorites", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfFavorites",
						Categories.System_UI,
						ResString.GetMultilingualString("9c2bc070-2399-459a-a3c3-626b4ba90f4d", "Maximum Number of Favorites"),
						ResString.GetMultilingualString("fad590a7-1bc7-4fa1-921e-4713fde40900", "Number of favorite to show in the Favorites list."),
						RegistryStorageFlags.Company, 12);
				});
			}
		}

		public IntRegistryItem MaximumNumberOfMyTasks
		{
			get
			{
				return GetItem("MaximumNumberOfMyTasks", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfMyTasks",
						Categories.System_UI,
						ResString.GetMultilingualString("c75f42e3-a5f9-464e-856d-f76c280d88ee", "Maximum Number of My Tasks"),
						ResString.GetMultilingualString("850dd651-8234-4754-aa19-4bebb77de0c7", "Number of tasks to show in the My Tasks list."),
						RegistryStorageFlags.Company, 50);
				});
			}
		}

		public BooleanRegistryItem OpenModuleInANewWindow
		{
			get
			{
				return GetItem("OpenModuleInANewWindow", delegate
				{
					return new BooleanRegistryItem(
						"OpenModuleInANewWindow",
						Categories.System_UI,
						ResString.GetMultilingualString("151ce74c-46d9-4046-a89a-c52964fe9d88", "Open Module In A New Window"),
						ResString.GetMultilingualString("a7e85822-ecb8-4ed4-ab98-8ab9cd3a1895", "Open module of main form in a new window."),
						RegistryStorageFlags.Company, false);
				});
			}
		}

		public IntRegistryItem MaximumNumberOfRecentModules
		{
			get
			{
				return GetItem("MaximumNumberOfRecentModules", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfRecentModules",
						Categories.System_UI,
						ResString.GetMultilingualString("3aa20f2f-9129-4237-ba08-3d7352f89330", "Maximum Number of Recent Modules"),
						ResString.GetMultilingualString("909aa02a-5d16-48e7-9b78-838c8ef236e5", "Number of modules to show in the Recent Modules list."),
						RegistryStorageFlags.Company, 5);
				});
			}
		}

		internal StringArrayRegistryItem FavoriteModules
		{
			get
			{
				return GetItem("FavoriteModules", () =>
				{
					var result = new StringArrayRegistryItem("FavoriteModules", Categories.System_UI, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden | RegistryOptions.NotCached, Array.Empty<string>());
					result.DataType.MaximumLength = 1000;

					return result;
				});
			}
		}

		internal StringArrayRegistryItem RecentModules
		{
			get
			{
				return GetItem("RecentModules", () =>
				{
					var result = new StringArrayRegistryItem("RecentModules", Categories.System_UI, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden | RegistryOptions.NotCached, Array.Empty<string>());
					result.DataType.MaximumLength = 1000;

					return result;
				});
			}
		}

		internal StringArrayRegistryItem RecentItems
		{
			get
			{
				return GetItem("RecentItems", () =>
				{
					var result = new StringArrayRegistryItem("RecentItems", Categories.System_UI, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden | RegistryOptions.NotCached, Array.Empty<string>());
					result.DataType.MaximumLength = 1000;

					return result;
				});
			}
		}

		public StringArrayRegistryItem RecentItemsByModule(string moduleKey)
		{
			return GetItem(moduleKey, () =>
			{
				var result = new StringArrayRegistryItem(moduleKey, RawDataRegistry.Categories.System_UI, null, null, RegistryStorageFlags.CompanyDepartment, RegistryOptions.IsHidden | RegistryOptions.NotCached, Array.Empty<string>());
				result.DataType.MaximumLength = 1000;

				return result;
			});
		}

		internal IRegistryItem GlobalFormTopCaption
		{
			get
			{
				return GetItem("GlobalFormTopCaption", delegate
				{
					return new PhysicalServerRegistryItem("GlobalFormTopCaption", Categories.System_FormTopCaption, ResString.GetMultilingualString("38dbe498-b83d-41cf-99ff-6a5b70f3619f", "Caption"), ResString.GetMultilingualString("26caaf63-8704-406a-8548-baa137f2db03", "Use this to set up e.g. (Test) or (Live) system captions for all forms in the system"), RegistryDataTypes.StringType, RegistryOptions.PreserveTestValue);
				});
			}
		}

		internal IRegistryItem ShowDatabaseName
		{
			get
			{
				return GetItem("ShowDatabaseName", delegate
				{
					return new PhysicalServerRegistryItem("ShowDatabaseName", Categories.System_FormTopCaption, ResString.GetMultilingualString("b63da1d2-2fe6-4946-b5a7-144562d73a3d", "Show Database Name"),
						ResString.GetMultilingualString("071416ab-f0d8-4ace-bbc9-3cae6c49520e", "Display the database name in the title bar of every form. This can help users to distinguish between multiple installations of the system."),
						RegistryDataTypes.BoolType, false);
				});
			}
		}

		internal IRegistryItem ShowBranchName
		{
			get
			{
				return GetItem("ShowBranchName", delegate
				{
					return new PhysicalServerRegistryItem("ShowBranchName", Categories.System_FormTopCaption, ResString.GetMultilingualString("08ce817b-f344-4993-a4f7-78161d0020bf", "Show Branch Name"),
						ResString.GetMultilingualString("514f42fb-9fa5-4ea3-b35b-c2a011315b0b", "Display the current branch name in the title bar of every form. This can help users to ensure that they are posting transactions in the correct branch."),
						RegistryDataTypes.BoolType, true);
				});
			}
		}

		internal IRegistryItem ShowCompanyName
		{
			get
			{
				return GetItem("ShowCompanyName", delegate
				{
					return new PhysicalServerRegistryItem("ShowCompanyName", Categories.System_FormTopCaption, ResString.GetMultilingualString("621bfc9e-be98-4194-900f-5990310b9d88", "Show Company Name"),
							ResString.GetMultilingualString("3eb5ad0d-81eb-4c9a-aa44-652105ac7ff3", "Display the current company name in the title bar of every form. This can help users to ensure that they are posting transactions in the correct company."),
							RegistryDataTypes.BoolType, true);
				});
			}
		}

		internal IRegistryItem ShowDepartmentName
		{
			get
			{
				return GetItem("ShowDepartmentName", delegate
				{
					return new PhysicalServerRegistryItem("ShowDepartmentName", Categories.System_FormTopCaption, ResString.GetMultilingualString("7e8fa7e9-9264-46b8-b5ef-0086bdb8804a", "Show Department Name"),
						ResString.GetMultilingualString("8b222c67-a807-4fa5-9e56-cd5db5f78d8c", "Display the current department name in the title bar of every form. This can help users to ensure that they are posting transactions in the correct department."),
						RegistryDataTypes.BoolType, true);
				});
			}
		}

		internal IRegistryItem ShowUserName
		{
			get
			{
				return GetItem("ShowUserName", delegate
				{
					return new PhysicalServerRegistryItem("ShowUserName", Categories.System_FormTopCaption, ResString.GetMultilingualString("7122A071-15A7-4FD4-8120-E33D3B05AEC3", "Show User Name"),
						ResString.GetMultilingualString("80379BF5-5879-4D17-AA52-3CD7458D087B", "Display the current user name in the title bar of every form. This can help users to ensure that they are posting transactions as the correct user."),
						RegistryDataTypes.BoolType, false);
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May not have logged in yet")]
		internal BinaryRegistryItem FormPosition
		{
			get
			{
				return GetItem("Form", delegate
				{
					return new BinaryRegistryItem("Form", Categories.Forms, null, null, RegistryStorageFlags.Company, RegistryOptions.NotLogged | RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem ShowZGridDragAndDropInformation
		{
			get
			{
				return GetItem("ShowZGridDragAndDropInformation", delegate
				{
					return new BooleanRegistryItem("ShowZGridDragAndDropInformation", Categories.System_UI, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
				});
			}
		}

		#endregion

		#region Password Control

		internal IRegistryItem PasswordHashingIterationsCount
		{
			get
			{
				return GetItem("PasswordHashingIterationsCount", delegate
				{
					return new IntRegistryItem("PasswordHashingIterationsCount",
						Categories.PasswordControl,
						(NoResString)"Password Hashing Iterations Count", // IsOnlyForCargoWise.
						(NoResString)"Number of times we are rehashing the passwords.", // IsOnlyForCargoWise.
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						200_000,
						1,
						int.MaxValue);
				});
			}
		}

		internal IRegistryItem PasswordHistoryCount
		{
			get
			{
				return GetItem("PasswordHistoryCount", delegate
				{
					return new IntRegistryItem("PasswordHistoryCount", Categories.PasswordControl, ResString.GetMultilingualString("07346aa1-9726-4ffd-bad8-fd686f55e5fb", "History Count"), ResString.GetMultilingualString("57cd25ee-8379-4f0f-af6d-a31c50f9795e", "Number of previously used passwords to remember."), null, RegistryStorageFlags.System, HideWhenADEnabled, 0, 0, 7);
				});
			}
		}

		internal IRegistryItem PasswordChangeDays
		{
			get
			{
				return GetItem("PasswordChangeDays", delegate
				{
					return new IntRegistryItem("PasswordChangeDays", Categories.PasswordControl, ResString.GetMultilingualString("2b73ec6f-1576-45f6-bdcd-8ac4918a7aca", "Password Change Days"), ResString.GetMultilingualString("cf3e5ab4-c86f-4928-8194-aa7b9d49c7d3", "Force password change after (n) days."), null, RegistryStorageFlags.System, HideWhenADEnabled, 0, 0, 365);
				});
			}
		}

		internal IRegistryItem PromptPasswordChangeBeforeExpireDays
		{
			get
			{
				return GetItem("PromptPasswordChangeBeforeExpireDays", delegate
				{
					return new IntRegistryItem("PromptPasswordChangeBeforeExpireDays", Categories.PasswordControl, ResString.GetMultilingualString("b560cf0a-40ae-4bd7-b461-ed7c94270e57", "Prompt Password Change Days"), ResString.GetMultilingualString("f3746c7b-1ad2-427b-a3da-32646818616e", "Prompt for password change (n) days before expiry."), RegistryStorageFlags.System, 7);
				});
			}
		}

		internal IRegistryItem LoginAttempts
		{
			get
			{
				return GetItem("LoginAttempts", delegate
				{
					return new IntRegistryItem("LoginAttempts", Categories.PasswordControl, ResString.GetMultilingualString("9d80c948-a16f-47ea-b47c-a4131a79049a", "Login Attempts"), ResString.GetMultilingualString("89615c66-c8ee-4921-94fb-b254eb2bf3c9", "Number of failed login attempts before login is locked out. (0 = Do not disable)."), RegistryStorageFlags.System, HideWhenADEnabled, 0);
				});
			}
		}

		public IRegistryItem LoginLockoutMinutes
		{
			get
			{
				return GetItem("LoginLockoutMinutes", delegate
				{
					return new IntRegistryItem("LoginLockoutMinutes", Categories.PasswordControl,
						ResString.GetMultilingualString("8df682cf-aea3-4a80-8055-16763bdb5d86", "Lockout Minutes"),
						ResString.GetMultilingualString("C47BAE33-7580-401C-8229-183DBCCD884F",
							"Number of minutes to lockout user after failed login attempts. \r\n(Note: 0 = Permanently locked out, and will display in the \"Locked Out Until\" field as '06-JUN-78')."),
						RegistryStorageFlags.System,
						HideWhenADEnabled,
						15);
				});
			}
		}

		internal IRegistryItem PasswordMinLength
		{
			get
			{
				return GetItem("PasswordMinLength", delegate
				{
					return new IntRegistryItem("PasswordMinLength", Categories.PasswordControl, ResString.GetMultilingualString("56a9b188-ff9b-4004-a355-9080c4edc394", "Minimum Length"), ResString.GetMultilingualString("fce0b8ce-906a-4023-b416-abe16dbe0675", "Minimum length of password."), RegistryStorageFlags.System, HideWhenADEnabled, 0);
				});
			}
		}

		internal IRegistryItem PasswordMinUpperAlphas
		{
			get
			{
				return GetItem("PasswordMinUpperAlphas", delegate
				{
					return new IntRegistryItem("PasswordMinUpperAlphas", Categories.PasswordControl, ResString.GetMultilingualString("fb823143-d7a3-4e7b-ae91-fe62da41b0dc", "Minimum Upper Alphas"), ResString.GetMultilingualString("c6e4bac8-8b71-44d8-8da6-642a39d22a8b", "Minimum number of uppercase alpha characters."), RegistryStorageFlags.System, HideWhenADEnabled, 0);
				});
			}
		}

		internal IRegistryItem PasswordMinLowerAlphas
		{
			get
			{
				return GetItem("PasswordMinLowerAlphas", delegate
				{
					return new IntRegistryItem("PasswordMinLowerAlphas", Categories.PasswordControl, ResString.GetMultilingualString("e1947f27-f4e5-49db-8da9-2baf78c4ef8c", "Minimum Lower Alphas"), ResString.GetMultilingualString("5dac5d95-1f03-44a1-9b28-7aee59884dc8", "Minimum number of lowercase alpha characters."), RegistryStorageFlags.System, HideWhenADEnabled, 0);
				});
			}
		}

		internal IRegistryItem PasswordMinNumeric
		{
			get
			{
				return GetItem("PasswordMinNumeric", delegate
				{
					return new IntRegistryItem("PasswordMinNumeric", Categories.PasswordControl, ResString.GetMultilingualString("ce8c63e2-b013-4a2f-bbe6-da0e6dd51930", "Minimum Numerics"), ResString.GetMultilingualString("fb8c4b3c-0d5b-411d-9e9f-ed7442be0b79", "Minimum number of numerics."), RegistryStorageFlags.System, HideWhenADEnabled, 0);
				});
			}
		}

		internal IRegistryItem PasswordMinNonAlphNums
		{
			get
			{
				return GetItem("PasswordMinNonAlphaNums", delegate
				{
					return new IntRegistryItem("PasswordMinNonAlphaNums", Categories.PasswordControl, ResString.GetMultilingualString("4e2221ac-e1ce-4bdd-8240-4c2d8f964211", "Minimum Non-Alphanumerics"), ResString.GetMultilingualString("af8532a9-4440-4c7f-b44f-b4a0c48676dc", "Minimum number of non-alphanumeric characters."), RegistryStorageFlags.System, HideWhenADEnabled, 0);
				});
			}
		}

		public BooleanRegistryItem EnforceChangePasswordAtNextLogin
		{
			get
			{
				return GetItem("EnforceChangePasswordAtNextLogin", delegate
				{
					return new BooleanRegistryItem(
						"EnforceChangePasswordAtNextLogin",
						Categories.PasswordControl,
						ResString.GetMultilingualString("4fb6ea1b-d185-46da-a447-e1f98dba4106", "Enforce 'Change Password At Next Login'"),
						ResString.GetMultilingualString("eb1524ff-de0c-445d-bd4c-dd48023c9ca6", "If set to 'Yes', the 'Change Password at Next Login' checkbox can not be unticked until the user changes their password."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		RegistryOptions HideWhenADEnabled => ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default;

		internal IRegistryItem ShowAvailableBranchesOnly
		{
			get
			{
				return GetItem("ShowAvailableBranchesOnly",
					delegate
					{
						return new BooleanRegistryItem(
							"ShowAvailableBranchesOnly",
							Categories.System_Staff,
							ResString.GetMultilingualString("85b8ea8c-c92a-4050-a462-eac40ae1d8f9", "Show Available Branches Only"),
							ResString.GetMultilingualString("bd4c1eed-269a-4029-837d-7d7c7701ff7d", "If set to 'Yes', the branch drop-down in the login dialog box will display the list of branches the nominated user is authorized to log in to. Authorized login Branches and Departments are set in the security tree at 'Login Branches and Departments' on Staff or Group security."),
							RegistryStorageFlags.System,
							false);
					});
			}
		}

		internal IRegistryItem ShowAvailableDepartmentsOnly
		{
			get
			{
				return GetItem("ShowAvailableDepartmentsOnly",
					delegate
					{
						return new BooleanRegistryItem(
							"ShowAvailableDepartmentsOnly",
							Categories.System_Staff,
							ResString.GetMultilingualString("164a23b0-97ef-4e45-ab4b-53329f484018", "Show Available Departments Only"),
							ResString.GetMultilingualString("060cf9c3-8903-4beb-9fea-531c6fe2be6f", "If set to 'Yes', the department drop-down in the login dialog box will display the list of departments the nominated user is authorized to log in to. Authorized login Branches and Departments are set in the security tree at 'Login Branches and Departments' on Staff or Group security."),
							RegistryStorageFlags.System,
							false
						);
					});
			}
		}

		#endregion

		#region DocumentScanning

		internal IRegistryItem BurnCDSettings
		{
			get
			{
				return GetItem("BurnCDSettingsXML", delegate
				{
					return new BinaryRegistryItem("BurnCDSettingsXML", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem DoNotDisplayReleaseNotesVersion
		{
			get
			{
				return GetItem("DoNotDisplayReleaseNotesVersion", delegate
				{
					return new StringRegistryItem("DoNotDisplayReleaseNotesVersion", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem DoNotDisplayUpdatesOnLogin
		{
			get
			{
				return GetItem("DoNotDisplayUpdatesOnLogin", delegate
				{
					return new BooleanRegistryItem("DoNotDisplayUpdatesOnLogin", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);
				});
			}
		}

		internal IRegistryItem DoNotDisplayUpdatesOnModuleEntry
		{
			get
			{
				return GetItem("DoNotDisplayUpdatesOnModuleEntry", delegate
				{
					return new BooleanRegistryItem("DoNotDisplayUpdatesOnModuleEntry", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
				});
			}
		}

		internal IRegistryItem ShowReadNotes
		{
			get
			{
				return GetItem("ShowReadNotes", delegate
				{
					return new BooleanRegistryItem("ShowReadNotes", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, false);
				});
			}
		}

		internal IRegistryItem ModuleToShowNotesFor
		{
			get
			{
				return GetItem("ModuleToShowNotesFor", delegate
				{
					return new StringRegistryItem("ModuleToShowNotesFor", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem DMMagnifyingGlassSettings
		{
			get
			{
				return GetItem("DMMagnifyingGlassSettingsXML", delegate
				{
					return new BinaryRegistryItem("DMMagnifyingGlassSettingsXML", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem DMThumbnailSettings
		{
			get
			{
				return GetItem("DMThumbnailSettingsXML", delegate
				{
					return new BinaryRegistryItem("DMThumbnailSettingsXML", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem DMImportConfigurationSettings
		{
			get
			{
				return GetItem("DMImportConfigurationSettingsXML", delegate
				{
					return new BinaryRegistryItem("DMImportConfigurationSettingsXML", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		internal IRegistryItem DMScanningFileOutputOptionSettings
		{
			get
			{
				return GetItem("DMScanningFileOutputOptionSettingsXML", delegate
				{
					return new BinaryRegistryItem("DMScanningFileOutputOptionSettingsXML", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden);
				});
			}
		}

		#endregion

		#region Debug Settings

		internal StringRegistryItem DebugBusinessObjectType
		{
			get
			{
				return GetItem("DebugBusinessObjectType", delegate
				{
					return new StringRegistryItem(
						"DebugBusinessObjectType",
						Categories.Debug, // Developer only text
						(NoResString)"Log Business Object Type", // Developer only text
						(NoResString)"Enter the Business object type to do a call stack log each time it is being created.\r\nE.g. Enterprise.MasterFiles.Business.OrgHeader", // Developer only text
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						EntityFrameworkRegistryDefaults.DebugBusinessObjectType
						);
				});
			}
		}

		public IntRegistryItem UberFactoryTimeoutPeriod
		{
			get
			{
				return GetItem("UberFactoryTimeoutPeriod", delegate
				{
					return new IntRegistryItem(
						"UberFactoryTimeoutPeriod",
						Categories.Debug,
						(NoResString)"UberFactory Timeout Period", // Developer only text
						(NoResString)"The period of time in seconds after which the UberFactory will time out.", // Developer only text
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						EntityFrameworkRegistryDefaults.UberFactoryTimeoutPeriod);
				});
			}
		}

		public IntRegistryItem DbConnectionTimeoutPeriod
		{
			get
			{
				return GetItem("DbConnectionTimeoutPeriod", delegate
				{
					return new IntRegistryItem(
						"DbConnectionTimeoutPeriod",
						Categories.Debug,
						(NoResString)"DbConnection Timeout Period", // Developer only text
						(NoResString)"The period of time in seconds after which DbConnections will time out. (0 to disable.)\r\nDefault: the minimum interval of database polling from upgrade checker or heartbeat updates + 60 seconds", // Developer only text
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						(EnvProxy.Instance?.SemaphoreProvider?.InternalHeartbeat?.KeepAliveIntervalMs ?? EntityFrameworkRegistryDefaults.DefaultHeartbeatPulseInMs) / 1000 + 60,
						0,
						int.MaxValue);
				});
			}
		}

		public BooleanRegistryItem FeatureTestModeEnabled =>
			GetItem(nameof(FeatureTestModeEnabled), () => new BooleanRegistryItem(
				nameof(FeatureTestModeEnabled),
				Categories.Debug, // Requires further product/UX review before promoting to user-visible
				(NoResString)"Enable Feature Test Mode", // Developer only text. Can't make this a resource string because another test fails saying that developer-only text shouldn't be resource stringed.
				(NoResString)"Enables the feature test module. Disabling will also disable all feature tests", // Developer only text. Can't make this a resource string because another test fails saying that developer-only text shouldn't be resource stringed.
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false
			));

		public BooleanRegistryItem WebVersionUserMonitoringEnabled =>
			GetItem(nameof(WebVersionUserMonitoringEnabled), () => new BooleanRegistryItem(
					nameof(WebVersionUserMonitoringEnabled),
					Categories.CargoWiseWebVersion,
					(NoResString)"Enabled Real User Monitoring",
					(NoResString)"Real User Monitoring provides detailed performance metrics and error tracking",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
					false));

		public StringRegistryItem WebVersionUserMonitoringUrl =>
			GetItem(nameof(WebVersionUserMonitoringUrl), () => new StringRegistryItem(
					nameof(WebVersionUserMonitoringUrl),
					Categories.CargoWiseWebVersion,
					(NoResString)"Monitoring Server URL",
					(NoResString)"The URL used to make requests to the Monitoring Server.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
					string.Empty));

		public StringRegistryItem WebVersionLaunchUrl =>
			GetItem(nameof(WebVersionLaunchUrl), () => new StringRegistryItem(
				nameof(WebVersionLaunchUrl),
				Categories.CargoWiseWebVersion,
				(NoResString)"Launch URL",
				(NoResString)"The URL used to launch a instance of CargoWise Web.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
				string.Empty));

		public StringRegistryItem BlazorUrl =>
			GetItem(nameof(BlazorUrl), () => new StringRegistryItem(
					nameof(BlazorUrl),
					Categories.Debug, // Requires further product/UX review before promoting to user-visible
					(NoResString)"Blazor URL", // Developer only text. Can't make this a resource string because another test fails saying that developer-only text shouldn't be resource stringed.
					(NoResString)"URL of the Blazor instance of this system.", // Developer only text. Can't make this a resource string because another test fails saying that developer-only text shouldn't be resource stringed.
																			   // UriRegistryDataType would be a better fit, but it doesn't allow flexibility of https or http depending on production vs. developer environment.
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers,
#if DEBUG
					"https://localhost:5001/backchannel" // Default for developers in Blazor
#else
					""
#endif
				));

		public StringRegistryItem WebVersionSiteOfflineMessage =>
			GetItem(nameof(WebVersionSiteOfflineMessage), () => new StringRegistryItem(
				nameof(WebVersionSiteOfflineMessage),
				Categories.Debug,
				(NoResString)"Site Offline Message",
				(NoResString)"If this is set, logins to Web Version will be blocked and this error message will be shown to users.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				string.Empty));

		internal IntRegistryItem BlazorBackchannelConnectionTimeout
		{
			get
			{
				return GetItem("BlazorBackchannelConnectionTimeout", delegate
				{
					return new IntRegistryItem(
						"BlazorBackchannelConnectionTimeout",
						Categories.Debug, // Requires further product/UX review before promoting to user-visible
						(NoResString)"Blazor Backchannel Connection Timeout", // Developer only text. Can't make this a resource string because another test fails saying that developer-only text shouldn't be resource stringed.
						(NoResString)"The blazor backchannel connection timeout for hybird mode in progress pop up window in seconds", // Developer only text. Can't make this a resource string because another test fails saying that developer-only text shouldn't be resource stringed.
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						30);
				});
			}
		}

		#endregion

		#region CFS

		#region Storage Free Days

		public IRegistryItem CFSSeaFreightLCLStorageFreeDays
		{
			get
			{
				return GetItem("CFSSeaFreightStorageFreeDays", delegate
				{
					return new IntRegistryItem("CFSSeaFreightStorageFreeDays", Categories.CFS_SeaFreight_General, ResString.GetMultilingualString("5280200f-ea7f-4114-aa63-083eddd1cb58", "LCL Storage Free Days"), ResString.GetMultilingualString("9586fd54-7654-4755-aae7-8dfe411a75a6", "The default number of free days for storage for containers and general cargo."), null, RegistryStorageFlags.Branch | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 3, 0, 255);
				});
			}
		}

		public IRegistryItem CFSAirFreightLCLStorageFreeDays
		{
			get
			{
				return GetItem("CFSAirFreightStorageFreeDays", delegate
				{
					return new IntRegistryItem("CFSAirFreightStorageFreeDays", Categories.CFS_AirFreight_General, ResString.GetMultilingualString("5280200f-ea7f-4114-aa63-083eddd1cb58", "LCL Storage Free Days"), ResString.GetMultilingualString("9586fd54-7654-4755-aae7-8dfe411a75a6", "The default number of free days for storage for containers and general cargo."), null, RegistryStorageFlags.Branch | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 1, 0, 255);
				});
			}
		}

		public IRegistryItem CFSSeaFreightDGLCLStorageFreeDays
		{
			get
			{
				return GetItem("CFSSeaFreightDGStorageFreeDays", delegate
				{
					return new IntRegistryItem("CFSSeaFreightDGStorageFreeDays", Categories.CFS_SeaFreight_DangerousGoods, ResString.GetMultilingualString("5280200f-ea7f-4114-aa63-083eddd1cb58", "LCL Storage Free Days"), ResString.GetMultilingualString("eeeb88e4-faae-4a25-9a74-b0778516804d", "The default number of free days for storage for dangerous goods cargo.  If any of the packages on a Shipment have a Dangerous Goods Code (UNDG Code) entered, then the whole shipment is determined to be dangerous goods."), null, RegistryStorageFlags.Branch | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 0, 0, 255);
				});
			}
		}

		public IRegistryItem CFSAirFreightDGLCLStorageFreeDays
		{
			get
			{
				return GetItem("CFSAirFreightDGStorageFreeDays", delegate
				{
					return new IntRegistryItem("CFSAirFreightDGStorageFreeDays", Categories.CFS_AirFreight_DangerousGoods, ResString.GetMultilingualString("5280200f-ea7f-4114-aa63-083eddd1cb58", "LCL Storage Free Days"), ResString.GetMultilingualString("eeeb88e4-faae-4a25-9a74-b0778516804d", "The default number of free days for storage for dangerous goods cargo.  If any of the packages on a Shipment have a Dangerous Goods Code (UNDG Code) entered, then the whole shipment is determined to be dangerous goods."), null, RegistryStorageFlags.Branch | RegistryStorageFlags.Company, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, 0, 0, 255);
				});
			}
		}

		#endregion

		#region AvailableCommences

		public IRegistryItem CFSSeaFreightLCLAvailableCommences
		{
			get
			{
				return GetItem("CFSSeaFreightAvailableCommences", delegate
				{
					return new CodePairRegistryItem("CFSSeaFreightAvailableCommences", Categories.CFS_SeaFreight_General, ResString.GetMultilingualString("88777e02-2e12-4fd5-885f-c9b59ca0145e", "LCL Available Commences"), ResString.GetMultilingualString("4da82ae5-aa76-40f0-a0c2-9a0e9e6d1314", "The default day that cargo is available for general cargo."), OLookUpEditType.AvailableCommences, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, nameof(AvailableCommence.NBD));
				});
			}
		}

		public IRegistryItem CFSAirFreightLCLAvailableCommences
		{
			get
			{
				return GetItem("CFSAirFreightAvailableCommences", delegate
				{
					return new CodePairRegistryItem("CFSAirFreightAvailableCommences", Categories.CFS_AirFreight_General, ResString.GetMultilingualString("88777e02-2e12-4fd5-885f-c9b59ca0145e", "LCL Available Commences"), ResString.GetMultilingualString("4da82ae5-aa76-40f0-a0c2-9a0e9e6d1314", "The default day that cargo is available for general cargo."), OLookUpEditType.AvailableCommences, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, nameof(AvailableCommence.NBD));
				});
			}
		}

		public IRegistryItem CFSSeaFreightDGLCLAvailableCommences
		{
			get
			{
				return GetItem("CFSSeaFreightDGAvailableCommences", delegate
				{
					return new CodePairRegistryItem("CFSSeaFreightDGAvailableCommences", Categories.CFS_SeaFreight_DangerousGoods, ResString.GetMultilingualString("88777e02-2e12-4fd5-885f-c9b59ca0145e", "LCL Available Commences"), ResString.GetMultilingualString("e5b10653-1a4d-46f4-9902-2da71c0c8aa5", "The default day that cargo is available for dangerous goods cargo.  If any of the packages on a Shipment have a Dangerous Goods Code (UNDG Code) entered, then the whole shipment is determined to be dangerous goods."), OLookUpEditType.AvailableCommences, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, nameof(AvailableCommence.IME));
				});
			}
		}

		public IRegistryItem CFSAirFreightDGLCLAvailableCommences
		{
			get
			{
				return GetItem("CFSAirFreightDGAvailableCommences", delegate
				{
					return new CodePairRegistryItem("CFSAirFreightDGAvailableCommences", Categories.CFS_AirFreight_DangerousGoods, ResString.GetMultilingualString("88777e02-2e12-4fd5-885f-c9b59ca0145e", "LCL Available Commences"), ResString.GetMultilingualString("e5b10653-1a4d-46f4-9902-2da71c0c8aa5", "The default day that cargo is available for dangerous goods cargo.  If any of the packages on a Shipment have a Dangerous Goods Code (UNDG Code) entered, then the whole shipment is determined to be dangerous goods."), OLookUpEditType.AvailableCommences, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, nameof(AvailableCommence.IME));
				});
			}
		}

		#endregion

		#region DefaultDepotForBuildHVLV

		internal IRegistryItem DefaultDepotForBuildHVLV
		{
			get
			{
				return GetItem("DefaultDepotForBuildHVLV", delegate
				{
					return new GuidRegistryItem("DefaultDepotForBuildHVLV", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsHidden, Guid.Empty);
				});
			}
		}

		#endregion

		#endregion

		#region Compliance

		BooleanRegistryDataType EnableComplianceRiskDataType
		{
			get => enableComplianceRiskDataType ?? (enableComplianceRiskDataType = (BooleanRegistryDataType)ObjectFactory.Get<IEnableComplianceRiskDataType>());
		}
		BooleanRegistryDataType enableComplianceRiskDataType;

		public BooleanRegistryItem EnableComplianceRisk
		{
			get
			{
				return GetItem("EnableComplianceRisk", delegate
				{
					var registryOption = RegistryOptions.IsHidden;
					if (EnvProxy.Instance.CurrentUser != null && EnvProxy.Instance.CurrentUser.IsSupportUser)
					{
						registryOption = RegistryOptions.IsOnlyForSupport;
					}

					return new BooleanRegistryItem(
						"EnableComplianceRisk",
						Categories.Compliance,
						ResString.GetMultilingualString("1213CEF9-18B2-45F2-AE8B-5248EFCDA4DD", "Enable ComplianceWise"),
						ResString.GetMultilingualString("A4027665-DA71-4983-9721-A0DFC67AFF88", @"This registry should only be updated by the Master Data Product team.
When enabled, ComplianceWise will be available on this system. Customers will also need to enable ComplianceWise on specific modules.
When disabled, ComplianceWise will not be available on this system."),
						RegistryStorageFlags.System,
						registryOption,
						true,
						EnableComplianceRiskDataType);
				});
			}
		}

		#endregion

		#region Others

		public const string RegistryUserUpdateVersionItemName = "RegistryUserUpdateVersion";

		public IntRegistryItem RegistryUserUpdateVersion
		{
			get
			{
				return GetItem(RegistryUserUpdateVersionItemName, delegate
				{
					return new IntRegistryItem(RegistryUserUpdateVersionItemName, Categories.System_Miscellaneous, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.NotLogged, CargoWise.Types.ZInt.Zero);
				});
			}
		}

		#region RemoteDesktop

		#region MicrosoftOffice365

		public StringArrayRegistryItem OpenInMicrosoftOffice365FileTypeList
		{
			get
			{
				return GetItem("OpenInMicrosoftOffice365FileTypeList", delegate
				{
					var item = new StringArrayRegistryItem(
						"OpenInMicrosoftOffice365FileTypeList",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("8FF31F1C-59A5-4DA5-A273-DE3ACBD2ED94", "File Types Open In Microsoft Office 365"),
						ResString.GetMultilingualString("CFBE5DF0-1FC0-48C6-9254-882FC3E26E67", "List of file types (extension name) that is configured to open in Microsoft Office 365."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController
						);
					item.DataType.Validating += OpenInMicrosoftOffice365FileTypeList_Validating;
					return item;
				});
			}
		}

		void OpenInMicrosoftOffice365FileTypeList_Validating(object sender, RegistryDataTypeValidatingEventArgs<string[]> e)
		{
			if (string.IsNullOrEmpty(MicrosoftOffice365ApplicationIdForDragDrop.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)))
			{
				throw new RegistryValidationException(Res.GetString("B3377CF6-B5B9-4FD0-9977-550E28AEBEFF", "In order to open eDocs in Microsoft Office 365, please set [Microsoft Office 365 Application ID For Drag and Drop] and [Microsoft Office 365 Tenant ID For Drag and Drop] first."));
			}
		}

		public StringRegistryItem MicrosoftOffice365ApplicationIdForDragDrop
		{
			get
			{
				return GetItem("MicrosoftOffice365ApplicationIdForDragDrop", () =>
				{
					var result = new StringRegistryItem("MicrosoftOffice365ApplicationIdForDragDrop",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("91CFEFB2-85DF-4BD2-8BC0-8B3C8A746148", "Application ID For Drag and Drop"),
						ResString.GetMultilingualString("311C3113-2450-4F4D-8F56-EBB826DAA396", @"This is the Application ID registered in Microsoft Azure. It should be a unique identifier like 'BA8E1921-C4FD-405F-AC36-5511EA937F45'.

Important: You are required to register your own Application ID under Enterprise applications in Microsoft Azure, then enter the ID in this registry item before drag from your organization's MS office 365."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						string.Empty);
					return result;
				});
			}
		}

		public StringRegistryItem MicrosoftOffice365TenantIdForDragDrop
		{
			get
			{
				return GetItem("MicrosoftOffice365TenantIdForDragDrop", () =>
				{
					var result = new StringRegistryItem("MicrosoftOffice365TenantIdForDragDrop",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("33A696D4-A26E-477F-86FA-5D2876C43467", "Tenant ID For Drag and Drop"),
						ResString.GetMultilingualString("63C4B762-674E-45CE-8C60-6EA5F8DAFC28", "The Tenant ID is used to identify the organization when authenticating the user. If your application is registered as 'Single' tenant, enter the Tenant ID, otherwise left blank and 'common' will be used in authority Uri."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						string.Empty);
					return result;
				});
			}
		}

		public IntRegistryItem MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop
		{
			get
			{
				return GetItem("MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop", () =>
				{
					return new IntRegistryItem("MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("B9FCAB94-C510-46BE-8F37-D08BE29D5ED3", "Attachment Email Fetch Limit"),
						ResString.GetMultilingualString("388CE16B-EF0D-47B6-B260-2424A25953F9", "This setting determines how many emails we scan to locate an attachment uploaded from Outlook. The default is set at 1,000, but if you encounter execution time-outs while dragging and dropping attachments, consider reducing this number."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						defaultValue: 1000,
						minValue: 100,
						maxValue: 1000);
				});
			}
		}

		#endregion MicrosoftOffice365

		internal BooleanRegistryItem RemoteAppEnableDragDropLite
		{
			get
			{
				return GetItem("RemoteAppEnableDragDropLite", delegate
				{
					return new BooleanRegistryItem(
						"RemoteAppEnableDragDropLite",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("D7E9469D-B213-4E01-87FF-A49F585E8281", "Enable Lite Message on Drag-drop Where Possible"),
						ResString.GetMultilingualString("938FA2A2-7D4C-4FA3-8B60-671F7E3BA9B2", "If drive mapping is enabled and the file location dragged from client side is accessible from server, send the file location rather than the file content over the remote channel when this option is enabled."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						false
						);
				});
			}
		}

		internal BooleanRegistryItem RemoteAppAlwaysShowSelectionFormOnFileDrop
		{
			get
			{
				return GetItem("RemoteAppAlwaysShowSelectionFormOnFileDrop", delegate
				{
					return new BooleanRegistryItem(
						"RemoteAppAlwaysShowSelectionFormOnFileDrop",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("EFC4AEB4-55F2-4DBE-88FF-2EBB7034B54B", "Always Show Selection Form on Drag-drop"),
						ResString.GetMultilingualString("88EE95EB-CB68-41CA-B36E-5B295CEB322C", "Show Selection Form on every drag-drop. For testing and troubleshooting purpose only."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						false
						);
				});
			}
		}

		internal BooleanRegistryItem RemoteAppSendTestingMessageOnInitialize
		{
			get
			{
				return GetItem("RemoteAppSendTestingMessageOnInitialize", delegate
				{
					return new BooleanRegistryItem(
						"RemoteAppSendTestingMessageOnInitialize",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("79D94B29-5986-4A63-A470-7C346C7482AF", "Send Testing Message on Initialize"),
						ResString.GetMultilingualString("18AB3EED-553D-48FF-831D-2711EE48A475", "Send OPENFILESUPPORTED message on Initialize to confirm channel is established and working."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						true
						);
				});
			}
		}

		internal IntRegistryItem RemoteAppShowFormViaMenuDelayMilliseconds
		{
			get
			{
				return GetItem("RemoteAppShowFormViaMenuDelayMilliseconds", () =>
				{
					return new IntRegistryItem("RemoteAppShowFormViaMenuDelayMilliseconds",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("568AEBAE-11CC-44B0-931B-9EBA5BB3DC2A", "Show Screen From Menu Delay"),
						ResString.GetMultilingualString("0F5F72D7-FB36-4974-9EFC-CAD15544D90A", "Delay in milliseconds to insert between clicking a popup menu and activating a new screen when running as a RemoteApp. Set this to the round-trip time between the local desktop and the remote terminal server to workaround screen flicker problems in Windows 7 and earlier."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch, 600);
				});
			}
		}

		internal IntRegistryItem RemoteAppWaitingForReconnectionTimeoutInSeconds
		{
			get
			{
				return GetItem("RemoteAppWaitingForReconnectionTimeoutInSeconds", () =>
				{
					return new IntRegistryItem("RemoteAppWaitingForReconnectionTimeoutInSeconds",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("BC354C5F-D4CE-424F-B408-7787993506FB", "Reconnection timeout in seconds"),
						ResString.GetMultilingualString("48CEC2FC-0FB3-4846-894A-18DC23819B94", "Timeout in seconds when waiting for reconnection response from client."),
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, 30)
					{
						DataType = new IntRegistryDataType(5, 300)
					};
				});
			}
		}

		public IntRegistryItem RemoteAppCheckDriveMappingTimeoutInSeconds
			=> GetItem(
				"RemoteAppCheckDriveMappingTimeoutInSeconds",
				() => new IntRegistryItem(
					"RemoteAppCheckDriveMappingTimeoutInSeconds",
					Categories.System_RemoteApp,
					ResString.GetMultilingualString("dc6435f8-60c5-4aa1-8354-f4f7a97a58ad", "Remote Client Checking Drive Mapping Timeout"),
					ResString.GetMultilingualString("d5b65931-a3a1-4534-9f67-41165a56a29e", "Timeout in seconds for remote client waiting for drive mapping result."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController,
					defaultValue: 3,
					minValue: 1,
					maxValue: 20));

		internal BooleanRegistryItem RemoteAppAllowEDocAccessWithoutConnector
		{
			get
			{
				return GetItem("RemoteAppAllowEDocAccessWithoutConnector", delegate
				{
					return new BooleanRegistryItem(
						"RemoteAppAllowEDocAccessWithoutConnector",
						Categories.System_RemoteApp,
						null,
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						false
						);
				});
			}
		}

		internal CodePairRegistryItem RemoteAppAllowEDocAccessWithoutConnectorMode
		{
			get
			{
				return GetItem("RemoteAppAllowEDocAccessWithoutConnectorMode", delegate
				{
					var optionList = new RemoteConnectingModes();

					return new CodePairRegistryItem(
						"RemoteAppAllowEDocAccessWithoutConnectorMode",
						Categories.System_RemoteApp,
						ResString.GetMultilingualString("B9E3C832-ACF4-4BCB-B945-C8F40591F11D", "Options for eDocs Remote Access Methods"),
						ResString.GetMultilingualString("924002D6-EC6F-448D-881C-4365793D60FC", "Choose from different access methods for eDocs. Warning: when server file access is selected, eDocs will open directly on the server if there is a program installed on the server that is associated with the file type. This is a potential security threat for systems with remote access.\r\n" +
																								"\r\nConnector Access Only: eDocs can only be accessed if remote plug in is installed and running properly.\r\n" +
																								"\r\nServer File Access Only: eDocs will always be accessed from server regardless of remote plug in is installed or not. Drag-drop is not supported when running in this mode.\r\n" +
																								"\r\nConnector or Server File Access: eDocs will be accessed either in Connector Access Only mode or Server File Access Only mode depending on if remote plug in is installed or not. There will be a 30 seconds delay when starting CW1 if remote plug in is not installed."),
						new CodeDescriptionPairListProvider(() => optionList),
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController,
#if DEBUG
						RemoteConnectingModes.ConnectorOrServer
#else
						optionList.DefaultCode
#endif
						);
				});
			}
		}

		#endregion RemoteDesktop

#if DEBUG
		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp")]
		public StringRegistryItem EnterpriseSDLCmdWebAddress
		{
			get
			{
				return GetItem("EnterpriseSDLCmdWebAddress", () =>
				{
					return new StringRegistryItem("EnterpriseSDLCmdWebAddress", Categories.PhysicalServer, (NoResString)"Application SDL Cmd Web Address", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, "FOR GEO TEAM ONLY");
				});
			}
		}
#endif

		public IntRegistryItem SystemUpgradeWarningPeriod
		{
			get
			{
				return GetItem("SystemUpgradeWarningPeriod", () =>
				{
					var dataType = new IntRegistryDataType(0, 1440);
					dataType.Validating += SystemUpgradeWarningPeriodValidating;
					return new IntRegistryItem(
						new RegistryItemImpl("SystemUpgradeWarningPeriod",
							Categories.System_Upgrade, ResString.GetMultilingualString("455E8B7D-0CA4-4FBE-94DE-4D68248A5AC4", "System Upgrade Warning Period"),
							ResString.GetMultilingualString("83D313B7-7813-4550-8D4B-A5300BAFE55D", "System upgrade warning period in minutes. This will cause the upgrade to pause for the given number of minutes before disconnecting users. During the period users will receive a warning that an upgrade is about to occur. Set to 0 to give no warning to users. Maximum is 1440 minutes (equals to 24 hours). Default value is 0. To ensure the function works correctly, the value cannot be less than current system heartbeat pulse duration: {0}.",
								HeartbeatpulseDuration),
							dataType,
							new NumericRegistryEditorInfo(0),
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							0));
				});
			}
		}
#if DEBUG
		internal
#endif
		double HeartbeatpulseDuration => HeartbeatDurationSeconds.Value / 60 * 0.4;

		void SystemUpgradeWarningPeriodValidating(object sender, RegistryDataTypeValidatingEventArgs<int> e)
		{
			if (e.ProposedValue > 0 && e.ProposedValue < HeartbeatpulseDuration)
			{
				throw new RegistryValidationException(Res.GetString("3ea366c5-f54c-45e5-8ed8-70e38fd9a680", "In order to keep the function working, System upgrade warning period cannot be less than current system heartbeat pulse duration: {0}.", HeartbeatpulseDuration));
			}
		}

		#endregion

		public CodePairRegistryItem EnglishSpelling
		{
			get
			{
				return GetItem("EnglishSpelling", () =>
				{
					return new EnglishSpellingRegistryItem("EnglishSpelling", Categories.System_Language, ResString.GetMultilingualString("05bf8c72-49c1-485e-800e-8f3602d3badb", "English Spelling"), ResString.GetMultilingualString("1934abe9-852a-4f75-b251-d1505e274bbb", "Default English Spelling preference for the company"), RegistryStorageFlags.Company);
				});
			}
		}

		#region WiseCloud

		public StringRegistryItem ClientIPAddressRestriction
		{
			get
			{
				return GetItem("ClientIPAddressRestriction", () =>
				{
					return new StringRegistryItem("ClientIPAddressRestriction", Categories.System_WiseCloud, ResString.GetMultilingualString("d9bdff06-da54-41a6-867b-190632cfa262", "Client IP Address Restriction"), ResString.GetMultilingualString("82a5b370-0564-4342-a32f-077d272d68a8", "Restrict access to certain client IP addresses. Enter one or more public IP addresses and/or IP address ranges separated by commas. An IP address range can be specified using network/mask CIDR shorthand."), new IPAddressRangesDataType(), RegistryStorageFlags.System, RegistryOptions.Default);
				});
			}
		}

		public MultilingualStringRegistryItem ClientIPAddressRestrictedMessage
		{
			get
			{
				return GetItem("ClientIPAddressRestrictedMessage", () =>
				{
					return new MultilingualStringRegistryItem("ClientIPAddressRestrictedMessage", Categories.System_WiseCloud, ResString.GetMultilingualString("ad64ab6d-3f9a-4894-b5a9-eaca342b01a1", "Client IP Address Restricted Message"), ResString.GetMultilingualString("00799a9b-fa6b-41d6-9e76-0525e0557aef", "Error message to display to users that attempt to access your system from outside the specified IP addresses defined by the Client IP Address Restriction registry item."), RegistryStorageFlags.System, RegistryOptions.Default, ResString.GetMultilingualString("95cea4b7-10fd-451b-8ee2-bcd19130bf8b", "You may not access the system from a computer outside of the authorized network."));
				});
			}
		}

		#endregion

		#region ASSESS

		public StringRegistryItem WiseTechAcademyLmsApiBaseAddress
		{
			get
			{
				return GetItem("WiseTechAcademyLmsApiBaseAddress", () =>
					new StringRegistryItem(
						name: "WiseTechAcademyLmsApiBaseAddress",
						category: Categories.System_ASSESS,
						caption: (NoResString)"LMS API Base Address",
						hint: (NoResString)"The URL pointing to the WiseTech Academy instance this CargoWise system integrates with.",
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: WTG.WiseTechAcademy.WiseTechAcademyApiClient.DefaultBaseAddress)
				);
			}
		}

		public StringRegistryItem AssessApiBaseAddress
		{
			get
			{
				return GetItem("AssessApiBaseAddress", () =>
					new StringRegistryItem(
						name: "AssessApiBaseAddress",
						category: Categories.System_ASSESS,
						caption: (NoResString)"ASSESS Service URL",
						hint: (NoResString)"The ASSESS Service base URL.",
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValue: "https://devtools-assess.wtg.zone/")
				);
			}
		}

		#endregion

		#region Testing Sub

		public StringRegistryItem FaxDestinationOverride
		{
			get
			{
				return GetItem("FaxDestinationOverride", delegate
				{
					return new StringRegistryItem(
					"FaxDestinationOverride",
					Categories.System_Testing,
					ResString.GetMultilingualString("748CD270-1F32-4d36-B656-A799FDCBFC4D", "Fax Destination Override"),
					ResString.GetMultilingualString("4C74D94D-13EE-4f53-AF67-16BE2AC7BECE", "Enter a fax number here to force all outbound faxes to be sent to this number. This feature is useful for test systems where administrators want to prevent test faxes being sent out to real customers."),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue);
				});
			}
		}

		public StringRegistryItem HostedNotificationsEmailOverride
		{
			get
			{
				return GetItem("HostedNotificationsEmailOverride", delegate
				{
					return new StringRegistryItem(
					"HostedNotificationsEmailOverride",
					Categories.System_Testing,
					(NoResString)"Hosted Notifications Email Override",
					(NoResString)"Infrastructure and other hosting related notifications for the hosted databases will be sent to this email address. Notification settings will be overridden.",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden,
					"Hosting.Notifications@wisetechglobal.com"
					);
				});
			}
		}

		public StringRegistryItem EmailDestinationOverride
		{
			get
			{
				return GetItem("EmailDestinationOverride", delegate
				{
					var result = new StringRegistryItem(
						"EmailDestinationOverride",
						Categories.System_Testing,
						ResString.GetMultilingualString("A7110F43-6D82-4dec-B7D2-8B3A4AB6AD12", "Email Destination Override"),
						ResString.GetMultilingualString("ECB66EA4-B46C-4950-9405-55836C6A9174", "Enter an email address here to force all emails to be sent to this email address (Fax emails will still be sent out to CargoWise fax server)."),
						new EmailDestinationOverrideDataType(),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);

					bool isMandatory = false;
					if (!EnvProxy.Instance.IsProductionSystem)
					{
						isMandatory = true;
					}
#if DEBUG
					if (!IgnoreIsDebugCheckForTest)
					{
						isMandatory = true;
					}
#endif
					if (isMandatory)
					{
						result.Options |= RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue;
					}
					return result;
				});
			}
		}

#if DEBUG
		public bool IgnoreIsDebugCheckForTest;
#endif

		public BooleanRegistryItem SystemEmailDestinationOverride
		{
			get
			{
				return GetItem("SystemEmailDestinationOverride", delegate
				{
					return new BooleanRegistryItem(
						"SystemEmailDestinationOverride",
						Categories.System_Testing,
						ResString.GetMultilingualString("8772d777-b820-4528-9bab-13a5cd335cf4", "System Email Destination Override"),
						ResString.GetMultilingualString("354da2e4-f509-4900-831c-85e8f0dcaf75", "Set this option to 'Yes' to force all system generated emails to be sent to 'Email Destination Override'."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem EHubTesting
		{
			get
			{
				return GetItem("EHubTesting", delegate
				{
					return new BooleanRegistryItem(
					"EHubTesting",
					Categories.System_Testing,
					ResString.GetMultilingualString("54387399-de29-4a04-b58c-2019a7de7214", "eHub Testing"),
					ResString.GetMultilingualString("369542f3-5b52-40c2-a21d-3f01f23ff3e6", "Set this option to 'Yes' to enable eHub Testing."),
						RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForCargoWise,
						false);
				});
			}
		}

		public BooleanRegistryItem ObtainDynamicSTLCollectorDefinitionsFromTheCode
		{
			get
			{
				return GetItem("ObtainDynamicSTLCollectorDefinitionsFromTheCode", delegate
				{
					return new BooleanRegistryItem(
						"ObtainDynamicSTLCollectorDefinitionsFromTheCode",
						Categories.System_Testing,
						(NoResString)"Obtain Dynamic STL Collector Definitions From The Code",
						(NoResString)"Set this option to 'Yes' on non-production systems to ask the STL service task to obtain the Dynamic STL collector scripts from the code rather than from the reference database.  This should be used when performing UAT on a collector that a developer is in the process of adding to the system.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForCargoWise,
						GetDefaultValueOfObtainDynamicSTLCollectorDefinitionsFromTheCode());
				});
			}
		}

		bool GetDefaultValueOfObtainDynamicSTLCollectorDefinitionsFromTheCode()
		{
			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			return productRegistration.Key.DatabaseType == DatabaseTypes.Codes.Test && productRegistration.IsWiseTechGlobalInternalSystem();
		}
#if DEBUG
		public BooleanRegistryItem RedirectReferenceDataForTests
		{
			get
			{
				return GetItem("RedirectReferenceDataForTests", delegate
				{
					return new BooleanRegistryItem(
					"RedirectReferenceDataForTests",
					Categories.System_Testing,
					(NoResString)"Redirect ReferenceData for tests",
					(NoResString)"Set this option to 'Yes' to enable the redirection of reference data queries to a test table in the Odyssey database for unit testing purposes",
						RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForCargoWise,
						true);
				});
			}
		}
#endif

		#endregion

		#region SelectNDRPath
		public SelectNDRPathRegistryItem SelectNDRPath
		{
			get
			{
				var item = GetItem("SelectNDRPath", delegate
				{
					return new SelectNDRPathRegistryItem(
						"SelectNDRPath",
						Categories.PhysicalServer_SMTP,
						ResString.GetMultilingualString("B72707BF-A132-4BFB-B7FD-3D49F65E312C", "Select NDR Path"),
						ResString.GetMultilingualString("77B8B0FC-C23C-4605-903B-AD15D57AE8B8", "This setting will set the Return-Path for NDR (Non-Delivery Receipt) notifications.\r\n\r\nNote:  This setting can only be configured when the Allow Emails To Be Sent From User's Address Registry setting is enabled."),
						new CodeDescriptionPairListProvider(() => GetSelectNDRPathList()),
						RegistryStorageFlags.System,
						RegistryOptions.IsReadOnly,
						GetSelectNDRPathList().DefaultCode);
				});
				item.Options = DataRegistry.Instance.AllowEmailsToBeSentFromUsersAddress ? RegistryOptions.Default : RegistryOptions.IsReadOnly;
				return item;
			}
		}

		internal CodeDescriptionPairList GetSelectNDRPathList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(Constants.SelectNDRPath.Codes.MB, Constants.SelectNDRPath.Descriptions.MB);
			result.AddPair(Constants.SelectNDRPath.Codes.CP, Constants.SelectNDRPath.Descriptions.CP);
			result.AddPair(Constants.SelectNDRPath.Codes.SU, Constants.SelectNDRPath.Descriptions.SU);
			result.DefaultCode = Constants.SelectNDRPath.Codes.MB;

			return result;
		}
		#endregion

		#region SCIM

		public StringRegistryItem ScimAudienceId
		{
			get
			{
				return GetItem("ScimAudienceId",
					() => new StringRegistryItem(
						"ScimAudienceId",
						Categories.System_SCIM,
						(NoResString)"Audience Id", // NoResString
						(NoResString)@"Enter the Azure audience (or Application ID) used for token authentication.", // NoResString
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"8adf8e6e-67b2-4cf2-a259-e3dc5476c621"))
				;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "NoResString")]
		public StringRegistryItem ScimKnownEndpointPath
		{
			get
			{
				return GetItem("ScimKnownEndpointPath",
					() => new StringRegistryItem(
						"ScimKnownEndpointPath",
						Categories.System_SCIM,
						(NoResString)"Known Endpoint Path",
						(NoResString)@"Enter the Known Endpoint Path for the SCIM service. The default option points to the Azure AD well-known endpoint.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"/.well-known/openid-configuration"))
				;
			}
		}

		public StringRegistryItem ScimIssuer
		{
			get
			{
				return GetItem("ScimIssuer",
					() => new StringRegistryItem(
						"ScimIssuer",
						Categories.System_SCIM,
						ResString.GetMultilingualString("378731F6-6833-4652-8038-0674C8B16C1F", "Issuer"),
						ResString.GetMultilingualString("4B637CC6-5839-42FE-B5C1-5B86BDEA6F61", @"Used to identify the issuer, or ""authorization server"" that constructs and returns the token."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty))
				;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "NoResString")]
		public StringRegistryItem ScimLoggingKafkaBrokers
		{
			get
			{
				return GetItem("ScimLoggingKafkaBrokers",
					() => new StringRegistryItem(
						"ScimLoggingKafkaBrokers",
						Categories.System_SCIM_Logging,
						(NoResString)"Kafka Target Brokers",
						(NoResString)@"Specifies the Kafka brokers as NLog Kafka target config 'brokers' attribute for SCIM.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"106-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,107-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,108-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,109-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,110-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044"))
				;
			}
		}

		public StringRegistryItem ScimLoggingKafkaTopic
		{
			get
			{
				return GetItem("ScimLoggingKafkaTopic",
					() => new StringRegistryItem(
							new RegistryItemImplWithDynamicDefaultValue(
							"ScimLoggingKafkaTopic",
							Categories.System_SCIM_Logging,
							(NoResString)"Kafka Target Topic", // NoResString
							(NoResString)@"Specifies the Kafka topic as NLog Kafka target config 'topic' attribute for SCIM.", // NoResString
							RegistryDataTypes.StringType,
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							ScimLoggingKafkaTopicDefaultValueGetter))
				);
			}
		}

		object ScimLoggingKafkaTopicDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return EnvProxy.Instance.IsProductionSystem
							? "topic-au2-prod-scim-service-logs-prod" // Constant string
							: "topic-au1-test-scim-service-logs-test"; // Constant string
		}

		public StringRegistryItem ScimLoggingKafkaTopicUsername
		{
			get
			{
				return GetItem("ScimLoggingKafkaTopicUsername",
					() => new StringRegistryItem(
							new RegistryItemImplWithDynamicDefaultValue(
								"ScimLoggingKafkaTopicUsername",
								Categories.System_SCIM_Logging,
								(NoResString)"Kafka Target Topic Username", // NoResString
								(NoResString)@"Specifies the Kafka topic username for SCIM.", // NoResString
									RegistryDataTypes.StringType,
								RegistryStorageFlags.System,
								RegistryOptions.IsOnlyForSupport,
								ScimLoggingKafkaTopicUsernameDefaultValueGetter))
				);
			}
		}

		object ScimLoggingKafkaTopicUsernameDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return EnvProxy.Instance.IsProductionSystem
							? "topic-au2-prod-scim-service-logs-prod" // Constant string
							: "topic-au1-test-scim-service-logs-test"; // Constant string
		}

		public StringRegistryItem ScimLoggingKafkaTopicPassword
		{
			get
			{
				return GetItem("ScimLoggingKafkaTopicPassword",
					() => new StringRegistryItem(
						"ScimLoggingKafkaTopicPassword",
						Categories.System_SCIM_Logging,
						(NoResString)"Kafka Target Topic Password", // NoResString
						(NoResString)@"Specifies the Kafka topic password for SCIM.", // NoResString
						new StringRegistryDataType(true),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						string.Empty))
				;
			}
		}

		public BooleanRegistryItem ScimCanLogin
		{
			get
			{
				return GetItem("ScimCanLogin",
					() => new BooleanRegistryItem(
							new BooleanRegistryItem(
								"ScimCanLogin",
								Categories.System_SCIM,
								ResString.GetMultilingualString("D0FB26BE-88BF-4394-9F96-0B20E7BD05E5", "Can Login Default"),
								ResString.GetMultilingualString("26E69C2D-AEC6-4498-8D44-313FE0D86F55", "Staff Can Login default value upon provision."),
								RegistryStorageFlags.System,
								RegistryOptions.Default,
								true))
				);
			}
		}

		public StringRegistryItem ScimCodeGenerationCharacters
		{
			get
			{
				return GetItem("ScimCodeGenerationCharacters", delegate
				{
					var item = new StringRegistryItem(
						"ScimCodeGenerationCharacters",
						Categories.System_SCIM,
						ResString.GetMultilingualString("8B98BB64-BF6A-41A0-B7B0-171FDB858E83", "Additional characters for code generation"),
						ResString.GetMultilingualString("EEACC31B-0C02-4AB0-80BD-FAA1A1414321", "This registry determines a set of additional characters that are used in conjunction with the letters for the Staff codes during SCIM imports. Letters and numbers only provide 46656 combinations. Default full set of characters increases this number to 287496 combinations. Changing this registry might take up to 1 hour to be applied, please wait before starting provisioning."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"0123456789!\"#$%&()*+,-./:;<=>@[\\]^_`{|}~");

					(item.DataType as StringRegistryDataType).Validating += ScimCodeGenerationCharacters_Validating;

					return item;
				});
			}
		}

		void ScimCodeGenerationCharacters_Validating(object sender, RegistryDataTypeValidatingEventArgs<string> e)
		{
			if (Regex.IsMatch(e.ProposedValue, "[a-zA-Z]"))
			{
				throw new RegistryValidationException(Res.GetString("0F669B30-CC92-4725-B27C-23073B5EA458", "Letters are used by default and are not allowed in this registry."));
			}

			foreach (var c in e.ProposedValue)
			{
				if (c == '\'' || c == '?' || c < 33 || c > 126)
				{
					throw new RegistryValidationException(Res.GetString("83F830BD-0359-452F-9058-3634E123CD9B", "Eligible characters are for codes between 33 and 126 except letters, single quote (') and question mark (?): 0123456789!\"#$%&()*+,-./:;<=>@[\\]^_`{|}~"));
				}
			}

			var charGroups = e.ProposedValue.GroupBy(c => c);
			foreach (var group in charGroups)
			{
				if (group.Count() > 1)
				{
					throw new RegistryValidationException(Res.GetString("9578005F-1D41-43BB-9BC6-AE2ACE9C3E32", "Duplicate character: {0}", group.Key));
				}
			}
		}
		#endregion
	}
}
