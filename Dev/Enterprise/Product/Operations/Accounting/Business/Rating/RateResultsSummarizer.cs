namespace Enterprise.Accounting.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;

	class RateResultsSummarizer
	{
		public RateResultsSummarizer(BusinessObjectFactory factory, bool setProviderForMergedRevenue = false)
		{
			this.factory = factory;
			this.setProviderForMergedRevenue = setProviderForMergedRevenue;
		}

		readonly BusinessObjectFactory factory;
		readonly bool setProviderForMergedRevenue;

		/// <summary>
		/// Merges AutoRateInfos where their adapter has MergeChargeOptions.CrossAdapter as the merge option.
		/// For other adapter merge types, these infos are left unmerged.
		/// </summary>
		internal List<AutoRateInfo> MergeInfosCrossAdapter(Dictionary<IAutoRating, AutoRateResult> resultsByAdapter)
		{
			var results = new List<AutoRateInfo>();
			var possiblyMergeableResults = new List<AutoRateInfo>();

			foreach (var pair in resultsByAdapter)
			{
				if (pair.Key.MergeCharges == MergeChargeOptions.CrossAdapter)
				{
					possiblyMergeableResults.AddRange(pair.Value.RateInfoCollection.Cast<AutoRateInfo>());
				}
				else
				{
					results.AddRange(pair.Value.RateInfoCollection.Cast<AutoRateInfo>());
				}
			}

			results.AddRange(GetMergedAutoRateInfos(possiblyMergeableResults));

			return results;
		}

		internal List<AutoRateInfo> GetMergedAutoRateInfos(IEnumerable<AutoRateInfo> possibleMergeableResults)
		{
			var mergeableResults = possibleMergeableResults.ToKeyListDictionary(x => new AutoRateInfoKey(x));

			var results = new List<AutoRateInfo>();

			foreach (var pair in mergeableResults)
			{
				results.Add(GetMergedAutoRateInfo(pair.Value, pair.Key.ChargeCode));
			}

			return results;
		}

		#region Merging Infos

		AutoRateInfo GetMergedAutoRateInfo(IEnumerable<AutoRateInfo> infosToMerge, AccChargeCode chargeCode)
		{
			var mergedInfo = new AutoRateInfo(factory);
			mergedInfo.ChargeCode = chargeCode;

			var calculationDescription = "";

			var infosByAutoRatedFor = infosToMerge.ToKeyListDictionary(x => x.AutoRatedForString);

			var infosSummarisedDescription = new ZStringBuilder();
			infosSummarisedDescription.Append(chargeCode != null ? chargeCode.AC_Code : ZString.Empty);

			foreach (var pair in infosByAutoRatedFor)
			{
				infosSummarisedDescription.Append(pair.Key);

				foreach (var info in pair.Value.OrderByDescending(x => x.Amount))
				{
					SetCurrencyIfEmpty(mergedInfo, info);
					SetProviderIfEmpty(mergedInfo, info, setProviderForMergedRevenue);

					mergedInfo.MergePaymentBasesFromAutoRateInfo(info);
					mergedInfo.DebtorOverridePK = info.DebtorOverridePK;
					calculationDescription += info.CalculationDescription + Environment.NewLine;

					infosSummarisedDescription.Append(string.Format("\t{0} {1}\t\t{2}", info.Amount.ToString("f2"), info.Currency, info.CalculationDescription).TrimEnd());

					// An example for calling this method from MergeInfosCrossAdapter is auto cost for a RunSheet with 2 legs.
					// Usually the results are the same for the legs and RateInfos are merged.
					// ALL attributes from the first info should be add to mergedInfo.
					// Later when we create a new cost/revenue charge we can have all the attributes from the info to save with the charge.
					// More later we use the attributes to merge charges from different autorating sessions.
					if (!mergedInfo.Attributes.Attributes.Any())
					{
						var sourceSet = info.Attributes;
						foreach (var attribute in sourceSet.Attributes)
						{
							mergedInfo.Attributes.AddOrReplace(sourceSet, attribute.Code);
						}
					}
					else
					{
						mergedInfo.AddMergedInfo(info);
					}
					mergedInfo.AppendUniqueInvoiceLineDescriptions(info);
				}
			}

			mergedInfo.CalculationDescription = calculationDescription.TrimEnd();

			var sourceSummaries = GetSourceSummaries(infosToMerge);
			var descriptionsForInfos = infosSummarisedDescription.ToStringWithNewLineBetweenAppends();
			mergedInfo.Description = GetSummarisedCalculationDescription(descriptionsForInfos, sourceSummaries);

			return mergedInfo;
		}

		static void SetCurrencyIfEmpty(AutoRateInfo newInfo, AutoRateInfo infoToAdd)
		{
			if (newInfo.Currency.IsEmpty && !infoToAdd.Currency.IsEmpty)
			{
				newInfo.Currency = infoToAdd.Currency;
			}
		}

		static void SetProviderIfEmpty(AutoRateInfo newInfo, AutoRateInfo infoToAdd, bool setProviderForMergedRevenue)
		{
			if (newInfo.ProviderPK.IsEmpty && (infoToAdd.IsCost || setProviderForMergedRevenue))
			{
				newInfo.ProviderPK = infoToAdd.ProviderPK;
			}
		}

		#region Get Calculation Descriptions

		static ZString GetSourceSummaries(IEnumerable<AutoRateInfo> infos)
		{
			var uniqueDescriptions = infos.Select(i => i.RateDescription).Distinct().ToArray();
			return new ZStringBuilder(uniqueDescriptions).ToStringWithDelimiterBetweenAppends(Environment.NewLine + Environment.NewLine);
		}

		static ZString GetSummarisedCalculationDescription(ZString descriptionsForInfo, ZString sourceDescriptions)
		{
			var result = new ZStringBuilder();

			result.AppendIfNotEmpty(descriptionsForInfo);

			if (!string.IsNullOrWhiteSpace(sourceDescriptions))
			{
				result.AppendLine();
				result.AppendLine();
				result.AppendLine(Res.GetString("816572b4-96a1-4b62-9c93-659d2931fa54", "Calculated from:"));
				result.Append(sourceDescriptions);
			}

			result.AppendLine(Res.GetString("21df310d-21c4-4b9e-bd3b-0ab51280d1de", @"User:		{0}", GlbStaff.CurrentUser.GS_FullName));
			result.AppendLine(Res.GetString("3d34ee42-30e2-48ee-94ab-d11b70aa336a", @"Time:		{0}", ZDateTime.Now));

			return result.ToString();
		}

		#endregion

		#endregion
	}
}
