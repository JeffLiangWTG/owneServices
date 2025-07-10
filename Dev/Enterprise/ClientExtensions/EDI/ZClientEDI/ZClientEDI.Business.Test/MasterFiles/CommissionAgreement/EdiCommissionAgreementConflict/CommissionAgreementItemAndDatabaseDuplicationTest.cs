using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class CommissionAgreementItemAndDatabaseDuplicationTest : TestCaseWithFactory
	{
		public void TestToDisplayText()
		{
			var commissionAgreement = Factory.New<OrgCommissionAgreement>();
			commissionAgreement.CA0_Name = "#1";
			var commissionAgreementItem = commissionAgreement.ProductItems.AddNew();
			var licDatabase = Factory.New<LicenceDatabase>();
			licDatabase.LD_ServerCode = "XXX";

			AssertEquals("Commission Agreement #1 (XXX Database)", new CommissionAgreementItemAndDatabaseDuplication(commissionAgreementItem, licDatabase).ToDisplayText());
			AssertEquals("Commission Agreement #1 (All Databases)", new CommissionAgreementItemAndDatabaseDuplication(commissionAgreementItem, null).ToDisplayText());
		}
	}
}