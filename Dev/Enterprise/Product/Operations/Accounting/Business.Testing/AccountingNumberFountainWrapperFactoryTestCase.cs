using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingNumberFountainWrapperFactoryTestCase : TestCaseWithFactory
	{
		public void TestARInvoiceNo()
		{
			CheckARNumberFountain(() => new AccountingNumberFountainWrapperFactory().ARInvoiceNo);
		}

		public void TestARCreditNoteNo()
		{
			CheckARNumberFountain(() => new AccountingNumberFountainWrapperFactory().ARCreditNoteNo);
		}

		public void TestARAdjustmentNoteNo()
		{
			CheckARNumberFountain(() => new AccountingNumberFountainWrapperFactory().ARAdjustmentNoteNo);
		}

		void CheckARNumberFountain(GetWrapperDelegate getWrapper)
		{
			AssertEquals("00001000", getWrapper().PeekPreliminary(Factory));
			AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 6);
			AssertEquals("001000", getWrapper().PeekPreliminary(Factory));
		}

		delegate AccountingNumberFountainWrapper GetWrapperDelegate();

		#region TestNumberFountainIncrementForVoucherNumber_ChinaCompany

		public void TestNumberFountainIncrementForVoucherNumber_ChinaCompany()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

			try
			{
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.SetupPeriods();
				testHelper.PostPeriodsForEntireYear(2004);

				ARInvoice testARInvoice = Factory.New<ARInvoice>();
				testARInvoice.AH_PostDate = new ZDateTime(2003, 8, 2);
				Factory.Save();

				ARInvoice testARInvoice2 = Factory.New<ARInvoice>();
				testARInvoice2.AH_PostDate = new ZDateTime(2003, 8, 2);
				Factory.Save();

				AccountingPeriodCalculator testCalc = new AccountingPeriodCalculator(Factory);

				// test AR Invoice
				ARInvoice loadedARInvoice = Factory.Load<ARInvoice>(testARInvoice.PK);
				ZString expectedVoucherNum = "0402001000";
				AssertEquals("should be the first number in the sequence", expectedVoucherNum, loadedARInvoice.AH_TransactionNum);

				ARInvoice loadedARInvoice2 = Factory.Load<ARInvoice>(testARInvoice2.PK);
				expectedVoucherNum = "0402001001";
				AssertEquals("Should be second number in the sequence", expectedVoucherNum, loadedARInvoice2.AH_TransactionNum);

				// test AR Credit Note
				ARCreditNote testARCreditNote = Factory.New<ARCreditNote>();
				testARCreditNote.AH_PostDate = new ZDateTime(2003, 8, 2);
				Factory.Save();

				ARCreditNote loadedARCreditNote = Factory.Load<ARCreditNote>(testARCreditNote.PK);
				expectedVoucherNum = "0402001000";
				AssertEquals("should be the first number in the sequence", expectedVoucherNum, loadedARCreditNote.AH_TransactionNum);

				// test AR Adjustment Note
				ARAdjustmentNote testARAdjustmentNote = Factory.New<ARAdjustmentNote>();
				testARAdjustmentNote.AH_PostDate = new ZDateTime(2003, 8, 2);
				Factory.Save();

				ARAdjustmentNote loadedARAdjustmentNote = Factory.Load<ARAdjustmentNote>(testARAdjustmentNote.PK);
				expectedVoucherNum = "0402001000";
				AssertEquals("should be the first number in the sequence", expectedVoucherNum, loadedARAdjustmentNote.AH_TransactionNum);

				// test APInvoice
				APInvoice testAPInvoice = Factory.New<APInvoice>();
				testAPInvoice.AH_TransactionNum = "TESTAPINV001";
				testAPInvoice.AH_PostDate = new ZDateTime(2003, 8, 3);
				Factory.Save();

				APInvoice loadedAPInvoice = Factory.Load<APInvoice>(testAPInvoice.PK);
				AssertEquals("AH_TransactionReference should not be set", ZString.Empty, loadedAPInvoice.AH_TransactionReference);

				// test APCreditNote
				APCreditNote testAPCreditNote = Factory.New<APCreditNote>();
				testAPCreditNote.AH_TransactionNum = "TESTAPCRD001";

				testAPCreditNote.AH_PostDate = new ZDateTime(2003, 8, 3);
				Factory.Save();

				APCreditNote loadedAPCreditNote = Factory.Load<APCreditNote>(testAPCreditNote.PK);
				AssertEquals("AH_TransactionReference should not be set", ZString.Empty, loadedAPCreditNote.AH_TransactionReference);

				// test APAdjustmentNote
				APAdjustmentNote testAPAdjustmentNote = Factory.New<APAdjustmentNote>();
				testAPAdjustmentNote.AH_PostDate = new ZDateTime(2003, 8, 3);
				testAPAdjustmentNote.AH_TransactionNum = "TESTAPADJ001";
				Factory.Save();

				APAdjustmentNote loadedAPAdjustmentNote = Factory.Load<APAdjustmentNote>(testAPAdjustmentNote.PK);
				AssertEquals("AH_TransactionReference should not be set", ZString.Empty, loadedAPAdjustmentNote.AH_TransactionReference);

				// test AP Journal
				APJournal testAPJournal = Factory.New<APJournal>();
				testAPJournal.AH_PostDate = new ZDateTime(2003, 8, 2);
				Factory.Save();

				APJournal loadedAPJournal = Factory.Load<APJournal>(testAPJournal.PK);
				expectedVoucherNum = "0402001000";
				AssertEquals("Should be the first number in the sequence", expectedVoucherNum, loadedAPJournal.AH_TransactionNum);

				// test AP Transfer 
				APTransfer testAPTransfer = (APTransfer)Transfer.New(typeof(APTransfer), Factory);
				testAPTransfer.AH_PostDate = new ZDateTime(2003, 8, 2);
				Factory.Save();

				expectedVoucherNum = "0402001000";
				AssertEquals("Should be first number in sequence", expectedVoucherNum, testAPTransfer.TransferFrom.AH_TransactionNum);
				AssertEquals("Should be first number in sequence", expectedVoucherNum, testAPTransfer.TransferTo.AH_TransactionNum);

				// test AP Contra
				Contra testContra = Contra.New(Factory);
				testContra.AH_PostDate = new ZDateTime(2003, 10, 3);
				Factory.Save();

				expectedVoucherNum = "0404001000";
				AssertEquals("Should be first number in sequence", expectedVoucherNum, testContra.AH_TransactionNum);

				// test ARInvoiceAgain
				ARInvoice testARInvoice3 = Factory.New<ARInvoice>();
				testARInvoice3.AH_PostDate = new ZDateTime(2003, 9, 2);
				Factory.Save();

				ARInvoice loadedARInv3 = Factory.Load<ARInvoice>(testARInvoice3.PK);
				expectedVoucherNum = "0403001000";
				AssertEquals("Should be first number in sequence since period was reset", expectedVoucherNum, loadedARInv3.AH_TransactionNum);

				// test AP Journal Again
				APJournal testAPJournal2 = Factory.New<APJournal>();
				testAPJournal2.AH_PostDate = new ZDateTime(2003, 9, 2);
				Factory.Save();

				APJournal loadedAPJournal2 = Factory.Load<APJournal>(testAPJournal2.PK);
				AssertEquals("Should be the first number in sequence since period was reset", "0403001000", loadedAPJournal2.AH_TransactionNum);

				APJournal testAPJournal3 = Factory.New<APJournal>();
				testAPJournal3.AH_PostDate = new ZDateTime(2003, 9, 2);
				Factory.Save();

				APJournal loadedAPJournal3 = Factory.Load<APJournal>(testAPJournal3.PK);
				AssertEquals("Should be the second number in the sequence", "0403001001", loadedAPJournal3.AH_TransactionNum);

				// test Opening Payment
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		#endregion

		#region TestNumberFountainIncrementForAustraliaCompany

		public void TestNumberFountainIncrementForAustraliaCompany()
		{
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			try
			{
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.SetupPeriods();
				testHelper.PostPeriodsForEntireYear(2004);

				// ARInvoice
				ARInvoice testARInvoice = Factory.New<ARInvoice>();
				testARInvoice.AH_PostDate = new ZDateTime(2003, 9, 2);
				Factory.Save();

				ARInvoice loadedARInvoice = Factory.Load<ARInvoice>(testARInvoice.PK);
				AssertEquals("Should not have the period prepended", "00001000", loadedARInvoice.AH_TransactionNum);

				// ARCreditNote
				ARCreditNote testARCreditNote = Factory.New<ARCreditNote>();
				testARCreditNote.AH_PostDate = new ZDateTime(2003, 10, 2);
				Factory.Save();

				ARCreditNote loadedARCreditNote = Factory.New<ARCreditNote>();
				AssertEquals("Should not have the period prepended", "00001000", loadedARInvoice.AH_TransactionNum);

				// ARInvoice again
				ARInvoice testARInvoice2 = Factory.New<ARInvoice>();
				testARInvoice2.AH_PostDate = new ZDateTime(2003, 9, 4);
				Factory.Save();

				ARInvoice loadedARInvoice2 = Factory.Load<ARInvoice>(testARInvoice2.PK);
				AssertEquals("Should not have the period prepended", "00001001", loadedARInvoice2.AH_TransactionNum);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		#endregion

		public void TestFactoryIsResetWhenChangingCompanies()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, 6);
			AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, 8);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Factory.New<ARInvoice>();
				Factory.Save();
				AssertEquals("001000", invoice.AH_TransactionNum);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Factory.New<ARInvoice>();
				Factory.Save();
				AssertEquals("00001000", invoice.AH_TransactionNum);
			}
		}
	}
}