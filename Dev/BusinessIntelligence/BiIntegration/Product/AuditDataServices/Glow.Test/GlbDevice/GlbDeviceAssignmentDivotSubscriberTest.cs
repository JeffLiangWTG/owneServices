using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Glow.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Glow.Test
{
	[TestedType(typeof(GlbDeviceAssignmentDivotSubscriberForTest))]
	class GlbDeviceAssignmentDivotSubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		[ExpectNoExceptions]
		public void TestProcessChanges_SetGlbStaffParent()
		{
			var factory = new BusinessObjectFactory();
			var glbDevice = factory.NewWithValidTestData<GlbDevice>();
			glbDevice.V3_HumanReadableIdentifier = "HRI001";
			var glbStaff = factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_FullName = "Test FullName";
			glbStaff.GS_Code = "TST";
			factory.Save();

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var dataTable = new DataTable();
			AddColumns(
				dataTable,
				GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc,
				GlbDeviceAssignmentDivotSchema.V7_ParentID,
				GlbDeviceAssignmentDivotSchema.V7_ParentTableCode,
				GlbDeviceAssignmentDivotSchema.V7_V3_Device);
			var dataRow = dataTable.NewRow();
			dataRow[GlbDeviceAssignmentDivotSchema.V7_ParentID.Name] = glbStaff.PK.ToGuid();
			dataRow[GlbDeviceAssignmentDivotSchema.V7_ParentTableCode.Name] = GlbStaffSchema.Constants.Prefix;
			dataRow[GlbDeviceAssignmentDivotSchema.V7_V3_Device.Name] = glbDevice.PK.ToGuid();
			dataTable.Rows.Add(dataRow);

			var subscriber = (GlbDeviceAssignmentDivotSubscriberForTest)NewDataChangeSubscriber();

			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			telematicsNotifier.Verify(m => m.NotifyMobileServicesOfNewDeviceParent(
				subscriber.FactoryForTest,
				"HRI001",
				"TST",
				"Test FullName",
				GlbStaffSchema.Constants.Prefix), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_SetRefEquipmentParent()
		{
			var factory = new BusinessObjectFactory();
			var glbDevice = factory.NewWithValidTestData<GlbDevice>();
			glbDevice.V3_HumanReadableIdentifier = "HRI001";
			var refEquipment = factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_ShortCode = "TST";
			refEquipment.RQ_Description = "Test Description";
			factory.Save();

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var dataTable = new DataTable();
			AddColumns(
				dataTable,
				GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc,
				GlbDeviceAssignmentDivotSchema.V7_ParentID,
				GlbDeviceAssignmentDivotSchema.V7_ParentTableCode,
				GlbDeviceAssignmentDivotSchema.V7_V3_Device);
			var dataRow = dataTable.NewRow();
			dataRow[GlbDeviceAssignmentDivotSchema.V7_ParentID.Name] = refEquipment.PK.ToGuid();
			dataRow[GlbDeviceAssignmentDivotSchema.V7_ParentTableCode.Name] = RefEquipmentSchema.Constants.Prefix;
			dataRow[GlbDeviceAssignmentDivotSchema.V7_V3_Device.Name] = glbDevice.PK.ToGuid();
			dataTable.Rows.Add(dataRow);

			var subscriber = (GlbDeviceAssignmentDivotSubscriberForTest)NewDataChangeSubscriber();

			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			telematicsNotifier.Verify(m => m.NotifyMobileServicesOfNewDeviceParent(
				subscriber.FactoryForTest,
				"HRI001",
				"TST",
				"Test Description",
				RefEquipmentSchema.Constants.Prefix), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestProcessChanges_ClearParent()
		{
			var factory = new BusinessObjectFactory();
			var glbDevice = factory.NewWithValidTestData<GlbDevice>();
			glbDevice.V3_HumanReadableIdentifier = "HRI001";
			var glbStaff = factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_FullName = "Test FullName";
			glbStaff.GS_Code = "TST";
			glbDevice.AssignedParentStaffID = glbStaff.PK;
			factory.Save();

			var telematicsNotifier = new Mock<ITelematicsNotifier>();
			var dataTable = new DataTable();
			AddColumns(
				dataTable,
				GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc,
				GlbDeviceAssignmentDivotSchema.V7_ParentID,
				GlbDeviceAssignmentDivotSchema.V7_ParentTableCode,
				GlbDeviceAssignmentDivotSchema.V7_V3_Device);
			var dataRow = dataTable.NewRow();
			dataRow[GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc.Name] = new DateTime(2020, 1, 1);
			dataRow[GlbDeviceAssignmentDivotSchema.V7_ParentID.Name] = glbStaff.PK.ToGuid();
			dataRow[GlbDeviceAssignmentDivotSchema.V7_ParentTableCode.Name] = GlbStaffSchema.Constants.Prefix;
			dataRow[GlbDeviceAssignmentDivotSchema.V7_V3_Device.Name] = glbDevice.PK.ToGuid();
			dataTable.Rows.Add(dataRow);

			var subscriber = (GlbDeviceAssignmentDivotSubscriberForTest)NewDataChangeSubscriber();

			using (ObjectFactory.Substitute(telematicsNotifier.Object))
			{
				subscriber.ProcessChanges(new DummyLogger(), dataTable);
			}
			telematicsNotifier.Verify(m => m.NotifyMobileServicesOfNewDeviceParent(
				subscriber.FactoryForTest,
				"HRI001",
				string.Empty,
				string.Empty,
				string.Empty), Times.Once);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbDeviceAssignmentDivotSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		void AddColumns(DataTable dataTable, params SchemaColumn[] schemaColumns)
		{
			foreach (var schemaColumn in schemaColumns)
			{
				dataTable.Columns.Add(schemaColumn.Name, schemaColumn.DotNetType);
			}
		}

		protected override DataTable GetTestDataTable() => null;
	}

	class GlbDeviceAssignmentDivotSubscriberForTest : GlbDeviceAssignmentDivotSubscriber
	{
		public BusinessObjectFactory FactoryForTest => businessObjectFactory;
	}
}
