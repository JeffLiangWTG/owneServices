using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Extensions.Testing
{
	sealed class ZServiceContainerExtensionsTest : TestCase
	{
		#region IAddOnSavingService extensions test

		public void TestAddOnSaving()
		{
			var instance = new MockOnSavingServiceBuilder();
			serviceContainer.AddOnSavingService(instance);
			AssertEquals(instance, serviceContainer.GetOnSavingService<MockOnSavingServiceBuilder>());
			serviceContainer.RemoveOnSavingService<MockOnSavingServiceBuilder>();
			AssertNull(serviceContainer.GetOnSavingService<MockOnSavingServiceBuilder>());
			AssertNoExceptionThrown(() => serviceContainer.RemoveOnSavingService<MockOnSavingServiceBuilder>());
		}

		class MockOnSavingServiceBuilder : IOnSavingServiceBuilder
		{
			public IEnumerable<IOnSavingService> Build(IEnumerable<BusinessObject> rows)
			{
				yield break;
			}
		}

		#endregion

		#region IAfterOnSavingBOProcessingService extensions test

		#region TestAddAndGetAfterOnSavingService

		public void TestAddAndGetAfterOnSavingService()
		{
			AssertNull("GetAfterOnSavingService returns null if service is not in a container.", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService1>());

			serviceContainer.AddAfterOnSavingService(afterOnSavingService1);
			serviceContainer.AddAfterOnSavingService(afterOnSavingService2);
			AssertEquals("GetAfterOnSavingService() should get the service added by AddService()", afterOnSavingService1, serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService1>());
			AssertEquals("GetAfterOnSavingService() should get the service added by AddService()", afterOnSavingService2, serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService2>());
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestAddAfterOnSavingService_ThrowExceptionIfServiceAlreadyExists()
		{
			serviceContainer.AddAfterOnSavingService(afterOnSavingService1);
			serviceContainer.AddAfterOnSavingService(afterOnSavingService1);
		}

		[ExpectNoExceptions]
		public void TestRemoveAfterOnSavingServiceTwice()
		{
			serviceContainer.AddAfterOnSavingService(afterOnSavingService1);
			serviceContainer.RemoveAfterOnSavingService<TestAfterOnSavingService1>();
			serviceContainer.RemoveAfterOnSavingService<TestAfterOnSavingService1>();
		}

		#endregion

		#region TestRemoveAfterOnSavingService

		public void TestRemoveAfterOnSavingService()
		{
			serviceContainer.AddAfterOnSavingService(afterOnSavingService1);
			AssertNotNull("Service1 should be added", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService1>());
			AssertNull("Service2 should not be added", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService2>());

			serviceContainer.AddAfterOnSavingService(afterOnSavingService2);
			AssertNotNull("Service1 should be added", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService1>());
			AssertNotNull("Service2 should be added", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService2>());

			serviceContainer.RemoveAfterOnSavingService<TestAfterOnSavingService1>();
			AssertNull("Service1 should be removed", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService1>());
			AssertNotNull("Service2 should not be removed", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService2>());

			serviceContainer.RemoveAfterOnSavingService<TestAfterOnSavingService2>();
			AssertNull("Service1 should be removed", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService1>());
			AssertNull("Service2 should be removed", serviceContainer.GetAfterOnSavingService<TestAfterOnSavingService2>());
		}

		#endregion

		#region TestAfterOnSavingBOProcessingServiceProviderUsed

		public void TestAfterOnSavingBOProcessingServiceProviderUsed()
		{
			AssertNull("AfterOnSavingBOProcessingServiceProvider is not in the container.", serviceContainer.GetService<AfterOnSavingBOProcessingServiceProvider>());

			serviceContainer.AddAfterOnSavingService(afterOnSavingService1);
			var serviceProvider = serviceContainer.GetService<AfterOnSavingBOProcessingServiceProvider>();
			AssertNotNull("AfterOnSavingBOProcessingServiceProvider should be added in the container.", serviceProvider);

			serviceContainer.AddAfterOnSavingService(afterOnSavingService2);
			AssertEquals("The container should have the same AfterOnSavingBOProcessingServiceProvider.", serviceProvider, serviceContainer.GetService<AfterOnSavingBOProcessingServiceProvider>());

			serviceContainer.RemoveAfterOnSavingService<TestAfterOnSavingService1>();
			AssertEquals("The container should have the same AfterOnSavingBOProcessingServiceProvider.", serviceProvider, serviceContainer.GetService<AfterOnSavingBOProcessingServiceProvider>());

			serviceContainer.RemoveAfterOnSavingService<TestAfterOnSavingService2>();
			AssertNull("AfterOnSavingBOProcessingServiceProvider should be removed from the container.", serviceContainer.GetService<AfterOnSavingBOProcessingServiceProvider>());
		}

		#endregion

		#endregion

		#region IAfterSaveInTransactionService extensions test

		public void TestAfterSaveInTransactionService()
		{
			AssertNull("GetAfterSaveInTransactionService returns null if service is not in a container.", serviceContainer.GetAfterSaveInTransactionService<TestAfterSaveInTransactionService1>());

			serviceContainer.AddAfterSaveInTransactionService(afterAfterSaveInTransactionService1);
			serviceContainer.AddAfterSaveInTransactionService(afterAfterSaveInTransactionService2);
			AssertEquals("GetAfterOnSavingService() should get the service added by AddService()", afterAfterSaveInTransactionService1, serviceContainer.GetAfterSaveInTransactionService<TestAfterSaveInTransactionService1>());
			AssertEquals("GetAfterOnSavingService() should get the service added by AddService()", afterAfterSaveInTransactionService2, serviceContainer.GetAfterSaveInTransactionService<TestAfterSaveInTransactionService2>());
		}

		public void AddAfterSaveInTransactionService_ThrowExceptionIfServiceAlreadyExists()
		{
			serviceContainer.AddAfterSaveInTransactionService(afterAfterSaveInTransactionService1);
			AssertExceptionThrown<ArgumentException>(() => serviceContainer.AddAfterSaveInTransactionService(afterAfterSaveInTransactionService1));
		}

		#endregion

		#region IAfterCommittedService extensions test

		public void TestAfterCommittedService()
		{
			AssertNull("GetAfterCommittedService returns null if service is not in a container.", serviceContainer.GetAfterCommittedService<TestAfterCommittedService1>());

			serviceContainer.AddAfterCommittedService(afterCommittedService1);
			serviceContainer.AddAfterCommittedService(afterCommittedService2);
			AssertEquals("GetAfterCommittedService() should get the service added by AddService()", afterCommittedService1, serviceContainer.GetAfterCommittedService<TestAfterCommittedService1>());
			AssertEquals("GetAfterCommittedService() should get the service added by AddService()", afterCommittedService2, serviceContainer.GetAfterCommittedService<TestAfterCommittedService2>());
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestAfterCommittedService_ThrowExceptionIfServiceAlreadyExists()
		{
			serviceContainer.AddAfterCommittedService(afterCommittedService1);
			serviceContainer.AddAfterCommittedService(afterCommittedService1);
		}

		#endregion

		#region Implementation

		readonly ZServiceContainer serviceContainer = new ZServiceContainer();
		readonly TestAfterOnSavingService1 afterOnSavingService1 = new TestAfterOnSavingService1();
		readonly TestAfterOnSavingService2 afterOnSavingService2 = new TestAfterOnSavingService2();
		readonly TestAfterSaveInTransactionService1 afterAfterSaveInTransactionService1 = new TestAfterSaveInTransactionService1();
		readonly TestAfterSaveInTransactionService2 afterAfterSaveInTransactionService2 = new TestAfterSaveInTransactionService2();
		readonly TestAfterCommittedService1 afterCommittedService1 = new TestAfterCommittedService1();
		readonly TestAfterCommittedService2 afterCommittedService2 = new TestAfterCommittedService2();

		class TestAfterOnSavingService1 : IAfterOnSavingBOProcessingService
		{
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				throw new NotImplementedException();
			}
		}

		class TestAfterOnSavingService2 : IAfterOnSavingBOProcessingService
		{
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				throw new NotImplementedException();
			}
		}

		class TestAfterSaveInTransactionService1 : IAfterSaveInTransactionService
		{
			public void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				throw new NotImplementedException();
			}
		}

		class TestAfterSaveInTransactionService2 : IAfterSaveInTransactionService
		{
			public void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				throw new NotImplementedException();
			}
		}

		class TestAfterCommittedService1 : IAfterCommittedService
		{
			public void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				throw new NotImplementedException();
			}
		}

		class TestAfterCommittedService2 : IAfterCommittedService
		{
			public void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
