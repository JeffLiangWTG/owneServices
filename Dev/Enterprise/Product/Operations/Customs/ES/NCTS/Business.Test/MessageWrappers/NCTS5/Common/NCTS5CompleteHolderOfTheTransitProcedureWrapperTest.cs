using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CompleteHolderOfTheTransitProcedureWrapperTest : WrapperHelperTest<NCTS5CompleteHolderOfTheTransitProcedureWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NctsHeader null", GetWrapper(null, false));

				var nctsHeader = Factory.New<NctsHeader>();
				AssertNull("NctsHeader with no org address in Principal null", GetWrapper(nctsHeader, false));

				var address = Factory.New<OrgAddress>();
				nctsHeader.Principal.E2_OA_Address = address.PK;
				AssertNull("NctsHeader with org address in Pricipal but no OrgHeader null", GetWrapper(nctsHeader, false));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("NctsHeader withMovementHeader and org address in Pricipal and OrgHeader associated", GetWrapper(nctsHeader, false));
			});
		}

		public void TestContactPerson()
		{
			CombineAssertions(() =>
			{
				var contactPerson = wrapper.ContactPerson;
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);

				wrapper = GetWrapper(nctsHeader, isRepresentativeDeclared: true);
				AssertNull("Expected empty ContactPerson when flag isRepresentativeDeclared is true", wrapper.ContactPerson);

				wrapper = GetWrapper(nctsHeader);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Expected filled name from Organzation/Details when not is override and no contact selected", "Org Name", contactPerson.Name);
				AssertEquals("Expected filled Email from Organzation/Details when not is override and no contact selected", "Email", contactPerson.Email);
				AssertEquals("Expected filled Phone from Organzation/Details when not is override and no contact selected", "Phone", contactPerson.PhoneNumber);

				AddContactDetails();
				wrapper = GetWrapper(nctsHeader);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Expected filled name from Organization/Contact when not is override and contact selected", "Contact Selected", contactPerson.Name);
				AssertEquals("Expected filled Email from Organization/Contact when not is override and contact selected", "Email Selected", contactPerson.Email);
				AssertEquals("Expected filled Phone from Organization/Contact when not is override and contact selected", "Phone Selected", contactPerson.PhoneNumber);

				SetOverrideToTrue();
				wrapper = GetWrapper(nctsHeader);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Expected filled name when is override", "Contact Override", contactPerson.Name);
				AssertEquals("Expected filled Email when is override", "Email Override", contactPerson.Email);
				AssertEquals("Expected filled Phone when is override", "Phone Override", contactPerson.PhoneNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var orgAddress = Factory.New<OrgAddress>();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Org Name";
			orgAddress.OA_Email = "Email";
			orgAddress.OA_Phone = "Phone";

			orgAddress.OA_OH = orgHeader.PK;
			nctsHeader.Principal.E2_OA_Address = orgAddress.PK;

			wrapper = GetWrapper(nctsHeader);
		}
		NctsHeader nctsHeader;
		OrgHeader orgHeader;
		NCTS5CompleteHolderOfTheTransitProcedureWrapper wrapper;

		void SetOverrideToTrue()
		{
			nctsHeader.Principal.E2_AddressOverride = true;

			nctsHeader.Principal.E2_Contact = "Contact Override";
			nctsHeader.Principal.E2_Phone = "Phone Override";
			nctsHeader.Principal.E2_Email = "Email Override";
		}

		void AddContactDetails()
		{
			var contact = orgHeader.Contacts.AddNew();
			nctsHeader.Principal.ContactPK = contact.PK;

			contact.OC_ContactName = "Contact Selected";
			contact.OC_Phone = "Phone Selected";
			contact.OC_Email = "Email Selected";
		}

		NCTS5CompleteHolderOfTheTransitProcedureWrapper GetWrapper(NctsHeader header, bool isRepresentativeDeclared = false) => NCTS5CompleteHolderOfTheTransitProcedureWrapper.New(header, isRepresentativeDeclared, false);

		protected override NCTS5CompleteHolderOfTheTransitProcedureWrapper GetProvider() => wrapper;
	}
}
