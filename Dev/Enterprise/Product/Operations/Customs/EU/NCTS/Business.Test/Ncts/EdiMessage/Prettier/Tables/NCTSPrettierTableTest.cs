using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierTableTest : TestCase
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("columns is null", () => new NCTSPrettierTable("Caption", columns: null, rows));
			AssertExceptionThrown<ArgumentException>("rows is null", () => new NCTSPrettierTable("Caption", columns, rows: null));
			AssertExceptionThrown<ArgumentException>("border is null", () => new NCTSPrettierTable("Caption", columns, rows, border: null));
			AssertExceptionThrown<ArgumentException>("width is null", () => new NCTSPrettierTable("Caption", columns, rows, width: null));
			AssertNoExceptionThrown("All ok", () => new NCTSPrettierTable("Caption", columns, rows));
		});

		public void TestCaption() => AssertEquals("Caption", table.Caption);

		public void TestBorder() => AssertEquals("2", table.Border);

		public void TestWidth() => AssertEquals("95%", table.Width);

		public void TestColumns() => AssertContainsExactElementsInExactOrder(
			expected: new (ZString Caption, ZString Width)[] {
				("Name", "width=25%"),
				("Value", "width=75%") },
			actual: table.Columns);

		public void TestAdditionalInfo() => AssertContainsExactElementsInExactOrder(
			new (ZString Key, ZString Value)[] {
				("Key 1", "Value 1"),
				("Key 2", "Value 2") },
			actual: table.AdditionalInfo);

		public void TestToString()
			=> AssertEquals(ExpectedHtml.Replace("\r\n", string.Empty).Replace("\t", string.Empty), table.ToString());

		protected override void SetUp()
		{
			base.SetUp();

			columns = new (ZString Caption, ZString Width)[] {
				("Name", "width=25%"),
				("Value", "width=75%")
			};
			rows = new[] {
				new object[] { "Test name 1", "Test value 1" },
				new object[] { "Test name 2", "Test value 2" },
			};
			var additionalInfo = new (ZString Key, ZString Value)[] {
				("Key 1", "Value 1"),
				("Key 2", "Value 2"),
			};
			table = new NCTSPrettierTable("Caption", columns, rows, additionalInfo, border: "2", width: "95%");
		}

		(ZString Caption, ZString Width)[] columns;
		object[][] rows;
		NCTSPrettierTable table;

		const string ExpectedHtml = @"<H3>Caption</H3>
<p><strong>Key 1: </strong>Value 1<br><strong>Key 2: </strong>Value 2</p>
<table border=""2"" cellpadding=""1"" cellspacing=""0"" width=""95%"" class=""table"">
	<tr>
		<td width=""25%"">Name</td>
		<td width=""75%"">Value</td>
	</tr>
	<tr>
		<td>Test name 1</td>
		<td>Test value 1</td>
	</tr>
	<tr>
		<td>Test name 2</td>
		<td>Test value 2</td>
	</tr>
</table>";
	}
}
