using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
public class JobDeclarationCustomsOfficeRequirementHelperTest : EU.Business.Declaration.Testing.JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
{
	public void TestOfficeCodeList_IMP()
	{
		CreateTestDataForOfficeCodeList();

		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var officeCode = dec.CustomsOffices.AddNew();
		officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfGuarantee;
		var officeCodeList = officeCode.Lookups.OfficeCodeList;
		officeCodeList.Load();
		AssertEquals(1, officeCodeList.Count);
		AssertContainsExactElementsInAnyOrder(new[] { "NL000074" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

		officeCode.CY_Code = EuOfficeCodesTypes.Codes.AuthorityControlCode;
		officeCodeList = officeCode.Lookups.OfficeCodeList;
		officeCodeList.Load();
		AssertEquals(2, officeCodeList.Count);
		AssertContainsExactElementsInAnyOrder(new[] { "NL000568", "NL000703" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

		officeCode.CY_Code = ZString.Empty;
		officeCodeList = officeCode.Lookups.OfficeCodeList;
		officeCodeList.Load();
		AssertEquals(4, officeCodeList.Count);
		AssertContainsExactElementsInAnyOrder(new[] { "NL000074", "NL000100", "NL000568", "NL000703" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
	}

	public void TestOfficeCodeList_EXP()
	{
		CreateTestDataForOfficeCodeList();

		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var officeCode = dec.CustomsOffices.AddNew();

		officeCode.CY_Code = EuOfficeCodesTypes.Codes.AuthorityControlCode;
		var officeCodeList = officeCode.Lookups.OfficeCodeList;
		officeCodeList.Load();
		AssertEquals(2, officeCodeList.Count);
		AssertContainsExactElementsInAnyOrder(new[] { "NL000568", "NL000703" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));

		officeCode.CY_Code = ZString.Empty;
		officeCodeList = officeCode.Lookups.OfficeCodeList;
		officeCodeList.Load();
		AssertEquals(6, officeCodeList.Count);
		AssertContainsExactElementsInAnyOrder(new[] { "IEDUB100", "NL000074", "NL000100", "NL000568", "NL000703", "IEDUB809" }, officeCodeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
	}

	void CreateTestDataForOfficeCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		var codeIEDUB100 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var codeNL000074 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NL000074", "Douane/Landelijk kantoor", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var codeNL000100 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NL000100", "Nationale helpdesk douane", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeIEDUB100.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeNL000074.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfGuarantee);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeNL000100.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExport);
		var codeNL000568 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NL000568", "Eindhoven", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var codeNL000703 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "NL000703", "Amsterdam", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var codeIEDUB809 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB809", "DUBLIN NORTH CITY DISTRICT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeNL000568.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.AuthorityControlCode);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeNL000703.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.AuthorityControlCode);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeIEDUB809.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.AuthorityControlCode);
		Factory.Save();
	}

	public override void TestMainOffice_Import()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertCustomsOfficeRequirementEquals("Import", new CustomsOfficeRequirement(ZString.Empty, true, true), officeHelper.MainOffice);
	}

	public override void TestMainOffice_Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertCustomsOfficeRequirementEquals("Export", new CustomsOfficeRequirement(ZString.Empty, true, false), officeHelper.MainOffice);
	}

	public override void TestMainOffice_Miscellaneous()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertCustomsOfficeRequirementEquals("Misc", new CustomsOfficeRequirement(ZString.Empty, true, false), officeHelper.MainOffice);
	}

	public override void TestOtherRequirements_Import()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

		AssertEquals(2, officeHelper.OtherRequirements.Count());
		AssertCustomsOfficeRequirementEquals("Office Of Guarantee ", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfGuarantee, false, true),
											 officeHelper.OtherRequirements.Single(x => EuOfficeCodesTypes.Codes.OfficeOfGuarantee.Equals(x.OfficeRole)));
		AssertCustomsOfficeRequirementEquals("Supervising Office ", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.AuthorityControlCode, false, true, "Supervising Office"),
											 officeHelper.OtherRequirements.Single(x => EuOfficeCodesTypes.Codes.AuthorityControlCode.Equals(x.OfficeRole)));
	}

	public override void TestOtherRequirements_Export()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals(2, officeHelper.OtherRequirements.Count());
		AssertCustomsOfficeRequirementEquals("Office Of Exit ", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, true, false),
								 officeHelper.OtherRequirements.Single(x => EuOfficeCodesTypes.Codes.OfficeOfExit.Equals(x.OfficeRole)));
		AssertCustomsOfficeRequirementEquals("Supervising Office ", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.AuthorityControlCode, false, true, "Supervising Office"),
											 officeHelper.OtherRequirements.Single(x => EuOfficeCodesTypes.Codes.AuthorityControlCode.Equals(x.OfficeRole)));
	}

	public override void TestOtherRequirements_Miscellaneous()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertEquals(false, officeHelper.OtherRequirements.Any());
	}

	protected override string SetupDeclarationForCacheKey()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		return "JobDeclarationCustomsOfficeRequirementHelper,NL,IMP";
	}

	protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	new JobDeclaration declaration => (JobDeclaration)base.declaration;
}
