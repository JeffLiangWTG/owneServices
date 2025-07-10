using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(NctsGroupNotification))]
	sealed class NctsGroupNotificationTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectTemplateTestCase<NctsGroupNotification>
	{
		public void TestValidateSendMode() => AssertEquals("NOE, ESM, ENG, ESG", BizObj.SendModeList.CodesAsString);

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override NctsGroupNotification GetBusinessObjectToClone() => new NctsGroupNotification();

		protected override NctsGroupNotification GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
