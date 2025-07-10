using System;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public abstract class ShipmentINVCDTLine : INVCDTLine
	{
		public ShipmentINVCDTLine(InvoiceWrapper invoiceWrapper)
			: base(invoiceWrapper)
		{
			if (Shipment == null)
			{
				throw new InvalidOperationException("Shipment cannot be null");
			}
		}

		#region Set Field Values

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			base.SetFieldValues(dataRow);

			dataRow.SetField(FieldPositions.ManifestSerialNumber, ManifestSerialNumber);
			dataRow.SetField(FieldPositions.DateCargoManifest, DateCargoManifest);
		}

		protected ZDateTime DateCargoManifest
		{
			get
			{
				ZDateTime result = Shipment.JS_E_DEP;

				if (result.IsEmpty)
				{
					result = (Consol != null) ? Consol.JK_JX_JA_E_DEP : ZDateTime.Today;
				}

				return result;
			}
		}

		protected ZString ManifestSerialNumber
		{
			get
			{
				ZString result;

				if (Consol != null)
				{
					result = Consol.JK_UniqueConsignRef;
				}
				else if (Invoice.Job != null)
				{
					result = Invoice.Job.JH_JobNum;
				}
				else
				{
					result = Invoice.AH_TransactionNum;
				}

				return result;
			}
		}

		protected override ZDecimal ChargeableWeight
		{
			get { return Shipment.JS_ActualChargeable; }
		}

		protected override ZDecimal GrossWeight
		{
			get { return Shipment.GrossWeightInKilograms; }
		}

		protected override ZInt NumberOfPieces
		{
			get { return Shipment.JS_OuterPacks; }
		}

		protected override ZDecimal Volume
		{
			get { return Shipment.MeasurementInCubicMetres; }
		}

		#endregion

		protected JASForwardingConsol Consol
		{
			get
			{
				JASForwardingConsol result = null;

				if (Shipment != null)
				{
					result = (JASForwardingConsol)Shipment.DepartureConsol;
					if (result == null && Shipment.Consols.Count > 0)
					{
						result = (JASForwardingConsol)Shipment.Consols[0];
					}
				}

				return result;
			}
		}

		protected JASForwardingShipment Shipment
		{
			get { return InvoiceWrapper.Shipment; }
		}
	}
}
