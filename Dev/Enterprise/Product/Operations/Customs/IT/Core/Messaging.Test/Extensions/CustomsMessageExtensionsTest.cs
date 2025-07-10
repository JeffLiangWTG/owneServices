using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class CustomsMessageExtensionsTest : TestCase
{
	public void TestSerializeWithTabSeparator()
	{
		AssertExceptionThrown<ArgumentNullException>("customsMessages are required", () => CustomsMessageExtensions.SerializeWithTabSeparator(null));

		AssertEquals("Serialized Message", "Field 1	Field 2	", CustomsMessageExtensions.SerializeWithTabSeparator(new SadCustomsMessage[] { new MockSadCustomsMessage1() }));
	}

	#region MockCustomsMessage1

	class MockSadCustomsMessage1 : SadCustomsMessage
	{
		[MessageLayout(Order = 0)]
		[MessageFieldStringRepresentation(CharType.Alphanumeric, 7, false)]
		public ZString MockField1 => "Field 1";

		[MessageLayout(Order = 1)]
		[MessageFieldStringRepresentation(CharType.Alphanumeric, 7, false)]
		public ZString MockField2 => "Field 2";
	}

	#endregion
}
