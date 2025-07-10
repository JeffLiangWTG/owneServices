using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class OrgAddressExtensionTest : TestCaseWithFactory
	{
		public void TestGetEOROrNIFCode()
		{
			var customCode = address.CustomsCodes.AddNew();

			CombineAssertions("When orgHeader is null or there are no CustomsCodes", () =>
			{
				AssertEquals("GetEOROrNIFCode returns empty string when there is no nif or eori in the orgHeader", ZString.Empty, address.GetEOROrNIFCode());
				AssertEquals("GetEOROrNIFCode returns empty string when orgAddress is null", ZString.Empty, (null as OrgAddress).GetEOROrNIFCode());
			});

			CombineAssertions("When orgHeader.OH_Category is NAT", () =>
			{
				address.Header.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "12456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEOROrNIFCode returns empty string when customsCode country is not ES", ZString.Empty, address.GetEOROrNIFCode());

				customCode.OK_RN_NKCodeCountry = "ES";
				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEOROrNIFCode returns empty string when customsCode codeType is NIF and country is ES but the regNo is empty", ZString.Empty, address.GetEOROrNIFCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";

				AssertEquals("GetEOROrNIFCode returns Country+Registration Number when customsCode country is ES but codeType is not NIF", "ES123456789A", address.GetEOROrNIFCode());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				AssertEquals("GetEOROrNIFCode returns correct value when customsCode country is ES and codeType is NIF", "123456789A", address.GetEOROrNIFCode());
			});

			CombineAssertions("When orgHeader.OH_Category is not NAT", () =>
			{
				address.Header.OH_Category = OrgConstants.Category.Business;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEOROrNIFCode returns empty string when customsCode codeType is NIF and Country is not ES", ZString.Empty, address.GetEOROrNIFCode());

				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEOROrNIFCode returns empty string when customsCode codeType is NIF but the regNo is empty", ZString.Empty, address.GetEOROrNIFCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEOROrNIFCode returns correct value when customsCode codeType is EOR", "IT123456789A", address.GetEOROrNIFCode());

				customCode.OK_CustomsRegNo = "IT123456789A";
				AssertEquals("GetEOROrNIFCode returns correct value when customsCode codeType is EOR and even if countryCode is in the regNo", "IT123456789A", address.GetEOROrNIFCode());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("GetEOROrNIFCode returns Country+Registration Number value when customsCode codeType is NIF, countryCode is ES and the category is Business", "ES123456789A", address.GetEOROrNIFCode());

				customCode.OK_CustomsRegNo = "A56789123";
				address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				AssertEquals("GetEOROrNIFCode returns EOR value when it has EOR and NIF Registration Numbers", "ESA12345678", address.GetEOROrNIFCode());
			});

			CombineAssertions("When there are multiple EORIs", () =>
			{
				address.CustomsCodes.DeleteAll();
				customCode = address.CustomsCodes.AddNew();

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "XI123456789A";
				customCode.OK_RN_NKCodeCountry = "GB";
				customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

				AssertEquals("GetEOROrNIFCode returns GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI", "XI123456789A", address.GetEOROrNIFCode());

				var customsCode2 = address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("GetEOROrNIFCode returns ES EORI when there are multiple (one EU and one GB), even when the ES one was added after", "ESA12345678", address.GetEOROrNIFCode());

				customsCode2.OK_RN_NKCodeCountry = "AU";
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
				AssertEquals("GetEOROrNIFCode returns GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after", "XI123456789A", address.GetEOROrNIFCode());

				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEOROrNIFCode returns the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI", "AUA12345678", address.GetEOROrNIFCode());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			address = organization.Addresses.AddNew();
		}

		OrgAddress address;
	}
}
