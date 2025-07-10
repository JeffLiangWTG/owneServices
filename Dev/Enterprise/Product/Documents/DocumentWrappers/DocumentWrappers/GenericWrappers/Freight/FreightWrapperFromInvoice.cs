using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromInvoice : FreightWrapper
	{
		public FreightWrapperFromInvoice(InvoicingBase invoice, BusinessObjectFactory factory)
			: base(invoice, factory)
		{
			InvoiceBO = invoice;
		}

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return InvoiceBO.PK;
		}

		protected override ZString GetFormattedTotalCO2e()
		{
			return ShipmentBO != null ? WrapperFromShipment.FormattedTotalCO2e : base.GetFormattedTotalCO2e();
		}

		protected override ZDateTime GetCO2eCalculationDate()
		{
			return ShipmentBO != null ? WrapperFromShipment.CO2eCalculationDate : base.GetCO2eCalculationDate();
		}

		protected override CO2eEmissionWrapperCollection GetCO2eEmissions()
		{
			return ShipmentBO != null ? WrapperFromShipment.CO2eEmissions : base.GetCO2eEmissions();
		}

		#region Implementation

		readonly InvoicingBase InvoiceBO;

		FreightWrapperFromShipment WrapperFromShipment
		{
			get
			{
				if (wrapperFromShipment == null && ShipmentBO != null)
				{
					wrapperFromShipment = new FreightWrapperFromShipment(ShipmentBO, Factory);
				}
				return wrapperFromShipment;
			}
		}
		FreightWrapperFromShipment wrapperFromShipment;

		ForwardingShipment ShipmentBO
		{
			get
			{
				if (object.ReferenceEquals(fShipmentBO, null))
				{
					fShipmentBO = ARInvoice?.Shipment?.CommonShipment;
					if (fShipmentBO == null)
					{
						fShipmentBO = Factory.GetNull<ForwardingShipment>();
					}
				}
				return fShipmentBO;
			}
		}
		ForwardingShipment fShipmentBO;

		#endregion
	}
}
