using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(PartyProvider))]
	abstract class PartyProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : PartyProvider
	{
		public void TestName()
		{
			CombineAssertions(() =>
			{
				if (ExpectNameToBeNullWithIdentificationNumber)
				{
					AssertNullOrEmpty("Name is null or empty with IdentificationNumber", Provider.Name);
				}
				else
				{
					AssertEquals("Name is not null or empty with IdentificationNumber", ExpectedName, Provider.Name);
				}

				address.Organisation.CustomsCodes.RemoveAll();

				if (ExpectNameToBeNullWithoutIdentificationNumber)
				{
					AssertNullOrEmpty("Name is null or empty without IdentificationNumber", Provider.Name);
				}
				else
				{
					AssertEquals("Name is not null or empty without IdentificationNumber", ExpectedName, Provider.Name);
				}
			});
		}

		protected virtual bool ExpectNameToBeNullWithIdentificationNumber => true;

		protected virtual bool ExpectNameToBeNullWithoutIdentificationNumber => false;

		protected virtual string ExpectedName => "Name";

		public void TestAddress()
		{
			if (ExpectAddressToBeNullWithIdentificationNumber)
			{
				AssertNull("Address is null with IdentificationNumber", provider.Address);
			}
			else
			{
				AssertNotNull("Address is not null with IdentificationNumber", provider.Address);
			}

			provider = CreateProvider(address);

			address.Organisation.CustomsCodes.RemoveAll();

			if (ExpectAddressToBeNullWithoutIdentificationNumber)
			{
				AssertNull("Address is null without IdentificationNumber", provider.Address);
			}
			else
			{
				AssertNotNull("Address is not null without IdentificationNumber", provider.Address);
			}
		}

		protected virtual bool ExpectAddressToBeNullWithIdentificationNumber => true;

		protected virtual bool ExpectAddressToBeNullWithoutIdentificationNumber => false;

		public void TestContactPerson()
		{
			if (ExpectProviderIncludesContactPerson)
			{
				AssertNotNull(Provider.ContactPerson);
			}
			else
			{
				AssertNull(Provider.ContactPerson);
			}
		}

		public void TestContactWithoutName()
		{
			if (ExpectContactToBeNullWithoutName || !ExpectProviderIncludesContactPerson)
			{
				address.E2_Contact = null;
				AssertNull(provider.ContactPerson);
			}
			else
			{
				AssertNotNull(provider.ContactPerson);
			}
		}
		protected virtual bool ExpectContactToBeNullWithoutName => true;

		public void TestContactWithoutPhone()
		{
			if (ExpectContactToBeNullWithoutPhone || !ExpectProviderIncludesContactPerson)
			{
				address.Contact.OC_Phone = null;
				AssertNull(provider.ContactPerson);
			}
			else
			{
				AssertNotNull(provider.ContactPerson);
			}
		}
		protected virtual bool ExpectContactToBeNullWithoutPhone => true;

		public void TestContactWithoutEmail()
		{
			if (ExpectContactToBeNullWithoutEmail || !ExpectProviderIncludesContactPerson)
			{
				AssertNull(provider.ContactPerson);
			}
			else
			{
				AssertNotNull(provider.ContactPerson);
			}
		}
		protected virtual bool ExpectContactToBeNullWithoutEmail => false;

		public void TestContactWhenExcludingContact()
		{
			var mockProvider = new Mock<PartyProvider>(new object[] { address, false });
			mockProvider.Protected()
				.Setup<ZBool>("IncludeContactPerson")
				.Returns(false);
			AssertNull(mockProvider.Object.ContactPerson);
			mockProvider.VerifyAll();
		}

		protected virtual bool ExpectProviderIncludesContactPerson => true;

		protected virtual T CreateProvider(JobDocAddress address) => (T)Activator.CreateInstance(typeof(T), address, false);

		protected override T GetProvider() => provider;

		protected virtual string AddressType => string.Empty;

		public void TestContactPersonProviderType()
		{
			AssertType(ExpectedContactPersonProviderType, Provider.ContactPerson);
		}
		protected virtual Type ExpectedContactPersonProviderType => ExpectProviderIncludesContactPerson ? typeof(ContactPersonProvider) : null;

		protected virtual void SetupAddress()
		{
			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "PartyID", Core.Constants.CountryCodes.Belgium)
			};
			var orgAddress = Factory.CreateOrgAddress("street");
			address = Factory.CreateJobDocAddress(addressType: AddressType, orgHeaderFullName: "Name", orgCusCodes: orgCusCodes, contactName: "ContactName", contactPhone: "ContactPhone", contactEmail: "ContactEmail", orgAddress: orgAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupAddress();
			provider = CreateProvider(address);
		}

		protected T provider;
		protected JobDocAddress address;
	}
}
