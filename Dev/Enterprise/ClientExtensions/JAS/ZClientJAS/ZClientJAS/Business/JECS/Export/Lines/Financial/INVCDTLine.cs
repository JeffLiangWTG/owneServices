using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public abstract class INVCDTLine : MessageLine
	{
		public static INVCDTLine New(InvoiceWrapper invoiceWrapper)
		{
			INVCDTLine result = null;

			if (invoiceWrapper.Shipment != null)
			{
				if (invoiceWrapper.Shipment.IsAir)
				{
					result = new AirINVCDTLine(invoiceWrapper);
				}
				else if (invoiceWrapper.Shipment.IsSea)
				{
					result = new MaritimeINVCDTLine(invoiceWrapper);
				}
			}

			if (result == null)
			{
				result = new NonShipmentINVCDTLine(invoiceWrapper);
			}

			return result;
		}

		public INVCDTLine(InvoiceWrapper invoiceWrapper)
		{
			this.InvoiceWrapper = invoiceWrapper;
		}

		#region Set Field Values

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.TransactionNumber, Invoice.AH_TransactionNum);
			SetSendingForwarderField(dataRow);
			dataRow.SetField(FieldPositions.TransactionDate, Invoice.AH_InvoiceDate);
			dataRow.SetField(FieldPositions.ISOCountryCode, Invoice.Branch.Country.Code);
			dataRow.SetField(FieldPositions.VATCode, Invoice.Branch.Country.ConsumptionTaxDescription);
			dataRow.SetField(FieldPositions.TransactionPerOperativo, InvoiceLineTypePrefix);
			dataRow.SetField(FieldPositions.Reference, (Invoice.Job != null) ? Invoice.Job.JH_JobNum : ZString.Empty);
			dataRow.SetField(FieldPositions.WeightUnit, "K");
			dataRow.SetField(FieldPositions.CurrencyCode, Invoice.TransactionCurrency.RX_Code);
			dataRow.SetField(FieldPositions.TotalTransaction, Invoice.AH_OSTotalAmount);
			SetReceivingForwarderFields(dataRow);
			SetShipmentRelatedNumericFields(dataRow);
			SetShipperFields(dataRow);
		}

		void SetShipmentRelatedNumericFields(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(FieldPositions.NumberOfPieces, NumberOfPieces);
			dataRow.SetField(FieldPositions.GrossWeight, GrossWeight);
			dataRow.SetField(FieldPositions.ChargeableWeight, ChargeableWeight);
			dataRow.SetField(FieldPositions.Volume, Volume);
		}

		void SetShipperFields(JXCFlatFileDataRow dataRow)
		{
			if (InvoiceWrapper.SendingForwarder != null)
			{
				dataRow.SetField(FieldPositions.ShipperName, InvoiceWrapper.SendingForwarder.OH_FullName, JXCConstants.INVCDTFieldBoundaries.ShipperNameMaxLength);
				dataRow.SetField(FieldPositions.ShipperAddress1, InvoiceWrapper.SendingForwarder.MainAddress.OA_Address1, JXCConstants.INVCDTFieldBoundaries.ShipperAddressMaxLength);
				dataRow.SetField(FieldPositions.ShipperAddress2, InvoiceWrapper.SendingForwarder.MainAddress.OA_Address2, JXCConstants.INVCDTFieldBoundaries.ShipperAddressMaxLength);
			}
		}

		void SetSendingForwarderField(JXCFlatFileDataRow dataRow)
		{
			if (InvoiceWrapper.SendingForwarder != null)
			{
				dataRow.SetField(FieldPositions.SendingNettingCode, InvoiceWrapper.SendingForwarder.NettingCode);
			}
		}

		void SetReceivingForwarderFields(JXCFlatFileDataRow dataRow)
		{
			if (InvoiceWrapper.ReceivingForwarder != null)
			{
				dataRow.SetField(FieldPositions.DestinationNettingCode, InvoiceWrapper.ReceivingForwarder.NettingCode);
				dataRow.SetField(FieldPositions.DestinationOfficeCode, InvoiceWrapper.ReceivingForwarder.OfficeCode);
			}
		}

		protected virtual ZInt NumberOfPieces { get { return 0; } }
		protected virtual ZDecimal GrossWeight { get { return 0; } }
		protected virtual ZDecimal ChargeableWeight { get { return 0; } }
		protected virtual ZDecimal Volume { get { return 0; } }

		#endregion

		protected InvoicingBase Invoice
		{
			get { return InvoiceWrapper.Invoice; }
		}

		protected override sealed ZString LineType
		{
			get
			{
				ZString result = "" + InvoiceLineTypePrefix;

				if (InvoiceWrapper.JASInvoicingBase.CreditNoteOrInvoice == ZArchitecture.Core.TransactionTypes.Invoice)
				{
					result += JXCConstants.LineTypes.INV;
				}
				else
				{
					result += JXCConstants.LineTypes.CDT;
				}

				return result;
			}
		}

		protected abstract char InvoiceLineTypePrefix { get; }
		protected abstract JXCConstants.INVCDTFieldPositions FieldPositions { get; }

		public readonly InvoiceWrapper InvoiceWrapper;
	}
}
