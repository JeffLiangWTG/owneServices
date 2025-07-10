using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	internal class TemporaryStorageHeaderValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestLocationOfGoods_PopulateWhenCusPermitHeaderMatch()
		{
			var header = TemporaryStorageHeader.New(Factory);
			header.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeIST;

			var oldCGL_Qualifier = header.GoodsLocation.CGL_Qualifier;
			var oldCGL_Type = header.GoodsLocation.CGL_Type;
			var oldAddressIdentificationHolderPK = header.GoodsLocation.Address.IdentificationHolderPK;
			var oldAddressAuthorisationNumber = header.GoodsLocation.Address.AuthorisationNumber;

			header.AMA_OA_Declarant = ZGuid.NewZGuid();

			AssertEquals("If CusPermitHeader does not match, CGL_Qualifier should not be updated.", oldCGL_Qualifier, header.GoodsLocation.CGL_Qualifier);
			AssertEquals("If CusPermitHeader does not match, CGL_Type should not be updated.", oldCGL_Type, header.GoodsLocation.CGL_Type);
			AssertEquals("If CusPermitHeader does not match, Address should not be updated.", oldAddressIdentificationHolderPK, header.GoodsLocation.Address.IdentificationHolderPK);
			AssertEquals("If CusPermitHeader does not match, Address.AuthorisationNumber should not be updated.", oldAddressAuthorisationNumber, header.GoodsLocation.Address.AuthorisationNumber);

			var declarant = Factory.NewWithValidTestData<OrgAddress>();

			var cusPermitHeader = Factory.New<CusAuthorisationHeader>();
			cusPermitHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			cusPermitHeader.CPH_OH_PermitHolder = declarant.OA_OH;
			cusPermitHeader.CPH_OA_AppliesTo = declarant.PK;
			cusPermitHeader.CPH_Number = "FRTSTXXX";

			header.AMA_OA_Declarant = declarant.PK;

			AssertEquals("If CusPermitHeader match, CGL_Qualifier should be " + CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Codes.AuthorizationNumber, header.GoodsLocation.CGL_Qualifier);
			AssertEquals("If CusPermitHeader match, CGL_Type should be " + CusGoodsLocationTypeList.Codes.AuthorizedPlace + "when AMA_ManifestType = " + FRConstants.TemporaryStorage.AppCodeIST, CusGoodsLocationTypeList.Codes.AuthorizedPlace, header.GoodsLocation.CGL_Type);
			AssertEquals("If CusPermitHeader does not match, Address should be CPH_OH_PermitHolder", cusPermitHeader.CPH_OH_PermitHolder, header.GoodsLocation.Address.IdentificationHolderPK);
			AssertEquals("If CusPermitHeader does not match, Address.AuthorisationNumber should be CPH_OH_PermitHolder", cusPermitHeader.CPH_Number, header.GoodsLocation.Address.AuthorisationNumber);

			var header2 = TemporaryStorageHeader.New(Factory);
			header2.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeLAD;

			var representative = Factory.NewWithValidTestData<OrgAddress>();

			cusPermitHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			cusPermitHeader.CPH_OH_PermitHolder = representative.OA_OH;
			cusPermitHeader.CPH_OA_AppliesTo = declarant.PK;
			cusPermitHeader.CPH_Number = "LADTXXX";

			oldCGL_Qualifier = header2.GoodsLocation.CGL_Qualifier;
			oldCGL_Type = header2.GoodsLocation.CGL_Type;
			oldAddressIdentificationHolderPK = header2.GoodsLocation.Address.IdentificationHolderPK;
			oldAddressAuthorisationNumber = header2.GoodsLocation.Address.AuthorisationNumber;

			header2.AMA_OA_Declarant = declarant.PK;

			AssertEquals("If CusPermitHeader does not match, CGL_Qualifier should not be updated.", oldCGL_Qualifier, header2.GoodsLocation.CGL_Qualifier);
			AssertEquals("If CusPermitHeader does not match, CGL_Type should not be updated.", oldCGL_Type, header2.GoodsLocation.CGL_Type);
			AssertEquals("If CusPermitHeader does not match, Address should not be updated.", oldAddressIdentificationHolderPK, header2.GoodsLocation.Address.IdentificationHolderPK);
			AssertEquals("If CusPermitHeader does not match, Address.AuthorisationNumber should not be updated.", oldAddressAuthorisationNumber, header2.GoodsLocation.Address.AuthorisationNumber);

			header2.AMA_OA_Representative = representative.PK;

			AssertEquals("If CusPermitHeader match, CGL_Qualifier should be " + CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Codes.AuthorizationNumber, header2.GoodsLocation.CGL_Qualifier);
			AssertEquals("If CusPermitHeader match, CGL_Type should be " + CusGoodsLocationTypeList.Codes.ApprovedPlace + "when AMA_ManifestType = " + FRConstants.TemporaryStorage.AppCodeLAD, CusGoodsLocationTypeList.Codes.ApprovedPlace, header2.GoodsLocation.CGL_Type);
			AssertEquals("If CusPermitHeader does not match, Address should be CPH_OH_PermitHolder", cusPermitHeader.CPH_OH_PermitHolder, header2.GoodsLocation.Address.IdentificationHolderPK);
			AssertEquals("If CusPermitHeader does not match, Address.AuthorisationNumber should be CPH_OH_PermitHolder", cusPermitHeader.CPH_Number, header2.GoodsLocation.Address.AuthorisationNumber);

			var cusPermitHeader2 = Factory.New<CusAuthorisationHeader>();
			cusPermitHeader2.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			cusPermitHeader2.CPH_OH_PermitHolder = representative.OA_OH;
			cusPermitHeader2.CPH_OA_AppliesTo = declarant.PK;
			cusPermitHeader2.CPH_Number = "LADTYYY";

			var header3 = TemporaryStorageHeader.New(Factory);
			header3.AMA_ManifestType = FRConstants.TemporaryStorage.AppCodeLAD;

			oldCGL_Qualifier = header3.GoodsLocation.CGL_Qualifier;
			oldCGL_Type = header3.GoodsLocation.CGL_Type;
			oldAddressIdentificationHolderPK = header3.GoodsLocation.Address.IdentificationHolderPK;
			oldAddressAuthorisationNumber = header3.GoodsLocation.Address.AuthorisationNumber;

			header3.AMA_OA_Declarant = declarant.PK;
			header3.AMA_OA_Representative = representative.PK;

			AssertEquals("If multiple CusPermitHeaders match, CGL_Qualifier should not be updated.", oldCGL_Qualifier, header3.GoodsLocation.CGL_Qualifier);
			AssertEquals("If multiple CusPermitHeaders match, CGL_Type should not be updated.", oldCGL_Type, header3.GoodsLocation.CGL_Type);
			AssertEquals("If multiple CusPermitHeaders match, Address should not be updated.", oldAddressIdentificationHolderPK, header3.GoodsLocation.Address.IdentificationHolderPK);
			AssertEquals("If multiple CusPermitHeaders match, Address.AuthorisationNumber should not be updated.", oldAddressAuthorisationNumber, header3.GoodsLocation.Address.AuthorisationNumber);
		}
	}
}
