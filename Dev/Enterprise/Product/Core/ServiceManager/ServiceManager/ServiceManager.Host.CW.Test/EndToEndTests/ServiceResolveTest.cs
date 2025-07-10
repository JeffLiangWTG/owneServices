using System;
using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Host;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW1.Test.EndToEndTests;

class ServiceResolveTest : TransactionedTestCase
{
	public void TestSameServicesAreResolvedIfRegistryItemValueChangesAfterHostStarts()
	{
		// Arrange
		using var flag1 = SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		var services =
			new ServiceCollection()
				.AddRegistrations(new[] { Db.ServerName, Db.DatabaseName });
		using var serviceProvider = services.BuildServiceProvider();

		serviceProvider.GetRequiredService<IHostRegistry>().Initialize();

		// Act
		var taskLoader1 = serviceProvider.GetRequiredService<IServiceTaskLoaderFactory>().CreateServiceTaskLoader();
		var transactionAdapter1 = serviceProvider.GetRequiredService<ITransactionAdapterFactory>().CreateTransactionAdapter();
		var tasksReloader1 = serviceProvider.GetRequiredService<IServiceTasksReloaderFactory>().CreateServiceTasksReloader();

		using var flag2 = SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		serviceProvider.GetRequiredService<IHostRegistry>().Refresh();

		var taskLoader2 = serviceProvider.GetRequiredService<IServiceTaskLoaderFactory>().CreateServiceTaskLoader();
		var transactionAdapter2 = serviceProvider.GetRequiredService<ITransactionAdapterFactory>().CreateTransactionAdapter();
		var tasksReloader2 = serviceProvider.GetRequiredService<IServiceTasksReloaderFactory>().CreateServiceTasksReloader();

		// Assert
		AssertType<NativeServiceTaskLoader>(taskLoader1);
		AssertType<NativeServiceTaskLoader>(taskLoader2);
		AssertType<NativeServiceTaskTransactionAdapter>(transactionAdapter1);
		AssertType<NativeServiceTaskTransactionAdapter>(transactionAdapter2);
		AssertType<NativeServiceTasksReloader>(tasksReloader1);
		AssertType<NativeServiceTasksReloader>(tasksReloader2);
	}
}

