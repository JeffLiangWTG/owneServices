using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionConsolValidator
	{
		public GatewayProfitRedistributionConsolValidator(BusinessObjectFactory factory, ForwardingConsol forwardingConsol, IDisposableProfitShareRedistributionLogger logger)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(forwardingConsol, nameof(forwardingConsol));
			Argument.NotNull(logger, nameof(logger));

			this.factory = factory;
			this.forwardingConsol = forwardingConsol;
			this.logger = logger;
		}

		readonly BusinessObjectFactory factory;
		readonly ForwardingConsol forwardingConsol;
		readonly IDisposableProfitShareRedistributionLogger logger;

		public bool IsValid()
		{
			if (ProfitShareHelper.IsConsolAlreadyProcessed(factory, forwardingConsol.PK, forwardingConsol.InvoicingSupporter))
			{
				logger.Error($"{forwardingConsol.JK_UniqueConsignRef} is already processed.");
				return false;
			}

			//Consol should be gateway consol (//Sending Agent or Receiving Agent - one should be part of OrgProxy current company)
			if (!forwardingConsol.IsGatewayConsol)
			{
				logger.Error($"{forwardingConsol.JK_UniqueConsignRef} is not a Gateway Consol.");
				return false;
			}

			return true;
		}
	}
}
