using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public abstract class MatchingProcessor
	{
		public IEnumerable<InvoiceLineGroup> Process(IEnumerable<MatchingStrategy> strategies, bool isCrossLedgerImport)
		{
			var lineGroupDictionary = new Dictionary<GroupingKey, InvoiceLineGroup>();
			foreach (var strategy in strategies)
			{
				foreach (OrgType orgType in Enum.GetValues(typeof(OrgType)))
				{
					var lineGroupsWithSuggestions = GetInvoiceLineGroupsWithSuggestions(strategy, orgType);
					foreach (var lineGroup in lineGroupsWithSuggestions)
					{
						if (!lineGroupDictionary.ContainsKey(lineGroup.Key))
						{
							lineGroupDictionary.Add(lineGroup.Key, lineGroup);
						}
						else if (!lineGroupDictionary[lineGroup.Key].Equals(lineGroup))
						{
							ReportDeveloperExceptionIfMultipleInstancesAreUsedForSameGrouping();
						}
					}
				}
			}

			var result = GetLineGroupsAfterAdditionalFiltering(lineGroupDictionary.Values.ToList(), isCrossLedgerImport);

			return result;
		}

		protected abstract IEnumerable<InvoiceLineGroup> GetInvoiceLineGroupsWithSuggestions(MatchingStrategy strategy, OrgType orgType);

		protected virtual List<InvoiceLineGroup> GetLineGroupsAfterAdditionalFiltering(List<InvoiceLineGroup> lineGroupsWithSuggestions, bool isCrossLedgerImport)
		{
			return lineGroupsWithSuggestions;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		static void ReportDeveloperExceptionIfMultipleInstancesAreUsedForSameGrouping()
		{
			var message = (NoResString)@"In different strategies the LineGroupProviders and ChargeGroupProviders should be shared, rather than initialized for each strategy.
											We are caching the Groups returned in both the LineGroupProviders and ChargeGroupProviders
											If the same Groups are shared between the strategies then it should be not be possible to have different LineGroup instance with the same grouping key.";

			ErrorReporter.ReportOnce("DifferentLineGroupObjectInitializedBetweenGroupsWithSameKey", message, new Exception(message));
		}
	}

	public class JobLevelMatchingProcessor : MatchingProcessor
	{
		protected override IEnumerable<InvoiceLineGroup> GetInvoiceLineGroupsWithSuggestions(MatchingStrategy strategy, OrgType orgType)
		{
			return strategy.GetInvoiceLineGroupsWithMatchingSuggestions(orgType, false);
		}
	}

	public class ConsolLevelMatchingProcessor : MatchingProcessor
	{
		protected override IEnumerable<InvoiceLineGroup> GetInvoiceLineGroupsWithSuggestions(MatchingStrategy strategy, OrgType orgType)
		{
			return strategy.GetInvoiceLineGroupsWithMatchingSuggestions(orgType, true);
		}

		protected override List<InvoiceLineGroup> GetLineGroupsAfterAdditionalFiltering(List<InvoiceLineGroup> lineGroupsWithSuggestions, bool isCrossLedgerImport)
		{
			var multiplier = isCrossLedgerImport ? 1 : -1;
			var result = new List<InvoiceLineGroup>();

			var invoiceLineGroups = lineGroupsWithSuggestions.OrderByDescending(y => y.Suggestions.Last().Key);

			var consolCostDictionary = new Dictionary<ZGuid, (ZDecimal ConsolCostAmount, List<InvoiceLineGroup> MatchingInvoiceLineGroups)>();
			foreach (var invoiceLineGroup in invoiceLineGroups)
			{
				if (!invoiceLineGroup.Suggestions.Last().Value.ChargeGroup.IsConsolRelated)
				{
					result.Add(invoiceLineGroup);
				}
				else
				{
					var recordedCharge = invoiceLineGroup.Suggestions.Last().Value.ChargeGroup.GetCharges().First();

					var consolCostPk = recordedCharge.E6_PK;
					var consolCostAmount = recordedCharge.ConsolCostAmount;

					if (!consolCostDictionary.ContainsKey(consolCostPk))
					{
						var matchingInvoiceLineGroups = new List<InvoiceLineGroup> { invoiceLineGroup };
						consolCostDictionary.Add(consolCostPk, (consolCostAmount, matchingInvoiceLineGroups));
					}
					else
					{
						if (!InvoiceLineGroup.CheckIfAnyLinesContainInLineGroups(invoiceLineGroup.GetLines().ToList(), consolCostDictionary[consolCostPk].MatchingInvoiceLineGroups))
						{
							consolCostDictionary[consolCostPk].MatchingInvoiceLineGroups.Add(invoiceLineGroup);
						}
					}
				}
			}

			foreach (var item in consolCostDictionary)
			{
				var totalConsolCostAmount = item.Value.MatchingInvoiceLineGroups.Sum(x => x.TotalAmount);

				if (item.Value.ConsolCostAmount == multiplier * totalConsolCostAmount)
				{
					result.AddRange(item.Value.MatchingInvoiceLineGroups);
				}
			}

			return result;
		}
	}
}
