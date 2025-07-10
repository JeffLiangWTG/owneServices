using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class AgentChargePostingDetails
	{
		public AgentChargePostingDetails(IJobCostingPlugIn consol,
			ProfitShareDetailCollection calculatedProfitShares,
			RefCurrency agentInvoicePostingCurrency,
			ZDecimal agentInvoicePostingExchangeRate,
			ChargePoster poster,
			JobConsolCostCollection consolCosts)
		{
			this.Consol = consol;
			if (consol == null)
			{
				throw new ArgumentNullException(nameof(consol), "Consol cannot be null when constructing class AgentChargePostingDetails");
			}

			this.CalculatedProfitShares = calculatedProfitShares;
			this.AgentInvoicePostingCurrency = agentInvoicePostingCurrency;
			this.AgentInvoicePostingExchangeRate = agentInvoicePostingExchangeRate;
			this.ChargePoster = poster;
			this.AllConsolCosts = consolCosts;
			this.AgentInvoices = new List<InvoicingBase>();
		}

		public void AddProfitShareApportionment(JobConsolCost profitShareApportionment)
		{
			ProfitShareApportionments.Add(profitShareApportionment);
		}

		public JobConsolCost GetRecevingAgentProfitShareApportionment()
		{
			JobConsolCost result = null;
			foreach (JobConsolCost app in ProfitShareApportionments)
			{
				if (app.E6_OH_Creditor == Consol.ReceivingAgentAPInvoicingParty.PK)
				{
					result = app;
					break;
				}
			}
			return result;
		}

		public InvoicingBase ConsolReceivingAgentARInvoice { get; set; }
		public JobConsolCost MasterFreightCollectApportionment { get; set; }
		public List<InvoicingBase> AgentInvoices { get; set; }
		public readonly IJobCostingPlugIn Consol;
		public readonly ProfitShareDetailCollection CalculatedProfitShares;
		public readonly RefCurrency AgentInvoicePostingCurrency;
		public readonly ZDecimal AgentInvoicePostingExchangeRate;
		public readonly ChargePoster ChargePoster;
		public readonly JobConsolCostCollection AllConsolCosts;
		readonly List<JobConsolCost> ProfitShareApportionments = new List<JobConsolCost>();
	}
}
