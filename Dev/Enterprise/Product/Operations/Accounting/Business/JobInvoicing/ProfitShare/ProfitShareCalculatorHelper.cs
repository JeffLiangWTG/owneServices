using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class ProfitShareCalculatorHelper
	{
		public ProfitShareCalculatorHelper(BusinessObjectFactory factory, IJobCostingPlugIn consol)
		{
			this.factory = factory;
			this.consol = consol;
		}

		readonly BusinessObjectFactory factory;
		readonly IJobCostingPlugIn consol;

		#region Helper

		public void CreateProfitShareForOrganisation(OrgProfitShareDetails profitShareAgreement, IJobInvoicingPlugIn plugin, ProfitShareDetailCollection profitShares, OrgHeader organisation, string partyType)
		{
			var effectiveOrganisation = GetOrgOrManagementOrgIfExists(organisation);
			if (effectiveOrganisation != null && ShouldCreateProfitShareForOrg(plugin, profitShareAgreement, effectiveOrganisation))
			{
				var result = profitShares.GetProfitShareForOrg(effectiveOrganisation, partyType);
				if (result == null)
				{
					result = new ProfitShareDetail(effectiveOrganisation, factory, partyType);
					result.Consol = consol;
					profitShares.Add(result);
				}

				var detail = new ProfitShareShipmentDetail(plugin, effectiveOrganisation, partyType, factory);

				if (detail.Job != null)
				{
					detail.ProfitShareAgreement = profitShareAgreement;
					result.ProfitShareShipmentDetails.Add(detail);
				}
			}
		}

		public bool CreateProfitRedistributionForOrganisation(IJobInvoicingPlugIn plugin, ProfitShareDetailCollection profitShares, OrgHeader organisation, string partyType, ZDecimal profitInLocalCurrency)
		{
			var effectiveOrganisation = GetOrgOrManagementOrgIfExists(organisation);
			if (effectiveOrganisation != null && effectiveOrganisation.OH_IsCreditor)
			{
				var result = profitShares.GetProfitShareForOrg(effectiveOrganisation);
				if (result == null)
				{
					result = new ProfitShareDetail(effectiveOrganisation, factory, partyType);
					result.Consol = consol;
					profitShares.Add(result);
				}

				var detail = new GatewayProfitRedistributionShipmentDetail(plugin, effectiveOrganisation, partyType, factory, profitInLocalCurrency);

				if (detail.Job != null)
				{
					result.ProfitShareShipmentDetails.Add(detail);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		///		Returns true if the organisation is not a proxy organisation of the current company,
		///		or if all related agents (receiving, sending, head office, controlling, pickup, delivery)
		///		that exist are proxy organisations of the current login company.
		/// </summary>
		bool ShouldCreateProfitShareForOrg(IJobInvoicingPlugIn plugin, OrgProfitShareDetails profitShareAgreement, OrgHeader organisation)
		{
			if (organisation == null)
			{
				return false;
			}

			if (!organisation.IsProxyOrg(GlbCompany.CurrentCompany))
			{
				return true;
			}

			var receivingAgent = GetOrgOrManagementOrgIfExists(plugin.InvoicingSupporter.ReceivingAgent);
			var sendingAgent = GetOrgOrManagementOrgIfExists(plugin.InvoicingSupporter.SendingAgent);
			var headOffice = GetOrgOrManagementOrgIfExists(profitShareAgreement.OrgProfitShareHeader.GroupNetworkOrFranchise);
			var controllingAgent = GetOrgOrManagementOrgIfExists(profitShareAgreement.ControllingAgent);
			var pickupAgent = GetOrgOrManagementOrgIfExists(plugin.InvoicingSupporter.PickUpAgent);
			var deliveryAgent = GetOrgOrManagementOrgIfExists(plugin.InvoicingSupporter.DeliveryAgent);

			// The `organisation` can be one of the agents, or not.
			// All agents must be proxy organisations for this login company for the profit share to be created.
			return ((OrgHeader[])[receivingAgent, sendingAgent, headOffice, controllingAgent, pickupAgent, deliveryAgent])
				.All(x => x == null || x.IsProxyOrg(GlbCompany.CurrentCompany));
		}

		OrgHeader GetOrgOrManagementOrgIfExists(OrgHeader org)
		{
			return org != null ? org.APGrouping : null;
		}

		/// <summary>
		/// If the controlling party is not an org proxy of the current company, then the head-office portion
		/// of the profit share should be redirected to the controlling party.
		/// Ie - the headoffice only receives profit share where the controlling party is the current company.
		/// </summary>
		public OrgHeader GetEffectiveHeadOffice(OrgHeader headOffice, OrgHeader controllingParty)
		{
			OrgHeader result = headOffice;
			if (controllingParty != null &&
				AccountingConfigurationRegistry.Instance.ProfitShareRedirectHeadOfficeIfExternal.Value &&
				!controllingParty.IsProxyOrg(GlbCompany.CurrentCompany))
			{
				result = controllingParty;
			}

			return result;
		}

		#endregion
	}
}
