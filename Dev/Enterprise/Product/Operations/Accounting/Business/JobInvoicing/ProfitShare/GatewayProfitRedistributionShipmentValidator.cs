using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionShipmentValidator
	{
		public GatewayProfitRedistributionShipmentValidator(BusinessObjectFactory factory, ForwardingShipment forwardingShipment, IDisposableProfitShareRedistributionLogger logger)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(forwardingShipment, nameof(forwardingShipment));
			Argument.NotNull(logger, nameof(logger));

			this.factory = factory;
			this.forwardingShipment = forwardingShipment;
			this.logger = logger;
		}

		readonly BusinessObjectFactory factory;
		readonly ForwardingShipment forwardingShipment;
		readonly IDisposableProfitShareRedistributionLogger logger;

		public bool IsValid()
		{
			if (ProfitShareHelper.IsShipmentAlreadyProcessed(factory, forwardingShipment.PK, forwardingShipment.InvoicingSupporter))
			{
				logger.Error($"{forwardingShipment.JS_UniqueConsignRef} is already processed.");
				return false;
			}

			bool result = true;

			if (forwardingShipment.PickupAgent == null && forwardingShipment.DeliveryAgent == null)
			{
				logger.Error($"{forwardingShipment.JS_UniqueConsignRef}: Pickup Agent and Delivery Agent both are not defined.");
				result = false;
			}

			if (forwardingShipment.PickupAgent != null)
			{
				var effectiveOrg = GetOrgOrManagementOrgIfExists(forwardingShipment.PickupAgent);
				if (!effectiveOrg.OH_IsCreditor)
				{
					logger.Error($"{forwardingShipment.JS_UniqueConsignRef}: Pickup Agent is not a creditor.");
					result = false;
				}
			}

			if (forwardingShipment.DeliveryAgent != null)
			{
				var effectiveOrg = GetOrgOrManagementOrgIfExists(forwardingShipment.DeliveryAgent);
				if (!effectiveOrg.OH_IsCreditor)
				{
					logger.Error($"{forwardingShipment.JS_UniqueConsignRef}: Delivery Agent is not a creditor.");
					result = false;
				}
			}

			return result;
		}

		OrgHeader GetOrgOrManagementOrgIfExists(OrgHeader org)
		{
			return org != null ? org.APGrouping : null;
		}
	}
}
