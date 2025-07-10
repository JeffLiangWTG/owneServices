using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Licensing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MailManager;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductRegistration.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[CodeProperty(LicenceDatabase.Schema.LD_ServerCode), DescriptionProperty(LicenceDatabase.Schema.AddressAsString)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class LicenceDatabase : AutoLicenceDatabase, IBilledDatabase
	{
		#region Schema

		public new abstract class Schema : AutoLicenceDatabase.Schema
		{
			public const string AddressAsString = "AddressAsString";
			public const string CompanyCode = "CompanyCode";
			public const string CompanyName = "CompanyName";
			public const string ConnectionDetails = "ConnectionDetails";
			public const string ReleaseRingDescription = "ReleaseRingDescription";
			public const string DatabaseId = "DatabaseId";
		}

		#endregion

		public LicenceDatabase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LD_LicenceType = DatabaseTypes.Codes.Production;
			LD_DBServerSecurityMode = DatabaseSecurityModePairList.Codes.Locked;
			LD_LicenceExpiry = ZDateTime.Today.AddDays(Licences.DefaultLicenceGracePeriodInDays);
			LD_Product = ProductTypes.Codes.CargoWiseOne;
			LD_Billable = DatabaseBillableFlagList.Codes.YesCustomer;
		}

		#endregion

		#region Loading

		public override void OnLoaded()
		{
			base.OnLoaded();

			using (SuspendSettingHasChanges())
			{
				var billingDb = ParentDatabase;
				if (billingDb != null)
				{
					productionDatabaseServerCode = billingDb.LD_ServerCode;
				}
			}
		}

		public static LicenceDatabase LoadFromEnterpriseAndServerCode(BusinessObjectFactory factory, string enterpriseCode, string serverCode)
		{
			var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
			query.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);
			var entQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
			entQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);
			query.AddSubQuery(entQuery, JoinCondition.And);
			return factory.LoadTop1<LicenceDatabase>(query);
		}

		public static LicenceDatabase Load(BusinessObjectFactory factory, string enterpriseCode, string companyCode, string serverCode)
		{
			ZDBOnlySubQuery enterpriseQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceCompanySchema.LC_LE);
			enterpriseQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);

			ZDBOnlySubQuery headerQuery = new ZDBOnlySubQuery(typeof(LicenceHeader), LicenceHeaderSchema.LA_LD);
			ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), LicenceCompanySchema.PK);

			if (!string.IsNullOrEmpty(companyCode))
			{
				companyQuery.AddToFilter(LicenceCompanySchema.LC_CompanyCode, companyCode);
			}

			companyQuery.AddSubQuery(enterpriseQuery, JoinCondition.And);
			headerQuery.AddSubQuery(LicenceHeaderSchema.LA_LC, companyQuery, JoinCondition.And);

			ZDBOnlyQuery databaseQuery = new ZDBOnlyQuery(typeof(LicenceDatabase));
			databaseQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, serverCode);
			databaseQuery.AddSubQuery(headerQuery, JoinCondition.And);

			return factory.LoadTop1<LicenceDatabase>(databaseQuery);
		}

		#endregion

		#region Properties

		#region DatabaseId

		public ZString DatabaseId
		{
			get { return LD_DatabaseNumber != 0 ? Base27Encoding.Encode(LD_DatabaseNumber) : ""; }
		}

		public ZPropertyInfo DatabaseIdInfo
		{
			get { return GetZPropertyInfo(Schema.DatabaseId); }
		}

		#endregion

		#region LD_ReleaseRing

		[List("Lookups.ReleaseRings")]
		public override ZString LD_ReleaseRing
		{
			get { return base.LD_ReleaseRing; }
			set { base.LD_ReleaseRing = value; }
		}

		[MaxLength(255)]
		public ZString ReleaseRingDescription
		{
			get { return Lookups.ReleaseRings.GetDescriptionFromCode(LD_ReleaseRing); }
		}

		public ZPropertyInfo ReleaseRingDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ReleaseRingDescription); }
		}

		#endregion

		#region LD_LE

		[RelatedBusinessObject("LicEnterprise")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid LD_LE
		{
			get { return base.LD_LE; }
			set { base.LD_LE = value; }
		}

		#endregion

		#region LD_LicenceExpiry

		public bool LD_LicenceExpiry_ReadOnly
		{
			get { return true; }
		}

		public bool LD_ManualLicenceExpiry_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region LD_DBServerSecurityMode

		[List("Lookups.DatabaseSecurityModesList")]
		public override ZString LD_DBServerSecurityMode
		{
			get { return base.LD_DBServerSecurityMode; }
			set { base.LD_DBServerSecurityMode = value; }
		}

		#endregion

		#region LD_OA_SoftwareInstallAddressDetails

		[List("Lookups.LicEnterpriseAddresses")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid LD_OA_SoftwareInstallAddressDetails
		{
			get { return base.LD_OA_SoftwareInstallAddressDetails; }
			set { base.LD_OA_SoftwareInstallAddressDetails = value; }
		}

		#endregion

		#region LD_AvailableUpgradeMethod

		[List("Lookups.UpgradeMethodsList")]
		public override ZString LD_AvailableUpgradeMethod
		{
			get { return base.LD_AvailableUpgradeMethod; }
			set { base.LD_AvailableUpgradeMethod = value; }
		}

		#endregion

		#region LD_OC_LicenseeAdminContact

		[List("Lookups.LicEnterpriseContacts")]
		public override ZGuid LD_OC_LicenseeAdminContact
		{
			get { return base.LD_OC_LicenseeAdminContact; }
			set { base.LD_OC_LicenseeAdminContact = value; }
		}

		#endregion

		#region LD_LicenceType

		[List("Lookups.DatabaseTypesList")]
		public override ZString LD_LicenceType
		{
			get { return base.LD_LicenceType; }
			set
			{
				if (LD_LicenceType != value)
				{
					base.LD_LicenceType = value;
					if (value == DatabaseTypes.Codes.Production && !LD_LD_ParentDatabase.IsEmpty)
					{
						LD_LD_ParentDatabase = ZGuid.Empty;
					}
					else if (value != DatabaseTypes.Codes.Production && LD_LD_ParentDatabase.IsEmpty)
					{
						var productionDbs = LicHeadersForAllCompanies.Cast<LicenceHeader>()
							.Where(x => x.LA_IsActive)
							.SelectMany(x => x.Company.LicDatabases.Cast<LicenceDatabase>())
							.Where(x => x.LD_LicenceType == DatabaseTypes.Codes.Production && x.LD_IsActive)
							.Distinct()
							.ToList();

						if (productionDbs.Count == 1)
						{
							LD_LD_ParentDatabase = productionDbs[0].PK;
							productionDatabaseServerCode = productionDbs[0].LD_ServerCode;
							ProductionDatabaseServerCodeInfo.RefreshBinding();
						}
					}

					if (value == DatabaseTypes.Codes.WisecloudTrial && LD_ManualLicenceExpiry.IsEmpty)
					{
						LD_ManualLicenceExpiry = ZDate.Today.AddDays(60);
					}
				}
			}
		}

		public ZString LD_LicenceTypeDescription => Lookups.DatabaseTypesList.GetDescriptionFromCode(LD_LicenceType);

		#endregion

		#region LD_LastHeartbeat

		public bool LD_LastHeartbeat_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region LD_DatabaseFilePathDetail

		public bool LD_DatabaseFilePathDetail_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region LD_NoOfActivePrintQueues

		public bool LD_NoOfActivePrintQueues_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region LD_SQLEdition

		[List("Lookups.SqlServerEditions")]
		[ActionField(ReadOnly = true)]
		public override ZString LD_SQLEdition
		{
			get { return base.LD_SQLEdition; }
			set { base.LD_SQLEdition = value; }
		}

		public bool LD_SQLEdition_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region LD_SQLVersion

		public bool LD_SQLVersion_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region LD_SQLVerString

		public bool LD_SQLVerString_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region LD_SQLVerStringForDisplay

		public ZString LD_SQLVerStringForDisplay
		{
			get { return ZArchitecture.Core.Utilities.FixNewLinesForEnvironment(LD_SQLVerString); }
		}

		public ZPropertyInfo LD_SQLVerStringForDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(LD_SQLVerStringForDisplay)); }
		}

		#endregion

		#region LD_OSName

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString LD_OSName
		{
			get { return base.LD_OSName; }
			set { base.LD_OSName = value; }
		}

		#endregion

		#region LD_OSVersion

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString LD_OSVersion
		{
			get { return base.LD_OSVersion; }
			set { base.LD_OSVersion = value; }
		}

		#endregion

		#region LD_SystemManufacturer

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString LD_SystemManufacturer
		{
			get { return base.LD_SystemManufacturer; }
			set { base.LD_SystemManufacturer = value; }
		}

		#endregion

		#region LD_BIOSDate

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_BIOSDate
		{
			get { return base.LD_BIOSDate; }
			set { base.LD_BIOSDate = value; }
		}

		#endregion

		#region LD_TotalPhysicalMemoryMB

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZInt LD_TotalPhysicalMemoryMB
		{
			get { return base.LD_TotalPhysicalMemoryMB; }
			set { base.LD_TotalPhysicalMemoryMB = value; }
		}

		#endregion

		#region LD_NoOfProcessorCores

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZInt LD_NoOfProcessorCores
		{
			get { return base.LD_NoOfProcessorCores; }
			set { base.LD_NoOfProcessorCores = value; }
		}

		#endregion

		#region LD_ProcessorType

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString LD_ProcessorType
		{
			get { return base.LD_ProcessorType; }
			set { base.LD_ProcessorType = value; }
		}

		#endregion

		#region LD_ProcessorSpeedMHz

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDecimal LD_ProcessorSpeedMHz
		{
			get { return base.LD_ProcessorSpeedMHz; }
			set { base.LD_ProcessorSpeedMHz = value; }
		}

		#endregion

		#region LD_VirtualMachineDetected

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZBool LD_VirtualMachineDetected
		{
			get { return base.LD_VirtualMachineDetected; }
			set { base.LD_VirtualMachineDetected = value; }
		}

		#endregion

		#region LD_OC_ContractInstallerOrInternalTechContact

		[List("Lookups.LicEnterpriseContacts")]
		public override ZGuid LD_OC_ContractInstallerOrInternalTechContact
		{
			get
			{
				return base.LD_OC_ContractInstallerOrInternalTechContact;
			}
			set
			{
				base.LD_OC_ContractInstallerOrInternalTechContact = value;
			}
		}

		#endregion

		#region LD_Billable

		[List("Lookups.BillableFlagList")]
		public override ZString LD_Billable { get => base.LD_Billable; set => base.LD_Billable = value; }

		[MaxLength(11)]
		[List("Lookups.BillableFlagList")]
		public ZString LD_BillableDescription
		{
			get { return Lookups.BillableFlagList.GetDescriptionFromCode(LD_Billable); }
			set
			{
				LD_Billable = Lookups.BillableFlagList.GetCodeFromDescription(value) ?? DatabaseBillableFlagList.Codes.InvalidCode;
			}
		}

		public ZWrappedPropertyInfo LD_BillableDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.LD_Billable, x => LD_BillableInfo); }
		}

		public bool IsBillable => IsBillableCode(LD_Billable);
		public static bool IsBillableCode(string billable) => billable == DatabaseBillableFlagList.Codes.YesCustomer || billable == DatabaseBillableFlagList.Codes.YesPartner;

		#endregion

		#region Current Version

		[RelatedBusinessObject("CurrentVersion")]
		[List("Lookups.CurrentVersions")]
		[ActionField(ReadOnly = true)]
		public override ZGuid LD_HL_CurrentRunningVersion
		{
			get { return base.LD_HL_CurrentRunningVersion; }
			set
			{
				if (LD_HL_CurrentRunningVersion != value)
				{
					ReleaseBuild previousBuild = CurrentVersion;
					base.LD_HL_CurrentRunningVersion = value;
					UpdateAvailableUpgradeMethod(previousBuild, CurrentVersion);
				}
			}
		}

		[ActionFieldFollow(false)]
		public virtual ReleaseBuild CurrentVersion
		{
			get { return Factory.Load<ReleaseBuild>(LD_HL_CurrentRunningVersion); }
		}

		void UpdateAvailableUpgradeMethod(ReleaseBuild previousBuild, ReleaseBuild newBuild)
		{
			if (LD_AvailableUpgradeMethod != UpgradeMethods.Codes.Blocked)
			{
				HttpDownload httpVersion = new HttpDownload();
				if (!httpVersion.IsSupportingVersion(previousBuild) &&
					httpVersion.IsSupportingVersion(newBuild))
				{
					this.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
				}
			}
		}

		[ActionField(ReadOnly = true)]
		public ZDateTime LD_CurrentVersionFirstReportLocal
		{
			get { return LD_CurrentVersionFirstReportUtc.ToLocalBranchTime(); }
			set { LD_CurrentVersionFirstReportUtc = value.ToUniversalBranchTime(); }
		}

		public virtual ZPropertyInfo LD_CurrentVersionFirstReportLocalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetWrappedZPropertyInfo(Schema.LD_CurrentVersionFirstReportUtc, x => LD_CurrentVersionFirstReportUtcInfo); }
		}

		[ActionField(ReadOnly = true)]
		public ZDateTime LD_CurrentVersionLastReportLocal
		{
			get { return LD_CurrentVersionLastReportUtc.ToLocalBranchTime(); }
			set { LD_CurrentVersionLastReportUtc = value.ToUniversalBranchTime(); }
		}

		public virtual ZPropertyInfo LD_CurrentVersionLastReportLocalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetWrappedZPropertyInfo(Schema.LD_CurrentVersionLastReportUtc, x => LD_CurrentVersionLastReportUtcInfo); }
		}

		#endregion

		#region Current Version's Exe Date

		public ZDateTime CurrentVersionExeDate
		{
			get
			{
				ReleaseBuild currentVersion = CurrentVersion;
				return (currentVersion != null) ? currentVersion.HL_ExeVersionDate : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo CurrentVersionExeDateInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentVersionExeDate)); }
		}

		#endregion

		#region Current Version's Release

		public ZString CurrentVersionRelease
		{
			get
			{
				ReleaseBuild currentVersion = CurrentVersion;
				return (currentVersion != null) ? currentVersion.ReleaseDisplayText : ZString.Empty;
			}
		}

		public ZPropertyInfo CurrentVersionReleaseInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentVersionRelease)); }
		}

		#endregion

		#region Sent Version

		[RelatedBusinessObject("SentVersion")]
		[List("Lookups.CurrentVersions")]
		[ActionField(ReadOnly = true)]
		public override ZGuid LD_HL_CurrentSentVersion
		{
			get { return base.LD_HL_CurrentSentVersion; }
			set { base.LD_HL_CurrentSentVersion = value; }
		}

		[ActionFieldFollow(false)]
		public virtual ReleaseBuild SentVersion
		{
			get { return Factory.Load<ReleaseBuild>(LD_HL_CurrentSentVersion); }
		}

		#endregion

		#region Sent Version's Exe Date

		public ZDateTime SentVersionExeDate
		{
			get
			{
				ReleaseBuild sentVersion = SentVersion;
				return (SentVersion != null) ? sentVersion.HL_ExeVersionDate : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo SentVersionExeDateInfo
		{
			get { return GetZPropertyInfo(nameof(SentVersionExeDate)); }
		}

		#endregion

		#region Sent Version's Release

		public ZString SentVersionRelease
		{
			get
			{
				ReleaseBuild sentVersion = SentVersion;
				return (SentVersion != null) ? sentVersion.ReleaseDisplayText : ZString.Empty;
			}
		}

		public ZPropertyInfo SentVersionReleaseInfo
		{
			get { return GetZPropertyInfo(nameof(SentVersionRelease)); }
		}

		#endregion

		#region AddressAsString

		public ZString AddressAsString
		{
			get { return SoftwareInstallAddressDetails != null ? SoftwareInstallAddressDetails.AddressAsASingleLine : ZString.Empty; }
		}

		public ZPropertyInfo AddressAsStringInfo
		{
			get { return GetZPropertyInfo(Schema.AddressAsString); }
		}

		#endregion

		#region Connection Details

		public ZBlob ConnectionDetails
		{
			get { return ConnectionDetailsNote.Rtf; }
			set
			{
				ConnectionDetailsNote.Rtf = value;
				HasChanges = true;
				ConnectionDetailsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConnectionDetailsInfo
		{
			get { return GetZPropertyInfo(Schema.ConnectionDetails); }
		}

		public ZBlob ConnectionDetails_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(ConnectionDetails);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				ConnectionDetails = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public HiddenRtfNote ConnectionDetailsNote
		{
			get
			{
				if (fConnectionDetailsNote == null)
				{
					fConnectionDetailsNote = new HiddenRtfNote(this);
				}

				return fConnectionDetailsNote;
			}
		}
		HiddenRtfNote fConnectionDetailsNote;

		#endregion

		#region CompanyCode

		public ZString CompanyCode
		{
			get { return LicEnterprise?.Header?.OH_Code ?? ZString.Empty; }
		}

		public ZPropertyInfo CompanyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CompanyCode); }
		}

		#endregion

		#region CompanyName

		public ZString CompanyName
		{
			get { return LicEnterprise?.Header?.OH_FullName ?? ZString.Empty; }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return GetZPropertyInfo(Schema.CompanyName); }
		}

		#endregion

		#region Expiry Message

		public HiddenTextNote CustomExpiredNote
		{
			get { return customExpiredNote ?? (customExpiredNote = new HiddenTextNote(this, "CustomExpiredMessage")); }
		}
		HiddenTextNote customExpiredNote;

		public HiddenTextNote CustomExpiryWeekNote
		{
			get { return customExpiryWeekNote ?? (customExpiryWeekNote = new HiddenTextNote(this, "CustomExpiryWeekMessage")); }
		}
		HiddenTextNote customExpiryWeekNote;

		public HiddenTextNote CustomExpiryMonthNote
		{
			get { return customExpiryMonthNote ?? (customExpiryMonthNote = new HiddenTextNote(this, "CustomExpiryMonthMessage")); }
		}
		HiddenTextNote customExpiryMonthNote;

		#endregion

		#region DateFromWhichDatabaseHeartbeatCanBeReset

		public static readonly ZDateTime DateFromWhichDatabaseHeartbeatCanBeReset = new ZDateTime(2005, 11, 11);

		#endregion

		#region DateFromWhichLicenceIsAutoDeployable

		public static readonly ZDateTime DateFromWhichLicenceIsAutoDeployable = new ZDateTime(2006, 03, 18);

		#endregion

		#region LatestSentOrCurrentVersion

		[ActionFieldFollow(false)]
		public ReleaseBuild LatestSentOrCurrentVersion
		{
			get
			{
				ReleaseBuild current = CurrentVersion;
				ReleaseBuild sent = SentVersion;
				if (current != null && sent != null)
				{
					return current.VersionNumber > sent.VersionNumber
						? current
						: sent;
				}
				else if (sent == null)
				{
					return current;
				}
				else
				{
					return sent;
				}
			}
		}

		#endregion

		#region Edition

		public const int MaxCountryUsers = 200;
		public const int MaxRegionUsers = 1000;

		#endregion

		#region Hosted Location

		[List("Lookups.HostedLocations")]
		public override ZString LD_HostedLocation
		{
			get { return base.LD_HostedLocation; }
			set { base.LD_HostedLocation = value; }
		}

		public ZString HostedLocationDesciption
		{
			get { return Lookups.HostedLocations.GetDescriptionFromCode(LD_HostedLocation); }
		}

		public static ZBool IsOnWiseCloud(string lD_HostedLocation)
		{
			return EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(lD_HostedLocation);
		}

		public ZBool IsHostedOnWiseCloud
		{
			get { return IsOnWiseCloud(LD_HostedLocation); }
		}

		#endregion

		#region Product

		[List("Lookups.ProductTypeList")]
		public override ZString LD_Product
		{
			get { return base.LD_Product; }
			set { base.LD_Product = value; }
		}

		public bool IsConsolidatedDatabase => EDIDataRegistry.Instance.ConsolidatedBillingSettings.Value.OfType<ConsolidatedBillingSetting>().Any(x => x.ProductCode.EqualsIgnoringCase(LD_Product));

		public LicenceHeader[] GetProductLicHeaders()
		{
			ZQuery query = new ZQuery(LicenceHeaderSchema.LA_LD, PK);
			var licHeader = Factory.Load<LicenceHeader>(query);

			return licHeader;
		}

		#endregion

		#region Status

		[List("Lookups.StatusList")]
		[ActionField(ReadOnly = true)]
		public override ZString LD_Status
		{
			get { return base.LD_Status; }
			set { base.LD_Status = value; }
		}

		#endregion

		#region Enterprise Code / Id

		public ZString EnterpriseCode
		{
			get { return LicEnterprise?.LE_EnterpriseCode ?? ""; }
		}

		public bool EnterpriseCode_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo EnterpriseCodeInfo
		{
			get { return GetZPropertyInfo(nameof(EnterpriseCode)); }
		}

		public ZString EnterpriseID
		{
			get { return LicEnterprise?.LE_EnterpriseID ?? ""; }
		}

		#endregion

		#region LD_OutboundEAdaptorUrl

		public bool LD_OutboundEAdaptorUrl_ReadOnly { get => true; }

		#endregion

		#region LD_NextRunTimeUtcUPG

		public bool LD_NextRunTimeUtcUPG_ReadOnly { get => true; }

		#endregion

		#region LD_NextRunTimeUtcMUG

		public bool LD_NextRunTimeUtcMUG_ReadOnly { get => true; }

		#endregion

		#region LD_ScheduleStateUPG

		public bool LD_ScheduleStateUPG_ReadOnly { get => true; }

		#endregion

		#region LD_ScheduleStateMUG

		public bool LD_ScheduleStateMUG_ReadOnly { get => true; }

		#endregion

		#region Feature Control

		[RelatedBusinessObject("FeatureSet")]
		[List("Lookups.FeatureSets")]
		[ResourceStringData("LicenceDatabase|LD_FCS_FeatureSet", Caption = "Feature Set")]
		public override ZGuid LD_FCS_FeatureSet
		{
			get => base.LD_FCS_FeatureSet;
			set
			{
				base.LD_FCS_FeatureSet = value;

				if (!IsInDatabase || (ZGuid)LD_FCS_FeatureSetInfo.OriginalValue != value)
				{
					hasUpdatedFeatureSet = true;
				}
				else if (IsInDatabase && (ZGuid)LD_FCS_FeatureSetInfo.OriginalValue == value)
				{
					hasUpdatedFeatureSet = false;
				}
			}
		}

		bool hasUpdatedFeatureSet;

		public FeatureControlSet FeatureSet => Factory.Load<FeatureControlSet>(LD_FCS_FeatureSet);

		public bool LD_FeatureControlRuleLastSyncUtc_ReadOnly { get => true; }
		public bool LD_FeatureControlRuleLastSyncContent_ReadOnly { get => true; }

		#endregion

		public override ZDateTime LD_SystemLastEditTimeUtc
		{
			get => base.LD_SystemLastEditTimeUtc;
			set
			{
				base.LD_SystemLastEditTimeUtc = value;

				if (hasUpdatedFeatureSet)
				{
					if (FeatureSet != null)
					{
						FeatureSet.FCS_SystemLastEditTimeUtc = value;
					}

					if (PreviousFeatureSet != null)
					{
						PreviousFeatureSet.FCS_SystemLastEditTimeUtc = value;
					}
				}
			}
		}

		public override ZString LD_SystemLastEditUser
		{
			get => base.LD_SystemLastEditUser;
			set
			{
				base.LD_SystemLastEditUser = value;

				if (hasUpdatedFeatureSet)
				{
					if (FeatureSet != null)
					{
						FeatureSet.FCS_SystemLastEditUser = value;
					}

					if (PreviousFeatureSet != null)
					{
						PreviousFeatureSet.FCS_SystemLastEditUser = value;
					}
				}
			}
		}

		FeatureControlSet PreviousFeatureSet
		{
			get
			{
				if (hasUpdatedFeatureSet)
				{
					return Factory.Load<FeatureControlSet>((ZGuid)LD_FCS_FeatureSetInfo.OriginalValue);
				}

				return null;
			}
		}

		[ActionField(ReadOnly = true)]
		public override ZInt LD_AvailablePhysicalMemoryMB { get => base.LD_AvailablePhysicalMemoryMB; set => base.LD_AvailablePhysicalMemoryMB = value; }

		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_CurrentVersionFirstReportUtc { get => base.LD_CurrentVersionFirstReportUtc; set => base.LD_CurrentVersionFirstReportUtc = value; }

		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_CurrentVersionLastReportUtc { get => base.LD_CurrentVersionLastReportUtc; set => base.LD_CurrentVersionLastReportUtc = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_DatabaseConfig { get => base.LD_DatabaseConfig; set => base.LD_DatabaseConfig = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_DatabaseFilePathDetail { get => base.LD_DatabaseFilePathDetail; set => base.LD_DatabaseFilePathDetail = value; }

		[ActionField(ReadOnly = true)]
		public override ZInt LD_DatabaseNumber { get => base.LD_DatabaseNumber; set => base.LD_DatabaseNumber = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_HostConnectionServerName { get => base.LD_HostConnectionServerName; set => base.LD_HostConnectionServerName = value; }

		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_HostDBCreateDate { get => base.LD_HostDBCreateDate; set => base.LD_HostDBCreateDate = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_HostDBInstance { get => base.LD_HostDBInstance; set => base.LD_HostDBInstance = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_HostDBName { get => base.LD_HostDBName; set => base.LD_HostDBName = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_HostServerName { get => base.LD_HostServerName; set => base.LD_HostServerName = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_InternalPop3EmailAddress { get => base.LD_InternalPop3EmailAddress; set => base.LD_InternalPop3EmailAddress = value; }

		[ActionField(ReadOnly = true)]
		public override ZInt LD_InternalPop3Port { get => base.LD_InternalPop3Port; set => base.LD_InternalPop3Port = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_InternalPop3UserName { get => base.LD_InternalPop3UserName; set => base.LD_InternalPop3UserName = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_InternalSmtpEmailAddress { get => base.LD_InternalSmtpEmailAddress; set => base.LD_InternalSmtpEmailAddress = value; }

		[ActionField(ReadOnly = true)]
		public override ZInt LD_InternalSmtpPort { get => base.LD_InternalSmtpPort; set => base.LD_InternalSmtpPort = value; }

		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_LastHeartbeat { get => base.LD_LastHeartbeat; set => base.LD_LastHeartbeat = value; }

		[ActionField(ReadOnly = true)]
		public override ZBool LD_LegacyInterfaceSupport { get => base.LD_LegacyInterfaceSupport; set => base.LD_LegacyInterfaceSupport = value; }

		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_LicenceExpiry { get => base.LD_LicenceExpiry; set => base.LD_LicenceExpiry = value; }

		[ActionField(ReadOnly = true)]
		public override ZBool LD_LogFileOnDifferentPhysicalVolume { get => base.LD_LogFileOnDifferentPhysicalVolume; set => base.LD_LogFileOnDifferentPhysicalVolume = value; }

		[ActionField(ReadOnly = true)]
		public override ZDecimal LD_MaxDataInMegBeforeAck { get => base.LD_MaxDataInMegBeforeAck; set => base.LD_MaxDataInMegBeforeAck = value; }

		[ActionField(ReadOnly = true)]
		public override ZShort LD_NoOfActivePrintQueues { get => base.LD_NoOfActivePrintQueues; set => base.LD_NoOfActivePrintQueues = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString LD_Password { get => base.LD_Password; set => base.LD_Password = value; }

		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_ProcessorReleaseDate { get => base.LD_ProcessorReleaseDate; set => base.LD_ProcessorReleaseDate = value; }

		[ActionField(ReadOnly = true)]
		public override ZByte LD_RetryTimeoutInMinutes { get => base.LD_RetryTimeoutInMinutes; set => base.LD_RetryTimeoutInMinutes = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_SQLVersion { get => base.LD_SQLVersion; set => base.LD_SQLVersion = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_SQLVerString { get => base.LD_SQLVerString; set => base.LD_SQLVerString = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_OutboundEAdaptorUrl { get => base.LD_OutboundEAdaptorUrl; set => base.LD_OutboundEAdaptorUrl = value; }

		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_NextRunTimeUtcUPG { get => base.LD_NextRunTimeUtcUPG; set => base.LD_NextRunTimeUtcUPG = value; }

		[ActionField(ReadOnly = true)]
		public override ZDateTime LD_NextRunTimeUtcMUG { get => base.LD_NextRunTimeUtcMUG; set => base.LD_NextRunTimeUtcMUG = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_ScheduleStateUPG { get => base.LD_ScheduleStateUPG; set => base.LD_ScheduleStateUPG = value; }

		[ActionField(ReadOnly = true)]
		public override ZString LD_ScheduleStateMUG { get => base.LD_ScheduleStateMUG; set => base.LD_ScheduleStateMUG = value; }

		[ActionField(ReadOnly = true)]
		public override ZBool LD_TokenAuthenticationEnabled { get => base.LD_TokenAuthenticationEnabled; set => base.LD_TokenAuthenticationEnabled = value; }

		public bool LD_TokenAuthenticationEnabled_ReadOnly => true;

		#region User Management

		public bool LD_StaffFirstReportUtc_ReadOnly => true;

		public bool ShouldCloneContactOnWebAccessOrgChanged
		{
			get
			{
				var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, PK);
				userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, SQLComparisonOperator.NotEqual, null);

				return IsInDatabase
					&& LD_IsActive
					&& WebAccessOrg != null
					&& Factory.ExistsInDatabase(EdiCustomerUserAccountSchema.Constants.TableName, userAccountQuery);
			}
		}

		public override ZGuid LD_OH_WebAccessOrg
		{
			get => base.LD_OH_WebAccessOrg;
			set
			{
				if (value != base.LD_OH_WebAccessOrg)
				{
					PreviousWebAccessOrgValue = base.LD_OH_WebAccessOrg;
					base.LD_OH_WebAccessOrg = value;
				}
			}
		}

		ZGuid PreviousWebAccessOrgValue;

		public bool LD_OH_WebAccessOrg_ReadOnly => !AllowEditWebAccessOrg && !IsInDatabase;

		public void RevertWebAccessOrgChange()
		{
			base.LD_OH_WebAccessOrg = PreviousWebAccessOrgValue;
		}

		public EDIOrgHeader EDIWebAccessOrg
		{
			get { return Factory.Load<EDIOrgHeader>(LD_OH_WebAccessOrg); }
		}

		public void WebAccessOrgClearAllNotifications()
		{
			if (LD_OH_WebAccessOrgInfo.Notifications.Any())
			{
#if DEBUG
				using (SuspendValidationTesting())
#endif
				{
					LD_OH_WebAccessOrgInfo.ClearAllNotifications();
				}
			}
		}

		#endregion

		#region Trusted Messaging

		public bool IsMultiTenantDatabase => Lookups.MultiTenantDatabaseProductTypes.ContainsCode(LD_Product);

		[List("Lookups.TrustedSystems")]
		public override ZGuid LD_ETS_TrustedSystem
		{
			get => base.LD_ETS_TrustedSystem;
			set => base.LD_ETS_TrustedSystem = value;
		}

		public EdiTrustedSystem TrustedSystem => Factory.Load<EdiTrustedSystem>(LD_ETS_TrustedSystem);

		public EdiTrustedSystem GetOrCreateTrustedSystem()
		{
			if (LD_ETS_TrustedSystem.IsEmpty)
			{
				EdiTrustedSystem newSystem = null;
				if (IsMultiTenantDatabase)
				{
					var query = new ZQuery(EdiTrustedSystemSchema.ETS_Product, LD_Product);
					newSystem = Factory.LoadTop1<EdiTrustedSystem>(query);
				}

				if (newSystem == null)
				{
					newSystem = Factory.New<EdiTrustedSystem>();
					newSystem.ETS_Product = LD_Product;
				}

				if (newSystem.ETS_Description.IsEmpty)
				{
					newSystem.ETS_Description = Description;
				}

				newSystem.GetOrCreateCertificateConfig();
				LD_ETS_TrustedSystem = newSystem.PK;
			}
			return TrustedSystem;
		}

		public void RemoveTrustedSystem()
		{
			if (!LD_ETS_TrustedSystem.IsEmpty)
			{
				var currentTrustedSys = TrustedSystem;
				if (IsMultiTenantDatabase)
				{
					currentTrustedSys.CancelChanges();
				}
				else
				{
					currentTrustedSys.Delete();
				}

				LD_ETS_TrustedSystem = ZGuid.Empty;
			}
		}

		public void UpdateMessagingConfigOnAllMultiTenantDatabases()
		{
			if (IsMultiTenantDatabase && !LD_ETS_TrustedSystem.IsEmpty)
			{
				var query = new ZQuery(LicenceDatabaseSchema.LD_Product, LD_Product);
				var queryConfigClause = new ZQuery(LicenceDatabaseSchema.LD_ETS_TrustedSystem, null);
				queryConfigClause.AddToFilter(JoinCondition.Or, LicenceDatabaseSchema.LD_ETS_TrustedSystem, SQLComparisonOperator.NotEqual, LD_ETS_TrustedSystem);
				query.AddToFilter(queryConfigClause);
				foreach (var db in Factory.Load<LicenceDatabase>(query))
				{
					db.LD_ETS_TrustedSystem = LD_ETS_TrustedSystem;
				}
			}
		}

		public ZString SystemID => TrustedSystem?.ETS_SystemID ?? ZString.Empty;
		#endregion

		#endregion

		#region Licence Key Deployment

		public bool VersionSupportsCr8Cr9
		{
			get
			{
				if (CurrentVersion == null)
				{
					return false;
				}

				return BuildIsSupported(CurrentVersion.VersionNumber, EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.Value);
			}
		}

		public bool VersionIsDeployable
		{
			get
			{
				return CurrentVersion != null &&
					   CurrentVersion.HL_ExeVersionDate >= DateFromWhichLicenceIsAutoDeployable;
			}
		}

		public bool VersionCanHaveHeartbeatReset
		{
			get
			{
				return CurrentVersion != null &&
					   CurrentVersion.HL_ExeVersionDate >= DateFromWhichDatabaseHeartbeatCanBeReset;
			}
		}

		public bool PublicEmailIsDeployable
		{
			get
			{
				return !LD_PublicEmailAddressForUpdate.IsEmpty &&
					   EmailAddressValidation.IsEmailAddressValid(LD_PublicEmailAddressForUpdate);
			}
		}

		public const int FirstReleaseSystemMessageCSR = 4461; // Mar 19 - CustomerServiceResponse

		public TriState VersionCanReceiveSystemMessageCSR
		{
			get
			{
				return CurrentVersion == null
					? TriState.NotDetermined
					: (BuildIsSupported(CurrentVersion.VersionNumber, "1.4." + FirstReleaseSystemMessageCSR.ToString(CultureInfo.InvariantCulture) + ".0") ? TriState.True : TriState.False);
			}
		}

		public const int FirstReleaseSystemMessageRDU = 4465; // Mar 23 - ReferenceDataUpdate

		public TriState VersionCanReceiveSystemMessageRDU
		{
			get
			{
				return CurrentVersion == null
					? TriState.NotDetermined
					: (BuildIsSupported(CurrentVersion.VersionNumber, "1.4." + FirstReleaseSystemMessageRDU.ToString(CultureInfo.InvariantCulture) + ".0") ? TriState.True : TriState.False);
			}
		}

		public const string FirstReleaseCSBiDirectionMessage = "1.4.4557.0"; // Jun 23, 2012 - Can update customer service from response

		internal const string LegacyRelease_EHubSystemMessagesNotSupported = "1.4.2.4";

		public TriState VersionCanSupportBiDirectionIncidentMessage
		{
			get { return CanSupportBiDirectionIncidentMessage(CurrentVersion); }
		}

		static public TriState CanSupportBiDirectionIncidentMessage(ReleaseBuild build)
		{
			TriState result;

			if (build == null)
			{
				result = TriState.NotDetermined;
			}
			else if (BuildIsSupported(build.VersionNumber, FirstReleaseCSBiDirectionMessage)
					 || BuildIsSupported(build.VersionNumber, EDIDataRegistry.Instance.ERequestsReleaseBuilds.Value)
					 || BuildIsSupported(build.VersionNumber, EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.Value))
			{
				result = TriState.True;
			}
			else
			{
				result = TriState.False;
			}

			return result;
		}

		public TriState VersionCanSupportSendIncidentEmailFromClient
		{
			get { return CanSupportSendIncidentEmailFromClient(CurrentVersion); }
		}

		public static TriState CanSupportSendIncidentEmailFromClient(ReleaseBuild build)
		{
			TriState result;

			if (build == null)
			{
				result = TriState.NotDetermined;
			}
			else if (BuildIsSupported(build.VersionNumber, EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.Value))
			{
				result = TriState.True;
			}
			else
			{
				result = TriState.False;
			}

			return result;
		}

		public TriState VersionCanSupportCustomExpiryMessages
		{
			get { return CanSupportCustomExpiryMessages(CurrentVersion); }
		}

		public static TriState CanSupportCustomExpiryMessages(ReleaseBuild build)
		{
			TriState result;

			if (build == null)
			{
				result = TriState.NotDetermined;
			}
			else if (BuildIsSupported(build.VersionNumber, EDIDataRegistry.Instance.CustomExpiryMessagesReleaseBuilds.Value))
			{
				result = TriState.True;
			}
			else
			{
				result = TriState.False;
			}

			return result;
		}

		/// <summary>
		/// No more messages via email
		/// </summary>
		public TriState VersionCanReceiveAllSystemMessages
		{
			get { return CanReceiveAllSystemMessages(CurrentVersion); }
		}

		public static TriState CanReceiveAllSystemMessages(ReleaseBuild build)
		{
			TriState result;

			if (build == null)
			{
				result = TriState.NotDetermined;
			}
			else if (BuildIsSupported(build.VersionNumber, EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.Value))
			{
				result = TriState.True;
			}
			else
			{
				result = TriState.False;
			}

			return result;
		}

		public TriState VersionHasGlowERequests
		{
			get { return HasGlowERequests(CurrentVersion); }
		}

		public static TriState HasGlowERequests(ReleaseBuild build)
		{
			TriState result;

			if (build == null)
			{
				result = TriState.NotDetermined;
			}
			else if (BuildIsSupported(build.VersionNumber, EDIDataRegistry.Instance.HasGlowERequests.Value))
			{
				result = TriState.True;
			}
			else
			{
				result = TriState.False;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool BuildIsSupported(VersionNumber buildVersion, string firstVersions)
		{
			var versionList = firstVersions.Split(',');
			foreach (string version in versionList)
			{
				string trimmed = version.Trim();
				if (trimmed.Length > 0)
				{
					try
					{
						var firstVersion = new VersionNumber(trimmed);
						if (firstVersion.Major < 14 && buildVersion.Major < 14)
						{
							// ediEnterprise: major.minor.release.patch
							if (buildVersion.Major > firstVersion.Major
								|| (buildVersion.Major == firstVersion.Major && buildVersion.Minor > firstVersion.Minor)
								|| (buildVersion.Major == firstVersion.Major && buildVersion.Minor == firstVersion.Minor
									&&
									(
										(buildVersion.Release >= firstVersion.Release && firstVersion.Patch == 0)
										||
										(buildVersion.Release == firstVersion.Release && buildVersion.Patch >= firstVersion.Patch))
								)
							)
							{
								return true;
							}
						}
						else if (firstVersion.Major >= 14 && buildVersion.Major >= 14)
						{
							// CW1: year.month.day.patch
							if (firstVersion.Patch == 0)
							{
								if (buildVersion.Major > firstVersion.Major
									|| (buildVersion.Major == firstVersion.Major && buildVersion.Minor > firstVersion.Minor)
									|| (buildVersion.Major == firstVersion.Major && buildVersion.Minor == firstVersion.Minor && buildVersion.Release >= firstVersion.Release))
								{
									return true;
								}
							}
							else if (buildVersion.Major == firstVersion.Major
									 && buildVersion.Minor == firstVersion.Minor
									 && buildVersion.Release == firstVersion.Release
									 && buildVersion.Patch >= firstVersion.Patch)
							{
								return true;
							}
						}
						else if (firstVersion.Major < 14 && firstVersion.Release <= 5000 && firstVersion.Patch == 0 && buildVersion.Major >= 14)
						{
							// ediEnterprise was branched off to LPB around release 5000 (1.4.5000.0)
							// So any version before that became part of CW1.
							// CW1 had the old style versioning from Aug 2013 to Mar 2014 (2.0.6.0 to 2.0.215.0)
							return true;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}
			}

			return false;
		}

		#endregion

		#region Related Business Objects

		#region Licence Headers (For All Companies)

		[ReadOnly(true)]
		[ActionFieldFollow(false)]
		public LicenceHeaderCollection LicHeadersForAllCompanies
		{
			get
			{
				if (fLicHeadersForAllCompanies == null)
				{
					var localLicHeadersForAllCompanies = new LicenceHeaderCollection(Factory, new ZQuery(LicenceHeaderSchema.LA_LD, PK));
					localLicHeadersForAllCompanies.Load();
					fLicHeadersForAllCompanies = localLicHeadersForAllCompanies;
				}

				return fLicHeadersForAllCompanies;
			}
		}
		LicenceHeaderCollection fLicHeadersForAllCompanies;

		[ReadOnly(true)]
		[ActionFieldFollow(false)]
		public LicenceHeaderCollection ActiveLicHeadersForAllCompanies
		{
			get
			{
				if (activeLicHeadersForAllCompanies == null)
				{
					var query = new ZQuery(LicenceHeaderSchema.LA_LD, PK);
					query.AddToFilter(LicenceHeaderSchema.LA_IsActive, ZBool.True);
					var localActiveLicHeadersForAllCompanies = new LicenceHeaderCollection(Factory, query);
					localActiveLicHeadersForAllCompanies.Load();
					activeLicHeadersForAllCompanies = localActiveLicHeadersForAllCompanies;
				}

				return activeLicHeadersForAllCompanies;
			}
		}
		LicenceHeaderCollection activeLicHeadersForAllCompanies;

		/// <summary>
		/// The licence corresponding to LD_OH_BillingParty, that owns per-database usage charges.
		/// Will be null if LD_OH_BillingParty is not set.
		/// </summary>
		[ActionFieldFollow(false)]
		public LicenceHeader UsageOwnerLicence
		{
			get
			{
				return !LD_OH_BillingParty.IsEmpty
					? LicHeadersForAllCompanies.Cast<LicenceHeader>().FirstOrDefault(x => x.Company.LC_OH == LD_OH_BillingParty)
					: null;
			}
		}

		/// <summary>
		/// The licence corresponding to LD_OH_BillingParty if defined, otherwise the first live, active licence.
		/// </summary>
		[ActionFieldFollow(false)]
		public LicenceHeader UsageOwnerOrFirstLicence
		{
			get { return UsageOwnerLicence ?? LicenceHeader.FirstActiveLive(LicHeadersForAllCompanies.Cast<LicenceHeader>()); }
		}

		#endregion

		#region ClientCompanies

		public string LicenceCodeForSystemMessage
		{
			get
			{
				if (!string.IsNullOrEmpty(CompanyCodeFromLastHeartbeat))
				{
					return LicEnterprise.LE_EnterpriseCode + CompanyCodeFromLastHeartbeat + LD_ServerCode;
				}

				var oldestActiveCompany = ClientCompanies.Where(x => x.LCC_DeactivateTimeUtc.IsEmpty)
					.OrderBy(x => x.LCC_CreateTimeUtc)
					.FirstOrDefault();
				if (oldestActiveCompany != null)
				{
					return oldestActiveCompany.LicenceCode;
				}

				var licHeader = ActiveLicHeadersForAllCompanies.Cast<LicenceHeader>().FirstOrDefault()
								?? LicHeadersForAllCompanies.Cast<LicenceHeader>().FirstOrDefault();

				if (licHeader != null)
				{
					return licHeader.LicenceCode;
				}

				return string.Empty;
			}
		}

		[ChildEditable]
		[ActionFieldFollow(false)]
		public ClientCompanyCollection ClientCompanies
		{
			get
			{
				if (clientCompanies == null)
				{
					clientCompanies = new ClientCompanyCollection(Factory, new ZQuery(ClientCompanySchema.LCC_LD, PK));
					RegisterEditableChildObject(clientCompanies);
				}

				return clientCompanies;
			}
		}
		ClientCompanyCollection clientCompanies;

		public ClientCompanyCollection ClientCompaniesExcludingDemo
		{
			get
			{
				if (clientCompaniesExcludingDemo == null)
				{
					clientCompaniesExcludingDemo = new ClientCompanyCollection(Factory, new ZQuery(ClientCompanySchema.LCC_LD, PK)
						.AddToFilter(ClientCompanySchema.LCC_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode));
				}

				return clientCompaniesExcludingDemo;
			}
		}
		ClientCompanyCollection clientCompaniesExcludingDemo;

		#endregion

		#region LicEnterprise

		[ActionFieldFollow(false)]
		public LicenceEnterprise LicEnterprise
		{
			get { return Factory.Load<LicenceEnterprise>(LD_LE); }
		}

		#endregion

		#region Licence Connections

		[ChildEditable(true)]
		[ActionFieldFollow(false)]
		public LicenceConnectionDependentCollection Connections
		{
			get
			{
				if (fConnections == null)
				{
					fConnections = new LicenceConnectionDependentCollection(this, Factory);
					fConnections.Load();
					RegisterEditableChildObject(fConnections);
				}

				return fConnections;
			}
		}

		LicenceConnectionDependentCollection fConnections;

		#endregion

		#region Premium Services

		[ChildEditable(true)]
		[ActionFieldFollow(false)]
		public ClientPremiumServiceCollection PremiumServices
		{
			get
			{
				if (premiumServices == null)
				{
					premiumServices = new ClientPremiumServiceCollection(this);
					RegisterEditableChildObject(premiumServices);
					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						premiumServices.SetReadOnlyIncludingChildren(true);
					}
				}

				return premiumServices;
			}
		}

		ClientPremiumServiceCollection premiumServices;

		#endregion

		#region PriceHeaderLinks

		[ChildEditable(true)]
		[ActionFieldFollow(false)]
		public EdiPriceHeaderLinkCollection PriceHeaderLinks
		{
			get
			{
				if (priceHeaderLinks == null)
				{
					priceHeaderLinks = new EdiPriceHeaderLinkCollection(this);
					RegisterEditableChildObject(priceHeaderLinks);
					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						priceHeaderLinks.SetReadOnlyIncludingChildren(true);
					}
				}

				return priceHeaderLinks;
			}
		}
		EdiPriceHeaderLinkCollection priceHeaderLinks;

		[ChildEditable(true)]
		[ActionFieldFollow(false)]
		public EdiPriceHeaderLinkCollection PriceHeaderLinksForBinding
		{
			get
			{
				if (priceHeaderLinksForBinding == null)
				{
					priceHeaderLinksForBinding = new EdiPriceHeaderLinkCollection(this);
					UpdatePriceHeaderLinksForBindingFilter();
					RegisterEditableChildObject(priceHeaderLinksForBinding);
					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						priceHeaderLinksForBinding.SetReadOnlyIncludingChildren(true);
					}
				}

				return priceHeaderLinksForBinding;
			}
		}
		EdiPriceHeaderLinkCollection priceHeaderLinksForBinding;

		void UpdatePriceHeaderLinksForBindingFilter()
		{
			if (priceHeaderLinksForBinding == null)
			{
				return;
			}

			if (showExpiredPriceHeaderLinks)
			{
				priceHeaderLinksForBinding.AdditionalFilter = new ZQuery();
			}
			else
			{
				var today = ZDateTime.Today;
				var validLastMonth = today.AddDays(1 - today.Day).AddMonths(-1).AddDays(15);
				var validToColumn = EdiPriceHeaderLinkSchema.PHL_ValidTo;
				var validToFilter = new ZQuery(validToColumn, null);
				validToFilter.AddToFilter(JoinCondition.Or, validToColumn, SQLComparisonOperator.GreaterThanOrEqualTo, validLastMonth);
				priceHeaderLinksForBinding.AdditionalFilter = validToFilter;
				// Prices are automatically expired by the presence of a later pricelist and often don't have a ValidTo
				// So also filter on ValidFrom based on the most recent pricelist
				if (priceHeaderLinksForBinding.Count > 1)
				{
					var validFrom = priceHeaderLinksForBinding
						.Where(x => x.PHL_ValidFrom < validLastMonth)
						.Max(x => x.PHL_ValidFrom);
					var expiredFilter = new ZQuery(EdiPriceHeaderLinkSchema.PHL_ValidFrom, SQLComparisonOperator.GreaterThanOrEqualTo, validFrom);
					expiredFilter.AddToFilter(validToFilter);
					priceHeaderLinksForBinding.AdditionalFilter = expiredFilter;
				}
			}
		}

		public ZBool ShowExpiredPriceHeaderLinks
		{
			get => showExpiredPriceHeaderLinks;
			set
			{
				showExpiredPriceHeaderLinks = value;
				ShowExpiredPriceHeaderLinksInfo.RefreshBinding();
				UpdatePriceHeaderLinksForBindingFilter();
			}
		}
		ZBool showExpiredPriceHeaderLinks = false;

		public virtual ZPropertyInfo ShowExpiredPriceHeaderLinksInfo
			=> GetZPropertyInfo(nameof(ShowExpiredPriceHeaderLinks));

		public EdiPriceHeaderLink PriceHeaderLinkForDate(ZDateTime dateTime)
		{
			var link = PriceHeaderLinks.Where(x => x.PHL_ValidFrom <= dateTime).OrderByDescending(x => x.PHL_ValidFrom).FirstOrDefault();
			return link != null && (link.PHL_ValidTo.IsEmpty || link.PHL_ValidTo >= dateTime)
				? link
				: null;
		}

		#endregion

		#region LicenceSettings

		[ChildEditable(true)]
		[ActionFieldFollow(false)]
		public EdiLicenceSettingCollection LicenceSettings
		{
			get
			{
				if (licenceSettings == null)
				{
					licenceSettings = new EdiLicenceSettingCollection(this);
					RegisterEditableChildObject(licenceSettings);
					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						licenceSettings.SetReadOnlyIncludingChildren(true);
					}
				}

				return licenceSettings;
			}
		}
		EdiLicenceSettingCollection licenceSettings;

		[ChildEditable(true)]
		[ActionFieldFollow(false)]
		public EdiLicenceSettingCollection LicenceSettingsForBinding
		{
			get
			{
				if (licenceSettingsForBinding == null)
				{
					licenceSettingsForBinding = new EdiLicenceSettingCollection(this);
					UpdateLicenceSettingsForBindingFilter();
					RegisterEditableChildObject(licenceSettingsForBinding);
					if (!EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed)
					{
						licenceSettingsForBinding.SetReadOnlyIncludingChildren(true);
					}
				}

				return licenceSettingsForBinding;
			}
		}
		EdiLicenceSettingCollection licenceSettingsForBinding;

		void UpdateLicenceSettingsForBindingFilter()
		{
			if (licenceSettingsForBinding == null)
			{
				return;
			}

			ZQuery expiredFilter;

			if (showExpiredLicenceSettings)
			{
				expiredFilter = new ZQuery();
			}
			else
			{
				var today = ZDateTime.Today;
				var validLastMonth = today.AddDays(1 - today.Day).AddMonths(-1).AddDays(15);
				var validToColumn = EdiLicenceSettingSchema.LS9_ValidTo;
				expiredFilter = new ZQuery(validToColumn, null);
				expiredFilter.AddToFilter(JoinCondition.Or, validToColumn, SQLComparisonOperator.GreaterThanOrEqualTo, validLastMonth);
			}

			licenceSettingsForBinding.AdditionalFilter = expiredFilter;
		}

		public ZBool ShowExpiredLicenceSettings
		{
			get => showExpiredLicenceSettings;
			set
			{
				showExpiredLicenceSettings = value;
				ShowExpiredLicenceSettingsInfo.RefreshBinding();
				UpdateLicenceSettingsForBindingFilter();
			}
		}
		ZBool showExpiredLicenceSettings = false;

		public virtual ZPropertyInfo ShowExpiredLicenceSettingsInfo
			=> GetZPropertyInfo(nameof(ShowExpiredLicenceSettings));

		#endregion

		#region Telematics Devices

		[ChildEditable]
		[ActionFieldFollow(false)]
		public ClientDeviceHeaderCollection TelematicsDevices
		{
			get
			{
				if (telematicsDevices == null)
				{
					telematicsDevices = new ClientDeviceHeaderCollection(this);
					RegisterEditableChildObject(telematicsDevices);
				}

				return telematicsDevices;
			}
		}

		ClientDeviceHeaderCollection telematicsDevices;

		#endregion

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		string VersionText(ZGuid releasePk)
		{
			ReleaseBuild build = releasePk.IsValid ? Factory.Load<ReleaseBuild>(releasePk) : null;
			return build != null ? build.VersionNumber.ToString() : string.Empty;
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>();
				result.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(Connections);
				return result.ToArray();
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			ConnectionDetails = ZBlob.Empty;
			Connections.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Supported Upgrade Method

		public string GetHighestSupportedUpgradeMethod()
		{
			string result = UpgradeMethods.Codes.Blocked;

			if (LD_AvailableUpgradeMethod != UpgradeMethods.Codes.Blocked)
			{
				if (VersionCanReceiveAllSystemMessages == TriState.True)
				{
					result = UpgradeMethods.Codes.Http;
				}
				else if (!LD_PublicEmailAddressForUpdate.IsEmpty)
				{
					if ((LD_AvailableUpgradeMethod == UpgradeMethods.Codes.Http || LD_AvailableUpgradeMethod.IsEmpty)
						&& HttpVersion.IsSupportingVersion(CurrentVersion))
					{
						result = UpgradeMethods.Codes.Http;
					}
				}
			}

			return result;
		}

		public bool IsUpgradeMethodSupported(string upgradeMethod)
		{
			switch (upgradeMethod)
			{
				case UpgradeMethods.Codes.Blocked:
					{
						return true;
					}
				case UpgradeMethods.Codes.Http:
					{
						return (!LD_PublicEmailAddressForUpdate.IsEmpty && HttpVersion.IsSupportingVersion(CurrentVersion))
							   ||
							   (VersionCanReceiveAllSystemMessages == TriState.True);
					}
				default:
					return false;
			}
		}

		#region Http Version

		HttpDownload HttpVersion
		{
			get
			{
				if (fHttpVersion == null)
				{
					fHttpVersion = new HttpDownload();
				}

				return fHttpVersion;
			}
		}

		HttpDownload fHttpVersion;

		#endregion

		#endregion

		#region Send System Expiry Update

		public void SendUpdateForSystemExpiry(string heartbeatCompanyCode)
		{
			var newDate = LD_ManualLicenceExpiry;
			if (newDate.IsEmpty)
			{
				newDate = ZDateTime.Today.AddDays(Licences.DefaultLicenceGracePeriodInDays);
			}
			SendUpdateForSystemExpiry(heartbeatCompanyCode, newDate);
		}

		public string CompanyCodeFromLastHeartbeat { get; set; }

		public void SendUpdateForSystemExpiry(string heartbeatCompanyCode, ZDateTime newLicenceExpiryDate)
		{
			if (newLicenceExpiryDate.IsEmpty)
			{
				throw new ArgumentException("new expiry date is blank");
			}

			CompanyCodeFromLastHeartbeat = heartbeatCompanyCode;
			var packet = GetSystemRegistrationKeyPacket(newLicenceExpiryDate);
			var sender = CreateSystemUpdatePacketSender();
			sender.Send(this, packet);
		}

		ISystemUpdatePacketSender CreateSystemUpdatePacketSender()
		{
			if (Globals.IsTest && SystemUpdatePacketSender != null)
			{
				return SystemUpdatePacketSender;
			}
			if (VersionCanReceiveSystemMessageRDU == TriState.True)
			{
				return new SystemUpdatePacketEHubSender();
			}
			else
			{
				return new SystemUpdatePacketMailSender();
			}
		}

		internal ISystemUpdatePacketSender SystemUpdatePacketSender;

		public SystemUpdatePacket GetSystemRegistrationKeyPacket(ZDateTime licenceExpiry)
		{
			bool isHostedWithCargoWise = EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(LD_HostedLocation);
			string cargowiseHostedLocation = isHostedWithCargoWise ? (string)LD_HostedLocation : Core.Constants.LicenceConstants.NotHostedWithCargoWise;
			var info = new BillingTimeZoneInfo(ZDateTime.UtcNow.ToDateTime());

			SystemUpdatePacket packet = new SystemUpdatePacket();
			ISystemRegistrationKey regKey = new SystemRegistrationKey(
				licenceExpiry.ToDateTime(),
				LD_HostServerSID.ToGuid(),
				LD_HostDBInstance,
				LD_HostDBName,
				LD_LicenceType,
				LD_DBServerSecurityMode,
				cargowiseHostedLocation, CustomExpiredNote.Text, CustomExpiryWeekNote.Text, CustomExpiryMonthNote.Text,
				DatabaseId,
				info.CurrentBillingTimeZoneUtcOffset, info.NextBillingTimeZoneUtcOffset, info.NextUtcOffsetEffectiveTimeUtc,
				BillingModel
			);

			packet.EncryptedSysRegKey = regKey.ToEncryptedKeyString();
			return packet;
		}

		#endregion

		#region Request Version Report

		public const string LegacyVersionReportRequestSubject = "Version Report Request";

		/// <summary>
		/// Request a version report confirmation.
		/// Called during after sending a new system registration to legacy (ediEnterprise) system
		/// to confirm that it was received.
		/// The request will be sent after 10 minutes to give the remote system time to process the new registration.
		/// </summary>
		public void RequestVersionReportAsConfirmationFromLegacySystem()
		{
			RequestVersionReportAfterDelay(TimeSpan.FromMinutes(10));
		}

		void RequestVersionReportAfterDelay(TimeSpan sendingDelay)
		{
			if (VersionCanHaveHeartbeatReset && PublicEmailIsDeployable)
			{
				EmailDef email = new EmailDef();
				email.AddRecipientForUserCommunication(LD_PublicEmailAddressForUpdate);
				email.Subject = LegacyVersionReportRequestSubject;

				if (sendingDelay == TimeSpan.Zero)
				{
					Env.OutgoingMailManager.CreateAndSave(email);
				}
				else
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					var mailItem = OutgoingMailCreator.Instance.NewMailItem(factory, email);
					mailItem.MI_SendDateTime += sendingDelay;
					factory.Save();
				}
			}
		}

		/// <summary>
		/// Request a version report.
		/// Called in response to a request from a user to reset the heartbeat or receive a heartbeat
		/// from the LicenceDatabaseForm.
		/// </summary>
		public void RequestVersionReportFromLegacySystem()
		{
			RequestVersionReportAfterDelay(TimeSpan.Zero);
		}

		public bool CanRequestVersionReportFromLegacySystem
		{
			get
			{
				return IsEnterpriseFamilyDatabase
					   && !HasErrors
					   && VersionCanHaveHeartbeatReset
					   && PublicEmailIsDeployable
					   && LD_Status != DatabaseStatusList.Codes.REG
					   && VersionCanReceiveAllSystemMessages != TriState.True;
			}
		}

		#endregion

		#region IsEnterpriseFamilyDatabase

		public bool IsEnterpriseFamilyDatabase => ProductTypes.IsEnterpriseFamily(LD_Product);

		#endregion

		#region Heartbeat Functionality

		/// <summary>
		/// Legacy (ediEnterprise) system registration reset.
		/// This method resets the SID and the expiry date, and requests a version report. When the version report arrives, our system will update 
		/// itself and send the new registration packet, thereby resetting the heartbeat
		/// </summary>
		public bool ResetHeartbeat()
		{
			bool result = false;

			if (CanRequestVersionReportFromLegacySystem)
			{
				result = true;
				LD_HostServerSID = ZGuid.Empty;
				LD_LicenceExpiry = ZDateTime.Empty; // Guarantee's registration packet will be sent.
				LD_LastHeartbeat = ZDateTime.Empty;
				LD_DatabaseFilePathDetail = ZString.Empty;
				LD_HostDBName = ZString.Empty;
				LD_HostDBInstance = ZString.Empty;
				LD_HostServerName = ZString.Empty;
				LD_InternalPop3EmailAddress = ZString.Empty;
				LD_InternalPop3Port = 0;
				LD_InternalPop3UserName = ZString.Empty;
				LD_InternalSmtpEmailAddress = ZString.Empty;
				LD_InternalSmtpPort = 0;
				Factory.Save();

				RequestVersionReportFromLegacySystem();
			}

			return result;
		}

		public bool ShouldUpdateFromHeartbeat
		{
			get
			{
				return LD_LastHeartbeat == ZDateTime.Empty ||
					   LD_LastHeartbeat.AddHours(23) < ZDateTime.Now;
			}
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;

			if (property.Name.IndexOf('+') == -1)
			{
				// this is a direct property of the class
				// and not a wrapped property of an inner object like LicHeader or LicCompany
				//
				var checkpoint = property.Name == Schema.ConnectionDetails ? EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails : EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails;
				result = checkpoint != null && !checkpoint.IsAllowed;
			}

			return result || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		public bool HasMultipleEnterprises
		{
			get
			{
				bool result = false;
				foreach (LicenceHeader header in LicHeadersForAllCompanies)
				{
					if (header.Company != null && header.Company.LC_LE != LD_LE)
					{
						result = true;
						break;
					}
				}

				return result;
			}
		}

		#region Product Keys

		public static LicenceDatabase[] GetAllProductKeysForUser(string staffCode)
		{
			return new BusinessObjectFactory().Load<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_GS_NKOwner, staffCode));
		}

		public static void CreateNewProductKey(string enterpriseCode, string serverCode, string owner)
		{
			var factory = new BusinessObjectFactory();

			var licenceEnterprise = factory.LoadFromNaturalKey<LicenceEnterprise>(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode);

			var licenceDatabase = factory.New<LicenceDatabase>();
			licenceDatabase.LD_LE = licenceEnterprise.PK; // The LicenceEnterprise primary key needed to be set first otherwise setting LD_LicenseType will fail!
			licenceDatabase.LD_ServerCode = serverCode;
			licenceDatabase.LD_LicenceType = "TST";
			licenceDatabase.LD_ReleaseRing = "ALP";
			licenceDatabase.LD_DBServerSecurityMode = "LCK";
			licenceDatabase.LD_HostedLocation = "NCW";
			licenceDatabase.LD_Product = "CW1";
			licenceDatabase.LD_CanReregisterToSameServer = true;
			licenceDatabase.LD_GS_NKOwner = owner;

			factory.Save();
		}

		public static void Unregister(ZGuid licenceDatabasePk)
		{
			var factory = new BusinessObjectFactory();

			var licenceDatabases = factory.Load<LicenceDatabase>(licenceDatabasePk);
			licenceDatabases.LD_Status = "NON";

			factory.Save();
		}

		#endregion

		#region User Created Companies

		public bool HasCompanyLicence()
		{
			if (IsEnterpriseFamilyDatabase)
			{
				var userCreates = SupportsUserCreatedCompanies(CurrentVersion);
				return userCreates != TriState.True;
			}
			else
			{
				return false;
			}
		}

		static public TriState SupportsUserCreatedCompanies(ReleaseBuild build)
		{
			TriState result;

			if (build == null)
			{
				result = TriState.NotDetermined;
			}
			else if (BuildIsSupported(build.VersionNumber, EDIDataRegistry.Instance.UserCreatedCompanyReleaseBuilds.Value))
			{
				result = TriState.True;
			}
			else
			{
				result = TriState.False;
			}

			return result;
		}

		#endregion

		#region Parent Database

		[ActionFieldFollow(false)]
		public LicenceDatabase ParentDatabase
		{
			get { return Factory.Load<LicenceDatabase>(LD_LD_ParentDatabase); }
		}

		[List("Lookups.ProductionDatabaseCodeDescriptionPairList")]
		public ZString ProductionDatabaseServerCode
		{
			get { return productionDatabaseServerCode; }
			set
			{
				SetNonPersistentPropertyValue(ProductionDatabaseServerCodeInfo, ref productionDatabaseServerCode, value);
				var selectedDb = LicEnterprise.Databases.Cast<LicenceDatabase>().FirstOrDefault(x => x.LD_ServerCode == productionDatabaseServerCode);
				if (selectedDb != null)
				{
					LD_LD_ParentDatabase = selectedDb.PK;
				}
				else
				{
					LD_LD_ParentDatabase = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateProductionDatabaseServerCode();
				}
			}
		}
		ZString productionDatabaseServerCode;

		public ZPropertyInfo ProductionDatabaseServerCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ProductionDatabaseServerCode)); }
		}

		#endregion

		#region BillingModel

		[List("Lookups.BillingModels")]
		public ZString BillingModel
		{
			get
			{
				return billingModel ?? (billingModel = CalculateBillingModel());
			}
		}
		string billingModel;

		public ZPropertyInfo BillingModelInfo
		{
			get { return GetZPropertyInfo(nameof(BillingModel)); }
		}

		public ZString BillingModelDesc
		{
			get
			{
				return BillingConstants.BillingModel.BillingModelList.GetDescriptionFromCode(BillingModel);
			}
		}

		public ZPropertyInfo BillingModelDescInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(BillingModelDesc), x => BillingModelInfo); }
		}

		public void InvalidateBillingModel()
		{
			billingModel = null;
			BillingModelInfo.RefreshBinding();
		}

		public ZDateTime FutureStlBillingModelDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (BillingModel == BillingConstants.BillingModel.ODM)
				{
					var link = PriceHeaderLinks.Where(x => x.PHL_ValidFrom > ZDateTime.Today).OrderBy(x => x.PHL_ValidFrom).FirstOrDefault();
					if (link != null)
					{
						result = link.PHL_ValidFrom;
					}
				}
				return result;
			}
		}

		string CalculateBillingModel()
		{
			string result = IsEnterpriseFamilyDatabase ? BillingConstants.BillingModel.ODM : "";
			var priceLink = PriceHeaderLinkForDate(ZDateTime.Today);
			if (priceLink != null)
			{
				result = BillingConstants.BillingModel.STL;
			}
			else
			{
				if (!LD_LD_ParentDatabase.IsEmpty)
				{
					var parent = ParentDatabase;
					if (parent != null)
					{
						result = parent.BillingModel;
					}
				}
			}

			return result;
		}

		#endregion

		#region Save

		public override void OnSaving()
		{
			SetDatabaseNumberIfRequired();
			ResetAllModules();
			ResetPasswordIfRequired();
			RunCommissionAgreementCustomizationAutoAddIfRequired();
			SetDefaultPriceCurrency();
			SetTechnicalContactNotificationGroup();
			DisableAutoLogin();
			DisableUserAccount();
			SetDefaultMasterOrg();
			LogChanges();
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					LD_DatabaseNumber = 0;
				}
				else
				{
					LD_PasswordInfo.Value = LD_PasswordInfo.OriginalValue;
				}

				RunRollbackActions();
			}
			else
			{
				hasUpdatedFeatureSet = false;
			}
		}

		void ResetPasswordIfRequired()
		{
			if (LD_StatusInfo.HasChanges && ((ZString)LD_StatusInfo.OriginalValue) == DatabaseStatusList.Codes.REG)
			{
				if (SupportsUserCreatedCompanies(CurrentVersion) == TriState.False)
				{
					LD_Password = "";
				}
				else
				{
					LD_Password = "-";
				}
			}
		}

		void ResetAllModules()
		{
			if ((LD_ProductInfo.HasChanges || LD_HL_CurrentRunningVersionInfo.HasChanges) &&
				!HasCompanyLicence())
			{
				bool needReset = false;
				if (LD_ProductInfo.HasChanges && ((ZString)LD_ProductInfo.OriginalValue) == ProductTypes.Codes.Enterprise)
				{
					needReset = true;
				}
				else if (LD_HL_CurrentRunningVersionInfo.HasChanges)
				{
					var originalBuild = Factory.Load<ReleaseBuild>(((ZGuid)LD_HL_CurrentRunningVersionInfo.OriginalValue));
					if (LicenceDatabase.SupportsUserCreatedCompanies(originalBuild) != TriState.True
						&& (LicenceDatabase.SupportsUserCreatedCompanies(CurrentVersion) == TriState.True))
					{
						needReset = true;
					}
				}

				if (needReset)
				{
					RemoveAllModules(false);
				}
			}
		}

		public int RemoveAllModules(bool includingLicencesWithSeats = true)
		{
			int modulesRemoved = 0;
			foreach (LicenceHeader licHeader in LicHeadersForAllCompanies)
			{
				if (includingLicencesWithSeats || !licHeader.HasPurchasedModules)
				{
					modulesRemoved += licHeader.Modules.Count;
					licHeader.Modules.RemoveAndDeleteAll();
				}
			}
			return modulesRemoved;
		}

		void RunCommissionAgreementCustomizationAutoAddIfRequired()
		{
			if (!IsInDatabase)
			{
				new EdiCommissionAgreementCustomizationAutoAdder(Factory).Execute(this);
			}
		}

		void SetDefaultPriceCurrency()
		{
			if (LD_IsBilledPerCompany && LD_IsBilledPerCompanyInfo.HasChanges)
			{
				var headers = ActiveLicHeadersForAllCompanies.OfType<LicenceHeader>().ToArray();

				var defaultCurrency = headers.FirstOrDefault(x => !x.LA_RX_NKPriceCurrency.IsEmpty)?.LA_RX_NKPriceCurrency ?? ZString.Empty;

				if (!defaultCurrency.IsEmpty)
				{
					foreach (var header in headers.Where(x => x.LA_RX_NKPriceCurrency.IsEmpty))
					{
						header.LA_RX_NKPriceCurrency = defaultCurrency;
					}
				}
			}
		}

		#region TechnicalContactNotificationGroup

		void SetTechnicalContactNotificationGroup()
		{
			if (ContractInstallerOrInternalTechContact is EDIOrgContact contact && !contact.IsInformationServicesTechnicalAdministrator)
			{
				contact.IsInformationServicesTechnicalAdministrator = true;
				RollbackActions.Push(() => { contact.IsInformationServicesTechnicalAdministrator = false; });
			}
		}

		void RunRollbackActions()
		{
			RollbackActions.ForEach(x => x());
			RollbackActions.Clear();
		}

		readonly Stack<Action> RollbackActions = new Stack<Action>();

		#endregion

		public void SetDatabaseNumberIfRequired()
		{
			if (!IsInDatabase && LD_DatabaseNumber == 0)
			{
				var fountain = Modules.ClientNumberFountainRegistration.GetInstance().LicenceDatabaseNumber;
				LD_DatabaseNumber = (int)fountain.GetNext(Factory);
			}
		}

		void DisableAutoLogin()
		{
			if (!IsInDatabase && (LicEnterprise?.LE_IsInternal ?? false))
			{
				LD_AllowAutoLogin = false;
			}
		}

		void SetDefaultMasterOrg()
		{
			var enterprise = LicEnterprise;
			if (LD_OH_WebAccessOrg.IsEmpty && (enterprise?.LE_IsInternal ?? false))
			{
				var defaultOrgCode = EDIDataRegistry.Instance.InternalEnterpriseMasterOrgs.Value.GetDescriptionFromCode(enterprise.LE_EnterpriseCode);
				if (!string.IsNullOrEmpty(defaultOrgCode))
				{
					var defaultOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, defaultOrgCode);
					if (defaultOrg != null)
					{
						LD_OH_WebAccessOrg = defaultOrg.PK;
					}
				}
			}
		}

		void DisableUserAccount()
		{
			if (!LD_IsActive)
			{
				var queryUserAccount = new ZQuery();
				queryUserAccount.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, PK);
				queryUserAccount.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsActive, true);

				var webAccessContacts = new List<OrgContact>();
				var userAccountsToDisable = Factory.Load<EdiCustomerUserAccount>(queryUserAccount);

				foreach (var item in userAccountsToDisable)
				{
					item.EUA_IsActive = false;

					if (item.WebAccessContact != null)
					{
						webAccessContacts.Add(item.WebAccessContact);
					}
				}

				if (webAccessContacts.Any())
				{
					var queryRelatedContacts = new ZQuery();
					queryRelatedContacts.AddToFilter(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, webAccessContacts.Select(w => w.PK).ToArray());
					queryRelatedContacts.AddToFilter(EdiCustomerUserAccountSchema.EUA_IsActive, true);

					var userAccountDisabledPKs = userAccountsToDisable.Select(u => u.PK);
					var userAccountWithRelatedContacts = Factory.Load<EdiCustomerUserAccount>(queryRelatedContacts);

					foreach (var contact in webAccessContacts)
					{
						var userAccountsOfContact = userAccountWithRelatedContacts.Where(c => c.EUA_OC_WebAccessContact == contact.PK).ToList();
						if (userAccountsOfContact.Count == 0 || (userAccountsOfContact.Count == 1 && userAccountDisabledPKs.Contains(userAccountsOfContact[0].PK)))
						{
							contact.OC_IsActive = false;
						}
					}
				}
			}
		}

		void LogChanges()
		{
			if (IsInDatabase && LD_OH_WebAccessOrgInfo.HasChanges)
			{
				var originalWebAccessOrgPK = (ZGuid)LD_OH_WebAccessOrgInfo.OriginalValue;
				var originalWebAccessOrgCode = originalWebAccessOrgPK.IsEmpty ? ZString.Empty : Factory.Load<OrgHeader>(originalWebAccessOrgPK)?.OH_Code ?? ZString.Empty;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(AutoEvents.EditedARecord, $"{nameof(LD_OH_WebAccessOrg)} changed from {originalWebAccessOrgCode},{LD_OH_WebAccessOrgInfo.OriginalValue} to {WebAccessOrg?.OH_Code},{LD_OH_WebAccessOrg}");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			if (new[] { LD_ServerCodeInfo, LD_DBServerSecurityModeInfo, LD_LicenceTypeInfo,
										LD_AvailableUpgradeMethodInfo, LD_ReleaseRingInfo, LD_HL_CurrentRunningVersionInfo }.Any(x => x.HasChanges))
			{
				ZString log = string.Format(CultureInfo.CurrentCulture, "Server {0}{1}", LD_ServerCode, ((LD_ServerCodeInfo.HasChanges && !LD_ServerCodeInfo.OriginalValue.IsEmpty) ? "(" + LD_ServerCodeInfo.OriginalValue + ") -" : " -"));
				log = LogHelper.AddChangeLog(log, "DB Security Mode", LD_DBServerSecurityModeInfo);
				log = LogHelper.AddChangeLog(log, "Licence Type", LD_LicenceTypeInfo);
				log = LogHelper.AddChangeLog(log, "Upgrade Method", LD_AvailableUpgradeMethodInfo);
				log = LogHelper.AddChangeLog(log, "Ring", LD_ReleaseRingInfo);
				if (LD_HL_CurrentRunningVersionInfo.HasChanges)
				{
					log = LogHelper.AddChangeLog(log, "Version", VersionText(LD_HL_CurrentRunningVersion), VersionText((ZGuid)LD_HL_CurrentRunningVersionInfo.OriginalValue));
				}

				log = log.EndsWith(" -", StringComparison.Ordinal) ? log.Left(log.Length - 2) : log;
				Logs.AddNew(IsInDatabase ? AutoEvents.EditedARecord : AutoEvents.AddedARecordToTheSystem, log);
			}
		}

		#endregion

		#region Changing

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (value && !IsSettingHasChangesSuspended)
				{
					foreach (var licHeader in Factory.Load<LicenceHeader>(new ZQuery(LicenceHeaderSchema.LA_LD, PK)))
					{
						var company = Factory.Load<LicenceCompany>(licHeader.LA_LC);
						var header = company != null ? Factory.Load<EDIOrgHeader>(company.LC_OH) : null;
						header?.MarkForSaving();
					}
				}
			}
		}

		#endregion

		#region Clone

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new string[]
			{
				Schema.LD_DatabaseNumber,

				// Cloning a registered database doesn't produce another registered database
				Schema.LD_Status
			};
		}

		#endregion

		public ZDateTime PreRegistrationExpiryDateLocal => LD_PreRegistrationExpiryDateUTC.ToLocalBranchTime(Factory);

		public ZString Description => FormattableString.Invariant($"{Lookups.ProductTypeList.GetDescriptionFromCode(LD_Product)} {LD_DatabaseNumber.ToString()}");

		public ZString ProductCodeDescription => FormattableString.Invariant($"{Lookups.ProductTypeList.GetDescriptionFromCode(LD_Product)} ({LD_Product})");

		public ZString EnterpriseIDWithoutDefault
		{
			get
			{
				var result = EnterpriseID;
				if (!string.IsNullOrEmpty(result) && result == EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value)
				{
					result = string.Empty;
				}

				return result;
			}
		}

		public void SetEnterpriseServerCode()
		{
			var org = EDIWebAccessOrg;
			if (org != null)
			{
				if (org.LicEnterprise == null || org.LicCompany == null)
				{
					org.CreateAndLoadLicenceForOrg();
				}

				if (EDIDataRegistry.Instance.ProductsRequiringEnterpriseCode.Value.Contains(LD_Product.ToString()) &&
					string.IsNullOrEmpty(org.LicenceEnterpriseCode))
				{
					org.GenerateNewLicenceCode();
				}

				if (!org.LicCompany.LicDatabases.Any(a => a.PK == PK))
				{
					org.LicCompany.LicDatabases.Add(this);
				}

				LD_LE = org.LicEnterprise.PK;
				LicEnterprise.LE_EnterpriseCode = org.LicEnterprise.LE_EnterpriseCode;
			}
		}

		public bool AllowEditWebAccessOrg { get; set; }

		#region Contact Cloner

		ContactCloner contactCloner;

		public void CloneContacts(bool runContactCloner, IProcessStatus processStatus = null)
		{
			contactCloner = null;
			if (runContactCloner)
			{
				contactCloner = new ContactCloner(Factory, processStatus);
				var originalWebAccessOrg = Factory.Load<OrgHeader>((ZGuid)LD_OH_WebAccessOrgInfo.OriginalValue);
				contactCloner.CloneContacts(originalWebAccessOrg, WebAccessOrg, this);
			}
		}

		public string MergeContactsPerson()
		{
			var errorMessage = string.Empty;
			if (contactCloner != null)
			{
				contactCloner.MergeContactsPerson();
				if (contactCloner.HasPersonMergeFailure)
				{
					var messageBuilder = new ZStringBuilder();
					messageBuilder.AppendLine("The following person(s) failed to merge. Please merge via Maintain -> Master Data -> MDM Administration.");
					messageBuilder.AppendLine();
					messageBuilder.Append(contactCloner.GetPersonMergeFailureMessage());
					errorMessage = messageBuilder.ToString();
				}
				contactCloner = null;
			}
			return errorMessage;
		}

		#endregion

		#region Org Suggestion

		public EdiLicenceDatabaseOrgSuggestionCollection OrgSuggestionCollections
		{
			get
			{
				if (orgSuggestionCollections == null)
				{
					orgSuggestionCollections = new EdiLicenceDatabaseOrgSuggestionCollection(this);
				}

				return orgSuggestionCollections;
			}
		}

		EdiLicenceDatabaseOrgSuggestionCollection orgSuggestionCollections;

		public EdiLicenceDatabaseOrgSuggestion TopOrgSuggestion => OrgSuggestionCollections.OrderByDescending(x => x.LDS_TotalScore).FirstOrDefault();

		#endregion
	}
}
