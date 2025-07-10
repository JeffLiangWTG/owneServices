using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

class MessageProviderHelperTest : TestCaseWithFactory
{
	[TestDate(2022, 8, 1, 1, 1, 1, 500)]
	public void TestGetCurrentDateTimeAsUnspecifiedDateTimeKind() => CombineAssertions(() =>
	{
		AssertEquals("Remove milliseconds", new DateTime(2022, 8, 1, 1, 1, 1), MessageProviderHelper.GetCurrentDateTimeAsUnspecifiedDateTimeKind(true));
		AssertEquals("Keep milliseconds", new DateTime(2022, 8, 1, 1, 1, 1, 500), MessageProviderHelper.GetCurrentDateTimeAsUnspecifiedDateTimeKind(false));
	});

	public void TestReturnNullIfEmpty()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty String", MessageProviderHelper.ReturnNullIfEmpty(ZString.Empty));
			AssertEquals("Not empty String", "ABC", MessageProviderHelper.ReturnNullIfEmpty(new ZString("ABC")));

			AssertNull("Empty Decimal", MessageProviderHelper.ReturnNullIfEmpty(ZDecimal.Zero));
			AssertEquals("Not empty Decimal", 123.1m, MessageProviderHelper.ReturnNullIfEmpty(new ZDecimal(123.1m)));

			AssertNull("Empty Decimal", MessageProviderHelper.ReturnNullIfEmpty(ZDateTime.Empty));
			AssertEquals("Not empty Decimal", new DateTime(2022, 01, 01), MessageProviderHelper.ReturnNullIfEmpty(new ZDateTime(2022, 01, 01)));
		});
	}

	public void TestIntReturnNullIfEmpty()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty Int String", MessageProviderHelper.IntReturnNullIfEmpty(ZString.Empty));
			AssertNull("Not empty String without Int value", MessageProviderHelper.IntReturnNullIfEmpty(new ZString("ABC")));
			AssertNull("Not empty String with Mixed value case 1", MessageProviderHelper.IntReturnNullIfEmpty(new ZString("231zxc")));
			AssertNull("Not empty String with Mixed value case 2", MessageProviderHelper.IntReturnNullIfEmpty(new ZString("qwe231")));
			AssertNull("Not empty String with Decimal value", MessageProviderHelper.IntReturnNullIfEmpty(new ZString("1.32")));

			AssertEquals("Not empty String with Int value", 231, MessageProviderHelper.IntReturnNullIfEmpty(new ZString("231")));
		});
	}

	public void TestDecimalReturnNullIfEmpty()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty Int String", MessageProviderHelper.DecimalReturnNullIfEmpty(ZString.Empty));
			AssertNull("Not empty String without Decimal value", MessageProviderHelper.DecimalReturnNullIfEmpty(new ZString("ABC")));
			AssertNull("Not empty String with Mixed value case 1", MessageProviderHelper.DecimalReturnNullIfEmpty(new ZString("231zxc")));
			AssertNull("Not empty String with Mixed value case 2", MessageProviderHelper.DecimalReturnNullIfEmpty(new ZString("qwe231")));

			AssertEquals("Not empty String with Decimal value", 1.32m, MessageProviderHelper.DecimalReturnNullIfEmpty(new ZString("1.32")));
			AssertEquals("Not empty String with Int value", 231.0m, MessageProviderHelper.DecimalReturnNullIfEmpty(new ZString("231")));
		});
	}
}
