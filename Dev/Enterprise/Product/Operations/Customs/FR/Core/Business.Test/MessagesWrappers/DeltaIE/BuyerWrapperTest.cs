using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class BuyerWrapperTest : Customs.Business.Testing.DataProviderTestCase<BuyerWrapper>
	{
		#region IdentificationNumber

		public void TestIdentificationNumber()
		{
			TestIdentificationNumberWhenBuyerHasEORI();
			TestIdentificationNumberWhenBuyerHasNoEORI();
		}

		void TestIdentificationNumberWhenBuyerHasEORI()
		{
			AssertEquals("IdentificationNumber should equal buyer EORI.", "FR12345678900001", GetAlternateProvider().IdentificationNumber);
		}

		void TestIdentificationNumberWhenBuyerHasNoEORI()
		{
			AssertEquals("IdentificationNumber should not be mapped when buyer has no EORI.", string.Empty, Provider.IdentificationNumber);
		}

		#endregion

		#region Name

		public void TestName()
		{
			TestNameWhenBuyerHasEORI();
			TestNameWhenBuyerHasNoEORI();
		}

		void TestNameWhenBuyerHasEORI()
		{
			AssertEquals("Name not be mapped when buyer has EORI.", null, GetAlternateProvider().Name);
		}

		void TestNameWhenBuyerHasNoEORI()
		{
			AssertEquals("Name should equal buyer OH_FullName.", "Name", Provider.Name);
		}

		#endregion

		#region Address

		public void TestAddress()
		{
			TestAddressWhenBuyerHasEORI();
			TestAddressWhenBuyerHasNoEORI();
		}

		void TestAddressWhenBuyerHasEORI()
		{
			AssertNull("Address should not be mapped when buyer has EORI", GetAlternateProvider().Address);
		}

		void TestAddressWhenBuyerHasNoEORI()
		{
			var address = Provider.Address;
			AssertType<OrganisationAddressWrapper>("Address should be of type AddressWrapper.", address);
			AssertEquals("Postcode should be equal to buyer.MainAddress.Postcode.", "24750", address.Postcode);
		}

		#endregion

		#region Setup

		protected override BuyerWrapper GetProvider()
		{
			var buyer = GetBuyer();
			return BuyerWrapper.New(buyer.MainAddress);
		}

		BuyerWrapper GetAlternateProvider()
		{
			var buyer = GetBuyer();
			buyer.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			buyer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			return BuyerWrapper.New(buyer.MainAddress);
		}

		OrgHeader GetBuyer()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "Name";
			buyer.MainAddress.Postcode = "24750";
			return buyer;
		}

		#endregion
	}
}
