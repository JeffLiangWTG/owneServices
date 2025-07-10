using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AdditionalFiscalReferenceCollection))]
	sealed class AdditionalFiscalReferenceCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaximumCollectionCount()
		{
			AssertEquals(1, GetCollectionToTest().MaxCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return new AdditionalFiscalReferenceCollection(bill);
		}
	}
}
