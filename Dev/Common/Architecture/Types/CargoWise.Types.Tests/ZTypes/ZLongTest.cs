using System;
using System.Globalization;

namespace CargoWise.Types.Tests
{
	public class ZLongTest : IZTypeTest
	{
		public void TestNullConstructor()
		{
			AssertEquals(ZLong.Zero, new ZLong(null));
		}

		public void TestConstructorFailsOnDecimalPrecisionLoss()
		{
			try
			{
				new ZLong(6.2m);
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
				new ZLong(new ZDecimal(6.2m));
				Fail("Did not throw expected exception");
			}
			catch (ZTypeValueException)
			{
				Assert(true);
			}
		}

		public void TestToZLong()
		{
			ZLong i = 65;
			AssertEquals(new ZLong(65), i);
		}

		public void TestCastingZShort()
		{
			ZShort actualZShort = 1;
			ZLong expected = 1;
			ZLong actualZLong = actualZShort;

			AssertEquals("Explicit cast", expected, (ZLong)actualZShort);
			AssertEquals("Implicit cast", expected, actualZLong);
		}

		public void TestPlusPlusOperator()
		{
			long i = 0;
			i++;
			ZLong z = 0;
			z++;

			AssertEquals("ZLong Z = 0;   Z++;   -   Z should be 1.", 1, z);
			Assert("ZLong++ does not match long++ operator.", z == i);
		}

		public void TestMinusMinusOperator()
		{
			long i = 3;
			i--;
			ZLong z = 3;
			z--;
			AssertEquals("ZLong Z = 3;	Z--;	Z should be 2", 2, z);
			Assert("ZLong-- does not match long-- operator.", z == i);
		}

		public void TestGreaterThanOperator()
		{
			ZLong lhs = 2;
			ZLong rhs = 1;

			Assert(lhs > rhs);
		}

		public void TestGreaterThanOrEqualOperator()
		{
			ZLong lhs = 2;
			ZLong rhs = 2;

			Assert(lhs >= rhs);
		}

		public void TestLessThanOperator()
		{
			ZLong lhs = 1;
			ZLong rhs = 2;

			Assert(lhs < rhs);
		}

		public void TestLessThanOrEqualOperator()
		{
			ZLong lhs = 2;
			ZLong rhs = 2;

			Assert(lhs <= rhs);
		}

		public void TestEqualOperator()
		{
			ZLong lhs = 2;
			ZLong rhs = 2;

			AssertEquals(true, lhs == rhs);

			lhs = 1;

			AssertEquals(false, lhs == rhs);
		}

		public void TestNotEqualOperator()
		{
			ZLong lhs = 2;
			ZLong rhs = 2;

			AssertEquals(false, lhs != rhs);

			lhs = 1;

			AssertEquals(true, lhs != rhs);
		}

		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>54</a>", new ZLong(54));
		}

		public void TestZero()
		{
			long zero = ZLong.Zero;
			AssertEquals(0, zero);
		}

		public void TestExplicitCastToZShort()
		{
			ZLong value = new ZLong(10);
			AssertEquals(new ZShort((short)10), (ZShort)value);
			value = new ZLong(-10);
			AssertEquals(new ZShort(-10), (ZShort)value);
		}

		public void TestCastToZDecimal()
		{
			ZLong i = 1;
			AssertEquals("ZLong should cast to ZDecimal properly.", new ZDecimal(1), (ZDecimal)i);
		}

