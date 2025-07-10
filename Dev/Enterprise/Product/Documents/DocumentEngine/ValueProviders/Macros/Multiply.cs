using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Multiply : MathOperationProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Multiply(\"{value1}\",\"{value2}\",...)>",
				ResString.GetMultilingualString("c0fb4496-ed2e-42ef-b670-1a81a7bc5dff", @"Will do the multiplication of two or more values."),
				new List<(string example, object expectedResult)> { ((NoResString)"<Multiply(\"<TotalOSAmount>\", \"<OSOutstandingAmount>\", \"<OSOutstandingAmount>\")>", "4000") });
		}

		protected override System.Func<decimal, decimal, decimal> Operation
		{
			get { return (x, y) => (x * y); }
		}

		public override string OperationName
		{
			get { return (NoResString)"Multiply"; }
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:\s*)Multiply(?:\s*)\((?:\s*)(?<operands>.*)\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
