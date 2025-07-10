using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class ConsolJobFact : JobFact, IConsolJobFact
	{
		public ConsolJobFact(IJobInvoicingPlugIn jobPlugin,
			IEnvironmentFact environmentFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null,
			OrgHeader sendingAgent = null,
			IOrganisationWithMainAddressFact sendingAgentFact = null,
			OrgHeader receivingAgent = null,
			IOrganisationWithMainAddressFact receivingAgentFact = null,
			RefUNLOCO loadPort = null,
			IUNLOCOFact loadPortFact = null,
			RefUNLOCO dischargePort = null,
			IUNLOCOFact dischargePortFact = null)
			: base(jobPlugin?.InvoicingSupporter?.Job, environmentFact, localClientFact, salesRepFact)
		{
			ConsolPK = jobPlugin.PK.IsValid ? jobPlugin.PK.ToGuid() : Guid.Empty;

			var invoiceSupporter = jobPlugin.InvoicingSupporter;
			ConsolType = invoiceSupporter.ConsolType;
			ContainerMode = invoiceSupporter.ContainerMode;
			TransportMode = invoiceSupporter.TransportMode;
			SendingAgentCountry = sendingAgent?.MainAddress?.Country?.Code ?? string.Empty;
			SendingAgent = new FactLeftJoin<IOrganisationWithMainAddressFact>(sendingAgentFact);
			ReceivingAgentCountry = receivingAgent?.MainAddress?.Country?.Code ?? string.Empty;
			ReceivingAgent = new FactLeftJoin<IOrganisationWithMainAddressFact>(receivingAgentFact);
			LoadPortCountry = loadPort?.Country?.Code ?? string.Empty;
			LoadPort = new FactLeftJoin<IUNLOCOFact>(loadPortFact);
			DischargePortCountry = dischargePort?.Country?.Code ?? string.Empty;
			DischargePort = new FactLeftJoin<IUNLOCOFact>(dischargePortFact);

			if (jobPlugin is ForwardingConsol consol)
			{
				SendingAgentGatewayFlag = consol.JK_SendingForwarderHandlingType;
				ReceivingAgentGatewayFlag = consol.JK_ReceivingForwarderHandlingType;
				PaymentType = consol.JK_PrepaidCollect;
			}
		}

		public Guid ConsolPK { get; }

		public string ConsolType { get; }

		public string ContainerMode { get; }

		public string TransportMode { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> SendingAgent { get; }

		public string SendingAgentGatewayFlag { get; }

		public FactLeftJoin<IOrganisationWithMainAddressFact> ReceivingAgent { get; }

		public string ReceivingAgentGatewayFlag { get; }

		public FactLeftJoin<IUNLOCOFact> LoadPort { get; }

		public FactLeftJoin<IUNLOCOFact> DischargePort { get; }

		public string SendingAgentCountry { get; }

		public string ReceivingAgentCountry { get; }

		public string LoadPortCountry { get; }

		public string DischargePortCountry { get; }

		public string PaymentType { get; }
	}
}
