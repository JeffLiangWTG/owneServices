using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	[TestedType(typeof(APAccQueryClaimCollection))]
	class APAccQueryClaimCollectionTest : AccQueryClaimCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APAccQueryClaimCollection(Factory);
		}

		public void TestFiltering()
		{
			ARInvoice arHeader = Factory.NewWithValidTestData<ARInvoice>();
			arHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			ARAccQueryClaim aRQueryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			aRQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aRQueryClaim.AY_AH = arHeader.PK;
			APInvoice apHeader = Factory.NewWithValidTestData<APInvoice>();
			apHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			APAccQueryClaim aPQueryClaim = Factory.NewWithValidTestData<APAccQueryClaim>();
			aPQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			aPQueryClaim.AY_AH = apHeader.PK;
			GlbCompany foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch foreignBranch = Factory.NewWithValidTestData<GlbBranch>();
			foreignBranch.GB_GC = foreignCompany.PK;
			APInvoice apHeader2 = Factory.NewWithValidTestData<APInvoice>();
			apHeader2.AH_GB = foreignBranch.PK;
			APAccQueryClaim aPQueryClaim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			aPQueryClaim2.AY_GB = foreignBranch.PK;
			aPQueryClaim2.AY_AH = apHeader2.PK;
			ARAccQueryClaim queryClaimWithNoInvoice = Factory.NewWithValidTestData<ARAccQueryClaim>(); // should be treated as 'AR'
			queryClaimWithNoInvoice.AY_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			APAccQueryClaimCollection queryClaimCollection = new APAccQueryClaimCollection(Factory);
			queryClaimCollection.Load();
			AssertEquals("Collection should contain only the ap record.", 1, queryClaimCollection.Count);
			Assert("Collection should contain the APQueryClaim", queryClaimCollection.Contains(aPQueryClaim));
			queryClaimCollection = new APAccQueryClaimCollection(Factory, foreignCompany);
			queryClaimCollection.Load();
			AssertEquals("Collection should contain only the ap record.", 1, queryClaimCollection.Count);
			Assert("Collection should contain the APQueryClaim", queryClaimCollection.Contains(aPQueryClaim2));
		}

		public override AccQueryClaimCollection CallConstructorWithCompany()
		{
			return new APAccQueryClaimCollection(Factory, Factory.NewWithValidTestData<GlbCompany>());
		}
	}
}
