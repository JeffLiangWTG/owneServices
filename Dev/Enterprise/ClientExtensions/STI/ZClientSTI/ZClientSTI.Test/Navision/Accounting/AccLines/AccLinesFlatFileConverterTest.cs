using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class AccLinesFlatFileConverterTest : TestCaseWithFactory
	{
		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestMapExport()
		{
			CreateInvoice(createExtraInvoices: true);
			AccLinesFlatFileConverterTestClass converter = new AccLinesFlatFileConverterTestClass();
			IValueObjectDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			InvoicingLineBase invoiceLine = (InvoicingLineBase)Invoice.Lines.AddNew();
			invoiceLine.FillWithValidTestData();
			Factory.Save();
			Xsd.TxnHeader header = (Xsd.TxnHeader)adapter.ExportToValueObject(Factory.Load(typeof(InvoicingBase), Invoice.PK), new ValueObjectExportContext(new NotificationBuffer()));
			FlatFileDataRowCollection dataRows = converter.MapExport(header);
			AssertEquals("2 row should be in collection", 2, dataRows.Count);
			AccLinesFlatFileDataRow row = (AccLinesFlatFileDataRow)dataRows[0];
			AssertEquals("Document Type", Constants.CreditNote, row.DocumentType);
			AssertEquals("Sell To Customer Number", header.DebtorOrCreditor.EDICode, row.SellToCustomerNumber);
			AssertEquals("Document Number", "S00004984A", row.DocumentNumber);
			AssertEquals("Line Number", "10000", row.LineNumber);
			AssertEquals("Type", Constants.GLAccount, row.Type);
			AssertEquals("GL Account Number", "4710.00.00", row.GLAccountNumber);
			AssertEquals("Location Code", "BNE", row.LocationCode);
			AssertEquals("Description", "Line Description", row.Description);
			AssertEquals("Quantity", Constants.Quantity, row.Quantity);
			AssertEquals("Unit Price", -100m, row.UnitPrice);
			AssertEquals("Unit Cost", -50m, row.UnitCost);
			AssertEquals("GST Percentage", 15m, row.GSTPercentage);
			AssertEquals("Amount Excluding GST", -100m, row.AmountExcludingGST);
			AssertEquals("Amount Including GST", -115m, row.AmountIncludingGST);
			AssertEquals("Job Number", "S00001968", row.JobNumber);
			AssertEquals("Currency", "USD", row.Currency);
			AssertEquals("Charge Code", "SFSUPER", row.ChargeCode);
			AssertEquals("Short Cut Dimension 1 Code", GlbBranch.CurrentBranch.GB_Code, row.ShortCutDimension1Code);
			row = (AccLinesFlatFileDataRow)dataRows[1];
			AssertEquals("Document Type", Constants.CreditNote, row.DocumentType);
			AssertEquals("Should be BillToCode: Header.DebtorOrCreditor.EDICode", header.DebtorOrCreditor.EDICode, row.SellToCustomerNumber);
			AssertEquals("Document Number", "S00004984A", row.DocumentNumber);
			AssertEquals("Line Number", "20000", row.LineNumber);
			AssertEquals("Type", Constants.GLAccount, row.Type);
			AssertEquals("GL Account Number", "", row.GLAccountNumber);
			AssertEquals("Location Code", "BNE", row.LocationCode);
			AssertEquals("Description", "", row.Description);
			AssertEquals("Qunatity", Constants.Quantity, row.Quantity);
			AssertEquals("Unit Price", 0m, row.UnitPrice);
			AssertEquals("Unit Cost", 0m, row.UnitCost);
			AssertEquals("GST Percentage", 0m, row.GSTPercentage);
			AssertEquals("Amount Excluding GST", 0m, row.AmountExcludingGST);
			AssertEquals("Amount Including GST", 0m, row.AmountIncludingGST);
			AssertEquals("Job Number", "", row.JobNumber);
			AssertEquals("Currency", "USD", row.Currency);
			AssertEquals("Charge Code", "", row.ChargeCode);
			AssertEquals("Short Cut Dimension 1 Code", GlbBranch.CurrentBranch.GB_Code, row.ShortCutDimension1Code);
		}

		public void TestTransactionHeaderIsNotCreditNote()
		{
			CreateInvoice(ZArchitecture.Core.TransactionTypes.Invoice);
			Invoice = Factory.Load<InvoicingBase>(Invoice.PK);
			AccLinesFlatFileConverterTestClass converter = new AccLinesFlatFileConverterTestClass();
			IValueObjectDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			Xsd.TxnHeader header = (Xsd.TxnHeader)adapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			FlatFileDataRowCollection dataRows = converter.MapExport(header);
			AssertEquals("1 row should be in collection", 1, dataRows.Count);
			AccLinesFlatFileDataRow row = (AccLinesFlatFileDataRow)dataRows[0];
			AssertEquals("Document Type should be 'Invoice'", Constants.Invoice, row.DocumentType);
		}

		public void TestGetGLAccount()
		{
			CreateInvoice();
			AccLinesFlatFileConverterTestClass converter = new AccLinesFlatFileConverterTestClass();
			Line.AL_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CFSUNPA")).PK;
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			AssertEquals("Should be 1050.10.10", "1050.10.10", converter.GetGLAccount(Line));
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			AssertEquals("Should be 1050.20.10", "1050.20.10", converter.GetGLAccount(Line));
			Line.AL_AG = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "6510.50.00").PK;
			Line.AL_AC = ZGuid.Empty;
			AssertEquals("Should be 6510.50.00", "6510.50.00", converter.GetGLAccount(Line));
		}

		public void TestGetJobNumber()
		{
			CreateInvoice();
			AccLinesFlatFileConverterTestClass converter = new AccLinesFlatFileConverterTestClass();
			AssertEquals("JobNumber should be S00001968", "S00001968", converter.GetJobNumber(Line));
			Line.AL_JH = ZGuid.Empty;
			AssertEquals("JobNumber should be empty", ZString.Empty, converter.GetJobNumber(Line));
		}

		void CreateExtraInvoices()
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			header.AH_TransactionNum = "00001000";
			header = Factory.NewWithValidTestData<AccTransactionHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			header.AH_TransactionNum = "00001001";
			header = Factory.NewWithValidTestData<AccTransactionHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			header.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			header.AH_TransactionNum = "00001000";
			Factory.Save();
		}

		#region Implementation
		InvoicingBase Invoice;
		InvoicingLineBase Line;
		AccTransactionHeader CreateInvoice(string transactionType = ZArchitecture.Core.TransactionTypes.CreditNote, bool createExtraInvoices = false)
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, ZBool.True);
			filter.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			OrgHeader consignor = Factory.LoadTop1<OrgHeader>(filter);
			consignor.OH_Code = "CONSGNOR";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_UniqueConsignRef = "S00001968";
			Factory.Save();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_JobNum = "S00001968";
			Factory.Save();
			AccTransactionHeader accHeader = Factory.NewWithValidTestData<AccTransactionHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			accHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader.AH_TransactionType = transactionType;
			accHeader.AH_TransactionNum = "00001000";
			accHeader.AH_OH = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			accHeader.AH_ConsolidatedInvoiceRef = "S00004984/A";
			accHeader.AH_JH = job.PK;
			accHeader.AH_RX_NKTransactionCurrency = "USD";
			Invoice = Factory.LoadTop1<InvoicingBase>(new ZQuery());
			Line = (InvoicingLineBase)Invoice.Lines.AddNew();
			Line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Line.AL_JH = job.PK;
			Line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			Line.AL_AH = accHeader.PK;
			Line.AL_GB = GlbBranch.CurrentBranch.PK;
			Line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			Line.AL_Desc = "Line Description";
			Line.AL_OSExTaxAmount = 98654.24m;
			Line.AL_RX_NKTransactionCurrency = "USD";
			Line.AL_ReverseDate = ZDateTime.Now;
			var expectedGLAccount = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "4710.00.00");
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "SFSUPER"));
			chargeCode.AC_AG_RevenueAccount = expectedGLAccount.PK;
			Line.AL_AC = chargeCode.PK;
			Line.AL_ExchangeRate = 0.5m; // Need to set it here because the test is editing an existing line
			Line.AL_OSExTaxAmount = 100m;
			Line.AL_LocalExTaxAmount = 200m;
			Line.AL_LocalTaxAmount = 15m;
			Line.AL_OverseasTotal = 115m;
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>(TestBusinessObjectKind.MinimumRequiredToSave);
			Line.AL_AT = taxRate.PK;
			Line.AL_TaxRateNumerator = 15;
			accHeader.AH_ExchangeRate = Line.AL_ExchangeRate;
			var chargeLine = Factory.NewWithValidTestData<BaseCharge>(TestBusinessObjectKind.MinimumRequiredToSave);
			chargeLine.JR_JH = job.PK;
			chargeLine.JR_RX_NKCostCurrency = "USD";
			chargeLine.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			chargeLine.JR_LocalCostAmt = -50m;
			chargeLine.JR_AL_ARLine = Line.PK;
			chargeLine.SetAmountsFromLinkedLinesForTests();
			Factory.Save();
			if (createExtraInvoices)
			{
				CreateExtraInvoices();
			}
			return accHeader;
		}

		class AccLinesFlatFileConverterTestClass : AccLinesFlatFileConverter
		{
			public AccLinesFlatFileConverterTestClass() : base(new NotificationBuffer(), new BusinessObjectFactory())
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}

			public new ZString GetGLAccount(InvoicingLineBase line)
			{
				return base.GetGLAccount(line);
			}

			public new ZString GetJobNumber(InvoicingLineBase line)
			{
				return base.GetJobNumber(line);
			}
		}
		#endregion
	}
}
