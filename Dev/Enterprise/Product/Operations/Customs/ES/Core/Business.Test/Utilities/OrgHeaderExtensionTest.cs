using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class OrgHeaderExtensionTest : TestCaseWithFactory
	{
		public void TestGetNIFCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("GetNIFCode returns empty string when there is no nif in the orgHeader", ZString.Empty, organization.GetNIFCode());
				AssertEquals("GetNIFCode returns empty string when orgHeader is null", ZString.Empty, (null as OrgHeader).GetNIFCode());

				var customCode = organization.CustomsCodes.AddNew();
				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "12456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetNIFCode returns empty string when customsCode country is not ES", ZString.Empty, organization.GetNIFCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_RN_NKCodeCountry = "ES";

				AssertEquals("GetNIFCode returns empty string when customsCode country is ES but codeType is not NIF", ZString.Empty, organization.GetNIFCode());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				AssertEquals("GetNIFCode returns correct value when customsCode country is ES and codeType is NIF", "12456789A", organization.GetNIFCode());
			});
		}

		public void TestGetPASCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("GetPASCode returns empty string when there is no pas in the orgHeader", ZString.Empty, organization.GetPASCode());
				AssertEquals("GetPASCode returns empty string when orgHeader is null", ZString.Empty, (null as OrgHeader).GetPASCode());

				var customCode = organization.CustomsCodes.AddNew();
				customCode.OK_CodeType = "PAS";
				customCode.OK_CustomsRegNo = "ABC123456";
				customCode.OK_RN_NKCodeCountry = "IT";
				AssertEquals("GetPASCode returns correct value when codeType is PAS", "ABC123456", organization.GetPASCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				AssertEquals("GetPASCode returns empty string when codeType is not PAS", ZString.Empty, organization.GetPASCode());
			});
		}

		public void TestGetIDCode()
		{
			var customCode = organization.CustomsCodes.AddNew();

			CombineAssertions("When orgHeader is null or there are no CustomsCodes or customCode is PAS", () =>
			{
				AssertEquals("GetIDCode returns empty string when there is no nif or eori in the orgHeader", ZString.Empty, organization.GetIDCode());
				AssertEquals("GetIDCode returns empty string when orgHeader is null", ZString.Empty, (null as OrgHeader).GetIDCode());
			});

			CombineAssertions("When orgHeader.OH_Category is NAT => needed NIF", () =>
			{
				organization.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "12456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetIDCode returns empty string when customsCode country is not ES", ZString.Empty, organization.GetIDCode());

				customCode.OK_RN_NKCodeCountry = "ES";
				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetIDCode returns empty string when customsCode codeType is NIF and country is ES but the regNo is empty", ZString.Empty, organization.GetIDCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";

				AssertEquals("GetIDCode returns Country+Registration Number when customsCode country is ES but codeType is not NIF", "ES123456789A", organization.GetIDCode());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				AssertEquals("GetIDCode returns correct value when customsCode country is ES and codeType is NIF", "123456789A", organization.GetIDCode());
			});

			CombineAssertions("When orgHeader.OH_Category is not NAT => needed EORI", () =>
			{
				organization.OH_Category = OrgConstants.Category.Business;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetIDCode returns empty string when customsCode codeType is NIF and Country is not ES", ZString.Empty, organization.GetIDCode());

				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetIDCode returns empty string when customsCode codeType is NIF but the regNo is empty", ZString.Empty, organization.GetIDCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetIDCode returns correct value when customsCode codeType is EOR", "IT123456789A", organization.GetIDCode());

				customCode.OK_CustomsRegNo = "IT123456789A";
				AssertEquals("GetIDCode returns correct value when customsCode codeType is EOR and even if countryCode is in the regNo", "IT123456789A", organization.GetIDCode());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("GetIDCode returns Country+Registration Number value when customsCode codeType is NIF, countryCode is ES and the category is Business", "ES123456789A", organization.GetIDCode());

				customCode.OK_CustomsRegNo = "A56789123";
				organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				AssertEquals("GetIDCode returns EOR value when it has EOR and NIF Registration Numbers", "ESA12345678", organization.GetIDCode());
			});

			CombineAssertions("When EORI or NIF not declared or empty => PAS", () =>
			{
				organization.CustomsCodes.RemoveAndDeleteAll();
				customCode = organization.CustomsCodes.AddNew();

				organization.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("GetIDCode returns PAS when there is no NIF or EOR", "GB333333333", organization.GetIDCode());

				organization.OH_Category = OrgConstants.Category.Business;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetIDCode returns PAS when customsCode codeType is NIF and Country is not ES", "GB333333333", organization.GetIDCode());

				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetIDCode returns PAS when customsCode codeType is NIF but the regNo is empty", "GB333333333", organization.GetIDCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetIDCode returns correct value when customsCode codeType is EOR", "IT123456789A", organization.GetIDCode());

				customCode.OK_CustomsRegNo = "IT123456789A";
				AssertEquals("GetIDCode returns correct value when customsCode codeType is EOR and even if countryCode is in the regNo", "IT123456789A", organization.GetIDCode());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("GetIDCode returns Country+Registration Number value when customsCode codeType is NIF, countryCode is ES and the category is Business", "ES123456789A", organization.GetIDCode());

				customCode.OK_CustomsRegNo = "A56789123";
				organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				AssertEquals("GetIDCode returns EOR value when it has EOR and NIF Registration Numbers", "ESA12345678", organization.GetIDCode());
			});

			CombineAssertions("When there are multiple EORIs", () =>
			{
				organization.CustomsCodes.RemoveAndDeleteAll();
				customCode = organization.CustomsCodes.AddNew();

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "XI123456789A";
				customCode.OK_RN_NKCodeCountry = "GB";
				customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

				AssertEquals("GetIDCode returns GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI", "XI123456789A", organization.GetIDCode());

				var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("GetIDCode returns ES EORI when there are multiple (one EU and one GB), even when the ES one was added after", "ESA12345678", organization.GetIDCode());

				customsCode2.OK_RN_NKCodeCountry = "AU";
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
				AssertEquals("GetIDCode returns GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after", "XI123456789A", organization.GetIDCode());

				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetIDCode returns the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI", "AUA12345678", organization.GetIDCode());
			});
		}

		public void TestGetEOROrNIFCode()
		{
			var customCode = organization.CustomsCodes.AddNew();

			CombineAssertions("When orgHeader is null or there are no CustomsCodes", () =>
			{
				AssertEquals("GetEOROrNIFCode returns empty string when there is no nif or eori in the orgHeader", ZString.Empty, organization.GetEOROrNIFCode());
				AssertEquals("GetEOROrNIFCode returns empty string when orgHeader is null", ZString.Empty, (null as OrgHeader).GetEOROrNIFCode());
			});

			CombineAssertions("When orgHeader.OH_Category is NAT => needed NIF", () =>
			{
				organization.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "12456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEOROrNIFCode returns empty string when customsCode country is not ES", ZString.Empty, organization.GetEOROrNIFCode());

				customCode.OK_RN_NKCodeCountry = "ES";
				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEOROrNIFCode returns empty string when customsCode codeType is NIF and country is ES but the regNo is empty", ZString.Empty, organization.GetEOROrNIFCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";

				AssertEquals("GetEOROrNIFCode returns Country+Registration Number when customsCode country is ES but codeType is not NIF", "ES123456789A", organization.GetEOROrNIFCode());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				AssertEquals("GetEOROrNIFCode returns correct value when customsCode country is ES and codeType is NIF", "123456789A", organization.GetEOROrNIFCode());
			});

			CombineAssertions("When orgHeader.OH_Category is not NAT => needed EORI", () =>
			{
				organization.OH_Category = OrgConstants.Category.Business;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEOROrNIFCode returns empty string when customsCode codeType is NIF and Country is not ES", ZString.Empty, organization.GetEOROrNIFCode());

				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEOROrNIFCode returns empty string when customsCode codeType is NIF but the regNo is empty", ZString.Empty, organization.GetEOROrNIFCode());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEOROrNIFCode returns correct value when customsCode codeType is EOR", "IT123456789A", organization.GetEOROrNIFCode());

				customCode.OK_CustomsRegNo = "IT123456789A";
				AssertEquals("GetEOROrNIFCode returns correct value when customsCode codeType is EOR and even if countryCode is in the regNo", "IT123456789A", organization.GetEOROrNIFCode());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("GetEOROrNIFCode returns Country+Registration Number value when customsCode codeType is NIF, countryCode is ES and the category is Business", "ES123456789A", organization.GetEOROrNIFCode());

				customCode.OK_CustomsRegNo = "A56789123";
				organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				AssertEquals("GetEOROrNIFCode returns EOR value when it has EOR and NIF Registration Numbers", "ESA12345678", organization.GetEOROrNIFCode());
			});

			CombineAssertions("When there are multiple EORIs", () =>
			{
				organization.CustomsCodes.RemoveAndDeleteAll();
				customCode = organization.CustomsCodes.AddNew();

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "XI123456789A";
				customCode.OK_RN_NKCodeCountry = "GB";
				customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

				AssertEquals("GetEOROrNIFCode returns GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI", "XI123456789A", organization.GetEOROrNIFCode());

				var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("GetEOROrNIFCode returns ES EORI when there are multiple (one EU and one GB), even when the ES one was added after", "ESA12345678", organization.GetEOROrNIFCode());

				customsCode2.OK_RN_NKCodeCountry = "AU";
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
				AssertEquals("GetEOROrNIFCode returns GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after", "XI123456789A", organization.GetEOROrNIFCode());

				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEOROrNIFCode returns the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI", "AUA12345678", organization.GetEOROrNIFCode());
			});
		}

		public void TestGetEORIForLRNGeneration_BranchProxy()
		{
			organization = GlbBranch.CurrentBranch.OrgProxy;
			var lrnGeneratorObject = GetLRNGenerator();

			var customCode = organization.CustomsCodes.AddNew();

			CombineAssertions("When orgHeader is null or there are no CustomsCodes or customCode is PAS", () =>
			{
				AssertEquals("GetEORIForLRNGeneration returns empty string when there is no nif or eori in the orgHeader", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());
			});

			CombineAssertions("When orgHeader.OH_Category is NAT => needed NIF", () =>
			{
				organization.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "12456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEORIForLRNGeneration returns empty string when customsCode country is not ES", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_RN_NKCodeCountry = "ES";
				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEORIForLRNGeneration returns empty string when customsCode codeType is NIF and country is ES but the regNo is empty", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";

				AssertEquals("GetEORIForLRNGeneration returns Country+Registration Number when customsCode country is ES but codeType is not NIF", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode country is ES and codeType is NIF", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());
			});

			CombineAssertions("When orgHeader.OH_Category is not NAT => needed EORI", () =>
			{
				organization.OH_Category = OrgConstants.Category.Business;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEORIForLRNGeneration returns empty string when customsCode codeType is NIF and Country is not ES", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEORIForLRNGeneration returns empty string when customsCode codeType is NIF but the regNo is empty", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode codeType is EOR", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "IT124356789A";
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode codeType is EOR and even if countryCode is in the regNo", "124356789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "213456789A";
				customCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("GetEORIForLRNGeneration returns Country+Registration Number value when customsCode codeType is NIF, countryCode is ES and the category is Business", "213456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "A56789123";
				organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				AssertEquals("GetEORIForLRNGeneration returns EOR value when it has EOR and NIF Registration Numbers", "A12345678", lrnGeneratorObject.GetEORIForLRNGeneration());
			});

			CombineAssertions("When EORI or NIF not declared or empty => PAS", () =>
			{
				organization.CustomsCodes.RemoveAndDeleteAll();
				customCode = organization.CustomsCodes.AddNew();

				organization.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("GetEORIForLRNGeneration returns PAS when there is no NIF or EOR", "333333333", lrnGeneratorObject.GetEORIForLRNGeneration());

				organization.OH_Category = OrgConstants.Category.Business;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEORIForLRNGeneration returns PAS when customsCode codeType is NIF and Country is not ES", "333333333", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEORIForLRNGeneration returns PAS when customsCode codeType is NIF but the regNo is empty", "333333333", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode codeType is EOR", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "IT124356789A";
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode codeType is EOR and even if countryCode is in the regNo", "124356789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("GetEORIForLRNGeneration returns Country+Registration Number value when customsCode codeType is NIF, countryCode is ES and the category is Business", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "A56789123";
				organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				AssertEquals("GetEORIForLRNGeneration returns EOR value when it has EOR and NIF Registration Numbers", "A12345678", lrnGeneratorObject.GetEORIForLRNGeneration());
			});

			CombineAssertions("When there are multiple EORIs", () =>
			{
				organization.CustomsCodes.RemoveAndDeleteAll();
				customCode = organization.CustomsCodes.AddNew();

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "XI123456789A";
				customCode.OK_RN_NKCodeCountry = "GB";
				customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

				AssertEquals("GetEORIForLRNGeneration returns GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI", "XI123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("GetEORIForLRNGeneration returns ES EORI when there are multiple (one EU and one GB), even when the ES one was added after", "A12345678", lrnGeneratorObject.GetEORIForLRNGeneration());

				customsCode2.OK_RN_NKCodeCountry = "AU";
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
				AssertEquals("GetEORIForLRNGeneration returns GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after", "XI123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEORIForLRNGeneration returns the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI", "A12345678", lrnGeneratorObject.GetEORIForLRNGeneration());

				customsCode2.OK_CustomsRegNo = "BE1234567890123456";
				AssertEquals("GetEORIForLRNGeneration returns empty when the EORI found is longer than 15", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());
			});
		}

		public void TestGetEORIForLRNGeneration_CompanyProxy()
		{
			organization = GlbBranch.CurrentBranch.Company.OrgProxy;
			var lrnGeneratorObject = GetLRNGenerator();

			var customCode = organization.CustomsCodes.AddNew();

			CombineAssertions("When orgHeader is null or there are no CustomsCodes or customCode is PAS", () =>
			{
				AssertEquals("GetEORIForLRNGeneration returns empty string when there is no nif or eori in the orgHeader", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());
			});

			CombineAssertions("When orgHeader.OH_Category is NAT => needed NIF", () =>
			{
				organization.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "12456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEORIForLRNGeneration returns empty string when customsCode country is not ES", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_RN_NKCodeCountry = "ES";
				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEORIForLRNGeneration returns empty string when customsCode codeType is NIF and country is ES but the regNo is empty", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";

				AssertEquals("GetEORIForLRNGeneration returns Country+Registration Number when customsCode country is ES but codeType is not NIF", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode country is ES and codeType is NIF", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());
			});

			CombineAssertions("When orgHeader.OH_Category is not NAT => needed EORI", () =>
			{
				organization.OH_Category = OrgConstants.Category.Business;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEORIForLRNGeneration returns empty string when customsCode codeType is NIF and Country is not ES", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEORIForLRNGeneration returns empty string when customsCode codeType is NIF but the regNo is empty", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode codeType is EOR", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "IT124356789A";
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode codeType is EOR and even if countryCode is in the regNo", "124356789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "213456789A";
				customCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("GetEORIForLRNGeneration returns Country+Registration Number value when customsCode codeType is NIF, countryCode is ES and the category is Business", "213456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "A56789123";
				organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				AssertEquals("GetEORIForLRNGeneration returns EOR value when it has EOR and NIF Registration Numbers", "A12345678", lrnGeneratorObject.GetEORIForLRNGeneration());
			});

			CombineAssertions("When EORI or NIF not declared or empty => PAS", () =>
			{
				organization.CustomsCodes.RemoveAndDeleteAll();
				customCode = organization.CustomsCodes.AddNew();

				organization.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("GetEORIForLRNGeneration returns PAS when there is no NIF or EOR", "333333333", lrnGeneratorObject.GetEORIForLRNGeneration());

				organization.OH_Category = OrgConstants.Category.Business;

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "IT";

				AssertEquals("GetEORIForLRNGeneration returns PAS when customsCode codeType is NIF and Country is not ES", "333333333", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = ZString.Empty;
				AssertEquals("GetEORIForLRNGeneration returns PAS when customsCode codeType is NIF but the regNo is empty", "333333333", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode codeType is EOR", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "IT124356789A";
				AssertEquals("GetEORIForLRNGeneration returns correct value when customsCode codeType is EOR and even if countryCode is in the regNo", "124356789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
				customCode.OK_CustomsRegNo = "123456789A";
				customCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("GetEORIForLRNGeneration returns Country+Registration Number value when customsCode codeType is NIF, countryCode is ES and the category is Business", "123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "A56789123";
				organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				AssertEquals("GetEORIForLRNGeneration returns EOR value when it has EOR and NIF Registration Numbers", "A12345678", lrnGeneratorObject.GetEORIForLRNGeneration());
			});

			CombineAssertions("When there are multiple EORIs", () =>
			{
				organization.CustomsCodes.RemoveAndDeleteAll();
				customCode = organization.CustomsCodes.AddNew();

				customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				customCode.OK_CustomsRegNo = "XI123456789A";
				customCode.OK_RN_NKCodeCountry = "GB";
				customCode.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

				AssertEquals("GetEORIForLRNGeneration returns GB EORI when customsCode codeType is EOR, Country is GB and code starts with XI", "XI123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				var customsCode2 = organization.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "A12345678", "ES");
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("GetEORIForLRNGeneration returns ES EORI when there are multiple (one EU and one GB), even when the ES one was added after", "A12345678", lrnGeneratorObject.GetEORIForLRNGeneration());

				customsCode2.OK_RN_NKCodeCountry = "AU";
				customsCode2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
				AssertEquals("GetEORIForLRNGeneration returns GB EORI when there are multiple (one non EU and one GB) and the GB one starts with XI, even when the GB one was added after", "XI123456789A", lrnGeneratorObject.GetEORIForLRNGeneration());

				customCode.OK_CustomsRegNo = "123456789A";
				AssertEquals("GetEORIForLRNGeneration returns the first EORI found when there are no EU EORIs and the only GB one's code doesn't start with XI", "A12345678", lrnGeneratorObject.GetEORIForLRNGeneration());

				customsCode2.OK_CustomsRegNo = "BE1234567890123456";
				AssertEquals("GetEORIForLRNGeneration returns empty when the EORI found is longer than 15", ZString.Empty, lrnGeneratorObject.GetEORIForLRNGeneration());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			organization = Factory.NewWithValidTestData<OrgHeader>();
		}

		OrgHeader organization;

		ILRNGenerator GetLRNGenerator()
		{
			var lrnGenerator = new Mock<ILRNGenerator>();
			lrnGenerator.Setup(x => x.Branch).Returns(GlbBranch.CurrentBranch);
			lrnGenerator.Setup(x => x.Factory).Returns(Factory);
			var numberFountain = Env.NumberFountains.EULocalReferenceNumber(GlbCompany.CurrentCompany.PK.ToGuid());
			lrnGenerator.Setup(x => x.LrnNumberFountain).Returns(numberFountain);
			return lrnGenerator.Object;
		}
	}
}
