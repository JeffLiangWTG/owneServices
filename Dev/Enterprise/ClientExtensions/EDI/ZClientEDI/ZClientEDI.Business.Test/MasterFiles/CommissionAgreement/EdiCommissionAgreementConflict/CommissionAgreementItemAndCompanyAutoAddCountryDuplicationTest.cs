using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class CommissionAgreementItemAndCompanyAutoAddCountryDuplicationTest : TestCaseWithFactory
	{
		public void TestToDisplayText()
		{
			var commissionAgreement = Factory.New<OrgCommissionAgreement>();
			commissionAgreement.CA0_Name = "#1";
			var commissionAgreementItem = commissionAgreement.ProductItems.AddNew();
			var licDatabase = Factory.New<LicenceDatabase>();
			licDatabase.LD_ServerCode = "XXX";

			AssertEquals("Commission Agreement #1 (Auto-add new AU companies for XXX Database)", new CommissionAgreementItemAndCompanyAutoAddCountryDuplication(commissionAgreementItem, licDatabase, "AU").ToDisplayText());
			AssertEquals("Commission Agreement #1 (Auto-add new AU companies for new Databases)", new CommissionAgreementItemAndCompanyAutoAddCountryDuplication(commissionAgreementItem, null, "AU").ToDisplayText());

			AssertEquals("Commission Agreement #1 (Auto-add new Unknown companies for XXX Database)", new CommissionAgreementItemAndCompanyAutoAddCountryDuplication(commissionAgreementItem, licDatabase, "").ToDisplayText());
			AssertEquals("Commission Agreement #1 (Auto-add new Unknown companies for new Databases)", new CommissionAgreementItemAndCompanyAutoAddCountryDuplication(commissionAgreementItem, null, "").ToDisplayText());
		}
	}
}