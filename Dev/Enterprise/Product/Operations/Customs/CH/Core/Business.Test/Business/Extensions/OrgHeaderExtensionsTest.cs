using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class OrgHeaderExtensionsTest : TestCaseWithFactory
{
	public void TestGetUIDNumber()
	{
		var codeType = OrgCusCode.SwissCodeTypes.UID;
		var orgHeader = Factory.New<OrgHeader>();
		AssertEquals(ZString.Empty, orgHeader.GetUIDNumber());
		AssertEquals(ZString.Empty, ((OrgHeader)null).GetUIDNumber());

		var customsCode = orgHeader.CustomsCodes.AddNew(codeType, "E-123.456.789");
		AssertEquals("CHE123456789", orgHeader.GetUIDNumber());
		customsCode.OK_CustomsRegNo = "CHE-123.456.789";
		AssertEquals("CHE123456789", orgHeader.GetUIDNumber());
		customsCode.OK_CustomsRegNo = "CHE123456789";
		AssertEquals("CHE123456789", orgHeader.GetUIDNumber());
		customsCode.OK_CustomsRegNo = "E123456789";
		AssertEquals("CHE123456789", orgHeader.GetUIDNumber());
	}

	public void TestGetVATNumber()
	{
		var codeType = OrgCusCode.CodeTypes.VATCode;
		var orgHeader = Factory.New<OrgHeader>();
		AssertEquals(ZString.Empty, orgHeader.GetVATNumber());
		AssertEquals(ZString.Empty, ((OrgHeader)null).GetVATNumber());

		var customsCode = orgHeader.CustomsCodes.AddNew(codeType, "E-123.456.789");
		AssertEquals("CHE123456789", orgHeader.GetVATNumber());
		customsCode.OK_CustomsRegNo = "CHE-123.456.789";
		AssertEquals("CHE123456789", orgHeader.GetVATNumber());
		customsCode.OK_CustomsRegNo = "CHE123456789";
		AssertEquals("CHE123456789", orgHeader.GetVATNumber());
		customsCode.OK_CustomsRegNo = "E123456789";
		AssertEquals("CHE123456789", orgHeader.GetVATNumber());
	}

	public void TestGetAEONumber()
	{
		var codeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
		var orgHeader = Factory.New<OrgHeader>();
		AssertEquals(ZString.Empty, orgHeader.GetAEONumber());
		AssertEquals(ZString.Empty, ((OrgHeader)null).GetAEONumber());

		var customsCode = orgHeader.CustomsCodes.AddNew(codeType, "AEO123");
		AssertEquals("AEO123", orgHeader.GetAEONumber());
	}

	public void TestGetBIDNumber()
	{
		const string codeType = OrgCusCode.SwissCodeTypes.BID;
		var orgHeader = Factory.New<OrgHeader>();

		AssertEquals("Null OrgHeader", ZString.Empty, ((OrgHeader)null).GetBIDNumber());
		AssertEquals("No BID", ZString.Empty, orgHeader.GetBIDNumber());

		orgHeader.CustomsCodes.AddNew(codeType, "UK123", Core.Constants.CountryCodes.UnitedKingdom);
		AssertEquals("Other country", ZString.Empty, orgHeader.GetBIDNumber());

		orgHeader.CustomsCodes.AddNew(codeType, "CH123");
		AssertEquals("Swiss BID", "CH123", orgHeader.GetBIDNumber());
	}

	public void TestGetCustomsRegNo()
	{
		var codeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
		var orgHeader = Factory.New<OrgHeader>();
		var address1 = orgHeader.Addresses.AddNew();
		var address2 = orgHeader.Addresses.AddNew();
		orgHeader.CustomsCodes.AddNew(codeType, "AUDUN1", Core.Constants.CountryCodes.Australia);
		orgHeader.CustomsCodes.AddNew(codeType, "AUDUN2", Core.Constants.CountryCodes.Australia).OK_OA_PremisesAddress = address1.PK;

		AssertEquals(ZString.Empty, orgHeader.GetCHCustomsRegNo(codeType, false, ZGuid.Empty));
		AssertEquals(ZString.Empty, orgHeader.GetCHCustomsRegNo(codeType, false, address1.PK));
		AssertEquals("AUDUN1", orgHeader.GetCHCustomsRegNo(codeType, true, ZGuid.Empty));
		AssertEquals("AUDUN2", orgHeader.GetCHCustomsRegNo(codeType, true, address1.PK));
		AssertEquals("AUDUN1", orgHeader.GetCHCustomsRegNo(codeType, true, address2.PK));

		orgHeader.CustomsCodes.AddNew(codeType, "CHDUN1", Core.Constants.CountryCodes.Switzerland);
		orgHeader.CustomsCodes.AddNew(codeType, "CHDUN2", Core.Constants.CountryCodes.Switzerland).OK_OA_PremisesAddress = address1.PK;

		AssertEquals("CHDUN1", orgHeader.GetCHCustomsRegNo(codeType, false, ZGuid.Empty));
		AssertEquals("CHDUN2", orgHeader.GetCHCustomsRegNo(codeType, false, address1.PK));
		AssertEquals("CHDUN1", orgHeader.GetCHCustomsRegNo(codeType, false, address2.PK));
		AssertEquals("CHDUN1", orgHeader.GetCHCustomsRegNo(codeType, true, ZGuid.Empty));
		AssertEquals("CHDUN2", orgHeader.GetCHCustomsRegNo(codeType, true, address1.PK));
		AssertEquals("CHDUN1", orgHeader.GetCHCustomsRegNo(codeType, true, address2.PK));
	}

	public void TestGetCHCustomsRegNoList() => CombineAssertions(() =>
	{
		var codeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
		var orgHeader = Factory.New<OrgHeader>();
		var address1 = orgHeader.Addresses.AddNew();
		var address2 = orgHeader.Addresses.AddNew();

		orgHeader.CustomsCodes.AddNew(codeType, "CHDUN1A1", Core.Constants.CountryCodes.Switzerland).OK_OA_PremisesAddress = address1.PK;
		orgHeader.CustomsCodes.AddNew(codeType, "CHDUN2A2", Core.Constants.CountryCodes.Switzerland).OK_OA_PremisesAddress = address2.PK;
		orgHeader.CustomsCodes.AddNew(codeType, "CHDUN3", Core.Constants.CountryCodes.Switzerland);
		orgHeader.CustomsCodes.AddNew(codeType, "AUDUN4", Core.Constants.CountryCodes.Australia);

		var result = orgHeader.GetCHCustomsRegNoList(codeType);

		AssertEquals("Result Count", 4, result.Length);
		Assert("Found CHDUN1A1", result.Any(o => o.OK_CustomsRegNo == "CHDUN1A1"));
		Assert("Found CHDUN2A2", result.Any(o => o.OK_CustomsRegNo == "CHDUN2A2"));
		Assert("Found CHDUN3", result.Any(o => o.OK_CustomsRegNo == "CHDUN3"));
		Assert("Found AUDUN4", result.Any(o => o.OK_CustomsRegNo == "AUDUN4"));
	});

	public void TestGetIdentificationNumberForCH()
	{
		var principal = Factory.New<JobDocAddress>();
		principal.E2_AddressOverride = true;
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.MainAddress;
		principal.E2_OA_Address = orgAddress.PK;

		orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;

		AssertEquals(ZString.Empty, orgHeader.GetIdentificationNumberForCH());
		AssertEquals(ZString.Empty, ((OrgHeader)null).GetIdentificationNumberForCH());

		orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123", Core.Constants.CountryCodes.Switzerland);
		AssertEquals("123", orgHeader.GetIdentificationNumberForCH());

		orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "321", Core.Constants.CountryCodes.Switzerland);
		AssertNullOrEmpty(orgHeader.GetIdentificationNumberForCH(false));
		AssertEquals("123", orgHeader.GetIdentificationNumberForCH());

		orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.UID, "456", Core.Constants.CountryCodes.Switzerland);
		AssertEquals("456", orgHeader.GetIdentificationNumberForCH());

		orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "789", Core.Constants.CountryCodes.Switzerland);
		AssertEquals("789", orgHeader.GetIdentificationNumberForCH());
	}

	public void TestGetIdentificationNumber_EU() => AssertTestGetIdentificationNumber(Core.Constants.CountryCodes.Germany);
	public void TestGetIdentificationNumber_GB() => AssertTestGetIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom);

	void AssertTestGetIdentificationNumber(ZString countryCode)
	{
		var principal = Factory.New<JobDocAddress>();
		principal.E2_AddressOverride = true;
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.MainAddress;
		principal.E2_OA_Address = orgAddress.PK;

		orgAddress.OA_RN_NKCountryCode = countryCode;

		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, orgHeader.GetIdentificationNumberForEU());
			AssertEquals(ZString.Empty, ((OrgHeader)null).GetIdentificationNumberForEU());

			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123", countryCode);
			AssertEquals("123", orgHeader.GetIdentificationNumberForEU());

			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "456", countryCode);
			AssertEquals("456", orgHeader.GetIdentificationNumberForEU());
		});
	}

	public void TestHasValidNumber()
	{
		var orgHeader = Factory.New<OrgHeader>();

		void assertResult(string orgCountry, string codeCountry, string codeType, string number, bool expectedResult)
		{
			orgHeader.OH_RL_NKClosestPort = orgCountry + "XXX";
			orgHeader.CustomsCodes.RemoveAll();
			if (number != null)
			{
				orgHeader.CustomsCodes.AddNew(codeType, number, codeCountry);
			}

			AssertEquals($"org={orgCountry} {codeCountry}-{codeType}={number}", expectedResult, orgHeader.HasValidNumber(codeType));
		}

		CombineAssertions(() =>
		{
			assertResult(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID, null, false);
			assertResult(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID, "XXX", false);
			assertResult(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID, "CHE105908411", false);
			assertResult(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID, "CHE105908410", true);
			assertResult(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID, null, false);
			assertResult(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID, "XXX", false);
			assertResult(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID, "CHE105908411", false);
			assertResult(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Switzerland, OrgCusCode.SwissCodeTypes.UID, "CHE105908410", true);
			assertResult(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland, OrgCusCode.CodeTypes.VATCode, null, false);
			assertResult(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland, OrgCusCode.CodeTypes.VATCode, "XXX", false);
			assertResult(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland, OrgCusCode.CodeTypes.VATCode, "CHE105908411", false);
			assertResult(Core.Constants.CountryCodes.Switzerland, Core.Constants.CountryCodes.Switzerland, OrgCusCode.CodeTypes.VATCode, "CHE105908410", true);
			assertResult(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Liechtenstein, OrgCusCode.CodeTypes.VATCode, null, false);
			assertResult(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Liechtenstein, OrgCusCode.CodeTypes.VATCode, "XXX", false);
			assertResult(Core.Constants.CountryCodes.Liechtenstein, Core.Constants.CountryCodes.Liechtenstein, OrgCusCode.CodeTypes.VATCode, "12345", true);
		});
	}

	public void TestIsPrivatePerson()
	{
		var orgHeader = Factory.New<OrgHeader>();

		AssertIsPrivatePerson(true, OrgConstants.Category.NaturalPersonIndividual);
		AssertIsPrivatePerson(false, OrgConstants.Category.Business);
		AssertIsPrivatePerson(false, OrgConstants.Category.NonGovernmentOrganisation);
		AssertIsPrivatePerson(false, OrgConstants.Category.Government);

		void AssertIsPrivatePerson(bool expecteddPrivatePerson, string organisationCategory)
		{
			orgHeader.OH_Category = organisationCategory;
			AssertEquals($"OH_Category={organisationCategory}", expecteddPrivatePerson, orgHeader.IsPrivatePerson());
		}
	}
}
