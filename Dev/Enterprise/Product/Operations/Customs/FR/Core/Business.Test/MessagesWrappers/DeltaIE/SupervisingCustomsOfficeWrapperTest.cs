using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class SupervisingCustomsOfficeWrapperTest : DataProviderTestCase<SupervisingCustomsOfficeWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal declaration customsOffice with role = CAU.", "FR000001", Provider.ReferenceNumber);
		}

		protected override SupervisingCustomsOfficeWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsOffices.RemoveAndDeleteAll();
			var supervisingOffice = declaration.CustomsOffices.AddNew();
			supervisingOffice.CY_Code = Enterprise.Customs.EU.Business.EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep;
			supervisingOffice.CY_Data = "FR000001";
			return SupervisingCustomsOfficeWrapper.New(declaration);
		}
	}
}
