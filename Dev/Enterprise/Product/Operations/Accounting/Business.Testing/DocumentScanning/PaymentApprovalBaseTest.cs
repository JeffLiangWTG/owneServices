using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class PaymentApprovalBaseTest<T> : TestCaseWithFactory where T : PaymentApprovalBase
	{
		protected abstract IEDocsViaUniversalXmlSupport EDocsViaUniversalXmlSupport();

		public void TestGetEDocViaUniversalXmlSupport()
		{
			var eDocsViaUniversalXmlSupport = EDocsViaUniversalXmlSupport();

			AssertEquals(null, eDocsViaUniversalXmlSupport.LoadBusinessObjectFromCode(Factory, "invalid-code"));

			var approval1 = Factory.NewWithValidTestData<T>();
			var approval2 = Factory.NewWithValidTestData<T>();
			var approval3 = Factory.NewWithValidTestData<T>();
			Factory.Save();

			AssertEquals("00001000", approval1.AV_PaymentApprovalReference);

			AssertEquals("00001001", approval2.AV_PaymentApprovalReference);

			AssertEquals("00001002", approval3.AV_PaymentApprovalReference);

			AssertEquals(approval2.PK, eDocsViaUniversalXmlSupport.LoadBusinessObjectFromCode(Factory, "00001001")?.PK);

			AssertNull("There is no business object matching the transaction number", eDocsViaUniversalXmlSupport.LoadBusinessObjectFromCode(Factory, "00000000"));

			Assert(eDocsViaUniversalXmlSupport.LoadBusinessObjectFromCode(new BusinessObjectFactory(), "00001001") is IDocManagerSupport);
		}

		public void TestNullOrWhiteSpaceArqumentThrows()
		{
			var eDocsViaUniversalXmlSupport = EDocsViaUniversalXmlSupport();

			AssertExceptionThrown<ArgumentException>("Factory Argument could not be null", () => eDocsViaUniversalXmlSupport.LoadBusinessObjectFromCode(null, "00001000"));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be string empty", () => eDocsViaUniversalXmlSupport.LoadBusinessObjectFromCode(Factory, ""));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be null", () => eDocsViaUniversalXmlSupport.LoadBusinessObjectFromCode(Factory, null));
		}

		public void TestGetEDocViaUniversalXmlSupportFormatValues()
		{
			AssertEquals(PaymentApprovalEDocsViaUniversalXmlSupport<T>.Constants.ExampleCodeFormat, EDocsViaUniversalXmlSupport().ExampleCodeFormat);
			AssertEquals(PaymentApprovalEDocsViaUniversalXmlSupport<T>.Constants.ExpectedCodeFormat, EDocsViaUniversalXmlSupport().ExpectedCodeFormat);
		}

		public void TestFundingBankAccountValidation_ChangeCurrencyWithActiveDeal()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.AUDBankAccount.PK,
				TestObjectCreator.CHNBankAccount.PK,
				hasActiveDeal: true,
				hasError: true
			);

		public void TestFundingBankAccountValidation_SameCurrencyWithActiveDeal()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.AUDBankAccount.PK,
				TestObjectCreator.AUDBankAccount2.PK,
				hasActiveDeal: true,
				hasError: false
			);

		public void TestFundingBankAccountValidation_ChangeCurrencyWithInActiveDeal()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.AUDBankAccount.PK,
				TestObjectCreator.CHNBankAccount.PK,
				hasActiveDeal: false,
				hasError: false
			);

		public void TestFundingBankAccountValidation_SameCurrencyWithInActiveDeal()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.AUDBankAccount.PK,
				TestObjectCreator.AUDBankAccount2.PK,
				hasActiveDeal: false,
				hasError: false
			);

		public void TestFundingBankAccountValidation_ChangeCurrencyWithActiveDeal_EmptyFundingBankAccount()
			=> AssertFundingBankAccountSavingError(
				ZGuid.Empty,
				TestObjectCreator.CHNBankAccount.PK,
				hasActiveDeal: true,
				hasError: true
			);

		public void TestFundingBankAccountValidation_ChangeCurrencyWithActiveDeal_EmptyFundingBankAccount2()
			=> AssertFundingBankAccountSavingError(
				TestObjectCreator.CHNBankAccount.PK,
				ZGuid.Empty,
				hasActiveDeal: true,
				hasError: true
			);

		public void TestFundingBankAccountValidation_SameCurrencyWithActiveDeal_EmptyFundingBankAccount()
			=> AssertFundingBankAccountSavingError(
				ZGuid.Empty,
				ZGuid.Empty,
				hasActiveDeal: true,
				hasError: false
			);

		void AssertFundingBankAccountSavingError(
			ZGuid originalFundingBankAccount,
			ZGuid fundingBankAccount,
			bool hasActiveDeal,
			bool hasError)
		{
			var approval = Factory.NewWithValidTestData<T>();
			approval.AV_PaymentType = ReceiptTypes.EPayment;
			approval.AV_AB_FundingBankAccount = originalFundingBankAccount;
			approval.AV_Amount = 100m;

			TestObjectCreator.CreateValidEPaymentDealForStatus(hasActiveDeal ? EPaymentStatusCodes.Deal.Accepted : EPaymentStatusCodes.Deal.Cancelled, approval);
			AssertNoNotifications(approval.AV_AB_FundingBankAccountInfo);
			AssertNoExceptionThrown(() => Factory.Save());

			approval.AV_AB_FundingBankAccount = fundingBankAccount;

			if (hasError)
			{
				AssertHasError(approval.AV_AB_FundingBankAccountInfo, "This payment has an active E-Payment Deal. Changing payment details is not permitted.");
				var exception = AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());
				AssertEquals("Another user created an active E-Payment Deal for this payment batch. Change of Funding Currency is not permitted.", exception.Message);
			}
			else
			{
				var isDifferentCurrency = approval.OriginalFundingCurrency != approval.FundingCurrency;
				if (isDifferentCurrency)
				{
					AssertEquals("PreRequisite", 1, approval.PaymentQuotes.Count);
				}

				AssertNoNotifications(approval.AV_AB_FundingBankAccountInfo);
				AssertNoExceptionThrown(() => Factory.Save());

				if (isDifferentCurrency)
				{
					AssertEquals("Active quote should be discarded when funding bank currency changed.", EPaymentStatusCodes.Quote.Discarded, approval.CurrentDealQuote.QU_Status);
				}
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
