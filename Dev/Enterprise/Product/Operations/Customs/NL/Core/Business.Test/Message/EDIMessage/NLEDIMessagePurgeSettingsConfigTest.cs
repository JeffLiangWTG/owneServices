using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
{
	public void TestGetPurgeSettings()
	{
		var config = new NLEDIMessagePurgeSettingsConfig();
		var result = config.GetPurgeSettings();

		CombineAssertions(() =>
		{
			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);

			AssertEquals("PurgeSettings grouped on ApplicationCode NLC", 1, settingsList.Count);
			AssertEquals("PurgeSettings apply for NLC", "NLC", result.FirstOrDefault().ApplicationCode);
			AssertEquals("PurgeSettings.PurgeTime for interchanges", (ZShort)6, result.FirstOrDefault().Interchanges[0].PurgeTime);
			AssertEquals("PurgeSettings.PurgeTimeUnit for interchanges", TimeUnit.Month, result.FirstOrDefault().Interchanges[0].PurgeTimeUnit);
			AssertContainsExactElementsInAnyOrder("Config Esists for 3 MessageTypes (NCT, DMS, EXT)", new[] { "NCT", "DMS", "EXT" }, result.FirstOrDefault().MessageTypes.Cast<MessageTypeObj>().Select(x => x.MessageType));
			foreach (MessageTypeObj messageType in result.FirstOrDefault().MessageTypes)
			{
				AssertMessageTypeObj(messageType);
			}
		});

		void AssertMessageTypeObj(MessageTypeObj messageType)
		{
			AssertNotNullOrEmpty($"{messageType.MessageType} - MessageTypeDescription should be filled", messageType.MessageTypeDescription);
			AssertEquals($"{messageType.MessageType} - PurgeTime should be 8", (ZShort)8, messageType.PurgeTime);
			AssertEquals($"{messageType.MessageType} - PurgeTimeUnit should be 'Year'", TimeUnit.Year, messageType.PurgeTimeUnit);
		}
	}

	public void TestAllMessageTypesHaveBeenSet()
	{
		AssertEquals("Please consider updating the Purge Settings when you modify the message type list", "DMS, EXT, NCT", new NLEDIMessageTypes().CodesAsString);
	}
}
