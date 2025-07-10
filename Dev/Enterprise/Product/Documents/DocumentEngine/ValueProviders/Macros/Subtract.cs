using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Subtract : MathOperationProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Subtract(\"{value1}\",\"{value2}\",...)>",
				ResString.GetMultilingualString("88d016ea-5bf6-4cbb-b2bb-0c191299770a", @"Will do the Subtraction of two or more values."),
				new List<(string example, object expectedResult)> { ((NoResString)"<Subtract(\"<TotalOSAmount>\", \"<OSOutstandingAmount>\", \"<OSOutstandingAmount>\")>", "60") });
		}

		protected override System.Func<decimal, decimal, decimal> Operation
		{
			get { return (x, y) => x - y; }
		}

		public override string OperationName
		{
			get { return (NoResString)"Subtract"; }
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:\s*)Subtract(?:\s*)\((?:\s*)(?<operands>.*)\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
