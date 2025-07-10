namespace Enterprise.Accounting.GUI
{
	using CargoWise.Common.Testing;
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business;
	using Enterprise.Integration.Accounting;
	using Enterprise.Services.OperationalActions.Support;

	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class AutoRatingActionMethodApplicator : OperationalActionMethodApplicator
	{
		public AutoRatingActionMethodApplicator(BusinessObjectFactory factory, bool autorateCosts = true, bool autorateRevenue = true)
			: base("Auto-Rating", factory)
		{
			this.options = new AutoRateOptions
			(
				autoRateCost: autorateCosts,
				autoRateRevenue: autorateRevenue,
				triggerSource: AutoRateTriggerSource.OperationalActions
			);
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length > 0)
			{
				log.SetSectionProgressMax(targets.Length);
				new AutoRatingStarter(targets, new ActionMethodUIInteractor(log)).ExecuteAutorating(options);
			}
		}

		readonly AutoRateOptions options;
	}
}
