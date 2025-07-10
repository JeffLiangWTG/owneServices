using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	[TestedType(typeof(ARAccQueryClaimCollection))]
	class ARAccQueryClaimCollectionTest : AccQueryClaimCollectionTest
	{
		public void TestAdditionalFilter()
		{
			GlbBranch otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch foreignBranch = Factory.NewWithValidTestData<GlbBranch>();
			foreignBranch.GB_GC = foreignCompany.PK;
			GlbBranch foreignBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			foreignBranch2.GB_GC = foreignCompany.PK;
			GlbCompany anotherforeignCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch foreignBranch3 = Factory.NewWithValidTestData<GlbBranch>();
			foreignBranch3.GB_GC = anotherforeignCompany.PK;
			ARInvoice arHeader = Factory.NewWithValidTestData<ARInvoice>();
			arHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			ARAccQueryClaim arQueryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			arQueryClaim.AY_GB = otherBranch.PK;
			arQueryClaim.AY_AH = arHeader.PK;
			ARInvoice arHeader2 = Factory.NewWithValidTestData<ARInvoice>();
			arHeader2.AH_GB = foreignBranch.PK;
			ARAccQueryClaim arQueryClaim2 = Factory.NewWithValidTestData<ARAccQueryClaim>();
			arQueryClaim2.AY_GB = foreignBranch.PK;
			arQueryClaim2.AY_AH = arHeader2.PK;
			ARInvoice arHeaderOtherBranch = Factory.NewWithValidTestData<ARInvoice>();
			arHeaderOtherBranch.AH_GB = otherBranch.PK;
			ARAccQueryClaim arQueryClaimOtherBranch = Factory.NewWithValidTestData<ARAccQueryClaim>();
			arQueryClaimOtherBranch.AY_GB = GlbBranch.CurrentBranch.PK;
			arQueryClaimOtherBranch.AY_AH = arHeaderOtherBranch.PK;
			ARInvoice arHeaderOtherForeignBranch = Factory.NewWithValidTestData<ARInvoice>();
			arHeaderOtherForeignBranch.AH_GB = foreignBranch3.PK;
			ARAccQueryClaim arQueryClaimOtherForeignBranch = Factory.NewWithValidTestData<ARAccQueryClaim>();
			arQueryClaimOtherForeignBranch.AY_GB = foreignBranch3.PK;
			arQueryClaimOtherForeignBranch.AY_AH = arHeaderOtherForeignBranch.PK;
			ARAccQueryClaim queryClaimWithNoInvoice = Factory.NewWithValidTestData<ARAccQueryClaim>(); // should be treated as 'AR'
			queryClaimWithNoInvoice.AY_GB = GlbBranch.CurrentBranch.PK;
			ARAccQueryClaim queryClaimWithNoInvoice2 = Factory.NewWithValidTestData<ARAccQueryClaim>(); // should be treated as 'AR'
			queryClaimWithNoInvoice2.AY_GB = foreignBranch2.PK;
			ARAccQueryClaim queryClaimWithNoInvoiceOtherBranch = Factory.NewWithValidTestData<ARAccQueryClaim>(); // should be treated as 'AR'
			queryClaimWithNoInvoiceOtherBranch.AY_GB = otherBranch.PK;
			ARAccQueryClaim queryClaimForeignWithNoInvoiceOtherBranch = Factory.NewWithValidTestData<ARAccQueryClaim>(); // should be treated as 'AR'
			queryClaimForeignWithNoInvoiceOtherBranch.AY_GB = foreignBranch3.PK;
			APInvoice apHeader = Factory.NewWithValidTestData<APInvoice>();
			apHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			APAccQueryClaim apQueryClaim = Factory.NewWithValidTestData<APAccQueryClaim>();
			apQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			apQueryClaim.AY_AH = apHeader.PK;
			APInvoice intercompanyAPHeader = Factory.NewWithValidTestData<APInvoice>();
			intercompanyAPHeader.AH_GB = otherBranch.PK;
			APAccQueryClaim intercompanyAPQueryClaim = Factory.NewWithValidTestData<APAccQueryClaim>();
			intercompanyAPQueryClaim.AY_GB = GlbBranch.CurrentBranch.PK;
			intercompanyAPQueryClaim.AY_AH = intercompanyAPHeader.PK;
			APInvoice intercompanyAPHeaderAssignedToWrongBranch = Factory.NewWithValidTestData<APInvoice>();
			intercompanyAPHeaderAssignedToWrongBranch.AH_GB = otherBranch.PK;
			APAccQueryClaim intercompanyAPQueryClaimAssignedToWrongBranch = Factory.NewWithValidTestData<APAccQueryClaim>();
			intercompanyAPQueryClaimAssignedToWrongBranch.AY_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			intercompanyAPQueryClaimAssignedToWrongBranch.AY_AH = intercompanyAPHeaderAssignedToWrongBranch.PK;
			APInvoice apHeader2 = Factory.NewWithValidTestData<APInvoice>();
			apHeader2.AH_GB = foreignBranch.PK;
			APAccQueryClaim apQueryClaim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			apQueryClaim2.AY_GB = foreignBranch.PK;
			apQueryClaim2.AY_AH = apHeader2.PK;
			APInvoice intercompanyAPHeader2 = Factory.NewWithValidTestData<APInvoice>();
			intercompanyAPHeader2.AH_GB = foreignBranch3.PK;
			APAccQueryClaim intercompanyAPQueryClaim2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			intercompanyAPQueryClaim2.AY_GB = foreignBranch.PK;
			intercompanyAPQueryClaim2.AY_AH = intercompanyAPHeader2.PK;
			APInvoice intercompanyAPHeaderAssignedToWrongBranch2 = Factory.NewWithValidTestData<APInvoice>();
			intercompanyAPHeaderAssignedToWrongBranch2.AH_GB = foreignBranch3.PK;
			APAccQueryClaim intercompanyAPQueryClaimAssignedToWrongBranch2 = Factory.NewWithValidTestData<APAccQueryClaim>();
			intercompanyAPQueryClaimAssignedToWrongBranch2.AY_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			intercompanyAPQueryClaimAssignedToWrongBranch2.AY_AH = intercompanyAPHeaderAssignedToWrongBranch2.PK;
			Factory.Save();
			AccQueryClaimCollection queryClaimCollection = new ARAccQueryClaimCollection(Factory);
			queryClaimCollection.Load();
			AssertEquals("Number of records", 3, queryClaimCollection.Count);
			Assert("Collection should contain the arQueryClaim", queryClaimCollection.Contains(arQueryClaim));
			Assert("Collection should contain the queryclaimwithNoInvoice", queryClaimCollection.Contains(queryClaimWithNoInvoice));
			Assert("Collection should contain the intercompanyAPQueryClaim", queryClaimCollection.Contains(intercompanyAPQueryClaim));
			queryClaimCollection = new ARAccQueryClaimCollection(Factory, foreignCompany);
			queryClaimCollection.Load();
			AssertEquals("Number of records", 3, queryClaimCollection.Count);
			Assert("Collection should contain the arQueryClaim", queryClaimCollection.Contains(arQueryClaim2));
			Assert("Collection should contain the arQueryClaim", queryClaimCollection.Contains(queryClaimWithNoInvoice2));
			Assert("Collection should contain the intercompanyAPQueryClaim", queryClaimCollection.Contains(intercompanyAPQueryClaim2));
		}

		public override AccQueryClaimCollection CallConstructorWithCompany()
		{
			return new ARAccQueryClaimCollection(Factory, Factory.NewWithValidTestData<GlbCompany>());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ARAccQueryClaimCollection(Factory);
		}
	}
}
