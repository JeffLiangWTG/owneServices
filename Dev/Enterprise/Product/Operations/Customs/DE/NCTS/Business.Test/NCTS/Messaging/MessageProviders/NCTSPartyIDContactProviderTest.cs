using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSPartyIDContactProviderTest : Customs.Business.Testing.DataProviderTestCase<INCTSPartyIDContact>
	{
		public void TestName_ProvideContactFalse_Contact() => CombineAssertions(() =>
		{
			staff = null;
			var provider = GetProviderWithProvideContactFalse();
			AssertNull("Name", provider.Name);
			AssertNull("PhomeNumber", provider.PhoneNumber);
			AssertNull("MailAddress", provider.MailAddress);
		});

		public void TestName_ProvideContactFalse_Staff() => CombineAssertions(() =>
		{
			var provider = GetProviderWithProvideContactFalse();
			AssertNull("Name", provider.Name);
			AssertNull("PhomeNumber", provider.PhoneNumber);
			AssertNull("MailAddress", provider.MailAddress);
		});

		public void TestName_Contact()
		{
			staff = null;
			AssertEquals("Fritz", Provider.Name);
		}

		public void TestName_Staff()
		{
			AssertEquals("Franz", Provider.Name);
		}

		public void TestMailAddress_Contact()
		{
			staff = null;
			AssertEquals("fritz@domain.org", Provider.MailAddress);
		}

		public void TestMailAddress_Staff()
		{
			AssertEquals("franz@spam.com", Provider.MailAddress);
		}

		public void TestPhoneNumber_Contact()
		{
			staff = null;
			AssertEquals("+491234567", Provider.PhoneNumber);
		}

		public void TestPhoneNumber_Staff()
		{
			AssertEquals("+441234567890", Provider.PhoneNumber);
		}

		public void TestNoContact_Contact() => CombineAssertions(() =>
		{
			orgAddress.OA_Address1 = "Address 1";
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			var provider = NCTSPartyIDContactProvider.NewOrNull(docAddress, provideContact: false);

			AssertNull("Name", provider.Name);
			AssertNull("PhoneNumber", provider.PhoneNumber);
			AssertNull("MailAddress", provider.MailAddress);
			AssertNotNull("MailAddress", provider.EoriNumber);
		});

		public void TestNoContact_Staff() => CombineAssertions(() =>
		{
			orgAddress.OA_Address1 = "Address 1";
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			var provider = NCTSPartyIDContactProvider.NewOrNull(docAddress, staff, provideContact: false);

			AssertNull("Name", provider.Name);
			AssertNull("PhoneNumber", provider.PhoneNumber);
			AssertNull("MailAddress", provider.MailAddress);
			AssertNotNull("MailAddress", provider.EoriNumber);
		});

		public void TestEoriNumber_EORI() => CombineAssertions(() =>
		{
			orgAddress.OA_Address1 = "Address 1";
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "987654321");
			AssertEquals("GR123456789", Provider.EoriNumber);
			AssertEquals("0001", Provider.EoriBranchSuffix);
		});

		public void TestEoriNumber_TCU() => CombineAssertions(() =>
		{
			orgAddress.OA_Address1 = "Address 1";
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "987654321");
			AssertEquals("GR987654321", Provider.EoriNumber);
			AssertNull(Provider.EoriBranchSuffix);
		});

		public void TestEquals() => CombineAssertions(() =>
		{
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			var provider1 = NCTSPartyIDContactProvider.NewOrNull(docAddress, staff);
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_FullName = "Franz";
			staff2.GS_WorkPhone = "+441234567890";
			staff2.GS_EmailAddress = "franz@spam.com";

			var provider2 = NCTSPartyIDContactProvider.NewOrNull(docAddress, staff2);
			AssertEquals("same address, same staff", true, provider1.Equals(provider2));

			staff2.GS_FullName = "Fritz";
			provider2 = NCTSPartyIDContactProvider.NewOrNull(docAddress, staff2);
			AssertEquals("same address, different staff", false, provider1.Equals(provider2));
		});

		public void TestGetSelectedAddressContact()
		{
			docAddress.E2_Contact = "Julian";

			var provider = NCTSPartyIDContactProvider.NewOrNull(docAddress, false);

			AssertEquals("Julian", provider.Name);
			AssertEquals("julian@test.com", provider.MailAddress);
			AssertEquals("+4900998877", provider.PhoneNumber);
		}

		public void TestNoContactSelectedAndNoFallback()
		{
			var provider = NCTSPartyIDContactProvider.NewOrNull(docAddress, fallback: false);

			AssertEquals(null, provider.Name);
			AssertEquals(null, provider.MailAddress);
			AssertEquals(null, provider.PhoneNumber);
		}

		public void TestNoContactSelectedAndFallbackEnabled()
		{
			var provider = NCTSPartyIDContactProvider.NewOrNull(docAddress, fallback: true);

			AssertEquals("Fritz", provider.Name);
			AssertEquals("fritz@domain.org", provider.MailAddress);
			AssertEquals("+491234567", provider.PhoneNumber);
		}

		public void TestGetSelectedAddressContact_UseCUSAllocationContact()
		{
			var provider = NCTSPartyIDContactProvider.NewOrNull(docAddress, staff: null, provideContact: true);

			AssertEquals("Fritz", provider.Name);
			AssertEquals("fritz@domain.org", provider.MailAddress);
			AssertEquals("+491234567", provider.PhoneNumber);
		}

		public void TestUseContactDetailsFromOverriddenAddress()
		{
			docAddress.E2_AddressOverride = true;

			docAddress.E2_Contact = "Jim";
			docAddress.E2_Email = "jim@mail.com";
			docAddress.E2_Phone = "+353871234567";

			var provider = NCTSPartyIDContactProvider.NewOrNull(docAddress, fallback: false);

			AssertEquals("Jim", provider.Name);
			AssertEquals("jim@mail.com", provider.MailAddress);
			AssertEquals("+353871234567", provider.PhoneNumber);
		}

		#region to be deleted
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3005:Don't call ToString() on a string.", Justification = "Testing. To avoid CS0201 diagnostic error.")]
		public void TestFacsimileNumber()
		{
			AssertExceptionThrown<NotImplementedException>(() => Provider.FacsimileNumber.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3005:Don't call ToString() on a string.", Justification = "Testing. To avoid CS0201 diagnostic error.")]
		public void TestPosition()
		{
			AssertExceptionThrown<NotImplementedException>(() => Provider.Position.ToString());
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress = orgHeader.Addresses.AddNew();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.Allocations.AddNew().PC_Type = "CUS";
			orgContact.OC_ContactName = "Fritz";
			orgContact.OC_Email = "fritz@domain.org";
			orgContact.OC_Phone = "+491234567";

			var orgContractWithoutAllocation = orgHeader.Contacts.AddNew();
			orgContractWithoutAllocation.OC_ContactName = "Julian";
			orgContractWithoutAllocation.OC_Email = "julian@test.com";
			orgContractWithoutAllocation.OC_Phone = "+4900998877";

			docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;

			staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Franz";
			staff.GS_WorkPhone = "+441234567890";
			staff.GS_EmailAddress = "franz@spam.com";
		}

		void CreateRegistrationNumber(string identificationType, string identificationNumber, string ebs = null)
		{
			orgHeader.CustomsCodes.AddNew(identificationType, identificationNumber, Core.Constants.CountryCodes.Greece);
			if (ebs != null)
			{
				orgAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
				var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebs, Core.Constants.CountryCodes.Germany);
				ebsCode.OK_OA_PremisesAddress = orgAddress.PK;
			}
		}

		INCTSPartyIDContact GetProviderWithProvideContactFalse() => NCTSPartyIDContactProvider.NewOrNull(docAddress, staff, false);

		JobDocAddress docAddress;
		GlbStaff staff;
		OrgHeader orgHeader;
		OrgAddress orgAddress;

		protected override INCTSPartyIDContact GetProvider() => NCTSPartyIDContactProvider.NewOrNull(docAddress, staff);
	}
}
