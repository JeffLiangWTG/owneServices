using System.Text.RegularExpressions;

namespace XH.XT.Monitoring.HealthCheckService
{
	public class XTRestSettings
	{
		public const string DirectxT = "DirectxT";
		public const string XHub = "XHub";

		public static readonly Regex ServerNameRegex = new(
			$@"/({Regex.Escape(DirectxT)}|{Regex.Escape(XHub)})/",
			RegexOptions.IgnoreCase);
		public string AlarmServerBaseUrl { get; set; }
		public string XTRestBaseUrl { get; set; }
		public string AccessToken { get; set; }
		public int WorkspaceTimeoutInSeconds { get; set; }
	}
}
