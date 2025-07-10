using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ParentJobTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		#region CreateCodeDescriptionPairListProvider

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ParentJobTypeCodeDescriptionPairProvider();
		}

		#endregion

		#region TestIsReturningCorrectCollection

		public override void TestIsReturningCorrectCollection()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("NON", "Standalone Booking (No Parent Job)");
			expectedList.AddPair("SHP", "Forwarding Shipment");
			expectedList.AddPair("ASH", "Agency Shipment");
			expectedList.AddPair("WHR", "Warehouse Receive");
			expectedList.AddPair("WHO", "Warehouse Order");
			expectedList.AddPair("CUS", "Customs Declaration");

			AssertContainsExactElementsInAnyOrder(expectedList, CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList());
		}

		#endregion
	}
}
