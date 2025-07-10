using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class MessageBuilderHelperTest : TestCase
{
	public void TestReturnDefaultValueIfNullOrEmpty_ZString()
	{
		var defaultValue = ".";

		CombineAssertions(() =>
		{
			ZString? inputValue = null;
			AssertEquals("Return default value if null", defaultValue, inputValue.ReturnDefaultValueIfNullOrEmpty(defaultValue));
			AssertNull("Return null default value if null", inputValue.ReturnDefaultValueIfNullOrEmpty(null));

			inputValue = "";
			AssertEquals("Return default value if empty", defaultValue, inputValue.ReturnDefaultValueIfNullOrEmpty(defaultValue));
			AssertNull("Return null default value if empty", inputValue.ReturnDefaultValueIfNullOrEmpty(null));

			inputValue = "XX";
			AssertEquals("Return input value if not null or empty", "XX", inputValue.ReturnDefaultValueIfNullOrEmpty(defaultValue));
		});
	}

	public void TestFallbackIfEmpty_ZString()
	{
		var defaultValue = "123";

		CombineAssertions(() =>
		{
			var inputValue = ZString.Empty;
			AssertEquals("Return default value if empty", defaultValue, inputValue.FallbackIfEmpty(defaultValue));

			inputValue = "456";
			AssertEquals("Return input value if not null or empty", "456", inputValue.FallbackIfEmpty(defaultValue));
		});
	}

	public void TestReturnNullIfEmpty_ZString()
	{
		CombineAssertions(() =>
		{
			var inputValue = ZString.Empty;
			AssertNull("Return Null if empty", inputValue.ReturnNullIfEmpty());

			inputValue = "456";
			AssertEquals("Return input value if not empty", "456", inputValue.ReturnNullIfEmpty());
		});
	}

	public void TestFallbackIfEmpty_ZDecimal()
	{
		var defaultValue = 99m;

		CombineAssertions(() =>
		{
			ZDecimal inputValue = 0m;
			AssertEquals("Return default value if empty", defaultValue, inputValue.FallbackIfEmpty(defaultValue));

			inputValue = 123m;
			AssertEquals("Return input value if not null or empty", 123m, inputValue.FallbackIfEmpty(defaultValue));
		});
	}

	public void TestReturnNullIfEmpty_ZDecimal()
	{
		CombineAssertions(() =>
		{
			ZDecimal inputValue = 0m;
			AssertNull("Return Null if empty", inputValue.ReturnNullIfEmpty());

			inputValue = 123m;
			AssertEquals("Return input value if not empty", 123m, inputValue.ReturnNullIfEmpty());
		});
	}

	public void TestToOptionalDateTime() => CombineAssertions(() =>
	{
		var inputValue = ZDateTime.Empty;
		AssertNull(inputValue.ToOptionalDateTime());

		inputValue = new ZDateTime(2022, 12, 1, 15, 20, 30);
		AssertEquals(new DateTime(2022, 12, 1, 15, 20, 30), inputValue.ToOptionalDateTime());
	});

	public void TestToOptionalDateTimeUTC()
	{
		CombineAssertions(() =>
		{
			ZDateTime inputValue = ZDateTime.Empty;
			AssertNull(inputValue.ToOptionalDateTimeUTC());

			inputValue = new ZDateTime(2022, 12, 1, 15, 0, 0);
			AssertEquals($"Local winter time: {inputValue}", new DateTime(2022, 12, 1, 14, 0, 0, 0, System.DateTimeKind.Utc), inputValue.ToOptionalDateTimeUTC());

			inputValue = new ZDateTime(2022, 8, 1, 15, 0, 0);
			AssertEquals($"Local summer time: {inputValue}", new DateTime(2022, 8, 1, 13, 0, 0, 0, System.DateTimeKind.Utc), inputValue.ToOptionalDateTimeUTC());
		});
	}

	public void TestToStringInvariantCulture_ZDecimal()
	{
		ZDecimal inputValue = 12345.3m;
		AssertEquals("12345.3", inputValue.ToStringInvariantCulture());
	}

	public void TestToStringInvariantCulture_ZInt()
	{
		ZInt inputValue = 12345;
		AssertEquals("12345", inputValue.ToStringInvariantCulture());
	}
}
