using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	[TestedType(typeof(eNettInboundPaymentDataAdapter))]
	sealed class eNettInboundPaymentDataAdapterTest : BaseAccountingDataAdapterTest<ReceiptPaymentBase, TxnHeader>
	{
		[TestDate(2006, 01, 05)]
		public void TestImportFromENett()
		{
			if (IsImportFromValueObjectSupported)
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				AccBankAccount bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.AUD, ObjectCreator.GLHeader1);
				ARInvoice invoiceToMatch1 = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV123", ObjectCreator.AUD, 1m, 123, 123, 123, 123);
				invoiceToMatch1.AH_OH = Debtor.PK;
				ARInvoice invoiceToMatch2 = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INVUSD", ObjectCreator.USD, 1m, 123, 123, 123, 123);
				invoiceToMatch2.AH_OH = Debtor.PK;
				ARInvoice invoiceToMatch3 = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV333", ObjectCreator.AUD, 1m, 123, 123, 123, 123);
				OrgHeader orgHeader = ObjectCreator.AALSHI;
				Env.Security.OrgDetailsModifyOrgTypeFlagAR.IsAllowed = true;
				orgHeader.OH_IsDebtor = true;
				invoiceToMatch3.AH_OH = orgHeader.PK;
				AssertEquals("Organisation should be a debtor", true, orgHeader.OH_IsDebtor);

				Job job = Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "00001000";

				List<JobCharge> charges = new List<JobCharge>();
				charges.Add(createJobCharge(job.PK, "AUD", 1m, 123m, 123m, invoiceToMatch1.Lines[0].PK));
				charges.Add(createJobCharge(job.PK, "USD", 1m, 123m, 123m, invoiceToMatch2.Lines[0].PK));
				charges.Add(createJobCharge(job.PK, "AUD", 1m, 123m, 123m, invoiceToMatch3.Lines[0].PK));
				Factory.Save();

				TxnHeader payment = GetFullyPopulatedXmlAPPayment(invoiceToMatch1.AH_TransactionNum, invoiceToMatch2.AH_TransactionNum, invoiceToMatch3.AH_TransactionNum, GlbCompany.CurrentCompany.LocalCurrency, 492m, 492m, -123m);

				ARReceipt newBizO = Factory.New<ARReceipt>();
				Debtor.OH_IsDebtor = true;
				Debtor.OH_IsCreditor = true;
				newBizO.AH_OH = Debtor.PK;
				newBizO.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
				newBizO.AH_AB = bank.PK;
				ValueObjectImportContext context = new ValueObjectImportContext(newBizO.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(newBizO.Header, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(newBizO, payment, context);

				Assert("Receipt shouldn't have been deleted due to errors in import", !newBizO.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.AccountsReceivable, newBizO.AH_Ledger);
				AssertEquals("Client", Debtor.PK, newBizO.AH_OH);
				AssertEquals("Description", "AR RECEIPT", newBizO.AH_Desc);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();

				AssertEquals("Should have created 6 matchlinks", 6, matchLinks.Count);
				Assert("First matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch1.PK)).Length == 1);
				Assert("Second matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newBizO.PK)).Length == 1);
				Assert("Third matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch2.PK)).Length == 1);
				Assert("Fourth matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch3.PK)).Length == 1);

				AssertEquals("Should have an outstanding amount", -123m, newBizO.AH_OutstandingAmount);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2006, 01, 05)]
		public void TestImportFromENettGetCorrectChequeOrReference()
		{
			if (IsImportFromValueObjectSupported)
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				var adapter = GetNewBizObjXmlDataAdapter();
				var notify = new NotificationBuffer();

				var proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				var bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.AUD, ObjectCreator.GLHeader1);
				var invoiceToMatch1 = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV123", ObjectCreator.AUD, 1m, 123, 123, 123, 123);
				invoiceToMatch1.AH_OH = Debtor.PK;

				var invoiceToMatch2 = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV123", ObjectCreator.AUD, 1m, 123, 123, 123, 123);
				invoiceToMatch2.AH_OH = Debtor.PK;

				var job = Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "00001000";

				var jobCharge1 = createJobCharge(job.PK, "AUD", 1m, 123m, 123m, invoiceToMatch1.Lines[0].PK);
				var jobCharge2 = createJobCharge(job.PK, "AUD", 1m, 123m, 123m, invoiceToMatch2.Lines[0].PK);

				Debtor.OH_IsDebtor = true;

				Factory.Save();

				var receipt1 = Factory.New<ARReceipt>();
				receipt1.AH_OH = Debtor.PK;
				receipt1.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
				receipt1.AH_AB = bank.PK;
				var context = new ValueObjectImportContext(receipt1.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(receipt1.Header, new ValueObjectExportContext(notify));

				var payment = GetFullyPopulatedXmlAPPayment(invoiceToMatch1.AH_TransactionNum, GlbCompany.CurrentCompany.LocalCurrency, 492m, 492m, -123m);
				payment.ChequeOrReference = "";
				receipt1.AH_ChequeOrReference = "";
				adapter.ImportFromValueObject(receipt1, payment, context);

				AssertEquals("When input value is empty", "", receipt1.AH_ChequeOrReference);
				Assert("ARReceipt should be deleted", receipt1.IsDeleted);
				Assert("Should contain error message", notify.AsString.Contains("Error: Bank Reference Number: Please enter a Reference Number."));

				notify.Clear();
				var receipt2 = Factory.New<ARReceipt>();
				receipt2.AH_OH = Debtor.PK;
				receipt2.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
				receipt2.AH_AB = bank.PK;
				context = new ValueObjectImportContext(receipt2.Factory, notify);
				payment = GetFullyPopulatedXmlAPPayment(invoiceToMatch2.AH_TransactionNum, GlbCompany.CurrentCompany.LocalCurrency, 492m, 492m, -123m);
				payment.ChequeOrReference = "12345678";
				receipt2.AH_ChequeOrReference = "";
				adapter.ImportFromValueObject(receipt2, payment, context);

				AssertEquals("When input value is specific", "12345678", receipt2.AH_ChequeOrReference);
				Assert("Should not contain error message", !notify.AsString.Contains("Error:"));

				ErrorReporter.Clear();
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2006, 01, 05)]
		public void TestMatching_USD_USD_No()
		{
			if (IsImportFromValueObjectSupported)
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				AccBankAccount bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.USD, ObjectCreator.GLHeader1);
				ARInvoice invoiceToMatch = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INVUSD", ObjectCreator.USD, 0.9m, 450, 40, 500, 0);
				invoiceToMatch.AH_OH = Debtor.PK;

				Job job = Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "00001000";

				List<JobCharge> charges = new List<JobCharge>();
				var charge = createJobCharge(job.PK, "USD", 0.9m, 500m, 450m, invoiceToMatch.Lines[0].PK);
				charge.JR_OSSellExRate = 0.9m;
				charges.Add(charge);

				ExchangeRateReader.GetReaderInstance().ClearCache();
				RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
				exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddDays(30);
				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_RX_NKExCurrency = "USD";
				exchangeRate.RE_SellRate = 0.8;
				exchangeRate.RE_StartDate = ZDateTime.Now.AddDays(-1);

				Factory.Save();

				TxnHeader payment = GetFullyPopulatedXmlAPPayment(invoiceToMatch.AH_TransactionNum, ObjectCreator.USD, 450, 562.5m, 450);

				ARReceipt receipt = Factory.New<ARReceipt>();
				Debtor.OH_IsDebtor = true;
				Debtor.OH_IsCreditor = true;
				receipt.AH_OH = Debtor.PK;
				receipt.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
				receipt.AH_AB = bank.PK;
				ValueObjectImportContext context = new ValueObjectImportContext(receipt.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(receipt.Header, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(receipt, payment, context);

				Assert("Receipt shouldn't have been deleted due to errors in import", !receipt.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.AccountsReceivable, receipt.AH_Ledger);
				AssertEquals("Client", Debtor.PK, receipt.AH_OH);
				AssertEquals("Description", "AR RECEIPT", receipt.AH_Desc);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();

				AssertEquals("Should have created 3 matchlinks: REC, INV, EXX", 3, matchLinks.Count);
				Assert("First matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch.PK)).Length == 1);
				Assert("Second matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receipt.PK)).Length == 1);
				ARExchangeDifference exchangeDifference = Factory.LoadTop1<ARExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference));
				Assert("Third matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, exchangeDifference.PK)).Length == 1);
				AssertEquals("Exchange difference amont", 62.5m, exchangeDifference.AH_InvoiceAmount);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2006, 01, 05)]
		public void TestMatching_USD_USD_Yes()
		{
			if (IsImportFromValueObjectSupported)
			{
				AccountingConfigurationRegistry.Instance.ENettUseInvoiceExchangeRateForForeignCurrencyReceipts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				AccBankAccount bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.USD, ObjectCreator.GLHeader1);
				ARInvoice invoiceToMatch = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INVUSD", ObjectCreator.USD, 0.9m, 450, 40, 500, 0);
				invoiceToMatch.AH_OH = Debtor.PK;

				Job job = Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "00001000";

				List<JobCharge> charges = new List<JobCharge>();
				var charge = createJobCharge(job.PK, "USD", 0.9m, 500m, 450m, invoiceToMatch.Lines[0].PK);
				charge.JR_OSSellExRate = 0.9m;
				charges.Add(charge);

				Factory.Save();

				TxnHeader payment = GetFullyPopulatedXmlAPPayment(invoiceToMatch.AH_TransactionNum, ObjectCreator.USD, 450, 562.5m, 450);

				ARReceipt receipt = Factory.New<ARReceipt>();
				Debtor.OH_IsDebtor = true;
				Debtor.OH_IsCreditor = true;
				receipt.AH_OH = Debtor.PK;
				receipt.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
				receipt.AH_AB = bank.PK;
				ValueObjectImportContext context = new ValueObjectImportContext(receipt.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(receipt.Header, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(receipt, payment, context);

				Assert("Receipt shouldn't have been deleted due to errors in import", !receipt.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.AccountsReceivable, receipt.AH_Ledger);
				AssertEquals("Client", Debtor.PK, receipt.AH_OH);
				AssertEquals("Description", "AR RECEIPT", receipt.AH_Desc);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();

				AssertEquals("Should have created 2 matchlinks: no EXX", 2, matchLinks.Count);
				Assert("First matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch.PK)).Length == 1);
				Assert("Second matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receipt.PK)).Length == 1);
				AssertEquals("Receipt's exchange rate should be equal to invoice's exchange rate", 0.9m, receipt.AH_ExchangeRate);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2006, 01, 05)]
		public void TestMatching_USD_AUD_MultiCurrency()
		{
			if (IsImportFromValueObjectSupported)
			{
				AccountingConfigurationRegistry.Instance.ENettUseInvoiceExchangeRateForForeignCurrencyReceipts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				AccBankAccount bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.USD, ObjectCreator.GLHeader1);
				ARInvoice invoiceToMatch = ObjectCreator.CreateARInvoice<ARInvoice>("INVUSD", ObjectCreator.AUD, 1m, Debtor);
				Job job = ObjectCreator.Job1;
				ARInvoiceLine line = ObjectCreator.CreateARInvoiceLine(invoiceToMatch, job, ObjectCreator.CC1, ObjectCreator.USD, 1.1111m, "First line", 10m);
				invoiceToMatch.AH_FullyPaidDate = ZDateTime.Empty;

				List<JobCharge> charges = new List<JobCharge>();
				var charge = createJobCharge(job.PK, "USD", 1.1111m, 9m, 10m, line.PK);
				charges.Add(charge);

				Factory.Save();

				TxnHeader payment = GetFullyPopulatedXmlAPPayment(invoiceToMatch.AH_TransactionNum, ObjectCreator.USD, 10m, 9m, -10m);

				ARReceipt receipt = Factory.New<ARReceipt>();
				Debtor.OH_IsDebtor = true;
				Debtor.OH_IsCreditor = true;
				receipt.AH_OH = Debtor.PK;
				receipt.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
				receipt.AH_AB = bank.PK;
				ValueObjectImportContext context = new ValueObjectImportContext(receipt.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(receipt.Header, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(receipt, payment, context);

				Assert("Receipt shouldn't have been deleted due to errors in import", !receipt.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.AccountsReceivable, receipt.AH_Ledger);
				AssertEquals("Client", Debtor.PK, receipt.AH_OH);
				AssertEquals("Description", "AR RECEIPT", receipt.AH_Desc);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();

				AssertEquals("Should have created 2 matchlinks: no EXX", 2, matchLinks.Count);
				Assert("First matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch.PK)).Length == 1);
				Assert("Second matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receipt.PK)).Length == 1);
				AssertEquals("Receipt's exchange rate should be equal to invoice's exchange rate", Env.CurrentCompany.ExchangeRate.GetRate(9m, 10m), receipt.AH_ExchangeRate);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2006, 01, 05)]
		public void TestMatchComPayTransactionsWhenARInvoiceIsIssuedWithPrefix()
		{
			if (IsImportFromValueObjectSupported)
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC1");
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				AccBankAccount bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.AUD, ObjectCreator.GLHeader1);
				ARInvoice invoiceToMatch = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV123", ObjectCreator.AUD, 1m, 123, 123, 123, 123);
				invoiceToMatch.AH_OH = Debtor.PK;

				Job job = Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "00001000";

				List<JobCharge> charges = new List<JobCharge>();
				charges.Add(createJobCharge(job.PK, "AUD", 1m, 123m, 123m, invoiceToMatch.Lines[0].PK));

				Factory.Save();

				ZString transactionNumberWithPrefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value + invoiceToMatch.AH_TransactionNum;

				TxnHeader payment = GetFullyPopulatedXmlAPPayment(transactionNumberWithPrefix, "", "", GlbCompany.CurrentCompany.LocalCurrency, 123m, 123m, -123m);

				ARReceipt newBizO = Factory.New<ARReceipt>();
				Debtor.OH_IsDebtor = true;
				Debtor.OH_IsCreditor = true;
				newBizO.AH_OH = Debtor.PK;
				newBizO.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
				newBizO.AH_AB = bank.PK;
				ValueObjectImportContext context = new ValueObjectImportContext(newBizO.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(newBizO.Header, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(newBizO, payment, context);

				Assert("Receipt shouldn't have been deleted due to errors in import", !newBizO.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.AccountsReceivable, newBizO.AH_Ledger);
				AssertEquals("Client", Debtor.PK, newBizO.AH_OH);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();

				AssertEquals("Should have created 2 matchlinks", 2, matchLinks.Count);
				Assert("First matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch.PK)).Length == 1);
				Assert("Second matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newBizO.PK)).Length == 1);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2006, 01, 05)]
		public void TestMatchComPayTransactionsWhenARInvoiceIsIssuedWithBrokerContainingDebtor()
		{
			if (IsImportFromValueObjectSupported)
			{
				AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupPeriods();

				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();

				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				AccBankAccount bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.AUD, ObjectCreator.GLHeader1);
				ARInvoice invoiceToMatch = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV123", ObjectCreator.AUD, 1m, 123, 123, 123, 123);
				invoiceToMatch.AH_OH = Debtor.PK;

				Job job = Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "00001000";

				List<JobCharge> charges = new List<JobCharge>();
				charges.Add(createJobCharge(job.PK, "AUD", 1m, 123m, 123m, invoiceToMatch.Lines[0].PK));

				Factory.Save();

				TxnHeader payment = GetFullyPopulatedXmlAPPayment(invoiceToMatch.AH_TransactionNum, "", "", GlbCompany.CurrentCompany.LocalCurrency, 123m, 123m, -123m);

				OrgHeader broker = ObjectCreator.TestOrganisation;
				broker.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "321321");

				ARReceipt newBizO = Factory.New<ARReceipt>();
				Debtor.OH_IsDebtor = true;
				Debtor.OH_IsCreditor = true;
				newBizO.AH_OH = broker.PK;
				newBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.eNettDirectCredit;
				newBizO.AH_AB = bank.PK;
				ValueObjectImportContext context = new ValueObjectImportContext(newBizO.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(newBizO.Header, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(newBizO, payment, context);

				Assert("Receipt shouldn't have been deleted due to errors in import", !newBizO.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.AccountsReceivable, newBizO.AH_Ledger);
				AssertEquals("Client", broker.PK, newBizO.AH_OH);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();

				AssertEquals("Should have created 4 matchlinks", 4, matchLinks.Count);
				Assert("First matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch.PK)).Length == 1);
				Assert("Second matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newBizO.PK)).Length == 1);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2006, 01, 05)]
		public void TestMatchComPayTransactionsWhenARInvoiceIsIssuedWithBrokerContainingDebtorWithPrefix()
		{
			if (IsImportFromValueObjectSupported)
			{
				AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ABC1");
				AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupPeriods();

				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();

				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				AccBankAccount bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.AUD, ObjectCreator.GLHeader1);
				ARInvoice invoiceToMatch = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV123", ObjectCreator.AUD, 1m, 123, 123, 123, 123);
				invoiceToMatch.AH_OH = Debtor.PK;

				Job job = Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "00001000";

				List<JobCharge> charges = new List<JobCharge>();
				charges.Add(createJobCharge(job.PK, "AUD", 1m, 123m, 123m, invoiceToMatch.Lines[0].PK));

				Factory.Save();

				ZString transactionNumberWithPrefix = AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.Value + invoiceToMatch.AH_TransactionNum;
				TxnHeader payment = GetFullyPopulatedXmlAPPayment(transactionNumberWithPrefix, "", "", GlbCompany.CurrentCompany.LocalCurrency, 123m, 123m, -123m);

				OrgHeader broker = ObjectCreator.TestOrganisation;
				broker.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "321321");

				ARReceipt newBizO = Factory.New<ARReceipt>();
				Debtor.OH_IsDebtor = true;
				Debtor.OH_IsCreditor = true;
				newBizO.AH_OH = broker.PK;
				newBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.eNettDirectCredit;
				newBizO.AH_AB = bank.PK;
				ValueObjectImportContext context = new ValueObjectImportContext(newBizO.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(newBizO.Header, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(newBizO, payment, context);

				Assert("Receipt shouldn't have been deleted due to errors in import", !newBizO.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.AccountsReceivable, newBizO.AH_Ledger);
				AssertEquals("Client", broker.PK, newBizO.AH_OH);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();

				AssertEquals("Should have created 4 matchlinks", 4, matchLinks.Count);
				Assert("First matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch.PK)).Length == 1);
				Assert("Second matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newBizO.PK)).Length == 1);
			}
			else
			{
				Assert(true);
			}
		}

		JobCharge createJobCharge(ZGuid jobHeaderPK, ZString sellCurrency, ZDecimal sellExRate, ZDecimal localSellAmt, ZDecimal oSSellAmt, ZGuid aL_ARLine)
		{
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_RX_NKSellCurrency = sellCurrency;
			charge.JR_OSSellExRate = sellExRate;
			charge.JR_LocalSellAmt = localSellAmt;
			charge.JR_OSSellAmt = oSSellAmt;
			charge.JR_AL_ARLine = aL_ARLine;
			charge.JR_AC = ObjectCreator.CC4.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_JH = jobHeaderPK;
			charge.SetAmountsToLinkedLinesForTests();
			return charge;
		}

		[TestDate(2006, 01, 05)]
		public void TestReceiptImportFromENettWithReceiptLocalTotalGreaterThanInvoicesLocalTotal()
		{
			if (IsImportFromValueObjectSupported)
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				NotificationBuffer notify = new NotificationBuffer();

				OrgHeader proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				proxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201649");
				AccBankAccount bank = ObjectCreator.CreateBankAccount("BANK", "Bank account", ObjectCreator.AUD, ObjectCreator.GLHeader1);
				ARInvoice invoiceToMatch1 = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV123", ObjectCreator.USD, 1m, 246m, 0m, 246m, 0m);
				invoiceToMatch1.AH_OH = Debtor.PK;
				ARInvoice invoiceToMatch2 = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INVUSD", ObjectCreator.USD, 1m, 246m, 0m, 246m, 0m);
				invoiceToMatch2.AH_OH = Debtor.PK;
				ARInvoice invoiceToMatch3 = (ARInvoice)ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV333", ObjectCreator.USD, 1m, 246m, 0m, 246m, 0m);
				invoiceToMatch3.AH_OH = Debtor.PK;
				Factory.Save();

				Job job = Factory.NewJobForTesting<Job>();
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_JobNum = "00001000";

				List<JobCharge> charges = new List<JobCharge>();
				charges.Add(createJobCharge(job.PK, "USD", 1m, 246m, 246m, invoiceToMatch1.Lines[0].PK));
				charges.Add(createJobCharge(job.PK, "USD", 1m, 246m, 246m, invoiceToMatch2.Lines[0].PK));
				charges.Add(createJobCharge(job.PK, "USD", 1m, 246m, 246m, invoiceToMatch3.Lines[0].PK));
				Factory.Save();

				TxnHeader payment = GetFullyPopulatedXmlAPPayment(invoiceToMatch1.AH_TransactionNum, invoiceToMatch2.AH_TransactionNum, invoiceToMatch3.AH_TransactionNum, GlbCompany.CurrentCompany.LocalCurrency, 1000m, 1000m, -246m);

				ARReceipt newBizO = Factory.New<ARReceipt>();
				Debtor.OH_IsDebtor = true;
				Debtor.OH_IsCreditor = true;
				newBizO.AH_OH = Debtor.PK;
				newBizO.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;
				newBizO.AH_AB = bank.PK;
				newBizO.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				ValueObjectImportContext context = new ValueObjectImportContext(newBizO.Factory, notify);
				((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(newBizO.Header, new ValueObjectExportContext(notify));
				adapter.ImportFromValueObject(newBizO, payment, context);

				Assert(string.Format("Receipt shouldn't have been deleted due to errors in import. Errors: {0}", notify.AsString), !newBizO.IsDeleted);

				AssertEquals("Ledger", LedgerTypes.AccountsReceivable, newBizO.AH_Ledger);
				AssertEquals("Client", Debtor.PK, newBizO.AH_OH);
				AssertEquals("Description", "AR RECEIPT", newBizO.AH_Desc);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();

				AssertEquals("Should have created 4 matchlinks", 4, matchLinks.Count);
				Assert("First matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch1.PK)).Length == 1);
				Assert("Second matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newBizO.PK)).Length == 1);
				Assert("Third matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch2.PK)).Length == 1);
				Assert("Fourth matchlink", matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoiceToMatch3.PK)).Length == 1);
			}
			else
			{
				Assert(true);
			}
		}

		#region Implementation

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return false; }
		}

		protected override ReceiptPaymentBase NewBusinessObject()
		{
			return Factory.New<ARReceipt>();
		}

		protected override ValueObjectDataAdapter<ReceiptPaymentBase, TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new eNettInboundPaymentDataAdapter();
		}

		TxnHeader GetFullyPopulatedXmlAPPayment(string txnNumber1, RefCurrency oSCurrency, decimal oSAmount, decimal localAmount, decimal oSPaidAmount)
		{
			return GetFullyPopulatedXmlAPPayment(txnNumber1, string.Empty, string.Empty, oSCurrency, oSAmount, localAmount, oSPaidAmount);
		}

		TxnHeader GetFullyPopulatedXmlAPPayment(string txnNumber1, string txnNumber2, string txnNumber3, RefCurrency oSCurrency, decimal oSAmount, decimal localAmount, decimal oSPaidAmount)
		{
			TxnHeader fullyPopulatedXmlPayment = new TxnHeader();
			fullyPopulatedXmlPayment.BankCode = "BANK";
			fullyPopulatedXmlPayment.ChequeDrawer = "CHEQUEDRAWER";
			fullyPopulatedXmlPayment.ChequeOrReference = "12345678";
			fullyPopulatedXmlPayment.CreatedUserId = GlbStaff.CurrentUser.GS_LoginName;
			fullyPopulatedXmlPayment.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, new ValueObjectExportContext(new NotificationBuffer()));
			fullyPopulatedXmlPayment.Department = "FEA";
			fullyPopulatedXmlPayment.Description = "Description of Transaction";
			fullyPopulatedXmlPayment.DrawerBank = "HSBC";
			fullyPopulatedXmlPayment.DrawerBankBranch = "BRANCH1";
			fullyPopulatedXmlPayment.DisbursementFlag = true;
			fullyPopulatedXmlPayment.DisbursementFlagSpecified = true;
			fullyPopulatedXmlPayment.DueDate = PostDate.AddDays(10);
			fullyPopulatedXmlPayment.GlAccount = ZString.Empty;
			fullyPopulatedXmlPayment.InvoiceDate = PostDate.AddDays(-10);
			fullyPopulatedXmlPayment.InvTerm = "INV";
			fullyPopulatedXmlPayment.InvTermDays = "20";
			fullyPopulatedXmlPayment.TxnType = TxnType.PAY;
			fullyPopulatedXmlPayment.Ledger = TxnLedgerType.AP;
			fullyPopulatedXmlPayment.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(localAmount, GlbCompany.CurrentCompany.LocalCurrency, typeof(APPayment));
			fullyPopulatedXmlPayment.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(localAmount, GlbCompany.CurrentCompany.LocalCurrency, typeof(APPayment));
			fullyPopulatedXmlPayment.LocalTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(0.0M, GlbCompany.CurrentCompany.LocalCurrency, typeof(APPayment));
			fullyPopulatedXmlPayment.LocalWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(0.0M, GlbCompany.CurrentCompany.LocalCurrency, typeof(APPayment));
			fullyPopulatedXmlPayment.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(oSAmount, oSCurrency, typeof(APPayment));
			fullyPopulatedXmlPayment.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(oSAmount, oSCurrency, typeof(APPayment));
			fullyPopulatedXmlPayment.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(40.0M, oSCurrency, typeof(APPayment));
			fullyPopulatedXmlPayment.OsWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(20.0M, oSCurrency, typeof(APPayment));
			fullyPopulatedXmlPayment.PostDate = PostDate;
			fullyPopulatedXmlPayment.ReceiptPaymentType = TxnHeaderReceiptPaymentType.STD;
			fullyPopulatedXmlPayment.ReceiptPaymentTypeSpecified = true;
			fullyPopulatedXmlPayment.TxnCategory = "DBT";
			fullyPopulatedXmlPayment.TxnReference = "TransactionReference";

			TxnHeader paidTransactionXml1 = fullyPopulatedXmlPayment.PaidTransactions.AddNew();
			paidTransactionXml1.TxnNumber = txnNumber1;
			paidTransactionXml1.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(Debtor, new ValueObjectExportContext(new NotificationBuffer()));
			paidTransactionXml1.Branch = GlbBranch.CurrentBranch.GB_Code;
			paidTransactionXml1.AmountPaidThisPayment.Value = oSPaidAmount;

			if (!string.IsNullOrEmpty(txnNumber2))
			{
				TxnHeader paidTransactionXml2 = fullyPopulatedXmlPayment.PaidTransactions.AddNew();
				paidTransactionXml2.TxnNumber = txnNumber2;
				paidTransactionXml2.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(Debtor, new ValueObjectExportContext(new NotificationBuffer()));
				paidTransactionXml2.Branch = GlbBranch.CurrentBranch.GB_Code;
				paidTransactionXml2.AmountPaidThisPayment.Value = oSPaidAmount;
			}

			if (!string.IsNullOrEmpty(txnNumber3))
			{
				TxnHeader paidTransactionXml3 = fullyPopulatedXmlPayment.PaidTransactions.AddNew();
				paidTransactionXml3.TxnNumber = txnNumber3;
				paidTransactionXml3.DebtorOrCreditor = new OrganisationValueObjectDataAdapter().ExportToValueObject(Debtor, new ValueObjectExportContext(new NotificationBuffer()));
				paidTransactionXml3.Branch = GlbBranch.CurrentBranch.GB_Code;
				paidTransactionXml3.AmountPaidThisPayment.Value = oSPaidAmount;
			}

			return fullyPopulatedXmlPayment;
		}

		void PokePropertiesForSave()
		{
			AccChargeCode cC1 = ChargeCodeCC1;
			AccChargeCode cC4 = ChargeCodeCC4;
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
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			PokePropertiesForSave();
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			PokePropertiesForSave();
			return null;
		}

		OrgHeader Debtor
		{
			get
			{
				if (debtor == null)
				{
					debtor = ObjectCreator.ABIGAS;
					debtor.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "123123");
				}
				return debtor;
			}
		}
		OrgHeader debtor;

		#endregion

	}
}
