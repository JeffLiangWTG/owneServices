using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing.India;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.Testing.India
{
	public class IndiaAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapperTest
		: AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapperTest
	{
		[TestDate(2018, 7, 16, 15, 26, 32)]
		public override void TestProperties()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
			Factory.Save();

			var originalTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
			originalTransaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, originalTransaction.PK));
			originalTransaction.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
			originalTransaction.AH_TransactionReference = "REF001";
			originalTransaction.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			Factory.Save();

			var reverseTransaction = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			reverseTransaction.AH_TransactionBelongsToGroup = originalTransaction.PK;
			(reverseTransaction as IAmending).FlagAsCreatedAmending();
			reverseTransaction.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
			reverseTransaction.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			Factory.Save();

			currentTransaction = originalTransaction;
			currentOriginalTransaction = null;
			currentAHFRecord = CreateIndiaAHFBizo();
			base.TestProperties();

			currentTransaction = reverseTransaction;
			currentOriginalTransaction = originalTransaction;
			currentAHFRecord = CreateIndiaAHFBizo();
			base.TestProperties();
		}

		protected override void AssertOriginalTransactionReferenceNumber(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) =>
			AssertEquals(nameof(info.OriginalTransactionReferenceNumber), currentOriginalTransaction?.AH_TransactionReference ?? string.Empty, info.OriginalTransactionReferenceNumber);

		protected override AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper GetAdditionalInfoForDocWrapper() => currentAHFRecord.AdditionaInfo;

		IndiaAccTransactionHeaderAuthorisationRecord CreateIndiaAHFBizo()
		{
			var authorizationRecord = Factory.NewWithValidTestData<IndiaAccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.AHF_ParentId = currentTransaction.PK;
			return authorizationRecord;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		IndiaAccTransactionHeaderAuthorisationRecord currentAHFRecord;
		InvoicingBase currentTransaction;
		InvoicingBase currentOriginalTransaction;
	}
}