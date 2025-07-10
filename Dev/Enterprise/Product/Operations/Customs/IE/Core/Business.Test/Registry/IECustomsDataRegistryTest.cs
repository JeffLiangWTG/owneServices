using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(IECustomsDataRegistry))]
	class IECustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<IECustomsDataRegistry>
	{
		public void TestIsDirectSendToCustomsForImportEnabled()
		{
			TestGenericRegistryItem(
				item: ItemSet.IsDirectSendToCustomsForImportEnabled,
				expectedName: "IsDirectSendToCustomsForImportEnabled",
				expectedCategory: IECustomsDataRegistry.Categories.Customs_Ireland,
				expectedCaption: "Enable 'Send To Customs' for Imports",
				expectedHint: "If turned on, the 'Send To Customs' menu item under the 'Brokerage' menu will be displayed for Import Declarations.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: false);
		}

		public void TestIsUCC6EnabledForImport()
		{
			TestGenericRegistryItem(
				item: ItemSet.IsUCC6EnabledForImport,
				expectedName: "IsUCC6EnabledForImport",
				expectedCategory: IECustomsDataRegistry.Categories.Customs_Ireland,
				expectedCaption: "Enable Submit Type V2 (UCC6) for Imports",
				expectedHint: "If turned on, the Submit Type V2 (UCC6) will be displayed for Import Declarations.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: false);
		}

		public void TestMessageProcessingNoOfDays()
		{
			TestGenericRegistryItem(
				item: ItemSet.MessageProcessingNoOfDays,
				expectedName: "IEMessageProcessingNoOfDays",
				expectedCategory: IECustomsDataRegistry.Categories.Customs_Ireland,
				expectedCaption: "Message Processing No Of Days",
				expectedHint: "System will only process messages that have been created in the last no of days.",
				expectedStorage: RegistryStorageFlags.System,
				expectedDefaultValue: 14);
		}

		public void TestSendImportAcknowledgements()
		{
			TestGenericRegistryItem(
				item: ItemSet.SendImportAcknowledgements,
				expectedName: "SendImportAcknowledgements",
				expectedCategory: IECustomsDataRegistry.Categories.Customs_Ireland_Notifications_Import,
				expectedCaption: "Send Import Acknowledgements To",
				expectedHint: "Send Import acknowledgements to staff member, nominated group or both",
				expectedStorage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch
			);
			var registryItem = IECustomsDataRegistry.Instance.SendImportAcknowledgements;
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
		}

		public void TestSendCustomsAndExciseReportAcknowledgements()
		{
			TestGenericRegistryItem(
				item: ItemSet.SendCustomsAndExciseReportAcknowledgements,
				expectedName: "SendCustomsAndExciseReportAcknowledgements",
				expectedCategory: IECustomsDataRegistry.Categories.Customs_Ireland_Notifications_CustomsAndExciseReport,
				expectedCaption: "Send Customs and Excise Report Acknowledgements To",
				expectedHint: "Send Customs and Excise Report acknowledgements to staff member, nominated group or both",
				expectedStorage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch
			);
			var registryItem = IECustomsDataRegistry.Instance.SendCustomsAndExciseReportAcknowledgements;
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
		}

		public void TestSendPreBoardingNotificationIds()
		{
			TestGenericRegistryItem(
				item: ItemSet.SendPreBoardingNotificationIds,
				expectedName: "SendPreBoardingNotificationIds",
				expectedCategory: IECustomsDataRegistry.Categories.Customs_Ireland_Notifications_PBNId,
				expectedCaption: "Send PBN Ids To",
				expectedHint: "Send Pre-Boarding Notification Ids to staff member, nominated group or both",
				expectedStorage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch
			);
			var registryItem = IECustomsDataRegistry.Instance.SendPreBoardingNotificationIds;
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
		}
	}
}
