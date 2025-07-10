using CargoWise.Common;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DisposableManagerServiceTest : TestCaseWithFactory
	{
		public void TestGetDisposableManager()
		{
			using (var manager = new DisposableManager())
			{
				using (Factory.AddDisposableService(manager))
				{
					AssertEquals(manager, Factory.GetDisposableManager());
				}
			}
		}

		public void TestGetDisposableManager_Null()
		{
			Factory.NameForDebugging = "TheFactory";
			AssertNull(Factory.GetDisposableManager());
			AssertEquals("Disposable manager service is not initialized. Factory: TheFactory", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestService_External_DisposeRemovesService()
		{
			int disposeCount = 0;
			var dispose = new DisposableAction(() => disposeCount++);
			using (var manager = new DisposableManager())
			{
				manager.Subscribe(dispose);
				using (Factory.AddDisposableService(manager))
				{
					AssertEquals("precondition", 0, disposeCount);
					AssertNotNull("precondition", Factory.ServiceContainer.GetService<DisposableManagerService>());
				}

				AssertNull("Service is removed", Factory.ServiceContainer.GetService<DisposableManagerService>());
				AssertEquals("But dispose hasn't ocurred", 0, disposeCount);
			}

			AssertEquals("Disposing the manager disposes", 1, disposeCount);
		}

		public void TestDisposeRemovesService()
		{
			int disposeCount = 0;
			var dispose = new DisposableAction(() => disposeCount++);
			using (Factory.AddDisposableService())
			{
				Factory.SubscribeForDispose(dispose);
				AssertEquals(0, disposeCount);
				AssertNotNull(Factory.ServiceContainer.GetService<DisposableManagerService>());
			}
			AssertEquals("Dispose occurrs because service is internally constructed", 1, disposeCount);
			AssertNull("Service is removed", Factory.ServiceContainer.GetService<DisposableManagerService>());
		}

		public void TestNestedDisposableServices()
		{
			int disposeCount = 0;
			var dispose1 = new DisposableAction(() => disposeCount++);
			var dispose2 = new DisposableAction(() => disposeCount++);
			var dispose3 = new DisposableAction(() => disposeCount++);

			using (Factory.AddDisposableService())
			{
				Factory.SubscribeForDispose(dispose1);

				using (Factory.AddDisposableService())
				{
					using (Factory.AddDisposableService())
					{
						Factory.SubscribeForDispose(dispose3);
						AssertEquals(0, disposeCount);
						AssertNotNull(Factory.ServiceContainer.GetService<DisposableManagerService>());
					}
					Factory.SubscribeForDispose(dispose2);
					AssertEquals(1, disposeCount);
					AssertNotNull(Factory.ServiceContainer.GetService<DisposableManagerService>());
				}
				AssertEquals(2, disposeCount);
				AssertNotNull(Factory.ServiceContainer.GetService<DisposableManagerService>());
			}
			AssertEquals("Dispose occurrs because service is internally constructed", 3, disposeCount);
			AssertNull("Service is removed", Factory.ServiceContainer.GetService<DisposableManagerService>());
		}

		public void TestSubscribeExtension_Uninitialized_ReportsError()
		{
			Factory.SubscribeForDispose(new DisposableAction(() => { }));
			AssertEquals("Disposable manager service is not initialized. Factory: ", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAddDisposableServiceIfRequired()
		{
			var disposeCount = 0;
			using (Factory.AddDisposableServiceIfRequired())
			{
				using (Factory.AddDisposableServiceIfRequired())
				{
					Factory.SubscribeForDispose(new DisposableAction(() => disposeCount++));
				}
				AssertEquals("The second nested service should not dispose", 0, disposeCount);
			}
			AssertEquals("Object should be disposed", 1, disposeCount);
		}
	}
}
