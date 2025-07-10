using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DataProviders
{
	public abstract class ZExpressionEvaluator
	{
		protected ZExpressionEvaluator(string functionString)
		{
			FunctionString = functionString;
		}
		protected readonly ZString FunctionString;

		ZString ReplaceAllPropertyNamesWithValues()
		{
			ZString result = FunctionString;
			while (RegexProvider.InnermostMacrosRegex.IsMatch(result))
			{
				result = RegexProvider.InnermostMacrosRegex.Replace(result, x => ReplacePropertyNameWithValue(x.Value.TrimStart('<').TrimEnd('>')));
			}
			return result;
		}

		string ReplacePropertyNameWithValue(string propertyName)
		{
			object value = GetValueFor(propertyName);
			if (value is ZBool)
			{
				return (ZBool)value ? (NoResString)"true" : (NoResString)"false";
			}
			else if (value is bool)
			{
				return (bool)value ? (NoResString)"true" : (NoResString)"false";
			}
			else if (value is string || value is ZString)
			{
				return value.ToString().EscapeForJScript();
			}
			else if (value == null)
			{
				return string.Empty;
			}
			return value.ToString();
		}

		protected abstract object GetValueFor(string propertyName);

		public bool Evaluate()
		{
			using (Culture.SetTemporarily(Culture.Default))
			{
				var useJs = RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value;
				return ExpressionEvaluator.Evaluate(ReplaceAllPropertyNamesWithValues(), useJs);
			}
		}

		public static bool Evaluate(string expression, IDocumentSupportable docSupportable, params IBODocDataProvider[] providers) =>
			Evaluate(expression, RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value, docSupportable, providers);

		public static bool Evaluate(string expression, bool useJSEngine, IDocumentSupportable docSupportable, params IBODocDataProvider[] providers)
		{
			try
			{
				LastValidDocumentFilterEnumExpressionError = ZString.Empty;

				if (!string.IsNullOrEmpty(expression))
				{
					if (IsInnermostRegexMatch(expression))
					{
						string translated = new TranslateMacroForDocumentFilter(new DataProviderList(providers)).ReplaceMacros(expression);
						return ExpressionEvaluator.Evaluate(translated, useJSEngine);
					}

					string message = string.Empty;
					if (docSupportable != null && IsValidFilterEnumExpression(expression, false, out message))
					{
						var valuePairs = expression.Split('=');

						var filterName = (DocumentFilters)Enum.Parse(typeof(DocumentFilters), valuePairs[0]);
						if (filterName == DocumentFilters.AdditionalMatch)
						{
							return docSupportable.DocumentSupporter.MatchFilterValue(valuePairs[1]);
						}
						else
						{
							string filterValue = docSupportable.DocumentSupporter.GetFilterValue(filterName);
							return filterValue == valuePairs[1];
						}
					}

					LastValidDocumentFilterEnumExpressionError = message;

					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				throw new ExpressionEvaluationException(string.Format("Error while evaluating expression.\r\n docSupportable: {0}\r\n providers: {1}", docSupportable, providers), ex);
			}
		}

		public static string LastValidDocumentFilterEnumExpressionError { get; private set; }

		public static bool IsInnermostRegexMatch(string expression)
		{
			return RegexProvider.InnermostMacrosRegex.IsMatch(expression);
		}

		public static bool IsValidFilter(string filter, bool supportMultipleValues, out string message)
		{
			if (IsInnermostRegexMatch(filter))
			{
				return IsValidFilterExpression(filter, out message);
			}
			else
			{
				return IsValidFilterEnumExpression(filter, supportMultipleValues, out message);
			}
		}

		static bool IsValidFilterEnumExpression(string filter, bool supportMultipleValues, out string message)
		{
			return IsValidFilterEnumExpression(filter, typeof(DocumentFilters), supportMultipleValues, out message);
		}

		public static bool IsValidTemplateFilterName(string filter, bool supportMultipleVaules, out string message)
		{
			if (IsInnermostRegexMatch(filter))
			{
				return IsValidFilterExpression(filter, out message);
			}
			else
			{
				return IsValidFilterEnumExpression(filter, typeof(MenuTemplateFilterType), supportMultipleVaules, out message);
			}
		}

		public static bool IsValidOtherDocumentOrEDocsFilterName(string filter, out string message)
		{
			if (!IsInnermostRegexMatch(filter))
			{
				message = Res.GetString("e9964c67-dcef-43fe-8a35-3e501d1b341d", "Filter must contain macros");
				return false;
			}
			return IsValidFilterExpression(filter, out message);
		}

		static bool IsValidFilterExpression(string filter, out string message)
		{
			try
			{
				var filterRepalced = RegexProvider.OutermostMacroRegex.Replace(filter, "");
				if (ExpressionEvaluator.IsValidLogicalExpression(filterRepalced))
				{
					message = string.Empty;
					return true;
				}
				else
				{
					message = Res.GetString("95f5f449-fc15-4a86-8b4f-851a4995ac5f", "The following expression {0} is incorrect. Please make sure:\r\n\u2022 you are using a True/False expression\r\n\u2022 you are not mixing legacy filters(e.g.CTY = AU) with other filters(e.g. \"<PropertyName>\" == \"My value\")\r\n\u2022 if you use a legacy filter, they cannot be combined.", filter);
					return false;
				}
			}
			catch (ExpressionEvaluationException)
			{
				message = Res.GetString("46cdedf5-5411-4227-94cb-f97b71c84e96", "Filter format is not a True/False expression. It should have a format similar to:\r\n\"<PropertyName>\" == \"My value\" && \"<OtherProperty>\" == \"Another value\"");
				return false;
			}
		}

		static bool IsValidFilterEnumExpression(string filter, Type enumType, bool supportMultipleValues, out string message)
		{
			var valuePairs = filter.Split('=');
			if (valuePairs.Length < 2 || valuePairs.Length > 2 && !supportMultipleValues)
			{
				message = Res.GetString("49a102c7-c251-4447-952f-94e5ba079167", "Filter format is incorrect, you must enter a Code, followed by the '=' sign, and followed by the expected value. e.g. MOD=SEA");
				return false;
			}

			var filterTypeString = valuePairs[0];
			if (filterTypeString.EndsWith("!", StringComparison.CurrentCulture))
			{
				filterTypeString = filterTypeString.TrimEnd('!');
			}

			if (!Enum.IsDefined(enumType, filterTypeString))
			{
				var validCodes = string.Join(", ", Enum.GetNames(enumType));
				message = Res.GetString("7fdbe0ff-b530-4a00-a32b-2c37182314aa", "'{0}' code is incorrect. The valid codes are: {1}", valuePairs[0], validCodes);
				return false;
			}

			message = string.Empty;
			return true;
		}
	}
}
