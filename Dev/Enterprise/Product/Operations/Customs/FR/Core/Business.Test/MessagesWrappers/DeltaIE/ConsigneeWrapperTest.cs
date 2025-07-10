using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class ConsigneeWrapperTest : DataProviderTestCase<ConsigneeWrapper>
	{
		#region IdentificationNumber

		public void TestIdentificationNumber()
		{
			TestIdentificationNumberWhenExporterHasEORI();
			TestIdentificationNumberWhenExporterHasNoEORI();
		}

		void TestIdentificationNumberWhenExporterHasEORI()
		{
			AssertEquals("IdentificationNumber should equal consignee EORI.", "FR12345678900001", GetAlternateProvider().IdentificationNumber);
		}

		void TestIdentificationNumberWhenExporterHasNoEORI()
		{
			AssertEquals("IdentificationNumber should not be mapped when consignee has no EORI.", string.Empty, Provider.IdentificationNumber);
		}

		#endregion

		#region Name

		public void TestName()
		{
			TestNameWhenExporterHasEORI();
			TestNameWhenExporterHasNoEORI();
		}

		void TestNameWhenExporterHasEORI()
		{
			AssertEquals("Name not be mapped when consignee has EORI.", null, GetAlternateProvider().Name);
		}

		void TestNameWhenExporterHasNoEORI()
		{
			AssertEquals("Name should equal consignee OH_FullName.", "Name", Provider.Name);
		}

		#endregion

		#region Address

		public void TestAddress()
		{
			TestAddressWhenConsigneeHasEORI();
			TestAddressWhenConsigneeHasNoEORI();
		}

		void TestAddressWhenConsigneeHasEORI()
		{
			AssertNull("Address should not be mapped when consignee has EORI", GetAlternateProvider().Address);
		}

		void TestAddressWhenConsigneeHasNoEORI()
		{
			var address = Provider.Address;
			AssertType<OrganisationAddressWrapper>("Address should be of type AddressWrapper.", address);
			AssertEquals("Postcode should be equal to consignee.MainAddress.Postcode.", "24750", address.Postcode);
		}

		#endregion

		#region Setup

		protected override ConsigneeWrapper GetProvider()
		{
			var consignee = GetConsignee();

			return ConsigneeWrapper.New(consignee.MainAddress);
		}

		ConsigneeWrapper GetAlternateProvider()
		{
			var consignee = GetConsignee();
			consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			consignee.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			return ConsigneeWrapper.New(consignee.MainAddress);
		}

		OrgHeader GetConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Name";
			consignee.MainAddress.Postcode = "24750";
			return consignee;
		}

		#endregion
	}
}
