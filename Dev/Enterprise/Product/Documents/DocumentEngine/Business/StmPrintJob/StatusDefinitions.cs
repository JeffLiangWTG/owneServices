using System;

namespace Enterprise.DocumentEngine
{
	public enum PrintJobStatus
	{
		QUE,
		FAL,
		WRK
	}

	public static class PrintJobStatusName
	{
		public static string Name(string code)
		{
			if (Enum.TryParse<PrintJobStatus>(code, true, out var result))
			{
				return GetName(result);
			}

			throw new ArgumentOutOfRangeException(nameof(code), code, "Supplied status code is not part of PrintJobStatus enumeration");

			string GetName(PrintJobStatus printJobStatus)
			{
				switch (printJobStatus)
				{
					case PrintJobStatus.QUE:
						return Res.GetString("0914FED5-3E93-4069-AC9B-A3CC0E8C818C", "Queued");
					case PrintJobStatus.FAL:
						return Res.GetString("65B2B34D-37E1-47FB-8396-3535AD163797", "Failed");
					default:
						return string.Empty;
				}
			}
		}
	}
}
