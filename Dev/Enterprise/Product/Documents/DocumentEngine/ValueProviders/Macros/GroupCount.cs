using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GroupCount : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GroupCount({tablename},{fieldname}[,{logicalExpression}])>",
				ResString.GetMultilingualString("287aeeda-24f8-4135-b6c2-c4677cc18688", @"Returns the number of groups that will be created if you group by the specified table and field. The logical expression supports:
	Logical Operators: AND (&&) and OR (||)
	Relational Comparison: EQUALS (==) and NOT EQUALS (!=).
Any field that is text requires """" to be placed around the property."),
				new List<(string example, object expectedResult)> {
					((NoResString)"<GroupCount(Lines, GST, \"<GST>\"==\"5\" && \"<Currency>\"==\"AUD\")>", 1),
					((NoResString)"<GroupCount(Lines, GST)>", 8) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			object result = 0;
			try
			{
				bool match = Regex.IsMatch(macro);
				List<string> parameters = new List<string>(Regex.Split(macro)).FindAll(new Predicate<string>((string a) => { return !(new ZString(a)).IsEmpty; }));

				string tableName = parameters[0];
				string[] columnNames = parameters[1].Split(new char[] { '+' });

				IDataRowSource dataSourceToGroup = report.DataProvider.GetDataRowSource(tableName);

				if (parameters.Count == 3)
				{
					ZString filterExpression = parameters[2];
					if (!filterExpression.IsEmpty)
					{
						dataSourceToGroup = dataSourceToGroup.Filter(filterExpression);
					}
				}
				result = dataSourceToGroup.GroupCount(columnNames);
			}
			catch (DataProviderException ex)
			{
				ReportMacroError(report, ex.Message);
			}

			return result;
		}

		protected override bool ShouldEvaluateInnerMacrosCore(string macro)
		{
			return false;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Group(?:[\s]*)Count(?:[\s]*)\((?:[\s]*)([^\s,.]+)(?:[\s]*),(?:[\s]*)([^,]*)(?:[\s,]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
