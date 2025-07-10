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

	internal class AutoRateHLSStrategy : AutoRateInvoicingStrategy
	{
		public AutoRateHLSStrategy(IBusiness businessObject, Job job)
			: base(businessObject, job)
		{
		}

		public override AutoRatesAdditionResult AddAutoRates(IAutoRatingGUIInteractor interactor, AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ZString[] adapterIDs, IEnumerable<IAutoRating> adapters = null)
		{
			return base.AddAutoRates(interactor, GetMergedAutoRateInfosResults(autoRatingResults), costOrSell, adapterIDs, adapters);
		}

		AutoRateInfoCollection GetMergedAutoRateInfosResults(AutoRateInfoCollection autoRatingResults)
		{
			var summarizer = new RateResultsSummarizer(Job.Factory, setProviderForMergedRevenue: true);
			var summarized = summarizer.GetMergedAutoRateInfos(autoRatingResults.Cast<AutoRateInfo>());

			autoRatingResults.RemoveAll();
			autoRatingResults.AddRange(summarized);

			return autoRatingResults;
		}
	}
}
