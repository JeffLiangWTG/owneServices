using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FileHeaderMapper : BaseMapper
	{
		public override void Map(FlatFileDataRowCollection rows, Enterprise.DataTransfer.Xml.IValueObject value)
		{
			Xsd.CusImportManifest importManifest = (Xsd.CusImportManifest)value;

			FromFileHeaderRow(rows[0], importManifest);
			rows.RemoveAt(0);

			FlatFileDataRowCollection collection = new FlatFileDataRowCollection();

			ZString lastRowCountry = ZString.Empty;

			foreach (FlatFileDataRow row in rows)
			{
				if (IsRowType(row, ShipnetConstants.Codes.RecordID.BookingHeader))
				{
					lastRowCountry = row[ShipnetConstants.BookingHeader.DischargePortCode].Trim().SubstringSafe(0, 2);
					MapToBookingHeader(collection, importManifest, lastRowCountry);
				}
				collection.Add(row);
			}

			MapToBookingHeader(collection, importManifest, lastRowCountry);
		}

		void MapToBookingHeader(FlatFileDataRowCollection collection, Xsd.CusImportManifest importManifest, ZString lastRowCountry)
		{
			if (collection.Count > 0)
			{
				BookingHeaderMapper.Map(collection, importManifest);
				collection.Clear();
			}
		}

		internal void FromFileHeaderRow(FlatFileDataRow row, Xsd.CusImportManifest importManifest)
		{
			importManifest.Vessel.Name = row[ShipnetConstants.FileHeader.VesselName];
			importManifest.Vessel.Lloyds = row[ShipnetConstants.FileHeader.VesselLloyds];
			importManifest.VoyageNumber = row[ShipnetConstants.FileHeader.VoyageNumber];
			importManifest.LastForeignPortOfDeparture.Port.Value = row[ShipnetConstants.FileHeader.LastPortOfLoadCode];
			ZDateTime departureDate;
			if (ZDateTime.TryParseExact(row[ShipnetConstants.FileHeader.SailDate] + row[ShipnetConstants.FileHeader.SailTime], out departureDate, "yyyyMMddHHmmss"))
			{
				importManifest.LastForeignPortOfDeparture.ActualDateTime = departureDate;
			}
		}

		#region Implementation

		BookingHeaderMapper BookingHeaderMapper
		{
			get
			{
				if (fBookingHeaderMapper == null)
				{
					fBookingHeaderMapper = new BookingHeaderMapper();
				}
				return fBookingHeaderMapper;
			}
		}
		BookingHeaderMapper fBookingHeaderMapper;

		#endregion
	}
}
