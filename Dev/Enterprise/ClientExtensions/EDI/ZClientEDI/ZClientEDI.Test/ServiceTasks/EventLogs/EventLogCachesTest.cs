using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	class EventLogCachesTest : TestCaseWithFactory
	{
		public void TestAbleToLoadCache_FirstTimeAccessInstance()
		{
			EDIDataRegistry.Instance.EventLogHighWaterMarkList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, String.Empty);
			var eventLogCaches = new EventLogCache();
			AssertEquals("Precodition: dictionary entry count", 0, eventLogCaches.EventLogCachesDictionary.Count);
			AssertNotNull("Expected the dictionary is created.", eventLogCaches.EventLogCachesDictionary);
			RegistryMachinesHandler.Sync("MyMachine+123412,AnotherMachine+123");
			eventLogCaches = new EventLogCache();
			AssertNotNull("Expected the dictionary is created.", eventLogCaches.EventLogCachesDictionary);
			AssertEquals("Number of elements in inside the dictionary when created.", "MyMachine+123412,AnotherMachine+123", EDIDataRegistry.Instance.EventLogHighWaterMarkList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Number of elements in inside the dictionary when created.", 2, eventLogCaches.EventLogCachesDictionary.Count);
		}

		public void TestCache()
		{
			RegistryMachinesHandler.Sync("MyMachine+123412,AnotherMachine+123");
			var eventLogCaches = new EventLogCache();
			int count = eventLogCaches.EventLogCachesDictionary.Count;
			AssertEquals("Precondition", 2, count);
			eventLogCaches.UpdateEventLogCache("123", "HelloMachine");
			AssertEquals("Should insert into new cache", count + 1, eventLogCaches.EventLogCachesDictionary.Count);
		}

		public void TestSync()
		{
			RegistryMachinesHandler.Sync("MyMachine+123412,AnotherMachine+123");
			var eventLogCaches = new EventLogCache();
			var lastRegistryItem = RegistryMachinesHandler.GetLatestListEventLogCache();
			AssertEquals("Precondition", 2, eventLogCaches.EventLogCachesDictionary.Count);
			AssertEquals("Precondition", 2, lastRegistryItem.Count());
			eventLogCaches.UpdateEventLogCache("123", "HelloMachine");
			eventLogCaches.Sync();
			AssertEquals("Expect success load into registry", lastRegistryItem.Count() + 1, RegistryMachinesHandler.GetLatestListEventLogCache().Count());
		}

		public void TestSyncWithUpdatingRegistryWhileProcess_MergeTwoDictionary()
		{
			RegistryMachinesHandler.Sync("MyMachine+123412,AnotherMachine+123");
			var eventLogCaches = new EventLogCache();
			AssertEquals("Precondition", 2, eventLogCaches.EventLogCachesDictionary.Count);
			eventLogCaches.UpdateEventLogCache("123", "HelloMachine");
			RegistryMachinesHandler.Sync("MyMachine+155555,AnotherMachine+155,AnotherAnotherMachine+1");
			AssertEquals("Precondition: Registry List Count", 3, RegistryMachinesHandler.GetLatestListEventLogCache().Count());
			eventLogCaches.Sync();
			AssertEquals("Should insert into new cache", 4, eventLogCaches.EventLogCachesDictionary.Count);
		}
	}
}
