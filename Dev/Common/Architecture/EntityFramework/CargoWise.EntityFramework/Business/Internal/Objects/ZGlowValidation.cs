using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using WTG.Rules;
using WTG.Rules.Engine;
using RuleNotificationType = WTG.Rules.NotificationType;

namespace CargoWise.EntityFramework
{
	public class ZGlowValidation
	{
		readonly Type type;
		readonly IImmutableDictionary<string, IEnumerable<IRule>> rules;
		readonly Dictionary<string, IEnumerable<Delegate>> delegatesCache = new Dictionary<string, IEnumerable<Delegate>>();

		public ZGlowValidation(Type t)
			: this(t, ObjectFactory.Get<IRuleEngineProvider>())
		{
		}

		internal ZGlowValidation(Type t, IRuleEngineProvider provider)
		{
			type = t;
			rules = GetRules(provider.GetRuleEngine());
		}

		public void Validate(ZPropertyInfo propertyInfo)
		{
			foreach (var runner in GetRunners(propertyInfo))
			{
				var computation = (RuleComputation<RuleValidationResult>)runner.DynamicInvoke(propertyInfo.BizObj);
				var result = computation.GetValue();

				if (result == null)
				{
					continue;
				}

				if (result is RuleValidationResult<string> strResult)
				{
					switch (result.Level)
					{
						case RuleNotificationType.Success:
							break;

						case RuleNotificationType.Error:
							propertyInfo.AddError(strResult.Text);
							break;
						case RuleNotificationType.Warning:
							propertyInfo.AddWarning(strResult.Text);
							break;
						default:
							ErrorReporter.ReportOnce("CW1 only supports Error and Warning. Property: " + propertyInfo.Name);
							break;
					}
				}
				else
				{
					ErrorReporter.ReportOnce("CW1 currently only supports string based messages: " + propertyInfo.Name);
				}
			}
		}

		IEnumerable<Delegate> GetRunners(ZPropertyInfo propertyInfo)
		{
			IEnumerable<Delegate> result;
			if (!delegatesCache.TryGetValue(propertyInfo.Name, out result))
			{
				IEnumerable<IRule> rulesForProperty;
				if (rules.TryGetValue(propertyInfo.Name, out rulesForProperty))
				{
					result = rulesForProperty.Select(rule => rule.GetRunner(type, _ => propertyInfo.BizObj)).ToImmutableArray();
				}
				else
				{
					result = Enumerable.Empty<Delegate>();
				}

				delegatesCache.Add(propertyInfo.Name, result);
			}

			return result;
		}

		IImmutableDictionary<string, IEnumerable<IRule>> GetRules(IRuleEngine ruleEngine)
		{
			var grouped = GetRulesCore(ruleEngine)
				.Where(rule => rule.RuleType == RuleType.Validation)
				.GroupBy(rule => rule.PropertyOrCommandName);

			var builder = ImmutableDictionary.CreateBuilder<string, IEnumerable<IRule>>();
			foreach (var property in grouped)
			{
				builder.Add(property.Key, ImmutableList.CreateRange(property));
			}

			return builder.ToImmutable();
		}

		IEnumerable<IRule> GetRulesCore(IRuleEngine ruleEngine)
		{
			var task = ruleEngine.GetRulesAsync(type);
			task.Wait();

			return ImmutableArray.CreateRange(task.Result);
		}
	}
}
