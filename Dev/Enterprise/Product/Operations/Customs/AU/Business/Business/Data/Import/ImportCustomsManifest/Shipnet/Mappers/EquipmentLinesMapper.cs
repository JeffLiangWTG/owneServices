using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EquipmentLinesMapper : BaseCargoMapper
	{
		public EquipmentLinesMapper(ZString cargoType)
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
					case ShipnetConstants.Codes.RecordID.CargoGroupItems:
						FromCargoGroupItemsRow(row);
						break;

					case ShipnetConstants.Codes.RecordID.EquipmentDetails:
						FromEquipmentDetailsRow(row, oceanBillDetails.AddNew());
						break;

					default:
						ForDescriptionRow(row);
						break;
				}
			}
		}

		internal void FromCargoGroupItemsRow(FlatFileDataRow row)
		{
			fumigation = row[ShipnetConstants.CargoGroupItems.Fumigated] == "Y";
			reportableDocuments = row[ShipnetConstants.CargoGroupItems.ReportableDocument] == "Y";
			personalEffects = row[ShipnetConstants.CargoGroupItems.PersonalEffects] == "Y";
		}

		internal void FromEquipmentDetailsRow(FlatFileDataRow row, Xsd.CusImportManifestOceanBillOceanBillDetail oceanBillDetail)
		{
			ShipnetFlatFileCargoTypes cargoTypes = new ShipnetFlatFileCargoTypes();
			oceanBillDetail.ContainerMode = cargoTypes.GetDescriptionFromCode(cargoType);
			oceanBillDetail.GoodsDescription = goodsDescription;
			oceanBillDetail.MarksAndNumbers = marksAndNumbers;
			oceanBillDetail.Indicators.Fumigation = fumigation;
			oceanBillDetail.Indicators.ReportableDocuments = reportableDocuments;
			oceanBillDetail.Indicators.PersonalEffects = personalEffects;

			oceanBillDetail.ContainerNumber = row[ShipnetConstants.EquipmentDetails.EquipmentNumber];
			oceanBillDetail.ContainerSizeOrISOCode = row[ShipnetConstants.EquipmentDetails.SizeType];
			oceanBillDetail.Weight.DimensionType = CMRQuantityUnits.Codes.Kilogram;
			oceanBillDetail.Weight.Value = ZDecimal.ParseSafe(row[ShipnetConstants.EquipmentDetails.TotalGrossWeightKGS], 0);
			oceanBillDetail.Volume.DimensionType = CMRQuantityUnits.Codes.CubicMetre;
			oceanBillDetail.Volume.Value = ZDecimal.ParseSafe(row[ShipnetConstants.EquipmentDetails.TotalGrossCubeCBM], 0);

			ZInt numberOfPacks;
			if (ZInt.TryParse(row[ShipnetConstants.EquipmentDetails.NoOfPackages].Trim(), out numberOfPacks))
			{
				oceanBillDetail.NumberOfPackages = numberOfPacks;
			}

			ZString packageType = row[ShipnetConstants.EquipmentDetails.Packages].Trim();
			oceanBillDetail.PackageType = packageType.SubstringSafe(packageType.Length - 2, 2);
			oceanBillDetail.SealNumber = row[ShipnetConstants.EquipmentDetails.SealNumber1];
			oceanBillDetail.Indicators.HazardousGoods = row[ShipnetConstants.EquipmentDetails.Hazardous] == "Y";
			oceanBillDetail.Indicators.ShipperOwnedContainer = row[ShipnetConstants.EquipmentDetails.ShippersOwn] == "Y";
		}

		internal ZBool fumigation = false;
		internal ZBool reportableDocuments = false;
		internal ZBool personalEffects = false;
	}
}
