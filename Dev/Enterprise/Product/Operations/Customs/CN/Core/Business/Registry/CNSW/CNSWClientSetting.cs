using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.CN.Business.XmlSerializers")]
	public class CNSWClientSetting : RegistryBusinessObjectTemplate
	{
		#region Schema and Constructors

		protected abstract class Schema
		{
			public const string MachineName = nameof(CNSWClientSetting.MachineName);
			public const string SendFolder = nameof(CNSWClientSetting.SendFolder);
			public const string ReceiveFolder = nameof(CNSWClientSetting.ReceiveFolder);
			public const string ErrorResponseFolder = nameof(CNSWClientSetting.ErrorResponseFolder);
			public const string ArchiveFolder = nameof(CNSWClientSetting.ArchiveFolder);
			public const string RunningIntervalInSeconds = nameof(CNSWClientSetting.RunningIntervalInSeconds);
			public const string EHubClientID = nameof(CNSWClientSetting.EHubClientID);
			public const string EHubClientStatus = nameof(CNSWClientSetting.EHubClientStatus);
			public const string AcdaSendFolder = nameof(CNSWClientSetting.AcdaSendFolder);
			public const string AcdaReceiveFolder = nameof(CNSWClientSetting.AcdaReceiveFolder);
			public const string AcdaErrorResponseFolder = nameof(CNSWClientSetting.AcdaErrorResponseFolder);
			public const string AcdaArchiveFolder = nameof(CNSWClientSetting.AcdaArchiveFolder);
		}

		public CNSWClientSetting() : base()
		{
		}

		public CNSWClientSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		#endregion

		#region Properties

		ZString fMachineName;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|MachineName", Caption = "Machine Name")]
		public ZString MachineName
		{
			get => fMachineName;
			set
			{
				if (MachineName != value)
				{
					CheckMaximumLength(MachineNameInfo, value);
					SetNonPersistentPropertyValue(MachineNameInfo, ref fMachineName, value.ToUpper());

					if (!IsValidationSuspended)
					{
						Validation.ValidateMachineName();
					}
					MachineNameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo MachineNameInfo => GetZPropertyInfo(Schema.MachineName);

		ZString fSendFolder;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|SendFolder", Caption = "Send Folder (Out Box)")]
		public ZString SendFolder
		{
			get => fSendFolder;
			set
			{
				if (SendFolder != value)
				{
					CheckMaximumLength(SendFolderInfo, value);
					SetNonPersistentPropertyValue(SendFolderInfo, ref fSendFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateSendFolder();
					}
					SendFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SendFolderInfo => GetZPropertyInfo(Schema.SendFolder);

		ZString fReceiveFolder;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|ReceiveFolder", Caption = "Receive Folder (In Box)")]
		public ZString ReceiveFolder
		{
			get => fReceiveFolder;
			set
			{
				if (ReceiveFolder != value)
				{
					CheckMaximumLength(ReceiveFolderInfo, value);
					SetNonPersistentPropertyValue(ReceiveFolderInfo, ref fReceiveFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateReceiveFolder();
					}
					ReceiveFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ReceiveFolderInfo => GetZPropertyInfo(Schema.ReceiveFolder);

		ZString fErrorResponseFolder;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|ErrorResponseFolder", Caption = "Error Response Folder (Fail Box)")]
		public ZString ErrorResponseFolder
		{
			get => fErrorResponseFolder;
			set
			{
				if (ErrorResponseFolder != value)
				{
					CheckMaximumLength(ErrorResponseFolderInfo, value);
					SetNonPersistentPropertyValue(ErrorResponseFolderInfo, ref fErrorResponseFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateErrorResponseFolder();
					}
					ErrorResponseFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ErrorResponseFolderInfo => GetZPropertyInfo(Schema.ErrorResponseFolder);

		ZString fArchiveFolder;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|ArchiveFolder", Caption = "Archive Folder")]
		public ZString ArchiveFolder
		{
			get => fArchiveFolder;
			set
			{
				if (ArchiveFolder != value)
				{
					CheckMaximumLength(ArchiveFolderInfo, value);
					SetNonPersistentPropertyValue(ArchiveFolderInfo, ref fArchiveFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateArchiveFolder();
					}
					ArchiveFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ArchiveFolderInfo => GetZPropertyInfo(Schema.ArchiveFolder);

		#region ACDA Folders

		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|AcdaSendFolder", Caption = "Send Folder (Out Box)")]
		public ZString AcdaSendFolder
		{
			get => fAcdaSendFolder;
			set
			{
				if (AcdaSendFolder != value)
				{
					CheckMaximumLength(AcdaSendFolderInfo, value);
					SetNonPersistentPropertyValue(AcdaSendFolderInfo, ref fAcdaSendFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateAcdaSendFolder();
					}
					AcdaSendFolderInfo.RefreshBinding();
				}
			}
		}
		ZString fAcdaSendFolder;

		public ZPropertyInfo AcdaSendFolderInfo => GetZPropertyInfo(Schema.AcdaSendFolder);

		ZString fAcdaReceiveFolder;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|AcdaReceiveFolder", Caption = "Receive Folder (In Box)")]
		public ZString AcdaReceiveFolder
		{
			get => fAcdaReceiveFolder;
			set
			{
				if (AcdaReceiveFolder != value)
				{
					CheckMaximumLength(AcdaReceiveFolderInfo, value);
					SetNonPersistentPropertyValue(AcdaReceiveFolderInfo, ref fAcdaReceiveFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateAcdaReceiveFolder();
					}
					AcdaReceiveFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AcdaReceiveFolderInfo => GetZPropertyInfo(Schema.AcdaReceiveFolder);

		ZString fAcdaErrorResponseFolder;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|AcdaErrorResponseFolder", Caption = "Error Response Folder (Fail Box)")]
		public ZString AcdaErrorResponseFolder
		{
			get => fAcdaErrorResponseFolder;
			set
			{
				if (AcdaErrorResponseFolder != value)
				{
					CheckMaximumLength(AcdaErrorResponseFolderInfo, value);
					SetNonPersistentPropertyValue(AcdaErrorResponseFolderInfo, ref fAcdaErrorResponseFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateAcdaErrorResponseFolder();
					}
					AcdaErrorResponseFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AcdaErrorResponseFolderInfo => GetZPropertyInfo(Schema.AcdaErrorResponseFolder);

		ZString fAcdaArchiveFolder;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|AcdaArchiveFolder", Caption = "Archive Folder")]
		public ZString AcdaArchiveFolder
		{
			get => fAcdaArchiveFolder;
			set
			{
				if (AcdaArchiveFolder != value)
				{
					CheckMaximumLength(AcdaArchiveFolderInfo, value);
					SetNonPersistentPropertyValue(AcdaArchiveFolderInfo, ref fAcdaArchiveFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateAcdaArchiveFolder();
					}
					AcdaArchiveFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AcdaArchiveFolderInfo => GetZPropertyInfo(Schema.AcdaArchiveFolder);

		#endregion

		ZInt fRunningIntervalInSeconds;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|RunningIntervalInSeconds", Caption = "Running Interval (in seconds)")]
		public ZInt RunningIntervalInSeconds
		{
			get => fRunningIntervalInSeconds;
			set
			{
				if (RunningIntervalInSeconds != value)
				{
					SetNonPersistentPropertyValue(RunningIntervalInSecondsInfo, ref fRunningIntervalInSeconds, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateRunningIntervalInSeconds();
					}
					RunningIntervalInSecondsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo RunningIntervalInSecondsInfo => GetZPropertyInfo(Schema.RunningIntervalInSeconds);

		string fEHubClientID;
		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|EHubClientID", Caption = "eHub Client ID")]
		public ZString EHubClientID
		{
			get => MachineName.IsEmpty ? ZString.Empty : fEHubClientID ??= GetEHubClientID(IsOverridden);
			private set => fEHubClientID = value;
		}

		public ZPropertyInfo EHubClientIDInfo => GetZPropertyInfo(Schema.EHubClientID);

		internal string GetEHubClientID(Func<bool> isOverridden = null)
		{
			string result = null;
			var fallbackLevel = CurrentFallbackLevel;
			var factory = CurrentFactory;
			if (fallbackLevel != null && factory != null)
			{
				var branchPK = fallbackLevel.BranchPK;
				var companyPK = fallbackLevel.CompanyPK(true);
				var branch = factory.Load<GlbBranch>(branchPK);
				var company = factory.Load<GlbCompany>(companyPK) ?? branch?.Company;
				if (company != null)
				{
					var isOverriddenAtBranchLevel = branch != null && (isOverridden?.Invoke() ?? true);
					result = $"{company.LicenceKeyIdentifier}{(isOverriddenAtBranchLevel ? branch?.GB_Code : string.Empty)}_CSW";
				}
			}
			return result;
		}

		internal IRegistryItemInternals RegistryItemInternals { get; set; }

		bool IsOverridden() => RegistryItemInternals == null || RegistryItemInternals.GetCurrentValueToUse(Guid.Empty, CurrentFallbackLevel.BranchPK, Guid.Empty) != ValueToUse.DefaultValue;

		[ResourceStringData("Enterprise.Customs.CN.Business.CNSWClientSetting|EHubClientStatus", Caption = "eHub Client Status")]
		public ZString EHubClientStatus => EHubClientID.IsEmpty ? ZString.Empty : new ZString(Constants.CNSWClient.EHubClientStatusOK);

		public ZPropertyInfo EHubClientStatusInfo => GetZPropertyInfo(Schema.EHubClientStatus);

		#endregion

		#region Validation

		CNSWClientSettingValidation fValidation;
		public CNSWClientSettingValidation Validation => fValidation ?? (fValidation = new CNSWClientSettingValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateMachineName();
			Validation.ValidateSendFolder();
			Validation.ValidateReceiveFolder();
			Validation.ValidateErrorResponseFolder();
			Validation.ValidateArchiveFolder();
			Validation.ValidateAcdaSendFolder();
			Validation.ValidateAcdaReceiveFolder();
			Validation.ValidateAcdaErrorResponseFolder();
			Validation.ValidateAcdaArchiveFolder();
			Validation.ValidateRunningIntervalInSeconds();
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CNSWClientSetting(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			(clone as CNSWClientSetting).RegistryItemInternals = RegistryItemInternals;
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.MachineName, MachineName);
			writer.WriteElementString(Schema.SendFolder, SendFolder);
			writer.WriteElementString(Schema.ReceiveFolder, ReceiveFolder);
			writer.WriteElementString(Schema.ErrorResponseFolder, ErrorResponseFolder);
			writer.WriteElementString(Schema.ArchiveFolder, ArchiveFolder);
			writer.WriteElementString(Schema.AcdaSendFolder, AcdaSendFolder);
			writer.WriteElementString(Schema.AcdaReceiveFolder, AcdaReceiveFolder);
			writer.WriteElementString(Schema.AcdaErrorResponseFolder, AcdaErrorResponseFolder);
			writer.WriteElementString(Schema.AcdaArchiveFolder, AcdaArchiveFolder);
			writer.WriteElementString(Schema.RunningIntervalInSeconds, RunningIntervalInSeconds.ToString());
			writer.WriteElementString(Schema.EHubClientID, GetEHubClientID());
			writer.WriteElementString(Schema.EHubClientStatus, EHubClientStatus);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			MachineName = reader.ReadElementString(Schema.MachineName);
			SendFolder = reader.ReadElementString(Schema.SendFolder);
			ReceiveFolder = reader.ReadElementString(Schema.ReceiveFolder);
			ErrorResponseFolder = reader.ReadElementString(Schema.ErrorResponseFolder);
			ArchiveFolder = reader.ReadElementString(Schema.ArchiveFolder);
			AcdaSendFolder = reader.ReadElementString(Schema.AcdaSendFolder);
			AcdaReceiveFolder = reader.ReadElementString(Schema.AcdaReceiveFolder);
			AcdaErrorResponseFolder = reader.ReadElementString(Schema.AcdaErrorResponseFolder);
			AcdaArchiveFolder = reader.ReadElementString(Schema.AcdaArchiveFolder);
			RunningIntervalInSeconds = reader.ReadElementStringAsZInt(Schema.RunningIntervalInSeconds);
			EHubClientID = reader.ReadElementString(Schema.EHubClientID);
			OriginalEHubClientStatus = reader.ReadElementString(Schema.EHubClientStatus);
		}

		#endregion

		#region eHub Client Status

		ZString OriginalEHubClientStatus { get; set; }

		public bool EHubClientRegistered => EHubClientStatus == Constants.CNSWClient.EHubClientStatusOK;

		public bool ShouldRegisterEHubClient => !EHubClientID.IsEmpty;

		public bool ShouldUnregisterEHubClient => OriginalEHubClientStatus == Constants.CNSWClient.EHubClientStatusOK;

		#endregion
	}
}
