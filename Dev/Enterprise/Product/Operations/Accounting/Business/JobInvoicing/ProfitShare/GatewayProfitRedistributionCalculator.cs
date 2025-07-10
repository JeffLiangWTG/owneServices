using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionCalculator : IProfitShareCalculator
	{
		public GatewayProfitRedistributionCalculator(BusinessObjectFactory factory,
			ProfitShareForwardingConsolWrapper consolWrapper,
			ProfitShareForwardingShipmentWrapper shipmentWrapper,
			OrgProfitShareDetails orgProfitShareDetails,
			ZDecimal profitInLocalCurrency,
			ForwardingProfitShareRedistribution profitShareRedistribution = null)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(consolWrapper, nameof(consolWrapper));
			Argument.NotNull(shipmentWrapper, nameof(shipmentWrapper));
			Argument.NotNull(orgProfitShareDetails, nameof(orgProfitShareDetails));

			calculatorHelper = new ProfitShareCalculatorHelper(factory, consolWrapper.Consol);
			this.shipmentWrapper = shipmentWrapper;
			this.orgProfitShareDetails = orgProfitShareDetails;
			this.profitInLocalCurrency = profitInLocalCurrency;

			if (profitShareRedistribution != null)
			{
				var consolidationProfitShare = profitShareRedistribution.ConsolProfitShares
					.Cast<ConsolidationProfitShare>()
					.FirstOrDefault(x => x.CPS_JK == consolWrapper.Consol.PK);

				if (consolidationProfitShare != null)
				{
					shipmentProfitShare = consolidationProfitShare.ShipmentProfitShares
						.Cast<ShipmentProfitShares>()
						.FirstOrDefault(x => x.PSS_JS == shipmentWrapper.Shipment.PK);

					if (shipmentProfitShare == null)
					{
						shipmentProfitShare = consolidationProfitShare.ShipmentProfitShares.AddNew();
						shipmentProfitShare.PSS_JS = shipmentWrapper.Shipment.PK;

						if (shipmentWrapper.Shipment.Job != null)
						{
							shipmentProfitShare.PSS_JH_ShipmentJob = shipmentWrapper.Shipment.Job.PK;
						}
					}
				}
			}
		}

		readonly ProfitShareCalculatorHelper calculatorHelper;
		readonly ProfitShareForwardingShipmentWrapper shipmentWrapper;
		readonly OrgProfitShareDetails orgProfitShareDetails;
		readonly ZDecimal profitInLocalCurrency;
		readonly ShipmentProfitShares shipmentProfitShare;

		public ProfitShareDetailCollection CreateProfitShares()
		{
			//Refactoring Notes: PS_PartyProfitSharePercent can't be zero, but Validations are on UI, not at enitity level
			//until we add validation, it's safe to check greater than 0,
			//necessary tests are present in GatewayProfitRedistributionProcessorTest and GatewayProfitRedistributionCalculatorTest

			if (shipmentWrapper.Shipment.PickupAgent == null && shipmentWrapper.Shipment.DeliveryAgent == null)
			{
				return new ProfitShareDetailCollection();
			}

			ResetProfitShare();

			var profitShares = new ProfitShareDetailCollection();
			var partyDetails = orgProfitShareDetails.PartyDetailsForGatewayProfitShareRedistribution.ToArray<OrgProfitShareParty>();
			var pickupAgentPartyDetail = partyDetails.SingleOrDefault(x => x.PS_PartyType == OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentPickupAgent);
			var deliveryAgentPartyDetail = partyDetails.SingleOrDefault(x => x.PS_PartyType == OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentDeliveryAgent);
			var shouldOnlyOnePartyGetEntireProfit = shipmentWrapper.Shipment.PickupAgent == null || shipmentWrapper.Shipment.DeliveryAgent == null;

			if (shouldOnlyOnePartyGetEntireProfit)
			{
				CreateProfitShares(profitShares, shipmentWrapper.Shipment.PickupAgent == null ? deliveryAgentPartyDetail : pickupAgentPartyDetail, shouldOnlyOnePartyGetEntireProfit);
			}
			else
			{
				foreach (var partyDetail in partyDetails)
				{
					CreateProfitShares(profitShares, partyDetail, shouldOnlyOnePartyGetEntireProfit);
				}
			}

			AssignProfitShareAmount();

			return profitShares;
		}

		void ResetProfitShare()
		{
			//Refactoring Notes: it looks duplicate as we anyway reset it during processing
			//- GatewayProfitRedistributionProcessor.ResetAllPreviouslyCalculatedValuesToZero
			if (shipmentProfitShare != null)
			{
				shipmentProfitShare.PSS_RedistributedAmount = 0;
				shipmentProfitShare.PSS_PickupAgentShare = 0;
				shipmentProfitShare.PSS_DeliveryAgentShare = 0;
				shipmentWrapper.JS_Calc_PickupAgentProfitShare = 0;
				shipmentWrapper.JS_Calc_DeliveryAgentProfitShare = 0;
			}
		}

		void CreateProfitShares(ProfitShareDetailCollection profitShares, OrgProfitShareParty partyDetail, bool shouldOnlyOnePartyGetEntireProfit)
		{
			if (partyDetail != null && partyDetail.PS_PartyProfitSharePercent > 0)
			{
				var profitAmount = GetProfitAmount(partyDetail, shouldOnlyOnePartyGetEntireProfit);
				var result = calculatorHelper.CreateProfitRedistributionForOrganisation(shipmentWrapper.Shipment, profitShares, GetPayeableAgent(partyDetail.PS_PartyType), partyDetail.PS_PartyType, profitAmount);

				if (result)
				{
					if (partyDetail.PS_PartyType == OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentPickupAgent)
					{
						shipmentWrapper.JS_Calc_PickupAgentProfitShare = profitAmount;
					}
					else if (partyDetail.PS_PartyType == OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentDeliveryAgent)
					{
						shipmentWrapper.JS_Calc_DeliveryAgentProfitShare = profitAmount;
					}
				}
			}
		}

		decimal GetProfitAmount(OrgProfitShareParty partyDetail, bool shouldOnlyOnePartyGetEntireProfit)
		{
			var partyProfitSharePercent = shouldOnlyOnePartyGetEntireProfit ? (ZDecimal)100m : partyDetail.PS_PartyProfitSharePercent;
			return AccountingUtils.Round(profitInLocalCurrency * partyProfitSharePercent / 100m, GlbCompany.CurrentCompany.LocalCurrency);
		}

		OrgHeader GetPayeableAgent(string partyType) =>
			partyType == OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentPickupAgent ? shipmentWrapper.Shipment.PickupAgent : shipmentWrapper.Shipment.DeliveryAgent;

		void AssignProfitShareAmount()
		{
			//Refactoring Notes: can be improved, later we again loop throw and find actual profit share.
			//GatewayProfitRedistributionProcessor.LogActualTotalProfitAvailableForRedistributionAndOriginallyRedistributed
			if (shipmentProfitShare != null)
			{
				shipmentProfitShare.PSS_PickupAgentShare = shipmentWrapper.JS_Calc_PickupAgentProfitShare;
				shipmentProfitShare.PSS_DeliveryAgentShare = shipmentWrapper.JS_Calc_DeliveryAgentProfitShare;
				shipmentProfitShare.PSS_RedistributedAmount = shipmentProfitShare.PSS_PickupAgentShare + shipmentProfitShare.PSS_DeliveryAgentShare;
			}
		}
	}
}
