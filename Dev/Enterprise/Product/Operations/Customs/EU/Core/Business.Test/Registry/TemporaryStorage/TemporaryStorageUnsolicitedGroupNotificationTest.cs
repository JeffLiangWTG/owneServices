using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(TemporaryStorageUnsolicitedGroupNotification))]
	class TemporaryStorageUnsolicitedGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<TemporaryStorageUnsolicitedGroupNotification>
	{
		public void TestValidateSendMode()
		{
			AssertEquals("NOE, ENG", BizObj.SendModeList.CodesAsString);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override TemporaryStorageUnsolicitedGroupNotification GetBusinessObjectToClone()
		{
			return new TemporaryStorageUnsolicitedGroupNotification();
		}

		protected override TemporaryStorageUnsolicitedGroupNotification GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
