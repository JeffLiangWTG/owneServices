using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusSupplyChainActorReferenceValidation))]
sealed class CusSupplyChainActorReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCFR_Reference() => CombineAssertions(() =>
	{
		const string errorMessage = "[NP70177] Format of entered Identification (BP-ID/UID/DUNS) is incorrect; please enter in correct format (BP-ID: 10 numeric digits starting with '1', UID: 'CHE' + 9 numeric digits, DUNS: 9 numeric digits).";

		AssertId("1012345678", "2");
		AssertId("CHE012345678", "XXX");
		AssertId("012345678");

		void AssertId(string validId, string invalidPrefix = null)
		{
			var charCut = invalidPrefix?.Length == 1 ? 2 : 1;
			CusReference.CFR_Reference = validId.Substring(0, validId.Length - charCut);
			AssertHasMessageError("To short", CusReference.CFR_ReferenceInfo, errorMessage);
			CusReference.CFR_Reference = $"{validId}0";
			AssertHasMessageError("To long", CusReference.CFR_ReferenceInfo, errorMessage);
			CusReference.CFR_Reference = Replace(validId, validId.Length - 1, "X");
			AssertHasMessageError("Not numeric", CusReference.CFR_ReferenceInfo, errorMessage);
			if (!string.IsNullOrEmpty(invalidPrefix))
			{
				CusReference.CFR_Reference = Replace(validId, 0, invalidPrefix);
				AssertHasMessageError("Invalid prefix", CusReference.CFR_ReferenceInfo, errorMessage);
			}
			CusReference.CFR_Reference = validId;
			AssertNoMessageError("Valid", CusReference.CFR_ReferenceInfo, errorMessage);
		}

		string Replace(string input, int start, string replacement) => input.Substring(0, start) + replacement + input.Substring(start + replacement.Length);
	});

	public void TestCheckCFR_Reference_MultipleDUN() => CombineAssertions(() =>
	{
		const string messageError = "contains more than one DUNS Registration Number";

		var orgWithBIDAndMultipleDUN = CreateOrganizationWithCustomsCodes("BID and DUNs", OrgCusCode.SwissCodeTypes.BID, OrgCusCode.CodeTypes.DataUniversalNumberingSystem, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		var orgWithMultipleDUN = CreateOrganizationWithCustomsCodes("Multiple DUN", OrgCusCode.CodeTypes.DataUniversalNumberingSystem, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		var orgWithNoID = CreateOrganizationWithCustomsCodes("No ID");

		CusReference.OwnerOrgPK = orgWithMultipleDUN.PK;
		CusReference.Validation.ValidateCFR_Reference();
		AssertHasErrorContaining("Organization with multiple DUNs", CusReference.CFR_ReferenceInfo, messageError);

		CusReference.OwnerOrgPK = orgWithBIDAndMultipleDUN.PK;
		CusReference.Validation.ValidateCFR_Reference();
		AssertNoErrorContaining("Organization with BID and multiple DUNs", CusReference.CFR_ReferenceInfo, messageError);

		CusReference.OwnerOrgPK = orgWithNoID.PK;
		CusReference.Validation.ValidateCFR_Reference();
		AssertNoErrorContaining("Organization with no ID", CusReference.CFR_ReferenceInfo, messageError);
	});

	public void TestCheckOwnerOrgPKAEONotRequired()
	{
		var organization = CreateOrganizationWithCustomsCodes("MSB");
		CusReference.OwnerOrgPK = organization.PK;
		AssertEquals("No AEO", false, CusReference.OwnerOrgPKInfo.Notifications.Any(e => e.Message == "Organization is missing a Registration Number of type 'AEO'."));
	}

	public void TestGetCustomsCodeMissingErrorForSelectedOwner()
	{
		var organization = CreateOrganizationWithCustomsCodes("MSB");
		CusReference.OwnerOrgPK = organization.PK;
		AssertHasError("No ID", CusReference.CFR_ReferenceInfo, "The selected Owner (MSB) does not contain a BP-ID or UID or DUNS Registration Number.");
	}

	OrgHeader CreateOrganizationWithCustomsCodes(string code, params string[] customCodes)
	{
		var organization = Factory.New<OrgHeader>();
		organization.OH_Code = code;
		customCodes.ForEach(c => organization.CustomsCodes.AddNew(c, c));
		return organization;
	}

	CusSupplyChainActorReference CusReference => cusReference ?? (cusReference = Factory.New<CusSupplyChainActorReference>());
	CusSupplyChainActorReference cusReference;
}
