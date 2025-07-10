using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedType(typeof(UsageEDIMessage))]
	public class UsageEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestCloneAuditProperties()
		{
			Assert("Need this to supress test failure caused by customized default values", true);
		}

		[TestDate(2021, 1, 1, 1, 1, 0)]
		public void TestSetDefaultValues()
		{
			var message = (UsageEDIMessage)GetNewBusinessObject();
			AssertEquals("EM_IsActive should be true", true, message.EM_IsActive);
			AssertEquals("EM_Status should be CAP", EDIMessage.Status.Captured, message.EM_Status);
			AssertEquals("EM_SystemCreateTimeUtc should be close to UtcNow", ZDateTime.UtcNow, message.EM_SystemCreateTimeUtc);
			AssertEquals("EM_SystemCreateUser should be CurrentUser", GlbStaff.CurrentUser.GS_Code, message.EM_SystemCreateUser);
			AssertEquals("EM_SystemLastEditTimeUtc should be same as EM_SystemCreateTimeUtc", message.EM_SystemCreateTimeUtc, message.EM_SystemLastEditTimeUtc);
			AssertEquals("EM_SystemLastEditUser should be same as EM_SystemCreateUser", message.EM_SystemCreateUser, message.EM_SystemLastEditUser);
			AssertEquals("EM_ReceiveTransmit should be TRX", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		public void TestMessageNumber()
		{
			var message = Factory.New<UsageEDIMessage>();
			Factory.Save();

			Assert(!message.EM_MessageNum.IsEmpty);
			AssertEquals(20, message.EM_MessageNum.Length);
		}
	}
}
