using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(ABLEntryNumRelatedPacksGenPivotCollection))]
	sealed class ABCEntryNumRelatedPacksGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			return entryNum.PackPivots;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ABLEntryNumRelatedPacksGenPivot>();
	}
}
