using System;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	public class CusGoodsLocationTest : EU.H7.Business.Testing.CusGoodsLocationTest
	{
		protected override Type LookupType => typeof(CusGoodsLocationLookups);

		public new void TestParent()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertType<AsycudaBill>(bill.CusGoodsLocation.Parent);
		}

		public void TestManifestParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestHeader>(header.CusGoodsLocation.ManifestHeaderParent);
		}
	}
}
