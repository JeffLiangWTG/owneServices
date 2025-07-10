using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Divide : MathOperationProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Divide(\"{value1}\",\"{value2}\",...)>",
				ResString.GetMultilingualString("d037a6a2-79d6-4500-ae73-bf99adf38b4f", @"Will do the division of two or more values."),
				new List<(string example, object expectedResult)> { ((NoResString)"<Divide(\"<TotalOSAmount>\", \"<OSOutstandingAmount>\", \"<OSOutstandingAmount>\")>", "40") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			try
			{
				return base.GetReplacementCore(macro, report);
			}
			catch (DivideByZeroException ex)
			{
				ReportMacroError(report, ex.Message);
				return string.Empty;
			}
		}

		protected override Func<decimal, decimal, decimal> Operation
		{
			get { return (x, y) => (x / y); }
		}

		public override string OperationName
		{
			get { return (NoResString)"Divide"; }
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:\s*)Divide(?:\s*)\((?:\s*)(?<operands>.*)\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
