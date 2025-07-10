using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	class ResponsibleOverallItemsForInvoiceLineChargeCodesFinderTest : TestCaseWithFactory
	{
		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestGetResponsibleOverallItems()
		{
			var chargeCode_XXX_ALL_ALL = NewChargeCodeWithDefaultItem("XXX", "ALL", "ALL");
			var chargeCode_YYY_ALL_ALL = NewChargeCodeWithDefaultItem("YYY", "ALL", "ALL");

			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var childOrg = Factory.NewWithValidTestData<OrgHeader>();
			var subChildOrg = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = childOrg.PK;
			((InvoicingLineBase)invoice.Lines.AddNew()).AL_AC = chargeCode_XXX_ALL_ALL.PK;
			((InvoicingLineBase)invoice.Lines.AddNew()).AL_AC = chargeCode_YYY_ALL_ALL.PK;
			((InvoicingLineBase)invoice.Lines.AddNew()).AL_AC = ZGuid.Empty;

			Factory.Save();

			var parentOrgOpportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, parentOrg);
			var parentOrgOpportunityChildOrgAgreementXXX = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(parentOrgOpportunity, childOrg, "XXX", "ALL", "ALL");
			var parentOrgOpportunityChildOrgAgreementYYY = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(parentOrgOpportunity, childOrg, "YYY", "ALL", "ALL");
			var parentOrgOpportunityChildOrgAgreementZZZ = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(parentOrgOpportunity, childOrg, "ZZZ", "ALL", "ALL");
			var parentOrgOpportunityChildOrgAgreementXXXForWbpStream = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(parentOrgOpportunity, childOrg, "XXX", "ALL", "ALL");
			parentOrgOpportunityChildOrgAgreementXXXForWbpStream.CA0_CommissionStream = "WBP";
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(parentOrgOpportunityChildOrgAgreementXXX, "ADL", 10);
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(parentOrgOpportunityChildOrgAgreementYYY, "ADL", 10);
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(parentOrgOpportunityChildOrgAgreementZZZ, "ADL", 10);
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(parentOrgOpportunityChildOrgAgreementXXXForWbpStream, "ADL", 10);

			var childOrgOppotunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, childOrg);
			var childOrgOppotunitySubChildOrgAgreementXXX = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(childOrgOppotunity, subChildOrg, "XXX", "ALL", "ALL");
			var childOrgOppotunitySubChildOrgAgreementYYY = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(childOrgOppotunity, subChildOrg, "YYY", "ALL", "ALL");
			var childOrgOppotunitySubChildOrgAgreementZZZ = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(childOrgOppotunity, subChildOrg, "ZZZ", "ALL", "ALL");
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(childOrgOppotunitySubChildOrgAgreementXXX, "ADL", 10);
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(childOrgOppotunitySubChildOrgAgreementYYY, "ADL", 10);
			OrgCommissionAgreementTestHelper.AddPercentageRecipient(childOrgOppotunitySubChildOrgAgreementZZZ, "ADL", 10);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
				new[]
				{
					parentOrgOpportunityChildOrgAgreementXXX,
					childOrgOppotunitySubChildOrgAgreementXXX,
					parentOrgOpportunityChildOrgAgreementYYY,
					childOrgOppotunitySubChildOrgAgreementYYY
				},
				ResponsibleOverallItemsForInvoiceLineChargeCodesFinder.GetResponsibleOverallItems(invoice, ZString.Empty).Select(x => x.CommissionAgreement));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
				new[]
				{
					parentOrgOpportunityChildOrgAgreementXXX,
					childOrgOppotunitySubChildOrgAgreementXXX,
					parentOrgOpportunityChildOrgAgreementYYY,
					childOrgOppotunitySubChildOrgAgreementYYY,
					parentOrgOpportunityChildOrgAgreementXXXForWbpStream
				},
				ResponsibleOverallItemsForInvoiceLineChargeCodesFinder.GetResponsibleOverallItems(invoice).Select(x => x.CommissionAgreement));
		}

		AccChargeCode NewChargeCodeWithDefaultItem(ZString product, ZString service, ZString subModule)
		{
			var result = Factory.NewWithValidTestData<AccChargeCode>();
			result.AC_IsCommissionable = true;
			result.AC_DefaultCommissionProduct = product;
			result.AC_DefaultCommissionService = service;
			result.AC_DefaultCommissionSubModule = subModule;

			return result;
		}
	}
}
