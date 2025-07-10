using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class TransactionExportTestDataHelper
	{
		public TransactionExportTestDataHelper(BusinessObjectFactory factory, bool shouldCreateInvoice = true)
		{
			Factory = factory;
			ShouldCreateInvoice = shouldCreateInvoice;
			Setup();
		}

		bool ShouldCreateInvoice { get; }

		#region Setup

		void Setup()
		{
			Header = Factory.NewWithValidTestData<OrgHeader>();
			Header.OH_IsDebtor = true;
			Header.OH_IsCreditor = true;
			Header.OH_FullName = @"The Fullname of the Organisation";

			ObjectCreator = new TestObjectCreator(Factory);

			Consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			Consol1.JK_MasterBillNum = "ABCDEFGH";

			Shipment1 = Consol1.Shipments.AddNew();
			Shipment1.JS_INCO = "FOB";
			Shipment1.JS_UniqueConsignRef = "S12341234";
			Shipment1.JS_HouseBill = "UVWXYZ";
			Shipment1.JS_RL_NKOrigin = "AUSYD";
			Shipment1.JS_RL_NKDestination = "USLAX";

			Job1 = Factory.NewJobForTesting<Job>();
			Job1.JH_ParentTableCode = "JS";
			Job1.JH_ParentID = Shipment1.PK;
			Job1.JH_GB = GlbBranch.CurrentBranch.PK;
			Job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Job1.JH_JobNum = Shipment1.JS_UniqueConsignRef;
			Job1.JH_GC = GlbCompany.CurrentCompany.PK;
			Job1.JH_JobNum = "HA233333";

			if (ShouldCreateInvoice)
			{
				ARInvoice = (ARInvoice)PopulateInvoice(typeof(ARInvoice), 100.00M, 10.00M, 0.50M, ObjectCreator.USD);
				ARInvoiceOriginalLedger = ARInvoice.AH_Ledger;
				ARInvoiceOriginalTransactionType = ARInvoice.AH_TransactionType;
			}

			Consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			Consol2.JK_MasterBillNum = "HAHAHAHHA";

			Shipment2 = Consol2.Shipments.AddNew();
			Shipment2.JS_INCO = "FOB";
			Shipment2.JS_UniqueConsignRef = "S00001234";
			Shipment2.JS_HouseBill = "UVWwwXYZ";
			Shipment2.JS_RL_NKOrigin = "AUSYD";
			Shipment2.JS_RL_NKDestination = "USLAX";

			Job2 = Factory.NewJobForTesting<Job>();
			Job2.JH_ParentTableCode = "JS";
			Job2.JH_ParentID = Shipment2.PK;
			Job2.JH_GB = GlbBranch.CurrentBranch.PK;
			Job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Job2.JH_JobNum = Shipment2.JS_UniqueConsignRef;

			Factory.Save();

			FillWIPAccrualBizObjWithTestData(typeof(WIP), ObjectCreator.CC4, 100.0M);

			AddTransactionsToDataBase();

			Factory.Save();
		}

		#endregion

		#region AddTransactionsToDataBase

		void AddTransactionsToDataBase()
		{
			Invoice = Factory.New<APInvoice>();
			Invoice.AH_OH = Header.PK;

			Invoice.AH_TransactionNum = "000010004";
			Invoice.AH_PostDate = PostDate;
			Invoice.AH_InvoiceDate = PostDate.AddDays(-1);
			Invoice.AH_DueDate = PostDate.AddDays(1);
			Invoice.AH_ConsolidatedInvoiceRef = Shipment1.JS_UniqueConsignRef;

			InvoicingLineBase line1 = (InvoicingLineBase)Invoice.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)Invoice.Lines.AddNew();

			AccChargeCode chargecode = ObjectCreator.CC2;
			line1.AL_AC = chargecode.PK;

			line1.AL_JH = Job1.PK;
			line1.AL_OSExTaxAmount = 35.00M;
			line1.AL_OSTaxAmount = 3.50M;
			line1.AL_AT = ObjectCreator.GST1.PK;
			line2.AL_OSWHTAmount = 5.00M;
			line2.AL_AW = Factory.NewWithValidTestData(typeof(AccWithholding)).PK;

			line2.AL_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			line2.AL_OSExTaxAmount = 100.00M;
			line2.AL_OSTaxAmount = 10.00M;

			var charge = ObjectCreator.CreateJobCharge(line1, Job1, chargecode, ObjectCreator.AUD);
			charge.JR_OSSellAmt = 0m;   // Should not create WIP 

			Factory.Save();
		}

		#endregion

		#region PopulateInvoice

		public InvoicingBase PopulateInvoice(Type type, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, RefCurrency currency)
		{
			Invoice = (InvoicingBase)Factory.New(type);

			Invoice.AH_OH = Header.PK;
			Invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			Invoice.AH_Desc = "This is a test description to see how the XML Export works";
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Invoice.AH_Ledger = "AR";
			Invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			Invoice.AH_ExchangeRate = exchangeRate;
			Invoice.AH_PostDate = PostDate;
			Invoice.AH_InvoiceDate = PostDate.AddDays(-1);
			Invoice.AH_InvoiceTerm = "INV";
			Invoice.AH_InvoiceTermDays = 1;
			Invoice.AH_DueDate = PostDate.AddDays(1);
			Invoice.AH_TransactionReference = @"Shipment ABC123";

			InvoicingLineBase line1 = (InvoicingLineBase)Invoice.Lines.AddNew();
			PopulateInvoiceLine(line1, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), 0.50M, Invoice.PK);

			InvoicingLineBase line2 = (InvoicingLineBase)Invoice.Lines.AddNew();
			PopulateInvoiceLine(line2, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), 0.50M, Invoice.PK);

			return Invoice;
		}

		void PopulateInvoiceLine(InvoicingLineBase line, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, ZGuid header)
		{
			line.AL_AC = ObjectCreator.CC1.PK;
			line.AL_AH = header;
			line.AL_AT = ObjectCreator.GST1.PK;
			line.AL_Desc = "Transaction Line Description";
			line.AL_RX_NKTransactionCurrency = ObjectCreator.USD.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_OSExTaxAmount = oSExTaxAmount;
			line.AL_OSTaxAmount = oSTaxAmount;
			line.AL_PostDate = PostDate;
			line.AL_Sequence = 1;
		}

		#endregion

		#region FillWIPAccrualBizObjWithTestData

		void FillWIPAccrualBizObjWithTestData(Type type, AccChargeCode chargeCode, decimal localExTaxAmount)
		{
			WIP = (BaseWIPAccrual)Factory.NewWithValidTestData(type);
			WIP.AL_AC = chargeCode.PK;

			WIP.AL_ExchangeRate = 1m;
			WIP.AL_OSExTaxAmount = localExTaxAmount;
			WIP.AL_LocalExTaxAmount = localExTaxAmount;
			WIP.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			WIP.AL_PostDate = PostDate;
			WIP.AL_AC = chargeCode.PK;
			WIP.AL_Desc = "Description of Accrual or WIP";
			WIP.AL_JH = Job2.PK;

			BaseCharge charge = Factory.NewWithValidTestData<BaseCharge>();
			charge.JR_JH = Job2.PK;
			if (WIP.AL_LineType == TransactionLineTypes.WIP)
			{
				charge.JR_AL_ARLine = WIP.PK;
				charge.SetChargeValuesFromLinkedARLineForTests();
				charge.JR_OSCostAmt = 0m;
			}
			if (WIP.AL_LineType == TransactionLineTypes.Accrual)
			{
				charge.JR_AL_APLine = WIP.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();
				charge.JR_OSSellAmt = 0m;
			}

			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(WIP);
		}

		#endregion

		ZDateTime PostDate
		{
			get { return new ZDateTime(2005, 01, 01, 10, 30, 0); }
		}

		public BaseWIPAccrual WIP;
		public ForwardingConsol Consol1;
		public ForwardingConsol Consol2;
		public ForwardingShipment Shipment1;
		public ForwardingShipment Shipment2;
		public Job Job1;
		public Job Job2;
		public InvoicingBase Invoice;
		public ARInvoice ARInvoice;
		public string ARInvoiceOriginalLedger;
		public string ARInvoiceOriginalTransactionType;
		public TestObjectCreator ObjectCreator;
		public OrgHeader Header;
		public readonly BusinessObjectFactory Factory;
	}
}
