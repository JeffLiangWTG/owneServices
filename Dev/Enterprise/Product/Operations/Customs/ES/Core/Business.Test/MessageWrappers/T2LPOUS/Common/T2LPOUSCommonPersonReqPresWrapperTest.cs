using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSCommonPersonReqPresWrapperTest : WrapperHelperTest<T2LPOUSCommonPersonReqPresWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				OrgAddress address = null;
				AssertNull("OrgAddress null", GetWrapper(address));
				address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", GetWrapper(address));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", GetWrapper(address));
			});
		}

		public void TestContactPerson()
		{
			CombineAssertions(() =>
			{
				var contactPerson = wrapper.ContactPerson;
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);
			});
		}

		public void TestContactPersonEmail()
		{
			CombineAssertions(() =>
			{
				var contactPerson = wrapper.ContactPerson;
				AssertEquals("Expected empty ContactPerson Email", ZString.Empty, contactPerson.Email);

				wrapper = GetWrapper(orgAddress, "wisetech@wisetechglobal.com");
				AssertEquals("Expected filled ContactPerson Email with one passed", "wisetech@wisetechglobal.com", wrapper.ContactPerson.Email);
			});
		}

		public void TestContactPerson_Phone()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF222222");
			orgHeader.MainAddress.OA_Phone = "987654321";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Phone = "123456789";

			wrapper = GetWrapper(orgAddress);
			var contactPerson = wrapper.ContactPerson;

			CombineAssertions(() =>
			{
				AssertEquals("Phone is taken from OrgAddress if not empty", "123456789", contactPerson.PhoneNumber);

				orgAddress.OA_Phone = ZString.Empty;
				wrapper = GetWrapper(orgAddress);
				contactPerson = wrapper.ContactPerson;
				AssertEquals("Phone is taken from MainAddress if OrgAddress Phone is empty", "987654321", contactPerson.PhoneNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			wrapper = GetWrapper(orgAddress);
		}
		OrgAddress orgAddress;
		T2LPOUSCommonPersonReqPresWrapper wrapper;

		T2LPOUSCommonPersonReqPresWrapper GetWrapper(OrgAddress orgAddress, string contactEmail = "") => T2LPOUSCommonPersonReqPresWrapper.New(orgAddress, contactEmail);

		protected override T2LPOUSCommonPersonReqPresWrapper GetProvider() => wrapper;
	}
}
