using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.AR.Manifest.Business.ARCustomsDataRegistry;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(ARCustomsDataRegistry))]
	class ARCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<ARCustomsDataRegistry>
	{
		public void TestEnableARManifests()
		{
			TestRegistryItem(
				ItemSet.EnableARManifests,
				"EnableARManifests",
				ARCustomsDataRegistry.Categories.Customs_Argentina,
				"Enable Argentina Manifest",
				"Enable Argentina Manifest?",
				RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestRegistryOptionsWhenEnableARManifestsIsFalse()
		{
			using (ARCustomsDataRegistry.Instance.EnableARManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsHidden, ARCustomsDataRegistry.Instance.ARManifestShowAIRFunctions.Options);
				AssertEquals(RegistryOptions.IsHidden, ARCustomsDataRegistry.Instance.ARTestingSystem.Options);
			}
		}

		public void TestRegistryOptionsWhenEnableARManifestsIsTrue()
		{
			using (ARCustomsDataRegistry.Instance.EnableARManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, ARCustomsDataRegistry.Instance.ARManifestShowAIRFunctions.Options);
				AssertEquals(RegistryOptions.IsOnlyForSupport, ARCustomsDataRegistry.Instance.ARTestingSystem.Options);
			}
		}

		public void TestARTestingSystem()
		{
			TestRegistryItem(
				ItemSet.ARTestingSystem,
				"IsARTesting",
				Categories.Customs_Argentina,
				"Is Test Mode?",
				"Should AR messages be sent to Test System rather than Production System?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestARMANGroupNotification()
		{
			TestGroupNotificationRegistryItem(ItemSet.ARMANGroupNotification,
				"ARMANGroupNotification",
				Categories.Customs_Argentina_Manifest,
				"Notification Group",
				"Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ManifestGroupNotification.Default);
		}

		void TestGroupNotificationRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
			RegistryStorageFlags expectedStorage, RegistryOptions options, GroupNotification expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
			AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
			AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
		}
	}
}
