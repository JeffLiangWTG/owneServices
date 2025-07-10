using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class AirINVCDTLine : ShipmentINVCDTLine
	{
		public AirINVCDTLine(InvoiceWrapper invoiceWrapper)
			: base(invoiceWrapper)
		{
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			base.SetFieldValues(dataRow);

			if (Consol != null)
			{
				dataRow.SetField(AINVCDTFieldPositions.AirlinePrefix, Consol.MasterBillAirlinePrefix);
				dataRow.SetField(AINVCDTFieldPositions.MAWBSerialNumber, Consol.MasterBillMAWB);
				dataRow.SetField(AINVCDTFieldPositions.FirstFlightNumber, Consol.JK_JX_JV_VoyageFlight);
			}

			dataRow.SetField(AINVCDTFieldPositions.AirportOfOrigin, GetIATACodeFromUNLOCO(Shipment.Origin));
			dataRow.SetField(AINVCDTFieldPositions.AirportOfDestination, GetIATACodeFromUNLOCO(Shipment.Destination));
			dataRow.SetField(AINVCDTFieldPositions.FirstFlightDate, Shipment.JS_E_DEP);
			dataRow.SetField(AINVCDTFieldPositions.ImportOrExportShipment, Shipment.IsImport() ? "I" : "E");
		}

		ZString GetIATACodeFromUNLOCO(RefUNLOCO port)
		{
			ZString result = "";

			if (port != null)
			{
				result = port.RL_IATA;
				if (result.IsEmpty && port.RL_Code.Length > 3)
				{
					result = port.RL_Code.Right(3);
				}
			}

			return result;
		}

		protected override int FieldCount
		{
			get { return JXCConstants.AINVCDTFieldCount; }
		}

		protected override char InvoiceLineTypePrefix
		{
			get { return JXCConstants.LineTypes.AirTransactionPrefix; }
		}

		protected JXCConstants.AINVCDTFieldPositions AINVCDTFieldPositions
		{
			get { return (JXCConstants.AINVCDTFieldPositions)FieldPositions; }
		}

		protected override JXCConstants.INVCDTFieldPositions FieldPositions
		{
			get { return new JXCConstants.AINVCDTFieldPositions(); }
		}
	}
}
