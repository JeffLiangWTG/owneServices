using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
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

		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals("ABL_BolType default value should be STD", "STD", bill.ABL_BolType);
		}
	}
}
