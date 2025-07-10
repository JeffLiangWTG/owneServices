using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Testing
{
	public class RemoteApiServiceTest : TestCaseWithFactory
	{
		public void TestDownloadCertificates()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;
				reg.KeyForTest.PasswordForTest = "123";

				var client = new RemoteApiServiceForTest();
				client.CertificatePairResponseForTest = new TrustedResponse<CertificatePair>()
				{
					Success = false,
					Messages = new[] { new ErrorMessage() { Code = "500", Message = "Something bad" } },
				};

				var rsp = client.DownloadCertificatesAsync(CancellationToken.None).Result;
				AssertEquals(false, rsp.Success);
				AssertEquals(false, rsp.Response);
				AssertEquals("Something bad", rsp.Messages.Single().Message);

				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value.Any());
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value.Any());
				AssertEquals("", WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value);

				var serverCert = UserPortalClientConfigurationTest.NewCertificate();
				var clientCert = UserPortalClientConfigurationTest.NewCertificate();
				client.CertificatePairResponseForTest = new TrustedResponse<CertificatePair>()
				{
					Success = true,
					Response = new CertificatePair() { RemoteCertificate = serverCert, LocalCertificate = clientCert },
				};

				rsp = client.DownloadCertificatesAsync(CancellationToken.None).Result;
				AssertEquals(true, rsp.Success);
				AssertEquals(true, rsp.Response);
				var pair = new UserPortalClientConfiguration().CertificatePairProvider.GetCertificatePair("CW1");
				AssertEquals(serverCert.Thumbprint, pair.RemoteCertificate.Thumbprint);
				AssertEquals(clientCert.Thumbprint, pair.LocalCertificate.Thumbprint);
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		public void TestDownloadCertificates_Developer()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.EnterpriseCodeForTest = "WTL";
				reg.KeyForTest.DatabaseNumberForTest = 100;
				reg.KeyForTest.PasswordForTest = "123";

				var client = new RemoteApiServiceForTest();
				var serverCert = UserPortalClientConfigurationTest.NewCertificate();
				var clientCert = UserPortalClientConfigurationTest.NewCertificate();
				client.CertificatePairResponseForTest = new TrustedResponse<CertificatePair>()
				{
					Success = true,
					Response = new CertificatePair() { RemoteCertificate = serverCert, LocalCertificate = clientCert },
				};

				var rsp = client.DownloadCertificatesAsync(CancellationToken.None).Result;
				AssertEquals(true, rsp.Success);
				AssertEquals(true, rsp.Response);
				var pair = new UserPortalClientConfiguration().CertificatePairProvider.GetCertificatePair("CW1");
				AssertEquals(serverCert.Thumbprint, pair.RemoteCertificate.Thumbprint);
				AssertEquals(clientCert.Thumbprint, pair.LocalCertificate.Thumbprint);
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		[TestDate(2024, 7, 1)]
		public void TestDownloadFeatureControlRuleAsync()
		{
			var utcNow = DateTime.SpecifyKind(ZDateTime.UtcNow.ToDateTime(), DateTimeKind.Utc);
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			var logger = new TestServiceLogger();
			var timeProvider = new FeatureControlManagerDateTimeProvider();
			timeProvider.CurrentUtcDateTimeOverride = utcNow;

			var manager = new FeatureControlManager(ObjectFactory.Get<IReadOnlyFeatureControlStorage>(), null, timeProvider);
			AssertNull(manager.GetFeatureData("CR5RESWIZ"));

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;
				reg.KeyForTest.PasswordForTest = "123";

				var client = new RemoteApiServiceForTest();
				client.SendRequestCoreResponseForTest = new TrustedResponse<FeatureControlResponse>()
				{
					Success = true,
					Response = new FeatureControlResponse() { RuleTimestampUtc = DateTime.MinValue } // no rules yet
				};

				var rsp = client.DownloadFeatureControlRuleAsync(logger, CancellationToken.None).Result;
				AssertEquals(true, rsp.Success);
				AssertEquals(true, rsp.Response);

				client.SendRequestCoreResponseForTest = new TrustedResponse<FeatureControlResponse>()
				{
					Success = true,
					Response = new FeatureControlResponse() //new rules
					{ RuleTimestampUtc = utcNow, RuleContent = CompressString(FeatureControlXml) }
				};
				rsp = client.DownloadFeatureControlRuleAsync(logger, CancellationToken.None).Result;
				AssertEquals(true, rsp.Success);
				AssertEquals(true, rsp.Response);

				client.SendRequestCoreResponseForTest = new TrustedResponse<FeatureControlResponse>()
				{
					Success = true,
					Response = new FeatureControlResponse()
					{ RuleTimestampUtc = DateTime.SpecifyKind(new ZDate(2024, 7, 1).ToDateTime(), DateTimeKind.Utc) }  //no new rules, skip
				};
				rsp = client.DownloadFeatureControlRuleAsync(logger, CancellationToken.None).Result;
				AssertEquals(true, rsp.Success);
				AssertEquals(true, rsp.Response);

				client.SendRequestCoreResponseForTest = new TrustedResponse<FeatureControlResponse>()
				{
					Success = true,
					Response = new FeatureControlResponse()
					{ RuleTimestampUtc = new ZDate(2024, 8, 1).ToDateTime(), RuleContent = CompressString("<a>a</a>") }  //corrupted data
				};
				rsp = client.DownloadFeatureControlRuleAsync(logger, CancellationToken.None).Result;
				AssertEquals(true, rsp.Success);
				AssertEquals(true, rsp.Response);
				AssertContains("There is an error in XML document (1, 2).", ErrorReporter.LastExceptionReported.Message);

				client.SendRequestCoreResponseForTest = new TrustedResponse<FeatureControlResponse>()
				{
					Success = true,
					Response = new FeatureControlResponse()
					{ RuleTimestampUtc = DateTime.SpecifyKind(new ZDate(2024, 7, 1).ToDateTime(), DateTimeKind.Utc) }  //no new rules, skip
				};
				rsp = client.DownloadFeatureControlRuleAsync(logger, CancellationToken.None).Result;
				AssertEquals(true, rsp.Success);
				AssertEquals(true, rsp.Response);

				//bad response
				var badRsp = new TrustedResponse<FeatureControlResponse>("123", "something wrong~")
				{
					Success = false,
					Response = null
				};
				client.SendRequestCoreResponseForTest = badRsp;
				rsp = client.DownloadFeatureControlRuleAsync(logger, CancellationToken.None).Result;
				AssertEquals(false, rsp.Success);
				AssertEquals(false, rsp.Response);

				//bad response, log type = warning
				var badRspSecretKeyNotUpToDate = new TrustedResponse<FeatureControlResponse>(WTG.TrustedMessaging.Constants.ErrorCodes.SecretKeyNotUpToDate, "SecretKeyNotUpToDate")
				{
					Success = false,
					Response = null
				};
				client.SendRequestCoreResponseForTest = badRspSecretKeyNotUpToDate;
				rsp = client.DownloadFeatureControlRuleAsync(logger, CancellationToken.None).Result;
				AssertEquals(false, rsp.Success);
				AssertEquals(false, rsp.Response);

				//bad response with null error messages
				var badRspNullMessages = new TrustedResponse<FeatureControlResponse>()
				{
					Success = false,
					Response = null,
					Messages = null
				};
				client.SendRequestCoreResponseForTest = badRspNullMessages;
				rsp = client.DownloadFeatureControlRuleAsync(logger, CancellationToken.None).Result;
				AssertEquals(false, rsp.Success);
				AssertEquals(false, rsp.Response);

#if NETFRAMEWORK
				AssertContains("rule=H4sIAAAAAAAEALNJtEu00U+0AwCj58g0CAAAAA==", ErrorReporter.LastMessageReported);

				AssertEquals(@"Information|Feature control rules are already up to date; skipping download.
Information|Successfully downloaded the latest feature control rules.
Information|Feature control rules are already up to date; skipping download.
Error|Encountered an invalid format in the feature control rules.
Information|Feature control rules are already up to date; skipping download.
Error|123 : something wrong~
Warning|1002 : SecretKeyNotUpToDate
", logger.ToString());
#elif NET
				AssertContains("rule=H4sIAAAAAAAACrNJtEu00U+0AwCj58g0CAAAAA==", ErrorReporter.LastMessageReported);

				AssertEquals(@"Information|Feature control rules are already up to date; skipping download.
Information|Successfully downloaded the latest feature control rules.
Information|Feature control rules are already up to date; skipping download.
Error|Encountered an invalid format in the feature control rules.
Information|Feature control rules are already up to date; skipping download.
Error|123 : something wrong~
Warning|1002 : SecretKeyNotUpToDate
", logger.ToString());
#endif

				manager = new FeatureControlManager(ObjectFactory.Get<IReadOnlyFeatureControlStorage>(), null, timeProvider);
				AssertEqualsIgnoreLineBreaks("", @"{
  ""UserSettings"": {
    ""Username"": ""john_doe_client_name"",
    ""Email"": ""john.doe@example.com"",
    ""Preferences"": {
      ""Theme"": ""dark"",
      ""Language"": ""en-US"",
      ""Notifications"": {
        ""Email"": true,
        ""SMS"": false
      }
    }
  }
}", manager.GetFeatureData("CR5RESWIZ").Parameter);

				ErrorReporter.Clear();
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		const string FeatureControlXml = @"<?xml version=""1.0""?>
<FeatureControl xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TimestampUtc>2024-07-01T00:00:00.000Z</TimestampUtc>
  <Rules>
    <Rule>
      <FCM_FeatureControlCode>CR5RESWIZ</FCM_FeatureControlCode>
      <FCR_RuleType>CLI</FCR_RuleType>
      <FCR_StartDateUtc>2024-07-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_Parameters>{
  ""UserSettings"": {
    ""Username"": ""john_doe_client_name"",
    ""Email"": ""john.doe@example.com"",
    ""Preferences"": {
      ""Theme"": ""dark"",
      ""Language"": ""en-US"",
      ""Notifications"": {
        ""Email"": true,
        ""SMS"": false
      }
    }
  }
}</FCR_Parameters>
    </Rule>
    <Rule>
      <FCM_FeatureControlCode>CR5RESWIZ</FCM_FeatureControlCode>
      <FCR_RuleType>GLB</FCR_RuleType>
      <FCR_StartDateUtc>2024-01-01T00:00:00Z</FCR_StartDateUtc>
      <FCR_EndDateUtc>2024-12-31T00:00:00Z</FCR_EndDateUtc>
      <FCR_Parameters>{
  ""UserSettings"": {
    ""Username"": ""john_doe_global_name"",
    ""Email"": ""john.doe@example.com"",
    ""Preferences"": {
      ""Theme"": ""dark"",
      ""Language"": ""en-US"",
      ""Notifications"": {
        ""Email"": true,
        ""SMS"": false
      }
    }
  }
}</FCR_Parameters>
    </Rule>
  </Rules>
</FeatureControl>";

		static byte[] CompressString(string input)
		{
			using (var ms = new MemoryStream())
			{
				using (var zipStream = new GZipStream(ms, CompressionMode.Compress))
				{
					var bytes = Encoding.UTF8.GetBytes(input);
					zipStream.Write(bytes, 0, bytes.Length);
				}
				return ms.ToArray();
			}
		}
	}

	public class RemoteApiServiceForTest : RemoteApiService
	{
		public RemoteApiServiceForTest() : base(new UserPortalClientConfiguration())
		{
		}

		public TrustedResponse<CertificatePair> CertificatePairResponseForTest { get; set; }
		public object SendRequestCoreResponseForTest { get; set; }

		protected async override Task<TrustedResponse<CertificatePair>> ActivationAsyncCore(string databaseNumber, string password, CancellationToken cancellationToken)
		{
			return await Task.FromResult(CertificatePairResponseForTest);
		}

		protected async override Task<TrustedResponse<TResponse>> SendRequestCore<TRequest, TResponse>(TRequest request, string product, string systemId, string relativeUrl, CancellationToken cancellationToken)
		{
			return await Task.FromResult((TrustedResponse<TResponse>)SendRequestCoreResponseForTest);
		}
	}

	class FeatureControlManagerDateTimeProvider : TimeProvider
	{
		public DateTime CurrentUtcDateTimeOverride { get; set; }

		public override DateTimeOffset GetUtcNow()
		{
			return CurrentUtcDateTimeOverride;
		}
	}
}
