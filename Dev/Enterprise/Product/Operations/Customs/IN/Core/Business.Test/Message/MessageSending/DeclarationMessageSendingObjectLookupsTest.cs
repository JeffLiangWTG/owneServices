using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DeclarationMessageSendingObjectLookups))]
sealed class DeclarationMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMessageTypes()
	{
		var messageSendingObject = CreateObjectForTest();
		var lookups = messageSendingObject.Lookups;
		var messageTypes = lookups.MessageTypes;
		CombineAssertions(() =>
		{
			AssertEquals("Message Types", new DeclarationMessageTypeList().CodesAsString, messageTypes.CodesAsString);
			AssertSame("Message Types List is cached", messageTypes, lookups.MessageTypes);
		});
	}

	DeclarationMessageSendingObject CreateObjectForTest()
	{
		var cusEntryHeader = Factory.New<CusEntryHeader>();
		return new DeclarationMessageSendingObject(cusEntryHeader);
	}
}
