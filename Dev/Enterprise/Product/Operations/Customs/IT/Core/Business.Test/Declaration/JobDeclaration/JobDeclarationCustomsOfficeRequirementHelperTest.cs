using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
sealed class JobDeclarationCustomsOfficeRequirementHelperTest : EU.Business.Declaration.Testing.JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
{
	public override void TestMainOffice_Import()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertCustomsOfficeRequirementEquals("Import", new CustomsOfficeRequirement("", isMandatory: true, isLocalCountryOnly: true, "Office of Presentation"), officeHelper.MainOffice);
	}

	public override void TestMainOffice_Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertCustomsOfficeRequirementEquals("Export", new CustomsOfficeRequirement("", isMandatory: true, isLocalCountryOnly: true, "Office of Presentation"), officeHelper.MainOffice);
	}

	public override void TestMainOffice_Miscellaneous()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertCustomsOfficeRequirementEquals("Miscellaneous", new CustomsOfficeRequirement("", isMandatory: true, isLocalCountryOnly: false), officeHelper.MainOffice);
	}

	public override void TestOtherRequirements_Import()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

		AssertEquals("Other Requirements Import: Office of Entry Not Available", true, officeHelper.OtherRequirements.All(x => x.OfficeRole != EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent));

		AssertCustomsOfficeRequirementEquals("Other Requirements Import: Supervising Customs Office",
			ExpectedSupervisingCustomsOfficeReq, officeHelper.OtherRequirements.Single(x => x.OfficeRole == "SCO"));

		AssertCustomsOfficeRequirementEquals("Other Requirements Import: Presentation for Centralized Clearance",
			ExpectedCustomOfficeOfPresentationReq,
			officeHelper.OtherRequirements.SingleOrDefault(r => r.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfPresentation));
	}

	public override void TestOtherRequirements_Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertCustomsOfficeRequirementEquals("Other Requirements Export", new CustomsOfficeRequirement("EXT", isMandatory: true, isLocalCountryOnly: false, "Office of Exit"), officeHelper.OtherRequirements.Single());
	}

	public void TestOtherRequirements_Export_UCC6()
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";

		var otherRequirements = officeHelper.OtherRequirements;
		AssertEquals("Count of OtherRequirements", 3, otherRequirements.Count());

		var actualPresentationRequirement = otherRequirements.Single(req => req.OfficeRole == "PRE");
		var expectedPresentationRequirement = new CustomsOfficeRequirement("PRE", isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false, "Office of Presentation for Centralized Clearance")
		{
			OfficeRolesForLookup = new ZString[] { "CCL" }
		};
		AssertCustomsOfficeRequirementEquals("PRE requirement", expectedPresentationRequirement, actualPresentationRequirement);

		var actualSupervisingRequirement = otherRequirements.Single(req => req.OfficeRole == "SVO");
		var expectedSupervisingRequirement = new CustomsOfficeRequirement("SVO", isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false)
		{
			OfficeRolesForLookup = System.Array.Empty<ZString>()
		};
		AssertCustomsOfficeRequirementEquals("SVO requirement", expectedSupervisingRequirement, actualSupervisingRequirement);
	}

	public override void TestOtherRequirements_Miscellaneous()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals(false, officeHelper.OtherRequirements.Any());
	}

	public void TestOfficeOfPresentationForCentClearance_PropertyConfiguration()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var officeOfPresentation = officeHelper
			.OtherRequirements
			.SingleOrDefault(r => r.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfPresentation);

		AssertNotNull("PRE - Office of Presentation for Centralized Clearance", officeOfPresentation);
		AssertEquals(nameof(CustomsOfficeRequirement.IsMandatory), false, officeOfPresentation.IsMandatory);
		AssertEquals(nameof(CustomsOfficeRequirement.IsLocalCountryOnly), false, officeOfPresentation.IsLocalCountryOnly);
		AssertEquals(nameof(CustomsOfficeRequirement.IsForeignCountryOnly), false, officeOfPresentation.IsForeignCountryOnly);
	}

	public void TestCacheKeyCombinationUCC6()
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		AssertEquals("CacheKey", "JobDeclarationCustomsOfficeRequirementHelper,IT,EXP,True", officeHelper.CacheKeyCombination);

		declaration.MessageVersion = "TXT";
		AssertEquals("CacheKey", "JobDeclarationCustomsOfficeRequirementHelper,IT,EXP,False", officeHelper.CacheKeyCombination);

		declaration.JE_MessageType = "IMP";
		AssertEquals("CacheKey", "JobDeclarationCustomsOfficeRequirementHelper,IT,IMP,True", officeHelper.CacheKeyCombination);
	}

	protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override string SetupDeclarationForCacheKey()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		return "JobDeclarationCustomsOfficeRequirementHelper,IT,EXP,False";
	}

	CustomsOfficeRequirement ExpectedCustomOfficeOfPresentationReq => new CustomsOfficeRequirement(
		EuOfficeCodesTypes.Codes.OfficeOfPresentation,
		false,
		false,
		"Office of Presentation for Centralized Clearance");

	CustomsOfficeRequirement ExpectedSupervisingCustomsOfficeReq => new CustomsOfficeRequirement(
		EuOfficeCodesTypes.Codes.AuthorityControlCode,
		false,
		false,
		"Supervising Customs Office");

	new JobDeclaration declaration => (JobDeclaration)base.declaration;
}
