using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNMessageSendingObjectLookups))]
	sealed class PBNMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageTypes()
		{
			var messageSendingObject = new PBNMessageSendingObject(Factory.New<AsycudaManifestHeader>());
			var lookups = messageSendingObject.Lookups;
			var messageTypes = lookups.MessageTypes;
			CombineAssertions(() =>
			{
				AssertSame("Cached", messageTypes, lookups.MessageTypes);
				AssertEquals("Message types", "CPB, LPB, LPC, UPB, UPD", messageTypes.CodesAsString);
			});
		}
	}
}
