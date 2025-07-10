using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARMatchingModule))]
	public class ARMatchingModuleTestCase : MatchingModuleTestCase
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ZARMatching;
		}

		public void TestFillCollection()
		{
			SetupTestData();

			LoadCollection_Exposed();

			UnmatchingRowCollection gridCollection = (UnmatchingRowCollection)((IFilterGridModuleInternalsForTesting)fMatchingModule).GridCollection;
			AssertEquals("There should be 2 elements in the collection", 2, gridCollection.Count);
			Assert("GridCollection should contain MatchGroup M00001334", gridCollection.ContainsMatchGroupNumber("M00001334"));
			Assert("GridCollection should contain MatchGroup M00001442", gridCollection.ContainsMatchGroupNumber("M00001442"));

			ARMatchingFilterBusinessObject matchingFilterBizO = (ARMatchingFilterBusinessObject)((IFilterGridModuleInternalsForTesting)fMatchingModule).FilterBusinessObject;
			((ModuleGuidFilter)matchingFilterBizO["Organisation"]).Property = TestOrg2.PK;
			((ModuleGuidFilter)matchingFilterBizO["Organisation"]).IsActive = true;

			LoadCollection_Exposed();
			gridCollection = (UnmatchingRowCollection)((IFilterGridModuleInternalsForTesting)fMatchingModule).GridCollection;

			AssertEquals("There should be 1 element in the collection", 1, gridCollection.Count);
			Assert("Grid collection should contain MatchGroup M00001442", gridCollection.ContainsMatchGroupNumber("M00001442"));

			((ModuleGuidFilter)matchingFilterBizO["Organisation"]).Property = TestOrg.PK;
			((ModuleGuidFilter)matchingFilterBizO["Organisation"]).IsActive = true;

			LoadCollection_Exposed();
			gridCollection = (UnmatchingRowCollection)((IFilterGridModuleInternalsForTesting)fMatchingModule).GridCollection;

			AssertEquals("There should be 2 elements in the collection", 2, gridCollection.Count);
		}
	}
}
