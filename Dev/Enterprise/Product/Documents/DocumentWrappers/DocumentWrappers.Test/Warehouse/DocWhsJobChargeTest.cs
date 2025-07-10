using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.MasterFiles.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Warehouse.Testing
{
	[TestedType(typeof(DocWhsJobCharge))]
	sealed class DocWhsJobChargeTest : DocJobChargeTest
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWhsJobCharge.New(Charge, Factory) };
		}

		public void TestSellAmountFromAccTransLine()
		{
			AssertEquals(0M, WhsChargeWrapper.SellAmountFromAccTransLine);
			var invoice = Factory.New<ARInvoice>();
			invoice.Lines.AddNew(typeof(ARInvoiceLine));
			var line = (TransactionLine)invoice.Lines[0];
			Charge.JR_AL_ARLine = line.PK;
			line.AL_OSAmount = 100.45m;
			line.AL_OSExTaxAmount = 987.34m;
			AssertEquals(987.34m, WhsChargeWrapper.SellAmountFromAccTransLine);
		}

		public void TestConsigneeName()
		{
			var receiveCharge = Factory.New<JobCharge>();
			var receive = Factory.New<WhsReceive>();
			receive.WD_DocketID = "R1";
			receiveCharge.JR_OrderReference = "R1";

			AssertEquals("", DocWhsJobCharge.New(receiveCharge, Factory).ConsigneeName);

			var orderCharge = Factory.New<JobCharge>();
			var order = Factory.New<WhsOrder>();
			order.WD_DocketID = "O1";
			orderCharge.JR_OrderReference = "O1";
			AssertEquals("", DocWhsJobCharge.New(orderCharge, Factory).ConsigneeName);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "CONSIGNEE";
			AssertEquals("CONSIGNEE", DocWhsJobCharge.New(orderCharge, Factory).ConsigneeName);
		}

		public override void TestDescription()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			Charge.JR_AC = chargeCode.PK;
			chargeCode.AC_Desc = "ChargeCodeDesc";
			WhsChargeWrapper.GroupType = InvoiceDetailReportSortList.Codes.ChargeCode;

			Charge.JR_Desc = "SomeDesciption";
			AssertEquals("SomeDesciption", WhsChargeWrapper.Description);

			Charge.JR_Desc = "ChargeCodeDesc SomeDesciption";
			AssertEquals("SomeDesciption", WhsChargeWrapper.Description);

			Charge.JR_Desc = "ChargeCodeDesc - SomeDesciption";
			AssertEquals("SomeDesciption", WhsChargeWrapper.Description);

			WhsChargeWrapper.GroupType = InvoiceDetailReportSortList.Codes.JobType;
			AssertEquals("ChargeCodeDesc - SomeDesciption", WhsChargeWrapper.Description);
		}

		public void TestStorageDescription()
		{
			Charge.JR_OrderReference = ""; // clear current docket attachment
			Charge.JR_Desc = "SomeDesciption";
			AssertEquals("SomeDesciption", WhsChargeWrapper.StorageDescription);

			Charge.JR_OrderReference = "W1"; // re-attach to docket created in setup
			AssertEquals("", WhsChargeWrapper.StorageDescription);
		}

		public void TestOrderReference()
		{
			Charge.JR_OrderReference = "SomeRef";
			AssertEquals("", WhsChargeWrapper.OrderReference);

			WhsReceive receive = Factory.New<WhsReceive>();
			receive.WD_DocketID = "SomeRef";
			receive.WD_ExternalReference = "OrderNo";

			AssertEquals("OrderNo", WhsChargeWrapper.OrderReference);

			ChargeWrapper = WhsChargeWrapper = DocWhsJobCharge.New(Charge, Factory);
			Charge.JR_OrderReference = "";
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = receive.PK;
			job.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			AssertEquals("", WhsChargeWrapper.OrderReference);
			Charge.JR_JH = job.PK;
			AssertEquals("OrderNo", WhsChargeWrapper.OrderReference);
		}

		public void TestJobDate()
		{
			Charge.JR_OrderReference = ""; // clear current docket attachment
			AssertEquals(ZDateTimeOffset.Empty, WhsChargeWrapper.JobDate);

			Charge.JR_OrderReference = "W1"; // re-attach to docket created in setup
			Factory.Load<WhsDocket>(WhsChargeWrapper.DocketPK).WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals(ZDateTimeOffset.Today, WhsChargeWrapper.JobDate);
		}

		public void TestGroupDesc()
		{
			WhsChargeWrapper.GroupDesc = "XXX";
			AssertEquals("GroupDesc", "XXX", WhsChargeWrapper.GroupDesc);
			WhsChargeWrapper.GroupDesc = "XYZ";
			AssertEquals("GroupDesc", "XYZ", WhsChargeWrapper.GroupDesc);

			WhsChargeWrapper.GroupDesc2 = "XXX";
			AssertEquals("GroupDesc2", "XXX", WhsChargeWrapper.GroupDesc2);
			WhsChargeWrapper.GroupDesc2 = "XYZ";
			AssertEquals("GroupDesc2", "XYZ", WhsChargeWrapper.GroupDesc2);

			WhsChargeWrapper.GroupDesc3 = "XXX";
			AssertEquals("GroupDesc3", "XXX", WhsChargeWrapper.GroupDesc3);
			WhsChargeWrapper.GroupDesc3 = "XYZ";
			AssertEquals("GroupDesc3", "XYZ", WhsChargeWrapper.GroupDesc3);
		}

		public void TestGroupType()
		{
			WhsChargeWrapper.GroupType = "XXX";
			AssertEquals("GroupType", "XXX", WhsChargeWrapper.GroupType);
			WhsChargeWrapper.GroupType = "XYZ";
			AssertEquals("GroupType", "XYZ", WhsChargeWrapper.GroupType);

			WhsChargeWrapper.GroupType2 = "XXX";
			AssertEquals("GroupType2", "XXX", WhsChargeWrapper.GroupType2);
			WhsChargeWrapper.GroupType2 = "XYZ";
			AssertEquals("GroupType2", "XYZ", WhsChargeWrapper.GroupType2);

			WhsChargeWrapper.GroupType3 = "XXX";
			AssertEquals("GroupType3", "XXX", WhsChargeWrapper.GroupType3);
			WhsChargeWrapper.GroupType3 = "XYZ";
			AssertEquals("GroupType3", "XYZ", WhsChargeWrapper.GroupType3);
		}

		#region TestGoodsBillToName

		public void TestGoodsBillToName()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1");
			order.WD_DocketID = "W00000123";

			var charge = Factory.New<JobCharge>();
			charge.JR_OrderReference = order.WD_DocketID;

			var docCharge = DocWhsJobCharge.New(charge, Factory);
			AssertEquals("", docCharge.GoodsBillToName);

			var goodsBillTo = helper.CreateClient("GOODSBILLTO", "GOODS BILL TO COMPANY");
			order.GoodsBillToDocAddress.E2_OA_Address = goodsBillTo.MainAddress.PK;
			AssertEquals("GOODS BILL TO COMPANY", docCharge.GoodsBillToName);

			order.GoodsBillToDocAddress.E2_AddressOverride = true;
			order.GoodsBillToDocAddress.E2_CompanyName = "OVERRIDEN COMPANY NAME";
			AssertEquals("OVERRIDEN COMPANY NAME", docCharge.GoodsBillToName);
		}

		#endregion

		#region Implementation

		DocWhsJobCharge WhsChargeWrapper;

		protected override void SetUp()
		{
			base.SetUp();
			ChargeWrapper = WhsChargeWrapper = DocWhsJobCharge.New(Charge, Factory);

			WhsWarehouse warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			WhsReceive receive = Factory.NewWithValidTestData<WhsReceive>();
			receive.WD_WW_Whs = warehouse.PK;
			receive.WD_DocketID = Charge.JR_OrderReference = "W1";
			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "TST";
			receive.WD_OH_Client = client.PK;
		}

		#endregion
	}
}
