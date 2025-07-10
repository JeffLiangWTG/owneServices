using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing.Samoa;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	public class SamoaAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapperTest
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

			var originalTransactionBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			originalTransactionBatch.AIB_GovernmentAllocatedNumber = "REF001";
			var originalTransactionPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(originalTransactionBatch, originalTransaction, Core.Constants.EInvoicingPivotState.Succeed);

			Factory.Save();

			var reverseTransaction = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
			reverseTransaction.AH_TransactionBelongsToGroup = originalTransaction.PK;
			(reverseTransaction as IAmending).FlagAsCreatedAmending();
			reverseTransaction.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
			reverseTransaction.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
			Factory.Save();

			currentTransaction = originalTransaction;
			currentOriginalTransaction = null;
			currentAHFRecord = CreateSamoaAHFBizo();
			base.TestProperties();

			currentTransaction = reverseTransaction;
			currentOriginalTransaction = originalTransaction;
			currentAHFRecord = CreateSamoaAHFBizo();
			base.TestProperties();
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestOriginalTransactionReferenceNumberWhenOriginalTransactionIsBeforeComplianceDate()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("WSBXL");
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.Debtor, OrgCusCode.CodeTypes.TaxFileCode, Core.Constants.CountryCodes.WesternSamoa, "12345678798");
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, company.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2018, 7, 12)))
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
				originalTransaction.AH_PostDate = new ZDateTime(2018, 7, 9, 15, 30, 0);

				var originalTransactionBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				originalTransactionBatch.AIB_GovernmentAllocatedNumber = "REF001";
				var originalTransactionPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(originalTransactionBatch, originalTransaction, Core.Constants.EInvoicingPivotState.Succeed);

				Factory.Save();

				var reverseTransaction = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
				reverseTransaction.AH_TransactionBelongsToGroup = originalTransaction.PK;
				(reverseTransaction as IAmending).FlagAsCreatedAmending();
				reverseTransaction.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				reverseTransaction.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck;
				Factory.Save();

				currentTransaction = originalTransaction;
				currentOriginalTransaction = null;
				currentAHFRecord = CreateSamoaAHFBizo();
				var additionalInfo = GetAdditionalInfoForDocWrapper();
				AssertEquals(String.Empty, additionalInfo.OriginalTransactionReferenceNumber);

				currentTransaction = reverseTransaction;
				currentOriginalTransaction = originalTransaction;
				currentAHFRecord = CreateSamoaAHFBizo();
				additionalInfo = GetAdditionalInfoForDocWrapper();
				AssertEquals("we expect the PreComplianceOriginalTransactionReferenceNumber when original transaction was created before the compliance date",
					"XXXXXXXX-XXXXXXXX-1", additionalInfo.OriginalTransactionReferenceNumber);
			}
		}

		protected override void AssertBusinessName(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.BusinessName), currentAHFRecord.BusinessName, info.BusinessName);
		protected override void AssertLocationName(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.LocationName), currentAHFRecord.LocationName, info.LocationName);
		protected override void AssertAddress(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.Address), currentAHFRecord.Address, info.Address);
		protected override void AssertDistrict(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.District), currentAHFRecord.District, info.District);
		protected override void AssertEInvoicePaymentMethod(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) => AssertEquals(nameof(info.EInvoicePaymentMethod), "Cash", info.EInvoicePaymentMethod);

		protected override void AssertOriginalTransactionReferenceNumber(AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper info) =>
			AssertEquals(nameof(info.OriginalTransactionReferenceNumber),
				currentOriginalTransaction == null
				? ZString.Empty
				: currentTransaction.IsPreEInvoicingTransaction()
					? ElectronicInvoicingHelper.TaxCorePreComplianceOriginalTransactionReferenceNumber
					: currentOriginalTransaction.EInvoicingGovernmentAllocatedNumber,
			info.OriginalTransactionReferenceNumber);

		protected override AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper GetAdditionalInfoForDocWrapper() => currentAHFRecord.AdditionaInfo;

		SamoaAccTransactionHeaderAuthorisationRecord CreateSamoaAHFBizo()
		{
			var authorizationRecord = Factory.NewWithValidTestData<SamoaAccTransactionHeaderAuthorisationRecord>();
			authorizationRecord.BusinessName = "SamoaBusiness";
			authorizationRecord.LocationName = "SamoaLocation";
			authorizationRecord.Address = "SamoaAddress";
			authorizationRecord.District = "SamoaDistrict";
			authorizationRecord.AHF_ParentId = currentTransaction.PK;
			return authorizationRecord;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		SamoaAccTransactionHeaderAuthorisationRecord currentAHFRecord;
		InvoicingBase currentTransaction;
		InvoicingBase currentOriginalTransaction;
	}
}
