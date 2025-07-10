using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class HouseBillTypePairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new HouseBillTypePairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("IAU", "IT Club Australia");
			expectedList.AddPair("ITP", "IT Club Australia Preprinted");
			expectedList.AddPair("INZ", "IT Club New Zealand");
			expectedList.AddPair("INP", "IT Club New Zealand Preprinted");
			expectedList.AddPair("TNZ", "TT Club / Australia / NZ");
			expectedList.AddPair("TTP", "TT Club / Australia / NZ Preprinted");
			expectedList.AddPair("FIA", "FIATA HBL");
			expectedList.AddPair("FIP", "FIATA HBL Preprinted");
			expectedList.AddPair("TAN", "TAN HBL");
			expectedList.AddPair("TAP", "TAN HBL Preprinted");
			expectedList.AddPair("EAG", "CargoWise Bill");
			expectedList.AddPair("EAP", "CargoWise Bill Preprinted");
			expectedList.AddPair("DHK", "DataHawk Bill");
			expectedList.AddPair("TUS", "TT Club United States");
			expectedList.AddPair("TUP", "TT Club United States Preprinted");
			expectedList.AddPair("CPT", "Carta Porte - Spanish");

			AssertContainsExactElementsInAnyOrder(expectedList, CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList());
		}
	}
}
