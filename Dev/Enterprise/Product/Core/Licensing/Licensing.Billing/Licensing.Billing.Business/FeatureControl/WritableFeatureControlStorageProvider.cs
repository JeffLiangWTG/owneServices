using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Licensing.Billing.Business
{
	public class WritableFeatureControlStorageProvider : IWritableFeatureControlStorage
	{
		public Task<string> GetDataAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(RuleContentRegistryItem.Value);
		}

		public Task SetDataAsync(string data, CancellationToken cancellationToken)
		{
			RuleContentRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, data);
			return Task.CompletedTask;
		}

		StringRegistryItem RuleContentRegistryItem => WebDataRegistry.Instance.FeatureControlRuleContent;
	}
}
