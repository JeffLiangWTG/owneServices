using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebServicesConfig : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Name = "Name";
			public const string Version = "Version";
			public const string IsEnabled = "IsEnabled";
			public const string IsAutoManaged = "IsAutoManaged";
			public const string IsCustomURL = "IsCustomURL";
			public const string URL = "URL";
			public const string NumberOfServerClusters = "NumberOfServerClusters";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebServicesConfig()
			{
				Name = Name,
				IsEnabled = IsEnabled,
				IsAutoManaged = IsAutoManaged,
				IsCustomURL = IsCustomURL,
				URL = URL,
				NumberOfServerClusters = NumberOfServerClusters,
			};
		}

		#endregion

		#region Properties

		[ReadOnly(true)]
		[MaxLength(50)]
		public ZString Name
		{
			get
			{
				return name;
			}
			set
			{
				SetNonPersistentPropertyValue(NameInfo, ref name, value);
			}
		}
		ZString name;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(Schema.Name);

		[ReadOnlyMember(nameof(URLReadOnly))]
		public ZString URL
		{
			get
			{
				return url;
			}
			set
			{
				if (value != url)
				{
					SetNonPersistentPropertyValue(URLInfo, ref url, value);

					if (!IsValidationSuspended)
					{
						ValidateUrl();
					}
				}
			}
		}
		ZString url;

		public ZPropertyInfo URLInfo => GetZPropertyInfo(Schema.URL);

		bool URLReadOnly => !IsCustomURL || WebServiceReadOnly;

		public void ValidateUrl()
		{
			URLInfo.ClearAllNotifications();

			if (IsCustomURL && URL.IsEmpty)
			{
				URLInfo.AddError(Res.GetString("9A29B25E-BB4C-4344-9548-A516A81792FB", "Field cannot be empty when Custom URL is enabled."));
			}

			if (IsAutoManaged && !URLInfo.OriginalValue.IsEmpty)
			{
				URLInfo.AddWarning(Res.GetString("6B438ECC-6685-435E-8E36-46E033783907", "Warning: Changing this field will cause the previous web service to tear down."));
			}
		}

		[ReadOnlyMember(nameof(WebServiceReadOnly))]
		public ZBool IsEnabled
		{
			get
			{
				return isEnabled;
			}
			set
			{
				if (value != isEnabled)
				{
					SetNonPersistentPropertyValue(IsEnabledInfo, ref isEnabled, value);

					if (!IsValidationSuspended)
					{
						ValidateIsEnabled();
					}
				}
			}
		}
		ZBool isEnabled;

		public void ValidateIsEnabled()
		{
			IsEnabledInfo.ClearAllNotifications();

			if (IsAutoManaged && !IsEnabled)
			{
				IsEnabledInfo.AddWarning(Res.GetString("7A6065F3-9922-46D9-86B2-3658E330C962", "Warning: This action will cause this web service to tear down."));
			}
		}

		public ZPropertyInfo IsEnabledInfo => GetZPropertyInfo(Schema.IsEnabled);

		[ReadOnlyMember(nameof(WebServiceReadOnly))]
		public ZBool IsCustomURL
		{
			get
			{
				return isCustomURL;
			}
			set
			{
				SetNonPersistentPropertyValue(IsCustomURLInfo, ref isCustomURL, value);
				if (!isCustomURL)
				{
					URL = ZString.Empty;
				}
			}
		}
		ZBool isCustomURL = false;

		public ZPropertyInfo IsCustomURLInfo => GetZPropertyInfo(Schema.IsCustomURL);

		[ReadOnly(false)]
		public ZBool IsAutoManaged
		{
			get
			{
				return isAutoManaged;
			}
			set
			{
				SetNonPersistentPropertyValue(IsAutoManagedInfo, ref isAutoManaged, value);
			}
		}
		ZBool isAutoManaged = true;

		public ZPropertyInfo IsAutoManagedInfo => GetZPropertyInfo(Schema.IsAutoManaged);

		[ReadOnly(false)]
		public ZShort NumberOfServerClusters
		{
			get
			{
				return numberOfServerClusters;
			}
			set
			{
				SetNonPersistentPropertyValue(NumberOfServerClustersInfo, ref numberOfServerClusters, value);
			}
		}
		ZShort numberOfServerClusters;

		public ZPropertyInfo NumberOfServerClustersInfo => GetZPropertyInfo(Schema.NumberOfServerClusters);

		#endregion

		bool WebServiceReadOnly => (!isAutoManaged && !Env.CurrentUser.IsSupportUser);

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Name = ZString.Empty;
			URL = ZString.Empty;
			IsEnabled = false;
			IsCustomURL = false;
			IsAutoManaged = true;
			NumberOfServerClusters = 1;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateUrl();
		}

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Name = reader.ReadElementString(Schema.Name);
			IsEnabled = reader.ReadElementStringAsZBool(Schema.IsEnabled);
			IsAutoManaged = reader.ReadElementStringAsZBool(Schema.IsAutoManaged);
			IsCustomURL = reader.ReadElementStringAsZBool(Schema.IsCustomURL);
			URL = reader.ReadElementString(Schema.URL);
			NumberOfServerClusters = reader.ReadElementStringAsZShort(Schema.NumberOfServerClusters);
			Version = reader.ReadElementStringAsZInt(Schema.Version);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Name, Name);
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
			writer.WriteElementString(Schema.IsAutoManaged, IsAutoManaged.ToString());
			writer.WriteElementString(Schema.IsCustomURL, IsCustomURL.ToString());
			writer.WriteElementString(Schema.URL, URL);
			writer.WriteElementString(Schema.NumberOfServerClusters, NumberOfServerClusters.ToString());
			writer.WriteElementString(Schema.Version, Version.ToString());
		}

		#endregion

		ZInt Version { get; set; } = 1;
	}
}
