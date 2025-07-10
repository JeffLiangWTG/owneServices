using System;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core.RunnableTask
{
	class ServiceTaskInfoTest
	{
		[Test]
		public void TestWrongParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => _ = new ServiceTaskInfo(null));
				Assert.That(result.ParamName, Is.EqualTo("hostedServiceAttribute"));

				result = Assert.Throws<ArgumentNullException>(() => serviceTaskInfo.CheckSatisfiesRequirementsForCurrentBranch(null));
				Assert.That(result.ParamName, Is.EqualTo("activeCompanies"));
			});
		}

		[SetUp]
		public void SetUp()
		{
			hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
			serviceTaskInfo = new ServiceTaskInfo(hostedServiceConfigMock.Object);
		}

		Mock<IHostedServiceAttribute> hostedServiceConfigMock;
		ServiceTaskInfo serviceTaskInfo;
	}
}
