using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ApprovalTaskUrl : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ApprovalTaskUrl({tableCode}, {taskPK})>",
				ResString.GetMultilingualString("145e71ef-2667-40da-90e8-f2c38357103f",
				@"Returns a URL that will allow user to action an Approval task. 
The link will work on systems that have GLOW Portal configured in the registry."),
				new List<(string example, object expectedResult)> {
					("<ApprovalTaskUrl(JS, <Task.P9_PK>)>", "https://myserver/Portals/goto/approval-JS?taskPK=48f224b7-79c9-4d82-8524-7b7d0b66e281"),
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var baseUrl = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (string.IsNullOrEmpty(baseUrl))
			{
				ReportMacroError(report, Res.GetString("c0ac56c9-ef76-4532-97a5-9cba0d69e482", "Please specify GLOW Portal URL in the Registry"));
			}

			var match = Regex.Match(macro);
			var tableCode = match.Groups["TableCode"].ToString().Trim();
			if (string.IsNullOrEmpty(tableCode))
			{
				ReportMacroError(report, Res.GetString("435495dc-b83b-4cef-8358-0a8121b8091e", "{0} not found", "TableCode"));
			}

			var taskPKAsString = match.Groups["TaskPK"].ToString().Trim();
			if (!Guid.TryParse(taskPKAsString, out var taskPK))
			{
				ReportMacroError(report, Res.GetString("0a9b3a3a-2025-4e7a-ae85-eff63e5f61a4", "{0} not found", "TaskPK"));
			}

			return !report.HasErrors
				? FormattableString.Invariant($"{baseUrl.TrimEnd('/')}/goto/approval-{tableCode}?taskPK={taskPK}")
				: string.Empty;
		}

		public override Regex Regex => regex;
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)ApprovalTaskUrl(?:[\s]*)\((?:[\s]*)(?<TableCode>[a-zA-Z0-9]*?)(?:[\s]*),(?<TaskPK>.*?)(?:[\s]*)??\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
