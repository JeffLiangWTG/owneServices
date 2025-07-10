using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class TaxGLMovementGeneralLedgerDataLineCreatorTest : GeneralLedgerDataLineCreatorTest
	{
		[TestDate(2023, 5, 18)]
		public void TestGLDAccountTypes()
		{
			var testData = GetTaxGLMovementAndExpectedDRCRLines(110m, 110m);
			CombineAssertions(() =>
			{
				AssertEquals(GLDAccountTypes.GLMovementDebitAccount, testData.DRCRLines[0].GLDAccountType);
				AssertEquals(GLDAccountTypes.GLMovementCreditAccount, testData.DRCRLines[1].GLDAccountType);
			});
		}

		[TestDate(2023, 5, 18)]
		public void TestGeneralLedgerDataBasic_ExchangeRate()
		{
			var taxGLMovement1 = GetAccTaxGLMovementForTest(Factory, 110m, 11m);
			var taxGLMovement2 = GetAccTaxGLMovementForTest(Factory, 110m, 0m);
			var taxGLMovement3 = GetAccTaxGLMovementForTest(Factory, 0m, 110m);

			var creator = new TaxGLMovementGeneralLedgerDataLineCreator();
			var entry1 = creator.CreateDRCREntries(((INeedRow)taxGLMovement1).Row);
			var entry2 = creator.CreateDRCREntries(((INeedRow)taxGLMovement2).Row);

			AssertEquals(10m, entry1.ExchangeRate);
			AssertEquals(0m, entry2.ExchangeRate);

			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			Factory.Save();

			var entry3 = creator.CreateDRCREntries(((INeedRow)taxGLMovement1).Row);
			var entry4 = creator.CreateDRCREntries(((INeedRow)taxGLMovement3).Row);

			AssertEquals(0.1m, entry3.ExchangeRate);
			AssertEquals(0m, entry4.ExchangeRate);
		}

		public override void TestHasValidControlAccount()
		{
			Assert("Don't need ControlAccount", true);
		}

		[TestDate(2023, 5, 18)]
		public void TestCreateDRCREntries_TaxGLMovement()
		{
			var testData = GetTaxGLMovementAndExpectedDRCRLines(110m, 110m);

			AssertGeneratedDRCRLines(testData.DRCRLines, testData.ExpectedDRCRLines);

			testData = GetTaxGLMovementAndExpectedDRCRLines(-110m, -110m);

			AssertGeneratedDRCRLines(testData.DRCRLines, testData.ExpectedDRCRLines);
		}

		(DebitCreditEntryItem[] DRCRLines, DebitCreditEntryItem[] ExpectedDRCRLines) GetTaxGLMovementAndExpectedDRCRLines(decimal osTaxAmount, decimal localTaxAmount)
		{
			var taxGLMovement = GetAccTaxGLMovementForTest(Factory, osTaxAmount, localTaxAmount);

			var absOSTaxAmount = Math.Abs(osTaxAmount);

			var expectedDRCRLines = new[]
			{
				new DebitCreditEntryItem { AccountPK = taxGLMovement.ATM_AG_DebitAccount, LocalAmount = 100m, OSAmount = absOSTaxAmount, DRCRSign = DebitCredit.DR, GLDAccountType =  GLDAccountTypes.GLMovementDebitAccount, JournalDate = ZDate.Today, GLDType = AccountingConstants.GLDTypeCodes.RealizeTaxGLMovement, Period = 202305 },
				new DebitCreditEntryItem { AccountPK = taxGLMovement.ATM_AG_CreditAccount, LocalAmount = -100m, OSAmount = absOSTaxAmount * (-1), DRCRSign = DebitCredit.CR, GLDAccountType =  GLDAccountTypes.GLMovementCreditAccount, JournalDate = ZDate.Today, GLDType = AccountingConstants.GLDTypeCodes.RealizeTaxGLMovement, Period = 202305 },
			};

			var creator = new TaxGLMovementGeneralLedgerDataLineCreator();
			var entry = creator.CreateDRCREntries(((INeedRow)taxGLMovement).Row);

			return (entry.EntryItems, expectedDRCRLines);
		}

		public static AccTaxGLMovement GetAccTaxGLMovementForTest(BusinessObjectFactory factory, decimal osTaxAmount, decimal localTaxAmount)
		{
			var testObjectCreator = new TestObjectCreator(factory);
			testObjectCreator.CreateTestPeriodsForEntireYear(2023);

			factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice));
			invoice.AH_TransactionType = TransactionTypes.InvoiceBatch;
			var objForTest = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			var taxTransaction = factory.New<AccTaxTransaction>();
			var taxConfiguration = factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			taxTransaction.ATT_ETC = taxConfiguration.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
			taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			taxTransaction.ATT_Ledger = LedgerTypes.AccountsReceivable;
			taxTransaction.ATT_Basis = TaxBasisList.PostingOnMatching.Code;
			taxTransaction.ATT_RX_NKOSTaxCurrency = "AUD";
			taxTransaction.ATT_OSTaxBaseAmount = osTaxAmount;
			taxTransaction.ATT_LocalTaxBaseAmount = osTaxAmount;
			taxTransaction.ATT_OSTaxAmount = osTaxAmount;
			taxTransaction.ATT_LocalTaxAmount = localTaxAmount;
			taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.Perceptions.Code;
			taxTransaction.ATT_Rate = 0.1274m;

			var pivots = factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK));
			if (pivots.Length == 0)
			{
				var pivot = factory.New<AccTaxRecordTransactionLinePivot>();
				pivot.ATP_ATT = taxTransaction.PK;
				pivot.FillWithValidTestData();
			}
			taxTransaction.ATT_PostDate = ZDate.Today;
			taxTransaction.ATT_TaxDate = ZDate.Today;
			taxTransaction.ATT_TaxSystemCode = "DNC";
			taxTransaction.ATT_AH = ((ITaxRecordParentBase)objForTest).PK;

			var taxRate = factory.NewWithValidTestData<AccTaxRate>();
			taxTransaction.ATT_AT_TaxID = taxRate.PK;

			factory.Save();

			var taxGLMovement = factory.New<AccTaxGLMovement>();

			taxGLMovement.ATM_ATT_TaxTransaction = taxTransaction.PK;
			taxGLMovement.ATM_Period = 202301;
			taxGLMovement.ATM_Date = ZDate.Today;
			taxGLMovement.ATM_Amount = 100m;
			taxGLMovement.ATM_Type = TaxGLMovementTypeList.Realised.Code;

			var glHeader1 = factory.NewWithValidTestData<AccGLHeader>();
			var glHeader2 = factory.NewWithValidTestData<AccGLHeader>();

			taxGLMovement.ATM_AG_DebitAccount = glHeader1.PK;
			taxGLMovement.ATM_AG_CreditAccount = glHeader2.PK;

			factory.Save();

			return taxGLMovement;
		}
	}
}
