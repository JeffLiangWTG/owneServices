using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPERecordBaseTest : TestCase
	{
		public void TestToZDateTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZDateTime.Empty, recordLine.ToZDateTime(""));
				AssertEquals("Whitespace", ZDateTime.Empty, recordLine.ToZDateTime("    "));
				AssertEquals("Valid", new ZDateTime(2005, 6, 23), recordLine.ToZDateTime("23JUN2005"));
			});
		}

		public void TestToZInt()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", 0, recordLine.ToZInt(""));
				AssertEquals("Whitespace", 0, recordLine.ToZInt("    "));
				AssertEquals("Numeric", 9, recordLine.ToZInt("9"));
				AssertEquals("Trailing spaces", 9, recordLine.ToZInt("9  "));
				AssertEquals("Leading and Trailing spaces", 9, recordLine.ToZInt("  9  "));
			});
		}

		protected void TestToZBool()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Yes", true, recordLine.ToZBool("Y"));
				AssertEquals("Space", false, recordLine.ToZBool(" "));
				AssertEquals("No", false, recordLine.ToZBool("N"));
			});
		}

		public void TestToZDecimal()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", 0m, recordLine.ToZDecimal(""));
				AssertEquals("Whitespace", 0m, recordLine.ToZDecimal("    "));
				AssertEquals("Decimal", 9.23m, recordLine.ToZDecimal("9.23"));
				AssertEquals("Trailing spaces", 9.23m, recordLine.ToZDecimal("9.23  "));
				AssertEquals("Leading and Trailing spaces", 9.23m, recordLine.ToZDecimal("  9.23  "));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			recordLine = new UPERecordBaseForTest();
		}

		UPERecordBaseForTest recordLine;
		class UPERecordBaseForTest : UPERecordBase
		{
			public new ZDateTime ToZDateTime(string value) => base.ToZDateTime(value);
			public new ZBool ToZBool(string value) => base.ToZBool(value);
			public new ZInt ToZInt(string value) => base.ToZInt(value);
			public new ZDecimal ToZDecimal(string value) => base.ToZDecimal(value);
		}
	}
}
