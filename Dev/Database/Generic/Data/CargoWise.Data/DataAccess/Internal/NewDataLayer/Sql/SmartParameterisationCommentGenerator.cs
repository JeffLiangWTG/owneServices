using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
#if DEBUG
	public
#endif
	class SmartParameterisationCommentGenerator
	{
		public string Generate(IZSqlParameter[] parameters)
		{
			_ = parameters ?? throw new ArgumentNullException(nameof(parameters));

			var now = DateTime.Now;
			var columnSummary = new Dictionary<SchemaColumn, HashSet<string>>();

			// THIS CODE IS NOT DIAGNOSTIC CODE - it implements smart parameterisation which 'rightsizes' the number of execution plans in SQL server
			foreach (var parameter in parameters)
			{
				if (!parameter.IsLiteralOnly)
				{
					var suffix = parameter.GetSqlParameterSuffix(now);
					if (!string.IsNullOrWhiteSpace(suffix))
					{
						if (!columnSummary.TryGetValue(parameter.SchemaColumn, out var hashSet))
						{
							hashSet = new HashSet<string>();
							columnSummary.Add(parameter.SchemaColumn, hashSet);
						}

						// this code removes smaller numerical values as the numbers are exponent based and 10^2 is inconsequential compared to 10^3 (but leaves all of the NOHISTOGRAM/FUTUREYEARS etc)
						if (int.TryParse(suffix, out var suffixNumericValue))
						{
							int hashSetValue = 0;
							hashSet.RemoveWhere(value => int.TryParse(value, out hashSetValue) && hashSetValue < suffixNumericValue);
						}
						hashSet.Add(suffix);
					}
				}
			}

			var parameterStats =
						string.Join(Environment.NewLine,
									 columnSummary
									 .OrderBy(sc => sc.Key.TableName)
									 .ThenBy(sc => sc.Key.Ordinal)
									 .Select(x => x.Key.Name + " = " + string.Join(",", x.Value.OrderBy(suffix => suffix))));

			return parameterStats.Length == 0 ? string.Empty : Environment.NewLine + Environment.NewLine + "/* Parameter Stats" + Environment.NewLine + parameterStats + Environment.NewLine + "End Parameter Stats */" + Environment.NewLine;
		}
	}
}
