using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare
{
	class GatewayProfitRedistributionValidator
	{
		public GatewayProfitRedistributionValidator(BusinessObjectFactory factory, IEnumerable<ProfitShareForwardingConsolWrapper> forwardingConsols, IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList, IDisposableProfitShareRedistributionLogger logger)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(forwardingConsols, nameof(forwardingConsols));
			Argument.NotNull(orgProfitShareDetailsList, nameof(orgProfitShareDetailsList));
			Argument.NotNull(logger, nameof(logger));

			this.factory = factory;
			this.forwardingConsols = forwardingConsols;
			this.orgProfitShareDetailsList = orgProfitShareDetailsList;
			this.logger = logger;
		}

		readonly BusinessObjectFactory factory;
		readonly IEnumerable<ProfitShareForwardingConsolWrapper> forwardingConsols;
		readonly IEnumerable<OrgProfitShareDetails> orgProfitShareDetailsList;
		readonly IDisposableProfitShareRedistributionLogger logger;

		public bool IsValid()
		{
			var result = true;

			if (AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value.GetCode(OrgProfitSharePartyLookups.PartyTypeCodes.LeadGatewayAgent).IsEmpty)
			{
				logger.Error((NoResString)"Profit Share Charge Codes Per Party registry is not set for Lead Gateway Agent party type.");
				return false;
			}

			logger.Information((NoResString)"Starting Consols' Validations ...");

			if (!forwardingConsols.Any())
			{
				logger.Error($"There is no consol for processing.");
				return false;
			}

			foreach (var wrapper in forwardingConsols)
			{
				var consolValidator = new GatewayProfitRedistributionConsolValidator(factory, wrapper.Consol, logger);
				if (!consolValidator.IsValid())
				{
					result = false;
					logger.Information($"{wrapper.Consol.JK_UniqueConsignRef} has failed to pass validation.");
				}
				else
				{
					logger.Information($"{wrapper.Consol.JK_UniqueConsignRef} is validated successfully.");
				}
			}
			logger.Information((NoResString)"Consols' Validations is completed.");

			logger.Information((NoResString)"Starting ProfitShare rules' Validations ...");

			if (!orgProfitShareDetailsList.Any())
			{
				logger.Error($"There is no profit share rule for processing.");
				return false;
			}

			var profitshareValidator = new GatewayProfitRedistributionOrgProfitShareDetailsValidator(orgProfitShareDetailsList, logger);

			if (!profitshareValidator.IsValid())
			{
				result = false;
			}

			logger.Information((NoResString)"ProfitShare rules' Validations is completed.");
			//Agency Office of the rules should not be blank
			//Agency Office of the rules should be one of the Branch Office of the current's company OrgProxy

			//Todo: refactor it later, take it from processor
			var shipments = forwardingConsols.SelectMany(x => x.Consol.Shipments.ToArray<ForwardingShipment>()).DistinctBy(x => x.PK).ToList();
			logger.Information((NoResString)"Starting Shipments' Validations ...");

			if (!shipments.Any())
			{
				logger.Error($"There is no shipment for processing.");
				return false;
			}

			foreach (var shipment in shipments)
			{
				var shipmentValidator = new GatewayProfitRedistributionShipmentValidator(factory, shipment, logger);
				if (!shipmentValidator.IsValid())
				{
					result = false;
					logger.Information($"{shipment.JS_UniqueConsignRef} has failed to pass validation.");
				}
				else
				{
					logger.Information($"{shipment.JS_UniqueConsignRef} is validated successfully.");
				}
			}
			logger.Information((NoResString)"Shipments' Validations is completed.");

			return result;
		}
	}
}
