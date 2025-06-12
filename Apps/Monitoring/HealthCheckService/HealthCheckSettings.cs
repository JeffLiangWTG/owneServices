using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XH.XT.Monitoring.HealthCheckService
{
	public class HealthCheckSettings
	{
		[Required]
		public IEnumerable<TrackingMessage> TrackingMessages { get; set; }

		public IEnumerable<CustomsWebServer> CustomsWebServers { get; set; }

		public IEnumerable<CustomsMailServer> CustomsMailServers { get; set; }
	}

	public class TrackingMessage
	{
		[Required]
		public string SourceParty { get; set; }
		[Required]
		public string DestinationParty { get; set; }
		public string Contract { get; set; }
		[Required]
		public string Description { get; set; }
		[Required]
		public int ErrorDelayMins { get; set; }
		public string ElasticIndex { get; set; } = "idx-*-*-xt-archive*";
	}

	public class CustomsWebServer
	{
		[Required]
		public string Name { get; set; }
		[Required]
		[Url]
		public string Url { get; set; }
	}
	public class CustomsMailServer
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public string Host { get; set; }
		[Range(25, 65535)]
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool EnableSSL { get; set; }
	}
}
