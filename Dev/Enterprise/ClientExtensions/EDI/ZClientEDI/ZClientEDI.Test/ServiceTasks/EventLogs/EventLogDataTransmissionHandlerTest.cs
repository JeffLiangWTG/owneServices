using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ServiceTasks.EventLogs;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	class EventLogDataTransmissionHandlerTest : TestCaseWithFactory
	{
		public void TestCorrectOutputFormat()
		{
			RegistryMachinesHandler.Sync("MyMachine+123412,AnotherMachine+123");
			var cacheLastLogHandler = new EventLogDataTransmissionHandler();
			var eventLogCaches = cacheLastLogHandler.EventLogCaches;
			string xml = @"<Event xmlns=" + "\"http://schemas.microsoft.com/win/2004/08/events/event\"" + @">
		<System>
			<Provider/>
			<Keywords>0x80000000000000</Keywords>
			<EventRecordID>4089894</EventRecordID>
			<Channel>Application</Channel>
		</System>
		<EventData>
		</EventData>
	</Event>";
			var lastCacheCount = eventLogCaches.EventLogCachesDictionary.Count;
			var lastRegistryItem = RegistryMachinesHandler.GetLatestListEventLogCache();
			Assert("Precodition", lastCacheCount > 0);
			cacheLastLogHandler.UpdateEventLogCache(XElement.Parse(xml), "MyHostName");
			AssertEquals("Expect success cache the eventlog", lastCacheCount + 1, eventLogCaches.EventLogCachesDictionary.Count);
			cacheLastLogHandler.SyncEventLogHighWaterMarkRegistryAndListProvidersAndTheirCapacityRegistry();
			AssertEquals("Expect success load into registry", lastRegistryItem.Count() + 1, RegistryMachinesHandler.GetLatestListEventLogCache().Count());
			var expected = new string[] { "MyHostName", "4089894" };
			AssertEquals("Expect correct string hostname message", "MyHostName", RegistryMachinesHandler.GetLatestListEventLogCache().Last().Machine);
			AssertEquals("Expect correct string eventlogid message", "4089894", RegistryMachinesHandler.GetLatestListEventLogCache().Last().EventRecordID);
		}

		public void TestSyncListProvidersAndTheirCapacityRegistry_OverrideExisting()
		{
			#region init
			var collection = new EventLogsListProvidersAndTheirCapacityCollection();
			collection.AddNewOrGetExisting(".NET Runtime", 2);
			collection.AddNewOrGetExisting("ediEnterpriseProcessController_ediSHAProd.db.Corporate.Cargowise.com_ediSHAProd", 2);
			collection.AddNewOrGetExisting("ediEnterpriseProcessController_ediSHAProd.db.Corporate.Cargowise.com_ediSHAProd", 3);
			collection.AddNewOrGetExisting("Test", 2);
			EDIDataRegistry.Instance.ListProvidersAndTheirCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			#endregion
			var cacheLastLogHandler = new EventLogDataTransmissionHandler();
			string ediSHAProd2 = @"<System xmlns=""http://schemas.microsoft.com/win/2004/08/events/event"">
      <Provider Name=""ediEnterpriseProcessController_ediSHAProd.db.Corporate.Cargowise.com_ediSHAProd"" />
      <EventID Qualifiers=""0"">1026</EventID>
      <Level>2</Level>
      <Task>0</Task>
      <Keywords>0x80000000000000</Keywords>
      <TimeCreated SystemTime=""2014-07-08T22:27:54.000000000Z"" />
      <EventRecordID>3966021</EventRecordID>
      <Channel>Application</Channel>
      <Computer>SYD-WDDB-2.corporate.cargowise.com</Computer>
      <Security />
    </System>";
			string ediSHAProd3 = @"<System xmlns=""http://schemas.microsoft.com/win/2004/08/events/event"">
		  <Provider Name=""ediEnterpriseProcessController_ediSHAProd.db.Corporate.Cargowise.com_ediSHAProd"" />
		  <EventID Qualifiers=""0"">1026</EventID>
		  <Level>3</Level>
		  <Task>0</Task>
		  <Keywords>0x80000000000000</Keywords>
		  <TimeCreated SystemTime=""2014-07-05T22:27:54.000000000Z"" />
		  <EventRecordID>4089894</EventRecordID>
		  <Channel>Application</Channel>
		  <Computer>SYD-WDDB-2.corporate.cargowise.com</Computer>
		  <Security />
		</System>";
			string configurationManagerAgent = @"<System xmlns=""http://schemas.microsoft.com/win/2004/08/events/event"">
		  <Provider Name=""Configuration Manager Agent"" />
		  <EventID Qualifiers=""0"">1026</EventID>
		  <Level>3</Level>
		  <Task>0</Task>
		  <Keywords>0x80000000000000</Keywords>
		  <TimeCreated SystemTime=""2014-07-05T22:27:54.000000000Z"" />
		  <EventRecordID>4089893</EventRecordID>
		  <Channel>Application</Channel>
		  <Computer>SYD-WDDB-2.corporate.cargowise.com</Computer>
		  <Security />
		</System>";
			cacheLastLogHandler.UpdateListProvidersAndTheirCapacityRegistryItemCache(XElement.Parse(ediSHAProd3));
			cacheLastLogHandler.UpdateListProvidersAndTheirCapacityRegistryItemCache(XElement.Parse(configurationManagerAgent));
			cacheLastLogHandler.UpdateListProvidersAndTheirCapacityRegistryItemCache(XElement.Parse(ediSHAProd2));
			cacheLastLogHandler.UpdateListProvidersAndTheirCapacityRegistryItemCache(XElement.Parse(configurationManagerAgent));
			AssertEquals(3, cacheLastLogHandler.ListProvidersAndTheirCapacityLocalCache.Count);
			var preListProvidersAndTheirCapacity = String.Join(", ", EDIDataRegistry.Instance.ListProvidersAndTheirCapacity.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Collection.Select(x => x.ProviderCode + "-" + x.LevelCode));
			cacheLastLogHandler.SyncEventLogHighWaterMarkRegistryAndListProvidersAndTheirCapacityRegistry();
			var postListProvidersAndTheirCapacity = String.Join(", ", EDIDataRegistry.Instance.ListProvidersAndTheirCapacity.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).Collection.Select(x => x.ProviderCode + "-" + x.LevelCode));
			AssertEquals(".NET Runtime-2, ediEnterpriseProcessController_ediSHAProd.db.Corporate.Cargowise.com_ediSHAProd-2, ediEnterpriseProcessController_ediSHAProd.db.Corporate.Cargowise.com_ediSHAProd-3, Test-2", preListProvidersAndTheirCapacity);
			AssertEquals(".NET Runtime-2, ediEnterpriseProcessController_ediSHAProd.db.Corporate.Cargowise.com_ediSHAProd-2, ediEnterpriseProcessController_ediSHAProd.db.Corporate.Cargowise.com_ediSHAProd-3, Test-2, Configuration Manager Agent-3", postListProvidersAndTheirCapacity);
		}
	}
}
