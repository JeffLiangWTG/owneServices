using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	[TestedType(typeof(DeltaIEFREDIMessage))]
	class DeltaIEFREDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessageDataObject()
		{
			var message = Factory.New<DeltaIEFREDIMessage>();
			AssertType<DeltaIEMessageDataObject>(message.MessageDataObject);
		}
	}
}
