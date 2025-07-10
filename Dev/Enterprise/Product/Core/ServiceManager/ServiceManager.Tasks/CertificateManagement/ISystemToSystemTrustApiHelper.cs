using System;
using System.Net.Http;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	interface ISystemToSystemTrustApiHelper
	{
		SystemToSystemTrustApiResponse SystemToSystemTrustApiGet(string relativeUri,
			string token = null);

		SystemToSystemTrustApiResponse SystemToSystemTrustApiPost(string relativeUri,
			StringContent stringContent,
			string token = null);

		TimeSpan SecondsDelayedBetweenRequests { get; }

		string SystemToSystemTrustApiEndpoint { get; }
	}
}
