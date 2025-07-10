using System;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class ICS2CustomsDataRegistry : RegistryItemSet, Integration.Customs.EUICS2.IICS2CustomsDataRegistry
	{
		public static ICS2CustomsDataRegistry Instance => instance ?? (instance = new ICS2CustomsDataRegistry());

		[ThreadStatic]
		static ICS2CustomsDataRegistry instance;

		ICS2CustomsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem IncludeMemberStateInSiteID => GetItem(
			"IncludeMemberStateInSiteID",
			() => new BooleanRegistryItem(
				"IncludeMemberStateInSiteID",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				ResString.GetMultilingualString("E0DC69FC-BF0E-4F72-BBA2-9DB67095C6D4", "Include Member State in Site ID"),
				ResString.GetMultilingualString("1526F4FD-DEDE-4477-8781-5EB3BFA5E6A2", "Set to Yes to include the member state code in the EDI Site ID."),
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false
			)
			{
				CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction
			}
		);

		public GroupNotificationRegistryItem<GroupNotification> EnableICS2ErrorsTo => GetItem(
			"EnableICS2ErrorsToRegistry",
			() => new GroupNotificationRegistryItem<GroupNotification>(
				"EnableICS2ErrorsToRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				ResString.GetMultilingualString("7C192A2D-0AF7-4AB4-B0D8-F2F74EEBA743", "Enable ICS2 Errors To"),
				ResString.GetMultilingualString("D1A9ED81-A4F9-4A87-88EE-EB7812B258CC", "Set ICS2 message errors to staff member, nominated group or combination of both."),
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
			)
		);

		public GroupNotificationRegistryItem<GroupNotification> EnableICS2AcknowledgementsTo => GetItem(
			"EnableICS2AcknowledgementsToRegistry",
			() => new GroupNotificationRegistryItem<GroupNotification>(
				"EnableICS2AcknowledgementsToRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				ResString.GetMultilingualString("9E2B19E5-F92D-4660-A132-A78301A2BD32", "Enable ICS2 Acknowledgements To"),
				ResString.GetMultilingualString("857A8B3B-48BC-4BE6-B502-C5DD480331AA", "Set ICS2 message acknowledgements to staff member, nominated group or combination of both."),
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
			)
		);

		public GroupNotificationRegistryItem<GroupNotification> EnableICS2RequestsTo => GetItem(
			"EnableICS2RequestsToRegistry",
			() => new GroupNotificationRegistryItem<GroupNotification>(
				"EnableICS2RequestsToRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				ResString.GetMultilingualString("F9A289F9-8D35-4338-8001-0BB3C3A8B943", "Enable ICS2 Requests To"),
				ResString.GetMultilingualString("6B70EE03-AF0C-4CAF-B7A9-09FA9BFDC7BD", "Set ICS2 message requests to staff member, nominated group or combination of both."),
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsOnlyForController,
				new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
			)
		);

		public GroupNotificationRegistryItem<GroupNotification> EnableICS2UnmatchedOrNotSentTo => GetItem(
			"EnableICS2UnmatchedOrNotSentToRegistry",
			() => new GroupNotificationRegistryItem<GroupNotification>(
				"EnableICS2UnmatchedOrNotSentToRegistry",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				ResString.GetMultilingualString("0DF3738E-5474-4853-B728-7C2A484F3517", "Enable ICS2 Unmatched/Not Sent To"),
				ResString.GetMultilingualString("AAA1FA3D-DFB0-4E82-80D2-3885060FCD1B", "A message has been received with either no Matching ICS2 Manifest record in CargoWise or a match has been found and no ENS has been filed."),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
			)
			{
				CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction
			}
		);

		public StringRegistryItem SenderPartyId => GetItem(
			"ICS2SenderPartyIdRegistry",
			() => new StringRegistryItem(
				"ICS2SenderPartyIdRegistry",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_ICS2,
				ResString.GetMultilingualString("7B48B727-AD48-4FC7-BE88-A7DE54F18868", "Sender Party Id"),
				ResString.GetMultilingualString("6B444974-EDB2-4E2A-B16B-C7B93E013502", "Please enter the Party Id to use for ICS2 messaging"),
				new StringRegistryDataType(CharacterCase.Upper),
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers
			)
		);

		#region IICS2CustomsDataRegistry Members

		IRegistryItem Integration.Customs.EUICS2.IICS2CustomsDataRegistry.EnableICS2ErrorsTo => EnableICS2ErrorsTo;

		IRegistryItem Integration.Customs.EUICS2.IICS2CustomsDataRegistry.EnableICS2AcknowledgementsTo => EnableICS2AcknowledgementsTo;

		IRegistryItem Integration.Customs.EUICS2.IICS2CustomsDataRegistry.EnableICS2RequestsTo => EnableICS2RequestsTo;

		IRegistryItem Integration.Customs.EUICS2.IICS2CustomsDataRegistry.EnableICS2UnmatchedOrNotSentTo => EnableICS2UnmatchedOrNotSentTo;

		#endregion
	}
}
