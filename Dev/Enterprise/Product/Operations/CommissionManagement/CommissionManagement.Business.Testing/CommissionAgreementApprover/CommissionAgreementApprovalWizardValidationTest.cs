using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	class CommissionAgreementApprovalWizardValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateFromType()
		{
			var agreementApprovalWizard = new CommissionAgreementApprovalWizard(Factory);

			agreementApprovalWizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.All;
			AssertMandatoryValidationError(agreementApprovalWizard.FromTypeInfo, false);
			AssertListValidationInvalidCodeError(agreementApprovalWizard.FromTypeInfo, false);

			agreementApprovalWizard.FromType = "XXX";
			AssertMandatoryValidationError(agreementApprovalWizard.FromTypeInfo, false);
			AssertListValidationInvalidCodeError(agreementApprovalWizard.FromTypeInfo, true);

			agreementApprovalWizard.FromType = "";
			AssertMandatoryValidationError(agreementApprovalWizard.FromTypeInfo, false);
			AssertListValidationInvalidCodeError(agreementApprovalWizard.FromTypeInfo, false);

			agreementApprovalWizard.Action = CommissionAgreementApprovalWizard.ActionType.Approve;
			agreementApprovalWizard.Validation.ValidateFromType();
			AssertMandatoryValidationError(agreementApprovalWizard.FromTypeInfo, true);
			AssertListValidationInvalidCodeError(agreementApprovalWizard.FromTypeInfo, false);
		}

		[TestDate(2015, 1, 1)]
		public void TestValidateFromDate()
		{
			var agreementApprovalWizard = new CommissionAgreementApprovalWizard(Factory);
			agreementApprovalWizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.SpecifiedDate;
			agreementApprovalWizard.FromDate = new ZDateTime(2020, 1, 1);
			AssertHasError(agreementApprovalWizard.FromDateInfo, "From date can not be in the future.");

			agreementApprovalWizard.FromDate = new ZDateTime(2000, 1, 1);
			AssertNoError(agreementApprovalWizard.FromDateInfo, "From date can not be in the future.");

			agreementApprovalWizard.FromDate = ZDateTime.Empty;
			AssertMandatoryValidationError(agreementApprovalWizard.FromDateInfo, true);

			agreementApprovalWizard.FromType = CommissionAgreementApprovalWizardFromTypeList.Codes.All;
			agreementApprovalWizard.Validation.ValidateFromDate();
			AssertMandatoryValidationError(agreementApprovalWizard.FromDateInfo, false);
		}

		public void TestApproveValidation_NoUnapprovedCommissionAgreements()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 0, wizard.CommissionAgreementApprovalItemCollection.Count);

			wizard.Action = CommissionAgreementApprovalWizard.ActionType.Approve;
			wizard.RunPreSaveValidation();
			AssertHasRowError(wizard, "There are no new or changed commission agreements that require approval.");
		}

		public void TestApproveValidation_NoAgreementSelected()
		{
			var unapprovedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = false;
			wizard.Action = CommissionAgreementApprovalWizard.ActionType.Approve;
			wizard.RunPreSaveValidation();
			AssertHasRowError(wizard, "Please include at least one commission agreement.");
		}

		public void TestDisapproveValidation_NoUnapprovedCommissionAgreements()
		{
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 0, wizard.CommissionAgreementApprovalItemCollection.Count);

			wizard.Action = CommissionAgreementApprovalWizard.ActionType.Disapprove;
			wizard.RunPreSaveValidation();
			AssertHasRowError(wizard, "There are no new or changed commission agreements that require approval.");
		}

		public void TestDisapproveValidation_NoAgreementSelected()
		{
			var unapprovedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			AssertEquals("Precondition", 1, wizard.CommissionAgreementApprovalItemCollection.Count);

			wizard.CommissionAgreementApprovalItemCollection[0].IsInclude = false;
			wizard.Action = CommissionAgreementApprovalWizard.ActionType.Disapprove;
			wizard.RunPreSaveValidation();
			AssertHasRowError(wizard, "Please include at least one commission agreement.");
		}
	}
}
