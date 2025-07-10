using System;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http.RequestHandlers
{
	[TestedType(typeof(ServiceHostPostRequestHandler))]
	class ServiceHostPostRequestHandlerTest : RequestHandlerBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			handler = new ServiceHostPostRequestHandler(new Mock<IServiceHostPostRequest>().Object, Mock.Of<IJsonConverter>(), new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/post"));
		}

		protected override Uri GetExpectedUri()
		{
			return new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/post");
		}
	}
}
