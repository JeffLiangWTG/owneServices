using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(CancelH7SendMessageWrapper))]
	class CancelH7SendMessageWrapperTest : H7CommonSendMessageWrapperBaseTest<CancelH7SendMessageWrapper>
	{
		public void TestDeclarationMRN()
		{
			AssertEquals("Expected filled DeclarationMRN", "testMRN", Provider.DeclarationMRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill.MovementReferenceNumber = "testMRN";
		}

		protected override CancelH7SendMessageWrapper GetWrapperCore(AsycudaBill bill, ICertificateProvider certificate)
		{
			return new CancelH7SendMessageWrapper(bill, certificate);
		}
	}
}
