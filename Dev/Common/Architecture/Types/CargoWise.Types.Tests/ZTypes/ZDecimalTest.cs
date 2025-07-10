using System;
using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public class ZDecimalTest : IZTypeTest
	{
		public void TestNullConstructor()
		{
			AssertEquals(ZDecimal.Zero, new ZDecimal(null));
		}

		public void TestCast()
		{
			decimal someDecimal = 9.6m;
			ZDecimal someZDecimal = 1.1m;
			double someDouble = 10.5;
			float someFloat = 123.56f;
			int someInt = 10;
			ZInt someZInt = 13;

			ZDecimal result;

			result = (ZDecimal)someDecimal;
			AssertEquals(9.6m, result);

			result = someZDecimal;
			AssertEquals(1.1m, result);

			result = (ZDecimal)someDouble;
			AssertEquals(10.5m, result);

			result = (ZDecimal)someFloat;
			AssertEquals(123.56m, result);

			result = (ZDecimal)someInt;
			AssertEquals(10m, result);

			result = (ZDecimal)someZInt;
			AssertEquals(13m, result);
		}

		public void TestRound()
		{
			AssertEquals(new ZDecimal(5.12), new ZDecimal(5.124).Round(2));
			AssertEquals(new ZDecimal(5.13), new ZDecimal(5.125).Round(2));
			AssertEquals(new ZDecimal(5.5), new ZDecimal(5.45).Round(1));
			AssertEquals(new ZDecimal(5.13234234), new ZDecimal(5.132342341).Round(8));
			AssertEquals(new ZDecimal(6), new ZDecimal(5.999).Round(2));
			AssertEquals(new ZDecimal(9), new ZDecimal(8.9).Round(0));
		}

		public void TestDecimalPlaces()
		{
			AssertEquals(0, new ZDecimal(6.0).DecimalPlaces);
			AssertEquals(1, new ZDecimal(6.1).DecimalPlaces);
			AssertEquals(2, new ZDecimal(6.15).DecimalPlaces);
			AssertEquals(2, new ZDecimal(6.150000).DecimalPlaces);
		}

		public void TestIsWithinSqlPrecisionAndScale()
		{
			AssertIsWithinSqlPrecisionAndScale(0M, 4, 2, true);
			AssertIsWithinSqlPrecisionAndScale(3M, 4, 2, true);
			AssertIsWithinSqlPrecisionAndScale(3M, 1, 0, true);
			AssertIsWithinSqlPrecisionAndScale(20.1M, 4, 2, true);
			AssertIsWithinSqlPrecisionAndScale(20.12M, 4, 2, true);
			AssertIsWithinSqlPrecisionAndScale(99.99M, 4, 2, true);

			AssertIsWithinSqlPrecisionAndScale(120M, 4, 2, false);
			AssertIsWithinSqlPrecisionAndScale(120.1M, 4, 2, false);
			AssertIsWithinSqlPrecisionAndScale(120.12M, 4, 2, false);

			AssertIsWithinSqlPrecisionAndScale(12M, 4, 0, true);
		}

		void AssertIsWithinSqlPrecisionAndScale(ZDecimal value, int precision, int scale, bool shouldBeValid)
		{
			string errorMessage = string.Format("Precision is <{0}>, Scale is <{1}>, Value {2} should be {3}valid.", precision, scale, value, (shouldBeValid) ? "" : "in");
			AssertEquals(errorMessage, shouldBeValid, value.IsWithinSqlPrecisionAndScale(precision, scale));
		}

		public void TestToZInt()
		{
			ZDecimal d = new ZDecimal(65.9);
			AssertEquals(new ZInt(65), d.ToZInt());
		}

		public void TestCastToInt()
		{
			ZDecimal d = new ZDecimal(65.9);
			AssertEquals(65, (int)d);
		}

		public void TestGreaterThanOperator()
		{
			ZDecimal lhs = 2m;
			ZDecimal rhs = 1m;

			Assert(lhs > rhs);
		}

		public void TestGreaterThanOrEqualOperator()
		{
			ZDecimal lhs = 2m;
			ZDecimal rhs = 2m;

			Assert(lhs >= rhs);

			rhs = 1m;

			Assert(lhs >= rhs);
		}

		public void TestLessThanOperator()
		{
			ZDecimal lhs = 1m;
			ZDecimal rhs = 2m;

			Assert(lhs < rhs);
		}

		public void TestLessThanOrEqualOperator()
		{
			ZDecimal lhs = 2m;
			ZDecimal rhs = 2m;

			Assert(lhs <= rhs);

			lhs = 1m;

			Assert(lhs <= rhs);
		}

		public void TestEqualOperator()
		{
			ZDecimal lhs = 2m;
			ZDecimal rhs = 2m;

			AssertEquals(true, lhs == rhs);

			lhs = 1m;

			AssertEquals(false, lhs == rhs);
		}

		public void TestNotEqualOperator()
		{
			ZDecimal lhs = 2m;
			ZDecimal rhs = 2m;

			AssertEquals(false, lhs != rhs);

			lhs = 1m;

			AssertEquals(true, lhs != rhs);
		}

		public void TestPlusPlusOperator()
		{
			decimal d = 0;
			d++;
			ZDecimal z = 0;
			z++;

			AssertEquals("ZDecimal Z = 0;   Z++;   -   Z should be 1.", 1m, z);
			Assert("ZDecimal++ does not match decimal++ operator.", z == d);
		}

		public void TestMinusMinusOperator()
		{
			decimal d = 2m;
			d--;
			ZDecimal z = 2m;
			z--;

			AssertEquals("ZDecimal Z = 2;   Z--;   -   Z should be 1.", 1m, z);
			Assert("ZDecimal-- does not match decimal++ operator.", z == d);
		}

		public void TestToStringTrimZeros_DecimalPlaces()
		{
			AssertEquals("10.25", new ZDecimal(10.25000m).ToStringTrimZeros(2));
			AssertEquals("10.25", new ZDecimal(10.25000m).ToStringTrimZeros(3));
			AssertEquals("10.123", new ZDecimal(10.12345m).ToStringTrimZeros(3));
			AssertEquals("0.25", new ZDecimal(0.25000m).ToStringTrimZeros(3));
			AssertEquals("0.25", new ZDecimal(0.25m).ToStringTrimZeros(3));
			AssertEquals("123", new ZDecimal(123m).ToStringTrimZeros(3));
			AssertEquals("123.456", new ZDecimal(123.45678m).ToStringTrimZeros(3));
			AssertEquals("123.4", new ZDecimal(123.45678m).ToStringTrimZeros(1));
		}

		public void TestToStringTrimZeros()
		{
			AssertEquals("10.25", new ZDecimal(10.25000m).ToStringTrimZeros());
			AssertEquals("10", new ZDecimal(10m).ToStringTrimZeros());
			AssertEquals("0", new ZDecimal(0m).ToStringTrimZeros());
			AssertEquals("100", new ZDecimal(100m).ToStringTrimZeros());
			AssertEquals("1000", new ZDecimal(1000m).ToStringTrimZeros());
			AssertEquals("-100", new ZDecimal(-100m).ToStringTrimZeros());
			AssertEquals("-100.25", new ZDecimal(-100.25000m).ToStringTrimZeros());
		}

		public void TestTestToStringTrimZerosWithFormat()
		{
			AssertEquals("10.25", new ZDecimal(10.25000m).ToStringTrimZeros("N"));
			AssertEquals("10", new ZDecimal(10m).ToStringTrimZeros("N"));
			AssertEquals("0", new ZDecimal(0m).ToStringTrimZeros("N"));
			AssertEquals("100", new ZDecimal(100m).ToStringTrimZeros("N"));
			AssertEquals("1,000", new ZDecimal(1000m).ToStringTrimZeros("N"));
			AssertEquals("-100", new ZDecimal(-100m).ToStringTrimZeros("N"));
			AssertEquals("-100.25", new ZDecimal(-100.25000m).ToStringTrimZeros("N"));
		}

		public void TestToStringWithDecimals()
		{
			ZDecimal value = new ZDecimal(10M);

			string decimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			AssertEquals("10", value.ToString(-1));
			AssertEquals("10", value.ToString(0));
			AssertEquals("10" + decimalSeparator + "0", value.ToString(1));
			AssertEquals("10" + decimalSeparator + "00", value.ToString(2));
			AssertEquals("10" + decimalSeparator + "000", value.ToString(3));
			AssertEquals("10" + decimalSeparator + "0000", value.ToString(4));
			AssertEquals("10" + decimalSeparator + "00000", value.ToString(5));
		}

		public void TestToStringWithDecimalsWhenValueHasDecimals()
		{
			string decimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			ZDecimal value = new ZDecimal(10.12);
			AssertEquals("10", value.ToString(-1));
			AssertEquals("10", value.ToString(0));
			AssertEquals("10" + decimalSeparator + "1", value.ToString(1));
			AssertEquals("10" + decimalSeparator + "12", value.ToString(2));
			AssertEquals("10" + decimalSeparator + "120", value.ToString(3));
			AssertEquals("10" + decimalSeparator + "1200", value.ToString(4));
			AssertEquals("10" + decimalSeparator + "12000", value.ToString(5));
		}

		public void TestToStringWithCommasAndPadingWithZeros()
		{
			AssertEquals("With Comma", "12,2", new ZDecimal(12.23M).ToString(1, true));
			AssertEquals("With Comma", "123,4750", new ZDecimal(123.475M).ToString(4, true));
			AssertEquals("With Comma", "789,587", new ZDecimal(789.5874321M).ToString(3, true));

			AssertEquals("With Comma", "12.2", new ZDecimal(12.23M).ToString(1, false));
			AssertEquals("With Comma", "123.4750", new ZDecimal(123.475M).ToString(4, false));
			AssertEquals("With Comma", "789.587", new ZDecimal(789.5874321M).ToString(3, false));
		}

		public void TestToStringWithFormatProvider()
		{
			CombineAssertions("With ES Number Format (1.000,000)", () =>
			{
				var esNumberFormat = CultureInfo.GetCultureInfo("es-ES").NumberFormat;

				AssertEquals("10,250", new ZDecimal(10.25000m).ToString("N3", esNumberFormat));
				AssertEquals("10,000", new ZDecimal(10m).ToString("N3", esNumberFormat));
				AssertEquals("0,000", new ZDecimal(0m).ToString("N3", esNumberFormat));
				AssertEquals("100,000", new ZDecimal(100m).ToString("N3", esNumberFormat));
				AssertEquals("1.000,000", new ZDecimal(1000m).ToString("N3", esNumberFormat));
				AssertEquals("1.000.000,000", new ZDecimal(1000000m).ToString("N3", esNumberFormat));
				AssertEquals("-100,000", new ZDecimal(-100m).ToString("N3", esNumberFormat));
				AssertEquals("-100,250", new ZDecimal(-100.25000m).ToString("N3", esNumberFormat));
			});

			CombineAssertions("With FR Number Format (1 000,000)", () =>
			{
				var frNumberFormat = CultureInfo.GetCultureInfo("fr-FR").NumberFormat;

				AssertEquals("10,250", new ZDecimal(10.25000m).ToString("N3", frNumberFormat));
				AssertEquals("10,000", new ZDecimal(10m).ToString("N3", frNumberFormat));
				AssertEquals("0,000", new ZDecimal(0m).ToString("N3", frNumberFormat));
				AssertEquals("100,000", new ZDecimal(100m).ToString("N3", frNumberFormat));
				AssertEquals("1 000,000", new ZDecimal(1000m).ToString("N3", frNumberFormat));
				AssertEquals("1 000 000,000", new ZDecimal(1000000m).ToString("N3", frNumberFormat));
				AssertEquals("-100,000", new ZDecimal(-100m).ToString("N3", frNumberFormat));
				AssertEquals("-100,250", new ZDecimal(-100.25000m).ToString("N3", frNumberFormat));
			});

			CombineAssertions("With US Number Format (1,000.000)", () =>
			{
				var usNumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

				AssertEquals("10.250", new ZDecimal(10.25000m).ToString("N3", usNumberFormat));
				AssertEquals("10.000", new ZDecimal(10m).ToString("N3", usNumberFormat));
				AssertEquals("0.000", new ZDecimal(0m).ToString("N3", usNumberFormat));
				AssertEquals("100.000", new ZDecimal(100m).ToString("N3", usNumberFormat));
				AssertEquals("1,000.000", new ZDecimal(1000m).ToString("N3", usNumberFormat));
				AssertEquals("1,000,000.000", new ZDecimal(1000000m).ToString("N3", usNumberFormat));
				AssertEquals("-100.000", new ZDecimal(-100m).ToString("N3", usNumberFormat));
				AssertEquals("-100.250", new ZDecimal(-100.25000m).ToString("N3", usNumberFormat));
			});
		}

		public void TestShowTruncationBehaviour()
		{
			string decimalSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			ZDecimal value = new ZDecimal(0.649);
			AssertEquals("0" + decimalSeparator + "64", value.Truncate(2).ToString());
			AssertEquals("0" + decimalSeparator + "649", value.Truncate(5).ToString());

			ZDecimal negativeValue = new ZDecimal(-1.649);
			AssertEquals("-1" + decimalSeparator + "64", negativeValue.Truncate(2).ToString());
			AssertEquals("-1" + decimalSeparator + "649", negativeValue.Truncate(5).ToString());
		}

		public void TestdecimalTruncateIsExtended()
		{
			ZDecimal value = new ZDecimal(20.45643);
			AssertEquals(20m, value.Truncate());
		}

		public void TestCeilingInvalidArgument()
		{
			AssertExceptionThrown<ArgumentException>(() => new ZDecimal(0.0).Ceiling(-1));
		}

		public void TestCeiling()
		{
			CombineAssertions(() =>
			{
				AssertEquals(new ZDecimal(15.72), new ZDecimal(15.711).Ceiling(2));
				AssertEquals(new ZDecimal(15.8), new ZDecimal(15.781).Ceiling(1));
				AssertEquals(new ZDecimal(0.2), new ZDecimal(0.12).Ceiling(1));
				AssertEquals(new ZDecimal(16), new ZDecimal(15.001).Ceiling(0));
				AssertEquals(new ZDecimal(15), new ZDecimal(15.0).Ceiling(0));
				AssertEquals(new ZDecimal(-15), new ZDecimal(-15.001).Ceiling(0));
				AssertEquals(new ZDecimal(-0.1), new ZDecimal(-0.12).Ceiling(1));
				AssertEquals(new ZDecimal(-15.7), new ZDecimal(-15.781).Ceiling(1));
				AssertEquals(new ZDecimal(-15.71), new ZDecimal(-15.711).Ceiling(2));
			});
		}

		public void TestExplicitCastToZInt()
		{
			ZDecimal value = new ZDecimal(10M);
			AssertEquals(new ZInt(10), (ZInt)value);
			value = new ZDecimal(10.77098M);
			AssertEquals(new ZInt(10), (ZInt)value);
			value = new ZDecimal(-10.987M);
			AssertEquals(new ZInt(-10), (ZInt)value);
		}

		public void TestParse()
		{
			foreach (string stringValue in new string[] { "0.37", "10.12", "12" })
			{
				AssertEquals(stringValue + " as string", decimal.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZDecimal.Parse(stringValue));
				AssertEquals(stringValue + " as ZString", decimal.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZDecimal.Parse(new ZString(stringValue)));
			}
		}

		public void TestCanParse()
		{
			Assert(!ZDecimal.CanParse(""));
			Assert(ZDecimal.CanParse("0"));
			Assert(ZDecimal.CanParse("0.25"));
			Assert(!ZDecimal.CanParse("abc"));
		}

		public void TestCanParseAsInteger()
		{
			Assert(!ZDecimal.CanParseAsInteger(""));
			Assert(ZDecimal.CanParseAsInteger("0"));
			Assert(!ZDecimal.CanParseAsInteger("0.25"));
			Assert(!ZDecimal.CanParseAsInteger("2.2"));
			Assert(!ZDecimal.CanParseAsInteger("abc"));
		}

		public void TestParseSafe()
		{
			foreach (string stringValue in new string[] { "0.37", "10.12", "12" })
			{
				AssertEquals(stringValue + " as string", decimal.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZDecimal.ParseSafe(stringValue, -1));
				AssertEquals(stringValue + " as ZString", decimal.Parse(stringValue, ObjectCache.CultureProvider.Culture.NumberFormat), ZDecimal.ParseSafe(new ZString(stringValue), -1));
			}
			foreach (string stringValue in new string[] { "", "NotNumber!" })
			{
				AssertEquals(stringValue + " as string", new ZDecimal(-1), ZDecimal.ParseSafe(stringValue, -1));
				AssertEquals(stringValue + " as ZString", new ZDecimal(-1), ZDecimal.ParseSafe(new ZString(stringValue), -1));
			}
		}

		[ExpectException(typeof(FormatException))]
		public void TestParseThrowsFormatExceptionForBadValue()
		{
			decimal.Parse("bad decimal");
		}

		public void TestTryParse()
		{
			AssertTryParse(decimal.MinValue.ToString(NumberFormatInfo.InvariantInfo), true, decimal.MinValue);
			AssertTryParse("100", true, 100);
			AssertTryParse(decimal.MaxValue.ToString(), true, decimal.MaxValue);

			AssertTryParse("1000000000000000000000000000000000", false, 0);
			AssertTryParse("-1000000000000000000000000000000000", false, 0);

			AssertTryParse("some junk", false, 0);
		}

		protected void AssertTryParse(string value, bool expectSuccess, decimal expectedResult)
		{
			bool success = ZDecimal.TryParse(value, out var result);
			AssertEquals("TryParse success.", expectSuccess, success);
			AssertEquals("TryParse result.", expectedResult, result);
		}

		public void TestTryParseWithNumberFormat()
		{
			CombineAssertions("With FR Number Format (1 000,000)", () =>
			{
				var frNumberFormat = CultureInfo.GetCultureInfo("fr-FR").NumberFormat;

				AssertTryParse(decimal.MinValue.ToString(frNumberFormat), frNumberFormat, true, decimal.MinValue);
				AssertTryParse(decimal.MaxValue.ToString(frNumberFormat), frNumberFormat, true, decimal.MaxValue);
				AssertTryParse("10,250", frNumberFormat, true, 10.250m);
				AssertTryParse("1 00,250", frNumberFormat, true, 100.250m);
				AssertTryParse("-100,250", frNumberFormat, true, -100.250m);
				AssertTryParse("10,12.34", frNumberFormat, false, 0);
			});

			CombineAssertions("With US Number Format (1,000.000)", () =>
			{
				var usNumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

				AssertTryParse(decimal.MinValue.ToString(usNumberFormat), usNumberFormat, true, decimal.MinValue);
				AssertTryParse(decimal.MaxValue.ToString(usNumberFormat), usNumberFormat, true, decimal.MaxValue);
				AssertTryParse("10.250", usNumberFormat, true, 10.250m);
				AssertTryParse("10,12.34", usNumberFormat, true, 1012.34m);
				AssertTryParse("-100.250", usNumberFormat, true, -100.250m);
				AssertTryParse("10.12,34", usNumberFormat, false, 0);
			});
		}

		protected void AssertTryParse(string value, IFormatProvider provider, bool expectSuccess, decimal expectedResult)
		{
			bool success = ZDecimal.TryParse(value, provider, out var result);
			AssertEquals(value + " TryParse success.", expectSuccess, success);
			AssertEquals(value + " TryParse result.", expectedResult, result);
		}

		public void TestZero()
		{
			decimal zero = ZDecimal.Zero;
			AssertEquals(0m, zero);
		}

		public void TestToStringTrimZerosTrailingZeroTruncation()
		{
			ZDecimal amount = new ZDecimal(6.120m);
			AssertEquals("6.12", amount.ToStringTrimZeros());
		}

		public void TestToStringTrimZerosTrailingZeroTruncationRemovesDecimalPoint()
		{
			ZDecimal amount = new ZDecimal(6.0m);
			AssertEquals("6", amount.ToStringTrimZeros());
		}

		public void TestXmlSerializable()
		{
			AssertZTypeSerializesToXml("<a>12.3</a>", new ZDecimal(12.3));
		}

		public void TestIsInRange()
		{
			var amount = new ZDecimal(6m);
			AssertEquals(true, amount.IsInRange(5m, 7m));
			AssertEquals(true, amount.IsInRange(6m, 6m));
			AssertEquals(true, amount.IsInRange(5.9m, 6.1m));
			AssertEquals(true, amount.IsInRange(-99999m, 99999m));
			AssertEquals(false, amount.IsInRange(5m, 5.9m));
			AssertEquals(false, amount.IsInRange(6.1m, 8m));
			AssertEquals(false, amount.IsInRange(-5m, -4m));
			var amount2 = new ZDecimal(-82.3m);
			AssertEquals(true, amount2.IsInRange(-100m, -70m));
			AssertEquals(false, amount2.IsInRange(70m, 100m));
		}

		public void TestIsInteger()
		{
			AssertEquals(true, new ZDecimal(5m).IsInteger);
			AssertEquals(false, new ZDecimal(5.2m).IsInteger);
			AssertEquals(false, new ZDecimal(0.25m).IsInteger);
			AssertEquals(true, new ZDecimal(0m).IsInteger);
		}

		#region IZTypeTest Overrides

		protected override IZType NewZ(object value)
		{
			return new ZDecimal(value);
		}

		protected override object[] EmptyValues
		{
			get { return new object[] { DBNull.Value, new ZDecimal(), 0M, 0, (short)0, (byte)0 }; }
		}

		protected override object[] ValidValues
		{
			get { return new object[] { -1M, 1M, 1, (short)1, (byte)1, decimal.MinValue, decimal.MaxValue }; }
		}

		protected override object[] UnsupportedValues
		{
			get { return new object[] { new object() }; }
		}

		#endregion
	}
}
