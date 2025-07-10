using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing;

[TestedType(typeof(AccountingJournalLineForGeneralLedgerData))]
public class AccountingJournalLineForGeneralLedgerDataTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<NullReferenceException>(() => new AccountingJournalLineForGeneralLedgerData(null));
		AssertNoExceptionThrown(() => new AccountingJournalLineForGeneralLedgerData(Factory.NewWithValidTestData<AccGeneralLedgerData>()));
	}

	public void TestPublicProperties()
	{
		var generalLedgerData = GetNewBusinessObject() as AccountingJournalLineForGeneralLedgerData;

		AssertEquals("100000", generalLedgerData.JournalEntriesNumber);
		AssertEquals(AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.Value, generalLedgerData.IsGenerateAndStoreJournalEntriesForPostedAccountingTransactionsOn);
		AssertEquals(chargeCode.PK, generalLedgerData.AL_AC);
		AssertEquals(chargeCode.PK, generalLedgerData.ChargeCode.PK);
		AssertEquals(glHeader.AG_DescriptionMultilingual, generalLedgerData.AL_Desc);
		AssertEquals(transactionLine.AL_RevRecognitionType, generalLedgerData.AL_RevRecognitionType);
		AssertEquals(1m, generalLedgerData.AL_ExchangeRate);
		AssertEquals("USD", generalLedgerData.AL_RX_NKTransactionCurrency);
		AssertEquals(glHeader.PK, generalLedgerData.AL_AG);
		AssertEquals(Env.CurrentCompanyPK, generalLedgerData.AL_GC);
		AssertEquals(accGeneralLedgerData.Company.PK, generalLedgerData.Company.PK);
		AssertEquals(Env.CurrentBranchPK, generalLedgerData.AL_GB);
		AssertEquals(accGeneralLedgerData.Branch.PK, generalLedgerData.Branch.PK);
		AssertEquals(Env.CurrentDepartmentPK, generalLedgerData.AL_GE);
		AssertEquals(accGeneralLedgerData.Department.PK, generalLedgerData.Department.PK);
		AssertEquals(202411, generalLedgerData.AL_PostPeriod);
		AssertEquals(ZDateTime.Today, generalLedgerData.AL_PostDate);
		AssertEquals("USD", generalLedgerData.CurrencyCode);
		AssertEquals(AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Description, generalLedgerData.TaxBasis);
		AssertEquals(taxGLMovement.PK, generalLedgerData.GLD_ATM_TaxGLMovement);
		AssertEquals(glHeader.AG_DescriptionMultilingual, generalLedgerData.GLAccountDescription);
		AssertEquals("CR", generalLedgerData.DebitCreditSign);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
		chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
		glHeader = Factory.NewWithValidTestData<AccGLHeader>();
		transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
		accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
		taxGLMovement = Factory.NewWithValidTestData<AccTaxGLMovement>();

		transactionLine.AL_Sequence = 1;
		transactionLine.AL_AC = chargeCode.PK;
		transactionLine.MultiSubAccountTypeCode = "123";
		transactionLine.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code;
		accGeneralLedgerData.GLD_AL_TransactionLine = transactionLine.PK;
		accGeneralLedgerData.GLD_AH_TransactionHeader = transactionHeader.PK;
		accGeneralLedgerData.GLD_JournalEntriesNumber = "100000";
		accGeneralLedgerData.GLD_AG_GLAccount = glHeader.PK;
		accGeneralLedgerData.GLD_ExchangeRate = 1m;
		accGeneralLedgerData.GLD_GC_Company = Env.CurrentCompanyPK;
		accGeneralLedgerData.GLD_GB_Branch = Env.CurrentBranchPK;
		accGeneralLedgerData.GLD_GE_Department = Env.CurrentDepartmentPK;
		accGeneralLedgerData.GLD_PostPeriod = 202411;
		accGeneralLedgerData.GLD_PostDate =  ZDateTime.Today;
		accGeneralLedgerData.GLD_Currency =  "USD";
		accGeneralLedgerData.GLD_ATM_TaxGLMovement = taxGLMovement.PK;
		accGeneralLedgerData.GLD_OSCreditAmount = 1000m;
		accGeneralLedgerData.GLD_OSDebitAmount = 0m;
		accGeneralLedgerData.GLD_LocalCreditAmount = 1000m;
		accGeneralLedgerData.GLD_LocalDebitAmount = 0m;

		return new AccountingJournalLineForGeneralLedgerData(accGeneralLedgerData);
	}

	[TestDate(2024, 11, 18)]
	public void TestSubAccount()
	{
		TestObjectCreator.CreateTestPeriodsForEntireYear(2024);
		AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
		AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

		var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
		var receipt = TestObjectCreator.CreateARReceipt(1m, 100m, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
		receipt.AH_AG = TestObjectCreator.GLHeader1.PK;

		aRInvoice.Lines[0].AL_PostDate = DateTime.Now;
		aRInvoice.Lines[0].AL_ReverseDate = DateTime.Now;
		aRInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;

		TestObjectCreator.CreateTransactionLineSubAccount<AccTransactionLineSubAccount>(aRInvoice.Lines[0].PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.ABIGAS.PK);
		TestObjectCreator.CreateTransactionHeaderSubAccount(receipt.PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.AALSHI.PK);

		Factory.Save();

		((INeedRow)aRInvoice.Lines[0]).Row.SetAdded();
		((INeedRow)receipt).Row.SetAdded();

		var processor = new GeneralLedgerDataProcessor();
		var processorAsInterface = processor as IGeneralLedgerDataProcessor;
		AssertNotNull(processorAsInterface);
		processorAsInterface.ProcessData(new[] { ((INeedRow)aRInvoice.Lines[0]).Row, ((INeedRow)receipt).Row });

		accGeneralLedgerData = (AccGeneralLedgerData)Factory.Load(typeof(AccGeneralLedgerData), new ZQuery(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, aRInvoice.PK))[0];

		AssertEquals("ORG: " + TestObjectCreator.ABIGAS.OH_Code, accGeneralLedgerData.SubAccount);

		var generalLedgerData = new AccountingJournalLineForGeneralLedgerData(accGeneralLedgerData);

		AssertEquals("ORG: " + TestObjectCreator.ABIGAS.OH_Code, generalLedgerData.MultiSubAccountTypeCode);
	}

	TestObjectCreator TestObjectCreator
	{
		get
		{
			if (fTestObjectCreator == null)
			{
				fTestObjectCreator = new TestObjectCreator(Factory);
			}
			return fTestObjectCreator;
		}
	}
	TestObjectCreator fTestObjectCreator;

	AccGeneralLedgerData accGeneralLedgerData;
	AccTransactionHeader transactionHeader;
	AccTransactionLines transactionLine;
	AccTaxGLMovement taxGLMovement;
	AccChargeCode chargeCode;
	AccGLHeader glHeader;
}
