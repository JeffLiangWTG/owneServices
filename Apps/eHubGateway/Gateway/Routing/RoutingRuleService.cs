using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.Common;
using Common.Logging;
using eServices.eHubDataAccess.Integration;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using eServices.Routing.OceanCarrierMessaging;

namespace CargoWise.eHub.Gateway.Routing
{
	public class RoutingRuleService : IRoutingRuleService
	{
		private readonly Func<eHubTransactionsContext> dbContextFactory;
		private readonly ILog logger;
		const string Client_OCM_Copying = "OCM_MutipleRecipientsCopying";
		private readonly Func<eHubTransactionsContext, IRoutingRuleFactory> ruleFactory;
		private readonly RoutingClientsConfiguration routingConfig;
		private readonly Func<IPartyAccessor> partyAccessorFactory;

		public RoutingRuleService() : this(
			() => new eHubTransactionsContext(),
			LogManager.GetLogger(typeof(RoutingRuleService)),
			DataAccessFactories.NewPartyAccessorInstance,
			System.Configuration.ConfigurationManager.GetSection("RoutingClients") as RoutingClientsConfiguration
			)
		{
		}

		public RoutingRuleService(
			Func<eHubTransactionsContext> contextFactory,
			ILog logger,
			Func<IPartyAccessor> partyAccessorFactory,
			RoutingClientsConfiguration routingConfig) : this(
				contextFactory,
				logger,
				(cn) => new RoutingRuleFactory(cn, logger),
				partyAccessorFactory,
				routingConfig)
		{
		}

		public RoutingRuleService(Func<eHubTransactionsContext> contextFactory, ILog logger, Func<eHubTransactionsContext, IRoutingRuleFactory> routingRuleFactory, Func<IPartyAccessor> partyAccessorFactory, RoutingClientsConfiguration routingConfig)
		{
			this.dbContextFactory = contextFactory;
			this.logger = logger;
			this.ruleFactory = routingRuleFactory;
			this.routingConfig = routingConfig;
			this.partyAccessorFactory = partyAccessorFactory;
		}

		public bool IsXHMessage(eHubGatewayMessage message, string senderID, out string resolvedRecipient)
		{
			resolvedRecipient = string.Empty;
			if (routingConfig != null && routingConfig.Enabled && routingConfig.Clients.TryGetValue(message.ClientID, out var routingClientConfig))
			{
				try
				{
					using (var context = dbContextFactory())
					{
						var routingRuleFactory = ruleFactory(context);

						logger.InfoFormat("Starting xHub Rule Evaluation for Sender {0} Recipient {1} Tracking ID '{2}'", senderID, message.ClientID, message.MessageTrackingID);

						Result[] results = null;
						switch (routingClientConfig.RoutingMethod)
						{
							case RoutingMethod.Standard:
								results = Evaluate(context, routingRuleFactory, routingClientConfig.RouteId, new Dictionary<string, string>() { { "SourceParty", senderID } }, message);
								break;
							case RoutingMethod.OCM:
								results = EvaluateOCM(context, routingRuleFactory, routingClientConfig.RouteId, new Dictionary<string, string>() { { "SourceParty", senderID } }, message);
								break;
						}
						if (results != null && results.Length > 0)
						{
							if (!string.IsNullOrWhiteSpace(results[0].RecipientId))
							{
								var result = partyAccessorFactory().IsXHSystem(results[0].RecipientId);
								logger.InfoFormat("Finished xHub Rule Evaluation for Sender {0} Recipient {1} Tracking ID '{2}' RouteResult={3} IsXHub={4}", senderID, message.ClientID, message.MessageTrackingID, results[0].RecipientId, result);
								if (result)
								{
									resolvedRecipient = results[0].RecipientId;
								}
								return result;
							}
							else
							{
								logger.WarnFormat("eHubGateway failed to route the message for Sender:{0}, Recipient: {1}, Tracking ID: '{2}', ErrorDescription: {3}", senderID, message.ClientID, message.MessageTrackingID, results[0]?.ErrorDescription);
							}
						}
						else
						{
							logger.WarnFormat("eHubGateway failed to route the message. Routing rule engine returned no results for Sender:{0}, Recipient: {1}, Tracking ID: '{2}'", senderID, message.ClientID, message.MessageTrackingID);
						}
						return false;
					}
				}
				catch (Exception ex)
				{
					logger.ErrorFormat("An exception occured during the RouteRule execution for Sender:{0}, Recipient: {1}, Tracking ID: '{2}'.", ex, senderID, message.ClientID, message.MessageTrackingID);
					throw ex;
				}
			}
			return false;
		}

		public Result[] Evaluate(eHubTransactionsContext dbContext, IRoutingRuleFactory ruleFactory, string ruleId, Dictionary<string, string> propertyFacts, eHubGatewayMessage message)
		{
			logger.InfoFormat("Starting evaluation in WS for RuleId [{0}]", ruleId);

			var rule = ruleFactory.GetForReading(ruleId);
			using (var messageFactResolver = new MessageFactResolver(message))
			{
				var results = rule.Evaluate(dbContext, new IFactResolver[] { new PropertyFactResolver(propertyFacts), messageFactResolver, new SqlFactResolver(dbContext) }, LogManager.GetLogger(typeof(Rule)));
				return results.ToArray();
			}
		}

		public Result[] EvaluateOCM(eHubTransactionsContext dbContext, IRoutingRuleFactory ruleFactory, string ruleId, Dictionary<string, string> propertyFacts, eHubGatewayMessage message)
		{
			var filteredResults = new List<Result>();

			using (var messageFactResolver = new MessageFactResolver(message))
			{
				var propertyFactResolver = new PropertyFactResolver(propertyFacts);
				var sqlFactResolver = new SqlFactResolver(dbContext);
				var resolvers = new IFactResolver[] { messageFactResolver, propertyFactResolver, sqlFactResolver };

				var results = RoutingRuleEvaluation.EvaluateOCM(dbContext, ruleFactory, Client_OCM_Copying, "SHIPPING_INSTRUCTION", resolvers, logger);

				if (results[1] != null)
				{
					filteredResults.Add(results[1]);
				}

				if (results.Count == 3)
				{
					filteredResults.Add(results[2]);
				}

				logger.Info("Finished processing message");
				return filteredResults.ToArray();
			}
		}
	}
}
