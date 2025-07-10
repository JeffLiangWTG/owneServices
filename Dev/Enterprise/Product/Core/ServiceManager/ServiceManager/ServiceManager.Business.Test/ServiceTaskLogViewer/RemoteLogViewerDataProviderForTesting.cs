using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Moq;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using static Enterprise.ServiceManager.Business.LogViewerDataProviderFactory;

namespace Enterprise.ServiceManager.Business.Testing
{
	sealed class RemoteLogViewerDataProviderForTesting : RemoteLogViewerDataProvider
	{
		readonly Exception responseException;

		public RemoteLogViewerDataProviderForTesting(string host, string taskCode, string testLogsDirectory)
			: base(host, taskCode, CreateMockHostCache(host))
		{
			this.testLogsDirectory = testLogsDirectory;
		}

		public RemoteLogViewerDataProviderForTesting(string host, string taskCode, IServiceHostsCache serviceHostsCache, string testLogsDirectory)
			: base(host, taskCode, serviceHostsCache)
		{
			this.testLogsDirectory = testLogsDirectory;
		}

		public RemoteLogViewerDataProviderForTesting(string host, string taskCode, IServiceHostsCache serviceHostsCache, Exception responseException, string testLogsDirectory)
			: base(host, taskCode, serviceHostsCache)
		{
			this.responseException = responseException;
			this.testLogsDirectory = testLogsDirectory;
		}

		static IServiceHostsCache CreateMockHostCache(string host)
		{
			var hostsCache = new Mock<IServiceHostsCache>();
			var hostsClient = new Mock<IServiceHostClient>();

			hostsClient.Setup(hc => hc.HostName).Returns(new ServiceHostName(host));
			hostsCache.Setup(hc => hc.AvailableServiceHosts).Returns(new List<IServiceHostClient>() { hostsClient.Object });
			return hostsCache.Object;
		}

		protected override void ProcessResponseCore(IServiceHostClient hostClient, string uri, Action<byte[]> action)
		{
			if (responseException != null)
			{
				throw responseException;
			}

			string result;
			if (uri.EndsWith("/", StringComparison.Ordinal))
			{
				result = "DBM_20070720.TXT\r\nLWK_20070720.TXT\r\nAD_20070720.TXT\r\nADC_20070720.TXT";
			}
			else if (uri.EndsWith("lwk", StringComparison.Ordinal))
			{
				result = "LWK_20070720.TXT";
			}
			else if (uri.EndsWith("ad", StringComparison.Ordinal))
			{
				result = "AD_20070720.TXT\r\nADC_20070720.TXT";
			}
			else if (uri.EndsWith("ts1", StringComparison.Ordinal))
			{
				result = "";
			}
			else if (uri.EndsWith("###", StringComparison.Ordinal))
			{
				throw new WebException("TestEx!");
			}
			else if (uri.EndsWith("LWK_20070720.TXT", StringComparison.Ordinal))
			{
				var buffer = new LocalLogViewerDataProvider(testLogsDirectory, "LWK").GetBytes("LWK_20070720.TXT");
				action(buffer);
				return;
			}
			else if (uri.EndsWith("AD_20070720.TXT", StringComparison.Ordinal))
			{
				var buffer = new LocalLogViewerDataProvider(testLogsDirectory, "AD").GetBytes("AD_20070720.TXT");
				action(buffer);
				return;
			}
			else if (uri.EndsWith("DBM_20070720.TXT", StringComparison.Ordinal))
			{
				var buffer = new LocalLogViewerDataProvider(testLogsDirectory, "DBM").GetBytes("DBM_20070720.TXT");
				action(buffer);
				return;
			}
			else
			{
				throw new NotSupportedException("Unknown request");
			}

			action(Encoding.ASCII.GetBytes(result));
		}

		readonly string testLogsDirectory;
	}
}
