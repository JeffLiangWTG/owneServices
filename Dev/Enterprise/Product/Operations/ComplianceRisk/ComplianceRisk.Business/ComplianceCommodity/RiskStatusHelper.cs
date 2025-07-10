using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business
{
	public static class RiskStatusHelper
	{
		#region Get Filter Code Description Pair List

		public static CodeDescriptionPairList GetOverallCodeDescriptionPairListForFilter()
		{
			var list = GetOverallRiskStatusList();

			return list;
		}

		public static CodeDescriptionPairList GetPartyCodeDescriptionPairListForFilter()
		{
			var list = GetPartyRiskStatusList();

			return list;
		}

		public static CodeDescriptionPairList GetLocationCodeDescriptionPairListForFilter()
		{
			var list = GetLocationRiskStatusList();

			return list;
		}

		public static CodeDescriptionPairList GetCommodityCodeDescriptionPairListForFilter()
		{
			var list = GetComplianceCommodityRiskStatusList();

			return list;
		}

		#endregion

		internal static ZString GetCommodityRiskStatus(this ComplianceRiskStatus complianceRiskStatus, List<(ZString HarmonizedCode, ZString RiskStatus, CommodityType CommodityType)> commodities)
		{
			ZString result;

			if (commodities.Count > 0)
			{
				var worstRating = commodities
					.Select(u => GetCommodityStatusRating(u.RiskStatus))
					.OrderByDescending(u => u).FirstOrDefault();

				result = GetCommodityRiskStatus(worstRating);
			}
			else
			{
				result = complianceRiskStatus.GetCommodityRiskStatusWhenNoCommodities();
			}

			return result;
		}

		internal static void SetCommodityRiskStatus(this ComplianceRiskStatus complianceRiskStatus, IEnumerable<ComplianceCommodityDetail> commodities)
		{
			if (complianceRiskStatus.IsProceedWithoutCommodityRiskAssessmentCheck)
			{
				complianceRiskStatus.COR_CommodityRisk = Codes.NotAssessed;
			}
			else if (commodities.Any())
			{
				var worstRating = commodities
					.Select(u => GetCommodityStatusRating(u.CCD_RiskStatus))
					.OrderByDescending(u => u).FirstOrDefault();

				complianceRiskStatus.COR_CommodityRisk = GetCommodityRiskStatus(worstRating);
			}
			else
			{
				complianceRiskStatus.COR_CommodityRisk = complianceRiskStatus.GetCommodityRiskStatusWhenNoCommodities();
			}
		}

		internal static void SetOverallRiskStatus(this ComplianceRiskStatus complianceRiskStatus, bool ignoreOverrideClear)
		{
			if (ignoreOverrideClear || complianceRiskStatus.COR_OverallRisk != Codes.OverrideClear)
			{
				complianceRiskStatus.COR_OverallRisk = complianceRiskStatus.GetOverallRiskStatus();
			}
		}

		internal static ZString GetOverallRiskStatus(this ComplianceRiskStatus complianceRiskStatus)
		{
			var statusFactors = new[] {
				complianceRiskStatus.COR_CommodityRisk,
				complianceRiskStatus.COR_LocationRisk,
				complianceRiskStatus.COR_PartyRisk
			};

			var worstRating = statusFactors
				.Select(u => GetJobComplianceRiskRating(u))
				.OrderByDescending(u => u).First();

			return worstRating switch
			{
				JobComplianceRiskStatusRating.Blocked => Codes.Blocked,
				JobComplianceRiskStatusRating.Held => Codes.Held,
				JobComplianceRiskStatusRating.NotAssessed => Codes.Clear,
				_ => Codes.Clear,
			};

			static JobComplianceRiskStatusRating GetJobComplianceRiskRating(string status)
			{
				return status switch
				{
					Codes.Blocked => JobComplianceRiskStatusRating.Blocked,
					Codes.HighRisk or Codes.PotentialRisk or Codes.Incomplete or Codes.Unknown => JobComplianceRiskStatusRating.Held,
					_ => JobComplianceRiskStatusRating.Clear,
				};
			}
		}

		internal static ZString GetPartyRiskStatusBasedOnPartiesSnapshot(List<(ZGuid PK, ZString ScreeningStatus)> parties)
		{
			var worstScreeningStatus = (string)ScreeningStatusUpdater.GetWorstScreeningStatus(parties.Select(u => u.ScreeningStatus));

			return worstScreeningStatus switch
			{
				ScreeningStatusesList.Codes.Matched => Codes.Blocked,
				_ => OrganisationsDataRegistry.ScreeningStatusNotClear(worstScreeningStatus) ? Codes.HighRisk : Codes.Clear,
			};
		}

		/// <summary>
		/// Tries to calculate the commodity risk status based on the <see cref="ComplianceRiskBusinessObject" /> sub providers, for instance Shipments.
		/// </summary>
		/// <param name="complianceRiskBizO">Parent Business Object, for instance Consolidation.</param>
		/// <param name="result"><see cref="ZString" /> result.</param>
		/// <returns><see langword="true" /> if the commodity risk status has been obtained successfully.</returns>
		internal static bool TryGetSubProvidersCommodityRiskStatus(this ComplianceRiskBusinessObject complianceRiskBizO, out ZString result)
		{
			var subProviders = complianceRiskBizO.ComplianceCommodityRiskStatusProvider.SubComplianceRiskStatusProviders.ToArray();

			if (subProviders.Length == 0)
			{
				if (!ComplianceRiskHelper.IsComplianceCommodityRiskAssessmentEnabled)
				{
					result = Codes.NotAssessed;
					return true;
				}

				// If there are no sub providers, then the parent commodity risk status is unknown.
				result = Codes.Unknown;
				return true;
			}

			var providerParentIds = subProviders
				.Select((provider) => provider.ParentID)
				.ToArray();
			var query = new ZQuery(ComplianceRiskStatusSchema.COR_ParentID, providerParentIds);
			var providerStatuses = complianceRiskBizO.ComplianceRiskStatus.Factory.Load<ComplianceRiskStatus>(query);

			if (providerStatuses.Length > 0)
			{
				var worstRating = providerStatuses
					.Select((status) => GetSubProviderCommodityStatusRating(status.COR_CommodityRisk))
					.OrderByDescending((rating) => rating)
					.First();

				result = GetSubProviderCommodityRiskStatus(worstRating);
				return true;
			}

			result = Codes.Unknown; // Ensure 'result' is assigned before returning false
			return false;
		}

		static CommodityStatusRating GetCommodityStatusRating(string riskStatus)
		{
			return riskStatus switch
			{
				Codes.Blocked => CommodityStatusRating.Blocked,
				Codes.PotentialRisk or Codes.HighRisk => CommodityStatusRating.HighRisk,
				Codes.NotChecked => CommodityStatusRating.Unknown,
				Codes.PossibleRisk => CommodityStatusRating.PossibleRisk,
				_ => CommodityStatusRating.Clear,
			};
		}

		/// <summary>
		/// Ranks the provided commodity risk status of a status sub provider.
		/// </summary>
		/// <param name="riskStatus">Risk status <see cref="string" /> to rank.</param>
		/// <returns><see cref="SubProviderCommodityStatusRating" /> result.</returns>
		static SubProviderCommodityStatusRating GetSubProviderCommodityStatusRating(string riskStatus) =>
			riskStatus switch
			{
				Codes.Blocked => SubProviderCommodityStatusRating.Blocked,
				Codes.PotentialRisk or Codes.HighRisk => SubProviderCommodityStatusRating.HighRisk,
				Codes.PossibleRisk => SubProviderCommodityStatusRating.PossibleRisk,
				Codes.Incomplete => SubProviderCommodityStatusRating.Incomplete,
				Codes.Clear => SubProviderCommodityStatusRating.Clear,
				Codes.NotApplicable => SubProviderCommodityStatusRating.NotApplicable,
				_ => SubProviderCommodityStatusRating.Unknown
			};

		static string GetCommodityRiskStatus(CommodityStatusRating worstRating) =>
			worstRating switch
			{
				CommodityStatusRating.Blocked => Codes.Blocked,
				CommodityStatusRating.HighRisk => Codes.HighRisk,
				CommodityStatusRating.PotentialRisk => Codes.PotentialRisk,
				CommodityStatusRating.Unknown => Codes.Unknown,
				CommodityStatusRating.PossibleRisk => Codes.PossibleRisk,
				_ => Codes.Clear,
			};

		/// <summary>
		/// Gets the commodity risk status code by the worst rating when no commodities are present.
		/// </summary>
		/// <param name="worstRating">Worst status rating to determine the commodity risk status code.</param>
		/// <returns>Commodity risk status code.</returns>
		static string GetSubProviderCommodityRiskStatus(SubProviderCommodityStatusRating worstRating) =>
			worstRating switch
			{
				SubProviderCommodityStatusRating.Blocked => Codes.Blocked,
				SubProviderCommodityStatusRating.HighRisk => Codes.HighRisk,
				SubProviderCommodityStatusRating.PotentialRisk => Codes.PotentialRisk,
				SubProviderCommodityStatusRating.Incomplete => Codes.Incomplete,
				SubProviderCommodityStatusRating.Unknown => !ComplianceRiskHelper.IsComplianceCommodityRiskAssessmentEnabled ? Codes.NotAssessed : Codes.Unknown,
				SubProviderCommodityStatusRating.PossibleRisk => Codes.PossibleRisk,
				SubProviderCommodityStatusRating.NotAssessed => Codes.NotAssessed,
				SubProviderCommodityStatusRating.Clear => Codes.Clear,
				_ => Codes.NotApplicable,
			};

		enum CommodityStatusRating
		{
			Clear,
			PossibleRisk,
			Unknown,
			PotentialRisk,
			HighRisk,
			Blocked,
		}

		enum SubProviderCommodityStatusRating
		{
			NotApplicable,
			Clear,
			NotAssessed,
			PossibleRisk,
			Unknown,
			Incomplete,
			PotentialRisk,
			HighRisk,
			Blocked,
		}

		enum JobComplianceRiskStatusRating
		{
			Clear,
			NotAssessed,
			Held,
			Blocked,
		}
	}
}
