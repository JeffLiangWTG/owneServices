using System;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http.RequestHandlers
{
	[TestedType(typeof(IsActiveRequestHandler))]
	class IsActiveRequestHandlerTest : RequestHandlerBaseTest
	{
		Mock<IHostServiceStatusProvider> mockHostServiceStatusProvider;
		Mock<IHttpRequestInfo> mockRequestInfo;

		protected override void SetUp()
		{
			base.SetUp();
			mockHostServiceStatusProvider = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
			mockRequestInfo = new Mock<IHttpRequestInfo>(MockBehavior.Strict);
			handler = new IsActiveRequestHandler(mockHostServiceStatusProvider.Object, new JsonNetConverter(), string.Empty);
		}

		protected override void TearDown()
		{
			mockHostServiceStatusProvider.VerifyNoOtherCalls();
			mockRequestInfo.VerifyNoOtherCalls();
		}

		protected override Uri GetExpectedUri()
		{
			return new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/isAlive");
		}

		[ExpectNoExceptions]
		public void TestHandle_CallsHostServiceStatusProviderIsReady_Success()
		{
			mockHostServiceStatusProvider.Setup(x => x.IsReady()).Returns(true);
			mockRequestInfo = new Mock<IHttpRequestInfo>(MockBehavior.Strict);
			var result = handler.Handle(mockRequestInfo.Object);
			NUnit.Framework.Assert.That(result, Is.EqualTo("true"));
			mockHostServiceStatusProvider.Verify(x => x.IsReady(), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestHandle_CallsHostServiceStatusProviderIsReady_Failure()
		{
			mockHostServiceStatusProvider.Setup(x => x.IsReady()).Returns(false);
			mockRequestInfo = new Mock<IHttpRequestInfo>(MockBehavior.Strict);
			var result = handler.Handle(mockRequestInfo.Object);
			NUnit.Framework.Assert.That(result, Is.EqualTo("false"));
			mockHostServiceStatusProvider.Verify(x => x.IsReady(), Times.Once);
		}
	}
}
