using System.Collections.Generic;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	class CustomsWareIntegrationOutOfLineForTest : CustomsWareIntegrationOutOfLine
	{
		protected override ICollection<SettingDetail> SettingsToValidate => new List<SettingDetail>();
	}
}
