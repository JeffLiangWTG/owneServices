using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using static Enterprise.ZArchitecture.Business.StmALog;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GetEventReferenceValue : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GetEventReferenceValue({EventReference},{EventParameter})>",
				ResString.GetMultilingualString("8dfae197-bdd3-4f89-ac4f-c9d803217949", "Returns the value of an event reference parameter."),
				new List<(string example, object expectedResult)> { ("<GetEventReferenceValue(\"<SL_Reference>\", \"ACT\")>", "123") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);

			if (match.Success)
			{
				var eventReference = match.Groups["EventReference"].Value;
				var eventParameter = match.Groups["EventParameter"].Value;
				if (!(eventReference.IsNullOrEmpty() || eventParameter.IsNullOrEmpty()))
				{
					if (GetParametersFromReference(eventReference, ParseReferenceError.None).TryGetValue(eventParameter, out var parameterValue))
					{
						return parameterValue;
					}
				}
			}
			return string.Empty;
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<\s*GetEventReferenceValue\s*\(\s*""(?<EventReference>.*?)"",\s*""(?<EventParameter>.*?)""?\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
