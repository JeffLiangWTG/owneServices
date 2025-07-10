using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.Foundation.Http;

namespace Enterprise.Customs.KR.ServiceTasks.Testing
{
	[TestedType(typeof(PublicKeyUpdateServiceTask))]
	sealed class PublicKeyUpdateServiceTaskTest : ServiceTaskTestCase<PublicKeyUpdateServiceTask>
	{
		public void TestHostedServiceAttributeParameters()
		{
			HostedServiceAttribute hostedServiceAttribute = GetHostedServiceAttributes().Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "KRP", hostedServiceAttribute.Code);
				AssertEquals("Description", "KR Customs Public Key Update", hostedServiceAttribute.Description);
				AssertEquals("Category", "KRC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1hour", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.KoreaSouth, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestRunTask_NoCompanyValidForMessaging()
		{
			var logs = InitialiseAndRunTaskSchedule(new PublicKeyUpdateServiceTask());

			CombineAssertions(() =>
			{
				AssertEquals("Did not run", 0, logs.Count);
				AssertEquals("No Valid KR Messaging Companies", 0, KRCustomsRegistry.Instance.CustomsCertificate.Value.Length);
			});
		}

		public void TestRunTask_CompaniesValidForMessaging()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			Factory.Save();
			var logs = RunServiceTask(PublicKey, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 1, logs.Count);
				AssertContains("KR Customs public key has successfully updated.", logs[0]);
			});
		}

		public void TestRunTask_DownloadedFailed()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			OutBoundServiceTaskTest.SetupCustomsRegistryCertificate();
			Factory.Save();
			var logs = RunServiceTask(null, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 0, logs.Count);
				AssertEquals("If downloading fails, it should not set an empty value to this registry", OutBoundServiceTaskTest.CertificateForTest, KRCustomsRegistry.Instance.CustomsCertificate.Value);
			});
		}

		public void TestRunTask_DownloadedSameCertificate()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			OutBoundServiceTaskTest.SetupCustomsRegistryCertificate();
			Factory.Save();
			var logs = RunServiceTask(System.Text.Encoding.UTF8.GetString(OutBoundServiceTaskTest.CertificateForTest), ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 0, logs.Count);
				AssertEquals("We should not update the certificate as it is the same", OutBoundServiceTaskTest.CertificateForTest, KRCustomsRegistry.Instance.CustomsCertificate.Value);
			});
		}

		public void TestRunTask_StartDateIsNotValid()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			Factory.Save();
			var logs = RunServiceTask(PublicKey, ZDateTime.Invalid, ZDateTime.Today.AddDays(10));

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 2, logs.Count);
				AssertContains("KR Customs public key has successfully updated.", logs[0]);
				AssertContains("There is something wrong with the KR Customs Public Key and the start and/or expiry date are invalid.", logs[1]);
			});
		}

		public void TestRunTask_EndDateIsNotValid()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			Factory.Save();
			var logs = RunServiceTask(PublicKey, ZDateTime.Today.AddDays(-10), ZDateTime.Invalid);

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 2, logs.Count);
				AssertContains("KR Customs public key has successfully updated.", logs[0]);
				AssertContains("There is something wrong with the KR Customs Public Key and the start and/or expiry date are invalid.", logs[1]);
			});
		}

		public void TestRunTask_FutureDate()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			Factory.Save();
			var logs = RunServiceTask(PublicKey, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(10));

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 2, logs.Count);
				AssertContains("KR Customs public key has successfully updated.", logs[0]);
				AssertContains("The KR Customs Public Key Start Date is still in the future and is not valid to use yet.", logs[1]);
			});
		}

		public void TestRunTask_PastDate()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			Factory.Save();
			var logs = RunServiceTask(PublicKey, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(-1));

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 2, logs.Count);
				AssertContains("KR Customs public key has successfully updated.", logs[0]);
				AssertContains("KR Customs Public Key is already expired.", logs[1]);
			});
		}

		public void TestRunTask_Left1Days()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			Factory.Save();
			var logs = RunServiceTask(PublicKey, ZDateTime.Today.AddDays(-10), ZDateTime.Today);

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 2, logs.Count);
				AssertContains("KR Customs public key has successfully updated.", logs[0]);
				AssertContains("KR customs public key will expire tomorrow.", logs[1]);
			});
		}

		public void TestRunTask_Left7Days()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			OutBoundServiceTaskTest.SetupCompanyCredentials(Factory);
			Factory.Save();
			var logs = RunServiceTask(PublicKey, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(3));

			CombineAssertions(() =>
			{
				AssertEquals("Log Count", 2, logs.Count);
				AssertContains("KR Customs public key has successfully updated.", logs[0]);
				AssertContains("KR Customs Public Key is about to be expired in the next 7 days.", logs[1]);
			});
		}

		public void TestGetCustomsPublicKeyPassed()
		{
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

			var httpResponseMessage = new HttpResponseMessage()
			{
				Content = new StringContent("Your response text")
			};

			Expression<Func<HttpRequestMessage, bool>> matchUri = x => x.RequestUri == new Uri("https://api.test.com/endpoint");
			Expression<Func<CancellationToken, bool>> anyToken = x => true;

			// SendAsync(matchUri, anyToken) will return a canned response message.
			httpMessageHandlerMock
				.Protected() // required as SendAsync is protected
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is(matchUri), ItExpr.Is(anyToken))
				.ReturnsAsync(httpResponseMessage);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			httpClientFactory.Setup(x => x.Create())
				.Returns(new HttpClient(httpMessageHandlerMock.Object));

			using var temp = ObjectFactory.Substitute(httpClientFactory.Object);

			var serviceTask = new PublicKeyUpdateServiceTask();

			var result = serviceTask.GetCustomsPublicKey("https://api.test.com/endpoint");
			Assert(result == "Your response text");

			// Verifying that SendAsync(matchUri, anyToken) was called exactly once.
			httpMessageHandlerMock
				.Protected()
				.Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.Is(matchUri), ItExpr.Is(anyToken));

			// Verifying that IHttpClientFactory.Create() was called exactly once.
			httpClientFactory.Verify(x => x.Create(), Times.Once);
		}

		public void TestGetCustomsPublicKeyFailed()
		{
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

			var httpResponseMessage = new HttpResponseMessage()
			{
				Content = new StringContent("This operation is forbidden"),
				StatusCode = HttpStatusCode.Forbidden
			};

			Expression<Func<HttpRequestMessage, bool>> matchUri = x => x.RequestUri == new Uri("https://api.test.com/forbidden");
			Expression<Func<CancellationToken, bool>> anyToken = x => true;

			// SendAsync(matchUri, anyToken) will return a canned response message.
			httpMessageHandlerMock
				.Protected() // required as SendAsync is protected.
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is(matchUri), ItExpr.Is(anyToken))
				.ReturnsAsync(httpResponseMessage);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			httpClientFactory.Setup(x => x.Create())
				.Returns(new HttpClient(httpMessageHandlerMock.Object));

			using var temp = ObjectFactory.Substitute(httpClientFactory.Object);

			var serviceTask = new PublicKeyUpdateServiceTask();

			try
			{
				serviceTask.GetCustomsPublicKey("https://api.test.com/forbidden");
				Assert("HttpRequestException expected", false);
			}
			catch (HttpRequestException e)
			{
				Assert("Exception message mismatch", e.GetFullMessage() == "Response status code does not indicate success: 403 (Forbidden).");
			}

			// Verifying that SendAsync(matchUri, anyToken) was called exactly once.
			httpMessageHandlerMock
				.Protected()
				.Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.Is(matchUri), ItExpr.Is(anyToken));

			// Verifying that IHttpClientFactory.Create() was called exactly once.
			httpClientFactory.Verify(x => x.Create(), Times.Once);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		internal static readonly byte[] DummyCertificate = new byte[] { 48, 130, 8 };

		TestServiceLogger RunServiceTask(string getCustomsPublicKeyReturnValue, ZDateTime startDate, ZDateTime expiryDate)
		{
			var checkCustomsPublicKeyMock = new Mock<CustomsPublicKeyValidityChecker>();
			checkCustomsPublicKeyMock.Setup(x => x.GetCustomsPublicKeyStartAndExpiryDates()).Returns((startDate, expiryDate));
			checkCustomsPublicKeyMock.CallBase = true;

			var keyUpdateTaskMock = new Mock<PublicKeyUpdateServiceTask>();
			keyUpdateTaskMock.Setup(x => x.GetCustomsPublicKeyValidityChecker()).Returns(checkCustomsPublicKeyMock.Object);
			keyUpdateTaskMock.CallBase = true;

			using var temp = ObjectFactory.Substitute(MakeSubstituteFactory(getCustomsPublicKeyReturnValue).Object);

			return InitialiseAndRunTaskSchedule(keyUpdateTaskMock.Object);
		}

		Mock<IHttpClientFactory> MakeSubstituteFactory(string getCustomsPublicKeyReturnValue)
		{
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

			var httpResponseMessage = new HttpResponseMessage();
			if (getCustomsPublicKeyReturnValue != null)
			{
				httpResponseMessage.Content = new StringContent(getCustomsPublicKeyReturnValue);
			}

			Expression<Func<CancellationToken, bool>> anyToken = x => true;

			// SendAsync(matchUri, anyToken) will return a canned response message.
			httpMessageHandlerMock
				.Protected() // required as SendAsync is protected
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.Is(anyToken))
				.ReturnsAsync(httpResponseMessage);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			httpClientFactory.Setup(x => x.Create())
				.Returns(new HttpClient(httpMessageHandlerMock.Object));

			return httpClientFactory;
		}

		public const string PublicKey = @"-----BEGIN CERTIFICATE-----
	MIIFOTCCBCGgAwIBAgIEBjuDxjANBgkqhkiG9w0BAQsFADBKMQswCQYDVQQGEwJL
	UjENMAsGA1UECgwES0lDQTEVMBMGA1UECwwMQWNjcmVkaXRlZENBMRUwEwYDVQQD
	DAxzaWduR0FURSBDQTUwHhcNMjIwNDI4MDA0OTUwWhcNMjMwNTExMTQ1OTU5WjB7
	MQswCQYDVQQGEwJLUjENMAsGA1UECgwES0lDQTETMBEGA1UECwwKbGljZW5zZWRD
	QTEPMA0GA1UECwwGU0VSVkVSMQ0wCwYDVQQLDARLSUNBMQ8wDQYDVQQLDAZTRVJW
	RVIxFzAVBgNVBAMMDjIxMS4xNzMuMzQuMTQ4MIIBIjANBgkqhkiG9w0BAQEFAAOC
	AQ8AMIIBCgKCAQEAySoyhkKkp4f7qT5fC97Z/VPt0iqAwQ9WzDuxSiWpY0hva4yW
	MOE3iSA969dT894lTEIJOm1vDMH/tI52Hi217xWs5qYfHceKRNKZT5wI4k1ElkBK
	Wh0KLTMta6IWsgiVu+5Atg1aV4tpZ0bH5B3mdTzqd1pGyweYZqZIzg82Y/yiawyg
	yNZXAP9RUjSW9T4gTgzDDfRccwrqYgck+KXsp/wpspDamcb/2rykvE7gOfhVuIv+
	B651n7bpKfkAIZoBVehYgOxdNv62J/dEBss10F1kVO4bAHg0g3BMzQedR1qvpv5w
	/iCm/ZGTScu+tjGoZRAd3i5SeslP92+hDy5BTwIDAQABo4IB9DCCAfAwgY8GA1Ud
	IwSBhzCBhIAU2L467EWZxZ7jnOqBH9IdErA2PoihaKRmMGQxCzAJBgNVBAYTAktS
	MQ0wCwYDVQQKDARLSVNBMS4wLAYDVQQLDCVLb3JlYSBDZXJ0aWZpY2F0aW9uIEF1
	dGhvcml0eSBDZW50cmFsMRYwFAYDVQQDDA1LSVNBIFJvb3RDQSA0ggIQHTAdBgNV
	HQ4EFgQUeYx6DoOOuKM0wIYiSgtbbdlJLaAwDgYDVR0PAQH/BAQDAgUgMBcGA1Ud
	IAQQMA4wDAYKKoMajJpEBQIBBDBtBgNVHREEZjBkoGIGCSqDGoyaRAoBAaBVMFMM
	DjIxMS4xNzMuMzQuMTQ4MEEwPwYKKoMajJpECgEBATAxMAsGCWCGSAFlAwQCAaAi
	BCBh9XV1a4gjc0mohSA3gi3q1PRbuXUO1v6Ex8BjySiYnzBfBgNVHR8EWDBWMFSg
	UqBQhk5sZGFwOi8vbGRhcC5zaWduZ2F0ZS5jb206Mzg5L291PWRwN3AzNzU2NCxv
	dT1jcmxkcCxvdT1BY2NyZWRpdGVkQ0Esbz1LSUNBLGM9S1IwRAYIKwYBBQUHAQEE
	ODA2MDQGCCsGAQUFBzABhihodHRwOi8vb2NzcC5zaWduZ2F0ZS5jb206OTAyMC9P
	Q1NQU2VydmVyMA0GCSqGSIb3DQEBCwUAA4IBAQB0FZIQKuLQeuy/2OFkxa+TZLVz
	cvOt2+DpWvG0cLmxQlPiEArIHSCMdsnhoHhNckM9eTv1UTMrGY6X3vblT3+jd0aK
	1ARHAjjZC3Gw/ifnTKJwlPL+iOh8Ae+TTLdb/1HdcQ4sgrrmJKRzoQWz8gk7IcsO
	+JjgBlR+LoxNJ+pu5+n2C2wQ6OvhkB21pHAdJ9obw53QMe6+U7SyVcYxpeoVxZDR
	J5apFe2eTp1lEfB0HDq7N81ECbxX5+pjiA7WRxt82PmznbrTGE9mhQpZSQ3mlzww
	S1lEQyVOCsWGuKrWFt30QtbCLr2YrrAFLWwnjSbTBrym1GkvK88MIaf+A+YD
	-----END CERTIFICATE-----";
	}
}
