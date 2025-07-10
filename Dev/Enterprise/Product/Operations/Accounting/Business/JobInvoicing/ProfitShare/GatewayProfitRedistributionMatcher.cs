using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionMatcher
	{
		public GatewayProfitRedistributionMatcher(BusinessObjectFactory factory, IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList, IDisposableProfitShareRedistributionLogger logger)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(orgProfitShareDetailsList, nameof(orgProfitShareDetailsList));
			Argument.NotNull(logger, nameof(logger));

			this.factory = factory;
			this.orgProfitShareDetailsList = orgProfitShareDetailsList;
			this.logger = logger;
		}

		readonly BusinessObjectFactory factory;
		readonly IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList;
		readonly IDisposableProfitShareRedistributionLogger logger;

		/// <summary>
		/// We shouldn't dispose Job here. If we dispose new created Job here for shipment, we may create a duplicate job for shipment
		///	in some other places, which will cause the duplicate issue.
		///	GatewayProfitShareRedistributionForm.Dispose will help us to dispose these Jobs Created during calculating profit shares.
		/// </summary>
		public OrgProfitShareDetails GetBestOrgProfitShareDetails(ForwardingShipment shipment)
		{
			var shipmentJobCreationError = shipment.CreateShipmentJobHeaderWithMutex();
			if (!string.IsNullOrEmpty(shipmentJobCreationError))
			{
				logger.Error($"{shipment.JS_UniqueConsignRef}: {shipmentJobCreationError}");
				return null;
			}

			var job = shipment.ShipmentJobHeader;
			var invoicingSupporter = shipment.InvoicingSupporter;
			var controllingCustomer = ControllingCustomerRetriever.GetControllingCustomer(invoicingSupporter);
			var orgOverrides = new OrganisationsWithTypes(job?.LocalCharges, invoicingSupporter);
			var pickupLocationCode = GetLocationCode(shipment.ConsignorPickupAddress);
			var deliveryLocationCode = GetLocationCode(shipment.ConsigneeDeliveryAddress);
			var isGroupageForced = IsGroupageContainerModeForced(shipment);

			// First call: generic agreements
			var genericOrgProfitShareDetails = GetBestMatches(invoicingSupporter, pickupLocationCode, deliveryLocationCode, orgOverrides, controllingCustomer, null, isGroupageForced)?.ToArray();
			if (genericOrgProfitShareDetails == null || !genericOrgProfitShareDetails.Any())
			{
				return null;
			}

			// Second call: further filter on generics agreements to get client specific ones, if any
			var results = GetBestMatches(invoicingSupporter, pickupLocationCode, deliveryLocationCode, orgOverrides, controllingCustomer, genericOrgProfitShareDetails, isGroupageForced);

			// Honor the greatest accepted start date and the greatest accepted end date
			return results != null && results.Any()
				? results
					.OrderByDescending(x => x.O4_StartDate)
					.OrderByDescending(x => x.O4_EndDate)
					.First()
				: null;
		}

		static bool IsGroupageContainerModeForced(ForwardingShipment shipment)
		{
			return
				shipment.JS_PackingMode == Core.Constants.ContainerModes.LCL &&
				shipment.Consols.OfType<ForwardingConsol>().Any(c => c.JK_ConsolMode == Core.Constants.ContainerModes.Groupage);
		}

		IEnumerable<OrgProfitShareDetails> GetBestMatches(IJobInvoicingSupporter shipment,
			ZString pickupLocationCode, ZString deliveryLocationCode, OrganisationsWithTypes orgOverrides, OrgHeader controllingParty,
			OrgProfitShareDetails[] orgProfitShareDetails = null, // To Apply Client Specific Filter
			bool isGroupageForced = false) 
		{
			var profitShareMatcher = new ProfitShareMatcher(factory);
			var ranker = profitShareMatcher.GetRanker(shipment.TransportMode,
				shipment.ContainerMode,
				pickupLocationCode,
				deliveryLocationCode,
				orgOverrides,
				controllingParty,
				JobTypesList.Codes.GCN,
				GatewayAgentTypesList.Codes.BGW,
				orgProfitShareDetails,
				isGroupageForced);

			if (orgProfitShareDetails == null)
			{
				var filteredMatches = profitShareMatcher.ApplyProfitShareDetailsFilter(orgProfitShareDetailsList, shipment.ATD, orgOverrides);
				return ranker.GetBestMatch(filteredMatches);
			}
			else
			{
				return ranker.GetBestMatch(orgProfitShareDetails);
			}
		}

		ZString GetLocationCode(JobDocAddress jobDocAddress)
			=> jobDocAddress == null || jobDocAddress.Address == null ? ZString.Empty : jobDocAddress.Address.OA_RL_NKRelatedPortCode;
	}
}
