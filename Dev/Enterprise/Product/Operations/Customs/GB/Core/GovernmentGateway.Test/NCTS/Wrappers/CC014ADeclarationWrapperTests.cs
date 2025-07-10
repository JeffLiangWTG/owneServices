using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GB.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	[TestedType(typeof(CC014ADeclarationWrapper))]
	class CC014ADeclarationWrapperTests : DeclarationWrapperAbstractTest<CC014ADeclarationWrapper>
	{
		public void TestDateOfCancellationRequest()
		{
			AssertEquals(ZDateTime.Today.ToString("yyyyMMdd"), wrapper.DateOfCancellationRequest);
		}

		public void TestCancellationReason()
		{
			header.ExplanationToCustomsForWhyCancelling = "Cancellation reason";
			AssertEquals("Cancellation reason", wrapper.CancellationReason);
		}

		public void TestCancellationReasonLanguage()
		{
			AssertEquals("", wrapper.CancellationReasonLanguage);
		}

		protected override CC014ADeclarationWrapper GetProvider() => wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			wrapper = new CC014ADeclarationWrapper(header);
		}

		NctsHeader header;
		CC014ADeclarationWrapper wrapper;
	}
}
