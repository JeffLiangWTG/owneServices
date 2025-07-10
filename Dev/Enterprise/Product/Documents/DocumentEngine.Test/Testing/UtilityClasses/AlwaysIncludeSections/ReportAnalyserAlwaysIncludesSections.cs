using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	sealed class ReportAnalyserAlwaysIncludesSections : ReportAnalyser
	{
		public ReportAnalyserAlwaysIncludesSections(Report report)
			: base(report)
		{
		}

		internal override bool EvaluateExpressionForConditionalAreas(string expression)
		{
			base.EvaluateExpressionForConditionalAreas(expression); // call base so any side effects have a chance to throw exceptions
			return true;
		}

		protected override bool IsAreaStart(string cellContents)
		{
			return base.IsAreaStart(cellContents)
				&& (
					cellContents.ToUpper().StartsWith(Constants.AreaIdentifierTags.Config) ||
					cellContents.ToUpper().StartsWith(Constants.ConditionalTags.If) ||
					cellContents.ToUpper().StartsWith(Constants.ConditionalTags.EndIf) ||
					cellContents.ToUpper().StartsWith(Constants.AreaIdentifierTags.EndOfReport) ||
					IsFirstAreaAfterConfig(cellContents)
				);
		}

		bool HasReturnedAreaAfterConfig;

		bool IsFirstAreaAfterConfig(string cellContents)
		{
			if (HasReturnedAreaAfterConfig)
			{
				return false;
			}
			else if (cellContents.StartsWith("#PageHeader:StartFromSecondPage"))
			{
				Globals.Message.ShowDeveloperErrorAlways("For this test to work please don't start a template with #PageHeader:StartFromSecondPage", "Error in Template");
				return true;
			}
			else
			{
				HasReturnedAreaAfterConfig = true;
				return true;
			}
		}
	}
}
