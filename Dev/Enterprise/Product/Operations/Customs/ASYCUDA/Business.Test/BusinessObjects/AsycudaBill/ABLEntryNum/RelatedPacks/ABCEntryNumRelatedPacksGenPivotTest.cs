using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(ABLEntryNumRelatedPacksGenPivot))]
	sealed class ABCEntryNumRelatedPacksGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var entryNum = Factory.New<ABLEntryNum>();
			var pivot = entryNum.PackPivots.AddNew();
			AssertEquals(CusEntryNumSchema.Constants.Prefix, pivot.XX_Relation1TableCode);
			AssertEquals(AsycudaPackSchema.Constants.Prefix, pivot.XX_Relation2TableCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			return entryNum.PackPivots.AddNew();
		}
	}
}
