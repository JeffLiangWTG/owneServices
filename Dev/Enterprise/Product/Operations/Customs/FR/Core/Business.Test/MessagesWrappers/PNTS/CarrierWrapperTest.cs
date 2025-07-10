using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class CarrierWrapperTest : Customs.Business.Testing.DataProviderTestCase<CarrierWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal EORI from the carrier address.", "FR12345678900001", Provider.IdentificationNumber);
		}

		public void TestName()
		{
			AssertEquals("Name should equal carrier OH_FullName.", "CarrierName", Provider.Name);
		}

		protected override CarrierWrapper GetProvider()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_FullName = "CarrierName";

			var carrierAddress = carrier.MainAddress;
			carrierAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			carrierAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);

			return CarrierWrapper.New(carrierAddress);
		}
	}
}
