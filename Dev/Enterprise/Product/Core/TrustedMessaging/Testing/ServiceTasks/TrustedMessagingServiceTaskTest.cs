using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Business;
using Enterprise.TrustedMessaging.ServiceTasks;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Testing
{
	[UseSnapshotProtection]
	[TestedType(typeof(TrustedMessagingServiceTask))]
	public class TrustedMessagingServiceTaskTest : ServiceTaskTestCase<TrustedMessagingServiceTask>
	{
		public void TestRunTask()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;
				reg.KeyForTest.PasswordForTest = "123";
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value.Any());
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value.Any());
				AssertEquals("", WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value);

				var serverCert = UserPortalClientConfigurationTest.NewCertificate();
				var clientCert = UserPortalClientConfigurationTest.NewCertificate();

				var process = new TrustedMessagingServiceTaskForTest();
				process.ApiServiceForTest = new RemoteApiServiceForTest()
				{
					CertificatePairResponseForTest = new TrustedResponse<CertificatePair>()
					{
						Success = true,
						Response = new CertificatePair() { RemoteCertificate = serverCert, LocalCertificate = clientCert },
					}
				};

				process.ApiServiceForTest.SendRequestCoreResponseForTest = new TrustedResponse<FeatureControlResponse>()
				{
					Success = true,
					Response = new FeatureControlResponse() { RuleTimestampUtc = DateTime.MinValue } // no rules yet
				};

				var logger = new TestServiceLogger();
				process.ServiceLogger = logger;

				logger.ClearLog();
				process.RunTask();

				var logs = $@"Debug|Service task has started.
Debug|Starting Certificates download...
Information|Certificates Downloaded.
Debug|Starting Feature Control download...
Information|Feature control rules are already up to date; skipping download.
Debug|Service task has completed.
";
				AssertEquals(logs, logger.ToString());

				var pair = new UserPortalClientConfiguration().CertificatePairProvider.GetCertificatePair("CW1");
				AssertEquals(serverCert.Thumbprint, pair.RemoteCertificate.Thumbprint);
				AssertEquals(clientCert.Thumbprint, pair.LocalCertificate.Thumbprint);
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		public void TestRunTask_Timeout()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;
				reg.KeyForTest.PasswordForTest = "123";
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value.Any());
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value.Any());
				AssertEquals("", WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value);

				var serverCert = UserPortalClientConfigurationTest.NewCertificate();
				var clientCert = UserPortalClientConfigurationTest.NewCertificate();

				var process = new TrustedMessagingServiceTaskForTest() { TaskTimeoutForTest = TimeSpan.FromSeconds(1) };
				process.ApiServiceForTest = new RemoteApiServiceForTimeoutTest()
				{
					CertificatePairResponseForTest = new TrustedResponse<CertificatePair>()
					{
						Success = true,
						Response = new CertificatePair() { RemoteCertificate = serverCert, LocalCertificate = clientCert },
					}
				};

				process.ApiServiceForTest.SendRequestCoreResponseForTest = new TrustedResponse<FeatureControlResponse>()
				{
					Success = true,
					Response = new FeatureControlResponse() { RuleTimestampUtc = DateTime.MinValue } // no rules yet
				};

				var logger = new TestServiceLogger();
				process.ServiceLogger = logger;

				logger.ClearLog();
				process.RunTask();

				var logs = $@"Debug|Service task has started.
Debug|Starting Certificates download...
Information|Certificates Downloaded.
Debug|Starting Feature Control download...
Error|Task was canceled or timed out.
Debug|Service task has completed.
";
				AssertEquals(logs, logger.ToString());
			}
			finally
			{
				reg.ResetKeyToDefault();
				Task.Delay(TimeSpan.FromSeconds(3)).Wait(); //make sure the ApiServiceForTest is finished.
			}
		}

		public void TestRunTask_EmptyCertificates()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			try
			{
				reg.KeyForTest.DatabaseNumberForTest = 100;
				reg.KeyForTest.PasswordForTest = "123";
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value.Any());
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value.Any());
				AssertEquals("", WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value);

				var process = new TrustedMessagingServiceTaskForTest();
				process.ApiServiceForTest = new RemoteApiServiceForTest()
				{
					CertificatePairResponseForTest = new TrustedResponse<CertificatePair>()
					{
						Success = false,
						Response = null,
						Messages = new List<ErrorMessage>() { new ErrorMessage() { Code = "100", Message = "something wrong~" } }
					}
				};

				var logger = new TestServiceLogger();
				process.ServiceLogger = logger;

				logger.ClearLog();
				process.RunTask();

				process.ApiServiceForTest = new RemoteApiServiceForTest()
				{
					CertificatePairResponseForTest = new TrustedResponse<CertificatePair>()
					{
						Success = false,
						Response = null,
						Messages = new List<ErrorMessage>() { new ErrorMessage() { Code = WTG.TrustedMessaging.Constants.ErrorCodes.SecretKeyNotUpToDate, Message = "SecretKeyNotUpToDate" } }
					}
				};
				process.RunTask();

				var logs = $@"Debug|Service task has started.
Debug|Starting Certificates download...
Error|100 : something wrong~
Debug|Service task has completed.
Debug|Service task has started.
Debug|Starting Certificates download...
Warning|1002 : SecretKeyNotUpToDate
Debug|Service task has completed.
";
				AssertEquals(logs, logger.ToString());

				var pair = new UserPortalClientConfiguration().CertificatePairProvider.GetCertificatePair("CW1");
				AssertEquals(null, pair.RemoteCertificate);
				AssertEquals(null, pair.LocalCertificate);
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingCentralSystemCertificate.Value.Any());
				AssertEquals(false, WebDataRegistry.Instance.TrustedMessagingClientSystemCertificate.Value.Any());
				AssertEquals("", WebDataRegistry.Instance.TrustedMessagingClientSystemCertificatePassword.Value);
			}
			finally
			{
				reg.ResetKeyToDefault();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		class TrustedMessagingServiceTaskForTest : TrustedMessagingServiceTask
		{
			public RemoteApiServiceForTest ApiServiceForTest { get; set; }
			public TimeSpan TaskTimeoutForTest { get; set; } = TimeSpan.FromMinutes(5);

			protected override RemoteApiService NewRemoteApiService() => ApiServiceForTest;

			protected override TimeSpan TaskTimeout => TaskTimeoutForTest;
		}

		class RemoteApiServiceForTimeoutTest : RemoteApiServiceForTest
		{
			protected override Task<TrustedResponse<TResponse>> SendRequestCore<TRequest, TResponse>(TRequest request, string product, string systemId, string relativeUrl, CancellationToken cancellationToken)
			{
				Task.Delay(TimeSpan.FromSeconds(2)).Wait();
				return base.SendRequestCore<TRequest, TResponse>(request, product, systemId, relativeUrl, cancellationToken);
			}
		}
	}
}
