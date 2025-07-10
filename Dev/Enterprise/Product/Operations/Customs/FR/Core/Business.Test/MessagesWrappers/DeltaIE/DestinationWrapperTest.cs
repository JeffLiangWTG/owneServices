using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class DestinationWrapperTest : Customs.Business.Testing.DataProviderTestCase<DestinationWrapper>
	{
		JobDeclaration jobDeclaration;

		protected override DestinationWrapper GetProvider()
		{
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			jobDeclaration.JE_CustomsOffice = "IT123";
			return DestinationWrapper.New(jobDeclaration);
		}

		public void TestCountryOfDestination()
		{
			AssertEquals("CountryOfDestination should be equal to JE_GoodsDestination.", Core.Constants.CountryCodes.France, Provider.CountryOfDestination);
		}

		public void TestRegionOfDestination()
		{
			AssertEquals("RegionOfDestination should be empty when E2_OA_Address does not exist.", ZString.Empty, Provider.RegionOfDestination);

			var declaration = Factory.New<JobDeclaration>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var address = importer.MainAddress;
			address.OA_PostCode = "24140";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = address.PK;

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Italy;
			var provider = DestinationWrapper.New(declaration);
			AssertEquals("RegionOfDestination should be empty when CountryOfDestination is not FR.", ZString.Empty, provider.RegionOfDestination);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			provider = DestinationWrapper.New(declaration);
			AssertEquals("RegionOfDestination should be equal to first two digits of OA_PostCode when CountryOfDestination is FR.", "24", provider.RegionOfDestination);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be FR when JE_CustomsOffice does not start with FR.", Core.Constants.CountryCodes.France, Provider.CcQualifier);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			declaration.JE_CustomsOffice = "FR123";

			var provider = DestinationWrapper.New(declaration);
			AssertEquals("CcQualifier should be empty when JE_CustomsOffice starts with FR.", ZString.Empty, provider.CcQualifier);
		}
	}
}
