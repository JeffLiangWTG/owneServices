using System;
using static System.FormattableString;

namespace Enterprise.Client.EDI
{
	public static class CrikeyWeb
	{
		public static Uri GetTestResultsUrl(Guid userTestPk)
		{
			return GetUrl(Invariant($"TestResults/{userTestPk}"));
		}

		public static Uri GetTestFailureHistoryUrl(Guid testMethodPk)
		{
			return GetUrl(Invariant($"failures/testFailureHistory/{testMethodPk}"));
		}

		static Uri GetUrl(string path)
		{
			return new UriBuilder("http", HostName) { Path = path }.Uri;
		}

		const string HostName = "crikey.wtg.zone";
	}
}
