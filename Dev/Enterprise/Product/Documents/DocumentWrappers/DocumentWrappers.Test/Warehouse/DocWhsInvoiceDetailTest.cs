using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsInvoiceDetail))]
	sealed class DocWhsInvoiceDetailTest : DocumentWrapperTestCase
	{
		#region Related Business Objects

		#region TestBranch

		public void TestBranch()
		{
			ARInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, DocWrapper.Branch.Code);
		}

		#endregion

		#region TestJobChargeLines

		public void TestJobChargeLines()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			WhsInvoice.ET_OH_Client = org.PK;

			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WhsInvoice.PK;
			job.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "CC1";
			chargeCode2.AC_Code = "CC2";
			chargeCode1.AC_Desc = "CC1DESC";
			chargeCode2.AC_Desc = "CC2DESC";
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;

			ARInvoice.AH_JH = job.PK;
			AccTransactionLines aRLine = Factory.New<AccTransactionLines>();
			aRLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			aRLine.AL_AH = ARInvoice.PK;

			JobCharge jobCharge1 = job.Charges.AddNew();
			JobCharge jobCharge2 = job.Charges.AddNew();
			jobCharge1.JR_AC = chargeCode1.PK;
			jobCharge2.JR_AC = chargeCode2.PK;
			jobCharge1.JR_AL_ARLine = aRLine.PK;
			jobCharge2.JR_AL_ARLine = aRLine.PK;

			org.MiscServ.OM_IMInvoiceDetailReportSort = InvoiceDetailReportSortList.Codes.ChargeCode;
			AssertEquals(2, DocWrapper.JobChargeLines.Count);
			AssertEquals(jobCharge1, (JobCharge)DocWrapper.JobChargeLines[0].WrappedObject);
			AssertEquals(jobCharge2, (JobCharge)DocWrapper.JobChargeLines[1].WrappedObject);

			AssertEquals("CC1 (CC1DESC)", DocWrapper.JobChargeLines[0].GroupDesc);
			AssertEquals("CC2 (CC2DESC)", DocWrapper.JobChargeLines[1].GroupDesc);

			DocWrapper = DocWhsInvoiceDetail.New(ARInvoice, Factory);
			org.MiscServ.OM_IMInvoiceDetailReportSort = InvoiceDetailReportSortList.Codes.JobType;
			AssertEquals("INWARDS", DocWrapper.JobChargeLines[0].GroupDesc);
			AssertEquals("ORDERS", DocWrapper.JobChargeLines[1].GroupDesc);

			var newDocWrapper = DocWhsInvoiceDetail.New(ARInvoice, Factory);
			AssertNotSame("The JobChargeLines value is cached on Factory level per DocWrapper PK", newDocWrapper.JobChargeLines, DocWrapper.JobChargeLines);
		}

		#endregion

		#region TestJobChargeLines_Sort

		public void TestJobChargeLines_Sort()
		{
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = WhsInvoice.PK;
			job.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;

			var chargeCode1 = Factory.New<AccChargeCode>();
			var chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "CC1";
			chargeCode2.AC_Code = "CC2";

			var receive = Factory.New<WhsReceive>();
			var order1 = Factory.New<WhsOrder>();
			var order2 = Factory.New<WhsOrder>();
			var order3 = Factory.New<WhsOrder>();
			receive.WD_DocketID = "I1";
			order1.WD_DocketID = "O1";
			order2.WD_DocketID = "O2";
			order3.WD_DocketID = "O3";
			receive.WD_ExternalReference = "INW1";
			order1.WD_ExternalReference = "ORD1";
			order2.WD_ExternalReference = "ORD2";
			order3.WD_ExternalReference = "ORD3";
			order1.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(-1);
			order2.WD_FinalisedDate = ZDateTimeOffset.Today;
			order3.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(-1);

			ARInvoice.AH_JH = job.PK;
			var aRLine = Factory.New<AccTransactionLines>();
			aRLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			aRLine.AL_AH = ARInvoice.PK;

			var jobCharge1 = job.Charges.AddNew();
			var jobCharge2 = job.Charges.AddNew();
			var jobCharge3 = job.Charges.AddNew();
			var jobCharge4 = job.Charges.AddNew();
			jobCharge1.JR_AC = chargeCode2.PK;
			jobCharge2.JR_AC = chargeCode1.PK;
			jobCharge3.JR_AC = chargeCode2.PK;
			jobCharge4.JR_AC = chargeCode2.PK;
			jobCharge1.JR_AL_ARLine = aRLine.PK;
			jobCharge2.JR_AL_ARLine = aRLine.PK;
			jobCharge3.JR_AL_ARLine = aRLine.PK;
			jobCharge4.JR_AL_ARLine = aRLine.PK;
			jobCharge1.JR_OrderReference = order2.WD_DocketID;
			jobCharge2.JR_OrderReference = receive.WD_DocketID;
			jobCharge3.JR_OrderReference = order3.WD_DocketID;
			jobCharge4.JR_OrderReference = order1.WD_DocketID;

			AssertEquals(4, DocWrapper.JobChargeLines.Count);
			AssertEquals("Charge2 Should be 1st", jobCharge2, (JobCharge)DocWrapper.JobChargeLines[0].WrappedObject);
			AssertEquals("Charge4 Should be 2nd", jobCharge4, (JobCharge)DocWrapper.JobChargeLines[1].WrappedObject);
			AssertEquals("Charge3 Should be 3rd", jobCharge3, (JobCharge)DocWrapper.JobChargeLines[2].WrappedObject);
			AssertEquals("Charge1 Should be 4th", jobCharge1, (JobCharge)DocWrapper.JobChargeLines[3].WrappedObject);
		}

		#endregion

		#region TestJobChargeLines_GroupDesc

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			data.Org1.MiscServ.OM_IMInvoiceDetailReportSort = InvoiceDetailReportSortList.Codes.ChargeCode;
			data.Org1.MiscServ.OM_IMInvoiceDetailReportSort2 = InvoiceDetailReportSortList.Codes.JobType;
			data.Org1.MiscServ.OM_IMInvoiceDetailReportSort3 = JobChargeAttribTypeList.Codes.DocketReference;

			// Create Warehouse Charge Codes.
			var receiveHandlingCharge = Helper.CreateChargeCode("WRECHAN", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			var outwardHandlingCharge = Helper.CreateChargeCode("WOUTHAN", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			Helper.CreateRateLine(warehouseRate, receiveHandlingCharge, "UNT", 5m);
			Helper.CreateRateLine(warehouseRate, warehouseStorageCharge, "UNT", 6m);
			Helper.CreateRateLine(warehouseRate, outwardHandlingCharge, "UNT", 7m);

			// Create Receive for Receive Handling and Warehouse Storage charges.
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = new ZDateTimeOffset(2011, 2, 3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - receive was not finalised.", true, receive.IsFinalised);
			Factory.Save();

			// Create Order for Outward Handling charge.
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR1", data.Part1, 10m, WhsPickOption.Codes.Manual);
			order.WD_RequiredDate = new ZDateTimeOffset(2011, 2, 5);
			WhsPick pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Precondition - order was not finalised.", true, order.IsFinalised);
			AssertEquals("Precondition - pick was not finalised.", true, pick.IsFinalised);

			// Create Periodic Billing. 
			var invoice = CreateInvoice(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 7));
			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(invoice);
			invoice.AutoRateJobHeader(null);
			jobHeader.SetDefaultsForJob();
			AssertEquals("Preconditon - 3 charges should be created", 3, jobHeader.Charges.Count);

			// Simulate old data by adding DocketReference attribute to Storage Charge. 
			var storageCharge = FindJobCharge(jobHeader.Charges, warehouseStorageCharge);
			var storageJobChargeAttrib = storageCharge.JobChargeAttributes.AddNew();
			storageJobChargeAttrib.EC_Name = JobChargeAttribTypeList.Codes.DocketReference;
			storageJobChargeAttrib.EC_Value = receive.WD_ExternalReference;

			// Post invoice so Transaction Header would be created.
			invoice.PostInvoice();
			Factory.Save();
			AssertEquals("Precondition - Charge was not posted.", true, jobHeader.Charges[0].IsRevenuePosted);
			AssertEquals("Precondition - Charge was not posted.", true, jobHeader.Charges[1].IsRevenuePosted);
			AssertEquals("Precondition - Charge was not posted.", true, jobHeader.Charges[2].IsRevenuePosted);
			AssertEquals("Precondition - Job should have one Transaction Header.", 1, jobHeader.PrintingFilter.Transactions.Count);

			var expectedGroupDesc1_ReceiveHandling = GetChargeCodeDesc(receiveHandlingCharge);
			var expectedGroupDesc1_WarehouseStorage = GetChargeCodeDesc(warehouseStorageCharge);
			var expectedGroupDesc1_OutwardsHandling = GetChargeCodeDesc(outwardHandlingCharge);
			var expectedGroupDesc3 = JobChargeAttribTypeList.Descriptions.DocketReference.GetUnresolvedString().ToUpper() + ": ";

			var documentWrapper = DocWhsInvoiceDetail.New(jobHeader.PrintingFilter.Transactions[0], Factory);
			AssertEquals("One charge line should exist.", 3, documentWrapper.JobChargeLines.Count);
			AssertDocJobChargeDescriptionsAreCorrect(documentWrapper.JobChargeLines, receiveHandlingCharge, expectedGroupDesc1_ReceiveHandling, "INWARDS", expectedGroupDesc3 + receive.WD_ExternalReference);
			AssertDocJobChargeDescriptionsAreCorrect(documentWrapper.JobChargeLines, warehouseStorageCharge, expectedGroupDesc1_WarehouseStorage, "STORAGE", expectedGroupDesc3 + "[None]");
			AssertDocJobChargeDescriptionsAreCorrect(documentWrapper.JobChargeLines, outwardHandlingCharge, expectedGroupDesc1_OutwardsHandling, "ORDERS", expectedGroupDesc3 + order.WD_ExternalReference);
		}

		string GetChargeCodeDesc(AccChargeCode chargeCode)
		{
			return string.Format("{0} ({1})", chargeCode.AC_Code, chargeCode.AC_Desc);
		}

		WhsInvoice CreateInvoice(OrgHeader client, WhsWarehouse warehouse, ZDateTime storageFromDate, ZDateTime storageToDate)
		{
			WhsInvoice result = Factory.New<WhsInvoice>();
			result.ET_OH_Client = client.PK;
			result.ET_WW = warehouse.PK;
			result.ET_StorageFromDate = storageFromDate;
			result.ET_StorageToDate = storageToDate;

			result.Validation.ValidateAll(); // needed because of some refresh issues with ET_StorageFromDate and ET_StorageToDate 

			return result;
		}

		void AssertDocJobChargeDescriptionsAreCorrect(DocWhsJobChargeCollection allDocJobCharges, AccChargeCode chargeCodeToFindDocJobChargeWith, ZString expectedGroupDesc, ZString expectedGroupDesc2, ZString expectedGroupDesc3)
		{
			DocWhsJobCharge docJobCharge = FindDocWhsCharge(allDocJobCharges, chargeCodeToFindDocJobChargeWith);
			AssertNotNull("No DocWhsJobCharge exist with charge code: " + chargeCodeToFindDocJobChargeWith.AC_Code, docJobCharge);
			AssertEquals("Incorrect Job Charge Lines group description.", expectedGroupDesc, docJobCharge.GroupDesc);
			AssertEquals("Incorrect Job Charge Lines group description.", expectedGroupDesc2, docJobCharge.GroupDesc2);
			AssertEquals("Incorrect Job Charge Lines group description.", expectedGroupDesc3, docJobCharge.GroupDesc3);
		}

		DocWhsJobCharge FindDocWhsCharge(DocWhsJobChargeCollection allDocJobCharges, AccChargeCode chargeCode)
		{
			foreach (DocWhsJobCharge docJobCharge in allDocJobCharges)
			{
				if (docJobCharge.ChargeCode.AccChargeCode == chargeCode)
				{
					return docJobCharge;
				}
			}
			return null;
		}

		JobCharge FindJobCharge(ChargeCollection allJobCharges, AccChargeCode chargeCode)
		{
			foreach (Charge charge in allJobCharges)
			{
				if (charge.ChargeCode == chargeCode)
				{
					return charge;
				}
			}
			return null;
		}

		#endregion

		#region TestContinerChargeHasRefrenceNumberJobChargeAttributes

		[TestDate(2011, 6, 1)]
		public void TestContinerChargeHasRefrenceNumberJobChargeAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			client.MiscServ.OM_IMInvoiceDetailReportSort = InvoiceDetailReportSortList.Codes.JobType;
			client.MiscServ.OM_IMInvoiceDetailReportSort2 = JobChargeAttribTypeList.Codes.DocketReference;

			// Create Warehouse Charge Codes.
			var receiveHandlingCharge = Helper.CreateChargeCode("WRECHAN", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			var continerCharge = Helper.CreateChargeCode("UNPACK", "Continer charge", ChargeCodeGroupList.Codes.WHSInwards, "");

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			Helper.CreateRateLine(warehouseRate, receiveHandlingCharge, "UNT", 5m);
			Helper.CreateRateLine(warehouseRate, continerCharge, "CN", 100m);

			// Create Receive for Receive Handling and Warehouse Storage charges.
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = new ZDateTimeOffset(2011, 2, 3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - receive was not finalised.", true, receive.IsFinalised);
			Helper.CreateWhsDocketContainer(receive, "CN1", "20GP", true, false);

			// Create Periodic Billing. 
			var invoice = CreateInvoice(data.Org1, data.Whs1, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 2, 7));
			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(invoice);
			invoice.AutoRateJobHeader(null);
			jobHeader.SetDefaultsForJob();
			AssertEquals("Preconditon - 2 charges should be created", 2, jobHeader.Charges.Count);

			// Post invoice so Transaction Header would be created.
			invoice.PostInvoice();
			Factory.Save();

			var continerJobCharge = FindJobCharge(jobHeader.Charges, continerCharge);
			AssertEquals("should have DocketReference.", "R1", continerJobCharge.JobChargeAttributes.Cast<JobChargeAttrib>().Single(c => c.EC_Name == JobChargeAttribTypeList.Codes.DocketReference).EC_Value);

			var expectedGroupDesc1_ReceiveHandling = GetChargeCodeDesc(receiveHandlingCharge);
			var expectedGroupDesc2_ContinerCharge = GetChargeCodeDesc(continerCharge);

			var documentWrapper = DocWhsInvoiceDetail.New(jobHeader.PrintingFilter.Transactions[0], Factory);
			AssertEquals("Two charge line should exist.", 2, documentWrapper.JobChargeLines.Count);
			AssertEquals("Should have warehouse charge", "WRECHAN (Warehouse Receive Handling)", expectedGroupDesc1_ReceiveHandling);
			AssertEquals("Should have container charge.", "UNPACK (Continer charge)", expectedGroupDesc2_ContinerCharge);
			AssertEquals("In order to put in same group descriptions should match.", documentWrapper.JobChargeLines[0].GroupDesc, documentWrapper.JobChargeLines[1].GroupDesc);
			AssertEquals("In order to put in same group descriptions should match.", documentWrapper.JobChargeLines[0].GroupDesc2, documentWrapper.JobChargeLines[1].GroupDesc2);
		}

		#endregion

		#region TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSInwards_Attrib1And2IsKey()
		{
			var receiveHandlingCharge = Helper.CreateChargeCode("WRECHAN", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(receiveHandlingCharge, true, true, false);
		}

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSInwards_Attrib2IsKey()
		{
			var receiveHandlingCharge = Helper.CreateChargeCode("WRECHAN", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(receiveHandlingCharge, false, true, false);
		}

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSInwards_Attrib3IsKey()
		{
			var receiveHandlingCharge = Helper.CreateChargeCode("WRECHAN", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(receiveHandlingCharge, false, false, true);
		}

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSStorage_Attrib1And2IsKey()
		{
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(warehouseStorageCharge, true, true, false);
		}

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSStorage_Attrib2IsKey()
		{
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(warehouseStorageCharge, false, true, false);
		}

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSStorage_Attrib3IsKey()
		{
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(warehouseStorageCharge, false, false, true);
		}

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSOutwards_Attrib1And2IsKey()
		{
			var outwardHandlingCharge = Helper.CreateChargeCode("WOUTHAN", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(outwardHandlingCharge, true, true, false);
		}

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSOutwards_Attrib2IsKey()
		{
			var outwardHandlingCharge = Helper.CreateChargeCode("WOUTHAN", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(outwardHandlingCharge, false, true, false);
		}

		[TestDate(2011, 6, 1)]
		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSOutwards_Attrib3IsKey()
		{
			var outwardHandlingCharge = Helper.CreateChargeCode("WOUTHAN", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(outwardHandlingCharge, false, false, true);
		}

		void JobChargeLines_GroupDesc_CopyAttributeInRightColumn_Core(AccChargeCode accChargeCode, bool iMAttrib1IsKey, bool iMAttrib2IsKey, bool iMAttrib3IsKey)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2012, 1, 1));
			Helper.CreateRateLine(warehouseRate, accChargeCode, "UNT", 5m);

			// Create Receive for Receive Handling and Warehouse Storage charges.
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = new ZDateTimeOffset(2011, 2, 3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, ZDate.Empty, ZDate.Empty, "PA1", "Box", "Blue", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - receive was not finalised.", true, receive.IsFinalised);
			Factory.Save();

			// Create Order for Outward Handling charge.
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1");
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "Box", "Blue", "", "");
			order.WD_RequiredDate = new ZDateTimeOffset(2011, 2, 5);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			WhsPick pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Precondition - order was not finalised.", true, order.IsFinalised);
			AssertEquals("Precondition - pick was not finalised.", true, pick.IsFinalised);

			data.Org1.MiscServ.OM_IMAttrib1IsKey = iMAttrib1IsKey;
			data.Org1.MiscServ.OM_IMAttrib2IsKey = iMAttrib2IsKey;
			data.Org1.MiscServ.OM_IMAttrib3IsKey = iMAttrib3IsKey;
			var jobChargeAttrib = CreatePeriodicBilling(data, 2011);
			Assert(iMAttrib1IsKey == jobChargeAttrib.Select(a => a).Any(a => a.EC_Name == "AT1" && a.EC_Value == "PA1"));
			Assert(iMAttrib2IsKey == jobChargeAttrib.Select(a => a).Any(a => a.EC_Name == "AT2" && a.EC_Value == "Box"));
			Assert(iMAttrib3IsKey == jobChargeAttrib.Select(a => a).Any(a => a.EC_Name == "AT3" && a.EC_Value == "Blue"));
		}

		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSInwards_SerialNumberIsKey()
		{
			var receiveHandlingCharge = Helper.CreateChargeCode("WRECHAN", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_SerialNumberCore(receiveHandlingCharge, serialNumberIsKey: true);
		}

		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSInwards_SerialNumberIsNotKey()
		{
			var receiveHandlingCharge = Helper.CreateChargeCode("WRECHAN", "Warehouse Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_SerialNumberCore(receiveHandlingCharge, serialNumberIsKey: false);
		}

		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSStorage_SerialNumberIsKey()
		{
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_SerialNumberCore(warehouseStorageCharge, serialNumberIsKey: true);
		}

		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSStorage_SerialNumberIsNotKey()
		{
			var warehouseStorageCharge = Helper.CreateChargeCode("WSTO", "Warehouse Storage", ChargeCodeGroupList.Codes.WHSStorage, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_SerialNumberCore(warehouseStorageCharge, serialNumberIsKey: false);
		}

		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSOutwards_SerialNumberIsKey()
		{
			var outwardHandlingCharge = Helper.CreateChargeCode("WOUTHAN", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_SerialNumberCore(outwardHandlingCharge, serialNumberIsKey: true);
		}

		public void TestJobChargeLines_GroupDesc_CopyAttributeInRightColumn_WHSOutwards_SerialNumberIsNotKey()
		{
			var outwardHandlingCharge = Helper.CreateChargeCode("WOUTHAN", "Warehouse Outwards Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			JobChargeLines_GroupDesc_CopyAttributeInRightColumn_SerialNumberCore(outwardHandlingCharge, serialNumberIsKey: false);
		}

		void JobChargeLines_GroupDesc_CopyAttributeInRightColumn_SerialNumberCore(AccChargeCode accChargeCode, bool serialNumberIsKey)
		{
			var previousYear = ZDate.Today.Year - 1;
			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			// Create Client Rate.
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			// Create Warehouse Rate Entry and Lines.
			var warehouseRate = Helper.CreateRateEntry(clientRate, new ZDate(previousYear, 1, 1), new ZDate(previousYear + 1, 1, 1));
			Helper.CreateRateLine(warehouseRate, accChargeCode, "UNT", 5m);

			// Create Receive for Receive Handling and Warehouse Storage charges.
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = new ZDateTimeOffset(previousYear, 2, 3);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN1", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - receive was not finalised.", true, receive.IsFinalised);
			Factory.Save();

			// Create Order for Outward Handling charge.
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1");
			var orderline = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderline.WE_SerialNumber = "SN1";
			order.WD_RequiredDate = new ZDateTimeOffset(previousYear, 2, 5);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Precondition - order was not finalised.", true, order.IsFinalised);
			AssertEquals("Precondition - pick was not finalised.", true, pick.IsFinalised);

			data.Org1.MiscServ.OM_IMSerialNumberIsKey = serialNumberIsKey;
			var jobChargeAttrib = CreatePeriodicBilling(data, previousYear);
			Assert(serialNumberIsKey == jobChargeAttrib.Select(a => a).Any(a => a.EC_Name == "SER" && a.EC_Value == "SN1"));
		}

		JobChargeAttribCollection CreatePeriodicBilling(TestDataSimpleEnvironment data, int year)
		{
			var invoice = CreateInvoice(data.Org1, data.Whs1, new ZDateTime(year, 2, 1), new ZDateTime(year, 2, 7));
			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(invoice);
			invoice.AutoRateJobHeader(null);
			Factory.Save();

			var charges = jobHeader.Charges;
			var iAdditionalJob = (IJobInvoicingPlugInAdditionalJobs)invoice;
			var additionalJobs = iAdditionalJob.AdditionalJobsToShowChargesFor.Select(x => x.InvoicingSupporter.Job).Cast<Job>().ToList();

			foreach (var job in additionalJobs)
			{
				if (job != null)
				{
					charges.AddRange(job.Charges);
				}
			}

			invoice.ET_StorageFromDate = ZDateTime.Today.AddDays(1);

			Assert("Preconditon - charges should be created", charges.Any());

			if (charges.Any())
			{
				var result = charges[0].JobChargeAttributes;

				for (int i = 1; i < charges.Count; i++)
				{
					result.AddRange(charges[i].JobChargeAttributes);
				}
				return result;
			}

			return null;
		}

		#endregion

		#endregion

		#region Properties

		public void TestToString()
		{
			ARInvoice.AH_TransactionNum = ARInvoice.AH_ConsolidatedInvoiceRef = "I00100";
			AssertEquals("I00100", DocWrapper.ToString());
		}

		public void TestJobNumber()
		{
			ARInvoice.AH_TransactionNum = ARInvoice.AH_ConsolidatedInvoiceRef = "I00100";
			AssertEquals("I00100", DocWrapper.JobNumber);
		}

		public void TestCurrencyCode()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "MAT";
			ARInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			AssertEquals("MAT", DocWrapper.CurrencyCode);
		}

		public void TestAccountCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTCODE";
			ARInvoice.AH_OH = org.PK;
			AssertEquals("TESTCODE", DocWrapper.AccountCode);
		}

		public void TestInvoiceTitle()
		{
			AssertEquals("Invoice", DocWrapper.InvoiceTitle);
		}

		public void TestFromDate()
		{
			ZDateTime date = ZDateTime.Today.AddDays(1);
			WhsInvoice.ET_StorageFromDate = date;
			AssertEquals(date, DocWrapper.FromDate);
		}

		#region TestFromDateWithNoMatchingWhsInvoice

		public void TestFromDateWithNoMatchingWhsInvoice()
		{
			ARInvoice.Job.JH_ParentID = ZGuid.NewZGuid();

			WhsInvoice.ET_StorageFromDate = ZDateTime.Today.AddDays(1);
			AssertEquals(ZDateTime.Empty, DocWrapper.FromDate);
		}

		#endregion

		public void TestToDate()
		{
			ZDateTime date = ZDateTime.Today.AddDays(2);
			WhsInvoice.ET_StorageToDate = date;
			AssertEquals(date, DocWrapper.ToDate);
		}

		#region TestToDateWithNoMatchingWhsInvoice

		public void TestToDateWithNoMatchingWhsInvoice()
		{
			ARInvoice.Job.JH_ParentID = ZGuid.NewZGuid();

			WhsInvoice.ET_StorageToDate = ZDateTime.Today.AddDays(2);
			AssertEquals(ZDateTime.Empty, DocWrapper.ToDate);
		}

		#endregion

		public void TestWarehouseName()
		{
			WhsWarehouse whs = Factory.New<WhsWarehouse>();
			whs.WW_WarehouseName = "WHS1NAME";
			WhsInvoice.ET_WW = whs.PK;
			AssertEquals("WHS1NAME", DocWrapper.WarehouseName);
		}

		#region TestWarehouseNameWithNoMatchingWhsInvoice

		public void TestWarehouseNameWithNoMatchingWhsInvoice()
		{
			ARInvoice.Job.JH_ParentID = ZGuid.NewZGuid();

			var whs = Factory.New<WhsWarehouse>();
			whs.WW_WarehouseName = "WHS1NAME";
			WhsInvoice.ET_WW = whs.PK;
			AssertEquals(ZString.Empty, DocWrapper.WarehouseName);
		}

		#endregion

		public void TestReportDesc()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			WhsInvoice.ET_OH_Client = org.PK;
			AssertEquals("Breakdown of Invoice charges by Invoice / Job Type. Sorted by Breakdown + Job Date + Reference + Product Code.", DocWrapper.ReportDescription);
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

			Job jobHeader = Helper.CreateRatingJob(WhsInvoice);
			ARInvoice.AH_JH = jobHeader.PK;

			DocWrapper = DocWhsInvoiceDetail.New(ARInvoice, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);

			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		ARInvoice ARInvoice;
		WhsInvoice WhsInvoice;
		DocWhsInvoiceDetail DocWrapper;

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

		#region Notify

		TestNotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer()); }
		}
		TestNotificationBuffer notify;

		#endregion

		#endregion
	}
}
