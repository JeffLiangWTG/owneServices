using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsBill))]
	sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
	{
		NctsBill nctsBill;
		public void TestGoodsItems()
		{
			AssertType<EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(nctsBill.GoodsItems);
		}

		public void TestCusInBondCargoDescType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), ((ICusInBondCargoDescTypeProvider)nctsBill).CusInBondCargoDescType);
		}

		public void TestGetCusSupportingInfoTypes_PreviousDocument()
		{
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsBill).GetCusSupportingInfoTypes();
			AssertEquals(typeof(CommonPreviousDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument]);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		public static NctsBill GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var nctsHeader = NctsHeaderTest.GetNewBusinessObject(factory);
			return nctsHeader.Bills.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsBill = GetNewBusinessObject(Factory);
		}
	}
}
