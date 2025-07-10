using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSInboundEDIMessage))]
	sealed class EMCSInboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.GbCustomsEMCS, message.EM_ApplicationCode);
			AssertNull(message.LinkedDeclaration);
		}

		public void TestLinkedDeclaration()
		{
			CombineAssertions(() =>
			{
				message.EM_LinkedObject = Factory.New<CusEntryHeader>();
				AssertNull(message.LinkedDeclaration);

				var declaration = Factory.New<EMCSJobDeclaration>();
				message.EM_LinkedObject = declaration;
				AssertNotNull(message.LinkedDeclaration);
				AssertType<EMCSJobDeclaration>(message.LinkedDeclaration);
				AssertEquals(declaration, message.LinkedDeclaration);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<EMCSInboundEDIMessage>();
		}

		EMCSInboundEDIMessage message;
	}
}
