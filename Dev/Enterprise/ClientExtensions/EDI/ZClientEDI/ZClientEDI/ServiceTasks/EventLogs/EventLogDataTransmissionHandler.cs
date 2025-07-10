using System;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.ServiceTasks.EventLogs
{
	public class EventLogDataTransmissionHandler
	{
		public EventLogDataTransmissionHandler()
		{
			var traceLevel = EDIDataRegistry.Instance.EvenLogTraceLevel.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (traceLevel == EDIDataRegistry.EvenLogTraceLevelOptions.CallStackOnly)
			{
				collectUnimportantData = collectNormalData = false;
				collectCallStackData = true;
			}
			else if (traceLevel == EDIDataRegistry.EvenLogTraceLevelOptions.Medium)
			{
				collectUnimportantData = false;
				collectNormalData = collectCallStackData = true;
			}
			else
			{
				collectUnimportantData = collectNormalData = collectCallStackData = true;
			}
			eventLogCaches = new EventLogCache();
			listProvidersAndTheirCapacityLocalCache = new EventLogsListProvidersAndTheirCapacityCollection();
		}

		#region Fields

		readonly bool collectUnimportantData;
		readonly bool collectNormalData;
		readonly bool collectCallStackData;
		readonly EventLogCache eventLogCaches;
		readonly EventLogsListProvidersAndTheirCapacityCollection listProvidersAndTheirCapacityLocalCache;

		#endregion

		internal EventLogCache EventLogCaches { get { return eventLogCaches; } }
		internal EventLogsListProvidersAndTheirCapacityCollection ListProvidersAndTheirCapacityLocalCache { get { return listProvidersAndTheirCapacityLocalCache; } }

		public virtual GlbStaff CurrentUser { get { return GlbStaff.CurrentUser; } }
		public virtual GlbCompany CurrentCompany { get { return GlbCompany.CurrentCompany; } }

		public void UpdateListProvidersAndTheirCapacityRegistryItemCache(XElement systemXml)
		{
			Argument.NotNull(systemXml, "systemXml");
			var providerName = Argument.NotNull(Argument.NotNull(systemXml.Element(XmlConverter.XmlPath + "Provider"), "Provider").Attribute("Name"), "Name").Value;
			var level = Argument.NotNull(systemXml.Element(XmlConverter.XmlPath + "Level"), "Level").Value;
			ListProvidersAndTheirCapacityLocalCache.AddNewOrGetExisting(new ZString(providerName), new ZInt(level));
		}

		public void UpdateEventLogCache(XElement xElement, string hostname)
		{
			eventLogCaches.UpdateEventLogCache(FindEventRecordID(xElement), hostname);
		}

		public void SyncEventLogHighWaterMarkRegistryAndListProvidersAndTheirCapacityRegistry()
		{
			eventLogCaches.Sync();
			SyncListProvidersAndTheirCapacityRegistry();
		}

		void SyncListProvidersAndTheirCapacityRegistry()
		{
			if (ListProvidersAndTheirCapacityLocalCache.Count > 0)
			{
				var listProvidersAndTheirCapacityLocalRegistry = EDIDataRegistry.Instance.ListProvidersAndTheirCapacity.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				foreach (var item in ListProvidersAndTheirCapacityLocalCache.Collection)
				{
					listProvidersAndTheirCapacityLocalRegistry.AddNewOrGetExisting(item.ProviderCode, item.LevelCode);
				}

				EDIDataRegistry.Instance.ListProvidersAndTheirCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, listProvidersAndTheirCapacityLocalRegistry);
			}
		}

		public bool AbleToCollect(State dataState)
		{
			return (collectUnimportantData && dataState == State.UnimportantData) ||
				(collectNormalData && dataState == State.NormalData) ||
				(collectCallStackData && dataState == State.HavingCallStackData);
		}

		internal virtual string FindHighWaterMark(string machinename)
		{
			Argument.NotNullOrEmpty(machinename, "machinename");
			return eventLogCaches.FindValue(machinename);
		}

		string FindEventRecordID(XElement xElement)
		{
			Argument.NotNull(xElement, "xElement");

			XNamespace df = xElement.Name.Namespace;
			XElement status = xElement.Element(df + "System").Element(df + "EventRecordID");
			return status.Value;
		}
	}

	public enum State { NormalData, HavingCallStackData, UnimportantData }
}
