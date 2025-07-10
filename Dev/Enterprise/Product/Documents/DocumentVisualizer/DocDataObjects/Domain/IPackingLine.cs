using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IPackingLine
	{
		ZString ContainerNumber { get; }
		ZInt Quantity { get; set; }
		ZInt PackingOrder { get; }
		ZString PackingLineID { get; }
		ICodeDescription Commodity { get; }
		ICodeDescription PackageType { get; }
		ICodeDescription EntryType { get;  }
		ZBool Complete { get; }
		ZBool Shortage { get; }
		ZShort ItemNumber { get; }

		IMeasurement Weight { get; }
		IMeasurement Volume { get; }
		IMeasurement Height { get; }
		IMeasurement Length { get; }
		IMeasurement Width { get; }

		ICountry Origin { get; }

		ZInt Outturn { get; }
		ZInt Damaged { get; }
		ZInt Pillaged { get; }
		ZString OutturnComment { get; }
		IMeasurement OutturnHeight { get; }
		IMeasurement OutturnLength { get; }
		IMeasurement OutturnVolume { get; }
		IMeasurement OutturnWeight { get; }
		IMeasurement OutturnWidth { get; }

		ZString GoodsDescription { get; set; }

		ZString ShortGoodsDescription { get; set; }
		ZString DetailedGoodsDescription { get; set; }

		ZString MarksAndNumbers { get; set; }
		IHarmonizedCode HarmonizedCode { get; set; }
		IHarmonizedCode ExportHarmonizedCode { get; set; }
		IHarmonizedCode ImportHarmonizedCode { get; set; }

		ZString ReferenceNumber { get; set; }
		ZString ShipmentEntryNumbers { get; set; }
		ZString ImportReferenceNumber { get; set; }
		ZString ExportReferenceNumber { get; set; }

		ZDecimal LoadingMeters { get; }
		ZShort EndItemNumber { get; }

		ZString VehicleColor { get; set; }
		ZString VehicleMake { get; set; }
		ZString VehicleModel { get; set; }
		ZInt VehicleNumberOfDoors { get; set; }
		ICodeDescription VehicleTransmission { get; set; }
		ZInt VehicleYear { get; set; }
		ZString VIN { get; set; }
		ZString ShipmentID { get; set; }
		ZString ShippersRef { get; set; }

		ZString GroupITNNumber { get; set; }
		ZString GroupPOFNumber { get; set; }
		ZString GroupDUENumber { get; set; }
		ZString GroupUCRNumber { get; set; }
		ZString GroupCTKNumber { get; set; }
		ZString GroupCTNNumber { get; set; }

		IReadOnlyCollection<IDangerousGood> DangerousGoods { get; }
		IReadOnlyCollection<IHarmonizedCode> HarmonizedCodes { get; }
		IReadOnlyCollection<IPackingLine> PackingLines { get; }

		ZBool RequiresTemperatureControl { get; set; }
		IMeasurement TemperatureMinimum { get; set; }
		IMeasurement TemperatureMaximum { get; set; }

		ZString CargoItem { get; set; }
		ZString PackageNumber { get; set; }
	}
}
