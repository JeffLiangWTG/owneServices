using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework.Business.Rules;
using WTG.Rules.Engine;
using WTG.Rules.Engine.Impl;

namespace CargoWise.EntityFramework.Testing
{
	internal class DummyRulesProvider : IRulesProvider
	{
		readonly Dictionary<Type, List<RuleInfo>> rules = new Dictionary<Type, List<RuleInfo>>();

		public DummyRulesProvider AddRule(Type bizoType, string propertyName, string validationMethodName, params object[] args)
		{
			Argument.NotNull(bizoType, nameof(bizoType));
			Argument.NotNull(propertyName, nameof(propertyName));
			Argument.NotNull(validationMethodName, nameof(validationMethodName));

			if (bizoType.GetProperty(propertyName) == null)
			{
				throw new ArgumentException($"{propertyName} does not exist for ${bizoType.FullName}");
			}

			var rule = AsRuleInfo(propertyName, validationMethodName, args);

			List<RuleInfo> rulesOfType;
			if (rules.TryGetValue(bizoType, out rulesOfType))
			{
				rulesOfType.Add(rule);
			}
			else
			{
				rules.Add(bizoType, new List<RuleInfo> { rule });
			}

			return this;
		}

		static RuleInfo AsRuleInfo(string propertyName, string validationMethodName, params object[] args)
		{
			var validationMethod = typeof(TestValidationMethods).GetMethod(validationMethodName);
			var methodParams = validationMethod.GetParameters();
			if (methodParams.Length != args.Length)
			{
				throw new ArgumentException("You must provide a value for every parameter");
			}

			var dependancies = new List<DependencyAndPath>();
			var constants = new List<ConstantAndValue>();
			for (var i = 0; i < methodParams.Length; i++)
			{
				var asString = args[i] as string;
				if (asString != null && asString.StartsWith("<") && asString.EndsWith(">"))
				{
					dependancies.Add(new DependencyAndPath(methodParams[i].Name, asString.Substring(1, asString.Length - 2)));
				}
				else
				{
					constants.Add(new ConstantAndValue(methodParams[i].Name, args[i]));
				}
			}

			return new RuleInfo(
				Guid.NewGuid(),
				propertyName,
				validationMethod,
				dependancies,
				constants);
		}

		IEnumerable<RuleInfo> GetRulesForTypes(IEnumerable<Type> types)
		{
			return types.SelectMany(t => rules.TryGetValue(t, out var rulesForType) ? rulesForType : Enumerable.Empty<RuleInfo>());
		}

		public Task<Func<IEnumerable<Type>, IEnumerable<IRuleInfo>>> GetRuleInfoAsync()
		{
			return Task.FromResult<Func<IEnumerable<Type>, IEnumerable<IRuleInfo>>>(GetRulesForTypes);
		}

		public IRuleEngine ToEngine()
		{
			var cache = new RulesProviderCache(this);
			var mapper = new BusinessObjectRuleMapperAdapter();

			return new RuleEngine(new BusinessObjectRuleFactory(mapper), cache);
		}

		public IRuleEngineProvider ToProvider()
		{
			return new EngineProvider(ToEngine());
		}

		class EngineProvider : IRuleEngineProvider
		{
			readonly IRuleEngine engine;

			public EngineProvider(IRuleEngine engine)
			{
				this.engine = engine;
			}

			public IRuleEngine GetRuleEngine()
			{
				return engine;
			}
		}
	}
}
