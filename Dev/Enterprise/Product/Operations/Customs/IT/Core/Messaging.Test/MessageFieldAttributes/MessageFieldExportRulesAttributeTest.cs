using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageFieldExportRulesAttributeTest : TestCase
{
	public void TestConstructor()
	{
		var attribute1 = new MessageFieldExportRulesAttribute();
		AssertEquals("The rules should be empty if nothing be passed to the constructor.", 0, attribute1.Rules.Length);

		var attributes2 = new MessageFieldExportRulesAttribute("R", "R23");
		AssertEquals("The rules should contains R and R23.", "R", attributes2.Rules[0]);
		AssertEquals("The rules should contains R and R23.", "R23", attributes2.Rules[1]);
	}
}
