using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.NumberFountain;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[UseSnapshotProtection]
sealed class DailySequenceNumberGeneratorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DailySequenceNumberGenerator(account: null, factory: null));
		AssertExceptionThrown<ArgumentNullException>(() => new DailySequenceNumberGenerator(account: null, factory: Factory));
		AssertExceptionThrown<ArgumentNullException>(() => new DailySequenceNumberGenerator(account: accountForNode1234, factory: null));
		AssertNoExceptionThrown(() => new DailySequenceNumberGenerator(account: accountForNode1234, factory: Factory));
	}

	[TestDate(2020, 07, 08, 10, 00, 00)]
	public void TestGenerate_RangesNotDefaultedFromAccount()
	{
		accountForNode1234.AccountRangeStart = "";
		accountForNode1234.AccountRangeEnd = "";

		var generatorForNode1234 = new DailySequenceNumberGenerator(accountForNode1234, Factory);
		AssertEquals("00", generatorForNode1234.Generate());
		AssertEquals("01", generatorForNode1234.Generate());
		AssertEquals("02", generatorForNode1234.Generate());

		const int actualValue = DailySequenceNumberFormatter.Constants.MinValue + 3;
		const int secondLastValue = DailySequenceNumberFormatter.Constants.MaxValue - 1;

		AssertNoExceptionThrown($"Generating numbers between {actualValue} and {secondLastValue}", () =>
		{
			for (int i = actualValue; i <= secondLastValue; i++)
			{
				generatorForNode1234.Generate();
			}
		});
		AssertEquals("zz", generatorForNode1234.Generate());
		AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() => generatorForNode1234.Generate());
	}

	[TestDate(2020, 01, 01)]
	public void TestGenerate_RangesDefaultedFromAccount()
	{
		accountForNode1234.AccountRangeStart = "1w";
		accountForNode1234.AccountRangeEnd = "20";

		var generatorForNode1234 = new DailySequenceNumberGenerator(accountForNode1234, Factory);
		AssertEquals("1w", generatorForNode1234.Generate());
		AssertEquals("1x", generatorForNode1234.Generate());
		AssertEquals("1y", generatorForNode1234.Generate());
		AssertEquals("1z", generatorForNode1234.Generate());
		AssertEquals("20", generatorForNode1234.Generate());
		AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() => generatorForNode1234.Generate());
	}

	[TestDate(2019, 07, 07, 10, 00, 00)]
	public void TestNewFountainIsCreatedPerDayPerNode()
	{
		void RunMultipleGeneration(DailySequenceNumberGenerator generator, int times)
		{
			for (int i = 0; i < times; i++)
			{
				generator.Generate();
			}
		}
		var generatorForNode1234 = new DailySequenceNumberGenerator(accountForNode1234, Factory);
		var generatorforNode5678 = new DailySequenceNumberGenerator(accountForNode5678, Factory);

		AssertEquals(new ZDate(2019, 07, 07), ZDate.Today);
		RunMultipleGeneration(generatorforNode5678, 3);
		AssertEquals("Last generation for node '5678' and '07/07/2019'", "03", generatorforNode5678.Generate());

		TestDateAttribute.AddYears(1);
		AssertEquals(new ZDate(2020, 07, 07), ZDate.Today);
		RunMultipleGeneration(generatorForNode1234, 5);
		AssertEquals("Last generation for node '1234' and '07/07/2020'", "05", generatorForNode1234.Generate());

		TestDateAttribute.AddDays(1);
		AssertEquals(new ZDate(2020, 07, 08), ZDate.Today);

		AssertEquals("1st generation for node '1234' and '08/07/2020'", "00", generatorForNode1234.Generate());

		AssertEquals("1st generation for node '5678' and '08/07/2020'", "00", generatorforNode5678.Generate());
		AssertEquals("2nd generation for node '5678' and '08/07/2020'", "01", generatorforNode5678.Generate());
		AssertEquals("3rd generation for node '5678' and '08/07/2020'", "02", generatorforNode5678.Generate());

		AssertEquals("2nd generation for node '1234' and '08/07/2020'", "01", generatorForNode1234.Generate());
	}

	public void TestCanGenerate()
	{
		accountForNode1234.AccountRangeStart = "00";
		accountForNode1234.AccountRangeEnd = "02";
		var generatorForNode1234 = new DailySequenceNumberGenerator(accountForNode1234, Factory);
		Assert("Can generate '00' (NumberFountain not created yet)", generatorForNode1234.CanGenerate());
		generatorForNode1234.Generate();

		Assert("Can generate '01'", generatorForNode1234.CanGenerate());
		generatorForNode1234.Generate();

		Assert("Can generate '02'", generatorForNode1234.CanGenerate());
		generatorForNode1234.Generate();

		Assert("Can't generate '03'", !generatorForNode1234.CanGenerate());
		AssertExceptionThrown<NumberFountainMaximumValueReachedException>(() => generatorForNode1234.Generate());
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.New<OrgHeader>().OH_Code = "DEC2";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var accountCollection = new AccountCollectionTestBuilder(company.PK.ToGuid())
				.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
				.AppendAccount("22222222222-001", "5678").AppendAccountDetail("5678-DEC2", "DEC2")
				.Build();

		accountForNode1234 = accountCollection[0];
		accountForNode5678 = accountCollection[1];
	}

	Account accountForNode1234;
	Account accountForNode5678;
}
