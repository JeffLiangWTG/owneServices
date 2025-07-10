using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(ECSDepartureFREDIMessage))]
	public class ECSDepartureFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageEXP = Factory.New<ECSDepartureFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.ECS, messageEXP.EM_MessageType);
			AssertEquals("Default message sub type", MessageSubTypeList.Codes.DEP, messageEXP.EM_MessageSubType);
		}
	}
}
