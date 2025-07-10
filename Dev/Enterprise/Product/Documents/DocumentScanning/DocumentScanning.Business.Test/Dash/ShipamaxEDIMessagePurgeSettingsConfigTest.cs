using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class ShipamaxEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new ShipamaxEDIMessagePurgeSettingsConfig();

			var result = config.GetPurgeSettings();
			var expectedPurgeTime = new ZShort(0);
			var expectedUnit = ZGuid.Empty;

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			var shipaMaxIntergrationMessage = settingsList[0];
			AssertEquals(ApplicationCodeList.Codes.ShipamaxIntegration, shipaMaxIntergrationMessage.ApplicationCode);
			AssertEquals(expectedPurgeTime, shipaMaxIntergrationMessage.PurgeTime);
			AssertEquals(expectedUnit, shipaMaxIntergrationMessage.PurgeTimeUnit);
			Assert(shipaMaxIntergrationMessage.IsUnpurgable);
		}
	}
}
