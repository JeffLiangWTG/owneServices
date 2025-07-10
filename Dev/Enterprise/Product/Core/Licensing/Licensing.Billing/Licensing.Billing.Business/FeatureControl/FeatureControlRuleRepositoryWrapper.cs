using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.Licensing.Billing.Business
{
	public class FeatureControlRuleRepositoryWrapper : IFeatureControlRuleRepository
	{
		public FeatureControlRuleRepositoryWrapper()
		{
			FeatureControlRuleRepository = new FeatureControlRuleRepository(ObjectFactory.Get<IWritableFeatureControlStorage>(), new ErrorReporterLogger<FeatureControlRuleRepository>(nameof(CargoWise.FeatureControl.FeatureControlRuleRepository)));
		}

		public IFeatureControlRuleRepository FeatureControlRuleRepository { get; }

		public Task<DateTime> GetFeatureControlTimestampUtcAsync(CancellationToken cancellationToken)
		{
			return FeatureControlRuleRepository.GetFeatureControlTimestampUtcAsync(cancellationToken);
		}

		public Task<bool> SaveFeatureControlRuleContentAsync(byte[] ruleContent, CancellationToken cancellationToken)
		{
			return FeatureControlRuleRepository.SaveFeatureControlRuleContentAsync(ruleContent, cancellationToken);
		}
	}
}
