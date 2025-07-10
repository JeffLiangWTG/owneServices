using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsDocketsInvoiceJobHistory))]
	public class DocWhsDocketsInvoiceJobHistoryTest : DocumentWrapperTestCase
	{
		#region Related Business Objects

		protected virtual void AssertJobChargeLines()
		{
			AssertEquals(5, DocWrapper.JobChargeLines.Count);
		}

		protected virtual void AssertJobChargeLinesSort()
		{
			AssertEquals("Charge2 Should be 1st", JobCharge2, (JobCharge)DocWrapper.JobChargeLines[0].WrappedObject);
			AssertEquals("Charge4 Should be 2nd", JobCharge4, (JobCharge)DocWrapper.JobChargeLines[1].WrappedObject);
			AssertEquals("Charge6 Should be 3rd", JobCharge6, (JobCharge)DocWrapper.JobChargeLines[2].WrappedObject);
			AssertEquals("Charge1 Should be 4th", JobCharge1, (JobCharge)DocWrapper.JobChargeLines[3].WrappedObject);
			AssertEquals("Charge3 Should be 5th", JobCharge3, (JobCharge)DocWrapper.JobChargeLines[4].WrappedObject);
		}

		#endregion

		#region Properties

		public void TestToString()
		{
			Job.JH_JobNum = "I00100";
			AssertEquals("I00100", DocWrapper.ToString());
		}

		public void TestJobNumber()
		{
			Job.JH_JobNum = "I00100";
			AssertEquals("I00100", DocWrapper.JobNumber);
		}

		public void TestAccountCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTCODE";
			ARInvoice.AH_OH = org.PK;
			AssertEquals("TESTCODE", DocWrapper.AccountCode);
		}

		public void TestFromDate()
		{
			ZDateTime date = ZDateTime.Today.AddDays(1);
			WhsInvoice.ET_StorageFromDate = date;
			AssertEquals(date, DocWrapper.FromDate);
		}

		public void TestToDate()
		{
			ZDateTime date = ZDateTime.Today.AddDays(2);
			WhsInvoice.ET_StorageToDate = date;
			AssertEquals(date, DocWrapper.ToDate);
		}

		public void TestWarehouseName()
		{
			WhsWarehouse whs = Factory.New<WhsWarehouse>();
			whs.WW_WarehouseName = "WHS1NAME";
			WhsInvoice.ET_WW = whs.PK;
			AssertEquals("WHS1NAME", DocWrapper.WarehouseName);
		}

		public void TestClientName()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("XXX");
			var invoice = Factory.New<WhsInvoice>();
			invoice.ET_OH_Client = client.PK;
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_Desc = invoice.ET_StorageJobNumber = "INV1";
			arInvoice.AH_OH = client.PK;

			var job = Helper.CreateRatingJob(invoice);
			job.JH_JobNum = "J1";
			job.JH_GE = Factory.New(typeof(GlbDepartment)).PK;
			arInvoice.AH_JH = job.PK;
			AssertEquals("XXX", DocWhsDocketsInvoiceJobHistory.New(arInvoice, Factory).ClientName);
		}

		#endregion

		#region Collections

		#region TestJobDocketLines

		public void TestJobDocketLines()
		{
			AssertEquals(4, DocWrapper.JobDocketLines.Count);

			//assert charge headers
			AssertEquals("CC2", DocWrapper.ChargeHeader1);
			AssertEquals("CC1", DocWrapper.ChargeHeader2);
			AssertEquals("", DocWrapper.ChargeHeader3);
			AssertEquals("", DocWrapper.ChargeHeader4);
			AssertEquals("", DocWrapper.ChargeHeader5);
			AssertEquals("", DocWrapper.ChargeHeader6);
			AssertEquals("", DocWrapper.ChargeHeader7);
			AssertEquals("", DocWrapper.ChargeHeader8);
			AssertEquals("", DocWrapper.ChargeHeader9);
			AssertEquals("", DocWrapper.ChargeHeader10);

			//assert totals
			AssertEquals(0m, ((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[0]).ChargeHeader1);
			AssertEquals(11m, ((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[0]).ChargeHeader2);
			AssertPropertiesWithZeroValue(((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[0]), 3);

			AssertEquals(13m, ((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[1]).ChargeHeader1);
			AssertEquals(31m, ((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[1]).ChargeHeader2);
			AssertPropertiesWithZeroValue(((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[1]), 3);

			AssertEquals(10m, ((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[2]).ChargeHeader1);
			AssertEquals(0m, ((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[2]).ChargeHeader2);
			AssertPropertiesWithZeroValue(((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[2]), 3);

			AssertEquals(12m, ((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[3]).ChargeHeader1);
			AssertEquals(0m, ((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[3]).ChargeHeader2);
			AssertPropertiesWithZeroValue(((DocWhsDocketLineWithCharges)DocWrapper.JobDocketLines[3]), 3);
		}

		void AssertPropertiesWithZeroValue(DocWhsDocketLineWithCharges docWhsLineWithCharges, ZInt startIndex)
		{
			for (int i = startIndex; i <= 10; i++)
			{
				AssertEquals(0m, docWhsLineWithCharges["ChargeHeader" + i.ToString()]);
			}
		}

		#endregion

		#region TestJobChargeLines_GroupDesc

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc()
		{
			ClearSetupData();

			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			data.Org1.MiscServ.OM_IMInvoiceDetailReportSort = InvoiceDetailReportSortList.Codes.JobType;

			// Create Warehouse Charge Codes.
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 6m);

			// Create Receive for Warehouse Storage charge.
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = new ZDateTimeOffset(2011, 2, 3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - receive was not finalised.", true, receive.IsFinalised);

			// Create Periodic Billing. 
			var invoice = CreateInvoice(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 7));
			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(invoice);
			invoice.AutoRateJobHeader(null);
			AssertEquals("Precondition - 1 charges should be created", 1, jobHeader.Charges.Count);

			// Hack to mark charge came from receive.
			jobHeader.Charges[0].JR_OrderReference = receive.WD_DocketID;

			// Post invoice so Transaction Header would be created.
			invoice.PostInvoice();
			Factory.Save();
			AssertEquals("Precondition - Charge was not posted.", true, jobHeader.Charges[0].IsRevenuePosted);
			AssertEquals("Precondition - Job should have one Transaction Header.", 1, jobHeader.PrintingFilter.Transactions.Count);

			var documentWrapper = DocWhsDocketsInvoiceJobHistory.New((ARInvoice)jobHeader.PrintingFilter.Transactions[0], Factory);
			AssertEquals("One charge line should exist.", 1, documentWrapper.JobChargeLines.Count);
			AssertEquals("Incorrect Job Charge Lines group description.", "STORAGE", documentWrapper.JobChargeLines[0].GroupDesc);

			var newDocumentWrapper = DocWhsDocketsInvoiceJobHistory.New((ARInvoice)jobHeader.PrintingFilter.Transactions[0], Factory);
			AssertNotSame("JobChargeLines should be different as they are cached on the Factory level per DocWrapper PK", documentWrapper.JobChargeLines, newDocumentWrapper.JobChargeLines);
			AssertEquals("One charge line should exist.", 1, newDocumentWrapper.JobChargeLines.Count);
			AssertEquals("Incorrect Job Charge Lines group description.", "STORAGE", newDocumentWrapper.JobChargeLines[0].GroupDesc);
			AssertNotSame("JobCharge DocWrappers should be different", documentWrapper.JobChargeLines[0], newDocumentWrapper.JobChargeLines[0]);
			AssertSame("Wrapped JobCharges should be same", documentWrapper.JobChargeLines[0].JobCharge, newDocumentWrapper.JobChargeLines[0].JobCharge);
		}

		protected void ClearSetupData()
		{
			// Delete incorrectly set data from SetUp() so we could use Factory.Save()
			Receive.Delete();
			Order1.Delete();
			Order2.Delete();
			Order3.Delete();
			ARLine.Delete();
			ARInvoice.Delete();
			Job.Delete();
		}

		protected WhsInvoice CreateInvoice(OrgHeader client, WhsWarehouse warehouse, ZDateTime storageFromDate, ZDateTime storageToDate)
		{
			WhsInvoice result = Factory.New<WhsInvoice>();
			result.ET_OH_Client = client.PK;
			result.ET_WW = warehouse.PK;
			result.ET_StorageFromDate = storageFromDate;
			result.ET_StorageToDate = storageToDate;

			result.Validation.ValidateAll(); // needed because of some refresh issues with ET_StorageFromDate and ET_StorageToDate 

			return result;
		}

		#endregion

		#region TestInvoiceJobHistoryTotalPallet

		public void TestInvoiceJobHistoryTotalPallet()
		{
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
			receive.WD_ArrivalDate = today.AddDays(-1).ToOffset();
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - receive was not finalised.", true, receive.IsFinalised);

			// Create Periodic Billing. 
			var invoice = CreateInvoice(data.Org1, data.Whs1, today.AddDays(-300), today);
			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(invoice);
			invoice.AutoRateJobHeader(null);
			AssertEquals("Precondition - 1 charges should be created", 1, jobHeader.Charges.Count);

			// Hack to mark charge came from receive.
			jobHeader.Charges[0].JR_OrderReference = receive.WD_DocketID;

			// Post invoice so Transaction Header would be created.
			invoice.PostInvoice();
			Factory.Save();
			AssertEquals("Precondition - Charge was not posted.", true, jobHeader.Charges[0].IsRevenuePosted);
			AssertEquals("Precondition - Job should have one Transaction Header.", 1, jobHeader.PrintingFilter.Transactions.Count);

			var documentWrapper = DocWhsDocketsInvoiceJobHistory.New((ARInvoice)jobHeader.PrintingFilter.Transactions[0], Factory);
			AssertEquals(totalPallets, documentWrapper.JobDocketLines[0].TotalPallets);
		}

		#endregion

		#endregion

		#region Hardcoded Headers

		public void TestChargeHeader1()
		{
			AssertEquals("", DocWrapper.ChargeHeader1);
			DocWrapper.ChargeHeader1 = "X";
			AssertEquals("X", DocWrapper.ChargeHeader1);
		}

		public void TestChargeHeader2()
		{
			AssertEquals("", DocWrapper.ChargeHeader2);
			DocWrapper.ChargeHeader2 = "2";
			AssertEquals("2", DocWrapper.ChargeHeader2);
		}

		public void TestChargeHeader3()
		{
			AssertEquals("", DocWrapper.ChargeHeader3);
			DocWrapper.ChargeHeader3 = "3";
			AssertEquals("3", DocWrapper.ChargeHeader3);
		}

		public void TestChargeHeader4()
		{
			AssertEquals("", DocWrapper.ChargeHeader4);
			DocWrapper.ChargeHeader4 = "4";
			AssertEquals("4", DocWrapper.ChargeHeader4);
		}

		public void TestChargeHeader5()
		{
			AssertEquals("", DocWrapper.ChargeHeader5);
			DocWrapper.ChargeHeader5 = "5";
			AssertEquals("5", DocWrapper.ChargeHeader5);
		}

		public void TestChargeHeader6()
		{
			AssertEquals("", DocWrapper.ChargeHeader6);
			DocWrapper.ChargeHeader6 = "6";
			AssertEquals("6", DocWrapper.ChargeHeader6);
		}

		public void TestChargeHeader7()
		{
			AssertEquals("", DocWrapper.ChargeHeader7);
			DocWrapper.ChargeHeader7 = "7";
			AssertEquals("7", DocWrapper.ChargeHeader7);
		}

		public void TestChargeHeader8()
		{
			AssertEquals("", DocWrapper.ChargeHeader8);
			DocWrapper.ChargeHeader8 = "8";
			AssertEquals("8", DocWrapper.ChargeHeader8);
		}

		public void TestChargeHeader9()
		{
			AssertEquals("", DocWrapper.ChargeHeader9);
			DocWrapper.ChargeHeader9 = "9";
			AssertEquals("9", DocWrapper.ChargeHeader9);
		}

		public void TestChargeHeader10()
		{
			AssertEquals("", DocWrapper.ChargeHeader10);
			DocWrapper.ChargeHeader10 = "10";
			AssertEquals("10", DocWrapper.ChargeHeader10);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			WhsInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			ARInvoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice.AH_Desc = WhsInvoice.ET_StorageJobNumber = "INV1";
			ARInvoice.AH_OH = org.PK;

			DocWrapper = DocWhsDocketsInvoiceJobHistory.New(ARInvoice, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);

			ChargeCodes = new List<ZString>(new ZString[] { "CC1", "CC2" });
			SetupJobCharges();

			base.SetUp();
		}

		void SetupJobCharges()
		{
			Job = Helper.CreateRatingJob(WhsInvoice);
			Job.JH_JobNum = "J1";
			Job.JH_GE = Factory.New(typeof(GlbDepartment)).PK;

			ARInvoice.AH_JH = Job.PK;

			ChargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			ChargeCode1.AC_Code = ChargeCodes[0];
			ChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			ChargeCode2.AC_Code = ChargeCodes[1];

			Receive = (WhsReceive)CreateDocket(Enterprise.Warehouse.Transactions.CodeLists.DocketType.Codes.Receive, "I1", "INW1", ZDateTimeOffset.Empty);
			Order1 = (WhsOrder)CreateDocket(Enterprise.Warehouse.Transactions.CodeLists.DocketType.Codes.Order, "O1", "ORD1", ZDateTimeOffset.Today.AddDays(-1));
			Order2 = (WhsOrder)CreateDocket(Enterprise.Warehouse.Transactions.CodeLists.DocketType.Codes.Order, "O2", "ORD2", ZDateTimeOffset.Today);
			Order3 = (WhsOrder)CreateDocket(Enterprise.Warehouse.Transactions.CodeLists.DocketType.Codes.Order, "O3", "ORD3", ZDateTimeOffset.Today.AddDays(-1));

			ARLine = Factory.New<AccTransactionLines>();
			ARLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ARLine.AL_AH = ARInvoice.PK;

			JobCharge1 = CreateJobCharge(ChargeCode2, ARLine, 10m, Order2.WD_DocketID);
			JobCharge2 = CreateJobCharge(ChargeCode1, ARLine, 11m, Receive.WD_DocketID);
			JobCharge3 = CreateJobCharge(ChargeCode2, ARLine, 12m, Order3.WD_DocketID);
			JobCharge4 = CreateJobCharge(ChargeCode2, ARLine, 13m, Order1.WD_DocketID);
			JobCharge5 = CreateJobCharge(ChargeCode2, ARLine, 14m, ZString.Empty);
			JobCharge6 = CreateJobCharge(ChargeCode1, ARLine, 15m, Order1.WD_DocketID);
			JobCharge7 = CreateJobCharge(ChargeCode1, ARLine, 16m, Order1.WD_DocketID);
		}

		WhsDocket CreateDocket(ZString docketType, ZString docketID, ZString externalReference, ZDateTimeOffset dateTime)
		{
			WhsDocket docket = null;

			switch (docketType)
			{
				case Enterprise.Warehouse.Transactions.CodeLists.DocketType.Codes.Receive:
					docket = Factory.New<WhsReceive>();
					docket.Lines.Add(Factory.New<WhsReceiveLine>());
					break;
				case Enterprise.Warehouse.Transactions.CodeLists.DocketType.Codes.Order:
					docket = Factory.New<WhsOrder>();
					docket.Lines.Add(Factory.New<WhsOrderLine>());
					break;
			}
			AssertNotNull(docket);

			docket.WD_DocketID = docketID;
			docket.WD_ExternalReference = externalReference;
			if (dateTime != ZDateTimeOffset.Empty) { docket.WD_FinalisedDate = dateTime; }

			return docket;
		}

		JobCharge CreateJobCharge(AccChargeCode chargeCode, AccTransactionLines aRLine, ZDecimal localSellAmt, ZString reference)
		{
			JobCharge jobCharge = Job.Charges.AddNew();
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_AL_ARLine = aRLine.PK;
			jobCharge.JR_LocalSellAmt = localSellAmt;
			jobCharge.JR_OrderReference = reference;

			return jobCharge;
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		ARInvoice ARInvoice;
		WhsInvoice WhsInvoice;
		protected DocWhsDocketsInvoiceJobHistory DocWrapper;
		Job Job;
		AccChargeCode ChargeCode1;
		AccChargeCode ChargeCode2;
		WhsReceive Receive;
		WhsOrder Order1;
		WhsOrder Order2;
		WhsOrder Order3;
		AccTransactionLines ARLine;
		protected JobCharge JobCharge1;
		protected JobCharge JobCharge2;
		protected JobCharge JobCharge3;
		protected JobCharge JobCharge4;
		protected JobCharge JobCharge5;
		protected JobCharge JobCharge6;
		protected JobCharge JobCharge7;
		List<ZString> ChargeCodes;

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

		#region Notify

		protected TestNotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer()); }
		}
		TestNotificationBuffer notify;

		#endregion

		#endregion
	}
}
