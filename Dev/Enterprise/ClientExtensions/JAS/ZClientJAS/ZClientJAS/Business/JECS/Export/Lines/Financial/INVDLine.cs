using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class INVDLine : MessageLine
	{
		public INVDLine(InvoicingLineBase invoiceLine)
		{
			this.InvoiceLine = invoiceLine;
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.INVD; }
		}

		protected override int FieldCount
		{
			get { return JXCConstants.INVDFieldCount; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			if (InvoiceLine != null)
			{
				dataRow.SetField(JXCConstants.INVDFieldPosition.RevenueCode, ChargeCode, JXCConstants.INVDFieldBoundaries.ChargeCodeMaxLength);
				dataRow.SetField(JXCConstants.INVDFieldPosition.RevenueDescription, InvoiceLine.AL_Desc, JXCConstants.INVDFieldBoundaries.DescriptionMaxLength);
				dataRow.SetField(JXCConstants.INVDFieldPosition.RevenueAmount, InvoiceLine.AL_OverseasTotal);
				dataRow.SetField(JXCConstants.INVDFieldPosition.VATCode, TaxCode, JXCConstants.INVDFieldBoundaries.TaxCodeMaxLength);
			}

			if (Shipment != null)
			{
				dataRow.SetField(JXCConstants.INVDFieldPosition.HouseBillNumber, Shipment.JS_HouseBill, JXCConstants.INVDFieldBoundaries.HouseBillMaxLength);
				dataRow.SetField(JXCConstants.INVDFieldPosition.ContainerNumber, ContainerNumber);
				dataRow.SetField(JXCConstants.INVDFieldPosition.SealNumber, SealNumber);
			}
		}

		ZString ChargeCode
		{
			get { return (InvoiceLine.GenericChargeBizO != null) ? InvoiceLine.GenericChargeBizO.VC_Code : ZString.Empty; }
		}

		ZString TaxCode
		{
			get { return InvoiceLine.TransactionHeader.Branch.Company.Country.ConsumptionTaxDescription; }
		}

		ZString ContainerNumber
		{
			get { return (FirstContainer != null) ? FirstContainer.JC_ContainerNum : ZString.Empty; }
		}

		ZString SealNumber
		{
			get { return (FirstContainer != null) ? FirstContainer.JC_SealNum : ZString.Empty; }
		}

		ForwardingContainer FirstContainer
		{
			get { return (ForwardingContainer)Shipment.Containers.FirstOrDefault(); }
		}

		protected
 JASForwardingShipment Shipment
		{
			get
			{
				JASForwardingShipment result = null;

				if (InvoiceLine != null && InvoiceLine.Job != null)
				{
					result = InvoiceLine.Factory.Load<JASForwardingShipment>(InvoiceLine.Job.JH_ParentID);
				}

				return result;
			}
		}

		public readonly InvoicingLineBase InvoiceLine;
	}
}

#region Implementation
#endregion
