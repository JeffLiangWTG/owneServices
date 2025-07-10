using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	[TestedType(typeof(AsycudaManifestModuleCollection))]

	sealed class AsycudaManifestModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultFilter()
		{
			var branchInLoginCompany = Factory.NewWithValidTestData<GlbBranch>();
			branchInLoginCompany.GB_GC = GlbCompany.CurrentCompany.PK;

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_GC = otherCompany.PK;

			var euh7Header = Factory.New<AsycudaManifestHeader>();
			euh7Header.AMA_GB = branchInLoginCompany.PK;
			euh7Header.AMA_JobReference = "TestReference1";

			var euh7HeaderInOtherBranch = Factory.New<AsycudaManifestHeader>();
			euh7HeaderInOtherBranch.AMA_GB = otherBranch.PK;
			euh7HeaderInOtherBranch.AMA_JobReference = "TestReference2";

			var otherHeader = Factory.New<ASYCUDA.Business.AsycudaManifestHeader>();
			otherHeader.AMA_GB = branchInLoginCompany.PK;
			otherHeader.AMA_JobReference = "TestReference3";

			Factory.Save();

			var collection = new AsycudaManifestModuleCollection(Factory);
			collection.Load();

			CombineAssertions("Should find only 1 manifest header under H7 job matching branch of current company", () =>
			{
				AssertEquals(1, collection.Count);
				AssertEquals(euh7Header.PK, collection[0].PK);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaManifestModuleCollection(Factory);
	}
}
