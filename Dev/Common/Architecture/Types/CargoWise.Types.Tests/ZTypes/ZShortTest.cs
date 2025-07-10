using System;
using System.Globalization;

namespace CargoWise.Types.Tests
{
	public class ZShortTest : IZTypeTest
	{
		public void TestNullConstructor()
		{
			AssertEquals(ZShort.Zero, new ZShort(null));
		}

		public void TestToZShort()
		{
			ZShort s = 65;
			AssertEquals(new ZInt(65), s);
		}

		public void TestGreaterThanOperator()
		{
			ZShort lhs = 2;
			ZShort rhs = 1;

			Assert(lhs > rhs);
		}

		public void TestGreaterThanOrEqualOperator()
		{
			ZShort lhs = 2;
			ZShort rhs = 2;

			Assert(lhs >= rhs);

			rhs = 1;

			Assert(lhs >= rhs);
		}

		public void TestLessThanOperator()
		{
			ZShort lhs = 1;
			ZShort rhs = 2;

			Assert(lhs < rhs);
		}

		public void TestLessThanOrEqualOperator()
		{
			ZShort lhs = 2;
			ZShort rhs = 2;

			Assert(lhs <= rhs);

			lhs = 1;

			Assert(lhs <= rhs);
		}

		public void TestEqualOperator()
		{
			ZShort lhs = 2;
			ZShort rhs = 2;

			AssertEquals(true, lhs == rhs);

			lhs = 1;

			AssertEquals(false, lhs == rhs);
		}

		public void TestNotEqualOperator()
		{
			ZShort lhs = 2;
			ZShort rhs = 2;

			AssertEquals(false, lhs != rhs);

			lhs = 1;

			AssertEquals(true, lhs != rhs);
		}

		public void TestPlusPlusOperator()
		{
			short s = 0;
			s++;
			ZShort z = 0;
			z++;

			AssertEquals("ZShort Z = 0;   Z++;   -   Z should be 1.", (byte)1, z);
			Assert("ZShort++ does not match short++ operator.", z == s);
		}

		public void TestMinusMinusOperator()
		{
			short s = 1;
			s--;
			ZShort z = 1;
			z--;

			AssertEquals("ZShort Z = 1;   Z--;   -   Z should be 0.", (byte)0, z);
			Assert("ZShort-- does not match short-- operator.", z == s);
		}

		public void TestToStringWithFormatProvider()
		{
			CombineAssertions("With ES Number Format (1.000)", () =>
			{
				var esNumberFormat = CultureInfo.GetCultureInfo("es-ES").NumberFormat;

				AssertEquals("100", new ZShort(100).ToString("N0", esNumberFormat));
				AssertEquals("1.000", new ZShort(1000).ToString("N0", esNumberFormat));
				AssertEquals("10.000", new ZShort(10000).ToString("N0", esNumberFormat));
				AssertEquals("-1.000", new ZShort(-1000).ToString("N0", esNumberFormat));
			});

			CombineAssertions("With FR Number Format (1 000)", () =>
			{
				var frNumberFormat = CultureInfo.GetCultureInfo("fr-FR").NumberFormat;

				AssertEquals("100", new ZShort(100).ToString("N0", frNumberFormat));
				AssertEquals("1 000", new ZShort(1000).ToString("N0", frNumberFormat));
				AssertEquals("10 000", new ZShort(10000).ToString("N0", frNumberFormat));
				AssertEquals("-1 000", new ZShort(-1000).ToString("N0", frNumberFormat));
			});

			CombineAssertions("With US Number Format (1,000)", () =>
			{
				var usNumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

				AssertEquals("100", new ZShort(100).ToString("N0", usNumberFormat));
				AssertEquals("1,000", new ZShort(1000).ToString("N0", usNumberFormat));
				AssertEquals("10,000", new ZShort(10000).ToString("N0", usNumberFormat));
				AssertEquals("-1,000", new ZShort(-1000).ToString("N0", usNumberFormat));
			});
		}

		public void TestTryParse()
		{
			AssertTryParse(short.MinValue.ToString(), true, short.MinValue);
			AssertTryParse("100", true, 100);
			AssertTryParse(short.MaxValue.ToString(), true, short.MaxValue);

			AssertTryParse((short.MaxValue + 1).ToString(), false, 0);
			AssertTryParse((short.MinValue - 1).ToString(), false, 0);

			AssertTryParse("some junk", false, 0);
		}

		void AssertTryParse(string value, bool expectSuccess, short expectedResult)
		{
			bool success = ZShort.TryParse(value, out var result);
			AssertEquals("TryParse success.", expectSuccess, success);
			AssertEquals("TryParse result.", expectedResult, result);
		}

		public void TestZero()
		{
			AssertEquals(new ZShort((short)0), ZShort.Zero);
		}

		public void TestParse()
		{
			AssertEquals(new ZShort((short)123), ZShort.Parse("123"));
		}

		public void TestParseSafe()
		{
			foreach (string stringValue in new string[] { "37", "0", "-12" })
			{
				AssertEquals(stringValue + " as string", short.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZShort.ParseSafe(stringValue, -1));
				AssertEquals(stringValue + " as ZString", short.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZShort.ParseSafe(new ZString(stringValue), -1));
			}
			foreach (string stringValue in new string[] { "", "NotNumber!" })
			{
				AssertEquals(stringValue + " as string", new ZInt(-1), ZShort.ParseSafe(stringValue, -1));
				AssertEquals(stringValue + " as ZString", new ZInt(-1), ZShort.ParseSafe(new ZString(stringValue), -1));
			}
		}

		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>54</a>", new ZShort((short)54));
		}

		public void TestCastToZDecimal()
		{
			ZShort s = 1;
			AssertEquals("ZShort should cast to ZDecimal properly.", new ZDecimal(1), (ZDecimal)s);
		}

		#region IZTypeTest Overrides

		protected override IZType NewZ(object value)
		{
			return new ZShort(value);
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { DBNull.Value, new ZShort(), (short)0, (byte)0 }; }
		}

		protected override object[] ValidValues
		{
			get { return new object[] { (short)-1, (short)1, (byte)1, short.MinValue, short.MaxValue }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object(), 0.0, 0M, 0 }; }
		}

		#endregion
	}
}
