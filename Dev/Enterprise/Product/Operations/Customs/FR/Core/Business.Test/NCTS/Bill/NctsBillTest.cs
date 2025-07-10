using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsBill))]
	sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGoodsItemsType()
		{
			AssertType<EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(nctsBill.GoodsItems);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => nctsBill;

		protected override BusinessObject GetNewBusinessObject() => nctsBill;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsBill;

		protected override void SetUp()
		{
			base.SetUp();
			nctsBill = CreateBill(Factory);
		}
		NctsBill nctsBill;
		NctsHeader nctsHeader;

		NctsBill CreateBill(BusinessObjectFactory factory)
		{
			nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew();
		}
	}
}
