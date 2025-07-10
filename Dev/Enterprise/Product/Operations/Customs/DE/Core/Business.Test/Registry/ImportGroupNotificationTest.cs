using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(ImportGroupNotification))]
	class ImportGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<ImportGroupNotification>
	{
		public void TestValidateSendMode()
		{
			AssertEquals("NOE, ESM, ENG, ESG", BizObj.SendModeList.CodesAsString);
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override ImportGroupNotification GetBusinessObjectToClone() => new ImportGroupNotification();

		protected override ImportGroupNotification GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
