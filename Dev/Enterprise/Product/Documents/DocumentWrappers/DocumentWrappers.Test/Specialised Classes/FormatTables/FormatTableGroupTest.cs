using System.Text;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.FormatTables.Testing
{
	sealed class FormatTableGroupTest : TestCase
	{
		public void TestEmpty()
		{
			FormatTableGroup group = new FormatTableGroup();

			AssertEquals(false, group.IncludeHeadingInBody);
			AssertResultHeading("", group);
			AssertResultBody("", group);
		}

		public void TestHeader()
		{
			FormatTable table1 = new FormatTable(true,
				new FormatColumn(new string[] { "Line 1" }, 20) { Heading = "Col1" },
				new FormatColumn(new string[] { "A" }, 10) { Heading = "Col2" }
				);

			FormatTable table2 = new FormatTable(true,
				new FormatColumn(new string[] { "Line 2" }, 20),
				new FormatColumn(new string[] { "B" }, 10)
				);

			FormatTable table3 = new FormatTable(true,
				new FormatColumn(new string[] { "Line 3" }, 20),
				new FormatColumn(new string[] { "C" }, 10)
				);

			FormatTable table1sub = new FormatTable(true,
				new FormatColumn("Sub1.1|Sub1.2|Sub1.3".Split('|'), 25) { LeftPadding = 5 }
				);

			FormatTable table2sub = new FormatTable(true,
				new FormatColumn("Sub2.1|Sub2.2|Sub2.3".Split('|'), 25) { LeftPadding = 5 }
				);

			FormatTable table3sub = new FormatTable(true,
				new FormatColumn("Sub3.1|Sub3.2|Sub3.3".Split('|'), 25) { LeftPadding = 5 }
				);

			FormatTableGroup group = new FormatTableGroup(
				table1, table1sub,
				table2, table2sub,
				table3, table3sub
				);

			const string expectedHeading =
				"Col1                Col2      \n" +
				"";

			const string expectedBody =
				"Line 1              A         \n" +
				"     Sub1.1                   \n" +
				"     Sub1.2                   \n" +
				"     Sub1.3                   \n" +
				"Line 2              B         \n" +
				"     Sub2.1                   \n" +
				"     Sub2.2                   \n" +
				"     Sub2.3                   \n" +
				"Line 3              C         \n" +
				"     Sub3.1                   \n" +
				"     Sub3.2                   \n" +
				"     Sub3.3                   \n" +
				"";

			AssertEquals(true, group.IncludeHeadingInBody);
			AssertResultHeading(expectedHeading, group);
			AssertResultBody(expectedBody, group);
		}

		#region Implementation

		void AssertResultBody(string expectedText, FormatTableGroup group)
		{
			StringBuilder builder = new StringBuilder();

			foreach (string line in group.Body)
			{
				builder.AppendLine(line);
			}

			AssertMultilineASCIIEquals("", expectedText, builder.ToString());
		}

		void AssertResultHeading(string expectedText, FormatTableGroup group)
		{
			StringBuilder builder = new StringBuilder();

			foreach (string line in group.Heading)
			{
				builder.AppendLine(line);
			}

			AssertMultilineASCIIEquals("", expectedText, builder.ToString());
		}

		#endregion
	}
}
