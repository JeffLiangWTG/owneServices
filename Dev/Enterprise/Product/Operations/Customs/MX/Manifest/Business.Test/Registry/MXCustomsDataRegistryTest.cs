using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.MX.Manifest.Business.MXCustomsDataRegistry;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(MXCustomsDataRegistry))]
	class MXCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<MXCustomsDataRegistry>
	{
		public void TestMXTestingSystem()
		{
			TestRegistryItem(
				ItemSet.MXTestingSystem,
				"IsMXTesting",
				Categories.Customs_Mexico,
				"Is Test Mode?",
				"Should MX messages be sent to Test System rather than Production System?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				ItemSet.IsTestLicence);
		}

		public void TestIsMXTestingSystem()
		{
			Instance.MXTestingSystem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(Instance.IsMXTestingSystem);
			Instance.MXTestingSystem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!Instance.IsMXTestingSystem);
		}

		public void TestIsTestLicence()
		{
			var registry = new MXCustomsDataRegistry();
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			Assert(!registry.IsTestLicence);
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			Assert(registry.IsTestLicence);
		}

		public void TestMXMANGroupNotification()
		{
			TestGroupNotificationRegistryItem(ItemSet.MXMANGroupNotification,
				"MXMANGroupNotification",
				Categories.Customs_Mexico_Manifest,
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

		public void TestMXWSVucemUrlServices()
		{
			TestGenericRegistryItem(
				ItemSet.WSVucem,
				"WSVucem",
				MXCustomsDataRegistry.Categories.Customs_Mexico,
				"Services for the VUCEM",
				"URLs for the reception of responses from the VUCEM",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport
			);
		}

		public void TestEnableMXManifests()
		{
			TestRegistryItem(
				ItemSet.EnableMXManifests,
				"EnableMXManifests",
				MXCustomsDataRegistry.Categories.Customs_Mexico,
				"Enable Mexico Manifest",
				"Enable Mexico Manifest?",
				RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestRegistryOptionsWhenEnableMXManifestsIsFalse()
		{
			using (MXCustomsDataRegistry.Instance.EnableMXManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(RegistryOptions.IsHidden, MXCustomsDataRegistry.Instance.MXTestingSystem.Options);
				AssertEquals(RegistryOptions.IsHidden, MXCustomsDataRegistry.Instance.MXMANGroupNotification.Options);
				AssertEquals(RegistryOptions.IsHidden, MXCustomsDataRegistry.Instance.MXManifestShowAIRFunctions.Options);
				AssertEquals(RegistryOptions.IsHidden, MXCustomsDataRegistry.Instance.WSVucem.Options);
			}
		}

		public void TestRegistryOptionsWhenEnableMXManifestsIsTrue()
		{
			using (MXCustomsDataRegistry.Instance.EnableMXManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(RegistryOptions.IsOnlyForSupport, MXCustomsDataRegistry.Instance.MXTestingSystem.Options);
				AssertEquals(RegistryOptions.Default, MXCustomsDataRegistry.Instance.MXMANGroupNotification.Options);
				AssertEquals(RegistryOptions.IsOnlyForSupport, MXCustomsDataRegistry.Instance.MXManifestShowAIRFunctions.Options);
				AssertEquals(RegistryOptions.IsOnlyForSupport, MXCustomsDataRegistry.Instance.WSVucem.Options);
			}
		}
	}
}
