using System;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	[TestedType(typeof(DirectDebitBatchHeader))]
	public class DirectDebitBatchHeaderTest : TransactionHeaderTest
	{
		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;

		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		#region TestDocumentSupporterBusinessContext

		public void TestDocumentSupporterBusinessContext()
		{
			AssertEquals(BusinessContext.DirectDebitBatch, Factory.New<DirectDebitBatchHeader>().DocumentSupporter.BusinessContext);
		}

		#endregion

		#region Concurrency Policy

		[SuspendCriticalValidation]
		public void TestStrictConcurrencyForAH_InvoiceAmount()
		{
			ConcurrencyTestHelper.AssertStrictConcurrencyForAccTransactionHeader<DirectDebitBatchHeader>(AccTransactionHeaderSchema.Constants.AH_InvoiceAmount, (ZDecimal)100m, (ZDecimal)90m, (ZDecimal)80m);
		}

		[SuspendCriticalValidation]
		public void TestStrictConcurrencyForAH_OSTotal()
		{
			ConcurrencyTestHelper.AssertStrictConcurrencyForAccTransactionHeader<DirectDebitBatchHeader>(AccTransactionHeaderSchema.Constants.AH_OSTotal, (ZDecimal)100m, (ZDecimal)90m, (ZDecimal)80m);
		}

		#endregion

		public void TestAH_OSTotalAmountReadOnlyness()
		{
			var batchHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			Assert("OS Total Amount should be read only", batchHeader.AH_OSTotalAmountInfo.ReadOnly);
		}

		public void TestAH_RX_NKTransactionCurrencyReadOnlyness()
		{
			var batchHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			Assert("Transaction Currency should be read only", batchHeader.AH_RX_NKTransactionCurrencyInfo.ReadOnly);

			batchHeader.AH_RX_NKTransactionCurrency_ReadOnly = false;
			Assert("Transaction Currency should be read only", batchHeader.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
		}

		public override void TestDefaultPostDateReadOnly()
		{
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);
		}

		public override void TestAH_PostDate_ReadOnly()
		{
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);
		}

		public void TestUserAllowedToBackPostWhenReversing()
		{
			var batchHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			Factory.Save();

			Assert(batchHeader.UserAllowedToBackPost);

			batchHeader.GenerateReverseTransaction(true);
			((IReversing)batchHeader).ReversingReason = "Test Reverse";

			Assert(!batchHeader.UserAllowedToBackPost);
		}

		public void TestTransactionNumMaxLengthIsLessThanOrEqualToAH_ReceiptBatchNoMaxLength()
		{
			var header = Factory.New<DirectDebitBatchHeader>();
			using (Db.Connection.BeginTransactionWithManager())
			{
				var transactionNum = header.NumberFountainForTransactionNumber_ForTestOnly.Generate(new AccountingNumberFountainDataSourceForTest(Factory, ZDateTime.Today, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment));
				var developerMessage = string.Format("{0} max length is {1}, but you will assign {2} with length {3} to {0} in SetReceiptBatchNo method",
					AccTransactionHeaderSchema.AH_ReceiptBatchNo.Name,
					AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength,
					AccTransactionHeaderSchema.AH_TransactionNum.Name,
					transactionNum.Length);
				Assert(developerMessage, transactionNum.Length <= AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength);
			}
		}

		public override void TestTransactionNumberGenerator()
		{
			Assert("Not applicable", true);
		}

		public void TestTransactionNumberGeneratorWithouteNettOutboundSubscriberContext()
		{
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var nonCurrentBranchUnderNonCurrentBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentBranchUnderNonCurrentBranch.GB_GC = nonCurrentCompany.PK;

			var nonCurrentCompanyPK = nonCurrentCompany.PK;
			var nonCurrentBranchUnderNonCurrentBranchPK = nonCurrentBranchUnderNonCurrentBranch.PK;

			var customisation = new TransactionNumberSequenceCustomisationCollection();
			var element = customisation.AddNew();
			element.Order = 1;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			element.Length = 8;
			element.Include = true;
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);

			Factory.Save();
			AssertEquals("Transaction number", "00001000", Header.AH_TransactionNum);

			var ddrBatch = Factory.New<DirectDebitBatchHeader>();
			ddrBatch.AH_InvoiceDate = ZDateTime.Today;
			ddrBatch.AH_PostDate = ZDateTime.Today;
			ddrBatch.AH_GC = nonCurrentCompanyPK;
			ddrBatch.AH_GB = nonCurrentBranchUnderNonCurrentBranchPK;
			ddrBatch.AH_GE = NonCurrentDepartment.PK;

			Factory.Save();
			//Without the context eNettOutboundSubscriberLWKServiceTask, the TransactionNum is generated under the context
			//CurrentCompanyPK by mistake, since 1000 is taken, the new value is 1001
			AssertEquals("Transaction number", "00001001", ddrBatch.AH_TransactionNum);
		}

		public void TestTransactionNumberGeneratorWitheNettOutboundSubscriberContext()
		{
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var nonCurrentBranchUnderNonCurrentBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentBranchUnderNonCurrentBranch.GB_GC = nonCurrentCompany.PK;

			var nonCurrentCompanyPK = nonCurrentCompany.PK;
			var nonCurrentBranchUnderNonCurrentBranchPK = nonCurrentBranchUnderNonCurrentBranch.PK;

			var customisation = new TransactionNumberSequenceCustomisationCollection();
			var element = customisation.AddNew();
			element.Order = 1;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			element.Length = 8;
			element.Include = true;
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);

			Factory.Save();
			AssertEquals("Transaction number", "00001000", Header.AH_TransactionNum);

			Factory.SetContext(Enterprise.Integration.Accounting.BusinessContext.eNettOutboundSubscriberLWKServiceTask);

			var ddrBatch = Factory.New<DirectDebitBatchHeader>();
			ddrBatch.AH_InvoiceDate = ZDateTime.Today;
			ddrBatch.AH_PostDate = ZDateTime.Today;
			ddrBatch.AH_GC = nonCurrentCompanyPK;
			ddrBatch.AH_GB = nonCurrentBranchUnderNonCurrentBranchPK;
			ddrBatch.AH_GE = NonCurrentDepartment.PK;

			Factory.Save();
			//With the context eNettOutboundSubscriberLWKServiceTask, the TransactionNum is generated under the context of
			//nonCurrentCompanyPK correctly, i.e. using the initial value 1000
			AssertEquals("Transaction number", "00001000", ddrBatch.AH_TransactionNum);
		}

		public void TestFileNumber()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();

			DirectDebitBatchHeader testHeader1 = Factory.New<DirectDebitBatchHeader>();
			testHeader1.AH_AB = testBank.PK;
			Factory.Save();
			AssertEquals("FileNumber should be 1", 1, testHeader1.FileNumber);

			DirectDebitBatchHeader testHeader2 = Factory.New<DirectDebitBatchHeader>();
			testHeader2.AH_AB = testBank.PK;
			Factory.Save();
			AssertEquals("FileNumber should be 2", 2, testHeader2.FileNumber);
		}

		public void TestValidationType()
		{
			DirectDebitBatchHeader testHeader = Factory.New<DirectDebitBatchHeader>();
			AssertEquals("Should be correct validation", typeof(DirectDebitBatchHeaderValidation), testHeader.Validation.GetType());
			testHeader.IsValidatingBatchForFileGeneration_ForTestOnly = true;
			AssertEquals("Should be correct validation", typeof(DirectDebitBatchHeaderValidationForFileGeneration), testHeader.Validation.GetType());
			testHeader.IsValidatingBatchForFileGeneration_ForTestOnly = false;
			AssertEquals("Should be correct validation", typeof(DirectDebitBatchHeaderValidation), testHeader.Validation.GetType());
		}

		public void TestDDRValidation_NotNullWhenReversing()
		{
			DirectDebitBatchHeader testHeader = Factory.New<DirectDebitBatchHeader>();
			testHeader.IsReverseTransaction = true;
			AssertNotNull("Should not be null", testHeader.DDRValidation_ForTestOnly);
			AssertEquals("Should be correct validation", typeof(DirectDebitBatchHeaderValidation), testHeader.DDRValidation_ForTestOnly.GetType());
		}

		public void TestAutoDDRValidationDependentOnFileBeingGenerated()
		{
			DirectDebitBatchHeader testHeader = Factory.New<DirectDebitBatchHeader>();
			TestObjectCreator.AUDBankAccount.AB_AllowAutoDDR = false;
			testHeader.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			testHeader.ValidateBeforePosting();
			AssertNoErrors("Should be no errors on bank account", testHeader.AH_ABInfo);
			testHeader.ValidateBeforeFileGeneration();
			AssertHasErrors("Should be an error on the bank account when generating the file", testHeader.AH_ABInfo);
		}

		public void TestNewDirectDebitBatchLoadsBankCurrency()
		{
			GlbCompany.CurrentCompany.SetCountry("ID");
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_RX_NKAccountCurrency = "AUD";
			DirectDebitBatchHeader testHeader = Factory.New<DirectDebitBatchHeader>();

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			SetUpDDRTransaction(aPPayment, testBank, 120.72m, 70.29m, "");
			testHeader.AH_AB = testBank.PK;

			AssertEquals(70.29m, testHeader.AH_OSExTaxAmount);
		}

		public void TestLoadExistingDetails()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			var aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 110m, "");
			SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 220m, "");
			SetUpDDRTransaction(directPayment, DDRBankAccount, 420m, 420m, "", null, true);

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			Assert(aPPayment.IncludeInTheBatch);
			Assert(aRPayment.IncludeInTheBatch);
			Assert(directPayment.IncludeInTheBatch);

			Factory.Save();

			var readFactory = new BusinessObjectFactory();
			var loadedBatchHeader = readFactory.Load(typeof(DirectDebitBatchHeader), testHeader.PK) as DirectDebitBatchHeader;

			AssertEquals(testHeader.AH_TransactionNum, loadedBatchHeader.AH_TransactionNum);
			AssertEquals(LedgerTypes.CashBook, loadedBatchHeader.AH_Ledger);
			AssertEquals(TransactionTypes.DDRBatch, loadedBatchHeader.AH_TransactionType);
			AssertEquals(750.0m, loadedBatchHeader.AH_InvoiceAmount);
			AssertEquals(750.0m, loadedBatchHeader.AH_OSTotal);
			AssertEquals(750.0m, loadedBatchHeader.AH_OSExTaxAmount);
			AssertEquals(DDRBankAccount.PK, loadedBatchHeader.AH_AB);
			AssertEquals(DDRBankAccount.AB_RX_NKAccountCurrency, loadedBatchHeader.AH_RX_NKTransactionCurrency);

			AssertEquals(1m, loadedBatchHeader.AH_ExchangeRate);
			AssertEquals(testHeader.AH_TransactionNum, loadedBatchHeader.AH_ReceiptBatchNo);
		}

		[TestDate(2014, 11, 25)]
		public void TestAH_PostDate()
		{
			var prevCountry = GlbCompany.CurrentCompany.Country.Code;
			var prevValue = AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value;

			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
				ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
				DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

				SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 0, "", new ZDateTime(2014, 11, 5, 0, 0, 0));
				SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 0, "", new ZDateTime(2014, 11, 6, 0, 0, 0));
				SetUpDDRTransaction(directPayment, DDRBankAccount, 420m, 0, "", new ZDateTime(2014, 11, 9, 0, 0, 0), true);

				Factory.Save();

				DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

				testHeader.AH_AB = DDRBankAccount.PK;
				testHeader.AH_InvoiceAmount = 120.0m;
				testHeader.AH_OSTotal = 110.0m;
				testHeader.AH_ExchangeRate = 0.91m;
				testHeader.AH_PostDate = new ZDateTime(2014, 11, 1, 0, 0, 0);
				Assert("Should not allow a Past date", testHeader.AH_PostDateInfo.HasError("The post date cannot be in the past"));

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				testHeader.AH_PostDate = new ZDateTime(2014, 11, 7, 0, 0, 0);
				Assert("Should not allow this Past date as it is not later than the DirectPayment transaction", testHeader.AH_PostDateInfo.HasError("Post Date must be equal to or later than each transaction's Post Date."));

				testHeader.AH_PostDate = new ZDateTime(2014, 11, 10, 0, 0, 0);
				Assert("No Error as a valid Past date has been selected", testHeader.AH_PostDateInfo.GetErrors().Count() == 0);

				testHeader.AH_PostDate = ZDateTime.Today.AddDays(1).Date;
				Assert("Should not allow a future date", testHeader.AH_PostDateInfo.HasError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(prevCountry);
				AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestLinesIsRegisteredAsEditableChildObject()
		{
			DirectDebitBatchHeader header = Factory.New<DirectDebitBatchHeader>();
			AssertEquals("Lines should be registered as an editable child object", true, header.IsRegisteredEditableChildObject(header.Lines));
		}

		public void TestLoadExistingLineDetails()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			var aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 110m, "");
			SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 220m, "");
			SetUpDDRTransaction(directPayment, DDRBankAccount, 420m, 420m, "", null, true);

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			Assert(aPPayment.IncludeInTheBatch);
			Assert(aRPayment.IncludeInTheBatch);
			Assert(directPayment.IncludeInTheBatch);

			Factory.Save();

			var readFactory = new BusinessObjectFactory();

			DirectDebitBatchHeader loadedBatchHeader = readFactory.Load(typeof(DirectDebitBatchHeader), testHeader.PK) as DirectDebitBatchHeader;

			AssertEquals(750.0m, loadedBatchHeader.AH_InvoiceAmount);
			AssertEquals(750.0m, loadedBatchHeader.AH_OSTotal);
			AssertEquals(750.0m, loadedBatchHeader.AH_OSExTaxAmount);
			AssertEquals(1m, loadedBatchHeader.AH_ExchangeRate);

			AssertEquals(testHeader.AH_TransactionNum, loadedBatchHeader.AH_TransactionNum);
			AssertEquals(LedgerTypes.CashBook, loadedBatchHeader.AH_Ledger);
			AssertEquals(TransactionTypes.DDRBatch, loadedBatchHeader.AH_TransactionType);
			AssertEquals(DDRBankAccount.PK, loadedBatchHeader.AH_AB);
			AssertEquals(DDRBankAccount.AB_RX_NKAccountCurrency, loadedBatchHeader.AH_RX_NKTransactionCurrency);
			AssertEquals(testHeader.AH_TransactionNum, loadedBatchHeader.AH_ReceiptBatchNo);

			AssertEquals(3, loadedBatchHeader.Lines.Count);
			foreach (TransactionHeader line in loadedBatchHeader.Lines)
			{
				AssertEquals(testHeader.AH_TransactionNum, line.AH_ReceiptBatchNo);
				Assert(testHeader.PK != line.PK);
			}
		}

		public void TestGenerateNewHeaderDetailsForLocalAmount()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetUpDDRPayments();
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			AssertEquals(3, testHeader.Lines.Count);

			AssertEquals(750m, testHeader.AH_InvoiceAmount);
			AssertEquals(750m, testHeader.AH_OSTotal);
			AssertEquals(1m, testHeader.AH_ExchangeRate);

			AssertEquals(DDRBankAccount.PK, testHeader.AH_AB);
			AssertEquals(DDRBankAccount.AB_RX_NKAccountCurrency, testHeader.AH_RX_NKTransactionCurrency);
			AssertEquals(LedgerTypes.CashBook, testHeader.AH_Ledger);
			AssertEquals(TransactionTypes.DDRBatch, testHeader.AH_TransactionType);

			foreach (TransactionHeader line in testHeader.Lines)
			{
				AssertEquals(testHeader.AH_TransactionNum, line.AH_ReceiptBatchNo);
				Assert(testHeader.PK != line.PK);
			}
		}

		public void TestGenerateNewHeaderDetailsForForeignAmount()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 70m, "");
			SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 150m, "");
			SetUpDDRTransaction(directPayment, DDRBankAccount, 420m, 420m, "", null, true);

			SetExclusionForBatch();

			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			AssertEquals(3, testHeader.Lines.Count);

			AssertEquals(750m, testHeader.AH_InvoiceAmount);
			AssertEquals(640m, testHeader.AH_OSTotal);
			AssertEquals(0.853333m, testHeader.AH_ExchangeRate);

			AssertEquals(DDRBankAccount.PK, testHeader.AH_AB);
			AssertEquals(DDRBankAccount.AB_RX_NKAccountCurrency, testHeader.AH_RX_NKTransactionCurrency);
			AssertEquals(LedgerTypes.CashBook, testHeader.AH_Ledger);
			AssertEquals(TransactionTypes.DDRBatch, testHeader.AH_TransactionType);

			foreach (TransactionHeader line in testHeader.Lines)
			{
				AssertEquals(testHeader.AH_TransactionNum, line.AH_ReceiptBatchNo);
				Assert(testHeader.PK != line.PK);
			}
		}

		public void TestGenerateNewHeaderDetailsWithNoBankSet()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetUpDDRPayments();
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

			AssertEquals(0, testHeader.Lines.Count);

			AssertEquals(0m, testHeader.AH_InvoiceAmount);
			AssertEquals(0m, testHeader.AH_OSTotal);
			AssertEquals(1m, testHeader.AH_ExchangeRate);
		}

		public void TestReceiptTypeSetToDDR()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			DDRBankAccount.AB_ShowDetailsOnDirectDebits = true;

			SetUpDDRPayments();
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			foreach (TransactionHeader line in testHeader.Lines)
			{
				AssertEquals(ReceiptTypes.DirectDebit, line.AH_ReceiptType);
			}
		}

		public void TestReceiptTypeSetToDDL()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			DDRBankAccount.AB_ShowDetailsOnDirectDebits = false;

			SetUpDDRPayments();
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			foreach (TransactionHeader line in testHeader.Lines)
			{
				AssertEquals(ReceiptTypes.DirectDebitLine, line.AH_ReceiptType);
			}
		}

		public void TestReversingWithBankSetDDR()
		{
			// Set up Header and Line with DDR as AH_ReceiptType
			DDRBankAccount.AB_ShowDetailsOnDirectDebits = false;

			SetUpDDRPayments();
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			//Pre Condition
			foreach (TransactionHeader line in testHeader.Lines)
			{
				AssertEquals(testHeader.AH_TransactionNum, line.AH_ReceiptBatchNo);
			}

			string originalDesc = testHeader.AH_Desc;

			testHeader.GenerateReverseTransaction(true);
			((IReversing)testHeader).ReversingReason = "Test Reverse";

			Factory.Save();

			Assert(testHeader.AH_IsCancelled);
			AssertEquals(0m, testHeader.AH_InvoiceAmount);
			AssertEquals(0m, testHeader.AH_OSTotal);
			AssertEquals(originalDesc + " Test Reverse", testHeader.AH_Desc);

			foreach (TransactionHeader line in testHeader.Lines)
			{
				AssertEquals("", line.AH_ReceiptBatchNo);
				AssertEquals(ReceiptTypes.DirectDebit, line.AH_ReceiptType);
			}
		}

		protected override void AssertReversedAmountsCorrectlyNegated(TransactionHeader reversingHeader)
		{
			AssertEquals("Reverse OS Ex Tax Amount", 0m, reversingHeader.AH_OSExTaxAmount);
			AssertEquals("Reverse Local Ex Tax Amount", 0m, reversingHeader.AH_LocalExTaxAmount);
			AssertEquals("Reverse Local Invoice Amount - DB Field", 0m, reversingHeader.AH_InvoiceAmount);
			AssertEquals("Reverse OS Total Amount - DB Field", 0m, reversingHeader.AH_OSTotal);
			AssertEquals("Reverse Transaction Outstanding Amount", 0m, reversingHeader.AH_OutstandingAmount);
		}

		public override void TestSetTransactionBelongsToGroupField()
		{
			AssertEquals("Initial belongs to group field", ZGuid.Empty, Header.AH_TransactionBelongsToGroup);

			((IReversing)Header).GenerateReverseTransaction(true);
			TransactionHeader reversingHeader = (TransactionHeader)((IReversing)Header).ReverseTransaction;

			ZGuid groupingGuid = ZGuid.NewZGuid();
			((IReversing)Header).SetTransactionBelongsToGroupField(groupingGuid);

			AssertEquals("Default behaviour at the moment is to set transaction belongs to group to PK of header," +
				"leaving the original transactionbelongstogroup as NULL",
				ZGuid.Empty, reversingHeader.AH_TransactionBelongsToGroup);

			AssertEquals("Original transaction's transactionbelongstogroup field should be emtpy",
				ZGuid.Empty, Header.AH_TransactionBelongsToGroup);
		}

		public void TestValidatePayeeBankForPaymentNonASBBank()
		{
			string invalidBankBSBNumber = "123456";

			DDRBankAccount.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.ANZ;
			DDRBankAccount.AB_AccountNum = "123456789";

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			testOrg.OH_Code = "QWERTY";
			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = invalidBankBSBNumber;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = DDRBankAccount.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			testHeader.ValidateBeforeFileGeneration();

			string expectedError = DirectDebitBatchHeaderValidation.GetInvalidBSBNumberNonASBErrorMsg(testOrg.OH_Code);
			IDirectDebitBatchTransaction line = testHeader.Lines[0];

			AssertEquals(invalidBankBSBNumber, accountDetails.A1_BankBsb);
			AssertEquals(invalidBankBSBNumber, line.PayeeBankBSB);
			AssertEquals(true, line.PayeeBankBSBInfo.HasError(expectedError));
		}

		public void TestValidatePayeeBankForDirectPaymentNonASBBank()
		{
			string invalidBankBSBNumber = "123456";
			string transactionNum = "TEST001";

			DDRBankAccount.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.ANZ;
			DDRBankAccount.AB_AccountNum = "123456789";

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_AB = DDRBankAccount.PK;
			directPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			//DirectPayment.AH_Calc_IncludeInDDRFile = true;
			directPayment.AH_DrawerBranch = invalidBankBSBNumber;
			directPayment.AH_TransactionNum = transactionNum;
			directPayment.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			testHeader.ValidateBeforeFileGeneration();

			string expectedError = DirectDebitBatchHeaderValidation.GetInvalidBSBNumberNonASBDirectPaymentErrorMsg(transactionNum);
			IDirectDebitBatchTransaction line = testHeader.Lines[0];

			AssertEquals(invalidBankBSBNumber, line.PayeeBankBSB);
			AssertEquals(true, line.PayeeBankBSBInfo.HasError(expectedError));
		}

		public void TestValidatePayeeBankForPaymentASBBank()
		{
			string invalidBankBSBNumber = "123-456";

			DDRBankAccount.AB_AutoDDRFormat = "ASB";
			DDRBankAccount.AB_AccountNum = "123456789";

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			testOrg.OH_Code = "QWERTY";
			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = invalidBankBSBNumber;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = DDRBankAccount.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			testHeader.ValidateBeforeFileGeneration();

			string expectedError = DirectDebitBatchHeaderValidation.GetInvalidBSBNumberASBErrorMsg(testOrg.OH_Code);
			IDirectDebitBatchTransaction line = testHeader.Lines[0];

			AssertEquals(invalidBankBSBNumber, accountDetails.A1_BankBsb);
			AssertEquals(invalidBankBSBNumber, line.PayeeBankBSB);
			AssertEquals(true, line.PayeeBankBSBInfo.HasError(expectedError));
		}

		public void TestValidatePayeeBankForDirectPaymentASBBank()
		{
			string invalidBankBSBNumber = "123-456";
			string transactionNum = "TEST001";

			DDRBankAccount.AB_AutoDDRFormat = "ASB";
			DDRBankAccount.AB_AccountNum = "123456789";

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			//DirectPayment.AH_Calc_IncludeInDDRFile = true;
			directPayment.AH_AB = DDRBankAccount.PK;
			directPayment.AH_DrawerBranch = invalidBankBSBNumber;
			directPayment.AH_TransactionNum = transactionNum;
			directPayment.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			testHeader.ValidateBeforeFileGeneration();

			string expectedError = String.Format(DirectDebitBatchHeaderValidation.InvalidBSBNumberASBDirectPaymentErrorMsg, transactionNum, System.Environment.NewLine);
			IDirectDebitBatchTransaction line = testHeader.Lines[0];

			AssertEquals(invalidBankBSBNumber, line.PayeeBankBSB);
			AssertEquals(true, line.PayeeBankBSBInfo.HasError(expectedError));
		}

		public void TestIncludeInbatchingAllSelectedInInitialNew()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			SetUpDDRPayments();
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			foreach (IDirectDebitBatchTransaction line in testHeader.Lines)
			{
				Assert(line.IncludeInTheBatch);
			}
		}

		public void TestOnlySelectedTransactionUpdateTheBatchTotal()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			APPayment trans1 = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			ARPayment trans2 = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment trans3 = Factory.NewWithValidTestData(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			trans1.AH_OSExTaxAmount = 10m;
			trans2.AH_OSExTaxAmount = 20m;
			trans3.AH_OSExTaxAmount = 30m;

			testHeader.Lines.Add(trans1);
			testHeader.Lines.Add(trans2);
			testHeader.Lines.Add(trans3);

			trans1.IncludeInTheBatch = ZBool.True;
			trans2.IncludeInTheBatch = ZBool.True;
			trans3.IncludeInTheBatch = ZBool.True;

			AssertEquals(60m, testHeader.AH_OSExTaxAmount);

			trans2.IncludeInTheBatch = ZBool.False;
			AssertEquals(40m, testHeader.AH_OSExTaxAmount);

			trans1.IncludeInTheBatch = ZBool.False;
			AssertEquals(30m, testHeader.AH_OSExTaxAmount);

			trans3.IncludeInTheBatch = ZBool.False;
			AssertEquals(0m, testHeader.AH_OSExTaxAmount);
		}

		public override void TestAH_OSExTaxAmount()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var directPayment = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			directPayment.AH_AB = DDRBankAccount.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPayment.AH_OSTotal = -130m;
			directPayment.AH_InvoiceAmount = -120m;
			directPayment.AH_GSTAmount = -10m;
			directPayment.AH_PostDate = ZDateTime.Today;
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_LineAmount = -120m;
			directPayment.Lines[0].AL_OSAmount = -130m;
			directPayment.Lines[0].AL_GSTVAT = -10m;
			Factory.Save();

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			Assert(directPayment.IncludeInTheBatch);
			Factory.Save();

			var readFactory = new BusinessObjectFactory();
			var loadedBatchHeader = readFactory.Load(typeof(DirectDebitBatchHeader), testHeader.PK) as DirectDebitBatchHeader;
			AssertEquals(130.0m, loadedBatchHeader.AH_InvoiceAmount);
			AssertEquals(130.0m, loadedBatchHeader.AH_OSTotal);
			AssertEquals(130.0m, loadedBatchHeader.AH_OSExTaxAmount);
		}

		public override void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			var exchangeRate = 0.57m;
			var osAmount = 200.453m;
			var osTaxAmount = 15.02m;
			var localAmount = Math.Round((osAmount / exchangeRate), 2);
			var localTaxAmount = Math.Round((osTaxAmount / exchangeRate), 2);
			var osTotalAmount = osAmount + osTaxAmount;

			LoadForeignCurrency(1000);
			Header.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Header.AH_ExchangeRate = exchangeRate;
			Header.AH_OSExTaxAmount = osTotalAmount;

			SetupForSave();

			var foreignBankAccount = TestObjectCreator.CreateBankAccount("SSS", "Foreign Bank", ForeignCurrency, TestObjectCreator.GLHeader1);

			var directPayment = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			directPayment.AH_AB = foreignBankAccount.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPayment.AH_OSTotal = -osTotalAmount;
			directPayment.AH_InvoiceAmount = -localAmount;
			directPayment.AH_GSTAmount = -localTaxAmount;
			directPayment.AH_ExchangeRate = exchangeRate;
			directPayment.AH_PostDate = ZDateTime.Today;
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_ExchangeRate = exchangeRate;
			directPayment.Lines[0].AL_LineAmount = -localAmount;
			directPayment.Lines[0].AL_OSAmount = -osTotalAmount;
			directPayment.Lines[0].AL_GSTVAT = -localTaxAmount;
			directPayment.IncludeInTheBatch = true;

			((DirectDebitBatchHeader)Header).Lines.Add(directPayment);

			Header.Factory.Save();

			var newFactoryForLoad = new BusinessObjectFactory();

			var loadedHeader = (TransactionHeader)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Header.PK);

			AssertEquals("OS Ex Tax Amount on Load", osAmount + osTaxAmount, loadedHeader.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 0m, loadedHeader.AH_OSTaxAmount);
		}

		public override void TestOSOutstandingAmountMatching()
		{
			Assert("You cannot match direct debit batches", true);
		}

		public void TestOnlySelectedTransactionsAreBatched()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment testDirectPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 70m, "");
			SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 80m, "");
			SetUpDDRTransaction(testDirectPayment, DDRBankAccount, 420m, 420m, "", null, true);

			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			IDirectDebitBatchTransaction excludedTransaction = testHeader.Lines[1];
			excludedTransaction.IncludeInTheBatch = ZBool.False;

			Factory.Save();

			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();
			DirectDebitBatchHeader loadedBatchHeader = readOnlyFactory.Load(typeof(DirectDebitBatchHeader), testHeader.PK) as DirectDebitBatchHeader;

			AssertEquals(750.0m - excludedTransaction.AH_LocalExTaxAmount, loadedBatchHeader.AH_InvoiceAmount);
			AssertEquals(570.0m - excludedTransaction.AH_OSExTaxAmount, loadedBatchHeader.AH_OSTotal);

			AssertEquals(2, loadedBatchHeader.Lines.Count);
			foreach (TransactionHeader line in loadedBatchHeader.Lines)
			{
				AssertEquals(testHeader.AH_TransactionNum, line.AH_ReceiptBatchNo);
				Assert(testHeader.PK != line.PK);
				Assert(((BusinessObject)excludedTransaction).PK != line.PK);
			}
		}

		public void TestAmountCalculation()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			Assert(!testHeader.HasChanges);

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 70m, "");
			SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 80m, "");
			SetUpDDRTransaction(directPayment, DDRBankAccount, 420m, 310m, "", null, true);

			testHeader.AH_AB = DDRBankAccount.PK;

			foreach (IDirectDebitBatchTransaction line in testHeader.Lines)
			{
				Assert(line.IncludeInTheBatch);
			}

			foreach (IDirectDebitBatchTransaction line in testHeader.Lines)
			{
				line.IncludeInTheBatch = ZBool.False;
			}

			AssertEquals(0m, testHeader.AH_OSExTaxAmount);

			testHeader.Lines[1].IncludeInTheBatch = ZBool.True;
			Assert(testHeader.AH_OSExTaxAmount != 0m);
		}

		public void TestRegenerateFileValidateBankForAutoDDR()
		{
			SetUpDDRPayments();
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			DDRBankAccount.AB_AllowAutoDDR = false;

			testHeader.ValidateBeforeFileGeneration();

			AssertHasError(testHeader.AH_ABInfo, "You cannot create DDR file for non-auto DDR Bank. Please check the Bank Master file.");
			DDRBankAccount.AB_AllowAutoDDR = true;

			testHeader.ValidateBeforeFileGeneration();
			AssertNoError(testHeader.AH_ABInfo, "You cannot create DDR file for non-auto DDR Bank. Please check the Bank Master file.");
		}

		public void TestBankAccountNumberLengthValidationForASBBank()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			DDRBankAccount.AB_AllowAutoDDR = ZBool.True;
			DDRBankAccount.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.ASB;
			DDRBankAccount.AB_AccountNum = "123456789";

			testHeader.ValidateBeforeFileGeneration();
			AssertNoError(testHeader.AH_ABInfo, "The Bank Account Number setup must be nine characters in length");

			DDRBankAccount.AB_AccountNum = "123456";
			testHeader.ValidateBeforeFileGeneration();
			AssertHasError(testHeader.AH_ABInfo, "The Bank Account Number setup must be nine characters in length");

			DDRBankAccount.AB_AccountNum = "1234567890123";
			testHeader.ValidateBeforeFileGeneration();
			AssertHasError(testHeader.AH_ABInfo, "The Bank Account Number setup must be nine characters in length");
		}

		public void TestBankAccountNumberLengthValidationForNonASBBank()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			DDRBankAccount.AB_AllowAutoDDR = ZBool.True;
			DDRBankAccount.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.ANZ;
			DDRBankAccount.AB_AccountNum = "123456789";

			testHeader.ValidateBeforeFileGeneration();
			AssertNoError(testHeader.AH_ABInfo, "The Bank Account Number setup must be nine characters or less");

			DDRBankAccount.AB_AccountNum = "123456";
			testHeader.ValidateBeforeFileGeneration();
			AssertNoError(testHeader.AH_ABInfo, "The Bank Account Number setup must be nine characters or less");

			DDRBankAccount.AB_AccountNum = "1234567890123";
			testHeader.ValidateBeforeFileGeneration();
			AssertHasError(testHeader.AH_ABInfo, "The Bank Account Number setup must be nine characters or less");
		}

		public void TestRolledUpDDBMarkedDifferently()
		{
			SetUpDDRPayments();
			Factory.Save();

			DDRBankAccount.AB_ShowDetailsOnDirectDebits = false;
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			Assert(testHeader.IsRolledUp);
		}

		public void TestNonRolledUpDDBMarkedDifferently()
		{
			SetUpDDRPayments();
			Factory.Save();

			DDRBankAccount.AB_ShowDetailsOnDirectDebits = true;
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			Assert(!testHeader.IsRolledUp);
		}

		public void TestChequeOrReferenceCopiedFromTransactionIfEmpty()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			Assert(!testHeader.AH_ChequeOrReference.IsEmpty);
			AssertEquals(testHeader.AH_TransactionNum, testHeader.AH_ChequeOrReference);
		}

		public void TestChequeOrReferenceNOTCopiedFromTransactionIfItisNotEmpty()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			testHeader.AH_ChequeOrReference = "TESTCHQ001";
			testHeader.AH_TransactionNum = "TRN1001";

			Factory.Save();

			Assert(testHeader.AH_ChequeOrReference != testHeader.AH_TransactionNum);
			AssertEquals("TESTCHQ001", testHeader.AH_ChequeOrReference);
		}

		public void TestValidateAutoDDRBank()
		{
			AccBankAccount dDRBank = TestObjectCreator.AUDBankAccount;
			dDRBank.AB_AllowAutoDDR = true;
			dDRBank.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.ANZ;

			OrgHeader dDRAccount = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = dDRAccount.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankName = "Test";
			accountDetails.A1_BankBsb = "132456";
			accountDetails.A1_BankAccount = "192837465";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			OrgHeader nonDDRAccount = TestObjectCreator.ABIGAS;
			accountDetails.A1_AccountName = "";

			APPayment dDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			APPayment nonDDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;

			dDRPayment.AH_AB = dDRBank.PK;
			dDRPayment.AH_OH = dDRAccount.PK;

			nonDDRPayment.AH_AB = dDRBank.PK;
			nonDDRPayment.AH_OH = nonDDRAccount.PK;

			DirectDebitBatchHeader testBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testBatch.AH_AB = dDRBank.PK;
			testBatch.Lines.Add(dDRPayment);
			testBatch.Lines.Add(nonDDRPayment);

			dDRPayment.IncludeInTheBatch = true;
			nonDDRPayment.IncludeInTheBatch = true;

			testBatch.DDRValidation_ForTestOnly.ValidateBeforePosting();

			Assert(testBatch.HasErrors);
		}

		public void TestOnlyAutoDDRBankRaisesFileGenerationEvent()
		{
			DDRFileEventRaised = false;
			SetUpDDRPayments();
			Factory.Save();

			DDRBankAccount.AB_ShowDetailsOnDirectDebits = true;
			DDRBankAccount.AB_AllowAutoDDR = true;
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.GenerateDDRFileEvent += new EventHandler(TestHeader_GenerateDDRFileEvent);
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			Assert(DDRFileEventRaised);
		}

		public void TestNonAutoDDRBankDoNotRaisesFileGenerationEvent()
		{
			DDRFileEventRaised = false;
			SetUpDDRPayments();
			Factory.Save();

			DDRBankAccount.AB_ShowDetailsOnDirectDebits = true;
			DDRBankAccount.AB_AllowAutoDDR = false;
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.GenerateDDRFileEvent += new EventHandler(TestHeader_GenerateDDRFileEvent);
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			Assert(!DDRFileEventRaised);
		}

		public void TestValidateEmptyBankAccount()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

			testHeader.Validation.ValidateAH_AB();
			AssertHasError(testHeader.AH_ABInfo, "Please enter a " + testHeader.AH_ABInfo.Description + ".");
		}

		public void TestGetDDRFileGenerator()
		{
			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			StringWriter writer = new StringWriter();
			DDRFileGenerator generator = dDRHeader.GetDDRFileGenerator_ForTestOnly(writer, Core.Constants.DDRFileFormat.BBL);
			AssertEquals("Generator should be a BBLDDRFileGenerator", typeof(BBLDDRFileGenerator), generator.GetType());

			generator = dDRHeader.GetDDRFileGenerator_ForTestOnly(writer, Core.Constants.DDRFileFormat.WNZ);
			AssertEquals("Generator should be a WNZDDRFileGenerator", typeof(WNZDDRFileGenerator), generator.GetType());

			generator = dDRHeader.GetDDRFileGenerator_ForTestOnly(writer, Core.Constants.DDRFileFormat.ANZ);
			AssertEquals("Generator should be ANZ Australia file generator", typeof(DDRFileGenerator), generator.GetType());

			generator = dDRHeader.GetDDRFileGenerator_ForTestOnly(writer, Core.Constants.DDRFileFormat.HSB);
			AssertEquals("Generator should be HSBC file generator", typeof(HSBCDDRFileGenerator), generator.GetType());

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			generator = dDRHeader.GetDDRFileGenerator_ForTestOnly(writer, Core.Constants.DDRFileFormat.ANZ);
			AssertEquals("Generator should be ANZ New Zealand file generator", typeof(ANZNewZealandFileGenerator), generator.GetType());

			generator = dDRHeader.GetDDRFileGenerator_ForTestOnly(writer, Core.Constants.DDRFileFormat.BCS);
			AssertEquals("Generator should be a BCSDDRFileGenerator", typeof(BCSDDRFileGenerator), generator.GetType());
		}

		public void TestGSTIncludedInBatch()
		{
			bool isReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;

			try
			{
				DirectPayment.DirectPayment directPayment1 = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
				DirectPayment.DirectPayment directPayment2 = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
				DirectPayment.DirectPayment directPayment3 = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

				directPayment1.AH_AB = DDRBankAccount.PK;
				directPayment2.AH_AB = DDRBankAccount.PK;
				directPayment3.AH_AB = DDRBankAccount.PK;

				directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
				directPayment2.AH_ReceiptType = ReceiptTypes.DirectDebit;
				directPayment3.AH_ReceiptType = ReceiptTypes.DirectDebit;

				DirectPayment.DirectPaymentLine testLine = (DirectPayment.DirectPaymentLine)directPayment1.Lines.AddNew();
				testLine.AL_OSExTaxAmount = 100m;
				testLine.AL_AT = TestObjectCreator.GST1.PK;

				testLine = (DirectPayment.DirectPaymentLine)directPayment2.Lines.AddNew();
				testLine.AL_OSExTaxAmount = 150m;
				testLine.AL_AT = TestObjectCreator.GST1.PK;

				testLine = (DirectPayment.DirectPaymentLine)directPayment3.Lines.AddNew();
				testLine.AL_OSExTaxAmount = 40m;
				testLine.AL_AT = TestObjectCreator.GST1.PK;

				Factory.Save();

				DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
				testHeader.AH_AB = DDRBankAccount.PK;

				Factory.Save();

				AssertEquals(3, testHeader.Lines.Count);

				AssertEquals(319m, testHeader.AH_InvoiceAmount);
				AssertEquals(319m, testHeader.AH_OSTotal);
				AssertEquals(0m, testHeader.AH_GSTAmount);
				AssertEquals(1m, testHeader.AH_ExchangeRate);

				AssertEquals(DDRBankAccount.PK, testHeader.AH_AB);
				AssertEquals(DDRBankAccount.AB_RX_NKAccountCurrency, testHeader.AH_RX_NKTransactionCurrency);
				AssertEquals(LedgerTypes.CashBook, testHeader.AH_Ledger);
				AssertEquals(TransactionTypes.DDRBatch, testHeader.AH_TransactionType);

				foreach (TransactionHeader line in testHeader.Lines)
				{
					AssertEquals(testHeader.AH_TransactionNum, line.AH_ReceiptBatchNo);
					Assert(testHeader.PK != line.PK);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = isReciprocal;
			}
		}

		public void TesttSuspendSettingLineBatchNoIsFalseByDefault()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			Assert(!testHeader.FIsSettingLineBatchNo_ForTestOnly);
		}

		public void TestSetSuspendSettingLineBatchNo()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 70m, "");
			SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 150m, "");
			SetUpDDRTransaction(directPayment, DDRBankAccount, 420m, 310m, "", null, true);

			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			testHeader.SuspendSettingLineBatchNo();

			Factory.Save();

			BusinessObjectFactory readFactory = new BusinessObjectFactory();
			DirectDebitBatchHeader loadedBatchHeader = readFactory.Load(typeof(DirectDebitBatchHeader), testHeader.PK) as DirectDebitBatchHeader;

			AssertEquals(0, loadedBatchHeader.Lines.Count);
		}

		protected override void AssertReverseTransactionDueDateValue(TransactionHeader reverseHeader)
		{
			AssertEquals("Due Date should be empty", ZDateTime.Empty, reverseHeader.AH_DueDate.Date);
		}

		public void TestDocManagerCode()
		{
			DirectDebitBatchHeader dDRBatchHeader = Factory.New<DirectDebitBatchHeader>();
			AssertType(typeof(DirectDebitBatchHeaderDocManagerInfo), dDRBatchHeader.DocManagerInfo);
			AssertEquals("Code should be DDR. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "DDR", dDRBatchHeader.DocManagerInfo.DocManagerCode);
		}

		public void TestReverseDoNotResetTransactionBankDetailsForDPY()
		{
			ZString chequeDrawer = "ChqDrawer";
			ZString drawerBank = "DrawerBank";
			ZString drawerBranch = "DrawerBranch";
			DDRBankAccount.AB_ShowDetailsOnDirectDebits = false;

			DirectPayment.DirectPayment directPayment = Factory.New<DirectPayment.DirectPayment>();
			directPayment.AH_AB = DDRBankAccount.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			directPayment.AH_DrawerBank = drawerBank;
			directPayment.AH_DrawerBranch = drawerBranch;
			directPayment.AH_ChequeDrawer = chequeDrawer;

			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			//Pre Condition
			AssertEquals(1, testHeader.Lines.Count);
			foreach (TransactionHeader line in testHeader.Lines)
			{
				AssertEquals(testHeader.AH_TransactionNum, line.AH_ReceiptBatchNo);
			}

			string originalDesc = testHeader.AH_Desc;

			testHeader.GenerateReverseTransaction(true);
			((IReversing)testHeader).ReversingReason = "Test Reverse";

			Factory.Save();

			foreach (TransactionHeader line in testHeader.Lines)
			{
				AssertEquals("", line.AH_ReceiptBatchNo);
				AssertEquals(ReceiptTypes.DirectDebit, line.AH_ReceiptType);
				AssertEquals(drawerBank, line.AH_DrawerBank);
				AssertEquals(drawerBranch, line.AH_DrawerBranch);
				AssertEquals(chequeDrawer, line.AH_ChequeDrawer);
			}
		}

		public void TestHumanReadableName()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			AssertEquals("Direct Debit Batch", testHeader.HumanReadableName);
		}

		public void TestRelatedTransactionPK()
		{
			var header = Factory.New<DirectDebitBatchHeader>();
			AssertEquals("RelatedTransactionPK", ZGuid.Empty, header.RelatedTransactionPK);

			ZGuid guid = ZGuid.NewZGuid();
			header.RelatedTransactionPK = guid;
			AssertEquals("RelatedTransactionPK", guid, header.RelatedTransactionPK);
		}

		public void TestIncludeInbatchBecomesReadOnlyCorrectly()
		{
			SetUpDDRPayments();
			Factory.Save();

			DDRBankAccount.AB_ShowDetailsOnDirectDebits = true;
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;

			Assert(!testHeader.IsInDatabase);
			AssertEquals("Number of TransactionLines before Saving", 3, testHeader.Lines.Count);
			Assert("'IncludeInTheBatch' column of Transaction Line 1 should not be read only before saving", !testHeader.Lines[0].IncludeInTheBatchInfo.ReadOnly);
			Assert("'IncludeInTheBatch' column of Transaction Line 2 should not be read only before saving", !testHeader.Lines[1].IncludeInTheBatchInfo.ReadOnly);
			Assert("'IncludeInTheBatch' column of Transaction Line 3 should not be read only before saving", !testHeader.Lines[2].IncludeInTheBatchInfo.ReadOnly);

			testHeader.Lines[1].IncludeInTheBatch = false;
			testHeader.Lines[2].IncludeInTheBatch = false;
			Factory.Save();

			testHeader.Lines.Load();
			AssertEquals("Number of TransactionLines After Saving", 1, testHeader.Lines.Count);
			Assert("'IncludeInTheBatch' column of Transaction Line 1 should be read only after saving", testHeader.Lines[0].IncludeInTheBatchInfo.ReadOnly);

			DDRBankAccount.AB_ShowDetailsOnDirectDebits = true;
			DirectDebitBatchHeader testHeader1 = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader1.AH_AB = DDRBankAccount.PK;
			AssertEquals("Number of TransactionLines before Saving", 2, testHeader1.Lines.Count);
			Assert("'IncludeInTheBatch' column of Transaction Line 1 should not be read only before saving", !testHeader1.Lines[0].IncludeInTheBatchInfo.ReadOnly);
			Assert("'IncludeInTheBatch' column of Transaction Line 2 should not be read only before saving", !testHeader1.Lines[1].IncludeInTheBatchInfo.ReadOnly);
		}

		#region TestOnSaving_ReverseDDRBatchHasTheSamePostDateAsReversePayment

		public void TestOnSaving_ReverseDDRBatchHasTheSamePostDateAsReversePayment()
		{
			BusinessObjectFactory tempFactory = new BusinessObjectFactory();

			ARPayment testPayment = tempFactory.NewWithValidTestData<ARPayment>();
			testPayment.AH_ReceiptBatchNo = "1234";
			tempFactory.Save();

			PaymentReversing reversing = new PaymentReversing(testPayment);
			reversing.Reverse();
			ARPayment reversePayment = testPayment.ReverseTransaction as ARPayment;
			reversePayment.AH_PostDate = ZDateTime.Now.AddDays(-2);
			tempFactory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.DDRBatch);
			DirectDebitBatchHeader reverseDDRBatch = tempFactory.LoadTop1<DirectDebitBatchHeader>(filter);
			AssertEquals("AH_PostDate", reversePayment.AH_PostDate, reverseDDRBatch.AH_PostDate);
		}

		public void TestReverseDDRBatchHasExchangeRateSetCorrectly()
		{
			BusinessObjectFactory tempFactory = new BusinessObjectFactory();

			TestObjectCreator testObjectCreator = new TestObjectCreator(tempFactory);

			APPayment testPayment = tempFactory.NewWithValidTestData<APPayment>();
			testPayment.AH_ReceiptBatchNo = "1234";
			//TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_AB = testObjectCreator.AUDBankAccount.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			testPayment.AH_ExchangeRate = 0.9m;
			testPayment.AH_OSExTaxAmount = 100m;
			tempFactory.Save();

			PaymentReversing reversing = new PaymentReversing(testPayment);
			reversing.Reverse();
			APPayment reversePayment = testPayment.ReverseTransaction as APPayment;
			reversePayment.AH_PostDate = ZDateTime.Now.AddDays(-2);
			tempFactory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.DDRBatch);
			DirectDebitBatchHeader reverseDDRBatch = tempFactory.LoadTop1<DirectDebitBatchHeader>(filter);
			AssertEquals("Exchange rate", 0.900009m, reverseDDRBatch.AH_ExchangeRate);
		}

		#endregion

		void SetUpDDRPayments()
		{
			// Inclusion Case
			SetInclusionCaseForBatch();

			// Exclusion Case
			SetExclusionForBatch();
		}

		void SetExclusionForBatch()
		{
			APPayment aPPaymentWithDifferentBank = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			ARPayment aRPaymentAlreadyBatched = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPaymentWithDifferentCompany = Factory.NewWithValidTestData(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			APPayment aPPaymentCancelled = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			APInvoice aPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;

			SetUpDDRTransaction(aPInvoice, DDRBankAccount, 1000m, 0, "");
			SetUpDDRTransaction(aPPaymentWithDifferentBank, TestObjectCreator.USDBankAccount, 1000m, 0, "");
			SetUpDDRTransaction(aRPaymentAlreadyBatched, DDRBankAccount, 1000m, 0, "");
			SetUpDDRTransaction(directPaymentWithDifferentCompany, DDRBankAccount, 1000m, 0, "", null, true);
			SetUpDDRTransaction(aPPaymentCancelled, DDRBankAccount, 1000m, 0, "");

			aRPaymentAlreadyBatched.AH_ReceiptBatchNo = "Batched";
			directPaymentWithDifferentCompany.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			directPaymentWithDifferentCompany.Lines[0].AL_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			aPPaymentCancelled.AH_IsCancelled = ZBool.True;
			TransactionMatchLink matchLink = ((IMatching)aPPaymentCancelled).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = aPPaymentCancelled.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			//MatchLink.AP_Amount = APPaymentCancelled.AH_OutstandingAmount;
		}

		void SetInclusionCaseForBatch()
		{
			APPayment aPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment testDirectPayment = Factory.NewWithValidTestData(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 110m, "");
			SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 220m, "");
			SetUpDDRTransaction(testDirectPayment, DDRBankAccount, 420m, 420m, "", null, true);
		}

		static void CreateNewLineForDirectPayment(DirectPayment.DirectPayment directPayment, decimal osExTaxAmount)
		{
			directPayment.Lines.AddNew();
			DirectPayment.DirectPaymentLine line = (DirectPayment.DirectPaymentLine)directPayment.Lines[0];
			line.AL_OSExTaxAmount = osExTaxAmount;
		}

		public void SetUpDDRTransaction(TransactionHeader payment, AccBankAccount dDRBankAccount, decimal amount, decimal foreignAmount, ZString batchNo, ZDateTime? postDate = null, bool isDirectPayment = false)
		{
			payment.AH_AB = dDRBankAccount.PK;

			payment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			payment.AH_GB = GlbBranch.CurrentBranch.PK;
			payment.AH_ReceiptBatchNo = batchNo;
			if (foreignAmount != 0m && foreignAmount != amount)
			{
				payment.AH_OSExTaxAmount = foreignAmount;
				payment.AH_OSTotal = foreignAmount;
				payment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				payment.AH_ExchangeRate = foreignAmount / amount;
			}
			else
			{
				payment.AH_OSExTaxAmount = amount;
				payment.AH_OSTotal = amount;
			}
			payment.AH_LocalExTaxAmount = amount;

			payment.AH_DrawerBank = DrawerBank;
			payment.AH_DrawerBranch = DrawerBranch;
			payment.AH_ChequeDrawer = ChequeDrawer;
			if (postDate.HasValue)
			{
				payment.AH_PostDate = postDate.Value;
			}

			if (isDirectPayment)
			{
				CreateNewLineForDirectPayment((DirectPayment.DirectPayment)payment, amount);
			}
		}

		public new void TestReverseTransactionSetComplianceSubTypeForPeru()
		{
			Assert(true);
		}

		public new void TestReverseTransactionDoeNotSetComplianceSubTypeForNonPeru()
		{
			Assert(true);
		}

		public override void TestGenerateReverseTransaction()
		{
			DirectDebitBatchHeader testHeader = Factory.New<DirectDebitBatchHeader>();
			testHeader.AH_ExchangeRate = 0.8M;
			testHeader.AH_OSExTaxAmount = 500M;
			testHeader.AH_OSTaxAmount = 50M;
			testHeader.AH_LocalExTaxAmount = 630M;
			testHeader.AH_LocalTaxAmount = 64M;
			APPayment line1 = Factory.New<APPayment>();
			testHeader.Lines.Add(line1);
			line1.AH_ReceiptBatchNo = "1";
			line1.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;

			testHeader.GenerateReverseTransaction(true);
			DirectDebitBatchHeader reversedHeader = (DirectDebitBatchHeader)testHeader.ReverseTransaction;
			AssertEquals("Reverse transaction should be the base transaction", reversedHeader, testHeader);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_OSExTaxAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_LocalExTaxAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_LocalTaxAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_OSTaxAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_OutstandingAmount);
			AssertEquals(ZDecimal.Zero, reversedHeader.AH_WithholdingTax);
			AssertEquals(0.8M, reversedHeader.AH_ExchangeRate);
			Assert(reversedHeader.IsReverseTransaction);
			AssertEquals(reversedHeader.OriginalTransaction, testHeader);
			AssertEquals(ZString.Empty, line1.AH_ReceiptBatchNo);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.DirectDebit, line1.AH_ReceiptType);
		}

		bool DDRFileEventRaised;
		void TestHeader_GenerateDDRFileEvent(object sender, EventArgs e)
		{
			DDRFileEventRaised = true;
		}

		protected override bool AreOriginalAndReverseTransactionsTheSame
		{
			get { return true; }
		}

		public void TestReverseDDRBatch_AH_OSTotal()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");
			directDebitBatchTransactionMock.Setup(x => x.AH_OSTotalAmount).Returns(220);

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(-220m, reverseTransaction.AH_OSTotal);
		}

		public void TestReverseDDRBatch_AH_InvoiceAmount()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");
			directDebitBatchTransactionMock.Setup(x => x.AH_LocalExTaxAmount).Returns(100);
			directDebitBatchTransactionMock.Setup(x => x.AH_LocalTaxAmount).Returns(10);
			directDebitBatchTransactionMock.Setup(x => x.AH_OSExTaxAmount).Returns(200);
			directDebitBatchTransactionMock.Setup(x => x.AH_OSTaxAmount).Returns(20);
			directDebitBatchTransactionMock.Setup(x => x.AH_OSTotalAmount).Returns(220);

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(-110m, reverseTransaction.AH_InvoiceAmount);
		}

		public void TestReverseDDRBatch_ExchangeRateWhenOriginalPaymentHasTax()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");
			directDebitBatchTransactionMock.Setup(x => x.AH_LocalExTaxAmount).Returns(100);
			directDebitBatchTransactionMock.Setup(x => x.AH_LocalTaxAmount).Returns(10);
			directDebitBatchTransactionMock.Setup(x => x.AH_OSExTaxAmount).Returns(200);
			directDebitBatchTransactionMock.Setup(x => x.AH_OSTaxAmount).Returns(20);
			directDebitBatchTransactionMock.Setup(x => x.AH_OSTotalAmount).Returns(220);

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(2m, reverseTransaction.AH_ExchangeRate);
		}

		public void TestReverseDDRBatch_IsReverseTransaction()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(true, reverseTransaction.IsReverseTransaction);
		}

		public void TestReverseDDRBatch_OriginalTransaction()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(directDebitBatchTransactionMock.Object as TransactionHeader, reverseTransaction.OriginalTransaction);
		}

		public void TestReverseDDRBatch_AH_RX_NKTransactionCurrency()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			var testObjectCreator = new TestObjectCreator(Factory);
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");
			directDebitBatchTransactionMock.Setup(x => x.AH_AB).Returns(testObjectCreator.USDBankAccount.PK);

			var reverseTransactionWithUSDBank = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(testObjectCreator.USDBankAccount, reverseTransactionWithUSDBank.BankAccount);
			AssertEquals("USD", reverseTransactionWithUSDBank.AH_RX_NKTransactionCurrency);

			directDebitBatchTransactionMock.Setup(x => x.AH_AB).Returns(testObjectCreator.AUDBankAccount.PK);
			var reverseTransactionWithAUDBank = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(testObjectCreator.AUDBankAccount, reverseTransactionWithAUDBank.BankAccount);
			AssertEquals("AUD", reverseTransactionWithAUDBank.AH_RX_NKTransactionCurrency);
		}

		public void TestReverseDDRBatch_AH_RX_NKTransactionCurrencyWhenBankAccountIsNull()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			var testObjectCreator = new TestObjectCreator(Factory);
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "USD";
			var reverseTransactionWithUSDBank = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(null, reverseTransactionWithUSDBank.BankAccount);
			AssertEquals("USD", reverseTransactionWithUSDBank.AH_RX_NKTransactionCurrency);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			var reverseTransactionWithAUDBank = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(null, reverseTransactionWithAUDBank.BankAccount);
			AssertEquals("AUD", reverseTransactionWithAUDBank.AH_RX_NKTransactionCurrency);
		}

		[TestDate(2022, 08, 26, 0, 0, 0)]
		public void TestReverseDDRBatch_AH_PostDateAndAH_InvoiceDateAndAH_FullyPaidDateShouldBeNow()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(ZDateTime.Now, reverseTransaction.AH_PostDate);
			AssertEquals(ZDateTime.Now, reverseTransaction.AH_InvoiceDate);
			AssertEquals(ZDateTime.Now, reverseTransaction.AH_FullyPaidDate);
		}

		public void TestReverseDDRBatch_AH_AB()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");
			directDebitBatchTransactionMock.Setup(x => x.AH_AB).Returns(new ZGuid("e03f70f4-d391-4e1e-a913-e03d4a2a63cd"));

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals("e03f70f4-d391-4e1e-a913-e03d4a2a63cd", reverseTransaction.AH_AB.ToString());
		}

		public void TestReverseDDRBatch_AH_OH()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");
			directDebitBatchTransactionMock.Setup(x => x.AH_OH).Returns(new ZGuid("e7eca40c-24ec-48a7-983b-3c50bbf12002"));

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals("e7eca40c-24ec-48a7-983b-3c50bbf12002", reverseTransaction.AH_OH.ToString());
		}

		public void TestReverseDDRBatch_AH_Desc()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO1234");

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals("Cancellation of BNO1234", reverseTransaction.AH_Desc);
		}

		public void TestReverseDDRBatch_AH_ReceiptTypeWhenOriginalReceiptTypeIsDDL()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO0001");
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptType).Returns(ReceiptTypes.DirectDebitLine);

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(ReceiptTypes.DirectDebit, reverseTransaction.AH_ReceiptType);
		}

		public void TestReverseDDRBatch_AH_ReceiptTypeWhenOriginalReceiptTypeIsDDR()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO0001");
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptType).Returns(ReceiptTypes.DirectDebit);

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(ReceiptTypes.NonRolledUpBatch, reverseTransaction.AH_ReceiptType);
		}

		public void TestReverseDDRBatch_AH_ReceiptTypeWhenOriginalReceiptTypeIsEND()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns("BNO0001");
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptType).Returns(ReceiptTypes.eNettDirectDebit);

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(ReceiptTypes.eNettDirectDebit, reverseTransaction.AH_ReceiptType);
		}

		public void TestReverseDDRBatch_WhenAH_ReceiptBatchNoIsEmpty()
		{
			var directDebitBatchTransactionMock = new Mock<IDirectDebitBatchTransaction>();
			directDebitBatchTransactionMock.Setup(x => x.Factory).Returns(Factory);
			directDebitBatchTransactionMock.Setup(x => x.AH_ReceiptBatchNo).Returns(ZString.Empty);

			var reverseTransaction = DirectDebitBatchHeader.ReverseDDRBatch(directDebitBatchTransactionMock.Object);
			AssertEquals(null, reverseTransaction);
		}

		#region DDR File generation

		public void TestCreateFile()
		{
			var header = Factory.New<DirectDebitBatchHeader>();
			using (var tempFile = TempFile.New())
			{
				header.AH_AB = BankAccount.PK;
				header.CreateFile(tempFile.Filename, () => ZSaveFileDialog.OpenFile(tempFile.Filename));
				AssertEquals("File should be created", true, File.ReadAllText(tempFile.Filename).IndexOf("Eagle Datamation00000000") != -1);

				var storageFile = (BusinessObject)header.DocManagerInfo.Files[0];
				var storageFileAsAttachment = (Enterprise.Integration.DocumentEngine.IDeliveryEmailAttachment)storageFile;
				var docType = (RefDocType)storageFile["DocType"];
				Assert("StorageFile should be saved", storageFile.IsInDatabase);
				AssertEquals("RefDocType should exist and be of the correct code", "DDR", docType.RT_DocType);
				AssertGreaterThan("StorageFile should be larger than zero bytes.", storageFileAsAttachment.FileSizeInBytes, 0);
				var expectedFileName = Path.GetFileName(tempFile.Filename);
				AssertEquals("StorageFile Name be the temp file name, without path.", expectedFileName, storageFileAsAttachment.FileName);

				var ddrEventLogs = header.Logs.Find(l => l.Event.SE_Code == Events.EditedARecord.Code && l.ReferenceFreeText == "DDR File Generated.");
				AssertEquals("Exactly one Event Log for DDR file should be created.", 1, ddrEventLogs.Count());
				Assert("Event Log should be saved.", ddrEventLogs.First().IsInDatabase);
			}
		}

		public void TestAddEventLogAndAttachDDRFileToEDocsAndSave_CustomFileFormat()
		{
			AssertAddEventLogAndAttachDDRFileToEDocsAndSave("Test1.png");
			AssertAddEventLogAndAttachDDRFileToEDocsAndSave("Test2");
			AssertAddEventLogAndAttachDDRFileToEDocsAndSave("Test3.csv");

			void AssertAddEventLogAndAttachDDRFileToEDocsAndSave(string expectedDDRFileName)
			{
				var header = Factory.New<DirectDebitBatchHeader>();
				var data = new MemoryStream(new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 });
				header.AH_AB = BankAccount.PK;
				header.AddEventLogAndAttachDDRFileToEDocsAndSave(data, expectedDDRFileName);

				var storageFile = (BusinessObject)header.DocManagerInfo.AllEDocs[0];
				Assert("StorageFile should be saved", storageFile.IsInDatabase);
				AssertEquals("RefDocType should exist and be of the correct code", "DDR", ((RefDocType)storageFile["DocType"]).RT_DocType);
				AssertGreaterThan("StorageFile should be larger than zero bytes.", ((Enterprise.Integration.DocumentEngine.IDeliveryEmailAttachment)storageFile).FileSizeInBytes, 0);
				AssertEquals("StorageFile Name", expectedDDRFileName, storageFile["SC_FileNameWithExtension"]);

				var ddrEventLogs = header.Logs.Find(l => l.Event.SE_Code == Events.EditedARecord.Code && l.ReferenceFreeText == "DDR File Generated.");
				AssertEquals("Exactly one Event Log for DDR file should be created.", 1, ddrEventLogs.Count());
				Assert("Event Log should be saved.", ddrEventLogs.First().IsInDatabase);
			}
		}

		public void TestCustomFileFormatThrows()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupSinglePeriod(30, ZDateTime.Now.Date, ZDateTime.Now.Date.AddDays(30));

			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.CUS;
			testBank.AB_AllowAutoDDR = true;
			var testOrg = TestObjectCreator.AALSHI;
			TestObjectCreator.AddAPBankAccountDetails(testOrg, ReceiptTypes.DirectDebit, TestObjectCreator.AUD);
			var aPPayment = TestObjectCreator.CreateAPPayment(1m, 500000000m, ZDateTime.Now.Date, ZDateTime.Now.Date, testOrg.PK, testBank.PK);
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = testBank.PK;
			aPPayment.AH_ChequeOrReference = "11";
			testHeader.Lines.Add(aPPayment);

			var testFilePath = Path.Combine(Env.TempPath, "TestDDRFile.ada");
			try
			{
				AssertExceptionThrown<NotSupportedException>("Custom DDR File Format should not call CreateFile(), use DirectDebitBatchDataExportAdapter.CreateFile() instead.", () => testHeader.CreateFile(testFilePath, () => ZSaveFileDialog.OpenFile(testFilePath)));
				Assert("File should not be created", !File.Exists(testFilePath));
			}
			finally
			{
				DeleteIfExists(testFilePath);
			}
		}

		public void TestCreateFileDoesNotWriteToUnmappedPath_WhenUnexpectedExceptionBeforeAttachedToEdocs()
		{
			var header = Factory.New<DirectDebitBatchHeader>();
			header.AH_AB = BankAccount.PK;
			header.CreateFile_ErrorAction_ForTestOnly = (unmappedPath) =>
			{
				throw new ApplicationException("Oh My! This was not expected");
			};

			var outputPath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());
			try
			{
				AssertExceptionThrown<ApplicationException>("Precondition: Unexpected exception should be thrown during CreateFile()", () => header.CreateFile(outputPath, () => ZSaveFileDialog.OpenFile(outputPath)));
				Assert("Final output path should not exist when an unexpected exception occurs before eDoc attached.", !File.Exists(outputPath));
			}
			finally
			{
				AccountingUtils.DeleteFileSafe(outputPath);
			}
		}
		#endregion

		#region Implementation

		const string DrawerBank = "DrawerBank";
		const string DrawerBranch = "DrawerBranch";
		const string ChequeDrawer = "ChequeDrawer";

		AccBankAccount fDDRBankAccount;
		AccBankAccount DDRBankAccount
		{
			get
			{
				if (fDDRBankAccount == null)
				{
					fDDRBankAccount = TestObjectCreator.AUDBankAccount;
					fDDRBankAccount.AB_AllowAutoDDR = ZBool.True;
				}
				return fDDRBankAccount;
			}
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(DirectDebitBatchHeaderValidation); }
		}

		protected override Type TypeOfReversalValidation
		{
			get { return typeof(DirectDebitBatchHeaderValidation); }
		}

		#endregion
	}
}
