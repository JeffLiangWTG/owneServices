using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
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
	public class AccHeaderFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestMapExport()
		{
			AccHeaderFlatFileConverterTestClass converter = new AccHeaderFlatFileConverterTestClass();
			NotificationBuffer notify = new NotificationBuffer();
			IValueObjectDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			Xsd.TxnHeader header = (Xsd.TxnHeader)adapter.ExportToValueObject(Factory.Load(typeof(InvoicingBase), CreateInvoice().PK), new ValueObjectExportContext(notify));
			FlatFileDataRowCollection dataRows = converter.MapExport(header);
			AssertEquals("Should be one row in the collection", 1, dataRows.Count);
			AccHeaderFlatFileDataRow row = (AccHeaderFlatFileDataRow)dataRows[0];
			AssertEquals("Transaction type", "Invoice", row.DocumentType);
			AssertEquals("Sell to Customer Number", "DEBTRCO", row.BillToCustomerNumber);
			AssertEquals("Sell to Name", "Debtor Full Name", row.BillToName);
			AssertEquals("Sell to Address1", "Suite 3", row.BillToAddress);
			AssertEquals("Sell to Address2", "50 Assembly Drive", row.BillToAddress2);
			AssertEquals("Sell to City", "Tullamarine", row.BillToCity);
			AssertEquals("Sell to Post Code", "3043", row.BillToPostCode);
			AssertEquals("Sell to County", "VIC", row.BillToCounty);
			AssertEquals("Sell to Country Code", "AU", row.BillToCountryCode);
			AssertEquals("Invoice Number", "S000165465A", row.InvoiceNumber);
			AssertEquals("Bill to Customer Number", "DEBTRCO", row.BillToCustomerNumber);
			AssertEquals("Bill to Name", "Debtor Full Name", row.BillToName);
			AssertEquals("Bill to Address1", "Suite 3", row.BillToAddress);
			AssertEquals("Bill to Address2", "50 Assembly Drive", row.BillToAddress2);
			AssertEquals("Bill to City", "Tullamarine", row.BillToCity);
			AssertEquals("Bill to Post Code", "3043", row.BillToPostCode);
			AssertEquals("Bill to County", "VIC", row.BillToCounty);
			AssertEquals("Bill to Country Code", "AU", row.BillToCountryCode);
			AssertEquals("Your Reference", "OrderReference", row.YourReference);
			AssertEquals("Post Date", new ZDateTime(2006, 2, 13), row.PostingDate);
			AssertEquals("Payment Terms Code", "INV30", row.PaymentTermsCode);
			AssertEquals("Due Date", new ZDateTime(2006, 2, 14), row.DueDate);
			AssertEquals("Customer Posting Group", "TPY", row.CustomerPostingGroup);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Australia, row.Currency);
			AssertEquals("Document Date", new ZDateTime(2006, 2, 13), row.DocumentDate);
			AssertEquals("External Document Number", "S000165465/A", row.ExternalDocumentNumber);
			AssertEquals("Short Dimension 1 Code", GlbBranch.CurrentBranch.GB_Code, row.ShortCutDimension1Code);
		}

		public void TestExportForJobNotRaisedAgainstShipmentOrJobDec()
		{
			InvoicingBase invoice = Factory.Load<InvoicingBase>(CreateInvoice().PK);
			invoice.AH_ConsolidatedInvoiceRef = "";
			AccHeaderFlatFileConverterTestClass converter = new AccHeaderFlatFileConverterTestClass();
			NotificationBuffer notify = new NotificationBuffer();
			IValueObjectDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			Xsd.TxnHeader header = (Xsd.TxnHeader)adapter.ExportToValueObject(invoice, new ValueObjectExportContext(notify));
			header.TxnType = Xsd.TxnType.CRD;
			FlatFileDataRowCollection dataRows = converter.MapExport(header);
			AssertEquals("1 row is returned", 1, dataRows.Count);
			AccHeaderFlatFileDataRow row = (AccHeaderFlatFileDataRow)dataRows[0];
			AssertEquals("Transaction Type", "CreditNote", row.DocumentType);
			AssertEquals("Sell To Customer Number", "DEBTRCO", row.SellToCustomerNumber);
			AssertEquals("Sell to customer name", "Debtor Full Name", row.SellToCustomerName);
			AssertEquals("Sell to Address", "Suite 3", row.SellToAddress);
			AssertEquals("Sell to Address 2", "50 Assembly Drive", row.SellToAddress2);
			AssertEquals("Sell to City", "Tullamarine", row.SellToCity);
			AssertEquals("Sell to Post Code", "3043", row.SellToPostCode);
			AssertEquals("Sell to County", "VIC", row.SellToCounty);
			AssertEquals("Sell to Country Code", "AU", row.SellToCountryCode);
			AssertEquals("Your Reference", "description", row.YourReference);
		}

		#region CreateInvoice
		AccTransactionHeader CreateInvoice()
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			ZString aUDCode = Core.Constants.CurrencyCodes.Australia;
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_ConsolidatedInvoiceRef = "S000165465/A";
			invoice.AH_PostDate = new ZDateTime(2006, 2, 13);
			invoice.AH_DueDate = new ZDateTime(2006, 2, 14);
			invoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			invoice.AH_InvoiceTermDays = 30;
			invoice.AH_RX_NKTransactionCurrency = aUDCode;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_TransactionNum = "000000001";
			invoice.AH_Desc = "description";
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>(TestBusinessObjectKind.MinimumRequiredToSave);
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_AH = invoice.PK;
			line.AL_RX_NKTransactionCurrency = aUDCode;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			debtor.OH_IsDebtor = true;
			debtor.OH_FullName = "Debtor Full Name";
			debtor.MainAddress.OA_Address1 = "Suite 3";
			debtor.MainAddress.OA_Address2 = "50 Assembly Drive";
			debtor.MainAddress.OA_City = "Tullamarine";
			debtor.MainAddress.OA_PostCode = "3043";
			debtor.MainAddress.OA_State = "Vic";
			debtor.OH_RL_NKClosestPort = "AUMEL";
			debtor.MiscServ.OM_OJ_ARDebtorGroup = Factory.LoadFromNaturalKey(typeof(OrgDebtorGroup), OrgDebtorGroupSchema.OJ_Code, (ZString)"TPY").PK;
			debtor.OH_Code = "DEBTRCO";
			invoice.AH_OH = debtor.PK;
			line.AL_OH = debtor.PK;
			Factory.Save();
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True));
			consignee.OH_FullName = "Consignee Full Name";
			consignee.MainAddress.OA_Address1 = "Level 2";
			consignee.MainAddress.OA_Address2 = "184 Bourke Road";
			consignee.MainAddress.OA_City = "Alexandria";
			consignee.MainAddress.OA_PostCode = "2015";
			consignee.MainAddress.OA_State = "NSW";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_Code = "CONCODE";
			Factory.Save();
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.DocsAndCartage.JP_OrderItemsAsString = "Order Reference";
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_JobNum = "S000165465";
			invoice.AH_JH = job.PK;
			line.AL_JH = job.PK;
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AL_ARLine = line.PK;
			Factory.Save();
			return invoice;
		}

		#endregion
		class AccHeaderFlatFileConverterTestClass : AccHeaderFlatFileConverter
		{
			public AccHeaderFlatFileConverterTestClass() : base(new NotificationBuffer(), new BusinessObjectFactory())
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
	}
}
