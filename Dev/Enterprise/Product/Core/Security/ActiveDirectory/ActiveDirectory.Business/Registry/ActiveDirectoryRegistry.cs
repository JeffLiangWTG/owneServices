using System;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Security.ActiveDirectory.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public sealed class ActiveDirectoryRegistry : RegistryItemSet
	{
		#region Singleton Pattern

		public static ActiveDirectoryRegistry Instance
		{
			get { return instance ?? (instance = new ActiveDirectoryRegistry()); }
		}

		[ThreadStatic]
		static ActiveDirectoryRegistry instance;

		ActiveDirectoryRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString System_Staff_ActiveDirectory { get { return CombineCategories(RawDataRegistry.Categories.System_Staff, ResString.GetMultilingualString("a6406e39-ec21-499c-bc32-92823aa463c5", "Active Directory")); } }
			public static MultilingualString System_Staff_ActiveDirectory_Users { get { return CombineCategories(System_Staff_ActiveDirectory, ResString.GetMultilingualString("6b769c1b-f1fe-4c95-8bc7-f9de43edde4b", "Users")); } }
			public static MultilingualString System_Staff_ActiveDirectory_Groups { get { return CombineCategories(System_Staff_ActiveDirectory, ResString.GetMultilingualString("c1ea2f3d-66db-4242-a901-180bd15b2c36", "Groups")); } }
		}

		#endregion

		#region ActiveDirectoryConfig

		[Obsolete("Use the other properties in this class to get the actual config option you need. The registry item won't have the correct value during the initial sync because validation invokes the sync before committing the value.")]
		public ADConfigRegistryItem ActiveDirectoryConfig
		{
			get
			{
				return GetItem("ADConfig", () =>
				{
					return new ADConfigRegistryItem(
						name: "ADConfig",
						category: Categories.System_Staff_ActiveDirectory,
						caption: ResString.GetMultilingualString("97bfb066-ad30-4113-8724-ab84ac30e964", "Enable Integration"),
						hint: ResString.GetMultilingualString("8695f651-8d2c-4101-ba5a-e06f1ba27cee", "Enables the integration of Active Directory users and groups with {0}. When integration is enabled, domain user accounts are used to gain access to {0}.", BrandingFactory.Instance.ProductName),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default) | RegistryOptions.PreserveTestValue,
						defaultValue: ADConfig.DefaultValue);
				});
			}
		}

		public ADConfig ActiveConfigOverride { private get; set; }

		ADConfig ActiveConfig
		{
#pragma warning disable 0618
			get { return ActiveConfigOverride ?? ActiveDirectoryConfig.Value; }
#pragma warning restore 0618
		}

		public bool IsIntegrationEnabled
		{
			get { return ActiveConfig.IsADIntegrationEnabled; }
			set { SetConfig(c => c.IsADIntegrationEnabled = value); }
		}

		public bool IsSingleSignOnEnabled
		{
			get { return ActiveConfig.IsSingleSignOn; }
			set { SetConfig(c => c.IsSingleSignOn = value); }
		}

		public EntitiesToSync EntitiesToSync
		{
			get { return ActiveConfig.EntitiesToSync; }
			set { SetConfig(c => c.EntitiesToSync = value); }
		}

		public SyncMode SyncMode
		{
			get { return ActiveConfig.SyncMode; }
			set { SetConfig(c => c.SyncMode = value); }
		}

		public SyncDirection SyncDirection
		{
			get { return ActiveConfig.SyncDirection; }
			set { SetConfig(c => c.SyncDirection = value); }
		}

		public SyncDirection SyncDirectionGroup
		{
			get { return ActiveConfig.SyncDirectionGroup; }
			set { SetConfig(c => c.SyncDirectionGroup = value); }
		}

		void SetConfig(Action<ADConfig> configSetter)
		{
			var adConfig = ActiveConfig;
			configSetter(adConfig);

#pragma warning disable 0618
			ActiveDirectoryConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, adConfig);
