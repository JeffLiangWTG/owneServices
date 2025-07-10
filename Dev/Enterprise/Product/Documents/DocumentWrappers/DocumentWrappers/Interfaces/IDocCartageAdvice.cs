using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public interface IDocCartageAdvice
	{
		ZString EmailSubjectNumber { get; }

		//Journey Headings
		MultilingualString JourneyOnePickUpHeading { get; }
		MultilingualString JourneyOneDeliverToHeading { get; }
		MultilingualString JourneyTwoPickUpHeading { get; }
		MultilingualString JourneyTwoDeliverToHeading { get; }

		//Cartage Addresses
		DocDocAddress JourneyOnePickUpAddress { get; }
		DocDocAddress JourneyOneDeliverToAddress { get; }
		DocDocAddress JourneyTwoPickUpAddress { get; }
		DocDocAddress JourneyTwoDeliverToAddress { get; }

		//Cartage Contacts
		ZString JourneyOnePickUpContactName { get; }
		ZString JourneyOnePickUpContactPhone { get; }
		ZString JourneyOneDeliverToContactName { get; }
		ZString JourneyOneDeliverToContactPhone { get; }
		ZString JourneyTwoPickUpContactName { get; }
		ZString JourneyTwoPickUpContactPhone { get; }
		ZString JourneyTwoDeliverToContactName { get; }
		ZString JourneyTwoDeliverToContactPhone { get; }

		ZBool PrintAsContainers { get; }
		ZBool PrintTwoJourneys { get; }

		//Instructions
		ZString EquipmentType { get; }
		ZString FullHandlingInstructions { get; }
		ZString FullCartageInstructions { get; }

		//Warehouse
		DocDocAddressCollection AddressesWithWareHousing { get; }

		CartageAdviceHelper CartageAdvice { get; }
		DocCompany CurrentCompany { get; }
		ZBool IsAir { get; }

		ZDateTime CartageCutOffDate { get; }
		ZDateTime CartageAvailableDate { get; }
		ZDateTime CutOffOrAvailableDate { get; }
		ZDateTime CartageReceivalDate { get; }
		ZDateTime CartageStorageCommenceDate { get; }
		ZDateTime PickupOrStorageCommenceDate { get; }
		ZString PickupOrStorageCommenceDateHeading { get; }
	}
}
