using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class CommissionAgreementItemAndCompanyDuplicationTest : TestCaseWithFactory
	{
		public void TestToDisplayText()
		{
			var commissionAgreement = Factory.New<OrgCommissionAgreement>();
			commissionAgreement.CA0_Name = "#1";
			var commissionAgreementItem = commissionAgreement.ProductItems.AddNew();
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "XXX";

			AssertEquals("Commission Agreement #1 (XXX Company)", new CommissionAgreementItemAndCompanyDuplication(commissionAgreementItem, clientCompany).ToDisplayText());
			AssertEquals("Commission Agreement #1 (All Companies)", new CommissionAgreementItemAndCompanyDuplication(commissionAgreementItem, null).ToDisplayText());
		}
	}
}