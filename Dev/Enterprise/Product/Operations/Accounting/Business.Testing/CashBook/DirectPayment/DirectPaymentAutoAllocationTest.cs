using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment.Testing
{
	public class DirectPaymentAutoAllocationTest : TestCaseWithFactory
	{
		public void TestSettingAutoPrintChequeBookDoesNotDefaultChequeNumber()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank, 1, 5, 5);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 55;

			DirectPayment testBizO = Factory.NewWithValidTestData<DirectPayment>();

			testBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testBizO.AH_AB = testBank.PK;
			testBizO.ChequeBookPK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", testBizO.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("ChequeOrReference should remain empty", testBizO.AH_ChequeOrReference.IsEmpty);
			Assert("ChequeOrReference number should be read only", testBizO.AH_ChequeOrReferenceInfo.ReadOnly);
			Assert("AutoAllocation should be enabled", ((IChequeNumberAutoAllocation)testBizO).IsAutoAllocationEnabled);

			testBizO.ChequeBookPK = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, Auto Allocation disabled", !testBizO.IsChequeNumberAutoAllocated_ForTestOnly);
			AssertEquals("ChequeOrReference should be set", "55", testBizO.AH_ChequeOrReference);
			Assert("ChequeOrReference number should not be read only", !testBizO.AH_ChequeOrReferenceInfo.ReadOnly);
			Assert("Cheque Book is not IsAutoPrint, AutoAllocation disabled", !((IChequeNumberAutoAllocation)testBizO).IsAutoAllocationEnabled);
		}

		public void TestSettingAutoPrintChequeBookWillResetChequeNo()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank, 1, 5, 5);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 55;

			DirectPayment testBizO = Factory.NewWithValidTestData<DirectPayment>();

			testBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testBizO.AH_AB = testBank.PK;
			testBizO.ChequeBookPK = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, Auto Allocation disabled", !testBizO.IsChequeNumberAutoAllocated_ForTestOnly);
			AssertEquals("ChequeOrReference should be set", "55", testBizO.AH_ChequeOrReference);

			testBizO.ChequeBookPK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", testBizO.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("AH_ChequeOrReference should be reset", testBizO.AH_ChequeOrReference.IsEmpty);
		}

		public void TestAutoAllocationLabels()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank, 1, 3, 2);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();

			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AH_AB = testBank.PK;
			Assert("ChequeBook is not set, AutoAllocation disabled", !testPayment.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("Label should be empty yet", testPayment.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			Assert("Label should be empty yet", testPayment.Calc_ChequeIsAutoPrintedLabel.IsEmpty);

			testPayment.ChequeBookPK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", testPayment.IsChequeNumberAutoAllocated_ForTestOnly);
			AssertEquals("Label should be set to right value", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, testPayment.Calc_ChequeNumberIsAutoAllocatedLabel);
			AssertEquals("Label should be set to right value", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, testPayment.Calc_ChequeIsAutoPrintedLabel);

			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Receipt type is not CHQ, AutoAllocation disabled", !testPayment.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("Label should become empty", testPayment.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			Assert("Label should become empty", testPayment.Calc_ChequeIsAutoPrintedLabel.IsEmpty);

			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.ChequeBookPK = testBookWithAutoAllocation.PK;

			((IChequeNumberAutoAllocation)testPayment).AssignChequeNumber("2");
			Factory.Save();
			AssertEquals("Cheque number should be set", "2", testPayment.AH_ChequeOrReference);
			Assert("TestPayment should be in database", testPayment.IsInDatabase);
			Assert("AutoAllocation enabled", testPayment.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("Label should become empty", testPayment.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			Assert("Label should become empty", testPayment.Calc_ChequeIsAutoPrintedLabel.IsEmpty);
		}

		public void TestAK_CurrentWillNotBeChangedWhenAutoAllocationEnabled()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank, 1, 3, 2);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 4;
			testChequeBook.AK_CurrentNo = 3;

			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AH_AB = testBank.PK;
			testPayment.ChequeBookPK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", testPayment.IsChequeNumberAutoAllocated_ForTestOnly);

			((IChequeNumberAutoAllocation)testPayment).AssignChequeNumber("2");
			testBookWithAutoAllocation.AK_CurrentNo++;
			Factory.Save();
			AssertEquals("Cheque number should be set", "2", testPayment.AH_ChequeOrReference);
			Assert("TestPayment should be in database", testPayment.IsInDatabase);
			testBookWithAutoAllocation.Reload();
			AssertEquals("AK_CurrentNo on cheque book should be incremented by +1", 3m, testBookWithAutoAllocation.AK_CurrentNo);

			testPayment = Factory.NewWithValidTestData<DirectPayment>();
			testPayment.ChequeBookPK = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, should return False", !testPayment.IsChequeNumberAutoAllocated_ForTestOnly);
			testPayment.AH_ChequeOrReference = "3";
			Factory.Save();
			Assert("TestPayment should be in database", testPayment.IsInDatabase);
			AssertEquals("AH_ChequeOrReference on cheque book should change", 4m, testChequeBook.AK_CurrentNo);
		}

		#region IChequeNumberAutoAllocation Members Tests

		public void TestIChequeNumberAutoAllocation_ChequeBookPK()
		{
			AccChequeBook fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			AssertNull("Should return empty cheque book", ((IChequeNumberAutoAllocation)testPayment).ChequeBook);
			testPayment.ChequeBookPK = fTestChequeBook.PK;
			AssertEquals("Should return fTestChequeBook PK", fTestChequeBook, ((IChequeNumberAutoAllocation)testPayment).ChequeBook);
		}

		public void TestIChequeNumberAutoAllocation_AssignChequeNumber()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccBankAccount testBank = newFactory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = newFactory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_IsActive = ZBool.True;
			testChequeBook.AK_AB = testBank.PK;
			newFactory.Save();

			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			Assert("AH_ChequeOrReference field should be empty by default", testPayment.AH_ChequeOrReference.IsEmpty);
			testPayment.AH_AB = testBank.PK;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.ChequeBookPK = testChequeBook.PK;
			((IChequeNumberAutoAllocation)testPayment).AssignChequeNumber("000123");
			AssertEquals("AH_ChequeOrReference should be set", "000123", testPayment.AH_ChequeOrReference);

			testPayment.ChequeBook.AK_IsActive = ZBool.False;
			((IChequeNumberAutoAllocation)testPayment).AllocationOrPrintingFailed();
			Assert("ChequeBook should be reloaded", testPayment.ChequeBook.AK_IsActive);
			Assert("AH_ChequeOrReference should be reset", testPayment.AH_ChequeOrReference.IsEmpty);
		}

		public void TestIChequeNumberAutoAllocation_IsAllocationPerformed()
		{
			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			Assert("Should return False by defualt", !((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
			((IChequeNumberAutoAllocation)testPayment).AssignChequeNumber("123");
			Assert("Should", ((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
		}

		public void TestIChequeNumberAutoAllocation_Printing_ObjectPK()
		{
			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			AssertEquals(testPayment.PK, ((IChequeNumberAutoAllocation)testPayment).Printing_ObjectPK);
		}

		public void TestIChequeNumberAutoAllocation_Printing_PrinterPK()
		{
			BusinessObject stmPrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fTestChequeBook.AK_SQ = stmPrintQueue.PK;
			Factory.Save();

			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			AssertEquals("Should return empty cheque book", ZGuid.Empty, ((IChequeNumberAutoAllocation)testPayment).Printing_PrinterPK);
			testPayment.ChequeBookPK = fTestChequeBook.PK;
			AssertEquals("Should return fTestChequeBook PK", stmPrintQueue.PK, ((IChequeNumberAutoAllocation)testPayment).Printing_PrinterPK);
		}

		public void TestIChequeNumberAutoAllocation_ChequeIsAutoPrinted()
		{
			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			Assert("Default value", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
			((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted = ZBool.True;
			Assert("Value should be changed", ((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
		}

		public void TestNeedPlaceOfSupplyWhenMultipleFixedPlaceOfSupplyIsNotAllowed()
		{
			var testPayment = Factory.NewWithValidTestData<DirectPayment>();
			AssertEquals("NeedPlaceOfSupply", false, testPayment.NeedPlaceOfSupplyAtHeaderLevel);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AssertEquals("NeedPlaceOfSupply", true, testPayment.NeedPlaceOfSupplyAtHeaderLevel);
				AssertEquals("NeedPlaceOfSupply", true, testPayment.NeedPlaceOfSupplyAtLineLevel);
			}
		}

		public void TestNeedPlaceOfSupplyWhenMultipleFixedPlaceOfSupplyIsAllowed()
		{
			var testPayment = Factory.NewWithValidTestData<DirectPayment>();
			AssertEquals("NeedPlaceOfSupply", false, testPayment.NeedPlaceOfSupplyAtHeaderLevel);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("NeedPlaceOfSupply", false, testPayment.NeedPlaceOfSupplyAtHeaderLevel);
				AssertEquals("NeedPlaceOfSupply", true, testPayment.NeedPlaceOfSupplyAtLineLevel);
			}
		}

		#endregion

		#region Implementation

		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		#endregion
	}
}
