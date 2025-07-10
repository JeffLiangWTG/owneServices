using System;
using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants.FreightShipmentDirection.Code;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class ShipmentJobFact : CustomFieldJobFact, IShipmentJobFact
	{
		public ShipmentJobFact(IJobInvoicingPlugIn jobPlugin,
			IEnvironmentFact environmentFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null,
			RefUNLOCO origin = null,
			IUNLOCOFact originFact = null,
			RefUNLOCO destination = null,
			IUNLOCOFact destinationFact = null,
			IOrganisationWithMainAddressFact consigneeFact = null,
			IOrganisationWithMainAddressFact consignorFact = null,
			OrgHeader controllingAgent = null,
			IOrganisationWithMainAddressFact controllingAgentFact = null,
			OrgHeader controllingCustomer = null,
			IOrganisationWithMainAddressFact controllingCustomerFact = null,
			OrgHeader pickupAgent = null,
			IOrganisationWithMainAddressFact pickupAgentFact = null,
			OrgHeader deliveryAgent = null,
			IOrganisationWithMainAddressFact deliveryAgentFact = null,
			IOrganisationWithMainAddressFact pickupLocalTransportFact = null,
			IOrganisationWithMainAddressFact deliveryLocalTransportFact = null)
			: base(jobPlugin?.InvoicingSupporter?.Job, environmentFact, localClientFact, salesRepFact)
		{
			ShipmentPK = jobPlugin.PK.IsValid ? jobPlugin.PK.ToGuid() : Guid.Empty;

			var invoiceSupporter = jobPlugin.InvoicingSupporter;

			TransportMode = invoiceSupporter.TransportMode;
			ContainerMode = invoiceSupporter.ContainerMode;
			ServiceLevel = invoiceSupporter.ServiceLevel;
			JobDirection = invoiceSupporter.IsExport ? Export
						 : invoiceSupporter.IsImport ? Import
						 : invoiceSupporter.IsCrossTrade ? CrossTrade
						 : invoiceSupporter.IsDomestic ? Domestic
						 : string.Empty;
			OriginCountry = origin?.RL_RN_NKCountryCode ?? string.Empty;
			Origin = new FactLeftJoin<IUNLOCOFact>(originFact);
			DestinationCountry = destination?.RL_RN_NKCountryCode ?? string.Empty;
			Destination = new FactLeftJoin<IUNLOCOFact>(destinationFact);
			Consignee = new FactLeftJoin<IOrganisationWithMainAddressFact>(consigneeFact);
			Consignor = new FactLeftJoin<IOrganisationWithMainAddressFact>(consignorFact);
			ControllingAgentCountry = controllingAgent?.MainAddress?.OA_RN_NKCountryCode ?? string.Empty;
			ControllingAgent = new FactLeftJoin<IOrganisationWithMainAddressFact>(controllingAgentFact);
			ControllingCustomerVerticalMarket = controllingCustomer?.MiscServ?.OM_CMIndustryVertical ?? string.Empty;
			ControllingCustomer = new FactLeftJoin<IOrganisationWithMainAddressFact>(controllingCustomerFact);
			PickupAgentCountry = pickupAgent?.MainAddress?.OA_RN_NKCountryCode ?? string.Empty;
			PickupAgent = new FactLeftJoin<IOrganisationWithMainAddressFact>(pickupAgentFact);
			DeliveryAgentCountry = deliveryAgent?.MainAddress?.OA_RN_NKCountryCode ?? string.Empty;
			DeliveryAgent = new FactLeftJoin<IOrganisationWithMainAddressFact>(deliveryAgentFact);
			PickupLocalTransport = new FactLeftJoin<IOrganisationWithMainAddressFact>(pickupLocalTransportFact);
			DeliveryLocalTransport = new FactLeftJoin<IOrganisationWithMainAddressFact>(deliveryLocalTransportFact);

			if (jobPlugin is ForwardingShipment shipment)
			{
				OuterPackUnit = shipment.JS_F3_NKPackType;
				ShipmentType = shipment.JS_ShipmentType;
				if (shipment.OuterPackLines.Count > 0)
				{
					FirstCommodityCode = shipment.OuterPackLines.Cast<ForwardingPackLine>().OrderBy(x => x.JL_PackLineId).First().JL_RH_NKCommodityCode;
				}
			}
		}

		public Guid ShipmentPK { get; }

		public string TransportMode { get; }

		public string ContainerMode { get; }

		public string ServiceLevel { get; }

		public string JobDirection { get; }

		public FactLeftJoin<IUNLOCOFact> Origin { get; }

		public FactLeftJoin<IUNLOCOFact> Destination { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> Consignee { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> Consignor { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> PickupLocalTransport { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> DeliveryLocalTransport { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> ControllingAgent { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> ControllingCustomer { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> PickupAgent { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> DeliveryAgent { get; }

		public string OuterPackUnit { get; }

		public string ControllingCustomerVerticalMarket { get; }

		public string OriginCountry { get; }

		public string DestinationCountry { get; }

		public string PickupAgentCountry { get; }

		public string DeliveryAgentCountry { get; }

		public string ControllingAgentCountry { get; }

		public string ShipmentType { get; }

		public string FirstCommodityCode { get; }
	}
}
