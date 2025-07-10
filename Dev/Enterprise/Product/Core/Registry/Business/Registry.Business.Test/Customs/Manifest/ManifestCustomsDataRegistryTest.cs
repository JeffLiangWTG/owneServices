using System;
using System.Linq;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Manifest.Testing
{
	[TestedType(typeof(ManifestCustomsDataRegistry))]
	sealed class ManifestCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<ManifestCustomsDataRegistry>
	{
		public void TestPreBoardingNotificationManifestEnabled()
		{
			TestRegistryItem(ItemSet.PreBoardingNotificationManifestEnabled,
				"PreBoardingNotificationManifestEnabled",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Enable Pre-boarding Notification Manifest",
				"Set this to enable Pre-boarding Notification Manifest module.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);

			Assert("CountryFilterPKs", ItemSet.PreBoardingNotificationManifestEnabled.CountryFilterPKs.Contains(Constants.CountryGuids.Ireland));
		}

		public void TestUSAirAMSEnableFDMMessage()
		{
			TestRegistryItem(
				ItemSet.EnableFDMMessage,
				"EnableFDMMessage",
				ManifestCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_AirAMS,
				"Enable FDM Message",
				"Enable Sending of FDM messages",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		public void TestUSAirAMSEnableMAWBMessaging()
		{
			TestRegistryItem(
				ItemSet.EnableMAWBMessaging,
				"EnableMAWBMessaging",
				ManifestCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_AirAMS,
				"Enable MAWB Messaging",
				"Enable Sending of messages from the MAWB",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		public void TestUSAirAMSGroupNotification()
		{
			var item = ItemSet.USAirAMSGroupNotification;
			TestGenericRegistryItem(
				item,
				"USAirAMSGroupNotification",
				ManifestCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_AirAMS,
				"Notifications Group",
				"Group to receive Air AMS messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default);
			AssertEquals(item.DefaultValue.SendMode, GroupNotification.StaffMemberOrNominatedGroup);
			AssertEquals(item.DefaultValue.SendGroupPK, Constants.Groups.PostMastersGroupPK);
		}

		public void TestUSHVLVAirAMSGroupNotification()
		{
			var item = ItemSet.USHVLVAirAMSGroupNotification;
			TestGenericRegistryItem(
				item,
				"USHVLVAirAMSGroupNotification",
				ManifestCustomsDataRegistry.Categories.Customs_UnitedStatesofAmerica_AirAMS,
				"HVLV Notifications",
				"Group to receive Air AMS messages sent by Customs for bills that originate from HVL shipments. If 'Send Error Only' ticked, Only when an error occurs then send the notification email.",
				RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.Default);
			AssertEquals(item.DefaultValue.SendMode, GroupNotification.StaffMemberOrNominatedGroup);
			AssertEquals(item.DefaultValue.SendGroupPK, Constants.Groups.PostMastersGroupPK);
		}

		public void TestManifestJobNumberCustomization()
		{
			TestGenericRegistryItem(
				ItemSet.ManifestJobNumberCustomization,
				"ManifestJobNumberCustomization",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Manifest Job Number Customization",
				"Override this value to customize how Manifest Job Numbers are formatted",
				RegistryStorageFlags.All);

			var dataType = (BillCustomisationRegistryDataType)ItemSet.ManifestJobNumberCustomization.DataType;
			AssertEquals("GeneratedNumberName", "Manifest Job Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength, dataType.MaxLength);
			AssertEquals("7", dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationIATA]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.DestinationUNLOCO]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginIATA]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.OriginUNLOCO]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.Direction]);
			AssertNull(dataType.DefaultValue.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.TransportMode]);
		}

		public void TestSendSuccessNotifications()
		{
			var testedItem = ItemSet.SendSuccessNotifications;
			TestGenericRegistryItem(
				testedItem,
				"SendSuccessNotifications",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Send success notifications",
				string.Empty,
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch
			);

			var options = (CodePairRegistryDataType)testedItem.DataType;
			AssertEquals("Options", "NOE, ENG, ESG", options.LookUpList.CodesAsString);

			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, testedItem.DefaultValue);
		}

		public void TestSendErrorNotifications()
		{
			var testedItem = ItemSet.SendErrorNotifications;
			TestGenericRegistryItem(
				testedItem,
				"SendErrorNotifications",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Send error notifications",
				string.Empty,
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch
			);

			var options = (CodePairRegistryDataType)testedItem.DataType;
			AssertEquals("Options", "NOE, ENG, ESG", options.LookUpList.CodesAsString);

			AssertEquals("Default value", Constants.EmailTo.StaffMemberAndNominatedGroup, testedItem.DefaultValue);
		}

		public void TestGroupToSendSuccessNotificationsTo()
		{
			TestGenericRegistryItem(
				ItemSet.GroupToSendSucceedNotification,
				"GroupToSendSucceedNotification",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Group to send Global Manifest Universal Event success response notification to",
				string.Empty,
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory
			);

			var dataType = ItemSet.GroupToSendSucceedNotification.DataType as GuidRegistryDataType;
			AssertNotNull(dataType);
			AssertEquals("Default value", new Guid(), dataType.DefaultValue);
		}

		public void TestGroupToSendErrorNotificationsTo()
		{
			TestGenericRegistryItem(
				ItemSet.GroupToSendErrorNotification,
				"GroupToSendErrorNotification",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Group to send Global Manifest Universal Event failure response notification to",
				string.Empty,
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory
			);

			var dataType = ItemSet.GroupToSendErrorNotification.DataType as GuidRegistryDataType;
			AssertNotNull(dataType);
			AssertEquals("Default value", new Guid(), dataType.DefaultValue);
		}

		public void TestAddExciseValueToDutyAmount()
		{
			TestRegistryItem(
				ItemSet.AddExciseValueToDutyAmount,
				"AddExciseValueToDutyAmount",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Add Excise Value to Duty Amount",
				"Add any Excise values to the Global Manifest Duty to Customs Declaration Synchronization",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				true
			);
		}

		public void TestEnableManifestConsolDecoupling()
		{
			TestRegistryItem(
				ItemSet.EnableManifestConsolDecoupling,
				"EnableManifestConsolDecoupling",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Enable Manifest Consol Decoupling",
				"Enable Decoupling of Manifests from their associated Consol",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false
			);
		}

		public void TestMaximumSearchableManifestBills()
		{
			TestGenericRegistryItem(
				ItemSet.MaximumSearchableManifestBills,
				"MaximumSearchableManifestBills",
				ManifestCustomsDataRegistry.Categories.Customs_Manifest,
				"Maximum Searchable Global Manifest Bills",
				"Maximum number of search rows in the Global Manifest Bill search grid",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default
			);

			var reg = ItemSet.MaximumSearchableManifestBills;
			AssertNotNull(reg);
			AssertEquals("Default value", 5000, reg.DefaultValue);
			AssertExceptionThrown<RegistryValidationException>("Maximum value should be 15000", () => ItemSet.MaximumSearchableManifestBills.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15001));
			AssertExceptionThrown<RegistryValidationException>("Minimum value should be 1", () => ItemSet.MaximumSearchableManifestBills.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0));
		}
	}
}
