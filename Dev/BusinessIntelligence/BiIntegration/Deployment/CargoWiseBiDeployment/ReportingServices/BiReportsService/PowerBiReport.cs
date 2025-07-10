using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	public class PowerBiReport
	{
		public string ReportName { get; set; }
		public string ReportPath { get; set; }
		public string BusinessArea { get; set; }
		public bool IsSystemLevel { get; set; }
		public string ResourceType { get; set; }
		[SuppressMessage("Microsoft.Design", "CA1056", Justification = "this is not a Uri")]
		public string UrlEncodedReportName { get; set; }
	}
}
