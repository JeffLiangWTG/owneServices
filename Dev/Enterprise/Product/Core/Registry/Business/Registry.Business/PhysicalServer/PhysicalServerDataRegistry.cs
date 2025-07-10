using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class PhysicalServerDataRegistry : RegistryItemSet
	{
		PhysicalServerDataRegistry()
		{
		}

		public static PhysicalServerDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new PhysicalServerDataRegistry();
				}
				return fInstance;
			}
		}
		[ThreadStatic]
		static PhysicalServerDataRegistry fInstance;

		static MultilingualString PhysicalServerCategory { get { return RawDataRegistry.Categories.PhysicalServer; } }

		static MultilingualString PhysicalServer_SMTPCategory => RawDataRegistry.Categories.PhysicalServer_SMTP;

		public override bool IsForProductivityWise => true;

		#region StmUpgrade Being Upgraded To

		public GuidRegistryItem StmUpgradeBeingUpgradedTo
		{
			get
			{
				return GetItem<GuidRegistryItem>("StmUpgradeBeingUpgradedTo", delegate
				{
					return new GuidRegistryItem(
						"StmUpgradeBeingUpgradedTo",
						PhysicalServerCategory,
						(NoResString)"StmUpgrade Being Upgraded To",
						(NoResString)"The PK of the StmUpgrade being upgraded to.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue,
						Guid.Empty);
				});
			}
		}

		#endregion

		#region Version Being Upgraded From

		public StringRegistryItem VersionBeingUpgradedFrom
		{
			get
			{
				return GetItem<StringRegistryItem>("VersionBeingUpgradedFrom", delegate
				{
					return new StringRegistryItem(
						"VersionBeingUpgradedFrom",
						PhysicalServerCategory,
						(NoResString)"Version Being Upgraded From",
						(NoResString)"The product version that is being upgraded from.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue,
						"");
				});
			}
		}

		#endregion

		#region SmsConfiguration
		#region SuppressResourceStringsCheckRegion

		public ServerUsernamePasswordConfigurationRegistryItem SmsConfiguration
		{
			get
			{
				return GetItem<ServerUsernamePasswordConfigurationRegistryItem>(
					"SmsConfiguration",
					new CreateItemDelegate<ServerUsernamePasswordConfigurationRegistryItem>(GetSmsConfiguration));
			}
		}

		ServerUsernamePasswordConfigurationRegistryItem GetSmsConfiguration()
		{
			ServerUsernamePasswordConfigurationRegistryItem result = new ServerUsernamePasswordConfigurationRegistryItem("SmsConfiguration", PhysicalServerCategory, (NoResString)"SMS Configuration", (NoResString)"Setup your SMS User Name and Login details.", RegistryStorageFlags.System);
			result.Options = RegistryOptions.IsHidden;
			return result;
		}

		#endregion
		#endregion

		#region SecondarySMTPServer

		public SecondarySMTPServersRegistryItem SecondarySMTPServers
		{
			get
			{
				return GetItem("SecondarySMTPServers", delegate
				{
					return new SecondarySMTPServersRegistryItem(
						"SecondarySMTPServers",
						PhysicalServer_SMTPCategory,
						ResString.GetMultilingualString("d70909df-7f70-453a-a8ed-63f62d96aacc", "Secondary SMTP Servers"),
						ResString.GetMultilingualString("4abc5b4c-dfaa-42ba-8f51-3541d52f7919", "This setting enables additional SMTP servers that will be used when the sender’s email address matches a Supported Domain. \r\n\r\nPlease note that the secondary SMTP Servers does not support servers that require OAuth 2.0 authentication."),
						RegistryStorageFlags.System,
						RawDataRegistry.Instance.SMTPServer.Options);
				});
			}
		}

		#endregion
	}
}
