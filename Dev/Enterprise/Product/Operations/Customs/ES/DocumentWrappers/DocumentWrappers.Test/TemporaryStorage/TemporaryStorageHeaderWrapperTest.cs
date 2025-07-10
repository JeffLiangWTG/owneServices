using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.ES.DocumentWrappers.TemporaryStorage.Testing;

[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Spain)]
[TestedType(typeof(TemporaryStorageHeaderWrapper))]
sealed class TemporaryStorageHeaderWrapperTest : DocBaseWrapperTest
{
	public void TestDestinationGoodsLocationAuthorisationNumber()
	{
		CombineAssertions(() =>
		{
			var destinationGoodsLocation = header.DestinationGoodsLocation;
			destinationGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertEquals("Address.AuthorisationNumber", ZString.Empty, Wrapper.DestinationGoodsLocationAuthorisationNumber);

			destinationGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			destinationGoodsLocation.Address.AuthorisationNumber = "ESAR1";
			AssertEquals("Address.AuthorisationNumber", "ESAR1", Wrapper.DestinationGoodsLocationAuthorisationNumber);
		});
	}

	public void TestPremises()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AR1";
		orgHeader.OH_FullName = "ACME";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		_ = AddNewPremise(CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse, "COD", "DESC", orgAddress.PK, "ES009999000001");
		_ = AddNewPremise(CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility, "CO2", "DESC2", orgAddress.PK, "ES009999000002");
		Factory.Save();

		CombineAssertions(() =>
		{
			header.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000001";
			AssertEquals("ADT", Wrapper.PremisesType);
			AssertEquals("COD", Wrapper.PremisesCode);
			AssertEquals("DESC", Wrapper.PremisesDescription);
			AssertEquals("ACME", Wrapper.PremisesOwner);

			header.DestinationGoodsLocation.Address.AuthorisationNumber = "ES009999000002";
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			var wrapper1 = TemporaryStorageHeaderWrapper.New(header, Factory);
			AssertEquals("LAM", wrapper1.PremisesType);
			AssertEquals("CO2", wrapper1.PremisesCode);
			AssertEquals("DESC2", wrapper1.PremisesDescription);
			AssertEquals("ACME", wrapper1.PremisesOwner);

			header.DestinationGoodsLocation.Address.AuthorisationNumber = "XXX";
			wrapper1 = TemporaryStorageHeaderWrapper.New(header, Factory);
			AssertEquals(ZString.Empty, wrapper1.PremisesType);
			AssertEquals(ZString.Empty, wrapper1.PremisesCode);
			AssertEquals(ZString.Empty, wrapper1.PremisesDescription);
			AssertEquals(ZString.Empty, wrapper1.PremisesOwner);
		});
	}

	CusTempStorageRegPremises AddNewPremise(string type, string code, string description, ZGuid addressPK, string location)
	{
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = type;
		premises.SRP_Code = code;
		premises.SRP_Description = description;
		premises.SRP_OA_PremisesAddress = addressPK;
		premises.SRP_CustomsLocation = location;
		return premises;
	}

	new TemporaryStorageHeaderWrapper Wrapper => (TemporaryStorageHeaderWrapper)base.Wrapper;

	protected override DocBaseWrapper GetNewDocumentWrapper() => TemporaryStorageHeaderWrapper.New(header, Factory);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader header;
}