		public void TestToStringWithFormatProvider()
		{
			CombineAssertions("With ES Number Format (1.000)", () =>
			{
				var esNumberFormat = CultureInfo.GetCultureInfo("es-ES").NumberFormat;

				AssertEquals("100", new ZLong(100).ToString("N0", esNumberFormat));
				AssertEquals("1.000", new ZLong(1000).ToString("N0", esNumberFormat));
				AssertEquals("1.000.000", new ZLong(1000000).ToString("N0", esNumberFormat));
				AssertEquals("-1.000", new ZLong(-1000).ToString("N0", esNumberFormat));
			});

			CombineAssertions("With FR Number Format (1 000)", () =>
			{
				var frNumberFormat = CultureInfo.GetCultureInfo("fr-FR").NumberFormat;

				AssertEquals("100", new ZLong(100).ToString("N0", frNumberFormat));
				AssertEquals("1 000", new ZLong(1000).ToString("N0", frNumberFormat));
				AssertEquals("1 000 000", new ZLong(1000000).ToString("N0", frNumberFormat));
				AssertEquals("-1 000", new ZLong(-1000).ToString("N0", frNumberFormat));
			});

			CombineAssertions("With US Number Format (1,000)", () =>
			{
				var usNumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

				AssertEquals("100", new ZLong(100).ToString("N0", usNumberFormat));
				AssertEquals("1,000", new ZLong(1000).ToString("N0", usNumberFormat));
				AssertEquals("1,000,000", new ZLong(1000000).ToString("N0", usNumberFormat));
				AssertEquals("-1,000", new ZLong(-1000).ToString("N0", usNumberFormat));
			});
		}

		#region Parsing Tests

		public void TestTryParse()
		{
			AssertTryParse(long.MinValue.ToString(), true, long.MinValue);
			AssertTryParse("100", true, 100);
			AssertTryParse(long.MaxValue.ToString(), true, long.MaxValue);

			AssertTryParse((((double)long.MinValue) - 1).ToString(), false, 0);
			AssertTryParse((((double)long.MaxValue) + 1).ToString(), false, 0);
			AssertTryParse("some junk", false, 0);
		}

		public void TestParse()
		{
			AssertEquals("Parse() result.", long.MinValue, ZLong.Parse(long.MinValue.ToString()));
			AssertEquals("Parse() result.", 100, ZLong.Parse("100"));
			AssertEquals("Parse() result.", long.MaxValue, ZLong.Parse(long.MaxValue.ToString()));

			AssertBadParseThrowsException((((double)long.MinValue) - 1).ToString());
			AssertBadParseThrowsException((((double)long.MaxValue) + 1).ToString());
			AssertBadParseThrowsException("some junk");
		}

		public void TestParseSafe()
		{
			foreach (string stringValue in new string[] { "37", "0", "-12" })
			{
				AssertEquals(stringValue + " as string", long.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZLong.ParseSafe(stringValue, -1));
				AssertEquals(stringValue + " as ZString", long.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZLong.ParseSafe(new ZString(stringValue), -1));
			}
			foreach (string stringValue in new string[] { "", "NotNumber!" })
			{
				AssertEquals(stringValue + " as string", new ZLong(-1), ZLong.ParseSafe(stringValue, -1));
				AssertEquals(stringValue + " as ZString", new ZLong(-1), ZLong.ParseSafe(new ZString(stringValue), -1));
			}
		}

		void AssertBadParseThrowsException(string value)
		{
			bool exceptionWasThrown = false;

			try
			{
				ZLong.Parse(value);
			}
			catch (Exception)
			{
				exceptionWasThrown = true;
			}

			Assert(exceptionWasThrown);
		}

		void AssertTryParse(string value, bool expectSuccess, long expectedResult)
		{
			bool success = ZLong.TryParse(value, out var result);
			AssertEquals("TryParse success.", expectSuccess, success);
			AssertEquals("TryParse result.", expectedResult, result);
		}

		#endregion

		#region IZTypeTest Overrides

		protected override IZType NewZ(object value)
		{
			return new ZLong(value);
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { DBNull.Value, new ZLong(), 0, (long)0, (short)0, (byte)0, null }; }
		}

		protected override object[] ValidValues
		{
			get { return new object[] { (long)1, -1, 1, null, (short)1, (byte)1, long.MinValue, long.MaxValue }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0.0, 0M }; }
		}

		#endregion
	}
}
