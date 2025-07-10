using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class DepartmentChooserTest : TestCaseWithFactory
	{
		public void TestDictionaryKeyStructure()
		{
			var key1 = new DepartmentChooser.KeyForDictionary() { TableName = "SHP", IsOriginLocal = false };
			var key2 = new DepartmentChooser.KeyForDictionary() { TableName = "SHP", IsOriginLocal = false };

			Assert("Precondition: Equals", (key1.Equals(key2)));
			AssertEquals(key1.GetHashCode(), key2.GetHashCode());

			key1 = new DepartmentChooser.KeyForDictionary() { TableName = "SHP", IsOriginLocal = false };
			key2 = new DepartmentChooser.KeyForDictionary() { TableName = "SHP", IsOriginLocal = true };

			Assert("Precondition: Not Equals", !key1.Equals(key2));
			AssertNotEquals(key1.GetHashCode(), key2.GetHashCode());
		}

		public void TestDBHitCountForInitDepartmentDictionary()
		{
			var dbHitCountBefore = Db.Connection.ExecutedCommandCount;
			var departmentChooser = DepartmentChooser.New(Factory);
			var dbHitCountAfter = Db.Connection.ExecutedCommandCount;
			var dbHits = dbHitCountAfter - dbHitCountBefore;
			Assert("DB hit count must be 0 but it was " + dbHits, dbHits < 1);
		}

		public void TestDBHitCountForGetDepartment()
		{
			var testBrokerage = Factory.New<BaseJobDeclaration>();
			testBrokerage.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			var dbHitCountBefore = Db.Connection.ExecutedCommandCount;
			DepartmentChooser.GetDepartment(testBrokerage);
			var dbHitCountAfter = Db.Connection.ExecutedCommandCount;
			var dbHits = dbHitCountAfter - dbHitCountBefore;
			Assert("DB hit count must be 4 or less but it was " + dbHits, dbHits < 5);

			var domesticDefaultDepartments = new JobInvoicingDefaultDepartmentsCollection();
			var domesticDefaultDepartment = domesticDefaultDepartments.AddNew();
			domesticDefaultDepartment.ConsolType = "ALL";
			domesticDefaultDepartment.Department = TestObjectCreator.FIADepartment.PK;
			AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticRail.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, domesticDefaultDepartments);
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, GlbBranch.CurrentBranch.HomePort, GlbBranch.CurrentBranch.HomePort, Constants.TransportModes.Rail, Constants.ContainerModes.LCL, false);
			dbHitCountBefore = Db.Connection.ExecutedCommandCount;
			DepartmentChooser.GetDepartment(MockPlugIn.Object);
			dbHitCountAfter = Db.Connection.ExecutedCommandCount;
			dbHits = dbHitCountAfter - dbHitCountBefore;
			Assert("DB hit count must be 3 or less but it was " + dbHits, dbHits < 4);
		}

		public void TestDictionaryInitialisation()
		{
			AssertNotNull(DepartmentChooser);
		}

		public void TestDictionaryCrapData()
		{
			AssertEquals(ZGuid.Empty, DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.Shipment, "blah", "blah2", "blah3", "blah"));
		}

		public void TestShippingManager()
		{
			ZString overseasPort = "XXBBB";
			ZString localPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			ZGuid department;

			// BOL Export
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportContainerised.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportBreakBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportRoRo.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.Liquid);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportLiquid.Value, department);

			// BOL Import
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportContainerised.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportBreakBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportRoRo.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.Liquid);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportLiquid.Value, department);

			// BOL Domestic
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticContainerised.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticBreakBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticRoRo.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.Liquid);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticLiquid.Value, department);

			// BOL Other
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherContainerised.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherBreakBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherRoRo.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBillOfLading, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.Liquid);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherLiquid.Value, department);

			// Booking Export
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportContainerised.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportBreakBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportRoRo.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.Liquid);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingExportLiquid.Value, department);

			// Booking Import
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportContainerised.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportBreakBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportRoRo.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.Liquid);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingImportLiquid.Value, department);

			// Booking Domestic
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticContainerised.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticBreakBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticRoRo.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, localPort, localPort, Constants.TransportModes.Sea, Constants.ContainerModes.Liquid);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingDomesticLiquid.Value, department);

			// Booking Other
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherContainerised.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.BreakBulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherBreakBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.RollOnRollOff);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherRoRo.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.Bulk);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherBulk.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyBooking, overseasPort, overseasPort, Constants.TransportModes.Sea, Constants.ContainerModes.Liquid);
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingOtherLiquid.Value, department);

			// Shipping Voyage Accounting
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencyVoyageAccounting, "", "", "", "");
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingVoyageAccounting.Value, department);

			// Shipping Sundry Charges
			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.AgencySundryCharges, "", "", "", "");
			AssertEquals(AccountingConfigurationRegistry.Instance.ShippingSundryCharges.Value, department);
		}

		public void TestDictionaryForCFSShipment()
		{
			ZString origin = "XXBBB";
			ZString destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			ZGuid department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.CFSShipment, origin, destination, Constants.TransportModes.Road, Constants.ContainerModes.LCL);
			AssertEquals("CFS Shipment should have a CFS Default Department", AccountingConfigurationRegistry.Instance.CfsUnpackRoad.Value, department);
		}

		public void TestDictionary()
		{
			ZString origin = "XXBBB";
			ZString destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			ZGuid department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.Shipment, origin, destination, Constants.TransportModes.Road, Constants.ContainerModes.LCL);
			AssertEquals("CFS Shipment should have a CFS Default Department", AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportRoad.Value.GetDefaultDepartment(), department);
		}

		public void TestDictionaryForWarehouse()
		{
			ZString origin = "XXBBB";
			ZString destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			ZGuid department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.WarehouseInwards, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("WhsInwards default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.WarehouseOutwards, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("WhsOutwards default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.WarehouseStorage, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("WhsStorage default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.WarehouseStocktake, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("WhsStocktake default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("WarehouseAdHocServiceJob default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);
		}

		public void TestDictionaryForTransitWarehouse()
		{
			ZString origin = "XXBBB";
			ZString destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			ZGuid department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.TransitReceive, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("TransitReceive default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.TransitDispatch, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("TransitDispatch default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.TransitReceiveTransportationUnit, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("TransitReceiveTransportationUnit default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);

			department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.TransitDispatchTransportationUnit, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals("TransitDispatchTransportationUnit default incorrect", AccountingConfigurationRegistry.Instance.WarehouseDefaultDept.Value, department);
		}

		public void TestDefaultToLoginDepartmentForProject()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Project, null, null, "SEA", "LCL", true);
			var regItem = AccountingConfigurationRegistry.Instance.ProjectDefaultToCurrentLoginDept;
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertNotEquals("Default Department should have a non-current department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDictionaryForProject()
		{
			Guid expected = Guid.NewGuid();
			ZString origin = "XXBBB";
			ZString destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			AccountingConfigurationRegistry.Instance.ProjectDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.ProjectDefaultDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, expected);

			ZGuid department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.Project, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(expected, department);
		}

		public void TestDefaultToLoginDepartmentForWorkItem()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.WorkItem, null, null, "SEA", "LCL", true);
			var regItem = AccountingConfigurationRegistry.Instance.WorkItemDefaultToCurrentLoginDept;
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertNotEquals("Default Department should have a non-current Department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDictionaryForWorkItem()
		{
			Guid expected = Guid.NewGuid();
			ZString origin = "XXBBB";
			ZString destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			AccountingConfigurationRegistry.Instance.WorkItemDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.WorkitemDefaultDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, expected);

			ZGuid department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.WorkItem, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(expected, department);
		}

		public void TestDefaultToLoginDepartmentForWorkRequest()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.WorkRequest, null, null, "SEA", "LCL", true);
			var regItem = AccountingConfigurationRegistry.Instance.CustomerServiceTicketDefaultToCurrentLoginDept;
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertNotEquals("Default Department should have a non-current Department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDictionaryForWorkRequest()
		{
			var expected = Guid.NewGuid();
			var origin = "XXBBB";
			var destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			AccountingConfigurationRegistry.Instance.CustomerServiceTicketDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CustomerServiceTicketDefaultDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, expected);

			var department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.WorkRequest, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(expected, department);
		}

		public void TestDictionaryForCustomsTransitNCTS()
		{
			var origin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			var destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";

			var department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, true);
			AssertEquals("Origin is local, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureSeaFcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Sea, Constants.ContainerModes.LCL, true);
			AssertEquals("Origin is local, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureSeaLcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Air, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureAir.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Rail, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureRail.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Road, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureRoad.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Mail, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is local", AccountingConfigurationRegistry.Instance.NCTSDeparturePost.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Unknown, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureOther.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Other, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is local", AccountingConfigurationRegistry.Instance.NCTSArrival.Value, department);

			destination = "XXBBB";

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, true);
			AssertEquals("Origin is local, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureSeaFcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Sea, Constants.ContainerModes.LCL, true);
			AssertEquals("Origin is local, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureSeaLcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Air, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureAir.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Rail, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureRail.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Road, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureRoad.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Mail, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDeparturePost.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Unknown, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureOther.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Other, DepartmentChooser.Anything, true);
			AssertEquals("Origin is local, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSArrival.Value, department);

			origin = "XXBBB";

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, true);
			AssertEquals("Origin is foreign, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureSeaFcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Sea, Constants.ContainerModes.LCL, true);
			AssertEquals("Origin is foreign, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureSeaLcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Air, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureAir.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Rail, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureRail.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Road, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureRoad.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Mail, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDeparturePost.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Unknown, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSDepartureOther.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Other, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is foreign", AccountingConfigurationRegistry.Instance.NCTSArrival.Value, department);

			destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, true);
			AssertEquals("Origin is foreign, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureSeaFcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Sea, Constants.ContainerModes.LCL, true);
			AssertEquals("Origin is foreign, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureSeaLcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Air, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureAir.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Rail, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureRail.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Road, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureRoad.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Mail, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is local", AccountingConfigurationRegistry.Instance.NCTSDeparturePost.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Unknown, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is local", AccountingConfigurationRegistry.Instance.NCTSDepartureOther.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.CustomsTransitNCTS, origin, destination, Constants.TransportModes.Other, DepartmentChooser.Anything, true);
			AssertEquals("Origin is foreign, destination is local", AccountingConfigurationRegistry.Instance.NCTSArrival.Value, department);
		}

		public void TestDictionaryForImportDeclarationWithNoPorts()
		{
			ZGuid department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Sea, Constants.ContainerModes.FCL, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Sea, Constants.ContainerModes.LCL, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportSeaLcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Sea, DepartmentChooser.Anything, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Air, DepartmentChooser.Anything, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportAirUld.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Rail, DepartmentChooser.Anything, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportRail.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Road, DepartmentChooser.Anything, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportRoad.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Mail, DepartmentChooser.Anything, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportPost.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", DepartmentChooser.Post, DepartmentChooser.Anything, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportPost.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Other, DepartmentChooser.Anything, true);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportOther.Value, department);
		}

		public void TestDictionaryForExportDeclarationWithNoPorts()
		{
			ZGuid department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Sea, Constants.ContainerModes.FCL, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsExportSeaFcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Sea, Constants.ContainerModes.LCL, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsExportSeaLcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Sea, DepartmentChooser.Anything, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsExportSeaFcl.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Air, DepartmentChooser.Anything, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsExportAirUld.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Rail, DepartmentChooser.Anything, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsExportRail.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Road, DepartmentChooser.Anything, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsExportRoad.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Mail, DepartmentChooser.Anything, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsExportPost.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", DepartmentChooser.Post, DepartmentChooser.Anything, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsExportPost.Value, department);

			department = DepartmentChooser.GetDepartmentCore_ForTestOnly(JobInvoicingConsumerTypes.Brokerage, "", "", Constants.TransportModes.Other, DepartmentChooser.Anything, false);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsOther.Value, department);
		}

		public void TestGetDepartmentTakingCommunityRegionSettingsIntoAccount()
		{
			var regValue = new JobInvoicingDefaultDepartmentsCollection { new JobInvoicingDefaultDepartments(true) { ConsolType = "ALL", Department = RegistryConstants.DepartmentPKs.ClearanceExportAir } };
			using (AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignAirOther.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue))
			{
				var countryCN = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.China));
				var countryBE = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Constants.CountryCodes.Belgium));
				var countryKR = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.KoreaSouth));

				AssertDefaultDepartmentTakingCommunityRegionSettingsIntoAccount("KRSEL", "USLAX", Array.Empty<Guid>(), RegistryConstants.DepartmentPKs.ClearanceExportAir);
				AssertDefaultDepartmentTakingCommunityRegionSettingsIntoAccount("KRSEL", "USLAX", new[] { countryKR.PK.ToGuid() }, RegistryConstants.DepartmentPKs.ForwardingExportAir);
				AssertDefaultDepartmentTakingCommunityRegionSettingsIntoAccount("AUSYD", "BEANR", new[] { countryBE.PK.ToGuid() }, RegistryConstants.DepartmentPKs.ForwardingExportAir);
				AssertDefaultDepartmentTakingCommunityRegionSettingsIntoAccount("BEANR", "AUSYD", new[] { countryBE.PK.ToGuid() }, RegistryConstants.DepartmentPKs.ForwardingImportAir);
				AssertDefaultDepartmentTakingCommunityRegionSettingsIntoAccount("CNSAM", "BEANR", new[] { countryBE.PK.ToGuid(), countryCN.PK.ToGuid() }, RegistryConstants.DepartmentPKs.ClearanceExportAir);
				AssertDefaultDepartmentTakingCommunityRegionSettingsIntoAccount("BEANR", "CNSAM", new[] { countryBE.PK.ToGuid() }, RegistryConstants.DepartmentPKs.ForwardingExportAir);
				AssertDefaultDepartmentTakingCommunityRegionSettingsIntoAccount("CNSAM", "BEANR", new[] { countryBE.PK.ToGuid() }, RegistryConstants.DepartmentPKs.ForwardingImportAir);
			}
		}

		void AssertDefaultDepartmentTakingCommunityRegionSettingsIntoAccount(string origin, string destination, Guid[] communityRegionSetting, ZGuid expected)
		{
			FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, communityRegionSetting);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			var job = TestObjectCreator.CreateJob(shipment, false);
			var department = Factory.Load<GlbDepartment>(DepartmentChooser.GetDepartment(shipment));

			AssertEquals(expected, department.PK);
		}

		[ExpectNoExceptions]
		public void TestGetDepartmentCore_WithNullConsumerType()
		{
			var department = DepartmentChooser.GetDepartmentCore_ForTestOnly(null, "", "", Constants.TransportModes.Sea, Constants.ContainerModes.FCL, false);
			AssertEquals("Null ConsumerType must return empty-guid", ZGuid.Empty, department);
		}

		public void TestDefaultToLoginDepartmentForBrokerage()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Brokerage, null, null, "SEA", "LCL", true);
			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.Value, department);

			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDefaultToLoginDepartmentForISF()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.ImporterSecurityFiling, null, null, "11", ZString.Empty, true);
			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Declaration without Origin or Destination should have a Default Department", AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.Value, department);

			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDefaultToLoginDepartmentForCustomsTransitNCTS()
		{
			MockSupporter = new Mock<IJobInvoicingSupporter>();
			MockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.CustomsTransitNCTS);
			MockSupporter.Setup(m => m.TransportMode).Returns("11");
			MockSupporter.Setup(m => m.ContainerMode).Returns(ZString.Empty);
			MockSupporter.Setup(m => m.IsImport).Returns(true);
			MockSupporter.Setup(m => m.ConsolType).Returns("AGT");
			MockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);
			MockSupporter.Setup(m => m.IsNCTSPhase4).Returns(false);

			MockPlugIn = new Mock<IJobInvoicingPlugIn>();
			MockPlugIn.Setup(m => m.InvoicingSupporter).Returns(MockSupporter.Object);

			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.NCTSDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("NCTS is not phase 4 and NCTSDefaultToCurrentLoginDept is false, Default Department should not be login(current) department", ZGuid.Empty, department);

			MockSupporter.Setup(m => m.IsNCTSPhase4).Returns(true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("NCTS is phase 4 and CustomsDefaultToCurrentLoginDept is true, Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.NCTSDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			MockSupporter.Setup(m => m.IsNCTSPhase4).Returns(false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("NCTS is not phase 4 and NCTSDefaultToCurrentLoginDept is true, Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			MockSupporter.Setup(m => m.IsNCTSPhase4).Returns(true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("NCTS is phase 4 and CustomsDefaultToCurrentLoginDept is false, Default Department should not be login(current) department", ZGuid.Empty, department);
		}

		public void TestDefaultToLoginDepartmentForCustomsTemporaryStorage()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.CustomsTemporaryStorage, null, null, "11", ZString.Empty, true);
			AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDefaultToLoginDepartmentForShippingManager_AgencyBooking()
		{
			AssertDefaultToLoginDepartmentForShippingManager(JobInvoicingConsumerTypes.AgencyBooking, ZGuid.Empty);
		}

		public void TestDefaultToLoginDepartmentForShippingManager_AgencyBillOfLading()
		{
			AssertDefaultToLoginDepartmentForShippingManager(JobInvoicingConsumerTypes.AgencyBillOfLading, ZGuid.Empty);
		}

		public void TestDefaultToLoginDepartmentForShippingManager_AgencyDetentionInvoice()
		{
			AssertDefaultToLoginDepartmentForShippingManager(JobInvoicingConsumerTypes.AgencyDetentionInvoice, ZGuid.Empty);
		}

		public void TestDefaultToLoginDepartmentForShippingManager_AgencyVoyageAccounting()
		{
			AssertDefaultToLoginDepartmentForShippingManager(JobInvoicingConsumerTypes.AgencyVoyageAccounting, AccountingConfigurationRegistry.Instance.ShippingVoyageAccounting.Value);
		}

		public void TestDefaultToLoginDepartmentForShippingManager_AgencySundryCharges()
		{
			AssertDefaultToLoginDepartmentForShippingManager(JobInvoicingConsumerTypes.AgencySundryCharges, AccountingConfigurationRegistry.Instance.ShippingSundryCharges.Value);
		}

		void AssertDefaultToLoginDepartmentForShippingManager(JobInvoicingConsumerType consumerType, ZGuid expectedWhenNotFound)
		{
			SetUpPlugInAndSupporterMocks(consumerType, null, null, "SEA", "LCL", true);
			AccountingConfigurationRegistry.Instance.ShippingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default department should be current login department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.ShippingDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals(expectedWhenNotFound, department);

			AccountingConfigurationRegistry.Instance.ShippingDefaultToCurrentLoginDept.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default department should be current login department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDefaultToLoginDepartmentForCFS()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.CFSShipment, null, null, "SEA", "FCL", true);
			AccountingConfigurationRegistry.Instance.CfsDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.CfsDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("CFS Shipment without Origin or Destination should have an empty Department", ZGuid.Empty, department);

			AccountingConfigurationRegistry.Instance.CfsDefaultToCurrentLoginDept.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDefaultToLoginDepartmentForWarehouse()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.WarehouseStorage, null, null, "SEA", "LCL", true);
			AccountingConfigurationRegistry.Instance.WarehouseDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.WarehouseDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertNotEquals("Warehouse Storage should have a non-current Department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.WarehouseDefaultToCurrentLoginDept.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDefaultToLoginDepartmentForForwarding()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, null, null, "SEA", "LCL", true);
			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Freight Shipment should have an empty Department", ZGuid.Empty, department);

			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			using (Env.Instance.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
				AssertEquals("Freight Shipment should have an empty Department", ZGuid.Empty, department);
			}
		}

		public void TestDefaultToLoginDepartmentForForwardingImportRailShipments()
		{
			ZGuid fCLRailDefaultDepartment = TestObjectCreator.FESDepartment.PK;
			SetupImportFCLRailDefaultDepartment(fCLRailDefaultDepartment);

			ZGuid lCLRailDefaultDepartment = TestObjectCreator.FISDepartment.PK;
			SetupImportLCLRailDefaultDepartment(lCLRailDefaultDepartment);

			ZGuid systemDefaultDepartment = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportRail.Value[0].Department;
			AssertSystemRailNonModeSpecificRegistryDepartmentIsDifferentToTestedDepartments(systemDefaultDepartment);

			RefUNLOCO foreignUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, foreignUNLOCO, GlbBranch.CurrentBranch.HomePort, Constants.TransportModes.Rail, Constants.ContainerModes.Bulk, true);
			ZGuid chosenDepartment = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Configured department should default", systemDefaultDepartment, chosenDepartment);

			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, foreignUNLOCO, GlbBranch.CurrentBranch.HomePort, Constants.TransportModes.Rail, Constants.ContainerModes.LCL, true);
			chosenDepartment = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Configured department should default", lCLRailDefaultDepartment, chosenDepartment);

			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, foreignUNLOCO, GlbBranch.CurrentBranch.HomePort, Constants.TransportModes.Rail, Constants.ContainerModes.FCL, true);
			chosenDepartment = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Configured department should default", fCLRailDefaultDepartment, chosenDepartment);
		}

		public void TestDefaultToLoginDepartmentForForwardingExportRailShipments()
		{
			ZGuid fCLRailDefaultDepartment = TestObjectCreator.FESDepartment.PK;
			SetupExportFCLRailDefaultDepartment(fCLRailDefaultDepartment);

			ZGuid lCLRailDefaultDepartment = TestObjectCreator.FISDepartment.PK;
			SetupExportLCLRailDefaultDepartment(lCLRailDefaultDepartment);

			ZGuid systemDefaultDepartment = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRail.Value[0].Department;
			AssertSystemRailNonModeSpecificRegistryDepartmentIsDifferentToTestedDepartments(systemDefaultDepartment);

			RefUNLOCO foreignUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, GlbBranch.CurrentBranch.HomePort, foreignUNLOCO, Constants.TransportModes.Rail, Constants.ContainerModes.Bulk, false);
			ZGuid chosenDepartment = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Configured department should default", systemDefaultDepartment, chosenDepartment);

			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, GlbBranch.CurrentBranch.HomePort, foreignUNLOCO, Constants.TransportModes.Rail, Constants.ContainerModes.LCL, false);
			chosenDepartment = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Configured department should default", lCLRailDefaultDepartment, chosenDepartment);

			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, GlbBranch.CurrentBranch.HomePort, foreignUNLOCO, Constants.TransportModes.Rail, Constants.ContainerModes.FCL, false);
			chosenDepartment = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Configured department should default", fCLRailDefaultDepartment, chosenDepartment);
		}

		public void TestDefaultToLoginDepartmentForForwardingDomesticRailShipments()
		{
			JobInvoicingDefaultDepartmentsCollection domesticDefaultDepartments = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments domesticDefaultDepartment = domesticDefaultDepartments.AddNew();
			domesticDefaultDepartment.ConsolType = "ALL";
			domesticDefaultDepartment.Department = TestObjectCreator.FIADepartment.PK;
			AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingDomesticRail.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, domesticDefaultDepartments);

			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, GlbBranch.CurrentBranch.HomePort, GlbBranch.CurrentBranch.HomePort, Constants.TransportModes.Rail, Constants.ContainerModes.LCL, false);
			ZGuid chosenDepartment = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Configured department should default", TestObjectCreator.FIADepartment.PK, chosenDepartment);
		}

		public void TestDefaultToLoginDepartmentForForwardingForeignRailShipments()
		{
			JobInvoicingDefaultDepartmentsCollection domesticDefaultDepartments = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments domesticDefaultDepartment = domesticDefaultDepartments.AddNew();
			domesticDefaultDepartment.ConsolType = "ALL";
			domesticDefaultDepartment.Department = TestObjectCreator.FIADepartment.PK;
			AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingForeignRail.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, domesticDefaultDepartments);

			RefUNLOCO foreignUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.Shipment, foreignUNLOCO, foreignUNLOCO, Constants.TransportModes.Rail, Constants.ContainerModes.LCL, false);
			ZGuid chosenDepartment = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Configured department should default", TestObjectCreator.FIADepartment.PK, chosenDepartment);
		}

		void SetupExportLCLRailDefaultDepartment(ZGuid department)
		{
			JobInvoicingDefaultDepartmentsCollection lCLDefaultDepartments = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments lCLDefaultDepartment = lCLDefaultDepartments.AddNew();
			lCLDefaultDepartment.ConsolType = "ALL";
			lCLDefaultDepartment.Department = department;
			AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRailLCL.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lCLDefaultDepartments);
		}

		void SetupExportFCLRailDefaultDepartment(ZGuid department)
		{
			JobInvoicingDefaultDepartmentsCollection fCLDefaultDepartments = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments fCLDefaultDepartment = fCLDefaultDepartments.AddNew();
			fCLDefaultDepartment.ConsolType = "ALL";
			fCLDefaultDepartment.Department = department;
			AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingExportRailFCL.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fCLDefaultDepartments);
		}

		void AssertSystemRailNonModeSpecificRegistryDepartmentIsDifferentToTestedDepartments(ZGuid systemDefaultDepartment)
		{
			AssertNotEquals("System Default Department Shouldn't Be FIS for Rail", TestObjectCreator.FISDepartment.PK, systemDefaultDepartment);
			AssertNotEquals("System Default Department Shouldn't Be FES for Rail", TestObjectCreator.FESDepartment.PK, systemDefaultDepartment);
		}

		void SetupImportLCLRailDefaultDepartment(ZGuid department)
		{
			JobInvoicingDefaultDepartmentsCollection lCLDefaultDepartments = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments lCLDefaultDepartment = lCLDefaultDepartments.AddNew();
			lCLDefaultDepartment.ConsolType = "ALL";
			lCLDefaultDepartment.Department = department;
			AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportRailLCL.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lCLDefaultDepartments);
		}

		void SetupImportFCLRailDefaultDepartment(ZGuid department)
		{
			JobInvoicingDefaultDepartmentsCollection fCLDefaultDepartments = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments fCLDefaultDepartment = fCLDefaultDepartments.AddNew();
			fCLDefaultDepartment.ConsolType = "ALL";
			fCLDefaultDepartment.Department = department;
			AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportRailFCL.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fCLDefaultDepartments);
		}

		public void TestDefaultToLoginDepartmentForQuotedBooking()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.QuotedBooking, null, null, "SEA", "LCL", true);
			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Quoted Booking should have an empty Department", ZGuid.Empty, department);

			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			using (Env.Instance.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
				AssertEquals("Quoted Booking should have an empty Department", ZGuid.Empty, department);
			}
		}

		public void TestDefaultToLoginDepartmentForOneOffQuotation()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.OneOffQuotation, null, null, "SEA", "LCL", true);
			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("One Off Quotation should have an empty Department", ZGuid.Empty, department);

			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			using (Env.Instance.SetTemporaryUserContext(new UserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
				AssertEquals("One Off Quotation should have an empty Department", ZGuid.Empty, department);
			}
		}

		public void TestCustomsOtherDrawBack()
		{
			BaseJobDeclaration testBrokerage = Factory.New<BaseJobDeclaration>();
			testBrokerage.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			ZGuid department = DepartmentChooser.GetDepartment(testBrokerage);
			AssertEquals("Default Department should be Customs Other department", AccountingConfigurationRegistry.Instance.CustomsOther.Value, department);
		}

		public void TestCustomsOtherRefund()
		{
			BaseJobDeclaration testBrokerage = Factory.New<BaseJobDeclaration>();
			testBrokerage.JE_MessageType = JobMessageTypeList.Codes.Refund;

			ZGuid department = DepartmentChooser.GetDepartment(testBrokerage);
			AssertEquals("Default Department should be Customs Other department", AccountingConfigurationRegistry.Instance.CustomsOther.Value, department);
		}

		public void TestCustomsOtherMiscellaneous()
		{
			BaseJobDeclaration testBrokerage = Factory.New<BaseJobDeclaration>();
			testBrokerage.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;

			ZGuid department = DepartmentChooser.GetDepartment(testBrokerage);
			AssertEquals("Default Department should be Customs Other department", AccountingConfigurationRegistry.Instance.CustomsOther.Value, department);
		}

		public void TestCustomsExWarehouse()
		{
			BaseJobDeclaration testBrokerage = Factory.New<BaseJobDeclaration>();
			testBrokerage.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			ZGuid department = DepartmentChooser.GetDepartment(testBrokerage);
			AssertEquals("Default Department should be Customs Other department", AccountingConfigurationRegistry.Instance.CustomsExWarehouse.Value, department);
		}

		public void TestCustomsOtherExcise()
		{
			BaseJobDeclaration testBrokerage = Factory.New<BaseJobDeclaration>();
			testBrokerage.JE_MessageType = DepartmentChooser.Excise;

			ZGuid department = DepartmentChooser.GetDepartment(testBrokerage);
			AssertEquals("Default Department should be Customs Other department", AccountingConfigurationRegistry.Instance.CustomsOther.Value, department);
		}

		public void TestCusMAWB()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.CusMAWB, null, null, "SEA", "LCL", true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be Air Cargo department", AccountingConfigurationRegistry.Instance.AirCargo.Value, department);
		}

		public void TestCusUnderbond()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.CusUnderbond, null, null, "SEA", "LCL", true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be Air Cargo department", AccountingConfigurationRegistry.Instance.AirCargoOutturns.Value, department);
		}

		public void TestCTOCusMAWB()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.CTOCusMAWB, null, null, "SEA", "LCL", true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be Air Cargo department", AccountingConfigurationRegistry.Instance.AirCargoCTO.Value, department);
		}

		public void TestCTOCusImportHAWB()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.CTOCusImportHAWB, null, null, Constants.TransportModes.Air, "", true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default department should be Air Cargo Department", AccountingConfigurationRegistry.Instance.AirCargoCTO.Value, department);
		}

		public void TestCTOCusExportHAWB()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.CTOCusExportHAWB, null, null, Constants.TransportModes.Air, "", true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default department should be Air Cargo Department", AccountingConfigurationRegistry.Instance.AirCargoCTO.Value, department);
		}

		public void TestValidActiveDeptCodeFromCustomDeptChooser()
		{
			SetCustomDefaultDepartmentConfigInRegistry("return 'FEA'");
			ZGuid department = DepartmentChooser.GetDepartment(Shipment);
			AssertEquals("Default department should be based on Custom Department Chooser result", TestObjectCreator.FEADepartment.PK, department);
		}

		public void TestEmptyDeptCodeFromCustomDeptChooser()
		{
			SetCustomDefaultDepartmentConfigInRegistry("return obj.SomeNonExistingPropertName");
			AccountingConfigurationRegistry.Instance.ForwardingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZGuid department = DepartmentChooser.GetDepartment(Shipment);
			AssertEquals("Default department should be based on fallback", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestGetDepartmentBizObjFromDeptCode()
		{
			AssertEquals("Departments should match", TestObjectCreator.FESDepartment, DepartmentChooser.GetDepartmentBizObjFromDeptCode(Factory, "FES"));
			AssertEquals("Departments should match", null, DepartmentChooser.GetDepartmentBizObjFromDeptCode(Factory, "***"));
			AssertEquals("Departments should match", null, DepartmentChooser.GetDepartmentBizObjFromDeptCode(Factory, ZString.Empty));
			AssertEquals("Departments should match", null, DepartmentChooser.GetDepartmentBizObjFromDeptCode(null, "FES"));
		}

		public void TestInvalidOrInactiveDeptFromCustomDeptChooser()
		{
			TestObjectCreator.FEADepartment.GE_IsActive = false;
			SetCustomDefaultDepartmentConfigInRegistry("return 'FEA'");
			ZGuid department = DepartmentChooser.GetDepartment(Shipment);
			AssertEquals("Default department should be empty", ZGuid.Empty, department);

			SetCustomDefaultDepartmentConfigInRegistry("return 'FFF'");
			department = DepartmentChooser.GetDepartment(Shipment);
			AssertEquals("Default department should be empty", ZGuid.Empty, department);
		}

		void SetCustomDefaultDepartmentConfigInRegistry(ZString config)
		{
			CustomDefaultDepartmentConfiguration customDefaultDepartmentConfiguration = new CustomDefaultDepartmentConfiguration();
			customDefaultDepartmentConfiguration.ConfigAsString = config;
			AccountingConfigurationRegistry.Instance.CustomDefaultDepartmentConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, customDefaultDepartmentConfiguration);
		}

		[ExpectNoExceptions]
		public void TestNoDuplicateDictionaryEntry()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.CTOCusMAWB, null, null, "SEA", "LCL", true);
			ZGuid department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			Assert(!department.IsEmpty);
		}

		public void TestDictionaryForContainerYard()
		{
			ZString origin = "XXBBB";
			ZString destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";

			var departmentPK = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.CYDReceiveAdvice, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.Value, departmentPK);

			var department = Factory.Load<GlbDepartment>(departmentPK);
			AssertNotNull(department);
			AssertEquals("YDJ", department.GE_Code);
			AssertEquals("Container Yard Jobs", department.GE_Desc);
			AssertEquals("Container Yard", department.GE_Activity);
			AssertEquals("Domestic", department.GE_Direction);
			AssertEquals("Other", department.GE_Mode);

			departmentPK = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.CYDReceiveAdvice, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.Value, departmentPK);

			departmentPK = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.CYDReleaseAdvice, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.Value, departmentPK);

			departmentPK = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.CYDTransportationUnit, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.Value, departmentPK);

			departmentPK = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.CYDAdHocServiceOrder, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.Value, departmentPK);

			departmentPK = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.MNRWorkOrderHeader, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.Value, departmentPK);

			departmentPK = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.CYDPeriodicInvoicing, origin, destination, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind);
			AssertEquals(AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.Value, departmentPK);
		}

		public void TestGetDepartment_FCLStorage()
		{
			var expectedDepartment = Factory.New<GlbDepartment>();
			expectedDepartment.GE_Code = "XXX";
			expectedDepartment.GE_Desc = "Some department set in the registry";

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.ContainerYardJobsDefaultDept.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedDepartment.PK.ToGuid()))
			{
				SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.FCLStorage, null, null, Constants.TransportModes.All, Constants.ContainerModes.FreightAllKind, true);
				var department = DepartmentChooser.GetDepartment(MockPlugIn.Object);

				AssertEquals(
					"FCLStorage consumer type should default to ContainerYardJobsDefaultDept registry value",
					expectedDepartment.PK,
					department
				);
			}
		}

		public void TestShipmentGetDepartment_NoExceptionThrown()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			Factory.Save();

			AssertNotNull(shipment.InvoicingSupporter);
			AssertNotNull(shipment.InvoicingSupporter.Job);
			using (AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNoExceptionThrown("No Exception Thrown on Shipment", () => DepartmentChooser.GetDepartment(shipment));
			}

			using (AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNoExceptionThrown("No Exception Thrown on Shipment", () => DepartmentChooser.GetDepartment(shipment));
			}
		}

		public void TestBookingGetDepartment_NoExceptionThrown()
		{
			var trackingBooking = QuotedBooking.CreateNewBooking(Factory);
			var bookingJob = Factory.NewJobForTesting<JobHeader>();
			bookingJob.JH_JobNum = "S001001";
			bookingJob.JH_ParentID = trackingBooking.PK;
			Factory.Save();

			AssertNotNull(trackingBooking.InvoicingSupporter);
			AssertNotNull(trackingBooking.InvoicingSupporter.Job);
			using (AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNoExceptionThrown("No Exception Thrown on Tracking Booking", () => DepartmentChooser.GetDepartment(trackingBooking));
			}

			using (AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNoExceptionThrown("No Exception Thrown on Tracking Booking", () => DepartmentChooser.GetDepartment(trackingBooking));
			}
		}

		public void TestCustomDefaultingRuleEngineIsInvokedWhenRegistryIsTurnedOn()
		{
			var jobPlugin = Shipment;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = jobPlugin.PK;

			SetCustomDefaultDepartmentConfigInRegistry("return 'FEA'");

			var defaultDepartment = ZGuid.NewZGuid();

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			var departmentDefaultingManagerMock = new Mock<IJobBillingDepartmentDefaultingManager>();
			departmentDefaultingManagerMock.Setup(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment));
			departmentDefaultingManagerMock.Setup(x => x.DefaultValue).Returns(defaultDepartment);

			ObjectFactory.Substitute(departmentDefaultingManagerMock.Object);

			AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var result = DepartmentChooser.GetDepartment(jobPlugin);
			AssertEquals("Falls back to Python version", TestObjectCreator.FEADepartment.PK, result);

			departmentDefaultingManagerMock.Verify(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment), Times.Never);
			departmentDefaultingManagerMock.Verify(x => x.DefaultValue, Times.Never);

			AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			result = DepartmentChooser.GetDepartment(jobPlugin);
			AssertEquals("Picks up the value from rules engine", defaultDepartment, result);

			departmentDefaultingManagerMock.Verify(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment), Times.Once);
			departmentDefaultingManagerMock.Verify(x => x.DefaultValue, Times.Exactly(2));
		}

		public void TestCustomDefaultingRuleEngineReturnsNullValue()
		{
			var jobPlugin = Shipment;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = jobPlugin.PK;

			SetCustomDefaultDepartmentConfigInRegistry("return 'FEA'");

			var result = DepartmentChooser.GetDepartment(jobPlugin);
			AssertEquals("Uses Python version", TestObjectCreator.FEADepartment.PK, result);

			var companyMock = new Mock<ICompany>();
			var branchMock = new Mock<IBranch>();
			var departmentMock = new Mock<IDepartment>();
			var departmentDefaultingManagerMock = new Mock<IJobBillingDepartmentDefaultingManager>();
			departmentDefaultingManagerMock.Setup(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment));
			departmentDefaultingManagerMock.Setup(x => x.DefaultValue).Returns((IZType)null);

			ObjectFactory.Substitute(departmentDefaultingManagerMock.Object);

			AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			result = DepartmentChooser.GetDepartment(jobPlugin);
			AssertEquals("Falls back to Python version", TestObjectCreator.FEADepartment.PK, result);

			departmentDefaultingManagerMock.Verify(x => x.SetDefaultValue(jobPlugin, Env.CurrentCompany, Env.CurrentBranch, Env.CurrentDepartment), Times.Once);
			departmentDefaultingManagerMock.Verify(x => x.DefaultValue, Times.Exactly(1));
		}

		public void TestCustomeDefaultingRuleEngine_FallsBackToEmptyDepartmentIfNoRulesFound()
		{
			AccountingConfigurationRegistry.Instance.CustomDepartmentDefaultingRuleEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var localClient = TestObjectCreator.ABIGAS;
			var context = "JDE";
			var expectedDepartment = TestObjectCreator.FIADepartment;

			var subContext = RulesContextSubType.ShipmentJob.GetCode();

			var ruleSet1 = TestObjectCreator.CreateProductionRuleSet(context, subContext, 1);
			TestObjectCreator.CreateProductionRules(ruleSet1, context, 1, "LocalClient", localClient.PK, expectedDepartment.PK);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			var job = TestObjectCreator.CreateJob(shipment, false, localClientOrg: localClient);

			var result = DepartmentChooser.GetDepartment(shipment);

			AssertEquals("FIA should be returned as department as rulset1 was satisfied", expectedDepartment.PK, result);

			job.JH_OA_LocalChargesAddr = TestObjectCreator.AALSHI.MainAddress.PK;

			result = DepartmentChooser.GetDepartment(shipment);

			AssertEquals("Empty value should be returned as department as no rule is satisfied", ZGuid.Empty, result);
		}

		public void TestDefaultToLoginDepartmentForTransportBooking()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.TransportBooking, null, null, "SEA", "LCL", true);
			var regItem = AccountingConfigurationRegistry.Instance.TransportBookingDefaultToCurrentLoginDept;
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertNotEquals("Default Department should have a non-current Department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDepartmentForTransportBooking()
		{
			var defaultDepartment = Guid.NewGuid();
			var origin = "XXBBB";
			var destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			AccountingConfigurationRegistry.Instance.TransportBookingDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.TransportBookingJobsDefaultDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultDepartment);

			var department = DepartmentChooser.GetDepartment(JobInvoicingConsumerTypes.TransportBooking, origin, destination, DepartmentChooser.Anything, Constants.ContainerModes.FreightAllKind);
			AssertEquals(defaultDepartment, department);
		}

		public void TestDefaultToLoginDepartmentForLandTransport()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.TransportConsignment, null, null, "SEA", "LCL", true);
			var regItem = AccountingConfigurationRegistry.Instance.LandTransportDefaultToCurrentLoginDept;
			regItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var department = DepartmentChooser.GetDepartment(MockPlugIn.Object);

			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var defaultDepartment = AccountingConfigurationRegistry.Instance.LandTransportJobsDefaultDept.Value;
			AssertNotEquals("Precondition: Default department from registry should not be equal to the current department", defaultDepartment, GlbDepartment.CurrentDepartment.PK.ToGuid());

			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);

			AssertNotEquals("Default Department should have a non-current Department", GlbDepartment.CurrentDepartment.PK, department);

			regItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			department = DepartmentChooser.GetDepartment(MockPlugIn.Object);

			AssertEquals("Default Department should be login(current) department", GlbDepartment.CurrentDepartment.PK, department);
		}

		public void TestDepartmentForLandTransport()
		{
			SetUpPlugInAndSupporterMocks(JobInvoicingConsumerTypes.TransportConsignment, null, null, "SEA", "LCL", true);
			var defaultDepartment = Guid.NewGuid();
			AccountingConfigurationRegistry.Instance.LandTransportDefaultToCurrentLoginDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.LandTransportJobsDefaultDept.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultDepartment);

			var department = DepartmentChooser.GetDepartment(MockPlugIn.Object);
			AssertEquals(defaultDepartment, department);
		}

		Mock<IJobInvoicingPlugIn> MockPlugIn;
		Mock<IJobInvoicingSupporter> MockSupporter;

		IJobInvoicingPlugIn Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = TestObjectCreator.CreateShipment("S00001000");
				}
				return shipment;
			}
		}
		IJobInvoicingPlugIn shipment;
		void SetUpPlugInAndSupporterMocks(JobInvoicingConsumerType consumerType, RefUNLOCO origin, RefUNLOCO destination, ZString transportMode, ZString containerMode, bool isImport)
		{
			MockSupporter = new Mock<IJobInvoicingSupporter>();
			MockSupporter.Setup(m => m.ConsumerType).Returns(consumerType);
			MockSupporter.Setup(m => m.Origin).Returns(origin);
			MockSupporter.Setup(m => m.Destination).Returns(destination);
			MockSupporter.Setup(m => m.TransportMode).Returns(transportMode);
			MockSupporter.Setup(m => m.ContainerMode).Returns(containerMode);
			MockSupporter.Setup(m => m.IsImport).Returns(isImport);
			MockSupporter.Setup(m => m.ConsolType).Returns("AGT");
			MockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.Empty);

			MockPlugIn = new Mock<IJobInvoicingPlugIn>();
			MockPlugIn.Setup(m => m.InvoicingSupporter).Returns(MockSupporter.Object);
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		DepartmentChooser fDepartmentChooser;
		protected DepartmentChooser DepartmentChooser
		{
			get
			{
				if (fDepartmentChooser == null)
				{
					fDepartmentChooser = DepartmentChooser.New(TestObjectCreator.Factory);
				}
				return fDepartmentChooser;
			}
		}
	}
}
