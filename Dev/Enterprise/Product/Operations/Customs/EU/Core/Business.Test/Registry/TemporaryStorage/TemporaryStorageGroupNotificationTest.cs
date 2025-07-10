using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(TemporaryStorageGroupNotification))]
	class TemporaryStorageGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<TemporaryStorageGroupNotification>
	{
		public void TestValidateSendMode()
		{
			AssertEquals("NOE, ESM, ENG, ESG", BizObj.SendModeList.CodesAsString);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override TemporaryStorageGroupNotification GetBusinessObjectToClone()
		{
			return new TemporaryStorageGroupNotification();
		}

		protected override TemporaryStorageGroupNotification GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
