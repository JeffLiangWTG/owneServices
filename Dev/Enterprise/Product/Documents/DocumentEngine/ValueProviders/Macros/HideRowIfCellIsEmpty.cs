using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class HideRowIfCellIsEmpty : ValueProvider, INonVisualisableValueProviderThatModifyDocumentLayout, IValueProviderThatShouldBeCopiedIfNewRowAdded
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<HideRowIfCellIsEmpty>",
				ResString.GetMultilingualString("c89a001e-8e67-46ed-bf8d-e15ea9a01348",
				@"This macro returns no value, but will cause the Document Engine to set the height of the current row to 0 (hides the row) if the rest of the contents of the cell are empty. 
Designed to be used in conjunction with a following macro. This means that if {0} was empty the example shown below would cause the whole row it was on to be hidden. {1}",
				"<Importer.Fax>", DocumentationForFormatting),
				new List<(string example, object expectedResult)> { ((NoResString)"<HideRowIfCellIsEmpty><Importer.Fax>", null) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Hide(?:[\s]*)Row(?:[\s]*)If(?:[\s]*)Cell(?:[\s]*)Is(?:[\s]*)Empty(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		/// <summary>
		/// This is the regex really used to interpret this line in the class Enterprise.DocumentEngine.Areas.Area 
		/// and by the visualiser to remove the macro text before visualising.
		/// </summary>
		internal static readonly Regex RegexToFindMacroAnyWhereInString = new Regex(@"<(?:[\s]*)Hide(?:[\s]*)Row(?:[\s]*)If(?:[\s]*)Cell(?:[\s]*)Is(?:[\s]*)Empty(?:[\s]*)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		#region INonVisualisableValueProviderThatModifyDocumentLayout

		public ZString DocumentationForFormatting => AdditionalDocumentation;
		public Regex RegexToReplaceMacro => RegexToFindMacroAnyWhereInString;

		#endregion

		#region IValueProviderThatShouldBeCopiedIfNewRowAdded

		public Regex RegexToFindMacro => RegexToFindMacroAnyWhereInString;

		#endregion
	}
}
