using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using PartyTypeCodes = Enterprise.MasterFiles.Business.OrgProfitSharePartyLookups.PartyTypeCodes;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	public interface IProfitShareCalculator
	{
		ProfitShareDetailCollection CreateProfitShares();
	}

	public class ProfitShareCalculator : IProfitShareCalculator
	{
		public ProfitShareCalculator(BusinessObjectFactory factory, IJobCostingPlugIn consol, params IJobInvoicingPlugIn[] shipments)
		{
			this.Shipments = shipments;
			this.Factory = factory;
			this.calculatorHelper = new ProfitShareCalculatorHelper(factory, consol);
			this.Consol = consol;
		}

		public ProfitShareCalculator(BusinessObjectFactory factory, params IJobInvoicingPlugIn[] shipments)
			: this(factory, null, shipments)
		{
		}

		// TODO: Get rid of IJobInvoicingPlugIn in favor to IJobInvoicingSupporter. All information we need is in
		//		 IJobInvoicingSupporter and IJobInvoicingPlugIn is just used for accessing IJobInvoicingSupporter.
		//		 Also, there should not be any mention about Consol, Shipment as this is generic calculator which can be
		//		 used with whatever implements IJobInvoicingSupporter.
		public readonly IJobInvoicingPlugIn[] Shipments;
		public readonly BusinessObjectFactory Factory;
		readonly ProfitShareCalculatorHelper calculatorHelper;
		readonly IJobCostingPlugIn Consol;

		public ProfitShareDetailCollection CreateProfitShares()
		{
			ProfitShareDetailCollection profitShares = new ProfitShareDetailCollection();

			foreach (IJobInvoicingPlugIn shipment in Shipments)
			{
				if (shipment != null)
				{
					OrgProfitShareDetails standardProfitShareAgreement = GetProfitShareAgreementForShipment(shipment);
					if (standardProfitShareAgreement != null)
					{
						CreateStandardAgreementProfitShares(profitShares, standardProfitShareAgreement, shipment);
					}
					else
					{
						CreateAgencyProfileProfitShares(profitShares, shipment);
					}
				}
			}

			return profitShares;
		}

		#region Agency Profile

		void CreateAgencyProfileProfitShares(ProfitShareDetailCollection profitShares, IJobInvoicingPlugIn shipment)
		{
			var controllingCustomer = ControllingCustomerRetriever.GetControllingCustomer(shipment.InvoicingSupporter);
			var controllingAgentProfile = CreateAgencyProfitShareForOrganisation(shipment, shipment.InvoicingSupporter.ControllingAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, profitShares, shipment.InvoicingSupporter.ControllingAgent, controllingCustomer);
			if (controllingAgentProfile != null && controllingAgentProfile.GroupNetworkOrFranchise != null)
			{
				var effectiveHeadOffice = calculatorHelper.GetEffectiveHeadOffice(controllingAgentProfile.GroupNetworkOrFranchise, shipment.InvoicingSupporter.ControllingAgent);
				CreateAgencyProfitShareForOrganisation(shipment, shipment.InvoicingSupporter.ControllingAgent, OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor, profitShares, effectiveHeadOffice, controllingCustomer);
			}

			CreateAgencyProfitShareForOrganisation(shipment, shipment.InvoicingSupporter.SendingAgent, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, profitShares, shipment.InvoicingSupporter.SendingAgent, controllingCustomer);
			CreateAgencyProfitShareForOrganisation(shipment, shipment.InvoicingSupporter.ReceivingAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, profitShares, shipment.InvoicingSupporter.ReceivingAgent, controllingCustomer);
		}
#if DEBUG
		public
#endif
		OrgAgentRelationship CreateAgencyProfitShareForOrganisation(IJobInvoicingPlugIn shipment, OrgHeader agreementOrganisation, ZString partyType, ProfitShareDetailCollection profitShares, OrgHeader creditorOrganization, OrgHeader controllingCustomer)
		{
			OrgAgentRelationship relationship = null;
			if (creditorOrganization != null)
			{
				if (agreementOrganisation != null)
				{
					relationship = new OrgAgentRelationship.Loader(Factory).LoadAgencyProfile((agreementOrganisation));
				}
				if (relationship != null)
				{
					var shipmentJob = new Job.Loader(shipment).Load(true);
					if (shipmentJob != null)
					{
						var agreement = shipmentJob.GetProfitShareDetails(relationship, controllingCustomer);
						if (agreement != null)
						{
							calculatorHelper.CreateProfitShareForOrganisation(agreement, shipment, profitShares, creditorOrganization, partyType);
						}
					}
				}
			}

			return relationship;
		}

		#endregion

		#region Standard Profit Share

		OrgProfitShareDetails GetProfitShareAgreementForShipment(IJobInvoicingPlugIn shipment)
		{
			if (ProfitShareAgreements.TryGetValue(shipment, out var cachedAgreement))
			{
				return cachedAgreement;
			}

			// Load the best [Sending Agent - Receiving Agent] relationship.
			// Note that each [Sending Agent - Receiving Agent - Controlling Customer] relationship is unique.
			var invoicingSupporter = shipment.InvoicingSupporter;
			var sendingAgent = GetSendingAgent(shipment);
			var receivingAgent = GetReceivingAgent(shipment);
			var pickupAgent = invoicingSupporter.PickUpAgent;
			var relationshipLoader = new OrgAgentRelationship.Loader(Factory);

			var relationship = relationshipLoader.Load(sendingAgent, receivingAgent, pickupAgent);
			if (relationship == null)
			{
				if (invoicingSupporter is not CommonShipmentInvoicingSupporter shipmentInvoicingSupporter)
				{
					return null;
				}

				// Fallback to the Consol Receiving Agent if it is different from Shipment Receiving Agent.
				var consolReceivingAgent = shipmentInvoicingSupporter.ConsolReceivingAgentWithNoShipmentDeliveryAgentFallback;
				if (receivingAgent?.PK == consolReceivingAgent?.PK)
				{
					return null;
				}

				relationship = relationshipLoader.Load(sendingAgent, consolReceivingAgent, pickupAgent);
				if (relationship == null)
				{
					return null;
				}
			}

			var shipmentJob = new Job.Loader(shipment).Load(true);
			if (shipmentJob == null)
			{
				return null;
			}

			var controllingCustomer = ControllingCustomerRetriever.GetControllingCustomer(invoicingSupporter);
			var agreement = shipmentJob.GetProfitShareDetails(relationship, controllingCustomer);
			ProfitShareAgreements.Add(shipment, agreement);

			return agreement;
		}

		readonly Dictionary<IJobInvoicingPlugIn, OrgProfitShareDetails> ProfitShareAgreements = new Dictionary<IJobInvoicingPlugIn, OrgProfitShareDetails>();

		void CreateStandardAgreementProfitShares(ProfitShareDetailCollection profitShares, OrgProfitShareDetails profitShareAgreement, IJobInvoicingPlugIn shipment)
		{
			var invoicingSupporter = shipment.InvoicingSupporter;

			foreach (OrgProfitShareParty item in profitShareAgreement.PartyDetails)
			{
				var partyTypeCode = item[OrgProfitSharePartySchema.PS_PartyType].ToString();
				OrgHeader orgMatchingPartyType = partyTypeCode switch
				{
					PartyTypeCodes.ReceivingAgent => GetReceivingAgent(shipment),
					PartyTypeCodes.SendingAgent => GetSendingAgent(shipment),
					PartyTypeCodes.HeadOfficeFranchisor => calculatorHelper.GetEffectiveHeadOffice(profitShareAgreement.OrgProfitShareHeader.GroupNetworkOrFranchise, profitShareAgreement.ControllingAgent),
					PartyTypeCodes.ControllingAgent => invoicingSupporter.ControllingAgent,
					PartyTypeCodes.PickupAgent => invoicingSupporter.PickUpAgent,
					PartyTypeCodes.DeliveryAgent => invoicingSupporter.DeliveryAgent,
					_ => null
				};

				if (orgMatchingPartyType != null)
				{
					calculatorHelper.CreateProfitShareForOrganisation(profitShareAgreement, shipment, profitShares, orgMatchingPartyType, partyTypeCode);
				}
			}
		}

		#endregion

		// While calculating profit share in a consol, then we should always use the sending agent from source consol to find related profit share agreement
		OrgHeader GetSendingAgent(IJobInvoicingPlugIn shipment)
		{
			return IsCreatedFromConsol ? Consol.SendingAgent : shipment.InvoicingSupporter.SendingAgent;
		}

		// While calculating profit share in a consol, if current shipment does not have delivery agent, then we should always use the receiving agent from source consol to find related profit share agreement
		OrgHeader GetReceivingAgent(IJobInvoicingPlugIn shipment)
		{
			return IsCreatedFromConsol ? shipment.InvoicingSupporter.DeliveryAgent ?? Consol.ReceivingAgent : shipment.InvoicingSupporter.ReceivingAgent;
		}

		bool IsCreatedFromConsol => Consol != null;
	}
}
