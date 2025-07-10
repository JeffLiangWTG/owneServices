using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class PWSUnknownRecordTest : TestCase
	{
		public void TestValidateRecordData()
		{
			NotificationBuffer notificationSubscriber = new NotificationBuffer();
			PWSUnknownRecord record = new PWSUnknownRecord("Whatever", notificationSubscriber);
			Assert("Should always return false", !record.HasErrors);
		}
	}
}
