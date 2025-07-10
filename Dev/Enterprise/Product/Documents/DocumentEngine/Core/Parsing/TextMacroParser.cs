using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.DocumentEngine
{
	internal class TextMacroParser
	{
		public IEnumerable<ITextMacroExpression> ParseMacro(string macro, IEnumerable<Type> businessObjectTypes)
		{
			var objects = CreateBusinessObjectIntances(businessObjectTypes);
			return ParseMacro(macro, objects);
		}

		IEnumerable<BusinessObject> CreateBusinessObjectIntances(IEnumerable<Type> businessObjectTypes)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			return businessObjectTypes.Select(factory.New);
		}

		public IEnumerable<ITextMacroExpression> ParseMacro(string macro, IEnumerable<IBusiness> businessObjects)
		{
			var bizos = businessObjects.Cast<BusinessObject>().ToArray();
			var dataProviders = BODocDataProvider.GetArray(bizos);
			var dataProviderList = new DataProviderList(dataProviders);
			var dataProvider = DataProviderFactory.GetBusinessObjectDataProvider(dataProviderList);
			return ParseMacro(macro, dataProvider, businessObjects.Select(b => b.GetType()));
		}

		ProviderCache ProviderCache { get; } = new ValueProviderCollector().ValueProviders;

		List<ITextMacroExpression> ParseMacro(string macro, BusinessObjectDataProvider dataProvider, IEnumerable<Type> businessObjectTypes)
		{
			var lastMatchIndex = 0;
			var result = new List<ITextMacroExpression>();
			macro = ReplaceBraces(macro);
			foreach (Match match in RegexProvider.OutermostMacroRegex.Matches(macro))
			{
				var valueProvider = GetValueProvider(match.Value, dataProvider);

				var innerMacro = match.Value;
				var innerStart = match.Index;
				var innerEnd = match.Index + match.Length;
				if (innerMacro.StartsWith("<") && innerMacro.EndsWith(">"))
				{
					if (match.Index > 0 && (macro[match.Index - 1] == '"' || macro[match.Index - 1] == '\''))
					{
						innerStart--;
					}
					if (innerEnd < macro.Length && (macro[innerEnd] == '"' || macro[innerEnd] == '\''))
					{
						innerEnd++;
					}
					innerMacro = innerMacro.Substring(1, innerMacro.Length - 2).Trim();
				}
				var str = macro.Substring(lastMatchIndex, innerStart - lastMatchIndex);
				if (!string.IsNullOrWhiteSpace(str))
				{
					result.Add(new TextMacroExpression
					{
						TextExpression = str,
						ExpressionType = MacroExpressionType.Unknown
					});
				}

				if (valueProvider != null)
				{
					var methods = dataProvider.FindColumn(innerMacro);
					if (methods == null)
					{
						result.Add(new TextMacroExpression
						{
							DataSourceType = null,
							ValueProvider = valueProvider,
							ExpressionType = MacroExpressionType.ValueProvider,
							Expressions = GetValueProviderInnerExpressions(valueProvider, innerMacro, businessObjectTypes)
						});
					}
					else
					{
						Type sourceType = null;
						foreach (var m in methods)
						{
							var expr = GenerateParseResult(sourceType, m, valueProvider);
							result.Add(expr);
							sourceType = expr.ReturnType;
						}
					}
				}
				else
				{
					throw new InvalidMacroException($"Invalid macro: {innerMacro}");
				}

				lastMatchIndex = innerEnd;
			}
			if (lastMatchIndex < macro.Length)
			{
				result.Add(new TextMacroExpression
				{
					TextExpression = macro.Substring(lastMatchIndex),
					ExpressionType = MacroExpressionType.Unknown,
				});
			}
			return result;
		}

		List<ITextMacroExpression> GetValueProviderInnerExpressions(ITextMacroValueProvider valueProvider, string innerMacro, IEnumerable<Type> businessObjectTypes)
		{
			var match = Regex.Match(innerMacro, @"^\s*\w*\s*[(](?<inner>.+)[)]\s*$");
			if (match.Success)
			{
				var inner = match.Groups["inner"].Value;
				var parts = SplitRespctingQuotesAndAngleBrackets(inner);
				return parts.Select(p =>
				{
					var exrs = ParseMacro(p, businessObjectTypes).ToList();
					if (exrs.Count == 1)
					{
						return exrs[0];
					}
					return new TextMacroExpression
					{
						Expressions = exrs,
						ExpressionType = MacroExpressionType.Unknown
					};
				}).ToList();
			}
			return null;
		}

		TextMacroExpression GenerateParseResult(Type sourceType, MethodInfoChainLink m, ITextMacroValueProvider valueProvider)
		{
			List<ITextMacroExpression> parameters = null;
			if (m.Parameters != null)
			{
				parameters = m.Parameters.Select(p =>
				{
					var inner = TryParseInnerExpressions(m.TypeToReflect, p).ToList();
					if (inner == null)
					{
						return new TextMacroExpression
						{
							TextExpression = p.ToString(),
							ExpressionType = MacroExpressionType.Unknown
						};
					}
					else if (inner.Count == 1)
					{
						return inner[0];
					}
					else
					{
						return new TextMacroExpression
						{
							Expressions = inner,
							ExpressionType = MacroExpressionType.Unknown
						};
					}
				}).ToList();
			}
			else if (m.Index != null)
			{
				parameters = new List<ITextMacroExpression>
					{
						new TextMacroExpression
						{
							TextExpression = m.Index,
							ExpressionType = MacroExpressionType.Unknown
						}
					};
			}
			return new TextMacroExpression
			{
				ExpressionType = MacroExpressionType.Method,
				DataSourceType = sourceType ?? m.MethodInfo.DeclaringType,
				MethodInfo = m.MethodInfo,
				Expressions = parameters,
				ValueProvider = valueProvider,
				ReturnType = m.TypeToReflect
			};
		}

		IEnumerable<ITextMacroExpression> TryParseInnerExpressions(Type sourceType, object macro)
		{
			if (macro?.GetType() == typeof(string) || macro.GetType() == typeof(ZString))
			{
				return ParseMacro(macro.ToString(), [sourceType]);
			}
			else if (macro != null)
			{
				// Don't know if this happends
				throw new InvalidOperationException("Unexpected parameter type " + macro.GetType());
			}
			return null;
		}

		ITextMacroValueProvider GetValueProvider(string macro, BusinessObjectDataProvider dataProvider)
		{
			var valueProvider = ProviderCache.GetProviderResponsibleFor(macro, Passes.FirstPass);

			if (valueProvider == null)
			{
				var dbProvider = new DBOrBOValueProvider();
				macro = macro.Trim('"');
				if (macro.StartsWith("<") && macro.EndsWith(">"))
				{
					macro = macro.Substring(1, macro.Length - 2);
				}
				if (dataProvider.DoesColumnExist(macro))
				{
					return dbProvider;
				}
				else
				{
					return null;
				}
			}
			return valueProvider;
		}

		static string ReplaceBraces(string str)
		{
			str = Regex.Replace(str, @"\""\{(.*?)\}\""", ev => $"<{ev.Groups[1].Value}>");
			str = Regex.Replace(str, @"\{(.*?)\}", ev => $"<{ev.Groups[1].Value}>");
			return str;
		}

		static string[] SplitRespctingQuotesAndAngleBrackets(string input, char delimitter = ',')
		{
			var result = new List<string>();
			var current = new List<char>();
			int quoteCount = 0;  // Tracks whether we're inside quotes
			int bracketCount = 0; // Tracks nesting level of brackets

			foreach (char c in input)
			{
				if (c == '\"') // Toggle quote count
				{
					quoteCount = quoteCount == 0 ? 1 : 0; // Only supports even quotes
				}
				else if (c == '<') // Enter a new bracket level
				{
					bracketCount++;
				}
				else if (c == '>') // Exit a bracket level
				{
					if (bracketCount > 0)
					{
						bracketCount--;
					}
				}
				else if (c == delimitter && quoteCount == 0 && bracketCount == 0) // Split only when not inside quotes or brackets
				{
					result.Add(new string(current.ToArray()));
					current.Clear();
					continue;
				}

				current.Add(c);
			}

			// Add the last segment
			if (current.Count > 0)
			{
				result.Add(new string(current.ToArray()));
			}

			return result.ToArray();
		}
	}
}
