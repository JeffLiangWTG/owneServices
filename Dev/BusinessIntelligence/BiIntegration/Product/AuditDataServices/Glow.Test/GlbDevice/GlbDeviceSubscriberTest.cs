using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Glow.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Glow.Test
{
	[TestedType(typeof(GlbDeviceSubscriber))]
	class GlbDeviceSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public void TestProperties()
		{
			var subscriber = (GlbDeviceSubscriber)NewDataChangeSubscriber();
			AssertEquals(GlbDeviceSchema.Instance, subscriber.Table);
			AssertNull(subscriber.SpecificColumns);
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyUpdate);
			Assert(!subscriber.NotifyDelete);
			AssertEquals("GDC", subscriber.Code);
			AssertEquals("GlbDevice Change Subscriber", subscriber.Description);
			Assert(subscriber.IsRequired());
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_TableWithNoRows()
		{
			var subscriber = (GlbDeviceSubscriber)NewDataChangeSubscriber();

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var dataTable = GetTestDataTable();

			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODRegistration(It.IsAny<BusinessObjectFactory>(), It.IsIn<string>(), It.IsIn<string>(), It.IsIn<DeviceKind>(), It.IsIn<string>()), Times.Never);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODDeregistration(It.IsAny<BusinessObjectFactory>(), It.IsIn<string>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_TableWithOneNewBYODRow()
		{
			var subscriber = (GlbDeviceSubscriber)NewDataChangeSubscriber();

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var dataTable = GetTestDataTable();
			AddRowToTable(dataTable, status: GlbDeviceStatusList.Codes.Registered, "AA341", model: "IPhone69", deviceKind: GlbDeviceKindCodes.AppleMobile, hardwareId: "FDF325566");
			AddRowToTable(dataTable, clientIdentifier: "AA345", model: "Galaxy s99", deviceKind: GlbDeviceKindCodes.Android, hardwareId: "3489576394876");

			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODRegistration(It.IsAny<BusinessObjectFactory>(), "AA341", "IPhone69", DeviceKind.AppleMobile, "FDF325566"), Times.Once);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODRegistration(It.IsAny<BusinessObjectFactory>(), "AA345", "Galaxy s99", DeviceKind.Android, "3489576394876"), Times.Once);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODDeregistration(It.IsAny<BusinessObjectFactory>(), It.IsIn<string>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_TableWithOneBYODRowToRegister()
		{
			var subscriber = (GlbDeviceSubscriber)NewDataChangeSubscriber();

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var dataTable = GetTestDataTable();
			var dataRow1 = AddRowToTable(dataTable, clientIdentifier: "AA341", model: "IPhone69", deviceKind: GlbDeviceKindCodes.AppleMobile, hardwareId: "FDF325566");
			var dataRow2 = AddRowToTable(dataTable, status: GlbDeviceStatusList.Codes.Unregistered, clientIdentifier: "AA345", model: "Galaxy s99", deviceKind: GlbDeviceKindCodes.Android, hardwareId: "3489576394876");
			dataTable.AcceptChanges();
			dataRow1[GlbDeviceSchema.V3_Status.Name] = GlbDeviceStatusList.Codes.PendingRegistration;
			dataRow2[GlbDeviceSchema.V3_Status.Name] = GlbDeviceStatusList.Codes.PendingRegistration;

			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODRegistration(It.IsAny<BusinessObjectFactory>(), "AA341", "IPhone69", DeviceKind.AppleMobile, "FDF325566"), Times.Never);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODRegistration(It.IsAny<BusinessObjectFactory>(), "AA345", "Galaxy s99", DeviceKind.Android, "3489576394876"), Times.Once);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODDeregistration(It.IsAny<BusinessObjectFactory>(), It.IsIn<string>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_TableWithOneBYODRowToUnregister()
		{
			var subscriber = (GlbDeviceSubscriber)NewDataChangeSubscriber();

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var dataTable = GetTestDataTable();
			var dataRow1 = AddRowToTable(dataTable, status: GlbDeviceStatusList.Codes.DeregistrationFailure, "AA341", model: "IPhone69", deviceKind: GlbDeviceKindCodes.AppleMobile, hardwareId: "FDF325566");
			var dataRow2 = AddRowToTable(dataTable, status: GlbDeviceStatusList.Codes.Registered, clientIdentifier: "AA345", model: "Galaxy s99", deviceKind: GlbDeviceKindCodes.Android, hardwareId: "3489576394876");
			dataTable.AcceptChanges();
			dataRow1[GlbDeviceSchema.V3_Status.Name] = GlbDeviceStatusList.Codes.PendingDeregistration;
			dataRow2[GlbDeviceSchema.V3_Status.Name] = GlbDeviceStatusList.Codes.PendingDeregistration;

			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODRegistration(It.IsAny<BusinessObjectFactory>(), It.IsIn<string>(), It.IsIn<string>(), It.IsIn<DeviceKind>(), It.IsIn<string>()), Times.Never);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODDeregistration(It.IsAny<BusinessObjectFactory>(), "AA341"), Times.Once);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODDeregistration(It.IsAny<BusinessObjectFactory>(), "AA345"), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_TableWithOneBYODRowWithNullOriginalValue()
		{
			var subscriber = (GlbDeviceSubscriber)NewDataChangeSubscriber();

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var dataTable = GetTestDataTable();
			var dataRow1 = AddRowToTable(dataTable, clientIdentifier: "AA341", model: "IPhone69", deviceKind: GlbDeviceKindCodes.AppleMobile, hardwareId: "FDF325566");
			var dataRow2 = AddRowToTable(dataTable, status: null, clientIdentifier: "AA345", model: "Galaxy s99", deviceKind: GlbDeviceKindCodes.Android, hardwareId: "3489576394876");
			dataTable.AcceptChanges();
			dataRow1[GlbDeviceSchema.V3_Status.Name] = GlbDeviceStatusList.Codes.PendingRegistration;
			dataRow2[GlbDeviceSchema.V3_Status.Name] = GlbDeviceStatusList.Codes.PendingRegistration;

			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODRegistration(It.IsAny<BusinessObjectFactory>(), "AA341", "IPhone69", DeviceKind.AppleMobile, "FDF325566"), Times.Never);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODRegistration(It.IsAny<BusinessObjectFactory>(), "AA345", "Galaxy s99", DeviceKind.Android, "3489576394876"), Times.Once);
			telematicsNotifier.Verify(n => n.NotifyMobileServicesOfBYODDeregistration(It.IsAny<BusinessObjectFactory>(), It.IsIn<string>()), Times.Never);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbDeviceSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals(1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		public void TestCustomFilter_NullRowValue()
		{
			var subscriber = new GlbDeviceSubscriber();

			var table = GetTestDataTable();
			var row = AddRowToTable(table);
			row[GlbDeviceSchema.V3_IsBYOD.Name] = DBNull.Value;
			row.AcceptChanges();

			AssertEquals(1, table.Rows.Count);
			AssertNoExceptionThrown(() =>
			{
				RunCustomFilter(row, subscriber);
				table.AcceptChanges();
			});
			AssertEquals("DBNull values should also be filtered.", 0, table.Rows.Count);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = AddRowToTable(table);
			if (shouldBeFiltered)
			{
				row[GlbDeviceSchema.V3_IsBYOD.Name] = 0;
				row.AcceptChanges();
			}
			return row;
		}

		DataRow AddRowToTable(DataTable dataTable, string status = GlbDeviceStatusList.Codes.PendingRegistration, string clientIdentifier = "", string model = "", string deviceKind = "", string hardwareId = "")
		{
			var dataRow = dataTable.NewRow();
			dataRow[GlbDeviceSchema.V3_IsBYOD.Name] = true;
			dataRow[GlbDeviceSchema.V3_Status.Name] = status;
			dataRow[GlbDeviceSchema.V3_HumanReadableIdentifier.Name] = clientIdentifier;
			dataRow[GlbDeviceSchema.V3_Model.Name] = model;
			dataRow[GlbDeviceSchema.V3_HardwareKind.Name] = deviceKind;
			dataRow[GlbDeviceSchema.V3_HardwareIdentifier.Name] = hardwareId;
			dataTable.Rows.Add(dataRow);
			return dataRow;
		}

		protected override DataTable GetTestDataTable()
		{
			var dataTable = new DataTable();
			AddColumns(
				dataTable,
				GlbDeviceSchema.V3_IsBYOD,
				GlbDeviceSchema.V3_Status,
				GlbDeviceSchema.V3_HumanReadableIdentifier,
				GlbDeviceSchema.V3_Model,
				GlbDeviceSchema.V3_HardwareKind,
				GlbDeviceSchema.V3_HardwareIdentifier);

			return dataTable;
		}

		void AddColumns(DataTable dataTable, params SchemaColumn[] schemaColumns)
		{
			foreach (var schemaColumn in schemaColumns)
			{
				dataTable.Columns.Add(schemaColumn.Name, schemaColumn.DotNetType);
			}
		}
	}
}
