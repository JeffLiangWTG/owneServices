using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business
{
	public static class RatingExtensions
	{
		/// <summary>
		///		The number of autoRate infos exceeding which will trigger parallelized calculation for better performance.
		/// </summary>
		public const int ThresholdForFindBestMatchesParallelizedCalculation = 100;

		public static IEnumerable<(AutoRateInfo rateInfo, BaseCharge charge, bool duplicateCharge)> FindBestMatches(this IEnumerable<AutoRateInfo> rateInfos, IEnumerable<BaseCharge> charges, CostSell costOrSell)
		{
			return rateInfos
				.Select(i => new FastAutoRateInfo(i, costOrSell))
				.FindBestMatches(charges, costOrSell);
		}

		static IEnumerable<(AutoRateInfo rateInfo, BaseCharge charge, bool duplicateCharge)> FindBestMatches(this IEnumerable<FastAutoRateInfo> fastRateInfos, IEnumerable<BaseCharge> charges, CostSell costOrSell)
		{
			// The performance of this method is tested in:
			// https://devops.wisetechglobal.com/wtg/InternalTools/_git/CWBenchmarks?path=%2FBusiness%2FRating%2FMergeChargesBenchmark.cs

			// In some cases, like warehouse, we may have thousands of charges with exactly the same attribute. So,
			// we group them by attributes (SimilarityKey) so that we calculate the score for them once, not M x N times.
			var newCharges = fastRateInfos
				.GroupBy(i => i.SimilarityKey)
				.ToDictionary(g => g.Key, g => g.ToList());

			var currentCharges = charges
				.Select(c => new FastCharge(c, costOrSell))
				.GroupBy(c => c.SimilarityKey)
				.ToDictionary(g => g.Key, g => g.ToList());

			var uniqueNewCharges = newCharges.Select(x => x.Value.First()).ToList();
			var uniqueCurrentCharges = currentCharges.Select(x => x.Value.First()).ToList();

			// Periodic Billing may have tens or even hundreds of thousands of charges.
			// Calculating scores NxM times will be quite CPU intensive, so, it makes sense to parallelize it.
			// On test data (10K of rate infos x 10K of charges) it shows improvement from 29 seconds to 6 seconds.
			var scores = newCharges.Count <= ThresholdForFindBestMatchesParallelizedCalculation
				? GetMatchingScores(uniqueNewCharges, uniqueCurrentCharges, costOrSell)
				: GetMatchingScoresParallelized(uniqueNewCharges, uniqueCurrentCharges, costOrSell);

			var matches = new List<(AutoRateInfo rateInfo, BaseCharge charge, bool duplicateCharge)>();

			// Matching AutoRateInfos groups with Charges groups from the highest score to the lowest.
			// A match with the highest score wins and corresponding AutoRate infos and existing Charges with the lowest score
			// get excluded.
			//
			// For example, for the following setup:
			//	3 x Info_1 (i.e. we have 3 instances of AutoRateInfo with the same attributes)
			//	4 x Info_2
			//	5 x Charge_A (i.e. we have 5 instances of Charge with the same attributes)
			//	1 x Charge_B
			//
			// We will calculate scores for each unique pair of attributes, i.e. 4 pairs:
			// 1: Info_1 Charge_A = 6
			// 2: Info_2 Charge_A = 12
			// 3: Info_1 Charge_B = 3
			// 4: Info_2 Charge_B = 5
			//
			// Here is the matching steps:
			//
			// 0. Matched: None
			//	  Unmatched: 3xInfo_1, 4x_Info_2, 5xCharge_A, 1xCharge_B
			//
			// 1. Matched: 4 x (Info_2 <=> Charge_A, Score 12)
			//	  Unmatched: 3xInfo_1, 1xCharge_A, 1xCharge_B
			//
			// 2. Matched: 4 x (Info_2 <=> Charge_A, Score 12), 1 x (Info_1 <=> Charge_A, Score 6)
			//	  Unmatched: 2xInfo_1, 1xCharge_B
			//
			// 3. Matched: 4 x (Info_2 <=> Charge_A, Score 12), 1 x (Info_1 <=> Charge_A, Score 6), 1 x (Info_1 <=> Charge_B, Score 3)
			//	  Unmatched: 1xInfo_1
			//
			foreach (var score in scores.Keys.OrderByDescending(s => s))
			{
				var sameScoreMatches = scores[score];
				var matchesAsList = sameScoreMatches as IList<MatchInfo> ?? sameScoreMatches.ToList();

				for (int i = 0; i < matchesAsList.Count; ++i)
				{
					var pair = matchesAsList[i];
					var similarNewCharges = newCharges[pair.FastRateInfo.SimilarityKey];
					var similarCurrentCharges = currentCharges[pair.FastCharge.SimilarityKey];

					// Auto-matching charges sharing the same attributes
					while (similarNewCharges.Count > 0 && similarCurrentCharges.Count > 0)
					{
						var newCharge = similarNewCharges[0];
						var existingCharge = similarCurrentCharges[0];

						matches.Add((newCharge.AutoRateInfo, existingCharge.Charge, false));
						similarNewCharges.Remove(newCharge);
						similarCurrentCharges.Remove(existingCharge);

						if (pair.RateInfo != null)
						{
							CreateRevenueMatchesForOtherCarriers(costOrSell, matchesAsList, i, matches, newCharges, currentCharges);
						}
					}
				}
			}

			return matches;
		}

		static void CreateRevenueMatchesForOtherCarriers(
			CostSell costOrSell,
			IList<MatchInfo> samesScoreMatches,
			int matchIndex,
			List<(AutoRateInfo rateInfo, BaseCharge charge, bool duplicateCharge)> matches,
			Dictionary<FastAutoRateInfo.SimilarityKeyImpl, List<FastAutoRateInfo>> newCharges,
			Dictionary<FastCharge.SimilarityKeyImpl, List<FastCharge>> currentCharges)
		{
			if (costOrSell == CostSell.Revenue)
			{
				Revenue_CreateRevenueMatchesForOtherCarriers(samesScoreMatches, matchIndex, matches, currentCharges);
			}
			else
			{
				Costing_CreateRevenueMatchesForOtherCarriers(samesScoreMatches, matchIndex, matches, newCharges);
			}
		}

		static void Revenue_CreateRevenueMatchesForOtherCarriers(
			IList<MatchInfo> samesScoreMatches,
			int matchIndex,
			List<(AutoRateInfo rateInfo, BaseCharge charge, bool duplicateCharge)> matches,
			Dictionary<FastCharge.SimilarityKeyImpl, List<FastCharge>> currentCharges)
		{
			var matchInfo = samesScoreMatches[matchIndex];
			if (!matchInfo.RateInfo.ProviderPK.IsEmpty
				|| !matchInfo.FastRateInfo.TransportProviderPK.IsEmpty)
			{
				// Revenue rate has a specific provider/creditor.
				// It's not valid to match it to a charge with another carrier/creditor.
				return;
			}

			var creditorPK = matchInfo.Charge.JR_OH_CostAccount;
			if (creditorPK.IsEmpty)
			{
				return;
			}

			var carriers = BuildCarrierSet(matchInfo.RateInfo);
			if (carriers == null || carriers.Count <= 1 || !carriers.Remove(creditorPK))
			{
				return;
			}

			// Given match has creditor that matches a carrier.
			// Check for other matches to other carriers.
			for (int j = matchIndex + 1; j < samesScoreMatches.Count; ++j)
			{
				var otherMatch = samesScoreMatches[j];
				if (ReferenceEquals(otherMatch.RateInfo, matchInfo.RateInfo)
					&& carriers.Remove(otherMatch.Charge.JR_OH_CostAccount))
				{
					// Note, AutoRateInfo shouldn't be modified by the caller.
					// So it should be fine to reuse the AutoRateInfo by reference and not have to make a clone.
					matches.Add((matchInfo.RateInfo, otherMatch.Charge, false));
					currentCharges[otherMatch.FastCharge.SimilarityKey].Remove(otherMatch.FastCharge);
				}
			}
		}

		static void Costing_CreateRevenueMatchesForOtherCarriers(
			IList<MatchInfo> samesScoreMatches,
			int matchIndex,
			List<(AutoRateInfo rateInfo, BaseCharge charge, bool duplicateCharge)> matches,
			Dictionary<FastAutoRateInfo.SimilarityKeyImpl, List<FastAutoRateInfo>> newCharges)
		{
			var matchInfo = samesScoreMatches[matchIndex];
			var creditorPK = matchInfo.RateInfo.ProviderPK;
			if (creditorPK.IsEmpty)
			{
				// Cost rate must have a creditor - that matches a carrier
				return;
			}

			if (!matchInfo.Charge.JR_OH_CostAccount.IsEmpty
				|| !matchInfo.FastCharge.TransportProviderPK.IsEmpty)
			{
				// We are only duplicating charges that don't have creditor, nor a provider.
				// The duplicates will get the carriers as the creditor.
				return;
			}

			var carriers = BuildCarrierSet(matchInfo.RateInfo);
			if (carriers == null || carriers.Count <= 1 || !carriers.Remove(creditorPK))
			{
				return;
			}

			// Given match has creditor that matches a carrier.
			// Check for other matches to other carriers.
			for (int j = matchIndex + 1; j < samesScoreMatches.Count; ++j)
			{
				var otherMatch = samesScoreMatches[j];
				if (ReferenceEquals(otherMatch.Charge, matchInfo.Charge)
					&& carriers.Remove(otherMatch.RateInfo.ProviderPK))
				{
					matches.Add((otherMatch.RateInfo, otherMatch.Charge, true));
					newCharges[otherMatch.FastRateInfo.SimilarityKey].Remove(otherMatch.FastRateInfo);
				}
			}
		}

		static HashSet<ZGuid> BuildCarrierSet(AutoRateInfo rateInfo)
		{
			var ratingOrgs = rateInfo.RatingOrgs;
			var possibleCarriers = ratingOrgs?.PossibleCarriers;
			if (possibleCarriers == null || !possibleCarriers.Any())
			{
				return null;
			}

			var result = new HashSet<ZGuid>();
			if (ratingOrgs.Carrier != null)
			{
				result.Add(ratingOrgs.Carrier.PK);
			}
			foreach (var possible in possibleCarriers)
			{
				result.Add(possible.PK);
			}

			return result;
		}

		/// <summary>
		/// Find best match for an Apportion Split Charge <see cref="ApportionSplitCharge"/> among existing charges
		/// with charge attributes.
		/// </summary>
		/// <param name="charge">The ApportionSplitCharge</param>
		/// <param name="charges">Existing charge to match the cost charge</param>
		/// <returns>Matched charges ordered by scores</returns>
		public static IEnumerable<BaseCharge> FindBestMatchesWithAttributes(this BaseCharge charge, IEnumerable<BaseCharge> charges)
		{
			if (!charge.JobChargeAttributes.Any())
			{
				return charges;
			}

			var fastInfo = new FastAutoRateInfo(charge);
			var bestMatches = new[] { fastInfo }.FindBestMatches(charges, CostSell.Cost);
			return bestMatches.Select(x => x.charge);
		}

		static IDictionary<long, IEnumerable<MatchInfo>> GetMatchingScores(IEnumerable<FastAutoRateInfo> rateInfos, IEnumerable<FastCharge> charges, CostSell costOrSell)
		{
			var scores = new Dictionary<long, IEnumerable<MatchInfo>>();

			foreach (var rateInfo in rateInfos)
			{
				foreach (var charge in charges)
				{
					var score = GetScore(rateInfo, charge, costOrSell);
					if (score <= 0)
					{
						continue;
					}

					if (!scores.TryGetValue(score, out var matches))
					{
						matches = new List<MatchInfo>();
						scores[score] = matches;
					}

					var list = (List<MatchInfo>)matches;
					list.Add(new MatchInfo(rateInfo, charge));
				}
			}

			return scores;
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "It is two different concurent collections")]
		static IDictionary<long, IEnumerable<MatchInfo>> GetMatchingScoresParallelized(IEnumerable<FastAutoRateInfo> rateInfos, IEnumerable<FastCharge> charges, CostSell costOrSell)
		{
			var scores = new ConcurrentDictionary<long, IEnumerable<MatchInfo>>();

			Parallel.ForEach(rateInfos, rateInfo =>
			{
				foreach (var charge in charges)
				{
					var score = GetScore(rateInfo, charge, costOrSell);
					if (score <= 0)
					{
						continue;
					}

					var newMatch = new MatchInfo(rateInfo, charge);

					scores.AddOrUpdate(
						score,
						addValue: new ConcurrentBag<MatchInfo> { newMatch },
						updateValueFactory: (key, existingMatches) =>
						{
							((ConcurrentBag<MatchInfo>)existingMatches).Add(newMatch);
							return existingMatches;
						});
				}
			});

			return scores;
		}

		static long GetScore(FastAutoRateInfo rateInfo, FastCharge charge, CostSell costOrSell)
		{
			if (!rateInfo.ChargePK.IsEmpty && rateInfo.ChargePK == charge.PK)
			{
				// ChargePK is set by CostBased Calculator, so, we should rely on it when matching charges
				return long.MaxValue;
			}

			var finalScore = 0L;
			var placeholder = 1L;

			foreach (var score in GetMatchScores(rateInfo, charge, costOrSell))
			{
				if (score == NotMatched)
				{
					return NotMatched;
				}

				finalScore += score * placeholder;
				placeholder *= 10L;
			}

			return finalScore;
		}

		static IEnumerable<long> GetMatchScores(FastAutoRateInfo rateInfo, FastCharge charge, CostSell costOrSell)
		{
			yield return GetChargeCodeMatchScore(rateInfo, charge);
			yield return GetCostReferenceMatchScore(rateInfo, charge);
			yield return GetSellReferenceMatchScore(rateInfo, charge);
			yield return GetPartyMatchScore(rateInfo, charge);
			yield return GetOrderReferenceMatchScore(rateInfo, charge);
			yield return GetCartageLegOrContainerMatchScore(rateInfo, charge);
			yield return GetProviderMatchScore(rateInfo, charge, costOrSell);
			yield return GetLocationInformationScore(rateInfo, charge);
			yield return GetServiceIDMatchScore(rateInfo, charge);
			yield return GetCurrencyMatchScore(rateInfo, charge);
			yield return GetCanReAutoRateMatchScore(charge);
		}

		const int NotMatched = -1;
		const int Ineffective = 0;

		static long GetCanReAutoRateMatchScore(FastCharge charge) =>
			charge.CanReAutoRate ? 2L : 1L;

		static long GetChargeCodeMatchScore(FastAutoRateInfo rateInfo, FastCharge charge) =>
			rateInfo.ChargeCodePK != charge.ChargeCodePK
				? NotMatched
				: Ineffective;

		static long GetCostReferenceMatchScore(FastAutoRateInfo rateInfo, FastCharge charge)
		{
			if (string.IsNullOrEmpty(rateInfo.OperationalJobRef) && !string.IsNullOrEmpty(charge.CostReference))
			{
				return 4L; // partial match
			}

			if (rateInfo.OperationalJobRef == charge.CostReference)
			{
				return 9L; // exact match
			}

			return NotMatched; // cannot be matched
		}

		static long GetSellReferenceMatchScore(FastAutoRateInfo rateInfo, FastCharge charge) =>
			rateInfo.SellReferenceNumber != charge.SellReference
				? NotMatched
				: Ineffective;

		/// <summary>
		///		Matching by service provide and transport provider. The following scores are calculated and confirmed with Peter:
		///
		///		Sell Carrier	|	Cost Service Provide	| Cost Carrier	|	 Score
		///			AAA												AAA				9
		///			AAA						AAA										8
		///			AAA						BBB						AAA				7
		///			AAA						AAA						BBB				6
		///			AAA																5
		///			AAA						BBB										4
		///			AAA												BBB				4
		///									AAA						BBB				5
		///			CCC						AAA						BBB				-1
		/// </summary>
		static int GetProviderMatchScore(FastAutoRateInfo rateInfo, FastCharge charge, CostSell costOrSell)
		{
			if (costOrSell != CostSell.Revenue)
			{
				// May have some side effects when cost autorated after revenue, I.e. costs may not properly match revenue charges.
				// If this happens, then this is the place to look at. But for now, keep it as it is as it was like this since 2019.
				return Ineffective;
			}

			var sellCarrier = rateInfo.TransportProviderPK;
			var costServiceProvider = charge.ServiceProviderPK;
			var costCarrier = charge.TransportProviderPK;

			// If all are empty, treat as a match
			if (sellCarrier.IsEmpty && costServiceProvider.IsEmpty && costCarrier.IsEmpty)
			{
				return 9;
			}

			// Empty sell carrier matches always
			if (sellCarrier.IsEmpty)
			{
				return 5;
			}

			if (sellCarrier == costCarrier && costServiceProvider.IsEmpty)
			{
				return 9;
			}

			if (sellCarrier == costServiceProvider && costCarrier.IsEmpty)
			{
				return 8;
			}

			if (sellCarrier == costCarrier && !costServiceProvider.IsEmpty)
			{
				return 7;
			}

			if (sellCarrier == costServiceProvider && !costCarrier.IsEmpty)
			{
				return 6;
			}

			if (costServiceProvider.IsEmpty && costCarrier.IsEmpty)
			{
				return 5;
			}

			if (costServiceProvider.IsEmpty || costCarrier.IsEmpty)
			{
				return 4;
			}

			return NotMatched;
		}

		static long GetServiceIDMatchScore(FastAutoRateInfo rateInfo, FastCharge charge)
		{
			// Seems like there is no consistency in CW1 codebase between empty string and null, so, better to cast values to empty string.
			if ((rateInfo.ServiceID ?? string.Empty) != (charge.ServiceID ?? string.Empty))
			{
				return NotMatched;
			}

			return Ineffective;
		}

		static long GetPartyMatchScore(FastAutoRateInfo rateInfo, FastCharge charge)
		{
			if (charge.Party == rateInfo.Party)
			{
				return 9L;
			}

			if (charge.Party.IsEmpty)
			{
				// If the charge has no party then a match on ProviderPK is just as good
				if (!rateInfo.Party.IsEmpty && charge.TransportProviderPK == rateInfo.Party)
				{
					return 9L;
				}
				else
				{
					return 1L;
				}
			}
			else if (rateInfo.Party.IsEmpty)
			{
				return 1L;
			}

			return NotMatched;
		}

		static long GetOrderReferenceMatchScore(FastAutoRateInfo rateInfo, FastCharge charge)
		{
			if (!string.IsNullOrEmpty(rateInfo.JobRef))
			{
				// Preserved the existing matching logic.
				//
				// Basically, for order reference we are matching with JR_Calc_RelatedJobNumber then with JR_OrderReference then with JR_JobNumber.
				// It is some domestic team stuff, basically, they put job number in OrderReference and then expect it to match corresponding charge by Job Number.
				//
				// Please talk to domestic team if you have issues with this.

				if (!string.IsNullOrEmpty(charge.RelatedJobNumber))
				{
					return StringComparer.CurrentCultureIgnoreCase.Compare(rateInfo.JobRef, charge.RelatedJobNumber) == 0
						? 9L
						: NotMatched;
				}

				if (!string.IsNullOrEmpty(charge.OrderReference))
				{
					return StringComparer.CurrentCultureIgnoreCase.Compare(rateInfo.JobRef, charge.OrderReference) == 0
						? 8L
						: NotMatched;
				}

				if (!string.IsNullOrEmpty(charge.JobNumber))
				{
					return StringComparer.CurrentCultureIgnoreCase.Compare(rateInfo.JobRef, charge.JobNumber) == 0
						? 7L
						// Charge may have been created while autorating sub shipment but it's job number is loaded from job header of lead shipment.
						// So, when new rate info has OrderReference == sub-shipment number, it does not match charge.JobNumber.
						// However, JobNumbersReference(s) may still match, if they present.
						: GetJobNumbersReferenceMatchScore(rateInfo.JobNumbersReference, charge.JobNumbersReference);
				}
			}

			if (string.IsNullOrEmpty(charge.OrderReference) && string.IsNullOrEmpty(charge.RelatedJobNumber))
			{
				return 1L;
			}

			return NotMatched;
		}

		static long GetCartageLegOrContainerMatchScore(FastAutoRateInfo rateInfo, FastCharge charge)
		{
			// Since a cartage leg has only 1 container, if there are cartage legs, comparing them is enough. We don't need to compare containers.
			// When there is no cartage leg in charge then we try match container codes and number.
			// A scenario when autorating runsheet and transport booking:
			// - Auto rate runsheet => autorate cost cartage legs (without recording leg PKs - see CommonWorkSheetRatingAdapterProvider) => cost charges have no CartageLegPKs
			// - Then autorate revenue port transport (with leg PKs - see CommonCartageRatingAdaptersProvider and CartageMoveRatingAdapter) => revenue charges have CartageLegPKs
			// - We then let the charges having the same container code and container number merge.
			if (charge.CartageLegPK.IsEmpty || rateInfo.CartageLegPK.IsEmpty)
			{
				return GetContainerCodeMatchScore(rateInfo, charge);
			}

			return rateInfo.CartageLegPK == charge.CartageLegPK ? 9L : NotMatched;
		}

		static long GetContainerCodeMatchScore(FastAutoRateInfo rateInfo, FastCharge charge)
		{
			var isRateContainerCodeEmpty = string.IsNullOrEmpty(rateInfo.ContainerCode);
			var isChargeContainerCodeEmpty = string.IsNullOrEmpty(charge.ContainerCode);
			if (isRateContainerCodeEmpty && isChargeContainerCodeEmpty)
			{
				return 9L;
			}

			if (StringComparer.OrdinalIgnoreCase.Compare(rateInfo.ContainerCode, charge.ContainerCode) == 0)
			{
				return GetContainerNumberMatchScore(rateInfo.ContainerNumber, charge.ContainerNumber);
			}

			if (isRateContainerCodeEmpty || isChargeContainerCodeEmpty)
			{
				return 1L;
			}

			return NotMatched;
		}

		/// <summary>
		/// Get container number score only when container codes matched.
		/// </summary>
		static long GetContainerNumberMatchScore(string rateContainerNumber, string chargeContainerNumber)
		{
			if (string.IsNullOrEmpty(rateContainerNumber))
			{
				return 2L;
			}

			if (rateContainerNumber == chargeContainerNumber)
			{
				return 9L;
			}

			if (string.IsNullOrEmpty(chargeContainerNumber))
			{
				return 2L;
			}

			return NotMatched;
		}

		static long GetCurrencyMatchScore(FastAutoRateInfo rateInfo, FastCharge charge)
		{
			// Keeping current functionality, i.e. we always have been looking at cost currency regardless of cost or sell.
			if (rateInfo.Currency == charge.CostCurrency)
			{
				return 9L;
			}

			if (string.IsNullOrEmpty(rateInfo.Currency) || string.IsNullOrEmpty(charge.CostCurrency))
			{
				return 1L;
			}

			return Ineffective;
		}

		static long GetLocationInformationScore(FastAutoRateInfo rateInfo, FastCharge charge)
		{
			if ((rateInfo.Location == charge.Location) && (rateInfo.LocationType == charge.LocationType))
			{
				return 9L;
			}

			if (string.IsNullOrEmpty(rateInfo.Location)
				&& string.IsNullOrEmpty(charge.Location)
				&& string.IsNullOrEmpty(rateInfo.LocationType)
				&& string.IsNullOrEmpty(charge.LocationType))
			{
				return 9L;
			}

			return NotMatched;
		}

		/// <summary>
		/// Only compare Job Number References when they have
		/// </summary>
		static long GetJobNumbersReferenceMatchScore(string rateReference, string chargeReference) =>
			!string.IsNullOrWhiteSpace(rateReference) &&
			!string.IsNullOrWhiteSpace(chargeReference) &&
			StringComparer.OrdinalIgnoreCase.Compare(rateReference, chargeReference) == 0
				? 6L
				: NotMatched;

		class FastAutoRateInfo
		{
			public FastAutoRateInfo(AutoRateInfo info, CostSell costOrSell)
			{
				AutoRateInfo = info;
				ContainerNumber = info.Attributes.Attributes
					.FirstOrDefault(x => x.Code == JobChargeAttribTypeList.Codes.ContainerNumber)?.Value ?? string.Empty;
				ContainerCode = info.Attributes.Attributes
					.FirstOrDefault(x => x.Code == JobChargeAttribTypeList.Codes.ContainerCode)?.Value ?? string.Empty;
				CartageLegPK = new ZGuid(
					info.Attributes.Attributes
						.FirstOrDefault(x => x.Code == JobChargeAttribTypeList.Codes.CartageLegPK)?.Value
				);
				JobNumbersReference = info.Attributes.Attributes
					.FirstOrDefault(x => x.Code == JobChargeAttribTypeList.Codes.JobNumbersReference)?.Value ?? string.Empty;
				TransportProviderPK = new ZGuid(
					info.Attributes.Attributes
						.FirstOrDefault(x => x.Code == JobChargeAttribTypeList.Codes.TransportProviderPK)?.Value);

				Location = info.Attributes.Attributes.FirstOrDefault(x => x.Code == JobChargeAttribTypeList.Codes.LocationDesc)?.Value;
				LocationType = info.Attributes.Attributes.FirstOrDefault(x => x.Code == JobChargeAttribTypeList.Codes.LocationType)?.Value;

				Currency = info.Currency;
				JobRef = info.JobRef;
				OperationalJobRef = info.OperationalJobRef;
				SellReferenceNumber = info.SellReferenceNumber;
				ChargePK = info.ChargePK;
				ChargeCodePK = info.ChargeCode.PK;

				Party = costOrSell == CostSell.Cost
					? info.CreditorPK
					: info.DebtorOverridePK;

				ServiceID = info.Attributes.Attributes.FirstOrDefault(x => x.Code == JobChargeAttribTypeList.Codes.ServiceID)?.Value;
			}

			public FastAutoRateInfo(BaseCharge charge)
			{
				Party = charge.JR_OH_CostAccount;
				ChargePK = charge.PK;
				ChargeCodePK = charge.JR_AC;
				Currency = charge.JR_CostCurrency;
				ContainerNumber = charge.JobChargeAttrib_ContainerNumber;
				ContainerCode = charge.JobChargeAttrib_ContainerCode;
				CartageLegPK = charge.JobChargeAttrib_CartageLegPK;
				JobNumbersReference = charge.JobChargeAttrib_JobNumbersReference;
				OperationalJobRef = charge.JR_CostReference;
				SellReferenceNumber = charge.JR_SellReference;
				JobRef = !string.IsNullOrEmpty(charge.JR_Calc_RelatedJobNumber)
					? charge.JR_Calc_RelatedJobNumber
					: !string.IsNullOrEmpty(charge.JR_OrderReference)
						? charge.JR_OrderReference
						: charge.JR_JobNumber;

				TransportProviderPK = new ZGuid(charge.JobChargeAttrib_TransportProviderPK);
				Location = charge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationDesc);
				LocationType = charge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationType);
				ServiceID = charge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.ServiceID);
			}

			public ZGuid Party { get; }
			public ZGuid ChargePK { get; }
			public ZGuid ChargeCodePK { get; }
			public ZGuid CartageLegPK { get; }
			public string Currency { get; }
			public string ContainerNumber { get; }
			public string ContainerCode { get; }
			public string JobRef { get; }
			public string OperationalJobRef { get; }
			public string SellReferenceNumber { get; }
			public string JobNumbersReference { get; }
			public ZGuid TransportProviderPK { get; }
			public AutoRateInfo AutoRateInfo { get; }
			public string Location { get; }
			public string LocationType { get; }
			public string ServiceID { get; }

			public SimilarityKeyImpl SimilarityKey => similarityKey ??= new SimilarityKeyImpl(this);
			SimilarityKeyImpl similarityKey;

			public class SimilarityKeyImpl
			{
				public SimilarityKeyImpl(FastAutoRateInfo info)
				{
					this.info = Argument.NotNull(info, nameof(info));
				}

				readonly FastAutoRateInfo info;

				bool Equals(SimilarityKeyImpl other)
				{
					return info.Party.Equals(other.info.Party) &&
						   info.ChargePK.Equals(other.info.ChargePK) &&
						   info.ChargeCodePK.Equals(other.info.ChargeCodePK) &&
						   info.CartageLegPK.Equals(other.info.CartageLegPK) &&
						   string.Equals(info.Currency, other.info.Currency, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(info.ContainerNumber, other.info.ContainerNumber, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(info.ContainerCode, other.info.ContainerCode, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(info.JobRef, other.info.JobRef, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(info.OperationalJobRef, other.info.OperationalJobRef, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(info.SellReferenceNumber, other.info.SellReferenceNumber, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(info.JobNumbersReference, other.info.JobNumbersReference, StringComparison.InvariantCultureIgnoreCase) &&
						   info.TransportProviderPK.Equals(other.info.TransportProviderPK) &&
						   string.Equals(info.Location, other.info.Location, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(info.LocationType, other.info.LocationType, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(info.ServiceID, other.info.ServiceID, StringComparison.InvariantCultureIgnoreCase);
				}

				public override bool Equals(object obj)
				{
					if (ReferenceEquals(null, obj))
					{
						return false;
					}

					if (ReferenceEquals(this, obj))
					{
						return true;
					}

					if (obj.GetType() != this.GetType())
					{
						return false;
					}

					return Equals((SimilarityKeyImpl)obj);
				}

				public override int GetHashCode()
				{
					unchecked
					{
						var hashCode = info.Party.GetHashCode();
						hashCode = (hashCode * 397) ^ info.ChargePK.GetHashCode();
						hashCode = (hashCode * 397) ^ info.ChargeCodePK.GetHashCode();
						hashCode = (hashCode * 397) ^ (info.ContainerCode != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(info.ContainerCode) : 0);
						hashCode = (hashCode * 397) ^ (info.JobRef != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(info.JobRef) : 0);
						hashCode = (hashCode * 397) ^ (info.OperationalJobRef != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(info.OperationalJobRef) : 0);
						hashCode = (hashCode * 397) ^ (info.SellReferenceNumber != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(info.SellReferenceNumber) : 0);
						hashCode = (hashCode * 397) ^ info.TransportProviderPK.GetHashCode();

						return hashCode;
					}
				}
			}
		}

		class FastCharge
		{
			public FastCharge(BaseCharge charge, CostSell costOrSell)
			{
				PK = charge.PK;
				Charge = charge;
				JobNumber = charge.JR_JobNumber;
				ChargeCodePK = charge.JR_AC;
				CanReAutoRate = charge.CanReautorate(costOrSell);
				CostCurrency = charge.JR_CostCurrency;
				CostReference = charge.JR_CostReference;
				SellReference = charge.JR_SellReference;
				ContainerCode = charge.JobChargeAttrib_ContainerCode;
				OrderReference = charge.JR_OrderReference;
				ContainerNumber = charge.JobChargeAttrib_ContainerNumber;
				CartageLegPK = charge.JobChargeAttrib_CartageLegPK;
				JobNumbersReference = Charge.JobChargeAttrib_JobNumbersReference;
				RelatedJobNumber = charge.JR_Calc_RelatedJobNumber;
				Party = costOrSell == CostSell.Cost
					? charge.JR_OH_CostAccount
					: charge.JR_OH_SellAccount;
				ServiceProviderPK = new ZGuid(charge.JobChargeAttrib_ServiceProviderPK);
				TransportProviderPK = new ZGuid(charge.JobChargeAttrib_TransportProviderPK);
				CostAccount = charge.JR_OH_CostAccount;
				Location = charge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationDesc);
				LocationType = charge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationType);
				ServiceID = charge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.ServiceID);
			}

			public ZGuid Party { get; }
			public ZGuid PK { get; }
			public ZGuid ChargeCodePK { get; }
			public ZGuid CartageLegPK { get; }
			public string JobNumber { get; }
			public string RelatedJobNumber { get; }
			public string CostReference { get; }
			public string SellReference { get; }
			public string OrderReference { get; }
			public string ContainerCode { get; }
			public string ContainerNumber { get; }
			public string CostCurrency { get; }
			public ZBool CanReAutoRate { get; }
			public string JobNumbersReference { get; }
			public ZGuid ServiceProviderPK { get; }
			public ZGuid TransportProviderPK { get; }
			public BaseCharge Charge { get; }
			public ZGuid CostAccount { get; }
			public string Location { get; }
			public string LocationType { get; }
			public string ServiceID { get; }

			public SimilarityKeyImpl SimilarityKey => similarityKey ??= new SimilarityKeyImpl(this);
			SimilarityKeyImpl similarityKey;

			public class SimilarityKeyImpl
			{
				public SimilarityKeyImpl(FastCharge charge)
				{
					this.charge = Argument.NotNull(charge, nameof(charge));
				}

				readonly FastCharge charge;

				bool Equals(SimilarityKeyImpl other)
				{
					if (ReferenceEquals(null, other))
					{
						return false;
					}

					if (ReferenceEquals(this, other))
					{
						return true;
					}

					return charge.Party.Equals(other.charge.Party) &&
						   charge.ChargeCodePK.Equals(other.charge.ChargeCodePK) &&
						   charge.CartageLegPK.Equals(other.charge.CartageLegPK) &&
						   string.Equals(charge.JobNumber, other.charge.JobNumber, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.RelatedJobNumber, other.charge.RelatedJobNumber, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.CostReference, other.charge.CostReference, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.SellReference, other.charge.SellReference, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.OrderReference, other.charge.OrderReference, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.ContainerCode, other.charge.ContainerCode, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.ContainerNumber, other.charge.ContainerNumber, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.CostCurrency, other.charge.CostCurrency, StringComparison.InvariantCultureIgnoreCase) &&
						   charge.CanReAutoRate.Equals(other.charge.CanReAutoRate) &&
						   string.Equals(charge.JobNumbersReference, other.charge.JobNumbersReference, StringComparison.InvariantCultureIgnoreCase) &&
						   charge.TransportProviderPK.Equals(other.charge.TransportProviderPK) &&
						   charge.CostAccount.Equals(other.charge.CostAccount) && string.Equals(charge.Location, other.charge.Location, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.LocationType, other.charge.LocationType, StringComparison.InvariantCultureIgnoreCase) &&
						   string.Equals(charge.ServiceID, other.charge.ServiceID, StringComparison.InvariantCultureIgnoreCase);
				}

				public override bool Equals(object obj)
				{
					if (ReferenceEquals(null, obj))
					{
						return false;
					}

					if (ReferenceEquals(this, obj))
					{
						return true;
					}

					if (obj.GetType() != this.GetType())
					{
						return false;
					}

					return Equals((SimilarityKeyImpl)obj);
				}

				public override int GetHashCode()
				{
					unchecked
					{
						var hashCode = charge.Party.GetHashCode();
						hashCode = (hashCode * 397) ^ charge.ChargeCodePK.GetHashCode();
						hashCode = (hashCode * 397) ^ (charge.JobNumber != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(charge.JobNumber) : 0);
						hashCode = (hashCode * 397) ^ (charge.CostReference != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(charge.CostReference) : 0);
						hashCode = (hashCode * 397) ^ (charge.SellReference != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(charge.SellReference) : 0);
						hashCode = (hashCode * 397) ^ (charge.ContainerCode != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(charge.ContainerCode) : 0);
						hashCode = (hashCode * 397) ^ (charge.ContainerNumber != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(charge.ContainerNumber) : 0);
						hashCode = (hashCode * 397) ^ charge.TransportProviderPK.GetHashCode();
						hashCode = (hashCode * 397) ^ charge.CostAccount.GetHashCode();
						return hashCode;
					}
				}
			}
		}

		class MatchInfo
		{
			public MatchInfo(FastAutoRateInfo rateInfo, FastCharge charge)
			{
				FastRateInfo = rateInfo;
				FastCharge = charge;
			}

			public FastAutoRateInfo FastRateInfo { get; }
			public FastCharge FastCharge { get; }

			public AutoRateInfo RateInfo => FastRateInfo.AutoRateInfo;
			public BaseCharge Charge => FastCharge.Charge;
		}
	}
}
