using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Filters.Testing
{
	[TestedType(typeof(OrgLedgerFilter))]
	public class OrgLedgerFilterTestCase : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgLedgerFilter(Factory, new OrgLedgerFilterCollection(Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.OH_IsDebtor = false;
			TestOrg1.OH_IsCreditor = true;
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2.OH_IsDebtor = true;
			TestOrg2.OH_IsCreditor = false;
		}

		protected OrgHeader TestOrg1;
		protected OrgHeader TestOrg2;

		protected OrgLedgerFilter TestOrgAccInfo;

		protected OrgLedgerFilterCollection OrgAccInfoCollection
		{
			get
			{
				if (fOrgAccInfoCollection == null)
				{
					fOrgAccInfoCollection = new OrgLedgerFilterCollection(Factory);
				}
				return fOrgAccInfoCollection;
			}
		}
		protected OrgLedgerFilterCollection fOrgAccInfoCollection;

		public void TestOrganizationValidation()
		{
			OrgLedgerFilter orgAccInfo1 = OrgAccInfoCollection.AddNew();
			OrgLedgerFilter orgAccInfo2 = OrgAccInfoCollection.AddNew();

			orgAccInfo1.Organization = TestOrg1.PK;
			orgAccInfo2.Organization = TestOrg2.PK;

			TestOrgAccInfo = OrgAccInfoCollection.AddNew();

			TestOrgAccInfo.Organization = TestOrg1.PK;
			Assert("Organization should have errors since an OrgInfo with this organization already exists in the collection", TestOrgAccInfo.OrganizationInfo.HasErrors());
		}

		public void TestDebtorCreditorCollection()
		{
			Factory.Save();

			OrgLedgerFilter ledgerFilter1 = OrgAccInfoCollection.AddNew();
			ledgerFilter1.OrgHeaders.Load();
			Assert("Collection should contain creditors", ledgerFilter1.OrgHeaders.Contains(TestOrg1));
			Assert("Collection should contain debtors", ledgerFilter1.OrgHeaders.Contains(TestOrg2));
		}
	}
}
