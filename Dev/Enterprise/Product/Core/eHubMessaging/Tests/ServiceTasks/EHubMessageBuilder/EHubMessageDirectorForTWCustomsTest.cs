using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.Tests
{
	class EHubMessageDirectorForTWCustomsTest : TestCaseWithFactory
	{
		public void TestSupportedMessageTypes()
		{
			var supportedMessageTypes = new HashSet<string> { "ECD", "ICD", "ADM", "TRA", "IEA", "CAA", "FHM", "NXM", "201", "207", "301", "31A", "31D", "401", "601", "603" };
			CombineAssertions(() =>
			{
				foreach (var applicationCode in supportedMessageTypes)
				{
					AssertSupportedMessageTypes(applicationCode);
				}
			});
		}

		void AssertSupportedMessageTypes(string applicationCode)
		{
			var interchange = CreateInterchange(applicationCode);
			var notifier = new NotificationBuffer();
			var messageDirector = new EHubMessageDirectorForTWCustoms(interchange, notifier);
			AssertNotNull($"EDI Interchange Application Code '{applicationCode}' is supported.", messageDirector.CreateBuilder());
		}

		EDIInterchange CreateInterchange(string type)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_SessionGUID = new ZGuid("166f827b-5daa-49bb-b364-42a22b4f438a");
			interchange.EI_From = "DummyFrom";
			interchange.EI_To = "DummyTo";
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TWCustoms;
			interchange.EI_InterchangeType = type;
			interchange.EI_InterchangeNum = "~BLAH00000000009999";
			return interchange;
		}
	}
}
