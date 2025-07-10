using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business
{
	public interface IAutoRatingStrategy
	{
		IBusiness HostBusinessEntity { get; }
		Job Job { get; }
		AutoRatesAdditionResult AddAutoRates(IAutoRatingGUIInteractor interactor, AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ZString[] adapterIDs, IEnumerable<IAutoRating> adapters = null);
		bool ShouldAddAutoRates { get; }
		bool Supports(CostSell costOrSell);
		RatingAdaptersProvider ParentProvider { get; }
	}

	public static class AutoRatingStrategyExtensions
	{
		public static RatingAdaptersProvider AdaptersProvider(this IAutoRatingStrategy strategy)
		{
			var ratingSupporter = ServiceLocator.GetService<IRatingSupporter>(strategy.HostBusinessEntity);
			return ratingSupporter?.AdaptersProvider;
		}

		public static int GetRatingAdaptersCount(this IAutoRatingStrategy strategy, AutoRateOptions options)
		{
			return strategy.AdaptersProvider()?.AdaptersCount(options) ?? 0;
		}
	}
}
