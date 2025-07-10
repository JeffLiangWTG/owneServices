using System;
using Enterprise.ZArchitecture.Core;
using WTG.ErrorReporting;

namespace Enterprise.ErrorReporting.Module.Filters
{
	class ErrorReportTypeList : CodeDescriptionPairList
	{
		public ErrorReportTypeList()
		{
			foreach (var name in Enum.GetNames(typeof(ErrorReportType)))
			{
				var value = (ErrorReportType)Enum.Parse(typeof(ErrorReportType), name);
				var valueText = value.ToString("D");
				AddPair(valueText, name);
			}
		}
	}
}
