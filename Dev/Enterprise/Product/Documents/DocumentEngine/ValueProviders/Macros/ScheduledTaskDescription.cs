using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class ScheduledTaskDescription : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ScheduledTaskDescription>",
				ResString.GetMultilingualString("f950f514-9624-418a-8ce4-5ebe66361a3d",
				@"Returns the {0} from the linked {1} record when a report is triggered through scheduling mechanism.
Otherwise hides entire row that contains the macro.", StmScheduleTask.Schema.S5_ScheduleDescription, "StmScheduleTask"),
				new List<(string example, object expectedResult)> { ("<ScheduledTaskDescription>", (NoResString)"Address Profile Report") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return report.ScheduleTask == null ? new RowHider() : report.ScheduleTask.S5_ScheduleDescription;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Scheduled\s*Task\s*Description\s*>$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
