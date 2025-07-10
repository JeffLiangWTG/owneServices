using System;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.ValueReplacers
{
	internal delegate object ReplacementProviderMethod(string macro, Report report);

	internal class DelegateValueProvider : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			throw new NotImplementedException();
		}

		public DelegateValueProvider(string macroName, ReplacementProviderMethod replacementProvider)
			: this(macroName, replacementProvider, Passes.FirstPass)
		{
		}

		public DelegateValueProvider(string macroName, ReplacementProviderMethod replacementProvider, Passes pass)
		{
			this.MacroName = macroName;
			this.ReplacementProvider = replacementProvider;
			fPass = pass;
			fRegex = new Regex(@"^<(?:[\s]*)" + Regex.Escape(macroName) + @"(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return ReplacementProvider(macro, report);
		}

		public override Enterprise.DocumentEngine.Visualisation.VisualiserComponentTypes ComponentType
		{
			get { return Enterprise.DocumentEngine.Visualisation.VisualiserComponentTypes.TextEdit; }
		}

		readonly Regex fRegex;
		public override Regex Regex
		{
			get { return fRegex; }
		}

		public override Passes PassToStartReplacingOn
		{
			get { return fPass; }
		}

		public string MacroName;
		readonly ReplacementProviderMethod ReplacementProvider;
		readonly Passes fPass;
	}
}
