using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.NumberFountain;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(MessageReferenceNumberGenerator))]
sealed class MessageReferenceNumberGeneratorTest : TestCaseWithFactory
{
	public void TestGenerateArugmentCheck()
	{
		var messageType = "A";
		var messageSubType = "AB";
		var messageOwner = "ABC";
		var companyPk = ZGuid.NewZGuid();
		var date = new ZDate(2021, 3, 16);

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("When messageType is null", () => Generator.Generate(null, messageSubType, messageOwner, companyPk, date));
			AssertExceptionThrown<ArgumentException>("When messageType is empty", () => Generator.Generate(string.Empty, messageSubType, messageOwner, companyPk, date));

			AssertExceptionThrown<ArgumentException>("When messageSubType is null", () => Generator.Generate(messageType, null, messageOwner, companyPk, date));
			AssertExceptionThrown<ArgumentException>("When messageSubType is empty", () => Generator.Generate(messageType, string.Empty, messageOwner, companyPk, date));

			AssertExceptionThrown<ArgumentException>("When messageOwner is null", () => Generator.Generate(messageType, messageSubType, null, companyPk, date));
			AssertExceptionThrown<ArgumentException>("When messageOwner is empty", () => Generator.Generate(messageType, messageSubType, string.Empty, companyPk, date));

			AssertExceptionThrown<ArgumentException>("When date is empty", () => Generator.Generate(messageType, messageSubType, messageOwner, companyPk, ZDate.Empty));

			AssertExceptionThrown<ArgumentException>("When companyPk is empty", () => Generator.Generate(messageType, messageSubType, messageOwner, ZGuid.Empty, date));

			AssertNoExceptionThrown("When parameters are valid", () => Generator.Generate(messageType, messageSubType, messageOwner, companyPk, date));
		});
	}

	public void TestGenerateSequence()
	{
		var messageType1 = "SB";
		var messageSubType1 = "SBF";
		var messageOwner1 = "INBLR4";
		var companyPk1 = ZGuid.NewZGuid();
		var date1 = new ZDate(2021, 3, 16);

		var messageType2 = "CGM";
		var messageSubType2 = "ACM";
		var messageOwner2 = "INMUM4";
		var companyPk2 = ZGuid.NewZGuid();
		var date2 = new ZDate(2022, 3, 16);

		CombineAssertions(() =>
		{
			AssertEquals("0000001", Generator.Generate(messageType1, messageSubType1, messageOwner1, companyPk1, date1));
			AssertEquals("message type 1, sub type 1, message owner 1, company pk 1, financial year 1", "0000002", Generator.Generate(messageType1, messageSubType1, messageOwner1, companyPk1, date1));
			AssertEquals("message type 2, sub type 1, message owner 1, company pk 1, financial year 1", "0000001", Generator.Generate(messageType2, messageSubType1, messageOwner1, companyPk1, date1));
			AssertEquals("message type 1, sub type 2, message owner 1, company pk 1, financial year 1", "0000001", Generator.Generate(messageType1, messageSubType2, messageOwner1, companyPk1, date1));
			AssertEquals("message type 1, sub type 1, message owner 2, company pk 1, financial year 1", "0000001", Generator.Generate(messageType1, messageSubType1, messageOwner2, companyPk1, date1));
			AssertEquals("message type 1, sub type 1, message owner 1, company pk 2, financial year 1", "0000001", Generator.Generate(messageType1, messageSubType1, messageOwner1, companyPk2, date1));
			AssertEquals("message type 1, sub type 1, message owner 1, company pk 1, financial year 2", "0000001", Generator.Generate(messageType1, messageSubType1, messageOwner1, companyPk1, date2));
		});
	}

	public void TestGenerateOverflow()
	{
		var messageType = "SB";
		var messageSubType = "SBF";
		var messageOwner = "OVERFLOW";
		var companyPk = ZGuid.NewZGuid();
		var date = new ZDate(2021, 3, 16);

		var fountainKey = $"INCustomsMessageNumber_{messageType}_{messageSubType}_{messageOwner}_{companyPk}_{Utils.GetIndianFinancialYear(date)}";
		var fountain = Env.NumberFountains.GetINCustomsMessageNumberFountain(fountainKey);
		fountain.SetNext(Factory, 9999999);

		string nextNumber() => Generator.Generate(messageType, messageSubType, messageOwner, companyPk, date);
		CombineAssertions(() =>
		{
			AssertEquals("Max value", "9999999", nextNumber());
			AssertExceptionThrown<NumberFountainMaximumValueReachedException>("Number fountain after max value", () => nextNumber());
		});
	}

	MessageReferenceNumberGenerator Generator => generator ??= new MessageReferenceNumberGenerator(Factory);
	MessageReferenceNumberGenerator generator;
}
