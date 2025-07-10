using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(ITCustomsNumberViewStmNumsWrapper))]
sealed class ITCustomsNumberViewStmNumsWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
{
	public void TestProperties()
	{
		var stmNum = Factory.New<CustomsNumberViewStmNums>();
		stmNum.Provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
		stmNum.SN_Owner = GlbCompany.CurrentCompany.PK;
		stmNum.SN_Type = NumberRangeTypeList.Codes.EntrySummaryDeclaration;
		stmNum.SN_FountainName = ITCustomsNumberViewStmNumsWrapper.GenerateFountainName(ZDate.Today.Year + 1, "1234567890");
		stmNum.HasChanges = false;
		var wrapper = new ITCustomsNumberViewStmNumsWrapper(stmNum);
		AssertEquals("HasChanges", false, wrapper.HasChanges);
		AssertEquals("YearOfApplicability", ZDate.Today.Year, wrapper.YearOfApplicability);
		AssertEquals("YearOfApplicabilityInfo.ReadOnly", false, wrapper.YearOfApplicabilityInfo.ReadOnly);
		AssertEquals("AppliesTo", "1234567890", wrapper.AppliesTo);
		AssertEquals("AppliesTo.ReadOnly", false, wrapper.AppliesToInfo.ReadOnly);
		AssertEquals("ReadOnly", false, wrapper.ReadOnly);
		AssertEquals($"Range Type: ENS, Year: {ZDate.Today.Year}, Applies To: 1234567890", wrapper.Detail);

		stmNum.SN_FountainName = ITCustomsNumberViewStmNumsWrapper.GenerateFountainName(ZDate.Today.Year + 1, "2345678901");
		AssertEquals("HasChanges", true, wrapper.HasChanges);
		AssertEquals("YearOfApplicability", ZDate.Today.Year + 1, wrapper.YearOfApplicability);
		AssertEquals("YearOfApplicabilityInfo.ReadOnly", false, wrapper.YearOfApplicabilityInfo.ReadOnly);
		AssertEquals("AppliesTo", "2345678901", wrapper.AppliesTo);
		AssertEquals("AppliesTo.ReadOnly", false, wrapper.AppliesToInfo.ReadOnly);
		AssertEquals("ReadOnly", false, wrapper.ReadOnly);
		AssertEquals($"Range Type: ENS, Year: {ZDate.Today.Year + 1}, Applies To: 2345678901", wrapper.Detail);

		Factory.Save();
		var newFactory = new BusinessObjectFactory();
		var query = new ZQuery(ViewStmNumsSchema.SN_Owner, stmNum.SN_Owner);
		query.AddToFilter(ViewStmNumsSchema.SN_Name, stmNum.SN_Name);
		stmNum = newFactory.LoadTop1<CustomsNumberViewStmNums>(query);
		stmNum.Provider = GlbCompany.CurrentCompany.CustomsNumberProvider;
		wrapper = new ITCustomsNumberViewStmNumsWrapper(stmNum);
		AssertEquals("HasChanges", false, wrapper.HasChanges);
		AssertEquals("YearOfApplicability", ZDate.Today.Year + 1, wrapper.YearOfApplicability);
		AssertEquals("YearOfApplicabilityInfo.ReadOnly", true, wrapper.YearOfApplicabilityInfo.ReadOnly);
		AssertEquals("AppliesTo", "2345678901", wrapper.AppliesTo);
		AssertEquals("AppliesTo.ReadOnly", true, wrapper.AppliesToInfo.ReadOnly);
		AssertEquals("ReadOnly", false, wrapper.ReadOnly);

		wrapper.YearOfApplicability = 2017;
		AssertEquals("HasChanges", true, wrapper.HasChanges);
		AssertEquals("YearOfApplicability", 2017, wrapper.YearOfApplicability);
		AssertEquals("YearOfApplicabilityInfo.ReadOnly", true, wrapper.YearOfApplicabilityInfo.ReadOnly);
		AssertEquals("AppliesTo", "2345678901", wrapper.AppliesTo);
		AssertEquals("AppliesTo.ReadOnly", true, wrapper.AppliesToInfo.ReadOnly);
		AssertEquals("ReadOnly", true, wrapper.ReadOnly);

		wrapper.AppliesTo = "3456789012";
		AssertEquals("HasChanges", true, wrapper.HasChanges);
		AssertEquals("YearOfApplicability", 2017, wrapper.YearOfApplicability);
		AssertEquals("AppliesTo", "3456789012", wrapper.AppliesTo);
		AssertEquals("SN_FountainName", "2017:3456789012", stmNum.SN_FountainName);
		AssertEquals("ReadOnly", true, wrapper.ReadOnly);
	}

	public void TestExtraDataFromFountainName()
	{
		ZShort year;
		ZString appliesTo;
		var fountainName = "";
		ITCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out year, out appliesTo);
		AssertEquals(ZShort.Zero, year);
		AssertEquals(ZString.Empty, appliesTo);
		fountainName = "asdb";
		ITCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out year, out appliesTo);
		AssertEquals(ZShort.Zero, year);
		AssertEquals(ZString.Empty, appliesTo);
		fountainName = "2016";
		ITCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out year, out appliesTo);
		AssertEquals((ZShort)2016, year);
		AssertEquals(ZString.Empty, appliesTo);
		fountainName = "217:";
		ITCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out year, out appliesTo);
		AssertEquals((ZShort)217, year);
		AssertEquals(ZString.Empty, appliesTo);
		fountainName = "2018:HELLO WORLD";
		ITCustomsNumberViewStmNumsWrapper.ExtraDataFromFountainName(fountainName, out year, out appliesTo);
		AssertEquals((ZShort)2018, year);
		AssertEquals("HELLO WORLD", appliesTo);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var stmNumsProvider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, currentCompany.GC_RN_NKCountryCode, currentCompany.PK);
		var stmNums = CustomsNumberViewStmNumsHelper.NewStmNums(Factory, stmNumsProvider, currentCompany.PK);
		return new ITCustomsNumberViewStmNumsWrapper(stmNums);
	}
}

public static class ITCustomsNumberViewStmNumsWrapperTestHelper
{
	public static CustomsNumberViewStmNums SetupTestGenericNumberRange(GlbCompany company, int year, ZString fountainAppliesTo, string fountainType, int currentValue, int minimumValue = 1, int maximumValue = 999999)
	{
		var customsNumber1 = company.CustomsNumberProvider.CustomsNumbers.AddNew();
		customsNumber1.SN_Type = fountainType;
		customsNumber1.SN_MinimumValue = minimumValue;
		customsNumber1.SN_MaximumValue = maximumValue;
		customsNumber1.SN_Value = currentValue;
		customsNumber1.SN_FountainName = ITCustomsNumberViewStmNumsWrapper.GenerateFountainName(year, fountainAppliesTo);
		customsNumber1.SN_Prefix = customsNumber1.SN_FountainName;
		return customsNumber1;
	}

	public static CustomsNumberViewStmNums SetupTestCustomsDeclarationsNumberRange(GlbCompany company, int year, ZString fountainAppliesTo, int currentValue, int minimumValue = 1, int maximumValue = 999999)
	{
		return SetupTestGenericNumberRange(company, year, fountainAppliesTo, NumberRangeTypeList.Codes.CustomsDeclarations, currentValue, minimumValue, maximumValue);
	}
}
