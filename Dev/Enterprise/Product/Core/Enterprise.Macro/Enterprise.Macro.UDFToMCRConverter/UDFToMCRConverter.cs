using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;

namespace Enterprise.Macro.UDFToMCRConverter
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
	public class UDFToMCRConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "It's readonly")]
		static readonly List<Func<ITextMacroExpression, ExpressionVariables, string>> converters = new()
		{
			MCRForBODocDataProviderCollectionHelper,
			ConvertMethodOrPropertyAccessor
		};

		public string Convert(string udfMacro, IAntlrMacroContext macroContext)
		{
			ExpressionVariables variables = new(macroContext.ParentType ?? macroContext.Parent.GetType());
			foreach (var kv in macroContext.Variables)
			{
				variables.VariableTypeNames[kv.Value.Item2] = $"@{kv.Key}";
			}
			var macroExpressions = ObjectFactory.Get<ITextMacroProcessor>().ParseMacro(udfMacro, variables.GetAllTypes().Where(t => typeof(IBusiness).IsAssignableFrom(t)));
			return ConvertExpressions(macroExpressions, variables);
		}

		static string ConvertExpressions(IEnumerable<ITextMacroExpression> macroExpressions, ExpressionVariables variables)
		{
			var mcr = new StringBuilder();
			Type resultType = null;
			string previousTextExpression = null;
			foreach (var expr in macroExpressions)
			{
				if (previousTextExpression != null)
				{
					if (expr.ReturnType != null)
					{
						mcr.Append(ReplaceBooleanLiteralBeforeField(expr.ReturnType, previousTextExpression, out var _));
					}
					else
					{
						mcr.Append(previousTextExpression);
					}
					previousTextExpression = null;
				}

				string currentMcr = null;
				if (expr.ExpressionType == MacroExpressionType.Unknown && !string.IsNullOrEmpty(expr.TextExpression))
				{
					currentMcr = ReplaceBooleanLiteralAfterField(resultType, expr.TextExpression, out var matched);
					if (matched)
					{
						mcr.Append(currentMcr);
					}
					else
					{
						previousTextExpression = expr.TextExpression;
					}
					resultType = null;
					continue;
				}

				if (resultType != null && resultType != expr.DataSourceType)
				{
					throw new InvalidMacroException($"Incompatible types. Return type of the prevoius expression ({resultType}) is not the sames as the source type of the current exoression ({expr.DataSourceType})");
				}

				currentMcr = ConvertExpression(expr, variables, out var exptResultType);
				if (resultType != null && mcr.Length > 0 && !currentMcr.StartsWith(".") && !currentMcr.StartsWith("["))
				{
					mcr.Append('.');
				}
				else if (resultType == null && expr.DataSourceType != null)
				{
					variables.VariableTypeNames.TryGetValue(expr.DataSourceType, out var varName);
					if (!string.IsNullOrEmpty(varName))
					{
						mcr.Append($"{varName}.");
					}
				}
				mcr.Append(currentMcr);
				resultType = expr.ReturnType ?? exptResultType;
			}

			if (previousTextExpression != null)
			{
				if (resultType != null)
				{
					mcr.Append(ReplaceBooleanLiteralBeforeField(resultType, previousTextExpression, out var _));
				}
				else
				{
					mcr.Append(previousTextExpression);
				}
			}

			return mcr.ToString();
		}

		static string ConvertExpression(ITextMacroExpression expr, ExpressionVariables variables)
		{
			return ConvertExpression(expr, variables, out var _);
		}

		static string ConvertExpression(ITextMacroExpression expr, ExpressionVariables variables, out Type resultType)
		{
			string mcr = null;
			resultType = expr.ReturnType;
			if (expr.ExpressionType == MacroExpressionType.Unknown)
			{
				if (!string.IsNullOrEmpty(expr.TextExpression))
				{
					mcr = expr.TextExpression;
				}
				else if (expr.Expressions?.Count > 0)
				{
					mcr = ConvertExpressions(expr.Expressions, variables);
				}
				else
				{
					throw new InvalidMacroException("Unknown expression");
				}
				return mcr;
			}
			else if (expr.ExpressionType == MacroExpressionType.ValueProvider)
			{
				if (ValueProviderConverter.EnvValueProviders.TryGetValue(expr.ValueProvider.GetType().FullName, out var valueProviderConverter))
				{
					mcr = $"{variables.EnvVariableName}.{valueProviderConverter.Item1}";
				}
				else if (ValueProviderConverter.ParametarizedValueProviders.TryGetValue($"{expr.ValueProvider.GetType().FullName},{expr.Expressions.Count}", out valueProviderConverter))
				{
					var pars = expr.Expressions.Select(e => ConvertExpression(e, variables)).ToArray();
					mcr = string.Format(valueProviderConverter.Item1, pars);
				}
				else
				{
					throw new InvalidMacroException($"Unknown value provider type: {expr.ValueProvider.GetType().FullName}");
				}
				resultType = valueProviderConverter.Item2;
				return mcr;
			}

			foreach (var func in converters)
			{
				var resultMcr = func(expr, variables);
				if (!string.IsNullOrEmpty(resultMcr))
				{
					mcr = resultMcr;
					break;
				}
			}

			if (mcr == null)
			{
				throw new InvalidMacroException($"Unknown expression. Type: {expr.ExpressionType} - Source: {expr.DataSourceType?.Name} - Text: {expr.TextExpression}");
			}

			return mcr;
		}

		static string ConvertMethodOrPropertyAccessor(ITextMacroExpression expr, ExpressionVariables variables)
		{
			if (expr.ExpressionType != MacroExpressionType.Method)
			{
				return null;
			}
			if (expr.MethodInfo == null)
			{
				throw new InvalidMacroException("MethodInfo is null");
			}
			if (expr.DataSourceType != null && !expr.MethodInfo.DeclaringType.IsAssignableFrom(expr.DataSourceType) && (!expr.DataSourceType.IsAssignableFrom(expr.MethodInfo.DeclaringType) || expr.DataSourceType.GetMember(expr.MethodInfo.Name) == null))
			{
				throw new InvalidMacroException($"Invalid macro: {expr.MethodInfo.Name} is not a member of {expr.DataSourceType.FullName}");
			}

			if (expr.MethodInfo.IsSpecialName && expr.MethodInfo.Name.StartsWith("get_"))
			{
				var name = expr.MethodInfo.Name.Substring(4);
				var properyInfo = expr.MethodInfo.DeclaringType.GetProperties().FirstOrDefault(p => p.Name == name);
				if (properyInfo != null)
				{
					if (name == "Item" && properyInfo.GetIndexParameters().Length > 0)
					{
						if (expr.Expressions == null || expr.Expressions.Count == 0)
						{
							return $".First()";
						}
						if (expr.Expressions.Count > 1)
						{
							throw new InvalidMacroException($"Invalid expression. Indexer with more that one parameters are not supported: {expr.MethodInfo.Name}");
						}

						var indexStr = ConvertExpression(expr.Expressions[0], variables);
						if (int.TryParse(indexStr, out var index))
						{
							return $"[{index - 1}]";
						}
						else
						{
							//TODO: UDF supports "first", "last" and "count" as index
							return $"[\"{index}\"]";
						}
					}
					else
					{
						return name;
					}
				}
			}
			var parameters = expr.Expressions.Select(e => ConvertExpression(e, variables));
			return $"{expr.MethodInfo.Name}({string.Join(", ", parameters)})";
		}

		static string MCRForBODocDataProviderCollectionHelper(ITextMacroExpression expr, ExpressionVariables variables)
		{
			if (expr.MethodInfo.DeclaringType != typeof(BODocDataProviderCollectionHelper))
			{
				return null;
			}
			switch (expr.MethodInfo.Name)
			{
				case "Find":
					return ConvertFindFunction(expr, variables);
				default:
					return expr.MethodInfo.Name;
			}
		}

		static string ConvertFindFunction(ITextMacroExpression expr, ExpressionVariables variables)
		{
			return $"First({{{ConvertExpressions(expr.Expressions[0].Expressions, variables)}}})";
		}

		static string ReplaceBooleanLiteralAfterField(Type lastResultType, string subStr, out bool replaced)
		{
			return ReplaceBooleanLiteral(@"^\s*(?<operator>==|!=)\s*""(?<bool>[YN])""", lastResultType, subStr, out replaced);
		}
		static string ReplaceBooleanLiteralBeforeField(Type lastResultType, string subStr, out bool replaced)
		{
			return ReplaceBooleanLiteral(@"""(?<bool>[YN])""\s*(?<operator>==|!=)\s*$", lastResultType, subStr, out replaced);
		}
		static string ReplaceBooleanLiteral(string regEx, Type lastResultType, string subStr, out bool replaced)
		{
			var matched = false;
			if (lastResultType == typeof(bool) || lastResultType == typeof(ZBool))
			{
				subStr = Regex.Replace(subStr, regEx, (match) =>
				{
					matched = true;
					return $" {match.Groups["operator"].Value} {ConvertUDFBoolean(match.Groups["bool"].Value)}";
				});
			}
			replaced = matched;
			return subStr;
		}

		static string ConvertUDFBoolean(string udfBoolean)
		{
			return udfBoolean == "Y" ? "true" : "false";
		}
	}
}
