using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	[TestedType(typeof(FinancialInvoiceDataAdapterTestClass))]
	class FinancialInvoiceXmlDataAdapterTest : BaseAccountingDataAdapterTest<InvoicingBase, Xsd.TxnHeader>
	{
		#region Exporting

		public void TestDepartmentActivity()
		{
			Xsd.TxnHeader xmlTransaction = new Xsd.TxnHeader();
			ExportFinancialInvoiceDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ExportToValueObject(CreateFullyPopulatedInvoiceBizObj(), xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(Xsd.DepartmentActivity.Forwarding, xmlTransaction.TxnLines[0].DepartmentActivity);
			Assert("DepartmentActivity was not specified", xmlTransaction.TxnLines[0].DepartmentActivitySpecified);
		}

		public void TestExportingLineWithAttachedShipment_BookingReferenceAndRevenueRecognitionDate()
		{
			InvoicingBase invoice = FullyPopulatedInvoiceWithOneLineBizObj;
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_BookingReference = "Booking Reference";

			Job lineJob = invoiceLine.InvoicingJob;
			lineJob.JH_JobNum = "0123456789";
			lineJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			lineJob.JH_ParentID = shipment.PK;

			var expectedRevRecognitionDate = invoiceLine.AL_ReverseDate;

			Factory.Save();

			Xsd.TxnHeader xmlTransaction = new Xsd.TxnHeader();
			ExportFinancialInvoiceDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ExportToValueObject(invoice, xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(1, xmlTransaction.TxnLines.Count);

			AssertEquals("Booking Reference", xmlTransaction.TxnLines[0].BookingReference);
			AssertEquals(expectedRevRecognitionDate, xmlTransaction.TxnLines[0].RevenueRecognitionDate);
		}

		public void TestExportingLineWithAttachedJobConsolWillProvideETDAndETA()
		{
			InvoicingBase invoice = FullyPopulatedInvoiceWithOneLineBizObj;
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			JobVoyage voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ANRO ASIA", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "192";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			origin.JA_E_DEP = new ZDateTime(2012, 5, 5);
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "CHRRC";
			destination.JB_E_ARV = new ZDateTime(2012, 6, 6);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;

			Job lineJob = invoiceLine.InvoicingJob;
			lineJob.JH_JobNum = "C123456789";
			lineJob.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			lineJob.JH_ParentID = consol.PK;

			Factory.Save();

			var jobDetails = invoiceLine.Job.LoadGenericJob<GenericJob>();
			AssertEquals("Expect View does not provide ETA", ZDateTime.Empty, jobDetails.VJ_ETA);
			AssertEquals("Expect View does not provide ETD", ZDateTime.Empty, jobDetails.VJ_ETD);
			AssertEquals(new ZDateTime(2012, 6, 6), jobDetails.InvoicingSupporter.ETA);
			AssertEquals(new ZDateTime(2012, 5, 5), jobDetails.InvoicingSupporter.ETD);

			Xsd.TxnHeader xmlTransaction = new Xsd.TxnHeader();
			ExportFinancialInvoiceDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ExportToValueObject(invoice, xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(1, xmlTransaction.TxnLines.Count);

			AssertEquals("Expect InvoicingSupporter to provide ETA", jobDetails.InvoicingSupporter.ETA, xmlTransaction.TxnLines[0].ETA);
			AssertEquals("Expect InvoicingSupporter to provide ETD", jobDetails.InvoicingSupporter.ETD, xmlTransaction.TxnLines[0].ETD);
		}

		public void TestExportingLineWithAttachedJobDecWillProvideAgentsReference()
		{
			InvoicingBase invoice = FullyPopulatedInvoiceWithOneLineBizObj;
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			const string AgentRef = "AGENTREF123";
			BaseJobDeclaration jobDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobDec.JE_AgentsReference = AgentRef;

			Job lineJob = invoiceLine.InvoicingJob;
			lineJob.JH_JobNum = "B123456789";
			lineJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			lineJob.JH_ParentID = jobDec.PK;

			Factory.Save();

			Xsd.TxnHeader xmlTransaction = new Xsd.TxnHeader();
			ExportFinancialInvoiceDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ExportToValueObject(invoice, xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(1, xmlTransaction.TxnLines.Count);

			AssertEquals(AgentRef, xmlTransaction.TxnLines[0].AgentsReference);
		}

		public void TestMultipleSubAccountsIsNotPopulatedForSisterCompanyInvoiceImport()
		{
			var invoice = CreateFullyPopulatedInvoiceBizObj();
			AssertEquals(2, invoice.Lines.Count);

			AssertEquals("Precondition", Core.Constants.SubAccountType.Organization, SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(invoice.Lines[1].SubAccounts[0].AL1_SubClassParentTableCode));
			AssertEquals(TestObjectCreator.ABIGAS.PK, invoice.Lines[1].SubAccounts[0].AL1_SubClassParentId);

			invoice.Factory.SetContext(BusinessContext.InterCompanyInvoiceExport);

			var xmlTransaction = new Xsd.TxnHeader();
			var adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ExportToValueObject(invoice, xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(2, xmlTransaction.TxnLines.Count);
			AssertEquals("Skip to import Sub Accounts when inter-company context specified", 0, xmlTransaction.TxnLines[0].SubAccounts.Count);
			AssertEquals(0, xmlTransaction.TxnLines[1].SubAccounts.Count);
		}

		public void TestExportMultipleSubAccounts()
		{
			var invoice = CreateFullyPopulatedInvoiceBizObj();
			AssertEquals(2, invoice.Lines.Count);

			AssertEquals("Precondition", Core.Constants.SubAccountType.Organization, SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(invoice.Lines[1].SubAccounts[0].AL1_SubClassParentTableCode));
			AssertEquals(TestObjectCreator.ABIGAS.PK, invoice.Lines[1].SubAccounts[0].AL1_SubClassParentId);

			var xmlTransaction = new Xsd.TxnHeader();
			var adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ExportToValueObject(invoice, xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(2, xmlTransaction.TxnLines.Count);
			AssertEquals("Import 2 Sub Accounts", 0, xmlTransaction.TxnLines[0].SubAccounts.Count);
			AssertEquals(2, xmlTransaction.TxnLines[1].SubAccounts.Count);

			AssertEquals(Core.Constants.SubAccountType.Organization, xmlTransaction.TxnLines[1].SubAccounts[0].Type.Code);
			AssertEquals(TestObjectCreator.ABIGAS.OH_Code, xmlTransaction.TxnLines[1].SubAccounts[0].Code);
			AssertEquals(Core.Constants.SubAccountType.StaffAndResources, xmlTransaction.TxnLines[1].SubAccounts[1].Type.Code);
			AssertEquals("TST", xmlTransaction.TxnLines[1].SubAccounts[1].Code);
		}

		public void TestExportingLineWithAttachedShipmentThatHasDeclaration()
		{
			InvoicingBase invoice = FullyPopulatedInvoiceWithOneLineBizObj;
			InvoicingLineBase invoiceLine = invoice.Lines[0];

			const string AgentRef = "AGENTREF123";
			BaseJobDeclaration jobDec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobDec.JE_AgentsReference = AgentRef;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			jobDec.JE_JS = shipment.PK;

			Job lineJob = invoiceLine.InvoicingJob;
			lineJob.JH_JobNum = "0123456789";
			lineJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			lineJob.JH_ParentID = shipment.PK;

			Factory.Save();

			Xsd.TxnHeader xmlTransaction = new Xsd.TxnHeader();
			ExportFinancialInvoiceDataAdapter adapter = new ExportFinancialInvoiceDataAdapter();
			adapter.ExportToValueObject(invoice, xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(1, xmlTransaction.TxnLines.Count);

			AssertEquals(AgentRef, xmlTransaction.TxnLines[0].AgentsReference);
		}

		public void TestExportingLineWithOSChargeAmount()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);

			PopulateInvoiceBizObj(typeof(ARInvoice), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);

			Job jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>(TestBusinessObjectKind.MinimumRequiredToSave);
			jobHeader.JH_JobNum = shipment.JobNumber;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipment.PK;

			Invoice.AH_JH = jobHeader.PK;

			ARInvoiceLine line1 = (ARInvoiceLine)Invoice.Lines[0];
			line1.AL_JH = jobHeader.PK;
			line1.AL_ExchangeRate = 0.5;
			line1.AL_OSExTaxAmount = 150m;
			line1.AL_OSTaxAmount = 15m;

			Charge chargeforLine1 = jobHeader.Charges.AddNew();
			chargeforLine1.JR_AC = line1.AL_AC;
			chargeforLine1.JR_JH = jobHeader.PK;
			chargeforLine1.JR_RX_NKSellCurrency = ObjectCreator.USD.RX_Code;
			chargeforLine1.JR_OSSellAmt = line1.AL_OSExTaxAmount;
			chargeforLine1.JR_OSSellExRate = line1.AL_ExchangeRate;
			chargeforLine1.JR_AL_ARLine = line1.PK;
			chargeforLine1.SetAmountsFromLinkedLinesForTests();

			ARInvoiceLine line1_2 = (ARInvoiceLine)Invoice.Lines[1];
			line1_2.AL_JH = jobHeader.PK;
			line1_2.AL_ExchangeRate = 0.5;
			line1_2.AL_OSExTaxAmount = 150m;
			line1_2.AL_OSTaxAmount = 15m;

			Charge chargeforLine1_2 = jobHeader.Charges.AddNew();
			chargeforLine1_2.JR_AC = line1_2.AL_AC;
			chargeforLine1_2.JR_JH = jobHeader.PK;
			chargeforLine1_2.JR_RX_NKSellCurrency = ObjectCreator.USD.RX_Code;
			chargeforLine1_2.JR_OSSellAmt = line1_2.AL_OSExTaxAmount;
			chargeforLine1_2.JR_OSSellExRate = line1_2.AL_ExchangeRate;
			chargeforLine1_2.JR_AL_ARLine = line1_2.PK;
			chargeforLine1_2.SetAmountsFromLinkedLinesForTests();

			Xsd.TxnLine xmlInvoiceLine = new Xsd.TxnLine();

			DataAdapter.PopulateValuesForXmlInvoiceLine(xmlInvoiceLine, line1, new NotificationBuffer());
			AssertEquals("OSChargeAmount Currency", xmlInvoiceLine.OSChargeAmount.CurrencyCode, ObjectCreator.USD.RX_Code);
			AssertEquals("OSChargeAmount", 150m, xmlInvoiceLine.OSChargeAmount.Value);

			PopulateInvoiceBizObj(typeof(APInvoice), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
			Invoice.AH_JH = jobHeader.PK;

			APInvoiceLine line2 = (APInvoiceLine)Invoice.Lines[0];
			line2.AL_JH = jobHeader.PK;
			line2.AL_ExchangeRate = 0.5;
			line2.AL_OSExTaxAmount = 150m;
			line2.AL_OSTaxAmount = 15m;

			Charge chargeforLine2 = jobHeader.Charges.AddNew();
			chargeforLine2.JR_AC = line2.AL_AC;
			chargeforLine2.JR_JH = jobHeader.PK;
			chargeforLine2.JR_RX_NKSellCurrency = ObjectCreator.USD.RX_Code;
			chargeforLine1.JR_OSSellAmt = line2.AL_OSExTaxAmount;
			chargeforLine1.JR_OSSellExRate = line2.AL_ExchangeRate;
			chargeforLine2.JR_AL_APLine = line2.PK;
			chargeforLine2.SetAmountsFromLinkedLinesForTests();

			xmlInvoiceLine = new Xsd.TxnLine();
			DataAdapter.PopulateValuesForXmlInvoiceLine(xmlInvoiceLine, line2, new NotificationBuffer());
			AssertEquals("OSChargeAmount shouldn't specify on on AP invoice ", false, xmlInvoiceLine.OSChargeAmount.IsSpecified);
		}

		[ExpectNoExceptions]
		public void TestTransactionCurrencyNullable()
		{
			InvoicingBase invoicingBase = CreateFullyPopulatedInvoiceBizObj();
			invoicingBase.AH_RX_NKTransactionCurrency = null;
			invoicingBase.Lines[0].Job.LoadGenericJob<GenericJob>();
			InvoicingLineBase invocingLine = (InvoicingLineBase)invoicingBase.Lines.AddNew();
			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			NotificationBuffer notiBuffer = new NotificationBuffer();
			DataAdapter.PopulateValuesForXmlInvoiceLine(txnLine, invocingLine, notiBuffer);
		}

		public void TestSetXmlFinancialInvoiceHeaderValues()
		{
			PopulateInvoiceBizObj(typeof(ARInvoice), 100.00M, 10.00M, 0.50M, ObjectCreator.USD);
			OrgDebtorGroup debtorGroup = Factory.New<OrgDebtorGroup>();
			debtorGroup.OJ_Code = "AAA";

			OrgCreditorGroup creditorGroup = Factory.New<OrgCreditorGroup>();
			creditorGroup.OG_Code = "ZZZ";

			Invoice.Header.MiscServ.OM_OG_APCreditorGroup = creditorGroup.PK;
			Invoice.Header.MiscServ.OM_OJ_ARDebtorGroup = debtorGroup.PK;
			Invoice.Header.CompanyData.OB_APExternalCreditorCode = "PLANETEXPRESS";
			Invoice.Header.CompanyData.OB_ARExternalDebtorCode = "ACME";

			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Cash Basis Indicator", Invoice.Branch.Company.GC_IsGSTCashBasis.ToString(), xmlTransactionHeader.CashBasisTaxIndicator.ToString());
			AssertEquals("Disbursement Flag", Invoice.AH_IsDisbursementCalc, xmlTransactionHeader.DisbursementFlag);
			Assert("Disbursement Flag Specified", xmlTransactionHeader.DisbursementFlagSpecified);
			AssertEquals("Due Date", Invoice.AH_DueDate, xmlTransactionHeader.DueDate);
			AssertEquals("GL Account", ZString.Empty, xmlTransactionHeader.GlAccount);
			AssertEquals("Invoice Date", Invoice.AH_InvoiceDate, xmlTransactionHeader.InvoiceDate);
			AssertEquals("Invoice Terms", Invoice.AH_InvoiceTerm, xmlTransactionHeader.InvTerm);
			AssertEquals("Invoice Term Days", Invoice.AH_InvoiceTermDays.ToString(), xmlTransactionHeader.InvTermDays);
			Assert("Is Specified", xmlTransactionHeader.IsSpecified);
			AssertEquals("Job Invoice Number", Invoice.JobNumber, xmlTransactionHeader.JobInvoiceNo);
			AssertEquals("DebtorOrCreditor EDICode", Invoice.Header.OH_Code, xmlTransactionHeader.DebtorOrCreditor.EDICode);
			AssertEquals("DebtorOrCreditor AR", 1, xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.AccountsReceivables.Count);
			AssertEquals("DebtorOrCreditor AR Account Group", Factory.Load<OrgDebtorGroup>(Invoice.Header.MiscServ.OM_OJ_ARDebtorGroup).OJ_Code, xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.AccountsReceivables[0].AccountGroup);
			AssertEquals("DebtorOrCreditor AR Account External Debtor Code", "ACME", xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.AccountsReceivables[0].ExternalDebtorCode);
			AssertEquals("DebtorOrCreditor AP", 1, xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.AccountsPayables.Count);
			AssertEquals("DebtorOrCreditor AP Account Group", Factory.Load<OrgCreditorGroup>(Invoice.Header.MiscServ.OM_OG_APCreditorGroup).OG_Code, xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.AccountsPayables[0].AccountGroup);
			AssertEquals("DebtorOrCreditor AP Account External Creditor Code", "PLANETEXPRESS", xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.AccountsPayables[0].ExternalCreditorCode);
			AssertEquals("Header GUID", "headerGUID", xmlTransactionHeader.TxnHeaderGUID);

			Assert("DebtorOrCreditor: EDI Code should not be empty", !xmlTransactionHeader.DebtorOrCreditor.EDICode.IsEmpty);
			Assert("DebtorOrCreditor: EDI Code should be specified", xmlTransactionHeader.DebtorOrCreditor.IsSpecified);

			Assert("DebtorOrCreditor: Name should not be empty", !xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.Name.IsEmpty);
			Assert("DebtorOrCreditor: Name should be specified", xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.IsSpecified);

			Assert("DebtorOrCreditor: Address 1 should not be empty", !xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.Addresses[0].AddressLine1.IsEmpty);
			Assert("DebtorOrCreditor: Address 1 should be specified", xmlTransactionHeader.DebtorOrCreditor.OrganisationDetails.Addresses[0].IsSpecified);

			AssertEquals("Transaction overriden address", "112 Bourke Road", xmlTransactionHeader.TxnOverrideAddress.AddressLine1);

			AssertEquals("Transaction overriden contact name", "Bourke", xmlTransactionHeader.TxnOverrideContact.Name);
			AssertEquals("Transaction overriden contact email", "Bourke@123abc.com", xmlTransactionHeader.TxnOverrideContact.EmailAddress);
		}

		public void TestSetXmlFinancialInvoiceLineValuesOnJobRelatedInvoice()
		{
			Invoice = CreateFullyPopulatedInvoiceBizObj();

			GenericJob jobDetails = Invoice.Lines[0].Job.LoadGenericJob<GenericJob>();

			InvoicingLineBase line = Invoice.Lines[0];
			Xsd.TxnLine xmlInvoiceLine = new Xsd.TxnLine();

			DataAdapter.SetXmlLineGuidToDummyString();
			DataAdapter.PopulateValuesForXmlInvoiceLine(xmlInvoiceLine, line, new NotificationBuffer());

			AssertEquals("Branch Code", line.Branch.GB_Code, xmlInvoiceLine.Branch);
			AssertEquals("Charge Code", line.ChargeCode.AC_Code, xmlInvoiceLine.ChargeCode);
			AssertEquals("Charge Code Group", line.ChargeCode.AC_ChargeGroup, xmlInvoiceLine.ChargeGroup);
			AssertEquals("Charge Code Sub Group", line.ChargeCode.AC_ChargeSubGroup, xmlInvoiceLine.ChargeSubGroup);

			if (line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue)
			{
				AssertEquals("GL Account (Revenue)", line.ChargeCode.RevenueAccount.AG_AccountNum, xmlInvoiceLine.GLAccount);
			}
			else if (line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost)
			{
				AssertEquals("GL Account (Cost)", line.ChargeCode.CostAccount.AG_AccountNum, xmlInvoiceLine.GLAccount);
			}

			AssertEquals("Consol or Job Number", line.JobNumber, xmlInvoiceLine.ConsolOrJobNo);
			AssertEquals("Transaction Line Consol or Job Type", Xsd.TxnLineConsolOrJobType.SHP, xmlInvoiceLine.ConsolOrJobType);
			AssertEquals("Consol or Job Type Specified", true, xmlInvoiceLine.ConsolOrJobTypeSpecified);
			AssertEquals("Department Code", line.Department.GE_Code, xmlInvoiceLine.Department);
			AssertEquals("Description", line.AL_Desc, xmlInvoiceLine.Description);

			AssertNotNull("Generic Job: Job Details should not be null", jobDetails);

			if (jobDetails.InvoicingSupporter.Destination != null)
			{
				AssertNotNull("Destination Port Code should not be null", xmlInvoiceLine.DestinationPortCode);
				AssertEquals("Destination Port Code", jobDetails.InvoicingSupporter.Destination.RL_PortName, xmlInvoiceLine.DestinationPortCode.City);
			}

			AssertEquals("House Bill No", jobDetails.InvoicingSupporter.HouseBillNumber, xmlInvoiceLine.HouseBIllNo);

			var paymentTermInfo = jobDetails.InvoicingSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue);
			AssertEquals("Incoterm", paymentTermInfo != null && paymentTermInfo.InfoType == PaymentTermType.Incoterm ? paymentTermInfo.Value : string.Empty, xmlInvoiceLine.Incoterm);

			AssertEquals("Is Final Charge", false, xmlInvoiceLine.IsFinalCharge);
			AssertEquals("Is Final Charge Specified", false, xmlInvoiceLine.IsFinalChargeSpecified); // only used for import
			AssertEquals("Is Specificied", true, xmlInvoiceLine.IsSpecified);
			AssertEquals("Line Type", line.AL_LineType, xmlInvoiceLine.LineType.ToString());
			AssertEquals("Master Bill Number", jobDetails.InvoicingSupporter.MasterBillNumber, xmlInvoiceLine.MasterBillNo);

			if (jobDetails.InvoicingSupporter.Origin != null)
			{
				AssertNotNull("Origin Port Code should not be null", xmlInvoiceLine.OriginPortCode);
				AssertEquals("Origin Port Code", jobDetails.InvoicingSupporter.Origin.RL_PortName, xmlInvoiceLine.OriginPortCode.City);
			}

			AssertEquals("Sequence", line.AL_Sequence.ToString(), xmlInvoiceLine.Sequence);
			AssertEquals("Tax Code", line.TaxRate.AT_Code, xmlInvoiceLine.TaxCode);
			AssertEquals("WHT Code", "", xmlInvoiceLine.WHTCode);
			AssertEquals("Tax Message", line.VATClass.A9_Code, xmlInvoiceLine.TaxMsgCode);
			AssertEquals("Line GUID", "lineGUID", xmlInvoiceLine.TxnLineGUID);

			AssertFinancialValueEquals("LocalInvoiceAmtExclTax", xmlInvoiceLine.LocalInvoiceAmtExclTax, line.AL_LocalExTaxAmount * -1, line.Branch.Company.LocalCurrency, line.GetType());
			AssertFinancialValueEquals("LocalInvoiceAmtInclTax", xmlInvoiceLine.LocalInvoiceAmtInclTax, line.AL_LocalTotalAmount * -1, line.Branch.Company.LocalCurrency, line.GetType());
			AssertFinancialValueEquals("LocalTaxAmount", xmlInvoiceLine.LocalTaxAmount, line.AL_LocalTaxAmount * -1, line.Branch.Company.LocalCurrency, line.GetType());
			AssertFinancialValueEquals("LocalWHTAmount", xmlInvoiceLine.LocalWHTAmount, line.AL_LocalWHTAmount * -1, line.Branch.Company.LocalCurrency, line.GetType());

			AssertFinancialValueEquals("OsInvoiceAmtExclTax ", xmlInvoiceLine.OsInvoiceAmtExclTax, line.AL_OSExTaxAmount * -1, line.TransactionCurrency, line.GetType());
			AssertFinancialValueEquals("OsInvoiceAmtInclTax", xmlInvoiceLine.OsInvoiceAmtInclTax, line.AL_OverseasTotal * -1, line.TransactionCurrency, line.GetType());
			AssertFinancialValueEquals("OsTaxAmount", xmlInvoiceLine.OsTaxAmount, line.AL_OSTaxAmount * -1, line.TransactionCurrency, line.GetType());
			AssertFinancialValueEquals("OsWHTAmount", xmlInvoiceLine.OsWHTAmount, line.AL_OSWHTAmount * -1, line.TransactionCurrency, line.GetType());
		}

		public void TestExportOfARInvoice()
		{
			PopulateInvoiceBizObj(typeof(ARInvoice), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, xmlTransactionHeader);

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);

			AssertAmountsOnXmlInvoiceHeader(xmlTransactionHeader, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
		}

		[TestDate(2006, 01, 05)]
		public void TestExportOfARInvoiceWithJob()
		{
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_INCO = "FOB";
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_HouseBill = "UVWXYZ";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 100.000M;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 50.000M;
			shipment.JS_UnitOfVolume = "TN";
			shipment.JS_ActualChargeable = 80.000M;

			Job job = ObjectCreator.CreateJob(shipment);
			AccChargeCode chargecode = ChargeCodeCC1;
			chargecode.AC_ChargeType = Constants.ChargeType.Revenue;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			Invoice.AH_OH = Header.PK;

			Invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_GE = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaFcl.Value.GetDefaultDepartment();
			Invoice.AH_Desc = "This is a test description to see how the XML Export works";
			Invoice.AH_TransactionNum = "000010004";
			Invoice.AH_PostDate = PostDate;
			Invoice.AH_InvoiceDate = PostDate.AddDays(-1);
			Invoice.AH_DueDate = PostDate.AddDays(1);
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			InvoicingLineBase line1 = (InvoicingLineBase)Invoice.Lines.AddNew();

			line1.GenericCharge = chargecode.PK;
			line1.AL_JH = job.PK;
			line1.AL_GB = GlbBranch.CurrentBranch.PK;
			line1.AL_GE = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaFcl.Value.GetDefaultDepartment();
			line1.AL_Desc = "1st Line Description";
			line1.AL_OSExTaxAmount = 35.00M;
			line1.AL_AT = ObjectCreator.GST1.PK;
			line1.AL_OSTaxAmount = 3.50M;

			JobCharge charge = ObjectCreator.CreateJobCharge(line1, job, chargecode, ObjectCreator.AUD);
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 100m;
			charge.JR_OSSellExRate = 0.35m;
			charge.JR_LocalSellAmt = 35m;

			AssertEquals("ARInvoice should have no errors: " + Invoice.NotificationsIncludingChildren.ToUniqueMessageListString(), false, Invoice.HasErrors);
			Factory.Save();

			DataAdapter.SetXmlLineGuidToDummyString();
			ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
			XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(DataAdapter.ValueObjectType);
			using (StringWriter writer = new StringWriter())
			{
				serialiser.WriteToXml(writer, DataAdapter, Invoice, context);
				AssertASCIIFileSameAsString(Retriever.SaveResourceToFile("ARInvoiceWithJob.xml"), writer.ToString());
			}
		}

		public void TestExportOfARCreditNote()
		{
			PopulateInvoiceBizObj(typeof(ARCreditNote), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, xmlTransactionHeader);

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);

			AssertAmountsOnXmlInvoiceHeader(xmlTransactionHeader, -600M, -660M, -60M, 0M, -300M, -330M, -30M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[0], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[1], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
		}

		public void TestExportOfARPositiveAdjustmentNote()
		{
			PopulateInvoiceBizObj(typeof(ARAdjustmentNote), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, xmlTransactionHeader);

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);

			AssertAmountsOnXmlInvoiceHeader(xmlTransactionHeader, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
		}

		public void TestExportOfARNegativeAdjustmentNote()
		{
			PopulateInvoiceBizObj(typeof(ARAdjustmentNote), -300.0M, -30.0M, 0.5M, ObjectCreator.USD);
			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, xmlTransactionHeader);

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, -600M, -660M, -60M, 0M, -300M, -330M, -30M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);

			AssertAmountsOnXmlInvoiceHeader(xmlTransactionHeader, -600M, -660M, -60M, 0M, -300M, -330M, -30M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[0], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[1], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
		}

		public void TestExportOfAPInvoice()
		{
			PopulateInvoiceBizObj(typeof(APInvoice), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, xmlTransactionHeader);

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);

			AssertAmountsOnXmlInvoiceHeader(xmlTransactionHeader, -600M, -660M, -60M, 0M, -300M, -330M, -30M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[0], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[1], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
		}

		public void TestExportOfAPCreditNote()
		{
			PopulateInvoiceBizObj(typeof(APCreditNote), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, xmlTransactionHeader);

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);

			AssertAmountsOnXmlInvoiceHeader(xmlTransactionHeader, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
		}

		public void TestExportOfPositiveAPAdjustmentNote()
		{
			PopulateInvoiceBizObj(typeof(APAdjustmentNote), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, xmlTransactionHeader);
			AssertAmountsOnInvoiceHeaderBizObj(Invoice, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);

			AssertAmountsOnXmlInvoiceHeader(xmlTransactionHeader, -600M, -660M, -60M, 0M, -300M, -330M, -30M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[0], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[1], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
		}

		public void TestExportOfNegativeAPAdjustmentNote()
		{
			PopulateInvoiceBizObj(typeof(APAdjustmentNote), -300.0M, -30.0M, 0.5M, ObjectCreator.USD);
			Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, xmlTransactionHeader);

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, -600M, -660M, -60M, 0M, -300M, -330M, -30M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);
			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], -300M, -330M, -30M, 0M, -150M, -165.0M, -15.0M, 0M);

			AssertAmountsOnXmlInvoiceHeader(xmlTransactionHeader, 600M, 660M, 60M, 0M, 300M, 330M, 30M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[0], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
			AssertAmountsOnXmlInvoiceLine(xmlTransactionHeader.TxnLines[1], 300M, 330M, 30M, 0M, 150M, 165.0M, 15.0M, 0M);
		}

		public void TestPaymentDetailsWithNoPaymentsForAnInvoice()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			Invoice.AH_InvoiceAmount = -100M;
			Invoice.AH_GSTAmount = -10M;
			Invoice.AH_OutstandingAmount = 0M;

			XsdInvoice = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("TxnHeader.BankCode", ZString.Empty, XsdInvoice.BankCode);
			AssertEquals("TxnHeader.ReceiptPaymentTypeSpecified", false, XsdInvoice.ReceiptPaymentTypeSpecified);
			AssertEquals("TxnHeader.ChequeOrReference", ZString.Empty, XsdInvoice.ChequeOrReference);

			// Receipt Fields shouldn't be populated for an AP Transaction
			AssertEquals("TxnHeader.ChequeDrawer", ZString.Empty, XsdInvoice.ChequeDrawer);
			AssertEquals("TxnHeader.DrawerBank", ZString.Empty, XsdInvoice.DrawerBank);
			AssertEquals("TxnHeader.DrawerBankBranch", ZString.Empty, XsdInvoice.DrawerBankBranch);
		}

		public void TestPaymentDetailsWithMultiplePaymentsForAnInvoice()
		{
			Invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "0001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
			Invoice.AH_OutstandingAmount = 0M;
			Invoice.AH_FullyPaidDate = ZDateTime.Now;

			AccTransactionHeader payment1 = CreateAPaymentForInvoice(Invoice, typeof(APPayment));
			payment1.AH_OSTotal = 55m;
			payment1.AH_InvoiceAmount = 100M;
			payment1.AH_GSTAmount = -45M;
			payment1.AH_FullyPaidDate = ZDateTime.Now;
			payment1.AH_ChequeOrReference = "Payment1";
			CreateAMatchLink(Invoice, "M00001001", -55M);
			CreateAMatchLink(((IMatching)Invoice).CurrentMatchGroup, payment1, "M00001001", 55M);

			AccTransactionHeader payment2 = CreateAPaymentForInvoice(Invoice, typeof(APPayment));
			payment2.AH_OSTotal = 55m;
			payment2.AH_InvoiceAmount = 200M;
			payment2.AH_GSTAmount = -145M;
			payment2.AH_FullyPaidDate = ZDateTime.Now;
			payment2.AH_ChequeOrReference = "Payment2";
			CreateAMatchLink(Invoice, "M00001002", -55M);
			CreateAMatchLink(((IMatching)Invoice).CurrentMatchGroup, payment2, "M00001002", 55M);

			Factory.Save();

			NotificationBuffer notify = new NotificationBuffer();
			XsdInvoice = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(notify));
			Xsd.TxnHeaderReceiptPaymentType receiptPaymentType = TxnHeaderMapper.GetTxnHeaderReceiptPaymentType(payment1.AH_ReceiptType, notify);

			AssertEquals("TxnHeader.BankCode", ZString.Empty, XsdInvoice.BankCode);
			AssertEquals("TxnHeader.ReceiptPaymentTypeSpecified", false, XsdInvoice.ReceiptPaymentTypeSpecified);
			AssertEquals("TxnHeader.ChequeOrReference", ZString.Empty, XsdInvoice.ChequeOrReference);

			// Receipt Fields shouldn't be populated for an AP Transaction
			AssertEquals("TxnHeader.ChequeDrawer", ZString.Empty, XsdInvoice.ChequeDrawer);
			AssertEquals("TxnHeader.DrawerBank", ZString.Empty, XsdInvoice.DrawerBank);
			AssertEquals("TxnHeader.DrawerBankBranch", ZString.Empty, XsdInvoice.DrawerBankBranch);
		}

		public void TestPaymentDetailsWithSingleCashPaymentForAnInvoice()
		{
			ZGuid group = ZGuid.NewZGuid();

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(Invoice, Invoice.TransactionCurrency, Invoice.AH_ExchangeRate, 100m, 10m, 0m, 100m, 10m, 0m);
			Invoice.AH_OutstandingAmount = 0M;
			Invoice.AH_TransactionBelongsToGroup = group;
			Invoice.AH_TransactionCount = 1;

			AccTransactionHeader payment = CreateAPaymentForInvoice(Invoice, typeof(APPayment));
			CreateAMatchLink(Invoice, "M00001234", -110M);
			CreateAMatchLink(((IMatching)Invoice).CurrentMatchGroup, payment, "M00001234", 110M);

			InvoicingBase creditNote = (InvoicingBase)Factory.New(typeof(APCreditNote));
			TestObjectCreator.CreateInvoiceLine(creditNote, creditNote.TransactionCurrency, creditNote.AH_ExchangeRate, 100m, 10m, 0m, 100m, 10m, 0m);
			creditNote.AH_OutstandingAmount = 0M;

			AccTransactionHeader paymentReversal = CreateAPaymentForInvoice(creditNote, typeof(APPayment));
			CreateAMatchLink(creditNote, "M00001234", 110M);
			CreateAMatchLink(paymentReversal, "M00001234", -110M);

			Invoice.AH_TransactionBelongsToGroup = group;
			Invoice.AH_TransactionCount = 1;
			payment.AH_TransactionBelongsToGroup = group;
			payment.AH_TransactionCount = 2;
			creditNote.AH_TransactionBelongsToGroup = group;
			creditNote.AH_TransactionCount = 3;
			paymentReversal.AH_TransactionBelongsToGroup = group;
			paymentReversal.AH_TransactionCount = 4;

			XsdInvoice = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.TxnHeaderReceiptPaymentType receiptPaymentType = TxnHeaderMapper.GetTxnHeaderReceiptPaymentType(payment.AH_ReceiptType, new NotificationBuffer());

			AssertEquals("TxnHeader.BankCode", payment.BankAccount.AB_Code, XsdInvoice.BankCode);
			AssertEquals("TxnHeader.ReceiptPaymentType", receiptPaymentType.ToString(), XsdInvoice.ReceiptPaymentType.ToString());
			AssertEquals("TxnHeader.ReceiptPaymentTypeSpecified", true, XsdInvoice.ReceiptPaymentTypeSpecified);
			AssertEquals("TxnHeader.ChequeOrReference", payment.AH_ChequeOrReference, XsdInvoice.ChequeOrReference);

			// Receipt Fields shouldn't be populated for an AP Transaction
			AssertEquals("TxnHeader.ChequeDrawer", ZString.Empty, XsdInvoice.ChequeDrawer);
			AssertEquals("TxnHeader.DrawerBank", ZString.Empty, XsdInvoice.DrawerBank);
			AssertEquals("TxnHeader.DrawerBankBranch", ZString.Empty, XsdInvoice.DrawerBankBranch);
		}

		public void TestPaymentDetailsWithSingleCashPaymentReversalForAnInvoice()
		{
			ZGuid group = ZGuid.NewZGuid();

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			TestObjectCreator.CreateInvoiceLine(Invoice, Invoice.TransactionCurrency, Invoice.AH_ExchangeRate, 100m, 10m, 0m, 100m, 10m, 0m);
			Invoice.AH_OutstandingAmount = 0M;
			Invoice.AH_TransactionBelongsToGroup = group;
			Invoice.AH_TransactionCount = 1;

			AccTransactionHeader payment = CreateAPaymentForInvoice(Invoice, typeof(APPayment));
			CreateAMatchLink(Invoice, "M00001234", -110M);
			CreateAMatchLink(payment, "M00001234", 110M);

			InvoicingBase creditNote = (InvoicingBase)Factory.New(typeof(APCreditNote));
			TestObjectCreator.CreateInvoiceLine(creditNote, creditNote.TransactionCurrency, creditNote.AH_ExchangeRate, 100m, 10m, 0m, 100m, 10m, 0m);
			creditNote.AH_OutstandingAmount = 0M;

			AccTransactionHeader paymentReversal = CreateAPaymentForInvoice(creditNote, typeof(APPayment));
			CreateAMatchLink(creditNote, "M00001234", 110M);
			CreateAMatchLink(paymentReversal, "M00001234", -110M);

			Invoice.AH_TransactionBelongsToGroup = group;
			Invoice.AH_TransactionCount = 1;
			payment.AH_TransactionBelongsToGroup = group;
			payment.AH_TransactionCount = 2;
			creditNote.AH_TransactionBelongsToGroup = group;
			creditNote.AH_TransactionCount = 3;
			paymentReversal.AH_TransactionBelongsToGroup = group;
			paymentReversal.AH_TransactionCount = 4;

			XsdInvoice = DataAdapter.ExportToValueObject(creditNote, new ValueObjectExportContext(new NotificationBuffer()));
			Xsd.TxnHeaderReceiptPaymentType receiptPaymentType = TxnHeaderMapper.GetTxnHeaderReceiptPaymentType(payment.AH_ReceiptType, new NotificationBuffer());

			AssertEquals("TxnHeader.BankCode", payment.BankAccount.AB_Code, XsdInvoice.BankCode);
			AssertEquals("TxnHeader.ReceiptPaymentType", receiptPaymentType.ToString(), XsdInvoice.ReceiptPaymentType.ToString());
			AssertEquals("TxnHeader.ReceiptPaymentTypeSpecified", true, XsdInvoice.ReceiptPaymentTypeSpecified);
			AssertEquals("TxnHeader.ChequeOrReference", payment.AH_ChequeOrReference, XsdInvoice.ChequeOrReference);

			// Receipt Fields shouldn't be populated for an AP Transaction
			AssertEquals("TxnHeader.ChequeDrawer", ZString.Empty, XsdInvoice.ChequeDrawer);
			AssertEquals("TxnHeader.DrawerBank", ZString.Empty, XsdInvoice.DrawerBank);
			AssertEquals("TxnHeader.DrawerBankBranch", ZString.Empty, XsdInvoice.DrawerBankBranch);
		}

		public void TestReceiptDetailsWithNoReceiptForAnInvoice()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			XsdInvoice = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("TxnHeader.BankCode", ZString.Empty, XsdInvoice.BankCode);
			AssertEquals("TxnHeader.ReceiptPaymentTypeSpecified", false, XsdInvoice.ReceiptPaymentTypeSpecified);
			AssertEquals("TxnHeader.ChequeOrReference", ZString.Empty, XsdInvoice.ChequeOrReference);
			AssertEquals("TxnHeader.ChequeDrawer", ZString.Empty, XsdInvoice.ChequeDrawer);
			AssertEquals("TxnHeader.DrawerBank", ZString.Empty, XsdInvoice.DrawerBank);
			AssertEquals("TxnHeader.DrawerBankBranch", ZString.Empty, XsdInvoice.DrawerBankBranch);
		}

		public void TestReceiptDetailsWithSingleReceiptForAnInvoice()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			Invoice.AH_InvoiceAmount = 100M;
			Invoice.AH_GSTAmount = 10M;

			Receipt receipt = CreateAReceiptForInvoice(Invoice, typeof(ARReceipt));
			CreateAMatchLink(Invoice, "M00001234", 110M);
			CreateAMatchLink(receipt, "M00001234", -110M);

			NotificationBuffer notify = new NotificationBuffer();
			XsdInvoice = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(notify));
			Xsd.TxnHeaderReceiptPaymentType receiptPaymentType = TxnHeaderMapper.GetTxnHeaderReceiptPaymentType(receipt.AH_ReceiptType, notify);

			AssertEquals("TxnHeader.BankCode", receipt.BankAccount.AB_Code, XsdInvoice.BankCode);
			AssertEquals("TxnHeader.ReceiptPaymentType", receiptPaymentType.ToString(), XsdInvoice.ReceiptPaymentType.ToString());
			AssertEquals("TxnHeader.ReceiptPaymentTypeSpecified", true, XsdInvoice.ReceiptPaymentTypeSpecified);
			AssertEquals("TxnHeader.ChequeOrReference", receipt.AH_ChequeOrReference, XsdInvoice.ChequeOrReference);
			AssertEquals("TxnHeader.ChequeDrawer", receipt.AH_ChequeDrawer, XsdInvoice.ChequeDrawer);
			AssertEquals("TxnHeader.DrawerBank", receipt.AH_DrawerBank, XsdInvoice.DrawerBank);
			AssertEquals("TxnHeader.DrawerBankBranch", receipt.AH_DrawerBranch, XsdInvoice.DrawerBankBranch);
		}

		public void TestReceiptDetailsWithMultipleReceiptsForAnInvoice()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			Invoice.AH_InvoiceAmount = 100M;
			Invoice.AH_GSTAmount = 10M;

			Receipt receipt1 = CreateAReceiptForInvoice(Invoice, typeof(ARReceipt));
			CreateAMatchLink(Invoice, "M00001234", 55M);
			CreateAMatchLink(receipt1, "M00001234", -55M);

			Receipt receipt2 = CreateAReceiptForInvoice(Invoice, typeof(ARReceipt));
			CreateAMatchLink(Invoice, "M00001235", 55M);
			CreateAMatchLink(receipt2, "M00001235", -55M);

			XsdInvoice = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("TxnHeader.BankCode", ZString.Empty, XsdInvoice.BankCode);
			AssertEquals("TxnHeader.ReceiptPaymentTypeSpecified", false, XsdInvoice.ReceiptPaymentTypeSpecified);
			AssertEquals("TxnHeader.ChequeOrReference", ZString.Empty, XsdInvoice.ChequeOrReference);
			AssertEquals("TxnHeader.ChequeDrawer", ZString.Empty, XsdInvoice.ChequeDrawer);
			AssertEquals("TxnHeader.DrawerBank", ZString.Empty, XsdInvoice.DrawerBank);
			AssertEquals("TxnHeader.DrawerBankBranch", ZString.Empty, XsdInvoice.DrawerBankBranch);
		}

		public void TestGetMatchLinks()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			Invoice.AH_InvoiceAmount = 100M;
			Invoice.AH_GSTAmount = 10M;

			Receipt receipt1 = CreateAReceiptForInvoice(Invoice, typeof(ARReceipt));

			AccTransactionMatchLink link1 = CreateAMatchLink(Invoice, "M00001234", 55M);
			AccTransactionMatchLink link2 = CreateAMatchLink(receipt1, "M00001234", -55M);

			TransactionMatchLinkCollection transMatchLinksRelatedtoInvoice = new TransactionMatchLinkCollection(Invoice.Factory);
			transMatchLinksRelatedtoInvoice.Add(link1);

			TransactionMatchLinkCollection matchLinks = DataAdapter.GetMatchLinks(transMatchLinksRelatedtoInvoice, Invoice.Factory);

			AssertEquals("No of links found", 2, matchLinks.Count);

			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			Invoice.AH_InvoiceAmount = 100M;
			Invoice.AH_GSTAmount = 10M;

			transMatchLinksRelatedtoInvoice = new TransactionMatchLinkCollection(Invoice.Factory);
			matchLinks = DataAdapter.GetMatchLinks(transMatchLinksRelatedtoInvoice, Invoice.Factory);

			AssertEquals("No link is found", 0, matchLinks.Count);
		}

		public void TestExportOwnerOrderReferences()
		{
			#region via Shipment

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			InvoicingBase shipmentRelatedInvoice = Factory.NewWithValidTestData<ARInvoice>();
			BaseJobDeclaration declaration = null;
			Xsd.TxnHeader xmlTransactionHeader = null;

			using (Job job = ObjectCreator.CreateJob(shipment))
			{
				shipmentRelatedInvoice.AH_JH = job.PK;
				shipmentRelatedInvoice.AH_OH = ObjectCreator.AALSHI.PK;

				var invoiceLine = (InvoiceLine)shipmentRelatedInvoice.Lines.AddNew();
				invoiceLine.AL_JH = job.PK;
				invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				TestObjectCreator.CreateJobCharge(invoiceLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				OrderItem item = NewOrderItem(shipment.DocsAndCartage, "orderRef from shipment");
				Factory.Save();

				xmlTransactionHeader = DataAdapter.ExportToValueObject(shipmentRelatedInvoice, new ValueObjectExportContext(new NotificationBuffer()));

				AssertNotNull("XmlTransactionHeader", xmlTransactionHeader);
				AssertEquals("OrderReference", "orderRef from shipment", xmlTransactionHeader.OrderReference);
				Assert("OwnerReference", xmlTransactionHeader.OwnerReference.IsEmpty);

				AssertEquals("XmlTransactionHeader.TxnLines.Count", 1, xmlTransactionHeader.TxnLines.Count);
				AssertTransactionLine(xmlTransactionHeader.TxnLines[0], "orderRef from shipment", ZString.Empty);

				declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OwnerRef = "ownerRef from dec";
				Factory.Save();

				xmlTransactionHeader = DataAdapter.ExportToValueObject(shipmentRelatedInvoice, new ValueObjectExportContext(new NotificationBuffer()));

				AssertNotNull("XmlTransactionHeader", xmlTransactionHeader);
				AssertEquals("OrderReference", "orderRef from shipment", xmlTransactionHeader.OrderReference);
				AssertEquals("OwnerReference", "ownerRef from dec", xmlTransactionHeader.OwnerReference);

				AssertEquals("XmlTransactionHeader.TxnLines.Count", 1, xmlTransactionHeader.TxnLines.Count);
				AssertTransactionLine(xmlTransactionHeader.TxnLines[0], "orderRef from shipment", "ownerRef from dec");
			}

			#endregion

			#region via B-job Declaration

			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			InvoicingBase declartionRelatedInvoice = Factory.NewWithValidTestData<ARInvoice>();

			using (Job job = ObjectCreator.CreateJob(declaration))
			{
				declartionRelatedInvoice.AH_JH = job.PK;
				declartionRelatedInvoice.AH_OH = ObjectCreator.AALSHI.PK;

				var invoiceLine = (InvoiceLine)declartionRelatedInvoice.Lines.AddNew();
				invoiceLine.AL_JH = job.PK;

				OrderItem item = NewOrderItem(declaration.DocsAndCartage, "orderRef from declaration");
				declaration.JE_OwnerRef = "owner ref";
				AssertNull("declaration.Shipment", declaration.Shipment);
				xmlTransactionHeader = DataAdapter.ExportToValueObject(declartionRelatedInvoice, new ValueObjectExportContext(new NotificationBuffer()));

				AssertNotNull("XmlTransactionHeader", xmlTransactionHeader);
				AssertEquals("OrderReference", "orderRef from declaration", xmlTransactionHeader.OrderReference);
				AssertEquals("OwnerReference", "owner ref", xmlTransactionHeader.OwnerReference);

				AssertEquals("XmlTransactionHeader.TxnLines.Count", 1, xmlTransactionHeader.TxnLines.Count);
				AssertTransactionLine(xmlTransactionHeader.TxnLines[0], "orderRef from declaration", "owner ref");
			}

			#endregion

			#region via S-job Declaration

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			NewOrderItem(shipment.DocsAndCartage, "orderRef from shipment");
			declaration.JE_JS = shipment.PK;

			xmlTransactionHeader = DataAdapter.ExportToValueObject(declartionRelatedInvoice, new ValueObjectExportContext(new NotificationBuffer()));

			AssertNotNull("XmlTransactionHeader", xmlTransactionHeader);
			AssertEquals("OrderReference", "orderRef from shipment", xmlTransactionHeader.OrderReference);
			AssertEquals("OwnerReference", "owner ref", xmlTransactionHeader.OwnerReference);

			AssertEquals("XmlTransactionHeader.TxnLines.Count", 1, xmlTransactionHeader.TxnLines.Count);
			AssertTransactionLine(xmlTransactionHeader.TxnLines[0], "orderRef from shipment", "owner ref");

			#endregion
		}

		void AssertTransactionLine(Xsd.TxnLine line, ZString orderReference, ZString ownerReference)
		{
			if (orderReference.IsEmpty)
			{
				Assert("OrderReferenceSpecified", !line.OrderReferenceSpecified);
			}
			else
			{
				Assert("OrderReferenceSpecified", line.OrderReferenceSpecified);
				AssertEquals("OrderReference", orderReference, line.OrderReference);
			}

			if (ownerReference.IsEmpty)
			{
				Assert("OwnerReferenceSpecified", !line.OwnerReferenceSpecified);
			}
			else
			{
				Assert("OrderReferenceSpecified", line.OwnerReferenceSpecified);
				AssertEquals("OwnerReference", ownerReference, line.OwnerReference);
			}
		}

		public void TextExportTxnNumber_ARInvoice()
		{
			string oldInvoiceTransactionNumberPrefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC");

				PopulateInvoiceBizObj(typeof(ARInvoice), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
				Invoice.AH_ConsolidatedInvoiceRef = ZString.Empty;
				Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));

				AssertEquals("Transaction Number", Invoice.TransactionNumberPrefixed, xmlTransactionHeader.TxnNumber);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldInvoiceTransactionNumberPrefix);
			}
		}

		public void TextExportTxnNumber_APInvoice()
		{
			string oldSelfBillingInvoiceTransactionNumberPrefix = AccountingConfigurationRegistry.Instance.SelfBillingInvoiceTransactionNumberPrefix.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.SelfBillingInvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC");

				PopulateInvoiceBizObj(typeof(APInvoice), 300.0M, 30.0M, 0.5M, ObjectCreator.USD);
				Invoice.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.SelfBilling;
				Xsd.TxnHeader xmlTransactionHeader = DataAdapter.ExportToValueObject(Invoice, new ValueObjectExportContext(new NotificationBuffer()));

				AssertEquals("Transaction Number", Invoice.TransactionNumberPrefixed, xmlTransactionHeader.TxnNumber);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.SelfBillingInvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldSelfBillingInvoiceTransactionNumberPrefix);
			}
		}

		public void TestSettlementGroupExport()
		{
			var arParty = Factory.New<OrgRelatedParty>();
			var apParty = Factory.New<OrgRelatedParty>();
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			arParty.PR_OH_Parent =
				apParty.PR_OH_Parent = invoice.Header.PK;

			arParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			arParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.AR;
			arParty.PR_GC = GlbCompany.CurrentCompany.PK;

			apParty.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			apParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.AP;
			apParty.PR_GC = GlbCompany.CurrentCompany.PK;

			var arOrg = Factory.NewWithValidTestData<OrgHeader>();
			arOrg.OH_FullName = "AR Settlement Group";
			arParty.PR_OH_RelatedParty = arOrg.PK;

			var apOrg = Factory.NewWithValidTestData<OrgHeader>();
			apOrg.OH_FullName = "AP Settlement Group";
			apParty.PR_OH_RelatedParty = apOrg.PK;

			Factory.Save();

			Xsd.TxnHeader transaction = DataAdapter.ExportToValueObject(invoice, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("DebtorOrCreditor AccountsReceivables", 1, transaction.DebtorOrCreditor.OrganisationDetails.AccountsReceivables.Count);
			AssertEquals("DebtorOrCreditor AccountsPayables", 1, transaction.DebtorOrCreditor.OrganisationDetails.AccountsPayables.Count);

			Xsd.SettlementDetails arDetails =
				transaction.DebtorOrCreditor.OrganisationDetails.AccountsReceivables[0].SettlementDetails;

			Xsd.SettlementDetails apDetails =
				transaction.DebtorOrCreditor.OrganisationDetails.AccountsPayables[0].SettlementDetails;

			AssertNotNull("arDetails", arDetails);
			AssertNotNull("apDetails", apDetails);

			AssertEquals(arOrg.OH_FullName, arDetails.SettlementGroup.Name);
			AssertEquals(apOrg.OH_FullName, apDetails.SettlementGroup.Name);

			AssertEquals(arOrg.OH_Code, arDetails.SettlementGroup.EDICode);
			AssertEquals(arOrg.OH_Code, arDetails.SettlementGroup.OwnerCode);

			AssertEquals(apOrg.OH_Code, apDetails.SettlementGroup.EDICode);
			AssertEquals(apOrg.OH_Code, apDetails.SettlementGroup.OwnerCode);
		}

		protected override string TransformVolatilePartsBeforeComparing(string originalOutput)
		{
			return Regex.Replace(originalOutput, "<Data>.*</Data>", string.Empty);
		}

		OrderItem NewOrderItem(JobDocsAndCartage docsAndCartage, ZString orderReference)
		{
			OrderItem item = docsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = orderReference.Left(25);
			return item;
		}

		Payment CreateAPaymentForInvoice(InvoicingBase invoice, Type typeToCreate)
		{
			Payment payment = (Payment)Factory.NewWithValidTestData(typeToCreate);
			payment.AH_OH = invoice.AH_OH;
			payment.AH_AB = ObjectCreator.AUDBankAccount.PK;
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment.AH_ChequeOrReference = "0123456789";

			return payment;
		}

		Receipt CreateAReceiptForInvoice(InvoicingBase invoice, Type typeToCreate)
		{
			Receipt receipt = (Receipt)Factory.NewWithValidTestData(typeToCreate);
			receipt.AH_OH = invoice.AH_OH;
			receipt.AH_AB = ObjectCreator.AUDBankAccount.PK;
			receipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			receipt.AH_ChequeOrReference = "0123456789";
			receipt.AH_ChequeDrawer = "Bob Jones";
			receipt.AH_DrawerBank = "ANZ Bank";
			receipt.AH_DrawerBranch = "Sydney Branch";

			return receipt;
		}

		AccTransactionMatchLink CreateAMatchLink(TransactionMatchLinkGroup group, AccTransactionHeader header, ZString matchGroupNumber, ZDecimal matchAmount)
		{
			AccTransactionMatchLink invoiceMatchLink = group.AddNew();
			invoiceMatchLink.AP_AH = header.PK;
			invoiceMatchLink.AP_MatchGroupNum = matchGroupNumber;
			invoiceMatchLink.AP_Amount = matchAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(invoiceMatchLink);

			return invoiceMatchLink;
		}

		AccTransactionMatchLink CreateAMatchLink(AccTransactionHeader header, ZString matchGroupNumber, ZDecimal matchAmount)
		{
			return CreateAMatchLink(((IMatching)header).CurrentMatchGroup, header, matchGroupNumber, matchAmount);
		}

		#endregion

		#region Importing

		[TestDate(2006, 01, 05)]
		public void TestImportRecoverableGSTVATPercentage()
		{
			Env.Security.AllowAPInvoiceLineVATRecoverableOverride.IsAllowed = true;

			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var txnHeader = GetFullyPopulatedXmlInvoice_AP();
			var txnLine = txnHeader.TxnLines[0];
			var invoice = DataAdapter.NewBusinessObject(txnHeader, context);
			ChargeCodeCC1.AC_ChargeType = Constants.ChargeType.Overhead;
			txnLine.RecoverableGSTVATPercentage = 70.32;
			Assert("Precondition: Invoice must support Recoverable VAT", invoice is APInvoice);

			DataAdapter.ImportFromValueObject(invoice, txnHeader, context);

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, "Error");
			AssertEquals("AL_InputGSTVATRecoverable", 0.7032m, invoice.Lines[0].AL_InputGSTVATRecoverable);
		}

		[TestDate(2006, 01, 05)]
		public void TestImportRecoverableGSTVATPercentageWhenNotSpecified()
		{
			Env.Security.AllowAPInvoiceLineVATRecoverableOverride.IsAllowed = true;

			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var txnHeader = GetFullyPopulatedXmlInvoice_AP();
			var txnLine = txnHeader.TxnLines[0];
			var invoice = DataAdapter.NewBusinessObject(txnHeader, context);
			Assert("Precondition: Invoice must support Recoverable VAT", invoice is APInvoice);

			DataAdapter.ImportFromValueObject(invoice, txnHeader, context);

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, "Error");
			AssertEquals("AL_InputGSTVATRecoverable should have default value if RecoverableGSTVATPercentage is not specified.", 1m, invoice.Lines[0].AL_InputGSTVATRecoverable);
		}

		public void TestImportRecoverableGSTVATPercentage_InvalidValue()
		{
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var txnHeader = GetFullyPopulatedXmlInvoice_AP();
			var txnLine = txnHeader.TxnLines[0];
			txnLine.RecoverableGSTVATPercentage = 120;

			var invoice = DataAdapter.NewBusinessObject(txnHeader, context);
			Assert("Precondition: Invoice must support Recoverable VAT", invoice is APInvoice);

			DataAdapter.ImportFromValueObject(invoice, txnHeader, context);

			ZString expectedMessage = "Error: Tax Recoverable Percentage: GST Recoverable % must be between 0 and 100.";

			TestHelper.AssertNotificationsContainsErrorMessage(notify, expectedMessage);
		}

		[TestDate(2006, 01, 05)]
		public void TestImportRecoverableGSTVATPercentageForNotSupportedBizo()
		{
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var txnHeader = GetFullyPopulatedXmlInvoice_AR();
			var txnLine = txnHeader.TxnLines[0];
			txnLine.RecoverableGSTVATPercentage = 70;

			var invoice = DataAdapter.NewBusinessObject(txnHeader, context);
			Assert("Precondition: Invoice should not support Recoverable VAT", invoice is ARInvoice);

			DataAdapter.ImportFromValueObject(invoice, txnHeader, context);

			TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, "Error");
			AssertEquals("AL_InputGSTVATRecoverable should not be imported for AR Invoice", 1m, invoice.Lines[0].AL_InputGSTVATRecoverable);
		}

		public void TestMinimumRequirementsMetForNewImport()
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			XsdInvoice = GetFullyPopulatedXmlInvoice_AP();
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);

			XsdInvoice.TxnType = Xsd.TxnType.INV;
			AssertEquals(true, DataAdapter.MinimumRequirementsMetForNewImport(XsdInvoice, context));

			XsdInvoice.TxnType = Xsd.TxnType.ADJ;
			AssertEquals(true, DataAdapter.MinimumRequirementsMetForNewImport(XsdInvoice, context));

			XsdInvoice.TxnType = Xsd.TxnType.CRD;
			AssertEquals(true, DataAdapter.MinimumRequirementsMetForNewImport(XsdInvoice, context));

			XsdInvoice.TxnType = Xsd.TxnType.JNL;
			AssertEquals(false, DataAdapter.MinimumRequirementsMetForNewImport(XsdInvoice, context));
		}

		public void TestImportTransactionWithDuplicatedTransactionNumber()
		{
			ImportTransactionWithDuplicatedTransactionNumberTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.INV, true);
			ImportTransactionWithDuplicatedTransactionNumberTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.CRD, true);
			ImportTransactionWithDuplicatedTransactionNumberTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.ADJ, true);

			ImportTransactionWithDuplicatedTransactionNumberTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.INV, false);
			ImportTransactionWithDuplicatedTransactionNumberTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.CRD, false);
			ImportTransactionWithDuplicatedTransactionNumberTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.ADJ, false);
		}

		void ImportTransactionWithDuplicatedTransactionNumberTest(Xsd.TxnLedgerType ledger, Xsd.TxnType transactionType, bool shouldHaveError)
		{
			if (XsdInvoice == null)
			{
				XsdInvoice = GetFullyPopulatedXmlInvoice_AP();
			}

			XsdInvoice.Ledger = ledger;
			XsdInvoice.TxnType = transactionType;
			XsdInvoice.OsInvoiceAmtInclTax.CurrencyCode = ObjectCreator.USD.RX_Code;
			SetOrganisation(XsdInvoice);

			XsdInvoice.TxnLines[0].WHTCode = ZString.Empty;
			XsdInvoice.TxnLines[1].WHTCode = ZString.Empty;

			if (XsdInvoice.Ledger == Xsd.TxnLedgerType.AR)
			{
				XsdInvoice.TxnNumber = ZString.Empty;
				if (transactionType == Xsd.TxnType.INV)
				{
					XsdInvoice.TxnCategory = "DBT";
				}
			}

			if (transactionType != Xsd.TxnType.INV && transactionType != Xsd.TxnType.CRD)
			{
				XsdInvoice.TxnLines[0].ConsolOrJobNo = XsdInvoice.TxnLines[1].ConsolOrJobNo = ZString.Empty;
			}

			TransactionHeader transaction = Factory.NewWithValidTestData<APInvoice>();
			transaction.AH_TransactionType = transactionType.ToString();
			transaction.AH_Ledger = ledger.ToString();
			transaction.AH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, XsdInvoice.DebtorOrCreditor.EDICode)).PK;
			transaction.AH_TransactionNum = "00001000";
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;

			NotificationBuffer notify = new NotificationBuffer();

			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, new ValueObjectExportContext(notify));
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);

			string errorMessage = "The transaction number '00001000' is already in use by another transaction in this file.";
			if (shouldHaveError)
			{
				TestHelper.AssertNotificationsContainsErrorMessage(notify, errorMessage);
			}
			else
			{
				TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, errorMessage);
			}
		}

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			ValueObjectDataAdapter<InvoicingBase, Xsd.TxnHeader> adapter = GetNewBizObjXmlDataAdapter();
			Xsd.TxnHeader exportedValueObject;
			if (sample.ConstructedValueObject != null)
			{
				exportedValueObject = sample.ConstructedValueObject;
			}
			else
			{
				exportedValueObject = (Xsd.TxnHeader)Activator.CreateInstance(adapter.ValueObjectType);
			}

			adapter.ExportToValueObject(sample.BizObj, exportedValueObject, new ValueObjectExportContext(new NotificationBuffer()));
			string exportedValueObjectXml = WriteBusinessObjectToXml(sample.BizObj, sample.ConstructedValueObject, sample.Description, (sample.ValidationKind & ValidationKind.Xsd) != 0);

			InvoicingBase bizObjToImportTo = NewBusinessObjectFromIValueObject(exportedValueObject);

			var createLog = bizObjToImportTo.GetLogs().AddNew();
			createLog.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;

			AssertImportFromThenExportToProducesSameXml(sample, bizObjToImportTo, exportedValueObject, exportedValueObjectXml, "From a new business object");

			// We do not update existing transactions in accounting. The line below is not required from the original test
			// AssertImportFromThenExportToProducesSameXml(Sample, BizObjToImportTo, ExportedValueObject, ExportedValueObjectXml, "Updating an already populated business object to ensure that updating updates existing collection items and doesn't add them if they already exist");

			bizObjToImportTo.Delete();
		}

		public void TestImportofARInvoice_ComplianceSequence()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff currentuser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentuser.GS_EmailAddress = "david.park@test.com";
			Factory.Save();
			group.Staff.Add(currentuser);
			AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Mexico);
			ComplianceSubTypeAttributionRuleConfigurationCollection collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			ComplianceSubTypeAttributionRuleConfiguration item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.DisbursementOnly; // "DSB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			XsdInvoice = GetFullyPopulatedXmlInvoice_AR();
			SetOrganisation(XsdInvoice);
			XsdInvoice.TxnLines[0].TaxCode = "XXXX";

			NotificationBuffer notify = new NotificationBuffer();
			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);
			Factory.Save();
			AssertEquals(1, DataAdapter.ComplianceEmails.Count);
			AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, "Warning: Line 1: No matches were found for the following Tax Rate: XXXX");

			Env.OutgoingMailManager.EmailsCreated.Clear();

			AccComplianceSequence sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, item.SubType, 2, 100, 25);
			sequence.XD_Prefix = "01.02-";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);
			Factory.Save();
			AssertEquals(0, DataAdapter.ComplianceEmails.Count);
			AssertEquals("No email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2018, 10, 10)]
		public void TestImportofARInvoice_DigitalSignature()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff currentuser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentuser.GS_EmailAddress = "david.park@test.com";
			Factory.Save();
			group.Staff.Add(currentuser);
			AccountingConfigurationRegistry.Instance.ComplianceInvoiceBookAllocaltionFailureNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Portugal);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "PTLIS";
			ComplianceSubTypeAttributionRuleConfigurationCollection collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			ComplianceSubTypeAttributionRuleConfiguration item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.Portugal;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.DisbursementOnly; // "DSB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			XsdInvoice = GetFullyPopulatedXmlInvoice_AR();
			SetOrganisation(XsdInvoice);
			XsdInvoice.TxnLines[0].TaxCode = "XXXX";

			NotificationBuffer notify = new NotificationBuffer();
			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			AccComplianceSequence sequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, item.SubType, 2, 100, 25);
			sequence.XD_Prefix = "0102";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);
			Factory.Save();
			AssertEquals(1, DataAdapter.DigitalSignatureEmails.Count);
			AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals($"When post transactions, CW1 failed to sign transactions with valid digital Signatures due to the following reason:{System.Environment.NewLine}Current invoice was not signed with a digital signature as it failed to find the previous invoice in the sequence.", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		public void TestImportofARInvoice()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.INV, true, true);
		}

		public void TestImportofARCreditNote()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.CRD, true, false);
		}

		public void TestImportofPositiveARAdjustmentNote()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.ADJ, true, true);
		}

		public void TestImportofNegativeARAdjustmentNote()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.ADJ, false, false);
		}

		public void TestImportofAPInvoice()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.INV, true, false, true);
		}

		public void TestImportofAPInvoiceWithJobTransform()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.INV, true, false, true);
		}

		public void TestImportofAPCreditNote()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.CRD, true, true, true);
		}

		public void TestImportofAPCreditNoteWithTransform()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.CRD, true, true, true);
		}

		[SuspendCriticalValidation]
		public void TestImportofPositiveAPAdjustmentNote()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.ADJ, true, false);
		}

		[SuspendCriticalValidation]
		public void TestImportofNegativeAPAdjustmentNote()
		{
			ImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.ADJ, false, true);
		}

		public void TestImportofAPInvoiceNotGSTRegistered()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			XsdInvoice = GetFullyPopulatedXmlInvoice_AP();
			SetOrganisation(XsdInvoice);
			XsdInvoice.TxnLines[0].TaxCode = "XXXX";

			NotificationBuffer notify = new NotificationBuffer();
			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);
			AssertEquals("Precondition: current company is not GST registered.", false, GlbCompany.CurrentCompany.GC_IsGSTRegistered);
			TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, "Error: Line 1: No matches were found for the following Tax Rate: XXXX");
		}

		void ImportTransactionTest(Xsd.TxnLedgerType ledger, Xsd.TxnType transactionType, bool bizObjHasPositiveValues, bool xmlObjHasPositiveValues)
		{
			ImportTransactionTest(ledger, transactionType, bizObjHasPositiveValues, xmlObjHasPositiveValues, false);
		}

		void ImportTransactionTest(Xsd.TxnLedgerType ledger, Xsd.TxnType transactionType,
			bool bizObjHasPositiveValues, bool xmlObjHasPositiveValues, bool performJobChargeTransform)
		{
			XsdInvoice = GetFullyPopulatedXmlInvoice_AP();

			XsdInvoice.Ledger = ledger;
			XsdInvoice.TxnType = transactionType;
			XsdInvoice.OsInvoiceAmtInclTax.CurrencyCode = ObjectCreator.USD.RX_Code;
			SetOrganisation(XsdInvoice);

			XsdInvoice.TxnLines[0].WHTCode = ZString.Empty;
			XsdInvoice.TxnLines[1].WHTCode = ZString.Empty;

			if (XsdInvoice.Ledger == Xsd.TxnLedgerType.AR)
			{
				XsdInvoice.TxnNumber = ZString.Empty;
				if (transactionType == Xsd.TxnType.INV || transactionType == Xsd.TxnType.CRD || transactionType == Xsd.TxnType.ADJ)
				{
					XsdInvoice.TxnCategory = "DCU";
				}
			}

			if (transactionType != Xsd.TxnType.INV && transactionType != Xsd.TxnType.CRD)
			{
				XsdInvoice.TxnLines[0].ConsolOrJobNo = XsdInvoice.TxnLines[1].ConsolOrJobNo = ZString.Empty;
			}

			ZDecimal localAmtExclTax = xmlObjHasPositiveValues ? 200 : -200;
			ZDecimal localAmtInclTax = xmlObjHasPositiveValues ? 220 : -220;
			ZDecimal localTaxAmt = xmlObjHasPositiveValues ? 20 : -20;
			ZDecimal localWhtAmt = xmlObjHasPositiveValues ? 0 : -0;
			ZDecimal osAmtExclTax = xmlObjHasPositiveValues ? 100 : -100;
			ZDecimal osAmtInclTax = xmlObjHasPositiveValues ? 110 : -110;
			ZDecimal osTaxAmt = xmlObjHasPositiveValues ? 10 : -10;
			ZDecimal osWhtAmt = xmlObjHasPositiveValues ? 0 : -0;

			SetAmountsOnXmlInvoiceHeader(XsdInvoice, localAmtExclTax, localAmtInclTax, localTaxAmt, localWhtAmt, osAmtExclTax, osAmtInclTax, osTaxAmt, osWhtAmt);

			NotificationBuffer notify = new NotificationBuffer();

			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, new ValueObjectExportContext(notify));
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(performJobChargeTransform);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);
			Factory.Save();

			if (performJobChargeTransform)
			{
				Job job = new BusinessObjectFactory().Load<Job>(Invoice.Lines[1].AL_JH);
				job.Charges.Load();
				Assert(job.Charges.Count > 0);
			}

			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, XsdInvoice);

			ZDecimal bizObjLocalAmtExclTax = bizObjHasPositiveValues ? 200 : -200;
			ZDecimal bizObjLocalAmtInclTax = bizObjHasPositiveValues ? 220 : -220;
			ZDecimal bizObjLocalTaxAmt = bizObjHasPositiveValues ? 20 : -20;
			ZDecimal bizObjLocalWhtAmt = bizObjHasPositiveValues ? 0 : 0;
			ZDecimal bizObjOsAmtExclTax = bizObjHasPositiveValues ? 100 : -100;
			ZDecimal bizObjOsAmtInclTax = bizObjHasPositiveValues ? 110 : -110;
			ZDecimal bizObjOsTaxAmt = bizObjHasPositiveValues ? 10 : -10;
			ZDecimal bizObjOsWhtAmt = bizObjHasPositiveValues ? 0 : 0;

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, bizObjLocalAmtExclTax, bizObjLocalAmtInclTax, bizObjLocalTaxAmt, bizObjLocalWhtAmt,
				bizObjOsAmtExclTax, bizObjOsAmtInclTax, bizObjOsTaxAmt, bizObjOsWhtAmt);

			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], bizObjLocalAmtExclTax / 2, bizObjLocalAmtInclTax / 2, bizObjLocalTaxAmt / 2,
				bizObjLocalWhtAmt / 2, bizObjOsAmtExclTax / 2, bizObjOsAmtInclTax / 2, bizObjOsTaxAmt / 2, bizObjOsWhtAmt / 2);

			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], bizObjLocalAmtExclTax / 2, bizObjLocalAmtInclTax / 2, bizObjLocalTaxAmt / 2,
				bizObjLocalWhtAmt / 2, bizObjOsAmtExclTax / 2, bizObjOsAmtInclTax / 2, bizObjOsTaxAmt / 2, bizObjOsWhtAmt / 2);
		}

		public void TestImportOfInvalidType_XsdTxnHeader()
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			XsdInvoice = GetFullyPopulatedXmlInvoice_AP();

			XsdInvoice.Ledger = Xsd.TxnLedgerType.AR;
			XsdInvoice.TxnType = Xsd.TxnType.DPY;

			InvoicingBase bizObj = DataAdapter.NewBusinessObject(XsdInvoice, context);
			AssertNull("BizObj should be null", bizObj);

			DataAdapter.ImportFromValueObject(bizObj, XsdInvoice, context);
			AssertNull("BizObj should still be null", bizObj);

			TestHelper.AssertNotificationsContainsErrorMessage(notify, "Error: This transaction cannot be imported. Ledger: AR, Transaction Type: DPY.");
		}

		public void TestImportNotificationHandlesOrganisationAndPayRef()
		{
			XsdInvoice = GetFullyPopulatedXmlInvoice_AR();

			XsdInvoice.OsWHTAmount.Value = 0M;
			XsdInvoice.PaymentReference = "ABCDEFG";
			XsdInvoice.TxnNumber = "TST00001";

			XsdInvoice.TxnLines[0].WHTCode = ZString.Empty;
			XsdInvoice.TxnLines[0].HouseBIllNo = ZString.Empty;
			XsdInvoice.TxnLines[0].OsWHTAmount.Value = 0M;

			NotificationBuffer firstNotify = new NotificationBuffer();
			InvoicingBase firstInvoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, firstNotify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, new ValueObjectExportContext(firstNotify));

			DataAdapter.ImportFromValueObject(firstInvoice, XsdInvoice, context);

			ZString messageToDisplay = "Pay Ref and Organization should be included, Notifications are as follows:" + System.Environment.NewLine + firstNotify.AsString;
			Assert(messageToDisplay, firstNotify.AsString.Contains("Transaction AR INV ABIGAS TST00001: (Pay Ref: ABCDEFG)"));

			XsdInvoice.PaymentReference = "";
			XsdInvoice.TxnNumber = "";
			NotificationBuffer secondNotify = new NotificationBuffer();
			InvoicingBase secondInvoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			context = new ValueObjectImportContext(Factory, secondNotify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, new ValueObjectExportContext(secondNotify));
			DataAdapter.ImportFromValueObject(secondInvoice, XsdInvoice, context);

			messageToDisplay = "Pay Ref and Organization should be blank, Notifications are as follows:" + System.Environment.NewLine + secondNotify.AsString;
			Assert(messageToDisplay, secondNotify.AsString.Contains("Transaction AR INV ABIGAS :"));
			Assert(messageToDisplay, !secondNotify.AsString.Contains("Pay Ref:"));
		}

		[TestDate(2006, 01, 05)]
		public void TestImportofTransactionAlreadyInDatabase_ForARTransactions()
		{
			XsdInvoice = GetFullyPopulatedXmlInvoice_AR();

			XsdInvoice.OsWHTAmount.Value = 0M;

			XsdInvoice.TxnLines[0].WHTCode = ZString.Empty;
			XsdInvoice.TxnLines[0].HouseBIllNo = ZString.Empty;
			XsdInvoice.TxnLines[0].OsWHTAmount.Value = 0M;

			XsdInvoice.TxnLines[1].WHTCode = ZString.Empty;
			XsdInvoice.TxnLines[1].HouseBIllNo = ZString.Empty;
			XsdInvoice.TxnLines[1].OsWHTAmount.Value = 0M;

			NotificationBuffer firstNotify = new NotificationBuffer();
			InvoicingBase firstInvoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, firstNotify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, new ValueObjectExportContext(firstNotify));

			DataAdapter.ImportFromValueObject(firstInvoice, XsdInvoice, context);

			ZString messageToDisplay = "First Notify has errors. Notifications are as follows:" + System.Environment.NewLine + firstNotify.AsString;
			AssertEquals(messageToDisplay, false, firstNotify.ContainsNotificationType(ErrorType.Error));

			Factory.Save();

			NotificationBuffer secondNotify = new NotificationBuffer();
			InvoicingBase secondInvoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			context = new ValueObjectImportContext(Factory, secondNotify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, new ValueObjectExportContext(secondNotify));

			DataAdapter.ImportFromValueObject(secondInvoice, XsdInvoice, context);

			messageToDisplay = "Second Notify has errors. Notifications are as follows:" + System.Environment.NewLine + secondNotify.AsString;
			AssertEquals(messageToDisplay, false, secondNotify.ContainsNotificationType(ErrorType.Error));
		}

		[TestDate(2006, 01, 05)]
		public void TestImportofTransactionAlreadyInDatabase_ForAPTransactions()
		{
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(true);
			XsdInvoice = GetFullyPopulatedXmlInvoice_AP();

			XsdInvoice.OsWHTAmount.Value = 0M;

			XsdInvoice.TxnLines[0].WHTCode = ZString.Empty;
			XsdInvoice.TxnLines[0].HouseBIllNo = ZString.Empty;
			XsdInvoice.TxnLines[0].OsWHTAmount.Value = 0M;

			XsdInvoice.TxnLines[1].WHTCode = ZString.Empty;
			XsdInvoice.TxnLines[1].HouseBIllNo = ZString.Empty;
			XsdInvoice.TxnLines[1].OsWHTAmount.Value = 0M;

			NotificationBuffer firstNotify = new NotificationBuffer();
			InvoicingBase firstInvoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, firstNotify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, new ValueObjectExportContext(firstNotify));

			DataAdapter.ImportFromValueObject(firstInvoice, XsdInvoice, context);

			ZString messageToDisplay = "First Notify has errors. Notifications are as follows:" + System.Environment.NewLine + firstNotify.AsString;
			AssertEquals(messageToDisplay, false, firstNotify.ContainsNotificationType(ErrorType.Error));

			Factory.Save();

			NotificationBuffer secondNotify = new NotificationBuffer();
			InvoicingBase secondInvoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			context = new ValueObjectImportContext(Factory, secondNotify);
			DataAdapter.ImportFromValueObject(secondInvoice, XsdInvoice, context);

			messageToDisplay = "Second Notify has errors because the transaction has already been imported" + System.Environment.NewLine + secondNotify.AsString;
			AssertEquals(messageToDisplay, true, secondNotify.ContainsNotificationType(ErrorType.Error));
			AssertEquals("ExceptionReporter should not catch any exceptions", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestDeletionOfTransaction_OneTransactionUsingJob()
		{
			Job lineJob = Factory.NewJobWithValidTestDataForTesting<Job>(TestBusinessObjectKind.MinimumRequiredToSave);

			Invoice = (InvoicingBase)Factory.NewWithValidTestData(typeof(APInvoice));
			InvoicingLineBase line = (InvoicingLineBase)Invoice.Lines.AddNew();
			line.AL_JH = lineJob.PK;
			DataAdapter.DeleteTransactionAndAnythingCreatedByItExposed(Invoice);

			AssertEquals("Header should be deleted", true, Invoice.IsDeleted);
			AssertEquals("Line should be deleted", true, line.IsDeleted);
			AssertEquals("Job should be deleted", true, lineJob.IsDeleted);
		}

		public void TestDeletionOfTransaction_InvoiceWithApportionmentCharges()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();
			var jobInvoicingPlugIn = shipment1 as IJobInvoicingPlugIn;
			jobInvoicingPlugIn.InvoicingSupporter.JobInvoicingSecurity.IsAllowed = true;
			var shipment2 = consol.Shipments.AddNew();
			jobInvoicingPlugIn = shipment2;
			jobInvoicingPlugIn.InvoicingSupporter.JobInvoicingSecurity.IsAllowed = true;
			Factory.Save();

			Invoice = (InvoicingBase)Factory.NewWithValidTestData(typeof(APInvoice));
			var chargeCode = TestObjectCreator.CC1;
			var taxRate = TestObjectCreator.GSTFREE1;

			Invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			Invoice.SubmittedFromInvoicingForm = true;
			JobConsolCost cost = null;
			try
			{
				cost = Invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				cost.E6_OSCostAmount = 20m;
				cost.E6_AT_TaxRate = taxRate.PK;
				cost.E6_ApportionmentMethod = "SHP";
				cost.SetIsUsedForApportionment();

				Invoice.ImportSingleCost(cost, null);
			}
			finally
			{
				Invoice.ClearApportionmentJobMutexes();
			}

			DataAdapter.DeleteTransactionAndAnythingCreatedByItExposed(Invoice);

			AssertEquals("Header should be deleted", true, Invoice.IsDeleted);
			AssertEquals("Lines should be deleted", true, Invoice.Lines.All(x => x.IsDeleted));
			AssertEquals("Charges should be deleted", true, Invoice.Lines.Cast<InvoicingLineBase>().All(x => x.ApportionmentChargeImportedFrom == null));
			AssertEquals("Cost should be deleted", true, cost?.IsDeleted);
			AssertEquals("should not have error for deleted charge", false, ErrorReporter.HasBeenReported("ApportionmentChargeImportedFromIsDeleted_4"));
		}

		public void TestDeletionOfTransactionReleaseMutexOnNewlyCreatedJob()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var loader = new JobHeader.Loader(shipment);
			using (var job = (Job)loader.TryCreateWithMutex(GlbBranch.CurrentBranch))
			{
				ZArchitecture.Data.Mutex.ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
				Assert("mutex should be locked", mutex.IsLocked);

				Invoice = (InvoicingBase)Factory.NewWithValidTestData(typeof(APInvoice));
				var line = (InvoicingLineBase)Invoice.Lines.AddNew();
				line.AL_JH = job.PK;

				DataAdapter.DeleteTransactionAndAnythingCreatedByItExposed(Invoice);
				mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);
				Assert("mutex should be released", !mutex.IsLocked);

				AssertEquals("Header should be deleted", true, Invoice.IsDeleted);
				AssertEquals("Line should be deleted", true, line.IsDeleted);
				AssertEquals("Job should be deleted", true, job.IsDeleted);
			}
		}

		public void TestDeletionOfTransaction_TwoTransactionUsingSameJob()
		{
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			Job sharedJob = Factory.NewJobWithValidTestDataForTesting<Job>(TestBusinessObjectKind.MinimumRequiredToSave);

			InvoicingBase invoice1 = (InvoicingBase)Factory.NewWithValidTestData(typeof(APInvoice));
			InvoicingLineBase line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
			line1.AL_JH = sharedJob.PK;

			InvoicingBase invoice2 = (InvoicingBase)Factory.NewWithValidTestData(typeof(APInvoice));
			InvoicingLineBase line2 = (InvoicingLineBase)invoice2.Lines.AddNew();
			line2.AL_JH = sharedJob.PK;

			DataAdapter.DeleteTransactionAndAnythingCreatedByItExposed(invoice1);
			AssertEquals("Header 1 should be deleted", true, invoice1.IsDeleted);
			AssertEquals("Line 1 should be deleted", true, line1.IsDeleted);
			AssertEquals("Job should NOT be deleted", false, sharedJob.IsDeleted);

			DataAdapter.DeleteTransactionAndAnythingCreatedByItExposed(invoice2);
			AssertEquals("Header 2 should be deleted", true, invoice2.IsDeleted);
			AssertEquals("Line 2 should be deleted", true, line2.IsDeleted);
			AssertEquals("Job should be deleted", true, sharedJob.IsDeleted);
		}

		public void TestDeletionOfTransaction_NonJobRelatedTransaction()
		{
			Invoice = (InvoicingBase)Factory.NewWithValidTestData(typeof(APInvoice));
			InvoicingLineBase line = (InvoicingLineBase)Invoice.Lines.AddNew();

			DataAdapter.DeleteTransactionAndAnythingCreatedByItExposed(Invoice);

			AssertEquals("Header should be deleted", true, Invoice.IsDeleted);
			AssertEquals("Line should be deleted", true, line.IsDeleted);
		}

		public void TestImportDataBranchDepartmentCombinationsValidationError()
		{
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var txnHeader = GetFullyPopulatedXmlInvoice_AP();
			var txnLine = txnHeader.TxnLines[0];
			var invoice = DataAdapter.NewBusinessObject(txnHeader, context);
			invoice.AH_TransactionNum = "TEST_TRANSACTIONNUM";

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsErrorMessage
				(Factory, () =>
				{
					DataAdapter.ImportFromValueObject(invoice, txnHeader, context);

					Assert("Should have error", notify.HasErrors);
					return notify.AsString;
				});
		}

		public void TestInvoicingPreSaveHelperType()
		{
			AssertType<InvoicingPreSaveHelper>(DataAdapter.InvoicingPreSaveHelper_ExposedForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestImportWithPreSaveActions()
		{
			var invoicingPreSaveHelper = new Mock<IInvoicingPreSaveHelper>(MockBehavior.Strict);
			invoicingPreSaveHelper.Setup(x => x.PreSaveActionsForNonJobBillingPosting(It.IsAny<InvoicingBase>()));

			XsdInvoice = GetFullyPopulatedXmlInvoice_AR();
			SetOrganisation(XsdInvoice);

			var notify = new NotificationBuffer();
			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			var context = new ValueObjectImportContext(Factory, notify);
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(true);
			DataAdapter.SubstituteInvoicingPreSaveHelper_ForTestOnly(invoicingPreSaveHelper.Object);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);
			invoicingPreSaveHelper.Verify(x => x.PreSaveActionsForNonJobBillingPosting(It.IsAny<InvoicingBase>()), Times.Once, "PreSaveAction is called for when import multiple invoices.");

			notify = new NotificationBuffer();
			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			context = new ValueObjectImportContext(Factory, notify);
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			invoicingPreSaveHelper.Invocations.Clear();
			DataAdapter.SubstituteInvoicingPreSaveHelper_ForTestOnly(invoicingPreSaveHelper.Object);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);
			invoicingPreSaveHelper.Verify(x => x.PreSaveActionsForNonJobBillingPosting(It.IsAny<InvoicingBase>()), Times.Never, "PreSaveAction is not called when import single invoice.");

			notify = new NotificationBuffer();
			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice));
			context = new ValueObjectImportContext(Factory, notify);
			XsdInvoice.Ledger = Xsd.TxnLedgerType.AP;
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(true);
			invoicingPreSaveHelper.Invocations.Clear();
			DataAdapter.SetDeleteFunctionalityToNormal();
			DataAdapter.SubstituteInvoicingPreSaveHelper_ForTestOnly(invoicingPreSaveHelper.Object);
			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);
			invoicingPreSaveHelper.Verify(x => x.PreSaveActionsForNonJobBillingPosting(It.IsAny<InvoicingBase>()), Times.Never, "PreSaveAction is not called when import multiple invoices with error.");
		}

		[TestDate(2006, 01, 05)]
		public void TestImportTransactionsWithSameFactoryCachesUSSalesTaxCalculatorObject()
		{
			var xsdInvoice1 = GetFullyPopulatedXmlInvoice_AR();
			SetOrganisation(xsdInvoice1);
			var xsdInvoice2 = GetFullyPopulatedXmlInvoice_AR();
			SetOrganisation(xsdInvoice2);
			var xsdInvoice3 = GetFullyPopulatedXmlInvoice_AR();
			SetOrganisation(xsdInvoice3);

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			mockCalculator.Setup(x => x.IsEnabled(It.IsAny<GlbBranch>())).Returns(true);
			mockCalculator.Setup(x => x.ShouldSetSalesTaxOnPost(It.IsAny<InvoicingBase>())).Returns(true);
			var calculationResult = new CalculationResult(5m);
			mockCalculator.Setup(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));
			mockCalculator.Setup(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var notify = new NotificationBuffer();
				var context = new ValueObjectImportContext(Factory, notify);
				DataAdapter = new FinancialInvoiceDataAdapterTestClass(true);

				var bizoInvoice1 = DataAdapter.NewBusinessObject(xsdInvoice1, context);
				DataAdapter.ImportFromValueObject(bizoInvoice1, xsdInvoice1, context);
				TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, "Error");
				Factory.Save();

				var bizoInvoice2 = DataAdapter.NewBusinessObject(xsdInvoice2, context);
				DataAdapter.ImportFromValueObject(bizoInvoice2, xsdInvoice2, context);
				TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, "Error");

				var bizoInvoice3 = DataAdapter.NewBusinessObject(xsdInvoice3, context);
				DataAdapter.ImportFromValueObject(bizoInvoice3, xsdInvoice3, context);
				TestHelper.AssertNotificationsDoesNotContainErrorMessage(notify, "Error");
				Factory.Save();
			}

			mockCalculator.Verify(x => x.Dispose(), Times.Never(), "One USSalesTaxCalculator should be created, and re-used for all XML transactions processed using a single Factory. As the calculator is cached, Dispose() is not called");
			mockCalculator.Verify(x => x.SetSalesTaxLineItem(It.IsAny<InvoicingBase>(), It.IsAny<decimal>()), Times.Exactly(3), "Sales tax line items should be added for each invoice");
		}

		#endregion

		#region AgentReference

		public void TestAgentReferenceForShipment()
		{
			ForwardingShipment testShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			testShipment.JS_UniqueConsignRef = "S103921";

			BaseJobDeclaration declaration = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)) as BaseJobDeclaration;
			declaration.JE_JS = testShipment.PK;
			declaration.JE_AgentsReference = "TESTAGENT";

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			testJob.JH_ParentID = testShipment.PK;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = testShipment.JS_UniqueConsignRef;

			InvoicingLineBase testInvoiceLine = Factory.NewWithValidTestData(typeof(ARInvoiceLine)) as InvoicingLineBase;
			testInvoiceLine.AL_JH = testJob.PK;
			Xsd.TxnLine testLine = new Xsd.TxnLine();

			DataAdapter.PopulateAgentsOwnerOrderReference(testLine, testInvoiceLine, new NotificationBuffer());

			AssertEquals(declaration.JE_AgentsReference, testLine.AgentsReference);
		}

		public void TestAgentReferenceForDeclaration()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)) as BaseJobDeclaration;
			declaration.JE_AgentsReference = "TESTAGENT";

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			testJob.JH_ParentID = declaration.PK;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			testJob.JH_JobNum = declaration.JE_DeclarationReference;

			InvoicingLineBase testInvoiceLine = Factory.NewWithValidTestData(typeof(ARInvoiceLine)) as InvoicingLineBase;
			testInvoiceLine.AL_JH = testJob.PK;
			Xsd.TxnLine testLine = new Xsd.TxnLine();

			DataAdapter.PopulateAgentsOwnerOrderReference(testLine, testInvoiceLine, new NotificationBuffer());

			AssertEquals(declaration.JE_AgentsReference, testLine.AgentsReference);
		}

		public void TestOrphanedJobHeader()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = ZGuid.NewZGuid();
			job.JH_JobNum = "SHP001";

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, 0);
			invoiceLine.AL_JH = job.PK;
			var testLine = new Xsd.TxnLine();

			AssertNoExceptionThrown(() => DataAdapter.PopulateAgentsOwnerOrderReference(testLine, invoiceLine, new NotificationBuffer()));
		}

		#endregion

		#region Cross Ledger

		public void TestGetNewBusinessObjectForCrossLedger()
		{
			Header.OH_Code = "TEST01";
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);

			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = Header.OH_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = Header.OH_Code;
			XsdInvoice = GetFullyPopulatedXmlInvoice_AR();

			BusinessObject bizO = DataAdapter.NewBusinessObject(XsdInvoice, context);
			AssertEquals(typeof(APInvoice), bizO.GetType());

			XsdInvoice.TxnType = Xsd.TxnType.CRD;
			bizO = DataAdapter.NewBusinessObject(XsdInvoice, context);
			AssertEquals(typeof(APCreditNote), bizO.GetType());

			XsdInvoice.TxnType = Xsd.TxnType.ADJ;
			bizO = DataAdapter.NewBusinessObject(XsdInvoice, context);
			AssertEquals(typeof(APAdjustmentNote), bizO.GetType());
		}

		public void TestTransformXsdForCrossLedgerImport()
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.ABIGAS, new ValueObjectExportContext(notify));

			XsdInvoice = GetFullyPopulatedXmlInvoice_AR();
			XsdInvoice.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.AALSHI, new ValueObjectExportContext(notify));

			ZDecimal localInvoiceAmtExclTax = XsdInvoice.LocalInvoiceAmtExclTax.Value;
			ZDecimal localInvoiceAmtInclTax = XsdInvoice.LocalInvoiceAmtInclTax.Value;
			ZDecimal localTaxAmount = XsdInvoice.LocalTaxAmount.Value;
			ZDecimal localWHTAmount = XsdInvoice.LocalWHTAmount.Value;
			ZDecimal osInvoiceAmtExclTax = XsdInvoice.OsInvoiceAmtExclTax.Value;
			ZDecimal osInvoiceAmtInclTax = XsdInvoice.OsInvoiceAmtInclTax.Value;
			ZDecimal osTaxAmount = XsdInvoice.OsTaxAmount.Value;
			ZDecimal osWHTAmount = XsdInvoice.OsWHTAmount.Value;

			ZDecimal localInvoiceAmtExclTaxLine = XsdInvoice.TxnLines[0].LocalInvoiceAmtExclTax.Value;
			ZDecimal localInvoiceAmtInclTaxLine = XsdInvoice.TxnLines[0].LocalInvoiceAmtInclTax.Value;
			ZDecimal localTaxAmountLine = XsdInvoice.TxnLines[0].LocalTaxAmount.Value;
			ZDecimal localWHTAmountLine = XsdInvoice.TxnLines[0].LocalWHTAmount.Value;
			ZDecimal osInvoiceAmtExclTaxLine = XsdInvoice.TxnLines[0].OsInvoiceAmtExclTax.Value;
			ZDecimal osInvoiceAmtInclTaxLine = XsdInvoice.TxnLines[0].OsInvoiceAmtInclTax.Value;
			ZDecimal osTaxAmountLine = XsdInvoice.TxnLines[0].OsTaxAmount.Value;
			ZDecimal osWHTAmountLine = XsdInvoice.TxnLines[0].OsWHTAmount.Value;

			APInvoice invoice = Factory.New<APInvoice>();

			XsdInvoice = DataAdapter.TransformXsdForCrossLedgerImport(invoice, XsdInvoice, context);

			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsPayable, XsdInvoice.Ledger.ToString());
			AssertEquals(((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation, XsdInvoice.DebtorOrCreditor);

			AssertEquals(-localInvoiceAmtExclTax, XsdInvoice.LocalInvoiceAmtExclTax.Value);
			AssertEquals(-localInvoiceAmtInclTax, XsdInvoice.LocalInvoiceAmtInclTax.Value);
			AssertEquals(-localTaxAmount, XsdInvoice.LocalTaxAmount.Value);
			AssertEquals(-localWHTAmount, XsdInvoice.LocalWHTAmount.Value);
			AssertEquals(-osInvoiceAmtExclTax, XsdInvoice.OsInvoiceAmtExclTax.Value);
			AssertEquals(-osInvoiceAmtInclTax, XsdInvoice.OsInvoiceAmtInclTax.Value);
			AssertEquals(-osTaxAmount, XsdInvoice.OsTaxAmount.Value);
			AssertEquals(-osWHTAmount, XsdInvoice.OsWHTAmount.Value);

			AssertEquals(ZArchitecture.Core.TransactionLineTypes.Cost, XsdInvoice.TxnLines[0].LineType.ToString());
			AssertEquals(-localInvoiceAmtExclTaxLine, XsdInvoice.TxnLines[0].LocalInvoiceAmtExclTax.Value);
			AssertEquals(-localInvoiceAmtInclTaxLine, XsdInvoice.TxnLines[0].LocalInvoiceAmtInclTax.Value);
			AssertEquals(-localTaxAmountLine, XsdInvoice.TxnLines[0].LocalTaxAmount.Value);
			AssertEquals(-localWHTAmountLine, XsdInvoice.TxnLines[0].LocalWHTAmount.Value);
			AssertEquals(-osInvoiceAmtExclTaxLine, XsdInvoice.TxnLines[0].OsInvoiceAmtExclTax.Value);
			AssertEquals(-osInvoiceAmtInclTaxLine, XsdInvoice.TxnLines[0].OsInvoiceAmtInclTax.Value);
			AssertEquals(-osTaxAmountLine, XsdInvoice.TxnLines[0].OsTaxAmount.Value);
			AssertEquals(-osWHTAmountLine, XsdInvoice.TxnLines[0].OsWHTAmount.Value);
		}

		public void TestTransformXsdForCrossLedgerImport_CrossCountry()
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.ABIGAS, new ValueObjectExportContext(notify));

			GlbBranch originalOverseasBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN");
			AssertNotNull("Overseas Branch", originalOverseasBranch);

			using (new TemporaryUserContext() { BranchPK = originalOverseasBranch.PK.ToGuid(), StaffLoginName = User.SupportUserName }.Set())
			{
				XsdInvoice = GetFullyPopulatedXmlInvoice_AR();
			}
			AssertEquals("Overseas Branch", "SIN", XsdInvoice.Branch);

			XsdInvoice.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.AALSHI, new ValueObjectExportContext(notify));

			ZDecimal localInvoiceAmtExclTax = XsdInvoice.LocalInvoiceAmtExclTax.Value;
			ZDecimal localInvoiceAmtInclTax = XsdInvoice.LocalInvoiceAmtInclTax.Value;
			ZDecimal localTaxAmount = XsdInvoice.LocalTaxAmount.Value;
			ZDecimal localWHTAmount = XsdInvoice.LocalWHTAmount.Value;
			ZDecimal osInvoiceAmtExclTax = XsdInvoice.OsInvoiceAmtExclTax.Value;
			ZDecimal osInvoiceAmtInclTax = XsdInvoice.OsInvoiceAmtInclTax.Value;
			ZDecimal osTaxAmount = XsdInvoice.OsTaxAmount.Value;
			ZDecimal osWHTAmount = XsdInvoice.OsWHTAmount.Value;

			ZDecimal localInvoiceAmtExclTaxLine = XsdInvoice.TxnLines[0].LocalInvoiceAmtExclTax.Value;
			ZDecimal localInvoiceAmtInclTaxLine = XsdInvoice.TxnLines[0].LocalInvoiceAmtInclTax.Value;
			ZDecimal localTaxAmountLine = XsdInvoice.TxnLines[0].LocalTaxAmount.Value;
			ZDecimal localWHTAmountLine = XsdInvoice.TxnLines[0].LocalWHTAmount.Value;
			ZDecimal osInvoiceAmtExclTaxLine = XsdInvoice.TxnLines[0].OsInvoiceAmtExclTax.Value;
			ZDecimal osInvoiceAmtInclTaxLine = XsdInvoice.TxnLines[0].OsInvoiceAmtInclTax.Value;
			ZDecimal osTaxAmountLine = XsdInvoice.TxnLines[0].OsTaxAmount.Value;
			ZDecimal osWHTAmountLine = XsdInvoice.TxnLines[0].OsWHTAmount.Value;

			APInvoice invoice = Factory.New<APInvoice>();
			AssertEquals("Local Branch", GlbBranch.CurrentBranch.PK, invoice.Branch.PK);

			XsdInvoice = DataAdapter.TransformXsdForCrossLedgerImport(invoice, XsdInvoice, context);

			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsPayable, XsdInvoice.Ledger.ToString());
			AssertEquals(((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation, XsdInvoice.DebtorOrCreditor);

			AssertEquals("LocalInvoiceAmtExclTax", -localInvoiceAmtInclTax, XsdInvoice.LocalInvoiceAmtExclTax.Value);
			AssertEquals("LocalInvoiceAmtInclTax", -localInvoiceAmtInclTax, XsdInvoice.LocalInvoiceAmtInclTax.Value);
			AssertEquals("LocalTaxAmount", 0m, XsdInvoice.LocalTaxAmount.Value);
			AssertEquals("LocalWHTAmount", -localWHTAmount, XsdInvoice.LocalWHTAmount.Value);
			AssertEquals("OsInvoiceAmtExclTax", -osInvoiceAmtInclTax, XsdInvoice.OsInvoiceAmtExclTax.Value);
			AssertEquals("OsInvoiceAmtInclTax", -osInvoiceAmtInclTax, XsdInvoice.OsInvoiceAmtInclTax.Value);
			AssertEquals("OsTaxAmount", 0m, XsdInvoice.OsTaxAmount.Value);
			AssertEquals("OsWHTAmount", -osWHTAmount, XsdInvoice.OsWHTAmount.Value);

			AssertEquals("LineType", ZArchitecture.Core.TransactionLineTypes.Cost, XsdInvoice.TxnLines[0].LineType.ToString());
			AssertEquals("Line LocalInvoiceAmtExclTax", -localInvoiceAmtInclTaxLine, XsdInvoice.TxnLines[0].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line LocalInvoiceAmtInclTax", -localInvoiceAmtInclTaxLine, XsdInvoice.TxnLines[0].LocalInvoiceAmtInclTax.Value);
			AssertEquals("Line LocalTaxAmount", 0m, XsdInvoice.TxnLines[0].LocalTaxAmount.Value);
			AssertEquals("Line LocalWHTAmount", -localWHTAmountLine, XsdInvoice.TxnLines[0].LocalWHTAmount.Value);
			AssertEquals("Line OsInvoiceAmtExclTax", -osInvoiceAmtInclTaxLine, XsdInvoice.TxnLines[0].OsInvoiceAmtExclTax.Value);
			AssertEquals("Line OsInvoiceAmtInclTax", -osInvoiceAmtInclTaxLine, XsdInvoice.TxnLines[0].OsInvoiceAmtInclTax.Value);
			AssertEquals("Luine OsTaxAmount", 0m, XsdInvoice.TxnLines[0].OsTaxAmount.Value);
			AssertEquals("Line OsWHTAmount", -osWHTAmountLine, XsdInvoice.TxnLines[0].OsWHTAmount.Value);
			AssertEquals("Line OriginalOsInvoiceAmtExclTax", -osInvoiceAmtExclTaxLine, XsdInvoice.TxnLines[0].OriginalOsInvoiceAmtExclTax.Value);
			AssertEquals("Line OriginalOsTaxAmount", -osTaxAmountLine, XsdInvoice.TxnLines[0].OriginalOsTaxAmount.Value);
			AssertEquals("Line OriginalLocalInvoiceAmtExclTax", -localInvoiceAmtExclTaxLine, XsdInvoice.TxnLines[0].OriginalLocalInvoiceAmtExclTax.Value);
			Assert("Line UseOriginalAmount", XsdInvoice.TxnLines[0].UseOriginalAmount);
		}

		public void TestTransformXsdForCrossLedgerImport_CrossCountry_SplitTaxLine()
		{
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.ABIGAS, new ValueObjectExportContext(notify));

			GlbBranch originalOverseasBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SIN");
			AssertNotNull("Overseas Branch", originalOverseasBranch);

			using (new TemporaryUserContext() { BranchPK = originalOverseasBranch.PK.ToGuid(), StaffLoginName = User.SupportUserName }.Set())
			{
				XsdInvoice = GetFullyPopulatedXmlInvoice_AR();
			}
			AssertEquals("Overseas Branch", "SIN", XsdInvoice.Branch);

			XsdInvoice.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.AALSHI, new ValueObjectExportContext(notify));

			ZDecimal localInvoiceAmtExclTax = XsdInvoice.LocalInvoiceAmtExclTax.Value;
			ZDecimal localInvoiceAmtInclTax = XsdInvoice.LocalInvoiceAmtInclTax.Value;
			ZDecimal localTaxAmount = XsdInvoice.LocalTaxAmount.Value;
			ZDecimal localWHTAmount = XsdInvoice.LocalWHTAmount.Value;
			ZDecimal osInvoiceAmtExclTax = XsdInvoice.OsInvoiceAmtExclTax.Value;
			ZDecimal osInvoiceAmtInclTax = XsdInvoice.OsInvoiceAmtInclTax.Value;
			ZDecimal osTaxAmount = XsdInvoice.OsTaxAmount.Value;
			ZDecimal osWHTAmount = XsdInvoice.OsWHTAmount.Value;

			ZDecimal localInvoiceAmtExclTaxLine1 = XsdInvoice.TxnLines[0].LocalInvoiceAmtExclTax.Value;
			ZDecimal localInvoiceAmtInclTaxLine1 = XsdInvoice.TxnLines[0].LocalInvoiceAmtInclTax.Value;
			ZDecimal localWHTAmountLine1 = XsdInvoice.TxnLines[0].LocalWHTAmount.Value;
			ZDecimal osInvoiceAmtExclTaxLine1 = XsdInvoice.TxnLines[0].OsInvoiceAmtExclTax.Value;
			ZDecimal osInvoiceAmtInclTaxLine1 = XsdInvoice.TxnLines[0].OsInvoiceAmtInclTax.Value;
			ZDecimal osWHTAmountLine1 = XsdInvoice.TxnLines[0].OsWHTAmount.Value;

			ZDecimal localInvoiceAmtExclTaxLine2 = XsdInvoice.TxnLines[1].LocalInvoiceAmtExclTax.Value;
			ZDecimal localInvoiceAmtInclTaxLine2 = XsdInvoice.TxnLines[1].LocalInvoiceAmtInclTax.Value;
			ZDecimal localWHTAmountLine2 = XsdInvoice.TxnLines[1].LocalWHTAmount.Value;
			ZDecimal osInvoiceAmtExclTaxLine2 = XsdInvoice.TxnLines[1].OsInvoiceAmtExclTax.Value;
			ZDecimal osInvoiceAmtInclTaxLine2 = XsdInvoice.TxnLines[1].OsInvoiceAmtInclTax.Value;
			ZDecimal osWHTAmountLine2 = XsdInvoice.TxnLines[1].OsWHTAmount.Value;

			APInvoice invoice = Factory.New<APInvoice>();
			AssertEquals("Local Branch", GlbBranch.CurrentBranch.PK, invoice.Branch.PK);

			AccountingConfigurationRegistry.Instance.SplitIntercompanyInvoiceTaxAmountIntoSeparateLine.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ObjectCreator.OverheadChargeCode.PK.ToGuid());
			XsdInvoice = DataAdapter.TransformXsdForCrossLedgerImport(invoice, XsdInvoice, context);

			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsPayable, XsdInvoice.Ledger.ToString());
			AssertEquals(((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation, XsdInvoice.DebtorOrCreditor);

			AssertNotEquals("LocalInvoiceAmtExclTax", -localInvoiceAmtExclTax, XsdInvoice.LocalInvoiceAmtExclTax.Value);
			AssertEquals("LocalInvoiceAmtExclTax", -localInvoiceAmtInclTax, XsdInvoice.LocalInvoiceAmtExclTax.Value);
			AssertEquals("LocalInvoiceAmtInclTax", -localInvoiceAmtInclTax, XsdInvoice.LocalInvoiceAmtInclTax.Value);
			AssertEquals("LocalTaxAmount", 0m, XsdInvoice.LocalTaxAmount.Value);
			AssertEquals("LocalWHTAmount", -localWHTAmount, XsdInvoice.LocalWHTAmount.Value);
			AssertNotEquals("OsInvoiceAmtExclTax", -osInvoiceAmtExclTax, XsdInvoice.OsInvoiceAmtExclTax.Value);
			AssertEquals("OsInvoiceAmtExclTax", -osInvoiceAmtInclTax, XsdInvoice.OsInvoiceAmtExclTax.Value);
			AssertEquals("OsInvoiceAmtInclTax", -osInvoiceAmtInclTax, XsdInvoice.OsInvoiceAmtInclTax.Value);
			AssertEquals("OsTaxAmount", 0m, XsdInvoice.OsTaxAmount.Value);
			AssertEquals("OsWHTAmount", -osWHTAmount, XsdInvoice.OsWHTAmount.Value);

			AssertEquals("LineType", ZArchitecture.Core.TransactionLineTypes.Cost, XsdInvoice.TxnLines[0].LineType.ToString());
			AssertEquals("Line[0] LocalInvoiceAmtExclTax", -localInvoiceAmtExclTaxLine1, XsdInvoice.TxnLines[0].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line[0] LocalInvoiceAmtInclTax", -localInvoiceAmtInclTaxLine1, XsdInvoice.TxnLines[0].LocalInvoiceAmtInclTax.Value);
			AssertEquals("Line[0] LocalTaxAmount", 0m, XsdInvoice.TxnLines[0].LocalTaxAmount.Value);
			AssertEquals("Line[0] LocalWHTAmount", -localWHTAmountLine1, XsdInvoice.TxnLines[0].LocalWHTAmount.Value);
			AssertEquals("Line[0] OsInvoiceAmtExclTax", -osInvoiceAmtExclTaxLine1, XsdInvoice.TxnLines[0].OsInvoiceAmtExclTax.Value);
			AssertEquals("Line[0] OsInvoiceAmtInclTax", -osInvoiceAmtInclTaxLine1, XsdInvoice.TxnLines[0].OsInvoiceAmtInclTax.Value);
			AssertEquals("Line[0] OsTaxAmount", 0m, XsdInvoice.TxnLines[0].OsTaxAmount.Value);
			AssertEquals("Line[0] OsWHTAmount", -osWHTAmountLine1, XsdInvoice.TxnLines[0].OsWHTAmount.Value);

			AssertEquals("LineType", ZArchitecture.Core.TransactionLineTypes.Cost, XsdInvoice.TxnLines[1].LineType.ToString());
			AssertEquals("Line[1] LocalInvoiceAmtExclTax", -localInvoiceAmtExclTaxLine2, XsdInvoice.TxnLines[1].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line[1] LocalInvoiceAmtInclTax", -localInvoiceAmtInclTaxLine2, XsdInvoice.TxnLines[1].LocalInvoiceAmtInclTax.Value);
			AssertEquals("Line[1] LocalTaxAmount", 0m, XsdInvoice.TxnLines[1].LocalTaxAmount.Value);
			AssertEquals("Line[1] LocalWHTAmount", -localWHTAmountLine2, XsdInvoice.TxnLines[1].LocalWHTAmount.Value);
			AssertEquals("Line[1] OsInvoiceAmtExclTax", -osInvoiceAmtExclTaxLine2, XsdInvoice.TxnLines[1].OsInvoiceAmtExclTax.Value);
			AssertEquals("Line[1] OsInvoiceAmtInclTax", -osInvoiceAmtInclTaxLine2, XsdInvoice.TxnLines[1].OsInvoiceAmtInclTax.Value);
			AssertEquals("Line[1] OsTaxAmount", 0m, XsdInvoice.TxnLines[1].OsTaxAmount.Value);
			AssertEquals("Line[1] OsWHTAmount", -osWHTAmountLine2, XsdInvoice.TxnLines[1].OsWHTAmount.Value);

			AssertEquals("LineType", ZArchitecture.Core.TransactionLineTypes.Cost, XsdInvoice.TxnLines[2].LineType.ToString());
			AssertEquals("Line[2] LocalInvoiceAmtExclTax", -localTaxAmount, XsdInvoice.TxnLines[2].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line[2] LocalInvoiceAmtInclTax", -localTaxAmount, XsdInvoice.TxnLines[2].LocalInvoiceAmtInclTax.Value);
			AssertEquals("Line[2] LocalTaxAmount", 0m, XsdInvoice.TxnLines[2].LocalTaxAmount.Value);
			AssertEquals("Line[2] LocalWHTAmount", 0m, XsdInvoice.TxnLines[2].LocalWHTAmount.Value);
			AssertEquals("Line[2] OsInvoiceAmtExclTax", -osTaxAmount, XsdInvoice.TxnLines[2].OsInvoiceAmtExclTax.Value);
			AssertEquals("Line[2] OsInvoiceAmtInclTax", -osTaxAmount, XsdInvoice.TxnLines[2].OsInvoiceAmtInclTax.Value);
			AssertEquals("Line[2] OsTaxAmount", 0m, XsdInvoice.TxnLines[2].OsTaxAmount.Value);
			AssertEquals("Line[2] OsWHTAmount", 0m, XsdInvoice.TxnLines[2].OsWHTAmount.Value);
		}

		public void TestCrossLedgerInvoiceImport()
		{
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(true);
			CrossLedgerImportTransactionTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.INV, true, true);
			CrossLedgerImportTransactionTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.CRD, true, false);
			CrossLedgerImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.INV, true, false);
			CrossLedgerImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.CRD, true, true);
		}

		[SuspendCriticalValidation]
		public void TestCrossLedgerPositiveAdjustmentImport()
		{
			CrossLedgerImportTransactionTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.ADJ, true, true);
			CrossLedgerImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.ADJ, true, false);
		}

		[SuspendCriticalValidation]
		public void TestCrossLedgerNegativeAdjustmentImport()
		{
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(true);
			CrossLedgerImportTransactionTest(Xsd.TxnLedgerType.AR, Xsd.TxnType.ADJ, false, false);
			CrossLedgerImportTransactionTest(Xsd.TxnLedgerType.AP, Xsd.TxnType.ADJ, false, true);
		}

		void CrossLedgerImportTransactionTest(Xsd.TxnLedgerType ledger, Xsd.TxnType transactionType, bool bizObjHasPositiveValues, bool xmlObjHasPositiveValues)
		{
			XsdInvoice = GetFullyPopulatedXmlInvoice_AP();

			XsdInvoice.Ledger = ledger;
			XsdInvoice.TxnType = transactionType;
			XsdInvoice.OsInvoiceAmtInclTax.CurrencyCode = ObjectCreator.USD.RX_Code;

			XsdInvoice.TxnLines[0].WHTCode = ZString.Empty;
			XsdInvoice.TxnLines[1].WHTCode = ZString.Empty;

			if (XsdInvoice.Ledger == Xsd.TxnLedgerType.AR)
			{
				XsdInvoice.TxnNumber = ZString.Empty;
			}

			if (transactionType != Xsd.TxnType.INV && transactionType != Xsd.TxnType.CRD)
			{
				XsdInvoice.TxnLines[0].ConsolOrJobNo = XsdInvoice.TxnLines[1].ConsolOrJobNo = ZString.Empty;
			}

			ZDecimal localAmtExclTax = xmlObjHasPositiveValues ? 200 : -200;
			ZDecimal localAmtInclTax = xmlObjHasPositiveValues ? 220 : -220;
			ZDecimal localTaxAmt = xmlObjHasPositiveValues ? 20 : -20;
			ZDecimal localWhtAmt = xmlObjHasPositiveValues ? 0 : -0;
			ZDecimal osAmtExclTax = xmlObjHasPositiveValues ? 100 : -100;
			ZDecimal osAmtInclTax = xmlObjHasPositiveValues ? 110 : -110;
			ZDecimal osTaxAmt = xmlObjHasPositiveValues ? 10 : -10;
			ZDecimal osWhtAmt = xmlObjHasPositiveValues ? 0 : -0;

			SetAmountsOnXmlInvoiceHeader(XsdInvoice, localAmtExclTax, localAmtInclTax, localTaxAmt, localWhtAmt, osAmtExclTax, osAmtInclTax, osTaxAmt, osWhtAmt);

			NotificationBuffer notify = new NotificationBuffer();

			Invoice = (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XsdInvoice, true));
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectCreator.AALSHI.OH_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = ObjectCreator.AALSHI.OH_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(ObjectCreator.AALSHI, new ValueObjectExportContext(notify));

			var ledgerBeforeTransform = XsdInvoice.Ledger;

			DataAdapter.ImportFromValueObject(Invoice, XsdInvoice, context);

			var ledgerAfterTransform = XsdInvoice.Ledger;
			if (ledgerBeforeTransform == Xsd.TxnLedgerType.AR && ledgerAfterTransform == Xsd.TxnLedgerType.AP)
			{
				Invoice.AH_TransactionNum = XsdInvoice.TxnNumber = Guid.NewGuid().ToString();
			}

			Factory.Save();

			AssertCommonFunctionalityForAllInvoicingBaseHeaders(Invoice, XsdInvoice);

			ZDecimal bizObjLocalAmtExclTax = bizObjHasPositiveValues ? 200 : -200;
			ZDecimal bizObjLocalAmtInclTax = bizObjHasPositiveValues ? 220 : -220;
			ZDecimal bizObjLocalTaxAmt = bizObjHasPositiveValues ? 20 : -20;
			ZDecimal bizObjLocalWhtAmt = bizObjHasPositiveValues ? 0 : 0;
			ZDecimal bizObjOsAmtExclTax = bizObjHasPositiveValues ? 100 : -100;
			ZDecimal bizObjOsAmtInclTax = bizObjHasPositiveValues ? 110 : -110;
			ZDecimal bizObjOsTaxAmt = bizObjHasPositiveValues ? 10 : -10;
			ZDecimal bizObjOsWhtAmt = bizObjHasPositiveValues ? 0 : 0;

			AssertAmountsOnInvoiceHeaderBizObj(Invoice, bizObjLocalAmtExclTax, bizObjLocalAmtInclTax, bizObjLocalTaxAmt, bizObjLocalWhtAmt,
				bizObjOsAmtExclTax, bizObjOsAmtInclTax, bizObjOsTaxAmt, bizObjOsWhtAmt);

			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[0], bizObjLocalAmtExclTax / 2, bizObjLocalAmtInclTax / 2, bizObjLocalTaxAmt / 2,
				bizObjLocalWhtAmt / 2, bizObjOsAmtExclTax / 2, bizObjOsAmtInclTax / 2, bizObjOsTaxAmt / 2, bizObjOsWhtAmt / 2);

			AssertAmountsOnInvoiceLineBizObj(Invoice.Lines[1], bizObjLocalAmtExclTax / 2, bizObjLocalAmtInclTax / 2, bizObjLocalTaxAmt / 2,
				bizObjLocalWhtAmt / 2, bizObjOsAmtExclTax / 2, bizObjOsAmtInclTax / 2, bizObjOsTaxAmt / 2, bizObjOsWhtAmt / 2);
		}

		#endregion

		#region Assert Helpers

		void AssertFinancialValueEquals(ZString description, Xsd.FinancialValue financialValue, ZDecimal expectedAmount, RefCurrency currency, Type transactionType)
		{
			AssertEquals("Amount on Financial Value: " + description, expectedAmount, financialValue.Value);
			AssertEquals("Currency on Financial Value: " + description, currency.RX_Code, financialValue.CurrencyCode);
		}

		void AssertCommonFunctionalityForAllInvoicingBaseHeaders(InvoicingBase invoice, Xsd.TxnHeader xmlTransactionHeader)
		{
			AssertEquals("Bank Code is not used for Invoices", ZString.Empty, xmlTransactionHeader.BankCode);
			AssertEquals("Cheque Drawer is not used for Invoices", ZString.Empty, xmlTransactionHeader.ChequeDrawer);
			AssertEquals("Cheque Or Reference is not used for Invoices", ZString.Empty, xmlTransactionHeader.ChequeOrReference);
			AssertEquals("Debtor Code", invoice.Header.OH_Code, xmlTransactionHeader.DebtorOrCreditor.EDICode);
			AssertEquals("Department Code", invoice.Department.GE_Code, xmlTransactionHeader.Department);
			AssertEquals("Invoice Description", invoice.AH_Desc, xmlTransactionHeader.Description);
			AssertEquals("Branch Code", invoice.Branch.GB_Code, xmlTransactionHeader.Branch);
			AssertEquals("Created User ID", "NewStaffUser", xmlTransactionHeader.CreatedUserId);
			AssertEquals("Drawer Bank", ZString.Empty, xmlTransactionHeader.DrawerBank);
			AssertEquals("Drawer Bank Branch", ZString.Empty, xmlTransactionHeader.DrawerBankBranch);
			AssertEquals("Ledger ", invoice.AH_Ledger, xmlTransactionHeader.Ledger.ToString());

			AssertEquals("Post Date", invoice.AH_PostDate, xmlTransactionHeader.PostDate);
			AssertEquals("Receipt Payment Type is not used for Invoices", Xsd.TxnHeaderReceiptPaymentType.CHQ, xmlTransactionHeader.ReceiptPaymentType); // Not applicable to invoice
			AssertEquals("Receipt Payment Type Specified (not used for Invoices)", false, xmlTransactionHeader.ReceiptPaymentTypeSpecified); // Not applicable to invoice
			if (invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable && invoice is InvoicingBase)
			{
				AssertEquals("Invoice Transaction Category", Core.Constants.TransactionCategory.Codes.Standard, invoice.AH_TransactionCategory);
			}
			else
			{
				if (invoice.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					AssertEquals("Transaction Category", invoice.AH_IsDisbursementCalc ? InvoiceTypesList.Codes.DisbursementInvoice : InvoiceTypesList.Codes.FinalInvoice, xmlTransactionHeader.TxnCategory);
				}
				else
				{
					AssertEquals("Transaction Category", invoice.AH_IsDisbursementCalc ? InvoiceTypesList.Codes.DisbursementInForeignCurrency : InvoiceTypesList.Codes.ForeignCurrencyInvoice, xmlTransactionHeader.TxnCategory);
				}
			}
			AssertEquals("Transaction Count", invoice.AH_TransactionCount.ToString(), xmlTransactionHeader.TxnCount);
			AssertEquals("Transaction Lines Count", invoice.Lines.Count, xmlTransactionHeader.TxnLines.Count);

			if (invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable)
			{
				AssertEquals("Transaction Number", invoice.AH_TransactionNum, xmlTransactionHeader.TxnNumber);
			}
			else if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable && invoice is InvoicingBase)
			{
				AssertEquals("Is Disbursement", invoice.AH_IsDisbursementCalc, xmlTransactionHeader.DisbursementFlag);
			}
			else
			{
				AssertEquals("Is Disbursement", false, invoice.AH_IsDisbursementCalc);
			}

			AssertEquals("Transaction Reference", invoice.AH_TransactionReference, xmlTransactionHeader.TxnReference);
			AssertEquals("Transaction Type", invoice.AH_TransactionType, xmlTransactionHeader.TxnType.ToString());
		}

		void AssertAmountsOnInvoiceHeaderBizObj(InvoicingBase invoiceBizObj,
			decimal localExTaxAmount, decimal localTotalAmount, decimal localTaxAmount, decimal localWHTAmount,
			decimal oSExTaxAmount, decimal oSTotalAmount, decimal oSTaxAmount, decimal oSWHTAmount)
		{
			AssertEquals("LocalExTaxAmount on BizObj InvoiceHeader", localExTaxAmount, invoiceBizObj.AH_LocalExTaxAmount);
			AssertEquals("LocalTotalAmount on BizObj InvoiceHeader", localTotalAmount, invoiceBizObj.AH_LocalTotalAmount);
			AssertEquals("LocalTaxAmount on BizObj InvoiceHeader", localTaxAmount, invoiceBizObj.AH_LocalTaxAmount);
			AssertEquals("LocalWHTAmount on BizObj InvoiceHeader", localWHTAmount, invoiceBizObj.AH_LocalWHTAmount);
			AssertEquals("OS Invoice Amount Excluding Tax on BizObj InvoiceHeader", oSExTaxAmount, invoiceBizObj.AH_OSExTaxAmount);
			AssertEquals("OS Invoice Amount Including Tax on BizObj InvoiceHeader", oSTotalAmount, invoiceBizObj.AH_OSTotalAmount);
			AssertEquals("OS Tax Amount on BizObj InvoiceHeader", oSTaxAmount, invoiceBizObj.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount on BizObj InvoiceHeader", oSWHTAmount, invoiceBizObj.AH_OSWHTAmount);
		}

		void AssertAmountsOnXmlInvoiceHeader(Xsd.TxnHeader xmlTransactionHeader,
			decimal localExTaxAmount, decimal localTotalAmount, decimal localTaxAmount, decimal localWHTAmount,
			decimal oSExTaxAmount, decimal oSTotalAmount, decimal oSTaxAmount, decimal oSWHTAmount)
		{
			AssertEquals("LocalExTaxAmount on XmlInvoice", localExTaxAmount, xmlTransactionHeader.LocalInvoiceAmtExclTax.Value);
			AssertEquals("LocalTotalAmount on XmlInvoice", localExTaxAmount + localTaxAmount, xmlTransactionHeader.LocalInvoiceAmtInclTax.Value);
			AssertEquals("LocalTaxAmount on XmlInvoice", localTaxAmount, xmlTransactionHeader.LocalTaxAmount.Value);
			AssertEquals("LocalWHTAmount on XmlInvoice", localWHTAmount, xmlTransactionHeader.LocalWHTAmount.Value);
			AssertEquals("OSInvoiceAmountExclTax on XmlInvoice", oSExTaxAmount, xmlTransactionHeader.OsInvoiceAmtExclTax.Value);
			AssertEquals("OSInvoiceAmountInclTax on XmlInvoice", oSExTaxAmount + oSTaxAmount, xmlTransactionHeader.OsInvoiceAmtInclTax.Value);
			AssertEquals("OSTaxAmount on XmlInvoice", oSTaxAmount, xmlTransactionHeader.OsTaxAmount.Value);
			AssertEquals("OSWHTAmount on XmlInvoice", oSWHTAmount, xmlTransactionHeader.OsWHTAmount.Value);
		}

		void AssertAmountsOnInvoiceLineBizObj(InvoicingLineBase invoiceLineBizObj,
			decimal localExTaxAmount, decimal localTotalAmount, decimal localTaxAmount, decimal localWHTAmount,
			decimal oSExTaxAmount, decimal oSTotalAmount, decimal oSTaxAmount, decimal oSWHTAmount)
		{
			AssertEquals("LocalExTaxAmount on BizObj TransactionLine", localExTaxAmount, invoiceLineBizObj.AL_LocalExTaxAmount);
			AssertEquals("LocalTotalAmount on BizObj TransactionLine", localExTaxAmount + localTaxAmount, invoiceLineBizObj.AL_LocalTotalAmount);
			AssertEquals("LocalTaxAmount on BizObj TransactionLine", localTaxAmount, invoiceLineBizObj.AL_LocalTaxAmount);
			AssertEquals("LocalWHTAmount on BizObj TransactionLine", localWHTAmount, invoiceLineBizObj.AL_LocalWHTAmount);
			AssertEquals("OS Invoice Amount Excluding Tax on BizObj TransactionLine", oSExTaxAmount, invoiceLineBizObj.AL_OSExTaxAmount);
			AssertEquals("OS Invoice Amount Including Tax on BizObj TransactionLine", oSExTaxAmount + oSTaxAmount, invoiceLineBizObj.AL_OverseasTotal);
			AssertEquals("OS Tax Amount on BizObj TransactionLine", oSTaxAmount, invoiceLineBizObj.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount on BizObj TransactionLine", oSWHTAmount, invoiceLineBizObj.AL_OSWHTAmount);
		}

		void AssertAmountsOnXmlInvoiceLine(Xsd.TxnLine xmlTransactionLine,
			decimal localExTaxAmount, decimal localTotalAmount, decimal localTaxAmount, decimal localWHTAmount,
			decimal oSExTaxAmount, decimal oSTotalAmount, decimal oSTaxAmount, decimal oSWHTAmount)
		{
			AssertEquals("LocalExTaxAmount on XmlInvoiceLine", localExTaxAmount, xmlTransactionLine.LocalInvoiceAmtExclTax.Value);
			AssertEquals("LocalTotalAmount on XmlInvoiceLine", localTotalAmount, xmlTransactionLine.LocalInvoiceAmtInclTax.Value);
			AssertEquals("LocalTaxAmount on XmlInvoiceLine", localTaxAmount, xmlTransactionLine.LocalTaxAmount.Value);
			AssertEquals("LocalWHTAmount on XmlInvoiceLine", localWHTAmount, xmlTransactionLine.LocalWHTAmount.Value);
			AssertEquals("OSInvoiceAmountExclTax on XmlInvoiceLine", oSExTaxAmount, xmlTransactionLine.OsInvoiceAmtExclTax.Value);
			AssertEquals("OSInvoiceAmountInclTax on XmlInvoiceLine", oSTotalAmount, xmlTransactionLine.OsInvoiceAmtInclTax.Value);
			AssertEquals("OSTaxAmount on XmlInvoiceLine", oSTaxAmount, xmlTransactionLine.OsTaxAmount.Value);
			AssertEquals("OSWHTAmount on XmlInvoiceLine", oSWHTAmount, xmlTransactionLine.OsWHTAmount.Value);
		}

		void SetAmountsOnXmlInvoiceHeader(Xsd.TxnHeader xmlTransactionHeader,
			decimal localExTaxAmount, decimal localTotalAmount, decimal localTaxAmount, decimal localWHTAmount,
			decimal oSExTaxAmount, decimal oSTotalAmount, decimal oSTaxAmount, decimal oSWHTAmount)
		{
			xmlTransactionHeader.LocalInvoiceAmtExclTax.Value = localExTaxAmount;
			xmlTransactionHeader.LocalInvoiceAmtInclTax.Value = localTotalAmount;
			xmlTransactionHeader.LocalTaxAmount.Value = localTaxAmount;
			xmlTransactionHeader.LocalWHTAmount.Value = localWHTAmount;
			xmlTransactionHeader.OsInvoiceAmtExclTax.Value = oSExTaxAmount;
			xmlTransactionHeader.OsInvoiceAmtInclTax.Value = oSTotalAmount;
			xmlTransactionHeader.OsTaxAmount.Value = oSTaxAmount;
			xmlTransactionHeader.OsWHTAmount.Value = oSWHTAmount;

			xmlTransactionHeader.TxnLines[0].LocalInvoiceAmtExclTax.Value = localExTaxAmount / 2;
			xmlTransactionHeader.TxnLines[0].LocalInvoiceAmtInclTax.Value = localTotalAmount / 2;
			xmlTransactionHeader.TxnLines[0].LocalTaxAmount.Value = localTaxAmount / 2;
			xmlTransactionHeader.TxnLines[0].LocalWHTAmount.Value = localWHTAmount / 2;
			xmlTransactionHeader.TxnLines[0].OsInvoiceAmtExclTax.Value = oSExTaxAmount / 2;
			xmlTransactionHeader.TxnLines[0].OsInvoiceAmtInclTax.Value = oSTotalAmount / 2;
			xmlTransactionHeader.TxnLines[0].OsTaxAmount.Value = oSTaxAmount / 2;
			xmlTransactionHeader.TxnLines[0].OsWHTAmount.Value = oSWHTAmount / 2;

			xmlTransactionHeader.TxnLines[1].LocalInvoiceAmtExclTax.Value = localExTaxAmount / 2;
			xmlTransactionHeader.TxnLines[1].LocalInvoiceAmtInclTax.Value = localTotalAmount / 2;
			xmlTransactionHeader.TxnLines[1].LocalTaxAmount.Value = localTaxAmount / 2;
			xmlTransactionHeader.TxnLines[1].LocalWHTAmount.Value = localWHTAmount / 2;
			xmlTransactionHeader.TxnLines[1].OsInvoiceAmtExclTax.Value = oSExTaxAmount / 2;
			xmlTransactionHeader.TxnLines[1].OsInvoiceAmtInclTax.Value = oSTotalAmount / 2;
			xmlTransactionHeader.TxnLines[1].OsTaxAmount.Value = oSTaxAmount / 2;
			xmlTransactionHeader.TxnLines[1].OsWHTAmount.Value = oSWHTAmount / 2;
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			retriever?.Dispose();
			retriever = null;
		}

		EmbeddedResourceRetriever Retriever => retriever ??= new ();
		EmbeddedResourceRetriever retriever;

		protected override InvoicingBase NewBusinessObjectFromIValueObject(IValueObject value)
		{
			if (value != null)
			{
				return (InvoicingBase)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(value as Xsd.TxnHeader));
			}
			else
			{
				return base.NewBusinessObjectFromIValueObject(null);
			}
		}

		protected override ValueObjectDataAdapter<InvoicingBase, Xsd.TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			var dataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			dataAdapter.SetXmlLineGuidToDummyString();
			return dataAdapter;
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "FinancialInvoice"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			PokePropertiesForSave();
			PopulateInvoiceBizObj(typeof(ARInvoice), 200.00M, 20.00M, 0.50M, ObjectCreator.USD);

			Invoice.AH_TransactionNum = ZString.Empty;
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;

			//Intend to create a transaction header with empty number.
			using (AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Factory.Save();
			}

			return new BusinessObjectAndExpectedOutputFileName(Invoice, Retriever.SaveResourceToFile("EmptyInvoice_Whidbey.xml"), ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			PokePropertiesForSave();
			Invoice = CreateFullyPopulatedInvoiceBizObj("ABC 0001");

			return new BusinessObjectAndExpectedOutputFileName(Invoice, Retriever.SaveResourceToFile("FullyPopulatedInvoice.xml"), ValidationKind.None, "Fully Populated Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			PokePropertiesForSave();
			PopulateInvoiceBizObj(typeof(APInvoice), 200.00M, 20.00M, 0.50M, ObjectCreator.USD, "000010003");

			return new BusinessObjectAndExpectedOutputFileName(Invoice, Retriever.SaveResourceToFile("SemiPopulatedInvoice_Whidbey.xml"), ValidationKind.None, "Semi Populated Invoice");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				// These items are never used for Invoices, Credit Notes or Adjustment Notes.
				// DebtorOrCreditor is already tested in the Organisation Data Adapter
				return new string[] { "GlAccount",
										"DrawerBank",
										"DrawerBankBranch",
										"ChequeDrawer",
										"IsFinalCharge",
										"TxnLines/IsFinalCharge",
										"ReceiptPaymentType",
										"ChequeOrReference",
										"TxnCategory",
										"BankCode",
										"ChequeBook",
										"DebtorOrCreditor/OwnerCode",
										"DebtorOrCreditor/OrganisationDetails",
										"PaidTransactions",

										//We only export these values
										"TxnLines/DepartmentActivity",
										"TxnLines/AgentsReference",
										"TxnLines/Weight",
										"TxnLines/Volume",
										"TxnLines/OSChargeAmount",
										"TxnLines/OSChargeTaxAmount",
										"TxnLines/BookingReference",
										"TxnLines/OwnerReference",
										"TxnLines/OrderReference",
										"DebtorOrCreditorGUID",
										"DebtorOrCreditor/Notes/CustomNoteTypeName",
										"DebtorOrCreditor/Notes/NoteData",
										"DebtorOrCreditor/Notes/NoteCreatedDateTime",

										//We only import these elements
										"OverrideSystemExchangeRate",
										"TxnLines/OverrideSystemExchangeRate",

										"OrderReference",
										"OwnerReference",
										"PaymentReference",

										// Tested by a dedicated test
										"Attachments/FileName",

										//Not implemented
										"TxnLines/ETA",
										"TxnLines/ETD",
										"TxnLines/RevenueRecognitionDate",
										"TxnLines/ModeOfTransport",
										"TxnLines/TaxMsgCode",
										"TxnLines/ConsolApportionmentMethod",
										"PaidTransactions/TxnLines/TaxMsgCode",

										"AmountPaidThisPayment/CurrencyCode",
										"TxnLines/eNettChargeCodeMapping",
										"ENettStoragePaymentDetails",
										"PaidTransactions",
										"PaymentReceiptBatchDate",
										"FullyPaidDate",
										"MatchStatus",
										"MatchStatusReasonCode",
										"Attachments/FilePath",
										"Attachments/DocumentType",
										"ThirdPartyReference",

										"TxnOverrideAddress",
										"TxnOverrideContact",

										//Not real node, just a flag
										"OsCurrencyEmptyFlag",
										"ShouldCreateDuringMatching",

										//We only import these during sister company invoice import.
										"TxnLines/TargetJobID",
										"TxnLines/RelatedJobID",
										"TxnLines/RelatedJobNumber",
										"TxnLines/OriginalShipmentJobNumber",
										"TxnLines/IsSplitLine",
										"TxnLines/OriginalLocalInvoiceAmtExclTax/CurrencyCode",
										"TxnLines/OriginalOsInvoiceAmtExclTax/CurrencyCode",
										"TxnLines/OriginalOsTaxAmount/CurrencyCode",
										"TxnLines/UseOriginalAmount",

										//Obsolate SubAccount element will NOT be exported. We keep it in XSD to support backward compatibility. Please use SubAccounts element instead.
										"TxnLines/SubAccount/Code",
										"TxnLines/SubAccount/Type/Code",
										"TxnLines/SubAccount/Type/Description"
									};
			}
		}

		OrgAddress overrideAddress;
		OrgContact overrideContact;

		void PopulateInvoiceBizObj(Type type, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, RefCurrency currency, string transactionNumber = null)
		{
			Invoice = (InvoicingBase)Factory.NewWithValidTestData(type);

			Invoice.AH_OH = Header.PK;
			Invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			Invoice.AH_Desc = "This is a test description to see how the XML Export works";

			if (type == typeof(ARInvoice))
			{
				Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			}

			Invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			Invoice.AH_ExchangeRate = exchangeRate;
			Invoice.AH_PostDate = PostDate;
			Invoice.AH_InvoiceDate = PostDate.AddDays(-1);
			Invoice.AH_DueDate = PostDate.AddDays(1);
			Invoice.AH_TransactionReference = @"Shipment ABC123";

			if (overrideAddress == null)
			{
				overrideAddress = TestObjectCreator.CreateAddress(Header, "112 Bourke Road");
			}

			if (overrideContact == null)
			{
				overrideContact = TestObjectCreator.CreateContact(Header, "Bourke", "Bourke@123abc.com");
			}

			Invoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			Invoice.AH_OC_InvoiceContactOverride = overrideContact.PK;

			if (transactionNumber != null)
			{
				Invoice.AH_TransactionNum = transactionNumber;
				Invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			}

			InvoicingLineBase line1 = (InvoicingLineBase)Invoice.Lines.AddNew();
			PopulateInvoiceBizObjLine(line1, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), 0.50M, Invoice.PK, currency);

			InvoicingLineBase line2 = (InvoicingLineBase)Invoice.Lines.AddNew();
			PopulateInvoiceBizObjLine(line2, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), 0.50M, Invoice.PK, currency);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, Invoice.PK);
			StmALog createLog = Factory.LoadTop1<StmALog>(filter);
			createLog.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();
		}

		void PopulateInvoiceBizObjLine(InvoicingLineBase line, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, ZGuid header, RefCurrency currency)
		{
			line.AL_AC = ChargeCodeCC1.PK;
			ObjectCreator.AttachJobToAPLine(line);
			line.AL_AH = header;
			line.AL_AT = ObjectCreator.GST1.PK;
			line.AL_AW = ObjectCreator.WHTFREE1.PK;
			line.AL_Desc = "Transaction Line Description";
			line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_PostDate = PostDate;
			line.AL_OSExTaxAmount = oSExTaxAmount;
			line.AL_OSTaxAmount = oSTaxAmount;
			ObjectCreator.AttachChargeToAPLine(line);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new FinancialInvoiceDataAdapterTestClass(false);
			SetUpBuyAndSellExchangeRateForUSD();
			TestHelper = new NotificationTestHelper();
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
		}

		void SetUpBuyAndSellExchangeRateForUSD()
		{
			RefExchangeRate uSDBuyExRate = Factory.New<RefExchangeRate>();
			uSDBuyExRate.RE_RX_NKExCurrency = ObjectCreator.USD.RX_Code;
			uSDBuyExRate.RE_StartDate = PostDate.AddDays(-2);
			uSDBuyExRate.RE_ExpiryDate = PostDate.AddDays(2);
			uSDBuyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			uSDBuyExRate.RE_SellRate = 0.5000M;
			uSDBuyExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;

			RefExchangeRate uSDSellExRate = Factory.New<RefExchangeRate>();
			uSDSellExRate.RE_RX_NKExCurrency = ObjectCreator.USD.RX_Code;
			uSDSellExRate.RE_StartDate = PostDate.AddDays(-2);
			uSDSellExRate.RE_ExpiryDate = PostDate.AddDays(2);
			uSDSellExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			uSDSellExRate.RE_SellRate = 0.5000M;
			uSDSellExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;

			Factory.Save();
		}

		void PokePropertiesForSave()
		{
			AccChargeCode cC1 = ChargeCodeCC1;
			AccChargeCode cC4 = ChargeCodeCC4;
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		InvoicingBase Invoice;
		Xsd.TxnHeader XsdInvoice;
		FinancialInvoiceDataAdapterTestClass DataAdapter;
		NotificationTestHelper TestHelper;

		#endregion

		#region Test Objects

		protected override void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample, InvoicingBase bizObjToImportTo, Xsd.TxnHeader exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
		{
			Assert(true); // Stupid test
		}

		#region FinancialInvoiceDataAdapterTestClass

		sealed class FinancialInvoiceDataAdapterTestClass : FinancialInvoiceDataAdapter
		{
			public FinancialInvoiceDataAdapterTestClass(bool isImportingMultipleInvoice)
				: base(isImportingMultipleInvoice)
			{
			}

			protected override void SetTxnLineGuid(Xsd.TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine)
			{
				if (ShouldSetXmlLineGuidToDummyString)
				{
					xmlInvoiceLine.TxnLineGUID = "lineGUID";
				}
				else
				{
					base.SetTxnLineGuid(xmlInvoiceLine, invoiceLine);
				}
			}

			protected override void DeleteTransactionAndAnythingCreatedByIt(InvoicingBase invoiceHeader)
			{
				if (NormalDeletionFunctionality)
				{
					base.DeleteTransactionAndAnythingCreatedByIt(invoiceHeader);
				}
				else
				{
					// Do nothing - allow tests to fail in their proper place rather than trying to access properties on a deleted BizObj.
				}
			}

			public void DeleteTransactionAndAnythingCreatedByItExposed(InvoicingBase invoice)
			{
				base.DeleteTransactionAndAnythingCreatedByIt(invoice);
			}

			public void SetDeleteFunctionalityToNormal()
			{
				NormalDeletionFunctionality = true;
			}

			bool NormalDeletionFunctionality;

			public void SetXmlLineGuidToDummyString() => ShouldSetXmlLineGuidToDummyString = true;

			bool ShouldSetXmlLineGuidToDummyString;

			#region Exposing protected methods

			public new Xsd.TxnHeader ExportToValueObject(InvoicingBase invoice, IValueObjectExportContext context)
			{
				return base.ExportToValueObject(invoice, context);
			}

			public new Xsd.TxnLineCollection GetTxnLineCollection(InvoicingLineBaseCollection invoiceLines, INotifications notify)
			{
				return base.GetTxnLineCollection(invoiceLines, notify);
			}

			public new void PopulateValuesForXmlInvoiceLine(Xsd.TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine, INotifications notify)
			{
				base.PopulateValuesForXmlInvoiceLine(xmlInvoiceLine, invoiceLine, notify);
			}

			public new void PopulateAgentsOwnerOrderReference(Xsd.TxnLine xmlInvoiceLine, InvoicingLineBase invoiceLine, INotifications notify)
			{
				base.PopulateAgentsOwnerOrderReference(xmlInvoiceLine, invoiceLine, notify);
			}

			public new InvoicingBase NewBusinessObject(Xsd.TxnHeader value, IValueObjectImportContext context)
			{
				return base.NewBusinessObject(value, context);
			}

			public new Xsd.TxnHeader TransformXsdForCrossLedgerImport(InvoicingBase invoiceHeader, Xsd.TxnHeader xmlInvoiceHeader, IValueObjectImportContext context)
			{
				return base.TransformXsdForCrossLedgerImport(invoiceHeader, xmlInvoiceHeader, context);
			}

			public new TransactionMatchLinkCollection GetMatchLinks(TransactionMatchLinkCollection matchLinks, BusinessObjectFactory factory)
			{
				return base.GetMatchLinks(matchLinks, factory);
			}

			public new bool MinimumRequirementsMetForNewImport(Xsd.TxnHeader xmlTxnHeader, IValueObjectImportContext context)
			{
				return base.MinimumRequirementsMetForNewImport(xmlTxnHeader, context);
			}

			#endregion
		}

		#endregion

		#region Fully Populated BusinessObject

		InvoicingBase CreateFullyPopulatedInvoiceBizObj(string transactionNumber = null) => PopulateInvoiceWithLines(true, true, transactionNumber);

		InvoicingBase FullyPopulatedInvoiceWithOneLineBizObj
		{
			get
			{
				if (fFullyPopulatedInvoiceWithOneLineBizObj == null)
				{
					fFullyPopulatedInvoiceWithOneLineBizObj = PopulateInvoiceWithLines(true);
				}

				return fFullyPopulatedInvoiceWithOneLineBizObj;
			}
		}

		InvoicingBase fFullyPopulatedInvoiceWithOneLineBizObj;

		InvoicingBase PopulateInvoiceWithLines(bool createLine1, bool createLine2 = false, string transactionNumber = null)
		{
			var orgHeader = TestObjectCreator.ABIGAS;
			var staff = TestObjectCreator.CreateStaff("TST");
			Factory.Save();

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "ABCDEFGH";
			ForwardingShipment shipment = consol.Shipments.AddNew();

			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_INCO = "FOB";
			shipment.JS_UniqueConsignRef = "S00001234";
			shipment.JS_HouseBill = "UVWXYZ";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 100.000M;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = 50.000M;
			shipment.JS_UnitOfVolume = "TN";
			shipment.JS_ActualChargeable = 80.000M;

			Job job = ObjectCreator.CreateJob(shipment);

			Factory.Save();

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			Invoice.AH_OH = Header.PK;

			Invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_GE = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaFcl.Value.GetDefaultDepartment();
			Invoice.AH_Desc = "This is a test description to see how the XML Export works";
			Invoice.AH_TransactionNum = transactionNumber ?? "000010004";
			Invoice.AH_PostDate = PostDate;
			Invoice.AH_InvoiceDate = PostDate.AddDays(-1);
			Invoice.AH_DueDate = PostDate.AddDays(1);
			Invoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			if (createLine1)
			{
				var line1 = (InvoicingLineBase)Invoice.Lines.AddNew();
				AccChargeCode chargecode = ChargeCodeCC1;
				chargecode.AC_ChargeSubGroup = "FUM";
				line1.AL_AC = chargecode.PK;
				line1.AL_JH = job.PK;
				line1.AL_GB = GlbBranch.CurrentBranch.PK;
				line1.AL_GE = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaFcl.Value.GetDefaultDepartment();
				line1.AL_Desc = "1st Line Description";
				line1.AL_OSExTaxAmount = 35.00M;
				line1.AL_AT = ObjectCreator.GST1.PK;
				line1.AL_OSTaxAmount = 3.50M;
				line1.AL_A9_VATClass = ObjectCreator.TaxMsg1.PK;
				line1.AL_Calc_InputGSTVATRecoverablePercentage = 79.25m; //it's set for one line only to test that we don't specify related xml value if AL_Calc_InputGSTVATRecoverablePercentage is not changed (equal 100)

				ObjectCreator.CreateJobCharge(line1, job, chargecode, ObjectCreator.AUD);
			}

			if (createLine2)
			{
				var line2 = (InvoicingLineBase)Invoice.Lines.AddNew();

				line2.AL_GB = GlbBranch.CurrentBranch.PK;
				line2.AL_GE = AccountingConfigurationRegistry.Instance.JobInvoicingDefaultDepartmentForwardingImportSeaFcl.Value.GetDefaultDepartment();
				line2.AL_Desc = "2nd Line Description";
				AccGLHeader gLHeader = Factory.LoadFromUniqueKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10"));
				gLHeader.AG_DisallowDirectPosting = false;
				line2.AL_AG = gLHeader.PK;

				line2.AL_OSExTaxAmount = 100.00M;
				line2.AL_AT = ObjectCreator.GST1.PK;
				line2.AL_OSTaxAmount = 10.00M;
				line2.AL_AW = ObjectCreator.WHT1.PK;
				line2.AL_OSWHTAmount = 5.00M;
				line2.AL_A9_VATClass = ObjectCreator.TaxMsg2.PK;

				AssertEquals("Precondition: Line with Sub Accounts should not link to a Job", ZGuid.Empty, line2.AL_JH);

				line2.SubAccounts.Add(TestObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line2.PK, Core.Constants.SubAccountType.Organization, orgHeader.PK));
				line2.SubAccounts.Add(TestObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line2.PK, Core.Constants.SubAccountType.StaffAndResources, staff.PK));
			}

			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, Invoice.PK);
			StmALog createLog = Factory.LoadTop1<StmALog>(filter);
			createLog.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;

			if (overrideAddress == null)
			{
				overrideAddress = TestObjectCreator.CreateAddress(Header, "112 Bourke Road");
			}

			if (overrideContact == null)
			{
				overrideContact = TestObjectCreator.CreateContact(Header, "Bourke", "Bourke@123abc.com");
			}

			Invoice.AH_OA_InvoiceAddressOverride = overrideAddress.PK;
			Invoice.AH_OC_InvoiceContactOverride = overrideContact.PK;

			Factory.Save();

			return Invoice;
		}

		#endregion

		#region Fully Populated Xml Object

		Xsd.TxnHeader GetFullyPopulatedXmlInvoice_AP()
		{
			return GetFullyPopulatedXmlInvoice(typeof(APInvoice));
		}

		protected Xsd.TxnHeader GetFullyPopulatedXmlInvoice_AR()
		{
			return GetFullyPopulatedXmlInvoice(typeof(ARInvoice));
		}

		Xsd.TxnHeader GetFullyPopulatedXmlInvoice(Type type)
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			Xsd.TxnHeader fFullyPopulatedXmlInvoice;

			fFullyPopulatedXmlInvoice = new Xsd.TxnHeader();
			fFullyPopulatedXmlInvoice.BankCode = ZString.Empty;
			fFullyPopulatedXmlInvoice.Branch = GlbBranch.CurrentBranch.GB_Code;
			fFullyPopulatedXmlInvoice.CashBasisTaxIndicator = Xsd.TxnHeaderCashBasisTaxIndicator.N;
			fFullyPopulatedXmlInvoice.ChequeDrawer = ZString.Empty;
			fFullyPopulatedXmlInvoice.ChequeOrReference = ZString.Empty;
			fFullyPopulatedXmlInvoice.CreatedUserId = GlbStaff.CurrentUser.GS_LoginName;
			fFullyPopulatedXmlInvoice.Department = "FEA";
			fFullyPopulatedXmlInvoice.Description = "Description of Transaction";
			fFullyPopulatedXmlInvoice.DrawerBankBranch = ZString.Empty;
			fFullyPopulatedXmlInvoice.DisbursementFlag = true;
			fFullyPopulatedXmlInvoice.DisbursementFlagSpecified = true;
			fFullyPopulatedXmlInvoice.DrawerBank = ZString.Empty;
			fFullyPopulatedXmlInvoice.DueDate = PostDate.AddDays(10);
			fFullyPopulatedXmlInvoice.GlAccount = ZString.Empty;
			fFullyPopulatedXmlInvoice.InvoiceDate = PostDate.AddDays(-10);
			fFullyPopulatedXmlInvoice.InvTerm = "INV";
			fFullyPopulatedXmlInvoice.InvTermDays = "20";

			if (type == typeof(APInvoice))
			{
				fFullyPopulatedXmlInvoice.TxnNumber = "00001000";
				fFullyPopulatedXmlInvoice.JobInvoiceNo = "S000010000/A";
				fFullyPopulatedXmlInvoice.TxnType = Xsd.TxnType.INV;
				fFullyPopulatedXmlInvoice.Ledger = Xsd.TxnLedgerType.AP;
			}
			else
			{
				fFullyPopulatedXmlInvoice.TxnType = Xsd.TxnType.INV;
				fFullyPopulatedXmlInvoice.Ledger = Xsd.TxnLedgerType.AR;
			}

			SetOrganisation(fFullyPopulatedXmlInvoice);
			fFullyPopulatedXmlInvoice.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, GlbCompany.CurrentCompany.LocalCurrency, type);
			fFullyPopulatedXmlInvoice.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(220M, GlbCompany.CurrentCompany.LocalCurrency, type);
			fFullyPopulatedXmlInvoice.LocalTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(20.0M, GlbCompany.CurrentCompany.LocalCurrency, type);
			fFullyPopulatedXmlInvoice.LocalWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(0.0M, GlbCompany.CurrentCompany.LocalCurrency, type);
			fFullyPopulatedXmlInvoice.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(400M, GlbCompany.CurrentCompany.LocalCurrency, type);
			fFullyPopulatedXmlInvoice.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(440M, GlbCompany.CurrentCompany.LocalCurrency, type);
			fFullyPopulatedXmlInvoice.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(40.0M, GlbCompany.CurrentCompany.LocalCurrency, type);
			fFullyPopulatedXmlInvoice.OsWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(20.0M, GlbCompany.CurrentCompany.LocalCurrency, type);
			fFullyPopulatedXmlInvoice.PostDate = PostDate;
			fFullyPopulatedXmlInvoice.ReceiptPaymentTypeSpecified = false;
			if (type == typeof(ARInvoice))
			{
				fFullyPopulatedXmlInvoice.TxnCategory = "DBT";
			}
			fFullyPopulatedXmlInvoice.TxnCount = "1";
			fFullyPopulatedXmlInvoice.TxnReference = "TransactionReference";

			fFullyPopulatedXmlInvoice.TxnLines = new Xsd.TxnLineCollection();

			Xsd.TxnLine invoiceLine1 = fFullyPopulatedXmlInvoice.TxnLines.AddNew();

			if (type == typeof(APInvoice))
			{
				invoiceLine1.ChargeCode = ChargeCodeCC1.AC_Code;
				invoiceLine1.ChargeGroup = ChargeCodeCC1.AC_ChargeGroup;
				invoiceLine1.ChargeSubGroup = ChargeCodeCC1.AC_ChargeSubGroup;
				invoiceLine1.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
				invoiceLine1.Incoterm = "FOB";
				invoiceLine1.LineType = Xsd.TxnLineLineType.CST;
			}
			else
			{
				invoiceLine1.ChargeCode = ChargeCodeCC4.AC_Code;
				invoiceLine1.ChargeGroup = ChargeCodeCC4.AC_ChargeGroup;
				invoiceLine1.ChargeSubGroup = ChargeCodeCC4.AC_ChargeSubGroup;
				invoiceLine1.LineType = Xsd.TxnLineLineType.REV;
			}

			invoiceLine1.ModeOfTransport = Xsd.TransportMode.SEA;
			invoiceLine1.Branch = GlbBranch.CurrentBranch.GB_Code;
			invoiceLine1.ConsolOrJobTypeSpecified = false;
			invoiceLine1.Department = "FEA";
			invoiceLine1.Description = "InvoiceLine1Description";
			invoiceLine1.DestinationPortCode = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			invoiceLine1.GLAccount = ZString.Empty;
			invoiceLine1.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(100M, GlbCompany.CurrentCompany.LocalCurrency, type);
			invoiceLine1.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(110M, GlbCompany.CurrentCompany.LocalCurrency, type);
			invoiceLine1.LocalTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(10M, GlbCompany.CurrentCompany.LocalCurrency, type);
			invoiceLine1.LocalWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(0M, GlbCompany.CurrentCompany.LocalCurrency, type);
			invoiceLine1.OriginPortCode = Xsd.UNLOCO.FromPortCode(Factory, "NZAKL");
			invoiceLine1.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, type);
			invoiceLine1.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(220M, ObjectCreator.USD, type);
			invoiceLine1.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(20M, ObjectCreator.USD, type);
			invoiceLine1.OsWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(10M, ObjectCreator.USD, type);
			invoiceLine1.Sequence = "1";
			invoiceLine1.TaxCode = ObjectCreator.GST1.AT_Code;
			invoiceLine1.WHTCode = ObjectCreator.WHT1.AW_Code;

			Xsd.TxnLine invoiceLine2 = fFullyPopulatedXmlInvoice.TxnLines.AddNew();

			if (type == typeof(APInvoice))
			{
				invoiceLine2.ChargeCode = ChargeCodeCC1.AC_Code;
				invoiceLine2.ChargeGroup = ChargeCodeCC1.AC_ChargeGroup;
				invoiceLine2.ChargeSubGroup = ChargeCodeCC1.AC_ChargeSubGroup;
				invoiceLine2.ConsolOrJobNo = Shipment2.JS_UniqueConsignRef;
				invoiceLine2.Incoterm = "FOB";
				invoiceLine2.LineType = Xsd.TxnLineLineType.CST;
			}
			else
			{
				invoiceLine2.ChargeCode = ChargeCodeCC4.AC_Code;
				invoiceLine2.ChargeGroup = ChargeCodeCC4.AC_ChargeGroup;
				invoiceLine2.ChargeSubGroup = ChargeCodeCC4.AC_ChargeSubGroup;
				invoiceLine2.LineType = Xsd.TxnLineLineType.REV;
			}

			invoiceLine2.Branch = GlbBranch.CurrentBranch.GB_Code;
			invoiceLine2.ConsolOrJobTypeSpecified = false;
			invoiceLine2.Department = "FEA";
			invoiceLine2.Description = "InvoiceLine2Description";
			invoiceLine2.DestinationPortCode = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			invoiceLine2.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(100M, GlbCompany.CurrentCompany.LocalCurrency, type);
			invoiceLine2.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(110M, GlbCompany.CurrentCompany.LocalCurrency, type);
			invoiceLine2.LocalTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(10M, GlbCompany.CurrentCompany.LocalCurrency, type);
			invoiceLine2.LocalWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(0M, GlbCompany.CurrentCompany.LocalCurrency, type);
			invoiceLine2.OriginPortCode = Xsd.UNLOCO.FromPortCode(Factory, "NZAKL");
			invoiceLine2.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(200M, ObjectCreator.USD, type);
			invoiceLine2.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(220M, ObjectCreator.USD, type);
			invoiceLine2.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(20M, ObjectCreator.USD, type);
			invoiceLine2.OsWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(10M, ObjectCreator.USD, type);
			invoiceLine2.Sequence = "2";
			invoiceLine2.TaxCode = ObjectCreator.GST1.AT_Code;
			invoiceLine2.WHTCode = ObjectCreator.WHT1.AW_Code;

			Factory.Save();

			return fFullyPopulatedXmlInvoice;
		}

		void SetOrganisation(Xsd.TxnHeader xmlInvoice)
		{
			xmlInvoice.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(
				xmlInvoice.Ledger == Xsd.TxnLedgerType.AP ? ObjectCreator.AALSHI : ObjectCreator.ABIGAS,
				new ValueObjectExportContext(new NotificationBuffer()));

			ObjectCreator.AALSHI.CompanyData.SetAPTaxApplicable(true);
			ObjectCreator.ABIGAS.CompanyData.SetARTaxApplicable(true);
		}

		#endregion

		#region Consol with 2 Shipments

		ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					CreateAConsolWith2Shipments();
				}

				return fConsol;
			}
		}
		ForwardingConsol fConsol;

		ForwardingShipment Shipment1
		{
			get
			{
				if (fShipment1 == null)
				{
					CreateAConsolWith2Shipments();
				}

				return fShipment1;
			}
		}

		ForwardingShipment fShipment1;

		ForwardingShipment Shipment2
		{
			get
			{
				if (fShipment2 == null)
				{
					CreateAConsolWith2Shipments();
				}

				return fShipment2;
			}
		}

		ForwardingShipment fShipment2;

		void CreateAConsolWith2Shipments()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			fConsol = testDataFactory.NewWithValidTestData<ForwardingConsol>();
			fConsol.JK_UniqueConsignRef = "C00002000";
			fConsol.JK_MasterBillNum = "NEWCONSOL";

			fShipment1 = Consol.Shipments.AddNew();
			fShipment1.JS_TransportMode = "SEA";
			fShipment1.JS_INCO = "FOB";
			fShipment1.JS_UniqueConsignRef = "S00001100";
			fShipment1.JS_HouseBill = "UVWXYZ";
			fShipment1.JS_RL_NKOrigin = "AUSYD";
			fShipment1.JS_RL_NKDestination = "USLAX";
			fShipment1.JS_ActualChargeable = 100M;

			fShipment2 = Consol.Shipments.AddNew();
			fShipment2.JS_TransportMode = "SEA";
			fShipment2.JS_INCO = "FOB";
			fShipment2.JS_UniqueConsignRef = "S00001101";
			fShipment2.JS_HouseBill = "ABCDEF";
			fShipment2.JS_RL_NKOrigin = "AUSYD";
			fShipment2.JS_RL_NKDestination = "USLAX";
			fShipment2.JS_ActualChargeable = 100M;

			new JobHeader.Loader(testDataFactory, fShipment2).TryCreateWithoutMutexForTestOnly();
			new JobHeader.Loader(testDataFactory, fShipment1).TryCreateWithoutMutexForTestOnly();

			testDataFactory.Save();
		}

		#endregion

		#endregion
	}
}
