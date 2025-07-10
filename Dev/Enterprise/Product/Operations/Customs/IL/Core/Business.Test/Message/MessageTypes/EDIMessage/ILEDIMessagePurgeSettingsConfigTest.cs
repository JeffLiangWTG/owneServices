using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new ILEDIMessagePurgeSettingsConfig();
			var expectedDuration = 7;
			var expectedUnit = TimeUnit.Year;

			var result = config.GetPurgeSettings();

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			AssertApplicationCode(ApplicationCodeList.Codes.ILCustoms, expectedDuration, expectedUnit, settingsList[0]);

			AssertEquals(1, settingsList[0].MessageTypes.Count);
			AssertMessageType(ILEDIMessageSubTypeList.Codes.ForwarderManifestResponse, ILEDIMessageSubTypeList.Descriptions.ForwarderManifestResponse, expectedDuration, expectedUnit, settingsList[0].MessageTypes[0]);
		}

		static void AssertApplicationCode(string applicationCode, int expectedDuration, CargoWise.Types.ZGuid expectedUnit, ApplicationCodeObj settingEntry)
		{
			AssertEquals(applicationCode, settingEntry.ApplicationCode);
			AssertEquals(expectedDuration, settingEntry.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, settingEntry.Interchanges[0].PurgeTimeUnit);
		}

		static void AssertMessageType(string messageSubTypeCode, string messageTypeDescription, int expectedDuration, CargoWise.Types.ZGuid expectedUnit, MessageTypeObj messageType)
		{
			AssertEquals(messageSubTypeCode, messageType.MessageSubType);
			AssertEquals(messageTypeDescription, messageType.MessageSubTypeDescription);
			AssertEquals(expectedDuration, messageType.PurgeTime);
			AssertEquals(expectedUnit, messageType.PurgeTimeUnit);
		}
	}
}