#pragma warning restore 0618
		}

		#endregion

		#region AttributeMapping

		public AttributeMapRegistryItem AttributeMapping
		{
			get
			{
				return GetItem("AttributeMapping", () =>
				{
					return new AttributeMapRegistryItem(
						name: "AttributeMapping",
						category: Categories.System_Staff_ActiveDirectory,
						caption: ResString.GetMultilingualString("78057104-dc25-4f47-9e7e-90759f1d41c6", "Attribute Mapping"),
						hint: ResString.GetMultilingualString("84ca5c50-b5d8-4abe-9199-12361bf40ae4", @"Allows Active Directory attributes to be mapped to their counterpart fields in {0}.

When changing the mapping to an AD attribute other than the default, please ensure that the attribute is writable and its type is compatible to the {0} field.

If a custom attribute is used in the mapping, ensure the custom attribute is defined in all of the domain(s) that AD Integration is enabled with.",
Core.Constants.ProductName),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default) | RegistryOptions.PreserveTestValue,
						defaultValue: AttributeMap.DefaultMap);
				});
			}
		}

		#endregion

		#region ReportHandledDirectoryExceptions

		public BooleanRegistryItem ReportHandledDirectoryExceptions
		{
			get
			{
				return GetItem("ReportHandledDirectoryExceptions", () =>
					new BooleanRegistryItem("ReportHandledDirectoryExceptions",
						Categories.System_Staff_ActiveDirectory,
						(NoResString)"Report Handled Directory Exceptions",
						(NoResString)"Sends error reports for exceptions normally handled and shown to the user in a message box.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						false));
			}
		}

		#endregion

		#region WiseCloudAccessSecurityGroup

		public StringArrayRegistryItem WiseCloudAccessSecurityGroup
		{
			get
			{
				return GetItem("WiseCloudAccessSecurityGroup", delegate
				{
					var result = new StringArrayRegistryItem(
						"WiseCloudAccessSecurityGroup",
						Categories.System_Staff_ActiveDirectory,
						(NoResString)"WiseCloud Access Security Group",
						(NoResString)"Newly created Active Directory users will be placed into the following Active Directory groups in each domain the system is integrated with. Existing users will not be affected.",
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden) | RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		#endregion

		#region RoboticProcessAutomationGroup

		public StringRegistryItem RoboticProcessAutomationGroup
		{
			get
			{
				return GetItem("RoboticProcessAutomationGroup", delegate
				{
					var result = new StringRegistryItem(
						"RoboticProcessAutomationGroup",
						Categories.System_Staff_ActiveDirectory,
						(NoResString)"Robotic Process Automation Group",
						(NoResString)"Staff with Is Robot role will be added into this group in Active Directory.",
						RegistryStorageFlags.System,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden) | RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		#endregion

		#region Login Prefixes

		public StringRegistryItem UserLoginPrefix
		{
			get
			{
				return GetItem("UserLoginPrefix", () =>
				{
					return new StringRegistryItem(
						name: "UserLoginPrefix",
						category: Categories.System_Staff_ActiveDirectory_Users,
						caption: ResString.GetMultilingualString("f5c9e794-4895-4631-b739-02c29e4e556f", "Login Name Prefix"),
						hint: ResString.GetMultilingualString("e35bdf09-43cb-4347-804c-51ea5df2dc4e", "The prefix which all staff login names are required to begin with."),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default) | RegistryOptions.PreserveTestValue,
						defaultValue: DefaultUserLoginPrefix
						);
				});
			}
		}

		public StringRegistryItem GroupNamePrefix
		{
			get
			{
				return GetItem("GroupNamePrefix", () =>
				{
					return new StringRegistryItem(
						name: "GroupNamePrefix",
						category: Categories.System_Staff_ActiveDirectory_Groups,
						caption: ResString.GetMultilingualString("4238e72f-d0f6-4949-bd00-74eaf2299fac", "Group Name Prefix"),
						hint: ResString.GetMultilingualString("9666521d-ebee-41ce-9181-49113b6bca87", "Prefix which all group descriptions are required to begin with."),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default) | RegistryOptions.PreserveTestValue
						);
				});
			}
		}

		static string DefaultUserLoginPrefix
		{
			get
			{
				if (EnvProxy.IsHostedWithCargowise)
				{
					var enterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;

					return !string.IsNullOrEmpty(enterpriseCode) ? enterpriseCode + "." : string.Empty;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		#endregion

		#region Disable AD Password Password Change

		public BooleanRegistryItem DisableADPasswordChange =>
	GetItem("DisableADPasswordChange", () =>
			new BooleanRegistryItem(
				name: "DisableADPasswordChange",
				category: Categories.System_Staff_ActiveDirectory_Users,
				caption: ResString.GetMultilingualString("16C00114-BA3E-4873-BB64-09D322972A4B", "Disable AD Password Change"),
				hint: ResString.GetMultilingualString("7C5036FF-19B1-4438-B862-7FA8C551B780", @"Disable Active Directory password change from {0}.
When it is disabled, password change is not allowed for AD Integrated system. If password change is required, it must be performed outside of {0}, for example in Windows.",
					Core.Constants.ProductName),
				storage: RegistryStorageFlags.System,
				options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default) | RegistryOptions.PreserveTestValue,
				false));

		#endregion

		#region Domain Credentials Collection

		public DomainCredentialsCollectionRegistryItem DomainCredentialsCollection
		{
			get
			{
				return GetItem("DomainCredentialsCollection", () =>
				{
					return new DomainCredentialsCollectionRegistryItem(
					name: "DomainCredentialsCollection",
					category: Categories.System_Staff_ActiveDirectory,
					caption: ResString.GetMultilingualString("2405D244-C4E6-4EA6-AA0F-233F925B6679", "Domain Credentials Collection"),
					hint: ResString.GetMultilingualString("71986953-12F5-499E-ADA1-A9444D5B3957", @"Setup the various domains you want {0} to integrate with.

- Domain Name: this is the Fully-Qualified Domain Name.
- Domain User Name: this is the name of a domain user with write access to the Users and Groups Organizational units.
- Domain User Password: this is the password for the use domain defined above.
- Default Domain: this is the domain used by {0} to create a new user if a new staff record does not specify a domain name.
- Users' Organizational Unit: {0} will only match {0} staffs with AD users located inside this organizational unit.
- Groups' Organization Unit:  {0} will only match {0} groups with AD groups located inside this organizational unit.
- Default Password: this is the password set for new users created in Active Directory by {0}.

After changing this registry item it is advisable to restart {0}.", Core.Constants.ProductName),
					storage: RegistryStorageFlags.System,
					options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default) | RegistryOptions.PreserveTestValue,
					defaultValue: new DomainCredentialsCollection());
				});
			}
		}

		#endregion

		public StringRegistryItem OneOffSyncMode
		{
			get
			{
				return GetItem("OneOffSyncMode", () =>
				{
					return new StringRegistryItem(
						name: "OneOffSyncMode",
						category: Categories.System_Staff_ActiveDirectory,
						caption: null,
						hint: null,
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsHidden | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue);
				});
			}
		}

		#region Should Unlink Inactive Staff

		public BooleanRegistryItem ShouldUnlinkInactiveStaff
		{
			get
			{
				return GetItem("ShouldUnlinkInactiveStaff", () =>
				{
					return new BooleanRegistryItem(
						name: "ShouldUnlinkInactiveStaff",
						category: Categories.System_Staff_ActiveDirectory_Users,
						caption: ResString.GetMultilingualString("3b05ccd4-40d0-4884-a408-491066ed597c", "Should Unlink Inactive Staff?"),
						hint: ResString.GetMultilingualString("1ffe870e-f6fb-42df-818f-bb59ef344b8c", @"By default, inactive staff are still linked to Active Directory and continue to synchronize with Active Directory including their active status. Deactivating a staff will cause its linked Active Directory user to be disabled by default.

When setting this registry to 'Yes', newly deactivated staff will be unlinked from Active Directory and will no longer be synchronized. The corresponding Active Directory users will not be disabled."),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default) | RegistryOptions.PreserveTestValue,
						defaultValue: false
						);
				});
			}
		}

		#endregion

		#region AD Password Settings

		public ADPasswordSettingsRegistryItem ADPasswordSettings =>
			GetItem("ADPasswordSettings", () =>
					new ADPasswordSettingsRegistryItem(
						name: "ADPasswordSettings",
						category: RawDataRegistry.Categories.PasswordControl,
						caption: ResString.GetMultilingualString("9D0B341F-1028-4F1A-9EAE-15494AC100AD", "AD Password Settings"),
						hint: ResString.GetMultilingualString("2488B413-24A6-40A3-B726-8BC2B6049FC5", @"Create password settings to override domain password policy in Active Directory."),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise && IsIntegrationEnabled ? RegistryOptions.MustOverrideDefaultValue : RegistryOptions.IsHidden) | RegistryOptions.PreserveTestValue
						)
					);

		#endregion

		#region Last Successful Sync UTC

		public DateTimeRegistryItem LastSuccessfulSyncUTC
		{
			get
			{
				return GetItem("LastSuccessfulSyncUTC", () =>
				{
					return new DateTimeRegistryItem(
						name: "LastSuccessfulSyncUTC",
						category: Categories.System_Staff_ActiveDirectory,
						caption: null,
						hint: null,
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.NotCached | RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue,
						defaultValue: SqlDateTime.MinValue.Value
					);
				});
			}
		}

		#endregion

		#region Last Failed Sync Staff PKs

		public GuidArrayRegistryItem LastFailedSyncStaffPKs
		{
			get
			{
				return GetItem("LastFailedSyncStaffPKs", () =>
				{
					return new GuidArrayRegistryItem(
						name: "LastFailedSyncStaffPKs",
						category: Categories.System_Staff_ActiveDirectory,
						caption: null,
						hint: null,
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue,
						defaultValue: Array.Empty<Guid>()
					);
				});
			}
		}

		#endregion

		#region Last Failed Sync Group PKs

		public GuidArrayRegistryItem LastFailedSyncGroupPKs
		{
			get
			{
				return GetItem("LastFailedSyncGroupPKs", () =>
				{
					return new GuidArrayRegistryItem(
						name: "LastFailedSyncGroupPKs",
						category: Categories.System_Staff_ActiveDirectory,
						caption: null,
						hint: null,
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue,
						defaultValue: Array.Empty<Guid>()
					);
				});
			}
		}

		#endregion

		#region AD Notification Group

		public GuidRegistryItem ADNotificationGroup
		{
			get
			{
				return GetItem("ADNotificationGroup", () =>
				{
					var result = new GuidRegistryItem(
						name: "ADNotificationGroup",
						category: Categories.System_Staff_ActiveDirectory,
						caption: ResString.GetMultilingualString("263AD2F1-9ED8-4D8B-B552-D54446184446", "Notification Group"),
						hint: ResString.GetMultilingualString("4F24D6C8-E92B-4B94-A883-AFDCD88F581B", "This group will receive notification emails from Active Directory Synchronization (ADS) service task."),
						storage: RegistryStorageFlags.System,
						options: (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.Default) | RegistryOptions.PreserveTestValue,
						defaultValue: RegistryConstants.GroupPKs.Notification);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Group to sync Regex for ediProd

		public StringRegistryItem GroupSyncRegex
		{
			get
			{
				return GetItem("GroupSyncRegex", () =>
				{
					var item = new StringRegistryItem(
						name: "GroupSyncRegex",
						category: Categories.System_Staff_ActiveDirectory_Groups,
						caption: (NoResString)"EDI Group Sync Regex",
						hint: (NoResString)@$"Only groups that match the following regex patterns will be synced to AD. The regex supports matching group descriptions ({GlbGroupSchema.Constants.GG_Desc}) and categories ({GlbGroupSchema.Constants.GG_Category}).
Use these formats:
* <DescRegex> - Match by group description.
* Category=<CategoryRegex> - Match by group category.
* <DescRegex>&&Category=<CategoryRegex> - Match by both description AND category.
* <DescRegex>||Category=<CategoryRegex> - Match by either description OR category.

Examples:
^Core - syncs groups with descriptions starting with 'Core'.
^Core|Release Group$ - syncs groups with descriptions starting with 'Core' or ending with 'Release Group'.
Category=^S|^CA2$ - syncs groups with categories starting with 'S' or exactly 'CA2'.
^Core&&Category=^CA2$ - syncs groups with descriptions starting with 'Core' and categories matching 'CA2'.
^Core||Category=^CA2$ - syncs groups with descriptions starting with 'Core' or categories matching 'CA2'.

The match is not case-sensitive.",
						storage: RegistryStorageFlags.System,
						options: (ClientHookLoader.Instance.Client == Clients.EDI ? RegistryOptions.Default : RegistryOptions.IsHidden) | RegistryOptions.PreserveTestValue
						);

					item.DataType = new GroupSyncRegexRegistryDataType();
					return item;
				});
			}
		}

		#endregion

	}
}
