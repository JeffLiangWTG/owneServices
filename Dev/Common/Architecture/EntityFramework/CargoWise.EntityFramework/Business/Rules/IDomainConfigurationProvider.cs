using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WTG.Rules.Configuration;

namespace CargoWise.EntityFramework.Business.Rules
{
	public interface IDomainConfigurationProvider : IRuleConfigurationAdapter
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Task<IReadOnlyDictionary<string, RuleConfiguration>> GetRuleConfiguration();
	}
}
