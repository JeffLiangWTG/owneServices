using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine.ValueReplacers
{
	[System.Diagnostics.DebuggerDisplay("FixedValueProvider: {MacroName}")]
	internal class FixedValueProvider : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			throw new NotImplementedException();
		}

		public FixedValueProvider(string macroName, object value)
			: this(macroName, value, Passes.FirstPass)
		{
		}

		public FixedValueProvider(string macroName, object value, Passes pass)
		{
			if (cachedRegex == null)
			{
				cachedRegex = new Dictionary<string, Regex>();
			}

			this.MacroName = macroName;
			this.Value = value;
			fPass = pass;
			if (!cachedRegex.TryGetValue(macroName, out fRegex))
			{
				fRegex = new Regex(@"^<(?:[\s]*)" + Regex.Escape(MacroName) + @"(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
				cachedRegex[macroName] = fRegex;
			}
		}

		[ThreadStatic]
		static Dictionary<string, Regex> cachedRegex;

		protected override object GetReplacementCore(string macro, Report report)
		{
			return Value;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		readonly Regex fRegex;

		public override Passes PassToStartReplacingOn
		{
			get { return fPass; }
		}
		readonly Passes fPass;

		readonly string MacroName;
		readonly object Value;

		public override VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.TextEdit; }
		}
	}
}
