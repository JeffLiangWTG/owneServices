using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class LocationOfGoodsCalculatorTest : TestCaseWithFactory
{
	public void TestCalculateGuardClauses()
	{
		AssertExceptionThrown<ArgumentNullException>("entryHeader is required", () => calculator.Calculate(null));
		AssertExceptionThrown<ArgumentNullException>("entryHeader.Declaration is required", () => calculator.Calculate(Factory.New<CusEntryHeader>()));
		AssertExceptionThrown<ArgumentNullException>("entryHeader.EntryInstruction is required", () => calculator.Calculate(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
		AssertNoExceptionThrown(() => calculator.Calculate(entryHeader));
	}

	public void TestCalculate()
	{
		CombineAssertions(() =>
		{
			AssertCodeAndCinPlaceOfExamination(ZString.Empty, ZString.Empty, false, "");
			AssertCodeAndCinPlaceOfExamination(ZString.Empty, ZString.Empty, true, "FE");
			AssertCodeAndCinPlaceOfExamination(ZString.Empty, "000123A", false, "000123A");
			AssertCodeAndCinPlaceOfExamination(ZString.Empty, "000123A", true, "000123A-FE");
			AssertCodeAndCinPlaceOfExamination(ZString.Empty, " 123A ", true, "123A-FE");

			AssertCodeAndCinPlaceOfExamination("X", ZString.Empty, false, "X");
			AssertCodeAndCinPlaceOfExamination("X", ZString.Empty, true, "X-FE");
			AssertCodeAndCinPlaceOfExamination("X", "000123A", false, "X-000123A");
			AssertCodeAndCinPlaceOfExamination("X", "000123A", true, "X-FE-000123A");
			AssertCodeAndCinPlaceOfExamination(" X ", " 123A ", true, "X-FE-123A");
		});

		void AssertCodeAndCinPlaceOfExamination(ZString locationQualifier, ZString locationOfGoods, ZBool electronicDocuments, ZString expectedCodeAndCinPlaceOfExamination)
		{
			declaration.JE_LocationQualifier = locationQualifier;
			declaration.JE_LocationOfGoods = locationOfGoods;
			entryInstruction.ElectronicDocuments = electronicDocuments;
			var assertionMessage = $"When JE_LocationQualifier = '{locationQualifier}' and JE_LocationOfGoods = '{locationOfGoods}' and ElectronicDocuments = '{electronicDocuments}', CodeAndCinPlaceOfExamination";
			AssertEquals(assertionMessage, expectedCodeAndCinPlaceOfExamination, calculator.Calculate(entryHeader));
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		calculator = new LocationOfGoodsCalculator();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryInstruction entryInstruction;
	LocationOfGoodsCalculator calculator;
}
