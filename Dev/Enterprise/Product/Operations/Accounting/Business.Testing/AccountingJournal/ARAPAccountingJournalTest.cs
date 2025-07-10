using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ARAPAccountingJournal))]
	public class ARAPAccountingJournalTest : AccountingJournalTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			return new ARAPAccountingJournal(arInvoice, ReadonlyFactory);
		}

		public void TestTaxDetailsIsPopulatedWhenConstructorIsInvoked()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var taxProcessorMock = new Mock<ITaxProcessor>();

			ObjectFactory.Substitute(taxProcessorMock.Object);

			var glMovementDetailsList = new List<IGLMovementDetails>();
			var glMovementDetailsMock1 = new Mock<IGLMovementDetails>();
			var glMovementDetailsMock2 = new Mock<IGLMovementDetails>();

			glMovementDetailsList.Add(glMovementDetailsMock1.Object);
			glMovementDetailsList.Add(glMovementDetailsMock2.Object);

			taxProcessorMock.Setup(x => x.GetTaxDetailsForAccountingJournal(ReadonlyFactory, arInvoice.PK, false)).Returns(glMovementDetailsList);

			var journal = new ARAPAccountingJournal(arInvoice, ReadonlyFactory);
			AssertEquals(2, journal.TaxDetails.Count());
			taxProcessorMock.Verify(x => x.GetTaxDetailsForAccountingJournal(ReadonlyFactory, arInvoice.PK, false), Times.Once);
		}

		public void TestSourceIdentifier()
		{
			var accountingJournal = CreateTestAccountingJournal();
			Assert(accountingJournal is ISourceIdentifierProvider);
			Assert((accountingJournal as ISourceIdentifierProvider).SourceIdentifier.IsValid);
			AssertNotEquals(accountingJournal.PK, (accountingJournal as ISourceIdentifierProvider).SourceIdentifier);
			AssertEquals(transaction.PK, (accountingJournal as ISourceIdentifierProvider).SourceIdentifier);
		}

		protected override AccountingJournal CreateTestAccountingJournal()
		{
			var invoice = Creator.CreateARInvoice<ARInvoice>("AR0001000", Creator.AUD, 1.0m, Creator.Debtor);
			var journal = Creator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			journal.AH_TransactionBelongsToGroup = invoice.PK;
			transaction = invoice;
			return new ARAPAccountingJournal(invoice, ReadonlyFactory);
		}

		public void TestALDescForReportingBook()
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			var reportingBook = factory.New<AccReportingBook>();
			var transfer = Transfer.New(typeof(ARTransfer), factory);
			transfer.TransferFrom.AH_Desc = "from desc";
			transfer.TransferTo.AH_Desc = "to desc";

			var dataTable = new System.Data.DataTable("GeneralLedgerTransactionData");
			dataTable.Columns.Add("TransactionHeaderID", typeof(Guid));
			dataTable.Columns.Add("TransactionLineID", typeof(Guid));
			dataTable.Columns.Add("GLType", typeof(string));
			var row = dataTable.Rows.Add();
			row["TransactionHeaderID"] = transfer.TransferFrom.PK.ToGuid();
			row = dataTable.Rows.Add();
			row["TransactionHeaderID"] = transfer.TransferTo.PK.ToGuid();

			var accountingJournal = new ARAPAccountingJournal(transfer.TransferFrom, factory, reportingBook, dataTable);
			AssertEquals(2, accountingJournal.Lines.Select(x => x.AL_Desc == "from desc").Count());
			AssertEquals(2, accountingJournal.Lines.Select(x => x.AL_Desc == "to desc").Count());

			var arInvoice = factory.NewWithValidTestData<ARInvoice>();
			var line = arInvoice.Lines.AddNew();
			arInvoice.AH_Desc = "desc";
			line.AL_Desc = "line desc";
			row = dataTable.Rows.Add();
			row["TransactionHeaderID"] = arInvoice.PK.ToGuid();

			accountingJournal =  new ARAPAccountingJournal(arInvoice, factory, reportingBook);
			Assert(accountingJournal.Lines.All(x => x.AL_Desc == ZString.Empty));
		}

		public override void TestAJOptionalFields()
		{
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader2.PK.ToGuid());

			var job = Creator.CreateJob(Creator.LocalClient, 1.0m, Creator.Agent, 1.0m);

			//AP Invoice
			var apInvoice = Creator.CreateAPInvoice<APInvoice>("AP100001", Creator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, Creator.Creditor1);
			apInvoice.AH_AB = Creator.AUDBankAccount.PK;
			apInvoice.AH_GB_TaxBranch = Creator.NonCurrentBranch.PK;
			apInvoice.Lines.RemoveAndDeleteAll();
			var line = Creator.CreateAPInvoiceLine(apInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AP Line 001", 250m);
			line.AL_AG = Creator.GLHeader2.PK;
			line.AL_GB_TaxBranch = Creator.NonCurrentBranch.PK;
			var charge = Creator.CreateJobCharge(line, job, Creator.CC1);
			charge.JR_GB_CostTaxBranch = Creator.NonCurrentBranch.PK;
			Factory.Save();

			var aj = new ARAPAccountingJournal(apInvoice, ReadonlyFactory);
			AssertEquals("Optional Field Count", 3, aj.ApplicableOptionalFields.Count);
			AssertEquals("CREDITOR", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.CreditorText));
			AssertEquals("CREDITOR Value", Creator.Creditor1.OH_Code, aj.ApplicableOptionalFields[AccountingJournal.CreditorText]);
			AssertEquals("STATUS", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.StatusText));
			AssertEquals("STATUS Value", "Completed", aj.ApplicableOptionalFields[AccountingJournal.StatusText]);
			AssertEquals("TAXBRANCH", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.TaxBranchText));
			AssertEquals("TAXBRANCH Value", Creator.NonCurrentBranch.GB_Code, aj.ApplicableOptionalFields[AccountingJournal.TaxBranchText]);

			//AR Invoice
			var arInvoice = Creator.CreateARInvoice<ARInvoice>("AR100001", Creator.AUD, 1.0m, Creator.Debtor);
			arInvoice.AH_AB = Creator.AUDBankAccount2.PK;
			arInvoice.AH_GB_TaxBranch = Creator.NonCurrentBranch.PK;
			arInvoice.Lines.RemoveAndDeleteAll();
			var line2 = Creator.CreateARInvoiceLine(arInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AR Line 001", 250m);
			var charge2 = Creator.CreateJobCharge(line2, job, Creator.CC1);
			charge2.JR_GB_SellTaxBranch = Creator.NonCurrentBranch.PK;
			line2.AL_AG = Creator.GLHeader1.PK;
			line2.AL_GB_TaxBranch = Creator.NonCurrentBranch.PK;
			Factory.Save();

			aj = new ARAPAccountingJournal(arInvoice, ReadonlyFactory);
			AssertEquals("Optional Field Count", 3, aj.ApplicableOptionalFields.Count);
			AssertEquals("DEBOTR", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.DebtorText));
			AssertEquals("DEBOTR Value", Creator.Debtor.OH_Code, aj.ApplicableOptionalFields[AccountingJournal.DebtorText]);
			AssertEquals("STATUS", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.StatusText));
			AssertEquals("STATUS Value", "Completed", aj.ApplicableOptionalFields[AccountingJournal.StatusText]);
			AssertEquals("TAXBRANCH", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.TaxBranchText));
			AssertEquals("TAXBRANCH Value", Creator.NonCurrentBranch.GB_Code, aj.ApplicableOptionalFields[AccountingJournal.TaxBranchText]);

			//Receipt
			var receipt = Creator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, Creator.AUDBankAccount.PK);
			receipt.AH_OH = Creator.AALSHI.PK;
			aj = new ARAPAccountingJournal(receipt, ReadonlyFactory);
			Factory.Save();

			AssertEquals("Optional Field Count", 2, aj.ApplicableOptionalFields.Count);
			AssertEquals("DEBOTR", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.DebtorText));
			AssertEquals("DEBOTR Value", Creator.AALSHI.OH_Code, aj.ApplicableOptionalFields[AccountingJournal.DebtorText]);
			AssertEquals("BANK_CODE", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.BankCodeText));
			AssertEquals("BANK_CODE Value", Creator.AUDBankAccount.AB_BankAbbreviation, aj.ApplicableOptionalFields[AccountingJournal.BankCodeText]);

			//Payment
			var payment = Creator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Payment, LedgerTypes.AccountsPayable, 100m, Creator.AUDBankAccount2.PK);
			payment.AH_OH = Creator.AALSHI.PK;
			aj = new ARAPAccountingJournal(payment, ReadonlyFactory);
			Factory.Save();

			AssertEquals("Optional Field Count", 2, aj.ApplicableOptionalFields.Count);
			AssertEquals("CREDITOR", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.CreditorText));
			AssertEquals("CREDITOR Value", Creator.AALSHI.OH_Code, aj.ApplicableOptionalFields[AccountingJournal.CreditorText]);
			AssertEquals("BANK_CODE", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.BankCodeText));
			AssertEquals("BANK_CODE Value", Creator.AUDBankAccount2.AB_BankAbbreviation, aj.ApplicableOptionalFields[AccountingJournal.BankCodeText]);
		}

		public override void TestAccountingJournalLines()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var accountingJournalFactory = new BusinessObjectFactory();

				var valuesForTest = new RevenueRecognitionCollection();
				var setting = valuesForTest.AddNew();
				setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
				setting.DirectionCode = Enterprise.Core.Constants.FreightShipmentDirection.Code.All;
				setting.Mode = Core.Constants.TransportModes.All;
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				var periodCal = new AccountingPeriodCalculator(accountingJournalFactory);
				var helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupSinglePeriod(ZDateTime.Today.Year * 100 + ZDateTime.Today.Month, ZDateTime.Today, ZDateTime.Today.AddDays(30));

				Factory.Save();

				var arInvoice = Creator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 250M, 25M, true);
				Factory.Save();

				var invoiceLine = arInvoice.Lines[0];
				var shipment = invoiceLine.InvoicingJob.Parent as ForwardingShipment;
				var journal = new ARAPAccountingJournal(arInvoice, ReadonlyFactory);
				var ajlines = journal.Lines.Cast<AccountingJournalLine>().ToArray();

				AssertEquals("Lines Count", 4, ajlines.Length);
				AssertAJLine(ajlines[0],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.ARControlAccount.PK),
							 GLControlAccounts.Instance.ARControlAccount.AG_Description,
							 ZString.Empty,
							 ZDateTime.Today,
							 periodCal.GetPeriodFromDate(ZDateTime.Today, arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 250M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				AssertAJLine(ajlines[1],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.ARSuspenseControlAccount.PK),
							 GLControlAccounts.Instance.ARSuspenseControlAccount.AG_Description,
							 ZString.Empty,
							 ZDateTime.Today,
							 periodCal.GetPeriodFromDate(ZDateTime.Today, arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 -250M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				AssertAJLine(ajlines[2],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.ARControlAccount.PK),
							 GLControlAccounts.Instance.ARControlAccount.AG_Description,
							 ZString.Empty,
							 ZDateTime.Today,
							 periodCal.GetPeriodFromDate(ZDateTime.Today, arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 25M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				AssertAJLine(ajlines[3],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.PendingGSTOutputControlAccount.PK),
							 GLControlAccounts.Instance.PendingGSTOutputControlAccount.AG_Description,
							 ZString.Empty,
							 ZDateTime.Today,
							 periodCal.GetPeriodFromDate(ZDateTime.Today, arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 -25M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				var eventValue = new EventValue(Events.CustomsCleared, eventTime: ZDateTimeOffset.Today.AddDays(25));
				shipment.Logs.AddNew(eventValue);
				invoiceLine.InvoicingJob.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();

				journal = new ARAPAccountingJournal(arInvoice, ReadonlyFactory);
				ajlines = journal.Lines.Cast<AccountingJournalLine>().ToArray();

				AssertEquals("Lines Count", 6, ajlines.Length);

				//Before Recognizing
				AssertAJLine(ajlines[0],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.ARControlAccount.PK),
							 GLControlAccounts.Instance.ARControlAccount.AG_Description,
							 ZString.Empty,
							 ZDateTime.Today,
							 periodCal.GetPeriodFromDate(ZDateTime.Today, arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 250M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				AssertAJLine(ajlines[1],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.ARSuspenseControlAccount.PK),
							 GLControlAccounts.Instance.ARSuspenseControlAccount.AG_Description,
							 ZString.Empty,
							 ZDateTime.Today,
							 periodCal.GetPeriodFromDate(ZDateTime.Today, arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 -250M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				AssertAJLine(ajlines[2],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.ARControlAccount.PK),
							 GLControlAccounts.Instance.ARControlAccount.AG_Description,
							 ZString.Empty,
							 ZDateTime.Today,
							 periodCal.GetPeriodFromDate(ZDateTime.Today, arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 25M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				AssertAJLine(ajlines[3],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.PendingGSTOutputControlAccount.PK),
							 GLControlAccounts.Instance.PendingGSTOutputControlAccount.AG_Description,
							 ZString.Empty,
							 ZDateTime.Today,
							 periodCal.GetPeriodFromDate(ZDateTime.Today, arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 -25M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				//After Recognizing
				AssertAJLine(ajlines[4],
							 arInvoice.Lines[0].AL_AC,
							 new ZGuid(GLControlAccounts.Instance.ARSuspenseControlAccount.PK),
							 GLControlAccounts.Instance.ARSuspenseControlAccount.AG_Description,
							 ZString.Empty,
							 eventValue.EventTime.ToZDateTime(),
							 periodCal.GetPeriodFromDate(eventValue.EventTime.ToZDateTime(), arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 250M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);

				AssertAJLine(ajlines[5],
							 arInvoice.Lines[0].AL_AC,
							 arInvoice.Lines[0].AL_AG,
							 arInvoice.Lines[0].GLHeader.AG_Description,
							 ZString.Empty,
							 eventValue.EventTime.ToZDateTime(),
							 periodCal.GetPeriodFromDate(eventValue.EventTime.ToZDateTime(), arInvoice.AH_GC),
							 RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate,
							 -250M,
							 AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code,
							 arInvoice.AH_GB,
							 arInvoice.AH_GE);
			}
		}

		public void TestAL_OSAmountOfAccountingJournalLines()
		{
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader2.PK.ToGuid());

			var job = Creator.CreateJob(Creator.LocalClient, 1.0m, Creator.Agent, 0.00055M);

			//Transfer
			var arTransferPositive = Creator.CreateTransfer<ARTransfer>(103184110M, new ZDateTime(2008, 6, 15), Creator.ABIGAS.PK, Creator.AALSHI.PK);
			Factory.Save();

			arTransferPositive.ExchangeRate.Currency = "USD";
			arTransferPositive.ExchangeRate.Rate = 0.00055M;
			arTransferPositive.AH_InvoiceAmount = 56751.26M;

			var aj = new ARAPAccountingJournal(arTransferPositive.TransferFrom, ReadonlyFactory);
			var ajLines = aj.Lines.OfType<AccountingJournalLine>().ToArray();
			AssertEquals(-103184110M, ajLines[0].AL_OSExTaxAmount);
			AssertEquals(103184110M, ajLines[1].AL_OSExTaxAmount);

			//AP Invoice
			var apInvoice = Factory.New<APInvoice>();
			apInvoice.AH_TransactionNum = "AP0001";
			apInvoice.AH_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			apInvoice.AH_ExchangeRate = 0.00055M;
			apInvoice.AH_AB = Creator.AUDBankAccount.PK;

			var line = Factory.NewWithValidTestData<APInvoiceLine>();
			line.AL_RX_NKTransactionCurrency = Creator.USD.RX_Code;
			line.AL_LocalExTaxAmount = 56751.26M;
			line.AL_AG = Creator.GLHeader1.PK;
			line.AL_GSTVAT = 0M;
			line.AL_OSAmount = 103184110M;
			line.Company.GC_IsReciprocal = true;
			apInvoice.Lines.Add(line);
			apInvoice.AH_OutstandingAmount = 0M;
			apInvoice.AH_FullyPaidDate = ZDateTime.Now;
			line.AL_AG = Creator.GLHeader2.PK;

			Creator.CreateJobCharge(line, job, Creator.CC1);

			aj = new ARAPAccountingJournal(apInvoice, ReadonlyFactory);
			ajLines = aj.Lines.OfType<AccountingJournalLine>().ToArray();
			AssertEquals(-103184110M, ajLines[0].AL_OSExTaxAmount);
			AssertEquals(103184110M, ajLines[1].AL_OSExTaxAmount);

			var testHeader2 = Factory.NewWithValidTestData<ARDiscount>();
			testHeader2.AH_OH = Creator.CreateOrgHeader("TOR", true, true).PK;
			testHeader2.ExchangeRate.Currency = "USD";
			testHeader2.ExchangeRate.Rate = 0.00055M;
			testHeader2.AH_OSExTaxAmount = 103184110M;
			testHeader2.AH_LocalExTaxAmount = 56751.26M;
			testHeader2.AH_AB = Creator.AUDBankAccount.PK;

			testHeader2.AH_TransactionType = TransactionTypes.Receipt;
			aj = new ARAPAccountingJournal(testHeader2, ReadonlyFactory);
			ajLines = aj.Lines.OfType<AccountingJournalLine>().ToArray();
			AssertEquals(-103184110M, ajLines[0].AL_OSExTaxAmount);
			AssertEquals(103184110M, ajLines[1].AL_OSExTaxAmount);

			//ExchangeDifference
			testHeader2.AH_TransactionType = TransactionTypes.ExchangeDifference;
			aj = new ARAPAccountingJournal(testHeader2, ReadonlyFactory);
			ajLines = aj.Lines.OfType<AccountingJournalLine>().ToArray();
			AssertEquals(103184110M, ajLines[0].AL_OSExTaxAmount);
			AssertEquals(-103184110M, ajLines[1].AL_OSExTaxAmount);

			//Contra
			testHeader2.AH_TransactionType = TransactionTypes.Contra;
			aj = new ARAPAccountingJournal(testHeader2, ReadonlyFactory);
			ajLines = aj.Lines.OfType<AccountingJournalLine>().ToArray();
			AssertEquals(-103184110M, ajLines[0].AL_OSExTaxAmount);
			AssertEquals(103184110M, ajLines[1].AL_OSExTaxAmount);

			//Journal
			testHeader2.AH_TransactionType = TransactionTypes.Journal;
			aj = new ARAPAccountingJournal(testHeader2, ReadonlyFactory);
			ajLines = aj.Lines.OfType<AccountingJournalLine>().ToArray();
			AssertEquals(103184110M, ajLines[0].AL_OSExTaxAmount);
			AssertEquals(-103184110M, ajLines[1].AL_OSExTaxAmount);
		}

		public void TestAccountingJournalLines_CashForTaxRecNot100Percent()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var accountingJournalFactory = new BusinessObjectFactory();

				var valuesForTest = new RevenueRecognitionCollection();
				var setting = valuesForTest.AddNew();
				setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
				setting.DirectionCode = Enterprise.Core.Constants.FreightShipmentDirection.Code.All;
				setting.Mode = Core.Constants.TransportModes.All;
				setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

				var periodCal = new AccountingPeriodCalculator(accountingJournalFactory);
				var helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupSinglePeriod(ZDateTime.Today.Year * 100 + ZDateTime.Today.Month, ZDateTime.Today, ZDateTime.Today.AddDays(30));

				Factory.Save();

				AssertAccountingJournalLines_Cash(0.6m, "TestA");
				AssertAccountingJournalLines_Cash(0.7m, "TestB");
				AssertAccountingJournalLines_Cash(0.8m, "TestC");
				AssertAccountingJournalLines_Cash(0m, "TestD");
			}
		}

		void AssertAccountingJournalLines_Cash(ZDecimal inputGSTVATRecoverable, ZString transactionNum)
		{
			var arInvoice = Creator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 250M, 25M, true);
			arInvoice.AH_TransactionNum = transactionNum;
			arInvoice.Lines[0].AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			Factory.Save();

			AssertEquals("Precondition", inputGSTVATRecoverable, arInvoice.Lines[0].AL_InputGSTVATRecoverable);
			AssertEquals("Precondition", AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code, arInvoice.Lines[0].AL_GSTVATBasis);
			AssertEquals("Precondition", TransactionLineTypes.Cost, arInvoice.Lines[0].AL_LineType);

			var invoiceLine = arInvoice.Lines[0];
			var shipment = invoiceLine.InvoicingJob.Parent as ForwardingShipment;
			var journal = new ARAPAccountingJournal(arInvoice, ReadonlyFactory);
			var ajlines = journal.Lines.OfType<AccountingJournalLine>().ToArray();

			var ajlinesForTaxReceivable = ajlines[3];
			AssertNotNull(ajlinesForTaxReceivable);
			AssertEquals("GL Account", GLControlAccounts.Instance.PendingGSTInputControlAccount.PK, ajlinesForTaxReceivable.AL_AG);
			AssertEquals("Account Description", GLControlAccounts.Instance.PendingGSTInputControlAccount.AG_Description, ajlinesForTaxReceivable.AL_Desc);
			AssertNotEquals("Amount", 15M, ajlinesForTaxReceivable.AL_LineAmount);
			AssertEquals("Amount", 25M, ajlinesForTaxReceivable.AL_LineAmount);
		}

		public void TestAJLine_Receipt_HighPrecisionExchangeRate()
		{
			AssertAJLineForHighPrecisionExchangeRate(() =>
			Creator.CreateARReceipt(0.1274m, 31757.72m, ZDateTime.Today, ZDateTime.Today, Creator.ABIGAS.PK, Creator.AUDBankAccount.PK),
			31757.72m,
			-31757.72m);
		}

		public void TestAJLine_Payment_HighPrecisionExchangeRate()
		{
			AssertAJLineForHighPrecisionExchangeRate(() =>
			Creator.CreateARPayment(0.1274m, 31757.72m, ZDateTime.Today, ZDateTime.Today, Creator.ABIGAS.PK, Creator.AUDBankAccount.PK),
			-31757.72m,
			31757.72m);
		}

		public void TestAJLine_Journal_HighPrecisionExchangeRate()
		{
			AssertAJLineForHighPrecisionExchangeRate(() =>
			{
				var arJournal = Creator.CreateJournal<ARJournal>(0m, ZDateTime.Today, Creator.ABIGAS.PK);
				arJournal.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				arJournal.AH_ExchangeRate = 0.1274m;
				arJournal.AH_OSExTaxAmount = 31757.72m;
				return arJournal;
			},
			31757.72m,
			-31757.72m);
		}

		public void TestAJLine_Contra_HighPrecisionExchangeRate()
		{
			AssertAJLineForHighPrecisionExchangeRate(() =>
			{
				var arContra = Creator.CreateContraRow(typeof(ARContraRow), "CONTRA1", 0m, false, 1);
				arContra.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				arContra.AH_ExchangeRate = 0.1274m;
				arContra.AH_OSExTaxAmount = -31757.72m;
				return arContra;
			},
			-31757.72m,
			31757.72m);
		}

		public void TestAJLine_Contra_HaveSameAJLinesInDifferentLedgerType()
		{
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.GLHeader2.PK.ToGuid());
			var orgHeader = Creator.CreateOrgHeader("TOR", true, true);

			GLControlAccounts.Instance.APControlAccount.AG_Description = "TRADE CREDITORS CONTROL";
			GLControlAccounts.Instance.ARControlAccount.AG_Description = "TRADE DEBTORS CONTROL";

			var aPjournalLinePositiveAmount = BuildARAPAccountingJournalForContra("AP", 10m, orgHeader.PK).Lines.Cast<AccountingJournalLine>().ToArray();
			var aRjournalLinePositiveAmount = BuildARAPAccountingJournalForContra("AR", 10m, orgHeader.PK).Lines.Cast<AccountingJournalLine>().ToArray();

			AssertEquals("The AP Accounting Journal would has two lines", aPjournalLinePositiveAmount.Length, 2);
			AssertEquals("The AR Accounting Journal would has two lines", aRjournalLinePositiveAmount.Length, 2);

			AssertEquals("The AP Accounting Journal would give an creditor Account row", aPjournalLinePositiveAmount[0].AL_Desc, "TRADE CREDITORS CONTROL");
			AssertEquals("The AP Accounting Journal would give an debetor row", aPjournalLinePositiveAmount[1].AL_Desc, "TRADE DEBTORS CONTROL");
			AssertEquals("The AP Accounting Journal would give an DR row", aPjournalLinePositiveAmount[0].DebitCreditSign, "DR");
			AssertEquals("The AP Accounting Journal would give an CR row", aPjournalLinePositiveAmount[1].DebitCreditSign, "CR");
			AssertEquals("The AP Accounting Journal would give an positive amount for the DR row", aPjournalLinePositiveAmount[0].AL_OSAmount, 10m);
			AssertEquals("The AP Accounting Journal would give an negative amount for the CR row", aPjournalLinePositiveAmount[1].AL_OSAmount, -10m);

			AssertEquals("The AR Accounting Journal would give an creditor Account row", aRjournalLinePositiveAmount[0].AL_Desc, "TRADE CREDITORS CONTROL");
			AssertEquals("The AR Accounting Journal would give an debetor row", aRjournalLinePositiveAmount[1].AL_Desc, "TRADE DEBTORS CONTROL");
			AssertEquals("The AR Accounting Journal would give an DR row", aRjournalLinePositiveAmount[0].DebitCreditSign, "DR");
			AssertEquals("The AR Accounting Journal would give an CR row", aRjournalLinePositiveAmount[1].DebitCreditSign, "CR");
			AssertEquals("The AR Accounting Journal would give an positive amount for the DR row", aRjournalLinePositiveAmount[0].AL_OSAmount, 10m);
			AssertEquals("The AR Accounting Journal would give an negative amount for the CR row", aRjournalLinePositiveAmount[1].AL_OSAmount, -10m);

			var aPJournalLineNegativeAmount = BuildARAPAccountingJournalForContra("AP", -10m, orgHeader.PK).Lines.Cast<AccountingJournalLine>().ToArray();
			var aRjournalLineNegativeAmount = BuildARAPAccountingJournalForContra("AR", -10m, orgHeader.PK).Lines.Cast<AccountingJournalLine>().ToArray();

			AssertEquals("The AP Accounting Journal would has two lines", aPJournalLineNegativeAmount.Length, 2);
			AssertEquals("The AR Accounting Journal would has two lines", aRjournalLineNegativeAmount.Length, 2);

			AssertEquals("The AP Accounting Journal would give an creditor Account row", aPjournalLinePositiveAmount[0].AL_Desc, "TRADE CREDITORS CONTROL");
			AssertEquals("The AP Accounting Journal would give an debetor row", aPjournalLinePositiveAmount[1].AL_Desc, "TRADE DEBTORS CONTROL");
			AssertEquals("The AP Accounting Journal would give an DR row", aPJournalLineNegativeAmount[0].DebitCreditSign, "CR");
			AssertEquals("The AP Accounting Journal would give an CR row", aPJournalLineNegativeAmount[1].DebitCreditSign, "DR");
			AssertEquals("The AP Accounting Journal would give an positive amount for the DR row", aPJournalLineNegativeAmount[0].AL_OSAmount, -10m);
			AssertEquals("The AP Accounting Journal would give an negative amount for the CR row", aPJournalLineNegativeAmount[1].AL_OSAmount, 10m);

			AssertEquals("The AR Accounting Journal would give an creditor Account row", aPjournalLinePositiveAmount[0].AL_Desc, "TRADE CREDITORS CONTROL");
			AssertEquals("The AR Accounting Journal would give an debetor row", aPjournalLinePositiveAmount[1].AL_Desc, "TRADE DEBTORS CONTROL");
			AssertEquals("The AR Accounting Journal would give an DR row", aRjournalLineNegativeAmount[0].DebitCreditSign, "CR");
			AssertEquals("The AR Accounting Journal would give an CR row", aRjournalLineNegativeAmount[1].DebitCreditSign, "DR");
			AssertEquals("The AR Accounting Journal would give an positive amount for the DR row", aRjournalLineNegativeAmount[0].AL_OSAmount, -10m);
			AssertEquals("The AR Accounting Journal would give an negative amount for the CR row", aRjournalLineNegativeAmount[1].AL_OSAmount, 10m);

			var aPjournalLinePositiveAmount_Reverse = BuildARAPAccountingJournalForContra("AP", -10m, orgHeader.PK).Lines.Cast<AccountingJournalLine>().ToArray();
			AssertEquals("The reverse Accounting Journal would has two lines", aPjournalLinePositiveAmount_Reverse.Length, 2);
			AssertEquals("The reverse Accounting Journal would give an DR row", aPjournalLinePositiveAmount_Reverse[0].DebitCreditSign, "CR");
			AssertEquals("The reverse Accounting Journal would give an CR row", aPjournalLinePositiveAmount_Reverse[1].DebitCreditSign, "DR");
			AssertEquals("The reverse Accounting Journal would give an positive amount for the DR row", aPjournalLinePositiveAmount_Reverse[0].AL_OSAmount, -10m);
			AssertEquals("The reverse Accounting Journal would give an negative amount for the CR row", aPjournalLinePositiveAmount_Reverse[1].AL_OSAmount, 10m);

			var aRJournalLineNegativeAmount_Reverse = BuildARAPAccountingJournalForContra("AR", 10m, orgHeader.PK).Lines.Cast<AccountingJournalLine>().ToArray();
			AssertEquals("The reverse Accounting Journal would has two lines", aRJournalLineNegativeAmount_Reverse.Length, 2);
			AssertEquals("The reverse Accounting Journal would give an DR row", aRJournalLineNegativeAmount_Reverse[0].DebitCreditSign, "DR");
			AssertEquals("The reverse Accounting Journal would give an CR row", aRJournalLineNegativeAmount_Reverse[1].DebitCreditSign, "CR");
			AssertEquals("The reverse Accounting Journal would give an positive amount for the DR row", aRJournalLineNegativeAmount_Reverse[0].AL_OSAmount, 10m);
			AssertEquals("The reverse Accounting Journal would give an negative amount for the CR row", aRJournalLineNegativeAmount_Reverse[1].AL_OSAmount, -10m);
		}

		ARAPAccountingJournal BuildARAPAccountingJournalForContra(string type, ZDecimal amount, ZGuid orgHeaderPK)
		{
			if (type == "AP")
			{
				var testHeader = Factory.NewWithValidTestData<APContraRow>();
				testHeader.AH_Ledger = LedgerTypes.AccountsPayable;
				testHeader.AH_OH = orgHeaderPK;
				testHeader.ExchangeRate.Currency = "USD";
				testHeader.ExchangeRate.Rate = 1M;
				testHeader.AH_OSTotalAmount = amount;
				testHeader.AH_LocalExTaxAmount = amount;
				testHeader.AH_AB = Creator.AUDBankAccount.PK;
				testHeader.AH_TransactionType = TransactionTypes.Contra;
				return new ARAPAccountingJournal(testHeader, ReadonlyFactory);
			}
			else if (type == "AR")
			{
				var testHeader = Factory.NewWithValidTestData<ARContraRow>();
				testHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
				testHeader.AH_OH = orgHeaderPK;
				testHeader.ExchangeRate.Currency = "USD";
				testHeader.ExchangeRate.Rate = 1M;
				testHeader.AH_OSTotalAmount = amount;
				testHeader.AH_LocalExTaxAmount = amount;
				testHeader.AH_AB = Creator.AUDBankAccount.PK;
				testHeader.AH_TransactionType = TransactionTypes.Contra;
				return new ARAPAccountingJournal(testHeader, ReadonlyFactory);
			}
			else
			{
				return null;
			}
		}

		public void TestAJLine_Transfer_HighPrecisionExchangeRate()
		{
			AssertAJLineForHighPrecisionExchangeRate(() =>
			{
				var transferFromRow = Creator.CreateTransferRow(typeof(ARTransferFromRow), "ARTRF1", 0m, false, 1);
				transferFromRow.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				transferFromRow.AH_ExchangeRate = 0.1274m;
				transferFromRow.AH_OSExTaxAmount = -31757.72m;

				var transferToRow = Creator.CreateTransferRow(typeof(ARTransferToRow), "ARTRF1", 0m, false, 2);
				transferToRow.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				transferToRow.AH_ExchangeRate = 0.1274m;
				transferToRow.AH_OSExTaxAmount = 31757.72m;

				return transferFromRow;
			},
			31757.72m,
			31757.72m);
		}

		public void TestAJLine_Overpayment_HighPrecisionExchangeRate()
		{
			AssertAJLineForHighPrecisionExchangeRate(() =>
			{
				var invoice = Creator.CreateARInvoice<ARInvoice>("", Creator.AUD, 0.1274m, Creator.ABIGAS);
				Creator.CreateARInvoiceLine(invoice, null, Creator.CC1, Creator.AUD, 0.1274m, "", 31757.72m);

				Factory.Save();

				var arOverpayment = Creator.CreateOverpayment<AROverpayment>(0m, ZDateTime.Today, Creator.ABIGAS.PK);
				arOverpayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				arOverpayment.AH_ExchangeRate = 0.1274m;
				arOverpayment.AH_OSExTaxAmount = -31757.72m;

				var matchLinkGroup = new TransactionMatchLinkGroup(Factory);
				var matchLink1 = Creator.CreateMatchLink(invoice);
				invoice.AH_OutstandingAmount = 0m;
				invoice.AH_FullyPaidDate = ZDateTime.Today;
				matchLinkGroup.Add(matchLink1);

				var matchLink2 = matchLinkGroup.AddNew();
				matchLink2.AP_AH = arOverpayment.PK;
				matchLink2.AP_Amount = arOverpayment.AH_InvoiceAmount + arOverpayment.AH_GSTAmount;
				matchLink2.AP_MatchDate = arOverpayment.AH_PostDate;
				matchLink2.AP_MatchGroupNum = "M0001";
				arOverpayment.AH_OutstandingAmount = 0m;
				arOverpayment.AH_FullyPaidDate = ZDateTime.Today;

				return arOverpayment;
			},
			-31757.72m,
			31757.72m);
		}

		public void TestMultiSubAccountTypeCode()
		{
			var apInvoice = Creator.CreateAPInvoice<APInvoice>("AP100001", Creator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, Creator.Creditor1);
			var accountingJournalForINV = GetJournalForMultiSubAccountTypeCode(apInvoice, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			var accountingJournalForINVWithMultiSubAccountTypeCode = accountingJournalForINV.Lines.Where(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1");
			AssertEquals(6, accountingJournalForINV.Lines.Count());
			AssertEquals(1, accountingJournalForINVWithMultiSubAccountTypeCode.Count());
			AssertEquals(apInvoice.Lines[0].GLHeader.PK, accountingJournalForINVWithMultiSubAccountTypeCode.First().GLHeader.PK);

			var apJournal = Creator.CreateJournal<APJournal>(0m, ZDateTime.Today, Creator.ABIGAS.PK);
			var accountingJournalForJNL = GetJournalForMultiSubAccountTypeCode(apJournal, LedgerTypes.AccountsPayable, TransactionTypes.Journal);
			var accountingJournalForJNLWithMultiSubAccountTypeCode = accountingJournalForJNL.Lines.Where(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1");
			AssertEquals(2, accountingJournalForJNL.Lines.Count());
			AssertEquals(1, accountingJournalForJNLWithMultiSubAccountTypeCode.Count());
			AssertEquals(apJournal.GLHeader.PK , accountingJournalForJNLWithMultiSubAccountTypeCode.First().GLHeader.PK);

			var apPayment = Creator.CreateAPPayment(0.1274m, 31757.72m, ZDateTime.Today, ZDateTime.Today, Creator.ABIGAS.PK, Creator.AUDBankAccount.PK);
			GetJournalForMultiSubAccountTypeCode(apPayment, LedgerTypes.AccountsPayable, TransactionTypes.Payment, false);

			var apDiscount = Factory.NewWithValidTestData<APDiscount>();
			GetJournalForMultiSubAccountTypeCode(apDiscount, LedgerTypes.AccountsPayable, TransactionTypes.Discount, false);

			var apContra = Creator.CreateContraRow(typeof(APContraRow), "CONTRA1", 0m, false, 1);
			GetJournalForMultiSubAccountTypeCode(apContra, LedgerTypes.AccountsPayable, TransactionTypes.Contra, false);

			var apTransfer = Creator.CreateTransferRow(typeof(APTransferFromRow), "ARTRF1", 0m, false, 1);
			GetJournalForMultiSubAccountTypeCode(apTransfer, LedgerTypes.AccountsPayable, TransactionTypes.Transfer, false);
		}

		void AssertAJLineForHighPrecisionExchangeRate(Func<TransactionHeader> createTransaction, ZDecimal expectedLine1Amount, ZDecimal expectedLine2Amount)
		{
			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_IsReciprocal = true;
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Name = "Your US Company";
			usCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var chicagoBranch = Factory.NewWithValidTestData<GlbBranch>();
			chicagoBranch.GB_GC = usCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, chicagoBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var transaction = createTransaction();
				Factory.Save();

				var journal = new ARAPAccountingJournal(transaction, ReadonlyFactory);
				var ajlines = journal.Lines.OfType<AccountingJournalLine>().ToArray();

				AssertEquals("Lines Count", 2, ajlines.Length);
				AssertEquals(expectedLine1Amount, ajlines[0].AL_OSExTaxAmount);
				AssertEquals(expectedLine2Amount, ajlines[1].AL_OSExTaxAmount);
			}
		}

		public void TestAJLines_CMTLines()
		{
			var aRInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV0001", Creator.USD, 2m, 100m, 0m, 200m, 0m, Creator.ABIGAS, Creator.CommentChargeCode.PK);
			Factory.Save();

			AssertNoExceptionThrown("No exception when amount of CMT line is zero", () =>
			{
				var journal = new ARAPAccountingJournal(aRInvoice, ReadonlyFactory);
				Assert(!journal.Lines.Any());
			});

			aRInvoice.Lines[0].AL_LineAmount = 1m;
			AssertExceptionThrown<InvalidAccountingJournalOperationException>("Exception occurs when amount of CMT line is not zero", () =>
			{
				new ARAPAccountingJournal(aRInvoice, ReadonlyFactory);
			});

			aRInvoice.Lines[0].AL_AC = Guid.Empty;
			aRInvoice.Lines[0].AL_AG = Creator.GLHeader1.PK;

			AssertNoExceptionThrown("No exception when chargeCode is null and AL_AG is not null", () =>
			{
				new ARAPAccountingJournal(aRInvoice, ReadonlyFactory);
			});
		}

		public void TestAJLines_INVWithEmptyGlHeader()
		{
			var aRInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV0001", Creator.USD, 2m, 100m, 0m, 200m, 0m, Creator.ABIGAS, Guid.Empty);
			aRInvoice.AH_PostDate = ZDateTime.Empty;
			Factory.Save();

			aRInvoice.Lines[0].AL_AG = Guid.Empty;

			AssertExceptionThrown<InvalidAccountingJournalOperationException>("Exception occurs when AL_AG is empty", () =>
			{
				new ARAPAccountingJournal(aRInvoice, ReadonlyFactory);
			});
		}
	}
}
