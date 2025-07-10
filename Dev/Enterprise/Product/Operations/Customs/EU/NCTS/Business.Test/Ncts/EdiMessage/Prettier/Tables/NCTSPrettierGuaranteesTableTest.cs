using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierGuaranteesTableTest : TestCase
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("null guarantees", () => new NCTSPrettierGuaranteesTable(guarantees: null));
			AssertNoExceptionThrown("All ok", () => new NCTSPrettierGuaranteesTable(guarantees));
		});

		public void TestCaption() => AssertEquals("Guarantees", table.Caption);

		public void TestBorder() => AssertEquals("1", table.Border);

		public void TestWidth() => AssertEquals("100%", table.Width);

		public void TestColumns() => AssertContainsExactElementsInExactOrder(
			expected: new (ZString Caption, ZString Width)[] {
				("Type", "width=25%"),
				("GRN Number", "width=25%"),
				("Other Number", "width=25%"),
				("Amount", "width=25%"),
			},
			actual: table.Columns);

		public void TestAdditionalInfo() => AssertContainsExactElementsInExactOrder(
			expected: Array.Empty<(ZString Key, ZString Value)>(),
			actual: table.AdditionalInfo);

		public void TestToString()
			=> AssertEquals(ExpectedHtml.Replace("\r\n", string.Empty).Replace("\t", string.Empty), table.ToString());

		protected override void SetUp()
		{
			base.SetUp();

			guarantees = new INCTSPrettierGuaranteeData[] {
				new NCTSPrettierGuaranteeData(type: "Type 1", grn: "GRN 1", otherNumber: "Number 1", amount: "Amount To Be Covered 1", currency: "Currency 1"),
				new NCTSPrettierGuaranteeData(type: "Type 2", grn: "GRN 2", otherNumber: "Number 2", amount: "Amount To Be Covered 2", currency: "Currency 2"),
			};
			table = new NCTSPrettierGuaranteesTable(guarantees);
		}

		INCTSPrettierGuaranteeData[] guarantees;
		NCTSPrettierGuaranteesTable table;

		const string ExpectedHtml = @"<H3>Guarantees</H3>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
	<tr>
		<td width=""25%"">Type</td>
		<td width=""25%"">GRN Number</td>
		<td width=""25%"">Other Number</td>
		<td width=""25%"">Amount</td>
	</tr>
	<tr>
		<td>Type 1</td>
		<td>GRN 1</td>
		<td>Number 1</td>
		<td>Amount To Be Covered 1 Currency 1</td>
	</tr>
	<tr>
		<td>Type 2</td>
		<td>GRN 2</td>
		<td>Number 2</td>
		<td>Amount To Be Covered 2 Currency 2</td>
	</tr>
</table>";
	}
}
