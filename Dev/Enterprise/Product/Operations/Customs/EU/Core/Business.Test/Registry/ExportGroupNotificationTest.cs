using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(ExportGroupNotification))]
	sealed class ExportGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<ExportGroupNotification>
	{
		public void TestValidateSendMode()
		{
			AssertEquals("NOE, ESM, ENG, ESG", BizObj.SendModeList.CodesAsString);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override ExportGroupNotification GetBusinessObjectToClone() => new ExportGroupNotification();

		protected override ExportGroupNotification GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
