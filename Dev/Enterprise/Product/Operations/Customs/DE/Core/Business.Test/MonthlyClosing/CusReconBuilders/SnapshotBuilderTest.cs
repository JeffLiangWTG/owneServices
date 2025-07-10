using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class SnapshotBuilderTest : TestCase
	{
		public void TestSerializeWithoutXMLDeclarationAndIndentation()
		{
			var snapshotBuilder = new SnapshotBuilderForTest();
			var actual = snapshotBuilder.GetXMLMessage();
			var expected = @"<DEMonthlyClosingEntrySnapshot xmlns=""http://www.cargowise.com/Schemas/DEMonthlyClosing""><Document><Division>4</Division><Type>ä</Type><IssuingDate>2021-12-03</IssuingDate></Document></DEMonthlyClosingEntrySnapshot>";
			AssertEquals(expected, actual);
		}

		public void TestDeserialize()
		{
			var xmlContent = @"<DEMonthlyClosingEntrySnapshot xmlns=""http://www.cargowise.com/Schemas/DEMonthlyClosing""><Document><Division>4</Division><Type>ä</Type><IssuingDate>2021-12-03</IssuingDate></Document></DEMonthlyClosingEntrySnapshot>";
			var deserialized = SnapshotBuilderForTest.Deserialize(xmlContent);

			CombineAssertions(() =>
			{
				AssertNotNull(deserialized);
				AssertEquals(1, deserialized.Document.Length);
			});
		}
	}

	class SnapshotBuilderForTest : SnapshotBuilder<DEMonthlyClosingEntrySnapshot>
	{
		public override DEMonthlyClosingEntrySnapshot GenerateMessage() => new DEMonthlyClosingEntrySnapshot()
		{
			Document = new DEMonthlyClosingEntrySnapshotDocument[]
			{
				new DEMonthlyClosingEntrySnapshotDocument
				{
					Type = "ä",
					IssuingDate = new System.DateTime(2021, 12, 03)
				}
			}
		};
	}
}
