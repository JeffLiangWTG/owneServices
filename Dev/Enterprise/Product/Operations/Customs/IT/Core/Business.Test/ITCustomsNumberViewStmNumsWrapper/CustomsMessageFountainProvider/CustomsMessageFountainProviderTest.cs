using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsMessageFountainProviderTest : TestCaseWithFactory
{
	[TestDate(2019, 01, 01)]
	public void TestCustomsMessageFountainProviderForImportMessage()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "AAAA").AppendAccountDetail("AAAA-DEC1", "DEC1")
			.AppendAccount("22222222222-001", "BBBB").AppendAccountDetail("BBBB-DEC2", "DEC2")
			.AppendAccount("33333333333-001", "CCCC").AppendAccountDetail("CCCC-DEC3", "DEC3")
			.Build();

		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "22222222222", currentValue: 999999);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "33333333333", currentValue: 10000);
		Factory.Save();

		CombineAssertions("Check for node [XXXX]", () =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("XXXX", "R", company);
			AssertEquals(company.PK, provider.Company.PK);
			AssertEquals("", provider.DeclarantTaxNumber);
			AssertEquals("2019:", provider.FountainPrefix);
			AssertEquals("R", provider.FountainType);
			AssertEquals("XXXX", provider.Node);
			AssertNull("Wrapper", provider.Wrapper);
			AssertNull("Number Fountain", provider.TryGetNumberFountain());
		});

		CombineAssertions("Check for node [AAAA]", () =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("AAAA", "R", company);
			AssertEquals(company.PK, provider.Company.PK);
			AssertEquals("11111111111", provider.DeclarantTaxNumber);
			AssertEquals("2019:11111111111", provider.FountainPrefix);
			AssertEquals("R", provider.FountainType);
			AssertEquals("AAAA", provider.Node);
			AssertNull("Wrapper", provider.Wrapper);
			AssertNull("Number Fountain", provider.TryGetNumberFountain());
		});

		CombineAssertions("Check for node [BBBB]", () =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("BBBB", "R", company);
			AssertEquals(company.PK, provider.Company.PK);
			AssertEquals("22222222222", provider.DeclarantTaxNumber);
			AssertEquals("2019:22222222222", provider.FountainPrefix);
			AssertEquals("R", provider.FountainType);
			AssertEquals("BBBB", provider.Node);
			AssertNull("Wrapper", provider.Wrapper);
			AssertNull("Number Fountain", provider.TryGetNumberFountain());
		});

		CombineAssertions("Check for node [CCCC]", () =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("CCCC", "R", company);
			AssertEquals(company.PK, provider.Company.PK);
			AssertEquals("33333333333", provider.DeclarantTaxNumber);
			AssertEquals("2019:33333333333", provider.FountainPrefix);
			AssertEquals("R", provider.FountainType);
			AssertEquals("CCCC", provider.Node);
			AssertNotNull("Wrapper", provider.Wrapper);
			AssertNotNull("Number Fountain", provider.TryGetNumberFountain());
		});
	}

	[TestDate(2020, 01, 01)]
	public void TestHasClonableNumberRanges()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "AAAA").AppendAccountDetail("AAAA-DEC1", "DEC1")
			.AppendAccount("22222222222-001", "BBBB").AppendAccountDetail("BBBB-DEC2", "DEC2")
			.AppendAccount("33333333333-001", "CCCC").AppendAccountDetail("CCCC-DEC3", "DEC3")
			.Build();

		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestGenericNumberRange(company, 2019, "11111111111", fountainType: "XX", currentValue: 1000, minimumValue: 1, maximumValue: 10000);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, 2019, "22222222222", currentValue: 1000, minimumValue: 1, maximumValue: 1000);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, 2019, "22222222222", currentValue: 999999, minimumValue: 1001, maximumValue: 999999);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, 2020, "33333333333", currentValue: 10000);
		Factory.Save();

		CombineAssertions("Check for node [AAAA] and [R]", () =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("AAAA", "R", company);
			Assert("No clonable number ranges", !provider.HasClonableNumberRanges);
		});

		CombineAssertions("Check for node [AAAA] and [XX]", () =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("AAAA", "XX", company);
			Assert("Has clonable number ranges", provider.HasClonableNumberRanges);
		});

		CombineAssertions("Check for node [BBBB] and [R]", () =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("BBBB", "R", company);
			Assert("Has clonable number ranges", provider.HasClonableNumberRanges);
		});

		CombineAssertions("Check for node [CCCC] and [R]", () =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("CCCC", "R", company);
			Assert("No clonable number ranges", !provider.HasClonableNumberRanges);
		});
	}

	[UseSnapshotProtection]
	[TestDate(2020, 01, 01)]
	public void TestTryCloneLastYearNumberRangesWhenSingleNumberRange()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("11111111111-001", "AAAA").AppendAccountDetail("AAAA-DEC1", "DEC1")
			.Build();

		var tempFactory = new BusinessObjectFactory();
		var tempCompany = tempFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(tempCompany, 2019, "11111111111", currentValue: 10000, minimumValue: 1, maximumValue: 10000);
		tempFactory.Save();
		company.Reload();

		CombineAssertions(() =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("AAAA", "R", company);
			AssertEquals("PRE-Condition: number of available number ranges", 1, company.CustomsNumberProvider.CustomsNumbers.Count);
			provider.TryCloneLastYearNumberRanges();
			AssertEquals("POST-Condition: number of available number ranges", 2, company.CustomsNumberProvider.CustomsNumbers.Count);
			var clonedNumberRange = company.CustomsNumberProvider.CustomsNumbers.SingleOrDefault(x => x.SN_Prefix == "2020:11111111111|1");
			AssertNotNull("Cloned number range not null", clonedNumberRange);
			AssertEquals("Fountain Type", "R", clonedNumberRange.SN_Type);
			AssertEquals("Minimum Value", 1L, clonedNumberRange.SN_MinimumValue);
			AssertEquals("Maximum Value", 10000L, clonedNumberRange.SN_MaximumValue);
			AssertEquals("Current Value", 1L, clonedNumberRange.SN_Value);
			AssertEquals("Fountain Name", "2020:11111111111", clonedNumberRange.SN_FountainName);
			AssertEquals("Prefix", "2020:11111111111|1", clonedNumberRange.SN_Prefix);
		});
	}

	[UseSnapshotProtection]
	[TestDate(2020, 01, 01)]
	public void TestTryCloneLastYearNumberRangesWhenMultipleNumberRanges()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("22222222222-001", "BBBB").AppendAccountDetail("BBBB-DEC2", "DEC2")
			.Build();

		var tempFactory = new BusinessObjectFactory();
		var tempCompany = tempFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, 2019, "22222222222", currentValue: 1000, minimumValue: 1, maximumValue: 1000);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, 2019, "22222222222", currentValue: 888888, minimumValue: 1001, maximumValue: 999999);
		tempFactory.Save();
		company.Reload();

		CombineAssertions(() =>
		{
			var provider = new CustomsMessageFountainProviderForTesting("BBBB", "R", company);
			AssertEquals("PRE-Condition: number of available number ranges", 2, company.CustomsNumberProvider.CustomsNumbers.Count);
			provider.TryCloneLastYearNumberRanges();
			AssertEquals("POST-Condition: number of available number ranges", 4, company.CustomsNumberProvider.CustomsNumbers.Count);

			var clonedNumberRange1 = company.CustomsNumberProvider.CustomsNumbers.SingleOrDefault(x => x.SN_Prefix == "2020:22222222222|1");
			AssertNotNull("Range1: Cloned number range not null", clonedNumberRange1);
			AssertEquals("Range1: Fountain Type", "R", clonedNumberRange1.SN_Type);
			AssertEquals("Range1: Minimum Value", 1L, clonedNumberRange1.SN_MinimumValue);
			AssertEquals("Range1: Maximum Value", 1000L, clonedNumberRange1.SN_MaximumValue);
			AssertEquals("Range1: Current Value", 1L, clonedNumberRange1.SN_Value);
			AssertEquals("Range1: Fountain Name", "2020:22222222222", clonedNumberRange1.SN_FountainName);
			AssertEquals("Range1: Prefix", "2020:22222222222|1", clonedNumberRange1.SN_Prefix);

			var clonedNumberRange2 = company.CustomsNumberProvider.CustomsNumbers.SingleOrDefault(x => x.SN_Prefix == "2020:22222222222|2");
			AssertNotNull("Range2: Cloned number range not null", clonedNumberRange2);
			AssertEquals("Range2: Fountain Type", "R", clonedNumberRange2.SN_Type);
			AssertEquals("Range2: Minimum Value", 1001L, clonedNumberRange2.SN_MinimumValue);
			AssertEquals("Range2: Maximum Value", 999999L, clonedNumberRange2.SN_MaximumValue);
			AssertEquals("Range2: Current Value", 1001L, clonedNumberRange2.SN_Value);
			AssertEquals("Range2: Fountain Name", "2020:22222222222", clonedNumberRange2.SN_FountainName);
			AssertEquals("Range2: Prefix", "2020:22222222222|2", clonedNumberRange2.SN_Prefix);
		});
	}

	[UseSnapshotProtection]
	[TestDate(2020, 01, 01)]
	public void TestTryCloneLastYearNumberRangesWhenNoNumberRanges()
	{
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("11111111111-001", "AAAA").AppendAccountDetail("AAAA-DEC1", "DEC1")
			.Build();
		var tempFactory = new BusinessObjectFactory();
		var tempCompany = tempFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(tempCompany, 2020, "11111111111", currentValue: 10000, minimumValue: 1, maximumValue: 10000);
		tempFactory.Save();
		company.Reload();

		var provider = new CustomsMessageFountainProviderForTesting("AAAA", "R", company);
		AssertEquals("PRE-Condition: number of available number ranges", 1, company.CustomsNumberProvider.CustomsNumbers.Count);
		provider.TryCloneLastYearNumberRanges();
		AssertEquals("POST-Condition: number of available number ranges", 1, company.CustomsNumberProvider.CustomsNumbers.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.New<OrgHeader>().OH_Code = "DEC2";
		Factory.New<OrgHeader>().OH_Code = "DEC3";
		Factory.Save();
	}
}

internal class CustomsMessageFountainProviderForTesting : CustomsMessageFountainProvider
{
	public CustomsMessageFountainProviderForTesting(ZString node, ZString fountainType, GlbCompany company)
	{
		NodeCore = node;
		FountainTypeCore = fountainType;
		CompanyCore = company;
	}

	protected override ZString NodeCore { get; }

	protected override ZString FountainTypeCore { get; }

	protected override GlbCompany CompanyCore { get; }
}
