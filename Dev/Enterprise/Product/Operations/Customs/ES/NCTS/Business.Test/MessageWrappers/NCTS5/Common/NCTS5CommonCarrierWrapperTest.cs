using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonCarrierWrapperTest : WrapperHelperTest<NCTS5CommonCarrierWrapper>
	{
		public void TestGetNewNCTS5CommonCarrierWrapper()
		{
			CombineAssertions(() =>
			{
				AssertNull("JobDocAddress null", NCTS5CommonCarrierWrapper.New(null));

				var docAddress = Factory.New<JobDocAddress>();
				AssertNull("JobDocAddress with no org address null", NCTS5CommonCarrierWrapper.New(docAddress));

				var address = Factory.New<OrgAddress>();
				docAddress.E2_OA_Address = address.PK;
				AssertNull("JobDocAddress with org address but no OrgHeader null", NCTS5CommonCarrierWrapper.New(docAddress));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", NCTS5CommonCarrierWrapper.New(address));
				AssertNotNull("JobDocAddress not nullnull", NCTS5CommonCarrierWrapper.New(docAddress));
			});
		}

		public void TestContactPerson()
		{
			var contactPerson = wrapper.ContactPerson;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ContactPerson", contactPerson);
				AssertSame("Cached ContactPerson", wrapper.ContactPerson, contactPerson);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var docAddress = Factory.New<JobDocAddress>();
			var orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;

			wrapper = NCTS5CommonCarrierWrapper.New(docAddress);
		}

		NCTS5CommonCarrierWrapper wrapper;

		protected override NCTS5CommonCarrierWrapper GetProvider() => wrapper;
	}
}
