using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Freight
{
	public interface IJobDocsAndCartage : IBusiness
	{
		ZGuid PK { get; }
		object this[string propertyName] { get; set; }

		ZString JP_ArrivalCartageRef { get; set; }
		ZString JP_CustomAttrib1 { get; set; }
		ZString JP_CustomAttrib2 { get; set; }
		ZDateTime JP_CustomDate1 { get; set; }
		ZDateTime JP_CustomDate2 { get; set; }
		ZDecimal JP_CustomDecimal1 { get; set; }
		ZDecimal JP_CustomDecimal2 { get; set; }
		ZBool JP_CustomFlag1 { get; set; }
		ZBool JP_CustomFlag2 { get; set; }
		ZDateTime JP_DeliveryCartageAdvised { get; set; }
		ZDateTime JP_DeliveryCartageCompleted { get; set; }
		ZDecimal JP_DeliveryLabourCharge { get; set; }
		ZDateTime JP_DeliveryLabourTime { get; set; }
		ZDateTime JP_DeliveryRequiredBy { get; set; }
		ZDecimal JP_DeliveryTruckWaitCharge { get; set; }
		ZDateTime JP_DeliveryTruckWaitTime { get; set; }
		ZDecimal JP_PickupTruckWaitCharge { get; set; }
		ZDateTime JP_PickupTruckWaitTime { get; set; }
		ZDateTime JP_EstimatedDelivery { get; set; }
		ZDateTime JP_EstimatedPickup { get; set; }
		ZString JP_ExportStatement { get; set; }
		ZDateTime JP_FCLAvailable { get; set; }
		ZString JP_FCLDeliveryEquipmentNeeded { get; set; }
		ZString JP_FCLPickupEquipmentNeeded { get; set; }
		ZDateTime JP_FCLStorageCommences { get; set; }
		ZBool JP_HasProhibitedPackaging { get; set; }
		ZBool JP_InsuranceRequired { get; set; }
		ZBool JP_IsContingencyRelease { get; set; }
		ZDecimal JP_LCLAirStorageCharge { get; set; }
		ZByte JP_LCLAirStorageDaysOrHours { get; set; }
		ZDateTime JP_LCLAvailable { get; set; }
		ZBool JP_LCLDatesOverrideConsol { get; set; }
		ZDateTime JP_LCLStorageCommences { get; set; }
		ZGuid JP_OA_DeliveryCartageCoAddr { get; set; }
		ZGuid JP_OA_PickupCartageCoAddr { get; set; }
		ZGuid JP_ParentID { get; set; }
		ZString JP_ParentTableCode { get; set; }
		ZDateTime JP_PickupCartageAdvised { get; set; }
		ZDateTime JP_PickupCartageCompleted { get; set; }
		ZDecimal JP_PickupLabourCharge { get; set; }
		ZDateTime JP_PickupLabourTime { get; set; }
		ZDateTime JP_PickupRequiredBy { get; set; }
		ZString JP_PrintOptionForPackagesOnAWB { get; set; }

		IDisposable TemporarilySetTransportBookingEventReference(string bookingReference);
	}
}