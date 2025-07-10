using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Registry.Testing
{
	[TestedType(typeof(ExitControlGroupNotification))]
	sealed class ExitControlGroupNotificationTest : Enterprise.Registry.Business.Testing.RegistryBusinessObjectTemplateTestCase<ExitControlGroupNotification>
	{
		public void TestValidateSendMode() => AssertEquals("NOE, ESM, ENG, ESG", BizObj.SendModeList.CodesAsString);
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override ExitControlGroupNotification GetBusinessObjectToClone() => new ExitControlGroupNotification();
		protected override ExitControlGroupNotification GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
