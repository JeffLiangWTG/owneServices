using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class G3CodedGenericLocationWrapperTest : TestCaseWithFactory
	{
		public void TestAuthorisationNumber()
		{
			AssertEquals("Expected filled AuthorisationNumber", "ESAH3", wrapper.AuthorisationNumber);
		}

		public void TestUNLOCOCode()
		{
			AssertNullOrEmpty("Expected UNLOCOCode to be empty", wrapper.UNLOCOCode);
		}

		public void TestCustomsOffice()
		{
			AssertNullOrEmpty("Expected CustomsOffice to be empty", wrapper.CustomsOffice);
		}

		public void TestGPS()
		{
			AssertNull("Expected GPS to be null", wrapper.GPS);
		}

		public void TestEconomicOperator()
		{
			AssertNullOrEmpty("Expected EconomicOperator to be empty", wrapper.EconomicOperator);
		}

		public void TestAdditionalId()
		{
			AssertNullOrEmpty("Expected AdditionalId to be empty", wrapper.AdditionalId);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.CusGoodsLocation.Address.AuthorisationNumber = "ESAH3";

			wrapper = new G3CodedGenericLocationWrapper(header);
		}

		G3CodedGenericLocationWrapper wrapper;
	}
}
