using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(ManifestMessageSendingObjectLookups))]
sealed class ManifestMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMessageTypesWhenNoMessages()
	{
		var messageTypes = lookups.MessageTypes;
		CombineAssertions(() =>
		{
			AssertEquals("Message Types", ManifestMessageTypeList.Codes.Fresh, messageTypes.CodesAsString);
			AssertEquals("Message Type Default", ManifestMessageTypeList.Codes.Fresh, messageTypes.DefaultCode);
			AssertSame("Message Types List is cached", messageTypes, lookups.MessageTypes);
		});
	}

	public void TestMessageTypesWhenMessagesExists()
	{
		manifestHeader.Messages.AddNew();
		var messageTypes = lookups.MessageTypes;
		CombineAssertions(() =>
		{
			AssertEquals("Message Types", new ManifestMessageTypeList().CodesAsString, messageTypes.CodesAsString);
			AssertSame("Message Types List is cached", messageTypes, lookups.MessageTypes);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		manifestHeader = Factory.New<CGMAsycudaManifestHeader>();
		var messageSendingObject = new ManifestMessageSendingObject(manifestHeader);
		lookups = messageSendingObject.Lookups;
	}

	CGMAsycudaManifestHeader manifestHeader;
	ManifestMessageSendingObjectLookups lookups;
}
