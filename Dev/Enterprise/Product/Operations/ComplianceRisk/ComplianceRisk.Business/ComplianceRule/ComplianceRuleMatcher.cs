using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public static class ComplianceRuleMatcher
	{
		internal static void MatchComplianceRule(ComplianceRiskBusinessObject pluginBizO)
		{
			if (pluginBizO.ComplianceCommodityRiskStatusProvider != null
				&& pluginBizO.ComplianceItemRiskStatusProvider.ComplianceRiskSupport.IsSupportInitialization()
				&& pluginBizO.IsCommodityRiskAssessable()
				&& pluginBizO.ComplianceItemRiskStatusProvider.JobTime.IsCurrent)
			{
				var commoditiesWithRules = GetCommoditiesWithRules(pluginBizO);
				var matched = false;

				foreach (var (commodity, rule) in commoditiesWithRules)
				{
					if (rule != null)
					{
						commodity.SetToStatusByComplianceRule(rule.CRU_RiskStatus);
						if (!matched)
						{
							if (!pluginBizO.ComplianceRiskStatus.IsAssessmentInitialized)
							{
								pluginBizO.ComplianceRiskStatus.InitializeAssessmentWorkflowAndApplyComplianceWithoutSave();
								pluginBizO.ComplianceRiskStatus.AssessmentInitializedByRule = true;
							}

							matched = true;
						}
					}
					else
					{
						commodity.BlockedByComplianceRule = false;
					}

					commodity.Validation.CheckCommodityStatus();
				}
			}
		}

		static IEnumerable<(ComplianceCommodityDetail commodity, ComplianceRule rule)> GetCommoditiesWithRules(ComplianceRiskBusinessObject pluginBizO)
		{
			var pointPairInfo = pluginBizO.ComplianceCommodityRiskStatusProvider?.AssessmentPointPairInfo;

			if (pointPairInfo?.SupportAssessmentByBorderWise ?? false)
			{
				var pointPairs = pointPairInfo.PointPairs.Select(u => (Origin: u.OriginPoint.Country ?? string.Empty, Destination: u.DestinationPoint.Country ?? string.Empty)).Distinct().ToArray();

				var origins = pointPairs.Select(u => u.Origin).Where(u => !string.IsNullOrEmpty(u)).Distinct().ToArray();
				var destinations = pointPairs.Select(u => u.Destination).Where(u => !string.IsNullOrEmpty(u)).Distinct().ToArray();

				ComplianceRule[] allRules;

				if (origins.Length > 0 || destinations.Length > 0)
				{
					var subQuery = new ZQuery();

					if (origins.Length > 0)
					{
						subQuery.AddToFilter(JoinCondition.Or, ComplianceRuleSchema.CRU_Origin, origins);
					}

					if (destinations.Length > 0)
					{
						subQuery.AddToFilter(JoinCondition.Or, ComplianceRuleSchema.CRU_Destination, destinations);
					}

					var zQuery = new ZQuery(ComplianceRuleSchema.CRU_RiskStatus, ComplianceRiskStatusCodeList.Codes.Blocked);
					zQuery.AddToFilter(subQuery);

					allRules = pluginBizO.ComplianceRiskStatus.Factory.Load<ComplianceRule>(zQuery);
				}
				else
				{
					allRules = Array.Empty<ComplianceRule>();
				}

				foreach (var commodity in GetApplicableCommodities(pluginBizO))
				{
					var commodityHsCode = ComplianceCommodityDetailCollection.ExtractAllNumbersFromTariffCode(commodity.CCD_HarmonizedCode);
					var applicableRules = GetApplicableRules(allRules, pointPairs, commodityHsCode);
					yield return (commodity, SelectRule(applicableRules));
				}
			}
		}

		#region private record ComplianceRuleMatch

		record ComplianceRuleMatch
		{
			public ComplianceRuleMatch(ComplianceRule rule, int dataPointsMatched, int codeLength, bool isBlocked)
			{
				Rule = rule;
				DataPointsMatched = dataPointsMatched;
				CodeLength = codeLength;
				IsBlocked = isBlocked;
			}

			public ComplianceRule Rule { get; }
			public int DataPointsMatched { get; }
			public int CodeLength { get; }
			public bool IsBlocked { get; }
		}

		#endregion

		/// <summary>
		/// Get the applicable rule based on Origin, Destination and Harmonized Code.
		/// When more than one rule matches the criteria, use the following priority:
		///   1. Match on more data points (e.g. Origin/Destination/Code has priority over Origin/Empty Destination/Code)
		///   2. Longer Code match(e.g. 123456 has priority over 1234 or empty)
		///   3. Blocked over Released
		/// </summary>
		/// <param name="rules">All rules</param>
		/// <returns>Applicable rule or null if no rules matched</returns>
		static ComplianceRule SelectRule(IEnumerable<ComplianceRuleMatch> matches)
		{
			return matches
				.OrderByDescending(m => m.DataPointsMatched)
				.ThenByDescending(m => m.CodeLength)
				.ThenByDescending(m => m.IsBlocked)
				.Select(m => m.Rule)
				.FirstOrDefault();
		}

		static IEnumerable<ComplianceCommodityDetail> GetApplicableCommodities(ComplianceRiskBusinessObject pluginBizO)
		{
			return pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Where(u => u.CommodityType != CommodityType.RelatedJobLink);
		}

		static void SetToStatusByComplianceRule(this ComplianceCommodityDetail commodity, ZString riskStatus)
		{
			commodity.NeedResetStatus = false;

			if (!commodity.CCD_RiskStatus.HasBlockedOrReleased())
			{
				commodity.CCD_RiskStatus = riskStatus;
			}

			// we set the flag even when the status was set to Blocked by user manually to display warning message
			commodity.BlockedByComplianceRule = commodity.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.Blocked
				&& riskStatus == ComplianceRiskStatusCodeList.Codes.Blocked;
		}

		static IEnumerable<ComplianceRuleMatch> GetApplicableRules(ComplianceRule[] allRules, (string Origin, string Destination)[] pointPairs, string harmonizedCode)
		{
			foreach (var rule in allRules)
			{
				foreach (var point in pointPairs)
				{
					if (!IsApplicable(rule, point, harmonizedCode))
					{
						continue;
					}

					var dataPointsMatched = 0;
					var codeLength = 0;
					if (!rule.CRU_Origin.IsEmpty && rule.CRU_Origin == point.Origin)
					{
						dataPointsMatched++;
					}

					if (!rule.CRU_Destination.IsEmpty && rule.CRU_Destination == point.Destination)
					{
						dataPointsMatched++;
					}

					if (!rule.CRU_HarmonizedCode.IsEmpty)
					{
						dataPointsMatched++;
						codeLength = rule.CRU_HarmonizedCode.Length;
					}

					yield return new ComplianceRuleMatch(rule, dataPointsMatched, codeLength, rule.CRU_RiskStatus == ComplianceRiskStatusCodeList.Codes.Blocked);
				}
			}
		}

		static bool IsApplicable(ComplianceRule rule, (string Origin, string Destination) pointPairs, string harmonizedCode)
		{
			return (rule.CRU_HarmonizedCode.IsEmpty || harmonizedCode.StartsWith(rule.CRU_HarmonizedCode)) &&
				(rule.CRU_Origin.IsEmpty && rule.CRU_Destination == pointPairs.Destination
				|| rule.CRU_Destination.IsEmpty && rule.CRU_Origin == pointPairs.Origin
				|| !rule.CRU_Origin.IsEmpty && !rule.CRU_Destination.IsEmpty && rule.CRU_Origin == pointPairs.Origin && rule.CRU_Destination == pointPairs.Destination);
		}
	}
}
