using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.FTP
{
	[XmlSerializerAssembly("Enterprise.ServiceManager.Tasks.FtpGenericPusherPuller.XmlSerializers")]
	public class FtpProfile : RegistryBusinessObjectTemplate
	{
		internal abstract class Schema
		{
			public const string FriendlyName = "FriendlyName";
			public const string LocalFolder = "LocalFolder";
			public const string RemoteLocation = "RemoteLocation";
			public const string FindFileMask = "FindFileMask";
			public const string RemoteUsername = "RemoteUsername";
			public const string RemotePassword = "RemotePassword";
			public const string ClobberOrMakeUnique = "ClobberOrMakeUnique";
			public const string PushOrPull = "PushOrPull";
			public const string DeleteSourceOption = "DeleteSourceOption";
			public const string RunPeriodSeconds = "RunPeriodSeconds";
			public const string EmailAlertOnFailure = "EmailAlertOnFailure";
			public const string LastRunUtc = "LastRunUtc";
		}

		#region Ctors and guff
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FtpProfile(fallbackLevel, factory);
		}

		public FtpProfile()
		{
		}

		public FtpProfile(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public FtpProfile(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		#endregion

		#region FriendlyName
		[MaxLength(255)]
		public ZString FriendlyName
		{
			get { return fFriendlyName; }
			set
			{
				SetNonPersistentPropertyValue(FriendlyNameInfo, ref fFriendlyName, value);
				if (!IsValidationSuspended)
				{
					ValidateFriendlyName();
				}
			}
		}
		ZString fFriendlyName;

		public ZPropertyInfo FriendlyNameInfo
		{
			get { return GetZPropertyInfo(Schema.FriendlyName); }
		}

		public void ValidateFriendlyName()
		{
		}
		#endregion

		#region LocalFolder
		[MaxLength(255)]
		public ZString LocalFolder
		{
			get { return fLocalFolder; }
			set
			{
				SetNonPersistentPropertyValue(LocalFolderInfo, ref fLocalFolder, value);
				if (!IsValidationSuspended)
				{
					ValidateLocalFolder();
				}
			}
		}
		ZString fLocalFolder;

		public ZPropertyInfo LocalFolderInfo
		{
			get { return GetZPropertyInfo(Schema.LocalFolder); }
		}

		public void ValidateLocalFolder()
		{
		}
		#endregion

		#region RemoteLocation
		[MaxLength(255)]
		public ZString RemoteLocation
		{
			get { return fRemoteLocation; }
			set
			{
				SetNonPersistentPropertyValue(RemoteLocationInfo, ref fRemoteLocation, value);
				if (!IsValidationSuspended)
				{
					ValidateRemoteLocation();
				}
			}
		}
		ZString fRemoteLocation;

		public ZPropertyInfo RemoteLocationInfo
		{
			get { return GetZPropertyInfo(Schema.RemoteLocation); }
		}

		public void ValidateRemoteLocation()
		{
		}
		#endregion

		#region FindFileMask
		[MaxLength(255)]
		public ZString FindFileMask
		{
			get { return fFindFileMask; }
			set
			{
				SetNonPersistentPropertyValue(FindFileMaskInfo, ref fFindFileMask, value);
				if (!IsValidationSuspended)
				{
					ValidateFindFileMask();
				}
			}
		}
		ZString fFindFileMask;

		public ZPropertyInfo FindFileMaskInfo
		{
			get { return GetZPropertyInfo(Schema.FindFileMask); }
		}

		public void ValidateFindFileMask()
		{
		}
		#endregion

		#region RemoteUsername
		[MaxLength(50)]
		public ZString RemoteUsername
		{
			get { return fRemoteUsername; }
			set
			{
				SetNonPersistentPropertyValue(RemoteUsernameInfo, ref fRemoteUsername, value);
				if (!IsValidationSuspended)
				{
					ValidateRemoteUsername();
				}
			}
		}
		ZString fRemoteUsername;

		public ZPropertyInfo RemoteUsernameInfo
		{
			get { return GetZPropertyInfo(Schema.RemoteUsername); }
		}

		public void ValidateRemoteUsername()
		{
		}
		#endregion

		#region RemotePassword
		[MaxLength(50)]
		public ZString RemotePassword
		{
			get { return fRemotePassword; }
			set
			{
				SetNonPersistentPropertyValue(RemotePasswordInfo, ref fRemotePassword, value);
				if (!IsValidationSuspended)
				{
					ValidateRemotePassword();
				}
			}
		}
		ZString fRemotePassword;

		public ZPropertyInfo RemotePasswordInfo
		{
			get { return GetZPropertyInfo(Schema.RemotePassword); }
		}

		public void ValidateRemotePassword()
		{
		}
		#endregion

		[List("Lookups.UniqueOptions")]
		#region ClobberOrMakeUnique
		[MaxLength(3)]
		public ZString ClobberOrMakeUnique
		{
			get { return fClobberOrMakeUnique; }
			set
			{
				SetNonPersistentPropertyValue(ClobberOrMakeUniqueInfo, ref fClobberOrMakeUnique, value);
				if (!IsValidationSuspended)
				{
					ValidateClobberOrMakeUnique();
				}
			}
		}
		ZString fClobberOrMakeUnique;

		public ZPropertyInfo ClobberOrMakeUniqueInfo
		{
			get { return GetZPropertyInfo(Schema.ClobberOrMakeUnique); }
		}

		public void ValidateClobberOrMakeUnique()
		{
		}
		#endregion

		#region PushOrPull
		[List("Lookups.PushOrPull")]
		[MaxLength(3)]
		public ZString PushOrPull
		{
			get { return fPushOrPull; }
			set
			{
				SetNonPersistentPropertyValue(PushOrPullInfo, ref fPushOrPull, value);
				if (!IsValidationSuspended)
				{
					ValidatePushOrPull();
				}
			}
		}
		ZString fPushOrPull;

		public ZPropertyInfo PushOrPullInfo
		{
			get { return GetZPropertyInfo(Schema.PushOrPull); }
		}

		public void ValidatePushOrPull()
		{
		}
		#endregion

		[List("Lookups.SuccessActions")]
		#region DeleteSourceOnSuccess
		[MaxLength(3)]
		public ZString DeleteSourceOption
		{
			get { return fDeleteSourceOption; }
			set
			{
				SetNonPersistentPropertyValue(DeleteSourceOptionInfo, ref fDeleteSourceOption, value);
				if (!IsValidationSuspended)
				{
					ValidateDeleteSourceOption();
				}
			}
		}
		ZString fDeleteSourceOption;

		public ZPropertyInfo DeleteSourceOptionInfo
		{
			get { return GetZPropertyInfo(Schema.DeleteSourceOption); }
		}

		public void ValidateDeleteSourceOption()
		{
		}
		#endregion

		#region RunPeriodSeconds
		public ZInt RunPeriodSeconds
		{
			get { return fRunPeriodSeconds; }
			set
			{
				SetNonPersistentPropertyValue(RunPeriodSecondsInfo, ref fRunPeriodSeconds, value);
				if (!IsValidationSuspended)
				{
					ValidateRunPeriodSeconds();
				}
			}
		}
		ZInt fRunPeriodSeconds;

		public ZPropertyInfo RunPeriodSecondsInfo
		{
			get { return GetZPropertyInfo(Schema.RunPeriodSeconds); }
		}

		public void ValidateRunPeriodSeconds()
		{
		}
		#endregion

		#region EmailAlertOnFailure
		[MaxLength(50)]
		public ZString EmailAlertOnFailure
		{
			get { return fEmailAlertOnFailure; }
			set
			{
				SetNonPersistentPropertyValue(EmailAlertOnFailureInfo, ref fEmailAlertOnFailure, value);
				if (!IsValidationSuspended)
				{
					ValidateEmailAlertOnFailure();
				}
			}
		}
		ZString fEmailAlertOnFailure;

		public ZPropertyInfo EmailAlertOnFailureInfo
		{
			get { return GetZPropertyInfo(Schema.EmailAlertOnFailure); }
		}

		public void ValidateEmailAlertOnFailure()
		{
		}
		#endregion

		#region LastRunUtc
		[ReadOnly(true)]
		public ZDateTime LastRunUtc
		{
			get { return fLastRunUtc; }
			set
			{
				SetNonPersistentPropertyValue(LastRunUtcInfo, ref fLastRunUtc, value);
				if (!IsValidationSuspended)
				{
					ValidateLastRunUtc();
				}
			}
		}
		ZDateTime fLastRunUtc;

		public ZPropertyInfo LastRunUtcInfo
		{
			get { return GetZPropertyInfo(Schema.LastRunUtc); }
		}

		public void ValidateLastRunUtc()
		{
		}
		#endregion

		FtpProfileLookups lookups;
		public FtpProfileLookups Lookups
		{
			get { return lookups ?? (lookups = new FtpProfileLookups()); }
		}

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LocalFolder, LocalFolder);
			writer.WriteElementString(Schema.RemoteLocation, RemoteLocation);
			writer.WriteElementString(Schema.FindFileMask, FindFileMask);
			writer.WriteElementString(Schema.RemoteUsername, RemoteUsername);
			writer.WriteElementString(Schema.RemotePassword, RemotePassword);
			writer.WriteElementString(Schema.ClobberOrMakeUnique, ClobberOrMakeUnique);
			writer.WriteElementString(Schema.PushOrPull, PushOrPull);
			writer.WriteElementString(Schema.DeleteSourceOption, DeleteSourceOption);
			writer.WriteElementString(Schema.RunPeriodSeconds, RunPeriodSeconds.ToString());
			writer.WriteElementString(Schema.EmailAlertOnFailure, EmailAlertOnFailure);
			writer.WriteElementString(Schema.LastRunUtc, LastRunUtc.ToString("yyyy-MM-dd HH:mm:ss"));
			writer.WriteElementString(Schema.FriendlyName, FriendlyName);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			LocalFolder = reader.ReadElementString(Schema.LocalFolder);
			RemoteLocation = reader.ReadElementString(Schema.RemoteLocation);
			FindFileMask = reader.ReadElementString(Schema.FindFileMask);
			RemoteUsername = reader.ReadElementString(Schema.RemoteUsername);
			RemotePassword = reader.ReadElementString(Schema.RemotePassword);
			ClobberOrMakeUnique = reader.ReadElementString(Schema.ClobberOrMakeUnique);
			PushOrPull = reader.ReadElementString(Schema.PushOrPull);
			DeleteSourceOption = reader.ReadElementString(Schema.DeleteSourceOption);
			RunPeriodSeconds = reader.ReadElementStringAsZInt(Schema.RunPeriodSeconds);
			EmailAlertOnFailure = reader.ReadElementString(Schema.EmailAlertOnFailure);
			LastRunUtc = reader.ReadElementStringAsZDateTime(Schema.LastRunUtc, "yyyy-MM-dd HH:mm:ss");
			FriendlyName = reader.ReadElementString(Schema.FriendlyName);
		}
		#endregion

		protected override ZString HumanReadableNameCore
		{
			get { return string.Format("Profile name '{4}'. {0} to/from {1} user {2}, local {3}. Option {5}", PushOrPull, RemoteLocation, RemoteUsername, LocalFolder, FriendlyName, DeleteSourceOption); }
		}
	}
}
