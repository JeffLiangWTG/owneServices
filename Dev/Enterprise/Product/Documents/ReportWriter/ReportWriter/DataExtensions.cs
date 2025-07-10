using System;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.ReportWriter
{
	public static class DataExtensions
	{
		public static int GetLastRowOfTemplate(this ExcelWorkSheet worksheet)
		{
			var lastRowOfTemplate = -1;
			if (worksheet != null)
			{
				worksheet.RefreshRowCount();
				var rowCount = worksheet.RowCount;
				for (int i = 0; i < rowCount; i++)
				{
					if (worksheet[i, 0].ToString().Equals(Constants.AreaIdentifierTags.EndOfReport, StringComparison.OrdinalIgnoreCase))
					{
						lastRowOfTemplate = i;
						break;
					}
				}
			}
			return lastRowOfTemplate;
		}
	}
}
