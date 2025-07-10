using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsDocketsInvoiceJobHistory))]
	sealed class DocWhsOrdersInvoiceJobHistoryTest : DocWhsDocketsInvoiceJobHistoryTest
	{
		#region Related Business Objects

		protected override void AssertJobChargeLines()
		{
			AssertEquals(4, DocWrapper.JobChargeLines.Count);
			AssertEquals(true, DocWrapper.JobChargeLines.Contains(JobCharge1));
			AssertEquals(true, DocWrapper.JobChargeLines.Contains(JobCharge3));
			AssertEquals(true, DocWrapper.JobChargeLines.Contains(JobCharge4));
			AssertEquals(true, DocWrapper.JobChargeLines.Contains(JobCharge6));
		}

		protected override void AssertJobChargeLinesSort()
		{
			AssertEquals("Charge4 Should be 2nd", JobCharge4, (JobCharge)DocWrapper.JobChargeLines[1].WrappedObject);
			AssertEquals("Charge6 Should be 3rd", JobCharge6, (JobCharge)DocWrapper.JobChargeLines[2].WrappedObject);
			AssertEquals("Charge1 Should be 4th", JobCharge1, (JobCharge)DocWrapper.JobChargeLines[3].WrappedObject);
			AssertEquals("Charge3 Should be 5th", JobCharge3, (JobCharge)DocWrapper.JobChargeLines[4].WrappedObject);
		}

		#endregion

		#region TestUnitsMetAccountsForShortfall

		public void TestUnitsMetAccountsForShortfall()
		{
			//Test to ensure that if an order is placed with a shortfall it is accounted for in the document variables
			ClearSetupData();

			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			ZShort totalPallets = 123;

			// Create Warehouse Charge Codes.
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, today.Date.AddYears(-1), today.AddYears(1).Date);
			Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 60m);

			// Create Receive for Warehouse Storage charge.
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_TotalPallets = totalPallets;
			receive.WD_ArrivalDate = today.AddDays(-2).ToOffset();
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 60m);
			order.WD_TotalPallets = totalPallets;
			order.WD_ArrivalDate = today.AddDays(-1).ToOffset();
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 40m);
			var pick = Helper.CreatePickByAttachingOrders(order);
			pick.FinalisePick();
			order.FinaliseDocket();

			// Create Periodic Billing. 
			var invoice = CreateInvoice(data.Org1, data.Whs1, today.AddDays(-300), today);
			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(invoice);
			invoice.AutoRateJobHeader(null);
			AssertEquals("Precondition - 1 charges should be created", 1, jobHeader.Charges.Count);

			// Hack to mark charge came from order.
			jobHeader.Charges[0].JR_OrderReference = order.WD_DocketID;

			// Post invoice so Transaction Header would be created.
			invoice.PostInvoice();
			Factory.Save();
			AssertEquals("Precondition - Charge was not posted.", true, jobHeader.Charges[0].IsRevenuePosted);
			AssertEquals("Precondition - Job should have one Transaction Header.", 1, jobHeader.PrintingFilter.Transactions.Count);

			var documentWrapper = DocWhsOrdersInvoiceJobHistory.New((ARInvoice)jobHeader.PrintingFilter.Transactions[0], Factory);
			AssertEquals(50m, documentWrapper.JobDocketLines[0].UnitsMet);
		}

		#endregion
	}
}
