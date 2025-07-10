
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoGroupItemsDataRow : BaseDataRow
	{
		public CargoGroupItemsDataRow(ZString rawRow)
			: base(rawRow, ShipnetConstants.FieldsCount.CargoGroupItems)
		{
		}

		protected override void ParseRawRowCore()
		{
			SetField(ShipnetConstants.CargoGroupItems.CGRSequence, GetValue(ShipnetConstants.CargoGroupItems.CGRSequencePosition, ShipnetConstants.CargoGroupItems.CGRSequenceMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.NumberOfPackages, GetValue(ShipnetConstants.CargoGroupItems.NumberOfPackagesPosition, ShipnetConstants.CargoGroupItems.NumberOfPackagesMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.TypeOfPackages, GetValue(ShipnetConstants.CargoGroupItems.TypeOfPackagesPosition, ShipnetConstants.CargoGroupItems.TypeOfPackagesMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.CargoCode, GetValue(ShipnetConstants.CargoGroupItems.CargoCodePosition, ShipnetConstants.CargoGroupItems.CargoCodeMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.CargoGroup, GetValue(ShipnetConstants.CargoGroupItems.CargoGroupPosition, ShipnetConstants.CargoGroupItems.CargoGroupMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.CargoDescription, GetValue(ShipnetConstants.CargoGroupItems.CargoDescriptionPosition, ShipnetConstants.CargoGroupItems.CargoDescriptionMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.GrossWeightKGS, GetValue(ShipnetConstants.CargoGroupItems.GrossWeightKGSPosition, ShipnetConstants.CargoGroupItems.GrossWeightKGSMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.NetWeightKGS, GetValue(ShipnetConstants.CargoGroupItems.NetWeightKGSPosition, ShipnetConstants.CargoGroupItems.NetWeightKGSMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.GrossCubeCBM, GetValue(ShipnetConstants.CargoGroupItems.GrossCubeCBMPosition, ShipnetConstants.CargoGroupItems.GrossCubeCBMMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.NetCubeCBM, GetValue(ShipnetConstants.CargoGroupItems.NetCubeCBMPosition, ShipnetConstants.CargoGroupItems.NetCubeCBMMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.ECNNumber, GetValue(ShipnetConstants.CargoGroupItems.ECNNumberPosition, ShipnetConstants.CargoGroupItems.ECNNumberMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.Hazardous, GetValue(ShipnetConstants.CargoGroupItems.HazardousPosition, ShipnetConstants.CargoGroupItems.HazardousMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.HazardousClass, GetValue(ShipnetConstants.CargoGroupItems.HazardousClassPosition, ShipnetConstants.CargoGroupItems.HazardousClassMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.UNNumber, GetValue(ShipnetConstants.CargoGroupItems.UNNumberPosition, ShipnetConstants.CargoGroupItems.UNNumberMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.Page, GetValue(ShipnetConstants.CargoGroupItems.PagePosition, ShipnetConstants.CargoGroupItems.PageMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.Flashpoint, GetValue(ShipnetConstants.CargoGroupItems.FlashpointPosition, ShipnetConstants.CargoGroupItems.FlashpointMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.TemperatureType, GetValue(ShipnetConstants.CargoGroupItems.TemperatureTypePosition, ShipnetConstants.CargoGroupItems.TemperatureTypeMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.Fumigated, GetValue(ShipnetConstants.CargoGroupItems.FumigatedPosition, ShipnetConstants.CargoGroupItems.FumigatedMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.ReportableDocument, GetValue(ShipnetConstants.CargoGroupItems.ReportableDocumentPosition, ShipnetConstants.CargoGroupItems.ReportableDocumentMaxLength));
			SetField(ShipnetConstants.CargoGroupItems.PersonalEffects, GetValue(ShipnetConstants.CargoGroupItems.PersonalEffectsPosition, ShipnetConstants.CargoGroupItems.PersonalEffectsMaxLength));
		}
	}
}
