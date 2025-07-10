
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoGroupDataRow : BaseDataRow
	{
		public CargoGroupDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.CargoGroup)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.CargoGroup.CGRSequence, GetValue(ShipnetConstants.CargoGroup.CGRSequencePosition, ShipnetConstants.CargoGroup.CGRSequenceMaxLength));
			SetField(ShipnetConstants.CargoGroup.NumberOfContainers, GetValue(ShipnetConstants.CargoGroup.NumberOfContainersPosition, ShipnetConstants.CargoGroup.NumberOfContainersMaxLength));
			SetField(ShipnetConstants.CargoGroup.SizeType, GetValue(ShipnetConstants.CargoGroup.SizeTypePosition, ShipnetConstants.CargoGroup.SizeTypeMaxLength));
			SetField(ShipnetConstants.CargoGroup.EmptyFull, GetValue(ShipnetConstants.CargoGroup.EmptyFullPosition, ShipnetConstants.CargoGroup.EmptyFullMaxLength));
			SetField(ShipnetConstants.CargoGroup.TotalGrossWeightKGS, GetValue(ShipnetConstants.CargoGroup.TotalGrossWeightKGSPosition, ShipnetConstants.CargoGroup.TotalGrossWeightKGSMaxLength));
			SetField(ShipnetConstants.CargoGroup.TotalNetWeightKGS, GetValue(ShipnetConstants.CargoGroup.TotalNetWeightKGSPosition, ShipnetConstants.CargoGroup.TotalNetWeightKGSMaxLength));
			SetField(ShipnetConstants.CargoGroup.TotalGrossCubeCBM, GetValue(ShipnetConstants.CargoGroup.TotalGrossCubeCBMPosition, ShipnetConstants.CargoGroup.TotalGrossCubeCBMMaxLength));
			SetField(ShipnetConstants.CargoGroup.TotalNetCubeCBM, GetValue(ShipnetConstants.CargoGroup.TotalNetCubeCBMPosition, ShipnetConstants.CargoGroup.TotalNetCubeCBMMaxLength));
			SetField(ShipnetConstants.CargoGroup.TotalNoOfPackages, GetValue(ShipnetConstants.CargoGroup.TotalNoOfPackagesPosition, ShipnetConstants.CargoGroup.TotalNoOfPackagesMaxLength));
			SetField(ShipnetConstants.CargoGroup.TotalContainerTare, GetValue(ShipnetConstants.CargoGroup.TotalContainerTarePosition, ShipnetConstants.CargoGroup.TotalContainerTareMaxLength));
			SetField(ShipnetConstants.CargoGroup.CAN, GetValue(ShipnetConstants.CargoGroup.CANPosition, ShipnetConstants.CargoGroup.CANMaxLength));
		}
	}
}
