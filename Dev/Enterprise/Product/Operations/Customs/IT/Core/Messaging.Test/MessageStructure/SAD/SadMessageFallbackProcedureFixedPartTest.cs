using Enterprise.Customs.IT.Messaging.SAD;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class SadMessageFallbackProcedureFixedPartTest : TestCase
{
	public void TestProperties()
	{
		var fixedPart = new SadMessageFallbackProcedureFixedPart(true, "MSGCODE", "DECTAXNO", "000001", 1);
		CombineAssertions("Assertions for RecordType = Header", () =>
		 {
			 AssertEquals("000001", fixedPart.AnnualProgressiveNumber);
			 AssertEquals("DECTAXNO", fixedPart.DeclarantTaxNumber);
			 AssertEquals("", fixedPart.EmptyField);
			 AssertEquals("MSGCODE", fixedPart.MessageCode);
			 AssertEquals(1, fixedPart.ProgressiveNumber);
			 AssertEquals("T", fixedPart.RecordType);
		 });

		fixedPart = new SadMessageFallbackProcedureFixedPart(false, "", "", "", 0);
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
