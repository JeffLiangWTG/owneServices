using System;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http.RequestHandlers
{
	[TestedType(typeof(ServiceHostGetRequestHandler))]
	class ServiceHostGetRequestHandlerTest : RequestHandlerBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			handler = new ServiceHostGetRequestHandler(new Mock<IServiceHostGetRequest>().Object, new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/get"));
		}

		protected override Uri GetExpectedUri()
		{
			return new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/get");
		}
	}
}
