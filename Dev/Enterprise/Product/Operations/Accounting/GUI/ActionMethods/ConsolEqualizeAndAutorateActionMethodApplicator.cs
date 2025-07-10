using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public class ConsolEqualizeAndAutorateActionMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to translate")]
		public ConsolEqualizeAndAutorateActionMethodApplicator(BusinessObjectFactory factory)
			: base("Auto-Cost with Volume Discount", factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length > 0)
			{
				log.SetSectionProgressMax(targets.Length * 2);

				var starter = new AutoRatingStarter(targets, new EqualizationActionMethodUIInteractor(log));
				starter.ExecuteAutorating(new AutoRateOptions
				(
					autoRateCost: true,
					triggerSource: AutoRateTriggerSource.OperationalActions,
					billingType: BillingType.EqualizeAndRate
				));
			}
		}
	}
}
