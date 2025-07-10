using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestEntryPhaseStatusList()
	{
		CusEntryHeader entry = Factory.New<CusEntryHeader>();
		AssertEquals(typeof(CustomsEntryPhaseStatusList), entry.Lookups.EntryPhaseStatusList.GetType());
	}

	public void TestMessageStatusList()
	{
		CusEntryHeader entry = Factory.New<CusEntryHeader>();
		AssertEquals(typeof(CustomsEntryMessageStatusList), entry.Lookups.MessageStatusList.GetType());
	}

	public void TestCH_EntryStatusList_Fallback()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.DMSFallbackIsActive = true;
		entryHeader.CH_Status = "SNT";

		var dataGrouping = declaration.CountryCode;
		var cusCodeHelper = new UniversalReferenceTestDataHelper(Factory);
		var cusCodeType = cusCodeHelper.CreateNewOrGetExistingCusCodeType("CSTA", "Entry Status List Type");
		cusCodeHelper.CreateNewOrGetExistingCusCodeList(dataGrouping, "CSTA", "CTL", "NI HAO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		cusCodeHelper.CreateNewOrGetExistingCusCodeList(dataGrouping, "CSTA", "NMB", "NI SHI SHEI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
		var result = entryHeader.Lookups.CH_EntryStatusList;
		AssertEquals("CTL, REL", result.CodesAsString);
		AssertEquals("NI HAO", result.GetDescriptionFromCode("CTL"));
		AssertEquals(ZString.Empty, result.GetDescriptionFromCode("REL"));
	}

	public void TestMessageStatusList_Fallback()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var companyPK = entryHeader.RegistryCompanyPK;
		NLCustomsRegistry.Instance.FallbackEmail.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, "MNL");
		entryHeader.DMSFallbackIsActive = true;
		entryHeader.CH_Status = "ERR";
		AssertEquals("ACC", entryHeader.Lookups.MessageStatusList.CodesAsString);
	}
}
