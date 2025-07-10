using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOK_CustomsRegNo_AEO() => CombineAssertions(() =>
	{
		const string message = "AE AEO number should consist of 7 digits.";

		var customsCode = GetOrgCusCode(_orgHeader, OrgCusCode.UnitedArabEmiratesCodeTypes.AEO);

		customsCode.OK_CustomsRegNo = "1234567890";
		AssertHasWarningContaining("10 digits", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "12345";
		AssertHasWarningContaining("5 digits", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "1234ABC";
		AssertHasWarningContaining("7 chars", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "1234567";
		AssertNoWarningContaining("7 digits", customsCode.OK_CustomsRegNoInfo, message);
	});

	public void TestCheckOK_CustomsRegNo_CBL() => CombineAssertions(() =>
	{
		var customsCode = GetOrgCusCode(_orgHeader, OrgCusCode.UnitedArabEmiratesCodeTypes.CBLSNumber);
		ValidationTestHelper.AssertErrorIfNotEntered(customsCode.OK_CustomsRegNoInfo);

		var message = "CBLS Number can only be a maximum of 7 characters.";
		customsCode.OK_CustomsRegNo = "1234567";
		AssertNoErrorContaining(customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "123&123";
		AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "12345678";
		AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, message);
	});

	public void TestCheckOK_CustomsRegNo_MPC() => CombineAssertions(() =>
	{
		var customsCode = GetOrgCusCode(_orgHeader, OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber);
		ValidationTestHelper.AssertErrorIfNotEntered(customsCode.OK_CustomsRegNoInfo);

		var message = "MPCI Party Id can only be a maximum of 7 characters.";
		customsCode.OK_CustomsRegNo = "1234567";
		AssertNoErrorContaining(customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "123&123";
		AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "12345678";
		AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, message);
	});

	public void TestCheckOK_CustomsRegNo_CCC() => CombineAssertions(() =>
	{
		const string message = "CCC – Customs Carrier Code must be the Carrier's 3-character SMDG code.";

		var customsCode = GetOrgCusCode(_orgHeader, OrgCusCode.CodeTypes.CarrierCode);
		customsCode.OK_CustomsRegNo = "AB";
		AssertHasErrorContaining("2 chars", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "ABC";
		AssertNoErrorContaining("3 chars", customsCode.OK_CustomsRegNoInfo, message);
	});

	public void TestCheckOK_CustomsRegNo_IDO() => CombineAssertions(() =>
	{
		const string message = "ID Number must be 15 digits.";

		var customsCode = GetOrgCusCode(_orgHeader, OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber);

		customsCode.OK_CustomsRegNo = "1234567890";
		AssertHasMessageErrorContaining("10 digits", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "1234567890123456";
		AssertHasMessageErrorContaining("16 digits", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "1234567890ABCDE";
		AssertHasMessageErrorContaining("15 chars", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "123456789012345";
		AssertNoMessageErrorContaining("15 digits", customsCode.OK_CustomsRegNoInfo, message);
	});

	public void TestCheckOK_CustomsRegNo_GTX() => CombineAssertions(() =>
	{
		const string message = "GTX Number must be 15 digits.";

		var customsCode = GetOrgCusCode(_orgHeader, OrgCusCode.CodeTypes.TaxFileCode);

		customsCode.OK_CustomsRegNo = "1234567890";
		AssertHasMessageErrorContaining("10 digits", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "1234567890123456";
		AssertHasMessageErrorContaining("16 digits", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "1234567890ABCDE";
		AssertHasMessageErrorContaining("15 chars", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "123456789012345";
		AssertNoMessageErrorContaining("15 digits", customsCode.OK_CustomsRegNoInfo, message);
	});

	public void TestCheckOK_CustomsRegNo_GCR() => CombineAssertions(() =>
	{
		const string message = "GCR Number must be 15 digits.";

		var customsCode = GetOrgCusCode(_orgHeader, OrgCusCode.CodeTypes.CorporationCode);

		customsCode.OK_CustomsRegNo = "1234567890";
		AssertHasMessageErrorContaining("10 digits", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "1234567890123456";
		AssertHasMessageErrorContaining("16 digits", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "1234567890ABCDE";
		AssertHasMessageErrorContaining("15 chars", customsCode.OK_CustomsRegNoInfo, message);

		customsCode.OK_CustomsRegNo = "123456789012345";
		AssertNoMessageErrorContaining("15 digits", customsCode.OK_CustomsRegNoInfo, message);
	});

	public void TestCheckOK_CodeType_MutualExclusivePartyIdentifiers() => CombineAssertions(() =>
	{
		const string message =
			"Only one of the following Types are allowed: 'PAS – Passport', 'IDO - ID Number', 'GTX – Government Tax File Code' or 'GCR – Government Corporation Code'.";

		AssertMutualExclusivePartyIdentifiers(false, ["PAS"]);
		AssertMutualExclusivePartyIdentifiers(false, ["IDO"]);
		AssertMutualExclusivePartyIdentifiers(false, ["GTX"]);
		AssertMutualExclusivePartyIdentifiers(false, ["GCR"]);

		AssertMutualExclusivePartyIdentifiers(true, ["PAS", "IDO"]);
		AssertMutualExclusivePartyIdentifiers(true, ["IDO", "GTX"]);
		AssertMutualExclusivePartyIdentifiers(true, ["GTX", "GCR"]);
		AssertMutualExclusivePartyIdentifiers(true, ["PAS", "IDO", "GTX"]);
		AssertMutualExclusivePartyIdentifiers(true, ["PAS", "IDO", "GCR"]);
		AssertMutualExclusivePartyIdentifiers(true, ["PAS", "IDO", "GTX", "GCR"]);

		AssertMutualExclusivePartyIdentifiers(false, ["PAS", "PAS"]);

		var ido = GetOrgCusCode(_orgHeader, OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber);
		var gtx = GetOrgCusCode(_orgHeader, OrgCusCode.CodeTypes.TaxFileCode);
		ido.OK_RN_NKCodeCountry = Constants.CountryCodes.SouthAfrica;
		ido.Validation.ValidateOK_CodeType();
		AssertNoErrorContaining("AE: GTX, ZA: IDO", ido.OK_CodeTypeInfo, message);
		gtx.Validation.ValidateOK_CodeType();
		AssertNoErrorContaining("AE: GTX, ZA: IDO", gtx.OK_CodeTypeInfo, message);
		return;

		void AssertMutualExclusivePartyIdentifiers(bool hasMutualExclusiveError, string[] codeTypes)
		{
			var orgHeader = Factory.New<OrgHeader>();
			var codes = codeTypes.Select(codeType => GetOrgCusCode(orgHeader, codeType)).ToList();
			foreach (var code in codes)
			{
				code.Validation.ValidateOK_CodeType();
				if (hasMutualExclusiveError)
				{
					AssertHasErrorContaining("AE Codes: " + string.Join(", ", codeTypes), code.OK_CodeTypeInfo, message);
				}
				else
				{
					AssertNoErrorContaining("AE Codes: " + string.Join(", ", codeTypes), code.OK_CodeTypeInfo, message);
				}
			}
		}
	});

	OrgCusCode GetOrgCusCode(OrgHeader orgHeader, string codeType)
	{
		var customsCode = orgHeader.CustomsCodes.AddNew();
		customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		customsCode.OK_CodeType = codeType;
		return customsCode;
	}

	protected override void SetUp()
	{
		base.SetUp();
		_orgHeader = Factory.New<OrgHeader>();
	}

	OrgHeader _orgHeader;
}
