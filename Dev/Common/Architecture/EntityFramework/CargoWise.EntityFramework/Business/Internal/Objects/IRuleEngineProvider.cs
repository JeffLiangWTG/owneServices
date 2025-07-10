using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework.Business.Rules;
using Enterprise.ZArchitecture.Core;
using WTG.Rules.Engine;
using WTG.Rules.Engine.Impl;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	public interface IRuleEngineProvider
	{
		IRuleEngine GetRuleEngine();
	}

	class DefaultRuleEngineProvider : IRuleEngineProvider
	{
		static string RulesPath => Path.Combine(AssemblyLoader.GetBinPath(), (NoResString)"Rules");

		public IRuleEngine GetRuleEngine()
		{
			return globalRuleEngine.Value;
		}

		#region Global rule engine

		[ThreadSafe] // Both Lazy and RuleEngine are designed with concurrency in mind
		static readonly Lazy<IRuleEngine> globalRuleEngine = new Lazy<IRuleEngine>(CreateRuleEngine);

		static IRuleEngine CreateRuleEngine()
		{
			var config = new YamlConfigurationProvider(RulesPath);
			var provider = new ConfigurationBasedRulesProvider(config);
			var cache = new RulesProviderCache(provider);
			var mapper = new BusinessObjectRuleMapperAdapter();

			return new RuleEngine(new BusinessObjectRuleFactory(mapper), cache);
		}

		#endregion
	}
}
