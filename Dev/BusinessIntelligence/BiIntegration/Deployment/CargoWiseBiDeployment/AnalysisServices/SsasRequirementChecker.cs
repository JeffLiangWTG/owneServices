namespace CargoWise.Bi.Deployment.AnalysisServices
{
	using System.Globalization;
	using System.Threading;
	using Microsoft.AnalysisServices;
	using WTG.StaticAnalysis.Annotation;

	public class SsasRequirementChecker
	{
		public static SsasRequirementChecker Instance
		{
			get { return instance ?? (instance = new SsasRequirementChecker()); }
		}
		[ThreadSafe]
		static SsasRequirementChecker instance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Analysis Server connection string")]
		public bool IsSsasMeetRequirementWithTabularServerMode(string analysisServer)
		{
			var result = false;

			if (!string.IsNullOrWhiteSpace(analysisServer))
			{
				using (var server = new Server())
				{
					int retryCount = 0;
					while (retryCount < 5)
					{
						try
						{
							server.Connect(string.Format(CultureInfo.InvariantCulture, "Data Source={0};Application Name=SSAS Requirement Checker", analysisServer));
							break;
						}
						catch (ConnectionException ex)
						{
							retryCount++;
							if (retryCount == 5)
							{
								throw new SsasException(string.Format(CultureInfo.InvariantCulture, "Failed to connect to analysis server [{0}].\r\n{1}", analysisServer, ex.Message), ex);
							}
						}
						Thread.Sleep(1000);
					}

					result = server.ServerMode == ServerMode.Tabular;
				}
			}

			return result;
		}
	}
}
