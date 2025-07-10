using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	abstract class FRReportFunctionalTestCase : ReportFunctionalTestCase
	{
		protected sealed override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected sealed override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => GetExpectedColumnsInAnyOrder().Select(x => new ReportSchemaColumn(x.Type, x.Name)).ToList();

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
				var actualRowResult = FormatRowsValues(resultsOrderedByExpectedColumnNames.Rows[i], resultsOrderedByExpectedColumnNames);
				AssertRowContains(orderedExpectedResults.ElementAt(i), actualRowResult);
			}
		}

		protected abstract IEnumerable<(Type Type, string Name)> GetExpectedColumnsInAnyOrder();

		protected abstract IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults();

		protected abstract IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters();

		protected abstract IEnumerable<string> GetParameterNameList();

		void AssertRowContains((string ColumnName, string Value)[] expectedValues, string row)
		{
			var sb = new ZStringBuilder();
			var expectedValuesDictionary = expectedValues.ToDictionary(x => x.ColumnName, x => x.Value);

			int count = 1;
			var expectedColumns = GetExpectedColumnsInAnyOrder().Select(x => x.Name);
			foreach (var expectedColumn in expectedColumns)
			{
				if (expectedValuesDictionary.ContainsKey(expectedColumn))
				{
					sb.Append($"[{expectedColumn}]='{expectedValuesDictionary[expectedColumn]}'");
					if (count < expectedColumns.Count())
					{
						sb.Append("; ");
					}
				}
				count++;
			}
			AssertContains(sb.ToString(), row);
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
