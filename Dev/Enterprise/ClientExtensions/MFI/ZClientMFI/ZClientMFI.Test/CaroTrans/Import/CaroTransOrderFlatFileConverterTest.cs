using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransOrderFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestMapImportedDataToValueObjects()
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			CaroTransOrderFlatFileFormat formater = new CaroTransOrderFlatFileFormat();
			FlatFileDataRow dataRow = formater.ConvertToRow("01,JERAUC0252002,A24200390,JER,AUC,20021209,BURNARD INTERNATIONAL,4,1438,2.9,20021211,20021216,NEW YORK,20021224,LOS ANGELES,COLUMBUS WAIKATO,694SB,TCKU9369958,20030112,20030127");
			dataRows.Add(dataRow);
			FlatFileDataRow auditRow = formater.ConvertToRow("99,1");
			dataRows.Add(auditRow);
			Xsd.Orders xsdOrders = new Xsd.Orders();
			MockCaroTransOrderFlatFileConverter converter = new MockCaroTransOrderFlatFileConverter(new NotificationBuffer(), Factory);
			converter.MapImport(xsdOrders, dataRows);
			AssertEquals("XsdOrders.Order.Count", 1, xsdOrders.Order.Count);
			Xsd.Order xsdOrder = xsdOrders.Order[0];
			AssertEquals("Order Number", "A24200390", xsdOrder.OrderIdentifier.OrderNumber);
			AssertEquals("House Bill", "JERAUC0252002", xsdOrder.OrderDetail.ShipmentPlanning.HouseBill);
			AssertEquals("Buyer Owner Code", "BURNARD INTERNATIONAL", xsdOrder.OrderDetail.Buyer.OwnerCode);
			AssertEquals("Goods Origin", "JER", xsdOrder.OrderDetail.ShipmentPlanning.GoodsOrigin.Value);
			AssertEquals("Goods Destination", GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AUC", xsdOrder.OrderDetail.ShipmentPlanning.GoodsDestination.Value);
			AssertEquals("Order DateTime", new DateTime(2002, 12, 09), xsdOrder.OrderDetail.OrderDateTime);
			AssertEquals("Packs", 4M, xsdOrder.OrderDetail.ShipmentPlanning.Packs.Value);
			AssertEquals("Weight", 1438M, xsdOrder.OrderDetail.ShipmentPlanning.Weight.Value);
			AssertEquals("Volume", 2.9M, xsdOrder.OrderDetail.ShipmentPlanning.Volume.Value);
			AssertEquals("Departure Vessel", "COLUMBUS WAIKATO", xsdOrder.OrderDetail.ShipmentPlanning.DepartureVessel);
			AssertEquals("Departure Voyage Flight", "694SB", xsdOrder.OrderDetail.ShipmentPlanning.DepartureVoyageFlight);
			AssertEquals("PlannedContainers.Count", 1, xsdOrder.OrderDetail.ShipmentPlanning.PlannedContainers.Count);
			AssertEquals("Container No", "TCKU9369958", xsdOrder.OrderDetail.ShipmentPlanning.PlannedContainers[0].Number);
			AssertEquals("Origin Receival", new DateTime(2002, 12, 11), xsdOrder.OrderDetail.Milestones.OriginReceival.Estimated);
			AssertEquals("UserDate.Count", 2, xsdOrder.OrderDetail.Milestones.UserDate.Count);
			AssertEquals("User Date 1", new DateTime(2002, 12, 16), xsdOrder.OrderDetail.Milestones.UserDate[0].Estimated);
			AssertEquals("User Date 2", new DateTime(2002, 12, 24), xsdOrder.OrderDetail.Milestones.UserDate[1].Estimated);
			AssertEquals("Departure Date", new DateTime(2003, 01, 12), xsdOrder.OrderDetail.Milestones.Departure.Estimated);
			AssertEquals("Arrival Date", new DateTime(2003, 01, 27), xsdOrder.OrderDetail.Milestones.Arrival.Estimated);
			//AssertEquals("", "", XsdOrder.OrderDetail.);
			//AssertEquals("", "", XsdOrder.OrderDetail.);
		}

		public void TestInvalidFileFormat()
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			CaroTransOrderFlatFileFormat formater = new CaroTransOrderFlatFileFormat();
			FlatFileDataRow orderRow1 = formater.ConvertToRow("01,JERAUC0252002,A24200390,JER,AUC,20021209,BURNARD INTERNATIONAL,4,1438,2.9,20021211,20021216,NEW YORK,20021224,LOS ANGELES,COLUMBUS WAIKATO,694SB,TCKU9369958,20030112,20030127");
			dataRows.Add(orderRow1);
			FlatFileDataRow orderRow2 = formater.ConvertToRow("01,CSCMEL0251004,32-54317-9,CSC,MEL,20021118,ATOTECH AUSTRALIA PTY LTD,1,45,0.5,20021121,20021126,CHARLESTON,20021209,LOS ANGELES,DIRECT CONDOR,689SB,PONU7288682,20021215,20030103");
			dataRows.Add(orderRow2);
			FlatFileDataRow auditRow = formater.ConvertToRow("99,2");
			dataRows.Add(auditRow);
			Xsd.Orders xsdOrders = new Xsd.Orders();
			MockCaroTransOrderFlatFileConverter converter = new MockCaroTransOrderFlatFileConverter(new NotificationBuffer(), Factory);
			converter.MapImport(xsdOrders, dataRows);
			AssertEquals("Data is valid", 2, xsdOrders.Order.Count);
			xsdOrders.Order.Clear();
			orderRow1.SetField(Constants.OrderRecord.RecordID, "02");
			converter.MapImport(xsdOrders, dataRows);
			AssertEquals("Data is not valid", 0, xsdOrders.Order.Count);
			orderRow1.SetField(Constants.OrderRecord.RecordID, "01");
			auditRow.SetField(Constants.OrderRecord.RecordID, "77");
			converter.MapImport(xsdOrders, dataRows);
			AssertEquals("Data is not valid", 0, xsdOrders.Order.Count);
			auditRow.SetField(Constants.OrderRecord.RecordID, "99");
			auditRow.SetField(1, "3");
			converter.MapImport(xsdOrders, dataRows);
			AssertEquals("Data is not valid", 0, xsdOrders.Order.Count);
			auditRow.SetField(1, "2");
			FlatFileDataRow extraRow = formater.ConvertToRow("blah");
			dataRows.Add(extraRow);
			converter.MapImport(xsdOrders, dataRows);
			AssertEquals("Data is not valid", 0, xsdOrders.Order.Count);
		}

		#region Implementation
		public class MockCaroTransOrderFlatFileConverter : CaroTransOrderFlatFileConverter
		{
			public MockCaroTransOrderFlatFileConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory)
			{
			}

			public new void MapImport(IValueObject valueObject, FlatFileDataRowCollection rows)
			{
				base.MapImport(valueObject, rows);
			}
		}
		#endregion
	}
}
