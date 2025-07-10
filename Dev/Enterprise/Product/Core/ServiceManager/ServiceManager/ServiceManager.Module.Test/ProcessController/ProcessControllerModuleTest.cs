using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Async;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Module.Testing
{
	[TestedType(typeof(ProcessControllerModule))]
	class ProcessControllerModuleTest : ZModuleBasherTest, IDisposable
	{
		public ProcessControllerModuleTest()
			: base()
		{
			var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
			serviceHostsCacheMock
				.Setup(c => c.ConfiguredServiceHosts)
				.Returns(Enumerable.Empty<IServiceHostClient>());
			serviceHostsCacheOverride = ObjectFactory.Substitute<IServiceHostsCache>(serviceHostsCacheMock.Object);
		}

		protected override void TearDown()
		{
			AsyncHelper.WaitAllActiveTasksForTest();
			base.TearDown();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ProcessController;
		}

		public void Dispose()
		{
			serviceHostsCacheOverride.Dispose();
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => true;
		readonly IDisposable serviceHostsCacheOverride;
	}
}
