using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Orders;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Orders.Testing
{
	[TestedType(typeof(GetCYContainerLoadListLinesWithSecurityContext))]
	class GetCYContainerLoadListLinesWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsContainerLoadListLinesColumns()
		{
			var fromContainerLoadListLine = GetColumnNames("SELECT * FROM dbo.ContainerLoadListLine");
			var fromTvf = GetColumnNames("SELECT * FROM GetCYContainerLoadListLinesWithSecurityContext(NEWID())");

			var missingColumns = fromContainerLoadListLine.Except(fromTvf.Append("CLL_LoadMode"));
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestGetCYContainerLoadListLines_SupplierWhenLoadListPartyIsSupplier()
		{
			SetupOrganisations();
			SetupOtherNoisyTestData();

			(var loadListLineINCPK, var loadListLineREJPK, var loadListLinePLCPK, var loadListLineAPPPK, var loadListLineSHPPK, var loadListLineCANPK, var loadListLineCNVPK) = SetupTestData(supplierPK);

			AssertContainsExactElementsInAnyOrder(new[] { (loadListLineINCPK, true), (loadListLineREJPK, true), (loadListLinePLCPK, false), (loadListLineAPPPK, false), (loadListLineSHPPK, false), (loadListLineCANPK, false), (loadListLineCNVPK, false) }, GetCYContainerLoadListLinePKsAndCanEditFlags(supplierPK));
		}

		public void TestGetCYContainerLoadListLines_SupplierWhenLoadListPartyIsManufacturer()
		{
			SetupOrganisations();
			SetupOtherNoisyTestData();

			(var loadListLineINCPK, var loadListLineREJPK, var loadListLinePLCPK, var loadListLineAPPPK, var loadListLineSHPPK, var loadListLineCANPK, var loadListLineCNVPK) = SetupTestData(manufacturerPK);

			AssertContainsExactElementsInAnyOrder(new[] { (loadListLineINCPK, true), (loadListLineREJPK, true), (loadListLinePLCPK, false), (loadListLineAPPPK, false), (loadListLineSHPPK, false), (loadListLineCANPK, false), (loadListLineCNVPK, false) }, GetCYContainerLoadListLinePKsAndCanEditFlags(supplierPK));
		}

		public void TestGetCYContainerLoadListLines_ManufacturerWhenLoadListPartyIsSupplier()
		{
			SetupOrganisations();
			SetupOtherNoisyTestData();

			SetupTestData(supplierPK);

			AssertContainsExactElementsInAnyOrder(Array.Empty<(Guid, bool)>(), GetCYContainerLoadListLinePKsAndCanEditFlags(manufacturerPK));
		}

		public void TestGetCYContainerLoadListLines_ManufacturerWhenLoadListPartyIsManufacturer()
		{
			SetupOrganisations();
			SetupOtherNoisyTestData();

			(var loadListLineINCPK, var loadListLineREJPK, var loadListLinePLCPK, var loadListLineAPPPK, var loadListLineSHPPK, var loadListLineCANPK, var loadListLineCNVPK) = SetupTestData(manufacturerPK);

			AssertContainsExactElementsInAnyOrder(new[] { (loadListLineINCPK, true), (loadListLineREJPK, true), (loadListLinePLCPK, false), (loadListLineAPPPK, false), (loadListLineSHPPK, false), (loadListLineCANPK, false), (loadListLineCNVPK, false) }, GetCYContainerLoadListLinePKsAndCanEditFlags(manufacturerPK));
		}

		public void TestGetCYContainerLoadListLines_ControllingCustomerWhenLoadListPartyIsSupplier()
		{
			SetupOrganisations();
			SetupOtherNoisyTestData();

			(var loadListLineINCPK, var loadListLineREJPK, var loadListLinePLCPK, var loadListLineAPPPK, var loadListLineSHPPK, var loadListLineCANPK, var loadListLineCNVPK) = SetupTestData(supplierPK);

			AssertContainsExactElementsInAnyOrder(new[] { (loadListLineINCPK, false), (loadListLineREJPK, false), (loadListLinePLCPK, false), (loadListLineAPPPK, false), (loadListLineSHPPK, false), (loadListLineCANPK, false), (loadListLineCNVPK, false) }, GetCYContainerLoadListLinePKsAndCanEditFlags(controllingCustomerPK));
		}

		public void TestGetCYContainerLoadListLines_ControllingCustomerWhenLoadListPartyIsManufacturer()
		{
			SetupOrganisations();
			SetupOtherNoisyTestData();

			(var loadListLineINCPK, var loadListLineREJPK, var loadListLinePLCPK, var loadListLineAPPPK, var loadListLineSHPPK, var loadListLineCANPK, var loadListLineCNVPK) = SetupTestData(manufacturerPK);

			AssertContainsExactElementsInAnyOrder(new[] { (loadListLineINCPK, false), (loadListLineREJPK, false), (loadListLinePLCPK, false), (loadListLineAPPPK, false), (loadListLineSHPPK, false), (loadListLineCANPK, false), (loadListLineCNVPK, false) }, GetCYContainerLoadListLinePKsAndCanEditFlags(controllingCustomerPK));
		}

		public void TestGetCYContainerLoadListLines_BuyerWhenLoadListPartyIsSupplier()
		{
			SetupOrganisations();
			SetupOtherNoisyTestData();

			(var loadListLineINCPK, var loadListLineREJPK, var loadListLinePLCPK, var loadListLineAPPPK, var loadListLineSHPPK, var loadListLineCANPK, var loadListLineCNVPK) = SetupTestData(supplierPK);

			AssertContainsExactElementsInAnyOrder(new[] { (loadListLineINCPK, false), (loadListLineREJPK, false), (loadListLinePLCPK, false), (loadListLineAPPPK, false), (loadListLineSHPPK, false), (loadListLineCANPK, false), (loadListLineCNVPK, false) }, GetCYContainerLoadListLinePKsAndCanEditFlags(buyerPK));
		}

		public void TestGetCYContainerLoadListLines_BuyerWhenLoadListPartyIsManufacturer()
		{
			SetupOrganisations();
			SetupOtherNoisyTestData();

			(var loadListLineINCPK, var loadListLineREJPK, var loadListLinePLCPK, var loadListLineAPPPK, var loadListLineSHPPK, var loadListLineCANPK, var loadListLineCNVPK) = SetupTestData(manufacturerPK);

			AssertContainsExactElementsInAnyOrder(new[] { (loadListLineINCPK, false), (loadListLineREJPK, false), (loadListLinePLCPK, false), (loadListLineAPPPK, false), (loadListLineSHPPK, false), (loadListLineCANPK, false), (loadListLineCNVPK, false) }, GetCYContainerLoadListLinePKsAndCanEditFlags(buyerPK));
		}

		public void TestGetContainerLoadListLines_NoDuplicateRows()
		{
			SetupOrganisations();

			var containerPK = TestDataCreator.CreateJobContainer("JC000", null);
			var orderPK = TestDataCreator.CreateJobOrderHeader("ORD000", buyerAddressPK, supplierAddress: supplierAddressPK);
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK, 1);
			var bookingHeader1PK = TestDataCreator.CreateJobSupplierBooking("SB000", supplierPK, status: "INC");
			var bookingLinePK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, bookingHeader1PK, "JSL001");
			var loadListHeader11PK = TestDataCreator.CreateCYContainerLoadList("CLH001", bookingHeader1PK, manufacturerPK, "INC");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader11PK, bookingLinePK, containerPK, null);

			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeader1PK, "JSB", "SCP", 0);
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeader1PK, "JSB", "SCP", 1);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", bookingHeader1PK, "JSB", "SUD", 0);
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", bookingHeader1PK, "JSB", "SUD", 1);

			var results = GetResults(manufacturerPK).Select();
			AssertEquals("Should only return 1 container load list line with context attached", 1, results.Length);
		}

		#region Implementation

		Guid buyerPK;
		Guid supplierPK;
		Guid manufacturerPK;
		Guid controllingCustomerPK;
		Guid buyerAddressPK;
		Guid supplierAddressPK;
		Guid manufacturerAddressPK;
		Guid controllingCustomerAddressPK;

		(Guid loadListLineINCPK, Guid loadListLineREJPK, Guid loadListLinePLCPK, Guid loadListLineAPPPK, Guid loadListLineSHPPK, Guid loadListLineCANPK, Guid loadListLineCNVPK) SetupTestData(Guid loadListParty)
		{
			var containerPK = TestDataCreator.CreateJobContainer("JC000", null);
			var orderPK = TestDataCreator.CreateJobOrderHeader("ORD000", buyerAddressPK, supplierAddress: supplierAddressPK);
			var orderLinePK = TestDataCreator.CreateJobOrderLine(orderPK, 1);
			var bookingHeader1PK = TestDataCreator.CreateJobSupplierBooking("SB000", supplierPK, status: "INC");
			var bookingLinePK = TestDataCreator.CreateJobSupplierBookingLine(orderLinePK, bookingHeader1PK, "JSL002");
			var loadListHeader11PK = TestDataCreator.CreateCYContainerLoadList("CLH001", bookingHeader1PK, loadListParty, "INC");
			var loadListLine11PK = TestDataCreator.CreateCYContainerLoadListLine(loadListHeader11PK, bookingLinePK, containerPK, null);
			var loadListHeader12PK = TestDataCreator.CreateCYContainerLoadList("CLH002", bookingHeader1PK, loadListParty, "REJ");
			var loadListLine12PK = TestDataCreator.CreateCYContainerLoadListLine(loadListHeader12PK, bookingLinePK, containerPK, null);
			var loadListHeader13PK = TestDataCreator.CreateCYContainerLoadList("CLH003", bookingHeader1PK, loadListParty, "PLC");
			var loadListLine13PK = TestDataCreator.CreateCYContainerLoadListLine(loadListHeader13PK, bookingLinePK, containerPK, null);
			var loadListHeader14PK = TestDataCreator.CreateCYContainerLoadList("CLH004", bookingHeader1PK, loadListParty, "APP");
			var loadListLine14PK = TestDataCreator.CreateCYContainerLoadListLine(loadListHeader14PK, bookingLinePK, containerPK, null);
			var loadListHeader15PK = TestDataCreator.CreateCYContainerLoadList("CLH005", bookingHeader1PK, loadListParty, "SHP");
			var loadListLine15PK = TestDataCreator.CreateCYContainerLoadListLine(loadListHeader15PK, bookingLinePK, containerPK, null);
			var loadListHeader16PK = TestDataCreator.CreateCYContainerLoadList("CLH006", bookingHeader1PK, loadListParty, "CAN");
			var loadListLine16PK = TestDataCreator.CreateCYContainerLoadListLine(loadListHeader16PK, bookingLinePK, containerPK, null);
			var loadListHeader17PK = TestDataCreator.CreateCYContainerLoadList("CLH007", bookingHeader1PK, loadListParty, "CNV");
			var loadListLine17PK = TestDataCreator.CreateCYContainerLoadListLine(loadListHeader17PK, bookingLinePK, containerPK, null);

			TestDataCreator.CreateDocAddress(supplierAddressPK, "", bookingHeader1PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(controllingCustomerAddressPK, "", bookingHeader1PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(manufacturerAddressPK, "", bookingLinePK, "JSL", "MAN");

			return (loadListLine11PK, loadListLine12PK, loadListLine13PK, loadListLine14PK, loadListLine15PK, loadListLine16PK, loadListLine17PK);
		}

		void SetupOtherNoisyTestData()
		{
			var buyer1PK = TestDataCreator.CreateOrganisation("ORG_BUY1", "Buyer");
			var supplier1PK = TestDataCreator.CreateOrganisation("ORG_SUP1", "Supplier");
			var manufacturer1PK = TestDataCreator.CreateOrganisation("ORG_MAN1", "Manufacturer");
			var controllingCustomer1PK = TestDataCreator.CreateOrganisation("ORG_CC1", "Controlling Customer");

			var buyerAddress1PK = TestDataCreator.CreateAddress(buyer1PK, "ORG_BUY1", "1 buyer st");
			var supplierAddress1PK = TestDataCreator.CreateAddress(supplier1PK, "ORG_SUP1", "1 supplier st");
			var manufacturerAddress1PK = TestDataCreator.CreateAddress(manufacturer1PK, "ORG_MAN1", "1 manufacturer st");
			var controllingCustomerAddress1PK = TestDataCreator.CreateAddress(controllingCustomer1PK, "ORG_CC1", "1 controllingCustomer st");

			var container1PK = TestDataCreator.CreateJobContainer("JC001", null);

			var order1PK = TestDataCreator.CreateJobOrderHeader("ORD001", buyerAddress1PK, supplierAddress: supplierAddress1PK);
			var orderLine1PK = TestDataCreator.CreateJobOrderLine(order1PK, 1);
			var bookingHeader1PK = TestDataCreator.CreateJobSupplierBooking("SB001", supplier1PK, status: "INC");
			var bookingLine11PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine1PK, bookingHeader1PK, "JSL003");
			var loadListHeader11PK = TestDataCreator.CreateCYContainerLoadList("CLH101", bookingHeader1PK, manufacturer1PK, "INC");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader11PK, bookingLine11PK, container1PK, null);
			var loadListHeader12PK = TestDataCreator.CreateCYContainerLoadList("CLH102", bookingHeader1PK, manufacturer1PK, "REJ");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader12PK, bookingLine11PK, container1PK, null);
			var loadListHeader13PK = TestDataCreator.CreateCYContainerLoadList("CLH103", bookingHeader1PK, manufacturer1PK, "PLC");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader13PK, bookingLine11PK, container1PK, null);
			var loadListHeader14PK = TestDataCreator.CreateCYContainerLoadList("CLH104", bookingHeader1PK, manufacturer1PK, "APP");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader14PK, bookingLine11PK, container1PK, null);
			var loadListHeader15PK = TestDataCreator.CreateCYContainerLoadList("CLH105", bookingHeader1PK, manufacturer1PK, "SHP");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader15PK, bookingLine11PK, container1PK, null);
			var loadListHeader16PK = TestDataCreator.CreateCYContainerLoadList("CLH106", bookingHeader1PK, manufacturer1PK, "CAN");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader16PK, bookingLine11PK, container1PK, null);
			var loadListHeader17PK = TestDataCreator.CreateCYContainerLoadList("CLH107", bookingHeader1PK, manufacturer1PK, "CNV");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader17PK, bookingLine11PK, container1PK, null);

			TestDataCreator.CreateDocAddress(supplierAddress1PK, "", bookingHeader1PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(controllingCustomerAddress1PK, "", bookingHeader1PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(manufacturerAddress1PK, "", bookingLine11PK, "JSL", "MAN");

			var buyer2PK = TestDataCreator.CreateOrganisation("ORG_BUY2", "Buyer");
			var supplier2PK = TestDataCreator.CreateOrganisation("ORG_SUP2", "Supplier");
			var manufacturer2PK = TestDataCreator.CreateOrganisation("ORG_MAN2", "Manufacturer");
			var controllingCustomer2PK = TestDataCreator.CreateOrganisation("ORG_CC2", "Controlling Customer");

			var buyerAddress2PK = TestDataCreator.CreateAddress(buyer2PK, "ORG_BUY2", "1 buyer st");
			var supplierAddress2PK = TestDataCreator.CreateAddress(supplier2PK, "ORG_SUP2", "1 supplier st");
			var manufacturerAddress2PK = TestDataCreator.CreateAddress(manufacturer2PK, "ORG_MAN21", "1 manufacturer st");
			var controllingCustomerAddress2PK = TestDataCreator.CreateAddress(controllingCustomer2PK, "ORG_CC21", "1 controllingCustomer st");

			var container2PK = TestDataCreator.CreateJobContainer("JC002", null);

			var order2PK = TestDataCreator.CreateJobOrderHeader("ORD002", buyerAddress2PK, supplierAddress: supplierAddress2PK);
			var orderLine21PK = TestDataCreator.CreateJobOrderLine(order2PK, 1);
			var bookingHeader2PK = TestDataCreator.CreateJobSupplierBooking("SB002", supplier2PK, status: "INC");
			var bookingLine2PK = TestDataCreator.CreateJobSupplierBookingLine(orderLine21PK, bookingHeader2PK, "JSL004");
			var loadListHeader21PK = TestDataCreator.CreateCYContainerLoadList("CLH201", bookingHeader2PK, supplier2PK, "INC");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader21PK, bookingLine2PK, container2PK, null);
			var loadListHeader22PK = TestDataCreator.CreateCYContainerLoadList("CLH202", bookingHeader2PK, supplier2PK, "REJ");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader22PK, bookingLine2PK, container2PK, null);
			var loadListHeader23PK = TestDataCreator.CreateCYContainerLoadList("CLH203", bookingHeader2PK, supplier2PK, "PLC");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader23PK, bookingLine2PK, container2PK, null);
			var loadListHeader24PK = TestDataCreator.CreateCYContainerLoadList("CLH204", bookingHeader2PK, supplier2PK, "APP");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader24PK, bookingLine2PK, container2PK, null);
			var loadListHeader25PK = TestDataCreator.CreateCYContainerLoadList("CLH205", bookingHeader2PK, supplier2PK, "SHP");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader25PK, bookingLine2PK, container2PK, null);
			var loadListHeader26PK = TestDataCreator.CreateCYContainerLoadList("CLH206", bookingHeader2PK, supplier2PK, "CAN");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader26PK, bookingLine2PK, container2PK, null);
			var loadListHeader27PK = TestDataCreator.CreateCYContainerLoadList("CLH207", bookingHeader2PK, supplier2PK, "CNV");
			TestDataCreator.CreateCYContainerLoadListLine(loadListHeader27PK, bookingLine2PK, container2PK, null);

			TestDataCreator.CreateDocAddress(supplierAddress2PK, "", bookingHeader2PK, "JSB", "SUD");
			TestDataCreator.CreateDocAddress(controllingCustomerAddress2PK, "", bookingHeader2PK, "JSB", "SCP");
			TestDataCreator.CreateDocAddress(manufacturerAddress2PK, "", bookingLine2PK, "JSL", "MAN");
		}

		void SetupOrganisations()
		{
			buyerPK = TestDataCreator.CreateOrganisation("ORG_BUY", "Buyer");
			supplierPK = TestDataCreator.CreateOrganisation("ORG_SUP", "Supplier");
			manufacturerPK = TestDataCreator.CreateOrganisation("ORG_MAN", "Manufacturer");
			controllingCustomerPK = TestDataCreator.CreateOrganisation("ORG_CC", "Controlling Customer");
			buyerAddressPK = TestDataCreator.CreateAddress(buyerPK, "ORG_BUY", "1 buyer st");
			supplierAddressPK = TestDataCreator.CreateAddress(supplierPK, "ORG_SUP", "1 supplier st");
			manufacturerAddressPK = TestDataCreator.CreateAddress(manufacturerPK, "ORG_MAN", "1 manufacturer st");
			controllingCustomerAddressPK = TestDataCreator.CreateAddress(controllingCustomerPK, "ORG_CC", "1 controllingCustomer st");
		}

		IEnumerable<string> GetColumnNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader.GetSchemaTable().Select().Select(x => (string)x["ColumnName"]);
			}
		}

		DataTable GetResults(Guid orgPK)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT results.* FROM dbo.GetCYContainerLoadListLinesWithSecurityContext('{orgPK}') results");
		}

		(Guid, bool)[] GetCYContainerLoadListLinePKsAndCanEditFlags(Guid orgPK) =>
			GetResults(orgPK)
			.Rows
			.Cast<DataRow>()
			.Select(row => (
				row.Field<Guid>("CLL_PK"),
				row.Field<bool>("CanEdit")
			))
			.ToArray();

		#endregion
	}
}
