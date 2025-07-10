using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(OrganisationCPQA))]
	public class OrganisationCPQATest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2006, 1, 6)]
		public void TestSelectionDate()
		{
			AssertEquals("Selection date:", new ZDateTime(2006, 1, 6), orgAttachee.SelectionDate);
		}

		public void TestFKColumnInCusEntryCPDecTable()
		{
			AssertEquals(CusEntryCPDecSchema.ON_ParentID, orgAttachee.FKColumnInCusEntryCPDecTable);
		}

		public void TestNotAddedToFactoryCache()
		{
			AssertEquals("Data should not be cached in the factory", 0, Factory.GetBizOsForPK(orgAttachee.PK.ToGuid()).Length);
		}

		public void TestIsInDatabase()
		{
			AssertEquals("IsInDatabase", true, orgAttachee.IsInDatabase);
			OrgHeader newOrg = Factory.NewWithValidTestData<OrgHeader>();

			OrganisationCPQA testOrgAttachee = new OrganisationCPQA(newOrg);
			AssertEquals("IsInDatabase", false, testOrgAttachee.IsInDatabase);
		}

		public void TestIsRiskCalculatedFromTariff()
		{
			ICPQALineAttachee orgAttacheeAsICPQA = orgAttachee;
			AssertEquals("IsRiskCalculatedFromTariff", false, orgAttacheeAsICPQA.IsRiskCalculatedFromTariff);
		}

		public void TestIsRiskHistorySupported()
		{
			var orgAttacheeAsICPQA = orgAttachee as ICPQALineAttachee;
			AssertEquals("IsRiskHistorySupported", false, orgAttacheeAsICPQA.IsRiskHistorySupported);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return orgAttachee;
		}

		#region Implementation

		OrganisationCPQA orgAttachee;

		protected override void SetUp()
		{
			base.SetUp();
			orgAttachee = new OrganisationCPQA(Org);
		}

		OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
				}
				return fOrg;
			}
		}
		OrgHeader fOrg;

		#endregion
	}
}
