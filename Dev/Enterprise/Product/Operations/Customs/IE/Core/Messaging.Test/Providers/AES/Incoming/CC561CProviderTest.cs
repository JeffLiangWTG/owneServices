using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC561C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC561CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestControlNotificationDateAndTime()
		{
			AssertEquals(ZDateTime.BrettsBirthday, provider.ControlNotificationDateAndTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC561CProvider(new Cc561C
			{
				ExportOperation = new ExportOperationType23
				{
					Mrn = "MRN",
					ControlNotificationDateAndTime = ZDateTime.BrettsBirthday.ToDateTime()
				}
			});
		}
		CC561CProvider provider;
	}
}
