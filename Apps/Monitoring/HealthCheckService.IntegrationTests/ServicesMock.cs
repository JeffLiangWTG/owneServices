using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using MailKit.Net.Smtp;
using Moq;
using Nest;
using XH.XT.Monitoring.HealthCheckService.XTRESTAPI;

namespace XH.XT.Monitoring.HealthCheckService.Tests
{
	public class ServicesMock
	{
		public Mock<XTRestClient> XTRestClientMock { get; set; }
		public Mock<IElasticClient> ElasticClientMock { get; set; }
		public Mock<IDateTimeProvider> DateTimeProvider { get; set; }
		public Mock<IHttpClientFactory> HttpClientFactoryProvider { get; set; }
		public Mock<ISmtpClient> SmtpClient { get; set; }

		public IEnumerable<(Type UnderlyingType, object Object)> GetMocks()
		{
			return GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => (x.GetValue(this) as Mock) != null)
				.Select(x =>
				{
					var underlyingType = x.PropertyType.GetGenericArguments()[0];
					var value = x.GetValue(this) as Mock;

					return (underlyingType, value?.Object);
				})
				.ToArray();
		}
	}
}
