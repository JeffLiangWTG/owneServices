using System;
using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public class ZIntTest : IZTypeTest
	{
		public void TestNullConstructor()
		{
			AssertEquals(ZInt.Zero, new ZInt(null));
		}

		public void TestConstructorFailsOnDecimalPrecisionLoss()
		{
			try
			{
				new ZInt(6.2m);
				Fail("Did not throw expected exception");
			}
			catch (ZTypeValueException)
			{
				Assert(true);
			}
		}

		public void TestConstructorFailsOnZDecimalPrecisionLoss()
		{
			try
			{
				new ZInt(new ZDecimal(6.2m));
				Fail("Did not throw expected exception");
			}
			catch (ZTypeValueException)
			{
				Assert(true);
			}
		}

		public void TestToZInt()
		{
			ZInt i = 65;
			AssertEquals(new ZInt(65), i);
		}

		public void TestCastingZShort()
		{
			ZShort actualZShort = 1;
			ZInt expected = 1;
			ZInt actualZInt = actualZShort;

			AssertEquals("Explicit cast", expected, (ZInt)actualZShort);
			AssertEquals("Implicit cast", expected, actualZInt);
		}

		public void TestPlusPlusOperator()
		{
			int i = 0;
			i++;
			ZInt z = 0;
			z++;

			AssertEquals("ZInt Z = 0;   Z++;   -   Z should be 1.", 1, z);
			Assert("ZInt++ does not match int++ operator.", z == i);
		}

		public void TestMinusMinusOperator()
		{
			int i = 3;
			i--;
			ZInt z = 3;
			z--;
			AssertEquals("ZInt Z = 3;	Z--;	Z should be 2", 2, z);
			Assert("ZInt-- does not match int-- operator.", z == i);
		}

		public void TestGreaterThanOperator()
		{
			ZInt lhs = 2;
			ZInt rhs = 1;

			Assert(lhs > rhs);
		}

		public void TestGreaterThanOrEqualOperator()
		{
			ZInt lhs = 2;
			ZInt rhs = 2;

			Assert(lhs >= rhs);
		}

		public void TestLessThanOperator()
		{
			ZInt lhs = 1;
			ZInt rhs = 2;

			Assert(lhs < rhs);
		}

		public void TestLessThanOrEqualOperator()
		{
			ZInt lhs = 2;
			ZInt rhs = 2;

			Assert(lhs <= rhs);
		}

		public void TestEqualOperator()
		{
			ZInt lhs = 2;
			ZInt rhs = 2;

			AssertEquals(true, lhs == rhs);

			lhs = 1;

			AssertEquals(false, lhs == rhs);
		}

		public void TestNotEqualOperator()
		{
			ZInt lhs = 2;
			ZInt rhs = 2;

			AssertEquals(false, lhs != rhs);

			lhs = 1;

			AssertEquals(true, lhs != rhs);
		}

		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>54</a>", new ZInt(54));
		}

		public void TestZero()
		{
			int zero = ZInt.Zero;
			AssertEquals(0, zero);
		}

		public void TestExplicitCastToZShort()
		{
			ZInt value = new ZInt(10);
			AssertEquals(new ZShort((short)10), (ZShort)value);
			value = new ZInt(-10);
			AssertEquals(new ZShort(-10), (ZShort)value);
		}

		public void TestCastToZDecimal()
		{
			ZInt i = 1;
			AssertEquals("ZInt should cast to ZDecimal properly.", new ZDecimal(1), (ZDecimal)i);
		}

		public void TestToStringWithFormatProvider()
		{
			CombineAssertions("With ES Number Format (1.000)", () =>
			{
				var esNumberFormat = CultureInfo.GetCultureInfo("es-ES").NumberFormat;

				AssertEquals("100", new ZInt(100).ToString("N0", esNumberFormat));
				AssertEquals("1.000", new ZInt(1000).ToString("N0", esNumberFormat));
				AssertEquals("1.000.000", new ZInt(1000000).ToString("N0", esNumberFormat));
				AssertEquals("-1.000", new ZInt(-1000).ToString("N0", esNumberFormat));
			});

			CombineAssertions("With FR Number Format (1 000)", () =>
			{
				var frNumberFormat = CultureInfo.GetCultureInfo("fr-FR").NumberFormat;

				AssertEquals("100", new ZInt(100).ToString("N0", frNumberFormat));
				AssertEquals("1 000", new ZInt(1000).ToString("N0", frNumberFormat));
				AssertEquals("1 000 000", new ZInt(1000000).ToString("N0", frNumberFormat));
				AssertEquals("-1 000", new ZInt(-1000).ToString("N0", frNumberFormat));
			});

			CombineAssertions("With US Number Format (1,000)", () =>
			{
				var usNumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

				AssertEquals("100", new ZInt(100).ToString("N0", usNumberFormat));
				AssertEquals("1,000", new ZInt(1000).ToString("N0", usNumberFormat));
				AssertEquals("1,000,000", new ZInt(1000000).ToString("N0", usNumberFormat));
				AssertEquals("-1,000", new ZInt(-1000).ToString("N0", usNumberFormat));
			});
		}

		#region Date/time Offset Conversion

		[TestDate(2015, 7, 14)]
		public void TestGetDateTimeFromMinutes()
		{
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(0), new ZInt(0).GetDateTimeFromMinutes());
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(10), new ZInt(10).GetDateTimeFromMinutes());
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(1440), new ZInt(1440).GetDateTimeFromMinutes());
		}

		#endregion

		#region Parsing Tests

		public void TestTryParse()
		{
			AssertTryParse(int.MinValue.ToString(), true, int.MinValue);
			AssertTryParse("100", true, 100);
			AssertTryParse(int.MaxValue.ToString(), true, int.MaxValue);

			AssertTryParse((((long)int.MinValue) - 1).ToString(), false, 0);
			AssertTryParse((((long)int.MaxValue) + 1).ToString(), false, 0);
			AssertTryParse("some junk", false, 0);
		}

		public void TestParseEmptyAsZero()
		{
			AssertEquals(ZInt.Zero, ZInt.ParseEmptyAsZero(""));
			AssertEquals(new ZInt(1), ZInt.ParseEmptyAsZero("1"));
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ ZInt.ParseEmptyAsZero("X"); });
		}

		public void TestCanParse()
		{
			AssertEquals("CanParse() result.", true, ZInt.CanParse(int.MinValue.ToString()));
			AssertEquals("CanParse() result.", true, ZInt.CanParse("100"));
			AssertEquals("CanParse() result.", true, ZInt.CanParse(int.MaxValue.ToString()));

			AssertEquals("CanParse() result.", false, ZInt.CanParse((((long)int.MinValue) - 1).ToString()));
			AssertEquals("CanParse() result.", false, ZInt.CanParse((((long)int.MaxValue) + 1).ToString()));
			AssertEquals("CanParse() result.", false, ZInt.CanParse("some junk"));
		}

		public void TestParse()
		{
			AssertEquals("Parse() result.", int.MinValue, ZInt.Parse(int.MinValue.ToString()));
			AssertEquals("Parse() result.", 100, ZInt.Parse("100"));
			AssertEquals("Parse() result.", int.MaxValue, ZInt.Parse(int.MaxValue.ToString()));

			AssertBadParseThrowsException((((long)int.MinValue) - 1).ToString());
			AssertBadParseThrowsException((((long)int.MaxValue) + 1).ToString());
			AssertBadParseThrowsException("some junk");
		}

		public void TestParseSafe()
		{
			foreach (string stringValue in new string[] { "37", "0", "-12" })
			{
				AssertEquals(stringValue + " as string", int.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZInt.ParseSafe(stringValue, -1));
				AssertEquals(stringValue + " as ZString", int.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZInt.ParseSafe(new ZString(stringValue), -1));
			}
			foreach (string stringValue in new string[] { "", "NotNumber!" })
			{
				AssertEquals(stringValue + " as string", new ZInt(-1), ZInt.ParseSafe(stringValue, -1));
				AssertEquals(stringValue + " as ZString", new ZInt(-1), ZInt.ParseSafe(new ZString(stringValue), -1));
			}
		}

		public void TestParseThrowsExceptionIfCanParseFails()
		{
			string badValue1 = (((long)int.MinValue) - 1).ToString();
			string badValue2 = (((long)int.MaxValue) + 1).ToString();
			string badValue3 = "some junk";

			AssertEquals("CanParse() result.", false, ZInt.CanParse(badValue1));
			AssertBadParseThrowsException(badValue1);

			AssertEquals("CanParse() result.", false, ZInt.CanParse(badValue2));
			AssertBadParseThrowsException(badValue2);

			AssertEquals("CanParse() result.", false, ZInt.CanParse(badValue3));
			AssertBadParseThrowsException(badValue3);
		}

		void AssertBadParseThrowsException(string value)
		{
			bool exceptionWasThrown = false;

			try
			{
				ZInt.Parse(value);
			}
			catch (Exception)
			{
				exceptionWasThrown = true;
			}

			Assert(exceptionWasThrown);
		}

		void AssertTryParse(string value, bool expectSuccess, int expectedResult)
		{
			bool success = ZInt.TryParse(value, out var result);
			AssertEquals("TryParse success.", expectSuccess, success);
			AssertEquals("TryParse result.", expectedResult, result);
		}

		#endregion

		#region IZTypeTest Overrides

		protected override IZType NewZ(object value)
		{
			return new ZInt(value);
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { DBNull.Value, new ZInt(), 0, (short)0, (byte)0, null }; }
		}

		protected override object[] ValidValues
		{
			get { return new object[] { -1, 1, null, (short)1, (byte)1, int.MinValue, int.MaxValue }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0.0, 0M }; }
		}

		#endregion
	}
}
