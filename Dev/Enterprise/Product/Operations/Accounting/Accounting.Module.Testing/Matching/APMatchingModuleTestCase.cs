using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APMatchingModule))]
	public class APMatchingModuleTestCase : MatchingModuleTestCase
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ZAPMatching;
		}

		public void TestFillCollection()
		{
			SetupTestData();
			LoadCollection_Exposed();

			UnmatchingRowCollection gridCollection = (UnmatchingRowCollection)((IFilterGridModuleInternalsForTesting)fMatchingModule).GridCollection;
			AssertEquals("Grid collection should have 3 elements in it", 3, gridCollection.Count);
			Assert("Grid collection should contain MatchGroupNum M00001334", gridCollection.ContainsMatchGroupNumber("M00001334"));
			Assert("Grid collection should contain MatchGroup M00001523", gridCollection.ContainsMatchGroupNumber("M00001523"));
			Assert("Grid collection should contain MatchGroup M00009332", gridCollection.ContainsMatchGroupNumber("M00009332"));

			APMatchingFilterBusinessObject matchingFilterBizO = (APMatchingFilterBusinessObject)((IFilterGridModuleInternalsForTesting)fMatchingModule).FilterBusinessObject;
			((ModuleGuidFilter)matchingFilterBizO["Organisation"]).Property = TestOrg2.PK;
			((ModuleGuidFilter)matchingFilterBizO["Organisation"]).IsActive = true;

			LoadCollection_Exposed();
			gridCollection = (UnmatchingRowCollection)((IFilterGridModuleInternalsForTesting)fMatchingModule).GridCollection;

			AssertEquals("Grid collection should have 1 element in it", 1, gridCollection.Count);
			Assert("Grid collection should contain MatchGroup M00009332", gridCollection.ContainsMatchGroupNumber("M00009332"));

			((ModuleGuidFilter)matchingFilterBizO["Organisation"]).Property = TestOrg.PK;
			((ModuleGuidFilter)matchingFilterBizO["Organisation"]).IsActive = true;

			LoadCollection_Exposed();
			gridCollection = (UnmatchingRowCollection)((IFilterGridModuleInternalsForTesting)fMatchingModule).GridCollection;

			AssertEquals("Grid collections should have 3 elements in it", 3, gridCollection.Count);
		}
	}
}
