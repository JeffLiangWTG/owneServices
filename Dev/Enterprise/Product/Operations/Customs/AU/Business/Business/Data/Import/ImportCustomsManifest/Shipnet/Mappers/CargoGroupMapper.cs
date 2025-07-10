using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoGroupMapper : BaseCargoMapper
	{
		public CargoGroupMapper(ZString cargoType)
			: base(cargoType)
		{
		}

		public override void Map(FlatFileDataRowCollection rows, Enterprise.DataTransfer.Xml.IValueObject value)
		{
			Xsd.CusImportManifestOceanBillOceanBillDetailCollection oceanBillDetails = (Xsd.CusImportManifestOceanBillOceanBillDetailCollection)value;

			foreach (FlatFileDataRow row in rows)
			{
				switch (row[ShipnetConstants.Common.RecordID])
				{
					case ShipnetConstants.Codes.RecordID.CargoGroup:
						FromCargoGroupRow(row);
						break;

					case ShipnetConstants.Codes.RecordID.CargoGroupItems:
						FromCargoGroupItemsRow(row, oceanBillDetails.AddNew());
						break;

					default:
						ForDescriptionRow(row);
						break;
				}
			}
		}

		internal void FromCargoGroupItemsRow(FlatFileDataRow row, Xsd.CusImportManifestOceanBillOceanBillDetail oceanBillDetail)
		{
			oceanBillDetail.Weight.DimensionType = CMRQuantityUnits.Codes.Kilogram;
			oceanBillDetail.Weight.Value = ZDecimal.ParseSafe(row[ShipnetConstants.CargoGroupItems.GrossWeightKGS], 0);
			oceanBillDetail.Volume.DimensionType = CMRQuantityUnits.Codes.CubicMetre;
			oceanBillDetail.Volume.Value = ZDecimal.ParseSafe(row[ShipnetConstants.CargoGroupItems.GrossCubeCBM], 0);
			oceanBillDetail.Indicators.HazardousGoods = row[ShipnetConstants.CargoGroupItems.Hazardous] == "Y";
			oceanBillDetail.Indicators.Fumigation = row[ShipnetConstants.CargoGroupItems.Fumigated] == "Y";
			oceanBillDetail.Indicators.ReportableDocuments = row[ShipnetConstants.CargoGroupItems.ReportableDocument] == "Y";

			ZInt result;
			if (ZInt.TryParse(row[ShipnetConstants.CargoGroupItems.NumberOfPackages], out result))
			{
				oceanBillDetail.NumberOfPackages = result;
			}
			oceanBillDetail.PackageType = row[ShipnetConstants.CargoGroupItems.TypeOfPackages];

			oceanBillDetail.GoodsDescription = goodsDescription;
			oceanBillDetail.MarksAndNumbers = marksAndNumbers;
			oceanBillDetail.ContainerMode = containerMode;
			oceanBillDetail.ContainerSizeOrISOCode = containerSizeOrISOCode;
		}

		internal void FromCargoGroupRow(FlatFileDataRow row)
		{
			containerMode = new ShipnetFlatFileCargoTypes().GetDescriptionFromCode(cargoType);
			containerSizeOrISOCode = row[ShipnetConstants.CargoGroup.SizeType];
		}

		internal ZString containerMode;
		internal ZString containerSizeOrISOCode;
	}
}
