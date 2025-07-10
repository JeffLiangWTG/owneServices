using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class PlaceOfProcessingProviderTest : DataProviderTestCase<PlaceOfProcessingProvider>
	{
		public void TestIdentificationOfLocation()
		{
			placeOfUseOrProcessing.CGL_AdditionalIdentifier = "A";

			placeOfUseOrProcessing.CGL_Qualifier = "U";
			var provider = GetProvider();
			AssertEquals("WHen CGL_Qualifier == U", placeOfUseOrProcessing.Unlocode, provider.IdentificationOfLocation);

			placeOfUseOrProcessing.CGL_Qualifier = "V";
			provider = GetProvider();
			AssertEquals("WHen CGL_Qualifier == V", placeOfUseOrProcessing.CGL_CustomsOffice, provider.IdentificationOfLocation);

			placeOfUseOrProcessing.CGL_Qualifier = "X";
			placeOfUseOrProcessing.Address.E2_GovRegNum = "12";
			provider = GetProvider();
			AssertEquals("WHen CGL_Qualifier == X", placeOfUseOrProcessing.Address.E2_GovRegNum, provider.IdentificationOfLocation);

			placeOfUseOrProcessing.CGL_Qualifier = "Y";
			placeOfUseOrProcessing.Address.AuthorisationNumber = "001";
			provider = GetProvider();
			AssertEquals("WHen CGL_Qualifier == Y", placeOfUseOrProcessing.Address.AuthorisationNumber, provider.IdentificationOfLocation);
		}

		public void TestQualifierIdentification()
		{
			AssertEquals("QualifierIdentification", "U", Provider.QualifierIdentification);
		}

		public void TestAdditionalIdentifier()
		{
			AssertEquals("AdditionalIdentifier", "AddIdentifier", Provider.AdditionalIdentifier);
		}

		public void TestLocationTypeCode()
		{
			AssertEquals("LocationTypeCode", "B", Provider.LocationTypeCode);
		}

		public void TestAddress()
		{
			AssertType<AddressProvider>(Provider.Address);
		}

		protected override PlaceOfProcessingProvider GetProvider() => PlaceOfProcessingProvider.New(placeOfUseOrProcessing);

		protected override void SetUp()
		{
			base.SetUp();
			var instruction = Factory.New<CusEntryInstruction>();
			placeOfUseOrProcessing = instruction.PlaceOfUseOrProcessingCollection.AddNew();
			placeOfUseOrProcessing.CGL_Qualifier = "U";
			placeOfUseOrProcessing.CGL_Type = "B";
			placeOfUseOrProcessing.AdditionalIdentifier = "AddIdentifier";
		}
		PlaceOfUseOrProcessing placeOfUseOrProcessing;
	}
}
