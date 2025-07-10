using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	[TestedType(typeof(OrgARQueryClaimDependentCollection))]
	public class OrgARQueryClaimDependentCollectionTest : MasterFiles.Business.Testing.OrgQueryClaimDependentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgARQueryClaimDependentCollection(Org, Factory);
		}

		public void TestLedgerFilter()
		{
			ARInvoice arHeader = Factory.NewWithValidTestData<ARInvoice>();
			ARAccQueryClaim aRQueryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			aRQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aRQueryClaim.AY_AH = arHeader.PK;
			aRQueryClaim.AY_OH_Debtor = Org.PK;
			aRQueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			APInvoice apHeader = Factory.NewWithValidTestData<APInvoice>();
			APAccQueryClaim aPQueryClaim = Factory.NewWithValidTestData<APAccQueryClaim>();
			aPQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aPQueryClaim.AY_AH = apHeader.PK;
			aPQueryClaim.AY_OH_Debtor = Org.PK;
			aPQueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			ARAccQueryClaim queryClaimWithNoInvoice = Factory.NewWithValidTestData<ARAccQueryClaim>(); // should be treated as 'AR'
			queryClaimWithNoInvoice.AY_GB = GlbBranch.CurrentBranch.PK;
			queryClaimWithNoInvoice.AY_OH_Debtor = Org.PK;
			queryClaimWithNoInvoice.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			Factory.Save();
			BusinessObjectCollection queryClaimCollection = GetCollectionToTest();
			queryClaimCollection.Load();
			AssertEquals("Collection should contain only the AR records.", 2, queryClaimCollection.Count);
			Assert("Collection should contain the ARQueryClaim", queryClaimCollection.Contains(aRQueryClaim));
			Assert("Collection should contain the ARQueryClaim", queryClaimCollection.Contains(queryClaimWithNoInvoice));
		}

		public void TestGlbCompanyFilter()
		{
			GlbCompany foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch foreignBranch = Factory.NewWithValidTestData<GlbBranch>();
			foreignBranch.GB_GC = foreignCompany.PK;
			AccQueryClaim queryClaim = (AccQueryClaim)Factory.NewWithValidTestData(typeof(APAccQueryClaim));
			queryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			queryClaim.AY_OH_Debtor = Org.PK;
			queryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			AccQueryClaim foreignQueryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			foreignQueryClaim.AY_GB = foreignBranch.PK;
			foreignQueryClaim.AY_OH_Debtor = Org.PK;
			foreignQueryClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			Factory.Save();
			OrgQueryClaimDependentCollection queryClaimCollection = (OrgQueryClaimDependentCollection)GetCollectionToTest();
			queryClaimCollection.Load();
			AssertEquals("Collection should contain only one record.", 1, queryClaimCollection.Count);
			Assert("Collection should contain the QueryClaim", queryClaimCollection.Contains(queryClaim));
		}
	}
}
