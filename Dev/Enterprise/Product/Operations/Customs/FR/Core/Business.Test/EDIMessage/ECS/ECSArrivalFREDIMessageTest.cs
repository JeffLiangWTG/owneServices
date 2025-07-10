using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(ECSArrivalFREDIMessage))]
	public class ECSArrivalFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValue()
		{
			var messageEXP = Factory.New<ECSArrivalFREDIMessage>();
			AssertEquals("Default message type", MessageTypeList.Codes.ECS, messageEXP.EM_MessageType);
			AssertEquals("Default message sub type", MessageSubTypeList.Codes.ARR, messageEXP.EM_MessageSubType);
		}
	}
}
