using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.DataProviders
{
	class MacroRegexReplacer
	{
		public MacroRegexReplacer(Report reportObject)
		{
			this.reportObject = reportObject;
		}

		public string ReplaceMacrosAndRebuildUdfParameterList(string dataSourceString)
		{
			string result = dataSourceString;
			udfParameters.Clear();

			while (RegexProvider.MacroRegexDouble.IsMatch(result))
			{
				result = RegexProvider.MacroRegexDouble.Replace(result, ReplaceMacrosOnly);
			}

			if (RegexProvider.UserRepositoryMacroRegex.IsMatch(result))
			{
				result = RegexProvider.UserRepositoryMacroRegex.Replace(result, ReplaceParameterlessMacro);
			}

			result = RegexProvider.InnermostMacrosRegex.Replace(result, ReplaceParameters);

			return result.UnEscapeAngleBrackets();
		}

		public string ReplaceParametersForSQLQueryHints(string sqlqueryhintsString)
		{
			string result = sqlqueryhintsString;
			result = RegexProvider.InnermostMacrosRegex.Replace(result, ReplaceParameters);
			return result.UnEscapeAngleBrackets();
		}

		string ReplaceMacrosOnly(Match aMatch)
		{
			string macro = "<" + aMatch.Groups[1].Value + ">";
			return reportObject.MacroTranslator.GetValue(macro, Passes.FirstPass).ToString();
		}

		string ReplaceParameterlessMacro(Match aMatch)
		{
			string macro = aMatch.Groups[0].Value;
			return reportObject.MacroTranslator.GetValue(macro, Passes.FirstPass).ToString();
		}

		string ReplaceParameters(Match aMatch)
		{
			string macro = aMatch.Groups[0].Value;
			object macroValue = reportObject.MacroTranslator.GetValue(macro, Passes.FirstPass, true);
			if (!RegexProvider.SingleMacroOnlyRegex.IsMatch(macroValue.ToString()))
			{
				var macroValueAsZType = macroValue as IZType;
				if (macroValueAsZType != null)
				{
					if (macroValueAsZType.IsEmpty)
					{
						macroValue = DBNull.Value;
					}
					else if (!macroValueAsZType.IsValid)
					{
						macroValue = DBNull.Value;
					}
					else
					{
						macroValue = ((IZTypeInternals)macroValue).GetValueForLogicalDataLayer(true);
					}
				}

				SqlParameter param;
				var macroValueWithParameterType = macroValue as ReplacementWithParameterType;
				if (macroValueWithParameterType != null)
				{
					param = GetSqlParameterForMacroAndAddTheNewOnesToParametersForUserDefinedFunctions(macro, macroValueWithParameterType.MacroValue, macroValueWithParameterType.ParameterTypeName);
				}
				else
				{
					var macroValueWithSqlDbType = macroValue as ReplacementWithSqlDbType;
					if (macroValueWithSqlDbType != null)
					{
						param = GetSqlParameterForMacroAndAddTheNewOnesToParametersForUserDefinedFunctions(macro, macroValueWithSqlDbType.MacroValue, macroValueWithSqlDbType.SqlDbType);
					}
					else
					{
						param = GetSqlParameterForMacroAndAddTheNewOnesToParametersForUserDefinedFunctions(macro, macroValue);
					}
				}
				return param.ParameterName;
			}
			return macro;
		}

		/// <summary>
		/// It caches Macro names to avoid generating duplicate sql parameter per macro
		/// It is not a critical issue, it just simplifies reading generated SQL queries specially for debugging purpose
		/// </summary>
		protected SqlParameter GetSqlParameterForMacroAndAddTheNewOnesToParametersForUserDefinedFunctions(string macro, object macroValue, string parameterTypeName = "")
		{
			SqlParameter sqlParameter;

			if (parameterDefinedForMacro.ContainsKey(macro))
			{
				sqlParameter = parameterDefinedForMacro[macro];
			}
			else
			{
				sqlParameter = new SqlParameter(SqlParameterNameGenerator.Next(), macroValue);
				SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(sqlParameter);
				if (!string.IsNullOrEmpty(parameterTypeName))
				{
					sqlParameter.TypeName = parameterTypeName;
				}
				parameterDefinedForMacro[macro] = sqlParameter;
				udfParameters.Add(sqlParameter);
			}

			return sqlParameter;
		}

		protected SqlParameter GetSqlParameterForMacroAndAddTheNewOnesToParametersForUserDefinedFunctions(string macro, object macroValue, SqlDbType sqlDbType)
		{
			SqlParameter sqlParameter;

			if (parameterDefinedForMacro.ContainsKey(macro))
			{
				sqlParameter = parameterDefinedForMacro[macro];
			}
			else
			{
				sqlParameter = new SqlParameter(SqlParameterNameGenerator.Next(), macroValue);
				sqlParameter.SqlDbType = sqlDbType;
				parameterDefinedForMacro[macro] = sqlParameter;
				udfParameters.Add(sqlParameter);
			}

			return sqlParameter;
		}

		public SqlParameter[] UdfParameters
		{
			get { return udfParameters.ToArray(); }
		}
		readonly List<SqlParameter> udfParameters = new List<SqlParameter>();

		readonly Report reportObject;
		readonly Dictionary<string, SqlParameter> parameterDefinedForMacro = new Dictionary<string, SqlParameter>();
	}
}
