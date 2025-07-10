using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	[XmlSerializerAssembly("Enterprise.Security.ActiveDirectory.XmlSerializers")]
	public class ADConfig : RegistryBusinessObjectTemplate
	{
		#region IsADIntegrationEnabled

		[ResourceStringData("ADConfig.IsADIntegrationEnabled", Caption = "Enabled")]
		public ZBool IsADIntegrationEnabled
		{
			get { return isADIntegrationEnabled; }
			set
			{
				if (isADIntegrationEnabled != value)
				{
					SetNonPersistentPropertyValue(IsADIntegrationEnabledInfo, ref isADIntegrationEnabled, value);

					if (!value)
					{
						IsSingleSignOn = false;
					}
					IsSingleSignOnInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}
		ZBool isADIntegrationEnabled;

		public ZPropertyInfo IsADIntegrationEnabledInfo => GetZPropertyInfo(nameof(IsADIntegrationEnabled));

		#endregion

		#region EntitiesToSync

		public EntitiesToSync EntitiesToSync
		{
			get
			{
				switch (EntitiesToSyncCode)
				{
					case "":
					case ActiveDirectory.EntitiesToSyncList.Codes.UsersAndGroups:
						return EntitiesToSync.UsersAndGroups;
					case ActiveDirectory.EntitiesToSyncList.Codes.Users:
						return EntitiesToSync.UsersOnly;
					default:
						return EntitiesToSync.UsersAndGroups;
				}
			}
			set
			{
				switch (value)
				{
					case EntitiesToSync.UsersOnly:
						EntitiesToSyncCode = ActiveDirectory.EntitiesToSyncList.Codes.Users;
						break;

					case EntitiesToSync.UsersAndGroups:
						EntitiesToSyncCode = ActiveDirectory.EntitiesToSyncList.Codes.UsersAndGroups;
						break;
				}
			}
		}

		[List("EntitiesToSyncList")]
		[ResourceStringData("ADConfig.EntitiesToSyncCode", Caption = "Entities to sync")]
		[MaxLength(3)]
		public ZString EntitiesToSyncCode
		{
			get { return entitiesToSyncCode; }
			set
			{
				SetNonPersistentPropertyValue(EntitiesToSyncCodeInfo, ref entitiesToSyncCode, value);
				ValidateEntitiesToSyncCode();
			}
		}
		ZString entitiesToSyncCode;

		void ValidateEntitiesToSyncCode()
		{
			if (new EntitiesToSyncList().ContainsCode(EntitiesToSyncCode))
			{
				EntitiesToSyncCodeInfo.ClearAllNotifications();
			}
			else
			{
				EntitiesToSyncCodeInfo.AddError(validationErrorMessage);
			}
		}
		ZString validationErrorMessage => Res.GetString("D597065C-2697-4B6C-B71D-725E9DDC367D", "Invalid Selection.");

		public ZPropertyInfo EntitiesToSyncCodeInfo => GetZPropertyInfo(nameof(EntitiesToSyncCode));

		protected bool EntitiesToSyncCode_ReadOnly => !IsADIntegrationEnabled;

		public CodeDescriptionPairList EntitiesToSyncList => new EntitiesToSyncList();

		#endregion

		#region SyncMode

		public SyncMode SyncMode
		{
			get
			{
				return GetSyncModeFromCode(SyncModeCode) ?? SyncMode.ADIsMaster;
			}
			set
			{
				SyncModeCode = GetCodeFromSyncMode(value);
			}
		}

		public static SyncMode? GetSyncModeFromCode(string code)
		{
			switch (code)
			{
				case ActiveDirectory.SyncModeList.Codes.ActiveDirectory:
					return SyncMode.ADIsMaster;
				case ActiveDirectory.SyncModeList.Codes.CargoWise:
					return SyncMode.EnterpriseIsMaster;
				default:
					return null;
			}
		}

		public static string GetCodeFromSyncMode(SyncMode mode)
		{
			switch (mode)
			{
				case SyncMode.ADIsMaster:
					return ActiveDirectory.SyncModeList.Codes.ActiveDirectory;
				case SyncMode.EnterpriseIsMaster:
					return ActiveDirectory.SyncModeList.Codes.CargoWise;
				default:
					throw new NotImplementedException();
			}
		}

		[List("SyncModeList")]
		[ResourceStringData("ADConfig.SyncModeCode", Caption = "Primary Data Source", FullDescription = "This will be used as the primary data source when synchronizing Staff which have just been linked with Active Directory.")]
		[MaxLength(3)]
		public ZString SyncModeCode
		{
			get { return syncModeCode; }
			set
			{
				SetNonPersistentPropertyValue(SyncModeCodeInfo, ref syncModeCode, value);
				ValidateSyncModeCode();
			}
		}
		ZString syncModeCode;

		void ValidateSyncModeCode()
		{
			if (SyncModeList.ContainsCode(SyncModeCode))
			{
				SyncModeCodeInfo.ClearAllNotifications();
			}
			else
			{
				SyncModeCodeInfo.AddError(validationErrorMessage);
			}
		}

		public ZPropertyInfo SyncModeCodeInfo => GetZPropertyInfo(nameof(SyncModeCode));

		protected bool SyncModeCode_ReadOnly => !IsADIntegrationEnabled;

		public CodeDescriptionPairList SyncModeList { get; } = new SyncModeList();

		#endregion

		#region SyncDirection

		public SyncDirection SyncDirection
		{
			get
			{
				return GetSyncDirectionFromCode(SyncDirectionCode) ?? SyncDirection.TwoWay;
			}
			set
			{
				SyncDirectionCode = GetCodeFromSyncDirection(value);
			}
		}

		public static SyncDirection? GetSyncDirectionFromCode(string code)
		{
			switch (code)
			{
				case ActiveDirectory.SyncDirectionList.Codes.TwoWay:
					return SyncDirection.TwoWay;
				case ActiveDirectory.SyncDirectionList.Codes.OneWay:
					return SyncDirection.OneWay;
				default:
					return null;
			}
		}

		public static string GetCodeFromSyncDirection(SyncDirection dir)
		{
			switch (dir)
			{
				case SyncDirection.TwoWay:
					return ActiveDirectory.SyncDirectionList.Codes.TwoWay;
				case SyncDirection.OneWay:
					return ActiveDirectory.SyncDirectionList.Codes.OneWay;
				default:
					throw new NotImplementedException();
			}
		}

		[List("SyncDirectionList")]
		[ResourceStringData("ADConfig.SyncDirectionCode", Caption = "Sync Direction (Staff)", FullDescription = "This determines if the sync is unidirectional or bidirectional for Staff.")]
		[MaxLength(4)]
		public ZString SyncDirectionCode
		{
			get { return syncDirectionCode; }
			set
			{
				SetNonPersistentPropertyValue(SyncDirectionCodeInfo, ref syncDirectionCode, value);
				ValidateSyncDirectionCode();
			}
		}
		ZString syncDirectionCode;

		void ValidateSyncDirectionCode()
		{
			if (SyncDirectionList.ContainsCode(SyncDirectionCode))
			{
				SyncDirectionCodeInfo.ClearAllNotifications();
			}
			else
			{
				SyncDirectionCodeInfo.AddError(validationErrorMessage);
			}
		}

		public ZPropertyInfo SyncDirectionCodeInfo => GetZPropertyInfo(nameof(SyncDirectionCode));

		protected bool SyncDirectionCode_ReadOnly => !IsADIntegrationEnabled;

		public CodeDescriptionPairList SyncDirectionList { get; } = new SyncDirectionList();

		#endregion

		#region SyncDirectionGroup

		public SyncDirection SyncDirectionGroup
		{
			get
			{
				return GetSyncDirectionFromCode(SyncDirectionGroupCode) ?? SyncDirection.TwoWay;
			}
			set
			{
				SyncDirectionGroupCode = GetCodeFromSyncDirection(value);
			}
		}

		[List("SyncDirectionList")]
		[ResourceStringData("ADConfig.SyncDirectionGroupCode", Caption = "Sync Direction (Group)", FullDescription = "This determines if the sync is unidirectional or bidirectional for Groups.")]
		[MaxLength(4)]
		public ZString SyncDirectionGroupCode
		{
			get { return syncDirectionGroupCode; }
			set
			{
				SetNonPersistentPropertyValue(SyncDirectionGroupCodeInfo, ref syncDirectionGroupCode, value);
				ValidateSyncDirectionGroupCode();
			}
		}
		ZString syncDirectionGroupCode;

		void ValidateSyncDirectionGroupCode()
		{
			if (SyncDirectionList.ContainsCode(SyncDirectionGroupCode))
			{
				SyncDirectionGroupCodeInfo.ClearAllNotifications();
			}
			else
			{
				SyncDirectionGroupCodeInfo.AddError(validationErrorMessage);
			}
		}

		public ZPropertyInfo SyncDirectionGroupCodeInfo => GetZPropertyInfo(nameof(SyncDirectionGroupCode));

		protected bool SyncDirectionGroupCode_ReadOnly => !IsADIntegrationEnabled;

		#endregion

		#region IsSingleSignOn

		[ResourceStringData("ADConfig.IsADIntegrationEnabled", Caption = "Enabled")]
		public ZBool IsSingleSignOn
		{
			get { return isSingleSignOn; }
			set { SetNonPersistentPropertyValue(IsSingleSignOnInfo, ref isSingleSignOn, value); }
		}
		ZBool isSingleSignOn;

		public ZPropertyInfo IsSingleSignOnInfo => GetZPropertyInfo(nameof(IsSingleSignOn));

		protected bool IsSingleSignOn_ReadOnly => !IsADIntegrationEnabled;

		#endregion

		public override bool Equals(object obj)
		{
			var config = obj as ADConfig;
			return config != null
				&& config.IsADIntegrationEnabled == IsADIntegrationEnabled
				&& config.EntitiesToSyncCode == EntitiesToSyncCode
				&& config.SyncModeCode == SyncModeCode
				&& config.IsSingleSignOn == IsSingleSignOn
				&& config.SyncDirectionCode == SyncDirectionCode
				&& config.SyncDirectionGroupCode == SyncDirectionGroupCode
				;
		}

		public override int GetHashCode()
		{
			return (IsADIntegrationEnabled.ToString() + EntitiesToSyncCode + SyncModeCode  + IsSingleSignOn.ToString() + SyncDirectionCode + SyncDirectionGroupCode).GetHashCode();
		}

		public static ADConfig DefaultValue
		{
			get
			{
				return new ADConfig
				{
					IsADIntegrationEnabled = ZBool.False,
					EntitiesToSync = EntitiesToSync.UsersAndGroups,
					SyncMode = SyncMode.ADIsMaster,
					IsSingleSignOn = ZBool.False,
					SyncDirection = SyncDirection.TwoWay,
					SyncDirectionGroup = SyncDirection.TwoWay
				};
			}
		}

		#region Serialization

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ADConfig
			{
				IsADIntegrationEnabled = IsADIntegrationEnabled,
				EntitiesToSync = EntitiesToSync,
				SyncMode = SyncMode,
				IsSingleSignOn = IsSingleSignOn,
				SyncDirection = SyncDirection,
				SyncDirectionGroup = SyncDirectionGroup,
			};
		}

		public new ADConfig Clone() => (ADConfig)GetClone(null, null);

		public void Read(XmlReaderWrapper reader)
		{
			reader.ReadElementString(Schema.ADConfig);
			ReadElements(reader);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsADIntegrationEnabled = reader.ReadElementStringAsZBool(Schema.IsADIntegrationEnabled);
			EntitiesToSyncCode = reader.ReadElementString(Schema.EntitiesToSyncCode);
			SyncModeCode = reader.ReadElementString(Schema.SyncModeCode);
			IsSingleSignOn = reader.ReadElementStringAsZBool(Schema.IsSingleSignOn);

			var syncDirection = reader.ReadElementString(Schema.SyncDirectionCode);
			SyncDirectionCode = string.IsNullOrEmpty(syncDirection) ? ActiveDirectory.SyncDirectionList.Codes.TwoWay : syncDirection;
			var syncDirectionGroupCode = reader.ReadElementString(Schema.SyncDirectionGroupCode);
			SyncDirectionGroupCode = string.IsNullOrEmpty(syncDirectionGroupCode) ? SyncDirectionCode : syncDirectionGroupCode;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsADIntegrationEnabled, IsADIntegrationEnabled.ToString());
			writer.WriteElementString(Schema.EntitiesToSyncCode, EntitiesToSyncCode);
			writer.WriteElementString(Schema.SyncModeCode, SyncModeCode);
			writer.WriteElementString(Schema.IsSingleSignOn, IsSingleSignOn.ToString());
			writer.WriteElementString(Schema.SyncDirectionCode, SyncDirectionCode);
			writer.WriteElementString(Schema.SyncDirectionGroupCode, SyncDirectionGroupCode);
		}

		static class Schema
		{
			internal const string ADConfig = "ADConfig";
			internal const string IsADIntegrationEnabled = "IsADIntegrationEnabled";
			internal const string EntitiesToSyncCode = "EntitiesToSyncCode";
			internal const string SyncModeCode = "SyncModeCode";
			internal const string IsSingleSignOn = "IsSingleSignOn";
			internal const string SyncDirectionCode = "SyncDirectionCode";
			internal const string SyncDirectionGroupCode = "SyncDirectionGroupCode";
		}

		#endregion
	}
}
