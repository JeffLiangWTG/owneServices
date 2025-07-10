using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CL.Manifest.Business.CLCustomsDataRegistry;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(CLCustomsDataRegistry))]
	class CLCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<CLCustomsDataRegistry>
	{
		public void TestEnableGlobalManifestForChile()
		{
			TestRegistryItem(
				ItemSet.EnableGlobalManifestForChile,
				"EnableGlobalManifestForChile",
				Categories.Customs_Chile,
				"Enable Global Manifest for Chile?",
				"Enable Global Manifest for Chile?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestIsGlobalManifestForChileEnabled()
		{
			Instance.EnableGlobalManifestForChile.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(Instance.IsGlobalManifestForChileEnabled);
			Instance.EnableGlobalManifestForChile.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!Instance.IsGlobalManifestForChileEnabled);
		}

		public void TestCLSMSMessageSending()
		{
			TestGenericRegistryItem(
				ItemSet.CLSMSMessageSending,
				"CLSMSMessageSending",
				CLCustomsDataRegistry.Categories.Customs_Chile,
				"SMS Message Sending Configuration",
				"The settings are for CL SMS Client Application which is a standalone tool installed on the client’s local machine. The tool sends and receives messages through the CW1 Remote Printing Client Software.",
				RegistryStorageFlags.Company
			);
		}

		public void TestCLMANGroupNotification()
		{
			TestGroupNotificationRegistryItem(ItemSet.CLMANGroupNotification,
				"CLMANGroupNotification",
				Categories.Customs_Chile_Manifest,
				"Notification Group",
				"Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ManifestGroupNotification.Default);
		}

		public void TestCLTestingSystem()
		{
			TestRegistryItem(
				ItemSet.CLTestingSystem,
				"IsCLTesting",
				Categories.Customs_Chile,
				"Is Test Mode?",
				"Should CL messages be sent to Test System rather than Production System?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		void TestGroupNotificationRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
			RegistryStorageFlags expectedStorage, RegistryOptions options, GroupNotification expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
			AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
			AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
		}

		public void TestRegistryOptionsWhenEnableGlobalManifestForChileIsFalse()
		{
			using (CLCustomsDataRegistry.Instance.EnableGlobalManifestForChile.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsHidden, CLCustomsDataRegistry.Instance.CLMANGroupNotification.Options);
				AssertEquals(RegistryOptions.IsHidden, CLCustomsDataRegistry.Instance.CLTestingSystem.Options);
				AssertEquals(RegistryOptions.IsHidden, CLCustomsDataRegistry.Instance.CLManifestShowAIRFunctions.Options);
				AssertEquals(RegistryOptions.IsHidden, CLCustomsDataRegistry.Instance.CLSMSMessageSending.Options);
			}
		}

		public void TestRegistryOptionsWhenEnableGlobalManifestForChileIsTrue()
		{
			using (CLCustomsDataRegistry.Instance.EnableGlobalManifestForChile.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.Default, CLCustomsDataRegistry.Instance.CLMANGroupNotification.Options);
				AssertEquals(RegistryOptions.IsOnlyForSupport, CLCustomsDataRegistry.Instance.CLTestingSystem.Options);
				AssertEquals(RegistryOptions.IsOnlyForSupport, CLCustomsDataRegistry.Instance.CLManifestShowAIRFunctions.Options);
				AssertEquals(RegistryOptions.Default, CLCustomsDataRegistry.Instance.CLSMSMessageSending.Options);
			}
		}
	}
}
