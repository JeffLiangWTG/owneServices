using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class CommissionAgreementItemAndCompanyAutoAddCountryConflictTest : TestCaseWithFactory
	{
		public void TestIsDeleted_DeleteAgreementItem()
		{
			var agreementItem = Factory.New<OrgCommissionAgreementItem>();
			var agreement = Factory.New<OrgCommissionAgreement>();
			var database = Factory.New<LicenceDatabase>();

			var conflict1 = new CommissionAgreementItemAndCompanyAutoAddCountryConflict(agreementItem, database, "AU", agreement);
			var conflict2 = new CommissionAgreementItemAndCompanyAutoAddCountryConflict(agreementItem, null, "AU", agreement);

			AssertEquals(false, conflict1.IsDeleted);
			AssertEquals(false, conflict2.IsDeleted);

			agreementItem.Delete();

			AssertEquals(true, conflict1.IsDeleted);
			AssertEquals(true, conflict2.IsDeleted);
		}

		public void TestIsDeleted_DeleteDatabase()
		{
			var agreementItem = Factory.New<OrgCommissionAgreementItem>();
			var agreement = Factory.New<OrgCommissionAgreement>();
			var database = Factory.New<LicenceDatabase>();

			var conflict1 = new CommissionAgreementItemAndCompanyAutoAddCountryConflict(agreementItem, database, "AU", agreement);
			var conflict2 = new CommissionAgreementItemAndCompanyAutoAddCountryConflict(agreementItem, null, "AU", agreement);

			AssertEquals(false, conflict1.IsDeleted);
			AssertEquals(false, conflict2.IsDeleted);

			database.Delete();

			AssertEquals(true, conflict1.IsDeleted);
			AssertEquals(false, conflict2.IsDeleted);
		}

		public void TestEquals()
		{
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			var agreement2 = Factory.New<OrgCommissionAgreement>();

			var item1A = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1, "AAA", "AAA", "AAA");
			var item1B = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "BBB", "BBB", "BBB");

			var database1 = Factory.New<LicenceDatabase>();
			var database2 = Factory.New<LicenceDatabase>();

			var agreement1Draft = agreement1.CreateDraft();
			var item1ADraft = agreement1Draft.GetItem(Tuple.Create(ZBool.True, (ZString)"AAA"), Tuple.Create(ZBool.True, (ZString)"AAA"), Tuple.Create(ZBool.True, (ZString)"AAA"));

			AssertEqualsAndHashCode(true, new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "AU", agreement2), new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "AU", agreement2));
			AssertEqualsAndHashCode(false, new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "AU", agreement2), new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "US", agreement2));
			AssertEqualsAndHashCode(false, new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "AU", agreement2), new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "AU", agreement1));
			AssertEqualsAndHashCode(false, new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "AU", agreement2), new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database2, "AU", agreement2));
			AssertEqualsAndHashCode(false, new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "AU", agreement2), new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1B, database1, "AU", agreement2));

			AssertEqualsAndHashCode(true, new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1B, null, "AU", agreement2), new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1B, null, "AU", agreement2));

			AssertEqualsAndHashCode(true, new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1A, database1, "AU", agreement2), new CommissionAgreementItemAndCompanyAutoAddCountryConflict(item1ADraft, database1, "AU", agreement2));
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