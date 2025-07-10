using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public interface ITimeSlotRequest
	{
		ZString PackingMode { get; }
		ZString TransportMode { get; }
		ZString EmailSubjectNumber { get; }
		ZString ConsolNumber { get; }
		ZString TransportHeading { get; }
		ZString TransportInfo { get; }
		ZString MasterBillHeading { get; }
		ZString MasterBillAndIssueHeading { get; }
		ZString MasterBillNum { get; }
		ZString MasterBillAndIssueDate { get; }
		ZString HouseBillHeading { get; }
		ZString HouseBillAndIssueHeading { get; }
		ZString HouseBill { get; }
		ZString HouseBillAndIssueDate { get; }
		ZString GoodsDescription { get; }
		ZString BookingReference { get; }
		ZString ETAString { get; }
		ZString ETDString { get; }
		ZString BookingETA { get; }
		ZString BookingETD { get; }
		ZString EquipmentType { get; }
		ZString FullCartageInstructions { get; }
		ZString FullHandlingInstructions { get; }
		MultilingualString JourneyOneDeliverToHeading { get; }

		ZDateTime CutOffOrAvailableDate { get; }
		ZDateTime PickupOrStorageCommenceDate { get; }

		DocOrganisation ConsignorOrg { get; }
		DocOrganisation ConsigneeOrg { get; }

		DocDocAddress CTOAddress { get; }
		DocDocAddress JourneyOneDeliverToAddress { get; }

		DocContacts NotifyParty { get; }

		DocUNLOCO OriginLoco { get; }
		DocUNLOCO DestinationLoco { get; }
		DocUNLOCO PortOfLoading { get; }
		DocUNLOCO PortOfDischarge { get; }

		IDocSimpleContainerCollection SimpleContainers { get; }
	}
}
