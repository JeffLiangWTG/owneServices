using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ShipnetFlatFileFormat : FixedWidthFlatFileFormat
	{
		public ShipnetFlatFileFormat()
		{
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotSupportedException("Exports are not supported");
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			ZString headerCode = GetValue(0, 3, rawRow);

			FlatFileDataRow result = null;

			switch (headerCode)
			{
				case ShipnetConstants.Codes.RecordID.FileHeader:
					result = new FileHeaderDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.BookingHeader:
					result = new BookingHeaderDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.ShipperDetails:
				case ShipnetConstants.Codes.RecordID.ConsigneeDetails:
				case ShipnetConstants.Codes.RecordID.FreightForwarderDetails:
				case ShipnetConstants.Codes.RecordID.CustomerDetails:
				case ShipnetConstants.Codes.RecordID.NotifyPartyDetails:
					result = new PartyDetailsDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.UltimateConsigneeDetails:
					result = new UltimateConsigneeDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.CargoGroup:
					result = new CargoGroupDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.CargoGroupItems:
					result = new CargoGroupItemsDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.CargoGroupDescriptions:
				case ShipnetConstants.Codes.RecordID.CargoItemDescriptions:
					result = new CargoDescriptionsDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.EquipmentDetails:
					result = new EquipmentDetailsDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.ChargeDetail:
					result = new ChargeDetailDataRow(rawRow);
					break;

				case ShipnetConstants.Codes.RecordID.Summary:
					result = new SummaryDataRow(rawRow);
					break;
			}

			return result;
		}

		#region FileExtensionTypes

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Txt; }
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.Txt; }
		}

		#endregion
	}
}
