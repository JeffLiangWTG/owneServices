using System.Text.RegularExpressions;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	public static class FilterValidator
	{
		public static void ValidateFilter(StmModuleFilter filter)
		{
			RelatedModuleFiltersHelper.ValidateFilterStrips(filter);

			if (!filter.HasErrors) // filter strip errors may cause exceptions to be thrown if we try to get the filter query.
			{
				var errorMessage = Res.GetString("60866583-4C86-4118-AB8C-6E502CE3D914", "The 'Startable Task Only' filter cannot be chosen on board section filters. Use the configuration option labeled 'Enable Show Startable Items filter by default' instead.");

				var filterQuery = RelatedModuleFiltersHelper.GetFilterQuery(filter);

				filter.ClearRowNotificationsContaining(errorMessage);

				var hasCurrentTasksSQLFunctionText = Regex.Match(
					filterQuery.LiteralTextADO,
					$@"({BMGlobalConstants.CurrentTasksSQLFunctionText}|{BMGlobalConstants.CurrentTasksIgnoringIterationsSQLFunctionText})\s*?\(", // ignore newLine, carriageReturn, tab, space before '('
					RegexOptions.IgnoreCase).Success;

				if (hasCurrentTasksSQLFunctionText)
				{
					filter.AddRowError(errorMessage);
				}
			}
		}
	}
}
