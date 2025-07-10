using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionAgreementApprovalItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsInclude_NonSpecifiedAction()
		{
			var opportunityStatuses = new OpportunityStatusCollection();
			opportunityStatuses.Add("EFF", (NoResString)"Effective", true, true, true, "");
			opportunityStatuses.Add("NON", (NoResString)"Non-Effective", false, true, true, "");
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, opportunityStatuses);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_Status = "NON";
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var reversedAgreement = opportunity.CommissionAgreements.AddNew();
			reversedAgreement.Reverse();
			reversedAgreement.FillWithValidTestData();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			wizard.Action = CommissionAgreementApprovalWizard.ActionType.NotSpecified;
			AssertEquals("Precondition", 2, wizard.CommissionAgreementApprovalItemCollection.Count);

			var agreementItem = wizard.CommissionAgreementApprovalItems.Single(x => x.CommissionAgreement == agreement);
			var reversedAgreementItem = wizard.CommissionAgreementApprovalItems.Single(x => x.CommissionAgreement == reversedAgreement);

			var expectedMessage = "Cannot approve agreement until opportunity is effective.";
			agreementItem.Validation.ValidateIsInclude();
			reversedAgreementItem.Validation.ValidateIsInclude();
			AssertNoWarning(agreementItem.IsIncludeInfo, expectedMessage);
			AssertNoWarning(reversedAgreementItem.IsIncludeInfo, expectedMessage);

			agreementItem.IsInclude = true;
			reversedAgreementItem.IsInclude = true;
			AssertHasWarning(agreementItem.IsIncludeInfo, expectedMessage);
			AssertNoWarning(reversedAgreementItem.IsIncludeInfo, expectedMessage);

			opportunity.P8_Status = "EFF";
			agreementItem.Validation.ValidateIsInclude();
			reversedAgreementItem.Validation.ValidateIsInclude();
			AssertNoWarning(agreementItem.IsIncludeInfo, expectedMessage);
			AssertNoWarning(reversedAgreementItem.IsIncludeInfo, expectedMessage);
		}

		public void TestIsInclude_ApproveAction()
		{
			var opportunityStatuses = new OpportunityStatusCollection();
			opportunityStatuses.Add("EFF", (NoResString)"Effective", true, true, true, "");
			opportunityStatuses.Add("NON", (NoResString)"Non-Effective", false, true, true, "");
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, opportunityStatuses);

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_Status = "NON";
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			var reversedAgreement = opportunity.CommissionAgreements.AddNew();
			reversedAgreement.Reverse();
			reversedAgreement.FillWithValidTestData();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			wizard.Action = CommissionAgreementApprovalWizard.ActionType.Approve;
			AssertEquals("Precondition", 2, wizard.CommissionAgreementApprovalItemCollection.Count);

			var agreementItem = wizard.CommissionAgreementApprovalItems.Single(x => x.CommissionAgreement == agreement);
			var reversedAgreementItem = wizard.CommissionAgreementApprovalItems.Single(x => x.CommissionAgreement == reversedAgreement);

			var expectedMessage = "Cannot approve agreement until opportunity is effective.";
			agreementItem.Validation.ValidateIsInclude();
			reversedAgreementItem.Validation.ValidateIsInclude();
			AssertNoError(agreementItem.IsIncludeInfo, expectedMessage);
			AssertNoError(reversedAgreementItem.IsIncludeInfo, expectedMessage);

			agreementItem.IsInclude = true;
			reversedAgreementItem.IsInclude = true;
			AssertHasError(agreementItem.IsIncludeInfo, expectedMessage);
			AssertNoError(reversedAgreementItem.IsIncludeInfo, expectedMessage);

			opportunity.P8_Status = "EFF";
			agreementItem.Validation.ValidateIsInclude();
			reversedAgreementItem.Validation.ValidateIsInclude();
			AssertNoError(agreementItem.IsIncludeInfo, expectedMessage);
			AssertNoError(reversedAgreementItem.IsIncludeInfo, expectedMessage);
		}

		public void TestIsInclude_ApproveAction_WithAgreementError()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var unapprovedAgreement1 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, org);
			unapprovedAgreement1.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			var unapprovedAgreement2 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreementForAllItems(Factory, org);
			unapprovedAgreement2.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var wizard = new CommissionAgreementApprovalWizard(anotherFactory);
			AssertEquals("Precondition", 2, wizard.CommissionAgreementApprovalItemCollection.Count);

			wizard.Action = CommissionAgreementApprovalWizard.ActionType.Approve;

			var agreementItem = wizard.CommissionAgreementApprovalItems.Single(x => x.CommissionAgreement.PK == unapprovedAgreement1.PK);
			agreementItem.IsInclude = true;
			AssertHasErrorContaining(agreementItem.IsIncludeInfo, "Other agreement(s) have a duplicate item.");
		}

		public void TestIsInclude_DisapproveAction()
		{
			var approvedAgreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			approvedAgreement.Approve();
			Factory.Save();

			var wizard = new CommissionAgreementApprovalWizard(Factory);
			wizard.Action = CommissionAgreementApprovalWizard.ActionType.Disapprove;
			AssertEquals("Precondition", 0, wizard.CommissionAgreementApprovalItemCollection.Count);

			var agreementItem = new CommissionAgreementApprovalItem(wizard, approvedAgreement);
			wizard.CommissionAgreementApprovalItemCollection.Add(agreementItem);
			agreementItem.IsInclude = false;

			var expectedMessage = "Cannot disapprove an approved agreement. Reverse or disable the agreement instead.";
			AssertNoError(agreementItem.IsIncludeInfo, expectedMessage);

			agreementItem.IsInclude = true;
			AssertHasError(agreementItem.IsIncludeInfo, expectedMessage);
		}
	}
}
