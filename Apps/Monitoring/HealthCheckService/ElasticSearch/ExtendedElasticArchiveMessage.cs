using System;
using Nest;
using XH.XT.ArchiveService.Common.ElasticSearch;

namespace XH.XT.Monitoring.HealthCheckService.ElasticSearch
{
	public class ExtendedElasticArchiveMessage : ElasticSearchArchiveMessage
	{
		[PropertyName("@timestamp")]
		public DateTime? Timestamp { get; set; }
	}
}
