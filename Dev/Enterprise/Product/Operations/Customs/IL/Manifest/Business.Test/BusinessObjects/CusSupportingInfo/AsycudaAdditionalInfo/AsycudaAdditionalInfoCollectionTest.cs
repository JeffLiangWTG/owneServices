using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaAdditionalInfoCollection))]
	sealed class AsycudaAdditionalInfoCollectionTest : CusSupportingInfoCollectionTest<AsycudaAdditionalInfo>
	{
		protected override CusSupportingInfoCollection<AsycudaAdditionalInfo> GetCusSupportingInfoCollection()
		{
			var bill = Factory.New<AsycudaBill>();
			return new AsycudaAdditionalInfoCollection(bill);
		}

		public void TestSetDefaultValuesForNewChild_SubType()
		{
			var cusSupportingInfoCollection = GetCusSupportingInfoCollection();
			var val = cusSupportingInfoCollection.AddNew();
			Assertion.AssertEquals(cusSupportingInfoCollection.CSI_SubType, val.CSI_SubType);
			Assertion.AssertEquals(cusSupportingInfoCollection.Master.PK, val.Parent.PK);
		}
	}
}
