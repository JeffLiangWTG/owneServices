using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierConsignmentsTableTest : TestCase
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("null consignments", () => new NCTSPrettierConsignmentsTable(consignments: null));
			AssertNoExceptionThrown("All ok", () => new NCTSPrettierConsignmentsTable(consignments));
		});

		public void TestCaption() => AssertEquals("Consignments", table.Caption);

		public void TestBorder() => AssertEquals("1", table.Border);

		public void TestWidth() => AssertEquals("100%", table.Width);

		public void TestColumns() => AssertContainsExactElementsInExactOrder(
			expected: new (ZString Caption, ZString Width)[] {
				("UCR Reference", "width=25%"),
				("Goods Items", "width=75%") },
			actual: table.Columns);

		public void TestAdditionalInfo() => AssertContainsExactElementsInExactOrder(
			new (ZString Key, ZString Value)[] {
				("Number of Consignments", "2"),
				("Number of Consignment Items", "3") },
			actual: table.AdditionalInfo);

		public void TestToString()
			=> AssertEquals(ExpectedHtml.Replace("\r\n", string.Empty).Replace("\t", string.Empty), table.ToString());

		protected override void SetUp()
		{
			base.SetUp();

			consignments = new INCTSPrettierConsignmentData[] {
				new NCTSPrettierConsignmentData("Reference 1", new [] { new NCTSPrettierGoodsItemData("Number 1", "Ref 1", "Description 1", "Code 1") }),
				new NCTSPrettierConsignmentData("Reference 2", new [] {
					new NCTSPrettierGoodsItemData("Number 2", "Ref 2", "Description 2", "Code 2"),
					new NCTSPrettierGoodsItemData("Number 3", "Ref 3", "Description 3", "Code 3"), })
			};
			table = new NCTSPrettierConsignmentsTable(consignments);
		}

		INCTSPrettierConsignmentData[] consignments;
		NCTSPrettierConsignmentsTable table;

		const string ExpectedHtml = @"<H3>Consignments</H3>
<p>
	<strong>Number of Consignments: </strong>2<br>
	<strong>Number of Consignment Items: </strong>3
</p>
<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
	<tr>
		<td width=""25%"">UCR Reference</td>
		<td width=""75%"">Goods Items</td>
	</tr>
	<tr>
		<td>Reference 1</td>
		<td>
			<p>
				<table border=""0"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
					<tr>
						<td class=""tdGoodsItemsTitle"">Item Number</td>
						<td class=""tdGoodsItemsTitle"">UCR Reference</td>
						<td class=""tdGoodsItemsTitle"">Description</td>
						<td class=""tdGoodsItemsTitle"">Code</td>
					</tr>
					<tr>
						<td>Number 1</td>
						<td>Ref 1</td>
						<td>Description 1</td>
						<td>Code 1</td>
					</tr>
				</table>
			</p>
		</td>
	</tr>
	<tr>
		<td>Reference 2</td>
		<td>
			<p>
				<table border=""0"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
					<tr>
						<td class=""tdGoodsItemsTitle"">Item Number</td>
						<td class=""tdGoodsItemsTitle"">UCR Reference</td>
						<td class=""tdGoodsItemsTitle"">Description</td>
						<td class=""tdGoodsItemsTitle"">Code</td>
					</tr>
					<tr>
						<td>Number 2</td>
						<td>Ref 2</td>
						<td>Description 2</td>
						<td>Code 2</td>
					</tr>
					<tr>
						<td>Number 3</td>
						<td>Ref 3</td>
						<td>Description 3</td>
						<td>Code 3</td>
					</tr>
				</table>
			</p>
		</td>
	</tr>
</table>";
	}
}
