using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace ZClientEDI.Business.Test.TrustedMessaging
{
	public class EDICertRequestTest : TestCaseWithFactory
	{
		public void TestWebExceptionHandling()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			var message = "The remote server returned an error: (401) Unauthorized.";

			var responseUnauthorized = new Mock<HttpWebResponse>();
			responseUnauthorized.Setup(m => m.StatusCode).Returns(HttpStatusCode.Unauthorized);
			responseUnauthorized.Setup(m => m.StatusDescription).Returns(message);
			responseUnauthorized.Setup(m => m.GetResponseStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(message)));

			var encoder = new EdiCertRequestEncoderForTest();
			var exception = new WebException(message, null, WebExceptionStatus.UnknownError, responseUnauthorized.Object);
			var certRequest = new EDICertRequestForTest(encoder);
			certRequest.ExceptionOnExecuteRequest = exception;

			var databaseNumber = 8;
			var soapTemplate = EDIDataRegistry.Instance.MyAccountCertificateAuthoritySoapRequestTemplate.Value;
			var subjectName = FormattableString.Invariant($"LD_DatabaseNumber:{databaseNumber}");
			var credentials = new NetworkCredential("testAccount", "testPassword");
			var requestContext = new EdiCertRequestContext { SystemIdentifier = databaseNumber.ToString() };
			var rsa = new RSACryptoServiceProvider(4096);
			var pubKey = rsa.ExportCspBlob(false);

			ExceptionReporterTestListener.Instance.Clear();
			certRequest.TrySubmitSafe(soapTemplate, subjectName, pubKey, credentials, requestContext, out _);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertStartsWith("", "System.Net.WebException: The remote server returned an error: (401) Unauthorized.", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEndsWith("", $"System Identifier: {databaseNumber}", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals(typeof(WebException), ExceptionReporterTestListener.Instance.Last().GetType());
			AssertEquals("Could not complete certificate request", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestExceptionHandling()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			var message = "Unknown error occurred";

			var encoder = new EdiCertRequestEncoderForTest();
			var exception = new Exception(message);
			var certRequest = new EDICertRequestForTest(encoder);
			certRequest.ExceptionOnExecuteRequest = exception;

			var databaseNumber = 8;
			var soapTemplate = EDIDataRegistry.Instance.MyAccountCertificateAuthoritySoapRequestTemplate.Value;
			var subjectName = FormattableString.Invariant($"LD_DatabaseNumber:{databaseNumber}");
			var credentials = new NetworkCredential("testAccount", "testPassword");
			var requestContext = new EdiCertRequestContext { SystemIdentifier = databaseNumber.ToString() };
			var rsa = new RSACryptoServiceProvider();
			var pubKey = rsa.ExportCspBlob(false);

			ExceptionReporterTestListener.Instance.Clear();
			certRequest.TrySubmitSafe(soapTemplate, subjectName, pubKey, credentials, requestContext, out _);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertStartsWith("", "System.Exception: Unknown error occurred", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEndsWith("", $"System Identifier: {databaseNumber}", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals(typeof(Exception), ExceptionReporterTestListener.Instance.Last().GetType());
			AssertEquals("Could not complete certificate request", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestExceptionHandling_DoNotReport_TestSystem()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			var message = "Unknown error occurred";

			var encoder = new EdiCertRequestEncoderForTest();
			var exception = new Exception(message);
			var certRequest = new EDICertRequestForTest(encoder);
			certRequest.ExceptionOnExecuteRequest = exception;

			var databaseNumber = 8;
			var soapTemplate = EDIDataRegistry.Instance.MyAccountCertificateAuthoritySoapRequestTemplate.Value;
			var subjectName = FormattableString.Invariant($"LD_DatabaseNumber:{databaseNumber}");
			var credentials = new NetworkCredential("testAccount", "testPassword");
			var requestContext = new EdiCertRequestContext { SystemIdentifier = databaseNumber.ToString() };
			var rsa = new RSACryptoServiceProvider();
			var pubKey = rsa.ExportCspBlob(false);

			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals(false, certRequest.TrySubmitSafe(soapTemplate, subjectName, pubKey, credentials, requestContext, out _));

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(null, ExceptionReporterTestListener.Instance.LastOrDefault());
		}

		public void TestServicePointManagerProperties()
		{
			try
			{
				var template =
@"<s:Envelope xmlns:a='http://www.w3.org/2005/08/addressing' xmlns:s='http://www.w3.org/2003/05/soap-envelope'>
<s:Header>
	<a:To>http://127.0.0.1/something/not/real/a.asp</a:To>
</s:Header>
</s:Envelope>";
				var rsa = new RSACryptoServiceProvider();
				var pubKey = rsa.ExportCspBlob(false);

				var request = new EDICertRequestForTest();
				AssertEquals(SecurityProtocolType.SystemDefault, ServicePointManager.SecurityProtocol);
				request.TrySubmitSafe(template, "subject", pubKey, null, new EdiCertRequestContext() { SystemIdentifier = "123" }, out _);
				AssertEquals(SecurityProtocolType.SystemDefault, ServicePointManager.SecurityProtocol);
				AssertEquals(@"START:ExecuteRequest|ServicePointManager.SecurityProtocol=SystemDefault
CreateHttpWebRequest|ServicePointManager.SecurityProtocol=SystemDefault
END:ExecuteRequest|ServicePointManager.SecurityProtocol=SystemDefault
", request.Log.ToString());
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestRetry()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			var soapTemplate = EDIDataRegistry.Instance.MyAccountCertificateAuthoritySoapRequestTemplate.Value;
			var subjectName = "subject:123";
			var credentials = new NetworkCredential("testAccount", "testPassword");
			var requestContext = new EdiCertRequestContext { SystemIdentifier = "123" };
			var rsa = new RSACryptoServiceProvider(4096);
			var pubKey = rsa.ExportCspBlob(false);

			var certRequest = new EDICertRequestForRetryTest(new EdiCertRequestEncoderForTest());

			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals(true, certRequest.TrySubmitSafe(soapTemplate, subjectName, pubKey, credentials, requestContext, out _));
			AssertEquals(1, certRequest.ExecuteRequestCallCount);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			certRequest.ExecuteRequestCallCount = 0;
			certRequest.ExecuteRequestCallback = () => throw new Exception("something bad");
			AssertEquals(false, certRequest.TrySubmitSafe(soapTemplate, subjectName, pubKey, credentials, requestContext, out _));
			AssertEquals(3, certRequest.ExecuteRequestCallCount);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertStartsWith("", "System.Exception: something bad", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEndsWith("", $"System Identifier: {123}", Env.OutgoingMailManager.EmailsCreated[0].Body);
			ExceptionReporterTestListener.Instance.Clear();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			certRequest.ExecuteRequestCallCount = 0;
			certRequest.ExecuteRequestCallback = () =>
			{
				if (certRequest.ExecuteRequestCallCount < 2 )
				{
					throw new Exception("something really bad");
				}
			};
			AssertEquals(true, certRequest.TrySubmitSafe(soapTemplate, subjectName, pubKey, credentials, requestContext, out _));
			AssertEquals(2, certRequest.ExecuteRequestCallCount);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetUpEmail();
		}

		void SetUpEmail()
		{
			var staffPMG = Factory.New<GlbStaff>();
			staffPMG.GS_Code = "PMG";
			staffPMG.GS_LoginName = "testUser";
			staffPMG.GS_IsSystemAccount = false;
			staffPMG.GS_EmailAddress = "testUser@test.com";
			Factory.Save();

			var groupPMG = Factory.Load<GlbGroup>(new ZQuery(Enterprise.ZArchitecture.Schema.GlbGroupSchema.GG_Code, "PMG")).FirstOrDefault();

			var result = Factory.New<GlbGroupLink>();
			result.GK_GG = groupPMG.PK;
			result.GK_GS = staffPMG.PK;

			Factory.Save();

			EnvProxy.Instance.Registry.MailboxEmailAddress = "test@example.com";
		}

		class EdiCertRequestEncoderForTest : IEDICertRequestEncoder
		{
			public string Decode(string response, string subjectName)
			{
				return string.Empty;
			}

			public string Encode(string requestTemplate, string subjectName, byte[] keyBlob)
			{
				return string.Empty;
			}
		}

		class EDICertRequestForTest : EDICertRequest
		{
			public EDICertRequestForTest(IEDICertRequestEncoder requestEncoder) : base(requestEncoder) { }

			public EDICertRequestForTest()
			{
			}

			protected override string ExecuteRequest(string caServer, ICredentials credentials, string soapRequestXml)
			{
				if (ExceptionOnExecuteRequest != null)
				{
					throw ExceptionOnExecuteRequest;
				}

				try
				{
					Log.AppendLine($"START:ExecuteRequest|ServicePointManager.SecurityProtocol={ServicePointManager.SecurityProtocol}");
					return base.ExecuteRequest(caServer, credentials, soapRequestXml);
				}
				finally
				{
					Log.AppendLine($"END:ExecuteRequest|ServicePointManager.SecurityProtocol={ServicePointManager.SecurityProtocol}");
				}
			}

			protected override HttpWebRequest CreateHttpWebRequest(Uri uri)
			{
				Log.AppendLine($"CreateHttpWebRequest|ServicePointManager.SecurityProtocol={ServicePointManager.SecurityProtocol}");
				return base.CreateHttpWebRequest(uri);
			}

			public Exception ExceptionOnExecuteRequest { get; set; }

			public readonly StringBuilder Log = new StringBuilder();

			protected override void SleepOnRetry() { }

			protected override int MaxTryCount => 1;
		}

		class EDICertRequestForRetryTest : EDICertRequestForTest
		{
			public EDICertRequestForRetryTest(IEDICertRequestEncoder requestEncoder) : base(requestEncoder)
			{
			}

			public EDICertRequestForRetryTest()
			{
			}

			public int ExecuteRequestCallCount { get; set; }
			public Action ExecuteRequestCallback { get; set; }
			protected override int MaxTryCount => 3;
			protected override void SleepOnRetry() => Thread.Sleep(1);
			protected override string ExecuteRequest(string caServer, ICredentials credentials, string soapRequestXml)
			{
				ExecuteRequestCallCount++;
				ExecuteRequestCallback?.Invoke();
				return "OK";
			}
		}
	}
}
