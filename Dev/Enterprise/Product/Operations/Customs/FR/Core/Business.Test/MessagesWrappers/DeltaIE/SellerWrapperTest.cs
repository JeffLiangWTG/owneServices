using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class SellerWrapperTest : Customs.Business.Testing.DataProviderTestCase<SellerWrapper>
	{
		#region IdentificationNumber

		public void TestIdentificationNumber()
		{
			//this needs refactor
			TestIdentificationNumberWhenSellerHasEORI();
			TestIdentificationNumberWhenSellerHasNoEORI();
		}

		void TestIdentificationNumberWhenSellerHasEORI()
		{
			AssertEquals("IdentificationNumber should equal seller EORI.", "FR12345678900001", GetAlternateProvider().IdentificationNumber);
		}

		void TestIdentificationNumberWhenSellerHasNoEORI()
		{
			AssertEquals("IdentificationNumber should not be mapped when seller has no EORI.", string.Empty, Provider.IdentificationNumber);
		}

		#endregion

		#region Name

		public void TestName()
		{
			TestNameWhenSellerHasEORI();
			TestNameWhenSellerHasNoEORI();
		}

		void TestNameWhenSellerHasEORI()
		{
			AssertEquals("Name not be mapped when seller has EORI.", null, GetAlternateProvider().Name);
		}

		void TestNameWhenSellerHasNoEORI()
		{
			AssertEquals("Name should equal seller OH_FullName.", "Name", Provider.Name);
		}

		#endregion

		#region Address

		public void TestAddress()
		{
			TestAddressWhenSellerHasEORI();
			TestAddressWhenSellerHasNoEORI();
		}

		void TestAddressWhenSellerHasEORI()
		{
			AssertNull("Address should not be mapped when seller has EORI", GetAlternateProvider().Address);
		}

		void TestAddressWhenSellerHasNoEORI()
		{
			var address = Provider.Address;
			AssertType<OrganisationAddressWrapper>("Address should be of type AddressWrapper.", address);
			AssertEquals("Postcode should be equal to seller.MainAddress.Postcode.", "24750", address.Postcode);
		}

		#endregion

		#region Setup

		protected override SellerWrapper GetProvider()
		{
			var seller = GetSeller();
			return SellerWrapper.New(seller.MainAddress);
		}

		SellerWrapper GetAlternateProvider()
		{
			//this needs refactor
			var seller = GetSeller();
			seller.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			seller.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			return SellerWrapper.New(seller.MainAddress);
		}

		OrgHeader GetSeller()
		{
			var seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "Name";
			seller.MainAddress.Postcode = "24750";
			return seller;
		}

		#endregion
	}
}
