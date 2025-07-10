using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Registry.Testing
{
	[TestedType(typeof(EmcsGroupNotification))]
	class EmcsGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<EmcsGroupNotification>
	{
		public void TestValidateSendMode()
		{
			AssertEquals("NOE, ESM, ENG, ESG", BizObj.SendModeList.CodesAsString);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override EmcsGroupNotification GetBusinessObjectToClone() => new EmcsGroupNotification();

		protected override EmcsGroupNotification GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
