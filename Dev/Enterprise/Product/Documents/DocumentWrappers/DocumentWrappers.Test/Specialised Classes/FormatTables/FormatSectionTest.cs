using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.FormatTables.Testing
{
	sealed class FormatSectionTest : TestCase
	{
		public void TestFlow_NoHeadingsInBody()
		{
			FormatSection section = new FormatSection(15, new FormatTable[]
			{
				new FormatTable(false, new FormatColumn(GetLines("C1L{0:00}", 10), 5) { Heading = "Col1" }),
				new FormatTable(false, new FormatColumn(GetLines("C2L{0:00}", 10), 5) { Heading = "Col2" }),
				new FormatTable(false, new FormatColumn(GetLines("C3L{0:00}", 10), 5) { Heading = "Col3" }),
				new FormatTable(false, new FormatColumn(GetLines("C4L{0:00}", 10), 5) { Heading = "Col4" }),
			});

			const string expectedWindow1 =
				"C1L00\r\n" +
				"C1L01\r\n" +
				"C1L02\r\n" +
				"C1L03\r\n" +
				"C1L04\r\n" +
				"C1L05\r\n" +
				"C1L06\r\n" +
				"C1L07\r\n" +
				"C1L08\r\n" +
				"C1L09\r\n" +
				"\r\n" +
				"C2L00\r\n" +
				"C2L01\r\n" +
				"C2L02\r\n" +
				"C2L03\r\n" +
				"";

			const string expectedWindow2 =
				"C2L04\r\n" +
				"C2L05\r\n" +
				"C2L06\r\n" +
				"C2L07\r\n" +
				"C2L08\r\n" +
				"C2L09\r\n" +
				"\r\n" +
				"C3L00\r\n" +
				"C3L01\r\n" +
				"C3L02\r\n" +
				"C3L03\r\n" +
				"C3L04\r\n" +
				"C3L05\r\n" +
				"C3L06\r\n" +
				"C3L07\r\n" +
				"";

			const string expectedFollowOn =
				"Col3 \r\n" +
				"C3L08\r\n" +
				"C3L09\r\n" +
				"\r\n" +
				"Col4 \r\n" +
				"C4L00\r\n" +
				"C4L01\r\n" +
				"C4L02\r\n" +
				"C4L03\r\n" +
				"C4L04\r\n" +
				"C4L05\r\n" +
				"C4L06\r\n" +
				"C4L07\r\n" +
				"C4L08\r\n" +
				"C4L09\r\n" +
				"";

			AssertMultilineASCIIEquals("Window1", expectedWindow1, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Window2", expectedWindow2, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Follow On", expectedFollowOn, GetString(section.ReadFollowOn()));
		}

		public void TestFlow_HeadingsInBody()
		{
			FormatSection section = new FormatSection(15, new FormatTable[]
			{
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C1L{0:00}", 10), 5) { Heading = "Col1" } }),
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C2L{0:00}", 10), 5) { Heading = "Col2" } }),
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C3L{0:00}", 10), 5) { Heading = "Col3" } }),
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C4L{0:00}", 10), 5) { Heading = "Col4" } }),
			});

			const string expectedWindow1 =
				"Col1 \r\n" +
				"C1L00\r\n" +
				"C1L01\r\n" +
				"C1L02\r\n" +
				"C1L03\r\n" +
				"C1L04\r\n" +
				"C1L05\r\n" +
				"C1L06\r\n" +
				"C1L07\r\n" +
				"C1L08\r\n" +
				"C1L09\r\n" +
				"\r\n" +
				"Col2 \r\n" +
				"C2L00\r\n" +
				"C2L01\r\n" +
				"";

			const string expectedWindow2 =
				"Col2 \r\n" +
				"C2L02\r\n" +
				"C2L03\r\n" +
				"C2L04\r\n" +
				"C2L05\r\n" +
				"C2L06\r\n" +
				"C2L07\r\n" +
				"C2L08\r\n" +
				"C2L09\r\n" +
				"\r\n" +
				"Col3 \r\n" +
				"C3L00\r\n" +
				"C3L01\r\n" +
				"C3L02\r\n" +
				"C3L03\r\n" +
				"";

			const string expectedFollowIn =
				"Col3 \r\n" +
				"C3L04\r\n" +
				"C3L05\r\n" +
				"C3L06\r\n" +
				"C3L07\r\n" +
				"C3L08\r\n" +
				"C3L09\r\n" +
				"\r\n" +
				"Col4 \r\n" +
				"C4L00\r\n" +
				"C4L01\r\n" +
				"C4L02\r\n" +
				"C4L03\r\n" +
				"C4L04\r\n" +
				"C4L05\r\n" +
				"C4L06\r\n" +
				"C4L07\r\n" +
				"C4L08\r\n" +
				"C4L09\r\n" +
				"";

			AssertMultilineASCIIEquals("Window1", expectedWindow1, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Window2", expectedWindow2, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Follow On", expectedFollowIn, GetString(section.ReadFollowOn()));
		}

		public void TestCaptionOverflow()
		{
			FormatTable[] tables =
			{
				new FormatTable(true, new FormatColumn(GetLines("C1L{0:00}", 10), 6) { Heading = "Col1.1\r\nCol1.2" }),
				new FormatTable(true, new FormatColumn(GetLines("C2L{0:00}", 10), 6) { Heading = "Col2.1\r\nCol2.2" }),
			};

			const string caption1 =
				"Col1.1\r\n" +
				"Col1.2\r\n" +
				"";

			const string caption2 =
				"Col2.1\r\n" +
				"Col2.2\r\n" +
				"";

			const string body1 =
				"C1L00 \r\n" +
				"C1L01 \r\n" +
				"C1L02 \r\n" +
				"C1L03 \r\n" +
				"C1L04 \r\n" +
				"C1L05 \r\n" +
				"C1L06 \r\n" +
				"C1L07 \r\n" +
				"C1L08 \r\n" +
				"";

			const string body2 =
				"C1L09 \r\n" +
				"";

			const string body3 =
				"C2L00 \r\n" +
				"";

			const string body4 =
				"C2L01 \r\n" +
				"C2L02 \r\n" +
				"C2L03 \r\n" +
				"C2L04 \r\n" +
				"C2L05 \r\n" +
				"C2L06 \r\n" +
				"C2L07 \r\n" +
				"C2L08 \r\n" +
				"C2L09 \r\n" +
				"";

			const string spacer =
				"\r\n" +
				"";

			FormatSection section;

			// asserting that the spacer and caption for the second table will not be added unless there is also room for atleast one line of data

			// 2 + 9 = 11
			// 2 + 1 + 1 + 2 + 1 + 9 = 16
			section = new FormatSection(11, tables);
			AssertMultilineASCIIEquals("11) page1", caption1 + body1, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("11) followon", caption1 + body2 + spacer + caption2 + body3 + body4, GetString(section.ReadFollowOn()));

			// 2 + 9 + 1 = 12
			// 2 + 1 + 9 = 12
			section = new FormatSection(12, tables);
			AssertMultilineASCIIEquals("12) page1", caption1 + body1 + body2, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("12) followon", caption2 + body3 + body4, GetString(section.ReadFollowOn()));

			// 2 + 9 + 1 = 12
			// 2 + 1 + 9 = 12
			section = new FormatSection(15, tables);
			AssertMultilineASCIIEquals("15) page1", caption1 + body1 + body2, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("15) followon", caption2 + body3 + body4, GetString(section.ReadFollowOn()));

			// 2 + 9 + 1 + 1 + 2 + 1 = 16
			// 2 + 9 = 11
			section = new FormatSection(16, tables);
			AssertMultilineASCIIEquals("16) page1", caption1 + body1 + body2 + spacer + caption2 + body3, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("16) followon", caption2 + body4, GetString(section.ReadPage()));
		}

		public void TestSpacerOverflow()
		{
			FormatTable[] tables =
			{
				new FormatTable(false, new FormatColumn(GetLines("C1L{0:00}", 10), 6) { Heading = "Col1.1\r\nCol1.2" }),
				new FormatTable(false, new FormatColumn(GetLines("C2L{0:00}", 10), 6) { Heading = "Col2.1\r\nCol2.2" }),
			};

			const string body1 =
				"C1L00 \r\n" +
				"C1L01 \r\n" +
				"C1L02 \r\n" +
				"C1L03 \r\n" +
				"C1L04 \r\n" +
				"C1L05 \r\n" +
				"C1L06 \r\n" +
				"C1L07 \r\n" +
				"C1L08 \r\n" +
				"";

			const string body2 =
				"C1L09 \r\n" +
				"";

			const string body3 =
				"C2L00 \r\n" +
				"";

			const string body4 =
				"C2L01 \r\n" +
				"C2L02 \r\n" +
				"C2L03 \r\n" +
				"C2L04 \r\n" +
				"C2L05 \r\n" +
				"C2L06 \r\n" +
				"";

			const string body5 =
				"C2L07 \r\n" +
				"C2L08 \r\n" +
				"C2L09 \r\n" +
				"";

			const string spacer =
				"\r\n" +
				"";

			FormatSection section;

			// asserting that the spacer before the second table will not be added unless there is also room for atleast one line of data

			// 9 = 9
			// 1 + 1 + 1 + 6 = 9
			// 3 = 3
			section = new FormatSection(9, tables);
			AssertMultilineASCIIEquals("9) page1", body1, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("9) page2", body2 + spacer + body3 + body4, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("9) page3", body5, GetString(section.ReadPage()));

			// 9 + 1 = 10
			// 1 + 6 + 3 = 10
			section = new FormatSection(10, tables);
			AssertMultilineASCIIEquals("10) page1", body1 + body2, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("10) page2", body3 + body4 + body5, GetString(section.ReadPage()));

			// 9 + 1 = 10
			// 1 + 6 + 3 = 10
			section = new FormatSection(11, tables);
			AssertMultilineASCIIEquals("11) page1", body1 + body2, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("11) page2", body3 + body4 + body5, GetString(section.ReadPage()));

			// 9 + 1 + 1 + 1 = 12
			// 3 + 6 = 9
			section = new FormatSection(12, tables);
			AssertMultilineASCIIEquals("12) page1", body1 + body2 + spacer + body3, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("12) page2", body4 + body5, GetString(section.ReadPage()));
		}

		public void TestPageToEnd()
		{
			FormatSection section = new FormatSection(15, new FormatTable[]
			{
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C1L{0:00}", 10), 5) { Heading = "Col1" } }),
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C2L{0:00}", 10), 5) { Heading = "Col2" } }),
			});

			const string expectedWindow1 =
				"Col1 \r\n" +
				"C1L00\r\n" +
				"C1L01\r\n" +
				"C1L02\r\n" +
				"C1L03\r\n" +
				"C1L04\r\n" +
				"C1L05\r\n" +
				"C1L06\r\n" +
				"C1L07\r\n" +
				"C1L08\r\n" +
				"C1L09\r\n" +
				"\r\n" +
				"Col2 \r\n" +
				"C2L00\r\n" +
				"C2L01\r\n" +
				"";

			const string expectedWindow2 =
				"Col2 \r\n" +
				"C2L02\r\n" +
				"C2L03\r\n" +
				"C2L04\r\n" +
				"C2L05\r\n" +
				"C2L06\r\n" +
				"C2L07\r\n" +
				"C2L08\r\n" +
				"C2L09\r\n" +
				"";

			AssertMultilineASCIIEquals("Window1", expectedWindow1, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Window2", expectedWindow2, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Window3", null, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Follow On", null, GetString(section.ReadFollowOn()));
		}

		public void TestNoTables()
		{
			FormatSection section = new FormatSection(15, Array.Empty<FormatTable>());

			AssertMultilineASCIIEquals("Window1", null, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Follow On", null, GetString(section.ReadFollowOn()));
		}

		public void TestEmptyTables()
		{
			FormatSection section = new FormatSection(15, new FormatTable[]
			{
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C1L{0:00}", 0), 5) { Heading = "Col1" } }),
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C2L{0:00}", 10), 5) { Heading = "Col2" } }),
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C3L{0:00}", 0), 5) { Heading = "Col3" } }),
				new FormatTable(true, new FormatColumn[] { new FormatColumn(GetLines("C4L{0:00}", 10), 5) { Heading = "Col4" } }),
			});

			const string expectedWindow1 =
				"Col2 \r\n" +
				"C2L00\r\n" +
				"C2L01\r\n" +
				"C2L02\r\n" +
				"C2L03\r\n" +
				"C2L04\r\n" +
				"C2L05\r\n" +
				"C2L06\r\n" +
				"C2L07\r\n" +
				"C2L08\r\n" +
				"C2L09\r\n" +
				"\r\n" +
				"Col4 \r\n" +
				"C4L00\r\n" +
				"C4L01\r\n" +
				"";

			const string expectedWindow2 =
				"Col4 \r\n" +
				"C4L02\r\n" +
				"C4L03\r\n" +
				"C4L04\r\n" +
				"C4L05\r\n" +
				"C4L06\r\n" +
				"C4L07\r\n" +
				"C4L08\r\n" +
				"C4L09\r\n" +
				"";

			AssertMultilineASCIIEquals("Window1", expectedWindow1, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Window2", expectedWindow2, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Window3", null, GetString(section.ReadPage()));
			AssertMultilineASCIIEquals("Follow On", null, GetString(section.ReadFollowOn()));
		}

		#region Implementation

		static string GetString(List<string> lines)
		{
			if (lines == null)
			{
				return null;
			}
			else
			{
				StringBuilder builder = new StringBuilder();

				foreach (string line in lines)
				{
					builder.AppendLine(line);
				}
				return builder.ToString();
			}
		}

		static IEnumerable<string> GetLines(string format, int count)
		{
			for (int i = 0; i < count; i++)
			{
				yield return string.Format(format, i);
			}
		}

		#endregion
	}
}
