using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3CommunicationWrapperTest : DataProviderTestCase<G3CommunicationWrapper>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Expected non-null wrapper when contact is not null", wrapper);

				wrapper = G3CommunicationWrapper.New(null);
				AssertNull("Expected null wrapper when contact is null", wrapper);
			});
		}

		public void TestCommunicationType()
		{
			CombineAssertions(() => {
				contact.OC_Email = "test@123.com";
				AssertEquals("Expected filled Communication Type for non-empty Email", "EM", wrapper.CommunicationType);

				contact.OC_Email = string.Empty;
				contact.OC_Mobile = "12345";
				AssertEquals("Expected filled Communication Type for non-empty Mobile", "TE", wrapper.CommunicationType);

				contact.OC_Mobile = string.Empty;
				AssertEquals("Expected filled empty Communication Type for empty Email and Mobile", string.Empty, wrapper.CommunicationType);
			});
		}

		public void TestCommunicationId()
		{
			CombineAssertions(() => {
				contact.OC_Email = "test@123.com";
				AssertEquals("Expected filled Communication Id for non-empty Email", "test@123.com", wrapper.CommunicationId);

				contact.OC_Email = string.Empty;
				contact.OC_Mobile = "12345";
				AssertEquals("Expected filled Communication Id for non-empty Mobile", "12345", wrapper.CommunicationId);

				contact.OC_Mobile = string.Empty;
				AssertEquals("Expected filled empty Communication Id for empty Email and Mobile", string.Empty, wrapper.CommunicationId);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			contact = Factory.New<OrgContact>();
			wrapper = G3CommunicationWrapper.New(contact);
		}

		protected override G3CommunicationWrapper GetProvider()
		{
			return wrapper;
		}

		G3CommunicationWrapper wrapper;
		OrgContact contact;
	}
}
