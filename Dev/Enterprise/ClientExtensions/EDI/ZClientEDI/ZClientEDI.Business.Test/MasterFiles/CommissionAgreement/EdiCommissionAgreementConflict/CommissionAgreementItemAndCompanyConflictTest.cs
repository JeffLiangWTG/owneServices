using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class CommissionAgreementItemAndCompanyConflictTest : TestCaseWithFactory
	{
		public void TestIsDeleted_DeleteAgreementItem()
		{
			var agreementItem = Factory.New<OrgCommissionAgreementItem>();
			var agreement = Factory.New<OrgCommissionAgreement>();
			var company = Factory.New<ClientCompany>();

			var conflict1 = new CommissionAgreementItemAndCompanyConflict(agreementItem, company, agreement);
			var conflict2 = new CommissionAgreementItemAndCompanyConflict(agreementItem, null, agreement);

			AssertEquals(false, conflict1.IsDeleted);
			AssertEquals(false, conflict2.IsDeleted);

			agreementItem.Delete();

			AssertEquals(true, conflict1.IsDeleted);
			AssertEquals(true, conflict2.IsDeleted);
		}

		public void TestIsDeleted_DeleteCompany()
		{
			var agreementItem = Factory.New<OrgCommissionAgreementItem>();
			var agreement = Factory.New<OrgCommissionAgreement>();
			var company = Factory.New<ClientCompany>();

			var conflict1 = new CommissionAgreementItemAndCompanyConflict(agreementItem, company, agreement);
			var conflict2 = new CommissionAgreementItemAndCompanyConflict(agreementItem, null, agreement);

			AssertEquals(false, conflict1.IsDeleted);
			AssertEquals(false, conflict2.IsDeleted);

			company.Delete();

			AssertEquals(true, conflict1.IsDeleted);
			AssertEquals(false, conflict2.IsDeleted);
		}

		public void TestEquals()
		{
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			var agreement2 = Factory.New<OrgCommissionAgreement>();

			var item1A = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1, "AAA", "AAA", "AAA");
			var item1B = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "BBB", "BBB", "BBB");

			var clientCompany1 = Factory.New<ClientCompany>();
			var clientCompany2 = Factory.New<ClientCompany>();

			var agreement1Draft = agreement1.CreateDraft();
			var item1ADraft = agreement1Draft.GetItem(Tuple.Create(ZBool.True, (ZString)"AAA"), Tuple.Create(ZBool.True, (ZString)"AAA"), Tuple.Create(ZBool.True, (ZString)"AAA"));

			AssertEqualsAndHashCode(true, new CommissionAgreementItemAndCompanyConflict(item1A, clientCompany1, agreement2), new CommissionAgreementItemAndCompanyConflict(item1A, clientCompany1, agreement2));
			AssertEqualsAndHashCode(false, new CommissionAgreementItemAndCompanyConflict(item1A, clientCompany1, agreement2), new CommissionAgreementItemAndCompanyConflict(item1A, clientCompany1, agreement1));
			AssertEqualsAndHashCode(false, new CommissionAgreementItemAndCompanyConflict(item1A, clientCompany1, agreement2), new CommissionAgreementItemAndCompanyConflict(item1A, clientCompany2, agreement2));
			AssertEqualsAndHashCode(false, new CommissionAgreementItemAndCompanyConflict(item1A, clientCompany1, agreement2), new CommissionAgreementItemAndCompanyConflict(item1B, clientCompany1, agreement2));

			AssertEqualsAndHashCode(true, new CommissionAgreementItemAndCompanyConflict(item1B, null, agreement2), new CommissionAgreementItemAndCompanyConflict(item1B, null, agreement2));

			AssertEqualsAndHashCode(true, new CommissionAgreementItemAndCompanyConflict(item1A, clientCompany1, agreement2), new CommissionAgreementItemAndCompanyConflict(item1ADraft, clientCompany1, agreement2));
		}

		void AssertEqualsAndHashCode<T>(bool expected, T x, T y)
		{
			AssertEquals(expected, x.Equals(y));

			if (expected)
			{
				AssertEquals(x.GetHashCode(), y.GetHashCode());
			}
			else
			{
				AssertNotEquals(x.GetHashCode(), y.GetHashCode());
			}
		}
	}
}