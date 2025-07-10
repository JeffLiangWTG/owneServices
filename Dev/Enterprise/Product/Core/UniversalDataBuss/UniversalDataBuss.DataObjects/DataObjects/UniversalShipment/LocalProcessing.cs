using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public partial class LocalProcessing : IDataObject
	{
		public LocalProcessing()
		{
		}

		public LocalProcessing(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public Commodity Commodity { get; set; }
		public CodeDescriptionPair PickupEquipmentNeeded { get; set; }
		public CodeDescriptionPair FCLPickupEquipmentNeeded { get; set; }
		public ZDateTime? EstimatedPickup { get; set; }
		public ZDateTime? PickupRequiredBy { get; set; }
		public ZDateTime? PickupRequiredFrom { get; set; }
		public ZDateTime? PickupCartageAdvised { get; set; }
		[MaxLength(35)]
		public ZString? ArrivalCartageRef { get; set; }
		public ZDateTime? PickupCartageCompleted { get; set; }
		public ZDateTime? PickupLabourTime { get; set; }
		public ZDecimal? PickupLabourCharge { get; set; }
		public ZDateTime? DemurrageOnPickupTime { get; set; }
		public ZDateTime? PickupTruckWaitTime { get; set; }
		public ZDecimal? DemurrageOnPickupCharge { get; set; }
		public ZDecimal? PickupTruckWaitCharge { get; set; }
		public CodeDescriptionPair PrintOptionForPackagesOnAWB { get; set; }
		public CodeDescriptionPair DeliveryEquipmentNeeded { get; set; }
		public CodeDescriptionPair FCLDeliveryEquipmentNeeded { get; set; }
		public ZDateTime? FCLAvailable { get; set; }
		public ZDateTime? FCLStorageCommences { get; set; }
		public ZDateTime? LCLAvailable { get; set; }
		public ZDateTime? LCLStorageCommences { get; set; }
		public ZByte? LCLAirStorageDaysOrHours { get; set; }
		public ZDecimal? LCLAirStorageCharge { get; set; }
		public ZDateTime? EstimatedDelivery { get; set; }
		public ZDateTime? DeliveryRequiredBy { get; set; }
		public ZDateTime? DeliveryRequiredFrom { get; set; }
		public ZDateTime? DeliveryCartageAdvised { get; set; }
		public ZDateTime? DeliveryCartageCompleted { get; set; }
		public ZDateTime? DeliveryLabourTime { get; set; }
		public ZDecimal? DeliveryLabourCharge { get; set; }
		public ZDateTime? DemurrageOnDeliveryTime { get; set; }
		public ZDateTime? DeliveryTruckWaitTime { get; set; }
		public ZDecimal? DemurrageOnDeliveryCharge { get; set; }
		public ZDecimal? DeliveryTruckWaitCharge { get; set; }
		public ZBool? HasProhibitedPackaging { get; set; }
		public ZBool? InsuranceRequired { get; set; }
		public ZBool? IsContingencyRelease { get; set; }
		public ZBool? LCLDatesOverrideConsol { get; set; }
		public ZByte? FCLPickupDetentionFreeDays { get; set; }
		public ZByte? FCLPickupDetentionDays { get; set; }
		public ZDecimal? FCLPickupDetentionCharge { get; set; }
		public ZByte? FCLDeliveryDetentionFreeDays { get; set; }
		public ZByte? FCLDeliveryDetentionDays { get; set; }
		public ZDecimal? FCLDeliveryDetentionCharge { get; set; }
		public CodeDescriptionPair ExportStatement { get; set; }

		public DataObjectList<OrderNumber> OrderNumberCollection { get; private set; }
		public DataObjectList<AdditionalService> AdditionalServiceCollection { get; private set; }
	}
}

