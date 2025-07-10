using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	[TestedType(typeof(H7Bill))]
	sealed class H7BillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestHeader()
		{
			var bill = GetNewBusinessObject() as H7Bill;
			AssertType<H7ManifestHeader>(bill.Header);
		}

		public void TestCusGoodsLocationType()
		{
			var bill = GetNewBusinessObject() as H7Bill;
			var cusGoodsLocationProvider = bill as ICusGoodsLocationProvider;

			CombineAssertions("CusGoodsLocation Type", () =>
			{
				AssertType<CusGoodsLocation>("ICusGoodsLocationProvider.GoodsLocation", cusGoodsLocationProvider.GoodsLocation);
				AssertType<CusGoodsLocation>("CusGoodsLocation", bill.CusGoodsLocation);
			});
		}

		public void TestLookupsType()
		{
			var bill = GetNewBusinessObject() as H7Bill;
			AssertType<H7BillLookups>(bill.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<H7ManifestHeader>();
			return header.Bills.AddNew();
		}
	}
}
