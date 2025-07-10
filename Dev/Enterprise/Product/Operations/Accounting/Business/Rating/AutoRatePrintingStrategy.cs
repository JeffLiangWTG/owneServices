namespace Enterprise.Accounting.Business
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Rating.Business;

	internal class AutoRatePrintingStrategy : AutoRateInvoicingStrategy
	{
		public AutoRatePrintingStrategy(IBusiness businessObject, Job job)
			: base(businessObject, job)
		{
		}

		public override AutoRatesAdditionResult AddAutoRates(IAutoRatingGUIInteractor interactor, AutoRateInfoCollection autoRatingResults, CostSell costOrSell, ZString[] adapterIDs, IEnumerable<IAutoRating> adapters = null)
		{
			Job.AutoRatedInfosForJobRevenue = autoRatingResults;

			return new AutoRatesAdditionResult(new List<ChargeWrapper>(), new List<ChargeWrapper>(), 0, new List<AutoRateInfo>(), costOrSell);
		}
	}
}
