using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.FormatTables.Testing
{
	sealed class FormatColumnTest : TestCase
	{
		public void TestEnumerator()
		{
			string[] inList = new string[] { "One", "Two", "Three" };
			string[] outList = new List<string>(new FormatColumn(inList, 1)).ToArray();

			AssertArrayEqualsByElements(inList, outList);
		}

		public void TestLeftPadding()
		{
			FormatColumn column = new FormatColumn(Array.Empty<string>(), 50);

			column.LeftPadding = 5;
			AssertEquals(5, column.LeftPadding);

			column.LeftPadding = -1;
			AssertEquals(0, column.LeftPadding);
		}

		public void TestRightPadding()
		{
			FormatColumn column = new FormatColumn(Array.Empty<string>(), 50);

			column.RightPadding = 5;
			AssertEquals(5, column.RightPadding);

			column.RightPadding = 0;
			AssertEquals(0, column.RightPadding);
		}

		public void TestOuterWidth()
		{
			FormatColumn column = new FormatColumn(Array.Empty<string>(), 50);
			column.LeftPadding = 1;
			column.RightPadding = 2;

			AssertEquals(53, column.OuterWidth);
		}
	}
}
