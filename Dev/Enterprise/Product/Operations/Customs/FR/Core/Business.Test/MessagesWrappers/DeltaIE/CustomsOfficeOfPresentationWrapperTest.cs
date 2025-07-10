using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CustomsOfficeOfPresentationWrapperTest : DataProviderTestCase<CustomsOfficeOfPresentationWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal declaration JE_CustomsOffice", "FR0000001", Provider.ReferenceNumber);
		}

		protected override CustomsOfficeOfPresentationWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "FR0000001";
			return CustomsOfficeOfPresentationWrapper.New(declaration);
		}
	}
}
