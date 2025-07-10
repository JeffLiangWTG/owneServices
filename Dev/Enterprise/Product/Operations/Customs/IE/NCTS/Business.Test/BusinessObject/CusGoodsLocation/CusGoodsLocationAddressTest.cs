using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationAddress))]
	sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<CusGoodsLocationAddressValidation>(cusGoodsLocationAddress.Validation);
		}

		public void TestAuthorisationNumberReadOnly()
		{
			cusGoodsLocationAddress.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertEquals("AuthorisationNumber readonly when CGL_Qualifier not Y", true, cusGoodsLocationAddress.AuthorisationNumberInfo.ReadOnly);

			cusGoodsLocationAddress.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertEquals("AuthorisationNumber writable when CGL_Qualifier Y", false, cusGoodsLocationAddress.AuthorisationNumberInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader.GoodsLocation.Address;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			cusGoodsLocationAddress = nctsHeader.MovementHeader.GoodsLocation.Address;
		}

		CusGoodsLocationAddress cusGoodsLocationAddress;
	}
}
