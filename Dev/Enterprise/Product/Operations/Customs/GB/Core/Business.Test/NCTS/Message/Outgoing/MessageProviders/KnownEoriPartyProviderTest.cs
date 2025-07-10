using System;
using System.Collections.Generic;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(KnownEoriPartyProvider))]
	class KnownEoriPartyProviderTest : PartyProviderAbstractTest<KnownEoriPartyProvider>
	{
		public void TestIncludeEORI()
		{
			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "175521246821", Core.Constants.CountryCodes.France)
			};
			var orgAddress = Factory.CreateOrgAddress("street");
			var address2 = Factory.CreateJobDocAddress(addressType: AddressType, orgHeaderFullName: "Name", orgCusCodes: orgCusCodes, contactName: "ContactName", contactPhone: "ContactPhone", contactEmail: "ContactEmail", orgAddress: orgAddress);

			var includeEoriProvider = CreateProvider(address2);

			GBCustomsDataRegistry.Instance.SuppressForeignEORI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("IncludeEORI should be true", provider.IncludeEORI);
			AssertEquals(false, includeEoriProvider.IncludeEORI);
		}

		public void TestIdentificationNumber()
		{
			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "175521246000", Core.Constants.CountryCodes.France)
			};
			var orgAddress = Factory.CreateOrgAddress("street");
			var address2 = Factory.CreateJobDocAddress(addressType: AddressType, orgHeaderFullName: "Name", orgCusCodes: orgCusCodes, contactName: "ContactName", contactPhone: "ContactPhone", contactEmail: "ContactEmail", orgAddress: orgAddress);

			var includeEoriProvider = CreateProvider(address2);

			GBCustomsDataRegistry.Instance.SuppressForeignEORI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("IdentificationNumber should not be empty", provider.IdentificationNumber, "XI175521246821");
			AssertEquals("IdentificationNumber should be empty", includeEoriProvider.IdentificationNumber, "");

			GBCustomsDataRegistry.Instance.SuppressForeignEORI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("IdentificationNumber should not be empty", includeEoriProvider.IdentificationNumber, "FR175521246000");
		}

		public void TestIdentificationNumberAndAddress()
		{
			GBCustomsDataRegistry.Instance.SuppressForeignEORI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertNotNull("IdentificationNumber should not be empty", provider.IdentificationNumber);
			AssertNull("Address should be null", provider.Address);
			AssertNull("Name should be null", provider.Name);

			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "test", Core.Constants.CountryCodes.UnitedKingdom)
			};
			var orgAddress = Factory.CreateOrgAddress("street");
			JobDocAddress address2 = Factory.CreateJobDocAddress(addressType: AddressType, orgHeaderFullName: "Name", orgCusCodes: orgCusCodes, contactName: "ContactName", contactPhone: "ContactPhone", contactEmail: "ContactEmail", orgAddress: orgAddress);

			KnownEoriPartyProvider provider2 = CreateProvider(address2);

			GBCustomsDataRegistry.Instance.SuppressForeignEORI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertNull("IdentificationNumber should be empty", provider2.IdentificationNumber);
			AssertNotNull("Address should not be null", provider2.Address);
			AssertNotNull("Name should be not null", provider2.Name);
		}

		protected override KnownEoriPartyProvider CreateProvider(JobDocAddress address)
		{
			return new KnownEoriPartyProvider(address, true, false);
		}
	}
}
