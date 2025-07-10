using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(ActiveDirectoryRegistry))]
	class ActiveDirectoryRegistryTest : RegistryItemSetTestCaseWithFactory<ActiveDirectoryRegistry>
	{
		public void TestActiveDirectoryIntegration_DefaultValue()
		{
			AssertEquals("Default should be Off", false, ActiveDirectoryRegistry.Instance.IsIntegrationEnabled);
		}

		public void TestSingleSignOn_DefaultValue()
		{
			Assert("Default is disabled", !ActiveDirectoryRegistry.Instance.IsSingleSignOnEnabled);
		}

		public void TestAllRegistryItemsAreOnlyEditableByEDISupportWhenHosted_OrMoreRestricted()
		{
			var allRegistryItems = typeof(ActiveDirectoryRegistry).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(property => typeof(IRegistryItem).IsAssignableFrom(property.PropertyType))
				.Select(property => property.GetValue(ActiveDirectoryRegistry.Instance, null))
				.Cast<IRegistryItem>();

			//Non-hosted
			EnvProxy.SetHostedLocationForTest("");
			foreach (var item in allRegistryItems)
			{
				bool isForEDI;
				if (SupportOnlyRegistryItems.Contains(item.Name))
				{
					isForEDI = (item.Options & RegistryOptions.IsOnlyForSupport) == RegistryOptions.IsOnlyForSupport;
				}
				else
				{
					isForEDI = (item.Options & RegistryOptions.Default) == RegistryOptions.Default;
				}
				Assert("Item " + item.Name + " should have RegistryOptions.IsOnlyEditableBySupportIfHosted", isForEDI);
			}

			//Hosted
			EnvProxy.SetHostedLocationForTest("SYD");
			RegistryItemDictionary.Instance.PurgeAll();

			var allowedRegistryOptionsForHosted = new[] { RegistryOptions.IsOnlyEditableBySupportIfHosted, RegistryOptions.IsOnlyForSupport, RegistryOptions.IsHidden };
			foreach (var item in allRegistryItems)
			{
				var isForEDI = allowedRegistryOptionsForHosted.Any(option => (item.Options & option) == option);
				Assert("Item " + item.Name + " should have RegistryOptions.IsOnlyEditableBySupportIfHosted", isForEDI);
			}
		}

		public void TestDefaultUserGroupPrefixes_WhenNotHosted()
		{
			AssertEquals(string.Empty, ItemSet.UserLoginPrefix.Value);
			AssertEquals(string.Empty, ItemSet.GroupNamePrefix.Value);
		}

		public void TestDefaultUserGroupPrefixes_WhenHosted()
		{
			EnvProxy.SetHostedLocationForTest("www.YourPlace.com");
			try
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				var enterpriseCode = registrationKey.EnterpriseCode;
				var databaseCode = registrationKey.ServerCode;

				var expectedUserPrefix = enterpriseCode + ".";
				var expectedGroupPrefix = string.Empty;

				AssertEquals(expectedUserPrefix, ItemSet.UserLoginPrefix.Value);
				AssertEquals(expectedGroupPrefix, ItemSet.GroupNamePrefix.Value);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(null);
			}
		}

		public void TestAllRegistryItemsAreUnderTheActiveDirectoryCategory()
		{
			foreach (var item in AllItems)
			{
				if (item.Name == "ADPasswordSettings")
				{
					AssertStartsWith(item.Name + " should be under the Password Control sub-category", "Password Control", item.Category);
				}
				else
				{
					AssertStartsWith(item.Name + " should be under the Active Directory sub-category", "System/Staff/Active Directory", item.Category);
				}
			}
		}

		public void TestAllActiveDirectoryRegistryItemsHaveThePreserveTestValueOption()
		{
			foreach (var item in AllItems)
			{
				Assert(item.Name + " should have the PreserveTestValue registry option set to true", item.Options.HasFlag(RegistryOptions.PreserveTestValue));
			}
		}

		public void TestDomainRegistryItems()
		{
			AssertEquals("System/Staff/Active Directory", ItemSet.DomainCredentialsCollection.Category);

			var credentials = ItemSet.DomainCredentialsCollection.Value;

			AssertEquals(0, credentials.Count);
		}

		public void TestDomainCollectionRegistryItem()
		{
			AssertEquals("System/Staff/Active Directory", ItemSet.DomainCredentialsCollection.Category);
			AssertEquals("The domain credentials collection should be empty by default", 0, ItemSet.DomainCredentialsCollection.Value.Count);
		}

		public void TestOneOffSyncMode()
		{
			AssertEquals("System/Staff/Active Directory", ItemSet.OneOffSyncMode.Category);
			Assert(ItemSet.OneOffSyncMode.HasOption(RegistryOptions.IsHidden));
			Assert(ItemSet.OneOffSyncMode.HasOption(RegistryOptions.NotCached));
			AssertEquals(RegistryStorageFlags.System, ItemSet.OneOffSyncMode.Storage);
		}

		public void TestSynchroniseDeactivation()
		{
			AssertEquals("Default should be Off", false, ActiveDirectoryRegistry.Instance.ShouldUnlinkInactiveStaff.Value);
		}

		public void TestWiseCloudAccessSecurityGroup_WhenHosted()
		{
			AssertEquals("System/Staff/Active Directory", ItemSet.WiseCloudAccessSecurityGroup.Category);
			EnvProxy.SetHostedLocationForTest("www.YourPlace.com");
			RegistryItemDictionary.Instance.PurgeAll();
			Assert(EnvProxy.IsHostedWithCargowise);
			Assert(ItemSet.WiseCloudAccessSecurityGroup.HasOption(RegistryOptions.IsOnlyForSupport));
			AssertEquals(RegistryStorageFlags.System, ItemSet.WiseCloudAccessSecurityGroup.Storage);
		}

		public void TestWiseCloudAccessSecurityGroup_WhenNotHosted()
		{
			EnvProxy.SetHostedLocationForTest("");
			Assert(!EnvProxy.IsHostedWithCargowise);
			Assert(ItemSet.WiseCloudAccessSecurityGroup.HasOption(RegistryOptions.IsHidden));
		}

		public void TestRoboticProcessAutomationGroup()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			Assert(EnvProxy.IsHostedWithCargowise);
			TestStringRegistryItem(ItemSet.RoboticProcessAutomationGroup,
				"RoboticProcessAutomationGroup",
				ActiveDirectoryRegistry.Categories.System_Staff_ActiveDirectory,
				(NoResString)"Robotic Process Automation Group",
				(NoResString)"Staff with Is Robot role will be added into this group in Active Directory.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				string.Empty,
				CharacterCase.Normal);

			//Non-hosted
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("RoboticProcessAutomationGroup");
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals(false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Non-Hosted Options", RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, ItemSet.RoboticProcessAutomationGroup.Options);
		}

		public void TestADPasswordSettings()
		{
			AssertEquals("Password Control", ItemSet.ADPasswordSettings.Category);
			AssertEquals(RegistryStorageFlags.System, ItemSet.ADPasswordSettings.Storage);

			// Not-hosted
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals("Is Not Hosted", false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Should be hidden when non-hosted", true, ItemSet.ADPasswordSettings.HasOption(RegistryOptions.IsHidden));

			// Hosted, AD on
			EnvProxy.SetHostedLocationForTest("HOM");
			AssertEquals("Is Hosted", true, EnvProxy.IsHostedWithCargowise);
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals("Should be shown and must override default when hosted and AD enabled", true, ItemSet.ADPasswordSettings.HasOption(RegistryOptions.MustOverrideDefaultValue));
			AssertEquals("Should be shown when hosted and AD enabled", false, ItemSet.ADPasswordSettings.HasOption(RegistryOptions.IsHidden));

			// Hosted, AD off
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals("Should be hidden when AD disabled", true, ItemSet.ADPasswordSettings.HasOption(RegistryOptions.IsHidden));
		}

		public void TestLastSuccessfulSyncByServiceTaskUTC()
		{
			Assert(ItemSet.LastSuccessfulSyncUTC.HasOption(RegistryOptions.IsHidden));
			Assert(ItemSet.LastSuccessfulSyncUTC.HasOption(RegistryOptions.NotCached));
			AssertEquals(SqlDateTime.MinValue.Value, ItemSet.LastSuccessfulSyncUTC.DefaultValue);
		}

		public void TestLastFailedSyncStaffPKs()
		{
			Assert(ItemSet.LastFailedSyncStaffPKs.HasOption(RegistryOptions.IsHidden));
			AssertEquals(0, ItemSet.LastFailedSyncStaffPKs.DefaultValue.Length);
		}

		public void TestLastFailedSyncGroupPKs()
		{
			Assert(ItemSet.LastFailedSyncGroupPKs.HasOption(RegistryOptions.IsHidden));
			AssertEquals(0, ItemSet.LastFailedSyncGroupPKs.DefaultValue.Length);
		}

		public void TestDisableADPasswordChange()
		{
			EnvProxy.SetHostedLocationForTest("");
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals("Category", "System/Staff/Active Directory/Users", ItemSet.DisableADPasswordChange.Category);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.DisableADPasswordChange.Storage);
			AssertEquals("DefaultValue", false, ItemSet.DisableADPasswordChange.DefaultValue);
			AssertEquals("RegistryOption.PreserveTestValue", true, ItemSet.DisableADPasswordChange.HasOption(RegistryOptions.PreserveTestValue));
			AssertEquals("Non-hosted, RegistryOptions.IsOnlyForSupport", false, ItemSet.DisableADPasswordChange.HasOption(RegistryOptions.IsOnlyForSupport));

			EnvProxy.SetHostedLocationForTest("SYD");
			RegistryItemDictionary.Instance.PurgeAll();
			AssertEquals("Hosted, RegistryOption.IsOnlyForSupport", true, ItemSet.DisableADPasswordChange.HasOption(RegistryOptions.IsOnlyForSupport));
		}

		public void TestADNotificationGroup()
		{
			AssertEquals("Name", "ADNotificationGroup", ItemSet.ADNotificationGroup.Name);
			AssertEquals("Category", ActiveDirectoryRegistry.Categories.System_Staff_ActiveDirectory, ItemSet.ADNotificationGroup.Category);
			AssertEquals("Caption", "Notification Group", ItemSet.ADNotificationGroup.Caption);
			AssertEquals("Hint", "This group will receive notification emails from Active Directory Synchronization (ADS) service task.", ItemSet.ADNotificationGroup.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ADNotificationGroup.Storage);
			AssertEquals("EditorInfo.FindBoxCollection", RegistryFindBoxCollection.GlbGroup, ((GuidFindBoxRegistryEditorInfo)ItemSet.ADNotificationGroup.EditorInfo).FindBoxCollection);
			AssertEquals("DefaultValue", RegistryConstants.GroupPKs.Notification, ItemSet.ADNotificationGroup.DefaultValue);
			AssertEquals("RegistryOption.PreserveTestValue", true, ItemSet.DisableADPasswordChange.HasOption(RegistryOptions.PreserveTestValue));

			// Non-hosted
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals("Hosting", false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Non-hosted, RegistryOptions.IsOnlyForSupport", false, ItemSet.ADNotificationGroup.HasOption(RegistryOptions.IsOnlyForSupport));

			//Hosted
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("ADNotificationGroup");
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("Hosting", true, EnvProxy.IsHostedWithCargowise);
			AssertEquals("Hosted, RegistryOptions.IsOnlyForSupport", true, ItemSet.ADNotificationGroup.HasOption(RegistryOptions.IsOnlyForSupport));
		}

		public void TestGroupSyncRegex()
		{
			// EDI
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				TestStringRegistryItem(ItemSet.GroupSyncRegex,
					"GroupSyncRegex",
					ActiveDirectoryRegistry.Categories.System_Staff_ActiveDirectory_Groups,
					(NoResString)"EDI Group Sync Regex",
					(NoResString)@"Only groups that match the following regex patterns will be synced to AD. The regex supports matching group descriptions (GG_Desc) and categories (GG_Category).
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
					RegistryStorageFlags.System,
					TextEditorType.TextBox,
					RegistryOptions.Default | RegistryOptions.PreserveTestValue,
					string.Empty,
					CharacterCase.Normal);
			}

			// Non-EDI
			((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Purge("GroupSyncRegex");
			AssertEquals("Non-EDI Options", RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, ItemSet.GroupSyncRegex.Options);

			AssertType<GroupSyncRegexRegistryDataType>(ItemSet.GroupSyncRegex.DataType);
		}

		protected IEnumerable<string> SupportOnlyRegistryItems
		{
			get
			{
				yield return "ReportHandledDirectoryExceptions";
			}
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "ADConfig";
				yield return "AttributeMapping";
				yield return "UserLoginPrefix";
				yield return "GroupNamePrefix";
				yield return "DomainCredentialsCollection";
				yield return "ShouldUnlinkInactiveStaff";
				yield return "ADPasswordSettings";
				yield return "DisableADPasswordChange";
			}
		}
	}
}
