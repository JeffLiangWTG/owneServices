using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PhysicalServerDataRegistry))]
	sealed class PhysicalServerDataRegistryTest : RegistryItemSetTestCaseWithFactory<PhysicalServerDataRegistry>
	{
		public void TestStmUpgradeBeingUpgradedTo()
		{
			Assert("StmUpgradeBeingUpgradedTo should be hidden",
				(ItemSet.StmUpgradeBeingUpgradedTo.Options & RegistryOptions.IsHidden) == RegistryOptions.IsHidden);
			AssertEquals("DefaultValue", Guid.Empty, ItemSet.StmUpgradeBeingUpgradedTo.DefaultValue);

			Guid newGuid = Guid.NewGuid();
			ItemSet.StmUpgradeBeingUpgradedTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals("Value", newGuid, ItemSet.StmUpgradeBeingUpgradedTo.Value);
			AssertEquals("GetValue()", newGuid, ItemSet.StmUpgradeBeingUpgradedTo.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.StmUpgradeBeingUpgradedTo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("Value", Guid.Empty, ItemSet.StmUpgradeBeingUpgradedTo.Value);
			AssertEquals("GetValue()", Guid.Empty, ItemSet.StmUpgradeBeingUpgradedTo.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestVersionBeingUpgradedFrom()
		{
			Assert("VersionBeingUpgradedFrom should be hidden",
				(ItemSet.VersionBeingUpgradedFrom.Options & RegistryOptions.IsHidden) == RegistryOptions.IsHidden);
			AssertEquals("DefaultValue", "", ItemSet.VersionBeingUpgradedFrom.DefaultValue);

			ItemSet.VersionBeingUpgradedFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "v1.1.2.2");
			AssertEquals("Value", "v1.1.2.2", ItemSet.VersionBeingUpgradedFrom.Value);
			AssertEquals("GetValue()", "v1.1.2.2", ItemSet.VersionBeingUpgradedFrom.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.VersionBeingUpgradedFrom.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertEquals("Value", "", ItemSet.VersionBeingUpgradedFrom.Value);
			AssertEquals("GetValue()", "", ItemSet.VersionBeingUpgradedFrom.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#region TestSmsConfiguration

		public void TestSmsConfiguration()
		{
			AssertEquals(RegistryOptions.IsHidden, ItemSet.SmsConfiguration.Options);
			AssertEquals("", ItemSet.SmsConfiguration.Value.UserName);
			AssertEquals("", ItemSet.SmsConfiguration.Value.Password);
		}

		#endregion

		public void TestSecondarySMTPServers()
		{
			TestGenericRegistryItem(item: ItemSet.SecondarySMTPServers,
						expectedName: "SecondarySMTPServers",
						expectedCategory: "Physical Server/Mail/Outgoing/SMTP",
						expectedCaption: "Secondary SMTP Servers",
						expectedHint: "This setting enables additional SMTP servers that will be used when the sender’s email address matches a Supported Domain. \r\n\r\nPlease note that the secondary SMTP Servers does not support servers that require OAuth 2.0 authentication.",
						expectedStorage: RegistryStorageFlags.System,
						expectedOptions: RawDataRegistry.Instance.SMTPServer.Options);
		}
	}
}
