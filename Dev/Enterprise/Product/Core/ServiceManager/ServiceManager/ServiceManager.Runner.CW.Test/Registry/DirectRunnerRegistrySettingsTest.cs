using System;
using System.Diagnostics;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Runner;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class DirectRunnerRegistrySettingsTest : TransactionedTestCase
	{
		public void TestSystemDataRegistryToRunnerRegistryPropertyMapping()
		{
			// Arrange
			var settings = new DirectRunnerRegistrySettings();
			
			// Act, Assert
			AssertEquals(settings.EnableStackTraceInUserContextSwitcher, SystemDataRegistry.Instance.EnableStackTraceInUserContextSwitcher.Value);
			AssertEquals(settings.ServiceTaskRunnerConnectionPoolingEnabled, SystemDataRegistry.Instance.ServiceTaskRunnerConnectionPoolingEnabled.Value);
			AssertEquals(settings.RunnerProcessPriorityValue, SystemDataRegistry.Instance.RunnerProcessPriorityValue);
			AssertEquals(settings.ServiceTaskUnloadTimeout, SystemDataRegistry.Instance.ServiceTaskUnloadTimeout);
		}

		public void TestSystemDataRegistryChangesAreReflectedInRunnerRegistry()
		{
			Test(o => o.EnableStackTraceInUserContextSwitcher, o => o.EnableStackTraceInUserContextSwitcher, true);
			Test(o => o.ServiceTaskRunnerConnectionPoolingEnabled, o => o.ServiceTaskRunnerConnectionPoolingEnabled, true);
			
			static void Test<TPropertyValue, TRegistryItem>(Func<DirectRunnerRegistrySettings, TPropertyValue> getPropertyValue, Func<SystemDataRegistry, TRegistryItem> getRegistryItem, TPropertyValue value)
				where TRegistryItem : StronglyTypedRegistryItem<TPropertyValue>
			{
				// Arrange
				using (getRegistryItem(SystemDataRegistry.Instance).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
				{
					// Act, Assert
					AssertEquals(getPropertyValue(new DirectRunnerRegistrySettings()), value);
				}
			}
		}

		public void TestSystemDataRegistryChangesIsReflectedForServiceTaskUnloadTimeout()
		{
			// Arrange
			using (SystemDataRegistry.Instance.ServiceTaskUnloadTimeoutInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				// Act, Assert
				AssertEquals(new DirectRunnerRegistrySettings().ServiceTaskUnloadTimeout, TimeSpan.FromSeconds(10));
			}
		}

		public void TestSystemDataRegistryChangesIsReflectedForRunnerProcessPriority()
		{
			// Arrange
			using (SystemDataRegistry.Instance.RunnerProcessPriority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ProcessPriorityOptions.Normal))
			{
				// Act, Assert
				AssertEquals(new DirectRunnerRegistrySettings().RunnerProcessPriorityValue, ProcessPriorityClass.Normal);
			}
		}
	}
}
