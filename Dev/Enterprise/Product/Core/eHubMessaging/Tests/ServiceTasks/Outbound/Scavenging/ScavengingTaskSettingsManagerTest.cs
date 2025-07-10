using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging;
using Enterprise.Registry.Business.eHub;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.Scavenging
{
	class ScavengingTaskSettingsManagerTest : TestCaseWithFactory
	{
		public void TestGetScavengingTaskSettings()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var manager = new Mock<ScavengingTaskSettingsManager>() { CallBase = true };
			var collection = new ScavengingSettingCollection();
			var setting = collection.AddNew();
			setting.TaskName = "TestTask";
			manager.Object.Collection = collection;

			var settings = manager.Object.GetScavengingTaskSettings("TestTask");
			AssertEquals("TestTask", settings.TaskName);
			AssertEquals(ZDateTime.Empty, settings.PeriodStart);
			AssertEquals(ZDateTime.Empty, settings.PeriodEnd);
			Assert(true);
			mock.VerifyAll();
		}

		public void TestCorruptRegistry()
		{
			var oldXML = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfScavengingSetting><ScavengingSetting><TaskName>task1</TaskName><LastFullAddToQueue>000</LastFullAddToQueue><LastAddToQueue>000</LastAddToQueue></ScavengingSetting><ScavengingSetting><TaskName>Value</TaskName><LastFullAddToQueue>000</LastFullAddToQueue><LastAddToQueue>000</LastAddToQueue></ScavengingSetting></ArrayOfScavengingSetting>";
			var value = System.Text.Encoding.Unicode.GetBytes(oldXML);

			var setBinaryValue = typeof(RegistryDataAccessor).GetMethod("SetBinaryValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				setBinaryValue.Invoke(registryDataAccessor, new object[] { "ScavengingTaskSettings", Guid.Empty, Guid.Empty, value, Guid.Empty, "BIN", true, false });
			}

			var manager = new ScavengingTaskSettingsManager();
			manager.Load();

			ErrorReporter.Instance.Clear();
			AssertEquals(manager.GetScavengingTaskSettings("task1").TaskName, "task1");
		}
	}
}
