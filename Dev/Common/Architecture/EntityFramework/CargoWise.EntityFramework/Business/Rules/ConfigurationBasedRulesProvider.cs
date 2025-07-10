using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WTG.Rules.Configuration;
using WTG.Rules.Engine.Impl;

namespace CargoWise.EntityFramework.Business.Rules
{
	public sealed class ConfigurationBasedRulesProvider : IRulesProvider
	{
		readonly IDomainConfigurationProvider provider;

		public ConfigurationBasedRulesProvider(IDomainConfigurationProvider provider)
		{
			this.provider = provider;
		}

		public async Task<Func<IEnumerable<Type>, IEnumerable<IRuleInfo>>> GetRuleInfoAsync()
		{
			var configs = await provider.GetRuleConfiguration();
			return types => types.SelectMany(t => configs.GetRules(t, provider, _ => throw new NotSupportedException()));
		}
	}
}
