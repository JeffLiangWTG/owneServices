using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class PartyContactWrapperTest : WrapperHelperTest<PartyContactWrapper>
	{
		public void TestGetNewPartyContactWrapper()
		{
			OrgAddress orgAddress = null;
			AssertNull("OrgAddress null", PartyContactWrapper.New(orgAddress));
			orgAddress = Factory.New<OrgAddress>();
			AssertNull("OrgAddress not null but null Header", PartyContactWrapper.New(orgAddress));
			var orgHeader = Factory.New<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			AssertNotNull("OrgAddress not null and no null Header", PartyContactWrapper.New(orgAddress));

			AssertNotNull("Not null even if all arguments are empty", PartyContactWrapper.New(ZString.Empty, ZString.Empty, ZString.Empty));

			JobDocAddress jobDocAddress = null;
			AssertNull("JobDocAddress null", PartyContactWrapper.New(jobDocAddress));
			jobDocAddress = Factory.New<JobDocAddress>();
			AssertNull("JobDocAddress not null but null Header", PartyContactWrapper.New(jobDocAddress));
			var orgHeader1 = Factory.New<OrgHeader>();
			jobDocAddress.OrganisationPK = orgHeader1.PK;
			AssertNotNull("JobDocAddress not null and no null Header", PartyContactWrapper.New(jobDocAddress));
		}

		public void TestContactPerson()
		{
			wrapper = GetWrapperJobDocAddress(docAddress);
			var contactPerson = wrapper;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", contactPerson, contactPerson);

				orgHeader.OH_FullName = "Org Name";
				orgAddress.OA_Email = "Email";
				orgAddress.OA_Phone = "Phone";
				wrapper = GetWrapperJobDocAddress(docAddress);
				contactPerson = wrapper;
				AssertEquals("Expected filled name from Organization/Details when not is override", "Org Name", contactPerson.Name);
				AssertEquals("Expected filled Email from Organization/Details when not is override", "Email", contactPerson.Email);
				AssertEquals("Expected filled Phone from Organization/Details when not is override", "Phone", contactPerson.PhoneNumber);

				AddContactDetails();
				contactPerson = PartyContactWrapper.New(docAddress, false);
				AssertEquals("Expected filled name from Organization/Contact when not is override and contact selected", "Contact Selected", contactPerson.Name);
				AssertEquals("Expected filled Email from Organization/Contact when not is override and contact selected", "Email Selected", contactPerson.Email);
				AssertEquals("Expected filled Phone from Organization/Contact when not is override and contact selected", "Phone Selected", contactPerson.PhoneNumber);

				SetOverrideToTrue();
				wrapper = GetWrapperJobDocAddress(docAddress);
				contactPerson = wrapper;
				AssertEquals("Expected filled name when is override", "Contact Override", contactPerson.Name);
				AssertEquals("Expected filled Email when is override", "Email Override", contactPerson.Email);
				AssertEquals("Expected filled Phone when is override", "Phone Override", contactPerson.PhoneNumber);
			});
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				orgHeader.OH_FullName = "OrgHeader full name";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled Name from OrgAddress.Header", "OrgHeader full name", wrapper.Name);

				wrapper = PartyContactWrapper.New("Name", ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled Name from ZString", "Name", wrapper.Name);
			});
		}

		public void TestEmail()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Email = "mail.mail@mail.com";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled Email from dbo.OrgAddress", "mail.mail@mail.com", wrapper.Email);

				wrapper = PartyContactWrapper.New(ZString.Empty, "Email", ZString.Empty);
				AssertEquals("Expected filled Email from ZString", "Email", wrapper.Email);
			});
		}

		public void TestPhoneNumber()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Phone = "123456789";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled PhoneNumber from dbo.OrgAddress", "123456789", wrapper.PhoneNumber);

				wrapper = PartyContactWrapper.New(ZString.Empty, ZString.Empty, "PhoneNumber");
				AssertEquals("Expected filled PhoneNumber from ZString", "PhoneNumber", wrapper.PhoneNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			docAddress = Factory.New<JobDocAddress>();
			orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			orgHeader = Factory.New<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;

			wrapper = GetWrapper(orgAddress);
		}
		OrgAddress orgAddress;
		OrgHeader orgHeader;
		PartyContactWrapper wrapper;
		JobDocAddress docAddress;

		void AddContactDetails()
		{
			var contact = orgHeader.Contacts.AddNew();
			docAddress.ContactPK = contact.PK;

			contact.OC_ContactName = "Contact Selected";
			contact.OC_Phone = "Phone Selected";
			contact.OC_Email = "Email Selected";
		}

		void SetOverrideToTrue()
		{
			docAddress.E2_AddressOverride = true;
			var contact = orgHeader.Contacts.AddNew();
			docAddress.ContactPK = contact.PK;

			docAddress.E2_Contact = "Contact Override";
			docAddress.E2_Phone = "Phone Override";
			docAddress.E2_Email = "Email Override";
		}

		PartyContactWrapper GetWrapper(OrgAddress orgAddress) => PartyContactWrapper.New(orgAddress);

		PartyContactWrapper GetWrapperJobDocAddress(JobDocAddress jobDocAddress) => PartyContactWrapper.New(jobDocAddress);

		protected override PartyContactWrapper GetProvider() => wrapper;
	}
}
