using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class UnapprovedInvoiceTransactionCandidateValidationTest : APInvoiceValidationTest
	{
		#region Overriden

		protected override void SetUp()
		{
			base.SetUp();
			SetUpRegistryForTest();
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(UAInvoice);
			}
		}

		protected override InvoiceBaseValidation GetValidation(Base.Transaction.TransactionHeader parent)
		{
			return new UnapprovedInvoiceCandidateValidation((APInvoice)parent);
		}

		#endregion

		public override void TestValidateAH_LocalOutstandingAmount()
		{
			Assert(true);
		}

		public override void TestCheckAH_OHCreditLimit()
		{
			Assert(true);
		}

		public override void TestValidationOnReceiptPaymentAH_ABWhenBankAccountBranchNotEqualInvoiceHeaderBranch()
		{
			Assert("Not Applicable", true);
		}

		public override void TestValidationOnReceiptPaymentAH_AB_NoErrorWhenBankAccountBranchIsNull()
		{
			Assert("Not Applicable", true);
		}

		public void TestSetTransactionNumEmptyForSelfBillingUAInvoice()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTransactionNumberCriticalValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var invoice = Factory.NewWithValidTestData(typeof(APInvoice)) as InvoicingBase;
				invoice.AH_TransactionNum = null;
				invoice.AH_OH = TestOrg.PK;
				invoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
				invoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
				invoice.IsSelfBillingInvoice = true;

				var validator = GetValidation(invoice);
				validator.ValidateAH_TransactionNum();
				AssertNoErrors("Pre -condition", invoice.AH_TransactionNumInfo);

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(invoice.PK, CriticalValidationInfoCollectorServiceKeyType.InvoicingBaseDidNotCreateTransactionNumberOnSaving);

				AssertNoExceptionThrown("Valid behavior to set empty transaction number and should not cause any error.", () => Factory.Save());
			}
		}

		[GuiTest]
		public void TestValidateAll()
		{
			AssertValidateAll(true);
			AssertValidateAll(false);
		}

		void AssertValidateAll(bool value)
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = invoice.Lines.AddNew() as APInvoiceLine;
			invoiceLine.AL_OSExTaxAmount = 100m;

			UnapprovedInvoiceCandidateValidation validation = new UnapprovedInvoiceCandidateValidation(invoice);
			validation.ValidateAll();
			AssertNoRowErrors("No approval required as invoice is below 1000", invoice);

			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;

			invoiceLine.AL_OSExTaxAmount = 1001m;
			validation.ValidateAll();
			AssertHasRowError(invoice, "This transaction requires a higher level of approval authority.");

			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = true;

			validation.ValidateAll();
			AssertNoRowErrors("No approval required as invoice amount is within current apporval limits", invoice);
		}

		#region Implementation

		void SetUpRegistryForTest()
		{
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			UpTo1000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 1000, AuthorisationCodes.NoApprovalRequired);
			UpTo2000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 2000, AuthorisationCodes.FirstApprovalRequiredOnly);
			UpTo3000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 3000, AuthorisationCodes.SecondApprovalRequiredOnly);
			Above3000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 3000, AuthorisationCodes.ThirdApprovalRequiredOnly);

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}

		protected PaymentTwelveLevelAuthorisationSettings UpTo1000;
		protected PaymentTwelveLevelAuthorisationSettings UpTo2000;
		protected PaymentTwelveLevelAuthorisationSettings UpTo3000;
		protected PaymentTwelveLevelAuthorisationSettings Above3000;

		#endregion

	}
}
