
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class MaritimeINVCDTLine : ShipmentINVCDTLine
	{
		public MaritimeINVCDTLine(InvoiceWrapper invoiceWrapper)
			: base(invoiceWrapper)
		{
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			base.SetFieldValues(dataRow);

			dataRow.SetField(MINVCDTFieldPositions.OBLSerialNumber, Shipment.JS_HouseBill);
			dataRow.SetField(MINVCDTFieldPositions.PortOfDischarge, Shipment.JS_RL_NKDestination);
			dataRow.SetField(MINVCDTFieldPositions.PortOfLoading, Shipment.JS_RL_NKOrigin);
			dataRow.SetField(MINVCDTFieldPositions.EstimatedShippingDate, Shipment.JS_E_DEP);

			if (Consol != null)
			{
				dataRow.SetField(MINVCDTFieldPositions.SSLCode, (Consol.ShippingLine != null) ? Consol.ShippingLine.JASWWMappedCode : ZString.Empty);
				ZString vesselAndVoyage = Consol.JK_JX_JV_NKVessel;
				vesselAndVoyage += (vesselAndVoyage.IsEmpty) ? Consol.JK_JX_JV_VoyageFlight : new ZString(" / " + Consol.JK_JX_JV_VoyageFlight);
				dataRow.SetField(MINVCDTFieldPositions.VesselAndVoyage, vesselAndVoyage);
			}
		}

		JXCConstants.MINVCDTFieldPositions MINVCDTFieldPositions
		{
			get { return (JXCConstants.MINVCDTFieldPositions)FieldPositions; }
		}

		protected override int FieldCount
		{
			get { return JXCConstants.MINVCDTFieldCount; }
		}

		protected override char InvoiceLineTypePrefix
		{
			get { return JXCConstants.LineTypes.MaritimeTransactionPrefix; }
		}

		protected override JXCConstants.INVCDTFieldPositions FieldPositions
		{
			get { return new JXCConstants.MINVCDTFieldPositions(); }
		}
	}
}
