using System.Collections.Generic;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RatingEnums;

namespace Enterprise.DocumentWrappers
{
	internal static class RateEntryExtensions
	{
		internal static List<RateLine> RateLinesIncludingRelated(this RateEntry rateEntry, PricingPage pricingPage)
		{
			var result = new List<RateLine>();
			var rateLineLoader = new PricingPageRateLineFactory(EntryTypes.Freight);
			foreach (var lineSet in rateLineLoader.LoadLineSets(pricingPage, new[] { rateEntry }))
			{
				result.AddRange(lineSet);
			}

			return result;
		}

		internal static bool HasVisibleLines(this RateEntry rateEntry, PricingPage pricingPage)
		{
			foreach (var line in rateEntry.RateLinesIncludingRelated(pricingPage))
			{
				if (line.IsVisibleOnDocuments())
				{
					return true;
				}
			}

			return false;
		}
	}
}
