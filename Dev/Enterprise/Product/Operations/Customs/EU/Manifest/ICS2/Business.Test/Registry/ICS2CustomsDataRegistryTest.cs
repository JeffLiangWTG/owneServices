using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2CustomsDataRegistry))]
	sealed class ICS2CustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<ICS2CustomsDataRegistry>
	{
		public void TestIsForProductivityWise()
		{
			AssertEquals(false, ICS2CustomsDataRegistry.Instance.IsForProductivityWise);
		}

		public void TestIncludeMemberStateInSiteID()
		{
			TestRegistryItem(ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID,
				"IncludeMemberStateInSiteID",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				"Include Member State in Site ID",
				"Set to Yes to include the member state code in the EDI Site ID.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);

			AssertContainsExactElementsInAnyOrder(Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, ItemSet.IncludeMemberStateInSiteID.CountryFilterPKs);
		}

		public void TestEnableICS2ErrorsTo()
		{
			TestGroupNotificationRegistryItem(ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo,
				"EnableICS2ErrorsToRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				"Enable ICS2 Errors To",
				"Set ICS2 message errors to staff member, nominated group or combination of both.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
			AssertEquals("Countryfilter", Enumerable.Empty<Guid>(), ItemSet.EnableICS2ErrorsTo.CountryFilterPKs);
		}

		public void TestEnableICS2AcknowledgementsTo()
		{
			TestGroupNotificationRegistryItem(ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo,
				"EnableICS2AcknowledgementsToRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				"Enable ICS2 Acknowledgements To",
				"Set ICS2 message acknowledgements to staff member, nominated group or combination of both.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
			AssertEquals("Countryfilter", Enumerable.Empty<Guid>(), ItemSet.EnableICS2AcknowledgementsTo.CountryFilterPKs);
		}

		public void TestEnableICS2RequestsTo()
		{
			TestGroupNotificationRegistryItem(ICS2CustomsDataRegistry.Instance.EnableICS2RequestsTo,
				"EnableICS2RequestsToRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				"Enable ICS2 Requests To",
				"Set ICS2 message requests to staff member, nominated group or combination of both.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
			AssertEquals("Countryfilter", Enumerable.Empty<Guid>(), ItemSet.EnableICS2RequestsTo.CountryFilterPKs);
		}

		public void TestEnableICS2UnmatchedOrNotSentTo()
		{
			TestGroupNotificationRegistryItem(ICS2CustomsDataRegistry.Instance.EnableICS2UnmatchedOrNotSentTo,
				"EnableICS2UnmatchedOrNotSentToRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				"Enable ICS2 Unmatched/Not Sent To",
				"A message has been received with either no Matching ICS2 Manifest record in CargoWise or a match has been found and no ENS has been filed.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));

			AssertContainsExactElementsInAnyOrder(Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, ItemSet.EnableICS2UnmatchedOrNotSentTo.CountryFilterPKs);
		}

		public void TestSenderPartyId()
		{
			TestStringRegistryItem(ICS2CustomsDataRegistry.Instance.SenderPartyId,
				"ICS2SenderPartyIdRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				"Sender Party Id",
				"Please enter the Party Id to use for ICS2 messaging",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForDevelopers,
				string.Empty,
				CharacterCase.Upper
			);

			AssertEquals(0, ItemSet.SenderPartyId.CountryFilterPKs.Count());
		}

		void TestGroupNotificationRegistryItem(IRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint,
		RegistryStorageFlags expectedStorage, RegistryOptions options, GroupNotification expectedDefaultValue)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorage, options);
			AssertEquals(((GroupNotification)item.DefaultValue).SendMode, expectedDefaultValue.SendMode);
			AssertEquals(((GroupNotification)item.DefaultValue).SendGroupPK, expectedDefaultValue.SendGroupPK);
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "EnableICS2FunctionsRegistry";
			}
		}
	}
}
