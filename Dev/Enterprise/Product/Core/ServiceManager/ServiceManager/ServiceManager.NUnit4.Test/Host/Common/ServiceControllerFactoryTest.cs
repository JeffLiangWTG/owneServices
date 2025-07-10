using System;
using System.Diagnostics;
using System.Linq;
using Enterprise.Dat.Implementation.Testing;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Common;

sealed class ServiceControllerFactoryTest
{
	readonly string serviceName = $"dummyService_{Guid.NewGuid()}";

	[Test]
	public void TestGetServices_Localhost_ReturnServices()
	{
		var factory = new ServiceControllerFactory();
		var result = factory.GetServices("localhost");
		Assert.That(result.Count(), Is.GreaterThan(0));
	}

	[Test]
	public void TestGetServices_InvalidHost_Throw()
	{
		var factory = new ServiceControllerFactory();
		var e = Assert.Throws<InvalidOperationException>(() => factory.GetServices("NotAServiceHost"));
		Assert.That(e?.Message, Is.EqualTo("Cannot open Service Control Manager on computer 'NotAServiceHost'. This operation might require other privileges."));
	}

	[Test]
	[DatRequiresAdminPrivileges("Installation and uninstallation of the service")]
	public void TestGetServices_ReturnNewService()
	{
		var factory = new ServiceControllerFactory();
		try
		{
			CreateDummyService();
			var result = factory.GetServices("localhost");
			Assert.That(result, Has.Some.Matches<IServiceController>(sc => sc.ServiceName == serviceName));
		}
		finally
		{
			DeleteDummyService();
		}
	}

	void CreateDummyService()
	{
		var hostExePath = "DummyServiceHost.exe";
		using var process = Process.Start("sc", $"CREATE \"{serviceName}\" start=disabled binpath= \"{hostExePath}\"");
		var isStopped = process.WaitForExit(milliseconds: 1000);
		Assert.That(isStopped, Is.EqualTo(true), "Installation of the dummy service");
		Assert.That(process?.ExitCode, Is.EqualTo(0), "Installation of the dummy service");
	}

	void DeleteDummyService()
	{
		using var process = Process.Start("sc", $"DELETE \"{serviceName}\"");
		var isStopped = process.WaitForExit(milliseconds: 1000);
		Assert.That(isStopped, Is.EqualTo(true), "Removal of the dummy service");
		Assert.That(process?.ExitCode, Is.EqualTo(0), "Removal of the dummy service");
	}
}
