using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing
{
	class BillingJob : ScavengingSubmissionJob<StmUsageData, LightweightOutboundBillingItem>
	{
#if DEBUG
		public
#endif
		BillingJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier)
			: base(serviceTaskSupport, notifier)
		{
		}

		public BillingJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

		internal override string MutexPrefix
		{
			get { return "SCVBLG"; }
		}

		internal override IReadOnlyCollection<LightweightOutboundBillingItem> GetPendingItems()
		{
			return BizOsToBillingItems(new BillingManager().GetPendingTransactions(Factory, BatchSize));
		}

		internal override IReadOnlyCollection<LightweightOutboundBillingItem> ValidatePendingItems(IReadOnlyCollection<LightweightOutboundBillingItem> itemsToValidate)
		{
			var itemsThatPassedBaseValidation = base.ValidatePendingItems(itemsToValidate);
			if (itemsThatPassedBaseValidation.Count <= 0)
			{
				return itemsThatPassedBaseValidation;
			}

			return BizOsToBillingItems(new BillingManager().ValidatePendingTransactions(Factory, itemsThatPassedBaseValidation.Select((bi) => bi.FullItem)));
		}

		static IReadOnlyCollection<LightweightOutboundBillingItem> BizOsToBillingItems(IEnumerable<BusinessObject> bizOs)
		{
			return bizOs.Cast<StmUsageData>().Select(item => new LightweightOutboundBillingItem(item)).ToList().AsReadOnly();
		}

		protected override void HandleFaultyItem(StmUsageData item, string error)
		{
			if (error.Contains((NoResString)"transaction validation failed") || error.Contains((NoResString)"outgoing message larger than the maximum receive size for the outbound service"))
			{
				BillingManager.MarkAsFailed(item.PK);
				if (CompanyTypeHelper.IsProductionSystem())
				{
					ErrorReporter.ReportOnce(GetExceptionKey(item, error), GetExceptionMessage(item, error));
				}
				else
				{
					Notifier.AddError(GetExceptionMessage(item, error));
				}
			}
		}

		protected override string GetSchemaName(LightweightOutboundBillingItem item)
		{
			return item.FullItem.SUD_Category == EDIMessageSchemaNameList.Codes.Usage ? EDIMessageSchemaNameList.Descriptions.Usage : EDIMessageSchemaNameList.Descriptions.Billing;
		}

		protected override string JobName
		{
			get { return (NoResString)"Billing"; }
		}

		protected override Stream SerializeToStream(LightweightOutboundBillingItem item)
		{
			return new MemoryStream(item.BillingData);
		}

		internal override int AdapterOutboxCountLimit
		{
			get { return BatchSize; }
		}

		static string GetExceptionKey(StmUsageData item, string error)
		{
			var missingElementsMatch = Regex.Match(error, "The element 'BillingTransaction'( in namespace '.+?')? has incomplete content\\. List of possible elements expected: '(?<missingElements>.+?)'( in namespace '.+?')?", RegexOptions.Singleline);
			var missingElementsString = missingElementsMatch.Groups["missingElements"].Captures.Cast<Capture>().Select(c => c.Value).FirstOrDefault();
			var missingElements = missingElementsString != null
				? missingElementsString.Split(',').Select(e => e.Trim())
				: Enumerable.Empty<string>();
			var invalidElements = from Match match in Regex.Matches(error, "The '(.+?:)?(?<element>\\w+?)' element is invalid", RegexOptions.Singleline)
								  from Capture capture in match.Groups["element"].Captures
								  select capture.Value;
			var problemElements = missingElements.Union(invalidElements).OrderBy(s => s, StringComparer.Ordinal);
			var errorSummary = string.Join(", ", problemElements);
			if (string.IsNullOrWhiteSpace(errorSummary))
			{
				errorSummary = (NoResString)"Unknown problem";
			}
			return string.Format(CultureInfo.InvariantCulture, "BillingTransaction.ExternalValidationError: {0}/{1} - {2}", item.SUD_Category, item.SUD_Code, errorSummary);
		}

		static string GetExceptionMessage(StmUsageData item, string error)
		{
			return
				BillingManager.GetTransactionXml(item) + System.Environment.NewLine +
				"----------" + System.Environment.NewLine +
				error;
		}

		internal const int BatchSize = 1000;
	}
}
