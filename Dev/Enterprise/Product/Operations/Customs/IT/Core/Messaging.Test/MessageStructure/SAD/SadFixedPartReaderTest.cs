using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class SadFixedPartReaderTest : TestCaseWithFactory
{
	public void TestLoadFromImportRow()
	{
		CombineAssertions("Test LoadFromRow()", () =>
		{
			AssertExceptionThrown<UnableToInterpretCustomsMessageException>("Should be exception when row is empty", () => SadFixedPartReader.LoadFromRow(ZString.Empty));
			AssertExceptionThrown<UnableToInterpretCustomsMessageException>("Should be exception when row is invalid [too short]", () => SadFixedPartReader.LoadFromRow("XXXX"));
			AssertExceptionThrown<UnableToInterpretCustomsMessageException>("Should be exception when row in invalid [progressive number is not a valid number]", () => SadFixedPartReader.LoadFromRow("TIM           000001AA"));
			AssertNoExceptionThrown("No Exception expected", () => SadFixedPartReader.LoadFromRow("TIM           00000101"));
		});

		var row = "TIM           00000101";
		var fixedPart = SadFixedPartReader.LoadFromRow(row);

		CombineAssertions("Test Properties", () =>
		{
			AssertEquals("RecordType", IdocRecordType.SadImportEntryHeader, fixedPart.RecordType);
			AssertEquals("AnnualProgressiveNumber", "000001", fixedPart.AnnualProgressiveNumber);
			AssertEquals("ProgressiveNumber", 1, fixedPart.ProgressiveNumber);
		});

		row = new ZString("?NB1          00000101");
		var fixedPartNb1 = SadFixedPartReader.LoadFromRow(row);

		CombineAssertions("Test Properties", () =>
		{
			AssertEquals("RecordType", IdocRecordType.SadNbLine, fixedPartNb1.RecordType);
			AssertEquals("AnnualProgressiveNumber", "000001", fixedPartNb1.AnnualProgressiveNumber);
			AssertEquals("ProgressiveNumber", 1, fixedPartNb1.ProgressiveNumber);
		});

		row = "XXX           AB000101";
		var fixedPartIdocHeader = SadFixedPartReader.LoadFromRow(row);

		CombineAssertions("Test Properties", () =>
		{
			AssertEquals("RecordType", IdocRecordType.Unkown, fixedPartIdocHeader.RecordType);
			AssertEquals("AnnualProgressiveNumber", "AB0001", fixedPartIdocHeader.AnnualProgressiveNumber);
			AssertEquals("ProgressiveNumber", 1, fixedPartIdocHeader.ProgressiveNumber);
		});
	}
	public void TestLoadFromExportRow()
	{
		CombineAssertions("Test LoadFromRow()", () =>
		{
			AssertExceptionThrown<UnableToInterpretCustomsMessageException>("Should be exception when row is empty", () => SadFixedPartReader.LoadFromRow(ZString.Empty));
			AssertExceptionThrown<UnableToInterpretCustomsMessageException>("Should be exception when row is invalid [too short]", () => SadFixedPartReader.LoadFromRow("XXXX"));
			AssertExceptionThrown<UnableToInterpretCustomsMessageException>("Should be exception when row in invalid [progressive number is not a valid number]", () => SadFixedPartReader.LoadFromRow("TET           000001AA"));
			AssertNoExceptionThrown("No Exception expected", () => SadFixedPartReader.LoadFromRow("TET           00000101"));
		});

		var row = "TET           00000101";
		var fixedPart = SadFixedPartReader.LoadFromRow(row);

		CombineAssertions("Test Properties", () =>
		{
			AssertEquals("RecordType", IdocRecordType.SadExportEntryHeader, fixedPart.RecordType);
			AssertEquals("AnnualProgressiveNumber", "000001", fixedPart.AnnualProgressiveNumber);
			AssertEquals("ProgressiveNumber", 1, fixedPart.ProgressiveNumber);
		});

		row = new ZString("?NB1          00000101");
		var fixedPartNb1 = SadFixedPartReader.LoadFromRow(row);

		CombineAssertions("Test Properties", () =>
		{
			AssertEquals("RecordType", IdocRecordType.SadNbLine, fixedPartNb1.RecordType);
			AssertEquals("AnnualProgressiveNumber", "000001", fixedPartNb1.AnnualProgressiveNumber);
			AssertEquals("ProgressiveNumber", 1, fixedPartNb1.ProgressiveNumber);
		});

		row = "XXX           AB000101";
		var fixedPartIdocHeader = SadFixedPartReader.LoadFromRow(row);

		CombineAssertions("Test Properties", () =>
		{
			AssertEquals("RecordType", IdocRecordType.Unkown, fixedPartIdocHeader.RecordType);
			AssertEquals("AnnualProgressiveNumber", "AB0001", fixedPartIdocHeader.AnnualProgressiveNumber);
			AssertEquals("ProgressiveNumber", 1, fixedPartIdocHeader.ProgressiveNumber);
		});
	}
}

