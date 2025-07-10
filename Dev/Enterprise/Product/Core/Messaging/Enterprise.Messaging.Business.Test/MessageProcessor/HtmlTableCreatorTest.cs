using System;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	public class HtmlTableCreatorTest : TestCase
	{
		public void TestTableCreation()
		{
			var expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead><tr><td>a</td><td>b</td></tr></table>";
			var creator = new HtmlTableCreator(new string[] { "Column 1", "Column 2" });
			creator.WriteRow("a", "b");
			AssertEquals("Write row", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead><tr align=\"center\"><td>a</td><td>b</td><th>c</th></tr></table>";
			creator = new HtmlTableCreator(new string[] { "Column 1", "Column 2" }, new NameValueCollection { { "width", "100%" } });
			var cell = new CellWithFormatting("c", true);
			creator.WriteRow(new NameValueCollection { { "align", "center" } }, "a", "b", cell);
			AssertEquals("Write row", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead><tr class=\"myRowClass\"><td>a</td><td>b</td></tr></table>";
			creator = new HtmlTableCreator(new string[] { "Column 1", "Column 2" });
			var cell1 = new CellWithFormatting("a");
			var cell2 = new CellWithFormatting("b");
			creator.WriteRowWithFormatting("myRowClass", cell1, cell2);
			AssertEquals("Write row with class, no attributes", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead><tr class=\"myRowClass\"><td width=\"100px\">a</td><td width=\"100px\">b</td></tr></table>";
			creator = new HtmlTableCreator(new string[] { "Column 1", "Column 2" });
			cell1.HtmlAttributes.Add("width", "100px");
			cell2.HtmlAttributes.Add("width", "100px");
			creator.WriteRowWithFormatting("myRowClass", cell1, cell2);
			AssertEquals("Write row with class and attributes", expectedHtml, creator.ToHtml());
		}

		public void TestTableCreationWithEmptyColumn()
		{
			var creator = new HtmlTableCreator(new string[] { "Column 1", "Column 2" }, null, true);
			creator.WriteRow("a", string.Empty);
			var html = creator.ToHtml();

			Assert("Column Header not hidden", html.Contains("th:nth-of-type(2)"));
			Assert("Column Cell not hidden", html.Contains("td:nth-of-type(2)"));
		}

		public void TestConstructors_Exception()
		{
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: tableAttributes", () => new HtmlTableCreator((NameValueCollection)null));
#else
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null. (Parameter 'tableAttributes')", () => new HtmlTableCreator((NameValueCollection)null));
#endif
			AssertExceptionThrown(typeof(ArgumentException),
#if NETFRAMEWORK
				"Column 'Column 2' in columnTitlesWithAttributes must have IsTitle set to true.\r\nParameter name: columnTitlesWithAttributes",
#else
				"Column 'Column 2' in columnTitlesWithAttributes must have IsTitle set to true. (Parameter 'columnTitlesWithAttributes')",
#endif
				() => new HtmlTableCreator(
					new NameValueCollection
					{
						{ "width", "100%" }
					},
					new[]
					{
						new CellWithFormatting("Column 1", true),
						new CellWithFormatting("Column 2",
						new NameValueCollection
						{
							{ "colspan", "3" },
							{ "BOB", "THE BUILDER" }
						},
						false)
					}));
		}

		public void TestConstructors()
		{
			var expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"></table>";
			var creator = new HtmlTableCreator();
			AssertEquals("Constructor HtmlTableCreator()", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator(new[] { "Column 1", "Column 2" });
			AssertEquals("Constructor HtmlTableCreator(IEnumerable<string> columnTitles)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator(new[] { "Column 1", "Column 2" }, new NameValueCollection { { "width", "100%" } });
			AssertEquals("Constructor HtmlTableCreator(IEnumerable<string> columnTitles, NameValueCollection additionalTableAttributes)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th colspan=\"3\" BOB=\"THE BUILDER\">Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator(new[] { new CellWithFormatting("Column 1", true), new CellWithFormatting("Column 2", new NameValueCollection { { "colspan", "3" }, { "BOB", "THE BUILDER" } }, true) });
			AssertEquals("Constructor HtmlTableCreator(IEnumerable<CellWithFormatting> columnTitlesWithAttributes)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th colspan=\"3\" BOB=\"THE BUILDER\">Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator(new[] { new CellWithFormatting("Column 1", true), new CellWithFormatting("Column 2", new NameValueCollection { { "colspan", "3" }, { "BOB", "THE BUILDER" } }, true) }, new NameValueCollection { { "width", "100%" } });
			AssertEquals("Constructor HtmlTableCreator(IEnumerable<CellWithFormatting> columnTitlesWithAttributes, NameValueCollection additionalTableAttributes)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"myTableClass\"></table>";
			creator = new HtmlTableCreator("myTableClass");
			AssertEquals("Constructor HtmlTableCreator(string tableClass)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"myTableClass\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator("myTableClass", new string[] { "Column 1", "Column 2" });
			AssertEquals("Constructor HtmlTableCreator(string tableClass, IEnumerable<string> columnTitles)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"myTableClass\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator("myTableClass", new[] { "Column 1", "Column 2" }, new NameValueCollection { { "width", "100%" } });
			AssertEquals("Constructor HtmlTableCreator(string tableClass, IEnumerable<string> columnTitles, NameValueCollection additionalTableAttributes)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th colspan=\"3\" BOB=\"THE BUILDER\">Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator("", new[] { new CellWithFormatting("Column 1", true), new CellWithFormatting("Column 2", new NameValueCollection { { "colspan", "3" }, { "BOB", "THE BUILDER" } }, true) });
			AssertEquals("Constructor HtmlTableCreator(string tableClass, IEnumerable<CellWithFormatting> columnTitlesWithAttributes)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th colspan=\"3\" BOB=\"THE BUILDER\">Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator("", new[] { new CellWithFormatting("Column 1", true), new CellWithFormatting("Column 2", new NameValueCollection { { "colspan", "3" }, { "BOB", "THE BUILDER" } }, true) }, new NameValueCollection { { "width", "100%" } });
			AssertEquals("Constructor HtmlTableCreator(string tableClass, IEnumerable<CellWithFormatting> columnTitlesWithAttributes, NameValueCollection additionalTableAttributes)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table width=\"100%\"></table>";
			creator = new HtmlTableCreator(new NameValueCollection { { "width", "100%" } });
			AssertEquals("Constructor HtmlTableCreator(NameValueCollection tableAttributes)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table width=\"100%\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th>Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator(new NameValueCollection { { "width", "100%" } }, new[] { "Column 1", "Column 2" });
			AssertEquals("Constructor HtmlTableCreator(NameValueCollection tableAttributes, IEnumerable<string> columnTitles)", expectedHtml, creator.ToHtml());

			expectedHtml = "<table width=\"100%\"><thead><tr class=\"tableheadings\"><th>Column 1</th><th colspan=\"3\" BOB=\"THE BUILDER\">Column 2</th></tr></thead></table>";
			creator = new HtmlTableCreator(new NameValueCollection { { "width", "100%" } }, new[] { new CellWithFormatting("Column 1", true), new CellWithFormatting("Column 2", new NameValueCollection { { "colspan", "3" }, { "BOB", "THE BUILDER" } }, true) });
			AssertEquals("Constructor HtmlTableCreator(NameValueCollection tableAttributes, IEnumerable<CellWithFormatting> columnTitlesWithAttributes)", expectedHtml, creator.ToHtml());
		}

		public void TestWriteRowWithFormatting()
		{
			var expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr align=\"center\"><td width=\"100px\" align=\"right\">a</td><td width=\"150px\">b</td></tr></table>";
			var creator = new HtmlTableCreator();
			var cell1 = new CellWithFormatting("a", "width", "100px");
			cell1.HtmlAttributes.Add("align", "right");
			var cell2 = new CellWithFormatting("b", "width", "150px");
			creator.WriteRowWithFormatting(new NameValueCollection { { "align", "center" } }, new[] { cell1, cell2 });

			AssertEquals("Write row with formatting", expectedHtml, creator.ToHtml());
		}

		public void TestHTMLEncoding()
		{
			var expectedTableWithHtmlEncoding = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>&lt;br /&gt;</td><td>&lt;br /&gt;</td></tr></table>";
			var expectedTableWithoutHtmlEncoding = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td><br /></td><td><br /></td></tr></table>";

			var creatorWithEncoding = new HtmlTableCreator();
			creatorWithEncoding.WriteRow("<br />", "<br />");
			AssertEquals("Table with HTML Encoding (Default behaviour)", expectedTableWithHtmlEncoding, creatorWithEncoding.ToHtml());

			var creatorWithoutEncoding = new HtmlTableCreator();
			creatorWithoutEncoding.EnableHTMLEncoding = false;
			creatorWithoutEncoding.WriteRow("<br />", "<br />");
			AssertEquals("Table without HTML Encoding (Overridden behavior)", expectedTableWithoutHtmlEncoding, creatorWithoutEncoding.ToHtml());
		}

		public void TestReplaceNewLineSymbolsWithBR()
		{
			var expectedHtml = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>Row 1<br>Row2<br>Row3</td></tr></table>";
			var creator = new HtmlTableCreator();
			creator.EnableHTMLEncoding = false;
			var cell = new CellWithFormatting("Row 1\r\nRow2\r\nRow3");
			creator.WriteRowWithFormatting(new CellWithFormatting[] { cell });

			AssertEquals("New line symbols were replaced by HTML tags", expectedHtml, creator.ToHtml());
		}
	}
}
