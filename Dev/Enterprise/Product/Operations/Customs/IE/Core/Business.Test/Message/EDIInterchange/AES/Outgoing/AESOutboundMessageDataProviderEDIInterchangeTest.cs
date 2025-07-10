using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AESOutboundMessageDataProviderEDIInterchange))]
	class AESOutboundMessageDataProviderEDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<AESOutboundMessageDataProviderEDIInterchange>();
			AssertEquals("EI_ApplicationCode", ApplicationCodes.IECustomsExport, interchange.EI_ApplicationCode);
		}
	}
}
