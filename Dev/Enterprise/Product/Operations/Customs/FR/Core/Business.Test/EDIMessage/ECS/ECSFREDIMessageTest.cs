using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(ECSFREDIMessage))]
	public class ECSFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageEXP = Factory.New<ECSFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.ECS, messageEXP.EM_MessageType);
		}
	}
}
