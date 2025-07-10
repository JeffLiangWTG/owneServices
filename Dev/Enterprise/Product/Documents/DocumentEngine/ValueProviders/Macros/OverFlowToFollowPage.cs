using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class OverFlowToFollowPage : ValueProvider, INonVisualisableValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<OverFlowToFollowPage(\"{fieldtitle}\"[[, {maxrowcount}], {Overflowbehaviour}])>",
			ResString.GetMultilingualString("ec19af82-30c2-48d4-b35d-c5b36cdc4f4a",
			@"This macro is applied as a prefix to all other macros in the cell, and is applied after all other macros in the cell have been replaced. 
If the replaced contents will not fit within the bounds of the cell, as much information as can be fit will be left in the cell and the remainder will be shown in a plain paper follow page following the main document. 
The text supplied in {0} will be used as a title for any text moved to a follow page. 
If {1} is included, the cell will be allowed to wrap and expand up to the number of rows defined in {2}. Default value is 1. 
The {3} can have the following values:  
	- {4} (default) :- Moves text that will not fit to a follow page and will add a ""(continued...)"" at the end of the original cell.  
	- {5} :- Moves text that will not fit to a follow page. Cuts off before the last character that will not fit.  
	- {6} :- If too long, moves all text to a follow page and will insert ""(continued...)"" in the original cell.  
	- {7} :- If too long, moves all text to a follow page. 
	- {8} :- If too long, truncate text and then show all text on a follow page.",
			"{fieldtitle}", "{rowcount}", "{maxrowcount}", "{Overflowbehaviour}",
			"WrapOverflowWithContinued", "WrapOverflowWithoutContinued", "MoveAllContentWithContinued",
			"MoveAllContentWithoutContinued", "MoveAllContentAndKeepOriginal"),
				new List<(string example, object expectedResult)> { ((NoResString)"<OverFlowToFollowPage(\"Notes\", 25, MoveAllContentWithContinued)><ExpandToFit><Notes>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return "";
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(string.Format("^{0}$", RegexPattern), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only. Regular Expression.")]
		const string RegexPattern = @"<[\s]*Over[\s]*Flow[\s]*To[\s]*Follow[\s]*Page[\s]*(\(?[\s]*""{1}[\s]*(?<fieldTitle>[^""]+)[\s]*""{1}[\s]*\)?,?[\s]*(?<rowCount>[0-9]*)\)?,?[\s]*(?<overflowBehavior>[A-Za-z]*)\))[\s]*>";

		/// <summary>
		/// This is the regex really used to interpret this line in the class Enterprise.DocumentEngine.Areas.Area 
		/// and by the visualiser to remove the macro text before visualising.
		/// </summary>
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		internal static Match RegexMatch(string macro)
		{
			var match = RegexToFindMacroAnyWhereInString.Match(macro);

			if (match.Success)
			{
				return match;
			}

			return null;
		}

		internal static int GetMaximumRows(string macro)
		{
			int result = 1;

			if (RegexMatch(macro) != null)
			{
				var groups = RegexMatch(macro).Groups;

				if (!int.TryParse(groups["rowCount"].Value, out result))
				{
					result = 1;
				}
			}

			return result;
		}

		internal static string GetFieldTitle(string macro)
		{
			string result = "";

			if (RegexMatch(macro) != null)
			{
				var groups = RegexMatch(macro).Groups;

				result = groups["fieldTitle"].Value;
			}

			return result;
		}

		internal static OverflowBehaviour GetOverflowBehavior(string macro)
		{
			var result = OverflowBehaviour.WrapOverflowWithContinued;

			if (RegexMatch(macro) != null)
			{
				var groups = RegexMatch(macro).Groups;
				var overflowBehaviourText = groups["overflowBehavior"].Value;

				if (Enum.IsDefined(typeof(OverflowBehaviour), overflowBehaviourText))
				{
					result = (OverflowBehaviour)Enum.Parse(typeof(OverflowBehaviour), overflowBehaviourText, true);
				}
			}

			return result;
		}

		#region INonVisualisableValueProvider

		public Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;

		#endregion
	}
}
