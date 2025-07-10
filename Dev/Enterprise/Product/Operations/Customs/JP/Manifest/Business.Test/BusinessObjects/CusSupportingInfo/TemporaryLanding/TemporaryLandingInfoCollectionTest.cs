using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(TemporaryLandingInfoCollection))]
	sealed class TemporaryLandingInfoCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<TemporaryLandingInfo>
	{
		public void TestAllowNewCore()
		{
			var testCollection = GetCusSupportingInfoCollection();
			Assert("Empty collection", testCollection.AllowNew);

			testCollection.AddNew();
			Assert("Contain 1 element", !testCollection.AllowNew);
		}

		protected override CusSupportingInfoCollection<TemporaryLandingInfo> GetCusSupportingInfoCollection()
		{
			var bill = Factory.New<AsycudaBill>();
			return new TemporaryLandingInfoCollection(bill);
		}
	}
}
