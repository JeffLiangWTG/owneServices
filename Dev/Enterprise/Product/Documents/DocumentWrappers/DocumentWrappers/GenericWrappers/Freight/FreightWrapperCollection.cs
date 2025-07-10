using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperCollection : GenericWrapperCollection<FreightWrapper>
	{
		public FreightWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public FreightWrapperCollection(ForwardingConsol consolBO, BusinessObjectFactory factory)
			: this(factory)
		{
			if (consolBO != null)
			{
				foreach (ForwardingShipment shipmentBO in consolBO.Shipments)
				{
					Add(new FreightWrapperFromShipment(shipmentBO, factory));
				}
			}
		}

		public FreightWrapperCollection(ForwardingConsol consolBO, DeliveryAgentOrgHeader deliveryAgent, BusinessObjectFactory factory)
			: this(factory)
		{
			if (consolBO != null)
			{
				foreach (ForwardingShipment shipmentBO in consolBO.Shipments)
				{
					if (shipmentBO.JS_IsForwardRegistered)
					{
						if (deliveryAgent == null ||
							shipmentBO.DeliveryAgent == null && deliveryAgent.PK.Equals(consolBO.ReceivingForwarderPK) ||
							shipmentBO.DeliveryAgent != null && shipmentBO.DeliveryAgent.PK.Equals(deliveryAgent.PK) && !shipmentBO.DeliveryAgent.PK.Equals(consolBO.ReceivingForwarderPK))
						{
							Add(new FreightWrapperFromShipment(shipmentBO, factory));
						}
					}
				}
			}
		}

		public FreightWrapperCollection(CFSLoadListConsol consolBO, BusinessObjectFactory factory)
			: this(factory)
		{
			if (consolBO != null)
			{
				foreach (CFSShipment shipmentBO in consolBO.Shipments)
				{
					Add(new FreightWrapperFromCFSShipment(shipmentBO, factory));
				}
			}
		}

		public FreightWrapperCollection(CFSShipment shipmentBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
			: this(factory)
		{
			if (shipmentBO != null)
			{
				if (routingLevel == RoutingLevel.Shipment)
				{
					foreach (CFSShipment coloadShipmentBO in shipmentBO.CoLoadShipments)
					{
						Add(new FreightWrapperFromCFSShipment(coloadShipmentBO, factory));
					}
				}
				else
				{
					foreach (CFSLoadListConsol consolBO in shipmentBO.Consols)
					{
						Add(new FreightWrapperFromCFSLoadList(consolBO, factory));
					}
				}
			}
		}

		public FreightWrapperCollection(ForwardingShipment shipmentBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
			: this(factory)
		{
			if (shipmentBO != null)
			{
				if (routingLevel == RoutingLevel.Shipment)
				{
					foreach (ForwardingShipment coloadShipmentBO in shipmentBO.CoLoadShipments)
					{
						Add(new FreightWrapperFromShipment(coloadShipmentBO, factory));
					}
				}
				else
				{
					foreach (ForwardingConsol consolBO in shipmentBO.Consols)
					{
						Add(new FreightWrapperFromConsol(consolBO, factory));
					}
				}
			}
		}

		public FreightWrapperCollection(Order orderBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
			: this(factory)
		{
			if (orderBO != null)
			{
				if (routingLevel == RoutingLevel.Shipment)
				{
					if (orderBO.Shipment != null)
					{
						Add(new FreightWrapperFromShipment(orderBO.Shipment, factory));
					}
				}
				else
				{
					if (orderBO.Shipment != null)
					{
						foreach (ForwardingConsol consolBO in orderBO.Shipment.Consols)
						{
							Add(new FreightWrapperFromConsol(consolBO, factory));
						}
					}
				}
			}
		}

		public FreightWrapperCollection(CommonCartage cartageBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
			: this(factory)
		{
			if (cartageBO != null)
			{
				if (routingLevel == RoutingLevel.Shipment)
				{
					if ((cartageBO.CartageParent as ForwardingShipment) != null)
					{
						Add(new FreightWrapperFromShipment(cartageBO.CartageParent as ForwardingShipment, factory));
					}
				}
				else
				{
					ForwardingShipment forwardingShipment = cartageBO.CartageParent as ForwardingShipment;
					if (forwardingShipment != null)
					{
						foreach (ForwardingConsol consolBO in forwardingShipment.Consols)
						{
							Add(new FreightWrapperFromConsol(consolBO, factory));
						}
					}
				}
			}
		}

		public FreightWrapperCollection(BaseJobDeclaration declarationBO, RoutingLevel routingLevel, BusinessObjectFactory factory)
			: this(factory)
		{
			if (declarationBO != null)
			{
				if (routingLevel == RoutingLevel.Shipment)
				{
					if (declarationBO.Shipment != null)
					{
						Add(new FreightWrapperFromShipment(declarationBO.Shipment, factory));
					}
				}
				else
				{
					if (declarationBO.Shipment != null)
					{
						foreach (ForwardingConsol consolBO in declarationBO.Shipment.Consols)
						{
							Add(new FreightWrapperFromConsol(consolBO, factory));
						}
					}
				}
			}
		}

		public FreightWrapperCollection(IEnumerable<WhsItemReceiveTransportationUnit> rtuCollection, BusinessObjectFactory factory)
			: this(factory)
		{
			if (rtuCollection != null)
			{
				foreach (WhsItemReceiveTransportationUnit rtu in rtuCollection)
				{
					Add(FreightWrapper.New(rtu, Factory).FirstOrDefault());
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public FreightWrapperCollection(OrgOpportunity orgOpportunity, BusinessObjectFactory factory)
			: this(factory)
		{
			if (orgOpportunity != null)
			{
				AddOneOffQuotes(orgOpportunity.RelatedChildActivityPivotCollection);
				this.Sort(delegate(BusinessObject x, BusinessObject y)
				{
					return ((x as FreightWrapper).WrappedObject as IRatingHeader).TH_QuoteNumber.CompareTo(((y as FreightWrapper).WrappedObject as IRatingHeader).TH_QuoteNumber);
				});
			}
		}

		void AddOneOffQuotes(IRelatedChildActivityPivotCollection pivots)
		{
			foreach (var pivot in pivots)
			{
				if (pivot.RAP_ChildActivityTableCode.Equals(RatingHeaderSchema.Constants.Prefix) || pivot.RAP_ChildActivityTableCode.Equals(ViewQuotedBookingSchema.Constants.Prefix))
				{
					if (pivot.RAP_ChildActivityTableCode.Equals(RatingHeaderSchema.Constants.Prefix) || pivot.RAP_ChildActivityTableCode.Equals(ViewQuotedBookingSchema.Constants.Prefix))
					{
						Quote bizO = Factory.Load<Quote>(RatingHeaderSchema.Constants.Prefix, pivot.RAP_ChildActivityID);
						if (CheckLoginCompanyMatches(bizO) && !bizO.IsCancelled && bizO.TH_OneTimeQuote)
						{
							Add(new FreightWrapperFromOneOffQuote(bizO, Factory));
						}
					}
				}
				if (pivot.ChildActivity != null)
				{
					AddOneOffQuotes(pivot.ChildActivity.RelatedChildActivityPivotCollection);
				}
			}
		}

		static bool CheckLoginCompanyMatches(Quote quote)
		{
			if (quote != null)
			{
				var company = quote.Company;
				if (company != null && company.PK != Env.CurrentCompany.PK)
				{
					return false;
				}
			}
			return true;
		}
	}
}
