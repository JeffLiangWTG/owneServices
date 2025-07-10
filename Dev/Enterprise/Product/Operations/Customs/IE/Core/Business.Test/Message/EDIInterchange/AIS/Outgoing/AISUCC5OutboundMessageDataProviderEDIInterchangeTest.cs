using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AISUCC5OutboundMessageDataProviderEDIInterchange))]
	class AISUCC5OutboundMessageDataProviderEDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<AISUCC5OutboundMessageDataProviderEDIInterchange>();
			AssertEquals("EI_ApplicationCode", ApplicationCodes.IECustomsUCC5Import, interchange.EI_ApplicationCode);
		}
	}
}
