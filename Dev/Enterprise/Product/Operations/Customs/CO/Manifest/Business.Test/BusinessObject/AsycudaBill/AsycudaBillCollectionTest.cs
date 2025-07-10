using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillCollection))]
	sealed class AsycudaBillCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaBillCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills;
		}
	}
}
