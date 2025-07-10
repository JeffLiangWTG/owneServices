using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonRepresentativeWrapperTest : WrapperHelperTest<NCTS5CommonRepresentativeWrapper>
	{
		public void TestGetNewNCTS5CommonRepresentativeWrapper()
		{
			CombineAssertions(() =>
			{
				AssertNull("JobDocAddress null", NCTS5CommonRepresentativeWrapper.New(null));

				var docAddress = Factory.New<JobDocAddress>();
				AssertNull("JobDocAddress with no org address null", NCTS5CommonRepresentativeWrapper.New(docAddress));

				var address = Factory.New<OrgAddress>();
				docAddress.E2_OA_Address = address.PK;
				AssertNull("JobDocAddress with org address but no OrgHeader null", NCTS5CommonRepresentativeWrapper.New(docAddress));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", NCTS5CommonRepresentativeWrapper.New(address));
				AssertNotNull("JobDocAddress not nullnull", NCTS5CommonRepresentativeWrapper.New(docAddress));
			});
		}

		public void TestStatus()
		{
			AssertEquals("Expected filled Status always with value 2", "2", wrapper.Status);
		}

		public void TestContactPerson()
		{
			var contactPerson = wrapper.ContactPerson;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

				wrapper = GetWrapper(docAddress);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Expected filled name from Organzation/Details when not is override", "Org Name", contactPerson.Name);
				AssertEquals("Expected filled Email from Organzation/Details when not is override", "Email", contactPerson.Email);
				AssertEquals("Expected filled Phone from Organzation/Details when not is override", "Phone", contactPerson.PhoneNumber);

				SetOverrideToTrue();
				wrapper = GetWrapper(docAddress);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Expected filled name when is override", "Contact Override", contactPerson.Name);
				AssertEquals("Expected filled Email when is override", "Email Override", contactPerson.Email);
				AssertEquals("Expected filled Phone when is override", "Phone Override", contactPerson.PhoneNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			docAddress = Factory.New<JobDocAddress>();
			var orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			orgHeader.OH_FullName = "Org Name";
			orgAddress.OA_Email = "Email";
			orgAddress.OA_Phone = "Phone";
			orgAddress.OA_OH = orgHeader.PK;

			wrapper = NCTS5CommonRepresentativeWrapper.New(docAddress);
		}
		JobDocAddress docAddress;
		NCTS5CommonRepresentativeWrapper wrapper;

		void SetOverrideToTrue()
		{
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Contact = "Contact Override";
			docAddress.E2_Phone = "Phone Override";
			docAddress.E2_Email = "Email Override";
		}

		NCTS5CommonRepresentativeWrapper GetWrapper(JobDocAddress docAddress) => NCTS5CommonRepresentativeWrapper.New(docAddress);

		protected override NCTS5CommonRepresentativeWrapper GetProvider() => wrapper;
	}
}
