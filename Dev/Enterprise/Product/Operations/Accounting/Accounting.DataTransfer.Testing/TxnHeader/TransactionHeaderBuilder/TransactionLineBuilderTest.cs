using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class TransactionLineBuilderTest : TestCaseWithFactory
	{
		public void TestAddTransactionLineToInvoiceBusinessObject_TaxErrors()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			TestObjectCreator.FillInvoiceWithMinimumTestData(Invoice);
			ObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;
			Factory.Save();

			TxnLine.TaxCode = ObjectCreator.GSTFREE1.AT_Code;
			TxnLine.OsInvoiceAmtExclTax.Value = 200M;
			TxnLine.OsTaxAmount.Value = 100M;
			TxnLine.TaxMsgCode = ObjectCreator.TaxMsg1.A9_Code;

			AssertEquals("Precondition: Invoice line count", 0, Invoice.Lines.Count);
			AssertEquals("Precondition: notifications", "", Notify.AsString);
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			AssertEquals("Invoice line count", 1, Invoice.Lines.Count);
			AssertMultilineASCIIEquals("Error: Line Tax Amount cannot be set if Tax Rate is zero\r\n", Notify.AsString);

			Notify.Clear();
			TxnLine.TaxCode = "";

			AssertEquals("Precondition: notifications", "", Notify.AsString);
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			AssertEquals("Invoice line count", 2, Invoice.Lines.Count);
			AssertMultilineASCIIEquals("Error: Line Tax Amount cannot be set if there is no Tax Code\r\n", Notify.AsString);
		}

		public void TestConvertToNotReportableTaxIDUsesLineTaxRate()
		{
			var line = Factory.New<APInvoiceLine>();
			var taxId = Factory.New<AccTaxRate>();
			line.AL_AT = taxId.PK;
			TransactionLineBuilder.ConvertToNotReportableTaxID(line);
			AssertEquals(taxId.PK, line.AL_AT);

			line.AL_TaxRateNumerator = 1;
			TransactionLineBuilder.ConvertToNotReportableTaxID(line);
			AssertNotEquals(taxId.PK, line.AL_AT);
			AssertEquals("NOTREPORT", line.TaxRate.AT_Code);
		}

		public void TestImportTxnLineWhenCreditorIsGSTRegistered()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			TestObjectCreator.FillInvoiceWithMinimumTestData(Invoice);
			ObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(true);
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;
			Factory.Save();

			TxnLine.TaxCode = ObjectCreator.GST1.AT_Code;
			TxnLine.OsInvoiceAmtExclTax.Value = 200M;
			TxnLine.OsTaxAmount.Value = 100M;
			TxnLine.TaxMsgCode = ObjectCreator.TaxMsg1.A9_Code;

			AssertEquals("Invoice line not yet created", 0, Invoice.Lines.Count);

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals("Invoice line created", 1, Invoice.Lines.Count);

			AssertEquals("MarkForWarningAsCreditorIsNotTaxRegisteredForTaxedTransaction", false, Invoice.Lines[0].MarkForWarningAsCreditorOrLoginCompanyIsNotTaxRegisteredForTaxedTransaction);
			AssertNoRowWarningContaining(Invoice.Lines[0], "Line Tax Amount has been re-calculated because current login company is not tax registered and/or Creditor is not tax applicable");

			AssertEquals("Re-calculated Exc Tax ", -200M, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", -100M, Invoice.Lines[0].AL_OSTaxAmount);
			AssertEquals("Total Amount", 300M, Invoice.Lines[0].AL_OSAmount);
			AssertEquals("Total Amount", 300M, Invoice.Lines[0].AL_OSAmount);
			AssertEquals("Tax Message GUID", ObjectCreator.TaxMsg1.PK, Invoice.Lines[0].AL_A9_VATClass);
		}

		public void TestImportTxnLineWhenCreditorIsNotGSTRegistered()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			ObjectCreator.ABIGAS.CompanyData.SetAPTaxApplicable(false);
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;

			TxnLine.TaxCode = ObjectCreator.GST1.AT_Code;
			TxnLine.OsInvoiceAmtExclTax.Value = 200M;
			TxnLine.OsTaxAmount.Value = 100M;
			TxnLine.TaxMsgCode = ObjectCreator.TaxMsg1.A9_Code;

			AssertEquals("Invoice line not yet created", 0, Invoice.Lines.Count);

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals("Invoice line created", 1, Invoice.Lines.Count);

			Assert("MarkForWarningAsCreditorIsNotTaxRegisteredForTaxedTransaction", Invoice.Lines[0].MarkForWarningAsCreditorOrLoginCompanyIsNotTaxRegisteredForTaxedTransaction);
			AssertHasRowWarning(Invoice.Lines[0], "Line Tax Amount has been re-calculated because current login company is not tax registered and/or Creditor is not tax applicable");

			AssertEquals("Re-calculated Exc Tax ", -300M, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0M, Invoice.Lines[0].AL_OSTaxAmount);
			AssertEquals("Total Amount", 300M, Invoice.Lines[0].AL_OSAmount);
			AssertEquals("Tax Message GUID", ZGuid.Empty, Invoice.Lines[0].AL_A9_VATClass);
		}

		public void TestImportTxnLineWhenChargeIsComment()
		{
			var commentCharge = ObjectCreator.CommentChargeCode;
			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;
			Factory.Save();

			TxnLine.OsInvoiceAmtExclTax.Value = 200M;
			SetXMLInvoiceLineChargeCode(TxnLine, commentCharge.AC_Code);

			AssertEquals("Invoice line not yet created", 0, Invoice.Lines.Count);

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AR INV 00001000: Line 1: ");

			AssertEquals("Invoice line not created", 1, Invoice.Lines.Count);
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Error: Line amount cannot be set if there is a comment charge entered");
		}

		public void TestChargeCodeOrganisationMapping()
		{
			AccChargeCode chargeCodeThatDoesExist = ObjectCreator.CC1;
			AccChargeCode chargeCodeToMapTo = ObjectCreator.CC2;
			Job job = ObjectCreator.CreateJob(Shipment1);
			OrgHeader org = GetOrgForChargeCodeMappingTest();

			GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
			globalChargeCode.YG_OH = org.PK;
			globalChargeCode.YG_Code = chargeCodeThatDoesExist.AC_Code;
			GlobalChargeCodeMapPivotOrganization pivot = globalChargeCode.PivotCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			pivot.YP_AC = chargeCodeToMapTo.PK;

			Factory.Save();

			SetXMLInvoiceLineChargeCode(TxnLine, chargeCodeThatDoesExist.AC_Code);
			TxnLine.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
			TxnLine.Department = "FIA";
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 1, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
		}

		public void TestTransactionLineDescriptionForIntercompanyImport()
		{
			var converter = new UnapprovedTransactionConverter(Factory);
			var currentCompany = GlbCompany.CurrentCompany;
			var efee = ObjectCreator.CreateGlobalChargeCode("ZZEFEE");
			efee.AC_GC = ZGuid.Empty;
			efee.AC_Code = "ZZEFEE";
			efee.AC_Desc = "EFEE global";
			efee.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			var efee2 = ObjectCreator.CreateChargeCode(
				"EFEE", "EFEE currentCompany", Core.Constants.ChargeType.Disbursement, 100, null, null, currentCompany);
			var efee3 = ObjectCreator.CreateChargeCode(
				"EFEE", "EFEE sisterCompany", Core.Constants.ChargeType.Disbursement, 100, null, null, localCompany);
			Factory.Save();

			ARInvoice sisterCompanyARInvoice;
			using (new TemporaryUserContext { BranchPK = localBranch.PK.ToGuid() }.Set())
			{
				var shipment = Factory.New<ForwardingShipment>();
				var job = ObjectCreator.CreateJob(shipment, false);
				sisterCompanyARInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("100000", ObjectCreator.AUD, 1m, currentCompany.OrgProxy);
				sisterCompanyARInvoice.AH_JH = job.PK;
				var line = ObjectCreator.CreateARInvoiceLine(sisterCompanyARInvoice, job, efee3, ObjectCreator.AUD, 1.0m, "ree line", 100);
				var jobCharge = job.Charges.AddNew();
				jobCharge.JR_AC = efee3.PK;
				jobCharge.JR_GB = localBranch.PK;
				jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
				jobCharge.JR_RX_NKSellCurrency = ObjectCreator.AUD.RX_Code;
				jobCharge.JR_OSSellAmt = 100;
				jobCharge.JR_OSSellExRate = 1;
				jobCharge.JR_LocalSellAmt = 100;
				jobCharge.JR_AL_ARLine = line.PK;
				jobCharge.JR_Desc = "desc sister company";
				Factory.Save();
			}

			var apInvoice = (APInvoice)converter.ConvertToAPUnsafe(sisterCompanyARInvoice, Factory, false);
			var alDescList = apInvoice.Lines.Cast<AccTransactionLines>().Select(x => x.AL_Desc).ToList();
			apInvoice.ReleaseAllMutexOnInvoice();

			Assert(alDescList.Contains("EFEE currentCompany"));
			using (AccountingConfigurationRegistry.Instance.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting.SetTemporaryValue(localCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, efee.PK.ToGuid()))
			{
				apInvoice = (APInvoice)converter.ConvertToAPUnsafe(sisterCompanyARInvoice, Factory, true);
				var alDesc = apInvoice.Lines.Cast<AccTransactionLines>().Select(x => x.AL_Desc).ToList();
				Assert(alDesc.Contains("desc sister company"));
			}
			
			apInvoice.ReleaseAllMutexOnInvoice();
		}

		public void TestCarryFarwordOriginalChargeDescriptionIsNotAppliedWhenCreateIntercompanyChargeCodeMapping()
		{
			var chargeCodeThatDoesExist = ObjectCreator.CC1;
			chargeCodeThatDoesExist.AC_GC = GlbCompany.CurrentCompany.PK;
			var chargeCodeToMapTo = ObjectCreator.CC2;
			chargeCodeToMapTo.AC_GC = localCompany.PK;
			var shipment = Factory.New<ForwardingShipment>();
			var job = ObjectCreator.CreateJob(shipment, false);
			var invoice = ObjectCreator.CreateARInvoice<ARInvoice>("100000", ObjectCreator.AUD, 1m, localCompany.OrgProxy);
			invoice.AH_JH = job.PK;
			var line = ObjectCreator.CreateARInvoiceLine(invoice, job, chargeCodeThatDoesExist, ObjectCreator.AUD, 1.0m, "ree line", 100);
			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = chargeCodeThatDoesExist.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_RX_NKSellCurrency = ObjectCreator.AUD.RX_Code;
			jobCharge.JR_OSSellAmt = 100;
			jobCharge.JR_OSSellExRate = 1;
			jobCharge.JR_LocalSellAmt = 100;
			jobCharge.JR_AL_ARLine = line.PK;
			jobCharge.JR_AT_SellGSTRate = ObjectCreator.GST1.PK;
			jobCharge.JR_Desc = "desc sister company";

			var globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			var aPPivot = globalChargeCode.PivotCollection.AddNew();
			aPPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			aPPivot.YP_AC = chargeCodeToMapTo.PK;
			var aRPivot = globalChargeCode.PivotCollection.AddNew();
			aRPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			aRPivot.YP_AC = chargeCodeThatDoesExist.PK;

			Factory.Save();
			var config = new TransactionBuilderConfig();
			TxnLine.ChargeCode = chargeCodeThatDoesExist.AC_Code;
			TxnLine.TxnLineGUID = line.PK.ToString();

			config.UseChargeDescAsLineDesc = true;
			Builder = new TransactionLineBuilder(new NotificationManager(Notify), config);
			var apInvoice = Factory.New<APInvoice>();
			apInvoice.AH_OH = invoice.Company.OrgProxy.PK;
			apInvoice.AH_TransactionNum = "k00012";
			
			using (new TemporaryUserContext { BranchPK = localBranch.PK.ToGuid() }.Set())
			{
				Builder.AddTransactionLineToInvoiceBusinessObject(apInvoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");
			}
			Assert(!config.UseChargeDescAsLineDesc);
			AssertEquals(chargeCodeToMapTo.AC_Desc, apInvoice.Lines.Cast<AccTransactionLines>().First().AL_Desc);
		}

		public void TestChargeCodeIntercompanyMapping()
		{
			AccChargeCode chargeCodeThatDoesExist = ObjectCreator.CC1;
			AccChargeCode chargeCodeToMapTo = ObjectCreator.CC2;
			Job job = ObjectCreator.CreateJob(Shipment1);
			OrgHeader org = GetOrgForChargeCodeMappingTest();
			GlbCompany aRCompany = Factory.NewWithValidTestData<GlbCompany>();
			aRCompany.GC_OH_OrgProxy = org.PK;
			GlbBranch aRCompanyBranch = aRCompany.Branches.AddNew();
			aRCompanyBranch.GB_OH_OrgProxy = ObjectCreator.ZECTRA.PK;
			chargeCodeThatDoesExist.AC_GC = aRCompany.PK;

			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompany aPPivot = globalChargeCode.PivotCollection.AddNew();
			aPPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			aPPivot.YP_AC = chargeCodeToMapTo.PK;
			GlobalChargeCodeMapPivotIntercompany aRPivot = globalChargeCode.PivotCollection.AddNew();
			aRPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			aRPivot.YP_AC = chargeCodeThatDoesExist.PK;

			Factory.Save();

			SetXMLInvoiceLineChargeCode(TxnLine, chargeCodeThatDoesExist.AC_Code);
			TxnLine.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
			TxnLine.Department = "FIA";
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 1, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);

			Invoice = Factory.NewWithValidTestData<APInvoice>();
			Invoice.AH_OH = ObjectCreator.ZECTRA.PK;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 1, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
		}

		public void TestChargeCodeIntercompanyMappingWithLocalClientOverride()
		{
			AccChargeCode chargeCodeThatDoesExist = ObjectCreator.CC1;
			AccChargeCode chargeCodeToMapTo = ObjectCreator.CC2;
			AccChargeCode chargeCodeToMapToWithLocalClientOverride = ObjectCreator.CC3;
			Job job = ObjectCreator.CreateJob(Shipment1);

			OrgHeader org = GetOrgForChargeCodeMappingTest();
			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress clientAddress = localClient.Addresses.AddNew(OrgAddressType.Office, true);
			job.JH_OA_LocalChargesAddr = clientAddress.PK;

			GlbCompany aRCompany = Factory.NewWithValidTestData<GlbCompany>();
			aRCompany.GC_OH_OrgProxy = org.PK;
			GlbBranch aRCompanyBranch = aRCompany.Branches.AddNew();
			aRCompanyBranch.GB_OH_OrgProxy = ObjectCreator.ZECTRA.PK;
			chargeCodeThatDoesExist.AC_GC = aRCompany.PK;

			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			GlobalChargeCodeMapPivotIntercompany aPPivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			aPPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			aPPivot.YP_AC = chargeCodeToMapTo.PK;

			GlobalChargeCodeMapPivotIntercompany aPPivotWithLocalClientOverride = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			aPPivotWithLocalClientOverride.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			aPPivotWithLocalClientOverride.YP_AC = chargeCodeToMapToWithLocalClientOverride.PK;
			aPPivotWithLocalClientOverride.YP_OH_LocalClientOverride = localClient.PK;

			GlobalChargeCodeMapPivotIntercompany aRPivot = globalChargeCode.PivotCollection.AddNew();
			aRPivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			aRPivot.YP_AC = chargeCodeThatDoesExist.PK;
			Factory.Save();

			SetXMLInvoiceLineChargeCode(TxnLine, chargeCodeThatDoesExist.AC_Code);
			TxnLine.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
			TxnLine.Department = "FIA";
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");
			AssertEquals("TransactionHeader Lines Count", 1, Invoice.Lines.Count);
			AssertEquals("Expect to use local client override mapping charge code where there is one", ObjectCreator.CC3.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);

			OrgHeader anotherClient = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress anotherClientAddress = anotherClient.Addresses.AddNew(OrgAddressType.Office, true);
			job.JH_OA_LocalChargesAddr = anotherClientAddress.PK;
			Invoice = Factory.NewWithValidTestData<APInvoice>();
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");
			AssertEquals("TransactionHeader Lines Count", 1, Invoice.Lines.Count);
			AssertEquals("Expect to use default mapping charge code when no local client override mapping exists", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
		}

		public void TestAddTransactionLineToDirectReceiptPaymentBusinessObject()
		{
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			gLHeader.AG_AccountNum = "1111.11.11";
			gLHeader.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			gLHeader.AG_DisallowDirectPosting = false;
			Factory.Save();
			AssertNotNull("Precondition: GL Account should exist", gLHeader);

			TxnLine.LineType = Xsd.TxnLineLineType.DPY;
			TxnLine.GLAccount = gLHeader.AG_AccountNum;
			TxnLine.Branch = GlbBranch.CurrentBranch.GB_Code;
			TxnLine.Department = GlbDepartment.CurrentDepartment.GE_Code;
			TxnLine.OsInvoiceAmtExclTax.Value = 1000m;
			TxnLine.TaxCode = ObjectCreator.GST1.AT_Code;
			TxnLine.OsTaxAmount.Value = 100m;
			TxnLine.TaxMsgCode = ObjectCreator.TaxMsg1.A9_Code;

			DirectPayment payment = Factory.New<DirectPayment>();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());

			Builder.AddTransactionLineToDirectReceiptPaymentBusinessObject(payment, TxnLine, importContext, ZString.Empty);

			AssertEquals("TransactionHeader Lines Count", 1, payment.Lines.Count);
			DirectPaymentLine newLine = (DirectPaymentLine)payment.Lines[0];
			AssertEquals("GL Account", TxnLine.GLAccount, newLine.GLHeader.AG_AccountNum);
			AssertEquals("Branch", TxnLine.Branch, newLine.Branch.GB_Code);
			AssertEquals("Department", TxnLine.Department, newLine.Department.GE_Code);
			AssertEquals("OSExTaxAmount", TxnLine.OsInvoiceAmtExclTax.Value, newLine.AL_OSExTaxAmount);
			AssertEquals("Tax Code", TxnLine.TaxCode, newLine.TaxRate.AT_Code);
			AssertEquals("OSTaxAmount", TxnLine.OsTaxAmount.Value, newLine.AL_OSTaxAmount);
			AssertEquals("Tax Message Code", TxnLine.TaxMsgCode, newLine.VATClass.A9_Code);
		}

		public void TestImportValidChargeCode()
		{
			ZString chargeCodeThatDoesExist = ObjectCreator.CC1.AC_Code;
			Job job = ObjectCreator.CreateJob(Shipment1);
			Factory.Save();

			ZQuery chargeCodeFilter = new ZQuery(ViewGenericChargeSchema.VC_Code, chargeCodeThatDoesExist);
			chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
			chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, ZBool.False);
			AssertEquals("Precondition: Charge Code exists", 1, Factory.GetDatabaseCount(typeof(GenericCharge), chargeCodeFilter));

			TxnLine.ChargeCode = chargeCodeThatDoesExist;
			TxnLine.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
			TxnLine.Department = "FIA";

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 1, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC1.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
		}

		public void TestImportValidChargeCodeWhenMappingExistsForCreditor()
		{
			AssertChargeCodeMappingForOrganisation();
		}

		protected void AssertChargeCodeMappingForOrganisation()
		{
			ZString chargeCodeThatDoesExist = ObjectCreator.CC1.AC_Code;
			ZString chargeCodeToMapTo = ObjectCreator.CC2.AC_Code;
			Job job = ObjectCreator.CreateJob(Shipment1);
			OrgHeader org = GetOrgForChargeCodeMappingTest();
			OrgPatternMatchOverride patternMatchOverride = org.CreatePatternMatchOverrideForTest();
			patternMatchOverride.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			patternMatchOverride.OO_LocalCode = chargeCodeToMapTo;
			patternMatchOverride.OO_ForeignCode = chargeCodeThatDoesExist;

			Factory.Save();

			ZQuery chargeCodeFilter = new ZQuery(ViewGenericChargeSchema.VC_Code, chargeCodeThatDoesExist);
			chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
			chargeCodeFilter.AddToFilter(ViewGenericChargeSchema.VC_IsGLAccount, ZBool.False);
			AssertEquals("Precondition: Charge Code exists", 1, Factory.GetDatabaseCount(typeof(GenericCharge), chargeCodeFilter));

			SetXMLInvoiceLineChargeCode(TxnLine, chargeCodeThatDoesExist);
			TxnLine.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
			TxnLine.Department = "FIA";
			Invoice.AH_OH = ObjectCreator.ABIGAS.PK;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1:");

			AssertEquals("TransactionHeader Lines Count", 1, Invoice.Lines.Count);
			AssertNotNull("TransactionHeader Line 1: Charge Code should not be null", Invoice.Lines[0].ChargeCode);
			AssertEquals("TransactionHeader Line 1: Charge Code", ObjectCreator.CC2.AC_Code, Invoice.Lines[0].ChargeCode.AC_Code);
		}

		protected virtual void SetXMLInvoiceLineChargeCode(Xsd.TxnLine txnLine, ZString chargeCodeThatDoesExist)
		{
			txnLine.ChargeCode = chargeCodeThatDoesExist;
		}

		protected virtual OrgHeader GetOrgForChargeCodeMappingTest()
		{
			return ObjectCreator.ABIGAS;
		}

		public void TestImportInvalidChargeCode_DoesNotExist()
		{
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			gLHeader.AG_AccountNum = "1111.11.11";
			gLHeader.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			gLHeader.AG_DisallowDirectPosting = false;
			Factory.Save();
			AssertNotNull("Precondition: GL Account should exist", gLHeader);

			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid());
			AssertEquals("PreCondition: Registry is set", gLHeader.PK.ToGuid(), (Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ZString chargeCodeThatDoesNotExist = "ZZZZZ";
			AssertEquals("Precondition: ZZZZZ Charge Code does not exist", 0, Factory.GetDatabaseCount(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_Code, chargeCodeThatDoesNotExist)));

			TxnLine.ChargeCode = chargeCodeThatDoesNotExist;
			TxnLine.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Warning: Transaction AP INV 00001000: Line 1:  Line  No matches were found for the following Charge Code: ZZZZZ");
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Warning: This transaction line has been allocated to the GL Journal Clearing Account");

			AssertEquals("Invoice charge code should be set.", gLHeader.PK, Invoice.Lines[0].GenericCharge);
			AssertEquals("Invoice charge code should be set.", gLHeader.PK, Invoice.Lines[0].AL_AG);
		}

		public void TestSetGenericJobReportsMutexError()
		{
			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, new ValueObjectImportContext(Factory, Notify), "Transaction AP INV 00001000: Line 1: ");
			Assert("AL_JH should be set to a valid value", Invoice.Lines[0].AL_JH.IsValid);
			AssertEquals("Job's company should be current login company", GlbCompany.CurrentCompany.PK, Invoice.Lines[0].Job.JH_GC);
			AssertEquals(false, Notify.HasErrors);
			AssertEquals(false, Notify.HasWarnings);

			Xsd.TxnLine txnLine2 = new Xsd.TxnLine();
			txnLine2.ChargeCode = ObjectCreator.CC1.AC_Code;
			txnLine2.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
			NotificationBuffer notify2 = new NotificationBuffer();
			TransactionLineBuilder builder2 = new TransactionLineBuilder(new NotificationManager(notify2), new TransactionBuilderConfig());
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			InvoicingBase invoice2 = (InvoicingBase)factory2.New(typeof(APInvoice));
			try
			{
				builder2.AddTransactionLineToInvoiceBusinessObject(invoice2, txnLine2, new ValueObjectImportContext(factory2, notify2), "Transaction AP INV 00001000: Line 1: ");
				Assert("AL_JH should be empty because second process can't lock mutex", invoice2.Lines[0].AL_JH.IsEmpty);
				TestHelper.AssertNotificationsContainsErrorMessage(notify2, "Error: You have created the job S00001001 on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job S00001001 to continue.");
			}
			finally
			{
				invoice2.ReleaseAllMutexOnInvoice();
			}
		}

		public void TestJobCreationError()
		{
			var consol = ObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			Factory.Save();
			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobNo = consol.JK_UniqueConsignRef;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, new ValueObjectImportContext(Factory, Notify), "Transaction AP INV 00001000: Line 1: ");
			Assert("AL_JH should be set to a valid value", Invoice.Lines[0].AL_JH.IsValid);
			AssertEquals("Job's company should be current login company", GlbCompany.CurrentCompany.PK, Invoice.Lines[0].Job.JH_GC);
			AssertEquals(false, Notify.HasErrors);
			AssertEquals(false, Notify.HasWarnings);

			Xsd.TxnLine txnLine2 = new Xsd.TxnLine();
			txnLine2.ChargeCode = ObjectCreator.CC1.AC_Code;
			var consol2 = ObjectCreator.CreateConsol(consolNum: "C002");
			txnLine2.ConsolOrJobNo = consol2.JK_UniqueConsignRef;
			NotificationBuffer notify2 = new NotificationBuffer();
			TransactionLineBuilder builder2 = new TransactionLineBuilder(new NotificationManager(notify2), new TransactionBuilderConfig());
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			InvoicingBase invoice2 = (InvoicingBase)factory2.New(typeof(APInvoice));

			builder2.AddTransactionLineToInvoiceBusinessObject(invoice2, txnLine2, new ValueObjectImportContext(factory2, notify2), "Transaction AP INV 00001000: Line 1: ");
			Assert("AL_JH should be empty because consol can't have job", invoice2.Lines[0].AL_JH.IsEmpty);
			TestHelper.AssertNotificationsContainsErrorMessage(notify2, "Error: Operational job 'C002' does not support creation of invoicing job.");
		}

		public void TestGenericJobDBHit()
		{
			int genericJobDBHits = Factory.GetTableHitCount(GenericJob.Schema.TableName);
			AssertEquals(0, genericJobDBHits);

			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.ConsolOrJobNo = Shipment1.JS_UniqueConsignRef;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, new ValueObjectImportContext(Factory, Notify), "Transaction AP INV 00001000: Line 1: ");
			AssertEquals(genericJobDBHits + 1, Factory.GetTableHitCount(GenericJob.Schema.TableName));

			InvoicingBase invoice2 = (InvoicingBase)Factory.New(typeof(APInvoice));
			try
			{
				Xsd.TxnLine txnLine2 = new Xsd.TxnLine();
				txnLine2.HouseBIllNo = "123456";
				Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine2, new ValueObjectImportContext(Factory, Notify), "Transaction AP INV 00001001: Line 1: ");
				AssertEquals(genericJobDBHits + 2, Factory.GetTableHitCount(GenericJob.Schema.TableName));
			}
			finally
			{
				invoice2.ReleaseAllMutexOnInvoice();
			}
		}

		public void TestImportValidGLAccount()
		{
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			gLHeader.AG_AccountNum = "1111.11.11";
			gLHeader.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			gLHeader.AG_DisallowDirectPosting = false;
			Factory.Save();
			AssertNotNull("Precondition: GL Account should exist", gLHeader);

			TxnLine.GLAccount = gLHeader.AG_AccountNum;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);

			AssertEquals("TransactionHeader Line Count", 1, Invoice.Lines.Count);
			InvoicingLineBase invoiceLine = Invoice.Lines[0];
			AssertEquals("GL Account on Line", gLHeader.AG_AccountNum, invoiceLine.GLHeader.AG_AccountNum);
		}

		public void TestImportMultipleSubAccounts()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var staff = testObjectCreator.CreateStaff("TST");

			var glHeader1 = testObjectCreator.GetGLAccountFromDB("2010.00.00");
			testObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, false);
			testObjectCreator.CreateGLHeaderSubAccount(glHeader1, GlbStaffSchema.Constants.Prefix, false);

			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice));

			TxnLine.GLAccount = "2010.00.00";
			var txnSubAccount1 = TxnLine.SubAccounts.AddNew();
			txnSubAccount1.Type.Code = Core.Constants.SubAccountType.Organization;
			txnSubAccount1.Code = testObjectCreator.ABIGAS.OH_Code;
			var txnSubAccount2 = TxnLine.SubAccounts.AddNew();
			txnSubAccount2.Type.Code = Core.Constants.SubAccountType.StaffAndResources;
			txnSubAccount2.Code = staff.GS_Code;

			Builder.AddTransactionLineToInvoiceBusinessObject(invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals(1, invoice.Lines.Count);
			var invoiceLine = invoice.Lines[0];
			AssertEquals(2, invoiceLine.SubAccounts.Count);
			var subAccountOrg = invoiceLine.SubAccounts.Cast<TransactionLineSubAccount>().Single(x => x.AL1_SubClassParentTableCode == OrgHeaderSchema.Constants.Prefix);
			var subAccountStaff = invoiceLine.SubAccounts.Cast<TransactionLineSubAccount>().Single(x => x.AL1_SubClassParentTableCode == GlbStaffSchema.Constants.Prefix);
			AssertEquals(testObjectCreator.ABIGAS.PK, subAccountOrg.AL1_SubClassParentId);
			AssertEquals(staff.PK, subAccountStaff.AL1_SubClassParentId);
		}

		public void TestImportMultipleSubAccountsWithEmptySubClassCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var glHeader1 = testObjectCreator.GetGLAccountFromDB("2010.00.00");
			testObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, false);

			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice));

			TxnLine.GLAccount = "2010.00.00";
			var txnSubAccount1 = TxnLine.SubAccounts.AddNew();
			txnSubAccount1.Type.Code = Core.Constants.SubAccountType.Organization;
			Assert("Pre-condition", string.IsNullOrEmpty(txnSubAccount1.Code));

			Builder.AddTransactionLineToInvoiceBusinessObject(invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals(1, invoice.Lines.Count);
			var invoiceLine = invoice.Lines[0];
			AssertEquals(1, invoiceLine.SubAccounts.Count);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, invoiceLine.SubAccounts[0].AL1_SubClassParentTableCode);
			AssertEquals("Sub Account with empty code could be imported, but with empty parent ID.", Guid.Empty, invoiceLine.SubAccounts[0].AL1_SubClassParentId);
		}

		public void TestImportMultipleSubAccountsWithEmptySubClassType()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			TxnLine.GLAccount = "2010.00.00";
			var txnSubAccount1 = TxnLine.SubAccounts.AddNew();
			txnSubAccount1.Code = testObjectCreator.ABIGAS.OH_Code;
			Assert("Pre-condition", string.IsNullOrEmpty(txnSubAccount1.Type.Code));

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals(1, Invoice.Lines.Count);
			var invoiceLine = Invoice.Lines[0];
			AssertEquals("Invalid sub account will not be imported.", 0, invoiceLine.SubAccounts.Count);
		}

		public void TestImportInvalidGLAccount_DoesNotExist()
		{
			var findAGlAccountForTestFilter = new ZQuery(ViewGenericChargeSchema.VC_IsGLAccount, ZBool.True);
			findAGlAccountForTestFilter.AddToFilter(ViewGenericChargeSchema.VC_DisallowDirectPosting, ZBool.False);
			findAGlAccountForTestFilter.AddToFilter(ViewGenericChargeSchema.VC_Type, "BSH");
			findAGlAccountForTestFilter.AddToFilter(ViewGenericChargeSchema.VC_IsControlAccount, ZBool.False);
			var chargeForRegistry = Factory.LoadTop1<GenericCharge>(findAGlAccountForTestFilter);

			AssertNotNull("Precondition: GenericCharge for Registry is not null", chargeForRegistry);

			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeForRegistry.PK.ToGuid());
			AssertEquals("PreCondition: Registry is set", (Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), chargeForRegistry.PK.ToGuid());

			var gLCodeThatDoesNotExist = "XXXX.XX.XX";
			var dataBaseCount = Factory.GetDatabaseCount(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "XXXX.XX.XX"));
			AssertEquals("Precondition: GL Account does not exist", 0, dataBaseCount);

			TxnLine.GLAccount = gLCodeThatDoesNotExist;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			HeaderBuilder.RunValidationAndReportErrors(Invoice);

			var expectedMessage = "Warning: Transaction AP INV 00001000: Line 1: No matches were found for the following GL Account: XXXX.XX.XX";
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Warning: This transaction line has been allocated to the GL Journal Clearing Account");
			AssertEquals("GL Code", (Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), Invoice.Lines[0].AL_AG);
			AssertNoErrors(Invoice.Lines[0].GenericChargeInfo);
		}

		public void TestImportInvalidGLAccount_IncorrectType()
		{
			var query = new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.Total);
			query.AddToFilter(AccGLHeaderSchema.AG_DisallowDirectPosting, false);
			var totalGLHeader = Factory.LoadTop1<AccGLHeader>(query);
			AssertEquals("Precondition: GL Account is a Total Account", Core.Constants.AccountType.Total, totalGLHeader.AG_AccountType);

			TxnLine.GLAccount = totalGLHeader.AG_AccountNum;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);

			string expectedMessage = "The following GL Account cannot be used in this transaction: " + totalGLHeader.AG_AccountNum + System.Environment.NewLine;
			expectedMessage += "Only 'Profit & Loss' or 'Balance Sheet' GL Account types may be used";

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestImportInvalidGLAccount_RegistryIsNotSet()
		{
			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("PreCondition: Registry Guid should be empty", Guid.Empty, (Guid)AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ZString gLAccountThatDoesNotExist = "XXXX.XX.XX";
			TxnLine.GLAccount = gLAccountThatDoesNotExist;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);

			string expectedMessage = "Error: Cannot import transaction: Please set up the GL Journal Clearing Account in the Registry";
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestImportValidTaxRate()
		{
			TxnLine.TaxCode = ObjectCreator.GST1.AT_Code;
			Factory.Save();
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);
			AssertEquals("Tax Code", ObjectCreator.GST1.AT_Code, Invoice.Lines[0].TaxRate.AT_Code);
		}

		public void TestImportInvalidTaxRate()
		{
			TxnLine.TaxCode = "BLAHBLAH";
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			string expectedMessage = "Error: Transaction AP INV 00001000: Line 1: No matches were found for the following Tax Rate: BLAHBLAH";
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestImportInvalidTaxAmount_AccountsReceivable()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));

			TxnLine.TaxCode = ObjectCreator.GST1.AT_Code;
			TxnLine.OsInvoiceAmtExclTax.Value = 200M;
			TxnLine.OsTaxAmount.Value = 100M;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			AssertEquals("OS Tax Amount", 20M, Invoice.Lines[0].AL_OSTaxAmount);
		}

		public void TestImportInvalidTaxAmount_AccountsPayable()
		{
			TxnLine.TaxCode = ObjectCreator.GST1.AT_Code;
			TxnLine.OsInvoiceAmtExclTax.Value = -200M;
			TxnLine.OsTaxAmount.Value = -100M;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			AssertEquals("OS Tax Amount", 100M, Invoice.Lines[0].AL_OSTaxAmount);
		}

		public void TestImportValidWithholdingRate()
		{
			TxnLine.WHTCode = ObjectCreator.WHT1.AW_Code;
			Factory.Save();
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);
			AssertEquals("WHT Code", ObjectCreator.WHT1.AW_Code, Invoice.Lines[0].Withholding.AW_Code);
		}

		public void TestImportInvalidWithholdingRate()
		{
			TxnLine.WHTCode = "BLAHBLAH";
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			string expectedMessage = "Error: Transaction AP INV 00001000: Line 1: No matches were found for the following WHT Rate: BLAHBLAH";
			TestHelper.AssertNotificationsContainsErrorMessage(Notify, expectedMessage);
		}

		public void TestImportTxnHeaderUsingJobNumber()
		{
			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL");
			shipment1.JS_HouseBill = "HAWB1111";
			Job job1 = ObjectCreator.CreateJob(shipment1);

			ForwardingShipment shipment2 = ObjectCreator.CreateShipment("S00001002", "AUSYD", "NZAKL");
			shipment2.JS_HouseBill = "HAWB1234";
			Job job2 = ObjectCreator.CreateJob(shipment2);

			AccChargeCode chargeCode = ObjectCreator.CC1;

			Factory.Save();

			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			txnLine.ConsolOrJobNo = shipment1.JS_UniqueConsignRef;
			txnLine.HouseBIllNo = shipment2.JS_HouseBill;
			txnLine.MasterBillNo = ZString.Empty;
			txnLine.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(-200M), "AUD");
			txnLine.ChargeCode = chargeCode.AC_Code;

			TransactionBuilderConfig config = new TransactionBuilderConfig();
			config.UseConsolOrJobNumberToMatchJob = false;
			Builder = new TransactionLineBuilder(new NotificationManager(Notify), config);
			HeaderBuilder = new TransactionHeaderBuilder(new NotificationManager(Notify), config);

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_TransactionNum = TestObjectCreator.GetRandomString(Invoice.AH_TransactionNumInfo.MaxLength);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Invoice.SubmittedFromInvoicingForm = true;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			Factory.Save();

			Job shipment2Job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, shipment2.JS_UniqueConsignRef));
			AssertNotNull("There should be a job created relating to Shipment1", shipment2Job);

			ZQuery invoiceLineForShipment2Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment2Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment2Job.PK);
			InvoicingLineBase invoiceLineForShipment2 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment2Filter);

			AssertNotNull("There should be an invoice line relating to job 2", invoiceLineForShipment2);
			AssertEquals("TransactionHeader Line for Shipment 1 Value", 200M, invoiceLineForShipment2.AL_OSExTaxAmount);
		}

		public void TestImportTxnHeaderWhenMatchingOnJobNumberDisabled()
		{
			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL");
			shipment1.JS_HouseBill = "HAWB1111";
			Job job1 = ObjectCreator.CreateJob(shipment1);

			ForwardingShipment shipment2 = ObjectCreator.CreateShipment("S00001002", "AUSYD", "NZAKL");
			shipment2.JS_HouseBill = "HAWB1234";
			Job job2 = ObjectCreator.CreateJob(shipment2);

			AccChargeCode chargeCode = ObjectCreator.CC1;

			Factory.Save();

			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			txnLine.ConsolOrJobNo = shipment1.JS_UniqueConsignRef;
			txnLine.HouseBIllNo = ZString.Empty;
			txnLine.MasterBillNo = ZString.Empty;
			txnLine.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(-200M), "AUD");
			txnLine.ChargeCode = chargeCode.AC_Code;

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_TransactionNum = TestObjectCreator.GetRandomString(Invoice.AH_TransactionNumInfo.MaxLength);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Invoice.SubmittedFromInvoicingForm = true;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			Factory.Save();

			Job shipment1Job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, shipment1.JS_UniqueConsignRef));
			AssertNotNull("There should be a job created relating to Shipment1", shipment1Job);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment1Job.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("TransactionHeader Line for Shipment 1 Value", 200M, invoiceLineForShipment1.AL_OSExTaxAmount);
		}

		public void TestImportTxnHeaderUsingHouseBillNumber()
		{
			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL");
			ObjectCreator.CreateJob(shipment1);
			shipment1.JS_HouseBill = "HAWB1111";

			ForwardingShipment shipment2 = ObjectCreator.CreateShipment("S00001002", "AUSYD", "NZAKL");
			ObjectCreator.CreateJob(shipment2);
			shipment2.JS_HouseBill = "HAWB1234";

			AccChargeCode chargeCode = ObjectCreator.CC1;

			Factory.Save();

			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			txnLine.ConsolOrJobNo = ZString.Empty;
			txnLine.HouseBIllNo = shipment1.JS_HouseBill;
			txnLine.MasterBillNo = ZString.Empty;
			txnLine.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(-200M), "AUD");
			txnLine.ChargeCode = chargeCode.AC_Code;

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_TransactionNum = TestObjectCreator.GetRandomString(Invoice.AH_TransactionNumInfo.MaxLength);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Invoice.SubmittedFromInvoicingForm = true;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			Factory.Save();

			Job shipment1Job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, shipment1.JS_UniqueConsignRef));
			AssertNotNull("There should be a job created relating to Shipment1", shipment1Job);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment1Job.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("TransactionHeader Line for Shipment 1 Value", 200M, invoiceLineForShipment1.AL_OSExTaxAmount);
		}

		public void TestImportOfJobRelatedInvoice_AR()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(ARInvoice));

			ForwardingShipment shipment = ObjectCreator.CreateShipment("S00001001", "NZAKL", "AUSYD");
			ObjectCreator.CreateJob(shipment);
			Factory.Save();

			TxnLine.ConsolOrJobNo = shipment.JS_UniqueConsignRef;
			TxnLine.ChargeCode = ZString.Empty;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);

			TestHelper.AssertNotificationsContainsErrorMessage(Notify, "Job Related AR Transactions cannot be imported");
		}

		public void TestImportOfJobRelatedInvoice_AP()
		{
			ForwardingShipment shipment = ObjectCreator.CreateShipment("S00001001", "NZAKL", "AUSYD");
			ObjectCreator.CreateJob(shipment);
			TxnLine.ConsolOrJobNo = shipment.JS_UniqueConsignRef;
			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			Factory.Save();

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);

			InvoicingLineBase invoiceLine = Invoice.Lines[0];

			AssertNotNull("TransactionHeader Line 1 should have a job", invoiceLine.Job);
			AssertEquals("TransactionHeader Line 1 job number", shipment.JS_UniqueConsignRef, invoiceLine.Job.JH_JobNum);
		}

		public void TestImportOfJobRelatedInvoice_UnapprovedInvoice()
		{
			ForwardingShipment shipment = ObjectCreator.CreateShipment("S00001001", "NZAKL", "AUSYD");
			ObjectCreator.CreateJob(shipment);
			TxnLine.ConsolOrJobNo = shipment.JS_UniqueConsignRef;
			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			Factory.Save();

			Invoice = Factory.New<UAInvoice>();

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);

			InvoicingLineBase invoiceLine = Invoice.Lines[0];

			AssertNotNull("TransactionHeader Line 1 should have a job", invoiceLine.Job);
			AssertEquals("TransactionHeader Line 1 job number", shipment.JS_UniqueConsignRef, invoiceLine.Job.JH_JobNum);
		}

		public void TestImportLineDescription()
		{
			var config = new TransactionBuilderConfig();
			config.SetLineDescription = false;
			Builder = new TransactionLineBuilder(new NotificationManager(Notify), config);

			TxnLine.ChargeCode = ObjectCreator.CC1.AC_Code;
			TxnLine.Description = "This is test description";

			TxnLine.IsSplitLine = false;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);
			AssertEquals("Line Description", "Charge Code 1", Invoice.Lines[0].AL_Desc);

			TxnLine.IsSplitLine = true;
			Invoice.Lines.RemoveAndDeleteAll();
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, ZString.Empty);
			AssertEquals("Line Description", "This is test description", Invoice.Lines[0].AL_Desc);
		}

		public void TestFindJobPK()
		{
			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			TransactionBuilderConfig config = new TransactionBuilderConfig();
			config.RunExtraValidation = false;
			TransactionLineBuilder builder = new TransactionLineBuilder(new NotificationManager(new NotificationBuffer()), config);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator objectCreator = new TestObjectCreator(newFactory);
			CommonShipment shipment1 = objectCreator.CreateShipment("S00001234", "AUSYD", "NZAKL");
			txnLine.ConsolOrJobNo = shipment1.JS_UniqueConsignRef;
			newFactory.Save();

			GenericJob noJobTypeSpecified = builder.GetGenericJob_ForTestOnly(Factory, txnLine);
			AssertNotNull("Job found where shipment num is correct and jobtype not specified", noJobTypeSpecified);

			txnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.SHP;
			txnLine.ConsolOrJobTypeSpecified = true;
			config = new TransactionBuilderConfig();
			config.RunExtraValidation = false;
			TransactionLineBuilder bulder = new TransactionLineBuilder(new NotificationManager(new NotificationBuffer()), config);
			GenericJob correctJobTypeSpecified = builder.GetGenericJob_ForTestOnly(Factory, txnLine);
			AssertNotNull("Job found where shipment num is correct and correct jobtype specified", correctJobTypeSpecified);

			txnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.BRK;
			txnLine.ConsolOrJobTypeSpecified = true;
			GenericJob inCorrectJobTypeSpecified = builder.GetGenericJob_ForTestOnly(Factory, txnLine);
			AssertNull("Job found where shipment num correct and incorrect jobtype specified", inCorrectJobTypeSpecified);
		}

		public void TestFindJobPK_CompanySpecificJobs()
		{
			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			TransactionBuilderConfig config = new TransactionBuilderConfig { RunExtraValidation = false };
			NotificationBuffer buffer = new NotificationBuffer();
			TransactionLineBuilder builder = new TransactionLineBuilder(new NotificationManager(buffer), config);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator objectCreator = new TestObjectCreator(newFactory);
			VoyageAccount voyageAccount1 = newFactory.NewWithValidTestData<VoyageAccount>();
			voyageAccount1.NA_JobNumber = "VA00000001";
			voyageAccount1.NA_GC = objectCreator.NonCurrentCompanyBranch.GB_GC;
			VoyageAccount voyageAccount2 = newFactory.NewWithValidTestData<VoyageAccount>();
			voyageAccount2.NA_JobNumber = "VA00000001";
			voyageAccount2.NA_GC = GlbCompany.CurrentCompany.PK;

			ForwardingShipment anyOtherNonCompanySpecificJob = newFactory.NewWithValidTestData<ForwardingShipment>();
			anyOtherNonCompanySpecificJob.JS_UniqueConsignRef = "S0001001";

			txnLine.ConsolOrJobNo = "VA00000001";
			txnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.AVA;
			newFactory.Save();

			GenericJob genericJob = builder.GetGenericJob_ForTestOnly(Factory, txnLine);
			Assert("Buffer should not contain any errors or warning", !buffer.HasErrors && !buffer.HasWarnings);
			AssertEquals("Should've found the one and only job", voyageAccount2.PK, genericJob.PK);
		}

		public void TestFindJobPK_TransportJob()
		{
			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			TransactionBuilderConfig config = new TransactionBuilderConfig { RunExtraValidation = false };
			NotificationBuffer buffer = new NotificationBuffer();
			TransactionLineBuilder builder = new TransactionLineBuilder(new NotificationManager(buffer), config);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator objectCreator = new TestObjectCreator(newFactory);

			var transport1 = (BusinessObject)newFactory.New<Freight.LocalCartage.Integration.ICommonCartage>();
			transport1[JobCartageSchema.JJ_ConsignmentID] = "T00000001";
			transport1[JobCartageSchema.JJ_GB] = objectCreator.NonCurrentCompanyBranch.PK;
			var transport2 = (BusinessObject)newFactory.New<Freight.LocalCartage.Integration.ICommonCartage>();
			transport2[JobCartageSchema.JJ_ConsignmentID] = "T00000002";
			transport2[JobCartageSchema.JJ_GB] = GlbBranch.CurrentBranch.PK;

			txnLine.ConsolOrJobNo = "T00000001";
			txnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.TRN;
			txnLine.ConsolOrJobTypeSpecified = true;
			newFactory.Save();

			GenericJob genericJob = builder.GetGenericJob_ForTestOnly(Factory, txnLine);
			AssertEquals("T00000001", genericJob.JobNumber);

			txnLine.ConsolOrJobNo = "T00000002";
			txnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.TRN;
			txnLine.ConsolOrJobTypeSpecified = true;
			newFactory.Save();

			genericJob = builder.GetGenericJob_ForTestOnly(Factory, txnLine);
			AssertEquals("T00000002", genericJob.JobNumber);
		}

		public void TestSettingIsFinalTrueOnAPInvoice()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			TxnLine.IsFinalCharge = true;
			TxnLine.IsFinalChargeSpecified = true;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, new ZString());
			AssertEquals("Is Final should be set", true, Invoice.Lines[0].AL_IsFinalCharge);

			Invoice = (InvoicingBase)Factory.New(typeof(APCreditNote));

			TxnLine.IsFinalCharge = true;
			TxnLine.IsFinalChargeSpecified = true;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, new ZString());
			AssertEquals("Is Final should be set", true, Invoice.Lines[0].AL_IsFinalCharge);
		}

		public void TestSettingIsFinalOnAPInvoiceWhenIsFinalSpecifiedIsFalse()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			TxnLine.IsFinalCharge = true;
			TxnLine.IsFinalChargeSpecified = true;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, new ZString());
			AssertEquals("Is Final should be set", true, Invoice.Lines[0].AL_IsFinalCharge);

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			TxnLine.IsFinalCharge = true;
			TxnLine.IsFinalChargeSpecified = false;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, new ZString());
			AssertEquals("Is Final should be set to fasle because IsFinalChargeSpecified was false", false, Invoice.Lines[0].AL_IsFinalCharge);
		}

		public void TestSettingIsFinalFalseOnAPInvoice()
		{
			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));

			TxnLine.IsFinalCharge = false;
			TxnLine.IsFinalChargeSpecified = true;

			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, new ZString());
			AssertEquals("Is Final should be set", false, Invoice.Lines[0].AL_IsFinalCharge);
		}

		public void TestBranchAndDepartmentSetFromJobInCrossLedger()
		{
			TransactionBuilderConfig config = new TransactionBuilderConfig();
			config.CrossLedgerImport = true;
			Builder = new TransactionLineBuilder(new NotificationManager(Notify), config); //CrossLedger Import enabled

			ForwardingShipment shipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL");
			shipment1.JS_HouseBill = "HAWB1111";
			Job job1 = ObjectCreator.CreateJob(shipment1);
			job1.JH_GB = ObjectCreator.NonCurrentCompanyBranch.PK;
			job1.JH_GE = ObjectCreator.NonCurrentDepartment.PK;

			AccChargeCode chargeCode = ObjectCreator.CC1;

			Factory.Save();

			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			txnLine.ConsolOrJobNo = shipment1.JS_UniqueConsignRef;
			txnLine.HouseBIllNo = ZString.Empty;
			txnLine.MasterBillNo = ZString.Empty;
			txnLine.Branch = GlbBranch.CurrentBranch.GB_Code;
			txnLine.Department = GlbDepartment.CurrentDepartment.GE_Code;
			txnLine.OsInvoiceAmtExclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode(new ZDecimal(-200M), "AUD");
			txnLine.ChargeCode = chargeCode.AC_Code;

			Factory.Save();

			Invoice = Factory.New<APInvoice>();
			Invoice.AH_TransactionNum = TestObjectCreator.GetRandomString(Invoice.AH_TransactionNumInfo.MaxLength);
			Invoice.AH_FullyPaidDate = ZDateTime.Empty;
			Invoice.SubmittedFromInvoicingForm = true;
			Invoice.AH_GB = ObjectCreator.NonCurrentCompanyBranch.PK;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNotNull("Check and create CC1 charge code.", ObjectCreator.CC1);
				Factory.Save();
			}

			Job shipment1Job = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, shipment1.JS_UniqueConsignRef));
			AssertNotNull("There should be a job created relating to Shipment1", shipment1Job);

			ZQuery invoiceLineForShipment1Filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Invoice.PK);
			invoiceLineForShipment1Filter.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, shipment1Job.PK);
			InvoicingLineBase invoiceLineForShipment1 = (InvoicingLineBase)Factory.LoadTop1(typeof(APInvoiceLine), invoiceLineForShipment1Filter);

			AssertNotNull("There should be an invoice line relating to job 1", invoiceLineForShipment1);
			AssertEquals("TransactionHeader Line Branch should be same as Job", ObjectCreator.NonCurrentCompanyBranch.PK, invoiceLineForShipment1.AL_GB);
			AssertEquals("TransactionHeader Line Department should be same as Job", ObjectCreator.NonCurrentDepartment.PK, invoiceLineForShipment1.AL_GE);
		}

		public void TestLineDescriptionSetIfChargeCodeAndGLAcountEmpty()
		{
			AccChargeCode chargeCode = ObjectCreator.CC1;
			AccGLHeader glAccount = ObjectCreator.GLHeader1;
			glAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Factory.Save();

			TransactionBuilderConfig config = new TransactionBuilderConfig();
			config.SetLineDescription = false;
			Builder = new TransactionLineBuilder(new NotificationManager(Notify), config);
			string expectedDescription = "I'm a description.";
			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			txnLine.Description = expectedDescription;
			txnLine.ChargeCode = "";
			txnLine.GLAccount = "";

			Invoice = Factory.New<APInvoice>();
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals("Line description must be copied because charge code is empty.", expectedDescription, Invoice.Lines[0].AL_Desc);

			txnLine.ChargeCode = chargeCode.AC_Code;
			txnLine.GLAccount = "";

			Invoice = Factory.New<APInvoice>();
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals("Line description must not be copied because charge code is set.", chargeCode.AC_Desc, Invoice.Lines[0].AL_Desc);

			txnLine.ChargeCode = "";
			txnLine.GLAccount = glAccount.AG_AccountNum;

			Invoice = Factory.New<APInvoice>();
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");

			AssertEquals("Line description must be copied because GL accoutn is set.", glAccount.AG_Description, Invoice.Lines[0].AL_Desc);

			config.SetLineDescription = true;
			Builder = new TransactionLineBuilder(new NotificationManager(Notify), config);
			Invoice = Factory.New<APInvoice>();
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, txnLine, ImportContext, "Transaction AP INV 00001000: Line 1: ");
			AssertEquals("Line description must be copied because SetLineDescription is true.", expectedDescription, Invoice.Lines[0].AL_Desc);
		}

		ARInvoice CreateSisterCompanyARInvoice(OrgHeader debtor)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Job job = ObjectCreator.CreateJob(shipment, false);
			ARInvoice invoice = ObjectCreator.CreateARInvoice<ARInvoice>("100000", ObjectCreator.AUD, 1m, debtor);
			invoice.AH_JH = job.PK;

			AccChargeCode gstFreeChargeCode = ObjectCreator.CreateChargeCode(
				"FCC", "GST-Free Charge Code", Core.Constants.ChargeType.Revenue, 0, Factory.Load<AccTaxRate>(AccountingConfigurationRegistry.Instance.MainFreeGSTTaxID.Value), null);
			ARInvoiceLine line1 = ObjectCreator.CreateARInvoiceLine(invoice, job, gstFreeChargeCode, ObjectCreator.AUD, 1.0m, "GST-free line", 100);
			ObjectCreator.CreateJobCharge(line1, job, gstFreeChargeCode, ObjectCreator.AUD);

			var gst = Factory.Load<AccTaxRate>(AccountingConfigurationRegistry.Instance.MainGSTTaxID.Value);
			gst.SetRateNumerator_ForTestOnly(10);
			AccChargeCode gstChargeCode = ObjectCreator.CreateChargeCode(
				"GCC", "GST Charge Code", Core.Constants.ChargeType.Revenue, 0, gst, null);
			ARInvoiceLine line2 = ObjectCreator.CreateARInvoiceLine(invoice, job, gstChargeCode, ObjectCreator.AUD, 1.0m, "GST line", 200);
			ObjectCreator.CreateJobCharge(line2, job, gstChargeCode, ObjectCreator.AUD);

			return invoice;
		}

		public void TestGSTOnImportedSisterCompanyInvoiceNotGSTRegistered()
		{
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			GlbCompany sisterCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			ARInvoice sisterCompanyARInvoice = CreateSisterCompanyARInvoice(localCompany.OrgProxy);

			Factory.Save();

			using (new TemporaryUserContext() { DepartmentPK = ObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = localBranch.PK.ToGuid() }.Set())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.False;
				foreach (ARInvoiceLine line in sisterCompanyARInvoice.Lines)
				{
					ObjectCreator.CreateChargeCode(line.ChargeCode.AC_Code.Substring(2), line.ChargeCode.AC_Desc, Core.Constants.ChargeType.Margin, 100, line.TaxRate, null);
				}
				APInvoice apInvoice = (APInvoice)converter.ConvertToAPUnsafe(sisterCompanyARInvoice, Factory, false);
				try
				{
					CombineAssertions(delegate
					{
						AssertNotEquals("Precondition: Importing company should be different to the sister company.", sisterCompany.PK, GlbCompany.CurrentCompany.PK);
						AssertEquals("Precondition: Importing company should not be GST Registered.", ZBool.False, GlbCompany.CurrentCompany.GC_IsGSTRegistered);
						AssertEquals("Precondition: Importing company should be in the same country as the sister company.", sisterCompany.Country.Code, GlbCompany.CurrentCompany.Country.Code);
					});

					CombineAssertions("Line 1", delegate
					{
						AssertEquals("Charge code on AP Invoice Line 1", sisterCompanyARInvoice.Lines[0].ChargeCode.AC_Code, apInvoice.Lines[0].ChargeCode.AC_Code);
						AssertEquals("Tax ID on AP Invoice Line 1 should be empty.", ZGuid.Empty, apInvoice.Lines[0].AL_AT);
						AssertEquals("Ex Tax on AP Invoice Line 1", 100M, apInvoice.Lines[0].AL_OSExTaxAmount);
						AssertEquals("Tax Amount on AP Invoice Line 1", 0M, apInvoice.Lines[0].AL_OSTaxAmount);
						AssertEquals("Total Amount on AP Invoice Line 1", 100M, apInvoice.Lines[0].AL_OverseasTotal);
					});

					CombineAssertions("Line 2", delegate
					{
						AssertEquals("Charge code on AP Invoice Line 2", sisterCompanyARInvoice.Lines[1].ChargeCode.AC_Code, apInvoice.Lines[1].ChargeCode.AC_Code);
						AssertEquals("Tax ID on AP Invoice Line 2 should be empty.", ZGuid.Empty, apInvoice.Lines[1].AL_AT);
						AssertEquals("Ex Tax on AP Invoice Line 2", 220M, apInvoice.Lines[1].AL_OSExTaxAmount);
						AssertEquals("Tax Amount on AP Invoice Line 2", 0M, apInvoice.Lines[1].AL_OSTaxAmount);
						AssertEquals("Total Amount on AP Invoice Line 2", 220M, apInvoice.Lines[1].AL_OverseasTotal);
					});
				}
				finally
				{
					apInvoice.ReleaseAllMutexOnInvoice();
				}
			}
		}

		public void TestGSTOnImportedSisterCompanyInvoiceGSTRegisteredGSTNotApplicable()
		{
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			GlbCompany sisterCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			ARInvoice sisterCompanyARInvoice = CreateSisterCompanyARInvoice(localCompany.OrgProxy);

			Factory.Save();

			using (new TemporaryUserContext() { DepartmentPK = ObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = localBranch.PK.ToGuid() }.Set())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
				sisterCompany.OrgProxy.CompanyData.SetAPTaxApplicable(ZBool.False);
				foreach (ARInvoiceLine line in sisterCompanyARInvoice.Lines)
				{
					ObjectCreator.CreateChargeCode(line.ChargeCode.AC_Code.Substring(2), line.ChargeCode.AC_Desc, Core.Constants.ChargeType.Margin, 100, line.TaxRate, null);
				}
				APInvoice apInvoice = (APInvoice)converter.ConvertToAPUnsafe(sisterCompanyARInvoice, Factory, false);
				try
				{
					CombineAssertions(delegate
					{
						AssertNotEquals("Precondition: Importing company should be different to the sister company.", sisterCompany.PK, GlbCompany.CurrentCompany.PK);
						AssertEquals("Precondition: Importing company should be GST registered.", ZBool.True, GlbCompany.CurrentCompany.GC_IsGSTRegistered);
						AssertEquals("Precondition: Sister company proxy organisation should not be enabled for GST.", ZBool.False, apInvoice.Header.CompanyData.IsAPTaxApplicable);
						AssertEquals("Precondition: Importing company should be in the same country as the sister company.", sisterCompany.Country.Code, GlbCompany.CurrentCompany.Country.Code);
					});

					CombineAssertions("Line 1", delegate
					{
						AssertEquals("Charge code on AP Invoice Line 1", sisterCompanyARInvoice.Lines[0].ChargeCode.AC_Code, apInvoice.Lines[0].ChargeCode.AC_Code);
						AssertEquals("Tax ID on AP Invoice Line 1 should be empty.", ZGuid.Empty, apInvoice.Lines[0].AL_AT);
						AssertEquals("Ex Tax on AP Invoice Line 1", 100M, apInvoice.Lines[0].AL_OSExTaxAmount);
						AssertEquals("Tax Amount on AP Invoice Line 1", 0M, apInvoice.Lines[0].AL_OSTaxAmount);
						AssertEquals("Total Amount on AP Invoice Line 1", 100M, apInvoice.Lines[0].AL_OverseasTotal);
					});

					CombineAssertions("Line 2", delegate
					{
						AssertEquals("Charge code on AP Invoice Line 2", sisterCompanyARInvoice.Lines[1].ChargeCode.AC_Code, apInvoice.Lines[1].ChargeCode.AC_Code);
						AssertEquals("Tax ID on AP Invoice Line 2 should be empty.", ZGuid.Empty, apInvoice.Lines[1].AL_AT);
						AssertEquals("Ex Tax on AP Invoice Line 2", 220M, apInvoice.Lines[1].AL_OSExTaxAmount);
						AssertEquals("Tax Amount on AP Invoice Line 2", 0M, apInvoice.Lines[1].AL_OSTaxAmount);
						AssertEquals("Total Amount on AP Invoice Line 2", 220M, apInvoice.Lines[1].AL_OverseasTotal);
					});
				}
				finally
				{
					apInvoice.ReleaseAllMutexOnInvoice();
				}
			}
		}

		public void TestGSTOnImportedSisterCompanyInvoiceGSTRegisteredGSTApplicable()
		{
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			GlbCompany sisterCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			ARInvoice sisterCompanyARInvoice = CreateSisterCompanyARInvoice(localCompany.OrgProxy);

			Factory.Save();

			using (new TemporaryUserContext() { DepartmentPK = ObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = localBranch.PK.ToGuid() }.Set())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
				sisterCompany.OrgProxy.CompanyData.SetAPTaxApplicable(ZBool.True);
				foreach (ARInvoiceLine line in sisterCompanyARInvoice.Lines)
				{
					ObjectCreator.CreateChargeCode(line.ChargeCode.AC_Code.Substring(2), line.ChargeCode.AC_Desc, Core.Constants.ChargeType.Margin, 100, line.TaxRate, null);
				}
				APInvoice apInvoice = (APInvoice)converter.ConvertToAPUnsafe(sisterCompanyARInvoice, Factory, false);
				try
				{
					CombineAssertions(delegate
					{
						AssertNotEquals("Precondition: Importing company should be different to the sister company.", sisterCompany.PK, GlbCompany.CurrentCompany.PK);
						AssertEquals("Precondition: Importing company should be GST registered.", ZBool.True, GlbCompany.CurrentCompany.GC_IsGSTRegistered);
						AssertEquals("Precondition: Sister company proxy organisation should be enabled for GST.", ZBool.True, apInvoice.Header.CompanyData.IsAPTaxApplicable);
						AssertEquals("Precondition: Importing company should be in the same country as the sister company.", sisterCompany.Country.Code, GlbCompany.CurrentCompany.Country.Code);
					});

					CombineAssertions("Line 1", delegate
					{
						AssertEquals("Charge code on AP Invoice Line 1", sisterCompanyARInvoice.Lines[0].ChargeCode.AC_Code, apInvoice.Lines[0].ChargeCode.AC_Code);
						AssertEquals("Tax ID on AP Invoice Line 1 should be the same.", sisterCompanyARInvoice.Lines[0].AL_AT, apInvoice.Lines[0].AL_AT);
						AssertEquals("Ex Tax on AP Invoice Line 1", 100M, apInvoice.Lines[0].AL_OSExTaxAmount);
						AssertEquals("Tax Amount on AP Invoice Line 1", 0M, apInvoice.Lines[0].AL_OSTaxAmount);
						AssertEquals("Total Amount on AP Invoice Line 1", 100M, apInvoice.Lines[0].AL_OverseasTotal);
					});

					CombineAssertions("Line 2", delegate
					{
						AssertEquals("Charge code on AP Invoice Line 2", sisterCompanyARInvoice.Lines[1].ChargeCode.AC_Code, apInvoice.Lines[1].ChargeCode.AC_Code);
						AssertEquals("Tax ID on AP Invoice Line 2 should be the same.", sisterCompanyARInvoice.Lines[1].AL_AT, apInvoice.Lines[1].AL_AT);
						AssertEquals("Ex Tax on AP Invoice Line 2", 200M, apInvoice.Lines[1].AL_OSExTaxAmount);
						AssertEquals("Tax Amount on AP Invoice Line 2", 20M, apInvoice.Lines[1].AL_OSTaxAmount);
						AssertEquals("Total Amount on AP Invoice Line 2", 220M, apInvoice.Lines[1].AL_OverseasTotal);
					});
				}
				finally
				{
					apInvoice.ReleaseAllMutexOnInvoice();
				}
			}
		}

		public void TestGSTOnImportedSisterCompanyInvoiceGSTRegisteredGSTApplicableDifferentCountry()
		{
			UnapprovedTransactionConverter converter = new UnapprovedTransactionConverter(Factory);
			GlbCompany sisterCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			ARInvoice sisterCompanyARInvoice = CreateSisterCompanyARInvoice(overseasCompany.OrgProxy);

			Factory.Save();

			using (new TemporaryUserContext() { DepartmentPK = ObjectCreator.FISDepartment.PK.ToGuid(), BranchPK = overseasBranch.PK.ToGuid() }.Set())
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
				sisterCompany.OrgProxy.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(ZBool.True);

				ARInvoiceLine line = (ARInvoiceLine)sisterCompanyARInvoice.Lines[0];
				AccTaxRate freeGSTRate = Factory.Load<AccTaxRate>(AccountingConfigurationRegistry.Instance.MainFreeGSTTaxID.Value);
				AccTaxRate gstRate = Factory.Load<AccTaxRate>(AccountingConfigurationRegistry.Instance.MainGSTTaxID.Value);
				gstRate.SetRateNumerator_ForTestOnly(10);
				AccTaxRate notReportRate = Factory.Load<AccTaxRate>(AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.Value);
				ObjectCreator.CreateChargeCode(line.ChargeCode.AC_Code.Substring(2), line.ChargeCode.AC_Desc, Core.Constants.ChargeType.Margin, 100, freeGSTRate, null);
				line = (ARInvoiceLine)sisterCompanyARInvoice.Lines[1];
				ObjectCreator.CreateChargeCode(line.ChargeCode.AC_Code.Substring(2), line.ChargeCode.AC_Desc, Core.Constants.ChargeType.Margin, 100, gstRate, null);

				APInvoice apInvoice = (APInvoice)converter.ConvertToAPUnsafe(sisterCompanyARInvoice, Factory, false);
				try
				{
					CombineAssertions(delegate
					{
						AssertNotEquals("Precondition: Importing company should be different to the sister company.", sisterCompany.PK, GlbCompany.CurrentCompany.PK);
						AssertEquals("Precondition: Importing company should be GST registered.", ZBool.True, GlbCompany.CurrentCompany.GC_IsGSTRegistered);
						AssertEquals("Precondition: Sister company proxy organisation should be enabled for GST.", ZBool.True, apInvoice.Header.CompanyData.IsAPTaxApplicable);
						AssertNotEquals("Precondition: Importing company should not be in the same country as the sister company.", sisterCompany.Country.Code, GlbCompany.CurrentCompany.Country.Code);
					});

					CombineAssertions("Line 1", delegate
					{
						AssertEquals("Charge code on AP Invoice Line 1", sisterCompanyARInvoice.Lines[0].ChargeCode.AC_Code, apInvoice.Lines[0].ChargeCode.AC_Code);
						AssertEquals("Tax code on AP Invoice Line 1", freeGSTRate.AT_Code, apInvoice.Lines[0].TaxRate.AT_Code);
						AssertEquals("Ex Tax on AP Invoice Line 1", 100M, apInvoice.Lines[0].AL_OSExTaxAmount);
						AssertEquals("Tax Amount on AP Invoice Line 1", 0M, apInvoice.Lines[0].AL_OSTaxAmount);
						AssertEquals("Total Amount on AP Invoice Line 1", 100M, apInvoice.Lines[0].AL_OverseasTotal);
					});

					CombineAssertions("Line 2", delegate
					{
						AssertEquals("Charge code on AP Invoice Line 2", sisterCompanyARInvoice.Lines[1].ChargeCode.AC_Code, apInvoice.Lines[1].ChargeCode.AC_Code);
						AssertEquals("Tax code on AP Invoice Line 2", notReportRate.AT_Code, apInvoice.Lines[1].TaxRate.AT_Code);
						AssertEquals("Ex Tax on AP Invoice Line 2", 220M, apInvoice.Lines[1].AL_OSExTaxAmount);
						AssertEquals("Tax Amount on AP Invoice Line 2", 0M, apInvoice.Lines[1].AL_OSTaxAmount);
						AssertEquals("Total Amount on AP Invoice Line 2", 220M, apInvoice.Lines[1].AL_OverseasTotal);
					});
				}
				finally
				{
					apInvoice.ReleaseAllMutexOnInvoice();
				}
			}
		}

		public void TestOverrideSystemExchangeRateAndExchangeRateCalculation()
		{
			TxnLine.OsInvoiceAmtExclTax.Value = 158M;
			TxnLine.OsInvoiceAmtExclTax.CurrencyCode = ObjectCreator.USD.RX_Code;
			TxnLine.LocalInvoiceAmtExclTax.Value = 69M;

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			Invoice.AH_RX_NKTransactionCurrency = TxnLine.OsInvoiceAmtExclTax.CurrencyCode;
			TxnLine.OverrideSystemExchangeRate = true;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AR INV 00001000");
			AssertEquals("OS Ex-Tax Amount", 158M, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Local Ex-Tax Amount", 69M, Invoice.Lines[0].AL_LocalExTaxAmount);
			AssertEquals("Re-calculated Exc Tax", 2.289855072M, Invoice.Lines[0].AL_ExchangeRate);

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			TxnLine.OverrideSystemExchangeRate = false;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AR INV 00001000");
			AssertEquals("OS Ex-Tax Amount", 158M, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Local Ex-Tax Amount", 158M, Invoice.Lines[0].AL_LocalExTaxAmount);
			AssertEquals("Re-calculated Exc Tax", 1M, Invoice.Lines[0].AL_ExchangeRate);

			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			TxnLine.OverrideSystemExchangeRate = true;
			TxnLine.OsInvoiceAmtExclTax.CurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Invoice.AH_RX_NKTransactionCurrency = TxnLine.OsInvoiceAmtExclTax.CurrencyCode;
			Builder.AddTransactionLineToInvoiceBusinessObject(Invoice, TxnLine, ImportContext, "Transaction AR INV 00001000");
			AssertEquals("OS Ex-Tax Amount", 158M, Invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Local Ex-Tax Amount", 158M, Invoice.Lines[0].AL_LocalExTaxAmount);
			AssertEquals("Re-calculated Exc Tax", 1M, Invoice.Lines[0].AL_ExchangeRate);
		}

		public void TestGetGenericJobWhenHouseBillNoIsRecordedInDeclarationButDeclarationNotExistInCurrentCompany()
		{
			var config = new TransactionBuilderConfig();
			config.RunExtraValidation = false;
			var builder = new TransactionLineBuilder(new NotificationManager(new NotificationBuffer()), config);

			var declaration = ObjectCreator.CreateDeclaration("B001");
			declaration.JE_GB = ObjectCreator.NonCurrentCompanyBranch.PK;
			declaration.JE_HouseBill = "122";
			Factory.Save();

			var txnLine = new Xsd.TxnLine();
			txnLine.ConsolOrJobType = Xsd.TxnLineConsolOrJobType.BRK;
			txnLine.ConsolOrJobTypeSpecified = true;
			txnLine.ConsolOrJobNo = "B001";
			txnLine.HouseBIllNo = "122";

			var job = builder.GetGenericJob_ForTestOnly(Factory, txnLine);
			AssertNull("Generic Job", job);
		}

		#region Test Intercompany Invoice Import

		public void TestInterCompanyInvoiceImport_TransactionContextIsINTART()
		{
			var chargeCode_SendingCompany_GST = ObjectCreator.GST11;
			var taxOverrideTaxRate = ObjectCreator.GSTFREE1;
			var transactionContext = TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			var transactionRule = TaxOverrideDefaultingRule.Codes.CopyARAmount;
			Action<InvoicingLineBase, InvoicingLineBase> generalAsserts = (arInvoiceLine, invoiceLine) =>
			{
				AssertEquals(arInvoiceLine.AL_AT, invoiceLine.AL_AT);
				AssertEquals(arInvoiceLine.AL_OSExTaxAmount, invoiceLine.AL_OSExTaxAmount);
				AssertEquals(arInvoiceLine.AL_OSTaxAmount, invoiceLine.AL_OSTaxAmount);
			};

			AssertInterCompanyInvoiceImport_TransactionContextIsALL(chargeCode_SendingCompany_GST, taxOverrideTaxRate, string.Empty,
				transactionContext: transactionContext, transactionRule: transactionRule, generalAsserts: generalAsserts, isEnableInterCompanyTaxOverride: true);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsINTSUM()
		{
			var chargeCode_SendingCompany_GST = ObjectCreator.GST11;
			var taxOverrideTaxRate = ObjectCreator.GSTFREE1;
			var transactionContext = TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			var transactionRule = TaxOverrideDefaultingRule.Codes.SumARAmount;
			Action<InvoicingLineBase, InvoicingLineBase> generalAsserts = (arInvoiceLine, invoiceLine) =>
			{
				AssertEquals(chargeCode_SendingCompany_GST.PK, arInvoiceLine.AL_AT);
				AssertEquals(taxOverrideTaxRate.PK, invoiceLine.AL_AT);
				AssertNotEquals(0M, arInvoiceLine.AL_OSTaxAmount);
				AssertEquals(arInvoiceLine.AL_OSExTaxAmount + arInvoiceLine.AL_OSTaxAmount, invoiceLine.AL_OSExTaxAmount);
				AssertEquals(0m, invoiceLine.AL_OSTaxAmount);
			};

			AssertInterCompanyInvoiceImport_TransactionContextIsALL(chargeCode_SendingCompany_GST, taxOverrideTaxRate, string.Empty,
				transactionContext: transactionContext, transactionRule: transactionRule, generalAsserts: generalAsserts, isEnableInterCompanyTaxOverride: true);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL()
		{
			var gst11ForAllTaxOverride = ObjectCreator.CreateTaxRate("ALL1", "GST Rate 11 For All Tax Override", AccTaxRate.Types.Rated, 11, string.Empty, 0, 1);
			Factory.Save();

			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GST11, gst11ForAllTaxOverride, string.Empty);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_CrossCountry()
		{
			var gst11ForAllTaxOverride = ObjectCreator.CreateTaxRate("ALL1", "GST Rate 11 For All Tax Override", AccTaxRate.Types.Rated, 11, string.Empty, 0, 1);
			Factory.Save();

			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GST11, gst11ForAllTaxOverride, string.Empty, CountryCodes.China, CurrencyCodes.China, 2M);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_TaxAmountNonZeroTaxRateIsZero()
		{
			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GST11, ObjectCreator.GSTFREE1, "Line Tax Amount cannot be set if Tax Rate is zero");
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_TaxAmountIsZeroTaxRateNonZero()
		{
			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GSTFREE1, ObjectCreator.GST11, "Line Tax Amount cannot be zero if Tax Rate is not zero");
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_IsOutsideExpectedTaxAmount()
		{
			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GST2, ObjectCreator.GST11, "Tax amount entered is outside the expected value for the selected tax rate");
		}

		#region When registry UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport is false

		public void TestInterCompanyInvoiceImport_TransactionContextIsINTART_TaxOverrideRegistryIsFalse()
		{
			var chargeCode_SendingCompany_GST = ObjectCreator.GST11;
			var taxOverrideTaxRate = ObjectCreator.GSTFREE1;
			var transactionContext = TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			var transactionRule = TaxOverrideDefaultingRule.Codes.CopyARAmount;

			AssertInterCompanyInvoiceImport_TransactionContextIsALL(chargeCode_SendingCompany_GST, taxOverrideTaxRate, string.Empty,
				transactionContext: transactionContext, transactionRule: transactionRule, isEnableInterCompanyTaxOverride: false);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsINTSUM_TaxOverrideRegistryIsFalse()
		{
			var chargeCode_SendingCompany_GST = ObjectCreator.GST11;
			var taxOverrideTaxRate = ObjectCreator.GSTFREE1;
			var transactionContext = TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			var transactionRule = TaxOverrideDefaultingRule.Codes.SumARAmount;

			AssertInterCompanyInvoiceImport_TransactionContextIsALL(chargeCode_SendingCompany_GST, taxOverrideTaxRate, string.Empty,
				transactionContext: transactionContext, transactionRule: transactionRule, isEnableInterCompanyTaxOverride: false);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_TaxOverrideRegistryIsFalse()
		{
			var gst11ForAllTaxOverride = ObjectCreator.CreateTaxRate("ALL1", "GST Rate 11 For All Tax Override", AccTaxRate.Types.Rated, 11, string.Empty, 0, 1);
			Factory.Save();

			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GST11, gst11ForAllTaxOverride, string.Empty, isEnableInterCompanyTaxOverride: false);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_CrossCountry_TaxOverrideRegistryIsFalse()
		{
			var taxOverrideTaxRate = ObjectCreator.CreateTaxRate("ALL1", "GST Rate 11 For All Tax Override", AccTaxRate.Types.Rated, 11, string.Empty, 0, 1);
			var notReportRate = AccTaxRate.GetNOTREPORTTaxID(Factory, GlbCompany.CurrentCompany);
			notReportRate.SetRate_ForTestOnly(0, 1);
			var chargeCode_SendingCompany_GST = ObjectCreator.GST11;
			Action<InvoicingLineBase, InvoicingLineBase> generalAsserts = (arInvoiceLine, invoiceLine) =>
			{
				AssertEquals(chargeCode_SendingCompany_GST.PK, arInvoiceLine.AL_AT);
				AssertEquals(notReportRate.PK, invoiceLine.AL_AT);
				AssertNotEquals(0M, arInvoiceLine.AL_OSTaxAmount);
				AssertEquals(arInvoiceLine.AL_OSExTaxAmount + arInvoiceLine.AL_OSTaxAmount, invoiceLine.AL_OSExTaxAmount);
				AssertEquals(0m, invoiceLine.AL_OSTaxAmount);
			};
			Factory.Save();

			AssertInterCompanyInvoiceImport_TransactionContextIsALL(chargeCode_SendingCompany_GST, taxOverrideTaxRate, string.Empty, CountryCodes.China, CurrencyCodes.China, 2M,
				generalAsserts: generalAsserts, isEnableInterCompanyTaxOverride: false);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_TaxAmountNonZeroTaxRateIsZero_TaxOverrideRegistryIsFalse()
		{
			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GST11, ObjectCreator.GSTFREE1, string.Empty, isEnableInterCompanyTaxOverride: false);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_TaxAmountIsZeroTaxRateNonZero_TaxOverrideRegistryIsFalse()
		{
			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GSTFREE1, ObjectCreator.GST11, string.Empty, isEnableInterCompanyTaxOverride: false);
		}

		public void TestInterCompanyInvoiceImport_TransactionContextIsALL_IsOutsideExpectedTaxAmount_TaxOverrideRegistryIsFalse()
		{
			AssertInterCompanyInvoiceImport_TransactionContextIsALL(ObjectCreator.GST2, ObjectCreator.GST11, string.Empty, isEnableInterCompanyTaxOverride: false);
		}

		#endregion

		void AssertInterCompanyInvoiceImport_TransactionContextIsALL
			(AccTaxRate chargeCode_SendingCompany_GST, AccTaxRate taxOverrideTaxRate, string errorMessage, string sendingCompanyCountry = CountryCodes.Australia, string sendingCompanyLocalCurrency = CurrencyCodes.Australia,
			decimal sendingCompanyARInvoiceExchangeRate = 1m, string transactionContext = TaxOverrideTransactionContext.Codes.All, string transactionRule = TaxOverrideDefaultingRule.Codes.NotApplicable,
			Action<InvoicingLineBase, InvoicingLineBase> generalAsserts = null, bool isEnableInterCompanyTaxOverride = true)
		{
			AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isEnableInterCompanyTaxOverride);

			var orgPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, sendingCompanyCountry));
			var sendingCompanyOrgProxy = ObjectCreator.CreateOrgHeader("INTORG", true, false, orgPort.Code);
			sendingCompanyOrgProxy.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true); // For cross country configuration
			var sendingCompany = ObjectCreator.CreateNewCompany("INT", sendingCompanyCountry, orgProxy: sendingCompanyOrgProxy);
			sendingCompany.GC_RX_NKLocalCurrency = sendingCompanyLocalCurrency;
			var sendingBranch = ObjectCreator.CreateNewBranch(sendingCompany, "BIN");
			var receivingDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			receivingDepartment.GE_Misc = false;
			Factory.Save();

			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK);
			periodManagementTestHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year, sendingCompany.PK);
			Factory.Save();

			var chargeCode_SendingCompany = ObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for sending company", ChargeType.Margin, 100, chargeCode_SendingCompany_GST, ObjectCreator.WHTFREE1, sendingCompany);
			var chargeCode_ReceivingCompany = ObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for receiving company", ChargeType.Margin, 100, ObjectCreator.GST2, ObjectCreator.WHTFREE1);
			Factory.Save();

			var taxMessage = ObjectCreator.CreateTaxMsg("Test", "Test", "Test", "Test");
			ObjectCreator.CreateTaxOverride(chargeCode_ReceivingCompany, taxOverrideTaxRate.PK, taxMessage.PK, transactionContext: transactionContext, defaultingRule: transactionRule);
			Factory.Save();

			var shipment = ObjectCreator.CreateShipment("S001");
			Factory.Save();

			ARInvoice arInvoice = null;
			InvoicingLineBase arInvoiceLine = null;
			var receivingBranchOrgProxy = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.OrgProxy.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sendingBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				ObjectCreator.CreateExchangeRate(ObjectCreator.AUD, sendingCompanyARInvoiceExchangeRate);
				receivingBranchOrgProxy.CompanyData.OB_IsDebtor = true;
				Factory.Save();

				var job = ObjectCreator.CreateJob(shipment);
				Factory.Save();

				arInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", ObjectCreator.AUD, sendingCompanyARInvoiceExchangeRate, receivingBranchOrgProxy);
				arInvoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
				arInvoice.AH_JH = job.PK;
				arInvoiceLine = ObjectCreator.CreateInvoiceLine(arInvoice, job, chargeCode_SendingCompany, 200m, ObjectCreator.AUD, sendingCompanyARInvoiceExchangeRate);
				ObjectCreator.CreateCharge(arInvoiceLine);
				Factory.Save();
			}

			var converter = new UnapprovedTransactionConverter(Factory);
			var convertedAPInvoice = converter.ConvertToAP(arInvoice, true);

			AssertEquals(1, convertedAPInvoice.Lines.Count);
			var invoiceLine = convertedAPInvoice.Lines.Cast<InvoicingLineBase>().First();

			if (!isEnableInterCompanyTaxOverride)
			{
				AssertEquals("Should use default tax rate logic instead of transaction context", true, invoiceLine.IsUseDefaultTaxOverrideLogic_ForTestOnly);

				if (generalAsserts == null)
				{
					AssertEquals(chargeCode_SendingCompany_GST.PK, arInvoiceLine.AL_AT);
					AssertEquals(arInvoiceLine.AL_AT, invoiceLine.AL_AT);
					AssertEquals(arInvoiceLine.AL_OSExTaxAmount, invoiceLine.AL_OSExTaxAmount);
					AssertEquals(arInvoiceLine.AL_OSTaxAmount, invoiceLine.AL_OSTaxAmount);
					AssertEquals(ZGuid.Empty, invoiceLine.AL_A9_VATClass);
				}
				else
				{
					generalAsserts(arInvoiceLine, invoiceLine);
				}

				AssertNoErrors(invoiceLine);
			}
			else
			{
				AssertEquals("Should use transaction context tax rate override", false, invoiceLine.IsUseDefaultTaxOverrideLogic_ForTestOnly);

				if (generalAsserts == null)
				{
					AssertEquals(chargeCode_SendingCompany_GST.PK, arInvoiceLine.AL_AT);
					AssertEquals(taxOverrideTaxRate.PK, invoiceLine.AL_AT);
					AssertEquals(arInvoiceLine.AL_OSExTaxAmount, invoiceLine.AL_OSExTaxAmount);
					AssertEquals(arInvoiceLine.AL_OSTaxAmount, invoiceLine.AL_OSTaxAmount);
					AssertEquals(taxMessage.PK, invoiceLine.AL_A9_VATClass);
				}
				else
				{
					generalAsserts(arInvoiceLine, invoiceLine);
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					AssertNoErrors(arInvoiceLine.AL_OSTaxAmountInfo);
				}
				else
				{
					AssertHasError(invoiceLine.AL_OSTaxAmountInfo, errorMessage);
				}
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			Invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
			TestObjectCreator.FillInvoiceWithMinimumTestData(Invoice);
			TxnLine = new Xsd.TxnLine();
			Notify = new NotificationBuffer();
			GetNewLineBuilder();
			HeaderBuilder = new TransactionHeaderBuilder(new NotificationManager(Notify), new TransactionBuilderConfig());
			TestHelper = new NotificationTestHelper();
			ObjectCreator = new TestObjectCreator(Factory);

			localCompany = ObjectCreator.CreateNewCompany("AUC");
			localBranch = ObjectCreator.CreateNewBranch(localCompany, "AB1");
			localCompany.GC_OH_OrgProxy = ObjectCreator.CreateOrgHeader("LOCALCOMP", true, true).PK;

			overseasCompany = ObjectCreator.CreateNewCompany("SGC", "SG");
			overseasBranch = ObjectCreator.CreateNewBranch(overseasCompany, "SB1");
			overseasCompany.GC_OH_OrgProxy = ObjectCreator.CreateOrgHeader("SGORGPROXY", true, true).PK;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
		}

		protected override void TearDown()
		{
			Invoice.ReleaseAllMutexOnInvoice();
			base.TearDown();
		}

		protected virtual void GetNewLineBuilder()
		{
			Builder = new TransactionLineBuilder(new NotificationManager(Notify), new TransactionBuilderConfig());
		}

		ForwardingShipment Shipment1
		{
			get
			{
				if (fShipment1 == null)
				{
					fShipment1 = ObjectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL");
					Factory.Save();
				}

				return fShipment1;
			}
		}

		ForwardingShipment fShipment1;

		protected ValueObjectImportContext ImportContext
		{
			get
			{
				if (fImportContext == null)
				{
					fImportContext = new ValueObjectImportContext(Factory, Notify);
				}
				return fImportContext;
			}
		}
		ValueObjectImportContext fImportContext;

		TransactionHeaderBuilder HeaderBuilder;
		protected TransactionLineBuilder Builder;
		protected NotificationBuffer Notify;
		NotificationTestHelper TestHelper;
		protected Xsd.TxnLine TxnLine;
		protected InvoicingBase Invoice;
		protected TestObjectCreator ObjectCreator;
		GlbCompany localCompany;
		GlbBranch localBranch;
		GlbCompany overseasCompany;
		GlbBranch overseasBranch;
	}
}
