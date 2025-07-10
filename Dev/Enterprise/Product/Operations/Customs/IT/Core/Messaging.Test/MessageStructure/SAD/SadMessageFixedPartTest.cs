using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class SadMessageFixedPartTest : TestCase
{
	public void TestProperties()
	{
		var fixedPart = new SadMessageFixedPart(true, "MSGCODE", "000001", 1);
		CombineAssertions("Assertions for RecordType = Header", () =>
		{
			AssertEquals("000001", fixedPart.AnnualProgressiveNumber);
			AssertEquals("", fixedPart.DeclarantTaxNumber);
			AssertEquals("", fixedPart.EmptyField);
			AssertEquals("MSGCODE", fixedPart.MessageCode);
			AssertEquals(1, fixedPart.ProgressiveNumber);
			AssertEquals("T", fixedPart.RecordType);
		});

		fixedPart = new SadMessageFixedPart(false, "", "", 0);
		CombineAssertions("Assertions for RecordType = Continuation", () =>
		{
			AssertEquals("", fixedPart.AnnualProgressiveNumber);
			AssertEquals("", fixedPart.DeclarantTaxNumber);
			AssertEquals("", fixedPart.EmptyField);
			AssertEquals("", fixedPart.MessageCode);
			AssertEquals(0, fixedPart.ProgressiveNumber);
			AssertEquals("?", fixedPart.RecordType);
		});
	}
}
