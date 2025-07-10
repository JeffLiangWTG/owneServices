using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BookingHeaderMapper : BaseMapper
	{
		#region Map

		public override void Map(FlatFileDataRowCollection rows, Enterprise.DataTransfer.Xml.IValueObject value)
		{
			Xsd.CusImportManifest importManifest = (Xsd.CusImportManifest)value;
			Xsd.CusImportManifestOceanBill oceanBill = importManifest.OceanBills.AddNew();

			FlatFileDataRowCollection collection = new FlatFileDataRowCollection();

			FlatFileDataRow consigneeRow = null;

			foreach (FlatFileDataRow row in rows)
			{
				switch (row[ShipnetConstants.Common.RecordID])
				{
					case ShipnetConstants.Codes.RecordID.BookingHeader:
						FromBookingHeaderRow(row, oceanBill, importManifest.Arrivals);
						break;

					case ShipnetConstants.Codes.RecordID.ShipperDetails:
						FromShipperDetailsRow(row, oceanBill);
						break;

					case ShipnetConstants.Codes.RecordID.ConsigneeDetails:
						if (consigneeRow == null)
						{
							consigneeRow = row;
						}
						break;

					case ShipnetConstants.Codes.RecordID.UltimateConsigneeDetails:
						if (consigneeRow == null || consigneeRow[ShipnetConstants.Common.RecordID] == ShipnetConstants.Codes.RecordID.ConsigneeDetails)
						{
							consigneeRow = row;
						}
						break;

					case ShipnetConstants.Codes.RecordID.FreightForwarderDetails:
						if (consigneeRow == null || consigneeRow[ShipnetConstants.PartyDetails.PartyCode].Trim().ToUpper() == row[ShipnetConstants.PartyDetails.PartyCode].Trim().ToUpper())
						{
							consigneeRow = row;
						}
						break;

					case ShipnetConstants.Codes.RecordID.CargoGroup:
						FromCargoGroupRow(row, oceanBill);
						if (IsRowType(row, ShipnetConstants.Codes.RecordID.CargoGroup))
						{
							MapToCargoGroupOrEquipmentLines(collection, oceanBill.OceanBillDetails);
						}
						collection.Add(row);
						break;

					case ShipnetConstants.Codes.RecordID.CargoGroupItems:
					case ShipnetConstants.Codes.RecordID.CargoGroupDescriptions:
					case ShipnetConstants.Codes.RecordID.EquipmentDetails:
					case ShipnetConstants.Codes.RecordID.CargoItemDescriptions:
						collection.Add(row);
						break;
				}
			}

			if (consigneeRow != null)
			{
				FromConsigneeDetailsRow(consigneeRow, oceanBill);
			}

			MapToCargoGroupOrEquipmentLines(collection, oceanBill.OceanBillDetails);

			if (!expectEQDLines)
			{
				MergeMultipleBulkOrBreakBulkLines(oceanBill.OceanBillDetails);
			}
		}

		#endregion

		#region FromBookingHeaderRow

		internal void FromBookingHeaderRow(FlatFileDataRow row, Xsd.CusImportManifestOceanBill oceanBill, Xsd.CusImportManifestArrivalCollection arrivals)
		{
			oceanBill.OceanBillNumber = row[ShipnetConstants.BookingHeader.ReferenceNo];
			oceanBill.PortOfDestination.Port.Value = row[ShipnetConstants.BookingHeader.DeliveryPortCode];
			ZString portOfDischarge = row[ShipnetConstants.BookingHeader.DischargePortCode].Trim().ToUpper();
			oceanBill.PortOfDischarge.Port.Value = portOfDischarge;
			oceanBill.PortOfLoading.Port.Value = row[ShipnetConstants.BookingHeader.LoadPortCode];
			oceanBill.PortOfOrigin.Port.Value = row[ShipnetConstants.BookingHeader.AcceptancePortCode];
			oceanBill.CountryOfOrigin = row[ShipnetConstants.BookingHeader.OriginCountryCode].SubstringSafe(0, 2);

			ShipnetFlatFileMethodsOfPayment methodsOfPayment = new ShipnetFlatFileMethodsOfPayment();
			oceanBill.MethodOfPayment = methodsOfPayment.GetDescriptionFromCode(row[ShipnetConstants.BookingHeader.PaymentTerms]);

			cargoType = row[ShipnetConstants.BookingHeader.EDIFACTCargoType];
			if (cargoType.Trim().IsEmpty)
			{
				cargoType = ShipnetConstants.Codes.EDIFACTCargoType.FCL;
			}

			expectEQDLines = (cargoType != ShipnetConstants.Codes.EDIFACTCargoType.BB && cargoType != ShipnetConstants.Codes.EDIFACTCargoType.U);

			if (!PortCodeIsInArrivals(portOfDischarge, arrivals))
			{
				ParseArrival(row, portOfDischarge, arrivals.AddNew());
			}

			oceanBill.CargoType = GetCargoType(oceanBill.PortOfLoading.Port.Value, oceanBill.PortOfDischarge.Port.Value);
		}

		#endregion

		#region ParseArrival

		void ParseArrival(FlatFileDataRow row, ZString portOfDischarge, Xsd.CusImportManifestArrival arrival)
		{
			arrival.ArrivalPort.Port.Value = portOfDischarge;
			arrival.BerthCode = row[ShipnetConstants.BookingHeader.Berth];

			ZDateTime estimatedTimeOfArrival;
			if (ZDateTime.TryParseExact(row[ShipnetConstants.BookingHeader.ETA], out estimatedTimeOfArrival, "yyyyMMdd"))
			{
				arrival.ArrivalPort.EstimatedDateTime = estimatedTimeOfArrival;
			}
		}

		#endregion

		#region FromShipperDetailsRow

		internal void FromShipperDetailsRow(FlatFileDataRow row, Xsd.CusImportManifestOceanBill oceanBill)
		{
			oceanBill.Consignor.EDICode = row[ShipnetConstants.PartyDetails.PartyCode];
			oceanBill.Consignor.OrganisationDetails.Name = row[ShipnetConstants.PartyDetails.PartyName];
			Xsd.OrgAddress orgAddress = oceanBill.Consignor.OrganisationDetails.Addresses.AddNew();
			orgAddress.AddressLine1 = row[ShipnetConstants.PartyDetails.PartyAddress1];
			orgAddress.AddressLine2 = row[ShipnetConstants.PartyDetails.PartyAddress2];
		}

		#endregion

		#region FromConsigneeDetailsRow

		internal void FromConsigneeDetailsRow(FlatFileDataRow row, Xsd.CusImportManifestOceanBill oceanBill)
		{
			oceanBill.Consignee.EDICode = row[ShipnetConstants.PartyDetails.PartyCode];
			oceanBill.Consignee.OrganisationDetails.Name = row[ShipnetConstants.PartyDetails.PartyName];
			Xsd.OrgAddress orgAddress = oceanBill.Consignee.OrganisationDetails.Addresses.AddNew();
			orgAddress.AddressLine1 = row[ShipnetConstants.PartyDetails.PartyAddress1];
			orgAddress.AddressLine2 = row[ShipnetConstants.PartyDetails.PartyAddress2];

			if (row[ShipnetConstants.PartyDetails.PartyFwdrRegNo].Trim() != ZString.Empty || IsRowType(row, ShipnetConstants.Codes.RecordID.FreightForwarderDetails))
			{
				oceanBill.IsForwarder = true;
			}
		}

		#endregion

		#region FromCargoGroupRow

		internal void FromCargoGroupRow(FlatFileDataRow row, Xsd.CusImportManifestOceanBill oceanBill)
		{
			if (row[ShipnetConstants.CargoGroup.EmptyFull] == ShipnetConstants.Codes.EmptyFull.Empty)
			{
				oceanBill.CargoType = "E";
			}
		}

		#endregion

		#region Implementation

		bool expectEQDLines;
		ZString cargoType = ZString.Empty;

		#region StringMerge

		ZString StringMerge(ZString string1, ZString string2)
		{
			ZString result = ZString.Empty;

			if (string1.IsEmpty)
			{
				result = string2;
			}
			else if (OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).GetSoundex(string1) != OrgPatternLanguageSetting.Get(Core.Constants.Languages.English).GetSoundex(string2))
			{
				result = string1 + " " + string2;
			}
			else
			{
				result = string1;
			}

			return result;
		}

		#endregion

		#region PortCodeIsInArrivals

		bool PortCodeIsInArrivals(ZString code, Xsd.CusImportManifestArrivalCollection arrivals)
		{
			foreach (Xsd.CusImportManifestArrival arrival in arrivals)
			{
				if (arrival.ArrivalPort.Port.Value == code)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region MergeMultipleBulkOrBreakBulkLines

		void MergeMultipleBulkOrBreakBulkLines(Xsd.CusImportManifestOceanBillOceanBillDetailCollection oceanBillDetails)
		{
			Xsd.CusImportManifestOceanBillOceanBillDetail mergedDetail = new Xsd.CusImportManifestOceanBillOceanBillDetail();

			mergedDetail.Weight.DimensionType = CMRQuantityUnits.Codes.Kilogram;
			mergedDetail.Volume.DimensionType = CMRQuantityUnits.Codes.CubicMetre;
			mergedDetail.PackageType = "YC";

			if (oceanBillDetails.Count > 1)
			{
				foreach (Xsd.CusImportManifestOceanBillOceanBillDetail detail in oceanBillDetails)
				{
					mergedDetail.Weight.Value += detail.Weight.Value;
					mergedDetail.Volume.Value += detail.Volume.Value;
					mergedDetail.Indicators.HazardousGoods = detail.Indicators.HazardousGoods;
					mergedDetail.Indicators.Fumigation = detail.Indicators.Fumigation;
					mergedDetail.Indicators.ReportableDocuments = detail.Indicators.ReportableDocuments;
					mergedDetail.NumberOfPackages += detail.NumberOfPackages;

					mergedDetail.GoodsDescription = StringMerge(mergedDetail.GoodsDescription, detail.GoodsDescription);
					mergedDetail.MarksAndNumbers = StringMerge(mergedDetail.MarksAndNumbers, mergedDetail.MarksAndNumbers);
					mergedDetail.ContainerMode = detail.ContainerMode;
					mergedDetail.ContainerSizeOrISOCode = detail.ContainerSizeOrISOCode;
				}

				oceanBillDetails.Clear();
				oceanBillDetails.Add(mergedDetail);
			}
		}

		#endregion

		#region MapToCargoGroupOrEquipmentLines

		void MapToCargoGroupOrEquipmentLines(FlatFileDataRowCollection rows, Xsd.CusImportManifestOceanBillOceanBillDetailCollection oceanBillDetails)
		{
			if (rows.Count > 0)
			{
				if (expectEQDLines)
				{
					new EquipmentLinesMapper(cargoType).Map(rows, oceanBillDetails);
				}
				else
				{
					new CargoGroupMapper(cargoType).Map(rows, oceanBillDetails);
				}

				rows.Clear();
			}
		}

		#endregion

		#region GetCargoType

		internal ZString GetCargoType(ZString loadPort, ZString dischargePort)
		{
			ZString loadCountry = loadPort.Trim().SubstringSafe(0, 2).ToUpper();
			ZString dischargeCountry = dischargePort.Trim().SubstringSafe(0, 2).ToUpper();

			ZString result = CMRImportCargoCodes.Codes.Import;

			if (loadCountry == Australia)
			{
				if (dischargeCountry == Australia)
				{
					result = CMRImportCargoCodes.Codes.Cabotage;
				}
				else
				{
					result = CMRImportCargoCodes.Codes.Export;
				}
			}

			return result;
		}

		#endregion

		const string Australia = "AU";

		#endregion
	}
}
