using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.ES.Business.Testing;

sealed class ESEDIMessagePurgeSettingsConfigTest : TestCaseWithFactory
{
	public void TestESCustomsMessage()
	{
		var setting = settingsList.FirstOrDefault(x => x.ApplicationCode == ApplicationCodeList.Codes.ESCustomsMessage);

		AssertNotNull("ESCustomsMessage PurgeSetting", setting);

		CombineAssertions(() =>
		{
			AssertEquals((ZShort)10, setting.PurgeTime);
			AssertEquals(TimeUnit.Year, setting.PurgeTimeUnit);
			AssertEquals((ZShort)10, setting.Interchanges[0].PurgeTime);
			AssertEquals(TimeUnit.Year, setting.Interchanges[0].PurgeTimeUnit);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var config = new ESEDIMessagePurgeSettingsConfig();
		settingsList = [.. config.GetPurgeSettings()];
	}
	List<ApplicationCodeObj> settingsList;
}
