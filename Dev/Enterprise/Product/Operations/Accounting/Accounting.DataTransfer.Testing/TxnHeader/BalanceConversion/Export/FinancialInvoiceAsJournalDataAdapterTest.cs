using System;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	[TestedType(typeof(FinancialInvoiceAsJournalDataAdapter))]
	sealed class FinancialInvoiceAsJournalDataAdapterTest : BaseAccountingDataAdapterTest<TransactionHeader, Xsd.TxnHeader>
	{
		#region Exporting

		public void TestSetXmlFinancialInvoiceAsJournalHeaderValues()
		{
			Type[] supportedTransactionTypes = new Type[] {
				typeof(ARInvoice), typeof(ARCreditNote), typeof(ARAdjustmentNote), typeof(ARJournal), typeof(ARReceipt), typeof(ARPayment), typeof(ARTransferToRow), typeof(ARTransferFromRow), typeof(ARContraRow),
				typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(APJournal), typeof(APReceipt), typeof(APPayment), typeof(APTransferToRow), typeof(APTransferFromRow), typeof(APContraRow) };

			string[] journalDebitCreditSign = new string[] { DebitCreditDataEntry.DR, DebitCreditDataEntry.CR };

			foreach (Type transactionType in supportedTransactionTypes)
			{
				TransactionHeader transaction = PopulateInvoiceBizObj(transactionType, 100.00M, 10.00M, 0.50M, ObjectCreator.USD);

				transaction.AH_OutstandingAmount = 55m;

				int testRunAmount = 1;
				Journal journal = transaction as Journal;
				if (journal != null)
				{
					testRunAmount = journalDebitCreditSign.Length;
				}

				for (int i = 0; i < testRunAmount; i++)
				{
					if (journal != null)
					{
						journal.DebitCreditSign = journalDebitCreditSign[i];
					}
					FinancialInvoiceAsJournalDataAdapter dataAdapter = new FinancialInvoiceAsJournalDataAdapter();
					Xsd.TxnHeader xmlTransactionHeader = dataAdapter.ExportToValueObject(transaction, new ValueObjectExportContext(new NotificationBuffer()));

					ZDecimal multiplier = TxnHeaderMapper.MultiplierForImportAndExport(transaction.GetType());
					if (journal != null && journal.DebitCreditSign == DebitCreditDataEntry.DR)
					{
						multiplier = -1;
					}
					AssertTxnHeaderFields(transaction, xmlTransactionHeader, multiplier);
				}
			}
		}

		void AssertTxnHeaderFields(TransactionHeader transaction, Xsd.TxnHeader txnHeader, ZDecimal multiplier)
		{
			string assertMessagePrefix = string.Format("{0}: ", transaction.GetType().Name);
			AssertEquals(assertMessagePrefix + "DebtorOrCreditor EDICode", transaction.Header.OH_Code, txnHeader.DebtorOrCreditor.EDICode);

			Assert(assertMessagePrefix + "DebtorOrCreditor: EDI Code should not be empty", !txnHeader.DebtorOrCreditor.EDICode.IsEmpty);
			Assert(assertMessagePrefix + "DebtorOrCreditor: EDI Code should be specified", txnHeader.DebtorOrCreditor.IsSpecified);

			Assert(assertMessagePrefix + "DebtorOrCreditor: Name should not be empty", !txnHeader.DebtorOrCreditor.OrganisationDetails.Name.IsEmpty);
			Assert(assertMessagePrefix + "DebtorOrCreditor: Name should be specified", txnHeader.DebtorOrCreditor.OrganisationDetails.IsSpecified);

			Assert(assertMessagePrefix + "DebtorOrCreditor: Address 1 should not be empty", !txnHeader.DebtorOrCreditor.OrganisationDetails.Addresses[0].AddressLine1.IsEmpty);
			Assert(assertMessagePrefix + "DebtorOrCreditor: Address 1 should be specified", txnHeader.DebtorOrCreditor.OrganisationDetails.Addresses[0].IsSpecified);

			AssertEquals(assertMessagePrefix + "Description", transaction.AH_TransactionType + ":" + transaction.AH_TransactionNum + ":" + transaction.AH_Desc, txnHeader.Description);
			AssertEquals(assertMessagePrefix + "Invoice Date", transaction.AH_InvoiceDate, txnHeader.InvoiceDate);
			AssertEquals(assertMessagePrefix + "Due Date", transaction.AH_DueDate, txnHeader.DueDate);

			AssertEquals(assertMessagePrefix + "LocalTotalAmount on XmlInvoice", transaction.AH_LocalOutstandingAmount * multiplier, txnHeader.LocalInvoiceAmtInclTax.Value);
			AssertEquals(assertMessagePrefix + "LocalExtTaxAmount on XmlInvoice", transaction.AH_LocalOutstandingAmount * multiplier, txnHeader.LocalInvoiceAmtExclTax.Value);
			AssertEquals(assertMessagePrefix + "Local Currency on XmlInvoice", transaction.Branch.Company.GC_RX_NKLocalCurrency, txnHeader.LocalInvoiceAmtInclTax.CurrencyCode);
			AssertEquals(assertMessagePrefix + "OSTotalAmount on XmlInvoice", transaction.AH_Calc_OSOutstandingAmount * multiplier, txnHeader.OsInvoiceAmtInclTax.Value);
			AssertEquals(assertMessagePrefix + "OSExTaxAmount on XmlInvoice", transaction.AH_Calc_OSOutstandingAmount * multiplier, txnHeader.OsInvoiceAmtExclTax.Value);
			AssertEquals(assertMessagePrefix + "OS Currency on XmlInvoice", transaction.AH_RX_NKTransactionCurrency, txnHeader.OsInvoiceAmtInclTax.CurrencyCode);
			AssertEquals(assertMessagePrefix + "Leger", transaction.AH_Ledger, txnHeader.Ledger.ToString());
			AssertEquals(assertMessagePrefix + "Department Code", transaction.Department.GE_Code, txnHeader.Department);
			AssertEquals(assertMessagePrefix + "Branch Code", transaction.Branch.GB_Code, txnHeader.Branch);
			AssertEquals(assertMessagePrefix + "Post Date", transaction.AH_PostDate, txnHeader.PostDate);

			AssertEquals(assertMessagePrefix + "GL Account", Factory.Load<AccGLHeader>(transaction.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable ?
																		(Guid)AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) : (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)).AG_AccountNum, txnHeader.GlAccount);
			AssertEquals(assertMessagePrefix + "DebtorCreditorGUID", transaction.AH_OH.ToString(), txnHeader.DebtorOrCreditorGUID);
			Assert(assertMessagePrefix + "Is Specified", txnHeader.IsSpecified);
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

		protected override TransactionHeader NewBusinessObjectFromIValueObject(IValueObject value)
		{
			if (value != null)
			{
				return (TransactionHeader)Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(value as Xsd.TxnHeader));
			}
			else
			{
				return base.NewBusinessObjectFromIValueObject(null);
			}
		}

		protected override ValueObjectDataAdapter<TransactionHeader, Xsd.TxnHeader> GetNewBizObjXmlDataAdapter()
		{
			return new FinancialInvoiceAsJournalDataAdapter();
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "FinancialInvoice"; }
		}

		protected override TransactionHeader NewBusinessObject()
		{
			return Factory.New<ARInvoice>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return GetPopulatedBizObjWithEmptyFieldsSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return GetPopulatedBizObjWithEmptyFieldsSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(Transaction, Retriever.SaveResourceToFile("Joutnal.xml"), ValidationKind.None, "Journal");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				// These items are never used for Invoices, Credit Notes or Adjustment Notes.
				// DebtorOrCreditor is already tested in the Organisation Data Adapter
				return new string[] { "DrawerBank",
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
										"TxnLines",

										//We only export these values
										"DebtorOrCreditor/Notes/CustomNoteTypeName",
										"DebtorOrCreditor/Notes/NoteData",
										"DebtorOrCreditor/Notes/NoteCreatedDateTime",

										//We only import these elements
										"OverrideSystemExchangeRate",

										"OrderReference",
										"OwnerReference",
										"PaymentReference",

										// Tested by a dedicated test
										"Attachments/FileName",

										// Only used for eNett Container Storage Payment transactions
										"ENettStoragePaymentDetails/ContainerReference",
										"ENettStoragePaymentDetails/TerminalCode",
										"ENettStoragePaymentDetails/PickupDate",

										"AmountPaidThisPayment/CurrencyCode",
										"PaidTransactions",
										"PaymentReceiptBatchDate",
										"FullyPaidDate",
										"TxnCount",
										"TxnNumber",
										"JobInvoiceNo",
										"TxnReference",
										"DisbursementFlag",
										"InvTerm",
										"InvTermDays",
										"GLPeriod",
										"MatchStatus",
										"MatchStatusReasonCode",
										"ThirdPartyReference",
										"LocalTaxAmount/CurrencyCode",
										"LocalWHTAmount/CurrencyCode",
										"OsTaxAmount/CurrencyCode",
										"OsWHTAmount/CurrencyCode",
										"CashBasisTaxIndicator",
										"TxnHeaderGUID",
										"Attachments/FilePath",
										"Attachments/DocumentType",
										//Not relevent to this transaction type
										"TxnOverrideAddress",
										"TxnOverrideContact",
										//Not real node, just a flag
										"OsCurrencyEmptyFlag",
										"ShouldCreateDuringMatching"
								};
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Transaction = PopulateInvoiceBizObj(typeof(APInvoice), 200.00M, 20.00M, 0.50M, ObjectCreator.USD);
			Transaction.AH_TransactionNum = "000010003";
			Factory.Save();
		}

		TransactionHeader Transaction;

		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.NewWithPrimaryKey<OrgHeader>(new Guid("E11DF28A-76A1-4912-83D2-8AF7546601CA"));
					fOrganisation.FillWithValidTestData();
					fOrganisation.OH_FullName = @"The Fullname of the Organisation";
				}

				return fOrganisation;
			}
		}
		OrgHeader fOrganisation;

		TransactionHeader PopulateInvoiceBizObj(Type type, decimal oSExTaxAmount, decimal oSTaxAmount, decimal exchangeRate, RefCurrency currency)
		{
			TransactionHeader transaction = (TransactionHeader)Factory.New(type);

			transaction.AH_OH = Organisation.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transaction.AH_Desc = "This is a test description to see how the XML Export works";

			if (type == typeof(ARInvoice))
			{
				transaction.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			}

			transaction.AH_RX_NKTransactionCurrency = currency.RX_Code;
			transaction.AH_ExchangeRate = exchangeRate;
			transaction.AH_PostDate = PostDate;
			transaction.AH_InvoiceDate = PostDate.AddDays(-1);
			transaction.AH_DueDate = PostDate.AddDays(1);
			transaction.AH_TransactionReference = @"ABC123";

			if (transaction.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
			{
				transaction.AH_TransactionNum = ZString.Empty;
			}

			InvoicingBase invoice = transaction as InvoicingBase;
			if (invoice != null)
			{
				InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
				PopulateInvoiceBizObjLine(line1, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), 0.50M, transaction.PK, currency);

				InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
				PopulateInvoiceBizObjLine(line2, ZArchitecture.Core.Utilities.Round(oSExTaxAmount / 2, 2), ZArchitecture.Core.Utilities.Round(oSTaxAmount / 2, 2), 0.50M, transaction.PK, currency);
			}
			transaction.AH_FullyPaidDate = ZDateTime.Empty;

			return transaction;
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

		#endregion
	}
}
