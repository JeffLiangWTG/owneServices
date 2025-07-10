using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusSupplyChainActorReferenceValidation))]
sealed class CusSupplyChainActorReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCFR_Code_NS30003() => CombineAssertions(() =>
	{
		const string messageError = "[NS30003] Supply Chain Actor Reference not allowed in a Simplified Declaration.";

		var organization = CreateOrganizationWithCustomsCodes("MSB", OrgCusCode.SwissCodeTypes.BID);
		var entryInstruction = CusSupplyChainActor.Parent;
		var declaration = entryInstruction.JobDeclaration;
		CusSupplyChainActor.OwnerOrgPK = organization.PK;

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		CusSupplyChainActor.Validation.ValidateCFR_Code();
		AssertNoMessageError("Simplified Import", CusSupplyChainActor.CFR_CodeInfo, messageError);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		CusSupplyChainActor.Validation.ValidateCFR_Code();
		AssertNoMessageError("Simplified ExportDeclarationActivation", CusSupplyChainActor.CFR_CodeInfo, messageError);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		CusSupplyChainActor.Validation.ValidateCFR_Code();
		AssertHasMessageError("Simplified Export", CusSupplyChainActor.CFR_CodeInfo, messageError);

		entryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		CusSupplyChainActor.Validation.ValidateCFR_Code();
		AssertNoMessageError("Ordinary Export", CusSupplyChainActor.CFR_CodeInfo, messageError);
	});

	public void TestCheckCFR_Reference() => CombineAssertions(() =>
	{
		const string errorMessage = "[NP70177] Format of entered Identification (BP-ID/UID/DUNS) is incorrect; please enter in correct format (BP-ID: 10 numeric digits starting with '1', UID: 'CHE' + 9 numeric digits, DUNS: 9 numeric digits).";

		AssertId("1012345678", "2");
		AssertId("CHE012345678", "XXX");
		AssertId("012345678");

		void AssertId(string validId, string invalidPrefix = null)
		{
			var charCut = invalidPrefix?.Length == 1 ? 2 : 1;
			CusSupplyChainActor.CFR_Reference = validId.Substring(0, validId.Length - charCut);
			AssertHasMessageError("To short", CusSupplyChainActor.CFR_ReferenceInfo, errorMessage);
			CusSupplyChainActor.CFR_Reference = $"{validId}0";
			AssertHasMessageError("To long", CusSupplyChainActor.CFR_ReferenceInfo, errorMessage);
			CusSupplyChainActor.CFR_Reference = Replace(validId, validId.Length - 1, "X");
			AssertHasMessageError("Not numeric", CusSupplyChainActor.CFR_ReferenceInfo, errorMessage);
			if (!string.IsNullOrEmpty(invalidPrefix))
			{
				CusSupplyChainActor.CFR_Reference = Replace(validId, 0, invalidPrefix);
				AssertHasMessageError("Invalid prefix", CusSupplyChainActor.CFR_ReferenceInfo, errorMessage);
			}
			CusSupplyChainActor.CFR_Reference = validId;
			AssertNoMessageError("Valid", CusSupplyChainActor.CFR_ReferenceInfo, errorMessage);
		}

		string Replace(string input, int start, string replacement) => input.Substring(0, start) + replacement + input.Substring(start + replacement.Length);
	});

	public void TestCheckCFR_Reference_MultipleDUN() => CombineAssertions(() =>
	{
		const string messageError = "contains more than one DUNS Registration Number";

		var orgWithBIDAndMultipleDUN = CreateOrganizationWithCustomsCodes("BID and DUNs", OrgCusCode.SwissCodeTypes.BID, OrgCusCode.CodeTypes.DataUniversalNumberingSystem, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		var orgWithMultipleDUN = CreateOrganizationWithCustomsCodes("Multiple DUN", OrgCusCode.CodeTypes.DataUniversalNumberingSystem, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		var orgWithNoID = CreateOrganizationWithCustomsCodes("No ID");

		CusSupplyChainActor.OwnerOrgPK = orgWithMultipleDUN.PK;
		CusSupplyChainActor.Validation.ValidateCFR_Reference();
		AssertHasErrorContaining("Organization with multiple DUNs", CusSupplyChainActor.CFR_ReferenceInfo, messageError);

		CusSupplyChainActor.OwnerOrgPK = orgWithBIDAndMultipleDUN.PK;
		CusSupplyChainActor.Validation.ValidateCFR_Reference();
		AssertNoErrorContaining("Organization with BID and multiple DUNs", CusSupplyChainActor.CFR_ReferenceInfo, messageError);

		CusSupplyChainActor.OwnerOrgPK = orgWithNoID.PK;
		CusSupplyChainActor.Validation.ValidateCFR_Reference();
		AssertNoErrorContaining("Organization with no ID", CusSupplyChainActor.CFR_ReferenceInfo, messageError);
	});

	public void TestCheckOwnerOrgPKAEONotRequired()
	{
		var organization = CreateOrganizationWithCustomsCodes("MSB");
		CusSupplyChainActor.OwnerOrgPK = organization.PK;
		AssertEquals("No AEO", false, CusSupplyChainActor.OwnerOrgPKInfo.Notifications.Any(e => e.Message == "Organization is missing a Registration Number of type 'AEO'."));
	}

	public void TestGetCustomsCodeMissingErrorForSelectedOwner()
	{
		var organization = CreateOrganizationWithCustomsCodes("MSB");
		CusSupplyChainActor.OwnerOrgPK = organization.PK;
		AssertHasError("No ID", CusSupplyChainActor.CFR_ReferenceInfo, "The selected Owner (MSB) does not contain a BP-ID or UID or DUNS Registration Number.");
	}

	OrgHeader CreateOrganizationWithCustomsCodes(string code, params string[] customCodes)
	{
		var organization = Factory.New<OrgHeader>();
		organization.OH_Code = code;
		customCodes.ForEach(c => organization.CustomsCodes.AddNew(c, "012345678"));
		return organization;
	}

	CusSupplyChainActorReference CusSupplyChainActor => cusSupplyChainActorReference ??= GetCusSupplyChainActor(Factory);
	CusSupplyChainActorReference cusSupplyChainActorReference;

	CusSupplyChainActorReference GetCusSupplyChainActor(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusSupplyChainActor = entryInstruction.SupplyChainActors.AddNew();
		cusSupplyChainActor.CFR_Code = "MF";
		cusSupplyChainActor.CFR_Reference = "X";

		return cusSupplyChainActor;
	}
}
