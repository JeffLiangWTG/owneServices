using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.AGS.Business.Testing
{
	[TestedType(typeof(StmALogValueObjectDataAdapter))]
	class DocTIPContainerLegTest : StmALogValueObjectDataAdapterTest
	{
		public void TestToXmlCollectionValueObject_Shipment()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BusinessObject bizObj = SetupShipmentWithEvents();
			Xsd.Events eventsValue = StmALogValueObjectDataAdapter.New(bizObj, "", EventsWithSourceType.Empty).ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(4, eventsValue.Event.Count);
			AssertXsdContainsExportedEvent(eventsValue, Events.Arrival.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.EntryWorkInProgress.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.AddedARecordToTheSystem.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.DataImport.Code);
		}

		ForwardingShipment SetupShipmentWithEvents()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			var nonSystemLogValue = new EventValue(Events.Arrival, reference: "nonsystem1", eventTime: new ZDateTimeOffset(2005, 1, 1));
			shipment.Logs.AddNew(nonSystemLogValue);
			nonSystemLogValue = new EventValue(Events.EntryWorkInProgress, reference: "nonsystem2", eventTime: new ZDateTimeOffset(2005, 2, 2));
			shipment.Logs.AddNew(nonSystemLogValue);
			nonSystemLogValue = new EventValue(Events.AddedARecordToTheSystem, reference: "system1", eventTime: new ZDateTimeOffset(2005, 3, 3));
			shipment.Logs.AddNew(nonSystemLogValue);
			nonSystemLogValue = new EventValue(Events.EditedARecord, reference: "system2", eventTime: new ZDateTimeOffset(2005, 4, 4));
			shipment.Logs.AddNew(nonSystemLogValue);
			nonSystemLogValue = new EventValue(Events.DeletedARecordInTheSystem, reference: "system3", eventTime: new ZDateTimeOffset(2005, 5, 5));
			shipment.Logs.AddNew(nonSystemLogValue);
			nonSystemLogValue = new EventValue(Events.DataExport, reference: "system4", eventTime: new ZDateTimeOffset(2005, 6, 6));
			shipment.Logs.AddNew(nonSystemLogValue);
			nonSystemLogValue = new EventValue(Events.DataImport, reference: "system5", eventTime: new ZDateTimeOffset(2005, 7, 7));
			shipment.Logs.AddNew(nonSystemLogValue);
			return shipment;
		}
	}
}
