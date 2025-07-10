using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class CommissionAgreementItemAndCompanyAutoAddDatabaseDuplicationTest : TestCaseWithFactory
	{
		public void TestToDisplayText()
		{
			var commissionAgreement = Factory.New<OrgCommissionAgreement>();
			commissionAgreement.CA0_Name = "#1";
			var commissionAgreementItem = commissionAgreement.ProductItems.AddNew();
			var licDatabase = Factory.New<LicenceDatabase>();
			licDatabase.LD_ServerCode = "XXX";

			AssertEquals("Commission Agreement #1 (Auto-add new companies for XXX Database)", new CommissionAgreementItemAndCompanyAutoAddDatabaseDuplication(commissionAgreementItem, licDatabase).ToDisplayText());
			AssertEquals("Commission Agreement #1 (Auto-add new companies for new Databases)", new CommissionAgreementItemAndCompanyAutoAddDatabaseDuplication(commissionAgreementItem, null).ToDisplayText());
		}
	}
}