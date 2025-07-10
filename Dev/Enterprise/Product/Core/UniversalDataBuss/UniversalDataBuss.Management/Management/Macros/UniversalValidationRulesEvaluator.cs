using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	public class UniversalValidationRulesEvaluator : IUniversalValidationRulesEvaluator
	{
		public void EvaluateRules(IDataObject dataObject, BusinessObject businessObject, IXmlImportLogger logger)
		{
			if (!TryGetValidationRules(businessObject.Factory, dataObject, businessObject, out var rules))
			{
				return;
			}

			var hasError = false;
			var failingRules = new List<ValidationRule>();

			foreach (var ruleSet in rules)
			{
				if (!IsRuleSetCriteriaMet(ruleSet, dataObject, logger))
				{
					continue;
				}

				foreach (var rule in ruleSet.ActiveRules)
				{
					if (!IsValidationRuleMet(ruleSet, rule, dataObject, businessObject, logger, out var validationRuleResult))
					{
						hasError = hasError || rule.IsError;
						failingRules.Add(validationRuleResult);
					}
				}
			}

			AddFailingValidationRuleResults(logger, failingRules);

			if (hasError)
			{
				throw new DataObjectReadFailureException(ZString.Empty);
			}
		}

		static bool TryGetValidationRules(BusinessObjectFactory factory, IDataObject dataObject, BusinessObject targetBizo, out IEnumerable<IReadOnlyUniversalValidationRuleSet> rules)
		{
			rules = null;
			if (dataObject is IValidationRuleCollectionParent ruleCollectionParent && ruleCollectionParent.ValidationRuleCollection != null)
			{
				var dataContext = targetBizo.GetUniversalDataContextManager().DataContextType.ToString();
				var ruleStore = ObjectFactory.Get<IUniversalValidationRuleStore>();
				if (ruleCollectionParent.ValidationRuleCollection.Any())
				{
					rules = ruleStore.GetActiveRulesByCode(factory, dataContext, ruleCollectionParent.ValidationRuleCollection.Select(r => (string)r.Code));
				}
				else
				{
					rules = ruleStore.GetActiveRulesByDataContext(factory, dataContext);
				}
			}

			return rules != null && rules.Any();
		}

		static bool IsRuleSetCriteriaMet(IReadOnlyUniversalValidationRuleSet ruleSet, IDataObject dataObject, IXmlImportLogger logger)
		{
			if (ruleSet.Criteria.IsEmpty)
			{
				return true;
			}

			var scope = new MacroScope();
			scope.SetVariable("UXML", dataObject);
			using (scope)
			{
				if (!TryEvaluateBooleanMacro(ruleSet.Criteria, scope, ruleSet.Code, "", logger, out var macroResult))
				{
					return false;
				}
				return macroResult;
			}
		}

		static bool IsValidationRuleMet(IReadOnlyUniversalValidationRuleSet ruleSet, IReadOnlyUniversalValidationRule rule, IDataObject dataObject, BusinessObject dataTarget, IXmlImportLogger logger, out ValidationRule result)
		{
			result = null;

			var scope = new MacroScope(dataTarget);
			scope.SetVariable("UXML", dataObject);
			using (scope)
			{
				if (!TryEvaluateBooleanMacro(rule.BusinessRule, scope, ruleSet.Code, rule.Sequence.ToString(), logger, out var macroResult))
				{
					// don't reject message if user has written incorrect macro
					return true;
				}

				if (!macroResult)
				{
					result = new ValidationRule
					{
						Code = ruleSet.Code,
						MessageLog = EvaluateMessageLogWithMacro(ruleSet, rule, scope, logger),
						Sequence = rule.Sequence,
						Result = rule.IsError ? ValidationLevels.Error : ValidationLevels.Warning
					};
					logger.Log(rule.IsError ? Enterprise.Integration.LogType.Error : Enterprise.Integration.LogType.Warning, $"Validation Rule {ruleSet.Code}, Sequence {rule.Sequence} is not met: Macro: {rule.BusinessRule}");
					return false;
				}

				return true;
			}
		}

		static string EvaluateMessageLogWithMacro(IReadOnlyUniversalValidationRuleSet ruleSet, IReadOnlyUniversalValidationRule rule, IMacroScope scope, IXmlImportLogger logger)
		{
			TryEvaluateMacro(rule.MessageLog, scope, ruleSet.Code, rule.Sequence.ToString(), null, out var result);
			return result as string ?? rule.MessageLog;
		}

		static bool TryEvaluateBooleanMacro(string macroText, IMacroScope scope, string ruleCode, string ruleSequence, IXmlImportLogger logger, out bool result)
		{
			result = false;
			if (!TryEvaluateMacro(macroText, scope, ruleCode, ruleSequence, logger, out var macroResult))
			{
				return false;
			}

			if (macroResult is bool booeleanResult)
			{
				result = booeleanResult;
				return true;
			}

			var sequenceLog = string.IsNullOrEmpty(ruleSequence) ? string.Empty : $", Sequence {ruleSequence}";
			var message = $"Rule {ruleCode}{sequenceLog}: Expected true/false value [{macroText}], Result: {macroResult}";
			logger.Log(Enterprise.Integration.LogType.Warning, message);
			return false;
		}

		static bool TryEvaluateMacro(string macroText, IMacroScope scope, string ruleCode, string ruleSequence, IXmlImportLogger logger, out object result)
		{
			var macro = UniversalValidationRuleMacrosCache.Instance.GetMacro(macroText);
			result = ObjectFactory.Get<IMacroEvaluator>().EvaluateMacroValue(scope, macro);

			if (macro.HasErrors())
			{
				var errorMessages = string.Join(",", macro.Errors.Select(e => e.Message));
				logger?.Log(Enterprise.Integration.LogType.Warning, $"Rule {ruleCode}, Sequence {ruleSequence}: Cannot evaluate macro [{macroText}], Error: {errorMessages}");
				return false;
			}
			return true;
		}

		static void AddFailingValidationRuleResults(IXmlImportLogger logger, List<ValidationRule> results)
		{
			if (results.Count == 0)
			{
				return;
			}

			if (logger.ValidationRuleCollection == null)
			{
				logger.ValidationRuleCollection = results;
				return;
			}

			logger.ValidationRuleCollection = logger.ValidationRuleCollection.Concat(results);
		}

		public static class ValidationLevels
		{
			public const string Error = "ERROR";
			public const string Warning = "WARNING";
		}
	}
}
