using System;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.CL.Manifest.Business.XmlSerializers")]
	public class CLSMSMessageSending : RegistryBusinessObjectTemplate, IxTCredentialProvider
	{
		#region Schema and Constructors

		protected abstract class Schema
		{
			public const string MachineName = "MachineName";
			public const string ApplicationNodeName = "ApplicationNodeName";
			public const string ApplicationNodePassword = "ApplicationNodePassword";
			public const string RunningIntervalInSeconds = "RunningIntervalInSeconds";
			public const string SendFolder = "SendFolder";
			public const string UnknownFolder = "UnknownFolder";
			public const string InvalidFolder = "InvalidFolder";
			public const string RejectedFolder = "RejectedFolder";
			public const string ReceiveFolder = "ReceiveFolder";
			public const string AcceptedFolder = "AcceptedFolder";
			public const string EnableSMSMessageSending = nameof(EnableSMSMessageSending);
			public const string XtCredentialStatus = nameof(XtCredentialStatus);
		}

		public static CLSMSMessageSending GetDefault()
		{
			var result = new CLSMSMessageSending();
			result.RunningIntervalInSeconds = 60;
			result.EnableSMSMessageSending = false;
			result.XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.New;

			return result;
		}

		public CLSMSMessageSending() : base()
		{
		}

		public CLSMSMessageSending(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new BusinessObjectFactory Factory => CurrentFactory;

		#endregion

		#region Properties

		ZString fMachineName;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|MachineName", Caption = "Machine Name")]
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

		ZString fApplicationNodeName;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|ApplicationNodeName", Caption = "Application Node Name")]
		[ReadOnlyMember(nameof(ApplicationNodeNameReadonly))]
		public ZString ApplicationNodeName
		{
			get => fApplicationNodeName;
			set
			{
				if (ApplicationNodeName != value)
				{
					CheckMaximumLength(ApplicationNodeNameInfo, value);
					SetNonPersistentPropertyValue(ApplicationNodeNameInfo, ref fApplicationNodeName, value.ToUpper());
					ApplicationNodeNameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ApplicationNodeNameInfo => GetZPropertyInfo(Schema.ApplicationNodeName);
		public bool ApplicationNodeNameReadonly => !GlbStaff.CurrentUser.GS_IsDeveloper;

		ZString fApplicationNodePassword;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|ApplicationNodePassword", Caption = "Application Node Password")]
		[ReadOnlyMember(nameof(ApplicationNodePasswordReadonly))]
		public ZString ApplicationNodePassword
		{
			get => fApplicationNodePassword;
			set
			{
				if (ApplicationNodePassword != value)
				{
					CheckMaximumLength(ApplicationNodePasswordInfo, value);
					SetNonPersistentPropertyValue(ApplicationNodePasswordInfo, ref fApplicationNodePassword, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateApplicationNodePassword();
					}
					ApplicationNodePasswordInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ApplicationNodePasswordInfo => GetZPropertyInfo(Schema.ApplicationNodePassword);
		public bool ApplicationNodePasswordReadonly => !(GlbStaff.CurrentUser.GS_IsDeveloper || XtCredentialStatusAllowsAction);

		ZInt fRunningIntervalInSeconds;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|RunningIntervalInSeconds", Caption = "Running Interval (in seconds)")]
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

		ZString fSendFolder;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|SendFolder", Caption = "Send Folder")]
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

		ZString fUnknownFolder;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|UnknownFolder", Caption = "Unknown Folder")]
		public ZString UnknownFolder
		{
			get => fUnknownFolder;
			set
			{
				if (UnknownFolder != value)
				{
					CheckMaximumLength(UnknownFolderInfo, value);
					SetNonPersistentPropertyValue(UnknownFolderInfo, ref fUnknownFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateUnknownFolder();
					}
					UnknownFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo UnknownFolderInfo => GetZPropertyInfo(Schema.UnknownFolder);

		ZString fInvalidFolder;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|InvalidFolder", Caption = "Invalid Folder")]
		public ZString InvalidFolder
		{
			get => fInvalidFolder;
			set
			{
				if (InvalidFolder != value)
				{
					CheckMaximumLength(InvalidFolderInfo, value);
					SetNonPersistentPropertyValue(InvalidFolderInfo, ref fInvalidFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateInvalidFolder();
					}
					InvalidFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo InvalidFolderInfo => GetZPropertyInfo(Schema.InvalidFolder);

		ZString fRejectedFolder;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|RejectedFolder", Caption = "Rejected Folder")]
		public ZString RejectedFolder
		{
			get => fRejectedFolder;
			set
			{
				if (RejectedFolder != value)
				{
					CheckMaximumLength(RejectedFolderInfo, value);
					SetNonPersistentPropertyValue(RejectedFolderInfo, ref fRejectedFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateRejectedFolder();
					}
					RejectedFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo RejectedFolderInfo => GetZPropertyInfo(Schema.RejectedFolder);

		ZString fReceiveFolder;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|ReceiveFolder", Caption = "Receive Folder")]
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

		ZString fAcceptedFolder;
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|AcceptedFolder", Caption = "Accepted Folder")]
		public ZString AcceptedFolder
		{
			get => fAcceptedFolder;
			set
			{
				if (AcceptedFolder != value)
				{
					CheckMaximumLength(AcceptedFolderInfo, value);
					SetNonPersistentPropertyValue(AcceptedFolderInfo, ref fAcceptedFolder, value.ToUpper());
					if (!IsValidationSuspended)
					{
						Validation.ValidateAcceptedFolder();
					}
					AcceptedFolderInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AcceptedFolderInfo => GetZPropertyInfo(Schema.AcceptedFolder);

		ZBool fEnableSMSMessageSending;
		[ReadOnlyMember(nameof(EnableSMSMessageSending_Readonly))]
		[ResourceStringData(
			"Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|EnableSMSMessageSending",
			Caption = "Enable SMS Message Sending",
			FullDescription = "Check this box to register to xT thereby enable SMS message sending."
		)]
		public ZBool EnableSMSMessageSending
		{
			get => fEnableSMSMessageSending;
			set
			{
				if (fEnableSMSMessageSending != value)
				{
					SetNonPersistentPropertyValue(EnableSMSMessageSendingInfo, ref fEnableSMSMessageSending, value);
					EnableSMSMessageSendingInfo.RefreshBinding();
				}
			}
		}
		public bool EnableSMSMessageSending_Readonly => !XtCredentialStatusAllowsAction;

		public ZPropertyInfo EnableSMSMessageSendingInfo => GetZPropertyInfo(nameof(EnableSMSMessageSending));

		ZString fXtCredentialStatus;
		[ReadOnlyMember(nameof(XtCredentialStatusReadonly))]
		[List(nameof(Lookups) + "." + nameof(CLSMSMessageSendingLookups.XtCredentialStatusList))]
		[ResourceStringData("Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending|XtCredentialStatus", Caption = "xT Credential Registry Status")]
		public ZString XtCredentialStatus
		{
			get => fXtCredentialStatus;
			set
			{
				if (fXtCredentialStatus != value)
				{
					SetNonPersistentPropertyValue(XtCredentialStatusInfo, ref fXtCredentialStatus, value);
					XtCredentialStatusInfo.RefreshBinding();
					EnableSMSMessageSendingInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo XtCredentialStatusInfo => GetZPropertyInfo(nameof(XtCredentialStatus));
		public bool XtCredentialStatusReadonly => !GlbStaff.CurrentUser.GS_IsDeveloper;

		#endregion

		#region xT credential

		bool XtCredentialStatusAllowsAction => XtCredentialStatus != CLSMSMessageXtCredentialStatusList.Codes.Awaiting;

		public XtCredentialAction RequiredXtCredentialAction
		{
			get
			{
				var result = XtCredentialAction.None;

				switch (XtCredentialStatus)
				{
					case CLSMSMessageXtCredentialStatusList.Codes.New:
					case CLSMSMessageXtCredentialStatusList.Codes.Error:
						if (applicationNodeValuesChanged && EnableSMSMessageSending)
						{
							result = XtCredentialAction.Update;
						}
						break;
					case CLSMSMessageXtCredentialStatusList.Codes.Registered:
						if (applicationNodeValuesChanged && EnableSMSMessageSending)
						{
							result = XtCredentialAction.Update;
						}
						else
						{
							result = XtCredentialAction.Delete;
						}
						break;
				}

				return result;
			}
		}

		public void XtCredentialRequestSent()
		{
			XtCredentialStatus = CLSMSMessageXtCredentialStatusList.Codes.Awaiting;
		}

		#endregion

		#region IxTCredentialProvider members

		string IxTCredentialProvider.Identifier => XtCredentialConstants.IdentifierList.CLC;

		ZGuid IxTCredentialProvider.LinkUniqueID
		{
			get
			{
				switch (CurrentFallbackLevel.Level)
				{
					case Integration.RegistryStorageFlags.Company:
						return CurrentFallbackLevel.CompanyPK(returnEmptyIfBranchPKIsPresent: false);
					case Integration.RegistryStorageFlags.Branch:
						return CurrentFallbackLevel.BranchPK;
					case Integration.RegistryStorageFlags.BranchDepartment:
						return CurrentFallbackLevel.DepartmentPK;
					default:
						return ZGuid.Empty;
				}
			}
		}

		string IxTCredentialProvider.LinkTableName
		{
			get
			{
				switch (CurrentFallbackLevel.Level)
				{
					case Integration.RegistryStorageFlags.Company:
						return GlbCompanySchema.Constants.TableName;
					case Integration.RegistryStorageFlags.Branch:
						return GlbBranchSchema.Constants.TableName;
					case Integration.RegistryStorageFlags.BranchDepartment:
						return GlbDepartmentSchema.Constants.TableName;
					default:
						return string.Empty;
				}
			}
		}

		XtCredentialAction IxTCredentialProvider.RequiredAction => RequiredXtCredentialAction;

		readonly ZDateTime fDateTime;
		DateTime IxTCredentialProvider.ChangeDateTime => (fDateTime.IsValid ? fDateTime : ZDateTime.Now).ToDateTime();

		string IxTCredentialProvider.EnterpriseCode => ApplicationNodeName;

		string IxTCredentialProvider.DatabaseCode => ZString.Empty;

		string IxTCredentialProvider.DatabaseNumber => ZString.Empty;

		string IxTCredentialProvider.Password => ApplicationNodePassword;

		string IxTCredentialProvider.OldPassword => originalApplicationNodePassword;

		BusinessObjectFactory IxTCredentialProvider.Factory => CurrentFactory;

		#endregion

		#region Validation

		CLSMSMessageSendingValidation fValidation;
		public CLSMSMessageSendingValidation Validation => fValidation ?? (fValidation = new CLSMSMessageSendingValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		CLSMSMessageSendingLookups fLookups;
		public CLSMSMessageSendingLookups Lookups => fLookups ?? (fLookups = new CLSMSMessageSendingLookups(this));

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CLSMSMessageSending(fallbackLevel, factory);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.MachineName, MachineName);
			writer.WriteElementString(Schema.ApplicationNodeName, ApplicationNodeName);
			writer.WriteElementString(Schema.ApplicationNodePassword, ApplicationNodePassword);
			writer.WriteElementString(Schema.RunningIntervalInSeconds, RunningIntervalInSeconds.ToString());
			writer.WriteElementString(Schema.SendFolder, SendFolder);
			writer.WriteElementString(Schema.UnknownFolder, UnknownFolder);
			writer.WriteElementString(Schema.InvalidFolder, InvalidFolder);
			writer.WriteElementString(Schema.RejectedFolder, RejectedFolder);
			writer.WriteElementString(Schema.ReceiveFolder, ReceiveFolder);
			writer.WriteElementString(Schema.AcceptedFolder, AcceptedFolder);
			writer.WriteElementString(Schema.EnableSMSMessageSending, EnableSMSMessageSending.ToString());
			writer.WriteElementString(Schema.XtCredentialStatus, XtCredentialStatus);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			MachineName = reader.ReadElementString(Schema.MachineName);
			ApplicationNodeName = reader.ReadElementString(Schema.ApplicationNodeName);
			ApplicationNodePassword = reader.ReadElementString(Schema.ApplicationNodePassword);
			RunningIntervalInSeconds = reader.ReadElementStringAsZInt(Schema.RunningIntervalInSeconds);
			SendFolder = reader.ReadElementString(Schema.SendFolder);
			UnknownFolder = reader.ReadElementString(Schema.UnknownFolder);
			InvalidFolder = reader.ReadElementString(Schema.InvalidFolder);
			RejectedFolder = reader.ReadElementString(Schema.RejectedFolder);
			ReceiveFolder = reader.ReadElementString(Schema.ReceiveFolder);
			AcceptedFolder = reader.ReadElementString(Schema.AcceptedFolder);
			EnableSMSMessageSending = reader.ReadElementStringAsZBool(Schema.EnableSMSMessageSending);
			XtCredentialStatus = reader.ReadElementString(Schema.XtCredentialStatus);

			CaptureOriginalValueOfApplicationNode();
		}

		protected override FallbackLevel CurrentFallbackLevelCore
		{
			get => base.CurrentFallbackLevelCore;
			set
			{
				base.CurrentFallbackLevelCore = value;
				SetApplicationNodeDefault(value);
			}
		}

		#endregion

		bool isReadingElementsForFirstTime = true;
		ZString originalApplicationNodeName;
		ZString originalApplicationNodePassword;
		bool originalEnableSMSMessageSending;
		void CaptureOriginalValueOfApplicationNode()
		{
			if (isReadingElementsForFirstTime)
			{
				originalApplicationNodeName = ApplicationNodeName;
				originalApplicationNodePassword = ApplicationNodePassword;
				originalEnableSMSMessageSending = EnableSMSMessageSending;
				isReadingElementsForFirstTime = false;
			}
		}
		bool applicationNodeValuesChanged =>
			originalApplicationNodeName != ApplicationNodeName
			|| originalApplicationNodePassword != ApplicationNodePassword
			|| originalEnableSMSMessageSending != EnableSMSMessageSending;

		void SetApplicationNodeDefault(FallbackLevel fallbackLevel)
		{
			using (SuspendSettingHasChanges())
			{
				if (ApplicationNodeName.IsEmpty)
				{
					ApplicationNodeName = GetDefaultApplicationNodeName(fallbackLevel, CurrentFactory);
				}

				if (ApplicationNodePassword.IsEmpty)
				{
					ApplicationNodePassword = GeneratePassword(10);
				}
			}
		}

		static ZString GetDefaultApplicationNodeName(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = ZString.Empty;

			if (fallbackLevel != null)
			{
				var companyPk = new ZGuid(fallbackLevel.CompanyPK(returnEmptyIfBranchPKIsPresent: false));
				if (companyPk.IsValid)
				{
					if (factory.Load<GlbCompany>(companyPk) is GlbCompany currentCompany)
					{
						result = currentCompany.LicenceKeyIdentifier + "_CLSMS";
					}
				}
			}

			return result;
		}

		static string GeneratePassword(int length)
		{
			var buffer = new byte[length];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(buffer);

			var charArray = new char[length];

			for (var iter = 0; iter < length; iter++)
			{
				var i = buffer[iter] % 62;
				if (i < 10)
				{
					charArray[iter] = (char)('0' + i);
				}
				else if (i < 36)
				{
					charArray[iter] = (char)('A' + i - 10);
				}
				else
				{
					charArray[iter] = (char)('a' + i - 36);
				}
			}

			return new string(charArray);
		}
	}
}
