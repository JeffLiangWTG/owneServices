using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class OrganisationTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		#region CreateCodeDescriptionPairListProvider

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new OrganisationTypeCodeDescriptionPairProvider();
		}

		#endregion

		#region TestIsReturningCorrectCollection

		public override void TestIsReturningCorrectCollection()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("LCI", "Consignee / Importer");
			expectedList.AddPair("LCF", "CFS / Depot");
			expectedList.AddPair("LCT", "CTO / Wharf");
			expectedList.AddPair("LCW", "Warehouse");
			expectedList.AddPair("LCY", "Container Yard");

			AssertContainsExactElementsInAnyOrder(expectedList, CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList());
		}

		#endregion
	}
}
