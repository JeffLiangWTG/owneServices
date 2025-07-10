using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceHostClient;
using ServiceManager.Integration.ServiceHostClient.Helpers;
using WTG.Foundation.Http;

namespace ServiceManager.Shared.CW
{
	sealed class EDIServiceHostClientFactory : ServiceHostClientFactory, IDisposable
	{
		static volatile bool servicePointInitialized;

		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "External use of httpClientFactory")]
		public EDIServiceHostClientFactory()
			: this(new ServiceHostHttpClient(
				new ServiceHostErrorReporter(),
				TimeSpan.FromMilliseconds(SystemDataRegistry.Instance.ServiceHostHttpRequestTimeoutInMilliseconds.Value),
				jsonDeserializer: new JsonDeserializer(),
				httpClientFactory: new HttpClientFactory(() => new HttpClientHandler())))
		{
		}

		internal EDIServiceHostClientFactory(ServiceHostHttpClient serviceHostHttpClient) : base(serviceHostHttpClient)
		{
			this.serviceHostHttpClient = serviceHostHttpClient;
			if (!servicePointInitialized)
			{
				InitializeServicePointParameters();
				servicePointInitialized = true;
			}
		}

		static void InitializeServicePointParameters()
		{
			ServicePointManager.DefaultConnectionLimit = 10;
			ServicePointManager.UseNagleAlgorithm = false; // better for short connections as per : http://www.winsocketdotnetworkprogramming.com/xmlwebservicesaspnetworkprogramming11e.html
			ServicePointManager.Expect100Continue = false; // same as above comment
			ServicePointManager.CheckCertificateRevocationList = false;
		}

		public void Dispose()
		{
			serviceHostHttpClient.Dispose();
		}

		readonly ServiceHostHttpClient serviceHostHttpClient;
	}
}
