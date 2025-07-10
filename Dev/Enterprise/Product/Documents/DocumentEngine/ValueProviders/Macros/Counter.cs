using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Counter : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Counter({key})>",
				ResString.GetMultilingualString("1b0367ca-383c-4673-9936-04631577b918", @"Returns a 1 based count of the number of times the expression is shown in the document. 
I.e: First time it is hit for a given key, returns a 1. Second time it returns a 2 etc. Every different key you use will start a new sequence at 1."),
				new List<(string example, object expectedResult)> { ((NoResString)"<Counter(Line)>", 1) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string counterName = Regex.Match(macro).Groups[1].Value;
			if (!Counters.ContainsKey(counterName))
			{
				Counters.Add(counterName, 0);
			}
			int result = ((int)Counters[counterName]) + 1;
			Counters[counterName] = result;
			return result;
		}
		readonly Hashtable Counters = new Hashtable();

		protected override void ResetCore()
		{
			Counters.Clear();
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Counter(?:[\s]*)\((?:[\s]*)([^\s,.]+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
