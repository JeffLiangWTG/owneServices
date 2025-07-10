using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[CodeProperty(LicenceHeader.Schema.LicenceCode), DescriptionProperty(LicenceHeader.Schema.AddressAsString)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class LicenceHeader : AutoLicenceHeader
	{
		#region Schema

		public new abstract class Schema : AutoLicenceHeader.Schema
		{
			public const string SendUpgrade = "SendUpgrade";
			public const string DatabaseCode = "DatabaseCode";
			public const string CompanyCode = "CompanyCode";
			public const string LicenceCode = "LicenceCode";
			public const string AddressAsString = "AddressAsString";
			public const string OrganisationCode = "OrganisationCode";
			public const string OrganisationFullName = "OrganisationFullName";
			public const string LA_LastDiscrepancyChange = "LA_LastDiscrepancyChange";
		}

		#endregion

		public LicenceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public static LicenceHeader LoadFromLicenceCode(BusinessObjectFactory factory, string licenceCode)
		{
			LicenceHeader result = null;
			if (licenceCode.Length == 9)
			{
				LicenceHeaderCollection licenceHeaders = new LicenceHeaderCollection(factory);
				ZGuid licenceGuid = new LicenceHeaderFindBoxListProvider(licenceHeaders).PrimaryKeyFromCode(licenceCode);
				result = factory.Load<LicenceHeader>(licenceGuid);
			}
			return result;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			OriginalLA_SiteLiveDate = LA_SiteLiveDate;
		}

		ZDateTime OriginalLA_SiteLiveDate;

		#endregion

		#region Properties

		public override ZBool LA_IsActive
		{
			get => base.LA_IsActive;
			set
			{
				base.LA_IsActive = value;
				Database?.Lookups.RefreshWebAccessOrgs();
			}
		}

		public override ZDateTime LA_SiteLiveDate
		{
			get
			{
				return base.LA_SiteLiveDate;
			}

			set
			{
				base.LA_SiteLiveDate = value;
				if (LA_AgreedLiveDate.IsEmpty && value.IsValid)
				{
					LA_AgreedLiveDate = value;
				}
			}
		}

		public bool LA_LastLicenceSyncCheck_ReadOnly
		{
			get { return true; }
		}

		#region Licence Code

		public ZString LicenceCode
		{
			get
			{
				return (Company != null && Company.LicEnterprise != null && Database != null)
					? Company.LicEnterprise.LE_EnterpriseCode + Company.LC_CompanyCode + DatabaseCode
					: "";
			}
		}

		public ZPropertyInfo LicenceCodeInfo
		{
			get { return GetZPropertyInfo(nameof(LicenceCode)); }
		}

		#endregion

		#region Core Module User Count

		ZShort? coreModuleUserCount;

		public ZShort CoreModuleUserCount
		{
			get
			{
				if (!coreModuleUserCount.HasValue)
				{
					LicenceModules core = GetCoreModule();
					coreModuleUserCount = (core != null) ? core.LM_UserCount : ZShort.Zero;
				}
				return coreModuleUserCount.Value;
			}
		}

		public ZPropertyInfo CoreModuleUserCountInfo
		{
			get { return GetZPropertyInfo(nameof(CoreModuleUserCount)); }
		}

		public LicenceModules GetCoreModule()
		{
			if (coreModule == null)
			{
				BusinessObject[] modules = Modules.Find(new ZQuery(LicenceModulesSchema.LM_GroupModuleCode, LegacyLicence.Codes.Core));
				coreModule = (modules.Length == 1) ? (LicenceModules)modules[0] : null;
			}
			return coreModule;
		}
		LicenceModules coreModule;

		[ActionFieldFollow(false)]
		public LicenceModules CoreModule
		{
			get { return GetCoreModule(); }
		}

		#endregion

		#region DatabaseCode

		public ZString DatabaseCode
		{
			get { return Database.LD_ServerCode; }
		}

		public ZPropertyInfo DatabaseCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DatabaseCode); }
		}

		#endregion

		#region DatabaseIsActive

		public ZBool DatabaseIsActive => Database.LD_IsActive;

		public ZPropertyInfo DatabaseIsActiveInfo => GetZPropertyInfo(nameof(DatabaseIsActive));

		#endregion

		#region Database's Current Version's Exe Version

		public ZString DatabaseCurrentVersionExeVersion
		{
			get
			{
				LicenceDatabase database = Database;
				ReleaseBuild currentVersion;
				return ((database != null) && ((currentVersion = database.CurrentVersion) != null)) ? currentVersion.ExeVersion : ZString.Empty;
			}
		}

		public ZPropertyInfo DatabaseCurrentVersionExeVersionInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseCurrentVersionExeVersion)); }
		}

		#endregion

		#region Database's Current Version's Release

		public ZString DatabaseCurrentVersionRelease
		{
			get
			{
				LicenceDatabase database = Database;
				return (database != null) ? database.CurrentVersionRelease : ZString.Empty;
			}
		}

		public ZPropertyInfo DatabaseCurrentVersionReleaseInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseCurrentVersionRelease)); }
		}

		#endregion

		#region Database's Sent Version's Exe Version

		public ZString DatabaseSentVersionExeVersion
		{
			get
			{
				LicenceDatabase database = Database;
				ReleaseBuild sentVersion;
				return ((database != null) && ((sentVersion = database.SentVersion) != null)) ? sentVersion.ExeVersion : ZString.Empty;
			}
		}

		public ZPropertyInfo DatabaseSentVersionExeVersionInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseSentVersionExeVersion)); }
		}

		#endregion

		#region Database's Sent Version's Release

		public ZString DatabaseSentVersionRelease
		{
			get
			{
				LicenceDatabase database = Database;
				return (database != null) ? database.SentVersionRelease : ZString.Empty;
			}
		}

		public ZPropertyInfo DatabaseSentVersionReleaseInfo
		{
			get { return GetZPropertyInfo(nameof(DatabaseSentVersionRelease)); }
		}

		#endregion

		#region CompanyCode

		public ZString CompanyCode
		{
			get { return Company?.LC_CompanyCode ?? ZString.Empty; }
		}

		public ZPropertyInfo CompanyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CompanyCode); }
		}

		#endregion

		#region OrganisationCode

		public ZString OrganisationCode
		{
			get { return Company != null && Company.Header != null ? Company.Header.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo OrganisationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationCode); }
		}

		#endregion

		#region OrganisationFullName

		public ZString OrganisationFullName
		{
			get { return Company != null && Company.Header != null ? Company.Header.OH_FullName : ZString.Empty; }
		}

		public ZPropertyInfo OrganisationFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationFullName); }
		}

		#endregion

		#region AddressAsString

		public ZString AddressAsString
		{
			get { return Database != null ? Database.AddressAsString : ZString.Empty; }
		}

		public ZPropertyInfo AddressAsStringInfo
		{
			get { return GetZPropertyInfo(Schema.AddressAsString); }
		}

		#endregion

		public bool Is24HrSupport
		{
			get { return LA_SupportMode == LicenceHeaderLookups.SupportModeConstants.Codes.Hour24; }
		}

		[List("Lookups.ActiveEditions")]
		[MaxLength(3)]
		public ZString Edition
		{
			get
			{
				if (LA_LicenceAdvStdOth != LicenceAdvStdOthList.Codes.SeatTransaction)
				{
					var db = Database;
					if (db != null && db.BillingModel == BillingConstants.BillingModel.STL)
					{
						return LicenceAdvStdOthList.Codes.SeatTransaction;
					}
				}

				return LA_LicenceAdvStdOth;
			}
			set
			{
				LA_LicenceAdvStdOth = value;
			}
		}

		public ZPropertyInfo EditionInfo
		{
			get { return GetZPropertyInfo(nameof(Edition)); }
		}

		public bool Edition_ReadOnly
		{
			get
			{
				var db = Database;
				return db != null && db.BillingModel == BillingConstants.BillingModel.STL;
			}
		}

		[List("Lookups.ActiveEditions")]
		public override ZString LA_LicenceAdvStdOth
		{
			get { return base.LA_LicenceAdvStdOth; }
			set
			{
				if (base.LA_LicenceAdvStdOth != value)
				{
					base.LA_LicenceAdvStdOth = value;
					if (Database != null && (Database.IsEnterpriseFamilyDatabase))
					{
						Modules.UpdateForChangedEdition();
					}
					EditionInfo.RefreshBinding();
				}
			}
		}

		public ZBool IsLive
		{
			get { return LA_IsActive && LA_SiteLiveDate <= ZDateTime.Now; }
		}

		public bool IsOnDemandModuleTypeAllowed
		{
			get
			{
				return IsPureOnDemand
					|| LA_LicenceAdvStdOth == LicenceAdvStdOthList.Codes.Hybrid
					|| LA_LicenceAdvStdOth == LicenceAdvStdOthList.Codes.SeatTransaction
					|| LA_LicenceAdvStdOth == LicenceAdvStdOthList.Codes.OnDemand;
			}
		}

		public bool IsPureOnDemand
		{
			get { return CalcIsPureOnDemand(LA_LicenceAdvStdOth); }
		}

		public static bool CalcIsPureOnDemand(string licenceCode)
		{
			switch (licenceCode)
			{
				case LicenceAdvStdOthList.Codes.OnDemand:
				case LicenceAdvStdOthList.Codes.ConversionToODPL:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// V1 Licencing supported the module licence types: NON, PUR, REN, TRI
		/// V2 Licencing (old On Demand licencing) added a header level "OnDemand" flag attribute to the licence key.
		///		If the module licence type was NON AND the OnDemand flag was true then the module was licenced.
		///		If the module licence type was other than NON then the usual licencing logic applied for user limits and expiry dates.
		///	V3 Licencing (new On Demand licencing) added the module types: ODM, OTM, OPN, SRU
		///	V4 Licencing added the module type: CPT
		/// </summary>

		bool IsOldOnDemandLicence
		{
			get
			{
				return IsOnDemandModuleTypeAllowed
					&& GetCoreModule().LM_LicenceType == LicenceTypes.Codes.NON
					&& SupportsLineLevelOnDemandLicenceTypes != TriState.True;
			}
		}

		public TriState SupportsLineLevelOnDemandLicenceTypes
		{
			get
			{
				if (Database == null || Database.CurrentVersion == null)
				{
					return TriState.NotDetermined;
				}
				else if (Database.CurrentVersion.VersionNumber >= LineLevelOnDemandLicenceVersion)
				{
					return TriState.True;
				}
				else
				{
					return TriState.False;
				}
			}
		}

		// This version number corresponds to the branch started on 9 Mar 09
		public static readonly VersionNumber LineLevelOnDemandLicenceVersion = new VersionNumber(1, 3, 3375, 0);

		public bool SupportsOpenLicence
		{
			get
			{
				return Database != null
					&& Database.CurrentVersion != null
					&& Database.CurrentVersion.VersionNumber >= OpenLicenceVersion;
			}
		}

		// 7 Apr 2010
		public static readonly VersionNumber OpenLicenceVersion = new VersionNumber(1, 4, 3749, 0);

		public bool IsCargoWiseInstallation
		{
			get { return EDIDataRegistry.Instance.InternalIncidentLicenceSettings.Value.LicenceEnterpriseKeys.ContainsLicenceEnterprise(Company.LC_LE); }
		}

		#region LA_AMS_USMode

		[List("Lookups.AMSModeList")]
		public override ZString LA_AMS_USMode
		{
			get { return base.LA_AMS_USMode; }
			set
			{
				base.LA_AMS_USMode = value;
			}
		}

		#endregion

		#region LA_SupportMode

		[List("Lookups.SupportModeList")]
		public override ZString LA_SupportMode
		{
			get { return base.LA_SupportMode; }
			set
			{
				base.LA_SupportMode = value;
			}
		}

		#endregion

		#region Product

		public ZString Product
		{
			get { var db = Database; return db != null ? db.LD_Product : ZString.Empty; }
		}

		#endregion

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZDateTime LA_LastFaxReport { get => base.LA_LastFaxReport; set => base.LA_LastFaxReport = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZBool LA_LastLicenceCheckInSync { get => base.LA_LastLicenceCheckInSync; set => base.LA_LastLicenceCheckInSync = value; }

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZDateTime LA_LastLicenceSyncCheck { get => base.LA_LastLicenceSyncCheck; set => base.LA_LastLicenceSyncCheck = value; }

		#endregion

		#region Related Business Objects

		#region ClientCompany

		[ActionFieldFollow(false)]
		public ClientCompany ClientCompany
		{
			get
			{
				if (clientCompany == null || (!IsDeleted && clientCompany.IsDeleted))
				{
					var query = new ZQuery(ClientCompanySchema.LCC_OH, Company.LC_OH);
					query.AddToFilter(ClientCompanySchema.LCC_LD, LA_LD);
					clientCompany = Factory.Load<ClientCompany>(query)
						.OrderBy(x => x.LCC_DeactivateTimeUtc.IsEmpty ? 0 : 1)
						.ThenByDescending(x => x.LCC_CreateTimeUtc)
						.FirstOrDefault();
				}
				return clientCompany;
			}
		}

		ClientCompany clientCompany;

		#endregion

		#region Licence Company

		[RelatedBusinessObject("Company")]
		public override ZGuid LA_LC
		{
			get { return base.LA_LC; }
			set
			{
				if (LA_LC.IsValid && value != LA_LC)
				{
					MarkOrgForSaving();
				}
				base.LA_LC = value;
			}
		}

		[ActionFieldFollow(false)]
		public LicenceCompany Company
		{
			get
			{
				if (fCompany == null)
				{
					fCompany = Factory.Load<LicenceCompany>(LA_LC);
				}

				return fCompany;
			}
		}

		LicenceCompany fCompany;

		#endregion

		#region Licence Database

		[RelatedBusinessObject("Database")]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid LA_LD
		{
			get { return base.LA_LD; }
			set
			{
				base.LA_LD = value;
				if (Database?.LD_Product.Equals(ProductTypes.Codes.BorderWise) ?? false)
				{
					LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
				}
			}
		}

		[ActionFieldFollow(false)]
		public LicenceDatabase Database => Factory.Load<LicenceDatabase>(LA_LD);

		#endregion

		#region Licence Modules

		[ChildEditable(false)]
		[ActionFieldFollow(false)]
		public LicenceModulesDependentCollection Modules
		{
			get
			{
				if (fModules == null)
				{
					fModules = new LicenceModulesDependentCollection(this, Factory);
					fModules.Load();

					RegisterEditableChildObject(fModules);

					var db = Database;
					if (db != null)
					{
						fModules.MergeWithDefaultModules(!db.HasCompanyLicence());
					}
				}

				return fModules;
			}
		}
		LicenceModulesDependentCollection fModules;

		[ActionFieldFollow(false)]
		public LicenceModulesView FilteredModules
		{
			get { return filteredModules ?? (filteredModules = new LicenceModulesView(Modules)); }
		}

		LicenceModulesView filteredModules;

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public ZBool HideUnlicenced
		{
			get { return FilteredModules.HideUnlicenced; }
			set { FilteredModules.HideUnlicenced = value; }
		}

		public ZString CalcIndentedGroupModuleDescription(string moduleCode)
		{
			return LicenceModuleList.Instance.GetIndentedDescriptionFromCode(moduleCode);
		}

		[ActionFieldFollow(false)]
		public LicenceModulesDependentCollection PurchasedModules
		{
			get
			{
				if (purchasedModules == null)
				{
					var query = new ZQuery(LicenceModulesSchema.LM_UserCount, SQLComparisonOperator.GreaterThan, (short)0);
					purchasedModules = new LicenceModulesDependentCollection(this, query);
					purchasedModules.Load();
				}
				return purchasedModules;
			}
		}

		LicenceModulesDependentCollection purchasedModules;

		public bool HasPurchasedModules
		{
			get
			{
				return Modules.Cast<LicenceModules>().Any(x => x.LM_UserCount > 0);
			}
		}

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			IsBeingDeleted = true;
			try
			{
				ClearForeignKeysToLicence();
				Modules.RemoveAndDeleteAll();
				var readonlyBilling = ReadonlyBilling;
				if (readonlyBilling != null)
				{
					readonlyBilling.Delete();
				}
				HasChanges = false;
				base.Delete();
			}
			finally
			{
				IsBeingDeleted = false;
			}
		}
		bool IsBeingDeleted;

		void ClearForeignKeysToLicence()
		{
			RemoveForeignKeyFromRelatedObjects<ClientWorkProject>(ClientWorkProjectSchema.CWP_LA);
			RemoveForeignKeyFromRelatedIncidentMains<SupportIncident>(IncidentConstants.IncidentType.SupportIncident);
		}

		void RemoveForeignKeyFromRelatedIncidentMains<T>(string incidentMainType)
			where T : IncidentMainBase
		{
			RemoveForeignKeyFromRelatedObjects<T>(IncidentMainSchema.IM_LA, new ZQuery(IncidentMainSchema.IM_IncidentType, incidentMainType));
		}

		void RemoveForeignKeyFromRelatedObjects<T>(SchemaGuidColumn foreignKeyColumn)
			where T : BusinessObject
		{
			RemoveForeignKeyFromRelatedObjects<T>(foreignKeyColumn, null);
		}

		void RemoveForeignKeyFromRelatedObjects<T>(SchemaGuidColumn foreignKeyColumn, ZQuery additionalFilter)
		   where T : BusinessObject
		{
			ZQuery query = new ZQuery(foreignKeyColumn, PK);
			if (additionalFilter != null)
			{
				query.AddToFilter(additionalFilter);
			}
			foreach (T bizO in Factory.Load<T>(query))
			{
				bizO[foreignKeyColumn] = ZGuid.Empty;
			}
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			if (IsInDatabase)
			{
				EmailOverallRepIfDatesChanged();
				EmailTrainingAndImplementationSchedulingIfInstallationDateChanged();
			}

			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
#if DEBUG
			RemindersCreatedInOnSavedForTesting.Clear();
#endif

			if (saveSucceeded)
			{
				if (LA_SiteLiveDate != OriginalLA_SiteLiveDate)
				{
					if (LA_SiteLiveDate.IsEmpty)
					{
						Reminder cancelReminder = GetCancelReminder();
						cancelReminder.CreateAppointment();
#if DEBUG
						RemindersCreatedInOnSavedForTesting.Add(cancelReminder);
#endif
					}
					else
					{
						Reminder sendReminder = GetSendReminder();
						if (sendReminder != null)
						{
							sendReminder.CreateAppointment();
#if DEBUG
							RemindersCreatedInOnSavedForTesting.Add(sendReminder);
#endif
						}
					}

					OriginalLA_SiteLiveDate = LA_SiteLiveDate;
				}
			}

			base.OnSaved(saveSucceeded);
		}

#if DEBUG
		public readonly List<Reminder> RemindersCreatedInOnSavedForTesting = new List<Reminder>();
#endif

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState =>
			DisableAutoLog ? AutologState.NotLogged : AutologState.AutoLogged;

		public bool DisableAutoLog { get; set; }

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				var log = ZString.Empty;

				if (!IsBeingDeleted)
				{
					log = "Server " + (Database != null && !Database.IsDeleted ? Database.LD_ServerCode : ZString.Empty) + ':';
					log = LogHelper.AddChangeLog(log, "Support Mode", LA_SupportModeInfo);
					log = LogHelper.AddShortDateChangeLog(log, "Installation Complete", LA_InstallationCompleteDateInfo);
					log = LogHelper.AddShortDateChangeLog(log, "Agreed Live", LA_AgreedLiveDateInfo);
					log = LogHelper.AddShortDateChangeLog(log, "Estimated Live", LA_EstimatedLiveDateInfo);
					log = LogHelper.AddShortDateChangeLog(log, "Site Live", LA_SiteLiveDateInfo);
					log = LogHelper.AddShortDateChangeLog(log, "Contract End", LA_ContractExpiryDateInfo);
					log = LogHelper.AddShortDateChangeLog(log, "Support Start", LA_SupportStartDateInfo);
				}

				return log.EndsWith(":", StringComparison.Ordinal) ? log.Left(log.Length - 1).TrimEnd() : log;
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LA_SupportMode = LicenceHeaderLookups.SupportModeConstants.Codes.Standard;
			LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Advanced;
		}

		#endregion

		#region Licence Key

		public string GenerateLicenceKey()
		{
			string result = "";

			LegacyLicence licenceGenerator = new LegacyLicence();
			bool isOldOnDemandLicence = IsOldOnDemandLicence;
			if (!isOldOnDemandLicence)
			{
				licenceGenerator.Type = LA_LicenceAdvStdOth;
			}
			licenceGenerator.SupportMode = Lookups.SupportModeList.GetDescriptionFromCode(LA_SupportMode);
			bool isCargoWise = EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetBoolFromCode(Database.LD_HostedLocation);
			licenceGenerator.ObsoleteHostedLocation = isCargoWise ? Database.LD_HostedLocation : ZString.Empty;

			var org = Company.Header;
			var webAddress = org.MainWebURL.PU_URL;
			licenceGenerator.Company.Set(org.PK.ToGuid(),
				org.LicenceEnterpriseCode,
				Database.LD_ServerCode,
				Company.LC_CompanyCode,
				org.OH_FullName,
				org.MainAddress.OA_Address1,
				org.MainAddress.OA_Address2,
				org.MainAddress.OA_City,
				org.MainAddress.OA_PostCode,
				org.MainAddress.OA_State,
				org.UNLOCO.Country.PK.ToGuid(),
				org.LicenceTaxationRegNo,
				org.LicenceBusinessRegNo,
				Company.LC_RX_NKCurrency,
				RefCurrency.LoadFromCurrencyCode(Factory, Company.LC_RX_NKCurrency).PK.ToGuid(),
				Company.LC_IsReciprocal,
				Company.LC_IsGSTRegistered,
				Company.LC_IsGSTCashBasis,
				Company.LC_IsWHTRegistered,
				Company.LC_IsWHTCashBasis,
				org.MainAddress.OA_Phone,
				org.MainAddress.OA_Fax,
				org.MainAddress.OA_Email,
				webAddress);

			foreach (LicenceModules module in Modules)
			{
				var checkPoint = licenceGenerator.GetCheckpointFromCode(module.LM_GroupModuleCode);

				if (checkPoint != null)
				{
					checkPoint.UserLimit = module.LM_UserCount;
					checkPoint.LicenceType = module.LM_LicenceType;

					if (module.LM_ExpiryDate.IsEmpty)
					{
						checkPoint.ExpiryDate = System.DateTime.MinValue;
					}
					else
					{
						checkPoint.ExpiryDate = module.LM_ExpiryDate.ToDateTime();
					}
				}
			}

			foreach (OrgAddress address in org.Addresses)
			{
				if (address.OA_IsActive && address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office))
				{
					ZString branchCode = address.OA_RL_NKRelatedPortCode.SubstringSafe(2, 3).ToUpper().PadRight(3, ' ');
					if (!branchCode.IsEmpty)
					{
						var branchName = !address.OA_CompanyNameOverride.IsEmpty ? address.OA_CompanyNameOverride : org.OH_FullName;
						var branch = new LicenceBranch();
						branch.Set(branchCode, branchName, address.OA_Address1, address.OA_Address2, address.OA_City, address.OA_State, address.OA_PostCode, address.OA_Phone, address.OA_Fax, address.OA_Email, webAddress, address.OA_RL_NKRelatedPortCode);
						licenceGenerator.Branches.Add(branch);
					}
				}
			}

			licenceGenerator.InstallationDetails.Set(LA_AMS_USMode);
			if (isOldOnDemandLicence)
			{
				((IExposeDeprecatedOnDemandModeFlag)licenceGenerator.InstallationDetails).OnDemandMode = true;
			}
			result = licenceGenerator.ToEncryptedKeyString();

			return result;
		}

		#endregion

		#region Date Change Notification

		void EmailOverallRepIfDatesChanged()
		{
			GlbStaff overallRep = Company.Header.StaffAssignments.OverallSalesRepStaff;

			if (overallRep != null && !overallRep.GS_EmailAddress.IsEmpty)
			{
				if (LA_AgreedLiveDateInfo.HasChanges ||
					LA_InstallationCompleteDateInfo.HasChanges ||
					LA_EstimatedLiveDateInfo.HasChanges ||
					LA_SiteLiveDateInfo.HasChanges ||
					LA_SupportStartDateInfo.HasChanges)
				{
					EmailDef email = new EmailDef(Env.CurrentUser.PK);
					email.FromDisplayName = "CargoWise Production System";
					email.AddRecipientForUserCommunication(overallRep.GS_EmailAddress);
					email.Subject = "Company " + Company.Header.OH_FullNameTruncated + " implementation dates modified.";

					string body = "Implementation Dates were changed for the below Organisation." + System.Environment.NewLine + System.Environment.NewLine;

					body += CompanyDetailsBody;
					body += System.Environment.NewLine + System.Environment.NewLine + DateChangedBody;
					body += System.Environment.NewLine + System.Environment.NewLine + ModuleListingBody + System.Environment.NewLine + System.Environment.NewLine;

					body += "These changes were made by " + Env.CurrentUser.FullName + ".";

					email.Body = body;

					Env.OutgoingMailManager.CreateAndSave(email);
				}
			}
		}

		void EmailTrainingAndImplementationSchedulingIfInstallationDateChanged()
		{
			if (LA_InstallationCompleteDateInfo.HasChanges)
			{
				EmailDef email = new EmailDef(Env.CurrentUser.PK);
				email.FromDisplayName = "CargoWise Production System";
				email.AddRecipientForUserCommunication(EDIDataRegistry.Instance.TrainingSchedulingEmailAddress.Value);
				email.AddRecipientForUserCommunication(EDIDataRegistry.Instance.ImplementationSchedulingEmailAddress.Value);
				email.Subject = "Company " + Company.Header.OH_FullNameTruncated + " installation date modified.";

				string body = "Installation Dates were changed for the below Organisation." + System.Environment.NewLine + System.Environment.NewLine;

				body += CompanyDetailsBody;
				body += System.Environment.NewLine + "\tInstallation Completion Date:\t\t" + LA_InstallationCompleteDate.ToShortDateString() + "  (was " + ((ZDateTime)LA_InstallationCompleteDateInfo.OriginalValue).ToShortDateString() + ")" + System.Environment.NewLine + System.Environment.NewLine;

				body += "These changes were made by " + Env.CurrentUser.FullName + ".";

				email.Body = body;

				Env.OutgoingMailManager.CreateAndSave(email);
			}
		}

		string CompanyDetailsBody
		{
			get
			{
				string body = "Company Name:\t\t\t\t" + Company.Header.OH_FullNameTruncated + System.Environment.NewLine;
				body += "Enterprise / Company Code:\t\t" + Company.Header.LicenceEnterpriseCode + " / " + Company.LC_CompanyCode + System.Environment.NewLine;
				body += "Database Code:\t\t\t\t" + Database.LD_ServerCode + System.Environment.NewLine;
				body += "Global Database / Company ID:\t" + Database.DatabaseId + " / " + Company.CompanyId + System.Environment.NewLine;

				if (Database.SoftwareInstallAddressDetails != null)
				{
					body += "Software Install Address:\t\t" + Database.SoftwareInstallAddressDetails.OA_Address1 + ", " + Database.SoftwareInstallAddressDetails.OA_City + ", " + Database.SoftwareInstallAddressDetails.OA_State + System.Environment.NewLine;
				}

				if (Database.LicenseeAdminContact != null)
				{
					body += "Licensee Admin Contact:\t\t\t" + Database.LicenseeAdminContact.OC_ContactName + System.Environment.NewLine;
				}

				body += "Email Address for Updates:\t\t" + Database.LD_PublicEmailAddressForUpdate + System.Environment.NewLine;

				return body;
			}
		}

		string DateChangedBody
		{
			get
			{
				ZString body = "\tInstallation Complete Date:\t\t\t" + LA_InstallationCompleteDate.ToShortDateString() + "  (was " + ((ZDateTime)LA_InstallationCompleteDateInfo.OriginalValue).ToShortDateString() + ")" + (LA_InstallationCompleteDateInfo.HasChanges ? "  CHANGED" : "") + System.Environment.NewLine;
				body += "\tAgreed Live Date:\t\t" + LA_AgreedLiveDate.ToShortDateString() + "  (was " + ((ZDateTime)LA_AgreedLiveDateInfo.OriginalValue).ToShortDateString() + ")" + (LA_AgreedLiveDateInfo.HasChanges ? "  CHANGED" : "") + System.Environment.NewLine;
				body += "\tEstimated Live Date:\t\t\t" + LA_EstimatedLiveDate.ToShortDateString() + "  (was " + ((ZDateTime)LA_EstimatedLiveDateInfo.OriginalValue).ToShortDateString() + ")" + (LA_EstimatedLiveDateInfo.HasChanges ? "  CHANGED" : "") + System.Environment.NewLine;
				body += "\tSite Live Date:\t\t\t" + LA_SiteLiveDate.ToShortDateString() + "  (was " + ((ZDateTime)LA_SiteLiveDateInfo.OriginalValue).ToShortDateString() + ")" + (LA_SiteLiveDateInfo.HasChanges ? "  CHANGED" : "") + System.Environment.NewLine;
				body += "\tSupport Start Date:\t\t" + LA_SupportStartDate.ToShortDateString() + "  (was " + ((ZDateTime)LA_SupportStartDateInfo.OriginalValue).ToShortDateString() + ")" + (LA_SupportStartDateInfo.HasChanges ? "  CHANGED" : "") + System.Environment.NewLine;

				return body;
			}
		}

		string ModuleListingBody
		{
			get
			{
				ZString body = "The above Organisation is licensed to use the following modules:" + System.Environment.NewLine + System.Environment.NewLine;

				foreach (LicenceModules module in Modules)
				{
					if (module.LM_Calc_IsEnabled && !((Env.Licence.GetCheckpointFromCode(module.LM_GroupModuleCode) is LanguageLicenceCheckpoint || Env.Licence.GetCheckpointFromCode(module.LM_GroupModuleCode) is LanguageLicenceChildCheckpoint) && module.LM_UserCount == 0))
					{
						string paddedModuleName = LicenceModuleList.Instance.Names.GetDescriptionFromCode(module.LM_GroupModuleCode).PadRight(40, ' ');
						body += "\t" + paddedModuleName + "\t\t" +
							module.LM_UserCount + " users\t\t" +
							module.Lookups.LicenceTypesList.GetDescriptionFromCode(module.LM_LicenceType);

						if (!module.LM_ExpiryDate.IsEmpty)
						{
							body += "\t\tExpires: " + module.LM_ExpiryDate.ToShortDateString();
						}

						body += System.Environment.NewLine;
					}
				}

				return body;
			}
		}

		#endregion

		#region ReadOnly

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;
			string propertyName = property.Name;

			if (propertyName.IndexOf('+') == -1)
			{
				// this is a direct property of the class
				// and not a wrapped property of an inner object like Billing
				//
				if (propertyName == LicenceHeaderSchema.LA_AgreedLiveDate.Name)
				{
					result = !EDISecurityCheckpoints.OrgLicenceModifyAgreedGoLive.IsAllowed;
				}
				else if (propertyName == LicenceHeaderSchema.LA_AMS_USMode.Name ||
					propertyName == LicenceHeaderSchema.LA_EstimatedLiveDate.Name ||
					propertyName == LicenceHeaderSchema.LA_SiteLiveDate.Name ||
					propertyName == LicenceHeaderSchema.LA_InstallationCompleteDate.Name
					)
				{
					result = !EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed;
				}
				else if (propertyName == LicenceHeaderSchema.LA_SupportMode.Name ||
					propertyName == LicenceHeaderSchema.LA_SupportStartDate.Name ||
					propertyName == LicenceHeaderSchema.LA_ContractExpiryDate.Name ||
					propertyName == LicenceHeaderSchema.LA_SpecialSupportConditions.Name)
				{
					result = !EDISecurityCheckpoints.OrgLicenceModifySupportAndContractDetails.IsAllowed;
				}
				else
				{
					result = !EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed;
				}
			}

			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region Calendar Reminders

		Reminder GetSendReminder()
		{
			string body = "Client " + OrganisationFullName + " (" + OrganisationCode + ") has a Go-Live date of " + LA_SiteLiveDate.ToShortDateString();
			body += System.Environment.NewLine + System.Environment.NewLine;
			body += "Contact Details:" + System.Environment.NewLine;
			body += Company.Header.MainAddress.AddressAsASingleLineWithoutCompanyName + System.Environment.NewLine;
			body += "Phone: " + Company.Header.MainAddress.OA_Phone + System.Environment.NewLine;
			body += "Mobile: " + Company.Header.MainAddress.OA_Mobile + System.Environment.NewLine;
			body += "Fax: " + Company.Header.MainAddress.OA_Fax + System.Environment.NewLine;
			body += "Email: " + Company.Header.MainAddress.OA_Email + System.Environment.NewLine;

			Reminder reminder = new Reminder(PK.ToString(), PK, new ZString(TableName), DateTimeKind.Local, LA_SiteLiveDate, LA_SiteLiveDate, "Client Go-Live - " + OrganisationCode, body);
			reminder.Recipients.Add("Go-Live Updates", EDIDataRegistry.Instance.GoLiveUpdatesEmailAddress.Value);
			GlbStaff salesRep = Company.Header.StaffAssignments.OverallSalesRepStaff;
			if (salesRep != null && !salesRep.GS_EmailAddress.IsEmpty)
			{
				reminder.Recipients.Add(salesRep.GS_FullName, salesRep.GS_EmailAddress);
			}
			reminder.AlarmPeriod = new TimeSpan(2, 0, 0, 0);

			return reminder;
		}

		Reminder GetCancelReminder()
		{
			string body = "Client " + OrganisationFullName + " (" + OrganisationCode + ") Go-Live has been cancelled ";

			Reminder reminder = new Reminder(PK.ToString(), PK, new ZString(TableName), DateTimeKind.Local, OriginalLA_SiteLiveDate, OriginalLA_SiteLiveDate, "Client Go-Live - " + OrganisationCode + " - CANCELLED", body);
			reminder.Recipients.Add("Go-Live Updates", EDIDataRegistry.Instance.GoLiveUpdatesEmailAddress.Value);
			GlbStaff salesRep = Company.Header.StaffAssignments.OverallSalesRepStaff;
			if (salesRep != null && !salesRep.GS_EmailAddress.IsEmpty)
			{
				reminder.Recipients.Add(salesRep.GS_FullName, salesRep.GS_EmailAddress);
			}
			reminder.AlarmPeriod = new TimeSpan(2, 0, 0, 0);
			reminder.ReminderType = ReminderType.Cancellation;

			return reminder;
		}

		#endregion

		#region Discrepancies

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection types = new NoteTypeCollection();
				types.Add(EDIPredefinedNoteTypes.Instance.LicenceDiscrepancy);
				return types;
			}
		}

		public ZString DiscrepancyText
		{
			get { return DiscrepancyNote.Text; }
			set { DiscrepancyNote.Text = value; }
		}

		UniqueNote DiscrepancyNote
		{
			get { return discrepancyNote ?? (discrepancyNote = new UniqueNote(this, EDIPredefinedNoteTypes.Instance.LicenceDiscrepancy)); }
		}

		UniqueNote discrepancyNote;

		[ReadOnly(true)]
		[ActionField(FieldType = ActionFieldType.Hidden)]
		public ZDateTime LA_LastDiscrepancyChange
		{
			// LA_LastFaxReport isn't used
			get { return LA_LastFaxReport; }
			set { LA_LastFaxReport = value; }
		}

		public ZPropertyInfo LA_LastDiscrepancyChangeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.LA_LastDiscrepancyChange, x => LA_LastFaxReportInfo); }
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return false;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return (NoResString)"Can't delete a LicenceHeader";
			}
		}

		#endregion

		#region ClientLicenceHeaderEx

		[ActionFieldFollow(false)]
		public ClientLicenceHeaderEx Billing
		{
			get { return GetClientLicenceHeaderEx(true); }
		}

		[ActionFieldFollow(false)]
		public ClientLicenceHeaderEx ReadonlyBilling
		{
			get { return GetClientLicenceHeaderEx(false); }
		}

		ClientLicenceHeaderEx billing;

		ClientLicenceHeaderEx GetClientLicenceHeaderEx(bool create)
		{
			if (billing == null || (!IsDeleted && billing.IsDeleted))
			{
				billing = Factory.LoadTop1<ClientLicenceHeaderEx>(new ZQuery(ClientLicenceHeaderExSchema.L0_LA, this.PK));
				if (billing == null && create)
				{
					billing = CreateClientLicenceHeaderEx();
				}
				if (billing != null)
				{
					RegisterEditableChildObject(billing);
				}
			}

			return billing;
		}

		ClientLicenceHeaderEx CreateClientLicenceHeaderEx()
		{
			ClientLicenceHeaderEx result = Factory.New<ClientLicenceHeaderEx>();
			result.L0_LA = this.PK;
			return result;
		}

		[List("Organisations")]
		public ZGuid OH_Owner
		{
			get
			{
				var readBilling = ReadonlyBilling;
				return readBilling != null ? readBilling.L0_OH_Owner : ZGuid.Empty;
			}
			set
			{
				var bill = Billing;
				if (bill.L0_OH_Owner != value)
				{
					Billing.L0_OH_Owner = value;
					OH_OwnerInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo OH_OwnerInfo
		{
			get { return GetZPropertyInfo(nameof(OH_Owner)); }
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion

		#region Changing

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (value)
				{
					MarkOrgForSaving();
				}
			}
		}

		void MarkOrgForSaving()
		{
			if (!IsSettingHasChangesSuspended && LA_LC.IsValid)
			{
				Factory.Load<LicenceCompany>(LA_LC)?.Header?.MarkForSaving();
			}
		}

		#endregion

		#region Billing

		[ActionFieldFollow(false)]
		public ClientInvoiceDelivery MaintenanceInvoiceDelivery
		{
			get
			{
				return (Company != null && Database != null)
					? Company.InvoiceDeliveries.FindByServerAndSystem(Database.LD_ServerCode, BillingConstants.BillingSystem.Maintenance)
					: null;
			}
		}

		public ClientLicencePriceHeader MaintenancePricesForDate(ZDateTime date)
		{
			ClientLicencePriceHeader prices = null;
			if (Company != null)
			{
				prices = Company.OneTimePriceHeaderForDate(date);

				if (prices == null)
				{
					ClientInvoiceDelivery invoiceDelivery = MaintenanceInvoiceDelivery;
					if (invoiceDelivery != null)
					{
						ZGuid payingOrgPk = invoiceDelivery.CalcInvoicedOrganisationPK(ZGuid.Empty);
						if (payingOrgPk.IsValid)
						{
							EDIOrgHeader payingOrg = Factory.Load<EDIOrgHeader>(payingOrgPk);
							if (payingOrg.LicCompany != null)
							{
								prices = payingOrg.LicCompany.OneTimePriceHeaderForDate(date);
							}
						}
					}
				}
			}

			return prices;
		}

		ZDateTime AnyLiveDateOrMax
		{
			get
			{
				if (!LA_AgreedLiveDate.IsEmpty)
				{
					return LA_AgreedLiveDate;
				}
				else if (!LA_SiteLiveDate.IsEmpty)
				{
					return LA_SiteLiveDate;
				}
				else if (!LA_EstimatedLiveDate.IsEmpty)
				{
					return LA_EstimatedLiveDate;
				}
				else
				{
					return ZDateTime.MaxSmallDateTime;
				}
			}
		}

		public static LicenceHeader FirstActiveLive(IEnumerable<LicenceHeader> list)
		{
			return list
				.OrderBy(x => x.LA_IsActive && (x.Company?.Header?.OH_IsActive ?? ZBool.False) ? 0 : 1) // prefer active
				.ThenBy(x => x.AnyLiveDateOrMax) // first live
				.ThenBy(x => x.CompanyCode)
				.FirstOrDefault();
		}

		#endregion
	}
}

