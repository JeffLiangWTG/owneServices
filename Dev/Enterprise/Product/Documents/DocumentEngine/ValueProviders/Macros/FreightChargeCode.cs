using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class FreightChargeCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FreightChargeCode>",
				ResString.GetMultilingualString("a9ff4e42-a339-4f6e-a481-ac8c634e13ac", "Returns a GUID value representing the Freight Charge Code from the registry."),
				new List<(string example, object expectedResult)> { ("<FreightChargeCode>", new Guid("466D1DF3-BF79-41F1-8B46-4561FC7C669B")) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return Env.Registry.FreightChargeCode;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Freight\s*Charge\s*Code\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
