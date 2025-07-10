
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EquipmentDetailsDataRow : BaseDataRow
	{
		public EquipmentDetailsDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.EquipmentDetails)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.EquipmentDetails.CGRSequence, GetValue(ShipnetConstants.EquipmentDetails.CGRSequencePosition, ShipnetConstants.EquipmentDetails.CGRSequenceMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.EquipmentNumber, GetValue(ShipnetConstants.EquipmentDetails.EquipmentNumberPosition, ShipnetConstants.EquipmentDetails.EquipmentNumberMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.SizeType, GetValue(ShipnetConstants.EquipmentDetails.SizeTypePosition, ShipnetConstants.EquipmentDetails.SizeTypeMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.EmptyFull, GetValue(ShipnetConstants.EquipmentDetails.EmptyFullPosition, ShipnetConstants.EquipmentDetails.EmptyFullMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.TotalGrossWeightKGS, GetValue(ShipnetConstants.EquipmentDetails.TotalGrossWeightKGSPosition, ShipnetConstants.EquipmentDetails.TotalGrossWeightKGSMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.TotalNetWeightKGS, GetValue(ShipnetConstants.EquipmentDetails.TotalNetWeightKGSPosition, ShipnetConstants.EquipmentDetails.TotalNetWeightKGSMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.TotalGrossCubeCBM, GetValue(ShipnetConstants.EquipmentDetails.TotalGrossCubeCBMPosition, ShipnetConstants.EquipmentDetails.TotalGrossCubeCBMMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.TotalNetCubeCBM, GetValue(ShipnetConstants.EquipmentDetails.TotalNetCubeCBMPosition, ShipnetConstants.EquipmentDetails.TotalNetCubeCBMMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.TotalTareWeight, GetValue(ShipnetConstants.EquipmentDetails.TotalTareWeightPosition, ShipnetConstants.EquipmentDetails.TotalTareWeightMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.NoOfPackages, GetValue(ShipnetConstants.EquipmentDetails.NoOfPackagesPosition, ShipnetConstants.EquipmentDetails.NoOfPackagesMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.Packages, GetValue(ShipnetConstants.EquipmentDetails.PackagesPosition, ShipnetConstants.EquipmentDetails.PackagesMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.SealNumber1, GetValue(ShipnetConstants.EquipmentDetails.SealNumber1Position, ShipnetConstants.EquipmentDetails.SealNumber1MaxLength));
			SetField(ShipnetConstants.EquipmentDetails.SealNumber2, GetValue(ShipnetConstants.EquipmentDetails.SealNumber2Position, ShipnetConstants.EquipmentDetails.SealNumber2MaxLength));
			SetField(ShipnetConstants.EquipmentDetails.Hazardous, GetValue(ShipnetConstants.EquipmentDetails.HazardousPosition, ShipnetConstants.EquipmentDetails.HazardousMaxLength));
			SetField(ShipnetConstants.EquipmentDetails.ShippersOwn, GetValue(ShipnetConstants.EquipmentDetails.ShippersOwnPosition, ShipnetConstants.EquipmentDetails.ShippersOwnMaxLength));
		}
	}
}
