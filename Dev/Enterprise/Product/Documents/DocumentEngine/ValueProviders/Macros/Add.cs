using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Add : MathOperationProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Add(\"{value1}\",\"{value2}\",...)>",
				ResString.GetMultilingualString("d68009c2-fcc2-4d6e-a9e0-91d42b26c663", @"Will do the addition of two or more values."),
				new List<(string example, object expectedResult)> { ((NoResString)"<Add(\"10\", \"20\", \"30\")>", "60"), ((NoResString)"<Add(\"<GenericTransactionHeader.TotalOSAmount>\",\"<GenericTransactionHeader.OSOutstandingAmount>\",\"<GenericTransactionHeader.OSOutstandingAmount>\")>", "100") });
		}

		protected override System.Func<decimal, decimal, decimal> Operation
		{
			get { return (x, y) => x + y; }
		}

		public override string OperationName
		{
			get { return (NoResString)"Add"; }
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:\s*)Add(?:\s*)\((?:\s*)(?<operands>.*)\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
