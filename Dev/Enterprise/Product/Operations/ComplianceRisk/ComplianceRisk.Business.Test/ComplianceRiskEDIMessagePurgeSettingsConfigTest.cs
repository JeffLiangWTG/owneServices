using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceRiskEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
	{
		public void TestGetPurgeSettings_ShouldReturnCorrectSettings()
		{
			var config = new ComplianceRiskEDIMessagePurgeSettingsConfig();
			var expectedDuration = 6;
			var expectedUnit = TimeUnit.Month;

			var result = config.GetPurgeSettings();

			AssertNotNull(result);
			var settingsList = new List<ApplicationCodeObj>(result);
			AssertEquals(1, settingsList.Count);

			var cpwRequestMessage = settingsList[0];
			AssertEquals(ApplicationCodeList.Codes.CPWRequestMessage, cpwRequestMessage.ApplicationCode);
			AssertEquals(expectedDuration, cpwRequestMessage.Interchanges[0].PurgeTime);
			AssertEquals(expectedUnit, cpwRequestMessage.Interchanges[0].PurgeTimeUnit);
		}
	}
}
