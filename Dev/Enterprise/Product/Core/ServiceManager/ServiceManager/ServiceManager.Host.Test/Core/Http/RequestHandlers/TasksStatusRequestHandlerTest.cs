using System;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http.RequestHandlers
{
	[TestedType(typeof(TasksStatusRequestHandler))]
	class TasksStatusRequestHandlerTest : RequestHandlerBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			handler = new TasksStatusRequestHandler(new Mock<ITaskStatusProvider>().Object, Mock.Of<IJsonConverter>(), string.Empty);
		}

		protected override Uri GetExpectedUri()
		{
			return new Uri($"http://localhost:7070/cargowise/processController/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/status");
		}
	}
}
