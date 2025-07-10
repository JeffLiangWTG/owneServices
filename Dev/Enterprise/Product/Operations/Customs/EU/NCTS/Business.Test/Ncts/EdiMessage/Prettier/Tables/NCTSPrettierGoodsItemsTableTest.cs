using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierGoodsItemsTableTest : TestCase
	{
		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("null goodsItems", () => new NCTSPrettierGoodsItemsTable(goodsItems: null));
			AssertNoExceptionThrown("All ok", () => new NCTSPrettierGoodsItemsTable(goodsItems));
		});

		public void TestCaption() => AssertEquals(ZString.Empty, table.Caption);

		public void TestBorder() => AssertEquals("0", table.Border);

		public void TestWidth() => AssertEquals("100%", table.Width);

		public void TestColumns() => AssertContainsExactElementsInExactOrder(
			expected: new (ZString Caption, ZString Width)[] {
				("Item Number", "class=tdGoodsItemsTitle"),
				("UCR Reference", "class=tdGoodsItemsTitle"),
				("Description", "class=tdGoodsItemsTitle"),
				("Code", "class=tdGoodsItemsTitle"),
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

			goodsItems = new INCTSPrettierGoodsItemData[] {
				new NCTSPrettierGoodsItemData("Number 1", "Ref 1", "Description 1", "Code 1"),
				new NCTSPrettierGoodsItemData("Number 2", "Ref 2", "Description 2", "Code 2"),
				new NCTSPrettierGoodsItemData("Number 3", "Ref 3", "Description 3", "Code 3"),
			};
			table = new NCTSPrettierGoodsItemsTable(goodsItems);
		}

		INCTSPrettierGoodsItemData[] goodsItems;
		NCTSPrettierGoodsItemsTable table;

		const string ExpectedHtml = @"
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
</table>";
	}
}
