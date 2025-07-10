using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Now : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(Useage,
				ResString.GetMultilingualString("ce421917-76c7-433f-a524-009a5f0459b1", "Returns the current system Date and Time as a DateTime value. NB: You'll need to format the cell on the Template to make sure the result is shown in the format you want to see it in."),
				new List<(string example, object expectedResult)> { ((NoResString)"<Now>", new ZDateTime(2018, 11, 7, 12, 0, 0)) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (nowTime.IsEmpty)
			{
				nowTime = ZDateTime.Now;
			}
			return nowTime;
		}
		ZDateTime nowTime = ZDateTime.Empty;

		protected override void ResetCore()
		{
			nowTime = ZDateTime.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Now(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Literal string is safe to use in this context.")]
		public const string Useage = "<Now>";
	}
}
