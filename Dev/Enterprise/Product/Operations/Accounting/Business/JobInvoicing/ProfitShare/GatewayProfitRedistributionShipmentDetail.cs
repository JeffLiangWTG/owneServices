using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionShipmentDetail : ProfitShareShipmentDetail
	{
		public GatewayProfitRedistributionShipmentDetail(IJobInvoicingPlugIn plugin, OrgHeader profitShareParty, string partyType, BusinessObjectFactory factory, ZDecimal profitInLocalCurrency)
			: base(plugin, profitShareParty, partyType, factory)
		{
			this.profitInLocalCurrency = profitInLocalCurrency;
		}

		readonly ZDecimal profitInLocalCurrency;

		public override List<ProfitShareCharge> ProfitShareCharges
		{
			get
			{
				var profitShareCharges = new List<ProfitShareCharge>();
				profitShareCharges.Add(new ProfitShareCharge(this, GlbCompany.CurrentCompany.LocalCurrency, profitInLocalCurrency, profitInLocalCurrency, profitInLocalCurrency, ZString.Empty));

				return profitShareCharges;
			}
		}

		/// <summary>
		/// The Job Invoicing Job that is related to this shipment
		/// </summary>
		public override Job Job
		{
			get
			{
				if (fJob == null && Parent != null && Parent is ForwardingShipment shipment)
				{
					fJob = shipment.ShipmentJobHeader as Job;
				}

				return fJob;
			}
		}

		Job fJob;
	}
}
