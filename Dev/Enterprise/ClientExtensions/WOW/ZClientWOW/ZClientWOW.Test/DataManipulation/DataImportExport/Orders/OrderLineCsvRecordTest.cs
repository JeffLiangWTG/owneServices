using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	public class OrderLineCsvRecordTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportOrderLinesCsv()
		{
			ZQuery filter = new ZQuery();
			OrderCollection orders = new OrderCollection(Factory, filter);
			orders.ApplySort(Order.Schema.JD_OrderNumber, ListSortDirection.Ascending);
			orders.DeleteAll();
			OrderCsvRecordTest.SetupOrderTestData(Factory);
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Orders\\Testing\\TestOrders.csv"))
			{
				ITransactionParticipant[] transactionActions;
				fImporter.ImportDataToFactory(fileContents, "", new NotificationBuffer(null), SourceInfo.EmptySourceInfo, out transactionActions);
				// number of order lines in first order
				AssertEquals("Order line count in first order", 2, orders[0].OrderLines.Count);
				// make sure the data was migrated correctly
				OrderLineCollection orderLines = orders[0].OrderLines;
				orderLines.ApplySort(WoolworthsOrderLine.Schema.JO_LineNo, ListSortDirection.Ascending);
				AssertEquals("OrderLines[0].JO_LineNo", 1, orderLines[0].JO_LineNo);
				AssertEquals("OrderLines[0].JO_LinePrice", new ZDecimal(9216 * 1.84), orderLines[0].JO_LinePrice);
				new WowTestUtil().AssertBusinessPropsEquals(orderLines[0], OrderLineCsvRecord.OrderLinePropertyMappings, null, null, new ZString("10744"), new ZString("U"), new ZDecimal(9216), new ZDecimal(1.84), new ZDateTime(new DateTime(2003, 11, 30)), new ZDateTime(new DateTime(2003, 12, 9)), new ZDateTime(new DateTime(2003, 12, 15)), ZDateTime.Empty, new ZDecimal(6), new ZDecimal(288), ZDateTime.Empty, new ZString("214"), new ZString("CHEF CRAFT MEASURE CUPS EVERYDAY"), new ZString("S"), new ZString(""), new ZBool(false), new ZString("CN"), new ZDecimal(16957.44));
				AssertEquals("OrderLines[1].JD_LineNo", 2, orderLines[1].JO_LineNo);
				AssertEquals("OrderLines[1].JO_LinePrice", new ZDecimal(9216 * 1.84), orderLines[1].JO_LinePrice);
				new WowTestUtil().AssertBusinessPropsEquals(orderLines[1], OrderLineCsvRecord.OrderLinePropertyMappings, null, null, new ZString("1370"), new ZString("U"), new ZDecimal(9216), new ZDecimal(1.84), new ZDateTime(new DateTime(2003, 11, 30)), new ZDateTime(new DateTime(2003, 12, 9)), new ZDateTime(new DateTime(2003, 12, 15)), new ZDateTime(new DateTime(2003, 12, 14)), new ZDecimal(6), new ZDecimal(288), ZDateTime.Empty, new ZString("214"), new ZString("CHEF CRAFT MEASURE CUPS EVERYDAY"), new ZString("S"), new ZString(""), new ZBool(false), new ZString("CN"), new ZDecimal(16957.44));
				// make sure doing this a second time produces no changes
				new WowTestUtil().CheckNoChangesAfterRunningImportSecondTime(Factory, fImporter, fileContents, new NotificationBuffer(null));
			}
		}

		// Make sure the line number unique constraint is not violated. If this constraint goes away, this test will always
		// pass (even if the code is not valid). This is acceptable as we just want to be able to post to the db without error.
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestLineNoUnique()
		{
			// create a new order
			Order newOrder = Factory.New<Order>();
			newOrder.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			newOrder.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			newOrder.JD_OrderNumber = "P08735";
			OrderLine newLine1 = newOrder.OrderLines.AddNew();
			newLine1.JO_LineNo = 2;
			OrderLine newLine2 = newOrder.OrderLines.AddNew();
			newLine2.JO_LineNo = 1;
			OrderCsvRecordTest.SetupOrderTestData(Factory);
			using (StreamReader fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Orders\\Testing\\TestOrders.csv"))
			{
				string fileContentsAsString = fileContents.ReadToEnd();
				fileContents.BaseStream.Position = 0;
				string[] lines = fileContentsAsString.Split('\n');
				ITransactionParticipant[] transactionActions;
				fImporter.ImportDataToFactory(fileContents, "", new NotificationBuffer(null), SourceInfo.EmptySourceInfo, out transactionActions);
				Hashtable lineNoSoFar = new Hashtable();
				foreach (DataRow row in ((INeedDataSet)Factory).Data.Tables[OrderLine.Schema.TableName].Rows)
				{
					if (row.RowState != DataRowState.Deleted)
					{
						string key = row[OrderLine.Schema.JO_LineNo] + "__" + row[OrderLine.Schema.JO_JD];
						AssertEquals("No duplicate line numbers", false, lineNoSoFar.Contains(key));
						lineNoSoFar[key] = null;
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeleteOrderLines()
		{
			var order = Factory.New<WoolworthsOrder>();
			order.JD_OrderNumber = "P08735";
			var line = (WoolworthsOrderLine)order.OrderLines.AddNew();
			line.JO_Partno = "10744";
			using (var fileContents = new WowTestUtil().GetStreamReaderForTestFile("DataImportExport\\Orders\\Testing\\DeleteOrderLine.csv"))
			{
				var fileContentsAsString = fileContents.ReadToEnd();
				fileContents.BaseStream.Position = 0;
				var lines = fileContentsAsString.Split('\n');
				var buffer = new NotificationBuffer(null);
				fImporter.ImportDataToFactory(fileContents, "", buffer, SourceInfo.EmptySourceInfo, out var transactionActions);
				AssertEquals("Order line should be deleted", true, line.IsDeleted);
			}
		}

		WowDataImporter fImporter;
		protected override void SetUp()
		{
			base.SetUp();
			fImporter = new TestWowDataImporter(Factory);
		}

		class TestWowDataImporter : WowDataImporter
		{
			public TestWowDataImporter(BusinessObjectFactory factory) : base(new SingleBusinessObjectFactoryProvider(factory))
			{
			}
		}
	}
}
