namespace Enterprise.Accounting.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;

	internal class AutoRateHLSPrintingStrategy : AutoRatePrintingStrategy
	{
		public AutoRateHLSPrintingStrategy(IBusiness businessObject, Job job)
			: base(businessObject, job)
		{
		}

		public override AutoRatesAdditionResult AddAutoRates(IAutoRatingGUIInteractor interactor, AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ZString[] adapterIDs, IEnumerable<IAutoRating> adapters = null)
		{
			Job.AutoRatedInfosForJobRevenue = autoRatingResults;
			Job.AutoRatedInfoGroupByChargeForJobRevenue = GetMergedAutoRateInfosResults(autoRatingResults);

			return new AutoRatesAdditionResult(new List<ChargeWrapper>(), new List<ChargeWrapper>(), 0, new List<AutoRateInfo>(), costOrSell);
		}

		AutoRateInfoCollection GetMergedAutoRateInfosResults(AutoRateInfoCollection autoRatingResults)
		{
			var summarizer = new RateResultsSummarizer(Job.Factory);
			var summarized = summarizer.GetMergedAutoRateInfos(autoRatingResults.ToArray().Cast<AutoRateInfo>());

			var result = new AutoRateInfoCollection(autoRatingResults.Factory);
			result.AddRange(summarized);

			return result;
		}
	}
}
