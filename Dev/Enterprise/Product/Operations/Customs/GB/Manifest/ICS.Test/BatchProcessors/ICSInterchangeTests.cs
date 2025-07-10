using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.GB.ICS.Testing
{
	[TestedType(typeof(ICSInterchange))]
	public class ICSInterchangeTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<ICSInterchange>();
			AssertEquals(ApplicationCodes.GbCustomsDeclarationServices, interchange.EI_ApplicationCode);
		}

		public void TestGBCustomsBusinessResponse()
		{
			var interchange = Factory.New<ICSInterchange>();
			interchange.EI_BodyText = ICSGBCustomsBusinessResponseTest.NotificationResponseInterchangeBody;
			AssertNotNull(interchange.GBCustomsBusinessResponse);
		}

		public void TestShouldSendViaEHub()
		{
			var interchange = Factory.New<ICSInterchange>();
			AssertEquals(true, interchange.ShouldSendViaEHub);
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
