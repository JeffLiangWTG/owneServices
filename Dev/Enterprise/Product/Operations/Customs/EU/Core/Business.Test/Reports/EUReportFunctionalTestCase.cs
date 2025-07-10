using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.EU.Business.Reports.Testing
{
	abstract class EUReportFunctionalTestCase : ReportFunctionalTestCase
	{
		protected sealed override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected sealed override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => GetExpectedColumnsInOutputOrder().Select(x => new ReportSchemaColumn(x.Type, x.Name)).ToList();

		protected sealed override List<string> ParametersValuesList
		{
			get
			{
				var parameterList = GetParametersValuesList().ToList();
				if (parameterList.Count != TotalNumberOfParams)
				{
					throw new Exception($"ParameterList count is incorrect, expected {TotalNumberOfParams} but was {parameterList.Count}");
				}
				return parameterList;
			}
		}

		protected sealed override void AssertTestResults(DataTable resultsOrderedByExpectedColumnNames)
		{
			var orderedExpectedResults = GetOrderedExpectedResults();
			AssertEquals("Expected rows count", orderedExpectedResults.Count(), resultsOrderedByExpectedColumnNames.Rows.Count);

			for (int i = 0; i < orderedExpectedResults.Count(); i++)
			{
				var actualRowResult = FormatRowsValues(resultsOrderedByExpectedColumnNames.Rows[i], resultsOrderedByExpectedColumnNames, ignoreGuid: true);
				AssertRowEquals(orderedExpectedResults.ElementAt(i), actualRowResult);
			}
		}

		protected abstract IEnumerable<(Type Type, string Name)> GetExpectedColumnsInOutputOrder();

		protected abstract IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults();

		protected abstract IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters();

		protected abstract IEnumerable<string> GetParameterNameList();

		void AssertRowEquals((string ColumnName, string Value)[] expectedValues, string row)
		{
			var sb = new ZStringBuilder();
			foreach (var (columnName, value) in expectedValues)
			{
				sb.Append($"[{columnName}]='{value}'");
			}
			AssertEquals(sb.ToStringWithDelimiterBetweenAppends("; "), row);
		}

		IEnumerable<string> GetParametersValuesList()
		{
			var populatedFilterParameters = GetPopulatedFilterParameters().ToDictionary(x => x.ParameterName, x => x.ParameterValue);

			var parameterNameList = GetParameterNameList();
			foreach (var parameterName in parameterNameList)
			{
				yield return populatedFilterParameters.ContainsKey(parameterName) ? $"'{populatedFilterParameters[parameterName]}'" : "NULL";
			}
		}

		int TotalNumberOfParams => GetParameterNameList().Count();
	}
}
