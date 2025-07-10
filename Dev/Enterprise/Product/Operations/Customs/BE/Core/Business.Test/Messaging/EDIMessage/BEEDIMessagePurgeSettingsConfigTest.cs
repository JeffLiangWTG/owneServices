using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class BEEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
{
	public void TestGetPurgeSettings()
	{
		var config = new BEEDIMessagePurgeSettingsConfig();
		var result = config.GetPurgeSettings();

		CombineAssertions(() =>
		{
			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);

			AssertEquals("PurgeSettings grouped on applicationCode BEC", 1, settingsList.Count);
			AssertEquals("PurgeSettings apply for BEC", "BEC", result.FirstOrDefault().ApplicationCode);
			AssertEquals("PurgeSettings.PurgeTime for interchanges", (ZShort)6, result.FirstOrDefault().Interchanges[0].PurgeTime);
			AssertEquals("PurgeSettings.PurgeTimeUnit for interchanges", TimeUnit.Month, result.FirstOrDefault().Interchanges[0].PurgeTimeUnit);
			AssertContainsExactElementsInAnyOrder("Config exists for 3 MessageTypes (AES, IMP and NCT)", new[] { "AES", "IMP", "NCT" }, result.FirstOrDefault().MessageTypes.Cast<MessageTypeObj>().Select(x => x.MessageType));
			foreach (MessageTypeObj messageType in result.FirstOrDefault().MessageTypes)
			{
				AssertMessageTypeObj(messageType);
			}
		});
	}

	void AssertMessageTypeObj(MessageTypeObj messageType)
	{
		AssertNotNullOrEmpty($"{messageType.MessageType} - MessageTypeDescription should be filled", messageType.MessageTypeDescription);
		AssertEquals($"{messageType.MessageType} - PurgeTime should be 8", (ZShort)8, messageType.PurgeTime);
		AssertEquals($"{messageType.MessageType} - PurgeTimeUnit should be 'Year'", TimeUnit.Year, messageType.PurgeTimeUnit);
	}
}
