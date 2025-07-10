using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.NumberFountain;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(LocalReferenceNumberGenerator))]
sealed class LocalReferenceNumberGeneratorTest : TestCaseWithFactory
{
	public void TestGenerateArgumentCheck()
	{
		var date = new ZDate(2021, 3, 16);

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("When Shipment Type is null", () => Generator.Generate(null, date));
			AssertExceptionThrown<ArgumentException>("When Shipment Type is empty", () => Generator.Generate(string.Empty, date));
			AssertExceptionThrown<ArgumentException>("When Date Type is empty", () => Generator.Generate("EXP", ZDate.Empty));

			AssertNoExceptionThrown("When parameters are valid", () => Generator.Generate("EXP", date));
		});
	}

	public void TestGenerateOverflow()
	{
		var shipmentType = "IMP";
		var date = new ZDate(2021, 3, 16);

		var fountainKey = $"{shipmentType}_{Utils.GetIndianFinancialYear(date)}";
		var fountain = Env.NumberFountains.GetINLocalReferenceNumberFountain(fountainKey);
		fountain.SetNext(Factory, 9999999);

		string nextNumber() => Generator.Generate(shipmentType, date);
		CombineAssertions(() =>
		{
			AssertEquals("Max value", "9999999", nextNumber());
			AssertExceptionThrown<NumberFountainMaximumValueReachedException>("Number fountain after max value", () => nextNumber());
		});
	}

	LocalReferenceNumberGenerator Generator => generator ??= new LocalReferenceNumberGenerator(Factory);
	LocalReferenceNumberGenerator generator;
}
