using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationAddress))]
	sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			AssertType<CusGoodsLocationAddressValidation>(locationAddress.Validation);
		}

		public void TestCusGoodsLocation()
		{
			AssertType<CusGoodsLocation>(locationAddress.GoodsLocation);
		}

		public void TestCheckE2_Contact_MaxLength()
		{
			AssertEquals(70, locationAddress.E2_ContactInfo.MaxLength);
		}

		public void TestE2_GovRegNum_Caption() => CombineAssertions(() =>
		{
			var goodsLocation = Factory.New<CusGoodsLocation>();
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			AssertEquals($"CGL_Qualifier={goodsLocation.CGL_Qualifier}", "EORI Number", goodsLocation.Address.E2_GovRegNumInfo.Description);
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertEquals($"CGL_Qualifier={goodsLocation.CGL_Qualifier}", "Authorization Number", goodsLocation.Address.E2_GovRegNumInfo.Description);
		});

		public void TestIdentificationHolderPK()
		{
			CombineAssertions("Identification Holder should be defaulted with declaration Consignor if empty.", () =>
			{
				AssertNull("Null consignor", locationAddress.GoodsLocation.Header.Consignor.Organisation);

				locationAddress.IdentificationHolderPK = ZGuid.Empty;
				AssertEquals("Empty base and null consignor", ZGuid.Empty, locationAddress.IdentificationHolderPK);

				locationAddress.IdentificationHolderPK = ZGuid.Missing;
				AssertEquals("Non-empty base and null consignor", ZGuid.Missing, locationAddress.IdentificationHolderPK);

				locationAddress.IdentificationHolderPK = ZGuid.Empty;
				var consignor = Factory.New<OrgHeader>();
				locationAddress.GoodsLocation.Header.Consignor.E2_OA_Address = consignor.MainAddress.PK;

				AssertNotNull("Valid consignor", locationAddress.GoodsLocation.Header);

				locationAddress.IdentificationHolderPK = ZGuid.Empty;
				AssertEquals("Empty base and valid consignor", locationAddress.GoodsLocation.Header.Consignor.Organisation.PK, locationAddress.IdentificationHolderPK);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => locationAddress;

		protected override void SetUp()
		{
			base.SetUp();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_LocationUse = "ARR";
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			cusGoodsLocation.Parent = movementHeader;
			locationAddress = cusGoodsLocation.Address;
		}

		CusGoodsLocationAddress locationAddress;
	}
}
