using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.AuditDataServices.Telematics.Subscribers;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Integration;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Telematics.Test
{
	[TestedType(typeof(GlbDeviceLocationSubscriber))]
	class GlbDeviceLocationSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[TestDate(2023, 2, 22)]
		public void TestProperties()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();
			AssertEquals("Should be using the schema for GlbDeviceLocation", GlbDeviceLocationSchema.Instance, subscriber.Table);
			AssertNull("Should not focus on specific columns, because it's only interested in the creation of new locations", subscriber.SpecificColumns);
			Assert("Should react to the creation of new location records", subscriber.NotifyInsert);
			AssertEquals("Should not react to the updating of location records", false, subscriber.NotifyUpdate);
			AssertEquals("Should not react to the deletion of location records", false, subscriber.NotifyDelete);
			AssertEquals("Should have a distinct code", "GDL", subscriber.Code);
			AssertEquals("Should have a meaningful description", "GlbDeviceLocation Change Subscriber", subscriber.Description);
			using (LocalCartageDataRegistry.Instance.UseGlbDeviceLocationSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("IsRequired should be true when UseGlbDeviceLocationSubscriber is true", true, subscriber.IsRequired());
			}
			using (LocalCartageDataRegistry.Instance.UseGlbDeviceLocationSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("IsRequired should be false when UseGlbDeviceLocationSubscriber is false", false, subscriber.IsRequired());
			}
		}

		public void TestProcessChanges_CreatesGpsCartageLegUpdater_UsingSuppliedFactory()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();

			var gpsCartageLegUpdater = new Mock<IGPSCartageLegUpdater>();
			var dataTable = GetTestDataTable();

			using (ObjectFactory.Substitute((arguments) =>
			{
				var factorySupplied = arguments[0];
				AssertEquals("Should use correct factory", subscriberFactory, factorySupplied);
				return gpsCartageLegUpdater.Object;
			}))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_TableWithNoRows()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();
			var gpsCartageLegUpdater = new Mock<IGPSCartageLegUpdater>();
			var dataTable = GetTestDataTable();

			using (ObjectFactory.Substitute(gpsCartageLegUpdater.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}

			gpsCartageLegUpdater.Verify(u => u.UpdateLastProcessedEventTime(It.IsAny<ZDateTime>()), Times.Never);
		}

		public void TestProcessChanges_TableWithOneNewRow()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();

			var gpsCartageLegUpdater = new Mock<IGPSCartageLegUpdater>();
			var dataTable = GetTestDataTable();
			AddRowToTable(dataTable, "198D51CF-5845-4099-A260-87DB95C1517D");
			AssertEquals("Precondition: factory save should not have been called yet", 0, subscriberFactory.SaveCount);

			using (ObjectFactory.Substitute(gpsCartageLegUpdater.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			gpsCartageLegUpdater.Verify(u => u.UpdateLastProcessedEventTime(MeasurementTimeUtc), Times.Once);
			AssertEquals("Should call factory save", 1, subscriberFactory.SaveCount);
		}

		public void TestProcessChanges_TableWithNoRows_WithoutMocking()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();
			var dataTable = GetTestDataTable();

			AssertNoExceptionThrown("Should not throw exception", () =>
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			});
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_WhenTableWithNoRows_ShouldNotCallGPSDtbConsignmentRunSheetInstructionUpdaterProcessLocation()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();
			var gpsDtbConsignmentRunSheetInstructionUpdater = new Mock<IGPSDtbConsignmentRunSheetInstructionUpdater>();
			var dataTable = GetTestDataTable();

			using (ObjectFactory.Substitute(gpsDtbConsignmentRunSheetInstructionUpdater.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}

			gpsDtbConsignmentRunSheetInstructionUpdater.Verify(u => u.ProcessLocation(It.IsAny<IDeviceLocationWithEntity>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_WhenTableWithMoreThanOneNewRow_ShouldCallGPSDtbConsignmentRunSheetInstructionUpdaterProcessLocation()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();
			var gpsDtbConsignmentRunSheetInstructionUpdater = new Mock<IGPSDtbConsignmentRunSheetInstructionUpdater>();
			var dataTable = GetTestDataTable();
			var device = subscriberFactory.NewWithValidTestData<GlbDevice>();
			subscriberFactory.Save();
			var deviceAssignmentDivot = subscriberFactory.NewWithValidTestData<GlbDeviceAssignmentDivot>();
			deviceAssignmentDivot.SetPropertyValue(GlbDeviceAssignmentDivotSchema.V7_V3_Device.Name, device.PK);
			deviceAssignmentDivot.SetPropertyValue(GlbDeviceAssignmentDivotSchema.V7_ParentTableCode.Name, new ZString(RefEquipmentSchema.Constants.Prefix));
			subscriberFactory.Save();
			var location1 = CreateNewTelDeviceLocationWithEntity(device.PK);
			var location2 = CreateNewTelDeviceLocationWithEntity(device.PK);
			AddRowToTable(dataTable, location1.PK.ToString());
			AddRowToTable(dataTable, location2.PK.ToString());

			using (ObjectFactory.Substitute(gpsDtbConsignmentRunSheetInstructionUpdater.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}

			gpsDtbConsignmentRunSheetInstructionUpdater.Verify(u => u.ProcessLocation(It.Is<IDeviceLocationWithEntity>(location => location.GetPropertyValue("PK").Equals(location1.PK))), Times.Once);
			gpsDtbConsignmentRunSheetInstructionUpdater.Verify(u => u.ProcessLocation(It.Is<IDeviceLocationWithEntity>(location => location.GetPropertyValue("PK").Equals(location2.PK))), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_WhenDeviceLinkToStaff_ShouldCallGPSDtbConsignmentRunSheetInstructionUpdaterProcessLocation_ButNotCallGpsCartageLegUpdater()
		{
			var subscriber = NewDataChangeSubscriberUsingFactory();

			var gpsDtbConsignmentRunSheetInstructionUpdater = new Mock<IGPSDtbConsignmentRunSheetInstructionUpdater>();
			var gpsCartageLegUpdater = new Mock<IGPSCartageLegUpdater>();
			var dataTable = GetTestDataTable();
			var device = subscriberFactory.NewWithValidTestData<GlbDevice>();
			subscriberFactory.Save();
			var deviceAssignmentDivot = subscriberFactory.NewWithValidTestData<GlbDeviceAssignmentDivot>();
			deviceAssignmentDivot.SetPropertyValue(GlbDeviceAssignmentDivotSchema.V7_V3_Device.Name, device.PK);
			deviceAssignmentDivot.SetPropertyValue(GlbDeviceAssignmentDivotSchema.V7_ParentTableCode.Name, new ZString(GlbStaffSchema.Constants.Prefix));
			subscriberFactory.Save();
			var location1 = CreateNewTelDeviceLocationWithEntity(device.PK);
			AddRowToTable(dataTable, location1.PK.ToString());

			using (ObjectFactory.Substitute(gpsCartageLegUpdater.Object))
			using (ObjectFactory.Substitute(gpsDtbConsignmentRunSheetInstructionUpdater.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}

			gpsDtbConsignmentRunSheetInstructionUpdater.Verify(u => u.ProcessLocation(It.Is<IDeviceLocationWithEntity>(location => location.GetPropertyValue("PK").Equals(location1.PK))), Times.Once);
			gpsCartageLegUpdater.Verify(u => u.ProcessLocation(It.IsAny<IDeviceLocationWithEntity>()), Times.Never);
		}

		GlbDeviceLocation CreateNewTelDeviceLocationWithEntity(ZGuid devicePK)
		{
			var location = subscriberFactory.NewWithValidTestData<GlbDeviceLocation>();

			location.SetPropertyValue(GlbDeviceLocationSchema.V2_V3_Device.Name, devicePK);
			subscriberFactory.Save();

			return location;
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbDeviceLocationSubscriber();

			var table = GetTestDataTable();
			var row1 = GetPopulatedDataRow(table, false);
			var row2 = GetPopulatedDataRow(table, false);
			GetPopulatedDataRow(table, true);
			GetPopulatedDataRow(table, true);

			for (var i = 0; i < 4; i++)
			{
				RunCustomFilter(table.Rows[i], subscriber);
			}
			table.AcceptChanges();

			AssertEquals("Should exclude location records that are in the distant future while last processed event time is still being used", 2, table.Rows.Count);
			AssertEquals(row1, table.Rows[0]);
			AssertEquals(row2, table.Rows[1]);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = AddRowToTable(table, "1322484A-114A-4AD4-849F-15EEE60BA1CB");
			if (shouldBeFiltered)
			{
				row[GlbDeviceLocationSchema.V2_MeasurementTimeUtc.Name] = ZDateTime.UtcToday.AddDays(2).ToDateTime();
				row.AcceptChanges();
			}
			return row;
		}

		ZDateTime MeasurementTimeUtc => new ZDateTime(2023, 2, 20);

		DataRow AddRowToTable(DataTable dataTable, string pk)
		{
			var dataRow = dataTable.NewRow();
			dataRow[GlbDeviceLocationSchema.PK.Name] = new Guid(pk);
			dataRow[GlbDeviceLocationSchema.V2_MeasurementTimeUtc.Name] = MeasurementTimeUtc;
			dataTable.Rows.Add(dataRow);
			return dataRow;
		}

		protected override DataTable GetTestDataTable()
		{
			var dataTable = new DataTable();
			dataTable.TableName = "GlbDeviceLocationSubscriberTest test table";
			AddColumns(
				dataTable,
				GlbDeviceLocationSchema.PK,
				GlbDeviceLocationSchema.V2_MeasurementTimeUtc
			);

			return dataTable;
		}

		void AddColumns(DataTable dataTable, params SchemaColumn[] schemaColumns)
		{
			foreach (var schemaColumn in schemaColumns)
			{
				dataTable.Columns.Add(schemaColumn.Name, schemaColumn.DotNetType);
			}
		}

		GlbDeviceLocationSubscriber NewDataChangeSubscriberUsingFactory()
		{
			subscriberFactory = new BusinessObjectFactory();
			return new GlbDeviceLocationSubscriber(subscriberFactory);
		}

		BusinessObjectFactory subscriberFactory;
	}
}
