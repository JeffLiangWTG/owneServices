using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.SWT.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class SWTClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestModuleOverrides()
		{
			ModuleOverrides moduleOverrides = ClientOverride.Instance.ModuleOverrides;
			AssertNotNull("ModuleOverrides should not be null", moduleOverrides);
			AssertNotNull("ModulesOverries should contain Orders", moduleOverrides[ModuleIDs.Orders, GlbCompany.CurrentCompany.GC_RN_NKCountryCode]);
			AssertEquals("SWTOrdersModuleOverride", typeof(SWTOrderModuleOverride).FullName, moduleOverrides[ModuleIDs.Orders, GlbCompany.CurrentCompany.GC_RN_NKCountryCode].TypePath.Split(',')[0]);
		}

		#region DBScriptsTest
		public class DBScriptsTest : TestCaseWithFactory
		{
			public void TestClientOrgsForNotYetArrived()
			{
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.ConsigneePK = consignee1.PK;
				shipment.ConsignorPK = consignor1.PK;
				shipment.JS_E_ARV = testDate.AddDays(3);
				shipment.JS_E_DEP = testDate.AddDays(-2);
				var consol = shipment.Consols.AddNew();
				SetupTransportLeg(consol.Transports[0], "boat1", "NZAKL", "HKHKG", testDate.AddDays(-3), testDate.AddDays(-2));
				SetupTransportLeg(consol.Transports.AddNew(), "boat2", "HKHKG", "GBWLV", testDate.AddDays(-1), testDate.AddDays(2));
				SetupTransportLeg(consol.Transports.AddNew(), "boat3", "GBWLV", "AUBNE", testDate.AddDays(3), testDate.AddDays(4));
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(twentyFootContainer, shipment, consol);
				Factory.Save();
				var table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT * FROM dbo.ClientOrgsForNotYetArrived('" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.SqlFormat + "')");
				AssertEquals("Row count", 1, table.Rows.Count);
				var row = table.Rows[0];
				AssertEquals("Row[ConsigneePK]", consignee1.PK, row["ConsigneePK"]);
				AssertEquals("Row[ConsignorPK]", consignor1.PK, row["ConsignorPK"]);
			}

			public void TestClientNotYetArrivedScriptForImporter()
			{
				var table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport '" + consignee1.PK.ToString() + "', NULL, '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 0, table.Rows.Count);
				shipment.JS_PackingMode = Constants.ContainerModes.FCL;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.ConsigneePK = consignee1.PK;
				shipment.ConsignorPK = consignor1.PK;
				shipment.JS_E_ARV = testDate.AddDays(3);
				shipment.JS_E_DEP = testDate.AddDays(-2);
				var consol = shipment.Consols.AddNew();
				SetupTransportLeg(consol.Transports[0], "boat1", "NZAKL", "HKHKG", testDate.AddDays(-2), testDate.AddDays(-2));
				SetupTransportLeg(consol.Transports.AddNew(), "boat2", "HKHKG", "GBWLV", testDate.AddDays(-1), testDate.AddDays(2));
				SetupTransportLeg(consol.Transports.AddNew(), "boat3", "GBWLV", "AUBNE", testDate.AddDays(3), testDate.AddDays(3), false);
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(twentyFootContainer, shipment, consol);
				AddOrderItemToShipment("order1", shipment);
				AddOrderItemToShipment("order2", shipment);
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport '" + consignee1.PK.ToString() + "', NULL, '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.SqlFormat + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				var row = table.Rows[0];
				AssertEquals("boat1/boat3", row["Vessel"]);
				AssertEquals(testDate.AddDays(3).ToShortDateString(), new ZDateTime(row["CurrentETA"]).ToShortDateString());
				AssertEquals(consignor1.OH_FullName, row["Org"]);
				AssertExpectedValuesInRow(row, "OrderNumbers", "ORDER1", "ORDER2");
				AssertExpectedValuesInRow(row, "ContainerDetails", "1x 20FT FR", "2x 40FT FR");
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.AddDays(4).ToShortDateString() + "'");
				AssertEquals("Row count", 0, table.Rows.Count);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport '" + consignee1.PK.ToString() + "', NULL, '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.SqlFormat + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				row = table.Rows[0];
				AssertEquals("boat1/boat3", row["Vessel"]);
				AssertEquals(testDate.AddDays(3).ToShortDateString(), new ZDateTime(row["CurrentETA"]).ToShortDateString());
				AssertEquals(consignor1.OH_FullName, row["Org"]);
				AssertExpectedValuesInRow(row, "OrderNumbers", "ORDER1", "ORDER2");
				AssertExpectedValuesInRow(row, "ContainerDetails", "LCL Freight");
				var notValidShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				notValidShipment.ConsigneePK = consignee1.PK;
				notValidShipment.ConsignorPK = consignor1.PK;
				consol = notValidShipment.Consols.AddNew();
				AddPackedContainer(fortyFootContainer, notValidShipment, consol);
				AddOrderRefToShipment("order3", notValidShipment);
				SetupTransportLeg(consol.Transports[0], "boat4", "NZAKL", "AUBNE", testDate.AddDays(-10), testDate.AddDays(-5));
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport '" + consignee1.PK.ToString() + "', NULL, '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				var anotherShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				anotherShipment.JS_TransportMode = Constants.TransportModes.Sea;
				anotherShipment.ConsigneePK = consignee1.PK;
				anotherShipment.ConsignorPK = consignor2.PK;
				anotherShipment.JS_E_ARV = testDate.AddDays(-1);
				anotherShipment.JS_E_DEP = testDate.AddDays(4);
				consol = anotherShipment.Consols.AddNew();
				AddPackedContainer(twentyFootContainer, anotherShipment, consol);
				AddPackedContainer(twentyFootContainer, anotherShipment, consol);
				AddPackedContainer(fortyFootContainer, anotherShipment, consol);
				AddOrderRefToShipment("order4", anotherShipment);
				SetupTransportLeg(consol.Transports[0], "boat5", "NZCHC", "AUBNE", testDate.AddDays(-1), testDate.AddDays(4));
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport '" + consignee1.PK.ToString() + "', NULL, '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 2, table.Rows.Count);
			}

			public void TestClientNotYetArrivedScriptForSupplier()
			{
				var table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport  NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 0, table.Rows.Count);
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.ConsigneePK = consignee1.PK;
				shipment.ConsignorPK = consignor1.PK;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
				shipment.JS_E_ARV = testDate.AddDays(4);
				shipment.JS_E_DEP = testDate.AddDays(-3);
				var consol = shipment.Consols.AddNew();
				SetupTransportLeg(consol.Transports[0], "boat1", "NZAKL", "HKHKG", testDate.AddDays(-3), testDate.AddDays(-2));
				SetupTransportLeg(consol.Transports.AddNew(), "boat2", "HKHKG", "GBWLV", testDate.AddDays(-1), testDate.AddDays(2));
				SetupTransportLeg(consol.Transports.AddNew(), "boat3", "GBWLV", "AUBNE", testDate.AddDays(3), testDate.AddDays(4), false);
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(twentyFootContainer, shipment, consol);
				AddOrderItemToShipment("order1", shipment);
				AddOrderItemToShipment("order2", shipment);
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport  NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				var row = table.Rows[0];
				AssertEquals("boat1/boat3", row["Vessel"]);
				AssertEquals(testDate.AddDays(4).ToShortDateString(), new ZDateTime(row["CurrentETA"]).ToShortDateString());
				AssertEquals(consignee1.OH_FullName, row["Org"]);
				AssertExpectedValuesInRow(row, "OrderNumbers", "ORDER1", "ORDER2");
				AssertExpectedValuesInRow(row, "ContainerDetails", "Break Bulk");
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport  NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.AddDays(4).ToShortDateString() + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				row = table.Rows[0];
				AssertEquals("Should show both vessel as ATD is not entered ", "boat1/boat3", row["Vessel"]);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport  NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				row = table.Rows[0];
				AssertEquals("boat1/boat3", row["Vessel"]);
				AssertEquals(testDate.AddDays(4).ToShortDateString(), new ZDateTime(row["CurrentETA"]).ToShortDateString());
				AssertEquals(consignee1.OH_FullName, row["Org"]);
				AssertExpectedValuesInRow(row, "OrderNumbers", "ORDER1", "ORDER2");
				AssertExpectedValuesInRow(row, "ContainerDetails", "LCL Freight");
				var notValidShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				notValidShipment.ConsigneePK = consignee1.PK;
				notValidShipment.ConsignorPK = consignor1.PK;
				consol = notValidShipment.Consols.AddNew();
				AddPackedContainer(fortyFootContainer, notValidShipment, consol);
				AddOrderRefToShipment("order3", notValidShipment);
				SetupTransportLeg(consol.Transports[0], "boat4", "NZAKL", "AUBNE", testDate.AddDays(-10), testDate.AddDays(-5));
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC dbo.Client_SWT_NotYetArrivedReport NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				var anotherShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				anotherShipment.JS_TransportMode = Constants.TransportModes.Sea;
				anotherShipment.ConsigneePK = consignee2.PK;
				anotherShipment.ConsignorPK = consignor1.PK;
				anotherShipment.JS_E_ARV = testDate.AddDays(4);
				anotherShipment.JS_E_DEP = testDate.AddDays(-3);
				consol = anotherShipment.Consols.AddNew();
				AddPackedContainer(twentyFootContainer, anotherShipment, consol);
				AddPackedContainer(twentyFootContainer, anotherShipment, consol);
				AddPackedContainer(fortyFootContainer, anotherShipment, consol);
				AddOrderRefToShipment("order4", anotherShipment);
				SetupTransportLeg(consol.Transports[0], "boat5", "NZAKL", "AUSYD", testDate.AddDays(-3), testDate.AddDays(4));
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC dbo.Client_SWT_NotYetArrivedReport NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 2, table.Rows.Count);
			}

			public void TestClientNotYetArrivedScriptForBCNShipment()
			{
				var table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport  NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 0, table.Rows.Count);
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.ConsigneePK = consignee1.PK;
				shipment.ConsignorPK = consignor1.PK;
				shipment.JS_PackingMode = Constants.ContainerModes.FCL;
				shipment.JS_E_ARV = testDate.AddDays(4);
				shipment.JS_E_DEP = testDate.AddDays(-3);
				var consol = shipment.Consols.AddNew();
				consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(twentyFootContainer, shipment, consol);
				AddOrderItemToShipment("order1", shipment);
				AddOrderItemToShipment("order2", shipment);
				var anotherShipment = Factory.New<ForwardingShipment>();
				anotherShipment.JS_TransportMode = Constants.TransportModes.Sea;
				anotherShipment.ConsigneePK = consignee2.PK;
				anotherShipment.ConsignorPK = consignor1.PK;
				anotherShipment.JS_E_ARV = testDate.AddDays(4);
				anotherShipment.JS_E_DEP = testDate.AddDays(-3);
				anotherShipment.Consols.Add(consol);
				AddPackedContainer(twentyFootContainer, anotherShipment, consol);
				AddPackedContainer(twentyFootContainer, anotherShipment, consol);
				AddPackedContainer(fortyFootContainer, anotherShipment, consol);
				AddOrderItemToShipment("order4", anotherShipment);
				SetupTransportLeg(consol.Transports[0], "boat1", "NZAKL", "HKHKG", testDate.AddDays(-3), testDate.AddDays(-2));
				SetupTransportLeg(consol.Transports.AddNew(), "boat2", "HKHKG", "GBWLV", testDate.AddDays(-1), testDate.AddDays(2));
				SetupTransportLeg(consol.Transports.AddNew(), "boat3", "GBWLV", "AUBNE", testDate.AddDays(3), testDate.AddDays(4));
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport  NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.ToShortDateString() + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				var row = table.Rows[0];
				AssertEquals("Will only show details of boat3 as Actual departure date is entered on the last leg", "boat3", row["Vessel"]);
				AssertEquals(testDate.AddDays(4).ToShortDateString(), new ZDateTime(row["CurrentETA"]).ToShortDateString());
				AssertContains(consignee2.OH_FullName, row["Org"].ToString());
				AssertContains(consignee1.OH_FullName, row["Org"].ToString());
				AssertExpectedValuesInRow(row, "OrderNumbers", "ORDER1", "ORDER2");
				AssertExpectedValuesInRow(row, "ContainerDetails", "3x 20FT FR", "3x 40FT FR");
				table = Utilities.GetDataTableFromQuery(Db.Connection, "EXEC Client_SWT_NotYetArrivedReport  NULL, '" + consignor1.PK.ToString() + "', '" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "', '" + testDate.AddDays(4).ToShortDateString() + "'");
				AssertEquals("Row count", 1, table.Rows.Count);
				row = table.Rows[0];
				AssertEquals("boat3", row["Vessel"]);
			}

			public void TestClientGetOrderRefsScript()
			{
				AddOrderRefToShipment("order1", shipment);
				AddOrderRefToShipment("order2", shipment);
				ForwardingShipment anotherShipment = Factory.New<ForwardingShipment>();
				AddOrderRefToShipment("order5", anotherShipment);
				AddOrderRefToShipment("order6", anotherShipment);
				Factory.Save();
				DataTable table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT dbo.ClientGetOrderRefs('" + shipment.PK.ToString() + "', NULL)");
				AssertEquals("Row count", 1, table.Rows.Count);
				AssertEquals("order1,order2".ToUpper(), table.Rows[0][0]);
				ForwardingConsol consol = shipment.Consols.AddNew();
				anotherShipment.Consols.Add(consol);
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT dbo.ClientGetOrderRefs(NULL,'" + consol.PK.ToString() + "')");
				AssertEquals("Row count", 1, table.Rows.Count);
				Assert(table.Rows[0][0].ToString().Contains("ORDER1"));
				Assert(table.Rows[0][0].ToString().Contains("ORDER2"));
				Assert(table.Rows[0][0].ToString().Contains("ORDER5"));
				Assert(table.Rows[0][0].ToString().Contains("ORDER6"));
				AddOrderItemToShipment("order3", shipment);
				AddOrderItemToShipment("order4", shipment);
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT dbo.ClientGetOrderRefs('" + shipment.PK.ToString() + "', NULL)");
				AssertEquals("Row count", 1, table.Rows.Count);
				Assert(table.Rows[0][0].ToString().Contains("ORDER3"));
				Assert(table.Rows[0][0].ToString().Contains("ORDER4"));
				AddOrderItemToShipment("order7", anotherShipment);
				AddOrderItemToShipment("order8", anotherShipment);
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT dbo.ClientGetOrderRefs(NULL, '" + consol.PK.ToString() + "')");
				AssertEquals("Row count", 1, table.Rows.Count);
				Assert(table.Rows[0][0].ToString().Contains("ORDER3"));
				Assert(table.Rows[0][0].ToString().Contains("ORDER4"));
				Assert(table.Rows[0][0].ToString().Contains("ORDER7"));
				Assert(table.Rows[0][0].ToString().Contains("ORDER8"));
			}

			public void TestClientContainerCountForShipmentScript()
			{
				var consol = shipment.Consols.AddNew();
				AddPackedContainer(fortyFootContainer, shipment, consol);
				AddPackedContainer(twentyFootContainer, shipment, consol);
				Factory.Save();
				var table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT dbo.ClientContainerCountForShipment('" + shipment.PK.ToString() + "', NULL)");
				AssertEquals("Row count", 1, table.Rows.Count);
				Assert(table.Rows[0][0].ToString().Contains("1x 20FT FR"));
				Assert(table.Rows[0][0].ToString().Contains("1x 40FT FR"));
				AddPackedContainer(twentyFootContainer, shipment, consol);
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT dbo.ClientContainerCountForShipment('" + shipment.PK.ToString() + "' , NULL)");
				AssertEquals("Row count", 1, table.Rows.Count);
				Assert(table.Rows[0][0].ToString().Contains("2x 20FT FR"));
				Assert(table.Rows[0][0].ToString().Contains("1x 40FT FR"));
				var anotherShipment = Factory.New<ForwardingShipment>();
				anotherShipment.Consols.Add(consol);
				AddPackedContainer(fortyFootContainer, anotherShipment, consol);
				AddPackedContainer(twentyFootContainer, anotherShipment, consol);
				Factory.Save();
				table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT dbo.ClientContainerCountForShipment(NULL , '" + consol.PK.ToString() + "')");
				AssertEquals("Row count", 1, table.Rows.Count);
				Assert(table.Rows[0][0].ToString().Contains("3x 20FT FR"));
				Assert(table.Rows[0][0].ToString().Contains("2x 40FT FR"));
			}

			#region Implementation
			ForwardingShipment shipment;
			RefContainer fortyFootContainer;
			RefContainer twentyFootContainer;
			OrgHeader consignee1;
			OrgHeader consignee2;
			OrgHeader consignor1;
			OrgHeader consignor2;
			ZDateTime testDate;
			protected override void SetUp()
			{
				base.SetUp();
				testDate = ZDateTime.Now;
				TestCaseHelper.RunClientDbCreateScripts();
				shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				fortyFootContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40FR"));
				twentyFootContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
				consignee1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUBNE"));
				consignee2 = Factory.LoadTop1<OrgHeader>(new ZQuery(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.NotEqual, consignee1.OH_Code), JoinCondition.And, new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "AUSYD")));
				consignor1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "NZAKL"));
				consignor2 = Factory.LoadTop1<OrgHeader>(new ZQuery(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.NotEqual, consignee1.OH_Code), JoinCondition.And, new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "NZCHC")));
			}

			void AssertExpectedValuesInRow(DataRow row, ZString columnName, params ZString[] expectedValues)
			{
				foreach (ZString expectedValue in expectedValues)
				{
					Assert("Expected " + expectedValue + " to contain in result " + row[columnName].ToString(), row[columnName].ToString().Contains(expectedValue));
				}
			}

			void SetupTransportLeg(Transport transportLeg, ZString vesselName, ZString loadPortCode, ZString discPortCode, ZDateTime etd, ZDateTime eta)
			{
				SetupTransportLeg(transportLeg, vesselName, loadPortCode, discPortCode, etd, eta, true);
			}

			void SetupTransportLeg(Transport transportLeg, ZString vesselName, ZString loadPortCode, ZString discPortCode, ZDateTime etd, ZDateTime eta, bool defaultATDFromETD)
			{
				transportLeg.JW_IsLinked = false;
				transportLeg.JW_Vessel = vesselName;
				transportLeg.JW_RL_NKLoadPort = loadPortCode;
				transportLeg.JW_RL_NKDiscPort = discPortCode;
				transportLeg.JW_ETA = eta;
				transportLeg.JW_ETD = etd;
				if (defaultATDFromETD)
				{
					transportLeg.JW_ATD = etd;
				}
			}

			void AddPackedContainer(RefContainer refContainer, ForwardingShipment shipmentToAddTo, ForwardingConsol consolToAddTo)
			{
				var line = shipmentToAddTo.OuterPackLines.AddNew();
				var container = Factory.New<ForwardingContainer>();
				container.JC_RC = refContainer.PK;
				consolToAddTo.Containers.Add(container);
				line.Containers.Add(container);
			}

			void AddOrderRefToShipment(ZString orderRef, ForwardingShipment shipmentToAddTo)
			{
				OrgHeader buyer = Factory.LoadTop1<OrgHeader>(new ZQuery());
				Order order = shipmentToAddTo.AttachedOrders.AddNew();
				order.JD_OrderNumber = orderRef;
				order.BuyerPK = buyer.PK;
			}

			void AddOrderItemToShipment(ZString orderItemOrderRef, ForwardingShipment shipmentToAddTo)
			{
				OrderItem item = shipmentToAddTo.DocsAndCartage.OrderItems.AddNew();
				item.JT_OrderReference = orderItemOrderRef;
			}
			#endregion
		}

		#endregion
		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
