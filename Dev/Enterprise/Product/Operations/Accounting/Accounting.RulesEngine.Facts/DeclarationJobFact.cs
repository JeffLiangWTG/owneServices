using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class DeclarationJobFact : CustomFieldJobFact, IDeclarationJobFact
	{
		public DeclarationJobFact(IJobInvoicingPlugIn jobPlugin, IEnvironmentFact environmentFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null)
			: base(jobPlugin?.InvoicingSupporter?.Job, environmentFact, localClientFact, salesRepFact)
		{
			ShipmentType = (jobPlugin.InvoicingSupporter as IServiceDirection)?.ServiceDirection;
			TransportMode = jobPlugin.InvoicingSupporter.TransportMode;
			ContainerMode = jobPlugin.InvoicingSupporter.ContainerMode;
		}

		public string ShipmentType { get; }

		public string TransportMode { get; }

		public string ContainerMode { get; }
	}
}
