using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class ConsolCostPosterTest : ConsolPostingWorkflowProcessorTest
	{
		protected override IProcessor CreateConsolPostingProcessor(ForwardingConsol consol)
		{
			return new ConsolCostPoster(consol);
		}
	}
}