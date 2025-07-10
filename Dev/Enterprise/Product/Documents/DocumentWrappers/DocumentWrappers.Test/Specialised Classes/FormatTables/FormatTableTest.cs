using System.Text;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.FormatTables.Testing
{
	sealed class FormatTableTest : TestCase
	{
		public void TestIncludeHeadingInBody()
		{
			FormatColumn[] columns = System.Array.Empty<FormatColumn>();

			AssertEquals(true, new FormatTable(true, columns).IncludeHeadingInBody);
			AssertEquals(false, new FormatTable(false, columns).IncludeHeadingInBody);
		}

		public void TestSuppressColumnIfInnerWidthIsZero()
		{
			const string expectedResult =
				"C1R1  C3R1\n" +
				"C1R2  C3R2\n" +
				"C1R3  C3R3\n" +
				"";

			FormatTable table = new FormatTable(false,
				new FormatColumn(new string[] { "C1R1", "C1R2", "C1R3" }, 4),
				new FormatColumn(new string[] { "C2R1", "C2R2", "C2R3" }, 0) { LeftPadding = 1 },
				new FormatColumn(new string[] { "C3R1", "C3R2", "C3R3" }, 4) { LeftPadding = 1 }
			);

			AssertResultBody(expectedResult, table);
		}

		public void TestSeparateRows()
		{
			const string column1row1 = "Some random text that is long enough to require wrapping";
			const string column2row1 = "Foo\nBar";
			const string column3row1 = "1000\n400";

			const string column1row2 = "Another chunck of text that needs to be wrapped";
			const string column2row2 = "Baz\n\nGaz";
			const string column3row2 = "950";

			const string expectedResult =
				"Some random text       Foo   1000\n" +
				"that is long enough    Bar    400\n" +
				"to require wrapping              \n" +
				"Another chunck of      Baz    950\n" +
				"text that needs to               \n" +
				"be wrapped             Gaz       \n" +
				"";

			FormatTable table = new FormatTable(false,
				new FormatColumn(new string[] { column1row1, column1row2 }, 20),
				new FormatColumn(new string[] { column2row1, column2row2 }, 5)
				{
					LeftPadding = 3,
				},
				new FormatColumn(new string[] { column3row1, column3row2 }, 4)
				{
					LeftPadding = 1,
					Options = FormatColumnOptions.RightAlign,
				}
			);

			AssertResultBody(expectedResult, table);
		}

		public void TestAlignment()
		{
			const string column1 = "LLLLL\nLLL\nL";
			const string column2 = "CCCCC\nCCC\nC";
			const string column3 = "RRRRR\nRRR\nR";

			const string expectedResult =
				"LLLLLCCCCCRRRRR\n" +
				"LLL   CCC   RRR\n" +
				"L      C      R\n" +
				"";

			FormatTable table = new FormatTable(false,
				new FormatColumn(new string[] { column1 }, 5)
				{
					Options = FormatColumnOptions.LeftAlign,
				},
				new FormatColumn(new string[] { column2 }, 5)
				{
					Options = FormatColumnOptions.CenterAlign,
				},
				new FormatColumn(new string[] { column3 }, 5)
				{
					Options = FormatColumnOptions.RightAlign,
				}
			);

			AssertResultBody(expectedResult, table);
		}

		public void TestCalculateTotalWidth()
		{
			FormatTable table = new FormatTable(false,
				new FormatColumn(System.Array.Empty<string>(), 5),
				new FormatColumn(System.Array.Empty<string>(), 5)
				{
					LeftPadding = 1
				},
				new FormatColumn(System.Array.Empty<string>(), 5)
				{
					LeftPadding = 2,
					RightPadding = 3,
				}
			);

			AssertEquals("TotalWidth", 21, table.CalculateTotalWidth());
		}

		public void TestHeadings()
		{
			FormatTable table = new FormatTable(false,
				new FormatColumn(new string[] { "value0" }, 6),
				new FormatColumn(new string[] { "value1" }, 6)
				{
					LeftPadding = 1,
					Heading = "Short",
				},
				new FormatColumn(new string[] { "value2" }, 6)
				{
					LeftPadding = 1,
					Heading = "Long Heading"
				}
			);

			const string expectedHeading =
				"       Short  Long  \n" +
				"              Headin\n" +
				"              g     \n" +
				"";

			const string expectedBody =
				"value0 value1 value2\n" +
				"";

			AssertResultHeading(expectedHeading, table);
			AssertResultBody(expectedBody, table);
		}

		#region Implementation

		void AssertResultBody(string expectedText, FormatTable table)
		{
			StringBuilder builder = new StringBuilder();

			foreach (string line in table.Body)
			{
				builder.AppendLine(line);
			}

			AssertMultilineASCIIEquals("", expectedText, builder.ToString());
		}

		void AssertResultHeading(string expectedText, FormatTable table)
		{
			StringBuilder builder = new StringBuilder();

			foreach (string line in table.Heading)
			{
				builder.AppendLine(line);
			}

			AssertMultilineASCIIEquals("", expectedText, builder.ToString());
		}

		#endregion
	}
}
