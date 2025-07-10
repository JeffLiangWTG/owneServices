using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class UpgradeRequest : NonPersistentBusinessObject, IObsoleteValidation
	{
		public UpgradeRequest(BusinessObjectFactory factory, LicenceDatabase database)
			: this(factory, null, database)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public UpgradeRequest(BusinessObjectFactory factory, EDIOrgHeader organisation, LicenceDatabase database)
			: base(factory)
		{
			this.organisation = organisation;
			this.database = database;
			SetUp();

			if (database.HasMultipleEnterprises)
			{
				AddRowError("Database is linked to multiple enterprises and must not be upgraded until this is fixed");
			}
		}

		readonly EDIOrgHeader organisation;
		readonly LicenceDatabase database;

		protected virtual void SetUp()
		{
			using (SuspendSettingHasChanges())
			{
				OrgContact contact = GetClientContact();
				if (contact != null)
				{
					ClientContactPK = contact.PK;
					ClientContactEmailAddress = contact.OC_Email;
				}
				ScheduledDateTime = ZDateTime.Now;
				SetBestSupportedUpgradeMethod(string.Empty);
			}
		}

		ReleaseBuild selectedReleaseBuild;

		public bool NotifyUser { get; set; } = (bool)UpgradesToClientSchema.L1_NotifyUser.SqlDbDefault;

		#region Organisation

		public ZGuid OrganisationPk
		{
			get { return organisation != null ? organisation.PK : ZGuid.Empty; }
		}

		#endregion

		#region LicDatabase

		public LicenceDatabase LicDatabase
		{
			get { return database; }
		}

		#endregion

		#region Properties Linked To Organisation And LicenceDatabase

		#region EnterpriseCode

		public ZString EnterpriseCode
		{
			get { return LicDatabase != null && LicDatabase.LicEnterprise != null ? LicDatabase.LicEnterprise.LE_EnterpriseCode : ZString.Empty; }
		}

		public ZPropertyInfo EnterpriseCodeInfo
		{
			get { return GetZPropertyInfo(nameof(EnterpriseCode)); }
		}

		#endregion

		#region CompanyName

		public ZString CompanyName
		{
			get { return organisation != null ? organisation.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyName)); }
		}

		#endregion

		#region PreferredUpgradeMethod

		public ZString PreferredUpgradeMethod
		{
			get { return LicDatabase != null ? LicDatabase.LD_AvailableUpgradeMethod : ZString.Empty; }
		}

		public ZPropertyInfo PreferredUpgradeMethodInfo
		{
			get { return GetZPropertyInfo(nameof(PreferredUpgradeMethod)); }
		}

		#endregion

		#region ServerCode

		public ZString ServerCode
		{
			get { return LicDatabase != null ? LicDatabase.LD_ServerCode : ZString.Empty; }
		}

		public ZPropertyInfo ServerCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ServerCode)); }
		}

		#endregion

		#region Release Ring

		public ZString ReleaseRing
		{
			get { return (LicDatabase != null) ? LicDatabase.LD_ReleaseRing : ZString.Empty; }
		}

		public ZPropertyInfo ReleaseRingInfo
		{
			get { return GetZPropertyInfo(nameof(ReleaseRing)); }
		}

		public void ValidateReleaseRing()
		{
			ReleaseRingInfo.ClearAllNotifications();
			if (selectedReleaseBuild != null)
			{
				if (new ReleaseRingComparer().Compare(selectedReleaseBuild.HL_ReleaseStatus, LicDatabase.LD_ReleaseRing) == -1)
				{
					SecurityCheckpoint checkpoint = EDISecurityCheckpoints.OrgLicenceModifySendUpgradeToHigherRingDB;
					if (checkpoint.IsAllowed)
					{
						ReleaseRingInfo.AddWarning("This server's release ring is higher than the selected build's release ring.");
					}
					else
					{
						ReleaseRingInfo.AddError(string.Format(CultureInfo.CurrentCulture, "You cannot send the selected build to this server because its release ring is higher than the build's release ring.{0}{0}If you need to send upgrade packages to client databases on a higher ring than the package, please ask your administrator to change either your Staff or Group Security Rights to allow access to:{0}{0}{1}", System.Environment.NewLine, checkpoint.DisplayTextPathToSecurityRight));
					}
				}
			}
		}

		#endregion

		#region SQL Version

		public ZString SqlVersion
		{
			get { return (LicDatabase == null) ? ZString.Empty : LicDatabase.LD_SQLVersion; }
		}

		public ZPropertyInfo SqlVersionInfo
		{
			get { return GetZPropertyInfo(nameof(SqlVersion)); }
		}

		#endregion

		#region Product

		public ZString Product => LicDatabase?.LD_Product ?? ZString.Empty;

		public ZPropertyInfo ProductInfo => GetZPropertyInfo(nameof(Product));

		public void ValidateProduct()
		{
			ProductInfo.ClearAllNotifications();
			if (selectedReleaseBuild != null && !selectedReleaseBuild.IsUpgradeableProduct(Product))
			{
				ProductInfo.AddError("The release build product does not match to license database product.");
			}
		}

		#endregion

		#endregion

		#region Own Properties

		#region SupportedUpgradeMethod

		[MaxLength(3)]
		public ZString SupportedUpgradeMethod
		{
			get { return fSupportedUpgradeMethod; }
			set
			{
				if (fSupportedUpgradeMethod != value)
				{
					CheckMaximumLength(SupportedUpgradeMethodInfo, value);
					SetNonPersistentPropertyValue(SupportedUpgradeMethodInfo, ref fSupportedUpgradeMethod, value);
					SetDefaultScheduledTime(ScheduledDateTime);
					if (!IsValidationSuspended)
					{
						ValidateSupportedUpgradeMethod();
					}
				}
			}
		}

		public static string HttpUploadVersionMessage
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} now requires a server at or above version {1} (or GPR release v{2}.{3}.{4}.{5}+ or STD release v{6}.{7}.{8}.{9}+)",
					UpgradeMethods.Descriptions.Http,
					HttpDownload.SecureDownloadVersionString,
					HttpDownload.GprVersion.Major, HttpDownload.GprVersion.Minor, HttpDownload.GprVersion.Release, HttpDownload.GprVersion.Patch,
					HttpDownload.StdVersion.Major, HttpDownload.StdVersion.Minor, HttpDownload.StdVersion.Release, HttpDownload.StdVersion.Patch);
			}
		}

		public void ValidateSupportedUpgradeMethod()
		{
			SupportedUpgradeMethodInfo.ClearAllNotifications();
			if (SupportedUpgradeMethod == UpgradeMethods.Codes.Http
				&& !SupportedUpgradeMethods.ContainsCode(UpgradeMethods.Codes.Http)
				&& LicDatabase != null && !new HttpDownload().IsSupportingVersion(LicDatabase.CurrentVersion))
			{
				SupportedUpgradeMethodInfo.AddWarning(HttpUploadVersionMessage);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(SupportedUpgradeMethodInfo, SupportedUpgradeMethods);
			}

			if (!PreferredUpgradeMethod.IsEmpty &&
				SupportedUpgradeMethod != PreferredUpgradeMethod)
			{
				SupportedUpgradeMethodInfo.AddWarning("You are ignoring client's Preferred UpgradeMethod " + PreferredUpgradeMethod);
			}
			ValidateClientContactEmailAddress();

			if (SupportedUpgradeMethod == UpgradeMethods.Codes.Blocked)
			{
				SupportedUpgradeMethodInfo.AddError("Please select another method if you want to send an upgrade to this database or delete the row if you do not want an upgrade to be sent.");
			}
		}

		public ZPropertyInfo SupportedUpgradeMethodInfo
		{
			get { return GetZPropertyInfo(nameof(SupportedUpgradeMethod)); }
		}

		ZString fSupportedUpgradeMethod;

		public CodeDescriptionPairList SupportedUpgradeMethods
		{
			get
			{
				if (supportedUpgradeMethods == null)
				{
					supportedUpgradeMethods = GetSupportedUpgradeMethods();
				}

				return supportedUpgradeMethods;
			}
		}

		CodeDescriptionPairList supportedUpgradeMethods;

		public CodeDescriptionPairList GetSupportedUpgradeMethods()
		{
			CodeDescriptionPairList result = new UpgradeMethods();

			if (LicDatabase != null)
			{
				if (!LicDatabase.IsUpgradeMethodSupported(UpgradeMethods.Codes.Http))
				{
					result.RemoveCode(UpgradeMethods.Codes.Http);
				}
			}
			else
			{
				result.Clear();
			}

			return result;
		}

		#endregion

		#region ScheduledUpgradeTime

		public ZDateTime ScheduledDateTime
		{
			get { return fScheduledDateTime; }
			set
			{
				SetNonPersistentPropertyValue(ScheduledDateTimeInfo, ref fScheduledDateTime, value);
				if (!IsValidationSuspended)
				{
					ValidateScheduledDateTime();
				}
			}
		}

		public virtual void ValidateScheduledDateTime()
		{
			ScheduledDateTimeInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ScheduledDateTimeInfo);
			if (ScheduledDateTime.IsEmpty && SupportedUpgradeMethod != UpgradeMethods.Codes.Blocked)
			{
				ScheduledDateTimeInfo.AddError("Scheduled Time should be specified");
			}
		}

		public ZPropertyInfo ScheduledDateTimeInfo
		{
			get { return GetZPropertyInfo(nameof(ScheduledDateTime)); }
		}

		ZDateTime fScheduledDateTime;

		#endregion

		#region ClientContact

		public ZGuid ClientContactPK
		{
			get { return clientContactPK; }
			set
			{
				SetNonPersistentPropertyValue(ClientContactPKInfo, ref clientContactPK, value);

				if (!IsValidationSuspended)
				{
					ValidateClientContactPK();
				}

				OrgContact clientContact = ClientContact;
				if (clientContact != null)
				{
					ClientContactEmailAddress = clientContact.OC_Email;
				}
				else
				{
					ClientContactEmailAddress = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ClientContactPKInfo
		{
			get { return GetZPropertyInfo(nameof(ClientContactPK)); }
		}

		public OrgContactDependentCollection ClientContacts
		{
			get { return (organisation != null) ? new OrgContactDependentCollection(organisation) : new OrgContactDependentCollection(Factory); }
		}

		public OrgContact ClientContact
		{
			get
			{
				OrgContact result = null;
				if (ClientContactPK.IsValid)
				{
					result = Factory.Load<OrgContact>(ClientContactPK);
				}
				return result;
			}
		}

		public void ValidateClientContactPK()
		{
			ClientContactPKInfo.ClearAllNotifications();
			if (!ClientContactPK.IsEmpty && (!ClientContactPK.IsValid || ClientContact == null))
			{
				ClientContactPKInfo.AddError("Enter a valid code.");
			}
		}

		ZGuid clientContactPK;

		#endregion

		#region ClientContactEmailAddress

		[MaxLength(254)]
		public ZString ClientContactEmailAddress
		{
			get { return fClientContactEmailAddress; }
			set
			{
				if (fClientContactEmailAddress != value)
				{
					CheckMaximumLength(ClientContactEmailAddressInfo, value);
					SetNonPersistentPropertyValue(ClientContactEmailAddressInfo, ref fClientContactEmailAddress, value);
					if (!IsValidationSuspended)
					{
						ValidateClientContactEmailAddress();
					}
				}
			}
		}

		public virtual void ValidateClientContactEmailAddress()
		{
			ClientContactEmailAddressInfo.ClearAllNotifications();
			if (SupportedUpgradeMethod != UpgradeMethods.Codes.Blocked &&
				!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(ClientContactEmailAddress))
			{
				ClientContactEmailAddressInfo.AddError("You should specify a valid email address for notification.");
			}
		}

		public ZPropertyInfo ClientContactEmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(ClientContactEmailAddress)); }
		}

		ZString fClientContactEmailAddress;

		public OrgContact GetClientContact()
		{
			OrgContact result = null;
			if (LicDatabase != null)
			{
				if (LicDatabase.ContractInstallerOrInternalTechContact != null &&
					!LicDatabase.ContractInstallerOrInternalTechContact.OC_Email.IsEmpty)
				{
					result = LicDatabase.ContractInstallerOrInternalTechContact;
				}
				else if (result == null && LicDatabase.LicenseeAdminContact != null &&
					!LicDatabase.LicenseeAdminContact.OC_Email.IsEmpty)
				{
					result = LicDatabase.LicenseeAdminContact;
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region Public Methods

		public void SetSelectedReleaseBuild(ReleaseBuild value)
		{
			selectedReleaseBuild = value;
			if (!IsValidationSuspended)
			{
				ValidateReleaseRing();
				ValidateProduct();
			}
		}

		public void SetBestSupportedUpgradeMethod(string desirableMethod)
		{
			if (LicDatabase != null)
			{
				if (PreferredUpgradeMethod == UpgradeMethods.Codes.Blocked)
				{
					SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
				}
				else if (LicDatabase.LD_PublicEmailAddressForUpdate.IsEmpty && LicDatabase.VersionCanReceiveAllSystemMessages == Customs.Business.TriState.False)
				{
					SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
				}
				else if (!PreferredUpgradeMethod.IsEmpty && SupportedUpgradeMethods.ContainsCode(PreferredUpgradeMethod))
				{
					SupportedUpgradeMethod = PreferredUpgradeMethod;
				}
				else if (!string.IsNullOrEmpty(desirableMethod) && SupportedUpgradeMethods.ContainsCode(desirableMethod))
				{
					SupportedUpgradeMethod = desirableMethod;
				}
				else if (SupportedUpgradeMethods.ContainsCode(UpgradeMethods.Codes.Http))
				{
					SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
				}
				else
				{
					SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
				}
			}
			else
			{
				SupportedUpgradeMethod = UpgradeMethods.Codes.Blocked;
			}

			SupportedUpgradeMethodInfo.RefreshBinding();
		}

		public void SetDefaultScheduledTime(ZDateTime defaultTime)
		{
			if (SupportedUpgradeMethod == UpgradeMethods.Codes.Blocked)
			{
				ScheduledDateTime = ZDateTime.Empty;
			}
			else if (defaultTime >= ZDateTime.Now)
			{
				ScheduledDateTime = defaultTime;
			}
			else
			{
				ScheduledDateTime = ZDateTime.Now;
			}

			ScheduledDateTimeInfo.RefreshBinding();
		}

		public UpgradesToClient CreateUpgradesToClient(ReleaseBuild build)
		{
			UpgradesToClient upgrade = null;

			if (build != null &&
				!SupportedUpgradeMethod.IsEmpty &&
				SupportedUpgradeMethod != UpgradeMethods.Codes.Blocked)
			{
				upgrade = Factory.New<UpgradesToClient>();
				upgrade.L1_HL = build.PK;
				upgrade.L1_GS_NKStaffCode = Env.CurrentUser?.Initials ?? User.WebUserCode;
				upgrade.L1_LD = LicDatabase.PK;
				upgrade.L1_OH = OrganisationPk;
				upgrade.L1_RequestedUpgradeMethod = SupportedUpgradeMethod;
				upgrade.L1_CurrentStatus = UpgradesToClientStatus.Codes.Queued;
				upgrade.L1_RequestedDateTime = ScheduledDateTime;
				upgrade.L1_OC = this.ClientContactPK;
				upgrade.L1_NotifyUser = NotifyUser;
			}

			return upgrade;
		}

		public string NotificationMessage
		{
			get
			{
				string msg = SupportedUpgradeMethod != UpgradeMethods.Codes.Blocked ?
					string.Format(CultureInfo.CurrentCulture, "Server {0} at {1} via {2}.\r\n", ServerCode, ScheduledDateTime, SupportedUpgradeMethods.GetDescriptionFromCode(SupportedUpgradeMethod)) :
					string.Format(CultureInfo.CurrentCulture, "Server {0} is {1}.\r\n", ServerCode, SupportedUpgradeMethods.GetDescriptionFromCode(SupportedUpgradeMethod));

				if (PreferredUpgradeMethod == UpgradeMethods.Codes.Http
					&& !SupportedUpgradeMethods.ContainsCode(UpgradeMethods.Codes.Http))
				{
					msg += string.Format(CultureInfo.CurrentCulture, " - {0}.\r\n", HttpUploadVersionMessage);
				}

				return msg;
			}
		}

		#endregion
	}
}
