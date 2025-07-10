using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class DailySequenceNumberFormatterTest : TestCase
{
	public void TestEncode_ThrowsException()
	{
		AssertExceptionThrown<ArgumentException>(() => formatter.Encode(-1));
		AssertNoExceptionThrown(() => formatter.Encode(0));
		AssertNoExceptionThrown(() => formatter.Encode(3843));
		AssertExceptionThrown<ArgumentException>(() => formatter.Encode(3844));
	}

	public void TestEncode()
	{
		AssertEquals("00", formatter.Encode(0));
		AssertEquals("01", formatter.Encode(1));
		AssertEquals("09", formatter.Encode(9));
		AssertEquals("0A", formatter.Encode(10));
		AssertEquals("0Q", formatter.Encode(26));
		AssertEquals("8T", formatter.Encode(525));
		AssertEquals("AE", formatter.Encode(634));
		AssertEquals("J5", formatter.Encode(1183));
		AssertEquals("OM", formatter.Encode(1510));
		AssertEquals("RF", formatter.Encode(1689));
		AssertEquals("Zz", formatter.Encode(2231));
		AssertEquals("a0", formatter.Encode(2232));
		AssertEquals("zZ", formatter.Encode(3817));
		AssertEquals("zy", formatter.Encode(3842));
		AssertEquals("zz", formatter.Encode(3843));
	}

	public void TestEncode_GeneratesUniqueValues()
	{
		var encodedValues = GetAllEncodedValues();
		AssertEquals("Algorithm should generate unique values", encodedValues.Count, encodedValues.Distinct().Count());
	}

	public void TestDecode_ThrowsException()
	{
		AssertExceptionThrown<ArgumentException>(() => formatter.Decode(""));
		AssertExceptionThrown<ArgumentException>(() => formatter.Decode("0"));
		AssertExceptionThrown<ArgumentException>(() => formatter.Decode("001"));
		AssertExceptionThrown<ArgumentException>(() => formatter.Decode("ìà"));
		AssertExceptionThrown<ArgumentException>(() => formatter.Decode("_^"));
		AssertNoExceptionThrown(() => formatter.Decode("00"));
		AssertNoExceptionThrown(() => formatter.Decode("zz"));
	}

	public void TestDecode()
	{
		AssertEquals(0, formatter.Decode("00"));
		AssertEquals(1, formatter.Decode("01"));
		AssertEquals(9, formatter.Decode("09"));
		AssertEquals(10, formatter.Decode("0A"));
		AssertEquals(26, formatter.Decode("0Q"));
		AssertEquals(525, formatter.Decode("8T"));
		AssertEquals(634, formatter.Decode("AE"));
		AssertEquals(1183, formatter.Decode("J5"));
		AssertEquals(1510, formatter.Decode("OM"));
		AssertEquals(1689, formatter.Decode("RF"));
		AssertEquals(2231, formatter.Decode("Zz"));
		AssertEquals(2232, formatter.Decode("a0"));
		AssertEquals(3817, formatter.Decode("zZ"));
		AssertEquals(3842, formatter.Decode("zy"));
		AssertEquals(3843, formatter.Decode("zz"));
	}

	public void TestDecode_RetrievesTheRightIndexFromEncodedValue()
	{
		CombineAssertions(() =>
		{
			var encodedValues = GetAllEncodedValues();
			foreach (var item in encodedValues)
			{
				AssertEquals($"'{item.Value}' decoded value should be", item.Key, formatter.Decode(item.Value));
			}
		});
	}

	public void TestConstants()
	{
		CombineAssertions(() =>
		{
			const int charBase = DailySequenceNumberFormatter.Constants.CharBase;
			const string charset = DailySequenceNumberFormatter.Constants.Charset;

			AssertEquals("CharBase", 62, charBase);
			AssertEquals("Charset", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", charset);
			AssertEquals("CharBase = Charset.Length", charset.Length, charBase);

			const int maxValue = DailySequenceNumberFormatter.Constants.MaxValue;
			const int minValue = DailySequenceNumberFormatter.Constants.MinValue;
			const int numberOfDigits = DailySequenceNumberFormatter.Constants.NumberOfDigits;

			AssertEquals("MaxValue", 3843, maxValue);
			AssertEquals("MaxValue = (CharBase ^ NumberOfDigits) - 1", (int)Math.Pow(charBase, numberOfDigits) - 1, maxValue);
			AssertEquals("MinValue", 0, minValue);
			AssertEquals("NumberOfDigits", 2, numberOfDigits);
		});
	}

	Dictionary<ZInt, ZString> GetAllEncodedValues()
	{
		var encodedValues = new Dictionary<ZInt, ZString>();
		for (int i = DailySequenceNumberFormatter.Constants.MinValue; i <= DailySequenceNumberFormatter.Constants.MaxValue; i++)
		{
			encodedValues.Add(i, formatter.Encode(i));
		}
		return encodedValues;
	}

	protected override void SetUp()
	{
		base.SetUp();
		formatter = new DailySequenceNumberFormatter();
	}

	DailySequenceNumberFormatter formatter;
}
