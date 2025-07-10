using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestsSubclassesOf(typeof(PartyProvider))]
	abstract class PartyProviderAbstractTest<T> : DataProviderTestCase<T> where T : PartyProvider
	{
		public void TestContactPerson()
		{
			AssertNull(provider.ContactPerson);
		}

		public virtual void TestAddress()
		{
			var orgAddress = Factory.CreateOrgAddress("street");
			address.E2_OA_Address = orgAddress.PK;

			if (ExpectProviderIncludesAddress)
			{
				AssertNotNull(provider.Address);
			}
			else
			{
				AssertNull(provider.Address);
			}
		}

		public virtual void TestIsEmpty()
		{
			var orgAddress = Factory.CreateOrgAddress("");
			address.E2_OA_Address = orgAddress.PK;
			AssertEquals(true, Provider.IsEmpty);
			AssertNull(Provider.Address);
			orgAddress = Factory.CreateOrgAddress("street");
			address.E2_OA_Address = orgAddress.PK;
			var provider = CreateProvider(address);
			AssertEquals(false, provider.IsEmpty);
		}

		public void TestAddressWithoutIdentificationNumberWhenExcludingParty()
		{
			var orgAddress = Factory.CreateOrgAddress("street");
			var data = Factory.CreateJobDocAddress(orgHeaderFullName: "Name", orgAddress: orgAddress);
			var mockProvider = new Mock<PartyProvider>(new object[] { data, false });
			mockProvider.Protected()
				.Setup<ZBool>("IncludeAddress")
				.Returns(false);
			mockProvider.SetupGet(m => m.Address).CallBase();
			AssertNull(mockProvider.Object.Address);
			mockProvider.VerifyAll();
		}

		protected virtual bool ExpectProviderIncludesAddress => true;

		protected virtual T CreateProvider(JobDocAddress address) => (T)Activator.CreateInstance(typeof(T), address, false);

		protected override T GetProvider() => provider;

		protected virtual string AddressType => string.Empty;

		protected override void SetUp()
		{
			base.SetUp();

			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI175521246821", Core.Constants.CountryCodes.UnitedKingdom)
			};
			var orgAddress = Factory.CreateOrgAddress("street");
			address = Factory.CreateJobDocAddress(addressType: AddressType, orgHeaderFullName: "Name", orgCusCodes: orgCusCodes, contactName: "ContactName", contactPhone: "ContactPhone", contactEmail: "ContactEmail", orgAddress: orgAddress);

			provider = CreateProvider(address);
		}

		protected T provider;
		protected JobDocAddress address;
	}
}
