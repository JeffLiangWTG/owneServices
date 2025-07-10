using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.IL.Business.ILCustomsDataRegistry;
using static Enterprise.ZArchitecture.Environment.RegistryItemSet;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILCustomsDataRegistry))]
	sealed class ILCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<ILCustomsDataRegistry>
	{
		public void TestILMANGroupNotification()
		{
			AssertGroupNotificationRegistryItem(ItemSet.ILMANGroupNotification,
				"ILMANGroupNotification",
				Categories.Customs_Israel_Manifest,
				"Notification Group",
				"Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ManifestGroupNotification.Default,
				CountryFilterPKs.Israel);
		}

		public void TestEnableILDeliveryOrder()
		{
			TestRegistryItem(
				ItemSet.EnableILDeliveryOrder,
				"EnableILDeliveryOrder",
				Categories.Customs_Israel_DeliveryOrder,
				"Enable IL Delivery Order",
				"Enable IL Delivery Order?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestILDELGroupNotification()
		{
			AssertGroupNotificationRegistryItem(ItemSet.ILDELGroupNotification,
				"ILDELGroupNotification",
				Categories.Customs_Israel_DeliveryOrder,
				"Notification Group",
				"Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ManifestGroupNotification.Default,
				CountryFilterPKs.Israel);
		}

		public void TestILEnableILManifest()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddRange(new ILManifestRegistryOptions());

			TestRegistryItem(item: ItemSet.ILEnableILManifest,
				expectedName: "ILEnableILManifest",
				expectedCategory: Categories.Customs_Israel_Manifest,
				expectedCaption: "Enable IL Manifest",
				expectedHint: "Enable IL Manifest",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.IsOnlyForDevelopers,
				expectedLookUpList: codeDescriptionPairList,
				expectedDefaultValue: "NONE"
			);
		}

		public void TestAllRegistryItemsHaveIsraelCountryFilter()
		{
			foreach (var registryItem in AllItems)
			{
				Assert(registryItem.Name + ".CountryFilterPK", registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Israel));
			}
		}

		public void TestEnableILGatePassMovements()
		{
			TestRegistryItem(
				ItemSet.EnableILGatePassMovements,
				"EnableILGatePassMovements",
				Categories.Customs_Israel_GatePassMovements,
				"Enable IL Gate pass Movements",
				"Enable IL Gate pass Movements?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestILGPMGroupNotification()
		{
			AssertGroupNotificationRegistryItem(ItemSet.ILGPMGroupNotification,
				"ILGPMGroupNotification",
				Categories.Customs_Israel_GatePassMovements,
				"Notification Group",
				"Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				ManifestGroupNotification.Default,
				CountryFilterPKs.Israel);
		}

		public void TestPullASyncMessagesEnabled()
		{
			TestRegistryItem(
				ItemSet.EnablePullASyncMessage,
				"EnablePullASyncMessage",
				Categories.Customs_Israel_IIG,
				"Enable Pull A-Sync Message",
				"Enable Pull A-Sync Message?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestDCAParametersForASyncMessage()
		{
			TestGenericRegistryItem(
				ItemSet.DCAParametersForASyncMessage,
				"DCAParametersForASyncMessage",
				Categories.Customs_Israel_IIG,
				"DCA Parameters for A-Sync Message",
				"DCA Parameters for A-Sync Message",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport);
		}

		public void TestPersonalDigitalSignatureFallbackConfiguration()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddRange(new DigitalSignatureFallbackList());

			TestRegistryItem(item: ItemSet.PersonalDigitalSignatureFallbackConfiguration,
				expectedName: "PersonalDigitalSignatureFallbackConfiguration",
				expectedCategory: Categories.Customs_Israel_DigitalSignature,
				expectedCaption: "Personal Digital Signature Fallback Configuration",
				expectedHint: "Select the method by which the system will locate a valid digital signature certificate when a personal signature is necessary",
				expectedStorage: RegistryStorageFlags.Company,
				expectedLookUpList: codeDescriptionPairList,
				expectedDefaultValue: "StaffOnly"
			);
		}

		public void TestSendILCustomsMessagesWithoutDigitalSignature()
		{
			TestRegistryItem(
				ItemSet.SendILCustomsMessagesWithoutDigitalSignature,
				"SendILCustomsMessagesWithoutDigitalSignature",
				Categories.Customs_Israel_DigitalSignature,
				"Send IL Customs messages without digital signature",
				"Send IL Customs messages without digital signature?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		void AssertGroupNotificationRegistryItem(IRegistryItem item,
			string expectedName,
			string expectedCategory,
			string expectedCaption,
			string expectedHint,
			RegistryStorageFlags expectedStorage,
			RegistryOptions options,
			GroupNotification expectedDefaultValue,
			System.Collections.Generic.IEnumerable<System.Guid> expectedCountryFilterPK)
		{
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
				AssertEquals(expectedCountryFilterPK, item.CountryFilterPKs);
				AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
				AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
			});
		}
	}
}
