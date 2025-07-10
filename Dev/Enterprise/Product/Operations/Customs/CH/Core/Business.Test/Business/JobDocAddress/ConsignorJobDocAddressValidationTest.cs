using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class ConsignorJobDocAddressValidationTest : JobDocAddressValidationTest
{
	public void TestCheckE2_OA_Address_NP70065()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		var consignor = Factory.New<OrgHeader>();
		consignor.MainAddress.OA_PostCode = "1234";

		var expectedMessageError = PassarValidationMessages.MessageNP70065_Consignor;

		declaration.ConsignorDocAddress.OrganisationPK = consignor.PK;
		AssertNoMessageErrorContaining("Assert if the postcode is in the list error occur", declaration.ConsignorDocAddress.E2_OA_AddressInfo, expectedMessageError);

		consignor.MainAddress.OA_PostCode = SwissCustomsConstants.PostCodes.Code7562;
		declaration.ConsignorDocAddress.OrganisationPK = ZGuid.Empty;
		declaration.ConsignorDocAddress.OrganisationPK = consignor.PK;
		AssertHasMessageErrorContaining("Assert if the postcode is in the list error occur", declaration.ConsignorDocAddress.E2_OA_AddressInfo, expectedMessageError);

		consignor.MainAddress.OA_PostCode = SwissCustomsConstants.PostCodes.Code7563;
		declaration.ConsignorDocAddress.OrganisationPK = ZGuid.Empty;
		declaration.ConsignorDocAddress.OrganisationPK = consignor.PK;
		AssertHasMessageErrorContaining("Assert if the postcode is in the list error occur", declaration.ConsignorDocAddress.E2_OA_AddressInfo, expectedMessageError);
	}
}
