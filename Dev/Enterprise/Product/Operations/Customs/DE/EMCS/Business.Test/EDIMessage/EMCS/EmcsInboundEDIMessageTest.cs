using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(EmcsInboundEDIMessage<IEmcsDataProvider>))]
	public class EmcsInboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsEmcsSystem, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", DE.Messaging.EDIMessageTypeList.Codes.EMCS, message.EM_MessageType);
			});
		}

		public void TestEM_LinkedObject_EMCSJobDeclaration()
		{
			var emcsJobDeclaration = Factory.New<EMCSJobDeclaration>();
			message.EM_LinkTable = emcsJobDeclaration.TableName;
			message.EM_LinkUniqueID = emcsJobDeclaration.PK;
			AssertEquals(emcsJobDeclaration, message.EM_LinkedObject);
		}

		public void TestEM_MessageInterpretation()
		{
			message.EM_MessageInterpretation = "<Node>Test text</Node><Node>Test text</Node>";
			AssertContains("Interpretation should be encoded to html format", "&lt;Node&gt;Test text&lt;/Node&gt;\n&lt;Node&gt;Test text&lt;/Node&gt;", message.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<EmcsInboundEDIMessage<IEmcsDataProvider>>();
		}
		EmcsInboundEDIMessage<IEmcsDataProvider> message;
	}
}
