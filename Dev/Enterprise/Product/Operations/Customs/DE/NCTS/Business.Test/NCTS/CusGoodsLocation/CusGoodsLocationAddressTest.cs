using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationAddress))]
	sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<CusGoodsLocationAddressValidation>(cusGoodsLocationAddress.Validation);
		}

		public void TestE2_Contact_MaxLength()
		{
			AssertEquals(70, cusGoodsLocationAddress.E2_ContactInfo.MaxLength);
		}

		public void TestE2_City()
		{
			AssertEquals(35, cusGoodsLocationAddress.E2_CityInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => cusGoodsLocationAddress;

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			cusGoodsLocationAddress = movementHeader.GoodsLocation.Address;
		}

		CusGoodsLocationAddress cusGoodsLocationAddress;
	}
}
