using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class UnapprovedCreditNoteTransactionCandidateValidationTest : APCreditNoteValidationTest
	{
		#region Overriden

		public override void TestCheckAH_LocalTotalAmount_DetectsCreditedExceedInvoiced()
		{
			Assert("Test is not applicable", true);
		}

		public override void TestCheckAH_LocalTaxAmount_DetectsTaxCreditedExceedInvoiced()
		{
			Assert("Test is not applicable", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetUpRegistryForTest();
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(UACreditNote);
			}
		}

		protected override InvoiceBaseValidation GetValidation(Base.Transaction.TransactionHeader parent)
		{
			return new UnapprovedCreditNoteCandidateValidation((UACreditNote)parent);
		}

		#endregion

		[GuiTest]
		public void TestValidateAll()
		{
			AssertValidateAll(true);
			AssertValidateAll(false);
		}

		void AssertValidateAll(bool value)
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

			APCreditNote note = Factory.New<APCreditNote>();
			APCreditNoteLine noteLine = note.Lines.AddNew() as APCreditNoteLine;
			noteLine.AL_OSExTaxAmount = 100m;

			UnapprovedCreditNoteCandidateValidation validation = new UnapprovedCreditNoteCandidateValidation(note);
			validation.ValidateAll();
			AssertNoRowErrors("No approval required as invoice is below 1000", note);

			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;

			noteLine.AL_OSExTaxAmount = 1001m;
			validation.ValidateAll();
			AssertHasRowError(note, "This transaction requires a higher level of approval authority.");

			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = true;

			validation.ValidateAll();
			AssertNoRowErrors("No approval required as invoice amount is within current apporval limits", note);
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
